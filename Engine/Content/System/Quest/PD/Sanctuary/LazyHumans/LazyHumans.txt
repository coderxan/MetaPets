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
    public class LazyHumans : MLQuest
    {
        public LazyHumans()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1074024; // Lazy Humans
            Description = 1074118; // Human fancy knows no bounds!  It's pathetic that they are so weak that they must create a special stool upon which to rest their feet when they recline!  Humans don't have any clue how to live.  Bring me some of these foot stools to examine and I may teach you something worthwhile.
            RefusalMessage = 1074063; // Fine then, I'm shall find another to run my errands then.
            InProgressMessage = 1074064; // Hurry up! I don't have all day to wait for you to bring what I desire!
            CompletionMessage = 1074065; // These human made goods are laughable! It offends so -- I must show you what elven skill is capable of!

            Objectives.Add(new CollectObjective(10, typeof(FootStool), 1022910)); // foot stool

            Rewards.Add(ItemReward.CarpentrySatchel);
        }
    }
}