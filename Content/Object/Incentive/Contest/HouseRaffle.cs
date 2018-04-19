using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

using Server;
using Server.Accounting;
using Server.ContextMenus;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Misc;
using Server.Network;
using Server.Regions;
using Server.Spells.Sixth;
using Server.Targeting;

namespace Server.Gumps
{
    public class HouseRaffleManagementGump : Gump
    {
        public enum SortMethod
        {
            Default,
            Name,
            Account,
            Address
        }

        public string Right(string text)
        {
            return String.Format("<DIV ALIGN=RIGHT>{0}</DIV>", text);
        }

        public string Center(string text)
        {
            return String.Format("<CENTER>{0}</CENTER>", text);
        }

        public string Color(string text, int color)
        {
            return String.Format("<BASEFONT COLOR=#{0:X6}>{1}</BASEFONT>", color, text);
        }

        public const int LabelColor = 0xFFFFFF;
        public const int HighlightColor = 0x11EE11;

        private HouseRaffleStone m_Stone;
        private int m_Page;
        private List<RaffleEntry> m_List;
        private SortMethod m_Sort;

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;
            int buttonId = info.ButtonID;

            switch (buttonId)
            {
                case 1: // Previous
                    {
                        if (m_Page > 0)
                            m_Page--;

                        from.SendGump(new HouseRaffleManagementGump(m_Stone, m_Sort, m_Page));

                        break;
                    }
                case 2: // Next
                    {
                        if ((m_Page + 1) * 10 < m_Stone.Entries.Count)
                            m_Page++;

                        from.SendGump(new HouseRaffleManagementGump(m_Stone, m_Sort, m_Page));

                        break;
                    }
                case 3: // Sort by name
                    {
                        from.SendGump(new HouseRaffleManagementGump(m_Stone, SortMethod.Name, 0));

                        break;
                    }
                case 4: // Sort by account
                    {
                        from.SendGump(new HouseRaffleManagementGump(m_Stone, SortMethod.Account, 0));

                        break;
                    }
                case 5: // Sort by address
                    {
                        from.SendGump(new HouseRaffleManagementGump(m_Stone, SortMethod.Address, 0));

                        break;
                    }
                default: // Delete
                    {
                        buttonId -= 6;

                        if (buttonId >= 0 && buttonId < m_List.Count)
                        {
                            m_Stone.Entries.Remove(m_List[buttonId]);

                            if (m_Page > 0 && m_Page * 10 >= m_List.Count - 1)
                                m_Page--;

                            from.SendGump(new HouseRaffleManagementGump(m_Stone, m_Sort, m_Page));
                        }

                        break;
                    }
            }
        }

        public HouseRaffleManagementGump(HouseRaffleStone stone)
            : this(stone, SortMethod.Default, 0)
        {
        }

