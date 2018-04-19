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
    public class TheBalanceOfNature : MLQuest
    {
        public override bool RecordCompletion { get { return true; } }

        public TheBalanceOfNature()
        {
            Activated = true;
            Title = 1072786; // The Balance of Nature
            Description = 1072829; // Ho, there human.  Why do you seek out the Huntsman?  The hunter serves the land by culling both predators and prey.  The hunter maintains the essential balance of life and does not kill for sport or glory.  If you seek my favor, human, then demonstrate you are capable of the duty.  Cull the wolves nearby.
            RefusalMessage = 1072830; // Then begone. I have no time to waste on you, human.
            InProgressMessage = 1072831; // The timber wolves are easily tracked, human.

            Objectives.Add(new KillObjective(15, new Type[] { typeof(TimberWolf) }, "timber wolves", new QuestArea(1074833, "Huntsman's Forest"))); // Huntsman's Forest

            Rewards.Add(new DummyReward(1072807)); // The boon of the Huntsman.
        }

        public override void GetRewards(MLQuestInstance instance)
        {
            instance.Player.SendLocalizedMessage(1074943, "", 0x2A); // You have gained the boon of the Huntsman!  You have been given a taste of the bittersweet duty of those who guard the balance.  You are one step closer to claiming your elven heritage.
            instance.ClaimRewards(); // skip gump
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Huntsman"), new Point3D(1676, 593, 16), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Huntsman"), new Point3D(1676, 593, 16), Map.Trammel);

            PutSpawner(new Spawner(5, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15), 0, 10, "TimberWolf"), new Point3D(1671, 592, 16), Map.Felucca);
            PutSpawner(new Spawner(5, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15), 0, 10, "TimberWolf"), new Point3D(1671, 592, 16), Map.Trammel);
        }
    }
}