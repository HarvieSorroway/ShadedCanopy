using DevInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadedCanopy.PlacedObjects.DeadlyLight
{
    internal class DeadlyLightExt : SCUtils.DevToolUtils.IDevObjectPageExt
    {
        public PlacedObject.Type PlacedObjectType => SCEnums.PlacedObjectType.DeadlyLight;

        public ObjectsPage.DevObjectCategories Category => ObjectsPage.DevObjectCategories.Decoration;

        public PlacedObjectRepresentation CreateRep(ObjectsPage page, PlacedObject p)
        {
            return new DeadlyLightRep(page.owner, PlacedObjectType.ToString() + "_Rep", page, p);
        }

        public PlacedObject.Data GenerateEmptyData(PlacedObject p)
        {
            return new DeadlyLightData(p);
        }

        public IEnumerable<UpdatableAndDeletable> RoomLoaded(Room room, PlacedObject placedObject)
        {
            yield return new DeadlyLight(room, placedObject);
        }
    }
}
