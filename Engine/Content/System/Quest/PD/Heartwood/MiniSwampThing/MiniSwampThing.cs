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
    public class MiniSwampThing : MLQuest
    {
        public MiniSwampThing()
        {
            Activated = true;
            Title = 1073072; // Mini Swamp Thing
            Description = 1073562; // Some say killing a boggling brings good luck. I don't place much stock in old wives' tales, but I can say a few dead bogglings would certainly be lucky for me! Help me out and I can reward you for your efforts.
            RefusalMessage = 1073580; // I hope you'll reconsider. Until then, farwell.
            InProgressMessage = 1073582; // Go back and kill all 20 bogglings!

            Objectives.Add(new KillObjective(12, new Type[] { typeof(Bogling) }, "boglings"));

            Rewards.Add(ItemReward.BagOfTreasure);
        }
    }
}