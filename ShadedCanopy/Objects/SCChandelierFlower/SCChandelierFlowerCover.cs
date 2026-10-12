using System;
using System.Collections.Generic;
using UnityEngine;
using ShadedCanopy.Objects.SCChandelierFlower.Mesh3D;
using SCUtils;

namespace ShadedCanopy.Objects.SCChandelierFlower
{
    using Triangle2D = Triangle<Vector2>;
    using Triangle3D = Triangle<Vector3>;
    using Triangle2DVector = List<Triangle<Vector2>>;
    using Triangle3DVector = List<Triangle<Vector3>>;

    internal class SCChandelierFlowerCover
    {
        SCChandelierFlower owner;
        TriangleMesh3D meshNormal;
        TriangleMesh3D meshWithered;
        TriangleMesh3D meshWork;
        SelfRestoringParameter spinAngle, flipAngle;
        Vector2 lastOwnerVel;
        bool hasLastOwnerVel;
        bool hadFruitLast;
        float noFruitLerp;
        float detachCoolDown;
        float wither;
        float lastWither;
        Vector2 lastFruitDir;
        bool hasLastFruitDir;
        float noiseTime;
        float spinNoiseSeed;
        float flipNoiseSeed;
        public int spriteCount => 0;

        int totalFaces;
        TriangleMesh[] triPool;

        public SCChandelierFlowerCover(SCChandelierFlower owner)
        {
            this.owner = owner;
            var prop = SCChandelierFlowerProperty.coverPhysicalProperty;
            this.spinAngle = new SelfRestoringParameter(prop.coverSpinGravity, prop.coverSpinAirFriction, prop.coverSpinMaxAngle);
            this.flipAngle = new SelfRestoringParameter(prop.coverFlipGravity, prop.coverFlipAirFriction, prop.coverFlipMaxAngle);
            
            // Build meshes
            var meshProp = MeshProp.From(SCChandelierFlowerProperty.normalCoverMeshProperty);
            var witheredMeshProp = MeshProp.From(SCChandelierFlowerProperty.coverWitheredMeshProperty);
            TriangleMesh3D t3m = MakeMesh(meshProp);
            TriangleMesh3D tinner = MakeInnerCoverMesh(meshProp);
            TriangleMesh3D toter = MakeOuterMesh(meshProp, owner.personalization.outerSpinOffset);
            TriangleMesh3D merged = TriangleMesh3D.Concat(new TriangleMesh3D[] { t3m, tinner, toter });
            this.meshNormal = merged.DeterministicSplitTriangle(meshProp.targetFaces);

            TriangleMesh3D t3mw = MakeMesh(witheredMeshProp);
            TriangleMesh3D tinnerw = MakeInnerCoverMesh(witheredMeshProp);
            TriangleMesh3D toterw = MakeOuterMesh(witheredMeshProp, owner.personalization.outerSpinOffset);
            TriangleMesh3D mergedw = TriangleMesh3D.Concat(new TriangleMesh3D[] { t3mw, tinnerw, toterw });
            this.meshWithered = mergedw.DeterministicSplitTriangle(meshProp.targetFaces);
            if (this.meshWithered.numVertices != this.meshNormal.numVertices || this.meshWithered.numTriangles != this.meshNormal.numTriangles)
            {
                this.meshWithered = this.meshNormal.Clone();
            }
            this.meshWork = this.meshNormal.Clone();
            this.totalFaces = this.meshNormal.numTriangles;
            this.lastOwnerVel = Vector2.zero;
            this.hasLastOwnerVel = false;
            this.hadFruitLast = owner.hangingFruit != null;
            this.noFruitLerp = this.hadFruitLast ? 0f : 1f;
            this.detachCoolDown = 0f;
            this.wither = owner.abstractChandelierFlower != null && owner.abstractChandelierFlower.isConsumed ? 1f : 0f;
            this.lastWither = this.wither;
            this.lastFruitDir = Vector2.zero;
            this.hasLastFruitDir = false;
            UnityEngine.Random.State state = UnityEngine.Random.state;
            UnityEngine.Random.InitState(owner.abstractPhysicalObject.ID.RandomSeed ^ 0x6A09E667);
            this.noiseTime = UnityEngine.Random.value * 1000f;
            this.spinNoiseSeed = UnityEngine.Random.value * 1000f;
            this.flipNoiseSeed = UnityEngine.Random.value * 1000f;
            UnityEngine.Random.state = state;
        }

