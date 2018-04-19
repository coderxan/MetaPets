using System;
using System.Collections.Generic;

using Server.Mobiles;
using Server.Network;

namespace Server.Items
{
    public class Spam : Food
    {
        [Constructable]
        public Spam()
            : base(0x1044)
        {
            Stackable = false;
            LootType = LootType.Blessed;
        }

        public Spam(Serial serial)
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