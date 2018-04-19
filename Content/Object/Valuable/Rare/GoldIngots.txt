using System;

namespace Server.Items
{
    public class DecoGoldIngots : Item
    {
        [Constructable]
        public DecoGoldIngots()
            : base(0x1BEA)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoGoldIngots(Serial serial)
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

    public class DecoGoldIngots2 : Item
    {
        [Constructable]
        public DecoGoldIngots2()
            : base(0x1BEB)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoGoldIngots2(Serial serial)
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

    public class DecoGoldIngots3 : Item
    {
        [Constructable]
        public DecoGoldIngots3()
            : base(0x1BED)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoGoldIngots3(Serial serial)
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

    public class DecoGoldIngots4 : Item
    {
        [Constructable]
        public DecoGoldIngots4()
            : base(0x1BEE)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoGoldIngots4(Serial serial)
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