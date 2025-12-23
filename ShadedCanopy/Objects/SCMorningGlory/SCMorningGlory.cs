using RWCustom;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace ShadedCanopy.Objects.SCMorningGlory
{
    internal class SCMorningGlory : PhysicalObject, IDrawable
    {
        AbstractMorningGlory abstractMorningGlory
        {
            get => this.abstractPhysicalObject as AbstractMorningGlory;
        }
        // 0: on wall, -2: mainBodyChunk, -1: fruit
        Vector2 stuckPos, coverPos;
        float segmentLength;
        public int nodeCount;
        float oughtLength;
        public SCMorningGloryFruit hangingFruit { get; private set; }
        public Personalization personalization;
        public int[] nodeChunkIdx;  // up and down: chunk storage
        SCMorningGloryCover3D cover;
        SCMorningGloryLeaf[] leaves;
        
        public SCMorningGlory(AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject)
        {

            base.bodyChunks = new BodyChunk[0];
            //base.bodyChunks[0] = new BodyChunk(this, 0, Vector2.zero, SCMorningGloryProperty.CoverPhysicalProperty.rad, SCMorningGloryProperty.CoverPhysicalProperty.mass);
            base.bodyChunkConnections = new BodyChunkConnection[0];
            base.airFriction = SCMorningGloryProperty.StalkMainChunkPhysicalProperty.airFriction;
            base.gravity = SCMorningGloryProperty.StalkMainChunkPhysicalProperty.gravity;
            base.bounce = SCMorningGloryProperty.StalkMainChunkPhysicalProperty.bounce;
            base.surfaceFriction = SCMorningGloryProperty.StalkMainChunkPhysicalProperty.surfaceFriction;
            base.collisionLayer = SCMorningGloryProperty.StalkMainChunkPhysicalProperty.collisionLayer;
            base.waterFriction = SCMorningGloryProperty.StalkMainChunkPhysicalProperty.waterFriction;
            base.buoyancy = SCMorningGloryProperty.StalkMainChunkPhysicalProperty.buoyancy;
            
        }

        public override void PlaceInRoom(Room placeRoom)
        {
            Vector2 pos = placeRoom.MiddleOfTile(this.abstractPhysicalObject.pos);
            this.coverPos = pos;
            int x = placeRoom.GetTilePosition(pos).x;
            this.stuckPos.y = -1;
            for (int i = placeRoom.GetTilePosition(pos).y; i < placeRoom.TileHeight; i++)
            {
                if (placeRoom.GetTile(x, i).Solid)
                {
                    this.stuckPos.y = placeRoom.MiddleOfTile(x, i).y - 10f;
                    break;
                }
            }
            if (this.stuckPos.y < 0)
            {
                throw new Exception("SCMorningGlory placed in Invalid place (no roof)");
            }
            this.stuckPos.x = pos.x;

            this.oughtLength = (this.stuckPos - this.coverPos).magnitude;
            int segCnt = Mathf.CeilToInt((this.stuckPos.y - pos.y) / ModifiableSCMorningGloryProperty.scMorningGlory.stalkSegmentMaxLength);
            segCnt = Mathf.Max(segCnt, ModifiableSCMorningGloryProperty.scMorningGlory.stalkSegmentMinCount);
            //int segCnt = ModifiableSCMorningGloryProperty.scMorningGlory.stalkSegmentCount - 1;
            this.segmentLength = (this.stuckPos.y - pos.y) / segCnt;
            this.segmentLength *= ModifiableSCMorningGloryProperty.scMorningGlory.stalkSegmentShrink;
            this.nodeCount = segCnt + 1;
            Vector2[] nodePos = new Vector2[this.nodeCount];
            
            float[] nodeMass = new float[this.nodeCount];
            for (int i = 0; i < this.nodeCount; i++)
            {
                nodePos[i] = Vector2.Lerp(this.stuckPos, this.coverPos + Vector2.down * this.segmentLength, i * 1.0f / (this.nodeCount - 1));
                nodeMass[i] = ModifiableSCMorningGloryProperty.scMorningGlory.stalkNodeMass;
            }
            nodeMass[0] = float.MaxValue;
            nodeMass[this.nodeCount - 1] = SCMorningGloryProperty.StalkMainChunkPhysicalProperty.mass;
            // 个性化/叶子罩子
            this.personalization = new Personalization(this.abstractPhysicalObject.ID.RandomSeed, this.nodeCount);
            this.cover = new SCMorningGloryCover3D(
                this,
                this.personalization.coverPetals,
                ModifiableSCMorningGloryProperty.scMorningGlory.coverNodeCount,
                ModifiableSCMorningGloryProperty.scMorningGlory.coverHeight,
                ModifiableSCMorningGloryProperty.scMorningGlory.coverWidth
            );
            this.cover.spinOffset = this.personalization.coverSpinOffset;
            this.leaves = new SCMorningGloryLeaf[this.personalization.leafCount];
            for (int i = 0; i < this.personalization.leafCount; ++i)
            {
                int nodeIdx = this.personalization.leafPosition[i];
                bool isLeft = (nodeIdx > this.nodeCount - 2);
                nodeIdx %= (this.nodeCount - 2);
                nodeIdx += 1;
                this.leaves[i] = new SCMorningGloryLeaf(
                    this,
                    nodeIdx,
                    isLeft,
                    this.personalization.leafSize[i],
                    this.personalization.leafAngle[i]
                    );
            }

            if (!this.abstractMorningGlory.isConsumed)
            {
                AbstractPhysicalObject abo = this.abstractMorningGlory.abstractFruit;
                if (abo.realizedObject != null)
                {
                    abo.realizedObject.Destroy();
                    this.room.RemoveObject(abo.realizedObject);
                    abo.realizedObject = null;
                }
                abo.realizedObject = new SCMorningGloryFruit(abo, this);
                this.hangingFruit = abo.realizedObject as SCMorningGloryFruit;
            } else
            {
                this.cover.width *= ModifiableSCMorningGloryProperty.scMorningGlory.coverNoFruitWidthMultiplier;
                this.cover.height *= ModifiableSCMorningGloryProperty.scMorningGlory.coverNoFruitHeightMultiplier;
            }
            // 设置一串bodychunk
            this.nodeChunkIdx = new int[this.nodeCount];
            for (int i = 0; i < this.nodeCount; ++i)
            {
                this.nodeChunkIdx[i] = i;
            }
            int tmp = this.nodeChunkIdx[0];
            this.nodeChunkIdx[0] = this.nodeChunkIdx[this.nodeCount - 1];
            this.nodeChunkIdx[this.nodeCount - 1] = tmp;

            this.bodyChunks = new BodyChunk[this.nodeCount];

            for (int i = 0; i < this.nodeCount; ++i)
            {
                this.bodyChunks[this.nodeChunkIdx[i]] = new BodyChunk(this, i, nodePos[i], SCMorningGloryProperty.StalkMainChunkPhysicalProperty.rad, nodeMass[i]);
            }
            this.bodyChunkConnections = new BodyChunkConnection[this.nodeCount - 1];
            for (int i = 0; i < this.nodeCount - 1; ++i)
            {
                this.bodyChunkConnections[i] = new BodyChunkConnection(
                    this.bodyChunks[this.nodeChunkIdx[i]],
                    this.bodyChunks[this.nodeChunkIdx[i + 1]],
                    this.segmentLength,
                    BodyChunkConnection.Type.Pull,
                    ModifiableSCMorningGloryProperty.scMorningGlory.stalkFlexibility,
                    -1);
            }
            base.PlaceInRoom(placeRoom);
            //this.firstChunk.HardSetPosition(this.nodePos[this.nodeCount - 2]);
            if (this.hangingFruit != null)
            {
                Vector2 fruitPos = nodePos[this.nodeCount - 1] + Vector2.down * ModifiableSCMorningGloryProperty.scMorningGlory.stalkFruitDistance;
                this.hangingFruit.firstChunk.HardSetPosition(fruitPos);
                Array.Resize(ref this.bodyChunkConnections, this.nodeCount);
                this.bodyChunkConnections[this.nodeCount - 1] = new BodyChunkConnection(
                    this.bodyChunks[this.nodeChunkIdx[this.nodeCount - 1]],
                    this.hangingFruit.firstChunk,
                    ModifiableSCMorningGloryProperty.scMorningGlory.stalkFruitDistance,
                    BodyChunkConnection.Type.Pull,
                    ModifiableSCMorningGloryProperty.scMorningGlory.stalkFlexibility,
                    -1);
            }
        }
        public override void Update(bool eu)
        {
            base.Update(eu);
            this.bodyChunks[this.nodeChunkIdx[0]].HardSetPosition(this.stuckPos);
            this.bodyChunks[this.nodeChunkIdx[0]].vel = Vector2.zero;
            if (this.hangingFruit != null)
            {
                if (this.hangingFruit.room == null)
                {
                    this.hangingFruit.PlaceInRoom(this.room);
                }
                if (this.hangingFruit.grabbed)
                {
                    float length = (this.firstChunk.pos - this.stuckPos).magnitude;
                    float expandPct = Mathf.Max(0f, (length - this.oughtLength) / this.segmentLength);
                    foreach (var a in this.hangingFruit.grabbedBy)
                    {
                        if (a.grabbed is Creature creature)
                        {
                            float power = expandPct = ModifiableSCMorningGloryProperty.scMorningGlory.stalkExtraPlayerElastic;
                            float powerMassRatio = this.firstChunk.mass / (this.firstChunk.mass + creature.firstChunk.mass);
                            Vector2 aim = RWCustom.Custom.DirVec(creature.firstChunk.pos, this.stuckPos);
                            creature.firstChunk.vel += aim * power * powerMassRatio;
                        }
                    }
                }
            }
            this.cover.widthCurvePct = ModifiableSCMorningGloryProperty.scMorningGlory.coverWidthCurvePct;
            this.cover.heightCurvePct = ModifiableSCMorningGloryProperty.scMorningGlory.coverHeightCurvePct;
            this.cover.Update();
            foreach (SCMorningGloryLeaf leaf in this.leaves)
            {
                leaf.Update();
            }
        }
        internal void PlayerReleaseGrasp(Player player)
        {
            //if (!player.standing)
            {
                float length = (this.firstChunk.pos - this.stuckPos).magnitude;
                float expandPct = Mathf.Max(0f, (length - this.oughtLength) / this.oughtLength);
                //float jumpBoost = RWCustom.Custom.LerpMap(expandPct,
                //    0f, ModifiableSCMorningGloryProperty.scMorningGlory.stalkMaxJumpBoostLenghPct,
                //    0f, ModifiableSCMorningGloryProperty.scMorningGlory.stalkMaxJumpBoost);
                float jumpBoost = ModifiableSCMorningGloryProperty.scMorningGlory.stalkMaxJumpBoost;
                float jumpSpeed = Mathf.Lerp(1f, 1.15f, player.Adrenaline);
                player.jumpBoost = jumpBoost;
                player.bodyChunks[0].vel.y += jumpBoost * jumpSpeed;
                player.bodyChunks[1].vel.y += Mathf.Max(0f, jumpBoost - 1) * jumpSpeed;
            }
        }

        public void FruitDetatched()
        {
            this.abstractMorningGlory.Consume();
            this.hangingFruit = null;
            Array.Resize(ref this.bodyChunkConnections, this.nodeCount - 1);
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            int leafSpriteCount = 0;
            foreach (SCMorningGloryLeaf leaf in this.leaves)
            {
                leafSpriteCount += leaf.spriteCount;
            }
            sLeaser.sprites = new FSprite[2 + this.cover.spriteCount + leafSpriteCount];
            sLeaser.sprites[0] = TriangleMesh.MakeLongMesh(this.nodeCount - 1, false, true);
            // 线
            TriangleMesh.Triangle[] trig =
            {
                new TriangleMesh.Triangle(0, 1, 2),
                new TriangleMesh.Triangle(1, 2, 3),
                new TriangleMesh.Triangle(0, 4, 5),
                new TriangleMesh.Triangle(4, 5, 6)
            };
            sLeaser.sprites[1] = new TriangleMesh("Futile_White", trig, true);
            int spriteIdx = 2;
            this.cover.InitiateSprite(spriteIdx, sLeaser, rCam);
            spriteIdx += this.cover.spriteCount;
            foreach (SCMorningGloryLeaf leaf in this.leaves)
            {
                leaf.InitiateSprite(spriteIdx, sLeaser, rCam);
                spriteIdx += leaf.spriteCount;
            }
            this.AddToContainer(sLeaser, rCam);
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            TriangleMesh trigMesh = sLeaser.sprites[0] as TriangleMesh;
            for (int i = 1; i < this.nodeCount; ++i)
            {
                int vertOffset = (i - 1) * 4;
                Vector2 posUp = Vector2.Lerp(this.bodyChunks[this.nodeChunkIdx[i - 1]].lastPos, this.bodyChunks[this.nodeChunkIdx[i - 1]].pos, timeStacker);
                Vector2 posDown = Vector2.Lerp(this.bodyChunks[this.nodeChunkIdx[i]].lastPos, this.bodyChunks[this.nodeChunkIdx[i]].pos, timeStacker);
                Vector2 d = RWCustom.Custom.PerpendicularVector(posUp - posDown).normalized;
                trigMesh.MoveVertice(vertOffset, posUp - d * (ModifiableSCMorningGloryProperty.scMorningGlory.stalkWidth / 2 + personalization.stalkWidthOffset[i - 1].x) - camPos);
                trigMesh.MoveVertice(vertOffset + 1, posUp + d * (ModifiableSCMorningGloryProperty.scMorningGlory.stalkWidth / 2 + personalization.stalkWidthOffset[i - 1].y) - camPos);
                trigMesh.MoveVertice(vertOffset + 2, posDown - d * (ModifiableSCMorningGloryProperty.scMorningGlory.stalkWidth / 2 + personalization.stalkWidthOffset[i].x) - camPos);
                trigMesh.MoveVertice(vertOffset + 3, posDown + d * (ModifiableSCMorningGloryProperty.scMorningGlory.stalkWidth / 2 + personalization.stalkWidthOffset[i].y) - camPos);
            }
            if (this.hangingFruit == null)
            {
                sLeaser.sprites[1].isVisible = false;
            }
            else
            {
                //1  0  4
                //2 3  5 6
                sLeaser.sprites[1].isVisible = true;
                TriangleMesh trig = sLeaser.sprites[1] as TriangleMesh;
                Vector2 posUp = Vector2.Lerp(this.bodyChunks[this.nodeChunkIdx[this.nodeCount - 1]].lastPos, this.bodyChunks[this.nodeChunkIdx[this.nodeCount - 1]].pos, timeStacker);
                Vector2 posDown = Vector2.Lerp(this.hangingFruit.firstChunk.lastPos, this.hangingFruit.firstChunk.pos, timeStacker);
                Vector2 l = (posUp - posDown).normalized;
                Vector2 d = RWCustom.Custom.PerpendicularVector(l).normalized;
                trig.MoveVertice(0, posUp - camPos);
                trig.MoveVertice(1, posUp - d * (ModifiableSCMorningGloryProperty.scMorningGlory.stalkWidth / 2 + personalization.stalkWidthOffset[this.nodeCount - 1].x) - camPos);
                trig.MoveVertice(4, posUp + d * (ModifiableSCMorningGloryProperty.scMorningGlory.stalkWidth / 2 + personalization.stalkWidthOffset[this.nodeCount - 1].y) - camPos);
                Vector2 left = RWCustom.Custom.RotateAroundOrigo(l * ModifiableSCMorningGloryProperty.scMorningGlory.fruitConnectDistance, -ModifiableSCMorningGloryProperty.scMorningGlory.fruitConnectAngle);
                Vector2 right = RWCustom.Custom.RotateAroundOrigo(l * ModifiableSCMorningGloryProperty.scMorningGlory.fruitConnectDistance, ModifiableSCMorningGloryProperty.scMorningGlory.fruitConnectAngle);
                Vector2 left_d = RWCustom.Custom.PerpendicularVector(posUp - (posDown + left)).normalized;
                Vector2 right_d = RWCustom.Custom.PerpendicularVector(posUp - (posDown + right)).normalized;
                trig.MoveVertice(2, posDown + left + left_d * ModifiableSCMorningGloryProperty.scMorningGlory.fruitConnectTipWidth / 2 - camPos);
                trig.MoveVertice(3, posDown + left - left_d * ModifiableSCMorningGloryProperty.scMorningGlory.fruitConnectTipWidth / 2 - camPos);
                trig.MoveVertice(5, posDown + right + right_d * ModifiableSCMorningGloryProperty.scMorningGlory.fruitConnectTipWidth / 2 - camPos);
                trig.MoveVertice(6, posDown + right - right_d * ModifiableSCMorningGloryProperty.scMorningGlory.fruitConnectTipWidth / 2 - camPos);
            }
            this.cover.DrawSprites(sLeaser, rCam, timeStacker, camPos);

            foreach (SCMorningGloryLeaf leaf in this.leaves)
                leaf.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }
        public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
        {
            // do nothing
        }
        public override void HitByWeapon(Weapon weapon)
        {
            base.HitByWeapon(weapon);
            this.cover.SpinDisturb(weapon.firstChunk.vel.magnitude * weapon.firstChunk.mass);
            foreach (SCMorningGloryLeaf leaf in this.leaves)
            {
                leaf.Disturb(weapon.firstChunk.vel.magnitude * weapon.firstChunk.mass);
            }
        }
        internal void FruitHitten(Weapon weapon)
        {
            this.cover.SpinDisturb(weapon.firstChunk.vel.magnitude * weapon.firstChunk.mass * 2f);
            this.cover.FlipDisturb(weapon.firstChunk.vel.magnitude * weapon.firstChunk.mass * 2f);
        }
        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            sLeaser.sprites[0].color = this.personalization.color;
            sLeaser.sprites[1].color = palette.blackColor;
            this.cover.ApplyPalette(sLeaser, rCam, palette);
            foreach (SCMorningGloryLeaf leaf in this.leaves)
                leaf.ApplyPalette(sLeaser, rCam, palette);
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner=null)
        {
            if (newContatiner == null)
            {
                newContatiner = rCam.ReturnFContainer(SCMorningGloryProperty.stalkContainer);
            }
            FContainer linkContainer = rCam.ReturnFContainer(SCMorningGloryProperty.fruitLinkerContainer);
            foreach (var fsp in sLeaser.sprites)
            {
                fsp.RemoveFromContainer();
            }

            newContatiner.AddChild(sLeaser.sprites[0]);
            linkContainer.AddChild(sLeaser.sprites[1]);
            this.cover.AddToContainer(sLeaser, rCam);
            foreach (SCMorningGloryLeaf leaf in this.leaves)
                leaf.AddToContainer(sLeaser, rCam);
        }


        internal class Personalization
        {
            public Color color;
            public int coverPetals;

            public float2[] stalkWidthOffset;
            public int leafCount;
            public int[] leafPosition;
            public float[] leafAngle;
            public Vector2[] leafSize;
            public float coverSpinOffset;
            public Personalization(int randomSeed, int stalkNodeCount)
            {
                UnityEngine.Random.State state = UnityEngine.Random.state;
                UnityEngine.Random.InitState(randomSeed);
                this.color = Color.Lerp(
                    ModifiableSCMorningGloryProperty.scMorningGlory.stalkColorRangeLeft,
                    ModifiableSCMorningGloryProperty.scMorningGlory.stalkColorRangeRight,
                    UnityEngine.Random.Range(0f, 1f));
                //this.coverAngle = UnityEngine.Random.Range(SCMorningGloryProperty.coverAngleMin, SCMorningGloryProperty.coverAngleMax);
                stalkWidthOffset = new float2[stalkNodeCount];
                this.coverSpinOffset = UnityEngine.Random.Range(0f, 360f);
                this.coverPetals = UnityEngine.Random.Range(
                    ModifiableSCMorningGloryProperty.scMorningGlory.coverPetalsMin,
                    ModifiableSCMorningGloryProperty.scMorningGlory.coverPetalsMax + 1);
                for (int i = 0; i < stalkNodeCount; i++)
                {
                    stalkWidthOffset[i] = new float2(
                        UnityEngine.Random.Range(ModifiableSCMorningGloryProperty.scMorningGlory.stalkWidthOffsetMin, ModifiableSCMorningGloryProperty.scMorningGlory.stalkWidthOffsetMax),
                        UnityEngine.Random.Range(ModifiableSCMorningGloryProperty.scMorningGlory.stalkWidthOffsetMin, ModifiableSCMorningGloryProperty.scMorningGlory.stalkWidthOffsetMax)
                    );
                }
                int leafAvailPos = (stalkNodeCount - 2) * 2;
                List<int> leafPos = new List<int>();
                for (int i = 0; i < leafAvailPos * 2; i++)
                {
                    if (UnityEngine.Random.value < SCMorningGloryProperty.leafProbability)
                    {
                        leafPos.Add(i);
                    }
                }
                this.leafCount = leafPos.Count;
                this.leafPosition = leafPos.ToArray();
                this.leafAngle = new float[this.leafCount];
                this.leafSize = new Vector2[this.leafCount];
                for (int i = 0; i < this.leafCount; i++)
                {
                    this.leafSize[i] = new Vector2(
                        UnityEngine.Random.Range(ModifiableSCMorningGloryProperty.scMorningGlory.leafWidthMin, ModifiableSCMorningGloryProperty.scMorningGlory.leafWidthMax),
                        UnityEngine.Random.Range(ModifiableSCMorningGloryProperty.scMorningGlory.leafLengthMin, ModifiableSCMorningGloryProperty.scMorningGlory.leafLengthMax)
                    );
                    this.leafAngle[i] = UnityEngine.Random.Range(ModifiableSCMorningGloryProperty.scMorningGlory.leafWidthMax, ModifiableSCMorningGloryProperty.scMorningGlory.leafAngleMax);
                }
                UnityEngine.Random.state = state;
            }
        }

        public class AbstractMorningGlory : AbstractConsumable
        {
            public AbstractMorningGlory(World world, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, int originRoom, int placedObjectIndex, PlacedObject.ConsumableObjectData consumableData) : base(world, SCEnums.AbstractObjectTypeType.SCMorningGlory, realizedObject, pos, ID, originRoom, placedObjectIndex, consumableData)
            {
            }
            public override void Consume()
            {
                base.Consume();
                abstractFruit = null;
            }
            public void SetUnconsumed(Room room)
            {
                this.isConsumed = false;
                this.abstractFruit = new SCMorningGloryFruit.AbstractMorningGloryFruit(
                    room.world,
                    null,
                    this.pos,
                    room.game.GetNewID()
                );
                this.abstractFruit.spawnFromStalk = true;
            }
            public override void Realize()
            {
                if (this.realizedObject != null)
                    return;
                this.realizedObject = new SCMorningGlory(this);
            }
            public SCMorningGloryFruit.AbstractMorningGloryFruit abstractFruit;   
        }
    }
}
