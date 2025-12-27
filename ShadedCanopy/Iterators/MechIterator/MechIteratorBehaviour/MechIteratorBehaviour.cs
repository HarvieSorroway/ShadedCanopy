using SCUtils.SCDevTools.NodeTreeManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ShadedCanopy.Iterators.MechIterator.MechIteratorConversation;

namespace ShadedCanopy.Iterators.MechIterator
{
    
    internal class MechIteratorBehaviour
    {
        public MechIterator owner;

        public MechIteratorConversation conversation;

        public Player noticedPlayer;

        //状态相关
        internal int timeInAction;
        internal MechState state = MechState.WaitForPlayer;
        internal MechState nextStateAfterAction;//下一个状态，用于特殊action自动切换状态
        public bool greetedPlayer;

        MechBehavAction behavAction;

        public MechIteratorBehaviour(MechIterator owner)
        {
            this.owner = owner;
            SwitchState(MechState.WaitForPlayer, true);
            SCDevNodeTreeManager.Track(this);
        }

        public virtual void Update()
        {
            if(conversation != null)
            {
                conversation.Update();
                if (conversation.slatedForDeletion)
                    conversation = null;
            }

            behavAction?.Update();
            timeInAction++;
        }

        public void SwitchState(MechState newState, bool forceNew = false)
        {
            if (state == newState && !forceNew)
                return;

            MechBehavAction nextAction = null;

            if (behavAction != null && behavAction.MatchedMechState(newState))
            {
                nextAction = behavAction;
            }
            else
            {
                if (newState == MechState.WaitForPlayer)
                    nextAction = new MechWaitForPlayerAction(this);
                else if (newState == MechState.GreetPlayer)
                    nextAction = new MechGreetPlayerAction(this);  
                else if (newState == MechState.StarePlayer)
                    nextAction = new MechStarePlayerAction(this);
            }

            if(nextAction != behavAction)
            {
                if(behavAction != null)
                    behavAction.SwitchTo(state, newState);
                behavAction = nextAction;
            }

            behavAction.OnActive(state, newState);
            state = newState;

            timeInAction = 0;
        }

        public MechState PopNextState()
        {
            var temp = nextStateAfterAction;
            nextStateAfterAction = null;
            return temp;
        }



        internal class MechState : ExtEnum<MechState>
        {
            public static readonly MechState WaitForPlayer = new MechState("WaitForPlayer", true);//等待玩家靠近
            public static readonly MechState GreetPlayer   = new MechState("GreetPlayer"  , true);//本循环首次见到玩家，打招呼
            public static readonly MechState StarePlayer   = new MechState("StarePlayer"  , true);//看玩家但什么也不做


            public MechState(string id, bool register = false) : base(id, register)
            {
            }
        }
    }
}
