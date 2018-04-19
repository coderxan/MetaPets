using System;

using Server;

namespace Server.Items
{
    public class StatueNorth : Item
    {
        [Constructable]
        public StatueNorth()
            : base(0x139B)
        {
            Weight = 10;
        }

        public StatueNorth(Serial serial)
            : base(serial)
        {
        }

        public override bool ForceShowProperties { get { return ObjectPropertyList.Enabled; } }

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

    public class StatueSouth : Item
    {
        [Constructable]
        public StatueSouth()
            : base(0x139A)
        {
            Weight = 10;
        }

        public StatueSouth(Serial serial)
            : base(serial)
        {
        }

        public override bool ForceShowProperties { get { return ObjectPropertyList.Enabled; } }

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

    public class StatueSouth2 : Item
    {
        [Constructable]
        public StatueSouth2()
            : base(0x1227)
        {
            Weight = 10;
        }

        public StatueSouth2(Serial serial)
            : base(serial)
        {
        }

        public override bool ForceShowProperties { get { return ObjectPropertyList.Enabled; } }

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

    public class StatueSouthEast : Item
    {
        [Constructable]
        public StatueSouthEast()
            : base(0x1225)
        {
            Weight = 10;
        }

        public StatueSouthEast(Serial serial)
            : base(serial)
        {
        }

        public override bool ForceShowProperties { get { return ObjectPropertyList.Enabled; } }

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

    public class BustSouth : Item
    {
        [Constructable]
        public BustSouth()
            : base(0x12CB)
        {
            Weight = 10;
        }

        public BustSouth(Serial serial)
            : base(serial)
        {
        }

        public override bool ForceShowProperties { get { return ObjectPropertyList.Enabled; } }

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

    public class StatueWest : Item
    {
        [Constructable]
        public StatueWest()
            : base(0x1226)
        {
            Weight = 10;
        }

        public StatueWest(Serial serial)
            : base(serial)
        {
        }

        public override bool ForceShowProperties { get { return ObjectPropertyList.Enabled; } }

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

    public class StatueEast : Item
    {
        [Constructable]
        public StatueEast()
            : base(0x139C)
        {
            Weight = 10;
        }

        public StatueEast(Serial serial)
            : base(serial)
        {
        }

        public override bool ForceShowProperties { get { return ObjectPropertyList.Enabled; } }

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

    public class StatueEast2 : Item
    {
        [Constructable]
        public StatueEast2()
            : base(0x1224)
        {
            Weight = 10;
        }

        public StatueEast2(Serial serial)
            : base(serial)
        {
        }

        public override bool ForceShowProperties { get { return ObjectPropertyList.Enabled; } }

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

    public class BustEast : Item
    {
        [Constructable]
        public BustEast()
            : base(0x12CA)
        {
            Weight = 10;
        }

        public BustEast(Serial serial)
            : base(serial)
        {
        }

        public override bool ForceShowProperties { get { return ObjectPropertyList.Enabled; } }

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

    public class SmallTowerSculpture : Item
    {
        [Constructable]
        public SmallTowerSculpture()
            : base(0x241A)
        {
            Weight = 20.0;
        }

        public SmallTowerSculpture(Serial serial)
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

    public class StatuePegasus : Item
    {
        [Constructable]
        public StatuePegasus()
            : base(0x139D)
        {
            Weight = 10;
        }

        public StatuePegasus(Serial serial)
            : base(serial)
        {
        }

        public override bool ForceShowProperties { get { return ObjectPropertyList.Enabled; } }

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

    public class StatuePegasus2 : Item
    {
        [Constructable]
        public StatuePegasus2()
            : base(0x1228)
        {
            Weight = 10;
        }

        public StatuePegasus2(Serial serial)
            : base(serial)
        {
        }

        public override bool ForceShowProperties { get { return ObjectPropertyList.Enabled; } }

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

    public class ArcanistStatueEastAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new ArcanistStatueEastDeed(); } }

        [Constructable]
        public ArcanistStatueEastAddon()
        {
            AddComponent(new AddonComponent(0x2D0E), 0, 0, 0);
        }

        public ArcanistStatueEastAddon(Serial serial)
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

    public class ArcanistStatueEastDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new ArcanistStatueEastAddon(); } }
        public override int LabelNumber { get { return 1072886; } } // arcanist statue (east)

        [Constructable]
        public ArcanistStatueEastDeed()
        {
        }

        public ArcanistStatueEastDeed(Serial serial)
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

    public class ArcanistStatueSouthAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new ArcanistStatueSouthDeed(); } }

        [Constructable]
        public ArcanistStatueSouthAddon()
        {
            AddComponent(new AddonComponent(0x2D0F), 0, 0, 0);
        }

        public ArcanistStatueSouthAddon(Serial serial)
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

    public class ArcanistStatueSouthDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new ArcanistStatueSouthAddon(); } }
        public override int LabelNumber { get { return 1072885; } } // arcanist statue (south)

        [Constructable]
        public ArcanistStatueSouthDeed()
        {
        }

        public ArcanistStatueSouthDeed(Serial serial)
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

    public class WarriorStatueEastAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new WarriorStatueEastDeed(); } }

        [Constructable]
        public WarriorStatueEastAddon()
        {
            AddComponent(new AddonComponent(0x2D12), 0, 0, 0);
        }

        public WarriorStatueEastAddon(Serial serial)
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

    public class WarriorStatueEastDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new WarriorStatueEastAddon(); } }
        public override int LabelNumber { get { return 1072888; } } // warrior statue (east)

        [Constructable]
        public WarriorStatueEastDeed()
        {
        }

        public WarriorStatueEastDeed(Serial serial)
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

    public class WarriorStatueSouthAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new WarriorStatueSouthDeed(); } }

        [Constructable]
        public WarriorStatueSouthAddon()
        {
            AddComponent(new AddonComponent(0x2D13), 0, 0, 0);
        }

        public WarriorStatueSouthAddon(Serial serial)
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

    public class WarriorStatueSouthDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new WarriorStatueSouthAddon(); } }
        public override int LabelNumber { get { return 1072887; } } // warrior statue (south)

        [Constructable]
        public WarriorStatueSouthDeed()
        {
        }

        public WarriorStatueSouthDeed(Serial serial)
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

    public class SquirrelStatueEastAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new SquirrelStatueEastDeed(); } }

        [Constructable]
        public SquirrelStatueEastAddon()
        {
            AddComponent(new AddonComponent(0x2D10), 0, 0, 0);
        }

        public SquirrelStatueEastAddon(Serial serial)
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

    public class SquirrelStatueEastDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new SquirrelStatueEastAddon(); } }
        public override int LabelNumber { get { return 1073398; } } // squirrel statue (east)

        [Constructable]
        public SquirrelStatueEastDeed()
        {
        }

        public SquirrelStatueEastDeed(Serial serial)
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

    public class SquirrelStatueSouthAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new SquirrelStatueSouthDeed(); } }

        [Constructable]
        public SquirrelStatueSouthAddon()
        {
            AddComponent(new AddonComponent(0x2D11), 0, 0, 0);
        }

        public SquirrelStatueSouthAddon(Serial serial)
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

    public class SquirrelStatueSouthDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new SquirrelStatueSouthAddon(); } }
        public override int LabelNumber { get { return 1072884; } } // squirrel statue (south)

        [Constructable]
        public SquirrelStatueSouthDeed()
        {
        }

        public SquirrelStatueSouthDeed(Serial serial)
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