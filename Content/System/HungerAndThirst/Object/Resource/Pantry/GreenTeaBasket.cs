﻿using System;

namespace Server.Items
{
    public class GreenTeaBasket : Item
    {
        [Constructable]
        public GreenTeaBasket()
            : base(0x284B)
        {
            Weight = 10.0;
        }

        public GreenTeaBasket(Serial serial)
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