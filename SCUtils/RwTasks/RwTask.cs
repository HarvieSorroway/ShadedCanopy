using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Delegate | AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
    public sealed class AsyncMethodBuilderAttribute : Attribute
    {
        public Type BuilderType { get; }

        public AsyncMethodBuilderAttribute(Type builderType)
        {
            BuilderType = builderType;
        }
    }
}
namespace SCUtils.RwTasks
{

    public enum RwTaskStatus
    {
        Pending,
        Succeeded,
        Canceled,
        Faulted
    }

    [AsyncMethodBuilder(typeof(RwTaskMethodBuilder))]
    public readonly partial struct RwTask
    {

        /// <summary>
        /// 延迟指定的帧数，并在 Update 结束阶段恢复执行。
        /// </summary>
        /// <param name="frames">要延迟的帧数。</param>
        /// <param name="token">取消令牌，用于取消等待。</param>
        public static RwTask DelayFrames(int frames, CancellationToken token = default)
        {
            var promise = RwTaskPromise.Create(token, RwLoopRunner.LateUpdateRunner);
            RwLoopRunner.LateUpdateRunner.ScheduleDelayFrames(promise, frames);
            return promise.Task;
        }

        /// <summary>
        /// 延迟指定的帧数，并在 Update 开始阶段恢复执行。
        /// </summary>
        /// <param name="frames">要延迟的帧数。</param>
        /// <param name="token">取消令牌，用于取消等待。</param>
        public static RwTask DelayEarlyFrames(int frames, CancellationToken token = default)
        {
            var promise = RwTaskPromise.Create(token, RwLoopRunner.EarlyUpdateRunner);
            RwLoopRunner.EarlyUpdateRunner.ScheduleDelayFrames(promise, frames);
            return promise.Task;
        }

        /// <summary>
        /// 延迟指定的帧数，并在 RawUpdate 结束阶段恢复执行。
        /// <para>适用于不受rw帧速率影响速度的逻辑。</para>
        /// </summary>
        /// <param name="frames">要延迟的帧数。</param>
        /// <param name="token">取消令牌，用于取消等待。</param>
        public static RwTask DelayRawFrames(int frames, CancellationToken token = default)
        {
            var promise = RwTaskPromise.Create(token, RwLoopRunner.LateRawUpdateRunner);
            RwLoopRunner.LateRawUpdateRunner.ScheduleDelayFrames(promise, frames);
            return promise.Task;
        }

        /// <summary>
        /// 延迟指定的帧数，并在 RawUpdate 开始阶段恢复执行。
        /// <para>适用于不受rw帧速率影响速度的逻辑。</para>
        /// </summary>
        /// <param name="frames">要延迟的帧数。</param>
        /// <param name="token">取消令牌，用于取消等待。</param>
        public static RwTask DelayEarlyRawFrames(int frames, CancellationToken token = default)
        {
            var promise = RwTaskPromise.Create(token, RwLoopRunner.EarlyRawUpdateRunner);
            RwLoopRunner.EarlyRawUpdateRunner.ScheduleDelayFrames(promise, frames);
            return promise.Task;
        }


        /// <summary>
        /// 延迟指定的秒数，并在 Update 结束阶段恢复执行。
        /// </summary>
        /// <param name="seconds">要延迟的秒数。</param>
        /// <param name="token">取消令牌，用于取消等待。</param>
        public static RwTask DelaySeconds(float seconds, CancellationToken token = default)
        {
            var promise = RwTaskPromise.Create(token, RwLoopRunner.LateUpdateRunner);
            RwLoopRunner.LateUpdateRunner.ScheduleDelaySeconds(promise, seconds);
            return promise.Task;
        }

        /// <summary>
        /// 延迟指定的秒数，并在 Update 开始阶段恢复执行。
        /// </summary>
        /// <param name="seconds">要延迟的秒数。</param>
        /// <param name="token">取消令牌，用于取消等待。</param>
        public static RwTask DelayEarlySeconds(float seconds, CancellationToken token = default)
        {
            var promise = RwTaskPromise.Create(token, RwLoopRunner.EarlyUpdateRunner);
            RwLoopRunner.EarlyUpdateRunner.ScheduleDelaySeconds(promise, seconds);
            return promise.Task;
        }

        /// <summary>
        /// 延迟指定的秒数，并在 RawUpdate 结束阶段恢复执行。
        /// <para>适用于不受rw帧速率影响速度的逻辑。</para>
        /// </summary>
        /// <param name="seconds">要延迟的秒数。</param>
        /// <param name="token">取消令牌，用于取消等待。</param>
        public static RwTask DelayRawSeconds(float seconds, CancellationToken token = default)
        {
            var promise = RwTaskPromise.Create(token, RwLoopRunner.LateRawUpdateRunner);
            RwLoopRunner.LateRawUpdateRunner.ScheduleDelaySeconds(promise, seconds);
            return promise.Task;
        }

        /// <summary>
        /// 延迟指定的秒数，并在 RawUpdate 开始阶段恢复执行。
        /// <para>适用于不受rw帧速率影响速度的逻辑。</para>
        /// </summary>
        /// <param name="seconds">要延迟的秒数。</param>
        /// <param name="token">取消令牌，用于取消等待。</param>
        public static RwTask DelayEarlyRawSeconds(float seconds, CancellationToken token = default)
        {
            var promise = RwTaskPromise.Create(token, RwLoopRunner.EarlyRawUpdateRunner);
            RwLoopRunner.EarlyRawUpdateRunner.ScheduleDelaySeconds(promise, seconds);
            return promise.Task;
        }

        /// <summary>
        /// 挂起当前任务，直到下一帧的 Update 开始阶段。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        public static RwTask YieldEarly(CancellationToken token = default)
        {
            var promise = RwYieldPromise.CreateYield(token, RwLoopRunner.EarlyUpdateRunner);
            return promise.Task;
        }

        /// <summary>
        /// 挂起当前任务，直到下一帧的 Update 结束阶段。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        public static RwTask Yield(CancellationToken token = default)
        {
            var promise = RwYieldPromise.CreateYield(token, RwLoopRunner.LateUpdateRunner);
            return promise.Task;
        }

        /// <summary>
        /// 挂起当前任务，直到下一帧的 RawUpdate 开始阶段。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        public static RwTask YieldEarlyRaw(CancellationToken token = default)
        {
            var promise = RwYieldPromise.CreateYield(token, RwLoopRunner.EarlyRawUpdateRunner);
            return promise.Task;
        }

        /// <summary>
        /// 挂起当前任务，直到下一帧的 RawUpdate 结束阶段。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        public static RwTask YieldRaw(CancellationToken token = default)
        {
            var promise = RwYieldPromise.CreateYield(token, RwLoopRunner.LateRawUpdateRunner);
            return promise.Task;
        }

        /// <summary>
        /// 异步等待，直到指定的条件为真。
        /// </summary>
        /// <param name="predicate">要评估的条件函数。当该函数返回 <c>true</c> 时停止等待。</param>
        /// <param name="token">用于取消等待操作的取消令牌。</param>
        /// <exception cref="OperationCanceledException">如果在等待期间取消令牌被触发。</exception>
        public static async RwTask WaitUntil(Func<bool> predicate, CancellationToken token = default)
        {
            while (!predicate())
            {
                await Yield(token);
            }
        }

        /// <summary>
        /// 无限期挂起，直到传入的 CancellationToken 被取消。
        /// </summary>
        public static RwTask WaitCanceled(CancellationToken token)
            => RwTaskPromise.Create(token, RwTaskContext.Current).Task;

        /// <summary>
        /// 无限期挂起，直到传入的 CancellationTokenSource 被取消。
        /// </summary>
        public static RwTask WaitCanceled(CancellationTokenSource cts)
            => WaitCanceled(cts?.Token ?? default);


        /// <summary>
        /// 等待 Token 被取消，但不会抛出 OperationCanceledException 异常。
        /// </summary>
        public static async RwTask WaitCanceledNoThrow(CancellationToken token)
        {
            try { await WaitCanceled(token); }
            catch (OperationCanceledException) { }
        }

        /// <summary>
        /// 等待传入的 任意一个 Token 被取消。
        /// </summary>
        public static async RwTask WaitCanceledAny(params CancellationToken[] tokens)
        {
            if (tokens == null || tokens.Length == 0) return;

            if (tokens.Length == 1)
            {
                await WaitCanceled(tokens[0]);
                return;
            }

            using var linked = CancellationTokenSource.CreateLinkedTokenSource(tokens);
            await WaitCanceled(linked.Token);
        }

        /// <summary>
        /// 创建一个任务，该任务在所有提供的任务完成后完成。
        /// </summary>
        public static RwTask WhenAll(IEnumerable<RwTask> tasks)
        {
            return WhenAll(tasks.ToArray());
        }

        /// <summary>
        /// 创建一个任务，该任务在所有提供的任务完成后完成。
        /// </summary>
        public static async RwTask WhenAll(params RwTask[] tasks)
        {
            if (tasks == null || tasks.Length == 0) return;
            var span = (RwTask[])tasks.Clone();
            var tcs = new RwTaskCompletionSource<bool>(RwTaskContext.Current);
            int remaining = tasks.Length;
            bool hasError = false;
            Lazy<ConcurrentBag<Exception>> exceptions = new Lazy<ConcurrentBag<Exception>>();
            foreach (var task in span)
            {
                task.GetAwaiter().OnCompleted(() =>
                {
                    try
                    {
                        task.GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        Volatile.Write(ref hasError, true);
                        exceptions.Value.Add(ex);
                    }
                    if (Interlocked.Decrement(ref remaining) == 0)
                    {
                        if (Volatile.Read(ref hasError))
                            tcs.TrySetException(new AggregateException("One or more tasks failed", exceptions.Value));
                        else
                            tcs.TrySetResult(true);
                    }
                });
            }
            await tcs.Task;
        }

        /// <summary>
        /// 创建一个任务，该任务在所有提供的任务完成后完成。
        /// </summary>
        public static RwTask<T[]> WhenAll<T>(IEnumerable<RwTask<T>> tasks)
        {
            return WhenAll(tasks.ToArray());
        }

        /// <summary>
        /// 创建一个任务，该任务在所有提供的任务完成后完成。
        /// </summary>
        public static async RwTask<T[]> WhenAll<T>(params RwTask<T>[] tasks)
        {
            if (tasks == null || tasks.Length == 0) return default;
            var span = (RwTask<T>[])tasks.Clone();
            var tcs = new RwTaskCompletionSource<T[]>(RwTaskContext.Current);
            int remaining = span.Length;
            bool hasError = false;
            Lazy<ConcurrentBag<Exception>> exceptions = new Lazy<ConcurrentBag<Exception>>();
            T[] results = new T[span.Length];
            for (int i = 0; i < span.Length; i++)
            {
                var idx = i;
                var task = span[i];
                task.GetAwaiter().OnCompleted(() =>
                {
                    try
                    {
                        var result = task.GetAwaiter().GetResult();
                        results[idx] = result;
                    }
                    catch (Exception ex)
                    {
                        Volatile.Write(ref hasError, true);
                        exceptions.Value.Add(ex);
                    }
                    if (Interlocked.Decrement(ref remaining) == 0)
                    {
                        if (Volatile.Read(ref hasError))
                            tcs.TrySetException(new AggregateException("One or more tasks failed", exceptions.Value));
                        else
                            tcs.TrySetResult(results);
                    }
                });
            }
            return await tcs.Task;
        }

        /// <summary>
        /// 创建一个任务，该任务在提供的任意一个任务完成时完成。
        /// </summary>
        public static RwTask WhenAny(IEnumerable<RwTask> tasks)
        {
            return WhenAny(tasks.ToArray());
        }

        /// <summary>
        /// 创建一个任务，该任务在提供的任意一个任务完成时完成。
        /// </summary>
        public static async RwTask WhenAny(params RwTask[] tasks)
        {
            if (tasks == null || tasks.Length == 0) return;
            var tcs = new RwTaskCompletionSource<bool>(RwTaskContext.Current);
            var span = (RwTask[])tasks.Clone();
            foreach (var task in span)
            {
                task.GetAwaiter().OnCompleted(() =>
                {
                    try
                    {
                        task.GetAwaiter().GetResult();
                        tcs.TrySetResult(true);
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                });
            }
            await tcs.Task;
            foreach(var task in span)
            {
                task.Forget(); //清理
            }
        }

        /// <summary>
        /// 创建一个任务，该任务在提供的任意一个任务完成时完成。
        /// </summary>
        public static RwTask<T> WhenAny<T>(IEnumerable<RwTask<T>> tasks)
        {
            return WhenAny(tasks.ToArray());
        }

        /// <summary>
        /// 创建一个任务，该任务在提供的任意一个任务完成时完成。
        /// </summary>
        public static async RwTask<T> WhenAny<T>(params RwTask<T>[] tasks)
        {
            if (tasks == null || tasks.Length == 0) return default;
            var tcs = new RwTaskCompletionSource<T>(RwTaskContext.Current);
            var span = (RwTask<T>[])tasks.Clone();
            foreach (var task in span)
            {
                task.GetAwaiter().OnCompleted(() =>
                {
                    try
                    {
                        var result = task.GetAwaiter().GetResult();
                        tcs.TrySetResult(result);
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                });
            }
            var re = await tcs.Task;
            foreach (var task in span)
            {
                task.Forget(); //清理
            }
            return re;
        }

        /// <summary>
        /// 异步等待 <see cref="WaitHandle"/> 在指定时间内收到信号。
        /// </summary>
        /// <param name="waitHandle">要等待的同步句柄（如 AutoResetEvent, ManualResetEvent 等）。</param>
        /// <param name="timeoutMilliseconds">等待的超时时间（毫秒）。</param>
        /// <param name="token">用于取消等待操作的取消令牌。</param>
        /// <returns>
        /// 一个包含布尔值的任务：
        /// <c>true</c> 表示句柄在超时前收到了信号；
        /// <c>false</c> 表示在收到信号前已超时。
        /// </returns>
        /// <exception cref="OperationCanceledException">如果取消令牌被触发。</exception>
        public static async RwTask<bool> WaitAsync(WaitHandle waitHandle, int timeoutMilliseconds, CancellationToken token)
        {
            if (waitHandle.WaitOne(0)) return true;
            if (token.IsCancellationRequested)
                return await RwTask.FromCanceled<bool>(token);

            var tcs = new RwTaskCompletionSource<bool>(RwTaskContext.Current);
            CancellationTokenRegistration ctr = default;
            ctr = token.Register(() =>
            {
                tcs.TrySetCanceled();
                ctr.Dispose();
            });

            RegisteredWaitHandle registeredWaitHandle = null;
            WaitOrTimerCallback callback = (state, timedOut) =>
            {
                ctr.Dispose();
                registeredWaitHandle?.Unregister(null);

                if (timedOut)
                {
                    tcs.TrySetResult(false);
                }
                else
                {
                    tcs.TrySetResult(true);
                }
            };

            registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(
                waitHandle,
                callback,
                null,
                timeoutMilliseconds,
                executeOnlyOnce: true
            );

            return await tcs.Task;
        }

        /// <summary>
        /// 创建一个处于已取消状态的任务。
        /// </summary>
        public static RwTask FromCanceled(CancellationToken token)
        {
            return new RwTask(token);
        }

        /// <summary>
        /// 创建一个处于已取消状态的带返回值的任务。
        /// </summary>
        public static RwTask<T> FromCanceled<T>(CancellationToken token)
        {
            return new RwTask<T>(token);
        }


        /// <summary>
        /// 创建一个处于异常状态的任务。
        /// </summary>
        public static RwTask FromException(Exception exception)
        {
            return new RwTask(exception);
        }

        /// <summary>
        /// 创建一个处于异常状态的带返回值的任务。
        /// </summary>
        public static RwTask<T> FromException<T>(Exception exception)
        {
            return new RwTask<T>(exception);
        }

        /// <summary>
        /// 创建一个已成功完成的任务。
        public static RwTask FromResult()
        {
            return new RwTask();
        }

        /// <summary>
        /// 创建一个已成功完成的带返回值的任务。
        /// </summary>
        public static RwTask<T> FromResult<T>(T result)
        {
            return new RwTask<T>(result);
        }
    }

    public readonly partial struct RwTask
    {
        private readonly IRwTaskSource _source;
        private readonly short _token;
        private readonly Exception? _exception;
        private readonly bool _isCanceled;

        internal RwTask(IRwTaskSource source, short token)
        {
            _source = source;
            _token = token;
            _exception = null;
            _isCanceled = false;
        }

        internal RwTask(Exception exception)
        {
            _exception = exception;
            _isCanceled = false;
            _source = null;
            _token = 0;
        }

        internal RwTask(CancellationToken token)
        {
            _exception = new OperationCanceledException(token);
            _isCanceled = true;
            _source = null;
            _token = 0;
        }

        /// <summary>
        /// 获取用于等待此任务的等待者（Awaiter）。
        /// </summary>
        public RwTaskAwaiter GetAwaiter()
        {
            return new RwTaskAwaiter(this);
        }

        public readonly struct RwTaskAwaiter : INotifyCompletion
        {
            private readonly RwTask _task;

            public RwTaskAwaiter(RwTask task)
            {
                _task = task;
            }

            /// <summary>
            /// 获取一个值，该值指示任务是否已完成。
            /// </summary>
            public bool IsCompleted
            {
                get
                {
                    if (_task._source == null) return true;
                    return _task._source.GetStatus(_task._token) != RwTaskStatus.Pending;
                }
            }

            /// <summary>
            /// 结束对任务的等待；如果任务失败，将在此处抛出异常。
            /// </summary>
            public void GetResult()
            {
                if (_task._source == null)
                {
                    if(_task._exception != null)
                        ExceptionDispatchInfo.Capture(_task._exception).Throw();
                }
                ((IRwTaskSourceVoid)(_task._source)).GetResult(_task._token);
            }

            /// <summary>
            /// 注册在任务完成时调用的后续操作。
            /// </summary>
            public void OnCompleted(Action continuation)
            {
                if(_task._source is null)
                {
                    continuation?.Invoke();
                    return;
                }
                _task._source.OnCompleted(continuation, _task._token);
               
            }
        }

        /// <summary>
        /// 指示任务是否已完成。
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                if (_source == null) return true;
                return GetAwaiter().IsCompleted;
            }
        }

        /// <summary>
        /// 指示任务是否已被取消。
        /// </summary>
        public bool IsCanceled
        {
            get
            {
                if (_source == null) return _isCanceled;
                return _source.GetStatus(_token) == RwTaskStatus.Canceled;
            }
        }

        /// <summary>
        /// 指示任务是否因异常而失败。
        /// </summary>
        public bool IsFaulted
        {
            get
            {
                if (_source == null) return _exception is not null && !_isCanceled;
                return _source.GetStatus(_token) == RwTaskStatus.Faulted;
            }
        }
    }

