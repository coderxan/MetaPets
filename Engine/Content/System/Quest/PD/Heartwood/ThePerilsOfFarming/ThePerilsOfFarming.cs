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
    public class ThePerilsOfFarming : MLQuest
    {
        public ThePerilsOfFarming()
        {
            Activated = true;
            Title = 1073664; // The Perils of Farming
            Description = 1073703; // I should be trimming back the vegetation here, but something nasty has taken root. Viscious vines I can't go near. If there's any hope of getting things under control, some one's going to need to destroy a few of those Whipping Vines. Someone strong and fast and tough.
            RefusalMessage = 1073733; // Perhaps you'll change your mind and return at some point.
            InProgressMessage = 1073744; // How are farmers supposed to work with these Whipping Vines around?

            Objectives.Add(new KillObjective(15, new Type[] { typeof(WhippingVine) }, "whipping vines"));

            Rewards.Add(ItemReward.LargeBagOfTreasure);
        }
    }
}