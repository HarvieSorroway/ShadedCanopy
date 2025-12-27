using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ShadedCanopy.Iterators.MechIterator.MechIteratorBehaviour;
using static ShadedCanopy.Iterators.MechIterator.MechIteratorConversation;

namespace ShadedCanopy.Iterators.MechIterator
{
    internal class MechNoticeNameChangeAction : MechBehavAction
    {
        bool conversationAdded;
        public MechNoticeNameChangeAction(MechIteratorBehaviour owner) : base(owner)
        {
        }

        public override void Update()
        {
            var noticedPlayerPos = Iterator.room.game.FirstRealizedPlayer.firstChunk.pos;
            Iterator.graphic.lookAtPos = noticedPlayerPos;

            if (Conversation == null && !conversationAdded && Iterator.graphic.ReadyForBehaviourAction)
            {
                NoticePlayerNameChangeConv();
                return;
            }
            if (conversationAdded && Conversation == null)
            {
                if(owner.nextStateAfterAction != null)
                    owner.SwitchState(owner.PopNextState());
                else
                    owner.SwitchState(MechState.StarePlayer);
            }

        }

        void NoticePlayerNameChangeConv()
        {
            Conversation = new MechIteratorConversation(Iterator);
            if (MiscData.playerNamePrefix == "Little Creature")
            {
                Conversation.AddEvent(new MechIteratorConversationTextEvent(Conversation, 40, "...检测到用户权限变更,权限等级已被升级"));
                Conversation.AddEvent(new MechIteratorConversationTextEvent(Conversation, 80, $"...同时用户名被修改为[{PlayernName}]\n此后我也将改为使用此称呼"));
                Conversation.AddEvent(new MechIteratorConversationPauseEvent(Conversation, 0, 40));
                Conversation.AddEvent(new MechIteratorConversationTextEvent(Conversation, 80, $"...欢迎回来，[{PlayernName}]"));
                Conversation.AddEvent(new MechIteratorClearLabelEvent(Conversation, 80));
            }

            MiscData.meetThisCycle = true;
            conversationAdded = true;
        }
    }
}
