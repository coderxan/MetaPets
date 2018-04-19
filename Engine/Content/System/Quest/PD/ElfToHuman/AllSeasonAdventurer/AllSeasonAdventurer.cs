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
    public class AllSeasonAdventurer : MLQuest
    {
        public override bool RecordCompletion { get { return true; } }

        public AllSeasonAdventurer()
        {
            Activated = true;
            Title = 1074353; // All Season Adventurer
            Description = 1074527; // It's all about hardship, suffering, struggle and pain.  Without challenges, you've got nothing to test yourself against -- and that's what life is all about.  Self improvement!  Honing your body and mind!  Overcoming obstacles ... You'll see what I mean if you take on my challenge.
            RefusalMessage = 1074528; // My way of life isn't for everyone, that's true enough.
            InProgressMessage = 1074529; // You're not making much progress in the honing-mind-and-body department, are you?
            CompletionNotice = CompletionNoticeShortReturn;

            Objectives.Add(new KillObjective(5, new Type[] { typeof(Efreet) }, "efreets", new QuestArea(1074808, "Fire"))); // Fire
            Objectives.Add(new KillObjective(5, new Type[] { typeof(IceFiend) }, "ice fiends", new QuestArea(1074809, "Ice"))); // Ice

            Rewards.Add(new DummyReward(1074875)); // Another step closer to becoming human.
        }

        public override void GetRewards(MLQuestInstance instance)
        {
            instance.Player.SendLocalizedMessage(1074947, "", 0x2A); // You have demonstrated your toughness!  Humans are able to endure unimaginable hardships in pursuit of their goals.  You are one step closer to achieving humanity.
            instance.ClaimRewards(); // skip gump
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Belulah"), new Point3D(3782, 1266, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 5, "Belulah"), new Point3D(3782, 1266, 0), Map.Trammel);
        }
    }
}