        public void Update()
        {
            if (this.owner.bodyChunks == null || this.owner.bodyChunks.Length == 0)
            {
                return;
            }
            this.lastWither = this.wither;
            bool hasFruitNow = this.owner.hangingFruit != null;
            if (this.hadFruitLast && !hasFruitNow)
            {
                var prop = SCChandelierFlowerProperty.coverPhysicalProperty;
                this.detachCoolDown = prop.coverDetachCooldownInit;
                float sign = Mathf.Sign(Mathf.PerlinNoise(this.spinNoiseSeed, this.noiseTime) - 0.5f);
                this.spinAngle.AddImpulse(sign * this.spinAngle.lowGravityRange * prop.coverDetachImpulseSpinMultiplier);
                this.flipAngle.AddImpulse(-sign * this.flipAngle.lowGravityRange * prop.coverDetachImpulseFlipMultiplier);
            }
            this.hadFruitLast = hasFruitNow;

            float targetNoFruit = hasFruitNow ? 0f : 1f;
            this.noFruitLerp += (targetNoFruit - this.noFruitLerp) * SCChandelierFlowerProperty.coverPhysicalProperty.coverNoFruitLerpSpeed;
            this.noFruitLerp = Mathf.Clamp01(this.noFruitLerp);
            this.detachCoolDown = Mathf.Max(0f, this.detachCoolDown - SCChandelierFlowerProperty.coverPhysicalProperty.coverDetachCooldownDecay);
            int witherTicks = Mathf.Max(1, SCChandelierFlowerProperty.coverPhysicalProperty.coverWitherTicks);
            float targetWither = hasFruitNow ? 0f : 1f;
            this.wither = Mathf.MoveTowards(this.wither, targetWither, 1f / witherTicks);

            DisturbUpdate();
            this.spinAngle.Update();
            this.flipAngle.Update();
        }

        public void DisturbUpdate()
        {
            var prop = SCChandelierFlowerProperty.coverPhysicalProperty;
            if (!this.hasLastOwnerVel)
            {
                this.lastOwnerVel = this.owner.firstChunk.vel;
                this.hasLastOwnerVel = true;
            }
            Vector2 v = this.owner.firstChunk.vel;
            Vector2 dv = v - this.lastOwnerVel;
            this.lastOwnerVel = v;
            float speed = v.magnitude;
            float accel = dv.magnitude;

            float noFruitMult = Mathf.Lerp(1f, prop.coverNoFruitDisturbMultiplier, this.noFruitLerp);
            float spinMult = prop.coverDistrubSpinMultipler * noFruitMult;
            float flipMult = prop.coverDistrubFlipMultipler * noFruitMult;

            float intensity = Mathf.Clamp01(prop.coverAutoDisturbProb * prop.coverAutoDisturbIntensityMultiplier);
            float detachCooldown01 = prop.coverDetachCooldownInit <= 0f ? 0f : Mathf.Clamp01(this.detachCoolDown / prop.coverDetachCooldownInit);
            intensity *= Mathf.Lerp(1f, prop.coverDetachNoiseIntensityMin, detachCooldown01);
            float accelClamped = Mathf.Min(accel, prop.coverDistrubMaxForce);

            this.noiseTime += prop.coverNoiseTimeBaseStep + speed * prop.coverNoiseTimeVelStep;
            float nSpin = Mathf.PerlinNoise(this.spinNoiseSeed, this.noiseTime) * 2f - 1f;
            float nFlip = Mathf.PerlinNoise(this.flipNoiseSeed, this.noiseTime * 0.9f) * 2f - 1f;

            if (speed > 0.1f)
            {
                this.spinAngle.AddTarget(nSpin * speed * spinMult * intensity * 0.6f);
                this.flipAngle.AddTarget(nFlip * speed * flipMult * intensity * 0.4f);
            }
            if (accelClamped > 0.01f)
            {
                this.spinAngle.AddImpulse(nSpin * accelClamped * spinMult * 0.8f);
                this.flipAngle.AddImpulse(nFlip * accelClamped * flipMult * 0.6f);
            }
        }

        public void SpinDisturb(float strength)
        {
            var prop = SCChandelierFlowerProperty.coverPhysicalProperty;
            float noFruitMult = Mathf.Lerp(1f, prop.coverNoFruitDisturbMultiplier, this.noFruitLerp);
            float mult = prop.coverDistrubSpinMultipler * noFruitMult;
            float sign = Mathf.Sign(Mathf.PerlinNoise(this.spinNoiseSeed, this.noiseTime * 3f) - 0.5f);
            float impulse = Mathf.Min(strength, prop.coverDistrubMaxForce * 10f) * mult * 0.15f;
            this.spinAngle.AddImpulse(sign * impulse);
        }

        public void FlipDisturb(float strength)
        {
            var prop = SCChandelierFlowerProperty.coverPhysicalProperty;
            float noFruitMult = Mathf.Lerp(1f, prop.coverNoFruitDisturbMultiplier, this.noFruitLerp);
            float mult = prop.coverDistrubFlipMultipler * noFruitMult;
            float sign = Mathf.Sign(Mathf.PerlinNoise(this.flipNoiseSeed, this.noiseTime * 3f) - 0.5f);
            float impulse = Mathf.Min(strength, prop.coverDistrubMaxForce * 10f) * mult * 0.12f;
            this.flipAngle.AddImpulse(sign * impulse);
        }

