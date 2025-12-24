using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SCUtils.RwTasks
{
    internal interface IRwPooledBox
    {
        void ReturnToPool();
    }

    //在异步下捕获StateMachine并保留，实现async函数多await复用单个StateMachine实例
    //且RwAsyncStateMachineBox实现池化复用
    internal sealed class RwAsyncStateMachineBox<TStateMachine> : IAsyncStateMachine, IRwPooledBox
        where TStateMachine : IAsyncStateMachine
    {
        private static readonly ConcurrentStack<RwAsyncStateMachineBox<TStateMachine>> _pool = new();

        public TStateMachine StateMachine;
        public readonly Action MoveNextAction;

        private RwAsyncStateMachineBox()
        {
            MoveNextAction = MoveNext;
        }

        public static RwAsyncStateMachineBox<TStateMachine> Rent()
        {
            if (!_pool.TryPop(out var box))
                box = new RwAsyncStateMachineBox<TStateMachine>();
            return box;
        }

        public void ReturnToPool()
        {
            StateMachine = default;
            _pool.Push(this);
        }

        public void MoveNext() => StateMachine.MoveNext();
        public void SetStateMachine(IAsyncStateMachine stateMachine) => StateMachine.SetStateMachine(stateMachine);
    }
}
