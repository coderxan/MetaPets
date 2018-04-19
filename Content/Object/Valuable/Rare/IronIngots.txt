using System;

namespace Server.Items
{
    public class DecoIronIngots : Item
    {
        [Constructable]
        public DecoIronIngots()
            : base(0x1BF1)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoIronIngots(Serial serial)
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

    public class DecoIronIngots2 : Item
    {
        [Constructable]
        public DecoIronIngots2()
            : base(0x1BF0)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoIronIngots2(Serial serial)
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

    public class DecoIronIngots3 : Item
    {
        [Constructable]
        public DecoIronIngots3()
            : base(0x1BF0)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoIronIngots3(Serial serial)
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

    public class DecoIronIngots4 : Item
    {
        [Constructable]
        public DecoIronIngots4()
            : base(0x1BF1)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoIronIngots4(Serial serial)
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

    public class DecoIronIngots5 : Item
    {
        [Constructable]
        public DecoIronIngots5()
            : base(0x1BF3)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoIronIngots5(Serial serial)
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

    public class DecoIronIngots6 : Item
    {
        [Constructable]
        public DecoIronIngots6()
            : base(0x1BF4)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoIronIngots6(Serial serial)
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