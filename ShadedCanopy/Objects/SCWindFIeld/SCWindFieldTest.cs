using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShadedCanopy.Objects.SCWindFIeld
{
    internal class SCWindFieldTest : SCWindField
    {
        static bool debug = false;


        Vector2[,] windGrid;//网格点在tile中心
        int[,] multiplier;

        DebugSprite[,] debugVisSprite;

        public SCWindFieldTest(Room room, int collisionLayer) : base(room, collisionLayer)
        {
            this.room = room;
            windGrid = new Vector2[room.Width, room.Height];
            multiplier = new int[room.Width, room.Height];

            for (int x = 0; x < room.Width; x++)
            {
                for (int y = 0; y < room.Height; y++)
                {
                    windGrid[x, y] = Vector2.zero;
                    multiplier[x, y] = room.GetTile(x, y).Solid ? 0 : 1;
                }
            }

            if(debug)
            {
                debugVisSprite = new DebugSprite[room.TileWidth, room.TileHeight];
                for (int i = 0; i < room.TileWidth; i++)
                {
                    for (int j = 0; j < room.TileHeight; j++)
                    {
                        debugVisSprite[i, j] = new DebugSprite(room.MiddleOfTile(i, j), new FSprite("pixel"), room);
                        debugVisSprite[i, j].sprite.scaleX = 2f;
                        debugVisSprite[i, j].sprite.scaleY = 2f;
                        debugVisSprite[i, j].sprite.color = Color.red;
                        room.AddObject(debugVisSprite[i, j]);
                    }
                }
            }
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            foreach (PhysicalObject pobj in room.physicalObjects[1])
            {
                if (pobj.CollideWithObjects || pobj.CollideWithTerrain)
                {
                    foreach (BodyChunk bc in pobj.bodyChunks)
                    {
                        if (bc.collideWithObjects || bc.collideWithTerrain)
                        {
                            Vector2 reliableVel = ((bc.pos - bc.lastPos) + (bc.lastPos - bc.lastLastPos)) * 0.5f * 0.1f;
                            ApplyVel2Grid(bc.pos , reliableVel);
                        }
                    }
                }
            }

            if (debug)
            {
                for (int x = 0; x < room.Width; x++)
                {
                    for (int y = 0; y < room.Height; y++)
                    {
                        Vector2 vel = GetVelGrid(x, y);

                        Vector2 pos = room.MiddleOfTile(x, y) + vel * 2f / 2f;
                        debugVisSprite[x, y].pos = (pos);
                        debugVisSprite[x, y].sprite.scaleY = 2f + vel.magnitude * 2f;
                        debugVisSprite[x, y].sprite.rotation = Custom.VecToDeg(vel.normalized);
                        debugVisSprite[x, y].sprite.color = Color.Lerp(Color.cyan, Color.red, vel.magnitude / 5f);
                    }
                }
            }


            DecayWind();
            FlowWind();
        }

        void DecayWind()
        {
            for(int x = 0; x < room.Width; x++)
            {
                for (int y = 0; y < room.Height; y++)
                {
                    windGrid[x, y] *= 0.985f;

                    Vector2 avgWind = Vector2.zero;

                    foreach(var dir in Custom.eightDirections)
                    {
                        avgWind += GetVelGrid(x + dir.x, y + dir.y);
                    }
                    avgWind /= 8f;
                    avgWind *= 0.95f;

                    windGrid[x, y] = Vector2.Lerp(windGrid[x, y], avgWind, Mathf.InverseLerp(0f, 5f, windGrid[x,y].magnitude)) * multiplier[x, y];
                }
            }   
        }

        void FlowWind()
        {
            for (int x = 0; x < room.Width; x++)
            {
                for (int y = 0; y < room.Height; y++)
                {
                    Vector2 targetPos = windGrid[x, y] + room.MiddleOfTile(x, y);
                    ApplyVel2Grid(targetPos, windGrid[x, y] * 0.8f);
                }
            }
        }

        void ApplyVel2Grid(Vector2 roomPos, Vector2 vel)
        {
            IntVector2 bottomLeftGridCoord = room.GetTilePosition(roomPos -  new Vector2(10f, 10f));
            float x = (roomPos.x - 10f - bottomLeftGridCoord.x * 20f) / 20f;
            float y = (roomPos.y - 10f - bottomLeftGridCoord.y * 20f) / 20f;

            float max = vel.magnitude;

            AccumulatetVelGrid(bottomLeftGridCoord.x, bottomLeftGridCoord.y, new Vector2(vel.x * (1f - x), vel.y * (1f - y)), 3f, max);
            AccumulatetVelGrid(bottomLeftGridCoord.x + 1, bottomLeftGridCoord.y, new Vector2(vel.x * x, vel.y * (1f - y)), 3f, max);
            AccumulatetVelGrid(bottomLeftGridCoord.x, bottomLeftGridCoord.y + 1, new Vector2(vel.x * (1f - x), vel.y * y), 3f, max);
            AccumulatetVelGrid(bottomLeftGridCoord.x + 1, bottomLeftGridCoord.y + 1, new Vector2(vel.x * x, vel.y * y), 3f, max);
        }

        void AccumulatetVelGrid(int x, int y, Vector2 val, float step, float max)
        {
            if (x < 0 || x >= windGrid.GetLength(0))
                return;
            if (y < 0 || y >= windGrid.GetLength(1))
                return;
            windGrid[x, y] = Vector2.ClampMagnitude(windGrid[x,y] + Vector2.ClampMagnitude(val * multiplier[x, y], step), Mathf.Max(max, windGrid[x, y].magnitude)); 
        }

        Vector2 GetVelGrid(int x, int y)
        {
            if (x < 0 || x >= windGrid.GetLength(0))
                return Vector2.zero;
            if (y < 0 || y >= windGrid.GetLength(1))
                return Vector2.zero;
            return windGrid[x, y];
        }

        void ApplyBodyChunkVel(BodyChunk bc)
        {
            ApplyVel2Grid(bc.pos, bc.vel);
        }

        public override Vector2 GetWind(Vector2 pos, int latency)
        {
            IntVector2 bottomLeftGridCoord = room.GetTilePosition(pos - new Vector2(10f, 10f));
            float x = (pos.x - 10f - bottomLeftGridCoord.x * 20f) / 20f;
            float y = (pos.y - 10f - bottomLeftGridCoord.y * 20f) / 20f;


            Vector2 a = Vector2.Lerp(GetVelGrid(bottomLeftGridCoord.x, bottomLeftGridCoord.y), GetVelGrid(bottomLeftGridCoord.x + 1, bottomLeftGridCoord.y), x);
            Vector2 b = Vector2.Lerp(GetVelGrid(bottomLeftGridCoord.x, bottomLeftGridCoord.y + 1), GetVelGrid(bottomLeftGridCoord.x + 1, bottomLeftGridCoord.y + 1), x);

            return Vector2.Lerp(a, b, y);
        }
    }
}
