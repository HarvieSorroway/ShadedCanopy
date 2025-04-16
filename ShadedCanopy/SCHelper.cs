using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShadedCanopy
{
    internal static class SCHelper
    {
        public static float EaseInOutCubic(float f)
        {
            return f < 0.5 ? 4 * f * f * f : 1 - Mathf.Pow(-2 * f + 2, 3) / 2;
        }
    }
}
