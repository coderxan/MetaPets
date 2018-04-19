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
    public class WisdomOfTheSphynx : MLQuest
    {
        public override bool RecordCompletion { get { return true; } }

        public WisdomOfTheSphynx()
        {
            Activated = true;
            Title = 1072784; // Wisdom of the Sphynx
            Description = 1072822; // I greet thee human and divine my boon thou seek.  Convey hence the object of my riddle and I shall reward thee with thy desire.<br><br>Three lives have I.<br>Gentle enough to soothe the skin,<br>Light enough to caress the sky,<br>Hard enough to crack rocks<br>What am I?
            RefusalMessage = 1072823; // As thou wish, human.
            InProgressMessage = 1072824; // I give thee a hint then human.  The answer to my riddle must be held carefully or it cannot be contained at all.  Bring this elusive item to me in a suitable container.
            CompletionMessage = 1074176; // Ah, thus it ends.

            Objectives.Add(new InternalObjective());

            Rewards.Add(new DummyReward(1072805)); // The boon of Enigma.
        }

        public override void GetRewards(MLQuestInstance instance)
        {
            instance.Player.SendLocalizedMessage(1074945, "", 0x2A); // You have gained the boon of Enigma!  You are wise enough to know how little you know.  You are one step closer to claiming your elven heritage.
            instance.ClaimRewards(); // skip gump
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Enigma"), new Point3D(1828, 961, 7), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Enigma"), new Point3D(1828, 961, 7), Map.Trammel);
        }

        private class InternalObjective : CollectObjective
        {
            public override bool ShowDetailed { get { return false; } }

            public InternalObjective()
                : base(1, typeof(Pitcher), 1074869) // The answer to the riddle.
            {
            }

            public override bool CheckItem(Item item)
            {
                Pitcher pitcher = item as Pitcher; // Only pitchers work

                return (pitcher != null && pitcher.Content == BeverageType.Water && pitcher.Quantity > 0);
            }
        }
    }
}