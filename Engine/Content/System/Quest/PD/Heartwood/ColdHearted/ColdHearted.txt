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
    public class ColdHearted : MLQuest
    {
        public ColdHearted()
        {
            Activated = true;
            Title = 1072991; // Cold Hearted
            Description = 1073027; // It's a big job but you look to be just the adventurer to do it! I'm so glad you came by ... I'm paying well for the death of six giant ice serpents and six frost spiders.  Hop to it, if you're so inclined.
            RefusalMessage = 1072270; // Well, okay. But if you decide you are up for it after all, c'mon back and see me.
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

            Objectives.Add(new KillObjective(6, new Type[] { typeof(IceSerpent) }, "giant ice serpents"));
            Objectives.Add(new KillObjective(6, new Type[] { typeof(FrostSpider) }, "frost spiders"));

            Rewards.Add(ItemReward.BagOfTrinkets);
        }
    }
}