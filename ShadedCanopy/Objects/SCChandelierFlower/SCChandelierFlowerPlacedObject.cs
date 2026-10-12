using System.Collections.Generic;

namespace ShadedCanopy.Objects.SCChandelierFlower
{
    internal class SCChandelierFlowerPlacedObject : SCUtils.DevToolUtils.IDevObjectPageExt
    {
        public PlacedObject.Type PlacedObjectType => SCEnums.PlacedObjectType.ChandelierFlower;

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
            SCChandelierFlower.AbstractChandelierFlower ac = new SCChandelierFlower.AbstractChandelierFlower(
                room.world,
                null,
                room.GetWorldCoordinate(placedObject.pos),
                room.game.GetNewID(),
                room.abstractRoom.index,
                itemIdx,
                placedObject.data as PlacedObject.ConsumableObjectData
            );
            ac.placedObjectOrigin = room.SetAbstractRoomAndPlacedObjectNumber(room.abstractRoom.name, itemIdx);
            if (!(room.game.session is StoryGameSession) || !(room.game.session as StoryGameSession).saveState.ItemConsumed(room.world, false, room.abstractRoom.index, itemIdx))
            {
                ac.SetUnconsumed(room);
                room.abstractRoom.AddEntity(ac);
                room.abstractRoom.AddEntity(ac.abstractFruit);
            }
            else
            {
                room.abstractRoom.AddEntity(ac);
            }
            yield break;
        }
    }
}

