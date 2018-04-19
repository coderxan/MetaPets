using System;

using Server;
using Server.Network;

namespace Server.Items
{
    public class PeachTreeAddon : BaseFruitTreeAddon
    {
        public override BaseAddonDeed Deed { get { return new PeachTreeDeed(); } }
        public override Item Fruit { get { return new Peach(); } }

        [Constructable]
        public PeachTreeAddon()
            : base()
        {
            AddComponent(new LocalizedAddonComponent(0xD9C, 1076270), 0, 0, 0);
            AddComponent(new LocalizedAddonComponent(0x3123, 1076270), 0, 0, 0);
        }

        public PeachTreeAddon(Serial serial)
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

    public class PeachTreeDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new PeachTreeAddon(); } }
        public override int LabelNumber { get { return 1076270; } } // Peach Tree

        [Constructable]
        public PeachTreeDeed()
            : base()
        {
            LootType = LootType.Blessed;
        }

        public PeachTreeDeed(Serial serial)
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