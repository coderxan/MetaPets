using System;
using System.Collections.Generic;

using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Items;
using Server.Mobiles;
using System.Text;

namespace Server.Engines.MLQuests.Definitions
{
    public class FriendOfTheFey : MLQuest
    {
        public override Type NextQuest { get { return typeof(TokenOfFriendship); } }

        public FriendOfTheFey()
        {
            Activated = true;
            Title = 1074284; // Friend of the Fey
            Description = 1074286; // The children of Sosaria understand the dedication and committment of an arcanist -- and will, from time to time offer their friendship.  If you would forge such a bond, first seek out a goodwill offering to present.  Pixies enjoy sweets and pretty things.
            RefusalMessage = 1074288; // There's always time to make new friends.
            InProgressMessage = 1074290; // I think honey and some sparkly beads would please a pixie.
            CompletionMessage = 1074292; // What have we here? Oh yes, gifts for a pixie.

            Objectives.Add(new CollectObjective(1, typeof(Beads), 1024235)); // beads
            Objectives.Add(new CollectObjective(1, typeof(JarHoney), 1022540)); // jar of honey

            Rewards.Add(new DummyReward(1074874)); // The opportunity to prove yourself worthy of learning to Summon Fey. (Sufficient spellweaving skill is required to cast the spell)
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 3, "Synaeva"), new Point3D(7064, 350, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 3, "Synaeva"), new Point3D(7064, 350, 0), Map.Trammel);
        }
    }

    public class TokenOfFriendship : MLQuest
    {
        public override Type NextQuest { get { return typeof(Alliance); } }
        public override bool IsChainTriggered { get { return true; } }

        public TokenOfFriendship()
        {
            Activated = true;
            Title = 1074293; // Token of Friendship
            Description = 1074297; // I've wrapped your gift suitably to present to a pixie of discriminating taste.  Seek out Arielle and give her your offering.
            RefusalMessage = 1074310; // I'll hold onto this gift in case you change your mind.
            InProgressMessage = 1074315; // Arielle wanders quite a bit, so I'm not sure exactly where to find her.  I'm sure she's going to love your gift.
            CompletionMessage = 1074319; // *giggle*  Oooh!  For me?

            Objectives.Add(new DeliverObjective(typeof(GiftForArielle), 1, "gift for Arielle", typeof(Arielle)));

            Rewards.Add(new DummyReward(1074874)); // The opportunity to prove yourself worthy of learning to Summon Fey. (Sufficient spellweaving skill is required to cast the spell)
        }
    }

    public class Alliance : MLQuest
    {
        public override bool IsChainTriggered { get { return true; } }

        public Alliance()
        {
            Activated = true;
            Title = 1074294; // Alliance
            Description = 1074298; // *giggle* Mean reapers make pixies unhappy.  *light-hearted giggle*  You could fix them!
            RefusalMessage = 1074311; // *giggle* Okies!
            InProgressMessage = 1074316; // Mean reapers are all around trees!  *giggle*  You fix them up, please.
            CompletionNotice = CompletionNoticeShortReturn;

            Objectives.Add(new KillObjective(20, new Type[] { typeof(Reaper) }, "reapers"));

            Rewards.Add(new ItemReward(1031607, typeof(SummonFeyScroll))); // Summon Fey
        }

        public override void GetRewards(MLQuestInstance instance)
        {
            instance.PlayerContext.SummonFey = true;
            instance.Player.SendLocalizedMessage(1074320, "", 0x2A); // *giggle* Mean reapers got fixed!  Pixie friend now! *giggle* When mean thingies bother you, a brave pixie will help.

            base.GetRewards(instance);
        }
    }
}