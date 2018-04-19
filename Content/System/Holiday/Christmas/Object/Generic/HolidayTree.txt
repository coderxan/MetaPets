using System;
using System.Collections;

using Server;
using Server.Gumps;
using Server.Items;
using Server.Multis;
using Server.Network;
using Server.Targeting;

namespace Server.Items
{
    public enum HolidayTreeType
    {
        Classic,
        Modern
    }

    public class HolidayTree : Item, IAddon
    {
        private ArrayList m_Components;
        private Mobile m_Placer;

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Placer
        {
            get { return m_Placer; }
            set { m_Placer = value; }
        }

        private class Ornament : Item
        {
            public override int LabelNumber { get { return 1041118; } } // a tree ornament

            public Ornament(int itemID)
                : base(itemID)
            {
                Movable = false;
            }

            public Ornament(Serial serial)
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

        private class TreeTrunk : Item
        {
            private HolidayTree m_Tree;

            public override int LabelNumber { get { return 1041117; } } // a tree for the holidays

            public TreeTrunk(HolidayTree tree, int itemID)
                : base(itemID)
            {
                Movable = false;
                MoveToWorld(tree.Location, tree.Map);

                m_Tree = tree;
            }

            public TreeTrunk(Serial serial)
                : base(serial)
            {
            }

            public override void OnDoubleClick(Mobile from)
            {
                if (m_Tree != null && !m_Tree.Deleted)
                    m_Tree.OnDoubleClick(from);
            }

            public override void Serialize(GenericWriter writer)
            {
                base.Serialize(writer);

                writer.Write((int)0); // version

                writer.Write(m_Tree);
            }

            public override void Deserialize(GenericReader reader)
            {
                base.Deserialize(reader);

                int version = reader.ReadInt();

                switch (version)
                {
                    case 0:
                        {
                            m_Tree = reader.ReadItem() as HolidayTree;

                            if (m_Tree == null)
                                Delete();

                            break;
                        }
                }
            }
        }

        public override int LabelNumber { get { return 1041117; } } // a tree for the holidays

        public HolidayTree(Mobile from, HolidayTreeType type, Point3D loc)
            : base(1)
        {
            Movable = false;
            MoveToWorld(loc, from.Map);

            m_Placer = from;
            m_Components = new ArrayList();

            switch (type)
            {
                case HolidayTreeType.Classic:
                    {
                        ItemID = 0xCD7;

                        AddItem(0, 0, 0, new TreeTrunk(this, 0xCD6));

                        AddOrnament(0, 0, 2, 0xF22);
                        AddOrnament(0, 0, 9, 0xF18);
                        AddOrnament(0, 0, 15, 0xF20);
                        AddOrnament(0, 0, 19, 0xF17);
                        AddOrnament(0, 0, 20, 0xF24);
                        AddOrnament(0, 0, 20, 0xF1F);
                        AddOrnament(0, 0, 20, 0xF19);
                        AddOrnament(0, 0, 21, 0xF1B);
                        AddOrnament(0, 0, 28, 0xF2F);
                        AddOrnament(0, 0, 30, 0xF23);
                        AddOrnament(0, 0, 32, 0xF2A);
                        AddOrnament(0, 0, 33, 0xF30);
                        AddOrnament(0, 0, 34, 0xF29);
                        AddOrnament(0, 1, 7, 0xF16);
                        AddOrnament(0, 1, 7, 0xF1E);
                        AddOrnament(0, 1, 12, 0xF0F);
                        AddOrnament(0, 1, 13, 0xF13);
                        AddOrnament(0, 1, 18, 0xF12);
                        AddOrnament(0, 1, 19, 0xF15);
                        AddOrnament(0, 1, 25, 0xF28);
                        AddOrnament(0, 1, 29, 0xF1A);
                        AddOrnament(0, 1, 37, 0xF2B);
                        AddOrnament(1, 0, 13, 0xF10);
                        AddOrnament(1, 0, 14, 0xF1C);
                        AddOrnament(1, 0, 16, 0xF14);
                        AddOrnament(1, 0, 17, 0xF26);
                        AddOrnament(1, 0, 22, 0xF27);

                        break;
                    }
                case HolidayTreeType.Modern:
                    {
                        ItemID = 0x1B7E;

                        AddOrnament(0, 0, 2, 0xF2F);
                        AddOrnament(0, 0, 2, 0xF20);
                        AddOrnament(0, 0, 2, 0xF22);
                        AddOrnament(0, 0, 5, 0xF30);
                        AddOrnament(0, 0, 5, 0xF15);
                        AddOrnament(0, 0, 5, 0xF1F);
                        AddOrnament(0, 0, 5, 0xF2B);
                        AddOrnament(0, 0, 6, 0xF0F);
                        AddOrnament(0, 0, 7, 0xF1E);
                        AddOrnament(0, 0, 7, 0xF24);
                        AddOrnament(0, 0, 8, 0xF29);
                        AddOrnament(0, 0, 9, 0xF18);
                        AddOrnament(0, 0, 14, 0xF1C);
                        AddOrnament(0, 0, 15, 0xF13);
                        AddOrnament(0, 0, 15, 0xF20);
                        AddOrnament(0, 0, 16, 0xF26);
                        AddOrnament(0, 0, 17, 0xF12);
                        AddOrnament(0, 0, 18, 0xF17);
                        AddOrnament(0, 0, 20, 0xF1B);
                        AddOrnament(0, 0, 23, 0xF28);
                        AddOrnament(0, 0, 25, 0xF18);
                        AddOrnament(0, 0, 25, 0xF2A);
                        AddOrnament(0, 1, 7, 0xF16);

                        break;
                    }
            }
        }

