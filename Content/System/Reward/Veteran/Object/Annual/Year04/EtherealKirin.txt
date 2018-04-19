using System;

using Server.Engines.VeteranRewards;
using Server.Items;
using Server.Mobiles;
using Server.Spells;

namespace Server.Mobiles
{
    public class EtherealKirin : EtherealMount
    {
        public override int LabelNumber { get { return 1049746; } } // Ethereal Ki-Rin Statuette

        [Constructable]
        public EtherealKirin()
            : base(0x25A0, 0x3E9C)
        {
        }

        public EtherealKirin(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (Name == "an ethereal kirin")
                Name = null;
        }
    }
}