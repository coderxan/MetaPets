using System;
using System.Collections.Generic;
using System.Text;

using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.MLQuests.Definitions
{
    public class Seasons : MLQuest
    {
        public override bool RecordCompletion { get { return true; } }

        public Seasons()
        {
            Activated = true;
            Title = 1072782; // Seasons
            Description = 1072802; // *rumbling growl* *sniff* ... not-smell ... seek-fight ... not-smell ... fear-stench ... *rumble* ... cold-soon-time comes ... hungry ... eat-fish ... sleep-soon-time ... *deep fang-filled yawn* ... much-fish.
            RefusalMessage = 1072810; // *yawn* ... cold-soon-time ... *growl*
            InProgressMessage = 1072811; // *sniff* *sniff* ... not-much-fish ... hungry ... *grumble*
            CompletionMessage = 1074174; // *sniff* fish! much-fish!

            Objectives.Add(new CollectObjective(20, typeof(RawFishSteak), 1022426)); // raw fish steak

            Rewards.Add(new DummyReward(1072803)); // The boon of Maul.
        }

        public override void GetRewards(MLQuestInstance instance)
        {
            instance.Player.SendLocalizedMessage(1074940, "", 0x2A); // You have gained the boon of Maul!  Your understanding of the seasons grows.  You are one step closer to claiming your elven heritage.
            instance.ClaimRewards(); // skip gump
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Darius"), new Point3D(4310, 954, 10), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Darius"), new Point3D(4310, 954, 10), Map.Trammel);

            PutSpawner(new Spawner(1, 5, 10, 0, 5, "MaulTheBear"), new Point3D(1730, 257, 16), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "MaulTheBear"), new Point3D(1730, 257, 16), Map.Trammel);
        }
    }
}