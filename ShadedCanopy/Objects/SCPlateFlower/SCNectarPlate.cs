using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using SCUtils.SCDevTools.NodeTreeManager;

namespace ShadedCanopy.Objects.SCNectarPlate
{
    public class SCNectarPlate : CosmeticSprite
    {
        public Vector2 rootPos;
        public Vector2 centerPos_outer;
        public Vector2 lastCenterPos_outer;
        public Vector2 centerPos_inner;
        public Vector2 lastCenterPos_inner;
        public Vector2 rimPos_outer;
        public Vector2 lastRimPos_outer;
        public Vector2[] rimPoints_outer;
        public Vector2[] rimPoints_inner;
        public Vector2 camPos;
        public int petals;
        public float outterRad;
        public Color color;
        public float innerRad { get => 0.6f * outterRad; }
        public float rootRad;
        public float plateDepth { get => 0.3f * outterRad; }
        [SCDevToolsInspectValue]
        public float crystalSize = 1.8f;
        public float crystalLength { get => 0.36f * outterRad * crystalSize; }
        public float nectarThickness { get => 0.5f * innerRad; }

        public static float yellowHue = 0.144445f;
        public static float cyanHue = 0.477778f;
        public static float satuation = 0.75f;
        public static int placeCoolDown = 40;

