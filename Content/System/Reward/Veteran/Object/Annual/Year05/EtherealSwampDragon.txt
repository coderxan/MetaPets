using System;

using Server.Engines.VeteranRewards;
using Server.Items;
using Server.Mobiles;
using Server.Spells;

namespace Server.Mobiles
{
    public class EtherealSwampDragon : EtherealMount
    {
        public override int LabelNumber { get { return 1049749; } } // Ethereal Swamp Dragon Statuette

        [Constructable]
        public EtherealSwampDragon()
            : base(0x2619, 0x3E98)
        {
        }

        public EtherealSwampDragon(Serial serial)
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

            if (Name == "an ethereal swamp dragon")
                Name = null;
        }
    }
}