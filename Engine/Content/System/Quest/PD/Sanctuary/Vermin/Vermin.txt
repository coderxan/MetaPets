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
    public class Vermin : MLQuest
    {
        public Vermin()
        {
            Activated = true;
            Title = 1072995; // Vermin
            Description = 1073029; // You've got to help me out! Those ratmen have been causing absolute havok around here.  Kill them off before they destroy my land.  I'll pay you if you kill off twelve of those dirty rats.
            RefusalMessage = 1072270; // Well, okay. But if you decide you are up for it after all, c'mon back and see me.
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

            Objectives.Add(new KillObjective(12, new Type[] { typeof(Ratman) }, "ratmen"));

            Rewards.Add(ItemReward.BagOfTrinkets);
        }
    }
}