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
    public class LeatherAndLace : MLQuest
    {
        public LeatherAndLace()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1074047; // Leather and Lace
            Description = 1074141; // No self respecting elf female would ever wear a studded bustier! I will prove it - bring me such clothing and I will show you how ridiculous they are!
            RefusalMessage = 1074063; // Fine then, I'm shall find another to run my errands then.
            InProgressMessage = 1074064; // Hurry up! I don't have all day to wait for you to bring what I desire!
            CompletionMessage = 1074065; // These human made goods are laughable! It offends so -- I must show you what elven skill is capable of!

            Objectives.Add(new CollectObjective(10, typeof(StuddedBustierArms), 1027180)); // studded bustier

            Rewards.Add(ItemReward.TailorSatchel);
        }
    }
}