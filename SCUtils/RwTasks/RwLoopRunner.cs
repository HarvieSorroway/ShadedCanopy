using System;
using System.Collections.Generic;
using System.Linq;
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

        static RwLoopRunner()
        {
            DefaultRunner = LateRawUpdateRunner;
        }


        private struct DelayItem
        {
            public RwTaskPromise Promise;
            public long TargetFrame;
            public double TargetTime;
            public bool IsTimeBased;
            public short Token;
        }

        private readonly List<DelayItem> _delayedTasks = new ();
        private readonly Func<float> _timeDelteFunc;


        private IRwTaskSource _head;
        private IRwTaskSource _tail;

        private object _lock = new object();

        private long _currentFrameCount = 0;
        private double _currentTime = 0;

        private RwLoopRunner(Func<float> timeDelteFunc)
        {
            _timeDelteFunc = timeDelteFunc;
        }

        public void ScheduleDelayFrames(RwTaskPromise promise, int frames)
        {
            lock (_lock)
            {
                _delayedTasks.Add(new DelayItem
                {
                    Promise = promise,
                    TargetFrame = _currentFrameCount + frames,
                    IsTimeBased = false,
                    Token = promise.Token,
                });
            }
        }

 
        public void ScheduleDelaySeconds(RwTaskPromise promise, float seconds)
        {
            lock (_lock)
            {
                _delayedTasks.Add(new DelayItem
                {
                    Promise = promise,
                    TargetTime = _currentTime + (double)seconds, 
                    IsTimeBased = true,
                    Token = promise.Token,
                });
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
            if (_delayedTasks.Count > 0)
            {
                int count = _delayedTasks.Count;
                for (int i = count - 1; i >= 0; i--)
                {
                    var item = _delayedTasks[i];
                    if(item.Promise.Token != item.Token || item.Promise.GetStatus(item.Token) != RwTaskStatus.Pending)
                    {
                        _delayedTasks.RemoveAt(i); //在外部取消或完成，删除
                        continue;
                    }
                    bool isReady = false;
                    if (item.IsTimeBased)
                    {
                        if (_currentTime >= item.TargetTime) isReady = true;
                    }
                    else
                    {
                        if (_currentFrameCount >= item.TargetFrame) isReady = true;
                    }

                    if (isReady)
                    {
                        item.Promise.SetResult(); 
                        _delayedTasks.RemoveAt(i);
                    }
                }
            }
        }

        private void TickExecuteTask()
        {
            var current = _head;
            _head = null;
            _tail = null;

            IRwTaskSource pendingHead = null;
            IRwTaskSource pendingTail = null;

            //保留双队列，虽然现在没什么用了
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

        public void Tick()
        {
            _currentFrameCount++;
            _currentTime += _timeDelteFunc();

            TickDelayedTasks();
            TickExecuteTask();
        }
    }
}
