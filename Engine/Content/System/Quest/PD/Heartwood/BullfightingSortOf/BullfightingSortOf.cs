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
    public class BullfightingSortOf : MLQuest
    {
        public BullfightingSortOf()
        {
            Activated = true;
            Title = 1072247; // Bullfighting ... Sort Of
            Description = 1072254; // You there! Yes, you.  Listen, I've got a little problem on my hands, but a brave, bold hero like yourself should find it a snap to solve.  Bottom line -- we need some of the bulls in the area culled.  You're welcome to any meat or hides, and of course, I'll give you a nice reward.
            RefusalMessage = 1072270; // Well, okay. But if you decide you are up for it after all, c'mon back and see me.
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

            Objectives.Add(new KillObjective(10, new Type[] { typeof(Bull) }, "bulls"));

            Rewards.Add(ItemReward.SmallBagOfTrinkets);
        }
    }
}