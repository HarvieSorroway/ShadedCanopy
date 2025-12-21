using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SCUtils.RwTasks
{
    public static class RwTaskExtensions
    {
        /// <summary>
        /// 对任务进行忽略处理，并在发生异常时执行回调。
        /// </summary>
        /// <remarks>
        /// <para>
        /// <strong>重要提示：</strong> 
        /// 由于 <see cref="RwTask"/> 使用了对象池机制，实例的回收依赖于 <c>GetResult()</c> 的调用（通常由 <c>await</c> 触发）。
        /// </para>
        /// <para>
        /// 此方法会在内部调用 <c>GetResult()</c>，从而确保 Task 实例被正确重置并归还到对象池中，防止内存泄漏。
        /// </para>
        /// </remarks>
        /// <param name="task">需要忽略等待的任务实例。</param>
        /// <param name="onException">当任务执行过程中抛出异常时的回调处理。如果为 null，异常将被忽略。</param>
        public static void Forget(this RwTask task, Action<Exception> onException = null)
        {
            var awaiter = task.GetAwaiter();
            if (awaiter.IsCompleted)
            {
                try { awaiter.GetResult(); }
                catch (Exception ex) { onException?.Invoke(ex); }
                return;
            }

            awaiter.OnCompleted(() =>
            {
                try { awaiter.GetResult(); }
                catch (Exception ex) { onException?.Invoke(ex); }
            });
        }

        /// <summary>
        /// 对带返回值的任务进行忽略处理，并在发生异常时执行回调。
        /// </summary>
        /// <remarks>
        /// <para>
        /// <strong>重要提示：</strong> 
        /// 由于 <see cref="RwTask{T}"/> 使用了对象池机制，实例的回收依赖于 <c>GetResult()</c> 的调用（通常由 <c>await</c> 触发）。
        /// </para>
        /// <para>
        /// 此方法会在内部调用 <c>GetResult()</c>，从而确保 Task 实例被正确重置并归还到对象池中，防止内存泄漏。
        /// </para>
        /// </remarks>
        /// <typeparam name="T">任务返回值的类型。</typeparam>
        /// <param name="task">需要忽略等待的任务实例。</param>
        /// <param name="onException">当任务执行过程中抛出异常时的回调处理。如果为 null，异常将被忽略。</param>
        public static void Forget<T>(this RwTask<T> task, Action<Exception> onException = null)
        {
            var awaiter = task.GetAwaiter();
            if (awaiter.IsCompleted)
            {
                try { _ = awaiter.GetResult(); }
                catch (Exception ex) { onException?.Invoke(ex); }
                return;
            }

            awaiter.OnCompleted(() =>
            {
                try { _ = awaiter.GetResult(); }
                catch (Exception ex) { onException?.Invoke(ex); }
            });
        }

        /// <summary>
        /// 异步等待，直到该游戏对象被销毁。
        /// </summary>
        /// <param name="obj">目标对象</param>
        public static RwTask WaitObjectDestroy(this UpdatableAndDeletable obj)
        {
            var token = obj.GetDestroyToken();
            return RwTask.WaitCanceled(token);
        }

        /// <summary>
        /// 获取与特定对象（UpdatableAndDeletable）绑定的销毁 Token。
        /// </summary>
        /// <remarks>
        /// 当该对象需要通知销毁时，这个 Token 会被触发。
        /// </remarks>
        public static CancellationToken GetDestroyToken(this UpdatableAndDeletable obj)
        {
            return _objDestroyTokens.GetValue(obj, key =>
            {
                var cts = new CancellationTokenSource();
                return cts;
            }).Token;
        }

        /// <summary>
        /// 尝试获取该对象的销毁 TokenSource。
        /// </summary>
        /// <returns></returns>
        internal static bool TryGetDestroyTokenSource(this UpdatableAndDeletable obj, out CancellationTokenSource cts)
        {
            if (_objDestroyTokens.TryGetValue(obj, out cts))
                return true;
            return false;
        }


        private static ConditionalWeakTable<UpdatableAndDeletable, CancellationTokenSource> _objDestroyTokens = new();
    }
}
