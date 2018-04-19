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
    public class DeathToTheNinja : MLQuest
    {
        public DeathToTheNinja()
        {
            Activated = true;
            Title = 1072913; // Death to the Ninja!
            Description = 1072966; // I wish to make a statement of censure against the elite ninjas of the Black Order.  Deliver, in the strongest manner, my disdain.  But do not make war on women, even those that take arms against you.  It is not ... fitting.
            RefusalMessage = 1072979; // As you wish.
            InProgressMessage = 1072980; // The Black Order's fortress home is well hidden.  Legend has it that a humble fishing village disguises the magical portal.

            // TODO: Verify that this has to be males only (as per the description)
            Objectives.Add(new KillObjective(10, new Type[] { typeof(EliteNinja) }, "elite ninjas", new QuestArea(1074804, "The Citadel"))); // The Citadel

            Rewards.Add(ItemReward.BagOfTreasure);
        }
    }
}