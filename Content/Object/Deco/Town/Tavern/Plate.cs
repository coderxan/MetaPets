﻿using System;

namespace Server.Items
{
    public class Plate : Item
    {
        [Constructable]
        public Plate()
            : base(0x9D7)
        {
            Weight = 1.0;
        }

        public Plate(Serial serial)
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