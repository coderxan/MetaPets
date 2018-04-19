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
    public class Ingenuity : MLQuest
    {
        public override bool RecordCompletion { get { return true; } }

        public Ingenuity()
        {
            Activated = true;
            Title = 1074350; // Ingenuity
            Description = 1074462; // The best thing about my job is that I do a little bit of everything, every day.  It's what we're good at really.  Just picking up something and making it do something else.  Listen, I'm really low on parts.  Are you interested in fetching me some supplies?
            RefusalMessage = 1074508; // Okay.  Best of luck with your other endeavors.
            InProgressMessage = 1074509; // Lord overseers are the best source I know for power crystals of the type I need.  Iron golems too, can have them but they're harder to find.
            CompletionMessage = 1074510; // Do you have those power crystals?  I'm ready to put the finishing touches on my latest experiment.
            CompletionNotice = CompletionNoticeShortReturn;

            Objectives.Add(new CollectObjective(10, typeof(PowerCrystal), "Power Crystals"));

            Rewards.Add(new DummyReward(1074875)); // Another step closer to becoming human.
        }

        public override void GetRewards(MLQuestInstance instance)
        {
            instance.Player.SendLocalizedMessage(1074946, "", 0x2A); // You have demonstrated your ingenuity!  Humans are jacks of all trades and know a little about a lot of things.  You are one step closer to achieving humanity.
            instance.ClaimRewards(); // skip gump
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Nedrick"), new Point3D(2958, 3466, 15), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Nedrick"), new Point3D(2958, 3466, 15), Map.Trammel);

            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Sledge"), new Point3D(2673, 2129, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Sledge"), new Point3D(2673, 2129, 0), Map.Trammel);
        }
    }
}