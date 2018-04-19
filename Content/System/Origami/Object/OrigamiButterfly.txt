using System;

using Server;
using Server.Network;

namespace Server.Items
{
    public class OrigamiButterfly : Item
    {
        public override int LabelNumber { get { return 1030296; } } // a delicate origami butterfly

        [Constructable]
        public OrigamiButterfly()
            : base(0x2838)
        {
            LootType = LootType.Blessed;
        }

        public OrigamiButterfly(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }
}