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
    public class OrcSlaying : MLQuest
    {
        public OrcSlaying()
        {
            Activated = true;
            Title = 1072986; // Orc Slaying
            Description = 1073015; // Those green-skinned freaks have run off with more of my livestock.  I want an orc scout killed for each sheep I lost and an orc for each chicken.  So that's four orc scouts and eight orcs I'll pay you to slay.
            RefusalMessage = 1072270; // Well, okay. But if you decide you are up for it after all, c'mon back and see me.
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

            Objectives.Add(new KillObjective(8, new Type[] { typeof(Orc) }, "orcs"));
            // TODO: This needs to be orc scouts but they aren't in the SVN
            Objectives.Add(new KillObjective(4, new Type[] { typeof(OrcishLord) }, "orcish lords"));

            Rewards.Add(ItemReward.BagOfTrinkets);
        }
    }
}