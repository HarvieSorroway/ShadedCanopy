using RWCustom;
using ShadedCanopy.FlashingEffect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShadedCanopy.PlacedObjects.DeadlyLight
{
    internal class DeadlyLight : CosmeticSprite
    {
        static int pixPerTile = 20;
        static float maskScale = pixPerTile / 20f;

        PlacedObject placedObject;
        RenderTexture mask;
        DeadlyLightData Data => placedObject.data as DeadlyLightData;

        Texture2D secondaryPalette;
        FlashingEffectManager.DeadlyLightShaderInstance shaderInstance;

        public DeadlyLight(Room room, PlacedObject placedObject)
        {
            this.room = room;
            this.placedObject = placedObject;
            SCUtils.Utils.Log("Deadly Light ctor");

            var param = GetMaskParam();
            mask = FlashingEffectManager.CreateMask(room, 20, param.Item1, param.Item2);
            shaderInstance = FlashingEffectManager.GetShaderInstance<FlashingEffectManager.DeadlyLightShaderInstance>();
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            rCam.LoadPalette(0, ref secondaryPalette);
            sLeaser.sprites = new FSprite[7];
            sLeaser.sprites[0] = new FSprite("pixel", true)
            {
                shader = Custom.rainWorld.Shaders["DeadlyLight_ForegroundGrab"]
            };
            //  1   3
            // 
            //0   2   4
            sLeaser.sprites[1] = new TriangleMesh("pixel", new TriangleMesh.Triangle[]
            {
                new TriangleMesh.Triangle(0, 1, 2),
                new TriangleMesh.Triangle(1, 2, 3),
                new TriangleMesh.Triangle(2, 3, 4),
            }, true, false);

            sLeaser.sprites[1].shader = Custom.rainWorld.Shaders["DeadlyLight"];
            (sLeaser.sprites[1] as TriangleMesh).UVvertices = new Vector2[]
            {
                new Vector2(0,1),
                new Vector2(0, 0),
                new Vector2(0.5f, 1f),
                new Vector2(1f,0f),
                 new Vector2(1f,1f),
            };
            for (int i = 2; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i] = new FSprite("pixel", true)
                {
                    color = Color.red,
                    scale = 3f
                };
            }

            AddToContainer(sLeaser, rCam, null);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (newContatiner == null)
            {
                newContatiner = rCam.ReturnFContainer("GrabShaders");
            }

            sLeaser.sprites[0].RemoveFromContainer();
            rCam.ReturnFContainer("Items").AddChild(sLeaser.sprites[0]);

            for (int i = 1;i < sLeaser.sprites.Length;i++)
            {
                var fSprite = sLeaser.sprites[i];
                fSprite.RemoveFromContainer();
                newContatiner.AddChild(fSprite);
            }
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            var s = sLeaser.sprites[1] as TriangleMesh;
            sLeaser.sprites[0].MoveToFront();

            Vector2 dir = Data.handlePos.normalized;
            Vector2 perpDir = Custom.PerpendicularVector(dir);
            Vector4 _LevelMaskScrPos_LevelSize = new Vector4(camPos.x, camPos.y, mask.width, mask.height);

            float width = Data.handlePos.magnitude;
            float height = Data.maxLightRange;
            float penumbraWidth = Data.penumbraWidth;

            Vector2 pos0 = placedObject.pos + perpDir * height - dir * penumbraWidth / 2f - camPos;
            Vector2 pos1 = placedObject.pos - camPos;
            Vector2 pos3 = placedObject.pos + Data.handlePos - camPos;
            Vector2 pos4 = placedObject.pos + Data.handlePos + perpDir * height + dir * penumbraWidth / 2f - camPos;
            Vector2 pos2 = (pos0 + pos4) / 2f;


            s.MoveVertice(0, pos0);
            s.MoveVertice(1, pos1);
            s.MoveVertice(2, pos2);
            s.MoveVertice(3, pos3);
            s.MoveVertice(4, pos4);
            sLeaser.sprites[2].SetPosition(pos0);
            sLeaser.sprites[3].SetPosition(pos1);
            sLeaser.sprites[4].SetPosition(pos2);
            sLeaser.sprites[5].SetPosition(pos3);
            sLeaser.sprites[6].SetPosition(pos4);

            if (s._renderLayer != null)
            {
                var param = GetMaskParam();
                FlashingEffectManager.CaculateLevelMask(mask, room, 20, param.Item1, param.Item2, 50f);

                s._renderLayer._material.SetTexture("_LevelMask", mask);
                s._renderLayer._material.SetTexture("_SecondaryPalTex", secondaryPalette);

                s._renderLayer._material.SetFloat("_LevelMaskScale", maskScale);
                s._renderLayer._material.SetFloat("_Exposure", Data.exposure);
                s._renderLayer._material.SetFloat("_ClampedDepth", Data.clampedDepth);

                s._renderLayer._material.SetVector("_LevelMaskScrPos_LevelSize", _LevelMaskScrPos_LevelSize);

                s._renderLayer._material.SetFloat("_PenumbraPercentage", Mathf.Min(0.5f, penumbraWidth / (width + penumbraWidth)));

                s._renderLayer._material.SetVector("lightScreenPos", param.Item2 - camPos);
            }

        }

        public override void Destroy()
        {
            base.Destroy();
            shaderInstance.Release();
            shaderInstance = null;

            mask.Release();
            UnityEngine.Object.Destroy(mask);
        }

        (Vector2, Vector2) GetMaskParam()
        {
            Vector2 basePos = placedObject.pos + Data.handlePos * 0.5f;
            Vector2 dir = Data.handlePos.normalized;
            Vector2 perpDir = Custom.PerpendicularVector(dir);

            float width = Data.handlePos.magnitude;
            float height = Data.maxLightRange;
            float penumbraWidth = Data.penumbraWidth;

            Vector2 LightSourcePos = basePos - perpDir * height * width / penumbraWidth;

            return (basePos, LightSourcePos);
        }
    }
}
