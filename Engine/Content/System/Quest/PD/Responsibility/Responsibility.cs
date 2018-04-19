using System;

using Server;
using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.MLQuests.Definitions
{
    public class Responsibility : BaseEscort
    {
        public Responsibility()
        {
            Activated = true;
            Title = 1074352; // Responsibility
            Description = 1074524; // Oh!  I just don't know what to do.  My mother is away and my father told me not to talk to strangers ... *worried frown*  But my grandfather has sent word that he has been hurt and needs me to tend his wounds.  He has a small farm southeast of here.  Would you ... could you ... escort me there safely?
            RefusalMessage = 1074525; // I hope my grandfather will be alright.
            InProgressMessage = 1074526; // Grandfather's farm is a ways west of the Shrine of Spirituality. So, we're not quite there yet.  Thank you again for keeping me safe.

            Objectives.Add(new EscortObjective(new QuestArea(1074781, "Sheep Farm"))); // Sheep Farm

            Rewards.Add(ItemReward.BagOfTrinkets);
        }

        // OSI sends this instead, but it doesn't make sense for an escortable
        //public override void OnComplete( MLQuestInstance instance )
        //{
        //	instance.Player.SendLocalizedMessage( 1073775, "", 0x23 ); // Your quest is complete. Return for your reward.
        //}

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30), 0, 5, "Lissbet"), new Point3D(1568, 1040, -7), Map.Ilshenar);
            PutSpawner(new Spawner(1, 5, 10, 0, 8, "GrandpaCharley"), new Point3D(1322, 1331, -14), Map.Ilshenar);
            PutSpawner(new Spawner(1, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30), 0, 3, "Sheep"), new Point3D(1308, 1324, -14), Map.Ilshenar);
        }
    }
}