    [AsyncMethodBuilder(typeof(RwTaskMethodBuilder<>))]
    public readonly struct RwTask<T>
    {
        private readonly T _result;
        private readonly IRwTaskSource _source;
        private readonly short _token;
        private readonly Exception? _exception;
        private readonly bool _isCanceled;

        public RwTask(T result)
        {
            _result = result;
            _source = null;
            _token = 0;
            _exception = null;
            _isCanceled = false;
        }
        public RwTask(IRwTaskSource<T> source, short token)
        {
            _result = default;
            _source = source;
            _token = token;
            _exception = null;
            _isCanceled = false;
        }

        internal RwTask(Exception exception)
        {
            _result = default;
            _exception = exception;
            _isCanceled = false;
            _source = null;
            _token = 0;
        }

        internal RwTask(CancellationToken token)
        {
            _result = default;
            _exception = new OperationCanceledException(token);
            _isCanceled = true;
            _source = null;
            _token = 0;
        }

        /// <summary>
        /// 获取用于等待此任务的等待者（Awaiter）。
        /// </summary>
        public RwTaskAwaiter GetAwaiter()
        {
            return new RwTaskAwaiter(this);
        }   

        public readonly struct RwTaskAwaiter : INotifyCompletion
        {
            private readonly RwTask<T> _task;

            public RwTaskAwaiter(RwTask<T> task)
            {
                _task = task;
            }

            /// <summary>
            /// 获取一个值，该值指示任务是否已完成。
            /// </summary>
            public bool IsCompleted
            {
                get
                {
                    if (_task._source == null) return true;
                    return _task._source.GetStatus(_task._token) != RwTaskStatus.Pending;
                }
            }

            /// <summary>
            /// 结束对任务的等待；如果任务失败，将在此处抛出异常。
            /// </summary>
            public T GetResult()
            {
                if (_task._source == null)
                {
                    if (_task._exception != null)
                        ExceptionDispatchInfo.Capture(_task._exception).Throw();
                    else
                        return _task._result;
                }
                return ((IRwTaskSource<T>)(_task._source)).GetResult(_task._token);
            }

            /// <summary>
            /// 注册在任务完成时调用的后续操作。
            /// </summary>
            public void OnCompleted(Action continuation)
            {
                if (_task._source is null)
                {
                    continuation?.Invoke();
                    return;
                }
                _task._source.OnCompleted(continuation, _task._token);
            }
        }

        /// <summary>
        /// 指示任务是否已完成。
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                if (_source == null) return true;
                return GetAwaiter().IsCompleted;
            }
        }
        /// <summary>
        /// 指示任务是否已被取消。
        /// </summary>
        public bool IsCanceled
        {
            get
            {
                if (_source == null) return _isCanceled;
                return _source.GetStatus(_token) == RwTaskStatus.Canceled;
            }
        }

        /// <summary>
        /// 指示任务是否因异常而失败。
        /// </summary>
        public bool IsFaulted
        {
            get
            {
                if (_source == null) return _exception is not null && !_isCanceled;
                return _source.GetStatus(_token) == RwTaskStatus.Faulted;
            }
        }
    }

}
