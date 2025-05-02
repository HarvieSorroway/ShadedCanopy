using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShadedCanopy
{
    internal static class SCHelper
    {
        public static float EaseInOutCubic(float f)
        {
            return f < 0.5 ? 4 * f * f * f : 1 - Mathf.Pow(-2 * f + 2, 3) / 2;
        }
    }

    internal class IDLabel : CosmeticSprite
    {
        FLabel label;
        Creature bindCreature;
        string text;

        public IDLabel(Room room, Creature creature)
        {
            this.room = room;
            this.bindCreature = creature;
            var p = creature.abstractCreature.personality;
            text = $"agg:{p.aggression:F2}, brv{p.bravery:F2}, eng:{p.energy:F2}, sym:{p.sympathy:F2}";
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[0];
            label = new FLabel(Custom.GetFont(), text);
            rCam.ReturnFContainer("HUD").AddChild(label);

         
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (slatedForDeletetion)
                return;

            if(bindCreature.slatedForDeletetion || bindCreature.room != room)
            {
                Destroy();
                return;
            }

            pos = bindCreature.DangerPos + Vector2.up * 80f;
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (!sLeaser.deleteMeNextFrame && (base.slatedForDeletetion || room != rCam.room))
            {
                sLeaser.CleanSpritesAndRemove();
                Destroy();
            }

            var smoothPos = Vector2.Lerp(lastPos, pos, timeStacker);
            label.SetPosition(smoothPos);
        }

        public override void Destroy()
        {
            if (slatedForDeletetion)
                return;
            base.Destroy();

            label.RemoveFromContainer();
            bindCreature = null;
        }
    }
}
