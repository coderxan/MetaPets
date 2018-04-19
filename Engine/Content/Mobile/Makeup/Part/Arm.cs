﻿using System;

using Server;

namespace Server.Items
{
    public class RightArm : Item
    {
        [Constructable]
        public RightArm()
            : base(0x1DA2)
        {
        }

        public RightArm(Serial serial)
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

    public class LeftArm : Item
    {
        [Constructable]
        public LeftArm()
            : base(0x1DA1)
        {
        }

        public LeftArm(Serial serial)
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