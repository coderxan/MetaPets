﻿using System;

using Server;

namespace Server.Items
{
    [Furniture]
    [FlipableAttribute(0x2bdb, 0x2bdc)]
    public class RedStocking : BaseContainer
    {
        public override int DefaultGumpID { get { return 0x103; } }
        public override int DefaultDropSound { get { return 0x42; } }

        [Constructable]
        public RedStocking()
            : base(Utility.Random(0x2BDB, 2))
        {
        }

        public RedStocking(Serial serial)
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