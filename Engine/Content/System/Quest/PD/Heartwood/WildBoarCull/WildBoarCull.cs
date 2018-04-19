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
    public class WildBoarCull : MLQuest
    {
        public WildBoarCull()
        {
            Activated = true;
            Title = 1072245; // Wild Boar Cull
            Description = 1072260; // A pity really.  With the balance of nature awry, we have no choice but to accept the responsibility of making it all right.  It's all a part of the circle of life, after all. So, yes, the boars are running rampant. There are far too many in the region.  Will you shoulder your obligations as a higher life form?
            RefusalMessage = 1072270; // Well, okay. But if you decide you are up for it after all, c'mon back and see me.
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

            Objectives.Add(new KillObjective(10, new Type[] { typeof(Boar) }, "boars"));

            Rewards.Add(ItemReward.SmallBagOfTrinkets);
        }
    }
}