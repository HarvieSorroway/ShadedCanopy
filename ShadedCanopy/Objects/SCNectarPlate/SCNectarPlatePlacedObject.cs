using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadedCanopy.Objects.SCNectarPlate
{
    public class SCNectarPlatePlacedObject : SCUtils.DevToolUtils.IDevObjectPageExt
    {
        public PlacedObject.Type PlacedObjectType => SCEnums.PlacedObjectType.NectarPlate;

        public DevInterface.ObjectsPage.DevObjectCategories Category => DevInterface.ObjectsPage.DevObjectCategories.Consumable;

        public DevInterface.PlacedObjectRepresentation CreateRep(DevInterface.ObjectsPage page, PlacedObject p)
        {
            return new DevInterface.ConsumableRepresentation(page.owner, PlacedObjectType.ToString() + "_Rep", page, p, PlacedObjectType.ToString());
        }

        public PlacedObject.Data GenerateEmptyData(PlacedObject p)
        {
            return new PlacedObject.ConsumableObjectData(p);
        }

        public IEnumerable<UpdatableAndDeletable> RoomLoaded(Room room, PlacedObject placedObject, int itemIdx)
        {
            AbstractNectarPlate ac = new AbstractNectarPlate(room.world, AbstractNectarPlate.NectarPlate, null, 
                room.GetWorldCoordinate(placedObject.pos),
                room.game.GetNewID(),
                room.abstractRoom.index,
                itemIdx,
                placedObject.data as PlacedObject.ConsumableObjectData);

            ac.placedObjectOrigin = room.SetAbstractRoomAndPlacedObjectNumber(room.abstractRoom.name, itemIdx);
            if (!(room.game.session is StoryGameSession) || !(room.game.session as StoryGameSession).saveState.ItemConsumed(room.world, false, room.abstractRoom.index, itemIdx))
            {
                ac.dead = false;
            }
            else
            {
                ac.dead = true;
            }
            room.abstractRoom.AddEntity(ac);
            yield break;
        }

    }
}
