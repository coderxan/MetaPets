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
    public class IngeniousArcheryPartOne : MLQuest
    {
        public IngeniousArcheryPartOne()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1073878; // Ingenious Archery, Part I
            Description = 1074068; // I have heard of a curious type of bow, you call it a "crossbow". It sounds fascinating and I would very much like to examine one closely. Would you be able to obtain such an instrument for me?
            RefusalMessage = 1073921; // I will patiently await your reconsideration.
            InProgressMessage = 1073924; // I will be in your debt if you bring me crossbows.
            CompletionMessage = 1073968; // My thanks for your service. Now, I shall teach you of elven archery.
            CompletionNotice = CompletionNoticeCraft;

            Objectives.Add(new CollectObjective(10, typeof(Crossbow), 1023919)); // crossbow

            Rewards.Add(ItemReward.FletchingSatchel);
        }
    }
}