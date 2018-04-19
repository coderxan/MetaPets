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
    public class TickTock : MLQuest
    {
        public TickTock()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1073907; // Tick Tock
            Description = 1074097; // Elves find it remarkable the human preoccupation with the passage of time. To have built instruments to try and capture time -- it is a fascinating notion. I would like to see how a clock is put together. Maybe you could provide some clocks for my experimentation?
            RefusalMessage = 1073921; // I will patiently await your reconsideration.
            InProgressMessage = 1073953; // I will be in your debt if you bring me clocks.
            CompletionMessage = 1073978; // Enjoy my thanks for your service.

            Objectives.Add(new CollectObjective(10, typeof(Clock), 1024171)); // clock

            Rewards.Add(ItemReward.TinkerSatchel);
        }
    }
}