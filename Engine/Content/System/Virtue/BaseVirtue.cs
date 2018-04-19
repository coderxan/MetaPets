using System;
using System.Collections.Generic;

using Server;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server
{
    public enum VirtueLevel
    {
        None,
        Seeker,
        Follower,
        Knight
    }

    public enum VirtueName
    {
        Humility,
        Sacrifice,
        Compassion,
        Spirituality,
        Valor,
        Honor,
        Justice,
        Honesty
    }

    public class VirtueHelper
    {
        public static bool HasAny(Mobile from, VirtueName virtue)
        {
            return (from.Virtues.GetValue((int)virtue) > 0);
        }

        public static bool IsHighestPath(Mobile from, VirtueName virtue)
        {
            return (from.Virtues.GetValue((int)virtue) >= GetMaxAmount(virtue));
        }

        public static VirtueLevel GetLevel(Mobile from, VirtueName virtue)
        {
            int v = from.Virtues.GetValue((int)virtue);
            int vl;
            int vmax = GetMaxAmount(virtue);

            if (v < 4000)
                vl = 0;
            else if (v >= vmax)
                vl = 3;
            else
                vl = (v + 9999) / 10000;

            return (VirtueLevel)vl;
        }

        public static int GetMaxAmount(VirtueName virtue)
        {
            if (virtue == VirtueName.Honor)
                return 20000;

            if (virtue == VirtueName.Sacrifice)
                return 22000;

            return 21000;
        }

        public static bool Award(Mobile from, VirtueName virtue, int amount, ref bool gainedPath)
        {
            int current = from.Virtues.GetValue((int)virtue);

            int maxAmount = GetMaxAmount(virtue);

            if (current >= maxAmount)
                return false;

            if ((current + amount) >= maxAmount)
                amount = maxAmount - current;

            VirtueLevel oldLevel = GetLevel(from, virtue);

            from.Virtues.SetValue((int)virtue, current + amount);

            gainedPath = (GetLevel(from, virtue) != oldLevel);

            return true;
        }

        public static bool Atrophy(Mobile from, VirtueName virtue)
        {
            return Atrophy(from, virtue, 1);
        }

        public static bool Atrophy(Mobile from, VirtueName virtue, int amount)
        {
            int current = from.Virtues.GetValue((int)virtue);

            if ((current - amount) >= 0)
                from.Virtues.SetValue((int)virtue, current - amount);
            else
                from.Virtues.SetValue((int)virtue, 0);

            return (current > 0);
        }

        public static bool IsSeeker(Mobile from, VirtueName virtue)
        {
            return (GetLevel(from, virtue) >= VirtueLevel.Seeker);
        }

        public static bool IsFollower(Mobile from, VirtueName virtue)
        {
            return (GetLevel(from, virtue) >= VirtueLevel.Follower);
        }

        public static bool IsKnight(Mobile from, VirtueName virtue)
        {
            return (GetLevel(from, virtue) >= VirtueLevel.Knight);
        }

        public static void AwardVirtue(PlayerMobile pm, VirtueName virtue, int amount)
        {
            if (virtue == VirtueName.Compassion)
            {
                if (pm.CompassionGains > 0 && DateTime.UtcNow > pm.NextCompassionDay)
                {
                    pm.NextCompassionDay = DateTime.MinValue;
                    pm.CompassionGains = 0;
                }

                if (pm.CompassionGains >= 5)
                {
                    pm.SendLocalizedMessage(1053004); // You must wait about a day before you can gain in compassion again.
                    return;
                }
            }

            bool gainedPath = false;
            string virtueName = Enum.GetName(typeof(VirtueName), virtue);

            if (VirtueHelper.Award(pm, virtue, amount, ref gainedPath))
            {
                // TODO: Localize?
                if (gainedPath)
                    pm.SendMessage("You have gained a path in {0}!", virtueName);
                else
                    pm.SendMessage("You have gained in {0}.", virtueName);

                if (virtue == VirtueName.Compassion)
                {
                    pm.NextCompassionDay = DateTime.UtcNow + TimeSpan.FromDays(1.0);
                    ++pm.CompassionGains;

                    if (pm.CompassionGains >= 5)
                        pm.SendLocalizedMessage(1053004); // You must wait about a day before you can gain in compassion again.
                }
            }
            else
            {
                // TODO: Localize?
                pm.SendMessage("You have achieved the highest path of {0} and can no longer gain any further.", virtueName);
            }
        }
    }

    public delegate void OnVirtueUsed(Mobile from);

    public class VirtueGump : Gump
    {
        private static Dictionary<int, OnVirtueUsed> m_Callbacks = new Dictionary<int, OnVirtueUsed>();

        public static void Initialize()
        {
            EventSink.VirtueGumpRequest += new VirtueGumpRequestEventHandler(EventSink_VirtueGumpRequest);
            EventSink.VirtueItemRequest += new VirtueItemRequestEventHandler(EventSink_VirtueItemRequest);
            EventSink.VirtueMacroRequest += new VirtueMacroRequestEventHandler(EventSink_VirtueMacroRequest);
        }

        public static void Register(int gumpID, OnVirtueUsed callback)
        {
            m_Callbacks[gumpID] = callback;
        }

        private static void EventSink_VirtueItemRequest(VirtueItemRequestEventArgs e)
        {
            if (e.Beholder != e.Beheld)
                return;

            e.Beholder.CloseGump(typeof(VirtueGump));

            if (e.Beholder.Kills >= 5)
            {
                e.Beholder.SendLocalizedMessage(1049609); // Murderers cannot invoke this virtue.
                return;
            }

            OnVirtueUsed callback = null;

            m_Callbacks.TryGetValue(e.GumpID, out callback);

            if (callback != null)
                callback(e.Beholder);
            else
                e.Beholder.SendLocalizedMessage(1052066); // That virtue is not active yet.
        }


        private static void EventSink_VirtueMacroRequest(VirtueMacroRequestEventArgs e)
        {
            int virtueID = 0;

            switch (e.VirtueID)
            {
                case 0:	// Honor
                    virtueID = 107; break;
                case 1:	// Sacrifice
                    virtueID = 110; break;
                case 2:	// Valor;
                    virtueID = 112; break;
            }

            EventSink_VirtueItemRequest(new VirtueItemRequestEventArgs(e.Mobile, e.Mobile, virtueID));
        }

        private static void EventSink_VirtueGumpRequest(VirtueGumpRequestEventArgs e)
        {
            Mobile beholder = e.Beholder;
            Mobile beheld = e.Beheld;

            if (beholder == beheld && beholder.Kills >= 5)
            {
                beholder.SendLocalizedMessage(1049609); // Murderers cannot invoke this virtue.
            }
            else if (beholder.Map == beheld.Map && beholder.InRange(beheld, 12))
            {
                beholder.CloseGump(typeof(VirtueGump));
                beholder.SendGump(new VirtueGump(beholder, beheld));
            }
        }

        private Mobile m_Beholder, m_Beheld;

        public VirtueGump(Mobile beholder, Mobile beheld)
            : base(0, 0)
        {
            m_Beholder = beholder;
            m_Beheld = beheld;

            Serial = beheld.Serial;

            AddPage(0);

            AddImage(30, 40, 104);

            AddPage(1);

            Add(new InternalEntry(61, 71, 108, GetHueFor(0))); // Humility
            Add(new InternalEntry(123, 46, 112, GetHueFor(4))); // Valor
            Add(new InternalEntry(187, 70, 107, GetHueFor(5))); // Honor
            Add(new InternalEntry(35, 135, 110, GetHueFor(1))); // Sacrifice
            Add(new InternalEntry(211, 133, 105, GetHueFor(2))); // Compassion
            Add(new InternalEntry(61, 195, 111, GetHueFor(3))); // Spiritulaity
            Add(new InternalEntry(186, 195, 109, GetHueFor(6))); // Justice
            Add(new InternalEntry(121, 221, 106, GetHueFor(7))); // Honesty

            if (m_Beholder == m_Beheld)
            {
                AddButton(57, 269, 2027, 2027, 1, GumpButtonType.Reply, 0);
                AddButton(186, 269, 2071, 2071, 2, GumpButtonType.Reply, 0);
            }
        }

        private static int[] m_Table = new int[24]
			{
				0x0481, 0x0963, 0x0965,
				0x060A, 0x060F, 0x002A,
				0x08A4, 0x08A7, 0x0034,
				0x0965, 0x08FD, 0x0480,
				0x00EA, 0x0845, 0x0020,
				0x0011, 0x0269, 0x013D,
				0x08A1, 0x08A3, 0x0042,
				0x0543, 0x0547, 0x0061
			};

        private int GetHueFor(int index)
        {
            if (m_Beheld.Virtues.GetValue(index) == 0)
                return 2402;

            int value = m_Beheld.Virtues.GetValue(index);

            if (value < 4000)
                return 2402;

            if (value >= 30000)
                value = 20000;	//Sanity


            int vl;

            if (value < 10000)
                vl = 0;
            else if (value >= 20000 && index == 5)
                vl = 2;
            else if (value >= 21000 && index != 1)
                vl = 2;
            else if (value >= 22000 && index == 1)
                vl = 2;
            else
                vl = 1;


            return m_Table[(index * 3) + (int)vl];
        }

        private class InternalEntry : GumpImage
        {
            public InternalEntry(int x, int y, int gumpID, int hue)
                : base(x, y, gumpID, hue)
            {
            }

            public override string Compile()
            {
                return String.Format("{{ gumppic {0} {1} {2} hue={3} class=VirtueGumpItem }}", X, Y, GumpID, Hue);
            }

            private static byte[] m_Class = Gump.StringToBuffer(" class=VirtueGumpItem");

            public override void AppendTo(IGumpWriter disp)
            {
                base.AppendTo(disp);

                disp.AppendLayout(m_Class);
            }
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            if (info.ButtonID == 1 && m_Beholder == m_Beheld)
                m_Beholder.SendGump(new VirtueStatusGump(m_Beholder));
        }
    }

    public class VirtueStatusGump : Gump
    {
        private Mobile m_Beholder;

        public VirtueStatusGump(Mobile beholder)
            : base(0, 0)
        {
            m_Beholder = beholder;

            AddPage(0);

            AddImage(30, 40, 2080);
            AddImage(47, 77, 2081);
            AddImage(47, 147, 2081);
            AddImage(47, 217, 2081);
            AddImage(47, 267, 2083);
            AddImage(70, 213, 2091);

            AddPage(1);

            AddHtml(140, 73, 200, 20, "The Virtues", false, false);

            AddHtmlLocalized(80, 100, 100, 40, 1051000, false, false); // Humility
            AddHtmlLocalized(80, 129, 100, 40, 1051001, false, false); // Sacrifice
            AddHtmlLocalized(80, 159, 100, 40, 1051002, false, false); // Compassion
            AddHtmlLocalized(80, 189, 100, 40, 1051003, false, false); // Spirituality
            AddHtmlLocalized(200, 100, 200, 40, 1051004, false, false); // Valor
            AddHtmlLocalized(200, 129, 200, 40, 1051005, false, false); // Honor
            AddHtmlLocalized(200, 159, 200, 40, 1051006, false, false); // Justice
            AddHtmlLocalized(200, 189, 200, 40, 1051007, false, false); // Honesty

            AddHtmlLocalized(75, 224, 220, 60, 1052062, false, false); // Click on a blue gem to view your status in that virtue.

            AddButton(60, 100, 1210, 1210, 1, GumpButtonType.Reply, 0);
            AddButton(60, 129, 1210, 1210, 2, GumpButtonType.Reply, 0);
            AddButton(60, 159, 1210, 1210, 3, GumpButtonType.Reply, 0);
            AddButton(60, 189, 1210, 1210, 4, GumpButtonType.Reply, 0);
            AddButton(180, 100, 1210, 1210, 5, GumpButtonType.Reply, 0);
            AddButton(180, 129, 1210, 1210, 6, GumpButtonType.Reply, 0);
            AddButton(180, 159, 1210, 1210, 7, GumpButtonType.Reply, 0);
            AddButton(180, 189, 1210, 1210, 8, GumpButtonType.Reply, 0);

            AddButton(280, 43, 4014, 4014, 9, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            switch (info.ButtonID)
            {
                case 1:
                    {
                        m_Beholder.SendGump(new VirtueInfoGump(m_Beholder, VirtueName.Humility, 1052051));
                        break;
                    }
                case 2:
                    {
                        m_Beholder.SendGump(new VirtueInfoGump(m_Beholder, VirtueName.Sacrifice, 1052053, @"http://update.uo.com/design_389.html"));
                        break;
                    }
                case 3:
                    {
                        m_Beholder.SendGump(new VirtueInfoGump(m_Beholder, VirtueName.Compassion, 1053000, @"http://update.uo.com/design_412.html"));
                        break;
                    }
                case 4:
                    {
                        m_Beholder.SendGump(new VirtueInfoGump(m_Beholder, VirtueName.Spirituality, 1052056));
                        break;
                    }
                case 5:
                    {
                        m_Beholder.SendGump(new VirtueInfoGump(m_Beholder, VirtueName.Valor, 1054033, @"http://update.uo.com/design_427.html"));
                        break;
                    }
                case 6:
                    {
                        m_Beholder.SendGump(new VirtueInfoGump(m_Beholder, VirtueName.Honor, 1052058, @"http://guide.uo.com/virtues_2.html"));
                        break;
                    }
                case 7:
                    {
                        m_Beholder.SendGump(new VirtueInfoGump(m_Beholder, VirtueName.Justice, 1052059, @"http://update.uo.com/design_413.html"));
                        break;
                    }
                case 8:
                    {
                        m_Beholder.SendGump(new VirtueInfoGump(m_Beholder, VirtueName.Honesty, 1052060));
                        break;
                    }
                case 9:
                    {
                        m_Beholder.SendGump(new VirtueGump(m_Beholder, m_Beholder));
                        break;
                    }
            }
        }
    }

    public class VirtueInfoGump : Gump
    {
        private Mobile m_Beholder;
        private int m_Desc;
        private string m_Page;
        private VirtueName m_Virtue;

        public VirtueInfoGump(Mobile beholder, VirtueName virtue, int description)
            : this(beholder, virtue, description, null)
        {
        }

        public VirtueInfoGump(Mobile beholder, VirtueName virtue, int description, string webPage)
            : base(0, 0)
        {
            m_Beholder = beholder;
            m_Virtue = virtue;
            m_Desc = description;
            m_Page = webPage;

            int value = beholder.Virtues.GetValue((int)virtue);

            AddPage(0);

            AddImage(30, 40, 2080);
            AddImage(47, 77, 2081);
            AddImage(47, 147, 2081);
            AddImage(47, 217, 2081);
            AddImage(47, 267, 2083);
            AddImage(70, 213, 2091);

            AddPage(1);

            int maxValue = VirtueHelper.GetMaxAmount(m_Virtue);

            int valueDesc;
            int dots;

            if (value < 4000)
                dots = value / 400;
            else if (value < 10000)
                dots = (value - 4000) / 600;
            else if (value < maxValue)
                dots = (value - 10000) / ((maxValue - 10000) / 10);
            else
                dots = 10;

            for (int i = 0; i < 10; ++i)
                AddImage(95 + (i * 17), 50, i < dots ? 2362 : 2360);


            if (value < 1)
                valueDesc = 1052044; // You have not started on the path of this Virtue.
            else if (value < 400)
                valueDesc = 1052045; // You have barely begun your journey through the path of this Virtue.
            else if (value < 2000)
                valueDesc = 1052046; // You have progressed in this Virtue, but still have much to do.
            else if (value < 3600)
                valueDesc = 1052047; // Your journey through the path of this Virtue is going well.
            else if (value < 4000)
                valueDesc = 1052048; // You feel very close to achieving your next path in this Virtue.
            else if (dots < 1)
                valueDesc = 1052049; // You have achieved a path in this Virtue.
            else if (dots < 9)
                valueDesc = 1052047; // Your journey through the path of this Virtue is going well.
            else if (dots < 10)
                valueDesc = 1052048; // You feel very close to achieving your next path in this Virtue.
            else
                valueDesc = 1052050; // You have achieved the highest path in this Virtue.


            AddHtmlLocalized(157, 73, 200, 40, 1051000 + (int)virtue, false, false);
            AddHtmlLocalized(75, 95, 220, 140, description, false, false);
            AddHtmlLocalized(70, 224, 229, 60, valueDesc, false, false);

            AddButton(65, 277, 1209, 1209, 1, GumpButtonType.Reply, 0);

            AddButton(280, 43, 4014, 4014, 2, GumpButtonType.Reply, 0);

            AddHtmlLocalized(83, 275, 400, 40, (webPage == null) ? 1052055 : 1052052, false, false); // This virtue is not yet defined. OR -click to learn more (opens webpage)


        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            switch (info.ButtonID)
            {
                case 1:
                    {
                        m_Beholder.SendGump(new VirtueInfoGump(m_Beholder, m_Virtue, m_Desc, m_Page));

                        if (m_Page != null)
                            state.Send(new LaunchBrowser(m_Page)); //No message about web browser starting on OSI
                        break;
                    }
                case 2:
                    {
                        m_Beholder.SendGump(new VirtueStatusGump(m_Beholder));
                        break;
                    }
            }
        }
    }
}