using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using RWCustom;

namespace ShadedCanopy.FlashingEffect
{
    internal class FlashingEffectTest : CosmeticSprite
    {
        static int pixPerTile = 20;

        static float maxLife = 160f;
        static float preFlashTime = 0.1f;
        static float flashExpandTime_1 = 0.15f;
        static float flashExpandTime_2 = 0.2f;

        BodyChunk bindChunk;

        RenderTexture levelMask;
        FlashingEffectManager.FlashBangShaderInstance flashBangShaderInstace_1, flashBangShaderInstace_2;

        float levelMaskScale;
        int life = (int)maxLife;
        int lastLife = (int)maxLife;

        bool MaskNeedUpdate => lastPos != pos;

        public FlashingEffectTest(Room room, BodyChunk bindChunk)
        {
            this.room = room;
            this.bindChunk = bindChunk;
            this.pos = this.lastPos = bindChunk.pos;

            levelMask = FlashingEffectManager.CreateMask(room, pixPerTile, pos);
            levelMaskScale = pixPerTile / 20f;
            flashBangShaderInstace_1 = FlashingEffectManager.GetFlashBangShaderInstance();
            flashBangShaderInstace_2 = FlashingEffectManager.GetFlashBangShaderInstance();
        }

    
        public override void Update(bool eu)
        {
            lastPos = pos;
            pos = bindChunk.pos;
            evenUpdate = eu;

            lastLife = life;
            if(life > 0)
            {
                life--;
                if (life == 0) 
                    Destroy();
            }
        }


        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[2];
            sLeaser.sprites[0] = new FSprite("Futile_White", true)
            {
                shader = Custom.rainWorld.Shaders[flashBangShaderInstace_1.Shader],
            };
            sLeaser.sprites[1] = new FSprite("Futile_White", true)
            {
                shader = Custom.rainWorld.Shaders[flashBangShaderInstace_2.Shader],
            };

            AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("GrabShaders"));
            
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            if (!sLeaser.deleteMeNextFrame && (base.slatedForDeletetion || room != rCam.room))
            {
                return;
            }
            Vector2 smoothPos = Vector2.Lerp(lastPos, pos, timeStacker);
            float smoothLife = 1f - Mathf.Lerp(lastLife, life, timeStacker) / maxLife;

            float strenght_1 = 0, strenght_2 = 0, grabWaveRad_1 = 0f, grabWaveRad_2 = 0f, grabWaveStrength_1 = 0f, grabWaveStrength_2 = 0f;
            Vector4 _LevelMaskScrPos_LevelSize = new Vector4(camPos.x, camPos.y, levelMask.width, levelMask.height);

            sLeaser.sprites[0].SetPosition(smoothPos - camPos);
            sLeaser.sprites[1].SetPosition(smoothPos - camPos);

            if (smoothLife < preFlashTime)
            {
                float stage_f = SCHelper.EaseInOutCubic((smoothLife - 0f) / preFlashTime);
                sLeaser.sprites[0].scale = stage_f * 5f;
                strenght_1 = 0.5f + 0.3f * stage_f;
                grabWaveRad_1 = grabWaveStrength_1 = 0f;
            }
            else if (smoothLife < flashExpandTime_1)
            {
                float stage_f = SCHelper.EaseInOutCubic((smoothLife - preFlashTime) / (flashExpandTime_1 - preFlashTime));
                sLeaser.sprites[0].scale = 5f + 45f * stage_f;
                strenght_1 = 0.8f + 0.2f * stage_f;
                grabWaveStrength_1 = stage_f;
                grabWaveRad_1 = stage_f * 0.3f;
            }
            else if(smoothLife < 0.6f)
            {
                float stage_f = SCHelper.EaseInOutCubic((smoothLife - flashExpandTime_1) / (0.6f - flashExpandTime_1));
                sLeaser.sprites[0].scale = 50f - stage_f * 10f;
                strenght_1 = 1.0f - stage_f;
                grabWaveRad_1 = stage_f * 0.7f + 0.3f;
                grabWaveStrength_1 = 1f - stage_f;
            }
            else
            {
                sLeaser.sprites[0].scale = 40f;
                strenght_1 = 0f;
                grabWaveRad_1 = 0f;
                grabWaveStrength_1 = 0f;
            }

            if (smoothLife < preFlashTime)
            {
                sLeaser.sprites[1].scale = 0f;
                strenght_2 = 0f;
                grabWaveRad_2 = grabWaveStrength_2 = 0f;
            }
            else if (smoothLife < flashExpandTime_2)
            {
                float stage_f = SCHelper.EaseInOutCubic((smoothLife - preFlashTime) / (flashExpandTime_2 - preFlashTime));
                sLeaser.sprites[1].scale = 80f * stage_f;
                strenght_2 = stage_f;
                grabWaveRad_2 = stage_f * 0.3f;
                grabWaveStrength_2 = stage_f;
            }
            else
            {
                float stage_f = (smoothLife - flashExpandTime_2) / (1f - flashExpandTime_2);
                sLeaser.sprites[1].scale = 80f - 40f * stage_f;
                strenght_2 = 1f - stage_f;
                grabWaveRad_2 = stage_f * 0.7f + 0.3f;
                grabWaveStrength_2 = 1f - stage_f;
            }

            if (MaskNeedUpdate && !slatedForDeletetion)
                FlashingEffectManager.CaculateLevelMask(levelMask, room, pixPerTile, smoothPos);

            if (sLeaser.sprites[0]._renderLayer != null)
            {
                sLeaser.sprites[0]._renderLayer._material.SetTexture("_LevelMask", levelMask);
                sLeaser.sprites[0]._renderLayer._material.SetFloat("_LevelMaskScale", levelMaskScale);
                
                sLeaser.sprites[0]._renderLayer._material.SetVector("_LevelMaskScrPos_LevelSize", _LevelMaskScrPos_LevelSize);
                sLeaser.sprites[0]._renderLayer._material.SetFloat("_Strength", strenght_1 * 1.5f);

                sLeaser.sprites[0]._renderLayer._material.SetFloat("_GrabWaveRad", grabWaveRad_1);
                sLeaser.sprites[0]._renderLayer._material.SetFloat("_GrabWaveStrength", grabWaveStrength_1);
            }
            if (sLeaser.sprites[1]._renderLayer != null)
            {
                sLeaser.sprites[1]._renderLayer._material.SetTexture("_LevelMask", levelMask);
                sLeaser.sprites[1]._renderLayer._material.SetFloat("_LevelMaskScale", levelMaskScale);

                sLeaser.sprites[1]._renderLayer._material.SetVector("_LevelMaskScrPos_LevelSize", _LevelMaskScrPos_LevelSize);
                sLeaser.sprites[1]._renderLayer._material.SetFloat("_Strength", strenght_2 * 2f);

                sLeaser.sprites[1]._renderLayer._material.SetFloat("_GrabWaveRad", grabWaveRad_2);
                sLeaser.sprites[1]._renderLayer._material.SetFloat("_GrabWaveStrength", grabWaveStrength_2);
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            if (slatedForDeletetion)
                return;
            UnityEngine.Object.Destroy(levelMask);
            flashBangShaderInstace_1.Release();
            flashBangShaderInstace_2.Release();
        }
    }
}
