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
    public class ForkedTongue : MLQuest
    {
        public ForkedTongue()
        {
            Activated = true;
            Title = 1073655; // Forked Tongue
            Description = 1073694; // I must implore you, brave traveler, to do battle with the vile reptiles which haunt these parts. Those hideous abominations, the Ophidians, are a blight across the land. If you were able to put down a host of the scaly warriors, the Knights or the Avengers, I would forever be in your debt.
            RefusalMessage = 1073733; // Perhaps you'll change your mind and return at some point.
            InProgressMessage = 1073735; // Have you killed the Ophidian Knights or Avengers?

            Objectives.Add(new KillObjective(10, new Type[] { typeof(OphidianKnight) }, "ophidian avengers or ophidian knight-errants"));

            Rewards.Add(ItemReward.LargeBagOfTreasure);
        }
    }
}