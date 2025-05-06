
using Expedition;
using MoreSlugcats;
using RWCustom;
using ShadedCanopy.Effect.SCSuperStructureEffect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Watcher;
using static ShadedCanopy.FlashingEffect.FlashingEffectManager;

namespace ShadedCanopy.FlashingEffect
{
    internal static partial class FlashingEffectManager
    {
        public static ComputeShader LevelMaskCS { get; private set; }
        public static Shader FlashBangShader { get; private set; }
        public static Shader DeadlyLightShader { get; private set; }
        public static int CreateLightMaskKernalIndex { get; private set; }
        public static int RadialBlurKernalIndex { get; private set; }
        public static int PenumbraKernalIndex { get; private set; }
        public static int StretchPenumbraKernalIndex { get; private set; }
        public static int FinalizeKernalIndex { get; private set; }

        public static void Init()
        {
            HooksOn();
            LoadAssets();
        }

        public static void LoadAssets()
        {
            string path = AssetManager.ResolveFilePath("AssetBundles/shadedcanopybundle");
            AssetBundle shadedcanopybundle = AssetBundle.LoadFromFile(path);

            LevelMaskCS = shadedcanopybundle.LoadAsset<ComputeShader>("assets/myshader/levellightmask.compute");
            CreateLightMaskKernalIndex = LevelMaskCS.FindKernel("CreateLightMask");
            RadialBlurKernalIndex = LevelMaskCS.FindKernel("RadialBlur");
            PenumbraKernalIndex = LevelMaskCS.FindKernel("Penumbra");
            FinalizeKernalIndex = LevelMaskCS.FindKernel("FinalizeResult");
            StretchPenumbraKernalIndex = LevelMaskCS.FindKernel("StretchPenumbra");

            FlashBangShader = shadedcanopybundle.LoadAsset<Shader>("assets/myshader/flashbang.shader");
            DeadlyLightShader = shadedcanopybundle.LoadAsset<Shader>("assets/myshader/deadlylight.shader");

            Custom.rainWorld.Shaders.Add("DeadlyLight", FShader.CreateShader("SC_DeadlyLight", DeadlyLightShader));
            Custom.rainWorld.Shaders.Add("DeadlyLight_ForegroundGrab", FShader.CreateShader("DeadlyLight_ForegroundGrab", shadedcanopybundle.LoadAsset<Shader>("assets/myshader/foregroundgrab.shader")));

            shadedcanopybundle.Unload(false);
            UnityEngine.Object.Destroy(shadedcanopybundle);
        }

        public static void HooksOn()
        {
            //On.Player.Jump += Player_Jump;
            On.Room.NowViewed += Room_NowViewed;
        }