        public void InitiateSprite(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            this.triPool = new TriangleMesh[this.totalFaces];
            for (int i = 0; i < this.totalFaces; i++)
            {
                TriangleMesh.Triangle[] tri = new TriangleMesh.Triangle[] { new TriangleMesh.Triangle(0, 1, 2) };
                this.triPool[i] = new TriangleMesh("Futile_White", tri, true);
            }
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            // Depth coloring is applied dynamically in DrawSprites because depth changes with rotation
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 posOffset = Vector2.Lerp(this.owner.firstChunk.lastPos, this.owner.firstChunk.pos, timeStacker) - camPos;

            float spinAngleVal = Mathf.Lerp(this.spinAngle.lastValue, this.spinAngle.value, timeStacker) + this.owner.personalization.coverSpinOffset;
            float flipAngleVal = Mathf.Lerp(this.flipAngle.lastValue, this.flipAngle.value, timeStacker);
            float witherT = Mathf.Lerp(this.lastWither, this.wither, timeStacker);

            if (this.meshWork != null && this.meshWithered != null && this.meshNormal != null && this.meshWork.numVertices == this.meshNormal.numVertices && this.meshWithered.numVertices == this.meshNormal.numVertices)
            {
                for (int i = 0; i < this.meshWork.p.Length; i++)
                {
                    this.meshWork.p[i] = Vector3.Lerp(this.meshNormal.p[i], this.meshWithered.p[i], witherT);
                }
            }
            TriangleMesh3D copy = this.meshWork.Clone();
            
            // Receptacle alignment rotation
            float rotateAngle = 0f;
            if (this.owner.hangingFruit != null)
            {
                Vector2 dir = Vector2.Lerp(this.owner.hangingFruit.LastRotation, this.owner.hangingFruit.Rotation, timeStacker);
                if (dir.sqrMagnitude > 0.0001f)
                {
                    rotateAngle = RWCustom.Custom.AimFromOneVectorToAnother(Vector2.zero, dir);
                    this.lastFruitDir = dir.normalized;
                    this.hasLastFruitDir = true;
                }
            }
            else if (this.owner.nodeCount >= 2)
            {
                BodyChunk bc = this.owner.bodyChunks[this.owner.nodeChunkIdx[this.owner.nodeCount - 2]];
                Vector2 upupPos = Vector2.Lerp(bc.lastPos, bc.pos, timeStacker) - camPos;
                Vector2 dir = upupPos - posOffset;
                if (this.owner.nodeCount >= 3)
                {
                    BodyChunk bc2 = this.owner.bodyChunks[this.owner.nodeChunkIdx[this.owner.nodeCount - 3]];
                    Vector2 upupupPos = Vector2.Lerp(bc2.lastPos, bc2.pos, timeStacker) - camPos;
                    Vector2 dir2 = upupupPos - upupPos;
                    if (dir.sqrMagnitude > 0.0001f) dir = dir.normalized;
                    if (dir2.sqrMagnitude > 0.0001f) dir2 = dir2.normalized;
                    Vector2 mixed = dir + dir2;
                    if (mixed.sqrMagnitude > 0.0001f) dir = mixed;
                }
                if (dir.sqrMagnitude > 0.0001f)
                {
                    if (this.hasLastFruitDir && this.noFruitLerp < 1f)
                    {
                        Vector2 stalkDir = dir.normalized;
                        Vector2 mixed = Vector2.Lerp(this.lastFruitDir, stalkDir, this.noFruitLerp);
                        if (mixed.sqrMagnitude > 0.0001f) dir = mixed;
                    }
                    rotateAngle = RWCustom.Custom.AimFromOneVectorToAnother(Vector2.zero, dir);
                }
            }

            Quaternion rotSpin = Quaternion.Euler(0f, spinAngleVal, 0f);
            Quaternion rotFlip = Quaternion.Euler(flipAngleVal, 0f, 0f);
            Quaternion rotRotate = Quaternion.Euler(0f, 0f, -rotateAngle);
            // apply spin
            copy.Rotate(rotSpin);
            bool[] triIsBack = new bool[copy.numTriangles];
            for (int i = 0; i < copy.numTriangles; i++)
            {
                var (pa, pb, pc) = copy.t[i];
                Vector3 c = (copy.p[pa] + copy.p[pb] + copy.p[pc]) / 3f;
                triIsBack[i] = (c.z < 0);
            }

            // Apply flip
            copy.Rotate(rotRotate * rotFlip);
            float coverScale = Mathf.Lerp(SCChandelierFlowerProperty.normalCoverMeshProperty.coverScale, SCChandelierFlowerProperty.coverWitheredMeshProperty.coverScale, witherT);
            for (int i = 0; i < copy.p.Length; i++)
            {
                copy.p[i] *= coverScale;
            }
            // Apply 2D alignment
            // TriangleMesh3D.RotateZ could do it, or we do it after projection.
            // Actually, Z rotation is just 2D rotation.
            
            Triangle3DVector tri3Ds = copy.Export();
            Color fogColor = rCam.currentPalette.fogColor;
            float lerpMult = SCChandelierFlowerProperty.outfitProperty.depthFogColorLerpMultiplier;
            tri3Ds.FillColor(fogColor, v => Mathf.Clamp01((coverScale - v.z) / coverScale / 2) * lerpMult);

            float vanishingZ = Mathf.Lerp(SCChandelierFlowerProperty.normalCoverMeshProperty.VanishingZ, SCChandelierFlowerProperty.coverWitheredMeshProperty.VanishingZ, witherT);
            Triangle3DVector tri3DsBack = new Triangle3DVector();
            Triangle3DVector tri3DsFront = new Triangle3DVector();
            for (int i = 0; i < tri3Ds.Count; i++)
            {
                if (triIsBack[i]) tri3DsBack.Add(tri3Ds[i]);
                else tri3DsFront.Add(tri3Ds[i]);
            }

            Triangle3DVector offsetedBack = tri3DsBack.PositionOffset(new Vector3(posOffset.x, posOffset.y, 0));
            Triangle3DVector offsetedFront = tri3DsFront.PositionOffset(new Vector3(posOffset.x, posOffset.y, 0));
            Triangle2DVector projectedBack = offsetedBack.Projection2D(new Vector3(1366f / 2f, 768f / 2f, vanishingZ));
            Triangle2DVector projectedFront = offsetedFront.Projection2D(new Vector3(1366f / 2f, 768f / 2f, vanishingZ));
            projectedBack.Sort();
            projectedFront.Sort();

            int meshIdx = 0;
            for (int i = 0; i < projectedBack.Count && meshIdx < this.triPool.Length; i++, meshIdx++)
            {
                Triangle2D t = projectedBack[i];
                TriangleMesh mesh = this.triPool[meshIdx];
                mesh.RemoveFromContainer();
                mesh.MoveVertice(0, t.A);
                mesh.MoveVertice(1, t.B);
                mesh.MoveVertice(2, t.C);
                mesh.verticeColors[0] = t.colorA;
                mesh.verticeColors[1] = t.colorB;
                mesh.verticeColors[2] = t.colorC;
                if (this.owner.containerDict != null && this.owner.containerDict.TryGetValue("Background1", out var bg1)) bg1.AddChild(mesh);
            }

            for (int i = 0; i < projectedFront.Count && meshIdx < this.triPool.Length; i++, meshIdx++)
            {
                Triangle2D t = projectedFront[i];
                TriangleMesh mesh = this.triPool[meshIdx];
                mesh.RemoveFromContainer();
                mesh.MoveVertice(0, t.A);
                mesh.MoveVertice(1, t.B);
                mesh.MoveVertice(2, t.C);
                mesh.verticeColors[0] = t.colorA;
                mesh.verticeColors[1] = t.colorB;
                mesh.verticeColors[2] = t.colorC;
                if (this.owner.containerDict != null && this.owner.containerDict.TryGetValue("Foreground1", out var fg1)) fg1.AddChild(mesh);
            }
        }

