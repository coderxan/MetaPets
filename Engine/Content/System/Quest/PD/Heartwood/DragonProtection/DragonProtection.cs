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
    public class DragonProtection : MLQuest
    {
        public DragonProtection()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1073915; // Dragon Protection
            Description = 1074105; // Mankind, I am told, knows how to take the scales of a terrible dragon and forge them into powerful armor. Such a feat of craftsmanship! I would give anything to view such a creation - I would even teach some of the prize secrets of the elven people.
            RefusalMessage = 1073921; // I will patiently await your reconsideration.
            InProgressMessage = 1073961; // I will be in your debt if you bring me dragon armor.
            CompletionMessage = 1073978; // Enjoy my thanks for your service.

            Objectives.Add(new CollectObjective(10, typeof(DragonHelm), 1029797)); // dragon helm

            Rewards.Add(ItemReward.BlacksmithSatchel);
        }
    }
}