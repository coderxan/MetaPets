using System;

using Server;
using Server.Network;

namespace Server.Items
{
    public class OrigamiFrog : Item
    {
        public override int LabelNumber { get { return 1030298; } } // a delicate origami frog

        [Constructable]
        public OrigamiFrog()
            : base(0x283A)
        {
            LootType = LootType.Blessed;
        }

        public OrigamiFrog(Serial serial)
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