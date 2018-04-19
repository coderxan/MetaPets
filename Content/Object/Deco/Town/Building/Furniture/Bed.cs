using System;

using Server;

namespace Server.Items
{
    /// <summary>
    /// Small Bed
    /// </summary>
    public class SmallBedEastAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new SmallBedEastDeed(); } }

        [Constructable]
        public SmallBedEastAddon()
        {
            AddComponent(new AddonComponent(0xA5D), 0, 0, 0);
            AddComponent(new AddonComponent(0xA62), 1, 0, 0);
        }

        public SmallBedEastAddon(Serial serial)
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

    public class SmallBedEastDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new SmallBedEastAddon(); } }
        public override int LabelNumber { get { return 1044322; } } // small bed (east)

        [Constructable]
        public SmallBedEastDeed()
        {
        }

        public SmallBedEastDeed(Serial serial)
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

    public class SmallBedSouthAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new SmallBedSouthDeed(); } }

        [Constructable]
        public SmallBedSouthAddon()
        {
            AddComponent(new AddonComponent(0xA63), 0, 0, 0);
            AddComponent(new AddonComponent(0xA5C), 0, 1, 0);
        }

        public SmallBedSouthAddon(Serial serial)
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

    public class SmallBedSouthDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new SmallBedSouthAddon(); } }
        public override int LabelNumber { get { return 1044321; } } // small bed (south)

        [Constructable]
        public SmallBedSouthDeed()
        {
        }

        public SmallBedSouthDeed(Serial serial)
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

    /// <summary>
    /// Large Bed
    /// </summary>
    public class LargeBedEastAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new LargeBedEastDeed(); } }

        [Constructable]
        public LargeBedEastAddon()
        {
            AddComponent(new AddonComponent(0xA7D), 0, 0, 0);
            AddComponent(new AddonComponent(0xA7C), 0, 1, 0);
            AddComponent(new AddonComponent(0xA79), 1, 0, 0);
            AddComponent(new AddonComponent(0xA78), 1, 1, 0);
        }

        public LargeBedEastAddon(Serial serial)
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

    public class LargeBedEastDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new LargeBedEastAddon(); } }
        public override int LabelNumber { get { return 1044324; } } // large bed (east)

        [Constructable]
        public LargeBedEastDeed()
        {
        }

        public LargeBedEastDeed(Serial serial)
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

    public class LargeBedSouthAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new LargeBedSouthDeed(); } }

        [Constructable]
        public LargeBedSouthAddon()
        {
            AddComponent(new AddonComponent(0xA83), 0, 0, 0);
            AddComponent(new AddonComponent(0xA7F), 0, 1, 0);
            AddComponent(new AddonComponent(0xA82), 1, 0, 0);
            AddComponent(new AddonComponent(0xA7E), 1, 1, 0);
        }

        public LargeBedSouthAddon(Serial serial)
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

    public class LargeBedSouthDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new LargeBedSouthAddon(); } }
        public override int LabelNumber { get { return 1044323; } } // large bed (south)

        [Constructable]
        public LargeBedSouthDeed()
        {
        }

        public LargeBedSouthDeed(Serial serial)
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

    /// <summary>
    /// Elven Bed
    /// </summary>
    public class ElvenBedEastAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new ElvenBedEastDeed(); } }

        [Constructable]
        public ElvenBedEastAddon()
        {
            AddComponent(new AddonComponent(0x304D), 0, 0, 0);
            AddComponent(new AddonComponent(0x304C), 1, 0, 0);
        }

        public ElvenBedEastAddon(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }

    public class ElvenBedEastDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new ElvenBedEastAddon(); } }
        public override int LabelNumber { get { return 1072861; } } // elven bed (east)

        [Constructable]
        public ElvenBedEastDeed()
        {
        }

        public ElvenBedEastDeed(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }

    public class ElvenBedSouthAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new ElvenBedSouthDeed(); } }

        [Constructable]
        public ElvenBedSouthAddon()
        {
            AddComponent(new AddonComponent(0x3050), 0, 0, 0);
            AddComponent(new AddonComponent(0x3051), 0, -1, 0);
        }

        public ElvenBedSouthAddon(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }

    public class ElvenBedSouthDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new ElvenBedSouthAddon(); } }
        public override int LabelNumber { get { return 1072860; } } // elven bed (south)

        [Constructable]
        public ElvenBedSouthDeed()
        {
        }

        public ElvenBedSouthDeed(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }

    /// <summary>
    /// Tall Elven Bed
    /// </summary>
    public class TallElvenBedEastAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new TallElvenBedEastDeed(); } }

        [Constructable]
        public TallElvenBedEastAddon()
        {
            AddComponent(new AddonComponent(0x3054), 0, 0, 0);
            AddComponent(new AddonComponent(0x3053), 1, 0, 0);
            AddComponent(new AddonComponent(0x3055), 2, -1, 0);
            AddComponent(new AddonComponent(0x3052), 2, 0, 0);
        }

        public TallElvenBedEastAddon(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }

    public class TallElvenBedEastDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new TallElvenBedEastAddon(); } }
        public override int LabelNumber { get { return 1072859; } } // tall elven bed (east)

        [Constructable]
        public TallElvenBedEastDeed()
        {
        }

        public TallElvenBedEastDeed(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }

    public class TallElvenBedSouthAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new TallElvenBedSouthDeed(); } }

        [Constructable]
        public TallElvenBedSouthAddon()
        {
            AddComponent(new AddonComponent(0x3058), 0, 0, 0); // angolo alto sx
            AddComponent(new AddonComponent(0x3057), -1, 1, 0); // angolo basso sx
            AddComponent(new AddonComponent(0x3059), 0, -1, 0); // angolo alto dx
            AddComponent(new AddonComponent(0x3056), 0, 1, 0); // angolo basso dx
        }

        public TallElvenBedSouthAddon(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }

    public class TallElvenBedSouthDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new TallElvenBedSouthAddon(); } }
        public override int LabelNumber { get { return 1072858; } } // tall elven bed (south)

        [Constructable]
        public TallElvenBedSouthDeed()
        {
        }

        public TallElvenBedSouthDeed(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }
}