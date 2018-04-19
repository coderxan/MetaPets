using System;
using System.Collections.Generic;

using Server;
using Server.ContextMenus;
using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Items;
using Server.Misc;
using Server.Mobiles;

namespace Server.Engines.MLQuests.Definitions
{
    public class BrotherlyLove : MLQuest
    {
        public BrotherlyLove()
        {
            Activated = true;
            OneTimeOnly = true;
            Title = 1072369; // Brotherly Love
            Description = 1072585; // *looks around nervously*  Do you travel to The Heartwood?  I have an urgent letter that must be delivered there in the next 30 minutes -- to Ahie the Cloth Weaver.  Will you undertake this journey?
            RefusalMessage = 1072587; // *looks disappointed* Let me know if you change your mind.
            InProgressMessage = 1072588; // You haven't lost the letter have you?  It must be delivered to Ahie directly.  Give it into no other hands.
            CompletionMessage = 1074579; // Yes, can I help you?
            CompletionNotice = CompletionNoticeShort;

            Objectives.Add(new DeliverObjective(typeof(APersonalLetterAddressedToAhie), 1, "letter", typeof(Ahie)));

            Rewards.Add(ItemReward.BagOfTrinkets);
        }
    }
}