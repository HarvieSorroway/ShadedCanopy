using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using ScavengerCosmetic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using static AbstractCreature;
using static ScavengerGraphics.Eartlers;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

namespace ShadedCanopy.Creatures.Scavengers
{
    internal static class ScavengerHooks
    {
        public static void HooksOn()
        {
            On.ScavengerGraphics.GenerateColors += ScavengerGraphics_GenerateColors;
            On.ScavengerGraphics.IndividualVariations.ctor += IndividualVariations_ctor;

            IL.ScavengerGraphics.ctor += ScavengerGraphics_ctor;
            On.ScavengerGraphics.ApplyPalette += ScavengerGraphics_ApplyPalette;

            On.Scavenger.ctor += Scavenger_ctor;

            On.ScavengerGraphics.Eartlers.GenerateSegments += Eartlers_GenerateSegments;

        }

        private static void Eartlers_GenerateSegments(On.ScavengerGraphics.Eartlers.orig_GenerateSegments orig, ScavengerGraphics.Eartlers self)
        {
            self.points = new List<Vertex[]>();
            List<Vertex> segment = new List<Vertex>();


            //第一类
            var basePos = new float2(0.5f, 0f);
            segment.Add(new Vertex(basePos, 1f));
            segment.Add(new Vertex(basePos + Custom.DegToFloat2(Mathf.Lerp(40f, 90f, Random.value)) * 0.4f , 1f));
            segment.Add(new Vertex(segment.Last().pos + Custom.DegToFloat2(Mathf.Lerp(30f, 60f, Random.value)) * Mathf.Lerp(0.8f, 1.2f, Random.value), 0.5f + 1f * Random.value));

        }

        private static void Scavenger_ctor(On.Scavenger.orig_ctor orig, Scavenger self, AbstractCreature abstractCreature, World world)
        {
            orig.Invoke(self, abstractCreature, world);
            if (!self.IsSCScav())
                return;
            SCScavExtra.TryGetSCScav(self, true);
        }


