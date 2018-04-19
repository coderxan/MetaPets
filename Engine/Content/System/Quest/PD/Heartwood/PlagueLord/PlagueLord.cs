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
    public class PlagueLord : MLQuest
    {
        public PlagueLord()
        {
            Activated = true;
            Title = 1073061; // Plague Lord
            Description = 1074692; // Some of the most horrific creatures have slithered out of the sinkhole there and begun terrorizing the surrounding area. The plague creatures are one of the most destruction of the minions of Paroxysmus.  Are you willing to do something about them?
            RefusalMessage = 1072270; // Well, okay. But if you decide you are up for it after all, c'mon back and see me.
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

            Objectives.Add(new KillObjective(10, new Type[] { typeof(PlagueSpawn) }, "plague spawns", new QuestArea(1074806, "The Palace of Paroxysmus"))); // The Palace of Paroxysmus
            Objectives.Add(new KillObjective(3, new Type[] { typeof(PlagueBeast) }, "plague beasts", new QuestArea(1074806, "The Palace of Paroxysmus"))); // The Palace of Paroxysmus
            Objectives.Add(new KillObjective(1, new Type[] { typeof(PlagueBeastLord) }, "plague beast lord", new QuestArea(1074806, "The Palace of Paroxysmus"))); // The Palace of Paroxysmus

            Rewards.Add(ItemReward.LargeBagOfTreasure);
        }
    }
}