using System;
using System.Collections.Generic;

using Server;
using Server.Multis;
using Server.Network;

namespace Server.Items
{
    [Furniture]
    [Flipable(0xa2c, 0xa34)]
    public class Drawer : BaseContainer
    {
        [Constructable]
        public Drawer()
            : base(0xA2C)
        {
            Weight = 1.0;
        }

        public Drawer(Serial serial)
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

    [Furniture]
    [Flipable(0xa30, 0xa38)]
    public class FancyDrawer : BaseContainer
    {
        [Constructable]
        public FancyDrawer()
            : base(0xA30)
        {
            Weight = 1.0;
        }

        public FancyDrawer(Serial serial)
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

    public class ElvenDresserEastAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new ElvenDresserEastDeed(); } }

        [Constructable]
        public ElvenDresserEastAddon()
        {
            AddComponent(new AddonComponent(0x30E4), 0, 0, 0);
            AddComponent(new AddonComponent(0x30E3), 0, -1, 0);
        }

        public ElvenDresserEastAddon(Serial serial)
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

    public class ElvenDresserEastDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new ElvenDresserEastAddon(); } }
        public override int LabelNumber { get { return 1073388; } } // elven dresser (east)

        [Constructable]
        public ElvenDresserEastDeed()
        {
        }

        public ElvenDresserEastDeed(Serial serial)
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

    public class ElvenDresserSouthAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new ElvenDresserSouthDeed(); } }

        [Constructable]
        public ElvenDresserSouthAddon()
        {
            AddComponent(new AddonComponent(0x30E5), 0, 0, 0);
            AddComponent(new AddonComponent(0x30E6), 1, 0, 0);
        }

        public ElvenDresserSouthAddon(Serial serial)
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

    public class ElvenDresserSouthDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new ElvenDresserSouthAddon(); } }
        public override int LabelNumber { get { return 1072864; } } // elven dresser (south)

        [Constructable]
        public ElvenDresserSouthDeed()
        {
        }

        public ElvenDresserSouthDeed(Serial serial)
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