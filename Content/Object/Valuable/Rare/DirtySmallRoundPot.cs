using System;

namespace Server.Items
{
    public class DirtySmallRoundPot : Item
    {
        [Constructable]
        public DirtySmallRoundPot()
            : base(0x9E7)
        {
            Weight = 1.0;
        }

        public DirtySmallRoundPot(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }
}