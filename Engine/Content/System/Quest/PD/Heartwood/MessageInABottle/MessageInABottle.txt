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
    public class MessageInABottleQuest : MLQuest
    {
        public MessageInABottleQuest()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1073894; // Message in a Bottle
            Description = 1074084; // We elves are interested in trading our wines with humans but we understand human usually trade such brew in strange transparent bottles. If you could provide some of these empty glass bottles, I might engage in a bit of elven winemaking.
            RefusalMessage = 1073921; // I will patiently await your reconsideration.
            InProgressMessage = 1073940; // I will be in your debt if you bring me empty bottles.
            CompletionMessage = 1073971; // My thanks for your service.  Here is something for you to enjoy.

            Objectives.Add(new CollectObjective(50, typeof(Bottle), 1023854)); // empty bottle

            Rewards.Add(ItemReward.TinkerSatchel);
        }
    }
}