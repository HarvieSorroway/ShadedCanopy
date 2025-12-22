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

        int timeInState;

        public MechIteratorBehaviour(MechIterator owner)
        {
            this.owner = owner;
        }

        public virtual void Update()
        {
            if(conversation != null)
            {
                conversation.Update();
                if (conversation.slatedForDeletion)
                    conversation = null;
            }

            if(noticedPlayer == null)
            {
                NoNoticedPlayerUpdate();
            }
            else
            {
                NoticedPlayerUpdate();
            }
            timeInState++;
        }

        void NoNoticedPlayerUpdate()
        {
            var noticedPlayerPos = owner.room.game.FirstRealizedPlayer.firstChunk.pos;
            if(Vector2.Distance(noticedPlayerPos, owner.pos) < 300f)
            {
                noticedPlayer = owner.room.game.FirstRealizedPlayer;
                owner.graphic.RequestSwitchAnimation(MechIteratorGraphic.AnimationID.LootAtPlayer);
                timeInState = 0;
            }
        }

        bool conversationAdded = false;
        void NoticedPlayerUpdate()
        {
            var noticedPlayerPos = owner.room.game.FirstRealizedPlayer.firstChunk.pos;
            owner.graphic.noticedPlayerPos = noticedPlayerPos;
            if (conversation == null && !conversationAdded && owner.graphic.ReadyForBehaviourAction)
            {
                conversation = new MechIteratorConversation(owner);
                conversation.AddEvent(new MechIteratorConversationTextEvent(conversation, 40, "...检测到计划外用户，已针对计划外用户校准语言系统"));
                conversation.AddEvent(new MechIteratorConversationTextEvent(conversation, 80, "...检测到计划外用户无法处理输入系统\n   将尝试生成全新适应性输入方案"));
                conversation.AddEvent(new MechIteratorConversationPauseEvent(conversation, 0, 80));
                conversation.AddEvent(new MechIteratorConversationTextEvent(conversation, 80, "...初始化已完成         \n   欢迎[用户#1BF52]"));
                conversation.AddEvent(new MechIteratorClearLabelEvent(conversation, 80)); 
                conversationAdded = true;
            }
            if (Vector2.Distance(noticedPlayerPos, owner.pos) > 500f)
            {
                noticedPlayer = null;
                owner.graphic.RequestSwitchAnimation(MechIteratorGraphic.AnimationID.Idle);
                timeInState = 0;
            }
        }
    }
}
