using System;
using System.Collections;
using System.Collections.Generic;

using Server;
using Server.ContextMenus;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Gumps
{
    public class TithingGump : Gump
    {
        private Mobile m_From;
        private int m_Offer;

        public TithingGump(Mobile from, int offer)
            : base(160, 40)
        {
            int totalGold = from.TotalGold;

            if (offer > totalGold)
                offer = totalGold;
            else if (offer < 0)
                offer = 0;

            m_From = from;
            m_Offer = offer;

            AddPage(0);

            AddImage(30, 30, 102);

            AddHtmlLocalized(95, 100, 120, 100, 1060198, 0, false, false); // May your wealth bring blessings to those in need, if tithed upon this most sacred site.

            AddLabel(57, 274, 0, "Gold:");
            AddLabel(87, 274, 53, (totalGold - offer).ToString());

            AddLabel(137, 274, 0, "Tithe:");
            AddLabel(172, 274, 53, offer.ToString());

            AddButton(105, 230, 5220, 5220, 2, GumpButtonType.Reply, 0);
            AddButton(113, 230, 5222, 5222, 2, GumpButtonType.Reply, 0);
            AddLabel(108, 228, 0, "<");
            AddLabel(112, 228, 0, "<");

            AddButton(127, 230, 5223, 5223, 1, GumpButtonType.Reply, 0);
            AddLabel(131, 228, 0, "<");

            AddButton(147, 230, 5224, 5224, 3, GumpButtonType.Reply, 0);
            AddLabel(153, 228, 0, ">");

            AddButton(168, 230, 5220, 5220, 4, GumpButtonType.Reply, 0);
            AddButton(176, 230, 5222, 5222, 4, GumpButtonType.Reply, 0);
            AddLabel(172, 228, 0, ">");
            AddLabel(176, 228, 0, ">");

            AddButton(217, 272, 4023, 4024, 5, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            switch (info.ButtonID)
            {
                case 0:
                    {
                        // You have decided to tithe no gold to the shrine.
                        m_From.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1060193);
                        break;
                    }
                case 1:
                case 2:
                case 3:
                case 4:
                    {
                        int offer = 0;

                        switch (info.ButtonID)
                        {
                            case 1: offer = m_Offer - 100; break;
                            case 2: offer = 0; break;
                            case 3: offer = m_Offer + 100; break;
                            case 4: offer = m_From.TotalGold; break;
                        }

                        m_From.SendGump(new TithingGump(m_From, offer));
                        break;
                    }
                case 5:
                    {
                        int totalGold = m_From.TotalGold;

                        if (m_Offer > totalGold)
                            m_Offer = totalGold;
                        else if (m_Offer < 0)
                            m_Offer = 0;

                        if ((m_From.TithingPoints + m_Offer) > 100000) // TODO: What's the maximum?
                            m_Offer = (100000 - m_From.TithingPoints);

                        if (m_Offer <= 0)
                        {
                            // You have decided to tithe no gold to the shrine.
                            m_From.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1060193);
                            break;
                        }

                        Container pack = m_From.Backpack;

                        if (pack != null && pack.ConsumeTotal(typeof(Gold), m_Offer))
                        {
                            // You tithe gold to the shrine as a sign of devotion.
                            m_From.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1060195);
                            m_From.TithingPoints += m_Offer;

                            m_From.PlaySound(0x243);
                            m_From.PlaySound(0x2E6);
                        }
                        else
                        {
                            // You do not have enough gold to tithe that amount!
                            m_From.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1060194);
                        }

                        break;
                    }
            }
        }
    }
}

namespace Server.Items
{
    public class Ankhs
    {
        public const int ResurrectRange = 2;
        public const int TitheRange = 2;
        public const int LockRange = 2;

        public static void GetContextMenuEntries(Mobile from, Item item, List<ContextMenuEntry> list)
        {
            if (from is PlayerMobile)
                list.Add(new LockKarmaEntry((PlayerMobile)from));

            list.Add(new ResurrectEntry(from, item));

            if (Core.AOS)
                list.Add(new TitheEntry(from));
        }

        public static void Resurrect(Mobile m, Item item)
        {
            if (m.Alive)
                return;

            if (!m.InRange(item.GetWorldLocation(), ResurrectRange))
                m.SendLocalizedMessage(500446); // That is too far away.
            else if (m.Map != null && m.Map.CanFit(m.Location, 16, false, false))
            {
                m.CloseGump(typeof(ResurrectGump));
                m.SendGump(new ResurrectGump(m, ResurrectMessage.VirtueShrine));
            }
            else
                m.SendLocalizedMessage(502391); // Thou can not be resurrected there!
        }

        private class ResurrectEntry : ContextMenuEntry
        {
            private Mobile m_Mobile;
            private Item m_Item;

            public ResurrectEntry(Mobile mobile, Item item)
                : base(6195, ResurrectRange)
            {
                m_Mobile = mobile;
                m_Item = item;

                Enabled = !m_Mobile.Alive;
            }

