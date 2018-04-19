using System;

using Server;

namespace Server.Items
{
    /// <summary>
    /// Wood Coffee Tables
    /// </summary>
    [Furniture]
    public class PlainLowTable : Item
    {
        [Constructable]
        public PlainLowTable()
            : base(0x281A)
        {
            Weight = 1.0;
        }

        public PlainLowTable(Serial serial)
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

    [Furniture]
    public class ElegantLowTable : Item
    {
        [Constructable]
        public ElegantLowTable()
            : base(0x2819)
        {
            Weight = 1.0;
        }

        public ElegantLowTable(Serial serial)
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

    /// <summary>
    /// Wood Kitchen Tables
    /// </summary>
    [Furniture]
    [Flipable(0xB90, 0xB7D)]
    public class LargeTable : Item
    {
        [Constructable]
        public LargeTable()
            : base(0xB90)
        {
            Weight = 1.0;
        }

        public LargeTable(Serial serial)
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

            if (Weight == 4.0)
                Weight = 1.0;
        }
    }

    [Furniture]
    [Flipable(0xB8F, 0xB7C)]
    public class YewWoodTable : Item
    {
        [Constructable]
        public YewWoodTable()
            : base(0xB8F)
        {
            Weight = 1.0;
        }

        public YewWoodTable(Serial serial)
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

            if (Weight == 4.0)
                Weight = 1.0;
        }
    }

