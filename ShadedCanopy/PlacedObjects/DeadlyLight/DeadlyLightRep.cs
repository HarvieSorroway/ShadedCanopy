using DevInterface;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShadedCanopy.PlacedObjects.DeadlyLight
{
    internal class DeadlyLightRep : ResizeableObjectRepresentation
    {
        public static Vector2 maxLightRangeLim = new Vector2(40f, 1200f);
        public static Vector2 penumbraWidthLim = new Vector2(10f, 1200f);
        public static Vector2 exposureLim = new Vector2(0f, 4f);
        public static Vector2 clampedDepthLim = new Vector2(0, 30);

        int lineSprite;
        public DeadlyLightRep(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj)
            : base(owner, IDstring, parentNode, pObj, "DeadlyLight", false)
        {
            subNodes.Add(new DeadlyLightPanel(owner, "Wawa_Token_Panel", this, new Vector2(0f, 100f)));
            (subNodes.Last() as DeadlyLightPanel).pos = (pObj.data as DeadlyLightData).panelPos;
            fSprites.Add(new FSprite("pixel", true));
            lineSprite = fSprites.Count - 1;
            owner.placedObjectsContainer.AddChild(fSprites[lineSprite]);
            fSprites[lineSprite].anchorY = 0f;
        }

        public override void Refresh()
        {
            base.Refresh();
            MoveSprite(lineSprite, absPos);
            fSprites[lineSprite].scaleY = (subNodes[1] as DeadlyLightPanel).pos.magnitude;
            fSprites[lineSprite].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[1] as DeadlyLightPanel).absPos);
            (pObj.data as DeadlyLightData).panelPos = (subNodes[1] as Panel).pos;
        }
    }

    internal class DeadlyLightPanel : Panel, IDevUISignals
    {
        static float height = 120f;
        DeadlyLightData LightData => (parentNode as DeadlyLightRep).pObj.data as DeadlyLightData;

        public DeadlyLightPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(250f, height), "DeadlyLight")
        {
            this.subNodes.Add(new DeadlyLightSlider(owner, "Light_Range_Slider", this, new Vector2(5f, height - 20f), "Light Range: ", 
                (f) =>
                {
                    LightData.maxLightRange = Mathf.Lerp(DeadlyLightRep.maxLightRangeLim.x, DeadlyLightRep.maxLightRangeLim.y, f);
                    return $"{LightData.maxLightRange:f2}";
                },
                () =>
                {
                    return Mathf.InverseLerp(DeadlyLightRep.maxLightRangeLim.x, DeadlyLightRep.maxLightRangeLim.y, LightData.maxLightRange);
                }));
            this.subNodes.Add(new DeadlyLightSlider(owner, "Penumbra_Width_Slider", this, new Vector2(5f, height - 40f), "Penumbra Width: ",
               (f) =>
               {
                   LightData.penumbraWidth = Mathf.Lerp(DeadlyLightRep.penumbraWidthLim.x, DeadlyLightRep.penumbraWidthLim.y, f);
                   return $"{LightData.penumbraWidth:f2}";
               }, 
               () =>
               {
                   return Mathf.InverseLerp(DeadlyLightRep.penumbraWidthLim.x, DeadlyLightRep.penumbraWidthLim.y, LightData.penumbraWidth);
               }));
            this.subNodes.Add(new DeadlyLightSlider(owner, "Exposure_Slider", this, new Vector2(5f, height - 60f), "Exposure: ",
               (f) =>
               {
                   LightData.exposure = Mathf.Lerp(DeadlyLightRep.exposureLim.x, DeadlyLightRep.exposureLim.y, f);
                   return $"{LightData.exposure:f2}";
               },
               () =>
               {
                   return Mathf.InverseLerp(DeadlyLightRep.exposureLim.x, DeadlyLightRep.exposureLim.y, LightData.exposure);
               }));
            this.subNodes.Add(new DeadlyLightSlider(owner, "ClampedDepth_Slider", this, new Vector2(5f, height - 80f), "Clamped Depth: ",
               (f) =>
               {
                   LightData.clampedDepth = Mathf.Clamp01(f);
                   return $"{(LightData.clampedDepth):f2}";
               },
               () =>
               {
                   return Mathf.Clamp01(LightData.clampedDepth);
               }));
        }

        public void Signal(DevUISignalType type, DevUINode sender, string message)
        {
        }

        class DeadlyLightSlider : Slider
        {
            Func<float, string> action;
            Func<float> refreshAction;
            public DeadlyLightSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title, Func<float, string> action, Func<float> refreshAction) : base(owner, IDstring, parentNode, pos, title, false, 110f)
            {
                this.action = action;
                this.refreshAction = refreshAction;
            }

            public override void NubDragged(float nubPos)
            {
                base.NubDragged(nubPos);
                NumberText = action.Invoke(nubPos);
            }

            public override void Refresh()
            {
                base.Refresh();

                float f = refreshAction.Invoke();
                NumberText = action.Invoke(f);
                base.RefreshNubPos(f);
            }
        }
    }
}
