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


            //float angle = 110f;

            //for(int i = 0;i < 4; i++)
            //{
            //    segment.Clear();
            //    float2 vertical = new float2(0.15f, -0.2f + i * 0.15f);
            //    segment.Add(new Vertex( vertical, 1.5f));//原点
            //    segment.Add(new Vertex(Custom.DegToFloat2(angle - i * 20) * (0.75f + i * 0.05f) + vertical, 1f));
            //    segment.Add(new Vertex(Custom.DegToFloat2(angle - i * 20 - 40f) * (0.05f + 0.05f * i) + vertical + segment.Last().pos, 0f));
            //    self.DefineBranch(segment);
            //}

            float angle = 90f;
            float length = 0.3f;
            float rad = 1f;

            segment.Add(new Vertex(new float2(0.15f, -0.2f), 1.5f));//原点
            for (int i = 0; i < 5; i++)
            {
                float2 posLast = segment.Last().pos;
                segment.Add(new Vertex(Custom.DegToFloat2(angle) * length + posLast, rad));

                rad *= 0.95f;
                length *= 0.95f;
                angle -= 40f;
            }
            self.DefineBranch(segment);


            return;
            bool elite = self.owner.scavenger.Elite;
            float num = (elite ? 1.75f : 1f);
            float2 angleLim = new float2(elite ? 45f : 15f, elite ? 90f : 45f);

            float width1 = (elite ? 1.5f : 1f);
            float rad1 = (elite ? 2f : 1f);
            float num4 = (elite ? 1f : 1f);
            float num5 = (elite ? 0f : 1f);
            float num6 = (elite ? 2f : 1f);
            float num7 = (elite ? 0f : 1f);

            self.points = new List<Vertex[]>();
            segment = new List<Vertex>();
            segment.Clear();

            //第一节
            segment.Add(new Vertex(new float2(0f, 0f), 1f));//原点
            segment.Add(new Vertex(Custom.DegToFloat2(Mathf.Lerp(40f, 90f, Random.value)) * 0.4f * width1, 1f * rad1));
            float2 point1 = Custom.DegToFloat2(Mathf.Lerp(angleLim.x, angleLim.y, Random.value) * num);
            float2 point2 = point1 - Custom.DegToFloat2(Mathf.Lerp(40f, 90f, Random.value)) * 0.4f * width1;
            if (point2.x < 0.2f)
            {
                point2 = new float2(Mathf.Lerp(point2.x, point1.x, 0.4f), point2.y);
            }
            segment.Add(new Vertex(point2, 1.5f * num4));
            segment.Add(new Vertex(point1, 2f * num5));
            self.DefineBranch(segment);
            segment.Clear();

            //第二节
            segment.Add(new Vertex(self.points[0][1].pos, 1f));
            int num8 = (((double)math.distance(self.points[0][1].pos, self.points[0][2].pos) > 0.6 && Random.value < 0.5f) ? 2 : 1);
            float2 float4 = math.lerp(self.points[0][1].pos, self.points[0][2].pos, Mathf.Lerp(0f, (num8 == 1) ? 0.7f : 0.25f, Random.value));
            segment.Add(new Vertex(float4, 1.2f));
            segment.Add(new Vertex(float4 + self.points[0][3].pos - self.points[0][2].pos + Custom.DegToFloat2(Random.value * 360f) * 0.1f, 1.75f));
            self.DefineBranch(segment);
            if (num8 == 2)
            {
                segment.Clear();
                float4 = math.lerp(self.points[0][1].pos, self.points[0][2].pos, Mathf.Lerp(0.45f, 0.7f, Random.value));
                segment.Add(new Vertex(float4, 1.2f));
                segment.Add(new Vertex(float4 + self.points[0][3].pos - self.points[0][2].pos + Custom.DegToFloat2(Random.value * 360f) * 0.1f, 1.75f));
                self.DefineBranch(segment);
            }
            bool flag = Random.value < 0.5f && !elite;
            if (flag)
            {
                segment.Clear();
                float2 float5 = Custom.DegToFloat2(90f + Mathf.Lerp(-20f, 20f, Random.value)) * Mathf.Lerp(0.2f, 0.5f, Random.value);
                if (float5.y > self.points[0][1].pos.y - 0.1f)
                {
                    float5 = new float2(float5.x, float5.y - 0.2f);
                }
                float num9 = Mathf.Lerp(0.8f, 2f, Random.value);
                if (Random.value < 0.5f)
                {
                    float5 += Custom.DegToFloat2(Mathf.Lerp(120f, 170f, Random.value)) * Mathf.Lerp(0.1f, 0.3f, Random.value);
                    segment.Add(new Vertex(new float2(0f, 0f), num9));
                    segment.Add(new Vertex(float5, num9));
                }
                else
                {
                    segment.Add(new Vertex(new float2(0f, 0f), 1f));
                    segment.Add(new Vertex(float5, (1f + num9) / 2f));
                    segment.Add(new Vertex(float5 + Custom.DegToFloat2(Mathf.Lerp(95f, 170f, Random.value)) * Mathf.Lerp(0.1f, 0.2f, Random.value), num9));
                }
                self.DefineBranch(segment);
            }
            if (Random.value > 0.25f || !flag || elite)
            {
                segment.Clear();
                float num10 = 1f + Random.value * 1.5f;
                bool flag2 = Random.value < 0.5f;
                segment.Add(new Vertex(new float2(0f, 0f), 1f));
                float num11 = Mathf.Lerp(95f, 135f, Random.value);
                float num12 = Mathf.Lerp(0.25f, 0.4f, Random.value) * num6;
                segment.Add(new Vertex(Custom.DegToFloat2(num11) * num12, (flag2 ? 0.8f : Mathf.Lerp(1f, num10, 0.3f)) * num6));
                segment.Add(new Vertex(Custom.DegToFloat2(num11 + Mathf.Lerp(5f, 35f, Random.value)) * Mathf.Max(num12 + 0.1f, Mathf.Lerp(0.3f, 0.6f, Random.value)), flag2 ? 0.8f : Mathf.Lerp(1f, num10, 0.6f)));
                segment.Add(new Vertex(segment[segment.Count - 1].pos.normalized() * (segment[segment.Count - 1].pos.magnitude() + Mathf.Lerp(0.15f, 0.25f, Random.value) * num6), num10 * num7));
                self.DefineBranch(segment);
            }
        }

        private static void Scavenger_ctor(On.Scavenger.orig_ctor orig, Scavenger self, AbstractCreature abstractCreature, World world)
        {
            orig.Invoke(self, abstractCreature, world);
            SCScavExtra.TryGetSCScav(self, true);
        }

        private static void ScavengerGraphics_ctor(MonoMod.Cil.ILContext il)
        {
            ILCursor c1 = new ILCursor(il);
            try
            {
                c1.GotoNext(MoveType.After, (i) => i.MatchNewobj<WobblyBackTufts>());
                c1.GotoNext(MoveType.After, (i) => i.MatchAdd());
                c1.GotoNext(MoveType.After, (i) => i.MatchAdd());
                c1.Index++;

                c1.Emit(OpCodes.Ldarg_0);
                c1.Emit(OpCodes.Ldloc_1);
                c1.EmitDelegate<Func<ScavengerGraphics, int, int>>((self, num) =>
                {
                    if (self.subModules.Count(i => i is HardBackSpikes) == 0)
                    {
                        var spike = new SCHardBackTufts(self, num);
                        self.subModules.Add(spike);
                        num += spike.totalSprites;
                    }
                    return num;
                });
                c1.Emit(OpCodes.Stloc_1);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private static void ScavengerGraphics_ApplyPalette(On.ScavengerGraphics.orig_ApplyPalette orig, ScavengerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig.Invoke(self, sLeaser, rCam, palette);
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


        private static void IndividualVariations_ctor(On.ScavengerGraphics.IndividualVariations.orig_ctor orig, ref ScavengerGraphics.IndividualVariations self, Scavenger scavenger)
        {
            orig.Invoke(ref self, scavenger);
            self.eyeSize *= 1.1f;
            self.fatness *= 1.05f;
            self.coloredEartlerTips = true;
            var extra = SCScavExtra.TryGetSCScav(scavenger);
            extra.InitGraphicsIndividualParam();
        }

        /// <summary>重新生成拾荒的配色 </summary>
        private static void ScavengerGraphics_GenerateColors(On.ScavengerGraphics.orig_GenerateColors orig, ScavengerGraphics self)
        {
            //self.scavenger.room.AddObject(new IDLabel(self.scavenger.room, self.scavenger));

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
