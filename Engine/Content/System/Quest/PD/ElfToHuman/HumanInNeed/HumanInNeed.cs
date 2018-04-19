using System;
using System.Collections.Generic;
using System.Text;

using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.MLQuests.Definitions
{
    // This is not a real quest, it is only used as a reference
    public class HumanInNeed : MLQuest
    {
        public override bool RecordCompletion { get { return true; } }

        public HumanInNeed()
        {
            Title = 1075011; // A quest that asks you to defend a human in need.
            Description = 0;
            RefusalMessage = 0;
            InProgressMessage = 0;
        }

        public static void AwardTo(PlayerMobile pm)
        {
            MLQuestSystem.GetOrCreateContext(pm).SetDoneQuest(MLQuestSystem.FindQuest(typeof(HumanInNeed)));
            pm.SendLocalizedMessage(1074949, "", 0x2A); // You have demonstrated your compassion!  Your kind actions have been noted.
        }
    }
}