        public override void OnAfterDelete()
        {
            for (int i = 0; i < m_Components.Count; ++i)
                ((Item)m_Components[i]).Delete();
        }

        private void AddOrnament(int x, int y, int z, int itemID)
        {
            AddItem(x + 1, y + 1, z + 11, new Ornament(itemID));
        }

        private void AddItem(int x, int y, int z, Item item)
        {
            item.MoveToWorld(new Point3D(this.Location.X + x, this.Location.Y + y, this.Location.Z + z), this.Map);

            m_Components.Add(item);
        }

        public HolidayTree(Serial serial)
            : base(serial)
        {
        }

        public bool CouldFit(IPoint3D p, Map map)
        {
            return map.CanFit((Point3D)p, 20);
        }

        Item IAddon.Deed
        {
            get { return new HolidayTreeDeed(); }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version

            writer.Write(m_Placer);

            writer.Write((int)m_Components.Count);

            for (int i = 0; i < m_Components.Count; ++i)
                writer.Write((Item)m_Components[i]);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                    {
                        m_Placer = reader.ReadMobile();

                        goto case 0;
                    }
                case 0:
                    {
                        int count = reader.ReadInt();

                        m_Components = new ArrayList(count);

                        for (int i = 0; i < count; ++i)
                        {
                            Item item = reader.ReadItem();

                            if (item != null)
                                m_Components.Add(item);
                        }

                        break;
                    }
            }

            Timer.DelayCall(TimeSpan.Zero, ValidatePlacement);
        }

        public void ValidatePlacement()
        {
            BaseHouse house = BaseHouse.FindHouseAt(this);

            if (house == null)
            {
                HolidayTreeDeed deed = new HolidayTreeDeed();
                deed.MoveToWorld(Location, Map);
                Delete();
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.InRange(this.GetWorldLocation(), 1))
            {
                if (m_Placer == null || from == m_Placer || from.AccessLevel >= AccessLevel.GameMaster)
                {
                    from.AddToBackpack(new HolidayTreeDeed());

                    this.Delete();

                    BaseHouse house = BaseHouse.FindHouseAt(this);

                    if (house != null && house.Addons.Contains(this))
                    {
                        house.Addons.Remove(this);
                    }

                    from.SendLocalizedMessage(503393); // A deed for the tree has been placed in your backpack.
                }
                else
                {
                    from.SendLocalizedMessage(503396); // You cannot take this tree down.
                }
            }
            else
            {
                from.SendLocalizedMessage(500446); // That is too far away.
            }
        }
    }

