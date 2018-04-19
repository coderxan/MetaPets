using System;
using System.Collections.Generic;

using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Items;
using Server.Mobiles;
using System.Text;

namespace Server.Engines.MLQuests.Definitions
{
    public class FiendishFriends : MLQuest
    {
        public override Type NextQuest { get { return typeof(CrackingTheWhipI); } }

        public FiendishFriends()
        {
            Activated = true;
            Title = 1074283; // Fiendish Friends
            Description = 1074285; // It is true that a skilled arcanist can summon and dominate an imp to serve at their pleasure.  To do such at thing though, you must master the miserable little fiends utterly by demonstrating your superiority.  Rough them up some -- kill a few.  That will do the trick.
            RefusalMessage = 1074287; // You're probably right.  They're not worth the effort.
            InProgressMessage = 1074289; // Surely you're not having difficulties swatting down those annoying pests?
            // TODO: Verify
            CompletionMessage = 1074291; // Hah!  You showed them!

            Objectives.Add(new KillObjective(50, new Type[] { typeof(Imp) }, "imps"));

            Rewards.Add(new DummyReward(1074873)); // The opportunity to prove yourself worthy of learning to Summon Fiends. (Sufficient spellweaving skill is required to cast the spell)
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "ElderBrae"), new Point3D(6266, 124, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 0, "ElderBrae"), new Point3D(6266, 124, 0), Map.Trammel);
        }
    }

    // TODO: Verify
    public class CrackingTheWhipI : MLQuest
    {
        public override Type NextQuest { get { return typeof(CrackingTheWhipII); } }
        public override bool IsChainTriggered { get { return true; } }

        public CrackingTheWhipI()
        {
            Activated = true;
            Title = 1074295; // Cracking the Whip
            Description = 1074300; // Now that you've shown those mini pests your might, you should collect suitable implements to use to train your summoned pet.  I suggest a stout whip.
            RefusalMessage = 1074313; // Heh. Changed your mind, eh?
            InProgressMessage = 1074317; // Well, hurry up.  If you don't get a whip how do you expect to control the little devil?
            CompletionMessage = 1074321; // That's a well-made whip.  No imp will ignore the sting of that lash.

            Objectives.Add(new CollectObjective(1, typeof(StoutWhip), "Stout Whip"));

            Rewards.Add(new DummyReward(1074873)); // The opportunity to prove yourself worthy of learning to Summon Fiends. (Sufficient spellweaving skill is required to cast the spell)
        }
    }

    // TODO: Verify
    public class CrackingTheWhipII : MLQuest
    {
        public override bool IsChainTriggered { get { return true; } }

        public CrackingTheWhipII()
        {
            Activated = true;
            Title = 1074295; // Cracking the Whip
            Description = 1074302; // Now you just need to make the little buggers fear you -- if you can slay an arcane daemon, you'll earn their subservience.
            RefusalMessage = 1074314; // If you're not up for it, so be it.
            InProgressMessage = 1074318; // You need to vanquish an arcane daemon before the imps will fear you properly.

            Objectives.Add(new KillObjective(1, new Type[] { typeof(ArcaneDaemon) }, 1029733)); // arcane demon

            Rewards.Add(new ItemReward(1031608, typeof(SummonFiendScroll))); // Summon Fiend
        }

        public override void GetRewards(MLQuestInstance instance)
        {
            instance.PlayerContext.SummonFiend = true;
            instance.Player.SendLocalizedMessage(1074322, "", 0x2A); // You've demonstrated your strength, got a means of control, and taught the imps to fear you.  You're ready now to summon them.

            base.GetRewards(instance);
        }
    }
}