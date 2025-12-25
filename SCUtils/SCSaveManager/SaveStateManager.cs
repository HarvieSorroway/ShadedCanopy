using Newtonsoft.Json;
using SlugBase.SaveData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCUtils.SCSaveManager
{

    internal static class SaveStateManager
    {
        private static List<ISaveManager> _allManagers = new List<ISaveManager>();

        static SaveStateManager()
        {
            foreach(var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.SafeGetTypes()))
            {
                if(type.IsAssignableFrom(typeof(ISaveManager)))
                {
                    _allManagers.Add((ISaveManager)type.GetProperty("Instance")?.GetValue(null));
                }
            }
        }

        public static void Init()
        {
            On.StoryGameSession.ctor += StoryGameSession_ctor;
            On.DeathPersistentSaveData.SaveToString += DeathPersistentSaveData_SaveToString;
            On.SaveState.RainCycleTick += SaveState_RainCycleTick;
            On.ProcessManager.PreSwitchMainProcess += ProcessManager_PreSwitchMainProcess;
        }

        private static void StoryGameSession_ctor(On.StoryGameSession.orig_ctor orig, StoryGameSession self, SlugcatStats.Name saveStateNumber, RainWorldGame game)
        {
            orig(self, saveStateNumber, game);
            foreach (var manager in _allManagers) 
            {
                manager.Init(self);
            }
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