        static float PosFuncLR(float i)
        {
            return i;
        }
        static float PosFuncMid(float i)
        {
            return (Mathf.Exp(i) - 1) / (Mathf.Exp(1) - 1);
        }
        static Color Pos2Color(float subPos, MeshProp prop)
        {
            var outf = SCChandelierFlowerProperty.outfitProperty;
            if (subPos <= prop.C1Pos) return outf.C1Color;
            else if (subPos <= prop.C2Pos) return outf.C2Color;
            else if (subPos <= prop.C3Pos) return outf.C3Color;
            else return outf.BaseColor;
        }

        TriangleMesh3D MakeMesh(MeshProp prop)
        {
            TriangleMesh3D aLeaf;
            List<float> divs = new List<float>();
            for (int i = 1; i <= prop.nDivTriangles + 1; i++)
            {
                divs.Add(i / (float)(prop.nDivTriangles + 1));
            }
            float[] exSubColor = new float[] { prop.C1Pos, prop.C2Pos, prop.C3Pos };
            for (int i = 0; i < prop.nSubColorCount; i++)
            {
                divs.Add(exSubColor[i]);
                divs.Add(exSubColor[i] + prop.smallEps);
            }
            divs.Sort();
            List<float> filteredDiv = new List<float>();
            filteredDiv.Add(0);
            filteredDiv.Add(divs[0]);
            for (int i = 1; i < divs.Count; i++)
            {
                if (divs[i] - divs[i - 1] > prop.smallEps / 2)
                {
                    filteredDiv.Add(divs[i]);
                }
            }
            int nVertPerLine = filteredDiv.Count;
            List<ValueTuple<int, int, int>> triangles = new List<ValueTuple<int, int, int>>();
            for (int i = 1; i < nVertPerLine; ++i)
            {
                triangles.Add((i - 1, i, nVertPerLine + i - 1));
                triangles.Add((i, nVertPerLine + i, nVertPerLine + i - 1));

                triangles.Add((i - 1 + nVertPerLine * 2, i + nVertPerLine * 2, i - 1));
                triangles.Add((i + nVertPerLine * 2, i, i - 1));
            }
            aLeaf = new TriangleMesh3D(nVertPerLine * 3, triangles.ToArray());
            aLeaf.keepBackSurface = true;
            Vector3 LeftBezierA0 = new Vector3(-prop.RightBezierA0.x, prop.RightBezierA0.y, prop.RightBezierA0.z);
            Vector3 LeftBezierA1Offset = new Vector3(-prop.RightBezierA1Offset.x, prop.RightBezierA1Offset.y, prop.RightBezierA1Offset.z);
            Vector3 LeftBezierB0 = new Vector3(-prop.RightBezierB0.x, prop.RightBezierB0.y, prop.RightBezierB0.z);
            Vector3 LeftBezierB1Offset = new Vector3(-prop.RightBezierB1Offset.x, prop.RightBezierB1Offset.y, prop.RightBezierB1Offset.z);
            int maxDepth = 8;
            UtilTools.BezierUniformLerpPrepare centerBezier = new UtilTools.BezierUniformLerpPrepare(
                prop.CenterBezierA0, prop.CenterBezierA0 + prop.CenterBezierA1Offset,
                prop.CenterBezierB0, prop.CenterBezierB0 + prop.CenterBezierB1Offset, maxDepth);
            UtilTools.BezierUniformLerpPrepare rightBezier = new UtilTools.BezierUniformLerpPrepare(
                prop.RightBezierA0, prop.RightBezierA0 + prop.RightBezierA1Offset,
                prop.RightBezierB0, prop.RightBezierB0 + prop.RightBezierB1Offset, maxDepth);
            UtilTools.BezierUniformLerpPrepare leftBezier = new UtilTools.BezierUniformLerpPrepare(
                LeftBezierA0, LeftBezierA0 + LeftBezierA1Offset,
                LeftBezierB0, LeftBezierB0 + LeftBezierB1Offset, maxDepth);

            for (int i = 0; i < filteredDiv.Count; ++i)
            {
                float subPos = filteredDiv[i];
                Vector3 mid = centerBezier.BezierUniformLerp(PosFuncMid(subPos));
                Vector3 right = rightBezier.BezierUniformLerp(PosFuncLR(subPos));
                Vector3 left = leftBezier.BezierUniformLerp(PosFuncLR(subPos));
                Color color = Pos2Color(subPos, prop);
                aLeaf.p[i] = mid;
                aLeaf.p[i + nVertPerLine] = left;
                aLeaf.p[i + nVertPerLine * 2] = right;
                aLeaf.SetColor(i, color);
                aLeaf.SetColor(i + nVertPerLine, color);
                aLeaf.SetColor(i + nVertPerLine * 2, color);
            }
            List<TriangleMesh3D> triMesh3dList = new List<TriangleMesh3D>();
            for (int i = 0; i < prop.nLeaves; ++i)
            {
                TriangleMesh3D newLeaf = aLeaf.Clone();
                newLeaf.RotateY(i * 360f / prop.nLeaves);
                triMesh3dList.Add(newLeaf);
            }
            TriangleMesh3D merged = TriangleMesh3D.Concat(triMesh3dList.ToArray());
            return merged;
        }

