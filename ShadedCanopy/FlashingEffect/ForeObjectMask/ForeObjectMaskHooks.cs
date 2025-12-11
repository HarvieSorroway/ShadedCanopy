using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ShadedCanopy.FlashingEffect.ForeObjectMask
{
    internal static partial class ForeObjectMaskHooks
    {
        public static ConditionalWeakTable<RoomCamera, ForeObjectMask> masks = new ConditionalWeakTable<RoomCamera, ForeObjectMask>();

        public static void HooksOn()
        {
            On.RoomCamera.ctor += RoomCamera_ctor;
            On.RoomCamera.DrawUpdate += RoomCamera_DrawUpdate;
            On.RoomCamera.ClearAllSprites += RoomCamera_ClearAllSprites;
            TrackedObjectHooks();
        }


        private static void RoomCamera_ctor(On.RoomCamera.orig_ctor orig, RoomCamera self, RainWorldGame game, int cameraNumber)
        {
            orig.Invoke(self, game, cameraNumber);
            masks.Add(self, new ForeObjectMask(self));
        }

        private static void RoomCamera_DrawUpdate(On.RoomCamera.orig_DrawUpdate orig, RoomCamera self, float timeStacker, float timeSpeed)
        {
            orig.Invoke(self, timeStacker, timeSpeed);
            if(masks.TryGetValue(self, out var mask))
            {
                mask.DrawUpdate(self, timeStacker);
            }
        }
        private static void RoomCamera_ClearAllSprites(On.RoomCamera.orig_ClearAllSprites orig, RoomCamera self)
        {
            orig.Invoke(self);
            if (masks.TryGetValue(self, out var mask))
            {
                mask.ClearAllSprites();
            }
        }
    }

    internal static partial class ForeObjectMaskHooks
    {
        public static void TrackedObjectHooks()
        {
            CopiedGraphicType = new()
            {
                typeof(RegionGate)
            };
            On.RoomCamera.SpriteLeaser.ctor += SpriteLeaser_ctor;
        }

        private static void SpriteLeaser_ctor(On.RoomCamera.SpriteLeaser.orig_ctor orig, RoomCamera.SpriteLeaser self, IDrawable obj, RoomCamera rCam)
        {
            orig.Invoke(self, obj, rCam);
            if (masks.TryGetValue(rCam, out var mask))
            {
                if(obj is RegionGate /*|| obj is Water*/)
                    mask.CopySLeaser(self);
            }
        }

        static HashSet<Type> CopiedGraphicType;
    }
}
