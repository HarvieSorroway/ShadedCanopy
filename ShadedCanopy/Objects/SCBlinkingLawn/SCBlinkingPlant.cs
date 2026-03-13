using ShadedCanopy.Objects.SCWindFIeld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShadedCanopy.Objects.SCBlinkingLawn
{
    internal struct PlantInfo
    {
        PlantInfo(string describ, string []spriteNames, Func<float> mass, Func<float> lightRad)
        {
            this.describ = describ;
            this.spriteNames = spriteNames;
            this.mass = mass;
            this.lightRad = lightRad;
        }
        public string describ;
        public string[] spriteNames;
        public Func<float> mass;
        public Func<float> lightRad;
        public static PlantInfo[] presetInfos =
        {
            new PlantInfo(
                "Small pixel",
                new string[]{
                    $"{SCBlinkingLawnHooks.BlinkingTawnPlantSpriteName}A0",
                    $"{SCBlinkingLawnHooks.BlinkingTawnPlantSpriteName}A1",
                },
                () => SCBlinkingPlantProperty.Value.AMass,
                () => SCBlinkingPlantProperty.Value.ALightRad
                ),
            new PlantInfo(
                "4 pixels med",
                new string[]{
                    $"{SCBlinkingLawnHooks.BlinkingTawnPlantSpriteName}B0",
                    $"{SCBlinkingLawnHooks.BlinkingTawnPlantSpriteName}B1",
                },
                () => SCBlinkingPlantProperty.Value.BMass,
                () => SCBlinkingPlantProperty.Value.BLightRad
                ),
            new PlantInfo(
                "11 pixels large",
                new string[]{
                    $"{SCBlinkingLawnHooks.BlinkingTawnPlantSpriteName}C0",
                    $"{SCBlinkingLawnHooks.BlinkingTawnPlantSpriteName}C1",
                },
                () => SCBlinkingPlantProperty.Value.CMass,
                () => SCBlinkingPlantProperty.Value.CLightRad
                ),
            new PlantInfo(
                "19 pixels XL",
                new string[]{
                    $"{SCBlinkingLawnHooks.BlinkingTawnPlantSpriteName}D0",
                    $"{SCBlinkingLawnHooks.BlinkingTawnPlantSpriteName}D1",
                },
                () => SCBlinkingPlantProperty.Value.DMass,
                () => SCBlinkingPlantProperty.Value.DLightRad
                ),
        };
    };
    

    internal class SCBlinkingPlant: CosmeticSprite
    {
        SCWindField windField;
        Vector2 rootPos;
        Vector2[] angle = new Vector2[2];
        Vector2[] angleVel = new Vector2[2];

        int latency;
        float depth;
        PlantInfo plantInfo;
        Color color;
        LightSource lightSource;
        float lightIntensity;
        float brightness, lastBrightness;
        // 有风力的时候（>thre）亮度线性提升。没有的时候线性下降
        bool isDeepest
        {
            get
            {
                return depth > SCBlinkingPlantProperty.Value.foreDepthThreshold;
            }
        }

        public SCBlinkingPlant(Room room, Vector2 pos, Color color, float? depth=default, int? style=default, SCWindField windField=default): base()
        {
            this.room = room;
            this.pos = pos;
            this.color = color;
            if (windField is null)
            {
                IEnumerable<SCWindField> windFields = from UpdatableAndDeletable uad in room.updateList
                                                      where uad is SCWindField
                                                      select uad as SCWindField;
                if (windFields.Any())
                {
                    windField = windFields.First();
                } else
                {
                    windField = new SCWindFieldTest(room, 1);
                    room.AddObject(windField);
                }
            }
            this.windField = windField;
            RWCustom.IntVector2 inTile = room.GetTilePosition(pos);
            inTile.y -= 1;
            while (inTile.y >= 0 && !room.GetTile(inTile).IsSolid())
            {
                inTile.y -= 1;
            }
            rootPos = room.MiddleOfTile(inTile);
            rootPos.x = pos.x;
            rootPos.y += 9f;
            if (!depth.HasValue)
                depth = UnityEngine.Random.Range(0f, 1f);
            this.depth = depth.Value;
            latency = Mathf.CeilToInt(SCBlinkingPlantProperty.Value.maxLatensity * this.depth);
            if (!style.HasValue)
                style = UnityEngine.Random.Range(0, 3);
            plantInfo = PlantInfo.presetInfos[style.Value];
            if (!isDeepest)
            {
                this.lightSource = new LightSource(pos, false, color, this);
                room.AddObject(this.lightSource);
                lightSource.requireUpKeep = false;
                lightSource.setRad = plantInfo.lightRad();
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[2];
            sLeaser.sprites[0] = new FSprite(plantInfo.spriteNames[0]);
            sLeaser.sprites[1] = new FSprite(plantInfo.spriteNames[1]);
            AddToContainer(sLeaser, rCam);
        }
        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner=null)
        {
            if (newContatiner == null)
            {
                if (this.depth <= SCBlinkingPlantProperty.Value.superforeDepthThreshold)
                {
                    newContatiner = rCam.ReturnFContainer("Bloom");
                }
                else if (this.depth <= SCBlinkingPlantProperty.Value.foreDepthThreshold)
                {
                    newContatiner = rCam.ReturnFContainer("Shortcuts");
                } else
                {
                    newContatiner = rCam.ReturnFContainer("Background");
                }
            }
            newContatiner.AddChild(sLeaser.sprites[0]);
            newContatiner.AddChild(sLeaser.sprites[1]);
        }
        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            base.ApplyPalette(sLeaser, rCam, palette);
            float realBrightness = Mathf.Lerp(SCBlinkingPlantProperty.Value.brightnessRange.x, SCBlinkingPlantProperty.Value.brightnessRange.y, brightness);
            Color britColor = Color.Lerp(rCam.currentPalette.blackColor, color, realBrightness);
            sLeaser.sprites[0].color = Color.Lerp(britColor, palette.fogColor, depth);
            sLeaser.sprites[1].alpha = 0.2f;
            sLeaser.sprites[1].color = Color.white;
        }
        public override void Update(bool eu)
        {
            if (slatedForDeletetion) return;
            base.Update(eu);
            angle[1] = angle[0];
            angleVel[1] = angleVel[0];
            angleVel[0] *= SCBlinkingPlantProperty.Value.airFriction;
            Vector2 wind = windField.GetWind(this.pos, this.latency);
            angleVel[0] += wind / plantInfo.mass();
            angleVel[0] += -angle[0] * SCBlinkingPlantProperty.Value.elasticity;
            angle[0] += angleVel[0];
            lastBrightness = brightness;
            if (wind.magnitude > SCBlinkingPlantProperty.Value.lightUpWindThreshold)
            {
                brightness = Mathf.Clamp01(brightness + SCBlinkingPlantProperty.Value.brightnessUpSpeed);
                lightIntensity = Mathf.Clamp01(lightIntensity + SCBlinkingPlantProperty.Value.lightUpSpeed);
            } else
            {
                brightness = Mathf.Clamp01(brightness - SCBlinkingPlantProperty.Value.brightnessDownSpeed);
                lightIntensity = Mathf.Clamp01(lightIntensity - SCBlinkingPlantProperty.Value.lightDownSpeed);
            }
            if (!isDeepest)
            {
                lightSource.setAlpha = lightIntensity * SCBlinkingPlantProperty.Value.lightnessMax;
                lightSource.setRad = plantInfo.lightRad();
            }
        }
        public override void Destroy()
        {
            base.Destroy();
            if (!isDeepest)
            {
                lightSource.Destroy();
            }
        }
        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 origDirc = (pos - rootPos);
            Vector2 dirc = origDirc + Vector2.Lerp(angle[1], angle[0], timeStacker);
            dirc = dirc.normalized;
            Vector2 targetPos = rootPos + dirc * Vector2.Distance(pos, rootPos);
            if (!isDeepest)
            {
                lightSource.pos = targetPos;
            }
            if (lastBrightness != brightness)
            {
                ApplyPalette(sLeaser, rCam, rCam.currentPalette);
            }
            sLeaser.sprites[0].SetPosition(targetPos - camPos);
            sLeaser.sprites[1].SetPosition(targetPos - camPos);
        }

    }
}
