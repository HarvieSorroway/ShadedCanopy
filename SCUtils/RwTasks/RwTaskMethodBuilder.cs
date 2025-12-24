using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SCUtils.RwTasks
{
    internal static class RwTaskContext
    {
        [ThreadStatic] public static RwLoopRunner Current;
    }

    internal readonly struct RwTaskScope : IDisposable
    {
        private readonly RwLoopRunner _last;
        public RwTaskScope(RwLoopRunner runner)
        {
            _last = RwTaskContext.Current;
            RwTaskContext.Current = runner;
        }
        public void Dispose()
        {
            RwTaskContext.Current = _last;
        }
    }

    public struct RwTaskMethodBuilder
    {
        private RwTaskPromise _promise;
        private bool _isCompleted;
        private Exception _exception;
        private bool _hasException;

        private Action _moveNext; //复用TStateMachine.MoveNext
        private IRwPooledBox _box; //池化TStateMachine捕获

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
                if (_promise != null) return _promise.Task;
                if (_hasException) return RwTask.FromException(_exception);
                if (_isCompleted) return RwTask.FromResult();

                _promise = RwTaskPromise.Create(CancellationToken.None);
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
                ReleaseBox();
            }
        }

        public void SetException(Exception exception)
        {
            if (_promise == null)
            {
                _exception = exception;
                _hasException = true;
            }
            else
            {
                _promise.SetException(exception);
                ReleaseBox();
            }
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            EnsurePromise();
            awaiter.OnCompleted(GetOrCreateMoveNext(ref stateMachine)); //减少隐式Action闭包
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            EnsurePromise();
            awaiter.UnsafeOnCompleted(GetOrCreateMoveNext(ref stateMachine)); //减少隐式Action闭包
        }


        private Action GetOrCreateMoveNext<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            if (_moveNext != null) return _moveNext;

            var box = RwAsyncStateMachineBox<TStateMachine>.Rent();
            _box = box;
            _moveNext = box.MoveNextAction;

            //由于builder是struct类型且StateMachine也是且StateMachine拥有_builder字段指向当前builder
            //必须先修改builder内字段然后再将StateMachine赋值给RwAsyncStateMachineBox
            box.StateMachine = stateMachine;
            return _moveNext;
        }

        private void EnsurePromise()
        {
            if (_promise == null) _promise = RwTaskPromise.Create(CancellationToken.None);
        }


        private void ReleaseBox()
        {
            _moveNext = null;
            _box?.ReturnToPool();
            _box = null;
        }
    }

    public struct RwTaskMethodBuilder<T>
    {
        private RwTaskPromise<T> _promise;
        private T _result;
        private bool _hasResult;
        private Exception _exception;
        private bool _hasException;

        private Action _moveNext; //复用TStateMachine.MoveNext
        private IRwPooledBox _box; //池化TStateMachine捕获

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
                if (_promise != null) return _promise.Task;
                if (_hasException) return RwTask.FromException<T>(_exception);
                if (_hasResult) return RwTask.FromResult<T>(_result);

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
                ReleaseBox();
            }
        }

        public void SetException(Exception exception)
        {
            if (_promise == null)
            {
                _exception = exception;
                _hasException = true;
            }
            else
            {
                _promise.SetException(exception);
                ReleaseBox();
            }
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            EnsurePromise();
            awaiter.OnCompleted(GetOrCreateMoveNext(ref stateMachine)); //减少隐式Action闭包
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            EnsurePromise();
            awaiter.UnsafeOnCompleted(GetOrCreateMoveNext(ref stateMachine)); //减少隐式Action闭包
        }

        private Action GetOrCreateMoveNext<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine
        {
            if (_moveNext != null) return _moveNext;

            var box = RwAsyncStateMachineBox<TStateMachine>.Rent();
            _box = box;
            _moveNext = box.MoveNextAction;

            //由于builder是struct类型且StateMachine也是且StateMachine拥有_builder字段指向当前builder
            //必须先修改builder内字段然后再将StateMachine赋值给RwAsyncStateMachineBox
            box.StateMachine = stateMachine; 
            return _moveNext;
        }

        private void EnsurePromise()
        {
            if (_promise == null) _promise = RwTaskPromise<T>.Create(CancellationToken.None);
        }


        private void ReleaseBox()
        {
            _moveNext = null;
            _box?.ReturnToPool();
            _box = null;
        }
    }
}
