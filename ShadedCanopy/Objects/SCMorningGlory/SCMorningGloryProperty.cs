using SCUtils.SCDevTools.NodeTreeManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace ShadedCanopy.Objects.SCMorningGlory
{
    internal static class SCMorningGloryProperty
    {
        // Fruit
        public static class FruitPhysicalProperty
        {
            public static float airFriction = 0.999f;
		    public static float gravity = 0.9f;
		    public static float bounce = 0.2f;
		    public static float surfaceFriction = 0.7f;
		    public static int collisionLayer = 1;
		    public static float waterFriction = 0.95f;
		    public static float buoyancy = 1.1f;
            public static float rad = 8f;
            public static float mass = 0.4f;
            public static float rollingFriction = 0.8f;
            public static float rollingRotationSpeedRatio = 0.1f;
        }
        public static int fruitBites = 4;
        public static int fruitFoodPoints = 3;
        public static string fruitContainerBeforePick = "Background";
        public static string fruitContainerAfterPick = "Items";
        public static float fruitSpriteScale = 0.8f;
        public static float fruitLightAlpha = 0.3f;
        public static float fruitLightRad = 80f;
        public static float fruitLightAlphaGrabbed = 0.5f;
        public static float fruitLightRadGrabbed = 100f;
        public static float fruitLightAlphaMaxSlope = 0.03f;
        public static float fruitLightRadMaxSlope = 1f;
        public static Color fruitBaseColor = Color.green;
        public static Player.ObjectGrabability fruitGrability = Player.ObjectGrabability.OneHand;


        // Stalk
        public static class StalkMainChunkPhysicalProperty
        {
            public static float airFriction = 0.85f;
            public static float gravity = 0.9f;
            public static float bounce = 0.2f;
            public static float surfaceFriction = 0.7f;
            public static int collisionLayer = 1;
            public static float waterFriction = 0.95f;
            public static float buoyancy = 1.1f;
            public static float rad = 1f;
            public static float mass = 1.2f;
            public static float rollingFriction = 0.8f;
            public static float rollingRotationSpeedRatio = 0.1f;
        }
        //public static float coverAngleMin = 60f;
        //public static float coverAngleMax = 120f;
        //public static float leafAngleMin = 10f;
        //public static float leafAngleMax = 40f;
        //public static float leafSizeMin = 7f;
        //public static float leafSizeMax = 12f;
        public static float leafProbability = 0.4f;
        public static string stalkContainer = "Background";
        public static string fruitLinkerContainer = "Midground";


    }
    public static class ModifiableSCMorningGloryProperty
    {
        [SCDevToolsInspectType("Root.TestMorningGlory", "TestMorningGlory")]
        public class _SCMorningGlory
        {
            [SCDevToolsInspectValue]
            public float stalkSegmentMaxLength = 25f;
            //[SCDevToolsInspectValue]
            //public int stalkSegmentCount = 10;
            [SCDevToolsInspectValue]
            public int stalkSegmentMinCount = 3;
            [SCDevToolsInspectValue]
            public float stalkNodeMass = 0.3f;
            [SCDevToolsInspectValue]
            public float stalkFlexibility = 1.1f; // 0.35f;
            [SCDevToolsInspectValue]
            public float stalkFruitDistance = 15f;
            [SCDevToolsInspectValue]
            public float stalkSegmentShrink = 0.6f;

            //[SCDevToolsInspectValue]
            public int coverPetalsMin = 5;
            //[SCDevToolsInspectValue]
            public int coverPetalsMax = 7;
            //[SCDevToolsInspectValue]
            public int coverNodeCount = 6;
            //[SCDevToolsInspectValue]
            public float coverHeight = 20f;
            //[SCDevToolsInspectValue]
            public float coverWidth = 15f;
            //[SCDevToolsInspectValue]
            public float coverLiftDistance = 5f;
            [SCDevToolsInspectValue]
            public float coverHeightCurvePct = 0.2f;
            [SCDevToolsInspectValue]
            public float coverWidthCurvePct = 0.3f;
            //[SCDevToolsInspectValue]
            public float coverDistrubFlipMultipler = 2f;
            //[SCDevToolsInspectValue]
            public float coverDistrubSpinMultipler = 6f;
            //[SCDevToolsInspectValue]
            public float coverAutoDisturbProb = 0.1f;
            [SCDevToolsInspectValue]
            public float coverDistrubMaxForce = 2f;


            [SCDevToolsInspectValue]
            public float coverNoFruitWidthMultiplier = 0.6f;
            [SCDevToolsInspectValue]
            public float coverNoFruitHeightMultiplier = 1.3f;


            [SCDevToolsInspectValue]
            public int fruitPickRequireTicks = 30;
            [SCDevToolsInspectValue]
            public float stalkExtraPlayerElastic = 0.4f;
            [SCDevToolsInspectValue]
            public float stalkMaxJumpBoost = 8f;
            [SCDevToolsInspectValue]
            public float stalkMaxJumpBoostLenghPct = 0.3f;

            [SCDevToolsInspectValue]
            public float leafWidthMin = 0.3f;
            [SCDevToolsInspectValue]
            public float leafWidthMax = 0.6f;
            [SCDevToolsInspectValue]
            public float leafLengthMin = 1f;
            [SCDevToolsInspectValue]
            public float leafLengthMax = 1.3f;
            [SCDevToolsInspectValue]
            public float leafAngleMin = 40f;
            [SCDevToolsInspectValue]
            public float leafAngleMax = 75f;
            //[SCDevToolsInspectValue]
            public float leafAutoDistrubProb = 0.1f;
            //[SCDevToolsInspectValue]
            public float leafDistrubMultipler = 1f;

            [SCDevToolsInspectValue]
            public float stalkNoFruitShirink = 0.5f;
            [SCDevToolsInspectValue]
            public float leafNoFruitAngleMultipler = 0.4f;
            [SCDevToolsInspectValue]
            public float leafNoFruitScale = 0.8f;
            [SCDevToolsInspectValue]
            public float stalkNoFruitColorChange = 0.85f;
            [SCDevToolsInspectValue]
            public float stalkNoFruitMaxFold = 2f;


            [SCDevToolsInspectValue]
            public float fruitConnectAngle = 60f;
            [SCDevToolsInspectValue]
            public float fruitConnectDistance = 6f;
            [SCDevToolsInspectValue]
            public float fruitConnectTipWidth = 2f;

            [SCDevToolsInspectValue]
            public float stalkWidth = 3f;
            //[SCDevToolsInspectValue]
            public float stalkWidthOffsetMin = -1.15f;
            //[SCDevToolsInspectValue]
            public float stalkWidthOffsetMax = 1.15f;

            [SCDevToolsInspectValue]
            public float coverGradientLength = 1f;
            [SCDevToolsInspectValue]
            public float coverGradientDarknessMax = 0.3f;

            [SCDevToolsInspectValue]
            public float stalkGradientLength = 1f;
            [SCDevToolsInspectValue]
            public float stalkGradientDarknessMax = 0.1f;


            [SCDevToolsInspectValue]
            public float fogDepth = 0.45f;
            // 颜色战地过大 最好放最后
            public Color stalkColorA = RWCustom.Custom.hexToColor("018C52");
            public Color stalkColorB = RWCustom.Custom.hexToColor("013E32");
            [SCDevToolsInspectValue]
            public Color stalkNoFruitColor = RWCustom.Custom.hexToColor("58561E");

        }
        //[SCDevToolsInspectType("Root", "TestMorningGlory")]
        //public class _SCMorningGlory
        //{
        //    [SCDevToolsInspectValue]
        //    public float stalkSegmentMaxLength = 10f;
        //    [SCDevToolsInspectValue]
        //    public float stalkFlexibility = 0.5f;
        //    [SCDevToolsInspectValue]
        //    public float stalkSegmentShrink = 0.8f;
        //}
        internal static _SCMorningGlory scMorningGlory = new();
    };

}
