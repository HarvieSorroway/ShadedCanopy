using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreSlugcats;
using UnityEngine;

namespace ShadedCanopy.Objects.SCNectarPlate
{
    public class SCNectarPlateHooks
    {
        public static void Hooks()
        {
            On.Player.Grabability += Player_Grabability;
            On.MoreSlugcats.SlugNPCAI.WantsToEatThis += SlugNPCAI_WantsToEatThis;
            On.MoreSlugcats.SlugNPCAI.HasEdible += SlugNPCAI_HasEdible;
            On.MoreSlugcats.SlugNPCAI.AteFood += SlugNPCAI_AteFood;
        }

        private static Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
        {
            if (obj is SCNectarPlate)
            {
                if (!(obj.abstractPhysicalObject as AbstractNectarPlate).dead)
                    if (!self.isSlugpup)
                        return Player.ObjectGrabability.Drag;
                    else return Player.ObjectGrabability.BigOneHand;
                else return Player.ObjectGrabability.CantGrab;
            }
            return orig(self, obj);
        }

        private static void SlugNPCAI_AteFood(On.MoreSlugcats.SlugNPCAI.orig_AteFood orig, MoreSlugcats.SlugNPCAI self, PhysicalObject food)
        {
            orig(self, food);
        }

        private static bool SlugNPCAI_HasEdible(On.MoreSlugcats.SlugNPCAI.orig_HasEdible orig, MoreSlugcats.SlugNPCAI self)
        {
            if (!self.IsFull)
            {
                for (int i = 0; i < self.cat.grasps.Length; i++)
                {
                    if (self.cat.grasps[i] != null && self.cat.grasps[i].grabbed != null)
                    {
                        if (self.cat.grasps[i].grabbed is SCNectarPlate && (double)UnityEngine.Random.value < Math.Pow((double)Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0.9f, 0.7f, self.creature.personality.sympathy)), 0.10000000149011612))
                        {
                            return true;
                        }
                    }
                }
            }
            return orig(self);
        }

        private static bool SlugNPCAI_WantsToEatThis(On.MoreSlugcats.SlugNPCAI.orig_WantsToEatThis orig, MoreSlugcats.SlugNPCAI self, PhysicalObject obj)
        {
            if (obj is SCNectarPlate && !self.IsFull)
            {
                return true;
            }
            return orig(self, obj);
        }

    }
}
