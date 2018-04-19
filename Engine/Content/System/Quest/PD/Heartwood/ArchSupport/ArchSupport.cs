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
    public class ArchSupport : MLQuest
    {
        public ArchSupport()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1073882; // Arch Support
            Description = 1074072; // How clever humans are - to understand the need of feet to rest from time to time!  Imagine creating a special stool just for weary toes.  I would like to examine and learn the secret of their making.  Would you bring me some foot stools to examine?
            RefusalMessage = 1073921; // I will patiently await your reconsideration.
            InProgressMessage = 1073928; // I will be in your debt if you bring me foot stools.
            CompletionMessage = 1073969; // My thanks for your service. Now, I will show you something of elven carpentry.

            Objectives.Add(new CollectObjective(10, typeof(FootStool), 1022910)); // foot stool

            Rewards.Add(ItemReward.CarpentrySatchel);
        }
    }
}