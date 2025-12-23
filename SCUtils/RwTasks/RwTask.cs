using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        /// <param name="frame">要延迟的帧数。</param>
        /// <param name="token">取消令牌，用于取消等待。</param>
        public static RwTask DelayFrames(int frame, CancellationToken token = default)
        {
            var promise = RwTaskPromise.Create(frame, token);
            RwLoopRunner.LateUpdateRunner.Schedule(promise);
            return promise.Task;
        }

        /// <summary>
        /// 延迟指定的帧数，并在 Update 开始阶段恢复执行。
        /// </summary>
        /// <param name="frame">要延迟的帧数。</param>
        /// <param name="token">取消令牌，用于取消等待。</param>
        public static RwTask DelayEarlyFrames(int frame, CancellationToken token = default)
        {
            var promise = RwTaskPromise.Create(frame, token);
            RwLoopRunner.EarlyUpdateRunner.Schedule(promise);
            return promise.Task;
        }

        /// <summary>
        /// 延迟指定的帧数，并在 RawUpdate 结束阶段恢复执行。
        /// <para>适用于不受rw帧速率影响速度的逻辑。</para>
        /// </summary>
        /// <param name="frame">要延迟的帧数。</param>
        /// <param name="token">取消令牌，用于取消等待。</param>
        public static RwTask DelayRawFrames(int frame, CancellationToken token = default)
        {
            var promise = RwTaskPromise.Create(frame, token);
            RwLoopRunner.LateRawUpdateRunner.Schedule(promise);
            return promise.Task;
        }

        /// <summary>
        /// 延迟指定的帧数，并在 RawUpdate 开始阶段恢复执行。
        /// <para>适用于不受rw帧速率影响速度的逻辑。</para>
        /// </summary>
        /// <param name="frame">要延迟的帧数。</param>
        /// <param name="token">取消令牌，用于取消等待。</param>
        public static RwTask DelayEarlyRawFrames(int frame, CancellationToken token = default)
        {
            var promise = RwTaskPromise.Create(frame, token);
            RwLoopRunner.EarlyRawUpdateRunner.Schedule(promise);
            return promise.Task;
        }

        /// <summary>
        /// 挂起当前任务，直到下一帧的 Update 开始阶段。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        public static RwTask YieldEarly(CancellationToken token = default)
        {
            var promise = RwTaskPromise.Create(1, token);
            RwLoopRunner.EarlyUpdateRunner.Schedule(promise);
            return promise.Task;
        }

        /// <summary>
        /// 挂起当前任务，直到下一帧的 Update 结束阶段。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        public static RwTask Yield(CancellationToken token = default)
        {
            var promise = RwTaskPromise.Create(1, token);
            RwLoopRunner.LateUpdateRunner.Schedule(promise);
            return promise.Task;
        }

        /// <summary>
        /// 挂起当前任务，直到下一帧的 RawUpdate 开始阶段。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        public static RwTask YieldEarlyRaw(CancellationToken token = default)
        {
            var promise = RwTaskPromise.Create(1, token);
            RwLoopRunner.EarlyRawUpdateRunner.Schedule(promise);
            return promise.Task;
        }

        /// <summary>
        /// 挂起当前任务，直到下一帧的 RawUpdate 结束阶段。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        public static RwTask YieldRaw(CancellationToken token = default)
        {
            var promise = RwTaskPromise.Create(1, token);
            RwLoopRunner.LateRawUpdateRunner.Schedule(promise);
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
            => DelayFrames(-1, token);

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
            var tcs = new RwTaskCompletionSource<bool>();
            int remaining = tasks.Length;
            bool hasError = false;
            List<Exception> exceptions = null;
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
                        if (exceptions == null)
                        {
                            exceptions = new List<Exception>();
                        }
                        exceptions.Add(ex);
                    }
                    if (Interlocked.Decrement(ref remaining) == 0)
                    {
                        if (Volatile.Read(ref hasError))
                            tcs.TrySetException(new AggregateException("One or more tasks failed", exceptions));
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
            var tcs = new RwTaskCompletionSource<T[]>();
            int remaining = span.Length;
            bool hasError = false;
            List<Exception> exceptions = null;
            T[] results = new T[span.Length];
            foreach (var task in span)
            {
                task.GetAwaiter().OnCompleted(() =>
                {
                    try
                    {
                        var result = task.GetAwaiter().GetResult();
                        results[span.Length - remaining] = result;
                    }
                    catch (Exception ex)
                    {
                        Volatile.Write(ref hasError, true);
                        if (exceptions == null)
                        {
                            exceptions = new List<Exception>();
                        }
                        exceptions.Add(ex);
                    }
                    if (Interlocked.Decrement(ref remaining) == 0)
                    {
                        if (Volatile.Read(ref hasError))
                            tcs.TrySetException(new AggregateException("One or more tasks failed", exceptions));
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
            var tcs = new RwTaskCompletionSource<bool>();
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
                task.GetAwaiter().OnCompleted(null); //释放tcs引用
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
            var tcs = new RwTaskCompletionSource<T>();
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
                task.GetAwaiter().OnCompleted(null); //释放tcs引用
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

            var tcs = new RwTaskCompletionSource<bool>();
            var ctr = token.Register(() => tcs.TrySetCanceled());

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
            var promise = RwTaskPromise.CreateCanceled(token);
            return promise.Task;
        }

        /// <summary>
        /// 创建一个处于已取消状态的带返回值的任务。
        /// </summary>
        public static RwTask<T> FromCanceled<T>(CancellationToken token)
        {
            var promise = RwTaskPromise<T>.CreateCanceled(token);
            return promise.Task;
        }

        /// <summary>
        /// 创建一个已成功完成的任务。
        public static RwTask FromeResult()
        {
            var promise = RwTaskPromise.CreateCompleted();
            return promise.Task;
        }

        /// <summary>
        /// 创建一个已成功完成的带返回值的任务。
        /// </summary>
        public static RwTask<T> FromResult<T>()
        {
            var promise = RwTaskPromise<T>.CreateCompleted();
            return promise.Task;
        }

        /// <summary>
        /// 测试样例
        /// </summary>
        /// <returns></returns>
        private static void NoAwait()
        {
            Test().Forget(); //一定要调用Forget以确保任务被正确回收（在不await和GetResult的情况下）
        }

        /// <summary>
        /// 测试样例
        /// </summary>
        /// <returns></returns>
        private static async RwTask Test()
        {
            SCHelperUtils.Log(await Test2());
        }


        /// <summary>
        /// 测试样例
        /// </summary>
        /// <returns></returns>
        private static async RwTask<DateTime> Test2()
        {
            SCHelperUtils.Log($"4 - {DateTime.Now}");
            await RwTask.DelayEarlyFrames(80);
            SCHelperUtils.Log($"3 - {DateTime.Now}");
            await RwTask.DelayEarlyFrames(80);
            SCHelperUtils.Log($"2 - {DateTime.Now}");
            await RwTask.DelayEarlyFrames(80);
            SCHelperUtils.Log($"1 - {DateTime.Now}");
            await RwTask.DelayEarlyFrames(80);
            return DateTime.Now;
        }

        private static async void SwitchToRwLoop()
        {
            //其他线程操作
            await RwTask.Yield();
            //主线程操作
        }
    }

    public readonly partial struct RwTask
    {
        private readonly IRwTaskSource _source;
        private readonly short _token;

        public RwTask(IRwTaskSource source, short token)
        {
            _source = source;
            _token = token;
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
                if (_task._source == null) return;
                ((RwTaskPromise)(_task._source)).GetResult(_task._token);
            }
            /// <summary>
            /// 注册在任务完成时调用的后续操作。
            /// </summary>
            public void OnCompleted(Action continuation)
            {
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
        /// 指示任务是否因异常而失败。
        /// </summary>
        public bool IsFaulted
        {
            get
            {
                if (_source == null) return false;
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

        public RwTask(T result)
        {
            _result = result;
            _source = null;
            _token = 0;
        }
        public RwTask(IRwTaskSource<T> source, short token)
        {
            _result = default;
            _source = source;
            _token = token;
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
                if (_task._source == null) return _task._result;
                return ((RwTaskPromise<T>)(_task._source)).GetResult(_task._token);
            }

            /// <summary>
            /// 注册在任务完成时调用的后续操作。
            /// </summary>
            public void OnCompleted(Action continuation)
            {
             
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
        /// 指示任务是否因异常而失败。
        /// </summary>
        public bool IsFaulted
        {
            get
            {
                if (_source == null) return false;
                return _source.GetStatus(_token) == RwTaskStatus.Faulted;
            }
        }
    }

}
