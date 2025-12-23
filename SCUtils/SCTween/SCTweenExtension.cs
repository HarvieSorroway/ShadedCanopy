using JetBrains.Annotations;
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
    internal static class SCTweenExtension
    {
        static float test1, test2;
        public static void Test()
        {
            SCHelperUtils.Log($"Tween Float Start at {DateTime.Now}");
            test1.TweenFloat(0f, 1f, 2f).RunAsync().Forget();

            SCHelperUtils.Log($"Tween Float2 Start at {DateTime.Now}");
            test2.TweenFloat(1f, 3f, 3f).RunAsync().Forget();
        }
        public static SCTweenContext<float> TweenFloat(this ref float target, float from, float to, float duration)
        {
            SCUtils.Log($"Tween Float Start at {DateTime.Now}, from {from} - to {to}, frames {Sec2Frame(duration)}");

            StrongBox<float> box = new StrongBox<float>(target);

            return new SCTweenContext<float>(
                box,
                from,
                to,
                Sec2Frame(duration),
                Mathf.Lerp
                );
        }

        public static SCTweenContext<Vector2> TweenVector2(this ref Vector2 target, Vector2 from, Vector2 to, float duration)
        {
            SCUtils.Log($"Tween Vector2 Start at {DateTime.Now}, from {from} - to {to}, frames {Sec2Frame(duration)}");

            StrongBox<Vector2> box = new StrongBox<Vector2>(target);
            return new SCTweenContext<Vector2>(
                box,
                from,
                to,
                Sec2Frame(duration),
                Vector2.Lerp
                );
        }

        public static SCTweenContext<Color> TweenColor(this Color target, Color from, Color to, float duration)
        {
            SCUtils.Log($"Tween Color Start at {DateTime.Now}, from {from} - to {to}, frames {Sec2Frame(duration)}");

            StrongBox<Color> box = new StrongBox<Color>(target);
            return new SCTweenContext<Color>(
                box,
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
