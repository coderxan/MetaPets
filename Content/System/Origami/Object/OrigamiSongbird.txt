using System;

using Server;
using Server.Network;

namespace Server.Items
{
    public class OrigamiSongbird : Item
    {
        public override int LabelNumber { get { return 1030300; } } // a delicate origami songbird

        [Constructable]
        public OrigamiSongbird()
            : base(0x283C)
        {
            LootType = LootType.Blessed;
        }

        public OrigamiSongbird(Serial serial)
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