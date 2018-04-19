using System;

using Server;
using Server.Network;

namespace Server.Items
{
    public class AppleTreeAddon : BaseFruitTreeAddon
    {
        public override BaseAddonDeed Deed { get { return new AppleTreeDeed(); } }
        public override Item Fruit { get { return new Apple(); } }

        [Constructable]
        public AppleTreeAddon()
            : base()
        {
            AddComponent(new LocalizedAddonComponent(0xD98, 1076269), 0, 0, 0);
            AddComponent(new LocalizedAddonComponent(0x3124, 1076269), 0, 0, 0);
        }

        public AppleTreeAddon(Serial serial)
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

    public class AppleTreeDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new AppleTreeAddon(); } }
        public override int LabelNumber { get { return 1076269; } } // Apple Tree

        [Constructable]
        public AppleTreeDeed()
            : base()
        {
            LootType = LootType.Blessed;
        }

        public AppleTreeDeed(Serial serial)
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