using System;

namespace Server.Items
{
    public class Cards : Item
    {
        [Constructable]
        public Cards()
            : base(0xE19)
        {
            Movable = true;
            Stackable = false;
        }

        public Cards(Serial serial)
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

    public class Cards2 : Item
    {
        [Constructable]
        public Cards2()
            : base(0xE16)
        {
            Movable = true;
            Stackable = false;
        }

        public Cards2(Serial serial)
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

    public class Cards3 : Item
    {
        [Constructable]
        public Cards3()
            : base(0xE15)
        {
            Movable = true;
            Stackable = false;
        }

        public Cards3(Serial serial)
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

    public class Cards4 : Item
    {
        [Constructable]
        public Cards4()
            : base(0xE17)
        {
            Movable = true;
            Stackable = false;
        }

        public Cards4(Serial serial)
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

    public class Cards5 : Item
    {
        [Constructable]
        public Cards5()
            : base(0xE18)
        {
            Movable = true;
            Stackable = false;
        }

        public Cards5(Serial serial)
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