        public HouseRaffleManagementGump(HouseRaffleStone stone, SortMethod sort, int page)
            : base(40, 40)
        {
            m_Stone = stone;
            m_Page = page;

            m_List = new List<RaffleEntry>(m_Stone.Entries);
            m_Sort = sort;

            switch (m_Sort)
            {
                case SortMethod.Name:
                    {
                        m_List.Sort(NameComparer.Instance);

                        break;
                    }
                case SortMethod.Account:
                    {
                        m_List.Sort(AccountComparer.Instance);

                        break;
                    }
                case SortMethod.Address:
                    {
                        m_List.Sort(AddressComparer.Instance);

                        break;
                    }
            }

            AddPage(0);

            AddBackground(0, 0, 618, 354, 9270);
            AddAlphaRegion(10, 10, 598, 334);

            AddHtml(10, 10, 598, 20, Color(Center("Raffle Management"), LabelColor), false, false);

            AddHtml(45, 35, 100, 20, Color("Location:", LabelColor), false, false);
            AddHtml(145, 35, 250, 20, Color(m_Stone.FormatLocation(), LabelColor), false, false);

            AddHtml(45, 55, 100, 20, Color("Ticket Price:", LabelColor), false, false);
            AddHtml(145, 55, 250, 20, Color(m_Stone.FormatPrice(), LabelColor), false, false);

            AddHtml(45, 75, 100, 20, Color("Total Entries:", LabelColor), false, false);
            AddHtml(145, 75, 250, 20, Color(m_Stone.Entries.Count.ToString(), LabelColor), false, false);

            AddButton(440, 33, 0xFA5, 0xFA7, 3, GumpButtonType.Reply, 0);
            AddHtml(474, 35, 120, 20, Color("Sort by name", LabelColor), false, false);

            AddButton(440, 53, 0xFA5, 0xFA7, 4, GumpButtonType.Reply, 0);
            AddHtml(474, 55, 120, 20, Color("Sort by account", LabelColor), false, false);

            AddButton(440, 73, 0xFA5, 0xFA7, 5, GumpButtonType.Reply, 0);
            AddHtml(474, 75, 120, 20, Color("Sort by address", LabelColor), false, false);

            AddImageTiled(13, 99, 592, 242, 9264);
            AddImageTiled(14, 100, 590, 240, 9274);
            AddAlphaRegion(14, 100, 590, 240);

            AddHtml(14, 100, 590, 20, Color(Center("Entries"), LabelColor), false, false);

            if (page > 0)
                AddButton(567, 104, 0x15E3, 0x15E7, 1, GumpButtonType.Reply, 0);
            else
                AddImage(567, 104, 0x25EA);

            if ((page + 1) * 10 < m_List.Count)
                AddButton(584, 104, 0x15E1, 0x15E5, 2, GumpButtonType.Reply, 0);
            else
                AddImage(584, 104, 0x25E6);

            AddHtml(14, 120, 30, 20, Color(Center("DEL"), LabelColor), false, false);
            AddHtml(47, 120, 250, 20, Color("Name", LabelColor), false, false);
            AddHtml(295, 120, 100, 20, Color(Center("Address"), LabelColor), false, false);
            AddHtml(395, 120, 150, 20, Color(Center("Date"), LabelColor), false, false);
            AddHtml(545, 120, 60, 20, Color(Center("Num"), LabelColor), false, false);

            int idx = 0;
            Mobile winner = m_Stone.Winner;

            for (int i = page * 10; i >= 0 && i < m_List.Count && i < (page + 1) * 10; ++i, ++idx)
            {
                RaffleEntry entry = m_List[i];

                if (entry == null)
                    continue;

                AddButton(13, 138 + (idx * 20), 4002, 4004, 6 + i, GumpButtonType.Reply, 0);

                int x = 45;
                int color = (winner != null && entry.From == winner) ? HighlightColor : LabelColor;

                string name = null;

                if (entry.From != null)
                {
                    Account acc = entry.From.Account as Account;

                    if (acc != null)
                        name = String.Format("{0} ({1})", entry.From.Name, acc);
                    else
                        name = entry.From.Name;
                }

                if (name != null)
                    AddHtml(x + 2, 140 + (idx * 20), 250, 20, Color(name, color), false, false);

                x += 250;

                if (entry.Address != null)
                    AddHtml(x, 140 + (idx * 20), 100, 20, Color(Center(entry.Address.ToString()), color), false, false);

                x += 100;

                AddHtml(x, 140 + (idx * 20), 150, 20, Color(Center(entry.Date.ToString()), color), false, false);
                x += 150;

                AddHtml(x, 140 + (idx * 20), 60, 20, Color(Center("1"), color), false, false);
                x += 60;
            }
        }

        private class NameComparer : IComparer<RaffleEntry>
        {
            public static readonly IComparer<RaffleEntry> Instance = new NameComparer();

            public NameComparer()
            {
            }

            public int Compare(RaffleEntry x, RaffleEntry y)
            {
                bool xIsNull = (x == null || x.From == null);
                bool yIsNull = (y == null || y.From == null);

                if (xIsNull && yIsNull)
                    return 0;
                else if (xIsNull)
                    return -1;
                else if (yIsNull)
                    return 1;

                int result = Insensitive.Compare(x.From.Name, y.From.Name);

                if (result == 0)
                    return x.Date.CompareTo(y.Date);
                else
                    return result;
            }
        }

        private class AccountComparer : IComparer<RaffleEntry>
        {
            public static readonly IComparer<RaffleEntry> Instance = new AccountComparer();

            public AccountComparer()
            {
            }

