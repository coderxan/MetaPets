using System;

namespace Server.Items
{
    public class DirtyFrypan : Item
    {
        [Constructable]
        public DirtyFrypan()
            : base(0x9DE)
        {
            Weight = 1.0;
        }

        public DirtyFrypan(Serial serial)
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