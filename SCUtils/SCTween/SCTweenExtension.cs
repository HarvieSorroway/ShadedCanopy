using JetBrains.Annotations;
using SCUtils.RwTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCUtils.SCTween
{
    internal static class SCTweenExtension
    {
        static float test1, test2;
        public static void Test()
        {
            SCHelperUtils.Log($"Tween Float Start at {DateTime.Now}");
            test1.TestTweenFloat(0f, 1f, 2f).Forget();

            SCHelperUtils.Log($"Tween Float2 Start at {DateTime.Now}");
            test2.TestTweenFloat(1f, 3f, 3f).Forget();
        }
        public static async RwTask TestTweenFloat(this float target, float from, float to, float duration)
        {
            int frame = Mathf.CeilToInt(duration * 40);

            for(int i = 0;i < frame;i++)
            {
                float t = (i + 1) / (float)frame;
                target = Mathf.Lerp(from, to, t);
                await RwTasks.RwTask.Yield();
            }
            SCHelperUtils.Log($"Tween Float Complete at {DateTime.Now}, value : {target}");
        }
    }
}
