using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ShadedCanopy.Effect.SCSuperStructureEffect
{
    internal class SCBoids : SCSuperStructureProjPart
    {
        static int boidSpriteStartIndex = 0;

        HashSet<Boid>[,] tileOfBoids;
        public List<Boid> totBoids = new List<Boid>();

        float followedBoidCount;
        int followedBoidIndex, noSwitchCounter;
        Vector2 followedBoidPos, lastFollowedBoidPos;


        public new Room room => projector.room;

        public SCBoids(SCSuperStructureProj proj, int boidsCount) : base(proj)
        {
            //构建查找用的哈希数组
            tileOfBoids = new HashSet<Boid>[room.Width, room.Height];
            for (int x = 0; x < room.Width; x++)
            {
                for (int y = 0; y < room.Height; y++)
                {
                    tileOfBoids[x, y] = new HashSet<Boid>();
                }
            }


            Boid newBoid;
            //散步boid
            for (int i = 0; i < boidsCount; i++)
            {
                newBoid = new Boid(this, new Vector2(Random.value * room.Width * 20f, Random.value * room.Height * 20f));
                totBoids.Add(newBoid);
            }
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            foreach (var boid in totBoids)
                boid.Update();
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[totBoids.Count];

            for (int i = 0; i < totBoids.Count; i++)
            {
                sLeaser.sprites[i + boidSpriteStartIndex] = new FSprite("Big_Menu_Arrow", true)
                {
                    scaleX = 0.25f,
                    scaleY = 0.5f,
                    color = Color.black,
                    //shader = rCam.room.game.rainWorld.Shaders["Hologram"]
                };
            }
            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
          
            for (int i = 0; i < totBoids.Count; i++)
            {
                Vector2 smoothPos = Vector2.Lerp(totBoids[i].lastPos, totBoids[i].pos, timeStacker) - camPos;
                sLeaser.sprites[i + boidSpriteStartIndex].SetPosition(smoothPos);
                sLeaser.sprites[i + boidSpriteStartIndex].rotation = Custom.VecToDeg(totBoids[i].dir);
            }
        }

        public IEnumerable<Boid> GetBoidWithinRad(Boid self, Vector2 pos, float rad, bool ignoreDistanceCheck = true, int checkCount = -1)
        {
            int intHalfRad = Mathf.CeilToInt(rad * 0.5f / 20f);
            int count = 0;
            var coord = room.GetTilePosition(pos);

            int leftX = Mathf.Clamp(coord.x - intHalfRad, 0, room.Width - 1);
            int rightX = Mathf.Clamp(coord.x + intHalfRad, 0, room.Width - 1);
            int downY = Mathf.Clamp(coord.y - intHalfRad, 0, room.Height - 1);
            int upY = Mathf.Clamp(coord.y + intHalfRad, 0, room.Height - 1);

            for (int x = leftX; x <= rightX; x++)
            {
                for (int y = downY; y <= upY; y++)
                {
                    foreach (var boid in tileOfBoids[x, y])
                    {
                        if (boid == self) continue;
                        if (checkCount > 0 && count == checkCount)
                            yield break;

                        if (!ignoreDistanceCheck)
                        {
                            if (Vector2.Distance(pos, boid.pos) < rad)
                                yield return boid;
                        }
                        else
                            yield return boid;
                        count++;
                    }
                }
            }
        }

        void ArrangeBoid(Boid boid)
        {
            if (boid.lastTile != boid.tile)
            {
                if (boid.lastTile.x >= 0 && boid.lastTile.y >= 0)
                {
                    tileOfBoids[boid.lastTile.x, boid.lastTile.y].Remove(boid);
                }
                tileOfBoids[boid.tile.x, boid.tile.y].Add(boid);
            }
        }


        public class Boid
        {
            public float vel;
            public Vector2 dir;
            public Vector2 pos, lastPos;

            public IntVector2 lastTile, tile;
            public float randomBias;
            public float flash;

            SCBoids effect;

            public Boid(SCBoids effect, Vector2 initPos)
            {
                this.effect = effect;
                pos = lastPos = initPos;
                lastTile = new IntVector2(-1, -1);
                tile = effect.room.GetTilePosition(pos);
                ClampInRoom(ref tile);
                effect.ArrangeBoid(this);

                dir = Custom.RNV();
                vel = Mathf.Lerp(5f, 15f, Random.value);
                randomBias = Random.value;
            }

            public Vector2 Vel
            {
                get => dir * vel;
                set { dir = value.normalized; vel = value.magnitude; }
            }

            public void Update()
            {
                lastPos = pos;
                pos += Vel;

                if (pos.x > effect.room.Width * 20f)
                {
                    pos.x -= effect.room.Width * 20f;
                    lastPos = pos;
                }
                else if (pos.x < 0f)
                {
                    pos.x += effect.room.Width * 20f;
                    lastPos = pos;
                }

                if (pos.y > effect.room.Height * 20f)
                {
                    pos.y -= effect.room.Height * 20f;
                    lastPos = pos;
                }
                else if (pos.y < 0f)
                {
                    pos.y += effect.room.Height * 20f;
                    lastPos = pos;
                }

                lastTile = tile;
                tile = effect.room.GetTilePosition(pos);
                ClampInRoom(ref tile);

                effect.ArrangeBoid(this);

                bool skipBoidsLogic = TerrainCheck();
                float skipUpdateRate = Mathf.Lerp(0f, 0.9f, Mathf.InverseLerp(10f, 50f, effect.GetBoidWithinRad(this, pos, 40f).Count()));
                if (Random.value < skipUpdateRate)
                {
                    skipBoidsLogic = true;
                }
                if(!skipBoidsLogic)
                {
                    Seperation();
                    Cohesion();
                    Alignment();
                }

                dir = Vector3.Slerp(dir, Custom.RNV(), randomBias * 0.1f);
                vel = Mathf.Clamp(Mathf.Lerp(vel, 3f + randomBias * Random.value, 0.15f), 0, 4f);
                flash = Custom.VecToDeg(dir) / 360f;
                //while (flash > 1)
                //    flash--;
            }

            void Seperation()
            {
                int count = 0;
                Vector2 center = Vector2.zero;
                foreach (var boid in effect.GetBoidWithinRad(this, pos, 5f, false, -1))
                {
                    center += boid.pos;
                    count++;
                }
                if (count == 0)
                    return;

                center /= count;

                Vector2 delta = pos - center;
                //dir = Vector3.Slerp(dir, delta.normalized, 0.325f);

                Vel += delta.normalized * Mathf.Lerp(10f, 0.1f, delta.magnitude / 5f);
            }

            void Cohesion()
            {
                int count = 0;
                Vector2 center = Vector2.zero;
                foreach (var boid in effect.GetBoidWithinRad(this, pos, 80f))
                {
                    center += boid.pos;
                    count++;
                }
                if (count == 0)
                    return;

                center /= count;

                Vector2 delta = center - pos;
                if (delta.magnitude > 10f)
                {
                    //Vel += (delta).normalized * Mathf.Clamp((10f / (delta.magnitude + 1f)), 0, 0.75f);
                    dir = Vector3.Slerp(dir, delta.normalized, 0.035f);
                }


            }

            void Alignment()
            {
                int count = 0;
                Vector2 alignmentVel = Vector2.zero;
                foreach (var boid in effect.GetBoidWithinRad(this, pos, 80f))
                {
                    alignmentVel += boid.dir;
                    count++;
                }
                if (count == 0)
                    return;
                alignmentVel /= count;

                dir = Vector3.Slerp(dir, alignmentVel.normalized, 0.1f);
            }

            bool TerrainCheck()
            {
                bool result = false;
                if (effect.room.aimap.getTerrainProximity(pos) < 5)
                {
                    result = true;
                    IntVector2 tilePosition = effect.room.GetTilePosition(pos);
                    Vector2 vector3 = new Vector2(0f, 0f);
                    for (int j = 0; j < 4; j++)
                    {
                        if (!effect.room.GetTile(tilePosition + Custom.fourDirections[j]).Solid && !effect.room.aimap.getAItile(tilePosition + Custom.fourDirections[j]).narrowSpace)
                        {
                            float num9 = 0f;
                            for (int k = 0; k < 4; k++)
                            {
                                num9 += (float)effect.room.aimap.getTerrainProximity(tilePosition + Custom.fourDirections[j] + Custom.fourDirections[k]);
                            }
                            vector3 += Custom.fourDirections[j].ToVector2() * num9;
                        }
                    }
                    dir = Vector2.Lerp(dir, vector3.normalized * 2f, 0.5f * Mathf.Pow(Mathf.InverseLerp(5f, 1f, (float)effect.room.aimap.getTerrainProximity(pos)), 0.25f));
                }
                return result;
            }

            void ClampInRoom(ref IntVector2 pos)
            {
                pos.x = Mathf.Clamp(pos.x, 0, effect.room.Width - 1);
                pos.y = Mathf.Clamp(pos.y, 0, effect.room.Height - 1);
            }

            float ManhatonDistance(Vector2 a, Vector2 b)
            {
                return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
            }
        }
    }
}
