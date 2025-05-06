using DevInterface;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCUtils.DevToolUtils
{
    public static class PlacedObjectExt
    {
        static Dictionary<PlacedObject.Type,IDevObjectPageExt> extInstances = new();
        internal static void Init()
        {
            On.DevInterface.ObjectsPage.CreateObjRep += ObjectsPage_CreateObjRep;
            On.DevInterface.ObjectsPage.DevObjectGetCategoryFromPlacedType += ObjectsPage_DevObjectGetCategoryFromPlacedType;

            On.PlacedObject.GenerateEmptyData += PlacedObject_GenerateEmptyData;
            On.Room.Loaded += Room_Loaded;
        }

        static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
        {
            orig.Invoke(self);
            foreach (var placed in self.roomSettings.placedObjects)
            {
                if (extInstances.TryGetValue(placed.type, out var ext))
                {
                    foreach(var obj in ext.RoomLoaded(self, placed))
                    {
                        self.AddObject(obj);
                    }
                }
            }
        }

        static void PlacedObject_GenerateEmptyData(On.PlacedObject.orig_GenerateEmptyData orig, PlacedObject self)
        {
            orig.Invoke(self);
            if (extInstances.TryGetValue(self.type, out var ext))
            {
                self.data = ext.GenerateEmptyData(self);
            }
        }

        static ObjectsPage.DevObjectCategories ObjectsPage_DevObjectGetCategoryFromPlacedType(On.DevInterface.ObjectsPage.orig_DevObjectGetCategoryFromPlacedType orig, ObjectsPage self, PlacedObject.Type type)
        {
            if (extInstances.TryGetValue(type, out var ext))
                return ext.Category;
            return orig.Invoke(self, type);
        }

        static void ObjectsPage_CreateObjRep(On.DevInterface.ObjectsPage.orig_CreateObjRep orig, ObjectsPage self, PlacedObject.Type tp, PlacedObject pObj)
        {
            if (extInstances.TryGetValue(tp, out var ext))
            {
                if (pObj == null)
                {
                    pObj = new PlacedObject(tp, null);
                    pObj.pos = self.owner.room.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new Vector2(-683f, 384f), 0.25f) + Custom.DegToVec(UnityEngine.Random.value * 360f) * 0.2f;
                    self.RoomSettings.placedObjects.Add(pObj);
                }

                var rep = ext.CreateRep(self, pObj);
                self.tempNodes.Add(rep);
                self.subNodes.Add(rep);
            }
            else
                orig.Invoke(self, tp, pObj);
        }

        public static void Register(IDevObjectPageExt devObjectPageExt)
        {
            extInstances.Add(devObjectPageExt.PlacedObjectType, devObjectPageExt);
        }
    }

    public interface IDevObjectPageExt
    {
        public PlacedObject.Type PlacedObjectType { get; }
        public ObjectsPage.DevObjectCategories Category { get; }
        public PlacedObject.Data GenerateEmptyData(PlacedObject p);
        public PlacedObjectRepresentation CreateRep(ObjectsPage page, PlacedObject p);
        public IEnumerable<UpdatableAndDeletable> RoomLoaded(Room room, PlacedObject placedObject);
    }
}
