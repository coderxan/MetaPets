using System;

namespace Server.Items
{
    public class DirtyRoundPot : Item
    {
        [Constructable]
        public DirtyRoundPot()
            : base(0x9DF)
        {
            Weight = 1.0;
        }

        public DirtyRoundPot(Serial serial)
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