using SCUtils;
using SCUtils.SCDevTools.NodeTreeManager;
using SCUtils.SCTween;
using SCUtils.RwTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ShadedCanopy.Iterators.MechIterator.Component;

namespace ShadedCanopy.Iterators.MechIterator
{
    
    internal class MechIterator : UpdatableAndDeletable, IDrawable
    {
        public MechIteratorGraphic graphic;
        public MechIteratorBehaviour behaviour;

        public Vector2 pos = new Vector2(683f, 365f);

        public LightSource lightSource;

        public ProjTextLabel currentLiveLabel;


        public MechIterator(Room room)
        {
            this.room = room;

            graphic = new MechIteratorGraphic(this);
            behaviour = new MechIteratorBehaviour(this);

            lightSource = new LightSource(pos, true, Color.cyan * 0.5f + Color.blue * 0.5f, this);
            room.AddObject(lightSource);

       
            //SCTween.TweenVector2((val) => this.pos = val,new Vector2(500f, 400f), new Vector2(800f, 400f), 3f)
            //    .SetEase(SCHelperUtils.EaseInOutCubic)
            //    .RunAsync()
            //    .Forget();
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (slatedForDeletetion)
                return;
            graphic.Update();
            behaviour.Update();
            if (currentLiveLabel != null && currentLiveLabel.slatedForDeletetion)
                currentLiveLabel = null;
            graphic.talkFlicker = (currentLiveLabel != null && currentLiveLabel.revealProgression < currentLiveLabel.MaxRevealProgression) ? 1f : 0f;


            lightSource.setPos = pos;
            lightSource.rad = 600f;
            lightSource.alpha = 1f;
        }

        #region DrawFunctions
        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            graphic.AddToContainer(sLeaser, rCam, newContatiner);
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            graphic.ApplyPalette(sLeaser, rCam, palette);
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            graphic.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            graphic.InitiateSprites(sLeaser, rCam);
        }
        #endregion

        public bool TryStartNewConvLabel(string convText)
        {
            if (currentLiveLabel != null)
            {
                TryTurnOffCurrentLabel();
                return false;
            }
            else
            {
                currentLiveLabel = new ProjTextLabel(room, convText, pos + new Vector2(140f, 0f));
                room.AddObject(currentLiveLabel);
                return true;
            }
        }

        public void TryTurnOffCurrentLabel()
        {
            if(currentLiveLabel != null)
            {
                currentLiveLabel.TurnOff();
            }
        }


        
    }
}
