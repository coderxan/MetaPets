using System;

using Server;
using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.MLQuests.Definitions
{
    [QuesterName("Jelrice (Ilshenar)")]
    public class Jelrice : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }

        public override bool CanShout { get { return true; } }
        public override void Shout(PlayerMobile pm)
        {
            MLQuestSystem.Tell(this, pm, 1074221); // Greetings!  I have a small task for you good traveler.
        }

        [Constructable]
        public Jelrice()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Jelrice";
            Title = "the trader";
            Race = Race.Human;
            BodyValue = 0x191;
            Female = true;
            Hue = Race.RandomSkinHue();
            InitStats(100, 100, 25);

            Utility.AssignRandomHair(this, true);

            AddItem(new Shoes(Utility.RandomNeutralHue()));
            AddItem(new Skirt(Utility.RandomBlueHue()));
            AddItem(new FancyShirt(Utility.RandomRedHue()));
        }

        public Jelrice(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}