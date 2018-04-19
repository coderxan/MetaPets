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
    public class SplitEnds : MLQuest
    {
        public SplitEnds()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1075506; // Split Ends
            Description = 1075507; // *sighs* I think bowcrafting is a might beyond my talents. Say there, you look a bit more confident with tools. Can I persuade thee to make a few arrows? You could have my satchel in return... 'tis useless to me! You'll need a fletching kit to start, some feathers, and a few arrow shafts. Just use the fletching kit while you have the other things, and I'm sure you'll figure out the rest.
            RefusalMessage = 1075508; // Oh. Well. I'll just keep trying alone, I suppose...
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!
            CompletionMessage = 1072272; // Thanks for helping me out.  Here's the reward I promised you.

            Objectives.Add(new CollectObjective(20, typeof(Arrow), 1023902)); // arrow

            Rewards.Add(new ItemReward(1074282, typeof(AndricSatchel))); // Craftsmans's Satchel
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Andric"), new Point3D(3742, 2582, 40), Map.Trammel);
        }
    }
}