    public class HolidayTreeDeed : Item
    {
        public override int LabelNumber { get { return 1041116; } } // a deed for a holiday tree

        [Constructable]
        public HolidayTreeDeed()
            : base(0x14F0)
        {
            Hue = 0x488;
            Weight = 1.0;
            LootType = LootType.Blessed;
        }

        public HolidayTreeDeed(Serial serial)
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

            LootType = LootType.Blessed;
        }

        public bool ValidatePlacement(Mobile from, Point3D loc)
        {
            if (from.AccessLevel >= AccessLevel.GameMaster)
                return true;

            if (!from.InRange(this.GetWorldLocation(), 1))
            {
                from.SendLocalizedMessage(500446); // That is too far away.
                return false;
            }

            if (DateTime.UtcNow.Month != 12)
            {
                from.SendLocalizedMessage(1005700); // You will have to wait till next December to put your tree back up for display.
                return false;
            }

            Map map = from.Map;

            if (map == null)
                return false;

            BaseHouse house = BaseHouse.FindHouseAt(loc, map, 20);

            if (house == null || !house.IsFriend(from))
            {
                from.SendLocalizedMessage(1005701); // The holiday tree can only be placed in your house.
                return false;
            }

            if (!map.CanFit(loc, 20))
            {
                from.SendLocalizedMessage(500269); // You cannot build that there.
                return false;
            }

            return true;
        }

        public void BeginPlace(Mobile from, HolidayTreeType type)
        {
            from.BeginTarget(-1, true, TargetFlags.None, new TargetStateCallback(Placement_OnTarget), type);
        }

        public void Placement_OnTarget(Mobile from, object targeted, object state)
        {
            IPoint3D p = targeted as IPoint3D;

            if (p == null)
                return;

            Point3D loc = new Point3D(p);

            if (p is StaticTarget)
                loc.Z -= TileData.ItemTable[((StaticTarget)p).ItemID].CalcHeight;	/* NOTE: OSI does not properly normalize Z positioning here.
													* A side affect is that you can only place on floors (due to the CanFit call).
													* That functionality may be desired. And so, it's included in this script.
													*/

            if (ValidatePlacement(from, loc))
                EndPlace(from, (HolidayTreeType)state, loc);
        }

        public void EndPlace(Mobile from, HolidayTreeType type, Point3D loc)
        {
            this.Delete();
            HolidayTree tree = new HolidayTree(from, type, loc);
            BaseHouse house = BaseHouse.FindHouseAt(tree);
            if (house != null)
                house.Addons.Add(tree);
        }

        public override void OnDoubleClick(Mobile from)
        {
            from.CloseGump(typeof(HolidayTreeChoiceGump));
            from.SendGump(new HolidayTreeChoiceGump(from, this));
        }
    }

    public class HolidayTreeChoiceGump : Gump
    {
        private Mobile m_From;
        private HolidayTreeDeed m_Deed;

        public HolidayTreeChoiceGump(Mobile from, HolidayTreeDeed deed)
            : base(200, 200)
        {
            m_From = from;
            m_Deed = deed;

            AddPage(0);

            AddBackground(0, 0, 220, 120, 5054);
            AddBackground(10, 10, 200, 100, 3000);

            AddButton(20, 35, 4005, 4007, 1, GumpButtonType.Reply, 0);
            AddHtmlLocalized(55, 35, 145, 25, 1018322, false, false); // Classic

            AddButton(20, 65, 4005, 4007, 2, GumpButtonType.Reply, 0);
            AddHtmlLocalized(55, 65, 145, 25, 1018321, false, false); // Modern
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (m_Deed.Deleted)
                return;

            switch (info.ButtonID)
            {
                case 1:
                    {
                        m_Deed.BeginPlace(m_From, HolidayTreeType.Classic);
                        break;
                    }
                case 2:
                    {
                        m_Deed.BeginPlace(m_From, HolidayTreeType.Modern);
                        break;
                    }
            }
        }
    }
}