        TriangleMesh3D MakeOuterMesh(MeshProp prop, float initSpinAngle)
        {
            TriangleMesh3D aLeaf;
            int nVertPerLine = prop.outerNDivTriangles + 2;
            List<ValueTuple<int, int, int>> triangles = new List<ValueTuple<int, int, int>>();
            for (int i = 1; i < nVertPerLine; ++i)
            {
                triangles.Add((i - 1, i, nVertPerLine + i - 1));
                triangles.Add((i, nVertPerLine + i, nVertPerLine + i - 1));

                triangles.Add((i - 1 + nVertPerLine * 2, i + nVertPerLine * 2, i - 1));
                triangles.Add((i + nVertPerLine * 2, i, i - 1));
            }

            aLeaf = new TriangleMesh3D(nVertPerLine * 3, triangles.ToArray());
            aLeaf.keepBackSurface = true;

            Vector3 leftBezierA0 = new Vector3(-prop.outerRightBezierA0.x, prop.outerRightBezierA0.y, prop.outerRightBezierA0.z);
            Vector3 leftBezierA1Offset = new Vector3(-prop.outerRightBezierA1Offset.x, prop.outerRightBezierA1Offset.y, prop.outerRightBezierA1Offset.z);
            Vector3 leftBezierB0 = new Vector3(-prop.outerRightBezierB0.x, prop.outerRightBezierB0.y, prop.outerRightBezierB0.z);
            Vector3 leftBezierB1Offset = new Vector3(-prop.outerRightBezierB1Offset.x, prop.outerRightBezierB1Offset.y, prop.outerRightBezierB1Offset.z);
            int maxDepth = 8;
            UtilTools.BezierUniformLerpPrepare centerBezier = new UtilTools.BezierUniformLerpPrepare(
                prop.outerCenterBezierA0, prop.outerCenterBezierA0 + prop.outerCenterBezierA1Offset,
                prop.outerCenterBezierB0, prop.outerCenterBezierB0 + prop.outerCenterBezierB1Offset, maxDepth);
            UtilTools.BezierUniformLerpPrepare rightBezier = new UtilTools.BezierUniformLerpPrepare(
                prop.outerRightBezierA0, prop.outerRightBezierA0 + prop.outerRightBezierA1Offset,
                prop.outerRightBezierB0, prop.outerRightBezierB0 + prop.outerRightBezierB1Offset, maxDepth);
            UtilTools.BezierUniformLerpPrepare leftBezier = new UtilTools.BezierUniformLerpPrepare(
                leftBezierA0, leftBezierA0 + leftBezierA1Offset,
                leftBezierB0, leftBezierB0 + leftBezierB1Offset, maxDepth);

            for (int i = 0; i < nVertPerLine; ++i)
            {
                float subPos = i / (float)(nVertPerLine - 1);
                Vector3 mid = centerBezier.BezierUniformLerp(PosFuncMid(subPos));
                Vector3 right = rightBezier.BezierUniformLerp(PosFuncLR(subPos));
                Vector3 left = leftBezier.BezierUniformLerp(PosFuncLR(subPos));
                Color color = prop.outerColor;
                aLeaf.p[i] = mid;
                aLeaf.p[i + nVertPerLine] = left;
                aLeaf.p[i + nVertPerLine * 2] = right;
                aLeaf.SetColor(i, color);
                aLeaf.SetColor(i + nVertPerLine, color);
                aLeaf.SetColor(i + nVertPerLine * 2, color);
            }

            int leafCount = Mathf.Max(1, prop.outerNLeaves);
            List<TriangleMesh3D> triMesh3dList = new List<TriangleMesh3D>();
            for (int i = 0; i < leafCount; ++i)
            {
                TriangleMesh3D newLeaf = aLeaf.Clone();
                newLeaf.RotateY(i * 360f / leafCount);
                triMesh3dList.Add(newLeaf);
            }

            TriangleMesh3D merged = TriangleMesh3D.Concat(triMesh3dList.ToArray());
            if (Mathf.Abs(initSpinAngle) > 0.0001f)
            {
                merged.RotateY(initSpinAngle);
            }

            return merged;
        }

