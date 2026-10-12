﻿﻿﻿using System;
using UnityEngine;

namespace ShadedCanopy.Objects.SCChandelierFlower
{
    internal static class SCChandelierFlowerHooks
    {
        public static readonly string ChandelierFlowerFruitSpriteName = "atlases/ChandelierFlower/ChandelierFlowerFruit";
        static bool _Inited = false;
        public static void Hook()
        {
            if (_Inited) return;

            On.Player.Grabability += Player_Grabability;
            On.Player.TossObject += Player_TossObject;
            On.SaveState.AbstractPhysicalObjectFromString += SaveState_AbstractPhysicalObjectFromString;

            _Inited = true;
        }
        public static void LoadResources(AssetBundle ab)
        {
            FShader tcwn = FShader.CreateShader("TexControledWaterNut", ab.LoadAsset<Shader>("TexControledWaterNut.shader"));
            // 通过shader的名字检测是不是合法shader
            if (tcwn.shader == null || tcwn.shader.name.Contains("Hidden/InternalErrorShader"))
            {
                String errmsg = $"SCCF failed load shader {tcwn.shader}";
                SCPlugin.Logger.LogFatal(errmsg);
                throw new ArgumentNullException(errmsg);
            }
            RWCustom.Custom.rainWorld.Shaders["TexControledWaterNut"] = tcwn;
            Futile.atlasManager.LoadImage($"{ChandelierFlowerFruitSpriteName}0");
            Futile.atlasManager.LoadImage($"{ChandelierFlowerFruitSpriteName}TCWN");
        }

        private static AbstractPhysicalObject SaveState_AbstractPhysicalObjectFromString(On.SaveState.orig_AbstractPhysicalObjectFromString orig, World world, string objString)
        {
            AbstractPhysicalObject apo = orig.Invoke(world, objString);
            if (apo.type == SCEnums.AbstractObjectTypeType.SCChandelierFlowerFruit)
            {
                AbstractPhysicalObject newApo = new SCChandelierFlowerFruit.AbstractChandelierFlowerFruit(apo.world, apo.realizedObject, apo.pos, apo.ID);
                apo = newApo;
            }
            return apo;
        }

        private static void Player_TossObject(On.Player.orig_TossObject orig, Player self, int grasp, bool eu)
        {
            if (self.grasps[grasp].grabbed is SCChandelierFlowerFruit fruit)
            {
                if (fruit.hasStalk)
                {
                    self.room.PlaySound(SoundID.Slugcat_Throw_Misc_Inanimate, self.grasps[grasp].grabbedChunk, false, 1f, 1f);
                    return;
                }
            }
            orig.Invoke(self, grasp, eu);
        }

        private static Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
        {
            if (obj is SCChandelierFlowerFruit)
            {
                return SCChandelierFlowerProperty.fruitObjectProperty.fruitGrability;
            }
            return orig.Invoke(self, obj);
        }
    }
}
