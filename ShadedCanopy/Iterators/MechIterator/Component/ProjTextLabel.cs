using Menu.Remix.MixedUI;
using RWCustom;
using SCUtils;
using SCUtils.SCDevTools.NodeTreeManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ShadedCanopy.Iterators.MechIterator.Component
{
    public class ProjTextLabel : CosmeticSprite
    {
        public static Color labelCol = Custom.hexToColor("7BFFF0");

        //public FLabel[] rLabels, gLabels, bLabels;
        FFont fontInfo;
        FLetterQuad[] quadInfos;
        public Vector2[] rBias, gBias, bBias;
        public float[] flashAlphas;

        List<Vector2> topLeftRelativePos = new List<Vector2>();
        List<FLetterQuadLine> quadLineInfo;

        //动画参数
        public float revealProgression, lastRevealProgression, revealTextRange;
        public float flicker;

        int stayOnTime;

        bool labelCleaned, turningOff;

        /// <summary>
        /// 有意义的最大显示进度，用于动画显示结束判断
        /// </summary>
        public float MaxRevealProgression => quadInfos.Length + revealTextRange;


        /// <summary>
        /// </summary>
        /// <param name="room"></param>
        /// <param name="text"></param>
        /// <param name="topLeft"></param>
        /// <param name="revealTextRange">处于动画状态下的字符数</param>
        /// <param name="stayOnTime">完成显示后存留时间，-1则为自动计算</param>
        public ProjTextLabel(Room room, string text, Vector2 topLeft, float revealTextRange = 5f, int stayOnTime = -1)
        {
            this.room = room;
            this.pos = this.lastPos = topLeft;
            this.revealTextRange = revealTextRange;

            if (stayOnTime == -1)
            {
                if (room.game.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Chinese ||
                   room.game.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Japanese ||
                   room.game.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Korean)
                {
                    this.stayOnTime = Mathf.CeilToInt(40f * text.Length * 2f / 10f);
                }
                else
                {
                    this.stayOnTime = Mathf.CeilToInt(40f * text.Length / 10f);
                }
            }
            else
                this.stayOnTime = stayOnTime;

            //计算字间距
            string font = Custom.GetDisplayFont();
            fontInfo = Futile.atlasManager.GetFontWithName(font);

            text = LabelTest.GlobalTextModifier(text);

            float anchorX = 0f, anchorY = 1f;
            List<FLetterQuad> quadInfoLst = new List<FLetterQuad>();
            FLetterQuadLine[] quadLines = Futile.atlasManager.GetFontWithName(font).GetQuadInfoForText(text, new FTextParams());
            float ymin = float.MaxValue;
            float ymax = float.MinValue;
            float xmin = float.MaxValue;
            float xmax = float.MinValue;
            int num5 = quadLines.Length;
            for (int i = 0; i < num5; i++)
            {
                FLetterQuadLine fletterQuadLine = quadLines[i];
                ymin = Math.Min(fletterQuadLine.bounds.yMin, ymin);
                ymax = Math.Max(fletterQuadLine.bounds.yMax, ymax);
            }
            float num6 = -(ymin + (ymax - ymin) * anchorY);
            for (int j = 0; j < num5; j++)
            {
                FLetterQuadLine fletterQuadLine2 = quadLines[j];
                float num7 = -fletterQuadLine2.bounds.width * anchorX;
                xmin = Math.Min(num7, xmin);
                xmax = Math.Max(num7 + fletterQuadLine2.bounds.width, xmax);
                int num8 = fletterQuadLine2.quads.Length;
                for (int k = 0; k < num8; k++)
                {
                    fletterQuadLine2.quads[k].CalculateVectors(num7 + fontInfo.offsetX, num6 + fontInfo.offsetY);
                }
            }

            foreach (var line in quadLines)
            {
                foreach (var quad in line.quads)
                {
                    topLeftRelativePos.Add(quad.rect.position + new Vector2(quad.charInfo.offsetX, quad.charInfo.offsetY));
                    quadInfoLst.Add(quad);
                }
            }
            text = Regex.Replace(text, "\n", string.Empty);


            //初始化标签
            //rLabels = new FLabel[text.Length];
            //gLabels = new FLabel[text.Length];
            //bLabels = new FLabel[text.Length];
            quadInfos = quadInfoLst.ToArray();

            rBias = new Vector2[text.Length];
            gBias = new Vector2[text.Length];
            bBias = new Vector2[text.Length];

            flashAlphas = new float[text.Length];

            for (int i = 0; i < text.Length; i++)
            {
                string charStr = text.Substring(i, 1);
                //rLabels[i] = new FLabel(font, charStr) { shader = room.game.rainWorld.Shaders[SCResources.AdditiveDefaultShaderName], color = Color.red , alpha = 0f};
                //gLabels[i] = new FLabel(font, charStr) { shader = room.game.rainWorld.Shaders[SCResources.AdditiveDefaultShaderName], color = Color.green , alpha = 0f};
                //bLabels[i] = new FLabel(font, charStr) { shader = room.game.rainWorld.Shaders[SCResources.AdditiveDefaultShaderName], color = Color.blue , alpha = 0f };

                rBias[i] = Custom.RNV() * Mathf.Lerp(5f, 8f, Random.value);
                gBias[i] = Custom.RNV() * Mathf.Lerp(5f, 8f, Random.value);
                bBias[i] = Custom.RNV() * Mathf.Lerp(5f, 8f, Random.value);

                if (string.IsNullOrWhiteSpace(charStr))
                {
                    flashAlphas[i] = 0f;
                }
                else
                    flashAlphas[i] = 1f;
            }
            SCDevNodeTreeManager.Track(this);
        }

        int RLabelIndex(int i)
        {
            return i * 4;
        }
        int GLabelIndex(int i)
        {
            return i * 4 + 1;
        }
        int BLabelIndex(int i)
        {
            return i * 4 + 2;
        }
        int FlahsyIndex(int i)
        {
            return i * 4 + 3;
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[quadInfos.Length * 4];
            for (int i = 0; i < quadInfos.Length; i++)
            {
                var quad = quadInfos[i];

                var rCharSprite = new CustomUVFSprite(fontInfo.element.name, true)
                {
                    shader = room.game.rainWorld.Shaders[SCResources.AdditiveDefaultShaderName],
                    color = Color.red,
                    alpha = 0
                };
                rCharSprite.SetCustomUVs(quad.charInfo.uvTopLeft, quad.charInfo.uvTopRight, quad.charInfo.uvBottomRight, quad.charInfo.uvBottomLeft);
                sLeaser.sprites[RLabelIndex(i)] = rCharSprite;

                var gCharSprite = new CustomUVFSprite(fontInfo.element.name, true)
                {
                    shader = room.game.rainWorld.Shaders[SCResources.AdditiveDefaultShaderName],
                    color = Color.green,
                    alpha = 0
                };
                gCharSprite.SetCustomUVs(quad.charInfo.uvTopLeft, quad.charInfo.uvTopRight, quad.charInfo.uvBottomRight, quad.charInfo.uvBottomLeft);
                sLeaser.sprites[GLabelIndex(i)] = gCharSprite;

                var bCharSprite = new CustomUVFSprite(fontInfo.element.name, true)
                {
                    shader = room.game.rainWorld.Shaders[SCResources.AdditiveDefaultShaderName],
                    color = Color.blue,
                    alpha = 0
                };
                bCharSprite.SetCustomUVs(quad.charInfo.uvTopLeft, quad.charInfo.uvTopRight, quad.charInfo.uvBottomRight, quad.charInfo.uvBottomLeft);
                sLeaser.sprites[BLabelIndex(i)] = bCharSprite;

                sLeaser.sprites[FlahsyIndex(i)] = new FSprite(SCResources.Blur80Atlas, true)
                {
                    shader = room.game.rainWorld.Shaders[SCResources.AdditiveDefaultShaderName],
                    color = Color.blue,
                    //scale = 4f,
                    alpha = 0f
                };
            }
            AddToContainer(sLeaser, rCam, null);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (newContatiner == null)
                newContatiner = rCam.ReturnFContainer("HUD");
            //for(int i = 0;i < rLabels.Length;i++)
            //{
            //    newContatiner.AddChild(sLeaser.sprites[i]);
            //    newContatiner.AddChild(rLabels[i]);
            //    newContatiner.AddChild(gLabels[i]);
            //    newContatiner.AddChild(bLabels[i]);
            //}

            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                newContatiner.AddChild(sLeaser.sprites[i]);
            }
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            if (slatedForDeletetion)
                return;

            lastRevealProgression = revealProgression;

            if (turningOff)
            {
                flicker += 1 / 40f;
                if (flicker >= 1f)
                {
                    Destroy();
                }
            }
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

            if (slatedForDeletetion)
            {
                if (!labelCleaned)
                    CleanOutLabels();
                return;
            }

            Vector2 smoothPos = Vector2.Lerp(lastPos, pos, timeStacker) - camPos;
            float smoothRevealProgression = Mathf.Lerp(lastRevealProgression, revealProgression, timeStacker);

            for (int i = 0; i < quadInfos.Length; i++)
            {
                float localReveal = Mathf.Clamp01((smoothRevealProgression - i) * (1f - flicker) / revealTextRange);

                float targetAlphaR, targetAlphaG, targetAlphaB;
                targetAlphaR = Mathf.Lerp(1f, labelCol.r, localReveal);
                targetAlphaG = Mathf.Lerp(1f, labelCol.g, localReveal);
                targetAlphaB = Mathf.Lerp(1f, labelCol.b, localReveal);

                sLeaser.sprites[RLabelIndex(i)].alpha = (Random.value < localReveal ? 1f : 0f) * targetAlphaR;
                Vector2 rBasePos = rBias[i] * Mathf.Pow(1f - localReveal, 2f) + smoothPos;
                (sLeaser.sprites[RLabelIndex(i)] as CustomUVFSprite).MoveVertices(quadInfos[i].topLeft + rBasePos, quadInfos[i].topRight + rBasePos, quadInfos[i].bottomRight + rBasePos, quadInfos[i].bottomLeft + rBasePos);

                sLeaser.sprites[GLabelIndex(i)].alpha = (Random.value < localReveal ? 1f : 0f) * targetAlphaG;
                Vector2 gBasePos = gBias[i] * Mathf.Pow(1f - localReveal, 2f) + smoothPos;
                (sLeaser.sprites[GLabelIndex(i)] as CustomUVFSprite).MoveVertices(quadInfos[i].topLeft + gBasePos, quadInfos[i].topRight + gBasePos, quadInfos[i].bottomRight + gBasePos, quadInfos[i].bottomLeft + gBasePos);

                sLeaser.sprites[BLabelIndex(i)].alpha = (Random.value < localReveal ? 1f : 0f) * targetAlphaB;
                Vector2 bBasePos = bBias[i] * Mathf.Pow(1f - localReveal, 2f) + smoothPos;
                (sLeaser.sprites[BLabelIndex(i)] as CustomUVFSprite).MoveVertices(quadInfos[i].topLeft + bBasePos, quadInfos[i].topRight + bBasePos, quadInfos[i].bottomRight + bBasePos, quadInfos[i].bottomLeft + bBasePos);

                sLeaser.sprites[FlahsyIndex(i)].SetPosition(quadInfos[i].rect.position + smoothPos);
                sLeaser.sprites[FlahsyIndex(i)].alpha = Mathf.Pow((sLeaser.sprites[RLabelIndex(i)].alpha + sLeaser.sprites[GLabelIndex(i)].alpha + sLeaser.sprites[BLabelIndex(i)].alpha) / 3f, 2f) * flashAlphas[i];
            }
        }

        public void TurnOff()
        {
            turningOff = true;
        }

        public override void Destroy()
        {
            base.Destroy();
            CleanOutLabels();
        }

        public void CleanOutLabels()
        {
            if (labelCleaned)
                return;

            //for(int i = 0;i < rLabels.Length; i++)
            //{
            //    rLabels[i].RemoveFromContainer();
            //    gLabels[i].RemoveFromContainer();
            //    bLabels[i].RemoveFromContainer();
            //}
            labelCleaned = true;
        }

    }
}
