using System;
using System.Collections.Generic;

using Server.ContextMenus;
using Server.Mobiles;
using Server.Multis;
using Server.Network;

namespace Server.Items
{
    public class PicnicBasket : BaseContainer
    {
        [Constructable]
        public PicnicBasket()
            : base(0xE7A)
        {
            Weight = 2.0; // Stratics doesn't know weight
        }

        public PicnicBasket(Serial serial)
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