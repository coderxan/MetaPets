using System;
using System.Collections;
using System.Collections.Generic;

using Server.ContextMenus;
using Server.Mobiles;
using Server.Multis;
using Server.Network;

namespace Server.Items
{
    [Furniture]
    [Flipable(0xe43, 0xe42)]
    public class WoodenChest : LockableContainer
    {
        [Constructable]
        public WoodenChest()
            : base(0xe43)
        {
            Weight = 2.0;
        }

        public WoodenChest(Serial serial)
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

            if (Weight == 15.0)
                Weight = 2.0;
        }
    }

    [Flipable(0xE43, 0xE42)]
    public class FillableWoodenChest : FillableContainer
    {
        [Constructable]
        public FillableWoodenChest()
            : base(0xE43)
        {
        }

        public FillableWoodenChest(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version == 0 && Weight == 2)
                Weight = -1;
        }
    }

    [Furniture]
    [Flipable(0x280B, 0x280C)]
    public class PlainWoodenChest : LockableContainer
    {
        [Constructable]
        public PlainWoodenChest()
            : base(0x280B)
        {
        }

        public PlainWoodenChest(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version == 0 && Weight == 15)
                Weight = -1;
        }
    }

    [Furniture]
    [Flipable(0x2813, 0x2814)]
    public class FinishedWoodenChest : LockableContainer
    {
        [Constructable]
        public FinishedWoodenChest()
            : base(0x2813)
        {
        }

        public FinishedWoodenChest(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version == 0 && Weight == 15)
                Weight = -1;
        }
    }

    [Furniture]
    [Flipable(0x280D, 0x280E)]
    public class OrnateWoodenChest : LockableContainer
    {
        [Constructable]
        public OrnateWoodenChest()
            : base(0x280D)
        {
        }

        public OrnateWoodenChest(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version == 0 && Weight == 15)
                Weight = -1;
        }
    }

    [Furniture]
    [Flipable(0x280F, 0x2810)]
    public class GildedWoodenChest : LockableContainer
    {
        [Constructable]
        public GildedWoodenChest()
            : base(0x280F)
        {
        }

        public GildedWoodenChest(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version == 0 && Weight == 15)
                Weight = -1;
        }
    }

    [DynamicFliping]
    [Flipable(0x9AB, 0xE7C)]
    public class MetalChest : LockableContainer
    {
        [Constructable]
        public MetalChest()
            : base(0x9AB)
        {
        }

        public MetalChest(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version == 0 && Weight == 25)
                Weight = -1;
        }
    }

    [Flipable(0x9AB, 0xE7C)]
    public class FillableMetalChest : FillableContainer
    {
        [Constructable]
        public FillableMetalChest()
            : base(0x9AB)
        {
        }

        public FillableMetalChest(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version == 0 && Weight == 25)
                Weight = -1;
        }
    }

    [DynamicFliping]
    [Flipable(0xE41, 0xE40)]
    public class MetalGoldenChest : LockableContainer
    {
        [Constructable]
        public MetalGoldenChest()
            : base(0xE41)
        {
        }

        public MetalGoldenChest(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version == 0 && Weight == 25)
                Weight = -1;
        }
    }

    [Flipable(0xE41, 0xE40)]
    public class FillableMetalGoldenChest : FillableContainer
    {
        [Constructable]
        public FillableMetalGoldenChest()
            : base(0xE41)
        {
        }

        public FillableMetalGoldenChest(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version == 0 && Weight == 25)
                Weight = -1;
        }
    }
}