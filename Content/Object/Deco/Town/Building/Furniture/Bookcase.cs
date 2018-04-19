using System;
using System.Collections;
using System.Collections.Generic;

using Server;
using Server.Mobiles;
using Server.Multis;
using Server.Network;

namespace Server.Items
{
    [Furniture]
    [Flipable(0xa9d, 0xa9e)]
    public class EmptyBookcase : BaseContainer
    {
        [Constructable]
        public EmptyBookcase()
            : base(0xA9D)
        {
        }

        public EmptyBookcase(Serial serial)
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

            if (version == 0 && Weight == 1.0)
                Weight = -1;
        }
    }

    [Furniture]
    [Flipable(0xa97, 0xa99, 0xa98, 0xa9a, 0xa9b, 0xa9c)]
    public class FullBookcase : BaseContainer
    {
        [Constructable]
        public FullBookcase()
            : base(0xA97)
        {
            Weight = 1.0;
        }

        public FullBookcase(Serial serial)
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

    public class ArcaneBookshelfEastAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new ArcaneBookshelfEastDeed(); } }

        [Constructable]
        public ArcaneBookshelfEastAddon()
        {
            AddComponent(new AddonComponent(0x3084), 0, 0, 0);
            AddComponent(new AddonComponent(0x3085), -1, 0, 0);
        }

        public ArcaneBookshelfEastAddon(Serial serial)
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

    public class ArcaneBookshelfEastDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new ArcaneBookshelfEastAddon(); } }
        public override int LabelNumber { get { return 1073371; } } // arcane bookshelf (east)

        [Constructable]
        public ArcaneBookshelfEastDeed()
        {
        }

        public ArcaneBookshelfEastDeed(Serial serial)
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

    public class ArcaneBookshelfSouthAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new ArcaneBookshelfSouthDeed(); } }

        [Constructable]
        public ArcaneBookshelfSouthAddon()
        {
            AddComponent(new AddonComponent(0x3087), 0, 0, 0);
            AddComponent(new AddonComponent(0x3086), 0, 1, 0);
        }

        public ArcaneBookshelfSouthAddon(Serial serial)
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

    public class ArcaneBookshelfSouthDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new ArcaneBookshelfSouthAddon(); } }
        public override int LabelNumber { get { return 1072871; } } // arcane bookshelf (south)

        [Constructable]
        public ArcaneBookshelfSouthDeed()
        {
        }

        public ArcaneBookshelfSouthDeed(Serial serial)
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

    [Flipable(0xA97, 0xA99, 0xA98, 0xA9A, 0xA9B, 0xA9C)]
    public class LibraryBookcase : FillableContainer
    {
        public override bool IsLockable { get { return false; } }
        public override int SpawnThreshold { get { return 5; } }

        protected override int GetSpawnCount()
        {
            return (5 - GetItemsCount());
        }

        public override void AcquireContent()
        {
            if (m_Content != null)
                return;

            m_Content = FillableContent.Library;

            if (m_Content != null)
                Respawn();
        }

        [Constructable]
        public LibraryBookcase()
            : base(0xA97)
        {
            Weight = 1.0;
        }

        public LibraryBookcase(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();

            if (version == 0 && m_Content == null)
                Timer.DelayCall(TimeSpan.Zero, new TimerCallback(AcquireContent));
        }
    }
}