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
    public static class SCTweenExtension
    {
        static float test1, test2;
        static Vector2 testV1;

        public static void Test()
        {
            test1.TweenFloat(0f, 1f, 2f).SetEase(SCHelperUtils.LerpEase).RunAsync().Forget();

            //testV1.TweenVector2(Vector2.zero, Vector2.right,3f).OnFinish(() =>
            //{
            //    SCHelperUtils.Log("Vector2 Tween Finished");
            //}).RunAsync().Forget();
        }
        public static SCTweenContext<float> TweenFloat(this float target, float from, float to, float duration)
        {
            SCHelperUtils.Log($"Tween Float Start at {DateTime.Now}, from {from} - to {to}, frames {Sec2Frame(duration)}");

            return new SCTweenContext<float>(
                (v) =>  target = v,
                from,
                to,
                Sec2Frame(duration),
                Mathf.Lerp
                );
        }

        public static SCTweenContext<Vector2> TweenVector2(this Vector2 target, Vector2 from, Vector2 to, float duration)
        {
            SCHelperUtils.Log($"Tween Vector2 Start at {DateTime.Now}, from {from} - to {to}, frames {Sec2Frame(duration)}");
            return new SCTweenContext<Vector2>(
                (v) => target = v,
                from,
                to,
                Sec2Frame(duration),
                Vector2.Lerp
                );
        }

        public static SCTweenContext<Color> TweenColor(this Color target, Color from, Color to, float duration)
        {
            SCHelperUtils.Log($"Tween Color Start at {DateTime.Now}, from {from} - to {to}, frames {Sec2Frame(duration)}");
            return new SCTweenContext<Color>(
                (v) => target = v,
                from,
                to,
                Sec2Frame(duration),
                Color.Lerp
                );
        }

        static int Sec2Frame(float sec)
        {
            return Mathf.CeilToInt(sec * 40);
        }
    }
}
