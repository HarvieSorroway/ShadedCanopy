using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Lumin;

namespace ShadedCanopy.Objects.SCChandelierFlower.Mesh3D
{
    using Triangle2D = Triangle<Vector2>;
    using Triangle3D = Triangle<Vector3>;
    using Triangle2DVector = List<Triangle<Vector2>>;
    using Triangle3DVector = List<Triangle<Vector3>>;
    internal class TriangleMesh3D: ICloneable
    {
        public Vector3[] p { get; private set; }
        public ValueTuple<int, int, int>[] t;
        Color[] c, backC;
        Vector2[] uv;
        public bool keepBackSurface;
        public TriangleMesh3D(int numVertices, int numTriangles)
        {
            p = new Vector3[numVertices];
            t = new ValueTuple<int, int, int>[numTriangles];
            c = new Color[numVertices];
            backC = null;
            uv = null;
            keepBackSurface = false;
        }
        public TriangleMesh3D(int numVertices, int[][] triangles) : this(numVertices, triangles.Length)
        {
            for (int i = 0; i < triangles.Length; i++)
            {
                t[i] = (triangles[i][0], triangles[i][1], triangles[i][2]);
            }
        }
        public TriangleMesh3D(int numVertices, ValueTuple<int, int, int>[] triangles) : this(numVertices, triangles.Length)
        {
            for (int i = 0; i < triangles.Length; i++)
            {
                t[i] = triangles[i];
            }
        }
        public TriangleMesh3D(Vector3[] vertices, int[][] triangles) : this(vertices.Length, triangles)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                p[i] = vertices[i];
            }
        }
        public TriangleMesh3D(Vector3[] vertices, ValueTuple<int, int, int>[] triangles) : this(vertices, triangles.Length)
        {
            for (int i = 0; i < triangles.Length; i++)
            {
                t[i] = triangles[i];
            }
        }
        public TriangleMesh3D(Vector3[] vertices, int numTriangles) : this(vertices.Length, numTriangles)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                p[i] = vertices[i];
            }
        }
        public void SetColor(int vert, Color color)
        {
            c[vert] = color;
        }
        public Color GetColor(int vert)
        {
            return c[vert];
        }
        public void FillColor(Color color)
        {
            for (int i = 0; i < c.Length; i++)
            {
                c[i] = color;
            }
        }
        public void FillBackColor(Color color)
        {
            if (backC == null)
            {
                backC = new Color[c.Length];
            }
            for (int i = 0; i < backC.Length; i++)
            {
                backC[i] = color;
            }
        }
        public void SetBackColor(int vert, Color color)
        {
            if (backC == null)
            {
                backC = new Color[c.Length];
            }
            backC[vert] = color;
        }
        public void SetUV(int vert, Vector2 uv)
        {
            if (this.uv == null)
            {
                this.uv = new Vector2[p.Length];
            }
            this.uv[vert] = uv;
        }
        public static bool FacingFront(Vector3 pa, Vector3 pb, Vector3 pc)
        {
            // 逆时针为正面，顺时针为背面
            Vector3 normal = Vector3.Cross(pb - pa, pc - pa);
            return normal.z < 0;
        }
        public static TriangleMesh3D Concat(TriangleMesh3D[] triMesh3d)
        {
            if (triMesh3d == null || triMesh3d.Length == 0)
            {
                throw new ArgumentException("Invalid triangle mesh array.");
            }

            int totalVertices = triMesh3d.Sum(mesh => mesh.numVertices);
            int totalTriangles = triMesh3d.Sum(mesh => mesh.numTriangles);
            TriangleMesh3D result = new TriangleMesh3D(totalVertices, totalTriangles);
            result.keepBackSurface = triMesh3d.Any(mesh => mesh.keepBackSurface);

            int vertexOffset = 0;
            int triangleOffset = 0;
            bool hasUV = false, hasBackColor = false;
            foreach (var mesh in triMesh3d)
            {
                Array.Copy(mesh.p, 0, result.p, vertexOffset, mesh.numVertices);
                Array.Copy(mesh.t, 0, result.t, triangleOffset, mesh.numTriangles);
                for (int i = triangleOffset; i < triangleOffset + mesh.numTriangles; i++)
                {
                    var (a, b, c) = result.t[i];
                    result.t[i] = (a + vertexOffset, b + vertexOffset, c + vertexOffset);
                }
                Array.Copy(mesh.c, 0, result.c, vertexOffset, mesh.numVertices);
                hasUV |= mesh.uv != null;
                hasBackColor |= mesh.backC != null;
                vertexOffset += mesh.p.Length;
                triangleOffset += mesh.t.Length;
            }
            if (hasUV)
            {
                result.uv = new Vector2[totalVertices];
                vertexOffset = 0;
                foreach (var mesh in triMesh3d)
                {
                    if (mesh.uv != null)
                    {
                        Array.Copy(mesh.uv, 0, result.uv, vertexOffset, mesh.uv.Length);
                    }
                    vertexOffset += mesh.p.Length;
                }
            }
            if (hasBackColor)
            {
                result.backC = new Color[totalVertices];
                vertexOffset = 0;
                foreach (var mesh in triMesh3d)
                {
                    if (mesh.backC != null)
                    {
                        Array.Copy(mesh.backC, 0, result.backC, vertexOffset, mesh.backC.Length);
                    } else if (mesh.keepBackSurface)
                    {
                        Array.Copy(mesh.c, 0, result.backC, vertexOffset, mesh.c.Length);
                    }
                    vertexOffset += mesh.p.Length;
                }
            }
            return result;
        }
        public static TriangleMesh3D Concat(IEnumerable<TriangleMesh3D> triMesh3d)
        {
            return Concat(triMesh3d.ToArray());
        }
        public void Rotate(Quaternion rot, Vector3 pivot)
        {
            for (int i = 0; i < p.Length; i++)
            {
                Vector3 dir = p[i] - pivot;
                dir = rot * dir;
                p[i] = pivot + dir;
            }
        }
        public void Rotate(Quaternion rot)
        {
            for (int i = 0; i < p.Length; i++)
            {
                p[i] = rot * p[i];
            }
        }
        public int numVertices => p.Length;
        public int numTriangles => t.Length;
        public void RotateX(float angleDeg) { Rotate(Quaternion.Euler(angleDeg, 0, 0)); }
        public void RotateY(float angleDeg) { Rotate(Quaternion.Euler(0, angleDeg, 0)); }
        public void RotateZ(float angleDeg) { Rotate(Quaternion.Euler(0, 0, angleDeg)); }
        public void RotateX(float angleDeg, Vector3 pivot) { Rotate(Quaternion.Euler(angleDeg, 0, 0), pivot); }
        public void RotateY(float angleDeg, Vector3 pivot) { Rotate(Quaternion.Euler(0, angleDeg, 0), pivot); }
        public void RotateZ(float angleDeg, Vector3 pivot) { Rotate(Quaternion.Euler(0, 0, angleDeg), pivot); }
        public TriangleMesh3D Clone()
        {
            TriangleMesh3D ret = new TriangleMesh3D(p, numTriangles);
            ret.keepBackSurface = this.keepBackSurface;
            for (int i = 0; i < t.Length; i++)
            {
                ret.t[i] = t[i];
            }
            for (int i = 0; i < c.Length; i++)
            {
                ret.c[i] = c[i];
            }
            if (backC != null)
            {
                ret.backC = new Color[backC.Length];
                for (int i = 0; i < backC.Length; i++)
                {
                    ret.backC[i] = backC[i];
                }
            }
            if (uv != null)
            {
                ret.uv = new Vector2[uv.Length];
                for (int i = 0; i < uv.Length; i++)
                {
                    ret.uv[i] = uv[i];
                }
            }
            return ret;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public static TriangleMesh3D operator +(TriangleMesh3D a, TriangleMesh3D b)
        {
            return Concat(new TriangleMesh3D[] { a, b });
        }
        public Triangle3DVector Export()
        {
            Triangle3DVector triangles = new Triangle3DVector();
            for (int i = 0; i < t.Length; i++)
            {
                var (ia, ib, ic) = t[i];
                var (pa, pb, pc) = (p[ia], p[ib], p[ic]);
                var (ca, cb, cc) = (c[ia], c[ib], c[ic]);
                if (!FacingFront(pa, pb, pc))
                {
                    if (backC == null && !keepBackSurface)
                    {
                        continue;
                    }
                    if (backC != null)
                    {
                        ca = backC[ia];
                        cb = backC[ib];
                        cc = backC[ic];
                    }

                }
                Triangle3D tri = new Triangle3D(pa, pb, pc, ca, cb, cc);
                if (uv != null)
                {
                    tri.uvA = uv[ia];
                    tri.uvB = uv[ib];
                    tri.uvC = uv[ic];
                }
                triangles.Add(tri);
            }
            return triangles;
        }
        public TriangleMesh3D AdaptiveSplitTriangle(int targetTriangleCount)
        {
            if (targetTriangleCount <= numTriangles)
            {
                return Clone();
            }
            List<Vector3> verts = new List<Vector3>(p);
            List<Color> colors = new List<Color>(c);
            List<ValueTuple<int, int, int>> tris = new();
            List<Color> backCs = null;
            List<Vector2> uvs = null;
            if (this.backC != null)
            {
                backCs = new List<Color>(backC);
            }
            if (this.uv != null)
            {
                uvs = new List<Vector2>(uv);
            }
            ValueTuple<float, int, int> GetCost(ValueTuple<int, int, int> tri)
            {
                var (ia, ib, ic) = tri;
                var (pa, pb, pc) = (verts[ia], verts[ib], verts[ic]);
                float dab = (pa - pb).magnitude;
                float dbc = (pb - pc).magnitude;
                float dca = (pc - pa).magnitude;
                if (dab >= dbc && dab >= dca)
                {
                    return (dab, Mathf.Max(ia, ib), Mathf.Min(ia, ib));
                }
                else if (dbc >= dab && dbc >= dca)
                {
                    return (dbc, Mathf.Max(ib, ic), Mathf.Min(ib, ic));
                }
                return (dca, Mathf.Max(ic, ia), Mathf.Min(ic, ia));
            }
            PriorityQueue<ValueTuple<int, int, int>, ValueTuple<float, int, int>> pq = new(isMaxHeap: true);
            foreach (var ti in t)
            {
                pq.Enqueue(ti, GetCost(ti));
            }
            while (pq.Count < targetTriangleCount)
            {
                pq.TryPeek(out var tri, out var cost);
                pq.Dequeue();
                List<ValueTuple<int, int, int>> relaTris = new();
                relaTris.Add(tri);
                while (!pq.IsEmpty)
                {
                    pq.TryPeek(out var nextTri, out var nextCost);
                    if (nextCost != cost)
                    {
                        break;
                    }
                    pq.Dequeue();
                    relaTris.Add(nextTri);
                }
                int pa = cost.Item2, pb = cost.Item3;
                verts.Add(Vector3.Lerp(verts[pa], verts[pb], 0.5f));
                colors.Add(Color.Lerp(colors[pa], colors[pb], 0.5f));
                if (backCs!=null)
                {
                    backCs.Add(Color.Lerp(backCs[pa], backCs[pb], 0.5f));
                }
                if (uvs != null)
                {
                    uvs.Add(Vector2.Lerp(uvs[pa], uvs[pb], 0.5f));
                }
                int newVertIndex = verts.Count - 1;
                foreach (var relaTri in relaTris)
                {
                    ValueTuple<int, int, int> newTri1, newTri2;
                    var (ia, ib, ic) = relaTri;
                    if ((ia == pa && ib == pb) || (ia == pb && ib == pa))
                    {
                        newTri1 = (ia, newVertIndex, ic);
                        newTri2 = (newVertIndex, ib, ic);
                    }
                    else if ((ib == pa && ic == pb) || (ib == pb && ic == pa))
                    {
                        newTri1 = (ia, ib, newVertIndex);
                        newTri2 = (ia, newVertIndex, ic);
                    }
                    else if ((ic == pa && ia == pb) || (ic == pb && ia == pa))
                    {
                        newTri1 = (newVertIndex, ib, ic);
                        newTri2 = (ia, ib, newVertIndex);
                    } else
                    {
                        throw new ArgumentException("Invalid triangle for splitting.");
                    }
                    pq.Enqueue(newTri1, GetCost(newTri1));
                    pq.Enqueue(newTri2, GetCost(newTri2));
                }
            }
            List<ValueTuple<int, int, int>> finalTris = new List<ValueTuple<int, int, int>>();
            foreach (var tri in pq)
            {
                finalTris.Add(tri);
            }
            TriangleMesh3D ret = new TriangleMesh3D(verts.ToArray(), finalTris.ToArray());
            ret.keepBackSurface = this.keepBackSurface;
            for (int i = 0; i < ret.numVertices; ++i)
            {
                ret.SetColor(i, colors[i]);
                if (backCs != null)
                {
                    ret.SetBackColor(i, backCs[i]);
                }
                if (uvs != null)
                {
                    ret.SetUV(i, uvs[i]);
                }
            }
            SCPlugin.Logger.LogInfo($"Adaptive split triangle mesh to {ret.numVertices} verts and {ret.numTriangles} triangles.");
            return ret;
        }

        public TriangleMesh3D DeterministicSplitTriangle(int targetTriangleCount)
        {
            if (targetTriangleCount <= numTriangles)
            {
                return Clone();
            }
            List<Vector3> verts = new List<Vector3>(p);
            List<Color> colors = new List<Color>(c);
            List<Color> backCs = null;
            List<Vector2> uvs = null;
            if (this.backC != null)
            {
                backCs = new List<Color>(backC);
            }
            if (this.uv != null)
            {
                uvs = new List<Vector2>(uv);
            }
            ValueTuple<int, int> GetEdge(ValueTuple<int, int, int> tri)
            {
                var (ia, ib, ic) = tri;
                ValueTuple<int, int> ab = (Mathf.Max(ia, ib), Mathf.Min(ia, ib));
                ValueTuple<int, int> bc = (Mathf.Max(ib, ic), Mathf.Min(ib, ic));
                ValueTuple<int, int> ca = (Mathf.Max(ic, ia), Mathf.Min(ic, ia));
                if (ab.CompareTo(bc) >= 0 && ab.CompareTo(ca) >= 0) return ab;
                if (bc.CompareTo(ab) >= 0 && bc.CompareTo(ca) >= 0) return bc;
                return ca;
            }

            PriorityQueue<ValueTuple<int, int, int>, ValueTuple<int, int>> pq = new(isMaxHeap: true);
            foreach (var ti in t)
            {
                pq.Enqueue(ti, GetEdge(ti));
            }
            while (pq.Count < targetTriangleCount)
            {
                pq.TryPeek(out var tri, out var edge);
                pq.Dequeue();
                List<ValueTuple<int, int, int>> relaTris = new();
                relaTris.Add(tri);
                while (!pq.IsEmpty)
                {
                    pq.TryPeek(out var nextTri, out var nextEdge);
                    if (nextEdge != edge)
                    {
                        break;
                    }
                    pq.Dequeue();
                    relaTris.Add(nextTri);
                }
                int pa = edge.Item1, pb = edge.Item2;
                verts.Add(Vector3.Lerp(verts[pa], verts[pb], 0.5f));
                colors.Add(Color.Lerp(colors[pa], colors[pb], 0.5f));
                if (backCs != null)
                {
                    backCs.Add(Color.Lerp(backCs[pa], backCs[pb], 0.5f));
                }
                if (uvs != null)
                {
                    uvs.Add(Vector2.Lerp(uvs[pa], uvs[pb], 0.5f));
                }
                int newVertIndex = verts.Count - 1;
                foreach (var relaTri in relaTris)
                {
                    ValueTuple<int, int, int> newTri1, newTri2;
                    var (ia, ib, ic) = relaTri;
                    if ((ia == pa && ib == pb) || (ia == pb && ib == pa))
                    {
                        newTri1 = (ia, newVertIndex, ic);
                        newTri2 = (newVertIndex, ib, ic);
                    }
                    else if ((ib == pa && ic == pb) || (ib == pb && ic == pa))
                    {
                        newTri1 = (ia, ib, newVertIndex);
                        newTri2 = (ia, newVertIndex, ic);
                    }
                    else if ((ic == pa && ia == pb) || (ic == pb && ia == pa))
                    {
                        newTri1 = (newVertIndex, ib, ic);
                        newTri2 = (ia, ib, newVertIndex);
                    }
                    else
                    {
                        throw new ArgumentException("Invalid triangle for splitting.");
                    }
                    pq.Enqueue(newTri1, GetEdge(newTri1));
                    pq.Enqueue(newTri2, GetEdge(newTri2));
                }
            }
            List<ValueTuple<int, int, int>> finalTris = new List<ValueTuple<int, int, int>>();
            foreach (var tri in pq)
            {
                finalTris.Add(tri);
            }
            TriangleMesh3D ret = new TriangleMesh3D(verts.ToArray(), finalTris.ToArray());
            ret.keepBackSurface = this.keepBackSurface;
            for (int i = 0; i < ret.numVertices; ++i)
            {
                ret.SetColor(i, colors[i]);
                if (backCs != null)
                {
                    ret.SetBackColor(i, backCs[i]);
                }
                if (uvs != null)
                {
                    ret.SetUV(i, uvs[i]);
                }
            }
            return ret;
        }
    }
}
