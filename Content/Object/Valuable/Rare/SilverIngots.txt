using System;

namespace Server.Items
{
    public class DecoSilverIngots : Item
    {
        [Constructable]
        public DecoSilverIngots()
            : base(0x1BFA)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoSilverIngots(Serial serial)
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

    public class DecoSilverIngots2 : Item
    {
        [Constructable]
        public DecoSilverIngots2()
            : base(0x1BF6)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoSilverIngots2(Serial serial)
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

    public class DecoSilverIngots3 : Item
    {
        [Constructable]
        public DecoSilverIngots3()
            : base(0x1BF7)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoSilverIngots3(Serial serial)
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

    public class DecoSilverIngots4 : Item
    {
        [Constructable]
        public DecoSilverIngots4()
            : base(0x1BF9)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoSilverIngots4(Serial serial)
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

    public class DecoSilverIngots5 : Item
    {
        [Constructable]
        public DecoSilverIngots5()
            : base(0x1BFA)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoSilverIngots5(Serial serial)
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