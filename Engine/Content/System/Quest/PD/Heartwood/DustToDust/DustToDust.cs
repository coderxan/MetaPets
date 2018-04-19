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
    public class DustToDust : MLQuest
    {
        public DustToDust()
        {
            Activated = true;
            Title = 1073074; // Dust to Dust
            Description = 1073564; // You want to hear about trouble? I got trouble. How's angry piles of granite walking around for trouble? Maybe they don't like the mining, maybe it's the farming. I don't know. All I know is someone's got to turn them back to potting soil. And it'd be worth a pretty penny to the soul that does it.
            RefusalMessage = 1073580; // I hope you'll reconsider. Until then, farwell.
            InProgressMessage = 1073584; // You got rocks in your head? I said to kill 12 earth elementals, okay?

            Objectives.Add(new KillObjective(12, new Type[] { typeof(EarthElemental) }, "earth elementals"));

            Rewards.Add(ItemReward.BagOfTreasure);
        }
    }
}