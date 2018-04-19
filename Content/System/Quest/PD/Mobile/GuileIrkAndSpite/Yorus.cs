using System;

using Server;
using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.MLQuests.Definitions
{
    [QuesterName("Yorus (Ilshenar)")]
    public class Yorus : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }

        public override bool CanShout { get { return true; } }
        public override void Shout(PlayerMobile pm)
        {
            MLQuestSystem.Tell(this, pm, 1074218); // Hey!  I want to talk to you, now.
        }

        [Constructable]
        public Yorus()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Yorus";
            Title = "the tinker";
            Race = Race.Human;
            BodyValue = 0x190;
            Female = false;
            Hue = Race.RandomSkinHue();
            InitStats(100, 100, 25);

            Utility.AssignRandomHair(this, true);

            AddItem(new Shoes(Utility.RandomNeutralHue()));
            AddItem(new LongPants(Utility.RandomBlueHue()));
            AddItem(new FancyShirt(Utility.RandomOrangeHue()));
            AddItem(new Cloak(Utility.RandomBrightHue()));
        }

        public Yorus(Serial serial)
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