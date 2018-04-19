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
    public class ItsAGhastlyJob : MLQuest
    {
        public ItsAGhastlyJob()
        {
            Activated = true;
            Title = 1073008; // It's a Ghastly Job
            Description = 1073012; // Why?  I ask you why?  They walk around after they're put in the ground.  It's just wrong in so many ways. Put them to proper rest, I beg you.  I'll find some way to pay you for the kindness. Just kill twelve ghouls.
            RefusalMessage = 1072270; // Well, okay. But if you decide you are up for it after all, c'mon back and see me.
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

            Objectives.Add(new KillObjective(12, new Type[] { typeof(Ghoul) }, "ghouls"));

            Rewards.Add(ItemReward.BagOfTrinkets);
        }
    }
}