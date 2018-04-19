using System;

namespace Server.Items
{
    public class SilverWire : Item
    {
        [Constructable]
        public SilverWire()
            : this(1)
        {
        }

        [Constructable]
        public SilverWire(int amount)
            : base(0x1877)
        {
            Stackable = true;
            Weight = 5.0;
            Amount = amount;
        }

        public SilverWire(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version < 1 && Weight == 2.0)
                Weight = 5.0;
        }
    }
}