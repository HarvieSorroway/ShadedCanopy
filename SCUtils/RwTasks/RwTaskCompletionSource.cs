using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SCUtils.RwTasks
{
    public class RwTaskCompletionSource<T>
    {
        private readonly RwTaskPromise<T> _promise;
        private short _token;

        public RwTask<T> Task => _promise.Task;

        public RwTaskCompletionSource(RwLoopRunner runner = null)
        {
            runner ??= RwLoopRunner.LateUpdateRunner;
            _promise = RwTaskPromise<T>.Create(CancellationToken.None);
            runner.Schedule(_promise);
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

            _promise.SetException(new OperationCanceledException(token));
            return true;
        }

        public void SetResult(T result)
        {
            if (!TrySetResult(result))
                throw new InvalidOperationException("Task is already completed.");
        }
    }
}