            public int Compare(RaffleEntry x, RaffleEntry y)
            {
                bool xIsNull = (x == null || x.From == null);
                bool yIsNull = (y == null || y.From == null);

                if (xIsNull && yIsNull)
                    return 0;
                else if (xIsNull)
                    return -1;
                else if (yIsNull)
                    return 1;

                Account a = x.From.Account as Account;
                Account b = y.From.Account as Account;

                if (a == null && b == null)
                    return 0;
                else if (a == null)
                    return -1;
                else if (b == null)
                    return 1;

                int result = Insensitive.Compare(a.Username, b.Username);

                if (result == 0)
                    return x.Date.CompareTo(y.Date);
                else
                    return result;
            }
        }

        private class AddressComparer : IComparer<RaffleEntry>
        {
            public static readonly IComparer<RaffleEntry> Instance = new AddressComparer();

            public AddressComparer()
            {
            }

            public int Compare(RaffleEntry x, RaffleEntry y)
            {
                bool xIsNull = (x == null || x.Address == null);
                bool yIsNull = (y == null || y.Address == null);

                if (xIsNull && yIsNull)
                    return 0;
                else if (xIsNull)
                    return -1;
                else if (yIsNull)
                    return 1;

                byte[] a = x.Address.GetAddressBytes();
                byte[] b = y.Address.GetAddressBytes();

                for (int i = 0; i < a.Length && i < b.Length; i++)
                {
                    int compare = a[i].CompareTo(b[i]);

                    if (compare != 0)
                        return compare;
                }

                return x.Date.CompareTo(y.Date);
            }
        }
    }
}

namespace Server.Items
{
    public class RaffleEntry
    {
        private Mobile m_From;
        private IPAddress m_Address;
        private DateTime m_Date;

        public Mobile From
        {
            get { return m_From; }
        }

        public IPAddress Address
        {
            get { return m_Address; }
        }

        public DateTime Date
        {
            get { return m_Date; }
        }

        public RaffleEntry(Mobile from)
        {
            m_From = from;

            if (m_From.NetState != null)
                m_Address = m_From.NetState.Address;
            else
                m_Address = IPAddress.None;

            m_Date = DateTime.UtcNow;
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(m_From);
            writer.Write(m_Address);
            writer.Write(m_Date);
        }

        public RaffleEntry(GenericReader reader, int version)
        {
            switch (version)
            {
                case 3: // HouseRaffleStone version changes
                case 2:
                case 1:
                case 0:
                    {
                        m_From = reader.ReadMobile();
                        m_Address = Utility.Intern(reader.ReadIPAddress());
                        m_Date = reader.ReadDateTime();

                        break;
                    }
            }
        }
    }

    public enum HouseRaffleState
    {
        Inactive,
        Active,
        Completed
    }

    public enum HouseRaffleExpireAction
    {
        None,
        HideStone,
        DeleteStone
    }

    [FlipableAttribute(0xEDD, 0xEDE)]
    public class HouseRaffleStone : Item
    {
        private const int EntryLimitPerIP = 4;
        private const int DefaultTicketPrice = 5000;
        private const int MessageHue = 1153;

        public static readonly TimeSpan DefaultDuration = TimeSpan.FromDays(7.0);
        public static readonly TimeSpan ExpirationTime = TimeSpan.FromDays(30.0);

        private HouseRaffleRegion m_Region;
        private Rectangle2D m_Bounds;
        private Map m_Facet;

        private Mobile m_Winner;
        private HouseRaffleDeed m_Deed;

        private HouseRaffleState m_State;
        private DateTime m_Started;
        private TimeSpan m_Duration;
        private HouseRaffleExpireAction m_ExpireAction;
        private int m_TicketPrice;

        private List<RaffleEntry> m_Entries;

        [CommandProperty(AccessLevel.GameMaster, AccessLevel.Seer)]
        public HouseRaffleState CurrentState
        {
            get { return m_State; }
            set
            {
                if (m_State != value)
                {
                    if (value == HouseRaffleState.Active)
                    {
                        m_Entries.Clear();
                        m_Winner = null;
                        m_Deed = null;
                        m_Started = DateTime.UtcNow;
                    }

                    m_State = value;
                    InvalidateProperties();
                }
            }
        }

        [CommandProperty(AccessLevel.GameMaster, AccessLevel.Seer)]
        public Rectangle2D PlotBounds
        {
            get { return m_Bounds; }
            set
            {
                m_Bounds = value;

                InvalidateRegion();
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster, AccessLevel.Seer)]
        public Map PlotFacet
        {
            get { return m_Facet; }
            set
            {
                m_Facet = value;

                InvalidateRegion();
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster, AccessLevel.Seer)]
        public Mobile Winner
        {
            get { return m_Winner; }
            set { m_Winner = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster, AccessLevel.Seer)]
        public HouseRaffleDeed Deed
        {
            get { return m_Deed; }
            set { m_Deed = value; }
        }

