using BepInEx;
using IL.RWCustom;
using SCUtils.SCDevTools.NodeTreeManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

namespace ShadedCanopy.Objects.SCBlinkingLawn
{
    internal class SCWindField: UpdatableAndDeletable
    {
        public static readonly int HistoryLength = 40;
        
        public Vector2[,,] wind;
        int collisionLayer;
        bool showVis;
        DebugSprite[,] debugVisSprite;
        public SCWindField(Room room, int collisionLayer) : base()
        {
            this.room = room;
            wind = new Vector2[room.TileWidth, room.TileHeight, HistoryLength];
            SCPlugin.Logger.LogInfo($"Initialized wind field with size {room.TileWidth}x{room.TileHeight} and history length {HistoryLength}");
            showVis = SCWindFieldProperty.Value.showVis != 0;
            this.collisionLayer = collisionLayer;
            if (showVis)
            {
                debugVisSprite = new DebugSprite[room.TileWidth, room.TileHeight];
                for (int i = 0; i < room.TileWidth; i++)
                {
                    for (int j = 0; j < room.TileHeight; j++)
                    {
                        debugVisSprite[i, j] = new DebugSprite(room.MiddleOfTile(i, j), new FSprite("pixel"), room);
                        debugVisSprite[i, j].sprite.scaleX = 2f;
                        debugVisSprite[i, j].sprite.scaleY = 2f;
                        debugVisSprite[i, j].sprite.color = new Color(0f, 1f, 0f, 0.5f);
                        room.AddObject(debugVisSprite[i, j]);
                    }
                }
            }
        }

        public static Vector2 StirWind(Vector2 current, BodyChunk bc, float cov)
        {
            Vector2 posV = bc.pos - bc.lastPos;
            Vector2 realV = (posV.sqrMagnitude > bc.vel.sqrMagnitude) ? bc.vel : posV;
            return Vector2.Lerp(current, realV, SCWindFieldProperty.Value.AirViscosity * bc.mass * cov);
        }
        public static List<ValueTuple<RWCustom.IntVector2, float>> TilesCoveredByCircle(Vector2 center, float radius, Room room) // 计算圆形覆盖的tile，并且估计覆盖率
        {
            List<ValueTuple<RWCustom.IntVector2, float>> result = new();
            Vector2 cirMin = center - new Vector2(radius, radius);
            Vector2 cirMax = center + new Vector2(radius, radius);
            RWCustom.IntVector2 tileMin = room.GetTilePosition(cirMin), tileMax = room.GetTilePosition(cirMax);
            for (int i = tileMin.x; i <= tileMax.x; i++)
            {
                for (int j = tileMin.y; j <= tileMax.y; j++)
                {
                    if (i >= 0 && i < room.TileWidth && j >= 0 && j < room.TileHeight)
                    {
                        Vector2 tileCenter = room.MiddleOfTile(i, j);
                        float dist = Vector2.Distance(tileCenter, center);
                        if (dist + 10 < radius)
                        {
                            result.Add((new RWCustom.IntVector2(i, j), 1f));
                        }
                        else if (dist - 10 < radius)
                        {
                            float cov = Mathf.Clamp01((radius - dist + 10) / 20f);
                            result.Add((new RWCustom.IntVector2(i, j), cov));
                        }
                    }
                }
            }
            return result;
        }
        public override void Update(bool eu)
        {
            base.Update(eu);
            for (int k = HistoryLength - 1; k > 0; k--)
            {
                for (int i = 0; i < room.TileWidth; i++)
                {
                    for (int j = 0; j < room.TileHeight; j++)
                    {
                        wind[i, j, k] = wind[i, j, k - 1];
                    }
                }
            }
            for (int i = 0; i < room.TileWidth; i++)
            {
                for (int j = 0; j < room.TileHeight; j++)
                {
                    wind[i, j, 0] *= SCWindFieldProperty.Value.WindDecay;
                }
            }
            foreach (PhysicalObject pobj in room.physicalObjects[collisionLayer])
            {
                if (pobj.CollideWithObjects || pobj.CollideWithTerrain)
                {
                    foreach (BodyChunk bc in pobj.bodyChunks)
                    {
                        if (bc.collideWithObjects || bc.collideWithTerrain)
                        {
                            List<ValueTuple<RWCustom.IntVector2, float>> tiles = TilesCoveredByCircle(bc.pos, bc.rad, room);
                            foreach (var tile in tiles)
                            {
                                RWCustom.IntVector2 pos = tile.Item1;
                                float cov = tile.Item2;
                                if ((bc.owner is Player player) && player.animation == Player.AnimationIndex.BellySlide)
                                {
                                    cov *= 2f;
                                }
                                wind[pos.x, pos.y, 0] = StirWind(wind[pos.x, pos.y, 0], bc, cov);
                            }
                        }
                    }
                }
            }
            if (showVis)
            {
                for (int i = 0; i < room.TileWidth; i++)
                {
                    for (int j = 0; j < room.TileHeight; j++)
                    {
                        Vector2 w = wind[i, j, 0];
                        debugVisSprite[i, j].sprite.rotation = Mathf.Atan2(w.x, w.y) * Mathf.Rad2Deg;
                        float wLengh = w.magnitude * 10;

                        debugVisSprite[i, j].sprite.scaleY = wLengh + 2;
                        debugVisSprite[i, j].sprite.anchorY = 1 / (wLengh + 2);
                    }
                }
            }
        }

        public Vector2 GetWind(Vector2 pos, int latency)
        {
            if (latency >= HistoryLength)
            {
                latency = HistoryLength - 1;
            }
            RWCustom.IntVector2 tilePos = room.GetTilePosition(pos);
            if (tilePos.x >= 0 && tilePos.x < room.TileWidth && tilePos.y >= 0 && tilePos.y < room.TileHeight)
            {
                return wind[tilePos.x, tilePos.y, latency];
            }
            else
            {
                return Vector2.zero;
            }
        }
    }

}
