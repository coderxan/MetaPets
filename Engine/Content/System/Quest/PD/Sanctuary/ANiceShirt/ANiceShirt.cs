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
    public class ANiceShirt : MLQuest
    {
        public ANiceShirt()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1074045; // A Nice Shirt
            Description = 1074139; // Humans call that a fancy shirt? I would wager the ends are frayed, the collar worn, the buttons loosely stitched. Bring me fancy shirts and I will demonstrate the many ways in which they are inferior.
            RefusalMessage = 1074063; // Fine then, I'm shall find another to run my errands then.
            InProgressMessage = 1074064; // Hurry up! I don't have all day to wait for you to bring what I desire!
            CompletionMessage = 1074065; // These human made goods are laughable! It offends so -- I must show you what elven skill is capable of!

            Objectives.Add(new CollectObjective(10, typeof(FancyShirt), 1027933)); // fancy shirt

            Rewards.Add(ItemReward.TailorSatchel);
        }
    }
}