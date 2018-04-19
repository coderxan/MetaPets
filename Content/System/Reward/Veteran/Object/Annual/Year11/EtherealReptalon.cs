using System;

using Server.Engines.VeteranRewards;
using Server.Items;
using Server.Mobiles;
using Server.Spells;

namespace Server.Mobiles
{
    public class EtherealReptalon : EtherealMount
    {
        public override int LabelNumber { get { return 1113812; } } // Ethereal Reptalon Statuette

        [Constructable]
        public EtherealReptalon()
            : base(0x2d95, 0x3e90)
        {
        }

        public EtherealReptalon(Serial serial)
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