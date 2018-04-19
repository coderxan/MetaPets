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
    public class MoltenReptiles : MLQuest
    {
        public MoltenReptiles()
        {
            Activated = true;
            Title = 1072989; // Molten Reptiles
            Description = 1073018; // I bet you can't kill ... say ten ... lava lizards!  I bet they're too much for you.  You may as well confess you can't ...
            RefusalMessage = 1073019; // Hahahaha!  I knew it!
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

            Objectives.Add(new KillObjective(10, new Type[] { typeof(LavaLizard) }, "lava lizards"));

            Rewards.Add(ItemReward.BagOfTrinkets);
        }
    }
}