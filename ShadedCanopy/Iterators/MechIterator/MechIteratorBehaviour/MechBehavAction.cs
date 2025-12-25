using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadedCanopy.Iterators.MechIterator
{
    internal class MechBehavAction
    {
        protected MechIteratorBehaviour owner;

        public MechIteratorSaveData MiscData => MechIteratorMiscSaveManager.Data;
        public MechIterator Iterator => owner.owner;

        public int TimeInAction => owner.timeInAction;
        public MechIteratorConversation Conversation
        {
            get => owner.conversation;
            set => owner.conversation = value;
        }

        public MechBehavAction(MechIteratorBehaviour owner)
        {
            this.owner = owner;
        }
        

        public virtual void Update()
        {
        }

        public virtual void SwitchTo(MechIteratorBehaviour.MechState old, MechIteratorBehaviour.MechState next)
        {
        }

        public virtual void OnActive(MechIteratorBehaviour.MechState old, MechIteratorBehaviour.MechState next)
        {
        }

        public virtual bool MatchedMechState(MechIteratorBehaviour.MechState testState)
        {
            throw new NotImplementedException();
        }
    }
}
