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
    public class ADishBestServedCold : MLQuest
    {
        public ADishBestServedCold()
        {
            Activated = true;
            Title = 1072372; // A Dish Best Served Cold
            Description = 1072657; // *mutter* I'll have my revenge.  Oh!  You there.  Fancy some orc extermination?  I despise them all.  Bombers, brutes -- you name it, if it's orcish I want it killed.
            RefusalMessage = 1072667; // Hrmph.  Well maybe another time then.
            InProgressMessage = 1072668; // Shouldn't you be slaying orcs?

            Objectives.Add(new KillObjective(10, new Type[] { typeof(Orc) }, "orcs", new QuestArea(1074807, "Sanctuary"))); // Sanctuary
            Objectives.Add(new KillObjective(5, new Type[] { typeof(OrcBomber) }, "orc bombers", new QuestArea(1074807, "Sanctuary"))); // Sanctuary
            Objectives.Add(new KillObjective(3, new Type[] { typeof(OrcBrute) }, "orc brutes", new QuestArea(1074807, "Sanctuary"))); // Sanctuary

            Rewards.Add(ItemReward.BagOfTreasure);
        }
    }
}