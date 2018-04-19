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
    public class CaretakerOfTheLand : MLQuest
    {
        public override bool RecordCompletion { get { return true; } }

        public CaretakerOfTheLand()
        {
            Activated = true;
            Title = 1072783; // Caretaker of the Land
            Description = 1072812; // Hrrrrr.  Hurrrr.  Huuuman.  *creaking branches*  Suuun on baaark, roooooots diiig deeeeeep, wiiind caaaresses leeeaves … Hrrrrr.  Saaap of Sooosaria feeeeeeds us.  Hrrrrr.  Huuuman leeearn.  Caaaretaker of plaaants … teeend … prooove.<br>
            RefusalMessage = 1072813; // Hrrrrr.  Hrrrrr.  Huuuman.
            InProgressMessage = 1072814; // Hrrrr. Hrrrr.  Roooooots neeeeeed saaap of Sooosaria.  Hrrrrr.  Roooooots tiiingle neeeaaar Yeeew.  Seeeaaarch.  Hrrrr!
            CompletionMessage = 1074175; // Thiiirsty. Hurrr. Hurrr.

            Objectives.Add(new CollectObjective(1, typeof(SapOfSosaria), "sap of sosaria"));

            Rewards.Add(new DummyReward(1072804)); // The boon of Strongroot.
        }

        public override void GetRewards(MLQuestInstance instance)
        {
            instance.Player.SendLocalizedMessage(1074941, "", 0x2A); // You have gained the boon of Strongroot!  You have been approved by one whose roots touch the bones of Sosaria.  You are one step closer to claiming your elven heritage.
            instance.ClaimRewards(); // skip gump
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Strongroot"), new Point3D(597, 1744, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Strongroot"), new Point3D(597, 1744, 0), Map.Trammel);

            PutSpawner(new Spawner(1, TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(2), 0, 12, "SapOfSosaria"), new Point3D(757, 1004, 0), Map.Felucca);
            PutSpawner(new Spawner(1, TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(2), 0, 12, "SapOfSosaria"), new Point3D(757, 1004, 0), Map.Trammel);
        }
    }
}