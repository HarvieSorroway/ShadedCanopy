using BepInEx.Logging;
using SCUtils.DevToolUtils;
using SCUtils.SCDevTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace SCUtils
{
    public static class SCUtils
    {
        static ManualLogSource logger;
        public static void Init(ManualLogSource manualLogSource)
        {
            logger = manualLogSource;
            PlacedObjectExt.Init();
            SCDevToolsEntry.Init();
        }

        public static void Log(string log)
        {
            logger.LogDebug(log);
        }

        #region AnimationEasings
        public static float LerpEase(float t)
        {
            return Mathf.Lerp(t, 1f, Mathf.Pow(t, 0.5f));
        }

        public static float EaseInOutSine(float t)
        {
            return -(Mathf.Cos(Mathf.PI * t) - 1) / 2;
        }

        public static float EaseInOutCubic(float f)
        {
            return f < 0.5 ? 4 * f * f * f : 1 - Mathf.Pow(-2 * f + 2, 3) / 2;
        }

        public static float EaseOutElastic(float t)
        {
            if (t == 0)
                return 0f;
            if (t == 1)
                return 1f;

            float p = 1f * .3f;
            float a = 1f;
            float s = p / 4;
            return (a * Mathf.Pow(2, -10 * t) * Mathf.Sin((t * 1f - s) * (2 * Mathf.PI) / p) + 1f) * 0.5f + t * 0.5f;
        }
        #endregion
    }
}
