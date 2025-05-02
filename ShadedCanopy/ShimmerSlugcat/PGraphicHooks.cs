using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using UnityEngine;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

namespace ShadedCanopy.ShimmerSlugcat
{
    public class PGraphicHooks
    {
        public static void Hooks()
        {
            On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
        }

        private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, UnityEngine.Vector2 camPos)
        {
            if (PlayerHooks.shimmerPlayer.TryGetValue(self.player, out var module))
            {
                bool flag = module.playerGrabbed;
                string str = flag ? "Midground" : "GrabShaders";
                FContainer fContainer = rCam.ReturnFContainer(str);

                for (int i = 0; i < 10; i++)
                {
                    if (!flag && sLeaser.sprites[i].container != fContainer)
                    {
                        sLeaser.sprites[i].RemoveFromContainer();
                        fContainer.AddChild(sLeaser.sprites[i]);
                    }
                }
            }
            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (self.player.slugcatStats.name == ShimmerPlugin.Shimmer)
            {
                for (int i = 0; i < 10; i++)
                {
                    if (i < 9 && i != 2 && !sLeaser.sprites[i].element.name.StartsWith("Shimmer"))
                    {
                        sLeaser.sprites[i].element = Futile.atlasManager.GetElementWithName("Shimmer" + sLeaser.sprites[i].element.name);
                    }

                    if (i < 9)
                    {
                        sLeaser.sprites[i].color = Color.Lerp(self.player.ShortCutColor(), new Color(0.9f, 0.9f, 0.9f), module.lightUpProgress);
                        sLeaser.sprites[i].alpha = module.playerGrabbed ? 1f : 0.8f;
                    }

                    if (sLeaser.sprites[i]._renderLayer != null)
                    {
                        sLeaser.sprites[i]._renderLayer._material.SetFloat("_ShimmerTailLightness", module.energy / PlayerHooks.ShimmerPlayerModule.maxEnergy);
                    }
                }

            }
        }

        private static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);
            if (self.owner is Player player && player.slugcatStats.name == ShimmerPlugin.Shimmer)
            {
                FContainer fContainer = rCam.ReturnFContainer("GrabShaders");
                for (int i = 0; i < sLeaser.sprites.Length; i++)
                {
                    if (i == 2)
                    {
                        TriangleMesh triangleMesh = TriangleMesh.MakeLongMesh(4, true, false, "atlases/ShimmerTail");
                        sLeaser.sprites[i] = triangleMesh;
                    }

                    if (i >= 0 && i <= 8)
                    {
                        sLeaser.sprites[i].shader = Custom.rainWorld.Shaders["ShimmerSkin"];
                    }

                    sLeaser.sprites[i].RemoveFromContainer();
                    fContainer.AddChild(sLeaser.sprites[i]);
                }
                sLeaser.sprites[9].RemoveFromContainer();
                fContainer.AddChild(sLeaser.sprites[9]);
                sLeaser.sprites[4].MoveToFront();
                sLeaser.sprites[9].MoveInFrontOfOtherNode(sLeaser.sprites[4]);
            }
        }
    }
}
