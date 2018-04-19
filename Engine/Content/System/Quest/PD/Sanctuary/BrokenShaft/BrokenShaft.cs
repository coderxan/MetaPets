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
    public class BrokenShaft : MLQuest
    {
        public BrokenShaft()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1074018; // Broken Shaft
            Description = 1074112; // What do humans know of archery? Humans can barely shoot straight. Why, your efforts are absurd. In fact, I will make a wager - if these so called human arrows I've heard about are really as effective and innovative as human braggarts would have me believe, then I'll trade you something useful.  I might even teach you something of elven craftsmanship.
            RefusalMessage = 1074063; // Fine then, I'm shall find another to run my errands then.
            InProgressMessage = 1074064; // Hurry up! I don't have all day to wait for you to bring what I desire!
            CompletionMessage = 1074065; // These human made goods are laughable! It offends so -- I must show you what elven skill is capable of!

            Objectives.Add(new CollectObjective(10, typeof(Arrow), 1023902)); // arrow

            Rewards.Add(ItemReward.FletchingSatchel);
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Beotham"), new Point3D(6285, 114, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Beotham"), new Point3D(6285, 114, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Danoel"), new Point3D(6282, 116, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Danoel"), new Point3D(6282, 116, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Tallinin"), new Point3D(6279, 122, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Tallinin"), new Point3D(6279, 122, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Tiana"), new Point3D(6257, 112, -10), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Tiana"), new Point3D(6257, 112, -10), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 0, "LorekeeperOolua"), new Point3D(6250, 124, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 0, "LorekeeperOolua"), new Point3D(6250, 124, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 0, "LorekeeperRollarn"), new Point3D(6244, 110, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 0, "LorekeeperRollarn"), new Point3D(6244, 110, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Dallid"), new Point3D(6277, 104, -10), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Dallid"), new Point3D(6277, 104, -10), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Canir"), new Point3D(6274, 130, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Canir"), new Point3D(6274, 130, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Yellienir"), new Point3D(6257, 126, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Yellienir"), new Point3D(6257, 126, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 0, "ElderOnallan"), new Point3D(6258, 108, -10), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 0, "ElderOnallan"), new Point3D(6258, 108, -10), Map.Felucca);
        }
    }
}