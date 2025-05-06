using RWCustom;
using ShadedCanopy.FlashingEffect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using UnityEngine;
using Watcher;

namespace ShadedCanopy.ShimmerSlugcat
{
    public class PlayerHooks
    {
        public static ConditionalWeakTable<Player, ShimmerPlayerModule> shimmerPlayer = new ConditionalWeakTable<Player, ShimmerPlayerModule>();
        public static ConditionalWeakTable<AbstractCreature, FlashedVictim> flashedVictim = new ConditionalWeakTable<AbstractCreature, FlashedVictim>();

        public static void Hooks()
        {
            On.Player.ctor += Player_ctor;
            On.Player.Update += Player_Update;
            On.Player.BiteEdibleObject += Player_BiteEdibleObject;
            On.Room.Update += Room_Update;
        }

        private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            if (self.slugcatStats.name == SCEnums.SlugStateName.Shimmer)
            {
                shimmerPlayer.Add(self, new ShimmerPlayerModule());
            }
        }

        private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);
            if (shimmerPlayer.TryGetValue(self, out var module))
            {
                module.playerGrabbed = GrabbedCondition(self);

                float tik = 0.5f;
                if (module.lightUp)
                {
                    if (self.dead || module.energy <= 0f) module.lightUp = false;

                    if (module.energy >= tik)
                        module.energy -= tik;

                    if (self.room != null)
                    {
                        if (module.lightUpProgress < 1f)
                        {
                            module.lightUpProgress += 0.025f;
                        }

                        if ((module.lightSource == null || module.lightSource.slatedForDeletetion))
                        {
                            module.lightSource = new LightSource(self.firstChunk.pos, false, Color.white, self, false)
                            {
                                rad = 5f,
                                alpha = 0.05f
                            };
                            self.room.AddObject(module.lightSource);
                        }
                    }
                }
                else
                {
                    if (module.lightUpProgress > 0.05f)
                    {
                        module.lightUpProgress -= 0.05f;
                    }
                    else module.lightUpProgress = 0f;

                    if (module.lightUpProgress <= 0f && module.lightSource != null)
                    {
                        module.lightSource.Destroy();
                    }
                }

                if (self.room != null && self.Consious)
                {
                    if (self.input[1].spec && !self.input[0].spec)
                    {
                        //爆闪
                        if (self.input[0].pckp)
                        {
                            if (module.energy >= ShimmerPlayerModule.maxEnergy)
                            {
                                self.room.AddObject(new FlashingEffectTest(self.room, self.firstChunk));
                                ShimmerFlash(self);
                                module.energy = 0f;
                            }
                        }
                        //普通发光
                        else
                        {
                            bool flag = !module.lightUp;
                            module.lightUp = flag;
                        }
                    }


                    if (self.input[0].pckp)
                    {
                        module.pressPickupCount++;
                        if (module.pressPickupCount >= 40f)
                        {
                            //食用手里的发光非食物物品（如果有的话
                            EatGlowingItemUpdate(self, module);
                        }
                    }
                    else module.pressPickupCount = 0;
                }

                if (module.lightSource != null)
                {
                    module.lightSource.setRad = Mathf.Lerp(5f, 600f, module.lightUpProgress);
                    module.lightSource.alpha = 0.05f + 0.95f * module.lightUpProgress;
                    module.lightSource.pos = self.DangerPos;
                }

                //测试用，按Q补满光能
                if (Input.GetKey(KeyCode.Q))
                {
                    module.energy = ShimmerPlayerModule.maxEnergy;
                }

            }
        }

        //使被爆闪恐吓的生物致盲并试图逃离
        private static void Room_Update(On.Room.orig_Update orig, Room self)
        {
            orig(self);
            try
            {
                for (int i = 0; i < self.abstractRoom.creatures.Count; i++)
                {
                    if (self.abstractRoom.creatures[i].state.alive && flashedVictim.TryGetValue(self.abstractRoom.creatures[i], out var victim))
                    {
                        AbstractCreature absCrit = self.abstractRoom.creatures[i];
                        if(absCrit.abstractAI == null || absCrit.abstractAI.RealAI == null) continue;
                        if (victim.panic > 0f)
                        {
                            if (!victim.alreadyPanic && !UseOffScreenExit(absCrit.creatureTemplate.type))
                            {
                                victim.panicPoint = absCrit.abstractAI.RealAI.threatTracker?.AddThreatPoint(null, self.GetWorldCoordinate(victim.panicSourcePos), 1f);
                                int num = absCrit.abstractAI.RealAI.threatTracker.FindMostAttractiveExit();

                                if (num > -1 && num < self.abstractRoom.nodes.Length && self.abstractRoom.nodes[num].type == AbstractRoomNode.Type.Exit)
                                {
                                    int num2 = self.world.GetAbstractRoom(self.abstractRoom.connections[num]).ExitIndex(self.abstractRoom.index);
                                    if (num2 > -1)
                                    {
                                        absCrit.abstractAI.MigrateTo(new WorldCoordinate(self.abstractRoom.connections[num], -1, -1, num2));
                                    }
                                }

                                if (absCrit.creatureTemplate.type == CreatureTemplate.Type.GarbageWorm && absCrit.realizedCreature != null)
                                {
                                    (absCrit.abstractAI.RealAI as GarbageWormAI).stress = 1f;
                                    (absCrit.realizedCreature as GarbageWorm).Retract();
                                }

                                victim.alreadyPanic = true;
                            }

                            if ((absCrit.creatureTemplate.type.value.Contains("Vulture") && absCrit.creatureTemplate.type != CreatureTemplate.Type.VultureGrub)
                                ||absCrit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.BigMoth)
                            {
                                List<UtilityComparer.UtilityTracker> trackers = absCrit.abstractAI.RealAI.utilityComparer.uTrackers;

                                if(absCrit.creatureTemplate.type == WatcherEnums.CreatureTemplateType.BigMoth)
                                {
                                    (absCrit.abstractAI.RealAI as BigMothAI).behavior = BigMothAI.Behavior.EscapeRain;
                                    (absCrit.abstractAI.RealAI as BigMothAI).focusCreature = null;

                                }
                                else
                                {
                                    for (int k = 0; k < trackers.Count; k++)
                                    {
                                        if (trackers[k].module is VultureAI.DisencouragedTracker || trackers[k].module is StuckTracker)
                                        {
                                            trackers[k].smoothedUtility = 1f;
                                        }
                                        else
                                        {
                                            trackers[k].smoothedUtility = 0f;
                                        }
                                    }
                                    (absCrit.abstractAI.RealAI as VultureAI).focusCreature = null;
                                    (absCrit.abstractAI.RealAI as VultureAI).behavior = VultureAI.Behavior.Disencouraged;
                                }

                                if (absCrit.abstractAI.RealAI.denFinder.GetDenPosition() != null)
                                {
                                    absCrit.abstractAI.RealAI.creature.abstractAI.SetDestination(absCrit.abstractAI.RealAI.denFinder.GetDenPosition().Value);
                                }
                            }

                        }
                        victim.panic--;

                        if (victim.alreadyPanic && victim.panic <= 0f && victim.panicPoint != null)
                        {
                            victim.victim.abstractAI.RealAI.threatTracker?.RemoveThreatPoint(victim.panicPoint);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }

        //通过发光食物补充光能
        private static void Player_BiteEdibleObject(On.Player.orig_BiteEdibleObject orig, Player self, bool eu)
        {
            if (shimmerPlayer.TryGetValue(self, out var module))
            {
                module.pressPickupCount = 0f;
                for (int i = 0; i < 2; i++)
                {
                    if (self.grasps[i] != null && self.grasps[i].grabbed is IPlayerEdible && (self.grasps[i].grabbed as IPlayerEdible).Edible)
                    {
                        module.energy = Mathf.Min(ShimmerPlayerModule.maxEnergy, EnergyFromFood(self.grasps[i].grabbed.abstractPhysicalObject.type));
                        break;
                    }
                }
            }
            orig(self, eu);
        }

        public static bool UseOffScreenExit(CreatureTemplate.Type type)
        {
            if (type.value.Contains("Vulture") && type != CreatureTemplate.Type.VultureGrub)
            {
                return true;
            }
            if (type == WatcherEnums.CreatureTemplateType.BigMoth)
            {
                return true;
            }
            return false;
        }

        //通过食用发光非食物物品补充光能
        public static void EatGlowingItemUpdate(Player self, ShimmerPlayerModule module)
        {
            if (self.grasps != null && self.grasps.Length > 0)
            {
                for (int i = 0; i < self.grasps.Length; i++)
                {
                    if (self.grasps[i] != null && self.grasps[i].grabbed != null)
                    {
                        PhysicalObject physicalObject = self.grasps[i].grabbed;

                        if (physicalObject is IPlayerEdible && self.FoodInStomach < self.MaxFoodInStomach)
                        {
                            break;
                        }
                        else if (GlowingNonFoodItem(physicalObject.abstractPhysicalObject.type) && module.energy < ShimmerPlayerModule.maxEnergy)
                        {
                            if (self.graphicsModule != null)
                            {
                                (self.graphicsModule as PlayerGraphics).BiteFly(i);
                            }

                            self.ReleaseGrasp(i);
                            physicalObject.Destroy();
                            module.energy = Mathf.Min(ShimmerPlayerModule.maxEnergy, module.energy + EnergyFromFood(physicalObject.abstractPhysicalObject.type));
                        }
                    }
                }
            }
        }

        public static void ShimmerFlash(Player self)
        {
            for (int i = 0; i < self.room.abstractRoom.creatures.Count; i++)
            {
                if (self.room.abstractRoom.creatures[i].state.alive && self.room.abstractRoom.creatures[i].realizedCreature != null)
                {
                    Creature creature = self.room.abstractRoom.creatures[i].realizedCreature;
                    if (self.room.ViewedByAnyCamera(creature.firstChunk.pos, 0f)
                        || Custom.DistNoSqrt(self.DangerPos, creature.firstChunk.pos) <= Mathf.Pow(ShimmerPlayerModule.flashRangeForOffScreen, 2f)
                        || CanBeFlashed(creature.Template.type))
                    {
                        if (!flashedVictim.TryGetValue(creature.abstractCreature, out var module))
                        {
                            flashedVictim.Add(creature.abstractCreature, new FlashedVictim(creature.abstractCreature, self.DangerPos));
                        }
                        
                        flashedVictim.TryGetValue(creature.abstractCreature, out var victim);
                        victim.panic = victim.maxPanic;
                        creature.Blind((int)(0.5f * victim.maxPanic));

                    }
                }
            }
        }

        //判断玩家是否被捕食者抓住
        public static bool GrabbedCondition(Player player)
        {
            if (!player.Consious && player.grabbedBy.Count > 0)
            {
                return true;
            }
            if (ModManager.JollyCoop && player.room != null && player.room.game != null && player.room.game.Players.Count > 1)
            {
                for (int i = 0; i < player.room.game.Players.Count; i++)
                {
                    if (player.room.game.Players[i].realizedCreature == null) continue;
                    Player player2 = player.room.game.Players[i].realizedCreature as Player;
                    if (player != player2 && player2.slugOnBack != null && player2.slugOnBack.slugcat == player)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static float EnergyFromFood(AbstractPhysicalObject.AbstractObjectType type)
        {
            if (type == AbstractPhysicalObject.AbstractObjectType.FlareBomb)
            {
                return 150f;
            }
            else if (type == AbstractPhysicalObject.AbstractObjectType.SlimeMold)
            {
                return 60f;
            }
            else if (type == DLCSharedEnums.AbstractObjectType.GlowWeed)
            {
                return 75f;
            }
            else if (type.value.Contains("OracleSwarmer"))
            {
                return 100f;
            }
            return 0f;
        }

        public static bool GlowingNonFoodItem(AbstractPhysicalObject.AbstractObjectType type)
        {
            if (type == AbstractPhysicalObject.AbstractObjectType.FlareBomb)
            {
                return true;
            }
            return false;
        }

        public static bool CanBeFlashed(CreatureTemplate.Type type)
        {
            if (type.value.Contains("LongLegs") || type == CreatureTemplate.Type.BlackLizard)
            {
                return false;
            }
            return true;
        }

        public class ShimmerPlayerModule
        {
            public static float maxEnergy = 300f;
            public static float flashRangeForOffScreen = 600f;
            public float energy = maxEnergy;
            public float lightUpProgress;
            public float pressPickupCount;
            public bool lightUp;
            public bool playerGrabbed;
            public LightSource lightSource;
            public ShimmerPlayerModule()
            {

            }
        }

        public class FlashedVictim
        {
            public AbstractCreature victim;
            public float panic;
            public float maxPanic;
            public Vector2 panicSourcePos;
            public bool alreadyPanic;
            public ThreatTracker.ThreatPoint panicPoint;

            public FlashedVictim(AbstractCreature abstractCreature, Vector2 panicSource, float panic = 280f)
            {
                victim = abstractCreature;
                this.panic = panic;
                this.maxPanic = panic;
                this.panicSourcePos = panicSource;
            }
        }
    }
}
