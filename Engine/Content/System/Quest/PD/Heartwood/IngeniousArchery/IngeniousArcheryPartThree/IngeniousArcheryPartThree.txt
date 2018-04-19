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
    public class IngeniousArcheryPartThree : MLQuest
    {
        public IngeniousArcheryPartThree()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1073880; // Ingenious Archery, Part III
            Description = 1074070; // My friend, I am in search of a device, a instrument of remarkable human ingenuity. It is a repeating crossbow. If you were to obtain such a device, I would gladly reveal to you some of the secrets of elven craftsmanship.
            RefusalMessage = 1073921; // I will patiently await your reconsideration.
            InProgressMessage = 1073926; // I will be in your debt if you bring me repeating crossbows.
            CompletionMessage = 1073968; // My thanks for your service. Now, I shall teach you of elven archery.
            CompletionNotice = CompletionNoticeCraft;

            Objectives.Add(new CollectObjective(10, typeof(RepeatingCrossbow), 1029923)); // repeating crossbow

            Rewards.Add(ItemReward.FletchingSatchel);
        }
    }
}