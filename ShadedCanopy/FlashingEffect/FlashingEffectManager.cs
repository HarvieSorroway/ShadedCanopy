
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShadedCanopy.FlashingEffect
{
    internal static partial class FlashingEffectManager
    {
        public static ComputeShader LevelMaskCS { get; private set; }
        public static Shader FlashBangShader { get; private set; }
        public static int CreateLightMaskKernalIndex { get; private set; }
        public static int RadialBlurKernalIndex { get; private set; }

        public static void Init()
        {
            //HooksOn();
            LoadAssets();
        }

        public static void LoadAssets()
        {
            string path = AssetManager.ResolveFilePath("AssetBundles/shadedcanopybundle");
            AssetBundle shadedcanopybundle = AssetBundle.LoadFromFile(path);

            LevelMaskCS = shadedcanopybundle.LoadAsset<ComputeShader>("assets/myshader/levellightmask.compute");
            CreateLightMaskKernalIndex = LevelMaskCS.FindKernel("CreateLightMask");
            RadialBlurKernalIndex = LevelMaskCS.FindKernel("RadialBlur");

            FlashBangShader = shadedcanopybundle.LoadAsset<Shader>("assets/myshader/flashbang.shader");
            //Custom.rainWorld.Shaders.Add("SC_FlashBang", FShader.CreateShader("SC_FlashBang", shadedcanopybundle.LoadAsset<Shader>("assets/myshader/flashbang.shader")));
        }

        public static void HooksOn()
        {
            On.Player.Jump += Player_Jump;
        }

        private static void Player_Jump(On.Player.orig_Jump orig, Player self)
        {
            orig.Invoke(self);
            self.room.AddObject(new FlashingEffectTest(self.room, self.firstChunk));
        }
    }

    internal static partial class FlashingEffectManager
    {
        static int maxLoadRoomGeometry = 5;
        static Dictionary<string, Texture2D> roomGeometryTexs = new Dictionary<string, Texture2D>();
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
        public static RenderTexture CreateMask(Room room, Vector2 lightSourceRoomPos, int pixPerTile)
        {
            RenderTexture renderTexture = new RenderTexture(room.Width * pixPerTile, room.Height * pixPerTile, 0) { filterMode = FilterMode.Bilinear };
            renderTexture.enableRandomWrite = true;

            CaculateLevelMask(renderTexture, room, lightSourceRoomPos, pixPerTile);

            return renderTexture;
        }

        /// <summary> 使用compute shader计算光源遮罩</summary>
        /// <param name="renderTexture">使用的rt</param>
        /// <param name="lightSourceRoomPos">光源点在room内的坐标，不使用屏幕坐标</param>
        /// <param name="pixPerTile">每个room中的tile对应levelmask的像素宽度，需要与<see cref="FlashingEffectManager.CreateMask(Room, Vector2, int)"/>中所指定的值一致</param>
        public static void CaculateLevelMask(RenderTexture renderTexture, Room room, Vector2 lightSourceRoomPos, int pixPerTile)
        {
            Vector4 normalizedLightSourceRoomPos = new Vector4(lightSourceRoomPos.x / 20f, lightSourceRoomPos.y / 20f, 0f, 0f);//默认一个tile为20单位长宽，进行归一化处理
            Vector2Int dispatchGrounpSize = new Vector2Int(Mathf.CeilToInt(room.Width * pixPerTile / 8f), Mathf.CeilToInt(room.Height * pixPerTile / 8f));

            var roomGeometry = TryLoadLevelGeometryTex(room);//防止room geometry未加载
            LevelMaskCS.SetTexture(CreateLightMaskKernalIndex, "Result", renderTexture);
            LevelMaskCS.SetTexture(CreateLightMaskKernalIndex, "RoomGeometry", roomGeometry);
            LevelMaskCS.SetVector("TargetRoomPos", normalizedLightSourceRoomPos);
            LevelMaskCS.Dispatch(CreateLightMaskKernalIndex, dispatchGrounpSize.x, dispatchGrounpSize.y, 1);

            LevelMaskCS.SetTexture(RadialBlurKernalIndex, "Result", renderTexture);
            LevelMaskCS.SetTexture(RadialBlurKernalIndex, "RoomGeometry", roomGeometry);
            LevelMaskCS.SetVector("TargetRoomPos", normalizedLightSourceRoomPos);
            LevelMaskCS.Dispatch(RadialBlurKernalIndex, dispatchGrounpSize.x, dispatchGrounpSize.y, 1);
        }
    }

    internal static partial class FlashingEffectManager
    {
        static List<FlashBangShaderInstance> flashBangShaderInstances = new List<FlashBangShaderInstance>();


        public static FlashBangShaderInstance GetFlashBangShaderInstance()
        {
            FlashBangShaderInstance result = null;
            foreach (var instance in flashBangShaderInstances)
            {
                if (!instance.used)
                {
                    result = instance;
                    break;
                }
            }
            if(result == null)
            {
                result = new FlashBangShaderInstance(flashBangShaderInstances.Count);
                flashBangShaderInstances.Add(result);
            }
            result.Get();
            return result;
        }

        public class FlashBangShaderInstance
        {
            string shortName;
            public string Shader => shortName;

            public bool used;

            public FlashBangShaderInstance(int index)
            {
                shortName = $"FlashBang_{index}";
                Custom.rainWorld.Shaders.Add(shortName, FShader.CreateShader(shortName, FlashBangShader));
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
