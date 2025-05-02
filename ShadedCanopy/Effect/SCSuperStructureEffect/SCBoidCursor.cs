using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ShadedCanopy.Effect.SCSuperStructureEffect
{
    internal class SCBoidCursor : SCSuperStructureProjPart
    {
        SCBoids boidsEffect;

        float followedBoidCount;
        int followedBoidIndex, noSwitchCounter;
        Vector2 followedBoidPos, lastFollowedBoidPos;
        int toleranceRange = 10;

        public SCBoidCursor(SCSuperStructureProj projector, SCBoids boidsEffect) : base(projector)
        {
            this.boidsEffect = boidsEffect;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            followedBoidCount = Mathf.Max(0f, followedBoidCount - 2f);
            if (noSwitchCounter > 0)
                noSwitchCounter--;
            else
            {

                for (int i = 0; i < 4; i++)
                {
                    int randomTryIndex = Random.Range(0, boidsEffect.totBoids.Count);
                    float newCount = boidsEffect.GetBoidWithinRad(boidsEffect.totBoids[randomTryIndex], boidsEffect.totBoids[randomTryIndex].pos, 80f, false).Count();
                    if (newCount > followedBoidCount - toleranceRange)
                    {
                        followedBoidCount = newCount;
                        followedBoidIndex = randomTryIndex;
                        noSwitchCounter = Random.Range(40, 160);
                    }
                }
            }

            lastFollowedBoidPos = followedBoidPos;
            if (Vector2.Distance(followedBoidPos, boidsEffect.totBoids[followedBoidIndex].pos) > 100f)
                lastFollowedBoidPos = followedBoidPos = boidsEffect.totBoids[followedBoidIndex].pos;
            else
                followedBoidPos = Vector2.Lerp(followedBoidPos, boidsEffect.totBoids[followedBoidIndex].pos, 0.15f);
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[2];
            sLeaser.sprites[0] = new FSprite("pixel", true)
            {
                scaleX = 1400f,
                scaleY = 2f,
                color = Color.black,
                //shader = rCam.room.game.rainWorld.Shaders["Hologram"],
                x = 700f,
                alpha = 0.8f
            };
            sLeaser.sprites[1] = new FSprite("pixel", true)
            {
                scaleX = 2f,
                scaleY = 1400f,
                color = Color.black,
                //shader = rCam.room.game.rainWorld.Shaders["Hologram"],
                y = 350,
                alpha = 0.8f
            };
            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            Vector2 smoothFollowPos = Vector2.Lerp(lastFollowedBoidPos, followedBoidPos, timeStacker);
            sLeaser.sprites[0].y = smoothFollowPos.y - camPos.y;
            sLeaser.sprites[1].x = smoothFollowPos.x - camPos.x;
        }
    }
}
