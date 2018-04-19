using System;

using Server;

namespace Server.Items
{
    public class DarkChocolate : CandyCane
    {
        public override int LabelNumber { get { return 1079994; } } // Dark chocolate
        public override double DefaultWeight { get { return 1.0; } }

        [Constructable]
        public DarkChocolate()
            : base(0xF10)
        {
            Hue = 0x465;
            LootType = LootType.Regular;
        }

        public DarkChocolate(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class MilkChocolate : CandyCane
    {
        public override int LabelNumber { get { return 1079995; } } // Milk chocolate
        public override double DefaultWeight { get { return 1.0; } }

        [Constructable]
        public MilkChocolate()
            : base(0xF18)
        {
            Hue = 0x461;
            LootType = LootType.Regular;
        }

        public MilkChocolate(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class WhiteChocolate : CandyCane
    {
        public override int LabelNumber { get { return 1079996; } } // White chocolate
        public override double DefaultWeight { get { return 1.0; } }

        [Constructable]
        public WhiteChocolate()
            : base(0xF11)
        {
            Hue = 0x47E;
            LootType = LootType.Regular;
        }

        public WhiteChocolate(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}