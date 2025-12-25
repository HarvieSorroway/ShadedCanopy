using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ShadedCanopy.Iterators.MechIterator.MechIteratorBehaviour;
using static ShadedCanopy.Iterators.MechIterator.MechIteratorConversation;

namespace ShadedCanopy.Iterators.MechIterator
{
    internal class MechGreetPlayerAction : MechBehavAction
    {
        bool conversationAdded, conversationInterrupted;


        public MechGreetPlayerAction(MechIteratorBehaviour owner) : base(owner)
        {
        }

        public override void Update()
        {
            var noticedPlayerPos = Iterator.room.game.FirstRealizedPlayer.firstChunk.pos;
            Iterator.graphic.lookAtPos = noticedPlayerPos;

            if (!owner.greetedPlayer)
                owner.greetedPlayer = true;


            if (Conversation == null && !conversationAdded && Iterator.graphic.ReadyForBehaviourAction)
            {
                if (MiscData.meetThisCycle)
                    StartCycleDualGreetConversation();
                else
                    StartCycleFirstGreetConversation();
            }
            else if (Conversation != null && Conversation.paused && !conversationAdded && Iterator.graphic.ReadyForBehaviourAction)
            {
                Conversation.Recover();
                conversationAdded = true;
            }

            if (conversationAdded && Conversation == null)
            {
                owner.SwitchState(MechState.StarePlayer);
            }


            if (Vector2.Distance(noticedPlayerPos, Iterator.pos) > 500f)
            {
                owner.noticedPlayer = null;
                conversationAdded = false;

                if (Conversation != null && !Conversation.slatedForDeletion)
                {
                    Conversation.InterruptWithRecoverConv(new MechIteratorConversationTextEvent(Conversation, 0, "...欢迎回来，请允许我继续"));
                    owner.greetedPlayer = false;
                }

                Iterator.graphic.RequestSwitchAnimation(MechIteratorGraphic.AnimationID.Idle);
                owner.SwitchState(MechState.WaitForPlayer);
            }
        }

        void StartCycleFirstGreetConversation()
        {
            Conversation = new MechIteratorConversation(Iterator);
            if (MiscData.meets == 0)//只在获得许可后第一次见面时播放
            {
                Conversation.AddEvent(new MechIteratorConversationTextEvent(Conversation, 40, "...检测到计划外用户，已针对计划外用户校准语言系统"));
                Conversation.AddEvent(new MechIteratorConversationTextEvent(Conversation, 80, "...检测到计划外用户无法处理输入系统\n   将尝试生成全新适应性输入方案"));
                Conversation.AddEvent(new MechIteratorConversationPauseEvent(Conversation, 0, 80));
                Conversation.AddEvent(new MechIteratorConversationTextEvent(Conversation, 80, "...初始化已完成         \n   欢迎[用户#1BF52]"));
                Conversation.AddEvent(new MechIteratorClearLabelEvent(Conversation, 80));
            }
            else
            {
                int selected = UnityEngine.Random.Range(0, 2);
                if(selected == 0)
                {
                    Conversation.AddEvent(new MechIteratorConversationTextEvent(Conversation, 40, "...欢迎回来，[用户#1BF52]"));
                }
                else if(selected == 1)
                {
                    Conversation.AddEvent(new MechIteratorConversationTextEvent(Conversation, 40, "...检测到先前用户资料，欢迎回来，[用户#1BF52]"));
                }
                else if(selected == 2)
                {
                    Conversation.AddEvent(new MechIteratorConversationTextEvent(Conversation, 40, "...欢迎回来，[用户#1BF52]，很高兴再次见到您"));
                }
                Conversation.AddEvent(new MechIteratorClearLabelEvent(Conversation, 80));
            }

            MiscData.meetThisCycle = true;
            conversationAdded = true;
        }

        void StartCycleDualGreetConversation()
        {
            Conversation = new MechIteratorConversation(Iterator);
            int selected = UnityEngine.Random.Range(0, 2);
            if(selected == 0)
            {
                Conversation.AddEvent(new MechIteratorConversationTextEvent(Conversation, 40, "...检测到用户再次返回，请问需要什么帮助？"));
            }
            else if(selected == 1)
            {
                Conversation.AddEvent(new MechIteratorConversationTextEvent(Conversation, 40, "...欢迎回来，[用户#1BF52]，需要我为您做些什么吗？"));
            }
            else if(selected == 2)
            {
                Conversation.AddEvent(new MechIteratorConversationTextEvent(Conversation, 40, "...很高兴再次见到您，[用户#1BF52]，正在分析可能的需求"));
            }
            Conversation.AddEvent(new MechIteratorClearLabelEvent(Conversation, 80));
            conversationAdded = true;
        }

        public override bool MatchedMechState(MechState testState)
        {
            return testState == MechState.GreetPlayer;
        }
    }
}
