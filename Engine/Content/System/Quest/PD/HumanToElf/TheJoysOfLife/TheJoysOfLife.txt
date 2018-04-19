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
    public class TheJoysOfLife : MLQuest
    {
        public override bool RecordCompletion { get { return true; } }

        public TheJoysOfLife()
        {
            Activated = true;
            Title = 1072787; // The Joys of Life
            Description = 1072832; // *giggle*  So serious, so grim!  *tickle*  Enjoy life!  Have fun!  Laugh!  Be merry!  *giggle*  Find three of my baubles ... *giggle* I hid them! *giggles hysterically*  Hid them!  La la la!  Bring them quickly!  They are magical and will hide themselves again if you are too slow.
            RefusalMessage = 1072833; // *giggle* Too serious.  Too thinky!
            InProgressMessage = 1072834; // Magical baubles hidden, find them as you're bidden!  *giggle*
            CompletionMessage = 1074177; // *giggle* So pretty!

            Objectives.Add(new CollectObjective(3, typeof(ABauble), "arielle's baubles"));

            Rewards.Add(new DummyReward(1072809)); // The boon of Arielle.
        }

        public override void GetRewards(MLQuestInstance instance)
        {
            instance.Player.SendLocalizedMessage(1074944, "", 0x2A); // You have gained the boon of Arielle!  You have been taught the importance of laughter and light spirits.  You are one step closer to claiming your elven heritage.
            instance.ClaimRewards(); // skip gump
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Arielle"), new Point3D(1560, 1182, -27), Map.Ilshenar);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Arielle"), new Point3D(3366, 292, 9), Map.Felucca); // Felucca spawn for reds

            PutSpawner(new Spawner(6, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30), 0, 20, "ABauble"), new Point3D(1585, 1212, -13), Map.Ilshenar);
        }
    }
}