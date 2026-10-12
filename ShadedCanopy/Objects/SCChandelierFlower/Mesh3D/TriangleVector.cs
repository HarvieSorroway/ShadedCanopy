using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShadedCanopy.Objects.SCChandelierFlower.Mesh3D
{
    using Triangle2D = Triangle<Vector2>;
    using Triangle3D = Triangle<Vector3>;
    using Triangle2DVector = List<Triangle<Vector2>>;
    using Triangle3DVector = List<Triangle<Vector3>>;

    public static class TriangleVectorExtensions
    {
        internal static void FillColor(this Triangle2DVector triangles, Color color)
        {
            for (int i = 0; i < triangles.Count; i++)
            {
                Triangle2D t = triangles[i];
                t.colorA = color;
                t.colorB = color;
                t.colorC = color;
                triangles[i] = t;
            }
        }
        internal static void FillColor(this Triangle3DVector triangles, Color color)
        {
            for (int i = 0; i < triangles.Count; i++)
            {
                Triangle3D t = triangles[i];
                t.colorA = color;
                t.colorB = color;
                t.colorC = color;
                triangles[i] = t;
            }
        }

        internal static void FillColor(this Triangle3DVector triangles, Color color, Func<Triangle3D, float> lerpFunc)
        {
            for (int i = 0; i < triangles.Count; i++)
            {
                Triangle3D t = triangles[i];
                float lerpValue = lerpFunc(t);
                t.colorA = Color.Lerp(t.colorA, color, lerpValue);
                t.colorB = Color.Lerp(t.colorB, color, lerpValue);
                t.colorC = Color.Lerp(t.colorC, color, lerpValue);
                triangles[i] = t;
            }
        }
        internal static void FillColor(this Triangle3DVector triangles, Color color, Func<Vector3, float> lerpFunc)
        {
            for (int i = 0; i < triangles.Count; i++)
            {
                Triangle3D t = triangles[i];
                t.colorA = Color.Lerp(t.colorA, color, lerpFunc(t.A));
                t.colorB = Color.Lerp(t.colorB, color, lerpFunc(t.B));
                t.colorC = Color.Lerp(t.colorC, color, lerpFunc(t.C));
                triangles[i] = t;
            }
        }

        internal static Vector2 ProjectTo2D(Vector3 point, Vector3 vanishingPoint)
        {
            // v.z<0:  z++ -> t--
            // v.z>0:  z++ -> t++
            // 如果屏幕外是z>0 那么消失点z是正的
            float t = -vanishingPoint.z / (point.z - vanishingPoint.z);
            // t--: more close to vanishingPoint
            return new Vector2(vanishingPoint.x + t * (point.x - vanishingPoint.x), vanishingPoint.y + t * (point.y - vanishingPoint.y));
        }

        internal static Triangle3DVector PositionOffset(this Triangle3DVector triangles, Vector3 offset)
        {
            Triangle3DVector ret = new();
            for (int i = 0; i < triangles.Count; i++)
            {
                Triangle3D t = triangles[i];
                if (t.hasUV)
                {
                    ret.Add(new Triangle3D(t.A + offset, t.B + offset, t.C + offset, t.colorA, t.colorB, t.colorC, t.uvA, t.uvB, t.uvC));
                } else
                {
                    ret.Add(new Triangle3D(t.A + offset, t.B + offset, t.C + offset, t.colorA, t.colorB, t.colorC));
                }
            }
            return ret;
        }
        internal static Triangle2DVector Projection2D(this Triangle3DVector triangles, Vector3? vanishingPoint=default)
        {
            Triangle2DVector projected = new();
            if (!vanishingPoint.HasValue || float.IsInfinity(vanishingPoint.Value.z))
            {   for (int i = 0; i < triangles.Count; i++)
                {
                    Triangle3D t = triangles[i];
                    float depth = (t.A.z + t.B.z + t.C.z) / 3;
                    if (t.hasUV)
                    {
                        projected.Add(new Triangle2D(t.A, t.B, t.C, t.colorA, t.colorB, t.colorC, t.uvA, t.uvB, t.uvC, depth));
                    }
                    else
                    {
                        projected.Add(new Triangle2D(t.A, t.B, t.C, t.colorA, t.colorB, t.colorC, depth));
                    }
                }
            }
            else
            {
                Vector3 vp = vanishingPoint.Value;
                for (int i = 0; i < triangles.Count; i++)
                {
                    Triangle3D t = triangles[i];
                    float depth = (t.A.z + t.B.z + t.C.z) / 3;
                    if (t.hasUV)
                    {
                        projected.Add(new Triangle2D(ProjectTo2D(t.A, vp), ProjectTo2D(t.B, vp), ProjectTo2D(t.C, vp), t.colorA, t.colorB, t.colorC, t.uvA, t.uvB, t.uvC, depth));
                    }
                    else
                    {
                        projected.Add(new Triangle2D(ProjectTo2D(t.A, vp), ProjectTo2D(t.B, vp), ProjectTo2D(t.C, vp), t.colorA, t.colorB, t.colorC, depth));
                    }
                }
            }
            return projected;
        }
        internal static ValueTuple<Triangle2DVector, Triangle2DVector> SplitByDepth(this Triangle2DVector triangles, float depthThreshold)
        {
            Triangle2DVector less = new();
            Triangle2DVector more = new();
            for (int i = 0; i < triangles.Count; i++)
            {
                Triangle2D t = triangles[i];
                if (t.d < depthThreshold)
                {
                    less.Add(t);
                }
                else
                {
                    more.Add(t);
                }
            }
            return (less, more);
        }
        internal static Triangle3DVector SplitTriangles(this Triangle3DVector input, float lengthThreshold)
        {
            Queue<Triangle3D> queue = new(input);
            Triangle3DVector ret = new();
            ValueTuple<int, int, int>[] orders = new (int, int, int)[]
            {
                (0, 1, 2),
                (1, 2, 0),
                (2, 0, 1),
            };
            while (queue.Any())
            {
                Triangle3D t = queue.Dequeue();
                var a = from orderId in Enumerable.Range(0, orders.Length)
                        let len = (t.GetPos(orders[orderId].Item1) - t.GetPos(orders[orderId].Item2)).sqrMagnitude
                        where len > lengthThreshold * lengthThreshold
                        select orderId;
                if (!a.Any())
                {
                    ret.Add(t);
                    continue;
                } else
                {
                    int oId = a.First();
                    float length = (t.GetPos(orders[oId].Item1) - t.GetPos(orders[oId].Item2)).magnitude;
                    int divCount = Mathf.CeilToInt(length / lengthThreshold);
                    Vector3[] poses = new Vector3[divCount + 1];
                    Color[] colors = new Color[divCount + 1];
                    Vector2[] uvs = t.hasUV ? new Vector2[divCount + 1] : null;

                    //float maxD = Mathf.Max((t.A - t.B).magnitude, (t.B - t.C).magnitude, (t.C - t.A).magnitude);
                    //SCPlugin.Logger.LogInfo($"Split tri maxLen {length} and will splitTo {divCount} parts, tri is {t.A}-{t.B}-{t.C}, maxD: {maxD}, queueLength: {queue.Count}");
                    for (int i = 0; i <= divCount; i++)
                    {
                        float sep = 1.0f * i / divCount;
                        poses[i] = Vector3.Lerp(t.GetPos(orders[oId].Item1), t.GetPos(orders[oId].Item2), sep);
                        colors[i] = Color.Lerp(t.GetColor(orders[oId].Item1), t.GetColor(orders[oId].Item2), sep);
                        if (t.hasUV)
                        {
                            uvs[i] = Vector2.Lerp(t.GetUV(orders[oId].Item1), t.GetUV(orders[oId].Item2), sep);
                        }
                    }
                    for (int i = 0; i < divCount; i++)
                    {
                        Triangle3D newTri;
                        if (t.hasUV)
                        {
                            newTri = new Triangle3D(poses[i], poses[i + 1], t.GetPos(orders[oId].Item3), colors[i], colors[i + 1], t.GetColor(orders[oId].Item3), uvs[i], uvs[i + 1], t.GetUV(orders[oId].Item3));
                        }
                        else
                        {
                            newTri = new Triangle3D(poses[i], poses[i + 1], t.GetPos(orders[oId].Item3), colors[i], colors[i + 1], t.GetColor(orders[oId].Item3));
                        }
                        float maxDist = Mathf.Max((newTri.A - newTri.B).magnitude, (newTri.B - newTri.C).magnitude, (newTri.C - newTri.A).magnitude);
                        //SCPlugin.Logger.LogInfo($"Split tri to new tri {newTri.A}-{newTri.B}-{newTri.C} with maxDist {maxDist}");
                        queue.Enqueue(newTri);
                    }
                }
            }
            return ret;
        }
        internal static TriangleMesh Export(this Triangle2DVector triangles, Vector2 offset=default)
        {
            TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[triangles.Count];
            for (int i = 0; i < triangles.Count; i++)
            {
                tris[i] = new TriangleMesh.Triangle(i * 3, i * 3 + 1, i * 3 + 2);
            }
            TriangleMesh ret = new TriangleMesh("Futile_White", tris, true);
            for (int i = 0; i < triangles.Count; i++)
            {
                Triangle2D t = triangles[i];
                ret.MoveVertice(i * 3, t.A + offset);
                ret.MoveVertice(i * 3 + 1, t.B + offset);
                ret.MoveVertice(i * 3 + 2, t.C + offset);
                ret.verticeColors[i * 3] = t.colorA;
                ret.verticeColors[i * 3 + 1] = t.colorB;
                ret.verticeColors[i * 3 + 2] = t.colorC;
                if (t.hasUV)
                {
                    ret.UVvertices[i * 3] = t.uvA;
                    ret.UVvertices[i * 3 + 1] = t.uvB;
                    ret.UVvertices[i * 3 + 2] = t.uvC;
                }
            }
            return ret;
        }
    }
}
