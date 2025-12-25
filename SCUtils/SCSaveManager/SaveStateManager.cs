using MonoMod.RuntimeDetour;
using Newtonsoft.Json;
using SlugBase.SaveData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace SCUtils.SCSaveManager
{

    public static class SaveStateManager
    {
        private static List<ISaveManager> _allManagers = new List<ISaveManager>();

        public static WeakReference<World> CurrentWorld = new WeakReference<World>(null);

        public static JsonSerializerSettings Settings { get; private set; }


        static SaveStateManager()
        {
            Settings = new JsonSerializerSettings();
            Settings.Converters.Add(new AbstractObjectConverter());
            JsonConvert.DefaultSettings = () => Settings;
            var inf = typeof(ISaveManager);
            foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.SafeGetTypes()))
            {
                if (inf.IsAssignableFrom(type) && !type.IsAbstract)
                {
                    if (type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static
                        | BindingFlags.FlattenHierarchy)?.GetValue(null) is ISaveManager manager)
                    {
                        _allManagers.Add(manager);
                        SCUtils.Log($"[SaveStateManager] find {type}");
                    }
                }
            }
        }

        public static void Init()
        {
    
            using (new DetourContext(100))
            {
                On.RainWorldGame.ctor += RainWorldGame_ctor;
                On.DeathPersistentSaveData.SaveToString += DeathPersistentSaveData_SaveToString;
                On.SaveState.RainCycleTick += SaveState_RainCycleTick;
                On.ProcessManager.PreSwitchMainProcess += ProcessManager_PreSwitchMainProcess;
            }
        }

        private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
        {
            orig(self, manager);
            CurrentWorld = new WeakReference<World>(self.world);
            if (self.IsStorySession)
                InitNewSession(self.GetStorySession);
        }

        private static void ProcessManager_PreSwitchMainProcess(On.ProcessManager.orig_PreSwitchMainProcess orig, ProcessManager self, ProcessManager.ProcessID ID)
        {
            if(self.currentMainLoop is RainWorldGame)
            {
                foreach (var manager in _allManagers)
                {
                    manager.Clear();
                }
            }
            orig(self, ID);
        }

        private static void SaveState_RainCycleTick(On.SaveState.orig_RainCycleTick orig, SaveState self, RainWorldGame game, bool depleteSwarmRoom)
        {
            orig(self, game, depleteSwarmRoom);
            foreach (var manager in _allManagers)
            {
                manager.RainCycleTick(game);
            }
        }

        private static string DeathPersistentSaveData_SaveToString(On.DeathPersistentSaveData.orig_SaveToString orig, DeathPersistentSaveData self, bool saveAsIfPlayerDied, bool saveAsIfPlayerQuit)
        {
            //由于DeathPersistentSaveData保存的代码位置一定会保存MiscWorldSaveData
            //且写入Slugbase缓冲区不会直接应用于实际储存所以这里合并处理
            Save(saveAsIfPlayerDied, saveAsIfPlayerQuit); 
            return orig(self, saveAsIfPlayerDied, saveAsIfPlayerQuit);
        }

        public static void InitNewSession(StoryGameSession newSession)
        {
            foreach (var manager in _allManagers)
                manager.Init(newSession);
        }

        public static void RainCycleTick(RainWorldGame game)
        {
            foreach (var manager in _allManagers)
                manager.RainCycleTick(game);
        }

        public static void Save(bool isDied, bool isQuit)
        {
            foreach(var manager in _allManagers)
                manager.Save(isDied, isQuit);
        }

    }

}
