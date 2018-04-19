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
    public class AFineFeast : MLQuest
    {
        public AFineFeast()
        {
            Activated = true;
            Title = 1072243; // A Fine Feast.
            Description = 1072261; // Mmm, I do love mutton!  It's slaughtering time again and my usual hirelings haven't turned up.  I've arranged for a butcher to come by and cut everything up but the basic sheep killing part I haven't gotten worked out yet.  Are you up for the task?
            RefusalMessage = 1072270; // Well, okay. But if you decide you are up for it after all, c'mon back and see me.
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

            Objectives.Add(new KillObjective(10, new Type[] { typeof(Sheep) }, "sheep"));

            Rewards.Add(ItemReward.SmallBagOfTrinkets);
        }
    }
}