using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ShadedCanopy.Iterators.MechIterator.MechIteratorBehaviour;

namespace ShadedCanopy.Iterators.MechIterator
{
    internal class MechStarePlayerAction : MechBehavAction
    {
        public MechStarePlayerAction(MechIteratorBehaviour owner) : base(owner)
        {
        }

        public override void Update()
        {
            if(owner.noticedPlayer == null)
            {
                owner.SwitchState(MechState.WaitForPlayer);
                return;
            }
            var output = PlayerDistance();
            Iterator.graphic.lookAtPos = output.pos;
            if (output.distance > 500f)
            {
                owner.noticedPlayer = null;
                Iterator.graphic.RequestSwitchAnimation(MechIteratorGraphic.AnimationID.Idle);
                owner.SwitchState(MechState.WaitForPlayer);
            }
        }

        public override bool MatchedMechState(MechIteratorBehaviour.MechState testState)
        {
            return testState == MechIteratorBehaviour.MechState.StarePlayer;
        }
    }
}
