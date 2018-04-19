using System;

using Server;
using Server.Network;

namespace Server.Items
{
    public class OrigamiShape : Item
    {
        public override int LabelNumber { get { return 1030299; } } // an intricate geometric origami shape

        [Constructable]
        public OrigamiShape()
            : base(0x283B)
        {
            LootType = LootType.Blessed;
        }

        public OrigamiShape(Serial serial)
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