        TriangleMesh3D MakeInnerCoverMesh(MeshProp prop)
        {
            TriangleMesh3D aLeaf;
            int nVert = prop.InnerNRidges * prop.InnerNVerticesPerRidge;
            int nTri = (prop.InnerNRidges - 1) * (prop.InnerNVerticesPerRidge - 1) * 2;
            aLeaf = new TriangleMesh3D(nVert, nTri);
            for (int i = 0; i < prop.InnerNRidges - 1; ++i)
            {
                for (int j = 0; j < prop.InnerNVerticesPerRidge - 1; ++j)
                {
                    int currJ = (i * prop.InnerNVerticesPerRidge + j);
                    int nextJ = (i + 1) * prop.InnerNVerticesPerRidge + j;
                    aLeaf.t[(i * (prop.InnerNVerticesPerRidge - 1) + j) * 2] = (currJ, nextJ, currJ + 1);
                    aLeaf.t[(i * (prop.InnerNVerticesPerRidge - 1) + j) * 2 + 1] = (nextJ, nextJ + 1, currJ + 1);
                }
            }
            UtilTools.BezierUniformLerpPrepare bezier = new UtilTools.BezierUniformLerpPrepare(
                new Vector3(0, prop.InnerBezierA0YZ.x, prop.InnerBezierA0YZ.y),
                new Vector3(0, prop.InnerBezierA0YZ.x + prop.InnerBezierA1OffsetYZ.x, prop.InnerBezierA0YZ.y + prop.InnerBezierA1OffsetYZ.y),
                new Vector3(0, prop.InnerBezierB0YZ.x, prop.InnerBezierB0YZ.y),
                new Vector3(0, prop.InnerBezierB0YZ.x + prop.InnerBezierB1OffsetYZ.x, prop.InnerBezierB0YZ.y + prop.InnerBezierB1OffsetYZ.y), 10);
            Vector3[] bezierLerps = new Vector3[prop.InnerNVerticesPerRidge];
            for (int i = 0; i < prop.InnerNVerticesPerRidge; ++i)
            {
                float t = i / (float)(prop.InnerNVerticesPerRidge - 1);
                bezierLerps[i] = bezier.BezierUniformLerp(t);
            }
            for (int i = 0; i < prop.InnerNRidges; ++i)
            {
                float angle = i * (prop.InnerAnglePerLeaf / (prop.InnerNRidges - 1)) - prop.InnerAnglePerLeaf / 2;
                Quaternion rot = Quaternion.Euler(0, angle, 0);
                for (int j = 0; j < prop.InnerNVerticesPerRidge; ++j)
                {
                    int vIdx = i * prop.InnerNVerticesPerRidge + j;
                    aLeaf.p[vIdx] = rot * bezierLerps[j];
                    aLeaf.SetColor(vIdx, Color.black);
                    aLeaf.SetBackColor(vIdx, Color.black);
                }
            }
            List<TriangleMesh3D> leafs = new();
            for (int i = 0; i < prop.nLeaves; ++i)
            {
                TriangleMesh3D newLeaf = aLeaf.Clone();
                newLeaf.RotateY(i * 360f / prop.nLeaves + prop.InnerAngleOffset);
                leafs.Add(newLeaf);
            }
            TriangleMesh3D ret = TriangleMesh3D.Concat(leafs);
            return ret;
        }

