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

                SCUtils.SCUtils.Init(Logger);
                SCResources.LoadResources(self);

                //ImguiRegister.TryInit();
                Objects.SCMorningGlory.SCMorningGloryHooks.Hook();
                Objects.SCMorningGlory.SCMorningGloryTest.HookTest();

                Objects.SCNectarPlate.SCNectarPlate.TestHooks();
                Objects.SCNectarPlate.SCNectarPlateHooks.Hooks();

                SCUtils.SCHelperUtils.Log($"{ModName} - {ModVersion} - {DateTime.Now}");
                inited = true;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
            }
        }
        
    }
}
