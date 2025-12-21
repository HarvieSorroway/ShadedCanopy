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
        /// <returns>返回一个等待任务。</returns>
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
        /// <returns>返回一个等待任务。</returns>
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
        /// <returns>返回一个等待任务。</returns>
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
        /// <returns>返回一个等待任务。</returns>
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
        /// <returns>返回一个等待任务。</returns>
        public static RwTask YieldEarly(CancellationToken token = default)
        {
            var promise = RwTaskPromise.Create(0, token);
            RwLoopRunner.EarlyUpdateRunner.Schedule(promise);
            return promise.Task;
        }

        /// <summary>
        /// 挂起当前任务，直到下一帧的 Update 结束阶段。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        /// <returns>返回一个等待任务。</returns>
        public static RwTask Yield(CancellationToken token = default)
        {
            var promise = RwTaskPromise.Create(0, token);
            RwLoopRunner.LateUpdateRunner.Schedule(promise);
            return promise.Task;
        }

        /// <summary>
        /// 挂起当前任务，直到下一帧的 RawUpdate 开始阶段。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        /// <returns>返回一个等待任务。</returns>
        public static RwTask YieldEarlyRaw(CancellationToken token = default)
        {
            var promise = RwTaskPromise.Create(0, token);
            RwLoopRunner.EarlyRawUpdateRunner.Schedule(promise);
            return promise.Task;
        }

        /// <summary>
        /// 挂起当前任务，直到下一帧的 RawUpdate 结束阶段。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        /// <returns>返回一个等待任务。</returns>
        public static RwTask YieldRaw(CancellationToken token = default)
        {
            var promise = RwTaskPromise.Create(0, token);
            RwLoopRunner.LateRawUpdateRunner.Schedule(promise);
            return promise.Task;
        }

        /// <summary>
        /// 无限期挂起，直到传入的 CancellationToken 被取消。
        /// </summary>
        /// <param name="token">用于触发取消的令牌</param>
        public static RwTask WaitCanceled(CancellationToken token)
            => DelayFrames(-1, token);

        /// <summary>
        /// 无限期挂起，直到传入的 CancellationTokenSource 被取消。
        /// </summary>
        /// <param name="cts">取消源，如果为 null 则使用默认 token</param>
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
        /// <param name="tokens">Token 数组</param>
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

        public static readonly RwTask CompletedTask = new RwTask(null, 0);


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

            public bool IsCompleted
            {
                get
                {
                    if (_task._source == null) return true;
                    return _task._source.GetStatus(_task._token) != RwTaskStatus.Pending;
                }
            }

            public void GetResult()
            {
                if (_task._source == null) return;
                ((RwTaskPromise)(_task._source)).GetResult(_task._token);
            }

            public void OnCompleted(Action continuation)
            {
                _task._source.OnCompleted(continuation, _task._token);
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

        public static readonly RwTask CompletedTask = new RwTask(null, 0);


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

            public bool IsCompleted
            {
                get
                {
                    if (_task._source == null) return true;
                    return _task._source.GetStatus(_task._token) != RwTaskStatus.Pending;
                }
            }

            public T GetResult()
            {
                if (_task._source == null) return _task._result;
                return ((RwTaskPromise<T>)(_task._source)).GetResult(_task._token);
            }

            public void OnCompleted(Action continuation)
            {
             
                _task._source.OnCompleted(continuation, _task._token);
            }
        }
    }

}
