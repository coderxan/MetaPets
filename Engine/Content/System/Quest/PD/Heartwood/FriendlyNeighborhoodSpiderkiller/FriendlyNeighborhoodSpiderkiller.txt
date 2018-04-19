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
    public class FriendlyNeighborhoodSpiderkiller : MLQuest
    {
        public FriendlyNeighborhoodSpiderkiller()
        {
            Activated = true;
            Title = 1073662; // Friendly Neighborhood Spider-killer
            Description = 1073701; // They aren't called Dread Spiders because they're fluffy and cuddly now, are they? No, there's nothing appealing about those wretches so I sure wouldn't lose any sleep if you were to exterminate a few. I'd even part with a generous amount of gold, I would.
            RefusalMessage = 1073733; // Perhaps you'll change your mind and return at some point.
            InProgressMessage = 1073742; // Dread Spiders? I say keep exterminating the arachnid vermin.

            Objectives.Add(new KillObjective(8, new Type[] { typeof(DreadSpider) }, "dread spiders"));

            Rewards.Add(ItemReward.LargeBagOfTreasure);
        }
    }
}