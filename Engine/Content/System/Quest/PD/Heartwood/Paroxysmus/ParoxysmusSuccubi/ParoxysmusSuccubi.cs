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
    public class ParoxysmusSuccubi : MLQuest
    {
        public ParoxysmusSuccubi()
        {
            Activated = true;
            Title = 1073067; // Paroxysmus' Succubi
            Description = 1074696; // The succubi that have congregated within the sinkhole to worship Paroxysmus pose a tremendous danger. Will you enter the lair and see to their destruction?
            RefusalMessage = 1072270; // Well, okay. But if you decide you are up for it after all, c'mon back and see me.
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

            Objectives.Add(new KillObjective(3, new Type[] { typeof(Succubus) }, "succubi", new QuestArea(1074806, "The Palace of Paroxysmus"))); // The Palace of Paroxysmus

            Rewards.Add(ItemReward.LargeBagOfTreasure);
        }
    }
}