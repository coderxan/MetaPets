using System;

using Server;

namespace Server.Items
{
    [Server.Engines.Craft.Forge]
    public class Forge : Item
    {
        [Constructable]
        public Forge()
            : base(0xFB1)
        {
            Movable = false;
        }

        public Forge(Serial serial)
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

    [Server.Engines.Craft.Forge]
    public class LargeForgeWest : Item
    {
        private InternalItem m_Item;
        private InternalItem2 m_Item2;

        [Constructable]
        public LargeForgeWest()
            : base(0x199A)
        {
            Movable = false;

            m_Item = new InternalItem(this);
            m_Item2 = new InternalItem2(this);
        }

        public LargeForgeWest(Serial serial)
            : base(serial)
        {
        }

        public override void OnLocationChange(Point3D oldLocation)
        {
            if (m_Item != null)
                m_Item.Location = new Point3D(X, Y + 1, Z);
            if (m_Item2 != null)
                m_Item2.Location = new Point3D(X, Y + 2, Z);
        }

        public override void OnMapChange()
        {
            if (m_Item != null)
                m_Item.Map = Map;
            if (m_Item2 != null)
                m_Item2.Map = Map;
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            if (m_Item != null)
                m_Item.Delete();
            if (m_Item2 != null)
                m_Item2.Delete();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write(m_Item);
            writer.Write(m_Item2);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            m_Item = reader.ReadItem() as InternalItem;
            m_Item2 = reader.ReadItem() as InternalItem2;
        }

        [Server.Engines.Craft.Forge]
        private class InternalItem : Item
        {
            private LargeForgeWest m_Item;

            public InternalItem(LargeForgeWest item)
                : base(0x1996)
            {
                Movable = false;

                m_Item = item;
            }

            public InternalItem(Serial serial)
                : base(serial)
            {
            }

            public override void OnLocationChange(Point3D oldLocation)
            {
                if (m_Item != null)
                    m_Item.Location = new Point3D(X, Y - 1, Z);
            }

            public override void OnMapChange()
            {
                if (m_Item != null)
                    m_Item.Map = Map;
            }

            public override void OnAfterDelete()
            {
                base.OnAfterDelete();

                if (m_Item != null)
                    m_Item.Delete();
            }

            public override void Serialize(GenericWriter writer)
            {
                base.Serialize(writer);

                writer.Write((int)0); // version

                writer.Write(m_Item);
            }

            public override void Deserialize(GenericReader reader)
            {
                base.Deserialize(reader);

                int version = reader.ReadInt();

                m_Item = reader.ReadItem() as LargeForgeWest;
            }
        }

        [Server.Engines.Craft.Forge]
        private class InternalItem2 : Item
        {
            private LargeForgeWest m_Item;

            public InternalItem2(LargeForgeWest item)
                : base(0x1992)
            {
                Movable = false;

                m_Item = item;
            }

            public InternalItem2(Serial serial)
                : base(serial)
            {
            }

            public override void OnLocationChange(Point3D oldLocation)
            {
                if (m_Item != null)
                    m_Item.Location = new Point3D(X, Y - 2, Z);
            }

            public override void OnMapChange()
            {
                if (m_Item != null)
                    m_Item.Map = Map;
            }

            public override void OnAfterDelete()
            {
                base.OnAfterDelete();

                if (m_Item != null)
                    m_Item.Delete();
            }

            public override void Serialize(GenericWriter writer)
            {
                base.Serialize(writer);

                writer.Write((int)0); // version

                writer.Write(m_Item);
            }

            public override void Deserialize(GenericReader reader)
            {
                base.Deserialize(reader);

                int version = reader.ReadInt();

                m_Item = reader.ReadItem() as LargeForgeWest;
            }
        }
    }

    [Server.Engines.Craft.Forge]
    public class LargeForgeEast : Item
    {
        private InternalItem m_Item;
        private InternalItem2 m_Item2;

        [Constructable]
        public LargeForgeEast()
            : base(0x197A)
        {
            Movable = false;

            m_Item = new InternalItem(this);
            m_Item2 = new InternalItem2(this);
        }

        public LargeForgeEast(Serial serial)
            : base(serial)
        {
        }

        public override void OnLocationChange(Point3D oldLocation)
        {
            if (m_Item != null)
                m_Item.Location = new Point3D(X + 1, Y, Z);
            if (m_Item2 != null)
                m_Item2.Location = new Point3D(X + 2, Y, Z);
        }

        public override void OnMapChange()
        {
            if (m_Item != null)
                m_Item.Map = Map;
            if (m_Item2 != null)
                m_Item2.Map = Map;
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            if (m_Item != null)
                m_Item.Delete();
            if (m_Item2 != null)
                m_Item2.Delete();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write(m_Item);
            writer.Write(m_Item2);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            m_Item = reader.ReadItem() as InternalItem;
            m_Item2 = reader.ReadItem() as InternalItem2;
        }

        [Server.Engines.Craft.Forge]
        private class InternalItem : Item
        {
            private LargeForgeEast m_Item;

