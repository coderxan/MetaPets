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
    public class FromTheGaultierCollection : MLQuest
    {
        public FromTheGaultierCollection()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1073905; // From the Gaultier Collection
            Description = 1074095; // It is my understanding, the females of humankind actually wear on certain occasions a studded bustier? This is not simply a fanciful tale? Remarkable! It sounds hideously uncomfortable as well as ludicrously impracticle. But perhaps, I simply do not understand the nuances of human clothing. Perhaps, I need to see such a studded bustier for myself?
            RefusalMessage = 1073921; // I will patiently await your reconsideration.
            InProgressMessage = 1073951; // I will be in your debt if you bring me studded bustiers.
            CompletionMessage = 1073976; // Truly, it is worse than I feared. Still, I appreciate your efforts on my behalf.

            Objectives.Add(new CollectObjective(10, typeof(StuddedBustierArms), 1027180)); // studded bustier

            Rewards.Add(ItemReward.TailorSatchel);
        }
    }
}