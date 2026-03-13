using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace ShadedCanopy.Objects.SCBlinkingLawn
{


    internal class SCBlinkingLawnRect: UpdatableAndDeletable
    {
        public PlacedObject pObj
        {
            get;
            private set;
        }
        SCBlinkingLawnRectData data
        {
            get { return pObj.data as SCBlinkingLawnRectData; }
        }
        public SCBlinkingLawnRect(Room room, PlacedObject pObj)
        {
            this.pObj = pObj;
            this.room = room;
            ready = false;
            this.visible = true;
        }
        bool ready, visible;
        SCBlinkingPlant[] plants;

        public void Refresh()
        {
            if (plants != null)
            {
                foreach (SCBlinkingPlant plant in plants)
                {
                    plant.Destroy();
                }
            }
            ready = false;
        }
        public void SetVisible(bool visible)
        {
            this.visible = visible;
            Refresh();
        }
        public bool Visible { get { return visible; } }
        void Rebuild()
        {
            plants = null;
            UnityEngine.Random.State state = UnityEngine.Random.state;
            UnityEngine.Random.InitState(this.data.seed);
            Vector2[] rect = new Vector2[4];
            rect[0] = this.pObj.pos;
            rect[1] = this.pObj.pos + this.data.handles[0];
            rect[2] = this.pObj.pos + this.data.handles[1];
            rect[3] = this.pObj.pos + this.data.handles[2];
            // 任意四边形(无论凹凸)的面积
            float triangleSize1 = 0.5f * Mathf.Abs(rect[0].x * rect[1].y + rect[1].x * rect[2].y + rect[2].x * rect[0].y - rect[1].x * rect[0].y - rect[2].x * rect[1].y - rect[0].x * rect[2].y);
            float triangleSize2 = 0.5f * Mathf.Abs(rect[0].x * rect[2].y + rect[2].x * rect[3].y + rect[3].x * rect[0].y - rect[2].x * rect[0].y - rect[3].x * rect[2].y - rect[0].x * rect[3].y);
            float rectSize = triangleSize1 + triangleSize2;
            SCPlugin.Logger.LogInfo($"Rebuilding lawn rect at {rect[0]}, {rect[1]}, {rect[2]}, {rect[3]}, and Area is {rectSize}");
            int plantCount = Mathf.CeilToInt(rectSize / 20f / 20f * this.data.densePerTile);
            Vector2[] poses = new Vector2[plantCount];
            Color[] colors = new Color[plantCount];
            float[] depths = new float[plantCount];
            int[] styles = new int[plantCount];
            int[] primeNumbers = { 2, 3, 5, 7, 11, 13 };
            int pIdx1 = UnityEngine.Random.Range(0, primeNumbers.Length);
            int pIdx2, pIdx3;
            do
            {
                pIdx2 = UnityEngine.Random.Range(0, primeNumbers.Length);
            } while (pIdx2 == pIdx1);
            do
            {
                pIdx3 = UnityEngine.Random.Range(0, primeNumbers.Length);
            } while (pIdx3 == pIdx1 || pIdx3 == pIdx2);

            for (int i = 0; i < plantCount; i++)
            {
                float t, x, y;
                t = HaltonSequence(i + 1, primeNumbers[pIdx1]);
                x = HaltonSequence(i + 1, primeNumbers[pIdx2]);
                y = HaltonSequence(i + 1, primeNumbers[pIdx3]);
                if (x + y > 1)
                {
                    x = 1 - x;
                    y = 1 - y;
                }
                if (t < triangleSize1 / rectSize)
                {
                    poses[i] = rect[0] + (rect[1] - rect[0]) * x + (rect[2] - rect[0]) * y;
                }
                else
                {
                    poses[i] = rect[0] + (rect[2] - rect[0]) * x + (rect[3] - rect[0]) * y;
                }
                Color ca = SCBlinkingPlantProperty.Value.plantColorA, cb = SCBlinkingPlantProperty.Value.plantColorB;
                colors[i] = SCUtils.UtilTools.ColorRandomLerp(ca, cb);
                float stylef = UnityEngine.Random.Range(0f, 1f);
                if (stylef <= SCBlinkingLawnProperty.Value.pTypeA)
                {
                    styles[i] = 0;
                } else if (stylef <= SCBlinkingLawnProperty.Value.pTypeA + SCBlinkingLawnProperty.Value.pTypeB)
                {
                    styles[i] = 1;
                } else if (stylef <= SCBlinkingLawnProperty.Value.pTypeA + SCBlinkingLawnProperty.Value.pTypeB + SCBlinkingLawnProperty.Value.pTypeC)
                {
                    styles[i] = 2;
                } else
                {
                    styles[i] = 3;
                }
                depths[i] = UnityEngine.Random.Range(0f, SCBlinkingLawnProperty.Value.maxDepth);
            }
            plants = new SCBlinkingPlant[plantCount];
            for (int i = 0; i < plantCount; i++)
            {
                plants[i] = new SCBlinkingPlant(room, poses[i], colors[i], depths[i], styles[i]);
                this.room.AddObject(plants[i]);
            }
            ready = true;
            UnityEngine.Random.state = state;
        }
        public override void Update(bool eu)
        {
            base.Update(eu);
            if (!ready && visible)
            {
                Rebuild();
            }
        }
        private static float HaltonSequence(int index, int base_num)
        {
            float result = 0;
            float f = 1f / base_num;
            int i = index;

            while (i > 0)
            {
                result += f * (i % base_num);
                i = Mathf.FloorToInt(i / base_num);
                f /= base_num;
            }

            return result;
        }
        public class SCBlinkingLawnRectData : PlacedObject.QuadObjectData
        {
            public int seed;
            public float densePerTile;

            public SCBlinkingLawnRectData(PlacedObject p) : base(p)
            {
                seed = UnityEngine.Random.Range(0, 10000);
                densePerTile = 10;
            }
            public override void FromString(string s)
            {
                base.FromString(s);
                string[] array = Regex.Split(s, "~");
                int seed;
                if (array.Length > 6 && int.TryParse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture, out seed))
                {
                    this.seed = seed;
                }
                float dense;
                if (array.Length > 7 && float.TryParse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture, out dense))
                {
                    this.densePerTile = dense;
                }
                this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 8);
            }
            public override string ToString()
            {
                string text = base.BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}", this.seed, this.densePerTile);
                text = SaveState.SetCustomData(this, text);
                return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", this.unrecognizedAttributes);
            }
        }
    }
}
