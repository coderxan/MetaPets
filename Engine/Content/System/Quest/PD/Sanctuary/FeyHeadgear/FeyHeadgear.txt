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
    public class FeyHeadgear : MLQuest
    {
        public FeyHeadgear()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1074043; // Fey Headgear
            Description = 1074137; // Humans do not deserve to wear a thing such as a flower garland. Help me prevent such things from falling into the clumsy hands of humans -- bring me flower garlands!
            RefusalMessage = 1074063; // Fine then, I'm shall find another to run my errands then.
            InProgressMessage = 1074064; // Hurry up! I don't have all day to wait for you to bring what I desire!
            CompletionMessage = 1074065; // These human made goods are laughable! It offends so -- I must show you what elven skill is capable of!

            Objectives.Add(new CollectObjective(10, typeof(FlowerGarland), 1028965)); // flower garland

            Rewards.Add(ItemReward.TailorSatchel);
        }
    }
}