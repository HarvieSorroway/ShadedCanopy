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
            public readonly static PlacedObject.Type MorngingGlory = new PlacedObject.Type("MorngingGlory", true);
        }

        internal static class SlugStateName
        {
            public readonly static SlugcatStats.Name Shimmer = new SlugcatStats.Name("Shimmer", true);
        }

        internal static class CreatureTemplateType
        {
            public readonly static CreatureTemplate.Type SCScavenger = new CreatureTemplate.Type("Scintivenger", true);
        }

        internal static class AbstractObjectTypeType
        {
            public readonly static AbstractPhysicalObject.AbstractObjectType SCMorningGlory = new AbstractPhysicalObject.AbstractObjectType("SCMorningGlory", true);
            public readonly static AbstractPhysicalObject.AbstractObjectType SCMorningGloryFruit = new AbstractPhysicalObject.AbstractObjectType("SCMorningGloryFruit", true);

        }
    }
}