        //添加背毛
        private static void ScavengerGraphics_ctor(MonoMod.Cil.ILContext il)
        {
            ILCursor c1 = new ILCursor(il);
            try
            {
                c1.GotoNext(MoveType.After, (i) => i.MatchNewobj<WobblyBackTufts>());
                c1.GotoNext(MoveType.After, (i) => i.MatchAdd());
                c1.GotoNext(MoveType.After, (i) => i.MatchAdd());
                c1.Index++;   // 位置：L48/IL025B this.ChestSprite = num++;之前

                c1.Emit(OpCodes.Ldarg_0);  // arg0: this
                c1.Emit(OpCodes.Ldloc_1);  // arg1: num
                c1.EmitDelegate<Func<ScavengerGraphics, int, int>>((self, num) =>
                {
                    if (self.subModules.Count(i => i is HardBackSpikes) == 0)
                    {
                        if(self.scavenger.IsSCScav())
                        {
                            var spike = new SCHardBackTufts(self, num);
                            self.subModules.Add(spike);
                            num += spike.totalSprites;
                        }
                    }
                    return num;
                });
                c1.Emit(OpCodes.Stloc_1);  // ret to num
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        //手臂颜色修改
        private static void ScavengerGraphics_ApplyPalette(On.ScavengerGraphics.orig_ApplyPalette orig, ScavengerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig.Invoke(self, sLeaser, rCam, palette);
            if (!self.scavenger.IsSCScav())
                return;
            var extra = SCScavExtra.TryGetSCScav(self.scavenger);
            if(extra.decorationColoredHands > 0f)
            {
                var blendedBodyColor = self.BlendedBodyColor;
                var decorationColor = Color.Lerp(blendedBodyColor, self.decorationColor.rgb, extra.decorationColoredHands);
                for (int l = 0; l < 2; l++)
                {
                    for (int m = 0; m < 4; m++)
                    {
                        for (int n = 7; n < 11; n++)

                        {
                            (sLeaser.sprites[self.hands[l].firstSprite] as TriangleMesh).verticeColors[n] = Color.Lerp(blendedBodyColor, decorationColor, self.iVars.handsHeadColor);
                        }
                        (sLeaser.sprites[self.hands[l].firstSprite] as TriangleMesh).verticeColors[6] = Color.Lerp(blendedBodyColor, decorationColor, 0.5f * self.iVars.handsHeadColor);
                        (sLeaser.sprites[self.hands[l].firstSprite] as TriangleMesh).verticeColors[11] = Color.Lerp(blendedBodyColor, decorationColor, 0.5f * self.iVars.handsHeadColor);
                        (sLeaser.sprites[self.hands[l].firstSprite] as TriangleMesh).verticeColors[5] = Color.Lerp(blendedBodyColor, decorationColor, 0.2f * self.iVars.handsHeadColor);
                        (sLeaser.sprites[self.hands[l].firstSprite] as TriangleMesh).verticeColors[12] = Color.Lerp(blendedBodyColor, decorationColor, 0.2f * self.iVars.handsHeadColor);
                    }
                    sLeaser.sprites[self.hands[l].firstSprite + 1].color = Color.Lerp(blendedBodyColor, decorationColor, self.iVars.handsHeadColor);
                }
            }
        }

        //初始化外观额外参数
        private static void IndividualVariations_ctor(On.ScavengerGraphics.IndividualVariations.orig_ctor orig, ref ScavengerGraphics.IndividualVariations self, Scavenger scavenger)
        {
            orig.Invoke(ref self, scavenger);
            self.eyeSize *= 1.1f;
            self.fatness *= 1.05f;
            self.coloredEartlerTips = true;
            var extra = SCScavExtra.TryGetSCScav(scavenger);
            extra.InitGraphicsIndividualParam();
        }

        //重新生成拾荒的配色
        private static void ScavengerGraphics_GenerateColors(On.ScavengerGraphics.orig_GenerateColors orig, ScavengerGraphics self)
        {
            //self.scavenger.room.AddObject(new IDLabel(self.scavenger.room, self.scavenger));
            if (!self.scavenger.IsSCScav())
            {
                orig.Invoke(self);
                return;
            }

            Personality personality = self.scavenger.abstractCreature.personality;

            float bodyHue = 0f;

            //高勇气的高侵略配色上限由红偏紫
            float hightLightHueTop = Mathf.Lerp(0f, -0.2f, Mathf.Pow(personality.bravery, 1.5f) + 0.1f * personality.energy);

            //高侵略低同情拾荒配色接近上限
            float highLightHue = Mathf.Lerp(0f, 0.45f, (1f - Mathf.Pow(1f - personality.sympathy, 2f) + personality.energy * 0.1f) * personality.aggression);
            if (Random.value > 0.2)
                highLightHue += Random.value * 0.3f - 0.15f;
            if(Random.value > 0.8)
                highLightHue += Random.value * 0.6f - 0.3f;

            //高同情拾荒配色接近蓝色
            highLightHue = Mathf.Lerp(highLightHue, 0.741f, personality.sympathy + (1 - personality.aggression) * 0.3f + (Random.value * 0.2f - 0.1f));

            float f = Random.value;
            if(f < 0.2f)//同色相
            {
                bodyHue = highLightHue;
                bodyHue += Random.value * 0.1f - 0.05f;
            }
            else if(f < 0.4f)//相反色
            {
                bodyHue = 1f + highLightHue;
                bodyHue += Random.value * 0.1f - 0.05f;
            }
            else if(f < 0.6f)//对比色
            {
                bodyHue = highLightHue + Random.value < 0.5f ? 0.33f : -0.33f;
                bodyHue += Random.value * 0.1f - 0.05f;
            }
            else
            {
                bodyHue = Random.value;
                bodyHue = Mathf.Lerp(bodyHue, Random.value < 0.5f ? highLightHue : 1f + highLightHue, Mathf.Pow(Random.value * 0.8f, 3f));
            }
           
            float lightness = Mathf.Lerp(0.075f, 0.25f, Random.value);
            float saturation = Mathf.Lerp(0.1f, 0.4f,Random.value * (1f - lightness));
            //saturation *= Random.value;

            bool lightBodyColor = false;
            bool paleGreyColor = false;
            if (Random.value < 0.1f)//小概率出现高明度
            {
                lightness = Mathf.Lerp(lightness, 0.6f, Random.value * 0.5f + 0.5f);
                lightBodyColor = true;
            }

            if(Random.value < 0.3f && !lightBodyColor)//较小概率出现0饱和度
            {
                saturation = 0f;
                paleGreyColor = true;
            }
            

            self.bodyColor = new HSLColor(bodyHue, saturation, lightness);
            self.bodyColorBlack = Custom.LerpMap((self.bodyColor.rgb.r + self.bodyColor.rgb.g + self.bodyColor.rgb.b) / 3f, 0.04f, 0.8f, 0.3f, 0.95f, 0.5f);
            self.bodyColorBlack = Mathf.Lerp(self.bodyColorBlack, Mathf.Lerp(0.5f, 1f, Random.value), Random.value * Random.value * Random.value);
            self.bodyColorBlack *= self.iVars.generalMelanin;



            float darker = Mathf.Pow(Random.value, 1.4f);
            darker = 1f - darker;
            self.headColor = new HSLColor(bodyHue + Random.value * 0.1f - 0.05f, Mathf.Lerp(saturation, 1f - Random.value, darker), Mathf.Lerp(lightness, 0.05f + 0.1f * Random.value, darker));

           
            self.headColor.saturation = self.headColor.saturation * Mathf.Pow(1f - self.iVars.generalMelanin, 2f);
            self.headColor.saturation = self.headColor.saturation * (0.1f + 0.9f * Mathf.InverseLerp(0.1f, 0f, Custom.DistanceBetweenZeroToOneFloats(self.bodyColor.hue, self.headColor.hue) * Custom.LerpMap(Mathf.Abs(0.5f - self.headColor.lightness), 0f, 0.5f, 1f, 0.3f)));

            if (Random.value < 0.6f || lightBodyColor)
            {
                self.headColor.lightness = Mathf.Lerp(self.headColor.lightness, 0f, Random.value * 0.3f + 0.7f);
            }


            if (self.headColor.lightness < 0.5f)
            {
                self.headColor.lightness = self.headColor.lightness * (0.5f + 0.5f * Mathf.InverseLerp(0.2f, 0.05f, Custom.DistanceBetweenZeroToOneFloats(self.bodyColor.hue, self.headColor.hue)));
            }
            self.headColorBlack = Custom.LerpMap((self.headColor.rgb.r + self.headColor.rgb.g + self.headColor.rgb.b) / 3f, 0.035f, 0.26f, 0.7f, 0.95f, 0.25f);
            self.headColorBlack = Mathf.Lerp(self.headColorBlack, Mathf.Lerp(0.8f, 1f, Random.value), Random.value * Random.value * Random.value);
            self.headColorBlack *= 0.2f + 0.7f * self.iVars.generalMelanin;
            self.headColorBlack = Mathf.Max(self.headColorBlack, self.bodyColorBlack);
            self.headColor.saturation = Custom.LerpMap(self.headColor.lightness * (1f - self.headColorBlack), 0f, 0.15f, 1f, self.headColor.saturation);

            if (self.headColor.lightness > self.bodyColor.lightness)
                self.headColor = self.bodyColor;

            if (self.headColor.saturation < self.bodyColor.saturation * 0.75f)
            {
                if (Random.value < 0.5f)
                    self.headColor.hue = self.bodyColor.hue;
                else
                    self.headColor.lightness = self.headColor.lightness * 0.25f;
                self.headColor.saturation = self.bodyColor.saturation * 0.75f;
            }

            if (paleGreyColor)
                self.headColor.saturation = 0f;


            //self.decorationColor = new HSLColor(bodyHue + Random.value * 0.1f - 0.05f, Mathf.Lerp(saturation, 1f, darker), Mathf.Lerp(lightness, 0f, darker));
            //self.decorationColor.lightness = self.decorationColor.lightness * Mathf.Lerp(self.iVars.generalMelanin, Random.value, 0.5f);

            self.eyeColor = new HSLColor(highLightHue,1f, (Random.value < 0.2f) ? (0.5f + Random.value * 0.5f) : 0.5f);
            self.decorationColor = new HSLColor(highLightHue + Mathf.Pow(Random.value, 2f) * 0.2f - 0.1f, self.eyeColor.saturation, self.eyeColor.lightness);

            self.bellyColor = new HSLColor(Mathf.Lerp(bodyHue, self.decorationColor.hue, Random.value * 0.1f),  Mathf.Lerp(0f, saturation, Random.value * 0.2f - 0.1f), lightness + 0.05f + 0.05f * Random.value);
        }
    }
}
