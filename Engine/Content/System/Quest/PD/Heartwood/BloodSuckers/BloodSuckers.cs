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
    public class BloodSuckers : MLQuest
    {
        public BloodSuckers()
        {
            Activated = true;
            Title = 1072997; // Blood Suckers
            Description = 1073025; // I bet you can't tangle with those bloodsuckers ... say around ten vampire bats!  I bet they're too much for you.  You may as well confess you can't ...
            RefusalMessage = 1073019; // Hahahaha!  I knew it!
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

            Objectives.Add(new KillObjective(10, new Type[] { typeof(VampireBat) }, "vampire bats"));

            Rewards.Add(ItemReward.BagOfTrinkets);
        }
    }
}