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
    public class ABigJob : MLQuest
    {
        public ABigJob()
        {
            Activated = true;
            Title = 1072988; // A Big Job
            Description = 1073017; // It's a big job but you look to be just the adventurer to do it! I'm so glad you came by ... I'm paying well for the death of five ogres and five ettins.
            RefusalMessage = 1072270; // Well, okay. But if you decide you are up for it after all, c'mon back and see me.
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

            Objectives.Add(new KillObjective(5, new Type[] { typeof(Ogre) }, "ogres"));
            Objectives.Add(new KillObjective(5, new Type[] { typeof(Ettin) }, "ettins"));

            Rewards.Add(ItemReward.BagOfTrinkets);
        }
    }
}