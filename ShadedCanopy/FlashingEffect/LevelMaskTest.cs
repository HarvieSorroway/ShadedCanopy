using IL;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShadedCanopy.FlashingEffect
{
    internal class LevelMaskTest : CosmeticSprite
    {
        static int pixPerTile = 40;
        BodyChunk bindchunk;
        RenderTexture levelMask;
        float levelMaskScale;
        bool MaskNeedUpdate => lastPos != pos;

        public LevelMaskTest(Room room, BodyChunk bodyChunk)
        {
            this.room = room;
            this.bindchunk = bodyChunk;
            levelMask = FlashingEffectManager.CreateMask(room, pixPerTile, pos);
            levelMaskScale = pixPerTile / 20f;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (bindchunk.owner.room != room)
                Destroy();

            this.pos = bindchunk.pos;
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("Futile_White", true)
            {
                shader = Custom.rainWorld.Shaders["LevelMaskTest"],
            };
            AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("GrabShaders"));
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            Vector2 smoothPos = Vector2.Lerp(lastPos, pos, timeStacker);
            sLeaser.sprites[0].SetPosition(smoothPos - camPos);
            sLeaser.sprites[0].scale = 50;

            Vector4 _LevelMaskScrPos_LevelSize = new Vector4(camPos.x, camPos.y, levelMask.width, levelMask.height);

            if (sLeaser.sprites[0]._renderLayer != null)
            {
                sLeaser.sprites[0]._renderLayer._material.SetTexture("_LevelMask", levelMask);
                sLeaser.sprites[0]._renderLayer._material.SetFloat("_LevelMaskScale", levelMaskScale);

                sLeaser.sprites[0]._renderLayer._material.SetVector("_LevelMaskScrPos_LevelSize", _LevelMaskScrPos_LevelSize);
            }

            if (MaskNeedUpdate && !slatedForDeletetion)
                FlashingEffectManager.CaculateLevelMask(levelMask, room, pixPerTile, smoothPos);
        }

        public override void Destroy()
        {
            base.Destroy();
            if (slatedForDeletetion)
                return;
            UnityEngine.Object.Destroy(levelMask);
        }
    }
}
