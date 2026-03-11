using SCUtils.SCDevTools.NodeTreeManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShadedCanopy.Objects.SCBlinkingLawn
{
    internal static class SCWindFieldProperty
    {

        [SCDevToolsInspectType("Root.TestBlinkingLawn", "TestWindFieldWindow")]
        public class _SCWindField
        {
            [SCDevToolsInspectValue]
            public float WindDecay = 0.95f;
            [SCDevToolsInspectValue]
            public float AirViscosity = 0.1f;
            [SCDevToolsInspectValue]
            public int showVis = 0;


        }
        public static _SCWindField Value = new();
    }
    internal static class SCBlinkingPlantProperty
    {
        [SCDevToolsInspectType("Root.TestBlinkingLawn", "TestBlinkingPlantWindow")]
        public class _SCBlinkingPlant
        {
            [SCDevToolsInspectValue]
            public float maxLatensity = 14;
            [SCDevToolsInspectValue]
            public float airFriction = 0.90f;
            [SCDevToolsInspectValue]
            public float elasticity = 0.07f;

            [SCDevToolsInspectValue]
            public float AMass = 0.5f;
            [SCDevToolsInspectValue]
            public int ALightRad = 30;
            [SCDevToolsInspectValue]
            public float BMass = 0.9f;
            [SCDevToolsInspectValue]
            public int BLightRad = 50;
            [SCDevToolsInspectValue]
            public float CMass = 1f;
            [SCDevToolsInspectValue]
            public int CLightRad = 60;
            [SCDevToolsInspectValue]
            public float DMass = 1.4f;
            [SCDevToolsInspectValue]
            public int DLightRad = 80;

            [SCDevToolsInspectValue]
            public float superforeDepthThreshold = 0.3f;
            [SCDevToolsInspectValue]
            public float foreDepthThreshold = 0.6f;
            [SCDevToolsInspectValue]
            public float lightUpWindThreshold = 0.1f;
            [SCDevToolsInspectValue]
            public float lightUpSpeed = 0.04f;
            [SCDevToolsInspectValue]
            public float lightDownSpeed = 0.01f;
            [SCDevToolsInspectValue]
            public float brightnessUpSpeed = 0.06f;
            [SCDevToolsInspectValue]
            public float brightnessDownSpeed = 0.01f;
            [SCDevToolsInspectValue]
            public Vector2 brightnessRange = new Vector2(0.3f, 1);


            [SCDevToolsInspectValue]
            public float lightnessMax = 0.05f;

            [SCDevToolsInspectValue]
            public Color plantColorA = new Color(95 / 255f, 238 / 255f, 254 / 255f);
            [SCDevToolsInspectValue]
            public Color plantColorB = new Color(61 / 255f, 204 / 255f, 202 / 255f);

        }
        public static _SCBlinkingPlant Value = new();
    }
    internal static class SCBlinkingLawnProperty
    {
        [SCDevToolsInspectType("Root.TestBlinkingLawn", "TestBlinkingLawnWindow")]
        public class _SCBlinkingLawn
        {
            [SCDevToolsInspectValue]
            public float maxDepth = 0.8f;
            [SCDevToolsInspectValue]
            public float densityPerLength = 0.15f;
            [SCDevToolsInspectValue]
            public float segmentTopLength = 5f;
            [SCDevToolsInspectValue]
            public float segmentBottomLength = 2f;
            [SCDevToolsInspectValue]
            public float pTypeA = 0.3f;
            [SCDevToolsInspectValue]
            public float pTypeB = 0.4f;
            [SCDevToolsInspectValue]
            public float pTypeC = 0.2f;
            [SCDevToolsInspectValue]
            public float pTypeD = 0.1f;
        }
        public static _SCBlinkingLawn Value = new();
    }
}
