using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCUtils.RwTasks
{
    public class RwLoopRunner
    {

        public static RwLoopRunner EarlyUpdateRunner { get; } = new();
        public static RwLoopRunner LateUpdateRunner { get; } = new();

        public static RwLoopRunner EarlyRawUpdateRunner { get; } = new();
        public static RwLoopRunner LateRawUpdateRunner { get; } = new();

        private IRwTaskSource _head;
        private IRwTaskSource _tail;

        public void Schedule(IRwTaskSource promise)
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

        public void Tick()
        {
            var current = _head;
            _head = null;
            _tail = null;

            IRwTaskSource pendingHead = null;
            IRwTaskSource pendingTail = null;

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
    }
}
