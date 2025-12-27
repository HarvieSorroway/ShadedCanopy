using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadedCanopy.Iterators.MechIterator
{
    internal static class MechIteratorHooks
    {
        public static void HooksOn()
        {
            On.Room.ReadyForAI += Room_ReadyForAI;
        }

        private static void Room_ReadyForAI(On.Room.orig_ReadyForAI orig, Room self)
        {
            orig.Invoke(self);
            if(self.game != null && self.game.IsStorySession && self.abstractRoom.name == "MU_LAB_S")
            {
                MechIterator iterator = new MechIterator(self);
                self.AddObject(iterator);
            }
        }
    }
}
