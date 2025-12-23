using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadedCanopy.Objects.SCMorningGlory
{
    internal static class SCMorningGloryHooks
    {
        static bool _Inited = false;
        public static void Hook()
        {
            if (_Inited) return;

            // TODO
            //On.ItemSymbol.SpriteNameForItem += ItemSymbol_SpriteNameForItem;
            //On.ItemSymbol.SymbolDataFromItem += ItemSymbol_SymbolDataFromItem;
            On.Player.Grabability += Player_Grabability;
            On.Player.TossObject += Player_TossObject;
            On.SaveState.AbstractPhysicalObjectFromString += SaveState_AbstractPhysicalObjectFromString;

            _Inited = true;
        }

        private static AbstractPhysicalObject SaveState_AbstractPhysicalObjectFromString(On.SaveState.orig_AbstractPhysicalObjectFromString orig, World world, string objString)
        {
            AbstractPhysicalObject apo = orig.Invoke(world, objString);
            if (apo.type == SCEnums.AbstractObjectTypeType.SCMorningGloryFruit)
            {
                AbstractPhysicalObject newApo = new SCMorningGloryFruit.AbstractMorningGloryFruit(apo.world, apo.realizedObject, apo.pos, apo.ID);
                apo = newApo;
            }
            return apo;
        }

        private static void Player_TossObject(On.Player.orig_TossObject orig, Player self, int grasp, bool eu)
        {
            if (self.grasps[grasp].grabbed is SCMorningGloryFruit fruit)
            {
                if (fruit.hasStalk)
                {
                    // copy from assembly
                    self.room.PlaySound(SoundID.Slugcat_Throw_Misc_Inanimate, self.grasps[grasp].grabbedChunk, false, 1f, 1f);
                    // 不扔
                    return;
                }
            }
            orig.Invoke(self, grasp, eu);
        }

        private static Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
        {
            if (obj is SCMorningGloryFruit)
            {
                return SCMorningGloryProperty.fruitGrability;
            }
            return orig.Invoke(self, obj);
        }

        private static IconSymbol.IconSymbolData? ItemSymbol_SymbolDataFromItem(On.ItemSymbol.orig_SymbolDataFromItem orig, AbstractPhysicalObject item)
        {
            throw new NotImplementedException();
        }

        private static string ItemSymbol_SpriteNameForItem(On.ItemSymbol.orig_SpriteNameForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData)
        {
            throw new NotImplementedException();
        }
    }
}
