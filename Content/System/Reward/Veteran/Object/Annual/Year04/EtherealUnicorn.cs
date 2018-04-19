using System;

using Server.Engines.VeteranRewards;
using Server.Items;
using Server.Mobiles;
using Server.Spells;

namespace Server.Mobiles
{
    public class EtherealUnicorn : EtherealMount
    {
        public override int LabelNumber { get { return 1049745; } } // Ethereal Unicorn Statuette

        [Constructable]
        public EtherealUnicorn()
            : base(0x25CE, 0x3E9B)
        {
        }

        public EtherealUnicorn(Serial serial)
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

            if (Name == "an ethereal unicorn")
                Name = null;
        }
    }
}