        [CommandProperty(AccessLevel.GameMaster, AccessLevel.Seer)]
        public DateTime Started
        {
            get { return m_Started; }
            set { m_Started = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster, AccessLevel.Seer)]
        public TimeSpan Duration
        {
            get { return m_Duration; }
            set { m_Duration = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsExpired
        {
            get
            {
                if (m_State != HouseRaffleState.Completed)
                    return false;

                return (m_Started + m_Duration + ExpirationTime <= DateTime.UtcNow);
            }
        }

        [CommandProperty(AccessLevel.GameMaster, AccessLevel.Seer)]
        public HouseRaffleExpireAction ExpireAction
        {
            get { return m_ExpireAction; }
            set { m_ExpireAction = value; }
        }

        [CommandProperty(AccessLevel.GameMaster, AccessLevel.Seer)]
        public int TicketPrice
        {
            get { return m_TicketPrice; }
            set
            {
                m_TicketPrice = Math.Max(0, value);
                InvalidateProperties();
            }
        }

        public List<RaffleEntry> Entries
        {
            get { return m_Entries; }
        }

        public override string DefaultName
        {
            get { return "a house raffle stone"; }
        }

        public override bool DisplayWeight
        {
            get { return false; }
        }

        private static List<HouseRaffleStone> m_AllStones = new List<HouseRaffleStone>();

        public static void CheckEnd_OnTick()
        {
            for (int i = 0; i < m_AllStones.Count; i++)
                m_AllStones[i].CheckEnd();
        }

        public static void Initialize()
        {
            for (int i = m_AllStones.Count - 1; i >= 0; i--)
            {
                HouseRaffleStone stone = m_AllStones[i];

                if (stone.IsExpired)
                {
                    switch (stone.ExpireAction)
                    {
                        case HouseRaffleExpireAction.HideStone:
                            {
                                if (stone.Visible)
                                {
                                    stone.Visible = false;
                                    stone.ItemID = 0x1B7B; // Non-blocking ItemID
                                }

                                break;
                            }
                        case HouseRaffleExpireAction.DeleteStone:
                            {
                                stone.Delete();
                                break;
                            }
                    }
                }
            }

            Timer.DelayCall(TimeSpan.FromMinutes(1.0), TimeSpan.FromMinutes(1.0), new TimerCallback(CheckEnd_OnTick));
        }

        [Constructable]
        public HouseRaffleStone()
            : base(0xEDD)
        {
            m_Region = null;
            m_Bounds = new Rectangle2D();
            m_Facet = null;

            m_Winner = null;
            m_Deed = null;

            m_State = HouseRaffleState.Inactive;
            m_Started = DateTime.MinValue;
            m_Duration = DefaultDuration;
            m_ExpireAction = HouseRaffleExpireAction.None;
            m_TicketPrice = DefaultTicketPrice;

            m_Entries = new List<RaffleEntry>();

            Movable = false;

            m_AllStones.Add(this);
        }

        public HouseRaffleStone(Serial serial)
            : base(serial)
        {
        }

        public bool ValidLocation()
        {
            return (m_Bounds.Start != Point2D.Zero && m_Bounds.End != Point2D.Zero && m_Facet != null && m_Facet != Map.Internal);
        }

        private void InvalidateRegion()
        {
            if (m_Region != null)
            {
                m_Region.Unregister();
                m_Region = null;
            }

            if (ValidLocation())
            {
                m_Region = new HouseRaffleRegion(this);
                m_Region.Register();
            }
        }

        private bool HasEntered(Mobile from)
        {
            Account acc = from.Account as Account;

            if (acc == null)
                return false;

            foreach (RaffleEntry entry in m_Entries)
            {
                if (entry.From != null)
                {
                    Account entryAcc = entry.From.Account as Account;

                    if (entryAcc == acc)
                        return true;
                }
            }

            return false;
        }

        private bool IsAtIPLimit(Mobile from)
        {
            if (from.NetState == null)
                return false;

            IPAddress address = from.NetState.Address;
            int tickets = 0;

            foreach (RaffleEntry entry in m_Entries)
            {
                if (Utility.IPMatchClassC(entry.Address, address))
                {
                    if (++tickets >= EntryLimitPerIP)
                        return true;
                }
            }

            return false;
        }

        public static string FormatLocation(Point3D loc, Map map, bool displayMap)
        {
            StringBuilder result = new StringBuilder();

            int xLong = 0, yLat = 0;
            int xMins = 0, yMins = 0;
            bool xEast = false, ySouth = false;

            if (Sextant.Format(loc, map, ref xLong, ref yLat, ref xMins, ref yMins, ref xEast, ref ySouth))
                result.AppendFormat("{0}°{1}'{2},{3}°{4}'{5}", yLat, yMins, ySouth ? "S" : "N", xLong, xMins, xEast ? "E" : "W");
            else
                result.AppendFormat("{0},{1}", loc.X, loc.Y);

            if (displayMap)
                result.AppendFormat(" ({0})", map);

            return result.ToString();
        }

        public Point3D GetPlotCenter()
        {
            int x = m_Bounds.X + m_Bounds.Width / 2;
            int y = m_Bounds.Y + m_Bounds.Height / 2;
            int z = (m_Facet == null) ? 0 : m_Facet.GetAverageZ(x, y);

            return new Point3D(x, y, z);
        }

        public string FormatLocation()
        {
            if (!ValidLocation())
                return "no location set";

            return FormatLocation(GetPlotCenter(), m_Facet, true);
        }

        public string FormatPrice()
        {
            if (m_TicketPrice == 0)
                return "FREE";
            else
                return String.Format("{0} gold", m_TicketPrice);
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (ValidLocation())
                list.Add(FormatLocation());

            switch (m_State)
            {
                case HouseRaffleState.Active:
                    {
                        list.Add(1060658, "ticket price\t{0}", FormatPrice()); // ~1_val~: ~2_val~
                        list.Add(1060659, "ends\t{0}", m_Started + m_Duration); // ~1_val~: ~2_val~
                        break;
                    }
                case HouseRaffleState.Completed:
                    {
                        list.Add(1060658, "winner\t{0}", (m_Winner == null) ? "unknown" : m_Winner.Name); // ~1_val~: ~2_val~
                        break;
                    }
            }
        }

        public override void OnSingleClick(Mobile from)
        {
            base.OnSingleClick(from);

            switch (m_State)
            {
                case HouseRaffleState.Active:
                    {
                        LabelTo(from, 1060658, String.Format("Ends\t{0}", m_Started + m_Duration)); // ~1_val~: ~2_val~
                        break;
                    }
                case HouseRaffleState.Completed:
                    {
                        LabelTo(from, 1060658, String.Format("Winner\t{0}", (m_Winner == null) ? "Unknown" : m_Winner.Name)); // ~1_val~: ~2_val~
                        break;
                    }
            }
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);

            if (from.AccessLevel >= AccessLevel.Seer)
            {
                list.Add(new EditEntry(from, this));

                if (m_State == HouseRaffleState.Inactive)
                    list.Add(new ActivateEntry(from, this));
                else
                    list.Add(new ManagementEntry(from, this));
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (m_State != HouseRaffleState.Active || !from.CheckAlive())
                return;

            if (!from.InRange(GetWorldLocation(), 2))
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
                return;
            }

            if (HasEntered(from))
            {
                from.SendMessage(MessageHue, "You have already entered this plot's raffle.");
            }
            else if (IsAtIPLimit(from))
            {
                from.SendMessage(MessageHue, "You may not enter this plot's raffle.");
            }
            else
            {
                from.SendGump(new WarningGump(1150470, 0x7F00, String.Format("You are about to purchase a raffle ticket for the house plot located at {0}.  The ticket price is {1}.  Tickets are non-refundable and you can only purchase one ticket per account.  Do you wish to continue?", FormatLocation(), FormatPrice()), 0xFFFFFF, 420, 280, new WarningGumpCallback(Purchase_Callback), null)); // CONFIRM TICKET PURCHASE
            }
        }

        public void Purchase_Callback(Mobile from, bool okay, object state)
        {
            if (Deleted || m_State != HouseRaffleState.Active || !from.CheckAlive() || HasEntered(from) || IsAtIPLimit(from))
                return;

            Account acc = from.Account as Account;

            if (acc == null)
                return;

            if (okay)
            {
                Container bank = from.FindBankNoCreate();

                if (m_TicketPrice == 0 || (from.Backpack != null && from.Backpack.ConsumeTotal(typeof(Gold), m_TicketPrice)) || (bank != null && bank.ConsumeTotal(typeof(Gold), m_TicketPrice)))
                {
                    m_Entries.Add(new RaffleEntry(from));

                    from.SendMessage(MessageHue, "You have successfully entered the plot's raffle.");
                }
                else
                {
                    from.SendMessage(MessageHue, "You do not have the {0} required to enter the raffle.", FormatPrice());
                }
            }
            else
            {
                from.SendMessage(MessageHue, "You have chosen not to enter the raffle.");
            }
        }

        public void CheckEnd()
        {
            if (m_State != HouseRaffleState.Active || m_Started + m_Duration > DateTime.UtcNow)
                return;

            m_State = HouseRaffleState.Completed;

            if (m_Region != null && m_Entries.Count != 0)
            {
                int winner = Utility.Random(m_Entries.Count);

                m_Winner = m_Entries[winner].From;

                if (m_Winner != null)
                {
                    m_Deed = new HouseRaffleDeed(this, m_Winner);

                    m_Winner.SendMessage(MessageHue, "Congratulations, {0}!  You have won the raffle for the plot located at {1}.", m_Winner.Name, FormatLocation());

                    if (m_Winner.AddToBackpack(m_Deed))
                    {
                        m_Winner.SendMessage(MessageHue, "The writ of lease has been placed in your backpack.");
                    }
                    else
                    {
                        m_Winner.BankBox.DropItem(m_Deed);
                        m_Winner.SendMessage(MessageHue, "As your backpack is full, the writ of lease has been placed in your bank box.");
                    }
                }
            }

            InvalidateProperties();
        }

        public override void OnDelete()
        {
            if (m_Region != null)
            {
                m_Region.Unregister();
                m_Region = null;
            }

            m_AllStones.Remove(this);

            base.OnDelete();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)3); // version