            public InternalItem(LargeForgeEast item)
                : base(0x197E)
            {
                Movable = false;

                m_Item = item;
            }

            public InternalItem(Serial serial)
                : base(serial)
            {
            }

            public override void OnLocationChange(Point3D oldLocation)
            {
                if (m_Item != null)
                    m_Item.Location = new Point3D(X - 1, Y, Z);
            }

            public override void OnMapChange()
            {
                if (m_Item != null)
                    m_Item.Map = Map;
            }

            public override void OnAfterDelete()
            {
                base.OnAfterDelete();

                if (m_Item != null)
                    m_Item.Delete();
            }

            public override void Serialize(GenericWriter writer)
            {
                base.Serialize(writer);

                writer.Write((int)0); // version

                writer.Write(m_Item);
            }

            public override void Deserialize(GenericReader reader)
            {
                base.Deserialize(reader);

                int version = reader.ReadInt();

                m_Item = reader.ReadItem() as LargeForgeEast;
            }
        }

        [Server.Engines.Craft.Forge]
        private class InternalItem2 : Item
        {
            private LargeForgeEast m_Item;

            public InternalItem2(LargeForgeEast item)
                : base(0x1982)
            {
                Movable = false;

                m_Item = item;
            }

            public InternalItem2(Serial serial)
                : base(serial)
            {
            }

            public override void OnLocationChange(Point3D oldLocation)
            {
                if (m_Item != null)
                    m_Item.Location = new Point3D(X - 2, Y, Z);
            }

            public override void OnMapChange()
            {
                if (m_Item != null)
                    m_Item.Map = Map;
            }

            public override void OnAfterDelete()
            {
                base.OnAfterDelete();

                if (m_Item != null)
                    m_Item.Delete();
            }

            public override void Serialize(GenericWriter writer)
            {
                base.Serialize(writer);

                writer.Write((int)0); // version

                writer.Write(m_Item);
            }

            public override void Deserialize(GenericReader reader)
            {
                base.Deserialize(reader);

                int version = reader.ReadInt();

                m_Item = reader.ReadItem() as LargeForgeEast;
            }
        }
    }

    [Server.Engines.Craft.Forge]
    public class ForgeComponent : AddonComponent
    {
        [Constructable]
        public ForgeComponent(int itemID)
            : base(itemID)
        {
        }

        public ForgeComponent(Serial serial)
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

    public class SmallForgeAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new SmallForgeDeed(); } }

        [Constructable]
        public SmallForgeAddon()
        {
            AddComponent(new ForgeComponent(0xFB1), 0, 0, 0);
        }

        public SmallForgeAddon(Serial serial)
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

    public class SmallForgeDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new SmallForgeAddon(); } }
        public override int LabelNumber { get { return 1044330; } } // small forge

        [Constructable]
        public SmallForgeDeed()
        {
        }

        public SmallForgeDeed(Serial serial)
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

    public class LargeForgeEastAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new LargeForgeEastDeed(); } }

        [Constructable]
        public LargeForgeEastAddon()
        {
            AddComponent(new ForgeComponent(0x1986), 0, 0, 0);
            AddComponent(new ForgeComponent(0x198A), 0, 1, 0);
            AddComponent(new ForgeComponent(0x1996), 0, 2, 0);
            AddComponent(new ForgeComponent(0x1992), 0, 3, 0);
        }

        public LargeForgeEastAddon(Serial serial)
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

    public class LargeForgeEastDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new LargeForgeEastAddon(); } }
        public override int LabelNumber { get { return 1044331; } } // large forge (east)

        [Constructable]
        public LargeForgeEastDeed()
        {
        }

        public LargeForgeEastDeed(Serial serial)
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

    public class LargeForgeSouthAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new LargeForgeSouthDeed(); } }

        [Constructable]
        public LargeForgeSouthAddon()
        {
            AddComponent(new ForgeComponent(0x197A), 0, 0, 0);
            AddComponent(new ForgeComponent(0x197E), 1, 0, 0);
            AddComponent(new ForgeComponent(0x19A2), 2, 0, 0);
            AddComponent(new ForgeComponent(0x199E), 3, 0, 0);
        }

        public LargeForgeSouthAddon(Serial serial)
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

    public class LargeForgeSouthDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new LargeForgeSouthAddon(); } }
        public override int LabelNumber { get { return 1044332; } } // large forge (south)

        [Constructable]
        public LargeForgeSouthDeed()
        {
        }

        public LargeForgeSouthDeed(Serial serial)
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

    public class ElvenForgeAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new ElvenForgeDeed(); } }

        [Constructable]
        public ElvenForgeAddon()
        {
            AddComponent(new AddonComponent(0x2DD8), 0, 0, 0);
        }

        public ElvenForgeAddon(Serial serial)
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

    public class ElvenForgeDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new ElvenForgeAddon(); } }
        public override int LabelNumber { get { return 1072875; } } // squirrel statue (east)

        [Constructable]
        public ElvenForgeDeed()
        {
        }

        public ElvenForgeDeed(Serial serial)
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