    /// <summary>
    /// Medium Stone Tables
    /// </summary>
    public class MediumStoneTableEastAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new MediumStoneTableEastDeed(); } }

        public override bool RetainDeedHue { get { return true; } }

        [Constructable]
        public MediumStoneTableEastAddon()
            : this(0)
        {
        }

        [Constructable]
        public MediumStoneTableEastAddon(int hue)
        {
            AddComponent(new AddonComponent(0x1202), 0, 0, 0);
            AddComponent(new AddonComponent(0x1201), 0, 1, 0);
            Hue = hue;
        }

        public MediumStoneTableEastAddon(Serial serial)
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
        }
    }

    public class MediumStoneTableEastDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new MediumStoneTableEastAddon(this.Hue); } }
        public override int LabelNumber { get { return 1044508; } } // stone table (east)

        [Constructable]
        public MediumStoneTableEastDeed()
        {
        }

        public MediumStoneTableEastDeed(Serial serial)
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

    public class MediumStoneTableSouthAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new MediumStoneTableSouthDeed(); } }

        public override bool RetainDeedHue { get { return true; } }

        [Constructable]
        public MediumStoneTableSouthAddon()
            : this(0)
        {
        }

        [Constructable]
        public MediumStoneTableSouthAddon(int hue)
        {
            AddComponent(new AddonComponent(0x1205), 0, 0, 0);
            AddComponent(new AddonComponent(0x1204), 1, 0, 0);
            Hue = hue;
        }

        public MediumStoneTableSouthAddon(Serial serial)
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
        }
    }

    public class MediumStoneTableSouthDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new MediumStoneTableSouthAddon(Hue); } }
        public override int LabelNumber { get { return 1044509; } } // stone table (South)

        [Constructable]
        public MediumStoneTableSouthDeed()
        {
        }

        public MediumStoneTableSouthDeed(Serial serial)
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
    /// Large Stone Tables
    /// </summary>
    public class LargeStoneTableEastAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new LargeStoneTableEastDeed(); } }

        public override bool RetainDeedHue { get { return true; } }

        [Constructable]
        public LargeStoneTableEastAddon()
            : this(0)
        {
        }

        [Constructable]
        public LargeStoneTableEastAddon(int hue)
        {
            AddComponent(new AddonComponent(0x1202), 0, 0, 0);
            AddComponent(new AddonComponent(0x1203), 0, 1, 0);
            AddComponent(new AddonComponent(0x1201), 0, 2, 0);
            Hue = hue;
        }

        public LargeStoneTableEastAddon(Serial serial)
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
        }
    }

    public class LargeStoneTableEastDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new LargeStoneTableEastAddon(this.Hue); } }
        public override int LabelNumber { get { return 1044511; } } // large stone table (east)

        [Constructable]
        public LargeStoneTableEastDeed()
        {
        }

        public LargeStoneTableEastDeed(Serial serial)
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

    public class LargeStoneTableSouthAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new LargeStoneTableSouthDeed(); } }

        public override bool RetainDeedHue { get { return true; } }

        [Constructable]
        public LargeStoneTableSouthAddon()
            : this(0)
        {
        }

        [Constructable]
        public LargeStoneTableSouthAddon(int hue)
        {
            AddComponent(new AddonComponent(0x1205), 0, 0, 0);
            AddComponent(new AddonComponent(0x1206), 1, 0, 0);
            AddComponent(new AddonComponent(0x1204), 2, 0, 0);
            Hue = hue;
        }

        public LargeStoneTableSouthAddon(Serial serial)
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
        }
    }

    public class LargeStoneTableSouthDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new LargeStoneTableSouthAddon(this.Hue); } }
        public override int LabelNumber { get { return 1044512; } } // large stone table (South)

        [Constructable]
        public LargeStoneTableSouthDeed()
        {
        }

        public LargeStoneTableSouthDeed(Serial serial)
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
    /// Ornate Elven Tables
    /// </summary>
    public class OrnateElvenTableEastAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new OrnateElvenTableEastDeed(); } }

        [Constructable]
        public OrnateElvenTableEastAddon()
        {
            AddComponent(new AddonComponent(0x308E), -1, 0, 0);
            AddComponent(new AddonComponent(0x308D), 0, 0, 0);
            AddComponent(new AddonComponent(0x308C), 1, 0, 0);
        }

        public OrnateElvenTableEastAddon(Serial serial)
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

    public class OrnateElvenTableEastDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new OrnateElvenTableEastAddon(); } }
        public override int LabelNumber { get { return 1073384; } } // ornate table (east)

        [Constructable]
        public OrnateElvenTableEastDeed()
        {
        }

        public OrnateElvenTableEastDeed(Serial serial)
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

    public class OrnateElvenTableSouthAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new OrnateElvenTableSouthDeed(); } }

        [Constructable]
        public OrnateElvenTableSouthAddon()
        {
            AddComponent(new AddonComponent(0x308F), 0, 1, 0);
            AddComponent(new AddonComponent(0x3090), 0, 0, 0);
            AddComponent(new AddonComponent(0x3091), 0, -1, 0);
        }

        public OrnateElvenTableSouthAddon(Serial serial)
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

    public class OrnateElvenTableSouthDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new OrnateElvenTableSouthAddon(); } }
        public override int LabelNumber { get { return 1072869; } } // ornate table (south)

        [Constructable]
        public OrnateElvenTableSouthDeed()
        {
        }

        public OrnateElvenTableSouthDeed(Serial serial)
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
    /// Fancy Elven Tables
    /// </summary>
    public class FancyElvenTableEastAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new FancyElvenTableEastDeed(); } }

        [Constructable]
        public FancyElvenTableEastAddon()
        {
            AddComponent(new AddonComponent(0x3094), -1, 0, 0);
            AddComponent(new AddonComponent(0x3093), 0, 0, 0);
            AddComponent(new AddonComponent(0x3092), 1, 0, 0);
        }

        public FancyElvenTableEastAddon(Serial serial)
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

    public class FancyElvenTableEastDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new FancyElvenTableEastAddon(); } }
        public override int LabelNumber { get { return 1073386; } } // hardwood table (east)

        [Constructable]
        public FancyElvenTableEastDeed()
        {
        }

        public FancyElvenTableEastDeed(Serial serial)
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

    public class FancyElvenTableSouthAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new FancyElvenTableSouthDeed(); } }

        [Constructable]
        public FancyElvenTableSouthAddon()
        {
            AddComponent(new AddonComponent(0x3095), 0, 1, 0);
            AddComponent(new AddonComponent(0x3096), 0, 0, 0);
            AddComponent(new AddonComponent(0x3097), 0, -1, 0);
        }

        public FancyElvenTableSouthAddon(Serial serial)
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

    public class FancyElvenTableSouthDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new FancyElvenTableSouthAddon(); } }
        public override int LabelNumber { get { return 1073385; } } // hardwood table (south)

        [Constructable]
        public FancyElvenTableSouthDeed()
        {
        }

        public FancyElvenTableSouthDeed(Serial serial)
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
    /// Alchemist Tables
    /// </summary>
    public class AlchemistTableEastAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new AlchemistTableEastDeed(); } }

        [Constructable]
        public AlchemistTableEastAddon()
        {
            AddComponent(new AddonComponent(0x2DD3), 0, 0, 0);
        }

        public AlchemistTableEastAddon(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }

    public class AlchemistTableEastDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new AlchemistTableEastAddon(); } }
        public override int LabelNumber { get { return 1073397; } } // alchemist table (east)

        [Constructable]
        public AlchemistTableEastDeed()
        {
        }

        public AlchemistTableEastDeed(Serial serial)
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

    public class AlchemistTableSouthAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new AlchemistTableSouthDeed(); } }

        [Constructable]
        public AlchemistTableSouthAddon()
        {
            AddComponent(new AddonComponent(0x2DD4), 0, 0, 0);
        }

        public AlchemistTableSouthAddon(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }

    public class AlchemistTableSouthDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new AlchemistTableSouthAddon(); } }
        public override int LabelNumber { get { return 1073396; } } // alchemist table (south)

        [Constructable]
        public AlchemistTableSouthDeed()
        {
        }

        public AlchemistTableSouthDeed(Serial serial)
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