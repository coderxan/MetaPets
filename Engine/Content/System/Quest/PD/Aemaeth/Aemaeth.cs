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
    public class Aemaeth1 : MLQuest
    {
        public override Type NextQuest { get { return typeof(Aemaeth2); } }

        public Aemaeth1()
        {
            Activated = true;
            Title = 1075321; // Aemaeth
            Description = 1075322; // My father died in an accident some months ago. My mother refused to accept his death. We had a little money set by, and she took it to a necromancer, who promised to restore my father to life. Well, he revived my father, all right, the cheat! Now my father is a walking corpse, a travesty . . . a monster. My mother is beside herself -- she won't eat, she can't sleep. I prayed at the shrine of Spirituality for guidance, and I must have fallen asleep. When I awoke, there was this basin of clear water. I cannot leave my mother, for I fear what she might do to herself. Could you take this to the graveyard, and give it to what is left of my father?
            RefusalMessage = 1075324; // Oh! Alright then. I hope someone comes along soon who can help me, or I dont know what will become of us.
            InProgressMessage = 1075325; // My father - or what remains of him - can be found in the graveyard northwest of the city.
            CompletionMessage = 1075326; // What is this you give me? A basin of water?
            CompletionNotice = CompletionNoticeShort;

            Objectives.Add(new DeliverObjective(typeof(BasinOfCrystalClearWater), 1, "Basin of Crystal Clear Water", typeof(SkeletonOfSzandor)));

            Rewards.Add(new DummyReward(1075323)); // Aurelia's gratitude.
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 4, "Aurelia"), new Point3D(1459, 3795, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 4, "Aurelia"), new Point3D(1459, 3795, 0), Map.Felucca);
        }
    }

    public class Aemaeth2 : MLQuest
    {
        public override bool IsChainTriggered { get { return true; } }

        public Aemaeth2()
        {
            Activated = true;
            OneTimeOnly = true;
            Title = 1075327; // Aemaeth
            Description = 1075328; // You tell me it is time to leave this flesh. I did not understand until now. I thought: I can see my wife and my daughter, I can speak. Is this not life? But now, as I regard my reflection, I see what I have become. This only a mockery of life. Thank you for having the courage to show me the truth. For the love I bear my wife and daughter, I know now that I must pass beyond the veil. Will you return this basin to Aurelia? She will know by this that I am at rest.
            RefusalMessage = 1075330; // You wont take this back to my daughter? Please, I cannot leave until she knows I am at peace.
            InProgressMessage = 1075331; // My daughter will be at my home, on the east side of the city.
            CompletionMessage = 1075332; // Thank goodness! Now we can honor my father for the great man he was while he lived, rather than the horror he became.
            CompletionNotice = CompletionNoticeShort;

            Objectives.Add(new DeliverObjective(typeof(BasinOfCrystalClearWater), 1, "Basin of Crystal Clear Water", typeof(Aurelia)));

            Rewards.Add(new ItemReward(1075304, typeof(MirrorOfPurification))); // Mirror of Purification
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 2, "SkeletonOfSzandor"), new Point3D(1277, 3731, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 2, "SkeletonOfSzandor"), new Point3D(1277, 3731, 0), Map.Felucca);
        }
    }
}