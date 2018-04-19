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
    public class CreepyCrawlies : MLQuest
    {
        public CreepyCrawlies()
        {
            Activated = true;
            Title = 1072987; // Creepy Crawlies
            Description = 1073016; // Disgusting!  The way they scuttle on those hairy legs just makes me want to gag. I hate spiders!  Rid the world of twelve and I'll find something nice to give you in thanks.
            RefusalMessage = 1072270; // Well, okay. But if you decide you are up for it after all, c'mon back and see me.
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

            Objectives.Add(new KillObjective(12, new Type[] { typeof(GiantSpider) }, "giant spiders"));

            Rewards.Add(ItemReward.BagOfTrinkets);
        }
    }
}