using IL.Watcher;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShadedCanopy.Objects.SCMorningGlory
{
    internal static class SCMorningGloryTest
    {
        static bool _Inited = false;
        static SCUtils.SCHelperUtils.CreatureFollowingLabel playerLabel;
        public static void HookTest()
        {
            if (_Inited) return;
            On.Player.Update += Player_Update;
            SCUtils.SCDevTools.NodeTreeManager.SCDevNodeTreeManager.Track(ModifiableSCMorningGloryProperty.scMorningGlory);

            //On.Player.ctor += Player_ctor;
            //On.Player.Update += Player_Update_label;
            //On.Player.NewRoom += Player_NewRoom;
            _Inited = true;
        }
        static ObjectIDGenerator oidg = new();

        private static void Player_NewRoom(On.Player.orig_NewRoom orig, Player self, Room newRoom)
        {
            orig.Invoke(self, newRoom);
            if (playerLabel.room != newRoom)
            {
                playerLabel = new SCUtils.SCHelperUtils.CreatureFollowingLabel(self, playerLabel.posOffset);
                newRoom.AddObject(playerLabel);
            }
        }

        private static void Player_Update_label(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig.Invoke(self, eu);
            playerLabel.text = $"canJump: {self.canJump}, bodyMode: {self.bodyMode}";
        }

        private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig.Invoke(self, abstractCreature, world);
            playerLabel = new SCUtils.SCHelperUtils.CreatureFollowingLabel(self, Vector2.left * 30f);
            self.room.AddObject(playerLabel);
        }

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
            } else
            {
                isPressed[kc] = false;
                return false;
            }
        }
        private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig.Invoke(self, eu);

            if (CheckKeyPress(KeyCode.M))
            {
                SCUtils.SCUtils.Log("SCMorningGloryTest: pressed M!");
                AbstractPhysicalObject abo = new SCMorningGloryFruit.AbstractMorningGloryFruit(
                    self.room.world,
                    null,
                    self.room.GetWorldCoordinate(self.mainBodyChunk.pos),
                    self.room.game.GetNewID()
                );
                self.room.abstractRoom.AddEntity(abo);
                abo.RealizeInRoom();
            }
            if (CheckKeyPress(KeyCode.N))
            {
                SCUtils.SCUtils.Log("SCMorningGloryTest: pressed N!");
                SCMorningGlory.AbstractMorningGlory abo = new SCMorningGlory.AbstractMorningGlory(
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
