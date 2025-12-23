using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace ShadedCanopy.Objects.SCMorningGlory
{
    internal class SCMorningGloryFruit: PlayerCarryableItem, IDrawable, IPlayerEdible, IHaveAStalk
    {
        int bitesLeft;
        public SCMorningGlory stalk;
        Vector2 rotation, lastRotation;
        bool containerUpdated;
        LightSource light;
        float lightRad, lightTargetRad;
        float lightAlpha, lightTargetAlpha;

        int pickTicking;

        public bool hasStalk { get => this.stalk != null; }
        public bool grabbed
        {
            get { return this.grabbedBy.Count > 0; }
        }
        public SCMorningGloryFruit(AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject)
        {
            base.bodyChunks = new BodyChunk[1];
            base.bodyChunks[0] = new BodyChunk(this, 0, Vector2.zero, SCMorningGloryProperty.FruitPhysicalProperty.rad, SCMorningGloryProperty.FruitPhysicalProperty.mass);
            base.CollideWithSlopes = true;
            this.bodyChunks[0].collideWithSlopes = true;
            base.bodyChunkConnections = new BodyChunkConnection[0];
            base.airFriction = SCMorningGloryProperty.FruitPhysicalProperty.airFriction;
            base.gravity = SCMorningGloryProperty.FruitPhysicalProperty.gravity;
            base.bounce = SCMorningGloryProperty.FruitPhysicalProperty.bounce;
            base.surfaceFriction = SCMorningGloryProperty.FruitPhysicalProperty.surfaceFriction;
            base.collisionLayer = SCMorningGloryProperty.FruitPhysicalProperty.collisionLayer;
            base.waterFriction = SCMorningGloryProperty.FruitPhysicalProperty.waterFriction;
            base.buoyancy = SCMorningGloryProperty.FruitPhysicalProperty.buoyancy;
            this.bitesLeft = SCMorningGloryProperty.fruitBites;
            this.rotation = this.lastRotation = Vector2.zero;
            this.containerUpdated = false;
            this.stalk = null;
            this.light = null;
        }

        public SCMorningGloryFruit(AbstractPhysicalObject abstractPhysicalObject, SCMorningGlory stalk): this(abstractPhysicalObject)
        {
            this.stalk = stalk;
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
                this.light.Destroy();
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
        void LightUpdate()
        {
            this.lightRad += Mathf.Sign(this.lightTargetRad - this.lightRad) * Mathf.Min(SCMorningGloryProperty.fruitLightRadMaxSlope, Mathf.Abs(this.lightTargetRad - this.lightRad));
            this.lightAlpha += Mathf.Sign(this.lightTargetAlpha - this.lightAlpha) * Mathf.Min(SCMorningGloryProperty.fruitLightAlphaMaxSlope, Mathf.Abs(this.lightTargetAlpha - this.lightAlpha));

            if (this.light != null)
            {
                this.light.setPos = this.firstChunk.pos + this.firstChunk.vel * 0.5f;
                this.light.color = this.color;
                this.light.alpha = this.lightAlpha;
                this.light.rad = this.lightRad;
                if (this.grabbedBy.Count > 0)
                {
                    this.lightTargetAlpha = SCMorningGloryProperty.fruitLightAlphaGrabbed;
                    this.lightTargetRad = SCMorningGloryProperty.fruitLightRadGrabbed;
                }
                else
                {
                    this.lightTargetAlpha = SCMorningGloryProperty.fruitLightAlpha;
                    this.lightTargetRad = SCMorningGloryProperty.fruitLightRad;
                }

                if (this.light.slatedForDeletetion || this.light.room != this.room)
                {

                    this.light = null;
                }
            }
            else
            {
                this.light = new LightSource(this.firstChunk.pos, false, this.color, this);
                this.room.AddObject(this.light);
            }
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
                this.rotation = RWCustom.Custom.DirVec(this.firstChunk.pos, this.stalk.firstChunk.pos);
            } else if (this.grabbed)
            {
                this.rotation = RWCustom.Custom.PerpendicularVector(RWCustom.Custom.DirVec(base.firstChunk.pos, this.grabbedBy[0].grabber.mainBodyChunk.pos));
                this.rotation.y = Mathf.Abs(this.rotation.y);
            }
            if (base.firstChunk.ContactPoint.y < 0)
            {
                this.rotation = (this.rotation - RWCustom.Custom.PerpendicularVector(this.rotation)
                    * SCMorningGloryProperty.FruitPhysicalProperty.rollingRotationSpeedRatio
                    * base.firstChunk.vel.x).normalized;
                BodyChunk firstChunk = base.firstChunk;
                firstChunk.vel.x = firstChunk.vel.x * SCMorningGloryProperty.FruitPhysicalProperty.rollingFriction;
            }
            this.LightUpdate();

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
                if (this.pickTicking > ModifiableSCMorningGloryProperty.scMorningGlory.fruitPickRequireTicks)
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
                this.cutOffStalk();
            }
        }
        public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
        {
            // do nothing
        }
        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[2];
            sLeaser.sprites[0] = new FSprite("DangleFruit0A", true);
            sLeaser.sprites[1] = new FSprite("DangleFruit0B", true);
            sLeaser.sprites[0].scale = SCMorningGloryProperty.fruitSpriteScale;
            sLeaser.sprites[1].scale = SCMorningGloryProperty.fruitSpriteScale;
            this.AddToContainer(sLeaser, rCam);
        }
        public void cutOffStalk()
        {
            Assert.IsNotNull(this.stalk);
            // 告诉藤蔓要断了
            this.stalk.FruitDetatched();
            this.containerUpdated = true;
            this.stalk = null;
            (this.abstractPhysicalObject as AbstractMorningGloryFruit).spawnFromStalk = false;
        }
        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 pos = Vector2.Lerp(this.firstChunk.lastPos, this.firstChunk.pos, timeStacker);
            Vector2 rot = Vector3.Slerp(this.lastRotation, this.rotation, timeStacker);
            if (this.containerUpdated)
            {
                this.AddToContainer(sLeaser, rCam);
                this.containerUpdated = false;
            }
            int atlasID = Mathf.Min(SCMorningGloryProperty.fruitBites - this.BitesLeft, 2);
            sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName($"DangleFruit{atlasID}A");
            sLeaser.sprites[1].element = Futile.atlasManager.GetElementWithName($"DangleFruit{atlasID}B");
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
            sLeaser.sprites[1].color = palette.blackColor.CloneWithNewAlpha(0.8f);
            this.color = SCMorningGloryProperty.fruitBaseColor;
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
                    newContatiner = rCam.ReturnFContainer(SCMorningGloryProperty.fruitContainerAfterPick);
                }
                else
                {
                    newContatiner = rCam.ReturnFContainer(SCMorningGloryProperty.fruitContainerBeforePick);
                }
            }
            foreach (var fsp in sLeaser.sprites)
            {
                fsp.RemoveFromContainer();
            }
            foreach (var fsp in sLeaser.sprites)
            {
                newContatiner.AddChild(fsp);
            }
        }


        public int BitesLeft
        {
            get { return bitesLeft; }
        }

        public int FoodPoints
        {
            get { return SCMorningGloryProperty.fruitFoodPoints; }
        }

        public bool Edible
        {
            get { return true; }
        }

        // ↓好像没用
        public bool AutomaticPickUp => throw new NotImplementedException();
        public class AbstractMorningGloryFruit : AbstractPhysicalObject
        {
            public AbstractMorningGloryFruit(World world, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID) : base(world, SCEnums.AbstractObjectTypeType.SCMorningGloryFruit, realizedObject, pos, ID)
            {
                this.spawnFromStalk = false;
            }
            public override void Realize()
            {
                if (this.realizedObject != null)
                    return;
                if (this.spawnFromStalk)
                    return;
                this.realizedObject = new SCMorningGloryFruit(this);
            }
            public bool spawnFromStalk;
        }
    }
}
