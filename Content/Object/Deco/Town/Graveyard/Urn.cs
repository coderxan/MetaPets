﻿using System;

namespace Server.Items
{
    public class SmallUrn : Item
    {
        [Constructable]
        public SmallUrn()
            : base(0x241C)
        {
            Weight = 20.0;
        }

        public SmallUrn(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}