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
    public class TroubleOnTheWing : MLQuest
    {
        public TroubleOnTheWing()
        {
            Activated = true;
            Title = 1072371; // Trouble on the Wing
            Description = 1072593; // Those gargoyles need to get knocked down a peg or two, if you ask me.  They're always flying over here and lobbing things at us. What a nuisance.  Drop a dozen of them for me, would you?
            RefusalMessage = 1072594; // Don't tell me you're a gargoyle sympathizer? *spits*
            InProgressMessage = 1072595; // Those blasted gargoyles hang around the old tower.  That's the best place to hunt them down.

            Objectives.Add(new KillObjective(12, new Type[] { typeof(Gargoyle) }, "gargoyles", new QuestArea(1074807, "Sanctuary"))); // Sanctuary

            Rewards.Add(ItemReward.BagOfTrinkets);
        }
    }
}