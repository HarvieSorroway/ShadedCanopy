using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShadedCanopy.ShimmerSlugcat
{
    public class ShimmerPlugin
    {
        public static Shader ShimmerSkin;
        public static SlugcatStats.Name Shimmer = new SlugcatStats.Name("Shimmer", false);


        public static void LoadShimmerAsset(RainWorld rainWorld)
        {
            try
            {
                string path = AssetManager.ResolveFilePath("AssetBundles/shimmerbodyshader");
                AssetBundle ab = AssetBundle.LoadFromFile(path);
                ShimmerSkin = ab.LoadAsset<Shader>("Assets/ShimmerSkin.shader");
                rainWorld.Shaders.Add("ShimmerSkin", FShader.CreateShader("ShimmerSkin", ShimmerSkin));
                Futile.atlasManager.LoadImage("atlases/ShimmerTail");
                Futile.atlasManager.LoadAtlas("atlases/ShimmerHead");
                Futile.atlasManager.LoadAtlas("atlases/ShimmerLegs");
                Futile.atlasManager.LoadAtlas("atlases/ShimmerPlayerArm");
                Futile.atlasManager.LoadAtlas("atlases/ShimmerHips");
                Futile.atlasManager.LoadAtlas("atlases/ShimmerBody");

            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }

    }
}
