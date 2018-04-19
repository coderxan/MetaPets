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
    public class ForkedTongues : MLQuest
    {
        public ForkedTongues()
        {
            Activated = true;
            Title = 1072984; // Forked Tongues
            Description = 1073013; // You can't trust them, you know.  Lizardmen I mean.  They have forked tongues ... and you know what that means.  Exterminate ten of them and I'll reward you.
            RefusalMessage = 1072270; // Well, okay. But if you decide you are up for it after all, c'mon back and see me.
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

            Objectives.Add(new KillObjective(10, new Type[] { typeof(Lizardman) }, "lizardmen"));

            Rewards.Add(ItemReward.BagOfTrinkets);
        }
    }
}