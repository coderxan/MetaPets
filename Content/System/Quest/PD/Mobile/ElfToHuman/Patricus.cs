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
    [QuesterName("Patricus (Vesper)")]
    public class Patricus : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }

        [Constructable]
        public Patricus()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Patricus";
            Title = "the Trader";
            Body = 400;
            Hue = Race.RandomSkinHue();
            InitStats(100, 100, 25);

            AddItem(new FancyShirt(Utility.RandomNeutralHue()));
            AddItem(new LongPants(Utility.RandomBrightHue()));
            AddItem(new Cloak(0x1BB));
            AddItem(new Shoes(Utility.RandomNeutralHue()));
            AddItem(new Backpack());
        }

        public Patricus(Serial serial)
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