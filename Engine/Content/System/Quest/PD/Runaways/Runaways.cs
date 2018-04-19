using System;

using Server;
using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.MLQuests.Definitions
{
    public class Runaways : MLQuest
    {
        public Runaways()
        {
            Activated = true;
            Title = 1072993; // Runaways!
            Description = 1073026; // You've got to help me out! Those wild ostards have been causing absolute havok around here.  Kill them off before they destroy my land.  There are around twelve of them.
            RefusalMessage = 1072270; // Well, okay. But if you decide you are up for it after all, c'mon back and see me.
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

            Objectives.Add(new KillObjective(12, new Type[] { typeof(FrenziedOstard) }, "frenzied ostards"));

            Rewards.Add(ItemReward.BagOfTrinkets);
        }
    }
}