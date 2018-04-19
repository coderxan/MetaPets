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
    public class LethalDarts : MLQuest
    {
        public LethalDarts()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1073876; // Lethal Darts
            Description = 1074066; // We elves are no strangers to archery but I would be interested in learning whether there is anything to learn from the human approach. I would gladly trade you something I have if you could teach me of the deadly crossbow bolt.
            RefusalMessage = 1073921; // I will patiently await your reconsideration.
            InProgressMessage = 1073922; // I will be in your debt if you bring me crossbow bolts.
            CompletionMessage = 1073968; // My thanks for your service. Now, I shall teach you of elven archery.
            CompletionNotice = CompletionNoticeCraft;

            Objectives.Add(new CollectObjective(10, typeof(Bolt), 1027163)); // crossbow bolt

            Rewards.Add(ItemReward.FletchingSatchel);
        }
    }
}