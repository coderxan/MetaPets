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
    public class NoGoodFishStealing : MLQuest
    {
        public NoGoodFishStealing()
        {
            Activated = true;
            Title = 1072251; // No Good, Fish Stealing ...
            Description = 1072265; // Mighty creatures they are, aye.  Fierce and strong, can't blame 'em for wanting to feed themselves an' all. Blame or no, they're eating all the fish up, so they got to go.  Lend a hand?
            RefusalMessage = 1072270; // Well, okay. But if you decide you are up for it after all, c'mon back and see me.
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

            Objectives.Add(new KillObjective(10, new Type[] { typeof(Walrus) }, "walruses"));

            Rewards.Add(ItemReward.SmallBagOfTrinkets);
        }
    }
}