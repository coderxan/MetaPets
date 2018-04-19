using System;

using Server.Engines.VeteranRewards;
using Server.Items;
using Server.Mobiles;
using Server.Spells;

namespace Server.Mobiles
{
    public class EtherealHiryu : EtherealMount
    {
        public override int LabelNumber { get { return 1113813; } } // Ethereal Hiryu Statuette

        [Constructable]
        public EtherealHiryu()
            : base(0x276A, 0x3E94)
        {
        }

        public EtherealHiryu(Serial serial)
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