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
    public class WaitingToBeFilled : MLQuest
    {
        public WaitingToBeFilled()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1074036; // Waiting to be Filled
            Description = 1074130; // The only good thing I can say about human made bottles is that they are empty and may yet still be filled with elven wine. Go now, fetch a number of empty bottles so that I might save them from the fate of carrying human-made wine.
            RefusalMessage = 1074063; // Fine then, I'm shall find another to run my errands then.
            InProgressMessage = 1074064; // Hurry up! I don't have all day to wait for you to bring what I desire!
            CompletionMessage = 1074065; // These human made goods are laughable! It offends so -- I must show you what elven skill is capable of!

            Objectives.Add(new CollectObjective(20, typeof(Bottle), 1023854)); // empty bottle

            Rewards.Add(ItemReward.TinkerSatchel);
        }
    }
}