using BepInEx;
using ShadedCanopy.Creatures.Scavengers;
using ShadedCanopy.FlashingEffect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace ShadedCanopy
{
    [BepInPlugin(ModID, ModName, ModVersion)]
    public class SCPlugin : BaseUnityPlugin
    {
        public const string ModID = "shaded_canopy";
        public const string ModName = "Shaded Canopy";
        public const string ModVersion = "0.0.1";

        

        public void OnEnable()
        {
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        }

        static bool inited;
        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig.Invoke(self);
            if (inited)
                return;

            FlashingEffectManager.Init();
            ScavengerHooks.HooksOn();

            inited = true;
        }

        public static void LoadAssets()
        {

        }
    }
}