        public SCNectarPlate(Room room, Vector2 centerPos, int petals, float outterRad, float rootRad = 200f)
        {
            this.room = room;
            this.rootPos = centerPos;
            this.centerPos_outer = centerPos;
            this.centerPos_inner = centerPos;
            this.rimPos_outer = centerPos;
            this.petals = petals;
            this.outterRad = outterRad;
            this.rootRad = rootRad;

            this.color = Custom.HSL2RGB((UnityEngine.Random.value < 0.25f? yellowHue : cyanHue) + UnityEngine.Random.Range(-0.05f, 0.05f), satuation, 0.5f);

            rimPoints_outer = new Vector2[petals];
            rimPoints_inner = new Vector2[petals];
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[2 * petals + 5];

            for (int i = 0; i < 2 * petals; i++)
            {
                sLeaser.sprites[i] = new CustomFSprite(i >= petals? "Futile_White" : "atlases/PlatePetal");
                if (i >= petals) 
                {
                    for (int j = 0; j < 4; j++)
                    {
                        (sLeaser.sprites[i] as CustomFSprite).verticeColors[j] = Color.cyan;
                    }
                }
                else
                {
                    (sLeaser.sprites[i] as CustomFSprite).verticeColors[2] = Color.blue;
                    (sLeaser.sprites[i] as CustomFSprite).verticeColors[3] = Color.blue;
                }
            }

            sLeaser.sprites[2 * petals] = TriangleMesh.MakeGridMesh("Futile_White", 2);
            sLeaser.sprites[2 * petals].color = Color.Lerp(color, Color.black, 0.4f);
            sLeaser.sprites[2 * petals].shader = Custom.rainWorld.Shaders["VectorCircle"];

            sLeaser.sprites[2 * petals + 1] = TriangleMesh.MakeGridMesh("Futile_White", 2);
            sLeaser.sprites[2 * petals + 1].color = color;
            sLeaser.sprites[2 * petals + 1].shader = Custom.rainWorld.Shaders["WaterNut"];

            sLeaser.sprites[2 * petals + 2] = new CustomFSprite("atlases/NectarCrystal");
            sLeaser.sprites[2 * petals + 3] = new CustomFSprite("atlases/NectarCrystal");
            sLeaser.sprites[2 * petals + 4] = new CustomFSprite("DangleFruit0A");

            for (int i = 0; i < 4; i++)
            {
                (sLeaser.sprites[2 * petals + 2] as CustomFSprite).verticeColors[i] = Color.Lerp(color, Color.black, 0.3f);
                (sLeaser.sprites[2 * petals + 3] as CustomFSprite).verticeColors[i] = color;
                (sLeaser.sprites[2 * petals + 4] as CustomFSprite).verticeColors[i] = Color.Lerp(color, Color.white, 0.75f);
            }

            this.AddToContainer(sLeaser, rCam, null);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            FContainer fContainer = rCam.ReturnFContainer("Midground");
            for (int i = sLeaser.sprites.Length - 1; i >= 0; i--)
            {
                fContainer.AddChild(sLeaser.sprites[i]);
            }
            for (int i = 0; i < petals; i++)
            {
                sLeaser.sprites[petals + i].MoveBehindOtherNode(sLeaser.sprites[petals - 1]);
            }

            int num = Mathf.FloorToInt(petals / 4f);
            for (int i = 0; i < num; i++)
            {
                sLeaser.sprites[i].MoveBehindOtherNode(sLeaser.sprites[petals - 1 - num]);
            }

            sLeaser.sprites[2 * petals].MoveBehindOtherNode(sLeaser.sprites[petals - 1 - num]);
            sLeaser.sprites[2 * petals + 1].MoveBehindOtherNode(sLeaser.sprites[petals - 1 - num]);
            sLeaser.sprites[2 * petals + 2].MoveBehindOtherNode(sLeaser.sprites[petals - 1 - num]);
            sLeaser.sprites[2 * petals + 3].MoveBehindOtherNode(sLeaser.sprites[petals - 1 - num]);
            sLeaser.sprites[2 * petals + 4].MoveBehindOtherNode(sLeaser.sprites[petals - 1 - num]);

        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            this.camPos = camPos;
            Vector2 _centerPos_outer = Vector2.Lerp(lastCenterPos_outer, centerPos_outer, timeStacker);
            Vector2 _rimPos_outer = Vector2.Lerp(lastRimPos_outer, rimPos_outer, timeStacker);
            Vector2 dir = _rimPos_outer - _centerPos_outer;
            float zRotDeg = Custom.VecToDeg(dir);
            float sinRot = Mathf.Clamp01(dir.magnitude / outterRad);
            float cosRot = Mathf.Sqrt(1f - sinRot * sinRot);
            centerPos_inner = _centerPos_outer - plateDepth * cosRot * dir.normalized - camPos;
            for (int i = 0; i < petals; i++)
            {
                Vector2 vec = RotatedRingPoints(i, zRotDeg, sinRot, _centerPos_outer) - camPos;
                Vector2 vec2 = (i == petals - 1 ? RotatedRingPoints(0, zRotDeg, sinRot, _centerPos_outer) : RotatedRingPoints(i + 1, zRotDeg, sinRot, _centerPos_outer)) - camPos;
                Vector2 vec_inner = RotatedRingPoints(i, zRotDeg, sinRot, _centerPos_outer,true, cosRot) - camPos;
                Vector2 vec2_inner = (i == petals - 1 ? RotatedRingPoints(0, zRotDeg, sinRot, _centerPos_outer, true, cosRot) : RotatedRingPoints(i + 1, zRotDeg, sinRot, _centerPos_outer, true, cosRot)) - camPos;

                int num = Mathf.FloorToInt(petals / 4f);
                Color colorDark = Color.Lerp(Color.Lerp(color, Color.gray, 0.85f), Color.black, 0.5f);
                float darkGradient = Mathf.InverseLerp(4f, 0f, Mathf.Min(Mathf.Abs(i - num), Mathf.Abs(i - (petals - num - 1))));
                Color lerpedColor1 = Color.Lerp(Color.Lerp(color, new Color(0.5f, 0.5f, 0.5f), 0.65f), Color.black, Mathf.Clamp(1f - sinRot, 0f, 0.75f) * darkGradient);
                Color lerpedColor2 = Color.Lerp(colorDark, Color.black, Mathf.Clamp(1f - sinRot, 0f, 0.75f) * darkGradient);

                (sLeaser.sprites[i] as CustomFSprite).MoveVertice(3, vec_inner);
                (sLeaser.sprites[i] as CustomFSprite).MoveVertice(0, vec);
                (sLeaser.sprites[i] as CustomFSprite).MoveVertice(1, vec2);
                (sLeaser.sprites[i] as CustomFSprite).MoveVertice(2, vec2_inner);

                (sLeaser.sprites[i] as CustomFSprite).verticeColors[0] = lerpedColor1;
                (sLeaser.sprites[i] as CustomFSprite).verticeColors[1] = lerpedColor1;
                (sLeaser.sprites[i] as CustomFSprite).verticeColors[2] = lerpedColor2;
                (sLeaser.sprites[i] as CustomFSprite).verticeColors[3] = lerpedColor2;

                (sLeaser.sprites[i + petals] as CustomFSprite).MoveVertice(0, centerPos_inner);
                (sLeaser.sprites[i + petals] as CustomFSprite).MoveVertice(1, vec_inner);
                (sLeaser.sprites[i + petals] as CustomFSprite).MoveVertice(2, vec2_inner);
                (sLeaser.sprites[i + petals] as CustomFSprite).MoveVertice(3, centerPos_inner);
                for (int j = 0; j < 4; j++)
                {
                    (sLeaser.sprites[i + petals] as CustomFSprite).verticeColors[j] = Color.Lerp(colorDark, Color.black, 0.75f);
                }
            }
            HandleNectarMesh(sLeaser, zRotDeg, sinRot, centerPos_inner, 2 * petals, 0.6f);
            HandleNectarMesh(sLeaser, zRotDeg, sinRot, centerPos_inner, 2 * petals + 1, 1f);

            Vector2 crystalBase = centerPos_inner + Custom.RotateAroundOrigo(Vector2.up * nectarThickness * cosRot, zRotDeg);
            HandleCrystalMesh(sLeaser, zRotDeg, sinRot, cosRot, crystalBase, 2 * petals + 2, 1f);
            HandleCrystalMesh(sLeaser, zRotDeg, sinRot, cosRot, crystalBase, 2 * petals + 3, 0.8f);
            HandleCrystalMesh(sLeaser, zRotDeg, sinRot, cosRot, crystalBase, 2 * petals + 4, 0.4f);
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            lastCenterPos_inner = centerPos_inner;
            lastCenterPos_outer = centerPos_outer;
            lastRimPos_outer = rimPos_outer;

            this.rimPos_outer = (Vector2)Futile.mousePosition + camPos;
            float dist = (rimPos_outer - rootPos).magnitude;
            if (dist >= outterRad )
            {
                Vector2 dir = (rimPos_outer - rootPos).normalized;
                if (dist <= rootRad + outterRad)
                {
                }
                else
                {
                    rimPos_outer = rootPos + dir * (outterRad + rootRad);
                }
                centerPos_outer = rimPos_outer - dir * outterRad;
            }
        }

