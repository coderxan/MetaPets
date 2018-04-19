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
    public class ScaleArmor : MLQuest
    {
        public ScaleArmor()
        {
            Activated = true;
            Title = 1074711; // Scale Armor
            Description = 1074712; // Here's what I need ... there are some creatures called hydra, fearsome beasts, whose scales are especially suitable for a new sort of armor that I'm developing.  I need a few such pieces and then some supple alligator skin for the backing.  I'm going to need a really large piece that's shaped just right ... the tail I think would do nicely.  I appreciate your help.
            RefusalMessage = 1073580; // I hope you'll reconsider. Until then, farwell.
            InProgressMessage = 1074724; // Hydras have been spotted in the Blighted Grove.  You won't get those scales without getting your feet wet, I'm afraid.
            CompletionMessage = 1074725; // I can't wait to get to work now that you've returned with my scales.

            Objectives.Add(new CollectObjective(1, typeof(ThrashersTail), "Thrasher's Tail"));
            Objectives.Add(new CollectObjective(10, typeof(HydraScale), "Hydra Scales"));

            Rewards.Add(ItemReward.BagOfTreasure);
        }
    }
}