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
    public class IngeniousArcheryPartTwo : MLQuest
    {
        public IngeniousArcheryPartTwo()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1073879; // Ingenious Archery, Part II
            Description = 1074069; // These human "crossbows" are complex and clever. The "heavy crossbow" is a remarkable instrument of war. I am interested in seeing one up close, if you could arrange for one to make its way to my hands.
            RefusalMessage = 1073921; // I will patiently await your reconsideration.
            InProgressMessage = 1073925; // I will be in your debt if you bring me heavy crossbows.
            CompletionMessage = 1073968; // My thanks for your service. Now, I shall teach you of elven archery.
            CompletionNotice = CompletionNoticeCraft;

            Objectives.Add(new CollectObjective(8, typeof(HeavyCrossbow), 1025116)); // heavy crossbow

            Rewards.Add(ItemReward.FletchingSatchel);
        }
    }
}