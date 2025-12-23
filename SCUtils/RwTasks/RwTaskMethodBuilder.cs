using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SCUtils.RwTasks
{
    internal static class RwTaskContext
    {
        [ThreadStatic] public static RwLoopRunner Current;

        public static void Schedule(IRwTaskSource src)
            => (Current ?? RwLoopRunner.LateUpdateRunner).Schedule(src);
    }

    internal readonly struct RwTaskScope : IDisposable
    {
        public RwTaskScope(RwLoopRunner runner)
        {
            RwTaskContext.Current = runner;
        }
        public void Dispose()
        {
            RwTaskContext.Current = null;
        }
    }

    public struct RwTaskMethodBuilder
    {
        private RwTaskPromise _promise;
        private bool _isCompleted;

        public static RwTaskMethodBuilder Create() => default;

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine) { }

        public RwTask Task
        {
            get
            {
                if (_promise != null)
                {
                    return _promise.Task;
                }

                if (_isCompleted)
                {
                    return RwTask.FromeResult();
                }
                _promise = RwTaskPromise.Create(0, CancellationToken.None);
                return _promise.Task;
            }
        }

        public void SetResult()
        {
            if (_promise == null)
            {
                _isCompleted = true;
            }
            else
            {
                _promise.SetResult();
            }
        }

        public void SetException(Exception exception)
        {
            if (_promise == null)
            {
                 _promise = RwTaskPromise.Create(0, CancellationToken.None);
            }
            _promise.SetException(exception);
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            if (_promise == null) _promise = RwTaskPromise.Create(0, CancellationToken.None);
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            if (_promise == null) _promise = RwTaskPromise.Create(0, CancellationToken.None);
            awaiter.UnsafeOnCompleted(stateMachine.MoveNext);
        }
    }

    public struct RwTaskMethodBuilder<T>
    {
        private RwTaskPromise<T> _promise;
        private T _result;
        private bool _hasResult;

        public static RwTaskMethodBuilder<T> Create() => default;

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine) { }

        public RwTask<T> Task
        {
            get
            {
                if (_promise != null)
                {
                    return _promise.Task;
                }

                if (_hasResult)
                {
                    return new RwTask<T>(_result);
                }

                _promise = RwTaskPromise<T>.Create(CancellationToken.None);
                return _promise.Task;
            }
        }

        public void SetResult(T result)
        {
            if (_promise == null)
            {
                _result = result;
                _hasResult = true;
            }
            else
            {
                _promise.SetResult(result);
            }
        }

        public void SetException(Exception exception)
        {
            if (_promise == null) _promise = RwTaskPromise<T>.Create(CancellationToken.None);
            _promise.SetException(exception);
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            if (_promise == null) _promise = RwTaskPromise<T>.Create(CancellationToken.None);
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            if (_promise == null) _promise = RwTaskPromise<T>.Create(CancellationToken.None);
            awaiter.UnsafeOnCompleted(stateMachine.MoveNext);
        }
    }
}
