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
            var output = PlayerDistance();
            if (output.distance < 300f)
            {
                owner.noticedPlayer = Iterator.room.game.FirstRealizedPlayer;
                Iterator.graphic.RequestSwitchAnimation(MechIteratorGraphic.AnimationID.LootAtPlayer);

                owner.SwitchState(MechState.GreetPlayer);
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