        private static void Room_NowViewed(On.Room.orig_NowViewed orig, Room self)
        {
            if (self.snowObject == null)
            {
                Shader.DisableKeyword("SNOW_ON");
            }
            Shader.SetGlobalFloat(RainWorld.ShadPropRimFix, 0f);
            if ((self.world.region != null && self.world.region.name == "HR") || self.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.LavaSurface) != null)
            {
                Shader.EnableKeyword("HR");
            }
            else
            {
                Shader.DisableKeyword("HR");
            }
            Shader.DisableKeyword("URBANLIFE");
            Shader.SetGlobalColor("_boxWormColor", BoxWormGraphics.BaseColor(self));
            if (self.fsRipple == null)
            {
                if (self.game.cameras[0].lastRippleState)
                {
                    self.AddObject(self.fsRipple = new RippleFullScreen());
                }
            }
            else if (!self.game.cameras[0].lastRippleState)
            {
                self.fsRipple.Destroy();
                self.fsRipple = null;
            }
            for (int i = 0; i < self.roomSettings.effects.Count; i++)
            {
                if (self.roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.GreenSparks)
                {
                    int num = 0;
                    while ((float)num < (float)(self.TileWidth * self.TileHeight) * self.roomSettings.effects[i].amount / 50f)
                    {
                        Vector2 vector = new Vector2(UnityEngine.Random.value * self.PixelWidth, UnityEngine.Random.value * self.PixelHeight);
                        if (!self.GetTile(vector).Solid && Mathf.Pow(UnityEngine.Random.value, 1f - self.roomSettings.effects[i].amount) > (float)(self.readyForAI ? self.aimap.getTerrainProximity(vector) : 5) * 0.05f && self.roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.GreenSparks)
                        {
                            self.AddObject(new GreenSparks.GreenSpark(vector));
                        }
                        num++;
                    }
                }
                else if (self.roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.ZeroGSpecks)
                {
                    int num2 = 0;
                    while ((float)num2 < 1000f * self.roomSettings.effects[i].amount)
                    {
                        self.AddObject(new BlinkSpeck());
                        num2++;
                    }
                }
                else if (self.roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.CorruptionSpores)
                {
                    int num3 = 0;
                    while ((float)num3 < 200f * self.roomSettings.effects[i].amount)
                    {
                        self.AddObject(new CorruptionSpore());
                        num3++;
                    }
                }
                else if (self.roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.SuperStructureProjector)
                {
                    //self.AddObject(new SuperStructureProjector(self, self.roomSettings.effects[i]));
                    self.AddObject(new SCSuperStructureProj(self));
                }
                else if (self.roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.ProjectedScanLines)
                {
                    //self.AddObject(new ProjectedScanLines(self, self.roomSettings.effects[i]));
                }
                else if (self.roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.AboveCloudsView)
                {
                    Shader.SetGlobalFloat(RainWorld.ShadPropRimFix, 1f);
                }
                else if (self.roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.RoofTopView)
                {
                    Shader.SetGlobalFloat(RainWorld.ShadPropRimFix, 1f);
                }
                else if (ModManager.Watcher && (self.roomSettings.effects[i].type == WatcherEnums.RoomEffectType.OuterRimView || self.roomSettings.effects[i].type == WatcherEnums.RoomEffectType.AncientUrbanView || self.roomSettings.effects[i].type == WatcherEnums.RoomEffectType.InnerOuterRimView))
                {
                    Shader.SetGlobalFloat(RainWorld.ShadPropRimFix, 1f);
                }
                else if (self.roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.PinkSky)
                {
                    Shader.SetGlobalFloat(RainWorld.ShadPropRimFix, 1f);
                }
                else if (self.roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.FairyParticles)
                {
                    int num4 = 0;
                    while ((float)num4 < 500f * self.roomSettings.effects[i].amount)
                    {
                        int num5 = ((UnityEngine.Random.value < 0.5f) ? 3 : 4);
                        FairyParticle fairyParticle = new FairyParticle((float)UnityEngine.Random.Range(0, 360), num5, 60f, 180f, 40f, 100f, 5f, 30f);
                        self.AddObject(fairyParticle);
                        num4++;
                    }
                    for (int j = 0; j < self.roomSettings.placedObjects.Count; j++)
                    {
                        if (self.roomSettings.placedObjects[j].type == PlacedObject.Type.FairyParticleSettings)
                        {
                            (self.roomSettings.placedObjects[j].data as PlacedObject.FairyParticleData).Apply(self);
                            break;
                        }
                    }
                }
                else if (self.roomSettings.effects[i].type == RoomSettings.RoomEffect.Type.DayNight)
                {
                    for (int k = 0; k < self.roomSettings.placedObjects.Count; k++)
                    {
                        if (self.roomSettings.placedObjects[k].type == PlacedObject.Type.DayNightSettings)
                        {
                            (self.roomSettings.placedObjects[k].data as PlacedObject.DayNightData).Apply(self);
                            break;
                        }
                    }
                }
            }
            if (ModManager.Expedition && self.game.rainWorld.ExpeditionMode && ExpeditionGame.activeUnlocks.Contains("bur-blinded"))
            {
                new PlacedObject.DayNightData(null)
                {
                    nightPalette = 10
                }.Apply(self);
                if (self.game.cameras[0].currentPalette.darkness < 0.8f)
                {
                    self.game.cameras[0].effect_dayNight = 1f;
                    self.game.cameras[0].currentPalette.darkness = 0.8f;
                }
                self.roomSettings.Clouds = 0.875f;
                self.world.rainCycle.sunDownStartTime = 0;
                self.world.rainCycle.dayNightCounter = 3750;
            }
            for (int l = 0; l < self.physicalObjects.Length; l++)
            {
                for (int m = 0; m < self.physicalObjects[l].Count; m++)
                {
                    self.physicalObjects[l][m].InitiateGraphicsModule();
                    if (self.physicalObjects[l][m].graphicsModule != null && !self.drawableObjects.Contains(self.physicalObjects[l][m].graphicsModule))
                    {
                        self.drawableObjects.Add(self.physicalObjects[l][m].graphicsModule);
                    }
                }
            }
            if (self.world.worldGhost != null)
            {
                for (int n = 0; n < self.cameraPositions.Length; n++)
                {
                    if (self.world.worldGhost.GhostMode(self, n) > 0f && (!ModManager.Watcher || self.world.worldGhost.ghostID != WatcherEnums.GhostID.SpinningTop))
                    {
                        self.AddObject(new GoldFlakes(self));
                        break;
                    }
                }
            }
            if (self.insectCoordinator != null)
            {
                self.insectCoordinator.NowViewed();
            }
        }

