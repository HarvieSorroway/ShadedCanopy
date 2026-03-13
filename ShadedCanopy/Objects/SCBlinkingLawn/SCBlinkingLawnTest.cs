using ShadedCanopy.Objects.SCWindFIeld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShadedCanopy.Objects.SCBlinkingLawn
{
    internal static class SCBlinkingLawnTest
    {
        static bool[] lastInput = new bool[256];
        public static void HookTest()
        {
            //On.Room.Loaded += Room_Loaded;
            //On.Player.Update += Player_Update;

            SCUtils.SCDevTools.NodeTreeManager.SCDevNodeTreeManager.Track(SCWindFieldProperty.Value);
            SCUtils.SCDevTools.NodeTreeManager.SCDevNodeTreeManager.Track(SCBlinkingPlantProperty.Value);
            SCUtils.SCDevTools.NodeTreeManager.SCDevNodeTreeManager.Track(SCBlinkingLawnProperty.Value);
        }

        private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig.Invoke(self, eu);
            bool input = Input.GetKey(KeyCode.P);
            if (input && !lastInput['P'])
            {
                if (self.room != null)
                {
                    //self.room.AddObject(new SCBlinkingPlant(self.room, self.mainBodyChunk.pos));
                    SCPlugin.Logger.LogInfo($"Spawn lawn segment in Player pos: {self.mainBodyChunk.pos}");
                    self.room.AddObject(new SCBlinkingLawnSegment(self.room, self.mainBodyChunk.pos + new Vector2(-100, 0), self.mainBodyChunk.pos + new Vector2(100, 0), null));
                }
            }
            lastInput['P'] = input;
        }

        private static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
        {
            orig.Invoke(self);
            self.AddObject(new SCWindFieldTest(self, 1));
        }
    }
}
