using System;

using Server;
using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.MLQuests.Definitions
{
    public class ViciousPredator : MLQuest
    {
        public ViciousPredator()
        {
            Activated = true;
            Title = 1072994; // Vicious Predator
            Description = 1073028; // You've got to help me out! Those dire wolves have been causing absolute havok around here.  Kill them off before they destroy my land.  They run around in a pack of around ten.
            RefusalMessage = 1072270; // Well, okay. But if you decide you are up for it after all, c'mon back and see me.
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

            Objectives.Add(new KillObjective(10, new Type[] { typeof(DireWolf) }, "dire wolves"));

            Rewards.Add(ItemReward.BagOfTrinkets);
        }
    }
}