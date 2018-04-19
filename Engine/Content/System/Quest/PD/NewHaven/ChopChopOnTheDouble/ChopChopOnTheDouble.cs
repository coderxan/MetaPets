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
    public class ChopChopOnTheDouble : MLQuest
    {
        public ChopChopOnTheDouble()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1075537; // Chop Chop, On The Double!
            Description = 1075538; // That's right, move it! I need sixty logs on the double, and they need to be freshly cut! If you can get them to me fast I'll have your payment in your hands before you have the scent of pine out from beneath your nostrils. Just get a sharp axe and hack away at some of the trees in the land and your lumberjacking skill will rise in no time.
            RefusalMessage = 1072981; // Or perhaps you'd rather not.
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!
            CompletionMessage = 1075539; // Ahhh! The smell of fresh cut lumber. And look at you, all strong and proud, as if you had done an honest days work!

            Objectives.Add(new CollectObjective(60, typeof(Log), 1027133)); // log

            Rewards.Add(new ItemReward(1074282, typeof(HargroveSatchel))); // Craftsmans's Satchel
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Hargrove"), new Point3D(3445, 2633, 28), Map.Trammel);
        }
    }
}