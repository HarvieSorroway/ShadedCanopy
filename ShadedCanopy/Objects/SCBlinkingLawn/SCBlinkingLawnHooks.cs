using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadedCanopy.Objects.SCBlinkingLawn
{
    internal static class SCBlinkingLawnHooks
    {
        public static readonly string BlinkingTawnPlantSpriteName = "atlases/BlinkingTawn/BlinkingPlant";

        public static void LoadResources()
        {
            foreach (string[] snames in from PlantInfo i in PlantInfo.presetInfos select i.spriteNames)
            {
                foreach (string sname in snames)
                {
                    Futile.atlasManager.LoadImage(sname);
                }
            }
        }
    }
}
