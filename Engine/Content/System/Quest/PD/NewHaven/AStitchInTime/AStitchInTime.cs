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
    public class AStitchInTime : MLQuest
    {
        public AStitchInTime()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1075523; // A Stitch in Time
            Description = 1075522; // Oh how I wish I had a fancy dress like the noble ladies of Castle British! I don't have much... but I have a few trinkets I might trade for it. It would mean the world to me to go to a fancy ball and dance the night away. Oh, and I could tell you how to make one! You just need to use your sewing kit on enough cut cloth, that's all.
            RefusalMessage = 1075526; // Won't you reconsider? It'd mean the world to me, it would!
            InProgressMessage = 1075527; // Hello again! Do you need anything? You may want to visit the tailor's shop for cloth and a sewing kit, if you don't already have them.
            CompletionMessage = 1075528; // It's gorgeous! I only have a few things to give you in return, but I can't thank you enough! Maybe I'll even catch Uzeraan's eye at the, er, *blushes* I mean, I can't wait to wear it to the next town dance!

            Objectives.Add(new CollectObjective(1, typeof(FancyDress), 1027935)); // fancy dress

            Rewards.Add(new ItemReward(1075524, typeof(AnOldRing))); // an old ring
            Rewards.Add(new ItemReward(1075525, typeof(AnOldNecklace))); // an old necklace
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Clairesse"), new Point3D(3492, 2546, 20), Map.Trammel);
        }
    }
}