            public override void OnClick()
            {
                Resurrect(m_Mobile, m_Item);
            }
        }

        private class LockKarmaEntry : ContextMenuEntry
        {
            private PlayerMobile m_Mobile;

            public LockKarmaEntry(PlayerMobile mobile)
                : base(mobile.KarmaLocked ? 6197 : 6196, LockRange)
            {
                m_Mobile = mobile;
            }

            public override void OnClick()
            {
                m_Mobile.KarmaLocked = !m_Mobile.KarmaLocked;

                if (m_Mobile.KarmaLocked)
                    m_Mobile.SendLocalizedMessage(1060192); // Your karma has been locked. Your karma can no longer be raised.
                else
                    m_Mobile.SendLocalizedMessage(1060191); // Your karma has been unlocked. Your karma can be raised again.
            }
        }

        private class TitheEntry : ContextMenuEntry
        {
            private Mobile m_Mobile;

            public TitheEntry(Mobile mobile)
                : base(6198, TitheRange)
            {
                m_Mobile = mobile;

                Enabled = m_Mobile.Alive;
            }

            public override void OnClick()
            {
                if (m_Mobile.CheckAlive())
                    m_Mobile.SendGump(new TithingGump(m_Mobile, 0));
            }
        }
    }

    public class AnkhWest : Item
    {
        private InternalItem m_Item;

        [Constructable]
        public AnkhWest()
            : this(false)
        {
        }

        [Constructable]
        public AnkhWest(bool bloodied)
            : base(bloodied ? 0x1D98 : 0x3)
        {
            Movable = false;

            m_Item = new InternalItem(bloodied, this);
        }

        public AnkhWest(Serial serial)
            : base(serial)
        {
        }

        public override bool HandlesOnMovement { get { return true; } } // Tell the core that we implement OnMovement

        public override void OnMovement(Mobile m, Point3D oldLocation)
        {
            if (Parent == null && Utility.InRange(Location, m.Location, 1) && !Utility.InRange(Location, oldLocation, 1))
                Ankhs.Resurrect(m, this);
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);
            Ankhs.GetContextMenuEntries(from, this, list);
        }

        [Hue, CommandProperty(AccessLevel.GameMaster)]
        public override int Hue
        {
            get { return base.Hue; }
            set { base.Hue = value; if (m_Item.Hue != value) m_Item.Hue = value; }
        }

        public override void OnDoubleClickDead(Mobile m)
        {
            Ankhs.Resurrect(m, this);
        }

        public override void OnLocationChange(Point3D oldLocation)
        {
            if (m_Item != null)
                m_Item.Location = new Point3D(X, Y + 1, Z);
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

            m_Item = reader.ReadItem() as InternalItem;
        }

        private class InternalItem : Item
        {
            private AnkhWest m_Item;

            public InternalItem(bool bloodied, AnkhWest item)
                : base(bloodied ? 0x1D97 : 0x2)
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

            public override bool HandlesOnMovement { get { return true; } } // Tell the core that we implement OnMovement

            public override void OnMovement(Mobile m, Point3D oldLocation)
            {
                if (Parent == null && Utility.InRange(Location, m.Location, 1) && !Utility.InRange(Location, oldLocation, 1))
                    Ankhs.Resurrect(m, this);
            }

            public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
            {
                base.GetContextMenuEntries(from, list);
                Ankhs.GetContextMenuEntries(from, this, list);
            }

            [Hue, CommandProperty(AccessLevel.GameMaster)]
            public override int Hue
            {
                get { return base.Hue; }
                set { base.Hue = value; if (m_Item.Hue != value) m_Item.Hue = value; }
            }

