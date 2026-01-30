using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using SCUtils.SCDevTools.NodeTreeManager;
using ShadedCanopy.Objects.SCNectarPlate;

namespace ShadedCanopy.Objects.SCNectarPlate
{
    public class SCNectarPlate : PhysicalObject, IDrawable, IPlayerEdible
    {
        public Vector2 anchorPos;
        public Vector2 rootPos = new Vector2(-1f, -1f);
        public Vector2 centerPos_outer;
        public Vector2 lastCenterPos_outer;
        public Vector2 centerPos_inner;
        public Vector2 rimPos_outer;
        public Vector2 lastRimPos_outer;
        public Vector2[] rimPoints_outer;
        public Vector2[] rimPoints_inner;
        public Vector2[] stemAnchors;
        public Vector2[,] stemPoints;
        public Vector2 camPos;
        public Vector2 idlePos = new Vector2(-1f, -1f);
        public int petals;
        public int grabCounter;
        public int feedCounter;
        public float outterRad;
        public Color color;
        public bool flipDirection;
        public float innerRad { get => 0.6f * outterRad; }
        public float rootRad;
        public float plateDepth { get => 0.3f * outterRad; }
        [SCDevToolsInspectValue]
        public float crystalSize = 1.8f;
        [SCDevToolsInspectValue]
        public float stemElastic = 0.2f;
        public float crystalLength { get => 0.36f * outterRad * crystalSize; }
        public float nectarThickness { get => 0.5f * innerRad; }
        public AbstractConsumable Consumable { get => (this.abstractPhysicalObject as AbstractNectarPlate) as AbstractConsumable; }
        public LightSource LightSource { get; set; }

        public static float yellowHue = 0.144445f;
        public static float cyanHue = 0.477778f;
        public static float satuation = 0.75f;
        public static float[] stemWidth = new float[8] { 0.48f, 0.36f, 0.28f, 0.18f, 0.22f, 0.24f, 0.27f, 0.29f };
        public static int placeCoolDown = 40;

        public SCNectarPlate(AbstractNectarPlate abstractNectarPlate, int petals, float outterRad, float rootRad = 200f) : base(abstractNectarPlate)
        {
            base.bodyChunks = new BodyChunk[1];
            base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 1f, 0.1f);
            this.bodyChunkConnections = new PhysicalObject.BodyChunkConnection[0];
            base.airFriction = 0.7f;
            base.gravity = 0f;
            this.bounce = 0.2f;
            this.surfaceFriction = 0.4f;
            this.collisionLayer = 1;
            base.waterFriction = 0.98f;
            base.buoyancy = 1f;
            base.bodyChunks[0].collideWithObjects = false;

            this.petals = petals;
            this.outterRad = outterRad;
            this.rootRad = rootRad;

            UnityEngine.Random.State state = UnityEngine.Random.state;
            UnityEngine.Random.InitState(this.abstractPhysicalObject.ID.RandomSeed);
            this.stemAnchors = new Vector2[2];
            this.stemAnchors[0] = new Vector2(UnityEngine.Random.Range(0.25f, 0.5f) * innerRad, UnityEngine.Random.Range(0.85f, 0.95f));
            this.stemAnchors[1] = new Vector2(UnityEngine.Random.Range(0.1f, 0.16f) * innerRad, UnityEngine.Random.Range(0.5f, 0.6f));

            this.stemPoints = new Vector2[4, 2];
            for (int i = 0; i < 4; i++)
            {
                stemPoints[i, 0] = this.centerPos_inner;
                stemPoints[i, 1] = this.centerPos_inner;
            }
            this.color = Custom.HSL2RGB((UnityEngine.Random.value < 0.25f ? yellowHue : cyanHue) + UnityEngine.Random.Range(-0.05f, 0.05f), satuation, 0.5f);
            this.flipDirection = UnityEngine.Random.value <= 0.5f;

            rimPoints_outer = new Vector2[petals];
            rimPoints_inner = new Vector2[petals];
            UnityEngine.Random.state = state;
        }

