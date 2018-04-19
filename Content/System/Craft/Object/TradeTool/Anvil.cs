using System;

using Server;

namespace Server.Items
{
    [FlipableAttribute(0xFAF, 0xFB0)]
    [Server.Engines.Craft.Anvil]
    public class Anvil : Item
    {
        [Constructable]
        public Anvil()
            : base(0xFAF)
        {
            Movable = false;
        }

        public Anvil(Serial serial)
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

    [Server.Engines.Craft.Anvil]
    public class AnvilComponent : AddonComponent
    {
        [Constructable]
        public AnvilComponent(int itemID)
            : base(itemID)
        {
        }

        public AnvilComponent(Serial serial)
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

    public class AnvilEastAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new AnvilEastDeed(); } }

        [Constructable]
        public AnvilEastAddon()
        {
            AddComponent(new AnvilComponent(0xFAF), 0, 0, 0);
        }

        public AnvilEastAddon(Serial serial)
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

    public class AnvilEastDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new AnvilEastAddon(); } }
        public override int LabelNumber { get { return 1044333; } } // anvil (east)

        [Constructable]
        public AnvilEastDeed()
        {
        }

        public AnvilEastDeed(Serial serial)
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

    public class AnvilSouthAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new AnvilSouthDeed(); } }

        [Constructable]
        public AnvilSouthAddon()
        {
            AddComponent(new AnvilComponent(0xFB0), 0, 0, 0);
        }

        public AnvilSouthAddon(Serial serial)
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

    public class AnvilSouthDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new AnvilSouthAddon(); } }
        public override int LabelNumber { get { return 1044334; } } // anvil (south)

        [Constructable]
        public AnvilSouthDeed()
        {
        }

        public AnvilSouthDeed(Serial serial)
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