            writer.WriteEncodedInt((int)m_State);
            writer.WriteEncodedInt((int)m_ExpireAction);

            writer.Write(m_Deed);

            writer.Write(m_Bounds);
            writer.Write(m_Facet);

            writer.Write(m_Winner);

            writer.Write(m_TicketPrice);
            writer.Write(m_Started);
            writer.Write(m_Duration);

            writer.Write(m_Entries.Count);

            foreach (RaffleEntry entry in m_Entries)
                entry.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 3:
                    {
                        m_State = (HouseRaffleState)reader.ReadEncodedInt();

                        goto case 2;
                    }
                case 2:
                    {
                        m_ExpireAction = (HouseRaffleExpireAction)reader.ReadEncodedInt();

                        goto case 1;
                    }
                case 1:
                    {
                        m_Deed = reader.ReadItem<HouseRaffleDeed>();

                        goto case 0;
                    }
                case 0:
                    {
                        bool oldActive = (version < 3) ? reader.ReadBool() : false;

                        m_Bounds = reader.ReadRect2D();
                        m_Facet = reader.ReadMap();

                        m_Winner = reader.ReadMobile();

                        m_TicketPrice = reader.ReadInt();
                        m_Started = reader.ReadDateTime();
                        m_Duration = reader.ReadTimeSpan();

                        int entryCount = reader.ReadInt();
                        m_Entries = new List<RaffleEntry>(entryCount);

                        for (int i = 0; i < entryCount; i++)
                        {
                            RaffleEntry entry = new RaffleEntry(reader, version);

                            if (entry.From == null)
                                continue; // Character was deleted

                            m_Entries.Add(entry);
                        }

                        InvalidateRegion();

                        m_AllStones.Add(this);

                        if (version < 3)
                        {
                            if (oldActive)
                                m_State = HouseRaffleState.Active;
                            else if (m_Winner != null)
                                m_State = HouseRaffleState.Completed;
                            else
                                m_State = HouseRaffleState.Inactive;
                        }

                        break;
                    }
            }
        }

        private class RaffleContextMenuEntry : ContextMenuEntry
        {
            protected Mobile m_From;
            protected HouseRaffleStone m_Stone;

            public RaffleContextMenuEntry(Mobile from, HouseRaffleStone stone, int label)
                : base(label)
            {
                m_From = from;
                m_Stone = stone;
            }
        }

        private class EditEntry : RaffleContextMenuEntry
        {
            public EditEntry(Mobile from, HouseRaffleStone stone)
                : base(from, stone, 5101) // Edit
            {
            }

            public override void OnClick()
            {
                if (m_Stone.Deleted || m_From.AccessLevel < AccessLevel.Seer)
                    return;

                m_From.SendGump(new PropertiesGump(m_From, m_Stone));
            }
        }

        private class ActivateEntry : RaffleContextMenuEntry
        {
            public ActivateEntry(Mobile from, HouseRaffleStone stone)
                : base(from, stone, 5113) // Start
            {
                if (!stone.ValidLocation())
                    Flags |= Network.CMEFlags.Disabled;
            }

            public override void OnClick()
            {
                if (m_Stone.Deleted || m_From.AccessLevel < AccessLevel.Seer || !m_Stone.ValidLocation())
                    return;

                m_Stone.CurrentState = HouseRaffleState.Active;
            }
        }

        private class ManagementEntry : RaffleContextMenuEntry
        {
            public ManagementEntry(Mobile from, HouseRaffleStone stone)
                : base(from, stone, 5032) // Game Monitor
            {
            }

            public override void OnClick()
            {
                if (m_Stone.Deleted || m_From.AccessLevel < AccessLevel.Seer)
                    return;

                m_From.SendGump(new HouseRaffleManagementGump(m_Stone));
            }
        }
    }

    public class HouseRaffleDeed : Item
    {
        private HouseRaffleStone m_Stone;
        private Point3D m_PlotLocation;
        private Map m_Facet;
        private Mobile m_AwardedTo;

        [CommandProperty(AccessLevel.GameMaster, AccessLevel.Seer)]
        public HouseRaffleStone Stone
        {
            get { return m_Stone; }
            set { m_Stone = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster, AccessLevel.Seer)]
        public Point3D PlotLocation
        {
            get { return m_PlotLocation; }
            set { m_PlotLocation = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster, AccessLevel.Seer)]
        public Map PlotFacet
        {
            get { return m_Facet; }
            set { m_Facet = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster, AccessLevel.Seer)]
        public Mobile AwardedTo
        {
            get { return m_AwardedTo; }
            set { m_AwardedTo = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster, AccessLevel.Seer)]
        public bool IsExpired
        {
            get { return (m_Stone == null || m_Stone.Deleted || m_Stone.IsExpired); }
        }

        public override string DefaultName
        {
            get { return "a writ of lease"; }
        }

        public override double DefaultWeight
        {
            get { return 1.0; }
        }

        [Constructable]
        public HouseRaffleDeed()
            : this(null, null)
        {
        }

        public HouseRaffleDeed(HouseRaffleStone stone, Mobile m)
            : base(0x2830)
        {
            m_Stone = stone;

            if (stone != null)
            {
                m_PlotLocation = stone.GetPlotCenter();
                m_Facet = stone.PlotFacet;
            }

            m_AwardedTo = m;

            LootType = LootType.Blessed;
            Hue = 0x501;
        }

        public HouseRaffleDeed(Serial serial)
            : base(serial)
        {
        }

        public bool ValidLocation()
        {
            return (m_PlotLocation != Point3D.Zero && m_Facet != null && m_Facet != Map.Internal);
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (ValidLocation())
            {
                list.Add(1060658, "location\t{0}", HouseRaffleStone.FormatLocation(m_PlotLocation, m_Facet, false)); // ~1_val~: ~2_val~
                list.Add(1060659, "facet\t{0}", m_Facet); // ~1_val~: ~2_val~
                list.Add(1150486); // [Marked Item]
            }

            if (IsExpired)
                list.Add(1150487); // [Expired]

            //list.Add( 1060660, "shard\t{0}", ServerList.ServerName ); // ~1_val~: ~2_val~
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!ValidLocation())
                return;

            if (IsChildOf(from.Backpack))
            {
                from.CloseGump(typeof(WritOfLeaseGump));
                from.SendGump(new WritOfLeaseGump(this));
            }
            else
            {
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version

            writer.Write(m_Stone);
            writer.Write(m_PlotLocation);
            writer.Write(m_Facet);
            writer.Write(m_AwardedTo);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                    {
                        m_Stone = reader.ReadItem<HouseRaffleStone>();

                        goto case 0;
                    }
                case 0:
                    {
                        m_PlotLocation = reader.ReadPoint3D();
                        m_Facet = reader.ReadMap();
                        m_AwardedTo = reader.ReadMobile();

                        break;
                    }
            }
        }

        private class WritOfLeaseGump : Gump
        {
            public WritOfLeaseGump(HouseRaffleDeed deed)
                : base(150, 50)
            {
                AddPage(0);

                AddImage(0, 0, 9380);
                AddImage(114, 0, 9381);
                AddImage(171, 0, 9382);
                AddImage(0, 140, 9383);
                AddImage(114, 140, 9384);
                AddImage(171, 140, 9385);
                AddImage(0, 182, 9383);
                AddImage(114, 182, 9384);
                AddImage(171, 182, 9385);
                AddImage(0, 224, 9383);
                AddImage(114, 224, 9384);
                AddImage(171, 224, 9385);
                AddImage(0, 266, 9386);
                AddImage(114, 266, 9387);
                AddImage(171, 266, 9388);

                AddHtmlLocalized(30, 48, 229, 20, 1150484, 200, false, false); // WRIT OF LEASE
                AddHtml(28, 75, 231, 280, FormatDescription(deed), false, true);
            }

            private static string FormatDescription(HouseRaffleDeed deed)
            {
                if (deed == null)
                    return String.Empty;

                if (deed.IsExpired)
                {
                    return String.Format(
                        "<bodytextblack>" +
                        "This deed once entitled the bearer to build a house on the plot of land " +
                        "located at {0} on the {1} facet.<br><br>" +

                        "The deed has expired, and now the indicated plot of land " +
                        "is subject to normal house construction rules.<br><br>" +

                        "This deed functions as a recall rune marked for the location of the plot it represents." +
                        "</bodytextblack>",
                        HouseRaffleStone.FormatLocation(deed.PlotLocation, deed.PlotFacet, false),
                        deed.PlotFacet
                    );
                }
                else
                {
                    int daysLeft = (int)Math.Ceiling((deed.Stone.Started + deed.Stone.Duration + HouseRaffleStone.ExpirationTime - DateTime.UtcNow).TotalDays);

                    return String.Format(
                        "<bodytextblack>" +
                        "This deed entitles the bearer to build a house on the plot of land " +
                        "located at {0} on the {1} facet.<br><br>" +

                        "The deed will expire after {2} more day{3} have passed, and at that time the right to place " +
                        "a house reverts to normal house construction rules.<br><br>" +

                        "This deed functions as a recall rune marked for the location of the plot it represents.<br><br>" +

                        "To place a house on the deeded plot, you must simply have this deed in your backpack " +
                        "or bank box when using a House Placement Tool there." +
                        "</bodytextblack>",
                        HouseRaffleStone.FormatLocation(deed.PlotLocation, deed.PlotFacet, false),
                        deed.PlotFacet,
                        daysLeft,
                        (daysLeft == 1) ? "" : "s"
                    );
                }
            }
        }
    }
}