        public Vector2 RotatedRingPoints(int index, float zRotDeg, float sinRot, Vector2 centerPos, bool inner = false, float cosRot = -1)
        {
            float deg = 2f * Mathf.PI * index / petals;
            float l = Mathf.Cos(deg) * (inner? innerRad : outterRad);
            Vector2 vec = centerPos + new Vector2(Mathf.Sin(deg) * (inner? innerRad : outterRad), l * sinRot) - (inner? cosRot * plateDepth * Vector2.up : Vector2.zero);
            vec = Custom.RotateAroundVector(vec, centerPos, zRotDeg);
            return vec;
        }

        public void HandleCrystalMesh(RoomCamera.SpriteLeaser sLeaser, float zRotDeg, float sinRot, float cosRot, Vector2 basePos, int index, float shadeMultiplier)
        {
            float tipLength = 0.5f * crystalLength * cosRot * shadeMultiplier;
            float sideTipLength = 0.3f * crystalLength * sinRot * shadeMultiplier;
            Vector2 pos_center = basePos + Custom.RotateAroundOrigo(Vector2.up * tipLength * cosRot, zRotDeg);
            Vector2 dir = Custom.DegToVec(zRotDeg);

            Vector2 meshTipPos1 = pos_center + Custom.RotateAroundOrigo(Vector2.up * Mathf.Max(tipLength, sideTipLength), zRotDeg);
            Vector2 meshTipPos2 = pos_center - Custom.RotateAroundOrigo(Vector2.up * Mathf.Max(tipLength, sideTipLength), zRotDeg);
            (sLeaser.sprites[index] as CustomFSprite).MoveVertice(0, meshTipPos1 + Custom.PerpendicularVector(dir) * 0.4f * crystalLength * shadeMultiplier);
            (sLeaser.sprites[index] as CustomFSprite).MoveVertice(1, meshTipPos1 - Custom.PerpendicularVector(dir) * 0.4f * crystalLength * shadeMultiplier);
            (sLeaser.sprites[index] as CustomFSprite).MoveVertice(2, meshTipPos2 - Custom.PerpendicularVector(dir) * 0.4f * crystalLength * shadeMultiplier);
            (sLeaser.sprites[index] as CustomFSprite).MoveVertice(3, meshTipPos2 + Custom.PerpendicularVector(dir) * 0.4f * crystalLength * shadeMultiplier);
        }

