using System;

using Server;
using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.MLQuests.Definitions
{
    public class SomethingToWailAbout : MLQuest
    {
        public SomethingToWailAbout()
        {
            Activated = true;
            Title = 1073071; // Something to Wail About
            Description = 1073561; // Can you hear them? The never-ending howling? The incessant wailing? These banshees, they never cease! Never! They haunt my nights. Please, I beg you -- will you silence them? I would be ever so grateful.
            RefusalMessage = 1073580; // I hope you'll reconsider. Until then, farwell.
            InProgressMessage = 1073581; // Until you kill 12 Wailing Banshees, there will be no peace.

            Objectives.Add(new KillObjective(12, new Type[] { typeof(WailingBanshee) }, "wailing banshees"));

            Rewards.Add(ItemReward.BagOfTreasure);
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 4, "Jelrice"), new Point3D(1176, 1196, -25), Map.Ilshenar);
        }
    }
}