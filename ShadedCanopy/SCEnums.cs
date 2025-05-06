using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadedCanopy
{
    internal static class SCEnums
    {
        internal static class PlacedObjectType
        {
            public readonly static PlacedObject.Type DeadlyLight = new PlacedObject.Type("DeadlyLight", true);
        }

        internal static class SlugStateName
        {
            public readonly static SlugcatStats.Name Shimmer = new SlugcatStats.Name("Shimmer", false);
        }
    }
}
