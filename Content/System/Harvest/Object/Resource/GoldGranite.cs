﻿using System;

using Server.Items;
using Server.Network;

namespace Server.Items
{
    public class GoldGranite : BaseGranite
    {
        [Constructable]
        public GoldGranite()
            : base(CraftResource.Gold)
        {
        }

        public GoldGranite(Serial serial)
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