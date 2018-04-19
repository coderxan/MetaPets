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
    public class BirdsOfAFeather : MLQuest
    {
        public BirdsOfAFeather()
        {
            Activated = true;
            Title = 1073007; // Birds of a Feather
            Description = 1073022; // I bet you can't kill ... ten harpies!  I bet they're too much for you.  You may as well confess you can't ...
            RefusalMessage = 1073019; // Hahahaha!  I knew it!
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

            Objectives.Add(new KillObjective(10, new Type[] { typeof(Harpy) }, "harpies"));

            Rewards.Add(ItemReward.BagOfTrinkets);
        }
    }
}