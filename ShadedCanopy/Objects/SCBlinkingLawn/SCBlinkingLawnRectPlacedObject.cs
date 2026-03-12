using DevInterface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace ShadedCanopy.Objects.SCBlinkingLawn
{

    public class SCBlinkingLawnRectControlPanel: Panel, IDevUISignals
    {
        public SCBlinkingLawnRectControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(250f, 70f), "Blinking Tawn Rect")
        {
            this.subNodes.Add(new SCBlinkingLawnRectDenseSlider(owner, "Dense_per_tile", this, new Vector2(5f, 45f), "每个图块中数量", 20));
            this.subNodes.Add(new SCBlinkingLawnRectRefreshButton(owner, "Refresh_Button", this, new Vector2(5f, 25f), 240f));
            this.subNodes.Add(new SCBlinkingLawnRectHiddenButton(owner, "Refresh_Button", this, new Vector2(5f, 5f), 240f));
        }
        public void Signal(DevUISignalType type, DevUINode sender, string message)
        {
        }
        class SCBlinkingLawnRectRefreshButton : Button
        {
            public SCBlinkingLawnRectRefreshButton(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, float width) : base(owner, IDstring, parentNode, pos, width, string.Empty)
            {
                this.Text = "重新生成";
            }
            public override void Clicked()
            {
                (this.parentNode.parentNode as SCBlinkingLawnRectRepresentation).bindObj.Refresh();
            }
        }
        class SCBlinkingLawnRectHiddenButton : Button
        {
            bool showState;
            public SCBlinkingLawnRectHiddenButton(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, float width) : base(owner, IDstring, parentNode, pos, width, string.Empty)
            {
                this.showState = (this.parentNode.parentNode as SCBlinkingLawnRectRepresentation).bindObj.Visible;
                this.Text = this.showState ? "隐藏效果" : "显示效果";
            }
            public override void Clicked()
            {
                this.showState = !this.showState;
                this.Text = this.showState ? "隐藏效果" : "显示效果";
                (this.parentNode.parentNode as SCBlinkingLawnRectRepresentation).bindObj.SetVisible(this.showState);
            }
        }
        class SCBlinkingLawnRectDenseSlider : Slider
        {
            float rangeRight;
            public SCBlinkingLawnRectDenseSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title, float rangeRight) : base(owner, IDstring, parentNode, pos, title, false, 110f)
            {
                this.rangeRight = rangeRight;
            }
            SCBlinkingLawnRect.SCBlinkingLawnRectData data
            {
                get
                {
                    return (this.parentNode.parentNode as SCBlinkingLawnRectRepresentation).pObj.data as SCBlinkingLawnRect.SCBlinkingLawnRectData;
                }
            }
            public override void Refresh()
            {
                base.Refresh();
                float nubPos = 0f;
                
                nubPos = data.densePerTile / this.rangeRight;
                if (nubPos > 1)
                    nubPos = 1;
                //2个有效数字
                base.NumberText = data.densePerTile.ToString("G2");
                base.RefreshNubPos(nubPos);
            }

            // Token: 0x060054F2 RID: 21746 RVA: 0x00552D5C File Offset: 0x00550F5C
            public override void NubDragged(float nubPos)
            {
                data.densePerTile = nubPos * this.rangeRight;
                this.parentNode.parentNode.Refresh();
                this.Refresh();
            }
        }
    }

    class SCBlinkingLawnRectRepresentation: QuadObjectRepresentation
    {
        public SCBlinkingLawnRect bindObj;
        public SCBlinkingLawnRectRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) : base(owner, IDstring, parentNode, pObj, name)
        {
            IEnumerable<SCBlinkingLawnRect> objInRoom = from UpdatableAndDeletable uad in owner.room.updateList
                                                        where uad is SCBlinkingLawnRect
                                                        select uad as SCBlinkingLawnRect;
            if (objInRoom.Any())
            {
                SCPlugin.Logger.LogInfo("Found existing SCBlinkingLawnRect in room, binding to it");
                bindObj = objInRoom.First();
            } else
            {
                SCPlugin.Logger.LogInfo("No existing SCBlinkingLawnRect found in room, creating new one");
                bindObj = new SCBlinkingLawnRect(owner.room, pObj);
                owner.room.AddObject(bindObj);
            }
            SCBlinkingLawnRectControlPanel panel = new SCBlinkingLawnRectControlPanel(owner, "BlinkingLawnRect_Panel", this, new Vector2(0f, 100f));
            panel.pos = Vector2.zero;
            this.subNodes.Add(panel);
        }
    }
    internal class SCBlinkingLawnRectPlacedObject : SCUtils.DevToolUtils.IDevObjectPageExt
    {
        public PlacedObject.Type PlacedObjectType => SCEnums.PlacedObjectType.BlinkingLawnRect;

        public DevInterface.ObjectsPage.DevObjectCategories Category => DevInterface.ObjectsPage.DevObjectCategories.Decoration;

        public DevInterface.PlacedObjectRepresentation CreateRep(DevInterface.ObjectsPage page, PlacedObject p)
        {
            return new SCBlinkingLawnRectRepresentation(page.owner, PlacedObjectType.ToString() + "_Rep", page, p, PlacedObjectType.ToString());
        }

        public PlacedObject.Data GenerateEmptyData(PlacedObject p)
        {
            return new SCBlinkingLawnRect.SCBlinkingLawnRectData(p);
        }

        public IEnumerable<UpdatableAndDeletable> RoomLoaded(Room room, PlacedObject placedObject, int itemIdx)
        {
            yield return new SCBlinkingLawnRect(room, placedObject);
        }
    }
}
