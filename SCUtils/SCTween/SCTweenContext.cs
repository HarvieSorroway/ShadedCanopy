using SCUtils.RwTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCUtils.SCTween
{
    /// <summary>
    /// 暂存补间动画信息的结构体。也可以暂存该结构体，以便获取动画完成状态。
    /// </summary>
    /// <typeparam name="T">动画目标类型</typeparam>
    public struct SCTweenContext<T>
    {
        internal T valueFrom, valueTo;
        internal int frames, i;
        internal int loop, looped;

        internal Action finishCallBack;
        internal Func<float, float> easeFunc;
        internal Func<T, T, float, T> lerpFunc;

        internal Action<T> setTargetFunc;

        bool _finished;
        /// <summary>
        /// 动画是否已经完成。
        /// </summary>
        public bool Finished => _finished;

        public SCTweenContext(Action<T> setTargetFunc, T from, T to, int frames, Func<T, T, float, T> lerpFunc)
        {
            this.setTargetFunc = setTargetFunc;
            this.valueFrom = from;
            this.valueTo = to;
            this.frames = frames;
            this.finishCallBack = null;
            this.easeFunc = null;
            this.lerpFunc = lerpFunc;
            _finished = false;
            i = 0;
            loop = looped = -1;
        }


        /// <summary>
        /// 设置非默认的插值行为，传入值为（from, to, t），返回插值结果。
        /// </summary>
        /// <param name="lerpFunc"></param>
        /// <returns></returns>
        public SCTweenContext<T> Do(Func<T, T, float, T> lerpFunc)
        {
            this.lerpFunc = lerpFunc;
            return this;
        }

        /// <summary>
        /// 设置补间动画的缓动函数，留空则为线性插值。
        /// </summary>
        /// <param name="easeFunc"></param>
        /// <returns></returns>
        public SCTweenContext<T> SetEase(Func<float, float> easeFunc)
        {
            this.easeFunc = easeFunc;
            return this;
        }


        /// <summary>
        /// 设置补间动画完成时的回调函数。
        /// </summary>
        /// <param name="callBack"></param>
        /// <returns></returns>
        public SCTweenContext<T> OnFinish(Action callBack)
        {
            this.finishCallBack = callBack;
            return this;
        }

        public SCTweenContext<T> Loop(int count = -1)
        {
            this.loop = count;
            return this;
        }
        

        /// <summary>
        /// 启动补间动画，返回一个可等待的任务，可调用Forget来减轻gc。
        /// </summary>
        /// <returns></returns>
        public async RwTask RunAsync()
        {
            do
            {
                looped++;
                for (i = 0; i < frames; i++)
                {
                    float t = (i + 1) / (float)frames;
                    if (easeFunc != null)
                        t = easeFunc.Invoke(t);

                    setTargetFunc.Invoke(lerpFunc.Invoke(valueFrom, valueTo, t));
                    SCHelperUtils.Log($"Tweening {typeof(T).Name}, frame {i}, value : {lerpFunc.Invoke(valueFrom, valueTo, t)}");
                    await RwTasks.RwTask.Yield();
                }
                SCHelperUtils.Log($"Tween {typeof(T).Name} Complete at {DateTime.Now}, value : {lerpFunc.Invoke(valueFrom, valueTo, 1f)}");
            } while ((loop > 0 && looped < loop));

            _finished = true;
            finishCallBack?.Invoke();
        }

        /// <summary>
        /// 同步方法，需要手动调用以推进动画。你真的会需要用这个吗？
        /// </summary>
        public void Run()
        {
            i++;
            float t = (i + 1) / (float)frames;
            if (easeFunc != null)
                t = easeFunc.Invoke(t);

            setTargetFunc.Invoke(lerpFunc.Invoke(valueFrom, valueTo, t));

            if (i >= frames)
            {
                looped++;
                if (loop < 0 || looped < loop)
                    i = 0;
                else
                {
                    _finished = true;
                    finishCallBack?.Invoke();
                }
            }
        }
    }
}
