using BepInEx;
using ShadedCanopy.FlashingEffect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Security.Permissions;


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

        private bool inited;

        public void OnEnable()
        {
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;

            ShimmerSlugcat.PlayerHooks.Hooks();
            ShimmerSlugcat.PGraphicHooks.Hooks();
        }

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig.Invoke(self);
            if (inited)
                return;

            FlashingEffectManager.Init();
            ShimmerSlugcat.ShimmerPlugin.LoadShimmerAsset(self);

            inited = true;
        }
        

    }
}
