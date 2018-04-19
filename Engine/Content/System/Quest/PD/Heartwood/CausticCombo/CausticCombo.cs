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
    public class CausticCombo : MLQuest
    {
        public CausticCombo()
        {
            Activated = true;
            Title = 1073062; // Caustic Combo
            Description = 1074693; // Vile creatures have exited the sinkhole and begun terrorizing the surrounding area.  The demons are bad enough, but the elementals are an abomination, their poisons seeping into the fertile ground here.  Will you enter the sinkhole and put a stop to their depredations?
            RefusalMessage = 1072270; // Well, okay. But if you decide you are up for it after all, c'mon back and see me.
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

            Objectives.Add(new KillObjective(3, new Type[] { typeof(PoisonElemental) }, "poison elementals", new QuestArea(1074806, "The Palace of Paroxysmus"))); // The Palace of Paroxysmus
            Objectives.Add(new KillObjective(6, new Type[] { typeof(AcidElemental) }, "acid elementals", new QuestArea(1074806, "The Palace of Paroxysmus"))); // The Palace of Paroxysmus

            Rewards.Add(ItemReward.LargeBagOfTreasure);
        }
    }
}