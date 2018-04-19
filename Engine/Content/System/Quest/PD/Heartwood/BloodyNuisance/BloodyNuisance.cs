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
    public class BloodyNuisance : MLQuest
    {
        public BloodyNuisance()
        {
            Activated = true;
            Title = 1072992; // Bloody Nuisance
            Description = 1073021; // I bet you can't kill ... ten gore fiends!  I bet they're too much for you.  You may as well confess you can't ...
            RefusalMessage = 1073019; // Hahahaha!  I knew it!
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

            Objectives.Add(new KillObjective(10, new Type[] { typeof(GoreFiend) }, "gore fiends"));

            Rewards.Add(ItemReward.BagOfTrinkets);
        }
    }
}