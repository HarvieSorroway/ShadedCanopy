using SCUtils.SCDevTools.NodeTreeManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShadedCanopy.Iterators.MechIterator
{
    [SCDevToolsInspectType("Root.RainWorld.Game.World.Room","MechIterator")]
    internal class MechIterator : UpdatableAndDeletable, IDrawable
    {
        public MechIteratorGraphic graphic;

        [SCDevToolsInspectValue] 
        public Vector2 pos = new Vector2(500f, 400f);

        //[SCDevToolsInspectValue]
        public int FibonacciSphereCastCount = 100;

        [SCDevToolsInspectValue]
        [SCDevToolsRangeField(10f, 1000f)]
        public float FibonacciSphereCastRad = 60f;

        [SCDevToolsInspectValue]
        [SCDevToolsRangeField(0f, 1f)]
        public float Expand = 1f;


        [SCDevToolsInspectValue]
        [SCDevToolsRangeField(-90f, 90f)]
        public float RotX = 0f;

        [SCDevToolsInspectValue]
        [SCDevToolsRangeField(-90f, 90f)]
        public float RotY = 0f;

        [SCDevToolsInspectValue]
        [SCDevToolsRangeField(-90f, 90f)]
        public float RotZ = 0f;
        public MechIterator(Room room)
        {
            graphic = new MechIteratorGraphic(this);
            this.room = room;
            SCDevNodeTreeManager.Track(this);

            room.AddObject(new MechIteratorGraphic.ProjTextLabel(room, "This is test text.", pos + new Vector2(100f, 0f)));
        }



        public override void Update(bool eu)
        {
            base.Update(eu);
            if (slatedForDeletetion)
                return;
            graphic.Update();
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
    }
}
