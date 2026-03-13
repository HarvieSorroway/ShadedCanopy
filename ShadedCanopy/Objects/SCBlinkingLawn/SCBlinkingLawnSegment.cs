using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShadedCanopy.Objects.SCBlinkingLawn
{
    internal class SCBlinkingLawnSegment: UpdatableAndDeletable
    {
        Vector2 left, right;
        int? seed;
        public SCBlinkingLawnSegment(Room room, Vector2 left, Vector2 right, int? seed)
        {
            this.room = room;
            this.left = left;
            this.right = right;
            this.seed = seed;
        }
        public override void Update(bool eu)
        {
            base.Update(eu);
            if (slatedForDeletetion)
                return;
            int n = Mathf.CeilToInt((right - left).magnitude * SCBlinkingLawnProperty.Value.densityPerLength);
            UnityEngine.Random.State oldState = UnityEngine.Random.state;
            if (this.seed.HasValue)
            {
                UnityEngine.Random.InitState(this.seed.Value);
            }
            // do sth
            float[] depths = new float[n];
            Vector2[] poses = new Vector2[n];
            int[] styles = new int[n];
            Color[] colors = new Color[n];
            Vector2 perpen = RWCustom.Custom.PerpendicularVector(right - left).normalized;
            for (int i = 0; i < n; ++i)
            {
                styles[i] = UnityEngine.Random.Range(0, PlantInfo.presetInfos.Length - 1);
                depths[i] = UnityEngine.Random.Range(0f, SCBlinkingLawnProperty.Value.maxDepth);
                Color ca = SCBlinkingPlantProperty.Value.plantColorA, cb = SCBlinkingPlantProperty.Value.plantColorB;
                colors[i] = SCUtils.UtilTools.ColorRandomLerp(ca, cb);
                float t1 = UnityEngine.Random.Range(0f, 1f);
                float t2 = UnityEngine.Random.Range(-1f, 1f);
                float topBottomAdd = SCBlinkingLawnProperty.Value.segmentTopLength + SCBlinkingLawnProperty.Value.segmentBottomLength;
                float topBottomSub = SCBlinkingLawnProperty.Value.segmentTopLength - SCBlinkingLawnProperty.Value.segmentBottomLength;
                poses[i] = Vector2.Lerp(left, right, t1) + perpen *(t2 * topBottomAdd / 2 + topBottomSub / 2);
            }
            for (int i = 0; i < n; ++i)
            {
                 room.AddObject(new SCBlinkingPlant(room, poses[i], colors[i], depths[i], styles[i]));
            }
            if (this.seed.HasValue)
            {
                UnityEngine.Random.state = oldState;
            }
            Destroy();
        }

    }
}
