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
    public class NothingFancy : MLQuest
    {
        public NothingFancy()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1073911; // Nothing Fancy
            Description = 1074101; // I am curious to see the results of human blacksmithing. To examine the care and quality of a simple item. Perhaps, a simple bascinet helmet? Yes, indeed -- if you could bring to me some bascinet helmets, I would demonstrate my gratitude.
            RefusalMessage = 1073921; // I will patiently await your reconsideration.
            InProgressMessage = 1073957; // I will be in your debt if you bring me bascinets.
            CompletionMessage = 1073978; // Enjoy my thanks for your service.

            Objectives.Add(new CollectObjective(15, typeof(Bascinet), 1025132)); // bascinet

            Rewards.Add(ItemReward.BlacksmithSatchel);
        }
    }
}