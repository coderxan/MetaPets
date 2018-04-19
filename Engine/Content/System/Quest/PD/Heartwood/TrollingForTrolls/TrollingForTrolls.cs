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
    public class TrollingForTrolls : MLQuest
    {
        public TrollingForTrolls()
        {
            Activated = true;
            Title = 1072985; // Trolling for Trolls
            Description = 1073014; // They may not be bright, but they're incredibly destructive. Kill off ten trolls and I'll consider it a favor done for me.
            RefusalMessage = 1072270; // Well, okay. But if you decide you are up for it after all, c'mon back and see me.
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

            Objectives.Add(new KillObjective(10, new Type[] { typeof(Troll) }, "trolls"));

            Rewards.Add(ItemReward.BagOfTrinkets);
        }
    }
}