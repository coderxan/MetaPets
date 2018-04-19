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
    public class DefendingTheHerd : MLQuest
    {
        public override bool RecordCompletion { get { return true; } }

        public DefendingTheHerd()
        {
            Activated = true;
            Title = 1072785; // Defending the Herd
            Description = 1072825; // *snort* ... guard-mates ... guard-herd *hoof stomp* ... defend-with-hoof-and-horn ... thirsty-drink.  *proud head-toss*
            RefusalMessage = 1072826; // *snort*
            InProgressMessage = 1072827; // *impatient hoof stomp* ... thirsty herd ... water scent.
            CompletionNotice = CompletionNoticeShort;

            Objectives.Add(new EscortObjective(new QuestArea(1074779, "Bravehorn's drinking pool"))); // Bravehorn's drinking pool

            Rewards.Add(new DummyReward(1072806)); // The boon of Bravehorn.
        }

        public override void GetRewards(MLQuestInstance instance)
        {
            instance.Player.SendLocalizedMessage(1074942, "", 0x2A); // You have gained the boon of Bravehorn!  You have glimpsed the nobility of those that sacrifice themselves for their people.  You are one step closer to claiming your elven heritage.
            instance.ClaimRewards(); // skip gump
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(90), 0, 5, "Bravehorn"), new Point3D(1193, 2467, 0), Map.Felucca);
            PutSpawner(new Spawner(1, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(90), 0, 5, "Bravehorn"), new Point3D(1193, 2467, 0), Map.Trammel);

            PutSpawner(new Spawner(5, TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(30), 0, 8, "BravehornsMate"), new Point3D(1192, 2467, 0), Map.Felucca);
            PutSpawner(new Spawner(5, TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(30), 0, 8, "BravehornsMate"), new Point3D(1192, 2467, 0), Map.Trammel);
        }
    }
}