using System;

namespace Server.Items
{
    public class DecoTarot : Item
    {
        [Constructable]
        public DecoTarot()
            : base(0x12A5)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoTarot(Serial serial)
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

    public class DecoTarot2 : Item
    {
        [Constructable]
        public DecoTarot2()
            : base(0x12A6)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoTarot2(Serial serial)
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

    public class DecoTarot3 : Item
    {
        [Constructable]
        public DecoTarot3()
            : base(0x12A7)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoTarot3(Serial serial)
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

    public class DecoTarot4 : Item
    {
        [Constructable]
        public DecoTarot4()
            : base(0x12A8)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoTarot4(Serial serial)
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

    public class DecoTarot5 : Item
    {
        [Constructable]
        public DecoTarot5()
            : base(0x12A9)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoTarot5(Serial serial)
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

    public class DecoTarot6 : Item
    {
        [Constructable]
        public DecoTarot6()
            : base(0x12AA)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoTarot6(Serial serial)
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

    public class DecoTarot7 : Item
    {
        [Constructable]
        public DecoTarot7()
            : base(0x12A5)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoTarot7(Serial serial)
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