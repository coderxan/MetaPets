using System;

using Server.Network;

namespace Server.Items
{
    public class PeculiarFish : BaseMagicFish
    {
        public override int LabelNumber { get { return 1041076; } } // highly peculiar fish

        [Constructable]
        public PeculiarFish()
            : base(66)
        {
        }

        public PeculiarFish(Serial serial)
            : base(serial)
        {
        }

        public override bool Apply(Mobile from)
        {
            from.Stam += 10;
            return true;
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

            if (Hue == 266)
                Hue = 66;
        }
    }
}