using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using UnityEngine;

namespace ShadedCanopy.ShimmerSlugcat
{
    public class PlayerHooks
    {
        public static ConditionalWeakTable<Player, ShimmerPlayerModule> shimmerPlayer = new ConditionalWeakTable<Player, ShimmerPlayerModule>();

        public static void Hooks()
        {
            On.Player.ctor += Player_ctor;
            On.Player.Update += Player_Update;
        }

        private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            if (self.slugcatStats.name == ShimmerPlugin.Shimmer)
            {
                shimmerPlayer.Add(self, new ShimmerPlayerModule());
            }
        }

        private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);
            if (shimmerPlayer.TryGetValue(self, out var module))
            {
                module.playerGrabbed = GrabbedCondition(self);

                float tik = 0.5f;
                if (module.lightUp)
                {
                    if(self.dead || module.energy <= 0f) module.lightUp = false;

                    if (module.energy >= tik)
                        module.energy -= tik;

                    if (self.room != null)
                    {
                        if (module.lightUpProgress < 1f)
                        {
                            module.lightUpProgress += 0.025f;
                        }

                        if ((module.lightSource == null || module.lightSource.slatedForDeletetion))
                        {
                            module.lightSource = new LightSource(self.firstChunk.pos, false, Color.white, self, false)
                            {
                                rad = 5f,
                                alpha = 0.05f
                            };
                            self.room.AddObject(module.lightSource);
                        }                        
                    }
                }
                else
                {
                    if (module.lightUpProgress > 0.05f)
                    {
                        module.lightUpProgress -= 0.05f;
                    }
                    else module.lightUpProgress = 0f;

                    if (module.lightUpProgress <= 0f && module.lightSource != null)
                    {
                        module.lightSource.Destroy();
                    }
                }

                if (self.room != null && self.Consious && self.input[1].spec && !self.input[0].spec)
                {
                    bool flag = !module.lightUp;
                    module.lightUp = flag;
                }

                if (module.lightSource != null)
                {
                    module.lightSource.setRad = Mathf.Lerp(5f, 600f, module.lightUpProgress);
                    module.lightSource.alpha = 0.05f + 0.95f * module.lightUpProgress;
                    module.lightSource.pos = self.DangerPos;
                }

                if (Input.GetKey(KeyCode.Q))
                {
                    module.energy = 300f;
                }
            }
        }

        public static bool GrabbedCondition(Player player)
        {
            if (!player.Consious && player.grabbedBy.Count > 0)
            {
                return true;
            }
            if (ModManager.JollyCoop && player.room != null && player.room.game != null && player.room.game.Players.Count > 1)
            {
                for (int i = 0; i < player.room.game.Players.Count; i++)
                {
                    if (player.room.game.Players[i].realizedCreature == null) continue;
                    Player player2 = player.room.game.Players[i].realizedCreature as Player;
                    if (player != player2 && player2.slugOnBack != null && player2.slugOnBack.slugcat == player)
                    {
                        return true;                        
                    }
                }
            }
            return false;
        }

        public class ShimmerPlayerModule
        {
            public float energy = 300f;
            public float lightUpProgress;
            public bool lightUp;
            public bool playerGrabbed;
            public LightSource lightSource;
            public ShimmerPlayerModule()
            {

            }
        }
    }
}
