using System;

namespace Server.Items
{
    public class DirtySmallPot : Item
    {
        [Constructable]
        public DirtySmallPot()
            : base(0x9DD)
        {
            Weight = 1.0;
        }

        public DirtySmallPot(Serial serial)
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