using System;

using Server.Engines.VeteranRewards;
using Server.Items;
using Server.Mobiles;
using Server.Spells;

namespace Server.Mobiles
{
    public class EtherealCuSidhe : EtherealMount
    {
        public override int LabelNumber { get { return 1080386; } } // Ethereal Cu Sidhe Statuette

        [Constructable]
        public EtherealCuSidhe()
            : base(0x2D96, 0x3E91)
        {
        }

        public EtherealCuSidhe(Serial serial)
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