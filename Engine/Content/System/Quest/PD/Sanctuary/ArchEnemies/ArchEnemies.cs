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
    public class ArchEnemies : MLQuest
    {
        public ArchEnemies()
        {
            Activated = true;
            Title = 1073085; // Arch Enemies
            Description = 1073575; // Vermin! They get into everything! I told the boy to leave out some poisoned cheese -- and they shot him. What else can I do? Unless…these ratmen are skilled with a bow, but I'd lay a wager you're better, eh? Could you skin a few of the wretches for me?
            RefusalMessage = 1073580; // I hope you'll reconsider. Until then, farwell.
            InProgressMessage = 1073595; // I don't see 10 tails from Ratman Archers on your belt -- and until I do, no reward for you.

            Objectives.Add(new KillObjective(10, new Type[] { typeof(RatmanArcher) }, "ratman archers"));

            Rewards.Add(ItemReward.BagOfTreasure);
        }
    }
}