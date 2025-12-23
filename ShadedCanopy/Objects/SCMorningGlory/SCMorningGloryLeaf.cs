using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShadedCanopy.Objects.SCMorningGlory
{
    internal class SCMorningGloryLeaf
    {
        SCMorningGlory owner;
        SelfRestoringParameter angleOffset;
        public float angle;
        int nodeIdx;
        Vector2 size;
        bool isLeft;
        int startIdx;
        public int spriteCount
        {
            get => 2;
        }
        public void Update()
        {
            DisturbUpdate();
            angleOffset.Update();
        }
        public SCMorningGloryLeaf(SCMorningGlory owner, int nodeIdx, bool isLeft, Vector2 size, float angle)
        {
            this.owner = owner;
            this.nodeIdx = nodeIdx;
            this.isLeft = isLeft;
            this.size = size;
            this.angle = angle;
            this.angleOffset = new SelfRestoringParameter();
        }
        public void InitiateSprite(int startIdx, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            this.startIdx = startIdx;
            sLeaser.sprites[startIdx] = new FSprite("DangleFruit0A", true);
            sLeaser.sprites[startIdx + 1] = new FSprite("DangleFruit0B", true);
            for (int i = 0; i < 2; ++i)
            {
                sLeaser.sprites[this.startIdx + i].anchorY = 0.01f;
                sLeaser.sprites[this.startIdx + i].scaleX = size.x;
                sLeaser.sprites[this.startIdx + i].scaleY = size.y;
            }
        }
        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            FContainer fContainer = rCam.ReturnFContainer("Midground");
            for (int i = 0; i < 2; ++i)
            {
                sLeaser.sprites[this.startIdx + i].RemoveFromContainer();
            }
            for (int i = 0; i < 2; ++i)
            {
                fContainer.AddChild(sLeaser.sprites[this.startIdx + i]);
            }
        }
        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            sLeaser.sprites[this.startIdx].color = this.owner.personalization.color;
            sLeaser.sprites[this.startIdx + 1].color = palette.blackColor;
        }
        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            BodyChunk thisNode = this.owner.bodyChunks[this.owner.nodeChunkIdx[this.nodeIdx]];
            BodyChunk prevNode = this.owner.bodyChunks[this.owner.nodeChunkIdx[this.nodeIdx - 1]];
            BodyChunk nextNode = this.owner.bodyChunks[this.owner.nodeChunkIdx[this.nodeIdx + 1]];
            Vector2 nodePos = Vector2.Lerp(thisNode.lastPos, thisNode.pos, timeStacker);
            Vector2 prevPos = Vector2.Lerp(prevNode.lastPos, prevNode.pos, timeStacker);
            Vector2 nextPos = Vector2.Lerp(nextNode.lastPos, nextNode.pos, timeStacker);
            Vector2 leafDirBase = RWCustom.Custom.DirVec(prevPos, nextPos);
            float angle = Mathf.Lerp(angleOffset.lastValue, angleOffset.value, timeStacker) + this.angle;
            Vector2 leafDir = RWCustom.Custom.RotateAroundOrigo(leafDirBase, (this.isLeft ? 1f : -1f) * angle);
            for (int i = 0; i < 2; ++i)
            {
                sLeaser.sprites[this.startIdx + i].rotation = RWCustom.Custom.VecToDeg(leafDir);
                sLeaser.sprites[this.startIdx + i].x = nodePos.x - camPos.x;
                sLeaser.sprites[this.startIdx + i].y = nodePos.y - camPos.y;
            }
            sLeaser.sprites[this.startIdx + 1].isVisible = false;
        }
        public void DisturbUpdate()  // 在自己不动的时候自动扰动
        {
            if (this.owner.firstChunk.vel.magnitude > 1f)
            {
                if (this.angleOffset.generallyStatic && UnityEngine.Random.value < ModifiableSCMorningGloryProperty.scMorningGlory.leafAutoDistrubProb)
                {
                    float force = this.owner.firstChunk.vel.magnitude;
                    float rnd = UnityEngine.Random.Range(-1f, 1f);
                    this.angleOffset.vel += rnd * force * ModifiableSCMorningGloryProperty.scMorningGlory.leafDistrubMultipler;
                }
            }
        }
        public void Disturb(float stength)  //强制转动扰动
        {
            float rnd = Mathf.Sign(UnityEngine.Random.Range(-1f, 1f));
            this.angleOffset.vel += stength * rnd * ModifiableSCMorningGloryProperty.scMorningGlory.leafDistrubMultipler;
        }
    }
}
