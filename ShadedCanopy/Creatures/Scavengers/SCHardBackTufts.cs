using RWCustom;
using ScavengerCosmetic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace ShadedCanopy.Creatures.Scavengers
{
    internal class SCHardBackTufts : BackDecals
    {
        int scaleGraf;
        float scaleGrafHeight;
        float scaleGrafWidth;
        float generalSize;
        float xFlip;
        float[] sizes;

        public SCHardBackTufts(ScavengerGraphics owner, int firstSprite) : base(owner, firstSprite)
        {
            scaleGraf = UnityEngine.Random.Range(0, 7);
            xFlip = -1f;
            if (scaleGraf == 3)
            {
                xFlip = 1f;
            }
            if (UnityEngine.Random.value < 0.025f)
            {
                xFlip = -xFlip;
            }
            if (UnityEngine.Random.value < 0.5f)
            {
                xFlip *= 0.5f + 0.5f * UnityEngine.Random.value;
            }

            pattern = ((UnityEngine.Random.value < 0.6f) ? BackDecals.Pattern.SpineRidge : BackDecals.Pattern.DoubleSpineRidge);
            if (UnityEngine.Random.value < 0.1f)
            {
                pattern = BackDecals.Pattern.RandomBackBlotch;
            }
            base.GeneratePattern(pattern);
            totalSprites = positions.Length;
            if (UnityEngine.Random.value < 0.5f)
            {
                if (UnityEngine.Random.value < 0.85f)
                {
                    scaleGraf = UnityEngine.Random.Range(0, 4);
                }
                else
                {
                    scaleGraf = 6;
                }
            }
            sizes = new float[positions.Length];
            float num = Mathf.Lerp(0.1f, 0.6f, UnityEngine.Random.value);
            float num2 = Mathf.Lerp(0.3f, 1f, UnityEngine.Random.value);
            generalSize = Custom.LerpMap((float)positions.Length, 5f, 35f, 1f, 0.2f);
            generalSize = Mathf.Lerp(generalSize, base.scavGrphs.scavenger.abstractCreature.personality.dominance, UnityEngine.Random.value);
            generalSize = Mathf.Lerp(generalSize, Mathf.Pow(UnityEngine.Random.value, 0.75f), UnityEngine.Random.value);

            for (int i = 0; i < sizes.Length; i++)
            {
                sizes[i] = Mathf.Lerp(num, 1f, Mathf.Sin(Mathf.Pow(Mathf.InverseLerp(top, bottom, positions[i].y), num2) * 3.1415927f));
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            scaleGrafHeight = Futile.atlasManager.GetElementWithName("LizardScaleA" + scaleGraf.ToString()).sourcePixelSize.y;
            scaleGrafWidth = Futile.atlasManager.GetElementWithName("LizardScaleA" + scaleGraf.ToString()).sourcePixelSize.x;
            for (int i = 0; i < totalSprites; i++)
            {
                sLeaser.sprites[firstSprite + i] = new CustomFSprite("LizardScaleA" + scaleGraf.ToString());
            }
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            bool flag = false;

            float smoothBristle = Mathf.Lerp(scavGrphs.lastBristle, scavGrphs.bristle, timeStacker);
            Color dark = scavGrphs.BlendedBodyColor;
            Color light = Color.Lerp(dark, scavGrphs.decorationColor.rgb, Mathf.Sin(Time.time * Mathf.PI * 0.5f) * 0.5f + 0.5f);
            light = Color.Lerp(light, scavGrphs.eyeColor.rgb, smoothBristle);

            if (owner is ScavengerGraphics)
            {
                flag = (owner as ScavengerGraphics).scavenger.Elite;
            }
            float num = (flag ? 1.5f : 1f) * 1.2f;
            float num2 = (flag ? 1.5f : 1f) * 1.5f;

            for (int i = 0; i < totalSprites; i++)
            {
                float2 f0 = new Vector2(positions[i].x, positions[i].y);
                float2 pos = scavGrphs.OnBackSurfacePos(f0, timeStacker);
                float2 f2 = scavGrphs.OnSpineUpDir(positions[i].y, timeStacker) * Mathf.Lerp(4f, 14f * num2, generalSize) * (1f - 0.5f * Mathf.InverseLerp(0.5f, 1f, positions[i].y)) + scavGrphs.OnSpineDir(positions[i].y, timeStacker) * Mathf.Lerp(4f * num2, 18f * num2, generalSize) * 0.5f * Mathf.InverseLerp(0.5f, 1f, positions[i].y);
                float num3 = 1f + Mathf.Lerp(scavGrphs.lastBristle, scavGrphs.bristle, timeStacker) * 0.5f;
                Vector2 vector = Custom.RNV() * UnityEngine.Random.value * 0.2f * smoothBristle;
                Vector2 dir = new Vector2(f2.x * num3 + vector.x, f2.y * num3 + vector.y);

                Vector2 midPos = new Vector2(pos.x - camPos.x, pos.y - camPos.y);
                float width = Mathf.Sign(Mathf.Lerp(scavGrphs.lastFlip, scavGrphs.flip, timeStacker) + positions[i].x * 0.5f) * Mathf.Lerp(0.5f * num, 1f * num, generalSize) * xFlip * scaleGrafWidth;
                float height = dir.magnitude;
                dir = dir.normalized;
                Vector2 perpDir = Custom.PerpendicularVector(dir);

                int index = firstSprite + i;
                var sprite = sLeaser.sprites[index] as CustomFSprite;

                sprite.MoveVertice(0, midPos - width * 0.5f * perpDir - height * 0.1f * dir);
                sprite.MoveVertice(1, midPos - width * 0.5f * perpDir + height * 0.9f * dir);
                sprite.MoveVertice(2, midPos + width * 0.5f * perpDir + height * 0.9f * dir);
                sprite.MoveVertice(3, midPos + width * 0.5f * perpDir - height * 0.1f * dir);

                sprite.verticeColors[0] = dark;
                sprite.verticeColors[1] = light;
                sprite.verticeColors[2] = light;
                sprite.verticeColors[3] = dark;



                //sLeaser.sprites[index].rotation = Custom.VecToDeg(dir.normalized);
                //sLeaser.sprites[index].x = pos.x - camPos.x;
                //sLeaser.sprites[index].y = pos.y - camPos.y;
                //sLeaser.sprites[index].scaleX = Mathf.Sign(Mathf.Lerp(scavGrphs.lastFlip, scavGrphs.flip, timeStacker) + positions[i].x * 0.5f) * Mathf.Lerp(0.5f * num, 1f * num, generalSize) * xFlip;
            }
        }
    }
}