        public void HandleNectarMesh(RoomCamera.SpriteLeaser sLeaser, float zRotDeg, float sinRot, Vector2 centerPos, int index, float shadeMultiplier)
        {
            float h = innerRad * sinRot;
            float l = Mathf.Max(h, nectarThickness) * shadeMultiplier;
            (sLeaser.sprites[index] as TriangleMesh).MoveVertice(6, Custom.RotateAroundVector(centerPos + new Vector2(-innerRad * shadeMultiplier, l), centerPos, zRotDeg));
            (sLeaser.sprites[index] as TriangleMesh).MoveVertice(7, Custom.RotateAroundVector(centerPos + new Vector2(0, l), centerPos, zRotDeg));
            (sLeaser.sprites[index] as TriangleMesh).MoveVertice(8, Custom.RotateAroundVector(centerPos + new Vector2(+innerRad * shadeMultiplier, l), centerPos, zRotDeg));
            (sLeaser.sprites[index] as TriangleMesh).MoveVertice(5, Custom.RotateAroundVector(centerPos + new Vector2(+innerRad * shadeMultiplier, 0), centerPos, zRotDeg));
            (sLeaser.sprites[index] as TriangleMesh).MoveVertice(2, Custom.RotateAroundVector(centerPos + new Vector2(+innerRad * shadeMultiplier, -h * shadeMultiplier), centerPos, zRotDeg));
            (sLeaser.sprites[index] as TriangleMesh).MoveVertice(1, Custom.RotateAroundVector(centerPos + new Vector2(0, -h * shadeMultiplier), centerPos, zRotDeg));
            (sLeaser.sprites[index] as TriangleMesh).MoveVertice(0, Custom.RotateAroundVector(centerPos + new Vector2(-innerRad * shadeMultiplier, -h * shadeMultiplier), centerPos, zRotDeg));
            (sLeaser.sprites[index] as TriangleMesh).MoveVertice(3, Custom.RotateAroundVector(centerPos + new Vector2(-innerRad * shadeMultiplier, 0), centerPos, zRotDeg));
            (sLeaser.sprites[index] as TriangleMesh).MoveVertice(4, centerPos);
        }

        public static void TestHooks()
        {
            On.Player.Update += Player_Update;
        }

        private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);
            if (placeCoolDown > 0) placeCoolDown--;
            if (self.room != null && self.room.game.IsArenaSession)
            {
                if (Input.GetKey(KeyCode.L) && placeCoolDown <= 0)
                {
                    self.room.AddObject(new SCNectarPlate(self.room, self.firstChunk.pos, UnityEngine.Random.Range(12, 16), 40f, 80f));
                    placeCoolDown = 40;
                }
            }
        }
    }
}
