using System;

using Server.Engines.VeteranRewards;
using Server.Items;
using Server.Mobiles;
using Server.Spells;

namespace Server.Mobiles
{
    public class EtherealRidgeback : EtherealMount
    {
        public override int LabelNumber { get { return 1049747; } } // Ethereal Ridgeback Statuette

        [Constructable]
        public EtherealRidgeback()
            : base(0x2615, 0x3E9A)
        {
        }

        public EtherealRidgeback(Serial serial)
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

            if (Name == "an ethereal ridgeback")
                Name = null;
        }
    }
}