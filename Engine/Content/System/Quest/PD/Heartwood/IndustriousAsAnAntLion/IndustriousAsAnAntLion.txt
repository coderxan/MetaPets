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
    public class IndustriousAsAnAntLion : MLQuest
    {
        public IndustriousAsAnAntLion()
        {
            Activated = true;
            Title = 1073665; // Industrious as an Ant Lion
            Description = 1073704; // Ants are industrious and Lions are noble so who'd think an Ant Lion would be such a problem? The Ant Lion's have been causing mindless destruction in these parts. I suppose it's just how ants are. But I need you to help eliminate the infestation. Would you be willing to help for a bit of reward?
            RefusalMessage = 1073733; // Perhaps you'll change your mind and return at some point.
            InProgressMessage = 1073745; // Please, rid us of the Ant Lion infestation.

            Objectives.Add(new KillObjective(12, new Type[] { typeof(AntLion) }, "ant lions"));

            Rewards.Add(ItemReward.LargeBagOfTreasure);
        }
    }
}