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
    public class HauteCouture : MLQuest
    {
        public HauteCouture()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1073901; // Hâute Couture
            Description = 1074091; // Most human apparel is interesting to elven eyes. But there is one garment - the flower garland - which sounds very elven indeed. Could I see how a human crafts such an object of beauty? In exchange, I could share with you the wonders of elven garments.
            RefusalMessage = 1073921; // I will patiently await your reconsideration.
            InProgressMessage = 1073947; // I will be in your debt if you bring me flower garlands.
            CompletionMessage = 1073973; // I appreciate your service. Now, see what elven hands can create.

            Objectives.Add(new CollectObjective(10, typeof(FlowerGarland), 1028965)); // flower garland

            Rewards.Add(ItemReward.TailorSatchel);
        }
    }
}