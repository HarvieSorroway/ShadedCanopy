using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadedCanopy.Iterators.MechIterator
{
    internal class MechIteratorConversation
    {
        MechIterator iterator;
        List<MechInteratorConversationEvent> events = new List<MechInteratorConversationEvent>();

        public bool slatedForDeletion = false;
        public bool paused;

        public MechIteratorConversation(MechIterator iterator)
        {
            this.iterator = iterator;
        }

        public void Update()
        {
            if (paused)
            {
                return;
            }
            if (events.Count == 0)
            {
                Destroy();
                return;
            }
            events[0].Update();
            if (events[0].Finished)
            {
                events.RemoveAt(0);
            }
        }

        public void Destroy()
        {
            slatedForDeletion = true;
        }

        public void AddEvent(MechInteratorConversationEvent newEvent)
        {
            events.Add(newEvent);
        }

        public void Interrupt()
        {
            events[0].Reset();
            iterator.TryTurnOffCurrentLabel();
            paused = true;
        }

        public void Recover()
        {
            paused = false;
        }

        public void InterruptWithRecoverConv(MechInteratorConversationEvent newEvent)
        {
            Interrupt();
            events.Insert(0, newEvent);
        }

        public class MechInteratorConversationEvent
        {
            public MechIteratorConversation owner;
            public int initialWait;

            public virtual bool Finished => true;
            public bool isActivated = false;
            public int timeInEvent;

            public MechInteratorConversationEvent(MechIteratorConversation owner, int initialWait)
            {
                this.owner = owner;
                this.initialWait = initialWait;
            }

            public virtual void Activate()
            {
                isActivated = true;
            }

            public virtual void Update()
            {
                if (!isActivated && timeInEvent == initialWait)
                {
                    Activate();
                }
                timeInEvent++;
            }

            public virtual void Reset()
            {
                isActivated = false;
                timeInEvent = 0;
            }
        }


        public class MechIteratorConversationTextEvent : MechInteratorConversationEvent
        {
            string text;
            bool waitForLabelClear, textAllRevealed;

            public override bool Finished => textAllRevealed;

            public MechIteratorConversationTextEvent(MechIteratorConversation owner, int initialWait, string origText) : base(owner, initialWait)
            {
                //TODO:处理文本
                this.text = origText;
            }

            public override void Activate()
            {
                if (owner.iterator.TryStartNewConvLabel(text))
                {
                    isActivated = true;
                }
                else
                {
                    waitForLabelClear = true;
                    return;
                }
            }

            public override void Update()
            {
                if (!isActivated)
                {
                    if (timeInEvent == initialWait)
                        Activate();
                    if (waitForLabelClear && owner.iterator.currentLiveLabel == null)
                        Activate();
                }
                else if (!textAllRevealed)
                {
                    owner.iterator.currentLiveLabel.revealProgression += 8 / 40f;
                    if(owner.iterator.currentLiveLabel.revealProgression >= owner.iterator.currentLiveLabel.MaxRevealProgression)
                    {
                        textAllRevealed = true;
                    }
                }
                timeInEvent++;
            }

            public override void Reset()
            {
                textAllRevealed = false;
                owner.iterator.TryTurnOffCurrentLabel();
                base.Reset();
            }
        }

        public class MechIteratorConversationPauseEvent : MechInteratorConversationEvent
        {
            int pauseDuration;
            public override bool Finished => timeInEvent >= pauseDuration;
            public MechIteratorConversationPauseEvent(MechIteratorConversation owner, int initialWait, int pauseDuration) : base(owner, initialWait)
            {
                this.pauseDuration = pauseDuration;
            }
        }

        public class MechIteratorClearLabelEvent : MechInteratorConversationEvent
        {
            public override bool Finished => isActivated;
            public MechIteratorClearLabelEvent(MechIteratorConversation owner, int initialWait) : base(owner, initialWait)
            {
            }

            public override void Activate()
            {
                base.Activate();
                owner.iterator.TryTurnOffCurrentLabel();
            }
        }

    }
}
