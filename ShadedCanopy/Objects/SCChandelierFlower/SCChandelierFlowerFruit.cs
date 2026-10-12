using RWCustom;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace ShadedCanopy.Objects.SCChandelierFlower
{
    internal class SCChandelierFlowerFruit: PlayerCarryableItem, IDrawable, IPlayerEdible, IHaveAStalk
    {
        int bitesLeft;
        public SCChandelierFlower stalk;
        Vector2 rotation, lastRotation;
        bool containerUpdated;

        int pickTicking;

        public bool hasStalk { get => this.stalk != null; }
        public Vector2 Rotation => this.rotation;
        public Vector2 LastRotation => this.lastRotation;
        public bool grabbed
        {
            get { return this.grabbedBy.Count > 0; }
        }
        public SCChandelierFlowerFruit(AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject)
        {
            base.bodyChunks = new BodyChunk[1];
            base.bodyChunks[0] = new BodyChunk(this, 0, Vector2.zero, SCChandelierFlowerProperty.fruitPhysicalProperty.rad, SCChandelierFlowerProperty.fruitPhysicalProperty.mass_off_stalk);
            base.CollideWithSlopes = true;
            this.bodyChunks[0].collideWithSlopes = true;
            base.bodyChunkConnections = new BodyChunkConnection[0];
            base.airFriction = SCChandelierFlowerProperty.fruitPhysicalProperty.airFriction;
            base.gravity = SCChandelierFlowerProperty.fruitPhysicalProperty.gravity;
            base.bounce = SCChandelierFlowerProperty.fruitPhysicalProperty.bounce;
            base.surfaceFriction = SCChandelierFlowerProperty.fruitPhysicalProperty.surfaceFriction;
            base.collisionLayer = SCChandelierFlowerProperty.fruitPhysicalProperty.collisionLayer;
            base.waterFriction = SCChandelierFlowerProperty.fruitPhysicalProperty.waterFriction;
            base.buoyancy = SCChandelierFlowerProperty.fruitPhysicalProperty.buoyancy;
            this.bitesLeft = SCChandelierFlowerProperty.fruitObjectProperty.fruitBites;
            this.rotation = this.lastRotation = Vector2.zero;
            this.containerUpdated = false;
            this.stalk = null;
        }

        public SCChandelierFlowerFruit(AbstractPhysicalObject abstractPhysicalObject, SCChandelierFlower stalk): this(abstractPhysicalObject)
        {
            this.stalk = stalk;
            this.firstChunk.mass = SCChandelierFlowerProperty.fruitPhysicalProperty.mass_on_stalk;
        }
        public void BitByPlayer(Creature.Grasp grasp, bool eu)
        {
            this.bitesLeft--;
            this.room.PlaySound((this.bitesLeft == 0) ? SoundID.Slugcat_Eat_Dangle_Fruit : SoundID.Slugcat_Bite_Dangle_Fruit, base.firstChunk);
            base.firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
            if (this.bitesLeft == 0)
            {
                (grasp.grabber as Player).ObjectEaten(this);
                grasp.Release();
                if (this.stalk != null)
                {
                    this.DetatchStalk();
                }
                this.Destroy();
            }
        }
        public override void PlaceInRoom(Room placeRoom)
        {
            base.PlaceInRoom(placeRoom);
            base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(this.abstractPhysicalObject.pos));
            this.rotation = RWCustom.Custom.RNV();
            this.lastRotation = this.rotation;
        }
        public override void Update(bool eu)
        {
            base.Update(eu);
            if (this.room.game.devToolsActive && Input.GetKey("b"))
            {
                base.firstChunk.vel += RWCustom.Custom.DirVec(base.firstChunk.pos, Futile.mousePosition) * 3f;
            }
            this.lastRotation = this.rotation;
            if (this.stalk != null)
            {
                Vector2 fruitPos = this.firstChunk.pos;
                Vector2 nodePos = this.stalk.firstChunk.pos;
                Vector2 fruitDir = nodePos - fruitPos;
                Vector2 dir = fruitDir;
                if (this.stalk.nodeCount >= 2)
                {
                    BodyChunk bc = this.stalk.bodyChunks[this.stalk.nodeChunkIdx[this.stalk.nodeCount - 2]];
                    Vector2 upupPos = bc.pos;
                    Vector2 stalkDir = upupPos - nodePos;
                    if (this.stalk.nodeCount >= 3)
                    {
                        BodyChunk bc2 = this.stalk.bodyChunks[this.stalk.nodeChunkIdx[this.stalk.nodeCount - 3]];
                        Vector2 upupupPos = bc2.pos;
                        stalkDir += upupupPos - upupPos;
                    }

                    if (fruitDir.sqrMagnitude > 0.0001f && stalkDir.sqrMagnitude > 0.0001f)
                    {
                        Vector2 a = fruitDir.normalized;
                        Vector2 b = stalkDir.normalized;
                        if (Vector2.Dot(a, b) < 0f)
                        {
                            dir = stalkDir;
                        }
                        else
                        {
                            Vector2 mixed = a + b * 0.5f;
                            if (mixed.sqrMagnitude > 0.0001f) dir = mixed;
                        }
                    }
                }
                if (dir.sqrMagnitude > 0.0001f)
                {
                    this.rotation = dir.normalized;
                }
            } else if (this.grabbed)
            {
                this.rotation = RWCustom.Custom.PerpendicularVector(RWCustom.Custom.DirVec(base.firstChunk.pos, this.grabbedBy[0].grabber.mainBodyChunk.pos));
                this.rotation.y = Mathf.Abs(this.rotation.y);
            }
            if (base.firstChunk.ContactPoint.y < 0)
            {
                this.rotation = (this.rotation - RWCustom.Custom.PerpendicularVector(this.rotation)
                    * SCChandelierFlowerProperty.fruitPhysicalProperty.rollingRotationSpeedRatio
                    * base.firstChunk.vel.x).normalized;
                BodyChunk firstChunk = base.firstChunk;
                firstChunk.vel.x = firstChunk.vel.x * SCChandelierFlowerProperty.fruitPhysicalProperty.rollingFriction;
            }

            if (this.grabbed && this.stalk != null)
            {
                bool wantPick = false;
                foreach (var grasp in this.grabbedBy)
                {
                    if (grasp.grabber is Player player)
                    {
                        if (player.input[0].pckp && player.input[0].y < 0)
                        {
                            wantPick = true;
                            break;
                        }
                    }
                }
                if (wantPick)
                {
                    this.pickTicking++;
                }
                else
                {
                    this.pickTicking = 0;
                }
                if (this.pickTicking > SCChandelierFlowerProperty.fruitObjectProperty.fruitPickRequireTicks)
                {
                    this.DetatchStalk();
                }
            }
            if (this.grabbed && this.stalk != null)
            {
                foreach (var grasp in this.grabbedBy)
                {
                    if (grasp.grabber is Player player)
                    {
                        if (player.input[0].jmp && !player.input[1].jmp)
                        {
                            if (player.canJump == 0 && player.bodyMode == Player.BodyModeIndex.Default)
                            {
                                player.ReleaseGrasp(grasp.graspUsed);
                                this.stalk.PlayerReleaseGrasp(player);
                                break;
                            }
                        }
                    }
                }
            }
        }
        public void ThrowByPlayer()
        {
        }

        public void DetatchStalk()
        {
            if (this.stalk != null)
            {
                this.CutOffStalk();
                this.firstChunk.mass = SCChandelierFlowerProperty.fruitPhysicalProperty.mass_off_stalk;
            }
        }
        public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
        {
            // do nothing
        }
        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[2];
            sLeaser.sprites[0] = new FSprite($"{SCChandelierFlowerHooks.ChandelierFlowerFruitSpriteName}TCWN", true);
            sLeaser.sprites[1] = new FSprite($"{SCChandelierFlowerHooks.ChandelierFlowerFruitSpriteName}0", true);

            var prop = SCChandelierFlowerProperty.fruitObjectProperty;
            sLeaser.sprites[0].scale = prop.fruitSpriteScale;
            sLeaser.sprites[0].alpha = prop.fruitLensAlpha;
            sLeaser.sprites[1].scale = prop.fruitSpriteScale * prop.fruitRimScale;
            sLeaser.sprites[1].alpha = prop.fruitRimAlpha;
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["TexControledWaterNut"];
            this.AddToContainer(sLeaser, rCam);
        }
        private void CutOffStalk()
        {
            Assert.IsNotNull(this.stalk);
            // 告诉藤蔓要断了
            this.stalk.FruitDetatched();
            this.containerUpdated = true;
            this.stalk = null;
            (this.abstractPhysicalObject as AbstractChandelierFlowerFruit).spawnFromStalk = false;
        }
        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 pos = Vector2.Lerp(this.firstChunk.lastPos, this.firstChunk.pos, timeStacker);
            if (this.stalk != null && this.grabbed)
            {
                pos.y += SCChandelierFlowerProperty.fruitObjectProperty.fruitGrabVisualOffsetY;
            }
            Vector2 rot = Vector3.Slerp(this.lastRotation, this.rotation, timeStacker);
            if (this.containerUpdated)
            {
                this.AddToContainer(sLeaser, rCam);
                this.containerUpdated = false;
            }
            foreach (var sprite in sLeaser.sprites)
            {
                sprite.x = pos.x - camPos.x;
                sprite.y = pos.y - camPos.y;
                sprite.rotation = RWCustom.Custom.VecToDeg(rot);
            }
            if (base.slatedForDeletetion || this.room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            var prop = SCChandelierFlowerProperty.fruitObjectProperty;
            sLeaser.sprites[1].color = palette.blackColor.CloneWithNewAlpha(prop.fruitRimAlpha);
            this.color = prop.fruitColor;
            sLeaser.sprites[0].color = this.color;
        }

        public override void HitByWeapon(Weapon weapon)
        {
            base.HitByWeapon(weapon);
            if (this.stalk != null)
            {
                this.stalk.FruitHitten(weapon);
            }
        }
        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner = null)
        {
            if (newContatiner == null)
            {
                if (this.stalk == null)
                {
                    newContatiner = rCam.ReturnFContainer(SCChandelierFlowerProperty.fruitObjectProperty.fruitContainerAfterPick);
                }
                else
                {
                    newContatiner = this.stalk.containerDict[SCChandelierFlowerProperty.fruitObjectProperty.fruitContainerBeforePick];
                }
            }
            sLeaser.sprites[0].RemoveFromContainer();
            sLeaser.sprites[1].RemoveFromContainer();
            newContatiner.AddChild(sLeaser.sprites[0]);
            newContatiner.AddChild(sLeaser.sprites[1]);
        }


        public int BitesLeft
        {
            get { return bitesLeft; }
        }

        public int FoodPoints
        {
            get { return SCChandelierFlowerProperty.fruitObjectProperty.fruitFoodPoints; }
        }

        public bool Edible
        {
            get { return true; }
        }

        public bool AutomaticPickUp => throw new NotImplementedException();
        public class AbstractChandelierFlowerFruit : AbstractPhysicalObject
        {
            public AbstractChandelierFlowerFruit(World world, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID) : base(world, SCEnums.AbstractObjectTypeType.SCChandelierFlowerFruit, realizedObject, pos, ID)
            {
                this.spawnFromStalk = false;
            }
            public override void Realize()
            {
                if (this.realizedObject != null)
                    return;
                if (this.spawnFromStalk)
                    return;
                this.realizedObject = new SCChandelierFlowerFruit(this);
            }
            public bool spawnFromStalk;
        }
    }
}
