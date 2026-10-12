using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace ShadedCanopy.Objects.SCChandelierFlower
{
    internal class SCChandelierFlower : PhysicalObject, IDrawable
    {
        public AbstractChandelierFlower abstractChandelierFlower
        {
            get => this.abstractPhysicalObject as AbstractChandelierFlower;
        }
        // 0: on wall, -2: mainBodyChunk, -1: fruit
        Vector2 stuckPos;
        float segmentLength;
        public int nodeCount;
        float oughtLength;
        public SCChandelierFlowerFruit hangingFruit { get; private set; }
        public Personalization personalization;
        public int[] nodeChunkIdx;  // up and down: chunk storage
        public SCChandelierFlowerCover cover;
        public Dictionary<string, FContainer> containerDict;
        
        public SCChandelierFlower(AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject)
        {
            base.bodyChunks = new BodyChunk[0];
            base.bodyChunkConnections = new BodyChunkConnection[0];
            base.airFriction = SCChandelierFlowerProperty.torusPhysicalProperty.airFriction;
            base.gravity = SCChandelierFlowerProperty.torusPhysicalProperty.gravity;
            base.bounce = SCChandelierFlowerProperty.torusPhysicalProperty.bounce;
            base.surfaceFriction = SCChandelierFlowerProperty.torusPhysicalProperty.surfaceFriction;
            base.collisionLayer = SCChandelierFlowerProperty.torusPhysicalProperty.collisionLayer;
            base.waterFriction = SCChandelierFlowerProperty.torusPhysicalProperty.waterFriction;
            base.buoyancy = SCChandelierFlowerProperty.torusPhysicalProperty.buoyancy;
        }

        public override void PlaceInRoom(Room placeRoom)
        {
            Vector2 pos = placeRoom.MiddleOfTile(this.abstractPhysicalObject.pos);
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
                throw new Exception("SCChandelierFlower placed in Invalid place (no roof)");
            }
            this.stuckPos.x = pos.x;

            this.oughtLength = (this.stuckPos - pos).magnitude;
            int segCnt = Mathf.CeilToInt((this.stuckPos.y - pos.y) / SCChandelierFlowerProperty.stalkObjectProperty.stalkSegmentMaxLength);
            segCnt = Mathf.Max(segCnt, SCChandelierFlowerProperty.stalkObjectProperty.stalkSegmentMinCount);
            this.segmentLength = (this.stuckPos.y - pos.y) / segCnt;
            this.segmentLength *= SCChandelierFlowerProperty.stalkObjectProperty.stalkSegmentShrink;
            this.nodeCount = segCnt + 1;
            Vector2[] nodePos = new Vector2[this.nodeCount];
            
            float[] nodeMass = new float[this.nodeCount];
            for (int i = 0; i < this.nodeCount; i++)
            {
                nodePos[i] = Vector2.Lerp(this.stuckPos, pos + Vector2.down * this.segmentLength, i * 1.0f / (this.nodeCount - 1));
                nodeMass[i] = SCChandelierFlowerProperty.stalkObjectProperty.stalkNodeMass;
            }
            nodeMass[0] = float.MaxValue;
            nodeMass[this.nodeCount - 1] = SCChandelierFlowerProperty.torusPhysicalProperty.mass;
            
            if (!this.abstractChandelierFlower.isConsumed)
            {
                AbstractPhysicalObject abo = this.abstractChandelierFlower.abstractFruit;
                if (abo.realizedObject != null)
                {
                    abo.realizedObject.Destroy();
                    this.room.RemoveObject(abo.realizedObject);
                    abo.realizedObject = null;
                }
                abo.realizedObject = new SCChandelierFlowerFruit(abo, this);
                this.hangingFruit = abo.realizedObject as SCChandelierFlowerFruit;
            }
            else
            {
                this.segmentLength *= SCChandelierFlowerProperty.stalkObjectProperty.stalkNoFruitShirink;
            }
            this.personalization = new Personalization(this.abstractPhysicalObject.ID.RandomSeed, this.nodeCount, this.abstractChandelierFlower.isConsumed);
            this.cover = new SCChandelierFlowerCover(this);
            
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
                this.bodyChunks[this.nodeChunkIdx[i]] = new BodyChunk(this, i, nodePos[i], SCChandelierFlowerProperty.torusPhysicalProperty.rad, nodeMass[i]);
                if (i != 0)
                {
                    this.bodyChunks[this.nodeChunkIdx[i]].collideWithObjects = false;
                }
            }
            this.bodyChunkConnections = new BodyChunkConnection[this.nodeCount - 1];
            for (int i = 0; i < this.nodeCount - 1; ++i)
            {
                this.bodyChunkConnections[i] = new BodyChunkConnection(
                    this.bodyChunks[this.nodeChunkIdx[i]],
                    this.bodyChunks[this.nodeChunkIdx[i + 1]],
                    this.segmentLength,
                    BodyChunkConnection.Type.Pull,
                    SCChandelierFlowerProperty.stalkObjectProperty.stalkFlexibility,
                    -1);
            }
            base.PlaceInRoom(placeRoom);
            if (this.hangingFruit != null)
            {
                Vector2 fruitPos = nodePos[this.nodeCount - 1] + Vector2.down * (SCChandelierFlowerProperty.stalkObjectProperty.stalkFruitDistance + 5f);
                this.hangingFruit.firstChunk.HardSetPosition(fruitPos);
                Array.Resize(ref this.bodyChunkConnections, this.nodeCount);
                this.bodyChunkConnections[this.nodeCount - 1] = new BodyChunkConnection(
                    this.bodyChunks[this.nodeChunkIdx[this.nodeCount - 1]],
                    this.hangingFruit.firstChunk,
                    SCChandelierFlowerProperty.stalkObjectProperty.stalkFruitDistance,
                    BodyChunkConnection.Type.Normal,
                    SCChandelierFlowerProperty.stalkObjectProperty.stalkFruitFlexibility,
                    -1);
            }
        }
        public override void Update(bool eu)
        {
            base.Update(eu);
            this.bodyChunks[this.nodeChunkIdx[0]].HardSetPosition(this.stuckPos);
            this.bodyChunks[this.nodeChunkIdx[0]].vel = Vector2.zero;
            if (this.nodeCount >= 2)
            {
                BodyChunk last = this.firstChunk;
                BodyChunk prev = this.bodyChunks[this.nodeChunkIdx[this.nodeCount - 2]];
                float minSep = Mathf.Max(SCChandelierFlowerProperty.stalkObjectProperty.stalkAntiInvertMinSeparation, this.segmentLength * 0.15f);
                float targetY = prev.pos.y - minSep;
                if (last.pos.y > targetY)
                {
                    last.pos.y = targetY;
                    last.vel.y = Mathf.Min(last.vel.y, prev.vel.y);
                }
                if (this.hangingFruit != null)
                {
                    float fruitTargetY = last.pos.y - SCChandelierFlowerProperty.stalkObjectProperty.fruitAntiInvertMinSeparation;
                    if (this.hangingFruit.firstChunk.pos.y > fruitTargetY)
                    {
                        this.hangingFruit.firstChunk.pos.y = fruitTargetY;
                        this.hangingFruit.firstChunk.vel.y = Mathf.Min(this.hangingFruit.firstChunk.vel.y, last.vel.y);
                    }
                }
            }
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
                            float power = SCChandelierFlowerProperty.stalkObjectProperty.stalkExtraPlayerElastic;
                            float powerMassRatio = this.firstChunk.mass / (this.firstChunk.mass + creature.firstChunk.mass);
                            Vector2 aim = RWCustom.Custom.DirVec(creature.firstChunk.pos, this.stuckPos);
                            creature.firstChunk.vel += aim * power * powerMassRatio;
                        }
                    }
                }
            }
            this.cover?.Update();
        }
        internal void PlayerReleaseGrasp(Player player)
        {
            float length = (this.firstChunk.pos - this.stuckPos).magnitude;
            float expandPct = Mathf.Max(0f, (length - this.oughtLength) / this.oughtLength);
            float jumpBoost = SCChandelierFlowerProperty.stalkObjectProperty.stalkMaxJumpBoost;
            float jumpSpeed = Mathf.Lerp(1f, 1.15f, player.Adrenaline);
            player.jumpBoost = jumpBoost;
            player.bodyChunks[0].vel.y += jumpBoost * jumpSpeed;
            player.bodyChunks[1].vel.y += Mathf.Max(0f, jumpBoost - 1) * jumpSpeed;
        }

        public void FruitDetatched()
        {
            this.abstractChandelierFlower.Consume();
            this.hangingFruit = null;
            Array.Resize(ref this.bodyChunkConnections, this.nodeCount - 1);
        }
        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            this.containerDict = new Dictionary<string, FContainer>()
            {
                { "Background0", new FContainer() },
                { "Background1", new FContainer() },
                { "Background2", new FContainer() },
                { "Midground0", new FContainer() },
                { "Midground1", new FContainer() },
                { "Midground2", new FContainer() },
                { "Foreground0", new FContainer() },
                { "Foreground1", new FContainer() },
                { "Foreground2", new FContainer() }
            };
            sLeaser.containers = this.containerDict.Values.ToArray();

            int coverSpriteCount = this.cover != null ? this.cover.spriteCount : 0;
            sLeaser.sprites = new FSprite[2 + coverSpriteCount];
            TriangleMesh mesh = TriangleMesh.MakeLongMesh(this.nodeCount - 1, false, true);
            if (!RWCustom.Custom.rainWorld.Shaders.TryGetValue("SCChandelierFlower", out var sh))
            {
                RWCustom.Custom.rainWorld.Shaders.TryGetValue("SCMorningGlory", out sh);
            }
            if (sh != null)
            {
                mesh.shader = sh;
            }
            for (int i = 0; i < this.nodeCount - 1; ++i)
            {
                for (int j = 0; j < 2; ++j)
                {
                    mesh.UVvertices[i * 4 + j * 2] = new Vector2(0f, 0.5f);
                    mesh.UVvertices[i * 4 + j * 2 + 1] = new Vector2(1f, 0.5f);
                }
            }
            sLeaser.sprites[0] = mesh;
            
            TriangleMesh.Triangle[] trig =
            {
                new TriangleMesh.Triangle(0, 1, 2),
                new TriangleMesh.Triangle(1, 2, 3),
                new TriangleMesh.Triangle(0, 4, 5),
                new TriangleMesh.Triangle(4, 5, 6)
            };
            TriangleMesh linkMesh = new TriangleMesh("Futile_White", trig, true);
            if (sh != null)
            {
                linkMesh.shader = sh;
            }
            sLeaser.sprites[1] = linkMesh;
            
            this.cover?.InitiateSprite(sLeaser, rCam);
            this.AddToContainer(sLeaser, rCam);
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            TriangleMesh trigMesh = sLeaser.sprites[0] as TriangleMesh;
            float stalkWidth = 4f; // Hardcode or add to properties later
            for (int i = 1; i < this.nodeCount; ++i)
            {
                int vertOffset = (i - 1) * 4;
                Vector2 posUp = Vector2.Lerp(this.bodyChunks[this.nodeChunkIdx[i - 1]].lastPos, this.bodyChunks[this.nodeChunkIdx[i - 1]].pos, timeStacker);
                Vector2 posDown = Vector2.Lerp(this.bodyChunks[this.nodeChunkIdx[i]].lastPos, this.bodyChunks[this.nodeChunkIdx[i]].pos, timeStacker);
                Vector2 d = RWCustom.Custom.PerpendicularVector(posUp - posDown).normalized;
                trigMesh.MoveVertice(vertOffset, posUp - d * (stalkWidth / 2 + personalization.stalkWidthOffset[i - 1].x) - camPos);
                trigMesh.MoveVertice(vertOffset + 1, posUp + d * (stalkWidth / 2 + personalization.stalkWidthOffset[i - 1].y) - camPos);
                trigMesh.MoveVertice(vertOffset + 2, posDown - d * (stalkWidth / 2 + personalization.stalkWidthOffset[i].x) - camPos);
                trigMesh.MoveVertice(vertOffset + 3, posDown + d * (stalkWidth / 2 + personalization.stalkWidthOffset[i].y) - camPos);
            }

            if (this.hangingFruit == null)
            {
                sLeaser.sprites[1].isVisible = false;
            }
            else
            {
                sLeaser.sprites[1].isVisible = true;
                TriangleMesh trig = sLeaser.sprites[1] as TriangleMesh;
                Vector2 posUp = Vector2.Lerp(this.bodyChunks[this.nodeChunkIdx[this.nodeCount - 1]].lastPos, this.bodyChunks[this.nodeChunkIdx[this.nodeCount - 1]].pos, timeStacker);
                Vector2 posDown = Vector2.Lerp(this.hangingFruit.firstChunk.lastPos, this.hangingFruit.firstChunk.pos, timeStacker);
                Vector2 l = (posUp - posDown).normalized;
                Vector2 d = RWCustom.Custom.PerpendicularVector(l).normalized;
                float fruitConnectDistance = 10f; // placeholder
                float fruitConnectAngle = 0.2f; // placeholder
                float fruitConnectTipWidth = 2f; // placeholder
                
                trig.MoveVertice(0, posUp - camPos);
                trig.MoveVertice(1, posUp - d * (stalkWidth / 2 + personalization.stalkWidthOffset[this.nodeCount - 1].x) - camPos);
                trig.MoveVertice(4, posUp + d * (stalkWidth / 2 + personalization.stalkWidthOffset[this.nodeCount - 1].y) - camPos);
                Vector2 left = RWCustom.Custom.RotateAroundOrigo(l * fruitConnectDistance, -fruitConnectAngle);
                Vector2 right = RWCustom.Custom.RotateAroundOrigo(l * fruitConnectDistance, fruitConnectAngle);
                Vector2 left_d = RWCustom.Custom.PerpendicularVector(posUp - (posDown + left)).normalized;
                Vector2 right_d = RWCustom.Custom.PerpendicularVector(posUp - (posDown + right)).normalized;
                trig.MoveVertice(2, posDown + left + left_d * fruitConnectTipWidth / 2 - camPos);
                trig.MoveVertice(3, posDown + left - left_d * fruitConnectTipWidth / 2 - camPos);
                trig.MoveVertice(5, posDown + right + right_d * fruitConnectTipWidth / 2 - camPos);
                trig.MoveVertice(6, posDown + right - right_d * fruitConnectTipWidth / 2 - camPos);
            }

            this.cover?.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }
        public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
        {
            // do nothing
        }
        public override void HitByWeapon(Weapon weapon)
        {
            base.HitByWeapon(weapon);
            this.cover?.SpinDisturb(weapon.firstChunk.vel.magnitude * weapon.firstChunk.mass);
        }
        internal void FruitHitten(Weapon weapon)
        {
            this.cover?.SpinDisturb(weapon.firstChunk.vel.magnitude * weapon.firstChunk.mass * 2f);
            this.cover?.FlipDisturb(weapon.firstChunk.vel.magnitude * weapon.firstChunk.mass * 2f);
        }
        public static float StalkDarknessAt(float stalkPos)
        {
            if (stalkPos > 0.2)
                return 0;
            return Mathf.InverseLerp(0.2f, 0, stalkPos);
        }
        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            Color baseColor = this.personalization.color;
            TriangleMesh tri = sLeaser.sprites[0] as TriangleMesh;
            for (int i = 1; i < this.nodeCount; ++i)
            {
                float prevPos = (i - 1) / (this.nodeCount - 1f);
                float nextPos = i / (this.nodeCount - 1f);
                int vertOffset = (i - 1) * 4;
                float prevDarkness = StalkDarknessAt(prevPos);
                float nextDarkness = StalkDarknessAt(nextPos);
                tri.verticeColors[vertOffset] = Color.Lerp(baseColor, Color.black, prevDarkness);
                tri.verticeColors[vertOffset + 1] = Color.Lerp(baseColor, Color.black, prevDarkness);
                tri.verticeColors[vertOffset + 2] = Color.Lerp(baseColor, Color.black, nextDarkness);
                tri.verticeColors[vertOffset + 3] = Color.Lerp(baseColor, Color.black, nextDarkness);
            }
            sLeaser.sprites[1].color = palette.blackColor;
            this.cover?.ApplyPalette(sLeaser, rCam, palette);
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner=null)
        {
            if (newContatiner == null)
            {
                newContatiner = rCam.ReturnFContainer(SCChandelierFlowerProperty.stalkObjectProperty.stalkContainer);
            }
            FContainer linkContainer = rCam.ReturnFContainer("Midground");
            foreach (var fsp in sLeaser.sprites)
            {
                fsp.RemoveFromContainer();
            }
            newContatiner.AddChild(sLeaser.sprites[0]);
            linkContainer.AddChild(sLeaser.sprites[1]);
            
            if (this.containerDict != null)
            {
                foreach (var kvp in this.containerDict)
                {
                    kvp.Value.RemoveFromContainer();
                    string layerName = kvp.Key.Substring(0, kvp.Key.Length - 1);
                    rCam.ReturnFContainer(layerName).AddChild(kvp.Value);
                }
            }
        }


        internal class Personalization
        {
            public Color color;

            public float2[] stalkWidthOffset;
            public float coverSpinOffset;
            public float outerSpinOffset;
            public Personalization(int randomSeed, int stalkNodeCount, bool isConsumed)
            {
                UnityEngine.Random.State state = UnityEngine.Random.state;
                UnityEngine.Random.InitState(randomSeed);
                
                this.color = SCUtils.UtilTools.ColorRandomLerp(
                    SCChandelierFlowerProperty.outfitProperty.stalkColorA,
                    SCChandelierFlowerProperty.outfitProperty.stalkColorB);
                stalkWidthOffset = new float2[stalkNodeCount];
                this.coverSpinOffset = UnityEngine.Random.Range(0f, 360f);
                this.outerSpinOffset = UnityEngine.Random.Range(0f, 360f);
                
                for (int i = 0; i < stalkNodeCount; i++)
                {
                    stalkWidthOffset[i] = new float2(0, 0); // Temporary setup
                }
                
                if (isConsumed)
                {
                    color = Color.Lerp(color, SCChandelierFlowerProperty.outfitProperty.stalkNoFruitColor, 0.5f);
                }
                UnityEngine.Random.state = state;
            }
        }

        public class AbstractChandelierFlower : AbstractConsumable
        {
            public AbstractChandelierFlower(World world, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, int originRoom, int placedObjectIndex, PlacedObject.ConsumableObjectData consumableData) : base(world, SCEnums.AbstractObjectTypeType.SCChandelierFlower, realizedObject, pos, ID, originRoom, placedObjectIndex, consumableData)
            {
            }
            public override void Consume()
            {
                if (!string.IsNullOrEmpty(this.placedObjectOrigin))
                {
                    base.Consume();
                }
                else
                {
                    this.isConsumed = true;
                }
                abstractFruit = null;
            }
            public void SetUnconsumed(Room room)
            {
                this.isConsumed = false;
                this.abstractFruit = new SCChandelierFlowerFruit.AbstractChandelierFlowerFruit(
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
                this.realizedObject = new SCChandelierFlower(this);
            }
            public SCChandelierFlowerFruit.AbstractChandelierFlowerFruit abstractFruit;   
        }
    }
}
