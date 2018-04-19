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
    public class MongbatMenace : MLQuest
    {
        public MongbatMenace()
        {
            Activated = true;
            Title = 1073003; // Mongbat Menace!
            Description = 1073033; // I imagine you don't know about the mongbats.  Well, you may think you do, but I know more than just about anyone. You see they come in two varieties ... the stronger and the weaker.  Either way, they're a menace.  Exterminate ten of the weaker ones and four of the stronger and I'll pay you an honest wage.
            RefusalMessage = 1072270; // Well, okay. But if you decide you are up for it after all, c'mon back and see me.
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

            Objectives.Add(new KillObjective(10, new Type[] { typeof(Mongbat) }, "mongbats"));
            Objectives.Add(new KillObjective(4, new Type[] { typeof(GreaterMongbat) }, "greater mongbats"));

            Rewards.Add(ItemReward.BagOfTrinkets);
        }
    }
}