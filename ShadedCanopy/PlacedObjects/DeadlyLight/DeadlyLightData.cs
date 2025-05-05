using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace ShadedCanopy.PlacedObjects.DeadlyLight
{
    internal class DeadlyLightData : PlacedObject.ResizableObjectData
    {
        public Vector2 panelPos;
        public float maxLightRange;
        public float penumbraWidth;
        public float exposure;
        public float clampedDepth;
        public bool reverseLightDir;

        public DeadlyLightData(PlacedObject owner) : base(owner)
        {
        }

        public override void FromString(string s)
        {
            string[] array = Regex.Split(s, "~");
            handlePos.x = float.Parse(array[0]);
            handlePos.y = float.Parse(array[1]);
            panelPos.x = float.Parse(array[2]);
            panelPos.y = float.Parse(array[3]);
            if(array.Length <= 4)
            {
                maxLightRange = 100f;
                penumbraWidth = 20f;
                reverseLightDir = false;
            }
            else
            {
                maxLightRange = float.Parse(array[4]);
                penumbraWidth = float.Parse(array[5]);
                reverseLightDir = bool.Parse(array[6]);
            }
            if(array.Length <= 7)
            {
                exposure = 1f;
            }
            else
            {
                exposure = float.Parse(array[7]);
            }

            if (array.Length <= 8)
            {
                clampedDepth = 0;
            }
            else
            {
                clampedDepth = float.Parse(array[8]);
            }

            unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 5);
        }

        public override string ToString()
        {
            string res = $"{handlePos.x}~{handlePos.y}~{panelPos.x}~{panelPos.y}~{maxLightRange}~{penumbraWidth}~{reverseLightDir}~{exposure}~{clampedDepth}";
            res = SaveState.SetCustomData(this, res);
            return SaveUtils.AppendUnrecognizedStringAttrs(res, "~", unrecognizedAttributes);
        }
    }
}
