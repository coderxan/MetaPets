using System;

using Server;

namespace Server.Items
{
    /// <summary>
    /// Gray Brick Fireplace
    /// </summary>
    public class GrayBrickFireplaceEastAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new GrayBrickFireplaceEastDeed(); } }

        [Constructable]
        public GrayBrickFireplaceEastAddon()
        {
            AddComponent(new AddonComponent(0x93D), 0, 0, 0);
            AddComponent(new AddonComponent(0x937), 0, 1, 0);
        }

        public GrayBrickFireplaceEastAddon(Serial serial)
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

    public class GrayBrickFireplaceEastDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new GrayBrickFireplaceEastAddon(); } }
        public override int LabelNumber { get { return 1061846; } } // grey brick fireplace (east)

        [Constructable]
        public GrayBrickFireplaceEastDeed()
        {
        }

        public GrayBrickFireplaceEastDeed(Serial serial)
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

    public class GrayBrickFireplaceSouthAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new GrayBrickFireplaceSouthDeed(); } }

        [Constructable]
        public GrayBrickFireplaceSouthAddon()
        {
            AddComponent(new AddonComponent(0x94B), -1, 0, 0);
            AddComponent(new AddonComponent(0x945), 0, 0, 0);
        }

        public GrayBrickFireplaceSouthAddon(Serial serial)
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

    public class GrayBrickFireplaceSouthDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new GrayBrickFireplaceSouthAddon(); } }
        public override int LabelNumber { get { return 1061847; } } // grey brick fireplace (south)

        [Constructable]
        public GrayBrickFireplaceSouthDeed()
        {
        }

        public GrayBrickFireplaceSouthDeed(Serial serial)
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
    /// Sandstone Fireplace
    /// </summary>
    public class SandstoneFireplaceEastAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new SandstoneFireplaceEastDeed(); } }

        [Constructable]
        public SandstoneFireplaceEastAddon()
        {
            AddComponent(new AddonComponent(0x489), 0, 0, 0);
            AddComponent(new AddonComponent(0x475), 0, 1, 0);
        }

        public SandstoneFireplaceEastAddon(Serial serial)
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

    public class SandstoneFireplaceEastDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new SandstoneFireplaceEastAddon(); } }
        public override int LabelNumber { get { return 1061844; } } // sandstone fireplace (east)

        [Constructable]
        public SandstoneFireplaceEastDeed()
        {
        }

        public SandstoneFireplaceEastDeed(Serial serial)
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

    public class SandstoneFireplaceSouthAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new SandstoneFireplaceSouthDeed(); } }

        [Constructable]
        public SandstoneFireplaceSouthAddon()
        {
            AddComponent(new AddonComponent(0x482), -1, 0, 0);
            AddComponent(new AddonComponent(0x47B), 0, 0, 0);
        }

        public SandstoneFireplaceSouthAddon(Serial serial)
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

    public class SandstoneFireplaceSouthDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new SandstoneFireplaceSouthAddon(); } }
        public override int LabelNumber { get { return 1061845; } } // sandstone fireplace (south)

        [Constructable]
        public SandstoneFireplaceSouthDeed()
        {
        }

        public SandstoneFireplaceSouthDeed(Serial serial)
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
    /// Stone Fireplace
    /// </summary>
    public class StoneFireplaceEastAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new StoneFireplaceEastDeed(); } }

        [Constructable]
        public StoneFireplaceEastAddon()
        {
            AddComponent(new AddonComponent(0x959), 0, 0, 0);
            AddComponent(new AddonComponent(0x953), 0, 1, 0);
        }

        public StoneFireplaceEastAddon(Serial serial)
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

    public class StoneFireplaceEastDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new StoneFireplaceEastAddon(); } }
        public override int LabelNumber { get { return 1061848; } } // stone fireplace (east)

        [Constructable]
        public StoneFireplaceEastDeed()
        {
        }

        public StoneFireplaceEastDeed(Serial serial)
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

    public class StoneFireplaceSouthAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new StoneFireplaceSouthDeed(); } }

        [Constructable]
        public StoneFireplaceSouthAddon()
        {
            AddComponent(new AddonComponent(0x967), -1, 0, 0);
            AddComponent(new AddonComponent(0x961), 0, 0, 0);
        }

        public StoneFireplaceSouthAddon(Serial serial)
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

    public class StoneFireplaceSouthDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new StoneFireplaceSouthAddon(); } }
        public override int LabelNumber { get { return 1061849; } } // stone fireplace (south)

        [Constructable]
        public StoneFireplaceSouthDeed()
        {
        }

        public StoneFireplaceSouthDeed(Serial serial)
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