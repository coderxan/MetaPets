using System;

using Server.Items;
using Server.Network;

namespace Server.Items
{
    public class BlueSnowflake : Item
    {
        [Constructable]
        public BlueSnowflake()
            : base(0x232E)
        {
            Weight = 1.0;
            LootType = LootType.Blessed;
        }

        public BlueSnowflake(Serial serial)
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
        }
    }
}