        readonly struct MeshProp
        {
            public readonly int nLeaves;
            public readonly Vector3 CenterBezierA0;
            public readonly Vector3 CenterBezierA1Offset;
            public readonly Vector3 CenterBezierB0;
            public readonly Vector3 CenterBezierB1Offset;
            public readonly Vector3 RightBezierA0;
            public readonly Vector3 RightBezierA1Offset;
            public readonly Vector3 RightBezierB0;
            public readonly Vector3 RightBezierB1Offset;
            public readonly int outerNLeaves;
            public readonly Vector3 outerCenterBezierA0;
            public readonly Vector3 outerCenterBezierA1Offset;
            public readonly Vector3 outerCenterBezierB0;
            public readonly Vector3 outerCenterBezierB1Offset;
            public readonly Vector3 outerRightBezierA0;
            public readonly Vector3 outerRightBezierA1Offset;
            public readonly Vector3 outerRightBezierB0;
            public readonly Vector3 outerRightBezierB1Offset;
            public readonly int outerNDivTriangles;
            public readonly Color outerColor;
            public readonly float C1Pos;
            public readonly float C2Pos;
            public readonly float C3Pos;
            public readonly int nSubColorCount;
            public readonly int nDivTriangles;
            public readonly float smallEps;
            public readonly Vector2 InnerBezierA0YZ;
            public readonly Vector2 InnerBezierA1OffsetYZ;
            public readonly Vector2 InnerBezierB0YZ;
            public readonly Vector2 InnerBezierB1OffsetYZ;
            public readonly float InnerAngleOffset;
            public readonly float InnerAnglePerLeaf;
            public readonly int InnerNRidges;
            public readonly int InnerNVerticesPerRidge;
            public readonly int targetFaces;

            MeshProp(
                int nLeaves,
                Vector3 centerBezierA0,
                Vector3 centerBezierA1Offset,
                Vector3 centerBezierB0,
                Vector3 centerBezierB1Offset,
                Vector3 rightBezierA0,
                Vector3 rightBezierA1Offset,
                Vector3 rightBezierB0,
                Vector3 rightBezierB1Offset,
                int outerNLeaves,
                Vector3 outerCenterBezierA0,
                Vector3 outerCenterBezierA1Offset,
                Vector3 outerCenterBezierB0,
                Vector3 outerCenterBezierB1Offset,
                Vector3 outerRightBezierA0,
                Vector3 outerRightBezierA1Offset,
                Vector3 outerRightBezierB0,
                Vector3 outerRightBezierB1Offset,
                int outerNDivTriangles,
                Color outerColor,
                float c1Pos,
                float c2Pos,
                float c3Pos,
                int nSubColorCount,
                int nDivTriangles,
                float smallEps,
                Vector2 innerBezierA0YZ,
                Vector2 innerBezierA1OffsetYZ,
                Vector2 innerBezierB0YZ,
                Vector2 innerBezierB1OffsetYZ,
                float innerAngleOffset,
                float innerAnglePerLeaf,
                int innerNRidges,
                int innerNVerticesPerRidge,
                int targetFaces)
            {
                this.nLeaves = nLeaves;
                this.CenterBezierA0 = centerBezierA0;
                this.CenterBezierA1Offset = centerBezierA1Offset;
                this.CenterBezierB0 = centerBezierB0;
                this.CenterBezierB1Offset = centerBezierB1Offset;
                this.RightBezierA0 = rightBezierA0;
                this.RightBezierA1Offset = rightBezierA1Offset;
                this.RightBezierB0 = rightBezierB0;
                this.RightBezierB1Offset = rightBezierB1Offset;
                this.outerNLeaves = outerNLeaves;
                this.outerCenterBezierA0 = outerCenterBezierA0;
                this.outerCenterBezierA1Offset = outerCenterBezierA1Offset;
                this.outerCenterBezierB0 = outerCenterBezierB0;
                this.outerCenterBezierB1Offset = outerCenterBezierB1Offset;
                this.outerRightBezierA0 = outerRightBezierA0;
                this.outerRightBezierA1Offset = outerRightBezierA1Offset;
                this.outerRightBezierB0 = outerRightBezierB0;
                this.outerRightBezierB1Offset = outerRightBezierB1Offset;
                this.outerNDivTriangles = outerNDivTriangles;
                this.outerColor = outerColor;
                this.C1Pos = c1Pos;
                this.C2Pos = c2Pos;
                this.C3Pos = c3Pos;
                this.nSubColorCount = nSubColorCount;
                this.nDivTriangles = nDivTriangles;
                this.smallEps = smallEps;
                this.InnerBezierA0YZ = innerBezierA0YZ;
                this.InnerBezierA1OffsetYZ = innerBezierA1OffsetYZ;
                this.InnerBezierB0YZ = innerBezierB0YZ;
                this.InnerBezierB1OffsetYZ = innerBezierB1OffsetYZ;
                this.InnerAngleOffset = innerAngleOffset;
                this.InnerAnglePerLeaf = innerAnglePerLeaf;
                this.InnerNRidges = innerNRidges;
                this.InnerNVerticesPerRidge = innerNVerticesPerRidge;
                this.targetFaces = targetFaces;
            }

            public static MeshProp From(SCChandelierFlowerProperty.NormalCoverMeshProperty p)
            {
                return new MeshProp(
                    p.meshProperty.nLeaves,
                    p.CenterBezierA0,
                    p.CenterBezierA1Offset,
                    p.CenterBezierB0,
                    p.CenterBezierB1Offset,
                    p.RightBezierA0,
                    p.RightBezierA1Offset,
                    p.RightBezierB0,
                    p.RightBezierB1Offset,
                    p.meshProperty.outerNLeaves,
                    p.outerCenterBezierA0,
                    p.outerCenterBezierA1Offset,
                    p.outerCenterBezierB0,
                    p.outerCenterBezierB1Offset,
                    p.outerRightBezierA0,
                    p.outerRightBezierA1Offset,
                    p.outerRightBezierB0,
                    p.outerRightBezierB1Offset,
                    p.meshProperty.outerNDivTriangles,
                    p.meshProperty.outerColor,
                    p.meshProperty.C1Pos,
                    p.meshProperty.C2Pos,
                    p.meshProperty.C3Pos,
                    p.meshProperty.nSubColorCount,
                    p.meshProperty.nDivTriangles,
                    p.meshProperty.smallEps,
                    p.InnerBezierA0YZ,
                    p.InnerBezierA1OffsetYZ,
                    p.InnerBezierB0YZ,
                    p.InnerBezierB1OffsetYZ,
                    p.meshProperty.InnerAngleOffset,
                    p.meshProperty.InnerAnglePerLeaf,
                    p.meshProperty.InnerNRidges,
                    p.meshProperty.InnerNVerticesPerRidge,
                    p.meshProperty.targetFaces);
            }

            public static MeshProp From(SCChandelierFlowerProperty.CoverWitheredMeshProperty p)
            {
                return new MeshProp(
                    p.meshProperty.nLeaves,
                    p.CenterBezierA0,
                    p.CenterBezierA1Offset,
                    p.CenterBezierB0,
                    p.CenterBezierB1Offset,
                    p.RightBezierA0,
                    p.RightBezierA1Offset,
                    p.RightBezierB0,
                    p.RightBezierB1Offset,
                    p.meshProperty.outerNLeaves,
                    p.outerCenterBezierA0,
                    p.outerCenterBezierA1Offset,
                    p.outerCenterBezierB0,
                    p.outerCenterBezierB1Offset,
                    p.outerRightBezierA0,
                    p.outerRightBezierA1Offset,
                    p.outerRightBezierB0,
                    p.outerRightBezierB1Offset,
                    p.meshProperty.outerNDivTriangles,
                    p.meshProperty.outerColor,
                    p.meshProperty.C1Pos,
                    p.meshProperty.C2Pos,
                    p.meshProperty.C3Pos,
                    p.meshProperty.nSubColorCount,
                    p.meshProperty.nDivTriangles,
                    p.meshProperty.smallEps,
                    p.InnerBezierA0YZ,
                    p.InnerBezierA1OffsetYZ,
                    p.InnerBezierB0YZ,
                    p.InnerBezierB1OffsetYZ,
                    p.meshProperty.InnerAngleOffset,
                    p.meshProperty.InnerAnglePerLeaf,
                    p.meshProperty.InnerNRidges,
                    p.meshProperty.InnerNVerticesPerRidge,
                    p.meshProperty.targetFaces);
            }
        }
    }

