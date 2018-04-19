using System;
using System.Collections.Generic;

using Server.ContextMenus;
using Server.Mobiles;
using Server.Multis;
using Server.Network;

namespace Server.Items
{
    public class Keg : BaseContainer
    {
        [Constructable]
        public Keg()
            : base(0xE7F)
        {
            Weight = 15.0;
        }

        public Keg(Serial serial)
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