            public override void OnDoubleClickDead(Mobile m)
            {
                Ankhs.Resurrect(m, this);
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

                m_Item = reader.ReadItem() as AnkhWest;
            }
        }
    }

    [TypeAlias("Server.Items.AnkhEast")]
    public class AnkhNorth : Item
    {
        private InternalItem m_Item;

        [Constructable]
        public AnkhNorth()
            : this(false)
        {
        }

        [Constructable]
        public AnkhNorth(bool bloodied)
            : base(bloodied ? 0x1E5D : 0x4)
        {
            Movable = false;

            m_Item = new InternalItem(bloodied, this);
        }

        public AnkhNorth(Serial serial)
            : base(serial)
        {
        }

        public override bool HandlesOnMovement { get { return true; } } // Tell the core that we implement OnMovement

        public override void OnMovement(Mobile m, Point3D oldLocation)
        {
            if (Parent == null && Utility.InRange(Location, m.Location, 1) && !Utility.InRange(Location, oldLocation, 1))
                Ankhs.Resurrect(m, this);
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);
            Ankhs.GetContextMenuEntries(from, this, list);
        }

        [Hue, CommandProperty(AccessLevel.GameMaster)]
        public override int Hue
        {
            get { return base.Hue; }
            set { base.Hue = value; if (m_Item.Hue != value) m_Item.Hue = value; }
        }

        public override void OnDoubleClickDead(Mobile m)
        {
            Ankhs.Resurrect(m, this);
        }

        public override void OnLocationChange(Point3D oldLocation)
        {
            if (m_Item != null)
                m_Item.Location = new Point3D(X + 1, Y, Z);
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

            m_Item = reader.ReadItem() as InternalItem;
        }

        [TypeAlias("Server.Items.AnkhEast+InternalItem")]
        private class InternalItem : Item
        {
            private AnkhNorth m_Item;

            public InternalItem(bool bloodied, AnkhNorth item)
                : base(bloodied ? 0x1E5C : 0x5)
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

            public override bool HandlesOnMovement { get { return true; } } // Tell the core that we implement OnMovement

            public override void OnMovement(Mobile m, Point3D oldLocation)
            {
                if (Parent == null && Utility.InRange(Location, m.Location, 1) && !Utility.InRange(Location, oldLocation, 1))
                    Ankhs.Resurrect(m, this);
            }

            public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
            {
                base.GetContextMenuEntries(from, list);
                Ankhs.GetContextMenuEntries(from, this, list);
            }

            [Hue, CommandProperty(AccessLevel.GameMaster)]
            public override int Hue
            {
                get { return base.Hue; }
                set { base.Hue = value; if (m_Item.Hue != value) m_Item.Hue = value; }
            }

            public override void OnDoubleClickDead(Mobile m)
            {
                Ankhs.Resurrect(m, this);
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

                m_Item = reader.ReadItem() as AnkhNorth;
            }
        }
    }

    /// <summary>
    /// This ankh heals and refreshes player stats
    /// </summary>
    public class RejuvinationAddonComponent : AddonComponent
    {
        public RejuvinationAddonComponent(int itemID)
            : base(itemID)
        {
        }

        public RejuvinationAddonComponent(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.BeginAction(typeof(RejuvinationAddonComponent)))
            {
                from.FixedEffect(0x373A, 1, 16);

                int random = Utility.Random(1, 4);

                if (random == 1 || random == 4)
                {
                    from.Hits = from.HitsMax;
                    SendLocalizedMessageTo(from, 500801); // A sense of warmth fills your body!
                }

                if (random == 2 || random == 4)
                {
                    from.Mana = from.ManaMax;
                    SendLocalizedMessageTo(from, 500802); // A feeling of power surges through your veins!
                }

                if (random == 3 || random == 4)
                {
                    from.Stam = from.StamMax;
                    SendLocalizedMessageTo(from, 500803); // You feel as though you've slept for days!
                }

                Timer.DelayCall(TimeSpan.FromHours(2.0), new TimerStateCallback(ReleaseUseLock_Callback), new object[] { from, random });
            }
        }

        public virtual void ReleaseUseLock_Callback(object state)
        {
            object[] states = (object[])state;

            Mobile from = (Mobile)states[0];
            int random = (int)states[1];

            from.EndAction(typeof(RejuvinationAddonComponent));

            if (random == 4)
            {
                from.Hits = from.HitsMax;
                from.Mana = from.ManaMax;
                from.Stam = from.StamMax;
                SendLocalizedMessageTo(from, 500807); // You feel completely rejuvinated!
            }
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

    public abstract class BaseRejuvinationAnkh : BaseAddon
    {
        public BaseRejuvinationAnkh()
        {
        }

        public override bool HandlesOnMovement { get { return true; } }

        private DateTime m_NextMessage;

        public override void OnMovement(Mobile m, Point3D oldLocation)
        {
            base.OnMovement(m, oldLocation);

            if (m.Player && Utility.InRange(Location, m.Location, 3) && !Utility.InRange(Location, oldLocation, 3))
            {
                if (DateTime.UtcNow >= m_NextMessage)
                {
                    if (Components.Count > 0)
                        ((AddonComponent)Components[0]).SendLocalizedMessageTo(m, 1010061); // An overwhelming sense of peace fills you.

                    m_NextMessage = DateTime.UtcNow + TimeSpan.FromSeconds(25.0);
                }
            }
        }

        public BaseRejuvinationAnkh(Serial serial)
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

    public class RejuvinationAnkhWest : BaseRejuvinationAnkh
    {
        [Constructable]
        public RejuvinationAnkhWest()
        {
            AddComponent(new RejuvinationAddonComponent(0x3), 0, 0, 0);
            AddComponent(new RejuvinationAddonComponent(0x2), 0, 1, 0);
        }

        public RejuvinationAnkhWest(Serial serial)
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

    public class RejuvinationAnkhNorth : BaseRejuvinationAnkh
    {
        [Constructable]
        public RejuvinationAnkhNorth()
        {
            AddComponent(new RejuvinationAddonComponent(0x4), 0, 0, 0);
            AddComponent(new RejuvinationAddonComponent(0x5), 1, 0, 0);
        }

        public RejuvinationAnkhNorth(Serial serial)
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
}