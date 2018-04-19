using System;

namespace Server.Items
{
    [Flipable(0x2310, 0x230F)]
    public class FormalShirt : BaseMiddleTorso
    {
        [Constructable]
        public FormalShirt()
            : this(0)
        {
        }

        [Constructable]
        public FormalShirt(int hue)
            : base(0x2310, hue)
        {
            Weight = 1.0;
        }

        public FormalShirt(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            if (Weight == 2.0)
                Weight = 1.0;
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}