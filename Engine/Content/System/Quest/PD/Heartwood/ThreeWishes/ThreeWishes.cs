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
    public class ThreeWishes : MLQuest
    {
        public ThreeWishes()
        {
            Activated = true;
            Title = 1073660; // Three Wishes
            Description = 1073699; // If I had but one wish, it would be to rid myself of these dread Efreet! Fire and ash, they are cunning and deadly! You look a brave soul - would you be interested in earning a rich reward for slaughtering a few of the smoky devils?
            RefusalMessage = 1073733; // Perhaps you'll change your mind and return at some point.
            InProgressMessage = 1073740; // Those smoky devils, the Efreets, are still about.

            Objectives.Add(new KillObjective(8, new Type[] { typeof(Efreet) }, "efreets"));

            Rewards.Add(ItemReward.LargeBagOfTreasure);
        }
    }
}