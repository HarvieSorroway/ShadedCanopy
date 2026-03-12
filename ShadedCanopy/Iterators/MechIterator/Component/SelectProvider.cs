using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShadedCanopy.Iterators.MechIterator.Component
{
    internal class SelectProvider : CosmeticSprite
    {
        public SelectProvider(Room room)
        {
            this.room = room;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
        }

        internal class Selection
        {
            SelectProvider provider;
            int jumpCount, noInputTimer;
            bool lastJump, jump;
            Rect sectionRect;

            public Selection(SelectProvider provider)
            {
                this.provider = provider;
            }
            public void Update()
            {
                lastJump = jump;
                jump = false;
                if (sectionRect.Contains(provider.room.game.FirstRealizedPlayer.DangerPos))
                {
                    //if(provider.room.game.FirstRealizedPlayer.animation == Player.AnimationIndex.)
                }
            }
        }
    }
}
