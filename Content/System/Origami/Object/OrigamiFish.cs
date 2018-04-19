using System;

using Server;
using Server.Network;

namespace Server.Items
{
    public class OrigamiFish : Item
    {
        public override int LabelNumber { get { return 1030301; } } // a delicate origami fish

        [Constructable]
        public OrigamiFish()
            : base(0x283D)
        {
            LootType = LootType.Blessed;
        }

        public OrigamiFish(Serial serial)
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