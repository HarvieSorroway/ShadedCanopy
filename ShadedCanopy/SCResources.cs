using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShadedCanopy
{
    internal static class SCResources
    {
        //shaders
        public static string AdditiveDefaultShaderName = "AdditiveDefault";

        //atlas
        public static string Blur40Atlas, Blur80Atlas;
        public static void LoadResources(RainWorld rainWorld)
        {
            ShimmerSlugcat.ShimmerPlugin.LoadShimmerAsset(rainWorld);

            string path = AssetManager.ResolveFilePath("AssetBundles/additive");
            AssetBundle ab = AssetBundle.LoadFromFile(path);

            AssetBundle abna = AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("AssetBundles/nabundle"));
            Objects.SCMorningGlory.SCMorningGloryHooks.LoadResources(abna);

            Custom.rainWorld.Shaders.Add(AdditiveDefaultShaderName, FShader.CreateShader("AdditiveDefault", ab.LoadAsset<Shader>("assets/myshader/dronemaster/additivedefault.shader")));

            Blur40Atlas = Futile.atlasManager.LoadImage("atlases/blur40").name;
            Blur80Atlas = Futile.atlasManager.LoadImage("atlases/blur80").name;

            Futile.atlasManager.LoadImage("atlases/PlatePetal");
            Futile.atlasManager.LoadImage("atlases/NectarCrystal");
        }   
    }
}
