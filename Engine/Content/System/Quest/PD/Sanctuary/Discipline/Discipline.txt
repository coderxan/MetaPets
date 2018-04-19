using System;
using System.Collections.Generic;

using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Items;
using Server.Mobiles;
using System.Text;

namespace Server.Engines.MLQuests.Definitions
{
    public class Discipline : MLQuest
    {
        public override Type NextQuest { get { return typeof(NeedsOfTheManySanctuary); } }

        public Discipline()
        {
            Activated = true;
            Title = 1072752; // Discipline
            Description = 1072761; // Learning to weave spells and control the forces of nature requires sacrifice, discipline, focus, and an unwavering dedication to Sosaria herself.  We do not teach the unworthy.  They do not comprehend the lessons nor the dedication required.  If you would walk the path of the Arcanist, then you must do as I require without hesitation or question.  Your first task is to rid our home of rats ... 50 of them in the next hour.
            RefusalMessage = 1072767; // *nods* Not everyone has the temperment to undertake the way of the Arcanist.
            InProgressMessage = 1072773; // You waste my time.  The task is simple. Kill 50 rats in an hour.
            // No completion message

            Objectives.Add(new TimedKillObjective(TimeSpan.FromHours(1), 50, new Type[] { typeof(Rat) }, "rats", new QuestArea(1074807, "Sanctuary"))); // Sanctuary

            Rewards.Add(new DummyReward(1074872)); // The opportunity to learn the ways of the Arcanist.
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Koole"), new Point3D(6257, 110, -10), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Koole"), new Point3D(6257, 110, -10), Map.Trammel);
        }
    }

    public class NeedsOfTheManySanctuary : MLQuest
    {
        public override Type NextQuest { get { return typeof(MakingAContributionSanctuary); } }
        public override bool IsChainTriggered { get { return true; } }

        public NeedsOfTheManySanctuary()
        {
            Activated = true;
            Title = 1072754; // Needs of the Many - Sanctuary
            Description = 1072763; // The way of the Arcanist involves cooperation with others and a strong committment to the community of your people.  We have run low on the cotton we use to pack wounds and our people have need.  Bring 10 bales of cotton to me.
            RefusalMessage = 1072768; // You endanger your progress along the path with your unwillingness.
            InProgressMessage = 1072775; // I care not where you acquire the cotton, merely that you provide it.
            CompletionMessage = 1074110; // Well, where are the cotton bales?

            Objectives.Add(new CollectObjective(10, typeof(Cotton), 1023577)); // bale of cotton

            Rewards.Add(new DummyReward(1074872)); // The opportunity to learn the ways of the Arcanist.
        }
    }

    public class MakingAContributionSanctuary : MLQuest
    {
        public override Type NextQuest { get { return typeof(SuppliesForSanctuary); } }
        public override bool IsChainTriggered { get { return true; } }

        public MakingAContributionSanctuary()
        {
            Activated = true;
            Title = 1072755; // Making a Contribution - Sanctuary
            Description = 1072764; // We must look to the defense of our people!  Bring boards for new arrows.
            RefusalMessage = 1072769; // The people have need of these items.  You are proving yourself inadequate to the demands of a member of this community.
            InProgressMessage = 1072776; // The requirements are simple -- 250 boards.
            CompletionMessage = 1074152; // Well, where are the boards?

            Objectives.Add(new CollectObjective(250, typeof(Board), 1027127)); // board

            Rewards.Add(new DummyReward(1074872)); // The opportunity to learn the ways of the Arcanist.
        }
    }

    public class SuppliesForSanctuary : MLQuest
    {
        public override Type NextQuest { get { return typeof(TheHumanBlight); } }
        public override bool IsChainTriggered { get { return true; } }

        public SuppliesForSanctuary()
        {
            Activated = true;
            Title = 1072756; // Supplies for Sanctuary
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

    public class TheHumanBlight : MLQuest
    {
        public override bool IsChainTriggered { get { return true; } }

        public TheHumanBlight()
        {
            Activated = true;
            Title = 1072757; // The Human Blight
            Description = 1072766; // You have proven your desire to contribute to the community and serve the people.  Now you must demonstrate your willingness to defend Sosaria from the greatest blight that plagues her.  The human vermin that have spread as a disease, despoiling the land are the greatest blight we face.  Kill humans and return to me the proof of your actions. Bring me 30 human ears.
            RefusalMessage = 1072771; // You must serve Sosaria with all your heart and strength.  Your unwillingness does not reflect favorably upon you.
            InProgressMessage = 1072778; // Why do you delay?  The human blight must be averted.
            CompletionMessage = 1074160; // I will take the ears you have collected now.  Hand them here.

            Objectives.Add(new CollectObjective(30, typeof(SeveredHumanEars), 1032591)); // severed human ears

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