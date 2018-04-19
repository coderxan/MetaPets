using System;
using System.Collections.Generic;

using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Items;
using Server.Mobiles;
using System.Text;

namespace Server.Engines.MLQuests.Definitions
{
    public class Patience : MLQuest
    {
        public override Type NextQuest { get { return typeof(NeedsOfTheManyHeartwood1); } }

        public Patience()
        {
            Activated = true;
            Title = 1072753; // Patience
            Description = 1072762; // Learning to weave spells and control the forces of nature requires sacrifice, discipline, focus, and an unwavering dedication to Sosaria herself.  We do not teach the unworthy.  They do not comprehend the lessons nor the dedication required.  If you would walk the path of the Arcanist, then you must do as I require without hesitation or question.  Your first task is to gather miniature mushrooms ... 20 of them from the branches of our mighty home.  I give you one hour to complete the task.
            RefusalMessage = 1072767; // *nods* Not everyone has the temperment to undertake the way of the Arcanist.
            InProgressMessage = 1072774; // The mushrooms I seek can be found growing here in The Heartwood. Seek them out and gather them.  You are running out of time.
            CompletionMessage = 1074166; // Have you gathered the mushrooms?

            Objectives.Add(new TimedCollectObjective(TimeSpan.FromHours(1), 20, typeof(MiniatureMushroom), "miniature mushrooms"));

            Rewards.Add(new DummyReward(1074872)); // The opportunity to learn the ways of the Arcanist.
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 3, "Aeluva"), new Point3D(7064, 349, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 3, "Aeluva"), new Point3D(7064, 349, 0), Map.Trammel);

            // Split up to prevent stacking on the spawner
            PutSpawner(new Spawner(20, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(30), 0, 30, "MiniatureMushroom"), new Point3D(7015, 366, 0), Map.Felucca);
            PutSpawner(new Spawner(20, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(30), 0, 30, "MiniatureMushroom"), new Point3D(7015, 366, 0), Map.Trammel);

            PutSpawner(new Spawner(5, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(30), 0, 20, "MiniatureMushroom"), new Point3D(7081, 373, 0), Map.Felucca);
            PutSpawner(new Spawner(5, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(30), 0, 20, "MiniatureMushroom"), new Point3D(7081, 373, 0), Map.Trammel);

            PutSpawner(new Spawner(15, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(30), 0, 20, "MiniatureMushroom"), new Point3D(7052, 414, 0), Map.Felucca);
            PutSpawner(new Spawner(15, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(30), 0, 20, "MiniatureMushroom"), new Point3D(7052, 414, 0), Map.Trammel);
        }
    }

    public class NeedsOfTheManyHeartwood1 : MLQuest
    {
        public override Type NextQuest { get { return typeof(NeedsOfTheManyHeartwood2); } }
        public override bool IsChainTriggered { get { return true; } }

        public NeedsOfTheManyHeartwood1()
        {
            Activated = true;
            Title = 1072797; // Needs of the Many - The Heartwood
            Description = 1072763; // The way of the Arcanist involves cooperation with others and a strong committment to the community of your people.  We have run low on the cotton we use to pack wounds and our people have need.  Bring 10 bales of cotton to me.
            RefusalMessage = 1072768; // You endanger your progress along the path with your unwillingness.
            InProgressMessage = 1072775; // I care not where you acquire the cotton, merely that you provide it.
            CompletionMessage = 1074110; // Well, where are the cotton bales?

            Objectives.Add(new CollectObjective(10, typeof(Cotton), 1023577)); // bale of cotton

            Rewards.Add(new DummyReward(1074872)); // The opportunity to learn the ways of the Arcanist.
        }
    }

    public class NeedsOfTheManyHeartwood2 : MLQuest
    {
        public override Type NextQuest { get { return typeof(MakingAContributionHeartwood); } }
        public override bool IsChainTriggered { get { return true; } }

        public NeedsOfTheManyHeartwood2()
        {
            Activated = true;
            Title = 1072797; // Needs of the Many - The Heartwood
            Description = 1072764; // We must look to the defense of our people!  Bring boards for new arrows.
            RefusalMessage = 1072769; // The people have need of these items.  You are proving yourself inadequate to the demands of a member of this community.
            InProgressMessage = 1072776; // The requirements are simple -- 250 boards.
            CompletionMessage = 1074152; // Well, where are the boards?

            Objectives.Add(new CollectObjective(250, typeof(Board), 1027127)); // board

            Rewards.Add(new DummyReward(1074872)); // The opportunity to learn the ways of the Arcanist.
        }
    }

    public class MakingAContributionHeartwood : MLQuest
    {
        public override Type NextQuest { get { return typeof(UnnaturalCreations); } }
        public override bool IsChainTriggered { get { return true; } }

        public MakingAContributionHeartwood()
        {
            Activated = true;
            Title = 1072798; // Making a Contribution - The Heartwood
            Description = 1072765; // With health and defense assured, we need look to the need of the community for food and drink.  We will feast on fish steaks, sweets, and wine.  You will supply the ingredients, the cooks will prepare the meal.  As a Arcanist relies upon others to build focus and lend their power to her workings, the community needs the effort of all to survive.
            RefusalMessage = 1072770; // Do not falter now.  You have begun to show promise.
            InProgressMessage = 1072777; // Where are the items you've been tasked to supply for the feast?
            CompletionMessage = 1074158; // Ah good, you're back.  We're eager for the feast.

            Objectives.Add(new CollectObjective(1, typeof(SackFlour), 1024153)); // sack of flour
            Objectives.Add(new CollectObjective(10, typeof(JarHoney), 1022540)); // jar of honey
            Objectives.Add(new CollectObjective(20, typeof(FishSteak), 1022427)); // fish steak

            Rewards.Add(new DummyReward(1074872)); // The opportunity to learn the ways of the Arcanist.
        }
    }

    public class UnnaturalCreations : MLQuest
    {
        public override bool IsChainTriggered { get { return true; } }

        public UnnaturalCreations()
        {
            Activated = true;
            Title = 1072758; // Unnatural Creations
            Description = 1072780; // You have proven your desire to contribute to the community and serve the people.  Now you must demonstrate your willingness to defend Sosaria from the greatest blight that plagues her.  Unnatural creatures, brought to a sort of perverted life, despoil our fair world.  Destroy them -- 5 Exodus Overseers and 2 Exodus Minions.
            RefusalMessage = 1072771; // You must serve Sosaria with all your heart and strength.  Your unwillingness does not reflect favorably upon you.
            InProgressMessage = 1072779; // Every moment you procrastinate, these unnatural creatures damage Sosaria.
            CompletionMessage = 1074167; // Well done!  Well done, indeed.  You are worthy to become an arcanist!

            Objectives.Add(new KillObjective(5, new Type[] { typeof(ExodusOverseer) }, "Exodus Overseers"));
            Objectives.Add(new KillObjective(2, new Type[] { typeof(ExodusMinion) }, "Exodus Minions"));

            Rewards.Add(new ItemReward(1031601, typeof(ArcaneCircleScroll))); // Arcane Circle
            Rewards.Add(new ItemReward(1031600, typeof(SpellweavingBook))); // Spellweaving Spellbook
            Rewards.Add(new ItemReward(1031602, typeof(GiftOfRenewalScroll))); // Gift of Renewal
        }

        public override void GetRewards(MLQuestInstance instance)
        {
            Spellweaving.AwardTo(instance.Player);
            base.GetRewards(instance);
        }
    }
}