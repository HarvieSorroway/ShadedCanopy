using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SCUtils.RwTasks
{
    public class RwTaskCompletionSource<T> : IDisposable
    {
        private readonly RwTaskPromise<T> _promise;
        private short _token;

        public RwTask<T> Task => new RwTask<T>(_promise, _token);

        public bool IsDisposed => _token != _promise.Token;

        public RwTaskCompletionSource(RwLoopRunner runner = null)
        {
            runner ??= RwLoopRunner.LateRawUpdateRunner;
            _promise = RwTaskPromise<T>.Create(CancellationToken.None, runner);
            _token = _promise.Token;
        }

        public bool TrySetResult(T result)
        {
            if (_promise.GetStatus(_token) != RwTaskStatus.Pending) return false;

            _promise.SetResult(result);
            return true;
        }

        public bool TrySetException(Exception exception)
        {
            if (_promise.GetStatus(_token) != RwTaskStatus.Pending) return false;

            _promise.SetException(exception);
            return true;
        }
        public bool TrySetCanceled(CancellationToken token = default)
        {
            if (_promise.GetStatus(_token) != RwTaskStatus.Pending) return false;
            _promise.SetCancel(token);
            return true;
        }

        public void SetResult(T result)
        {
            if (!TrySetResult(result))
                throw new InvalidOperationException("Task is already completed.");
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                try
                {
                    Task.GetAwaiter().GetResult();
                }
                catch { }
            }
        }
    }

    public class RwTaskCompletionSource : IDisposable
    {
        private readonly RwTaskPromise _promise;
        private short _token;

        public RwTask Task => new RwTask(_promise, _token);

        public bool IsDisposed => _token != _promise.Token;

        public RwTaskCompletionSource(RwLoopRunner runner = null)
        {
            runner ??= RwLoopRunner.LateRawUpdateRunner;
            _promise = RwTaskPromise.Create(CancellationToken.None, runner);
            _token = _promise.Token;
        }

        public bool TrySetResult()
        {
            if (_promise.GetStatus(_token) != RwTaskStatus.Pending) return false;

            _promise.SetResult();
            return true;
        }

        public bool TrySetException(Exception exception)
        {
            if (_promise.GetStatus(_token) != RwTaskStatus.Pending) return false;

            _promise.SetException(exception);
            return true;
        }
        public bool TrySetCanceled(CancellationToken token = default)
        {
            if (_promise.GetStatus(_token) != RwTaskStatus.Pending) return false;
            _promise.SetCancel(token);
            return true;
        }

        public void SetResult()
        {
            if (!TrySetResult())
                throw new InvalidOperationException("Task is already completed.");
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                try
                {
                    Task.GetAwaiter().GetResult();
                }
                catch { }
            }
        }
    }
}
