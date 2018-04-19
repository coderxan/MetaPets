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
    public class AHeroInTheMaking : MLQuest
    {
        public AHeroInTheMaking()
        {
            Activated = true;
            Title = 1072246; // A Hero in the Making
            Description = 1072257; // Are you new around here?  Well, nevermind that.  You look ready for adventure, I can see the gleam of glory in your eyes!  Nothing is more valiant, more noble, more praiseworthy than mongbat slaying.
            RefusalMessage = 1072270; // Well, okay. But if you decide you are up for it after all, c'mon back and see me.
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

            Objectives.Add(new KillObjective(10, new Type[] { typeof(Mongbat) }, "mongbats"));

            Rewards.Add(ItemReward.SmallBagOfTrinkets);
        }
    }
}