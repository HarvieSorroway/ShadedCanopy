using BepInEx;
using BepInEx.Logging;
using Newtonsoft.Json;
using SCUtils;
using SCUtils.RwTasks;
using SCUtils.SCSaveManager;
using ShadedCanopy.Creatures;
using ShadedCanopy.Creatures.Scavengers;
using ShadedCanopy.FlashingEffect;
using ShadedCanopy.Imgui;
using ShadedCanopy.Iterators.MechIterator;
using ShadedCanopy.SaveDatas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618


namespace ShadedCanopy
{
    [BepInPlugin(ModID, ModName, ModVersion)]
    public class SCPlugin : BaseUnityPlugin
    {
        new internal static ManualLogSource Logger;

        public const string ModID = "shaded_canopy";
        public const string ModName = "Shaded Canopy";
        public const string ModVersion = "0.0.1";

        private bool inited;

        public SCPlugin()
        {
            Logger = base.Logger;
        }
        public void OnEnable()
        {
            try
            {
                On.RainWorld.OnModsInit += RainWorld_OnModsInit;
                SCCritobs.Init();
            }
            catch(Exception e)
            {
                Logger.LogFatal($"Exception during {ModName} OnEnable: {e}");
            }
        }

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig.Invoke(self);
            if (inited)
                return;
            try
            {
                inited = true;

                ShimmerSlugcat.PlayerHooks.Hooks();
                ShimmerSlugcat.PGraphicHooks.Hooks();
                FlashingEffectManager.Init();
                ScavengerHooks.HooksOn();
                PlacedObjects.SCPlacedObjects.Init();

                MechIteratorHooks.HooksOn();

                SCUtils.SCUtils.Init(Logger);
                SCResources.LoadResources(self);

                //ImguiRegister.TryInit();
                Objects.SCMorningGlory.SCMorningGloryHooks.Hook();
                Objects.SCMorningGlory.SCMorningGloryTest.HookTest();

                SCUtils.SCHelperUtils.Log($"{ModName} - {ModVersion} - {DateTime.Now}");

                //On.RainWorldGame.RawUpdate += RainWorldGame_RawUpdate;
            }
            catch (Exception e)
            {
                Logger.LogFatal($"Exception during {ModName} RainWorld_OnModsInit: {e}");
            }
        }

        /*
        private void RainWorldGame_RawUpdate(On.RainWorldGame.orig_RawUpdate orig, RainWorldGame self, float dt)
        {
            orig(self, dt);
            if (Input.GetKeyDown(KeyCode.E))
            {
                SCHelperUtils.Log(SCDeathPersistentManager.Data);
            }
            else if (Input.GetKeyDown(KeyCode.F)) 
            {
                SCDeathPersistentManager.Data.foodPrintUnlocked = true;
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                SCDeathPersistentManager.Data.foodPrintUnlocked = true;
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                SCDeathPersistentManager.Data.stashPearls.Add(new DataPearl.AbstractDataPearl(self.world, AbstractPhysicalObject.AbstractObjectType.DataPearl,
                    null, self.Players[0].pos, self.GetNewID(), -1, -1, null, DataPearl.AbstractDataPearl.DataPearlType.GW));
            }
            else if (Input.GetKeyDown(KeyCode.I))
            {
                foreach(var data in SCDeathPersistentManager.Data.stashPearls)
                {
                    SCHelperUtils.Log(JsonConvert.SerializeObject(data));
                }
            }
            else if (Input.GetKeyDown(KeyCode.G))
            {
                SCDeathPersistentManager.Data.meetThisCycle = true;
            }
        }
        */
    }
}