        public override void PlaceInRoom(Room placeRoom)
        {
            base.PlaceInRoom(placeRoom);

            Vector2 centerPos = placeRoom.MiddleOfTile(this.abstractPhysicalObject.pos);
            this.anchorPos = centerPos;
            this.centerPos_outer = centerPos;
            this.rimPos_outer = centerPos;

            Vector2 vec;
            vec.x = UnityEngine.Random.Range(1f, 3f) * 0.1f * outterRad;
            vec.y = UnityEngine.Random.Range(1f, 3f) * 0.1f * outterRad;
            vec.y = Mathf.Abs(vec.y);
            if (flipDirection)
            {
                vec.x = Mathf.Abs(vec.x);
            }
            else vec.x = - Mathf.Abs(vec.x);

            Vector2 idle = centerPos + vec;
            this.firstChunk.HardSetPosition(idle);
            this.idlePos = idle;

            if (this.LightSource == null || this.LightSource.room != placeRoom || this.LightSource.slatedForDeletetion)
            {
                this.LightSource = new LightSource(idle, false, color, this,true);
                this.LightSource.rad = this.outterRad * 3f;
                this.LightSource.alpha = 1f;
                this.LightSource.stayAlive = true;
                placeRoom.AddObject(this.LightSource);
            }

            this.FindRootPos();
        }