    internal class SelfRestoringParameter
    {
        public float value, lastValue;
        public float gravity, airFriction;
        public float lowGravityRange;
        public float vel;
        float target;
        public bool generallyStatic
        {
            get { return Mathf.Abs(this.value) < 0.5f && Mathf.Abs(this.vel) < 0.5f && Mathf.Abs(this.target) < 0.5f; }
        }
        public SelfRestoringParameter(float gravity = 0.9f, float airFriction = 0.97f, float lowGravityRange = 20f)
        {
            this.gravity = gravity;
            this.airFriction = airFriction;
            this.lowGravityRange = lowGravityRange;
            this.HardSetValue(0);
            this.vel = 0;
            this.target = 0;
        }
        public void HardSetValue(float v)
        {
            this.value = v;
            this.lastValue = v;
            this.vel = 0f;
            this.target = v;
        }
        public void AddTarget(float v)
        {
            this.target = Mathf.Clamp(this.target + v, -this.lowGravityRange, this.lowGravityRange);
        }
        public void AddImpulse(float v)
        {
            this.vel += v;
            float maxVel = this.lowGravityRange * 0.25f;
            this.vel = Mathf.Clamp(this.vel, -maxVel, maxVel);
        }
        public void Update()
        {
            this.lastValue = this.value;
            this.vel += (this.target - this.value) * this.gravity;
            this.vel *= this.airFriction;
            this.value += this.vel;
            this.target *= 0.94f;
            if (this.value > this.lowGravityRange)
            {
                this.value = this.lowGravityRange;
                this.vel *= 0.3f;
            }
            else if (this.value < -this.lowGravityRange)
            {
                this.value = -this.lowGravityRange;
                this.vel *= 0.3f;
            }
            if (this.generallyStatic)
            {
                this.value = this.vel = 0f;
                this.target = 0f;
            }
        }
    }
}
