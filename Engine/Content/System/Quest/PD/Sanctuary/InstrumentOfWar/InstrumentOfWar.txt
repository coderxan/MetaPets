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
    public class InstrumentOfWar : MLQuest
    {
        public InstrumentOfWar()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1074055; // Instrument of War
            Description = 1074149; // Pathetic, this human craftsmanship! Take their broadswords - overgrown butter knives, in reality. No, I cannot do them justice - you must see for yourself. Bring me broadswords and I will demonstrate their feebleness.
            RefusalMessage = 1074063; // Fine then, I'm shall find another to run my errands then.
            InProgressMessage = 1074064; // Hurry up! I don't have all day to wait for you to bring what I desire!
            CompletionMessage = 1074065; // These human made goods are laughable! It offends so -- I must show you what elven skill is capable of!

            Objectives.Add(new CollectObjective(12, typeof(Broadsword), 1023934)); // broadsword

            Rewards.Add(ItemReward.BlacksmithSatchel);
        }
    }
}