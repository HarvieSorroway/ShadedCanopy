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

        public static Vector3 BezierLerp(Vector3 a0, Vector3 a1, Vector3 b0, Vector3 b1, float t)
        {
            // 不是均匀插值
            Vector3 p = Vector3.Lerp(a0, b0, t);
            Vector3 p0 = Vector3.Lerp(a0, a1, t);
            Vector3 p1 = Vector3.Lerp(b1, b0, t);
            p0 = Vector3.Lerp(p0, p, t);
            p1 = Vector3.Lerp(p, p1, t);
            return Vector3.Lerp(p0, p1, t);
            // 计算的是四个控制点的三次贝塞尔曲线的插值
        }

        // 数值计算贝塞尔曲线在长度层面插值的坐标
        public class BezierUniformLerpPrepare
        {
            Vector3 a0, a1, b0, b1;
            float[] length;
            int maxDepth;
            public BezierUniformLerpPrepare(Vector3 a0, Vector3 a1, Vector3 b0, Vector3 b1, int maxDepth)
            {
                this.a0 = a0;
                this.a1 = a1;
                this.b0 = b0;
                this.b1 = b1;
                this.maxDepth = maxDepth;
                // max_depth=0: [1]
                // max_depth=1: [3]
                // max_depth=2: [7]
                length = new float[(1 << (maxDepth + 1))];
                CalcLength(1, 0, 1, 0);
            }
            void CalcLength(int i, float l, float r, int depth)
            {
                if (depth == maxDepth)
                {
                    length[i] = (BezierLerp(a0, a1, b0, b1, r) - BezierLerp(a0, a1, b0, b1, l)).magnitude;
                }
                else
                {
                    float mid = (l + r) / 2;
                    CalcLength(i * 2, l, mid, depth + 1);
                    CalcLength(i * 2 + 1, mid, r, depth + 1);
                    length[i] = length[i * 2] + length[i * 2 + 1];
                }
            }
            float FindBestLerp(int i, float l, float r, float targeti, int depth)
            {
                if (depth == maxDepth)
                {
                    return l + (r - l) * targeti;
                }
                else
                {
                    float mid = (l + r) / 2;
                    float leftRatio = length[i * 2] / length[i];
                    if (targeti <= leftRatio)
                    {
                        return FindBestLerp(i * 2, l, mid, targeti / leftRatio, depth + 1);
                    }
                    else
                    {
                        return FindBestLerp(i * 2 + 1, mid, r, (targeti - leftRatio) / (1 - leftRatio), depth + 1);
                    }
                }
            }
            public Vector3 BezierUniformLerp(float t)
            {
                float realT = FindBestLerp(1, 0, 1, t, 0);
                return BezierLerp(a0, a1, b0, b1, realT);
            }
        }
    }
}
