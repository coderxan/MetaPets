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
    public class Specimens : MLQuest
    {
        public Specimens()
        {
            Activated = true;
            Title = 1072999; // Specimens
            Description = 1073032; // I admire them, you know.  The solen have their place -- regimented, organized.  They're fascinating to watch with their constant strife between red and black.  I can't help but want to stir things up from time to time.  And that's where you come in.  Kill either twelve red or twelve black solen workers and let's see what happens next!
            RefusalMessage = 1072270; // Well, okay. But if you decide you are up for it after all, c'mon back and see me.
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

            ObjectiveType = ObjectiveType.Any;

            Objectives.Add(new KillObjective(12, new Type[] { typeof(RedSolenWorker) }, "red solen workers"));
            Objectives.Add(new KillObjective(12, new Type[] { typeof(BlackSolenWorker) }, "black solen workers"));

            Rewards.Add(ItemReward.BagOfTrinkets);
        }
    }
}