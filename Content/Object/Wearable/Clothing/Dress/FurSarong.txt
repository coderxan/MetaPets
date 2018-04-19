using System;

namespace Server.Items
{
    [Flipable(0x230C, 0x230B)]
    public class FurSarong : BaseOuterLegs
    {
        [Constructable]
        public FurSarong()
            : this(0)
        {
        }

        [Constructable]
        public FurSarong(int hue)
            : base(0x230C, hue)
        {
            Weight = 3.0;
        }

        public FurSarong(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (Weight == 4.0)
                Weight = 3.0;
        }
    }
}