        private static void Player_Jump(On.Player.orig_Jump orig, Player self)
        {
            orig.Invoke(self);
            if (self.room.updateList.Where((u) => u is SCBoids).Count() > 0)
                return;
            //self.room.AddObject(new LevelMaskTest(self.room, self.firstChunk));
            self.room.AddObject(new FlashingEffectTest(self.room, self.firstChunk));
            //self.room.AddObject(new BoidsEffect(self.room, 300));
        }
    }

    internal static partial class FlashingEffectManager
    {
        static int maxLoadRoomGeometry = 5;
        static Dictionary<string, Texture2D> roomGeometryTexs = new Dictionary<string, Texture2D>();
        static Dictionary<Vector2Int, RenderTexture> interTex = new Dictionary<Vector2Int, RenderTexture>();
        static List<string> lastLoadedRoom = new List<string>();

        /// <summary> 创建当前房间的几何信息贴图并自动托管。一共只会暂存<see cref="FlashingEffectManager.maxLoadRoomGeometry"/>张贴图，超出数量的贴图将会被销毁 </summary>
        public static Texture2D TryLoadLevelGeometryTex(Room room)
        {
            if(roomGeometryTexs.ContainsKey(room.abstractRoom.name))
            {
                lastLoadedRoom.Remove(room.abstractRoom.name);
                lastLoadedRoom.Add(room.abstractRoom.name);
                return roomGeometryTexs[room.abstractRoom.name];
            }
            while(lastLoadedRoom.Count >= maxLoadRoomGeometry)
            {
                string roomsToRemove = lastLoadedRoom.Pop();
                UnityEngine.Object.Destroy(roomGeometryTexs[roomsToRemove]);
                roomGeometryTexs.Remove(roomsToRemove);
            }

            Texture2D geometryTex = new Texture2D(room.Width, room.Height)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };

            for(int x = 0; x < room.Width; x++)
            {
                for(int y = 0; y < room.Height; y++)
                {
                    if (room.GetTile(x, y).Solid)
                        geometryTex.SetPixel(x, y, Color.red);
                    else if (room.GetTile(x, y).Terrain == Room.Tile.TerrainType.Slope)
                        geometryTex.SetPixel(x, y, Color.green);
                    else
                        geometryTex.SetPixel(x, y, Color.black);
                }
            }
            geometryTex.Apply();
            roomGeometryTexs.Add(room.abstractRoom.name, geometryTex);

            return geometryTex;
        }

        /// <summary> 创建LevelMask的RT，并自动应用一次计算 </summary>
        /// <param name="room"></param>
        /// <param name="lightSourceRoomPos"></param>
        /// <param name="pixPerTile">每个room中的tile对应levelmask的像素宽度，需要与<see cref="FlashingEffectManager.CaculateLevelMask(RenderTexture, Room, Vector2, int)"/>中所指定的值一致</param>
        /// <returns></returns>
        public static RenderTexture CreateMask(Room room, int pixPerTile, Vector2 lightSourceRoomPos, Vector2? biasedLightSourcePos = null)
        {
            RenderTexture renderTexture = new RenderTexture(room.Width * pixPerTile, room.Height * pixPerTile, 0) { filterMode = FilterMode.Bilinear };
            renderTexture.enableRandomWrite = true;

            CaculateLevelMask(renderTexture, room, pixPerTile, lightSourceRoomPos);

            return renderTexture;
        }

        /// <summary> 使用compute shader计算光源遮罩</summary>
        /// <param name="renderTexture">使用的rt</param>
        /// <param name="lightSourceRoomPos">光源点在room内的坐标，不使用屏幕坐标</param>
        /// <param name="pixPerTile">每个room中的tile对应levelmask的像素宽度，需要与<see cref="FlashingEffectManager.CreateMask(Room, Vector2, int)"/>中所指定的值一致</param>
        /// <param name="biasedLightSourcePos">偏移后实际用于计算方向的光源坐标，同样使用room内的坐标。主要用于实现锥形光照</param>
        /// <param name="range">实际计算光照的tile半径，用于优化</param>
        public static void CaculateLevelMask(RenderTexture renderTexture, Room room, int pixPerTile, Vector2 lightSourceRoomPos, Vector2? biasedLightSourcePos = null, float range = 30f)
        {
            Vector4 normalizedLightSourceRoomPos = new Vector4(lightSourceRoomPos.x / 20f, lightSourceRoomPos.y / 20f, 0f, 0f);//默认一个tile为20单位长宽，进行归一化处理
            Vector4 normalizedBiasedLightSourceRoomPos = biasedLightSourcePos == null ? normalizedLightSourceRoomPos : new Vector4(biasedLightSourcePos.Value.x / 20f, biasedLightSourcePos.Value.y / 20f, 0f, 0f);

            Vector2Int dispatchGrounpSize = new Vector2Int(Mathf.CeilToInt(room.Width * pixPerTile / 8f), Mathf.CeilToInt(room.Height * pixPerTile / 8f));

            LevelMaskCS.SetVector("TargetRoomPos", normalizedLightSourceRoomPos);
            LevelMaskCS.SetVector("LightRoomPos", normalizedBiasedLightSourceRoomPos);
            LevelMaskCS.SetFloat("maxTileDistance", range);

            var interTex = GetInterTex(renderTexture);

            var roomGeometry = TryLoadLevelGeometryTex(room);//防止room geometry未加载

            //遮罩计算
            LevelMaskCS.SetTexture(CreateLightMaskKernalIndex, "Result", renderTexture);
            LevelMaskCS.SetTexture(CreateLightMaskKernalIndex, "InterTex", interTex);
            LevelMaskCS.SetTexture(CreateLightMaskKernalIndex, "RoomGeometry", roomGeometry);

            LevelMaskCS.Dispatch(CreateLightMaskKernalIndex, dispatchGrounpSize.x, dispatchGrounpSize.y, 1);

            //半影计算
            LevelMaskCS.SetTexture(PenumbraKernalIndex, "Result", renderTexture);
            LevelMaskCS.SetTexture(PenumbraKernalIndex, "InterTex", interTex);
            LevelMaskCS.SetTexture(PenumbraKernalIndex, "RoomGeometry", roomGeometry);

            LevelMaskCS.Dispatch(PenumbraKernalIndex, dispatchGrounpSize.x, dispatchGrounpSize.y, 1);

            //合并输出
            LevelMaskCS.SetTexture(FinalizeKernalIndex, "Result", renderTexture);
            LevelMaskCS.SetTexture(FinalizeKernalIndex, "InterTex", interTex);
            LevelMaskCS.SetTexture(FinalizeKernalIndex, "RoomGeometry", roomGeometry);

            LevelMaskCS.Dispatch(FinalizeKernalIndex, dispatchGrounpSize.x, dispatchGrounpSize.y, 1);

            //半影延申
            LevelMaskCS.SetTexture(StretchPenumbraKernalIndex, "Result", renderTexture);
            LevelMaskCS.SetTexture(StretchPenumbraKernalIndex, "InterTex", interTex);
            LevelMaskCS.SetTexture(StretchPenumbraKernalIndex, "RoomGeometry", roomGeometry);

            LevelMaskCS.Dispatch(StretchPenumbraKernalIndex, dispatchGrounpSize.x, dispatchGrounpSize.y, 1);

            //合并输出
            LevelMaskCS.Dispatch(FinalizeKernalIndex, dispatchGrounpSize.x, dispatchGrounpSize.y, 1);

            //摸糊计算
            LevelMaskCS.SetTexture(RadialBlurKernalIndex, "Result", renderTexture);
            LevelMaskCS.SetTexture(RadialBlurKernalIndex, "InterTex", interTex);
            LevelMaskCS.SetTexture(RadialBlurKernalIndex, "RoomGeometry", roomGeometry);
            LevelMaskCS.Dispatch(RadialBlurKernalIndex, dispatchGrounpSize.x, dispatchGrounpSize.y, 1);

            //合并输出
            LevelMaskCS.Dispatch(FinalizeKernalIndex, dispatchGrounpSize.x, dispatchGrounpSize.y, 1);
        }

        /// <summary> 创建计算用的中间贴图，详细使用参考<see cref="CaculateLevelMask"/> </summary>
        /// <param name="target">目标贴图</param>
        static RenderTexture GetInterTex(RenderTexture target)
        {
            Vector2Int size = new Vector2Int(target.width, target.height);
            if(!interTex.TryGetValue(size, out var tex))
            {
                tex = new RenderTexture(size.x, size.y, 0) { filterMode = FilterMode.Bilinear };
                tex.enableRandomWrite = true;

                interTex.Add(size, tex);
            }
            return tex;
        }
    }

    internal static partial class FlashingEffectManager
    {
        static List<ShaderInstance> shaderInstances = new List<ShaderInstance>();


        /// <summary>创建一个shader副本，用于防止合批处理,参考<see cref="FlashingEffectManager.ShaderInstance"/></summary>
        /// <typeparam name="T"></typeparam>
        public static T GetShaderInstance<T>() where T : ShaderInstance
        {
            T result = null;
            int count = 0;
            foreach(var instance in shaderInstances.Where(i => i is T))
            {
                if (!instance.used)
                {
                    result = instance as T;
                    break;
                }
                count++;
            }
            if (result == null)
            {
                result = InternalCreateNewShaderInstance(typeof(T), count) as T;
            }

            shaderInstances.Add(result);
            result.Get();
            return result;
        }

        static ShaderInstance InternalCreateNewShaderInstance(Type t, int index)
        {
            if(t == typeof(FlashBangShaderInstance))
                return new FlashBangShaderInstance(index);
            else if(t == typeof(DeadlyLightShaderInstance))
                return new DeadlyLightShaderInstance(index);

            throw new ArgumentException($"{t} is not a valid ShaderInstance type");
        }

        public class FlashBangShaderInstance : ShaderInstance
        {
            public override string ShaderName => "FlashBang";
            public override Shader TargetShader => FlashBangShader;

            public FlashBangShaderInstance(int index) : base(index)
            {
            }
        }

        public class DeadlyLightShaderInstance : ShaderInstance
        {

            public override string ShaderName => "DeadlyLight";
            public override Shader TargetShader => DeadlyLightShader;

            public DeadlyLightShaderInstance(int index) : base(index)
            {
            }
        }

        public abstract class ShaderInstance
        {
            string shader;
            public virtual string ShaderName => "";
            public virtual Shader TargetShader => null;

            public string Shader => shader;
            public bool used { get; private set; }

            public ShaderInstance(int index)
            {
                shader = $"{ShaderName}_{index}";
                Custom.rainWorld.Shaders.Add(Shader, FShader.CreateShader(Shader, TargetShader));
            }

            public void Get()
            {
                used = true;
            }

            public void Release()
            {
                used = false;
            }
        }
    }
}
