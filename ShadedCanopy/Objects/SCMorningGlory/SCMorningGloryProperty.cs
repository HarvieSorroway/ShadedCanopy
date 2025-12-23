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
        public static int fruitBites = 3;
        public static int fruitFoodPoints = 3;
        public static string fruitContainerBeforePick = "Background";
        public static string fruitContainerAfterPick = "Items";
        public static float fruitSpriteScale = 1.3f;
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
        public static float leafProbability = 0.2f;
        public static string stalkContainer = "Background";
        public static string fruitLinkerContainer = "Midground";


    }
    public static class ModifiableSCMorningGloryProperty
    {
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
        internal static SCUtils.SCDevTools._SCMorningGlory scMorningGlory = new();
    };

}
