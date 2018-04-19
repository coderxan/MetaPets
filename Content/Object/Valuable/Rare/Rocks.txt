using System;

namespace Server.Items
{
    public class DecoRocks : Item
    {
        [Constructable]
        public DecoRocks()
            : base(0x1367)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoRocks(Serial serial)
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

    public class DecoRocks2 : Item
    {
        [Constructable]
        public DecoRocks2()
            : base(0x136D)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoRocks2(Serial serial)
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