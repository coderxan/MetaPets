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
    public class BuildingTheBetterCrossbow : MLQuest
    {
        public BuildingTheBetterCrossbow()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1074022; // Building the Better Crossbow
            Description = 1074116; // More is always better for a human, eh? Take these repeating crossbows. What sort of mind invents such a thing? I must look at it more closely. Bring such a contraption to me and you'll receive a token for your efforts.
            RefusalMessage = 1074063; // Fine then, I'm shall find another to run my errands then.
            InProgressMessage = 1074064; // Hurry up! I don't have all day to wait for you to bring what I desire!
            CompletionMessage = 1074065; // These human made goods are laughable! It offends so -- I must show you what elven skill is capable of!

            Objectives.Add(new CollectObjective(10, typeof(RepeatingCrossbow), 1029923)); // repeating crossbow

            Rewards.Add(ItemReward.FletchingSatchel);
        }
    }
}