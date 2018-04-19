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
    public class StopHarpingOnMe : MLQuest
    {
        public StopHarpingOnMe()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1073881; // Stop Harping on Me
            Description = 1074071; // Humans artistry can be a remarkable thing. For instance, I have heard of a wonderful instrument which creates the most melodious of music. A lap harp. I would be ever so grateful if I could examine one in person.
            RefusalMessage = 1073921; // I will patiently await your reconsideration.
            InProgressMessage = 1073927; // I will be in your debt if you bring me lap harp.
            CompletionMessage = 1073969; // My thanks for your service. Now, I will show you something of elven carpentry.

            Objectives.Add(new CollectObjective(20, typeof(LapHarp), 1023762)); // lap harp

            Rewards.Add(ItemReward.CarpentrySatchel);
        }
    }
}