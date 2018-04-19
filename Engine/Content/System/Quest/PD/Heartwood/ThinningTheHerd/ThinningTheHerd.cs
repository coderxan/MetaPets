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
    public class ThinningTheHerd : MLQuest
    {
        public ThinningTheHerd()
        {
            Activated = true;
            Title = 1072249; // Thinning the Herd
            Description = 1072263; // Psst!  Hey ... psst!  Listen, I need some help here but it's gotta be hush hush.  I don't want THEM to know I'm onto them.  They watch me.  I've seen them, but they don't know that I know what I know.  You know?  Anyway, I need you to scare them off by killing a few of them.  That'll send a clear message that I won't suffer goats watching me!
            RefusalMessage = 1072270; // Well, okay. But if you decide you are up for it after all, c'mon back and see me.
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

            Objectives.Add(new KillObjective(10, new Type[] { typeof(Goat) }, "goats"));

            Rewards.Add(ItemReward.SmallBagOfTrinkets);
        }
    }
}