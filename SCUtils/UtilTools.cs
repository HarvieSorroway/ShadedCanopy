using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCUtils
{
    public static class UtilTools
    {
        public static Color ColorRandomLerp(Color a, Color b)
        {
            float rr = UnityEngine.Random.Range(a.r, b.r),
                rg = UnityEngine.Random.Range(a.g, b.g),
                rb = UnityEngine.Random.Range(a.b, b.b),
                ra = UnityEngine.Random.Range(a.a, b.a);
            return new Color(rr, rg, rb, ra);
        }
    }
}
