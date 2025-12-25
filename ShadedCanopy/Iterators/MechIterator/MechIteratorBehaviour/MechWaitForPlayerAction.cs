using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ShadedCanopy.Iterators.MechIterator.MechIteratorBehaviour;

namespace ShadedCanopy.Iterators.MechIterator
{
    internal class MechWaitForPlayerAction : MechBehavAction
    {
        public MechWaitForPlayerAction(MechIteratorBehaviour owner) : base(owner)
        {
        }

        public override void Update()
        {
            var noticedPlayerPos = Iterator.room.game.FirstRealizedPlayer.firstChunk.pos;
            if (Vector2.Distance(noticedPlayerPos, Iterator.pos) < 300f)
            {
                owner.noticedPlayer = Iterator.room.game.FirstRealizedPlayer;
                Iterator.graphic.RequestSwitchAnimation(MechIteratorGraphic.AnimationID.LootAtPlayer);


                if (!owner.greetedPlayer)
                    owner.SwitchState(MechState.GreetPlayer);
                else
                    owner.SwitchState(MechState.StarePlayer);
            }
        }

        public override void OnActive(MechState old, MechState next)
        {
            base.OnActive(old, next);
            Iterator.graphic.RequestSwitchAnimation(MechIteratorGraphic.AnimationID.Idle);
        }

        public override bool MatchedMechState(MechState testState)
        {
            return testState == MechState.WaitForPlayer;
        }
    }
}
