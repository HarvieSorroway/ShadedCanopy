using SCUtils.RwTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SCUtils.SCTween
{
    public interface ISCTweenContext
    {
        RwTask RunAsync(CancellationToken token = default);
    }

    public class SCTweenContext<T> : ISCTweenContext
    {
        public readonly Action<T> setValueFunction;
        public readonly T From, To;
        public int frames;
        public Func<float, float> easeFunction;
        public Func<T, T, float, T> lerpFunction;
        public Action finishCallBack;


        public SCTweenContext(
            Action<T> setValueFunction,
            T from,
            T to,
            int durationFrames,
            Func<T, T, float, T> lerpFunction)
        {
            this.setValueFunction = setValueFunction;
            From = from;
            To = to;
            frames = durationFrames;
            this.lerpFunction = lerpFunction;
        }

        public SCTweenContext<T> SetEase(Func<float, float> easeFunction)
        {
            this.easeFunction = easeFunction;
            return this;
        }

        public SCTweenContext<T> OnFinish(Action finishCallBack)
        {
            this.finishCallBack = finishCallBack;
            return this;
        }
        
        public async RwTask RunAsync(CancellationToken token = default)
        {
            for(int i = 0; i <= frames; i++)
            {
                if (token.IsCancellationRequested)
                    break;

                float t = (float)i / frames;
                if (easeFunction != null)
                    t = easeFunction(t);
                setValueFunction.Invoke(lerpFunction(From, To, t));
                await RwTasks.RwTask.Yield();
            } 
            finishCallBack?.Invoke();
        }
    }
}
