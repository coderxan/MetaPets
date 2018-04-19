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
    public class Squishy : MLQuest
    {
        public Squishy()
        {
            Activated = true;
            Title = 1072998; // Squishy
            Description = 1073031; // Have you ever seen what a slime can do to good gear?  Well, it's not pretty, let me tell you!  If you take on my task to destroy twelve of them, bear that in mind.  They'll corrode your equipment faster than anything.
            RefusalMessage = 1072270; // Well, okay. But if you decide you are up for it after all, c'mon back and see me.
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

            Objectives.Add(new KillObjective(12, new Type[] { typeof(Slime) }, "slimes"));

            Rewards.Add(ItemReward.BagOfTrinkets);
        }
    }
}