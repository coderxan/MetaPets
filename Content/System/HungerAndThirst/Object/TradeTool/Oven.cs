using System;

using Server;

namespace Server.Items
{
    public class StoneOvenEastAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new StoneOvenEastDeed(); } }

        [Constructable]
        public StoneOvenEastAddon()
        {
            AddComponent(new AddonComponent(0x92C), 0, 0, 0);
            AddComponent(new AddonComponent(0x92B), 0, 1, 0);
        }

        public StoneOvenEastAddon(Serial serial)
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

    public class StoneOvenEastDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new StoneOvenEastAddon(); } }
        public override int LabelNumber { get { return 1044345; } } // stone oven (east)

        [Constructable]
        public StoneOvenEastDeed()
        {
        }

        public StoneOvenEastDeed(Serial serial)
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

    public class StoneOvenSouthAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new StoneOvenSouthDeed(); } }

        [Constructable]
        public StoneOvenSouthAddon()
        {
            AddComponent(new AddonComponent(0x931), -1, 0, 0);
            AddComponent(new AddonComponent(0x930), 0, 0, 0);
        }

        public StoneOvenSouthAddon(Serial serial)
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

    public class StoneOvenSouthDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new StoneOvenSouthAddon(); } }
        public override int LabelNumber { get { return 1044346; } } // stone oven (south)

        [Constructable]
        public StoneOvenSouthDeed()
        {
        }

        public StoneOvenSouthDeed(Serial serial)
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

    public class ElvenStoveEastAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new ElvenStoveEastDeed(); } }

        [Constructable]
        public ElvenStoveEastAddon()
        {
            AddComponent(new AddonComponent(0x2DDB), 0, 0, 0);
        }

        public ElvenStoveEastAddon(Serial serial)
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

    public class ElvenStoveEastDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new ElvenStoveEastAddon(); } }
        public override int LabelNumber { get { return 1073395; } } // elven oven (east)

        [Constructable]
        public ElvenStoveEastDeed()
        {
        }

        public ElvenStoveEastDeed(Serial serial)
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

    public class ElvenStoveSouthAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new ElvenStoveSouthDeed(); } }

        [Constructable]
        public ElvenStoveSouthAddon()
        {
            AddComponent(new AddonComponent(0x2DDC), 0, 0, 0);
        }

        public ElvenStoveSouthAddon(Serial serial)
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

    public class ElvenStoveSouthDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new ElvenStoveSouthAddon(); } }
        public override int LabelNumber { get { return 1073394; } } // elven oven (south)

        [Constructable]
        public ElvenStoveSouthDeed()
        {
        }

        public ElvenStoveSouthDeed(Serial serial)
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