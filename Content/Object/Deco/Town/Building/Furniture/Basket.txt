using System;
using System.Collections.Generic;

using Server.ContextMenus;
using Server.Mobiles;
using Server.Multis;
using Server.Network;

namespace Server.Items
{
    public class Basket : BaseContainer
    {
        [Constructable]
        public Basket()
            : base(0x990)
        {
            Weight = 1.0; // Stratics doesn't know weight
        }

        public Basket(Serial serial)
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