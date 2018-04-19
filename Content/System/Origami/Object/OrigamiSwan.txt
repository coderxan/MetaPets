using System;

using Server;
using Server.Network;

namespace Server.Items
{
    public class OrigamiSwan : Item
    {
        public override int LabelNumber { get { return 1030297; } } // a delicate origami swan

        [Constructable]
        public OrigamiSwan()
            : base(0x2839)
        {
            LootType = LootType.Blessed;
        }

        public OrigamiSwan(Serial serial)
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