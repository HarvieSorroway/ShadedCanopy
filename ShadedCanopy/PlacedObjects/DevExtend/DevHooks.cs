using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShadedCanopy.PlacedObjects.DevExtend
{
    internal class DevHooks
    {
        public static void HooksOn()
        {
            On.DevInterface.ObjectsPage.CreateObjRep += ObjectsPage_CreateObjRep;
            On.DevInterface.ObjectsPage.DevObjectGetCategoryFromPlacedType += ObjectsPage_DevObjectGetCategoryFromPlacedType;

            On.PlacedObject.GenerateEmptyData += PlacedObject_GenerateEmptyData;
            On.Room.Loaded += Room_Loaded;
        }

        private static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
        {
            orig.Invoke(self);
            foreach(var placed in self.roomSettings.placedObjects)
            {
                if(placed.type == SCEnums.PlacedObjectType.DeadlyLight)
                {
                    self.AddObject(new DeadlyLight.DeadlyLight(self, placed));
                }
            }
        }

        private static void PlacedObject_GenerateEmptyData(On.PlacedObject.orig_GenerateEmptyData orig, PlacedObject self)
        {
            orig.Invoke(self);
            if (self.type == SCEnums.PlacedObjectType.DeadlyLight)
            {
                self.data = new DeadlyLight.DeadlyLightData(self);
            }
        }


        private static DevInterface.ObjectsPage.DevObjectCategories ObjectsPage_DevObjectGetCategoryFromPlacedType(On.DevInterface.ObjectsPage.orig_DevObjectGetCategoryFromPlacedType orig, global::DevInterface.ObjectsPage self, PlacedObject.Type type)
        {
            if (type == SCEnums.PlacedObjectType.DeadlyLight)
                return DevInterface.ObjectsPage.DevObjectCategories.Decoration;
            return orig.Invoke(self, type);
        }

        private static void ObjectsPage_CreateObjRep(On.DevInterface.ObjectsPage.orig_CreateObjRep orig, global::DevInterface.ObjectsPage self, PlacedObject.Type tp, PlacedObject pObj)
        {
            if (tp == SCEnums.PlacedObjectType.DeadlyLight)
            {
                if (pObj == null)
                {
                    pObj = new PlacedObject(tp, null);
                    pObj.pos = self.owner.room.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new Vector2(-683f, 384f), 0.25f) + Custom.DegToVec(UnityEngine.Random.value * 360f) * 0.2f;
                    self.RoomSettings.placedObjects.Add(pObj);
                }

                //BuffPlugin.Log($"Dev - {self.owner == null} {}")
                var rep = new DeadlyLight.DeadlyLightRep(self.owner, tp.ToString() + "_Rep", self, pObj);
                self.tempNodes.Add(rep);
                self.subNodes.Add(rep);
            }
            else
                orig.Invoke(self, tp, pObj);
        }
    }
}