        public void FindRootPos()
        {
            for (int i = 0; i < Mathf.Min(40, this.room.Height); i++)
            {
                Vector2 vec = this.anchorPos + 20f * Vector2.down * i;
                if (this.room.GetTile(vec).Solid)
                {
                    this.rootPos = vec;
                    break;
                }
                else
                {
                    for (int j = 0; j < 8; j++)
                    {

                        if (this.room.GetTile(vec + 20f * Custom.IntVector2ToVector2(Custom.eightDirections[j])).Solid)
                        {
                            this.rootPos = vec + 20f * Custom.IntVector2ToVector2(Custom.eightDirections[j]);
                            break;
                        }
                    }
                }
            }
            if (rootPos.x < 0f && rootPos.y < 0f)
            {
                rootPos = this.anchorPos;
            }
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            TriangleMesh stemMesh = sLeaser.sprites[2 * petals + 5] as TriangleMesh;
            Color colorDark = Color.Lerp(Color.Lerp(color, Color.gray, 0.85f), Color.black, 0.5f);
            Color colorBright = Color.Lerp(Color.Lerp(color, Color.gray, 0.45f), Color.black, 0.1f);

            for (int i = 0; i < stemMesh.verticeColors.Length; i++)
            {
                stemMesh.verticeColors[i] = Color.Lerp(colorDark, colorBright, Mathf.InverseLerp(stemMesh.verticeColors.Length / 2 - 1, stemMesh.verticeColors.Length - 1, i));
            }
            sLeaser.sprites[2 * petals + 6].color = colorDark;
            sLeaser.sprites[2 * petals + 7].color = colorBright;

        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[2 * petals + 8];

            for (int i = 0; i < 2 * petals; i++)
            {
                sLeaser.sprites[i] = new CustomFSprite(i >= petals ? "Futile_White" : "atlases/PlatePetal");
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

            sLeaser.sprites[2 * petals + 5] = TriangleMesh.MakeLongMesh(10, false, true);
            sLeaser.sprites[2 * petals + 6] = new CustomFSprite("Circle20");
            sLeaser.sprites[2 * petals + 6].scale = stemWidth[0] * innerRad / 10f;
            sLeaser.sprites[2 * petals + 7] = new CustomFSprite("Circle20");
            sLeaser.sprites[2 * petals + 7].scale = stemWidth[7] * innerRad / 10f;

            /*
            sLeaser.sprites[2 * petals + 8] = new FSprite("Circle20");
            sLeaser.sprites[2 * petals + 8].color = Color.red;
            sLeaser.sprites[2 * petals + 8].scale = 0.25f;
            */

            this.AddToContainer(sLeaser, rCam, null);
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
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
            sLeaser.sprites[2 * petals + 5].MoveToBack();
            sLeaser.sprites[2 * petals + 6].MoveToBack();
            sLeaser.sprites[2 * petals + 7].MoveToBack();

            //rCam.ReturnFContainer("Bloom").AddChild(sLeaser.sprites[2 * petals + 8]);
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            this.camPos = camPos;
            Vector2 _centerPos_outer = Vector2.Lerp(lastCenterPos_outer, centerPos_outer, timeStacker);
            Vector2 _rimPos_outer = Vector2.Lerp(lastRimPos_outer, rimPos_outer, timeStacker);
            Vector2 dir = _rimPos_outer - _centerPos_outer;
            float zRotDeg = Custom.VecToDeg(dir);
            float sinRot = Mathf.Clamp01(dir.magnitude / outterRad);
            float cosRot = Mathf.Sqrt(1f - sinRot * sinRot);
            centerPos_inner = _centerPos_outer - plateDepth * cosRot * dir.normalized - camPos;

            sLeaser.sprites[2 * petals + 6].SetPosition(centerPos_inner);
            sLeaser.sprites[2 * petals + 7].SetPosition(rootPos);

            for (int i = 0; i < petals; i++)
            {
                Vector2 vec = RotatedRingPoints(i, zRotDeg, sinRot, _centerPos_outer) - camPos;
                Vector2 vec2 = (i == petals - 1 ? RotatedRingPoints(0, zRotDeg, sinRot, _centerPos_outer) : RotatedRingPoints(i + 1, zRotDeg, sinRot, _centerPos_outer)) - camPos;
                Vector2 vec_inner = RotatedRingPoints(i, zRotDeg, sinRot, _centerPos_outer, true, cosRot) - camPos;
                Vector2 vec2_inner = (i == petals - 1 ? RotatedRingPoints(0, zRotDeg, sinRot, _centerPos_outer, true, cosRot) : RotatedRingPoints(i + 1, zRotDeg, sinRot, _centerPos_outer, true, cosRot)) - camPos;

                int num = Mathf.FloorToInt(petals / 4f);
                Color colorDark = Color.Lerp(Color.Lerp(color, Color.gray, 0.85f), Color.black, 0.5f);
                float darkGradient = Mathf.InverseLerp(4f, 0f, Mathf.Min(Mathf.Abs(i - num), Mathf.Abs(i - (petals - num - 1))));
                Color lerpedColor1 = Color.Lerp(Color.Lerp(color, new Color(0.5f, 0.5f, 0.5f), 0.65f), Color.black, Mathf.Clamp(1f - sinRot, 0f, 0.75f) * darkGradient);
                Color lerpedColor2 = Color.Lerp(colorDark, Color.black, Mathf.Clamp(1f - sinRot, 0f, 0.75f) * darkGradient);
                //Outer petals
                (sLeaser.sprites[i] as CustomFSprite).MoveVertice(3, vec_inner);
                (sLeaser.sprites[i] as CustomFSprite).MoveVertice(0, vec);
                (sLeaser.sprites[i] as CustomFSprite).MoveVertice(1, vec2);
                (sLeaser.sprites[i] as CustomFSprite).MoveVertice(2, vec2_inner);

                (sLeaser.sprites[i] as CustomFSprite).verticeColors[0] = lerpedColor1;
                (sLeaser.sprites[i] as CustomFSprite).verticeColors[1] = lerpedColor1;
                (sLeaser.sprites[i] as CustomFSprite).verticeColors[2] = lerpedColor2;
                (sLeaser.sprites[i] as CustomFSprite).verticeColors[3] = lerpedColor2;
                //Inner plate
                (sLeaser.sprites[i + petals] as CustomFSprite).MoveVertice(0, centerPos_inner);
                (sLeaser.sprites[i + petals] as CustomFSprite).MoveVertice(1, vec_inner);
                (sLeaser.sprites[i + petals] as CustomFSprite).MoveVertice(2, vec2_inner);
                (sLeaser.sprites[i + petals] as CustomFSprite).MoveVertice(3, centerPos_inner);
                for (int j = 0; j < 4; j++)
                {
                    (sLeaser.sprites[i + petals] as CustomFSprite).verticeColors[j] = Color.Lerp(colorDark, Color.black, 0.75f);
                }
            }
            //Nectar and crystal
            HandleNectarMesh(sLeaser, zRotDeg, sinRot, centerPos_inner, 2 * petals, 0.6f);
            HandleNectarMesh(sLeaser, zRotDeg, sinRot, centerPos_inner, 2 * petals + 1, 1f);

            Vector2 crystalBase = centerPos_inner + Custom.RotateAroundOrigo(Vector2.up * nectarThickness * cosRot, zRotDeg);
            HandleCrystalMesh(sLeaser, zRotDeg, sinRot, cosRot, crystalBase, 2 * petals + 2, 1f);
            HandleCrystalMesh(sLeaser, zRotDeg, sinRot, cosRot, crystalBase, 2 * petals + 3, 0.8f);
            HandleCrystalMesh(sLeaser, zRotDeg, sinRot, cosRot, crystalBase, 2 * petals + 4, 0.4f);
            //Stem
            TriangleMesh stemMesh = sLeaser.sprites[2 * petals + 5] as TriangleMesh;
            for (int i = 0; i < stemMesh.vertices.Length / 2; i++)
            {
                Vector2 vec = EasyBezier(DrawStemPos(0, timeStacker) + camPos, DrawStemPos(1, timeStacker), DrawStemPos(2, timeStacker), DrawStemPos(3, timeStacker), Mathf.InverseLerp(0, stemMesh.vertices.Length / 2 - 1, i)) - camPos;
                int num;
                if (i < 5) num = Mathf.Clamp(i, 0, 2);
                else if (i < stemMesh.vertices.Length / 2 - 4) num = 3;
                else num = i - stemMesh.vertices.Length / 2 + 8;
                Vector2 vec2 = vec - (i == 0 ? centerPos_outer - camPos : EasyBezier(DrawStemPos(0, timeStacker) + camPos, DrawStemPos(1, timeStacker), DrawStemPos(2, timeStacker), DrawStemPos(3, timeStacker), Mathf.InverseLerp(0, stemMesh.vertices.Length / 2 - 1, i - 1)) - camPos);

                stemMesh.MoveVertice(2 * i, vec + Custom.PerpendicularVector(vec2) * innerRad * stemWidth[num]);
                stemMesh.MoveVertice(2 * i + 1, vec - Custom.PerpendicularVector(vec2) * innerRad * stemWidth[num]);
            }

            if(this.LightSource != null)
            {
                this.LightSource.pos = this.centerPos_inner;
            }
            //sLeaser.sprites[2 * petals + 8].SetPosition(Vector2.Lerp(this.stemPoints[1,1], this.stemPoints[1,0], timeStacker) - camPos);
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            lastCenterPos_outer = centerPos_outer;
            lastRimPos_outer = rimPos_outer;

            //Limiting bodychunk pos
            if (idlePos.x >= 0f && idlePos.y >= 0f)
            {
                float distToIdle = (this.firstChunk.pos - idlePos).magnitude;
                Vector2 dirToIdle = (-this.firstChunk.pos + idlePos).normalized;
                this.firstChunk.vel += stemElastic * distToIdle * dirToIdle;
            }

            float dist = (this.firstChunk.pos - anchorPos).magnitude;
            if (dist >= outterRad)
            {
                Vector2 dir = (this.firstChunk.pos - anchorPos).normalized;

                if (dist <= rootRad + outterRad)
                {
                }
                else
                {
                    this.firstChunk.pos = anchorPos + dir * (outterRad + rootRad);
                }
                centerPos_outer = this.firstChunk.pos - dir * outterRad;
            }

            this.rimPos_outer = this.firstChunk.pos;

            //Stem
            for (int i = 0; i < 4; i++)
            {
                stemPoints[i, 1] = stemPoints[i, 0];
            }
            stemPoints[0, 0] = this.centerPos_inner;
            stemPoints[3, 0] = this.rootPos;
            Vector2 vec = this.centerPos_inner + camPos - this.rootPos;
            float deg = Custom.VecToDeg(vec);
            float num = -1f;
            if (deg <= -45f && deg >= -135f)
            {
                num = Mathf.Lerp(-1f, 1f, Mathf.InverseLerp(45f, -45f, deg + 90f));
            }
            else if (deg < -135f || deg > 135f)
            {
                num = 1f;
            }
            else if (deg >= 45f)
            {
                num = Mathf.Lerp(1f, -1f, Mathf.InverseLerp(45f, -45f, deg - 90f));
            }
            float num2 = flipDirection ? -0.5f : +0.5f;
            stemPoints[1, 0] = Vector2.Lerp(this.rootPos, this.centerPos_inner + camPos, stemAnchors[0].y) + Custom.PerpendicularVector(vec) * num * stemAnchors[0].x * this.innerRad * num2;
            stemPoints[2, 0] = Vector2.Lerp(this.rootPos, this.centerPos_inner + camPos, stemAnchors[1].y) + Custom.PerpendicularVector(vec) * num * stemAnchors[1].x * this.innerRad * num2;

            //Other updates
            bool playerLicking = false;
            if (this.grabbedBy != null && this.grabbedBy.Count > 0)
            {
                for (int i = 0; i < this.grabbedBy.Count; i++)
                {
                    if (this.grabbedBy[i].grabber != null && this.grabbedBy[i].grabber is Player player && player.Consious && player.input[0].pckp && player.FoodInStomach < player.MaxFoodInStomach && player.input[0].x == 0 && player.input[0].y == 0 && !player.input[0].jmp)
                    {
                        playerLicking = true;
                        if (grabCounter >= 60)
                        {
                            player.Blink(5);
                            Vector2 vec2 = (this.firstChunk.pos - player.firstChunk.pos).normalized;
                            this.firstChunk.vel += 5f * vec2;
                            Vector2 vec3 = (this.anchorPos - player.firstChunk.pos).normalized;
                            player.firstChunk.vel += vec3;
                            player.aerobicLevel = 0.8f;
                            if (this.room != null)
                            {
                                if (UnityEngine.Random.value <= 0.04f)
                                {
                                    this.room.AddObject(new WaterDrip(player.firstChunk.pos, new Vector2(UnityEngine.Random.Range(-7f, 7f), UnityEngine.Random.Range(0f, 7f)), false));
                                }
                            }

                            if (feedCounter > 0) feedCounter--;
                            if (feedCounter <= 0)
                            {
                                feedCounter = 20;
                                player.AddQuarterFood();
                            }
                        }
                    }
                }
            }
            if (playerLicking)
            {
                grabCounter++;
            }
            else
            {
                feedCounter = 20;
                grabCounter = 0;
            }
        }

        public Vector2 RotatedRingPoints(int index, float zRotDeg, float sinRot, Vector2 centerPos, bool inner = false, float cosRot = -1)
        {
            float deg = 2f * Mathf.PI * index / petals;
            float l = Mathf.Cos(deg) * (inner ? innerRad : outterRad);
            Vector2 vec = centerPos + new Vector2(Mathf.Sin(deg) * (inner ? innerRad : outterRad), l * sinRot) - (inner ? cosRot * plateDepth * Vector2.up : Vector2.zero);
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

        public Vector2 DrawStemPos(int index, float timeStacker)
        {
            return Vector2.Lerp(stemPoints[index, 1], stemPoints[index, 0], timeStacker);
        }

        public Vector2 EasyBezier(Vector2 A, Vector2 handleA, Vector2 handleB, Vector2 B, float num)
        {
            Vector2 lerpA = Vector2.Lerp(A, handleA, num);
            Vector2 lerpB = Vector2.Lerp(handleB, B, num);
            return Vector2.Lerp(lerpA, lerpB, num);
        }

        public void BitByPlayer(Creature.Grasp grasp, bool eu)
        {

        }

        public void ThrowByPlayer()
        {

        }

        public bool Edible => false;
        public int BitesLeft => 1;
        public int FoodPoints => 999;
        public bool AutomaticPickUp => false;


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
                    AbstractNectarPlate abstractNectarPlate = new AbstractNectarPlate(self.room.world, AbstractNectarPlate.NectarPlate, null, self.abstractCreature.pos, self.room.game.GetNewID(), self.room.abstractRoom.index, 9, null);
                    self.room.abstractRoom.AddEntity(abstractNectarPlate);
                    SCNectarPlate nectarPlate = new SCNectarPlate(abstractNectarPlate, UnityEngine.Random.Range(12, 16), 40f, 80f);
                    nectarPlate.PlaceInRoom(self.room);
                    placeCoolDown = 40;
                }
            }
        }
    }
}
