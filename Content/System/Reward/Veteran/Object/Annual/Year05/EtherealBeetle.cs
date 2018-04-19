using System;

using Server.Engines.VeteranRewards;
using Server.Items;
using Server.Mobiles;
using Server.Spells;

namespace Server.Mobiles
{
    public class EtherealBeetle : EtherealMount
    {
        public override int LabelNumber { get { return 1049748; } } // Ethereal Beetle Statuette

        [Constructable]
        public EtherealBeetle()
            : base(0x260F, 0x3E97)
        {
        }

        public EtherealBeetle(Serial serial)
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

            if (Name == "an ethereal beetle")
                Name = null;
        }
    }
}