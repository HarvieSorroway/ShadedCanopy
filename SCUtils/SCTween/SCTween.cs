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
    public static class SCTween
    {
        static float test1, test2;
        static Vector2 testV1;

        public static void Test()
        {
        }
        public static SCTweenContext<float> TweenFloat(Action<float> setVal, float from, float to, float duration)
        {
            SCUtils.Log($"Tween Float Start at {DateTime.Now}, from {from} - to {to}, frames {Sec2Frame(duration)}");

            return new SCTweenContext<float>(
                setVal,
                from,
                to,
                Sec2Frame(duration),
                Mathf.Lerp
                );
        }

        public static SCTweenContext<Vector2> TweenVector2(Action<Vector2> setVal, Vector2 from, Vector2 to, float duration)
        {
            SCUtils.Log($"Tween Vector2 Start at {DateTime.Now}, from {from} - to {to}, frames {Sec2Frame(duration)}");

            return new SCTweenContext<Vector2>(
                setVal,
                from,
                to,
                Sec2Frame(duration),
                Vector2.Lerp
                );
        }
        
        public static SCTweenContext<Color> TweenColor(Action<Color> setVal, Color from, Color to, float duration)
        {
            SCUtils.Log($"Tween Color Start at {DateTime.Now}, from {from} - to {to}, frames {Sec2Frame(duration)}");

            return new SCTweenContext<Color>(
                setVal,
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
