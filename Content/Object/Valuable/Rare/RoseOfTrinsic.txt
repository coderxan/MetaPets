using System;

namespace Server.Items
{
    public class DecoRoseOfTrinsic : Item
    {
        [Constructable]
        public DecoRoseOfTrinsic()
            : base(0x234C)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoRoseOfTrinsic(Serial serial)
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

    public class DecoRoseOfTrinsic2 : Item
    {
        [Constructable]
        public DecoRoseOfTrinsic2()
            : base(0x234D)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoRoseOfTrinsic2(Serial serial)
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

    public class DecoRoseOfTrinsic3 : Item
    {
        [Constructable]
        public DecoRoseOfTrinsic3()
            : base(0x234B)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoRoseOfTrinsic3(Serial serial)
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