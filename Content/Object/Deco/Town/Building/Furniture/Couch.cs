using System;

using Server;

namespace Server.Items
{
    public class ElvenLoveseatEastAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new ElvenLoveseatEastDeed(); } }

        [Constructable]
        public ElvenLoveseatEastAddon()
        {
            AddComponent(new AddonComponent(0x3089), 0, 0, 0);
            AddComponent(new AddonComponent(0x3088), 1, 0, 0);
        }

        public ElvenLoveseatEastAddon(Serial serial)
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

    public class ElvenLoveseatEastDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new ElvenLoveseatEastAddon(); } }
        public override int LabelNumber { get { return 1073372; } } // elven loveseat (east)

        [Constructable]
        public ElvenLoveseatEastDeed()
        {
        }

        public ElvenLoveseatEastDeed(Serial serial)
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

    public class ElvenLoveseatSouthAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new ElvenLoveseatSouthDeed(); } }

        [Constructable]
        public ElvenLoveseatSouthAddon()
        {
            AddComponent(new AddonComponent(0x308A), 0, 0, 0);
            AddComponent(new AddonComponent(0x308B), 0, -1, 0);
        }

        public ElvenLoveseatSouthAddon(Serial serial)
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

    public class ElvenLoveseatSouthDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new ElvenLoveseatSouthAddon(); } }
        public override int LabelNumber { get { return 1072867; } } // elven loveseat (south)

        [Constructable]
        public ElvenLoveseatSouthDeed()
        {
        }

        public ElvenLoveseatSouthDeed(Serial serial)
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