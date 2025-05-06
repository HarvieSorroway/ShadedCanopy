using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCUtils.DevToolUtils;
using ShadedCanopy.PlacedObjects.DeadlyLight;

namespace ShadedCanopy.PlacedObjects
{
    internal static class SCPlacedObjects
    {
        public static void Init()
        {
            PlacedObjectExt.Register(new DeadlyLightExt());
        }
    }
}
