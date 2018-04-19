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
    public class AnimatedMonstrosity : MLQuest
    {
        public AnimatedMonstrosity()
        {
            Activated = true;
            Title = 1072990; // Animated Monstrosity
            Description = 1073020; // I bet you can't kill ... say twelve ... flesh golems!  I bet they're too much for you.  You may as well confess you can't ...
            RefusalMessage = 1073019; // Hahahaha!  I knew it!
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

            Objectives.Add(new KillObjective(12, new Type[] { typeof(FleshGolem) }, "flesh golems"));

            Rewards.Add(ItemReward.BagOfTrinkets);
        }
    }
}