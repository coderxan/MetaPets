﻿using System;

using Server.Items;
using Server.Network;

namespace Server.Items
{
    public class CopperGranite : BaseGranite
    {
        [Constructable]
        public CopperGranite()
            : base(CraftResource.Copper)
        {
        }

        public CopperGranite(Serial serial)
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