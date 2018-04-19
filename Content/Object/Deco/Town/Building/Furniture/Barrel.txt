using System;
using System.Collections;
using System.Collections.Generic;

using Server;
using Server.ContextMenus;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Network;

namespace Server.Items
{
    public class Barrel : BaseContainer
    {
        [Constructable]
        public Barrel()
            : base(0xE77)
        {
            Weight = 25.0;
        }

        public Barrel(Serial serial)
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

            if (Weight == 0.0)
                Weight = 25.0;
        }
    }

    public class FillableBarrel : FillableContainer
    {
        public override bool IsLockable { get { return false; } }

        [Constructable]
        public FillableBarrel()
            : base(0xE77)
        {
        }

        public FillableBarrel(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();

            if (version == 0 && Weight == 25)
                Weight = -1;
        }
    }
}

namespace Server.Multis
{
    public class LockableBarrel : LockableContainer
    {
        [Constructable]
        public LockableBarrel()
            : base(0xE77)
        {
            Weight = 1.0;
        }

        public LockableBarrel(Serial serial)
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

            if (Weight == 8.0)
                Weight = 1.0;
        }
    }
}