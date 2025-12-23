using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SCUtils.RwTasks
{
    public class RwLoopRunner
    {

        public static RwLoopRunner EarlyUpdateRunner { get; } = new(() => 1 / 40f);
        public static RwLoopRunner LateUpdateRunner { get; } = new(() => 1 / 40f);

        public static RwLoopRunner EarlyRawUpdateRunner { get; } = new(() => Time.deltaTime);
        public static RwLoopRunner LateRawUpdateRunner { get; } = new(() => Time.deltaTime);

        public static RwLoopRunner DefaultRunner { get; }

        private static readonly ConcurrentStack<DelayNode> _pool = new ConcurrentStack<DelayNode>();

        private static DelayNode Create()
        {
            if(_pool.TryPop(out var re))
            {
                return re;
            }
            return new DelayNode();
        }

        private static void Release(DelayNode item)
        {
            item.Reset();
            _pool.Push(item);
        }

        static RwLoopRunner()
        {
            DefaultRunner = LateRawUpdateRunner;
        }


        private class DelayNode
        {
            public RwTaskPromise Promise;
            public long TargetFrame;
            public double TargetTime;
            public bool IsTimeBased;
            public short Token;
            public DelayNode NextNode;

            public void Reset()
            {
                Promise = null;
                IsTimeBased = false;
                NextNode = null;
                TargetFrame = 0;
                TargetTime = 0;
            }
        }

        private readonly Func<float> _timeDelteFunc;


        private IRwTaskSource _head;
        private IRwTaskSource _tail;
        private DelayNode _delayHead;
        private DelayNode _delayTail;

        private object _lock = new object();

        private long _currentFrameCount = 0;
        private double _currentTime = 0;

        private RwLoopRunner(Func<float> timeDelteFunc)
        {
            _timeDelteFunc = timeDelteFunc;
        }

        public void ScheduleDelayFrames(RwTaskPromise promise, int frames)
        {
            var node = Create();
            node.Promise = promise;
            node.TargetFrame = _currentFrameCount + frames;
            node.IsTimeBased = false;
            node.Token = promise.Token;
            lock (_lock)
            {
                node.NextNode = null;

                if (_delayHead == null)
                {
                    _delayHead = node;
                    _delayTail = node;
                }
                else
                {
                    _delayTail.NextNode = node;
                    _delayTail = node;
                }
            }
        }

 
        public void ScheduleDelaySeconds(RwTaskPromise promise, float seconds)
        {
            var node = Create();
            node.Promise = promise;
            node.TargetTime = _currentTime + (double)seconds;
            node.IsTimeBased = true;
            node.Token = promise.Token;
            lock (_lock)
            {
                node.NextNode = null;
                if (_delayHead == null)
                {
                    _delayHead = node;
                    _delayTail = node;
                }
                else
                {
                    _delayTail.NextNode = node;
                    _delayTail = node;
                }

            }
        }

        public void Schedule(IRwTaskSource promise)
        {
            lock (_lock)
            {
                promise.NextNode = null;

                if (_head == null)
                {
                    _head = promise;
                    _tail = promise;
                }
                else
                {
                    _tail.NextNode = promise;
                    _tail = promise;
                }
            }
        }

        private void TickDelayedTasks()
        {
            DelayNode tmpHead = null;
            DelayNode node = null;
            DelayNode lastNode = null;
            lock (_lock)
            {
                lastNode = null;
                tmpHead = node = _delayHead;
                _delayTail = null;
                _delayHead = null;
            }
            try
            {
                while (node != null)
                {
                    bool needRelease = false;
                    if (node.Promise.Token != node.Token || node.Promise.GetStatus(node.Token) != RwTaskStatus.Pending)
                    {
                        needRelease = true;
                    }
                    else
                    {
                        bool isReady = false;
                        if (node.IsTimeBased)
                        {
                            if (_currentTime >= node.TargetTime) isReady = true;
                        }
                        else
                        {
                            if (_currentFrameCount >= node.TargetFrame) isReady = true;
                        }

                        if (isReady)
                        {
                            node.Promise.SetResult();

                            needRelease = true;
                        }
                    }
                    if (needRelease)
                    {
                        var tmp = node;
                        if (node == tmpHead) tmpHead = node.NextNode;
                        else lastNode.NextNode = node.NextNode;
                        node = node.NextNode;
                        Release(tmp);
                    }
                    else
                    {
                        lastNode = node;
                        node = node.NextNode;
                    }
                }
            }
            finally
            {
                lock (_lock)
                {
                    if (tmpHead == null && lastNode == null)
                    {

                    }
                    else if (_delayHead == null)
                    {
                        _delayHead = tmpHead;
                        _delayTail = lastNode;
                    }
                    else
                    {
                        _delayTail.NextNode = tmpHead;
                        _delayTail = lastNode;
                    }
                }
            }
        }

        private void TickExecuteTask()
        {
            IRwTaskSource current = null;
            lock (_lock)
            {
                 current = _head;
                _head = null;
                _tail = null;
            }
            IRwTaskSource pendingHead = null;
            IRwTaskSource pendingTail = null;
            try
            {
                //保留双链表，虽然现在没什么用了
                while (current != null)
                {
                    var next = current.NextNode;

                    current.NextNode = null;

                    bool isFinished = current.Execute();

                    if (!isFinished)
                    {
                        if (pendingHead == null)
                        {
                            pendingHead = current;
                            pendingTail = current;
                        }
                        else
                        {
                            pendingTail.NextNode = current;
                            pendingTail = current;
                        }
                    }

                    current = next;
                }
            }
            finally
            {
                lock (_lock)
                {
                    if (pendingHead != null)
                    {
                        if (_head == null)
                        {
                            _head = pendingHead;
                            _tail = pendingTail;
                        }
                        else
                        {
                            _tail.NextNode = pendingHead;
                            _tail = pendingTail;
                        }
                    }
                }
            }
        }

        public void Tick()
        {
            _currentFrameCount++;
            _currentTime += _timeDelteFunc();

            TickDelayedTasks();
            TickExecuteTask();
        }
    }
}
