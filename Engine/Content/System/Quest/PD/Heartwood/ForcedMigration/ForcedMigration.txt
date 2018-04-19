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
    public class ForcedMigration : MLQuest
    {
        public ForcedMigration()
        {
            Activated = true;
            Title = 1072250; // Forced Migration
            Description = 1072264; // Chirp chirp ... tweet chirp.  Tra la la.  Bloody birds and their blasted noise.  I've tried everything but they just won't stop that infernal clamor.  Return me to blessed silence and I'll make it worth your while.
            RefusalMessage = 1072270; // Well, okay. But if you decide you are up for it after all, c'mon back and see me.
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

            Objectives.Add(new KillObjective(10, new Type[] { typeof(Bird) }, "birds"));

            Rewards.Add(ItemReward.SmallBagOfTrinkets);
        }
    }
}