using IL.Watcher;
using RWCustom;
using SCUtils.RwTasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShadedCanopy.Objects.SCChandelierFlower
{
    internal static class SCChandelierFlowerTest
    {
        static bool _Inited = false;
        public static void HookTest()
        {
            if (_Inited) return;
            SCUtils.SCDevTools.NodeTreeManager.SCDevNodeTreeManager.Track(SCChandelierFlowerProperty.fruitPhysicalProperty);
            SCUtils.SCDevTools.NodeTreeManager.SCDevNodeTreeManager.Track(SCChandelierFlowerProperty.fruitObjectProperty);
            SCUtils.SCDevTools.NodeTreeManager.SCDevNodeTreeManager.Track(SCChandelierFlowerProperty.torusPhysicalProperty);
            SCUtils.SCDevTools.NodeTreeManager.SCDevNodeTreeManager.Track(SCChandelierFlowerProperty.stalkObjectProperty);
            SCUtils.SCDevTools.NodeTreeManager.SCDevNodeTreeManager.Track(SCChandelierFlowerProperty.outfitProperty);
            SCUtils.SCDevTools.NodeTreeManager.SCDevNodeTreeManager.Track(SCChandelierFlowerProperty.coverPhysicalProperty);
            SCUtils.SCDevTools.NodeTreeManager.SCDevNodeTreeManager.Track(SCChandelierFlowerProperty.normalCoverMeshProperty);
            SCUtils.SCDevTools.NodeTreeManager.SCDevNodeTreeManager.Track(SCChandelierFlowerProperty.coverWitheredMeshProperty);

            On.Player.Update += Player_Update;

            _Inited = true;
        }
        static ObjectIDGenerator oidg = new();

        static Dictionary<KeyCode, bool> isPressed = new();
        public static bool CheckKeyPress(KeyCode kc)
        {
            if (!isPressed.ContainsKey(kc))
            {
                isPressed[kc] = false;
            }
            if (Input.GetKey(kc))
            {
                if (!isPressed[kc])
                {
                    isPressed[kc] = true;
                    return true;
                }
                return false;
            }
            else
            {
                isPressed[kc] = false;
                return false;
            }
        }
        private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig.Invoke(self, eu);

            if (CheckKeyPress(KeyCode.Period))
            {
                SCUtils.SCUtils.Log("SCChandelierFlowerTest: pressed .!");
                SCChandelierFlower.AbstractChandelierFlower abo = new SCChandelierFlower.AbstractChandelierFlower(
                    self.room.world,
                    null,
                    self.room.GetWorldCoordinate(self.mainBodyChunk.pos + Vector2.up * 30f),
                    self.room.game.GetNewID(),
                    -1, -1,
                    null
                );
                abo.SetUnconsumed(self.room);
                self.room.abstractRoom.AddEntity(abo);
                self.room.abstractRoom.AddEntity(abo.abstractFruit);
                abo.RealizeInRoom();
            }
        }

    }
}
