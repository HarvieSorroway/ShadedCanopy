using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace ShadedCanopy.Objects.SCChandelierFlower.Mesh3D
{
    internal struct Triangle<PVector>: IComparable<Triangle<PVector>>
    {
        ValueTuple<PVector, PVector, PVector> pos;
        ValueTuple<Vector2, Vector2, Vector2>? uv;
        ValueTuple<Color, Color, Color> color;
        float depth;
        public Triangle(PVector v1, PVector v2, PVector v3, Color c1, Color c2, Color c3, float depth=0)
        {
            pos = (v1, v2, v3);
            color = (c1, c2, c3);
            uv = null;
            this.depth = depth;
        }
        public Triangle(PVector v1, PVector v2, PVector v3, Color c1, Color c2, Color c3, Vector2 uv1, Vector2 uv2, Vector2 uv3, float depth=0): this(v1, v2, v3, c1, c2, c3, depth)
        {
            uv = (uv1, uv2, uv3);
        }
        public PVector GetPos(int index)
        {
            return index switch
            {
                0 => pos.Item1,
                1 => pos.Item2,
                2 => pos.Item3,
                _ => throw new IndexOutOfRangeException("Index must be 0, 1, or 2.")
            };
        }
        public Color GetColor(int index)
        {
            return index switch
            {
                0 => color.Item1,
                1 => color.Item2,
                2 => color.Item3,
                _ => throw new IndexOutOfRangeException("Index must be 0, 1, or 2.")
            };
        }
        public Vector2 GetUV(int index)
        {
            if (!uv.HasValue) throw new InvalidOperationException("This triangle does not have UV coordinates.");
            return index switch
            {
                0 => uv.Value.Item1,
                1 => uv.Value.Item2,
                2 => uv.Value.Item3,
                _ => throw new IndexOutOfRangeException("Index must be 0, 1, or 2.")
            };
        }
        public PVector A
        {
            get => pos.Item1;
            set => pos.Item1 = value;
        }
        public PVector B
        {
            get => pos.Item2;
            set => pos.Item2 = value;
        }
        public PVector C
        {
            get => pos.Item3;
            set => pos.Item3 = value;
        }
        public Color colorA
        {
            get => color.Item1;
            set => color.Item1 = value;
        }
        public Color colorB
        {
            get => color.Item2;
            set => color.Item2 = value;
        }
        public Color colorC
        {
            get => color.Item3;
            set => color.Item3 = value;
        }
        public float d
        {
            get => depth;
            set => depth = value;
        }
        public bool hasUV => uv.HasValue;
        public Vector2 uvA
        {
            get => uv.Value.Item1;
            set
            {
                if (uv.HasValue)
                {
                    var temp = uv.Value;
                    temp.Item1 = value;
                    uv = temp;
                }
            }
        }
        public Vector2 uvB
        {
            get => uv.Value.Item2;
            set
            {
                if (uv.HasValue)
                {
                    var temp = uv.Value;
                    temp.Item2 = value;
                    uv = temp;
                }
            }
        }
        public Vector2 uvC
        {
            get => uv.Value.Item3;
            set
            {
                if (uv.HasValue)
                {
                    var temp = uv.Value;
                    temp.Item3 = value;
                    uv = temp;
                }
            }

        }

        public int CompareTo(Triangle<PVector> other)
        {
            if (this.depth < other.depth) return -1;
            else if (this.depth > other.depth) return 1;
            else return 0;
        }

    }
}
