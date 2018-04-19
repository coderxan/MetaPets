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
    public class HeaveHo : MLQuest
    {
        public override bool RecordCompletion { get { return true; } }

        public HeaveHo()
        {
            Activated = true;
            Title = 1074351; // Heave Ho!
            Description = 1074519; // Ho there!  There's nothing quite like a day's honest labor to make you appreciate being alive.  Hey, maybe you'd like to help out with this project?  These crates need to be delivered to Sledge.  The only thing is -- it's a bit of a rush job and if you don't make it in time, he won't take them.  Can I trust you to help out?
            RefusalMessage = 1074521; // Oh yah, if you're too busy, no problem.
            InProgressMessage = 1074522; // Sledge can be found in Buc's Den.  Better hurry, he won't take those crates if you take too long with them.
            CompletionMessage = 1074523; // Hey, if you have cargo for me, you can start unloading over here.
            CompletionNotice = CompletionNoticeShort;

            Objectives.Add(new TimedDeliverObjective(TimeSpan.FromHours(1), typeof(CrateForSledge), 5, "Crates for Sledge", typeof(Sledge)));

            Rewards.Add(new DummyReward(1074875)); // Another step closer to becoming human.
        }

        public override void GetRewards(MLQuestInstance instance)
        {
            instance.Player.SendLocalizedMessage(1074948, "", 0x2A); // You have demonstrated your physical strength!  Humans can carry vast loads without complaint.  You are one step closer to achieving humanity.
            instance.ClaimRewards(); // skip gump
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Patricus"), new Point3D(3007, 823, -2), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Patricus"), new Point3D(3007, 823, -2), Map.Trammel);
        }
    }
}