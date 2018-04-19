using System;

namespace Server.Items
{
    public class DecoNightshade : Item
    {
        [Constructable]
        public DecoNightshade()
            : base(0x18E7)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoNightshade(Serial serial)
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

    public class DecoNightshade2 : Item
    {
        [Constructable]
        public DecoNightshade2()
            : base(0x18E5)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoNightshade2(Serial serial)
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

    public class DecoNightshade3 : Item
    {
        [Constructable]
        public DecoNightshade3()
            : base(0x18E6)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoNightshade3(Serial serial)
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

    public class DecoNightshade4 : Item
    {
        [Constructable]
        public DecoNightshade4()
            : base(0x18E8)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoNightshade4(Serial serial)
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