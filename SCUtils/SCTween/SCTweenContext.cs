using SCUtils.RwTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCUtils.SCTween
{
    public class SCTweenContextBase
    {
        public virtual async RwTask RunAsync()
        {
        }
    }

    public class SCTweenContext<T> : SCTweenContextBase
        {
        public readonly StrongBox<T> boxedTarget;
        public readonly T From, To;
        public int frames;
        public Func<float, float> easeFunction;
        public Func<T, T, float, T> lerpFunction;
        public Action finishCallBack;


        public SCTweenContext(
            StrongBox<T> boxedTarget,
            T from,
            T to,
            int durationFrames,
            Func<T, T, float, T> lerpFunction)
        {
            this.boxedTarget = boxedTarget;
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
        
        public override async RwTask RunAsync()
        {
            for(int i = 0; i <= frames; i++)
            {
                float t = (float)i / frames;
                if (easeFunction != null)
                    t = easeFunction(t);
                boxedTarget.Value = lerpFunction(From, To, t);
                await RwTasks.RwTask.Yield();
                
                SCHelperUtils.Log($"Tween {typeof(T).Name} Complete at {DateTime.Now}, value : {boxedTarget.Value}");
            } 
            finishCallBack?.Invoke();
        }
    }
}