namespace Server.Regions
{
    public class HouseRaffleRegion : BaseRegion
    {
        private HouseRaffleStone m_Stone;

        public HouseRaffleRegion(HouseRaffleStone stone)
            : base(null, stone.PlotFacet, Region.DefaultPriority, stone.PlotBounds)
        {
            m_Stone = stone;
        }

        public override bool AllowHousing(Mobile from, Point3D p)
        {
            if (m_Stone == null)
                return false;

            if (m_Stone.IsExpired)
                return true;

            if (m_Stone.Deed == null)
                return false;

            Container pack = from.Backpack;

            if (pack != null && ContainsDeed(pack))
                return true;

            BankBox bank = from.FindBankNoCreate();

            if (bank != null && ContainsDeed(bank))
                return true;

            return false;
        }

        private bool ContainsDeed(Container cont)
        {
            List<HouseRaffleDeed> deeds = cont.FindItemsByType<HouseRaffleDeed>();

            for (int i = 0; i < deeds.Count; ++i)
            {
                if (deeds[i] == m_Stone.Deed)
                    return true;
            }

            return false;
        }

        public override bool OnTarget(Mobile m, Target t, object o)
        {
            if (m.Spell != null && m.Spell is MarkSpell && m.AccessLevel == AccessLevel.Player)
            {
                m.SendLocalizedMessage(501800); // You cannot mark an object at that location.
                return false;
            }

            return base.OnTarget(m, t, o);
        }
    }
}