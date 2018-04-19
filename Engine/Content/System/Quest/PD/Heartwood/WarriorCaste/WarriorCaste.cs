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
    public class WarriorCaste : MLQuest
    {
        public WarriorCaste()
        {
            Activated = true;
            Title = 1073078; // Warrior Caste
            Description = 1073568; // The Terathan are an aggressive species. Left unchecked, they will swarm across our lands. And where will that leave us? Compost in the hive, that's what! Stop them, stop them cold my friend. Kill their warriors and you'll check their movement, that is certain.
            RefusalMessage = 1073580; // I hope you'll reconsider. Until then, farwell.
            InProgressMessage = 1073588; // Unless you kill at least 10 Terathan Warriors, you won't have any impact on their hive.

            Objectives.Add(new KillObjective(10, new Type[] { typeof(TerathanWarrior) }, "terathan warriors"));

            Rewards.Add(ItemReward.BagOfTreasure);
        }
    }
}