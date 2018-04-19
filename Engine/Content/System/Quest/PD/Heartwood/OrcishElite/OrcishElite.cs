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
    public class OrcishElite : MLQuest
    {
        public OrcishElite()
        {
            Activated = true;
            Title = 1073081; // Orcish Elite
            Description = 1073571; // Foul brutes! No one loves an orc, but some of them are worse than the rest. Their Captains and their Bombers, for instance, they're the worst of the lot. Kill a few of those, and the rest are just a rabble. Exterminate a few of them and you'll make the world a sunnier place, don't you know.
            RefusalMessage = 1073580; // I hope you'll reconsider. Until then, farwell.
            InProgressMessage = 1073591; // The only good orc is a dead orc - and 4 dead Captains and 6 dead Bombers is even better!

            Objectives.Add(new KillObjective(6, new Type[] { typeof(OrcBomber) }, "orc bombers"));
            Objectives.Add(new KillObjective(4, new Type[] { typeof(OrcCaptain) }, "orc captain"));

            Rewards.Add(ItemReward.BagOfTreasure);
        }
    }
}