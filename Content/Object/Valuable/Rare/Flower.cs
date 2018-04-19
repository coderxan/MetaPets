using System;

namespace Server.Items
{
    public class DecoFlower : Item
    {
        [Constructable]
        public DecoFlower()
            : base(0x18DA)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoFlower(Serial serial)
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

    public class DecoFlower2 : Item
    {
        [Constructable]
        public DecoFlower2()
            : base(0x18D9)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoFlower2(Serial serial)
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