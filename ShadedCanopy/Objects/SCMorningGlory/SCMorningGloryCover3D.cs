using IL.Menu;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ShadedCanopy.Objects.SCMorningGlory
{
    internal class SCMorningGloryCover3D
    {
        public int edgeCount
        {
            private set; get;
        }
        public int nodeCount
        {
            private set; get;
        }
        
        SCMorningGlory owner;
        Vector3[][] vertices;
        public float height, width;
        public float heightCurvePct, widthCurvePct;
        SelfRestoringParameter spinAngle, flipAngle;
        public float spinOffset;
        int startIdx;
        Color color;
        public int spriteCount
        {
            get => (this.edgeCount + 1) / 2 * 2;
        }
        int frontSpriteCount { get => this.spriteCount / 2; }
        int backSpriteCount { get => this.spriteCount / 2; }
        int frontSpriteStartIdx { get => this.startIdx + this.backSpriteCount; }
        int backSpriteStartIdx { get => this.startIdx; }
        public SCMorningGloryCover3D(SCMorningGlory owner, int edgeCount, int nodeCount, float height, float width)
        {
            this.edgeCount = edgeCount;
            this.nodeCount = nodeCount;
            this.owner = owner;
            this.width = width;
            this.height = height;
            this.heightCurvePct = this.widthCurvePct = 0f;
            this.spinAngle = new SelfRestoringParameter();
            this.flipAngle = new SelfRestoringParameter();
            UpdateVertices();
        }
        void UpdateVertices()
        {
            this.vertices = new Vector3[this.edgeCount][];
            Vector3 up = new Vector3(0f, 0f, 0f);
            Vector3 orig = new Vector3(0f, -this.height, 0f);
            for (int i = 0; i < this.edgeCount; ++i)
            {
                this.vertices[i] = new Vector3[this.nodeCount];
                Vector2 buff = RWCustom.Custom.RotateAroundOrigo(new Vector2(this.width, 0f), i * (360f / this.edgeCount));
                Vector3 width = new Vector3(buff.x, -this.height, buff.y);
                Vector3 upCurve = up - (up - orig) * this.heightCurvePct;
                Vector3 widthCurve = width - (width - orig) * this.widthCurvePct;
                // bezier
                for (int j = 0; j < this.nodeCount; ++j)
                {
                    float f = j * 1.0f / (this.nodeCount - 1);
                    Vector3 vector = Vector3.Lerp(upCurve, widthCurve, f);
                    Vector3 cA = Vector3.Lerp(up, upCurve, f);
                    Vector3 cB = Vector3.Lerp(widthCurve, width, f);
                    cA = Vector3.Lerp(cA, vector, f);
                    cB = Vector3.Lerp(vector, cB, f);
                    Vector3 pos = Vector3.Lerp(cA, cB, f);
                    this.vertices[i][j] = pos;
                }
            }
        }
        public void Update()
        {
            UpdateVertices();
            DisturbUpdate();
            this.spinAngle.Update();
            this.flipAngle.Update();
        }
        public void DisturbUpdate()  // 在自己不动的时候自动扰动
        {
            if (this.owner.firstChunk.vel.magnitude > 1f)
            {
                if (this.spinAngle.generallyStatic && UnityEngine.Random.value < ModifiableSCMorningGloryProperty.scMorningGlory.coverAutoDisturbProb)
                {
                    float force = this.owner.firstChunk.vel.magnitude;
                    force = Mathf.Min(force, ModifiableSCMorningGloryProperty.scMorningGlory.coverDistrubMaxForce);
                    float rnd = UnityEngine.Random.Range(-1f, 1f);
                    this.spinAngle.vel += rnd * force * ModifiableSCMorningGloryProperty.scMorningGlory.coverDistrubSpinMultipler;
                }
                if (this.flipAngle.generallyStatic && UnityEngine.Random.value < ModifiableSCMorningGloryProperty.scMorningGlory.coverAutoDisturbProb)
                {
                    float force = this.owner.firstChunk.vel.magnitude;
                    force = Mathf.Min(force, ModifiableSCMorningGloryProperty.scMorningGlory.coverDistrubMaxForce);
                    float rnd = UnityEngine.Random.Range(-1f, 1f);
                    this.flipAngle.vel += rnd * force * ModifiableSCMorningGloryProperty.scMorningGlory.coverDistrubFlipMultipler;
                }
            }
        }
        public void SpinDisturb(float stength)  //强制转动扰动
        {
            float spinRnd = Mathf.Sign(UnityEngine.Random.Range(-1f, 1f));
            this.spinAngle.vel += stength * spinRnd * ModifiableSCMorningGloryProperty.scMorningGlory.coverDistrubSpinMultipler;
        }
        public void FlipDisturb(float stength)  //强制翻转扰动
        {
            float flipRnd = Mathf.Sign(UnityEngine.Random.Range(-1f, 1f));
            this.flipAngle.vel += stength * flipRnd * ModifiableSCMorningGloryProperty.scMorningGlory.coverDistrubFlipMultipler;
        }
        Vector3[][] ReflectingVertice(float spin, float flip)
        {
            Vector3[][] ret = new Vector3[this.edgeCount][];
            Quaternion rot1 = Quaternion.Euler(0f, spin, 0f);
            Quaternion rot2 = Quaternion.Euler(flip, 0f, 0f);
            Quaternion totalRot = rot2 * rot1;
            for (int i = 0; i < this.edgeCount; ++i)
            {
                ret[i] = new Vector3[this.nodeCount];
                for (int j = 0; j < this.nodeCount; ++j)
                {
                    ret[i][j] = totalRot * this.vertices[i][j];
                }
            }
            return ret;
        }
        public void InitiateSprite(int startIdx, RoomCamera.SpriteLeaser spriteLeaser, RoomCamera rCam)
        {
            this.startIdx = startIdx;
            for (int i = 0; i < this.spriteCount; ++i)
            {
                TriangleMesh.Triangle[] trig = new TriangleMesh.Triangle[(this.nodeCount - 1) * 2];
                for (int j = 0; j < this.nodeCount - 1; ++j)
                {
                    trig[j * 2] = new TriangleMesh.Triangle(j, j + 1, j + this.nodeCount);
                    trig[j * 2 + 1] = new TriangleMesh.Triangle(j + 1, j + this.nodeCount, j + this.nodeCount + 1);
                }
                TriangleMesh trigMesh = new TriangleMesh("Futile_White", trig, true);
                trigMesh.shader = RWCustom.Custom.rainWorld.Shaders["SCMorningGlory"];
                spriteLeaser.sprites[i + this.startIdx] = trigMesh;
            }
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            FContainer backContainer = rCam.ReturnFContainer("BackgroundShortcuts");
            FContainer frontConatiner = rCam.ReturnFContainer("Midground");
            for (int i = 0; i < this.spriteCount; ++i)
            {
                sLeaser.sprites[i + this.startIdx].RemoveFromContainer();
            }
            for (int i = 0; i < this.backSpriteCount; ++i)
            {
                backContainer.AddChild(sLeaser.sprites[i + this.startIdx]);
            }
            for (int i = 0; i < this.frontSpriteCount; ++i)
            {
                frontConatiner.AddChild(sLeaser.sprites[i + frontSpriteStartIdx]);
            }
        }
        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            for (int i = 0; i < this.spriteCount; ++i)
            {
                sLeaser.sprites[i + this.startIdx].color = RWCustom.Custom.HSL2RGB(i * 1.0f / this.spriteCount, 1f, 0.5f);
            }
            this.color = Color.Lerp(this.owner.personalization.color, palette.fogColor, ModifiableSCMorningGloryProperty.scMorningGlory.fogDepth);
        }
        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 posOffset = Vector2.Lerp(this.owner.firstChunk.lastPos, this.owner.firstChunk.pos, timeStacker) - camPos;
            float rotateAngle;
            Vector2 rotateDir;
            if (this.owner.hangingFruit != null)
            {
                Vector2 fruitPos = Vector2.Lerp(this.owner.hangingFruit.firstChunk.lastPos, this.owner.hangingFruit.firstChunk.pos, timeStacker) - camPos;
                rotateAngle = RWCustom.Custom.AimFromOneVectorToAnother(fruitPos, posOffset);
                rotateDir = RWCustom.Custom.DirVec(fruitPos, posOffset);
            } else
            {
                BodyChunk bc = this.owner.bodyChunks[this.owner.nodeChunkIdx[this.owner.nodeCount - 2]];
                Vector2 upupPos = Vector2.Lerp(bc.lastPos, bc.pos, timeStacker) - camPos;
                rotateAngle = RWCustom.Custom.AimFromOneVectorToAnother(posOffset, upupPos);
                rotateDir = RWCustom.Custom.DirVec(posOffset, upupPos);
            }
            posOffset += rotateDir * ModifiableSCMorningGloryProperty.scMorningGlory.coverLiftDistance;
            for (int i = 0; i < this.spriteCount; ++i)
            {
                sLeaser.sprites[i + this.startIdx].isVisible = false;
            }
            float spinAngle = Mathf.Lerp(this.spinAngle.lastValue, this.spinAngle.value, timeStacker);
            float flipAngle = Mathf.Lerp(this.flipAngle.lastValue, this.flipAngle.value, timeStacker);
            Vector3[][] vert = ReflectingVertice(spinAngle, flipAngle);
            int allocBackSpriteIdx = this.backSpriteStartIdx, allocFrontSpriteIdx = this.frontSpriteStartIdx;
            for (int i = 0; i < this.edgeCount; ++i)
            {
                Vector3[] curr = vert[i], next= vert[(i + 1) % this.edgeCount];
                int spriteIdx;
                bool inFront;
                if (curr[this.nodeCount - 1].x < next[this.nodeCount - 1].x)
                {
                    spriteIdx = allocFrontSpriteIdx++;
                    inFront = true;
                }
                else
                {
                    spriteIdx = allocBackSpriteIdx++;
                    inFront = false;
                }
                TriangleMesh mesh = sLeaser.sprites[spriteIdx] as TriangleMesh;
                mesh.isVisible = true;
                for (int j = 0; j < this.nodeCount; ++j)
                {
                    Vector2 va = curr[j];
                    Vector2 vb = next[j];
                    va = RWCustom.Custom.RotateAroundOrigo(va, rotateAngle);
                    vb = RWCustom.Custom.RotateAroundOrigo(vb, rotateAngle);
                    mesh.MoveVertice(j, va + posOffset);
                    mesh.MoveVertice(j + this.nodeCount, vb + posOffset);
                }
                for (int j = 0; j < this.nodeCount; ++j)
                {
                    mesh.UVvertices[j] = new Vector2(0, Mathf.InverseLerp(this.vertices[0][0].y, this.vertices[0][this.nodeCount - 1].y, this.vertices[0][j].y));
                    mesh.UVvertices[j + this.nodeCount] = new Vector2(1, Mathf.InverseLerp(this.vertices[0][0].y, this.vertices[0][this.nodeCount - 1].y, this.vertices[0][j].y));
                }
                Color color = Color.Lerp(this.color, (i % 2 == 0 ? Color.white : Color.black), 0.1f);
                if (!inFront)
                {
                    color = Color.Lerp(color, Color.white, 0.2f);
                }
                mesh.color = color;
                mesh._renderLayer?._material.SetFloat("_GradientLength", ModifiableSCMorningGloryProperty.scMorningGlory.coverGradientLength);
                mesh._renderLayer?._material.SetFloat("_DarknessStart", 0);
                mesh._renderLayer?._material.SetFloat("_DarknessEnd", ModifiableSCMorningGloryProperty.scMorningGlory.coverGradientDarknessMax);
            }
        }
    }
    internal class SelfRestoringParameter
    {
        public float value, lastValue;
        public float gravity, airFriction;
        public float lowGravityRange;
        public float vel;
        public bool generallyStatic
        {
            get { return Mathf.Abs(this.value) < 1f && Mathf.Abs(this.vel) < 1f; }
        }
        public SelfRestoringParameter(float gravity = 0.9f, float airFriction = 0.97f, float lowGravityRange = 20f)
        {
            this.gravity = gravity;
            this.airFriction = airFriction;
            this.lowGravityRange = lowGravityRange;
            this.HardSetValue(0);
            this.vel = 0;
        }
        public void HardSetValue(float v)
        {
            this.value = v;
            this.lastValue = v;
        }
        public void Update()
        {
            this.lastValue = this.value;
            this.value += this.vel;
            this.vel *= this.airFriction;
            float sign = Mathf.Sign(this.value);
            if (Mathf.Abs(this.value) < this.lowGravityRange)
            {
                this.vel -= sign * this.gravity * 0.2f;
            }
            else
            {
                this.vel -= sign * this.gravity;
            }
            if (this.generallyStatic)
            {
                this.value = this.vel = 0f;
            }
        }
    }
}
