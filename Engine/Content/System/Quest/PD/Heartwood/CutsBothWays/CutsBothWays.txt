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
    public class CutsBothWays : MLQuest
    {
        public CutsBothWays()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1073913; // Cuts Both Ways
            Description = 1074103; // What would you say is a typical human instrument of war? Is a broadsword a typical example? I wish to see more of such human weapons, so I would gladly trade elven knowledge for human steel.
            RefusalMessage = 1073921; // I will patiently await your reconsideration.
            InProgressMessage = 1073959; // I will be in your debt if you bring me broadswords.
            CompletionMessage = 1073978; // Enjoy my thanks for your service.

            Objectives.Add(new CollectObjective(12, typeof(Broadsword), 1023934)); // broadsword

            Rewards.Add(ItemReward.BlacksmithSatchel);
        }
    }
}