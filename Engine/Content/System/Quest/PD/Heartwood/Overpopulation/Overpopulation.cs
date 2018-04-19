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
    public class Overpopulation : MLQuest
    {
        public Overpopulation()
        {
            Activated = true;
            Title = 1072252; // Overpopulation
            Description = 1072267; // I just can't bear it any longer.  Sure, it's my job to thin the deer out so they don't overeat the area and starve themselves come winter time.  Sure, I know we killed off the predators that would do this naturally so now we have to make up for it.  But they're so graceful and innocent.  I just can't do it. Will you?
            RefusalMessage = 1072270; // Well, okay. But if you decide you are up for it after all, c'mon back and see me.
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

            Objectives.Add(new KillObjective(10, new Type[] { typeof(Hind) }, "hinds"));

            Rewards.Add(ItemReward.SmallBagOfTrinkets);
        }
    }
}