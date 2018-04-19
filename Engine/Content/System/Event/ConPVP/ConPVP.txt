using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using Server;
using Server.Commands;
using Server.ContextMenus;
using Server.Engines.PartySystem;
using Server.Ethics;
using Server.Ethics.Evil;
using Server.Ethics.Hero;
using Server.Factions;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Regions;
using Server.Spells;
using Server.Spells.Bushido;
using Server.Spells.Chivalry;
using Server.Spells.Necromancy;
using Server.Spells.Ninjitsu;
using Server.Spells.Seventh;
using Server.Spells.Spellweaving;
using Server.Targeting;

namespace Server.Engines.ConPVP
{
    public class PreferencesController : Item
    {
        private Preferences m_Preferences;

        //[CommandProperty( AccessLevel.GameMaster )]
        public Preferences Preferences { get { return m_Preferences; } set { } }

        public override string DefaultName
        {
            get { return "preferences controller"; }
        }

        [Constructable]
        public PreferencesController()
            : base(0x1B7A)
        {
            Visible = false;
            Movable = false;

            m_Preferences = new Preferences();

            if (Preferences.Instance == null)
                Preferences.Instance = m_Preferences;
            else
                Delete();
        }

        public override void Delete()
        {
            if (Preferences.Instance != m_Preferences)
                base.Delete();
        }

        public PreferencesController(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);

            m_Preferences.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_Preferences = new Preferences(reader);
                        Preferences.Instance = m_Preferences;
                        break;
                    }
            }
        }
    }

    public class Preferences
    {
        private ArrayList m_Entries;
        private Hashtable m_Table;

        public ArrayList Entries { get { return m_Entries; } }

        public PreferencesEntry Find(Mobile mob)
        {
            PreferencesEntry entry = (PreferencesEntry)m_Table[mob];

            if (entry == null)
            {
                m_Table[mob] = entry = new PreferencesEntry(mob, this);
                m_Entries.Add(entry);
            }

            return entry;
        }

        private static Preferences m_Instance;

        public static Preferences Instance { get { return m_Instance; } set { m_Instance = value; } }

        public Preferences()
        {
            m_Table = new Hashtable();
            m_Entries = new ArrayList();
        }

        public Preferences(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            switch (version)
            {
                case 0:
                    {
                        int count = reader.ReadEncodedInt();

                        m_Table = new Hashtable(count);
                        m_Entries = new ArrayList(count);

                        for (int i = 0; i < count; ++i)
                        {
                            PreferencesEntry entry = new PreferencesEntry(reader, this, version);

                            if (entry.Mobile != null)
                            {
                                m_Table[entry.Mobile] = entry;
                                m_Entries.Add(entry);
                            }
                        }

                        break;
                    }
            }
        }

        public void Serialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)0); // version;

            writer.WriteEncodedInt((int)m_Entries.Count);

            for (int i = 0; i < m_Entries.Count; ++i)
                ((PreferencesEntry)m_Entries[i]).Serialize(writer);
        }
    }

    public class PreferencesEntry
    {
        private Mobile m_Mobile;
        private ArrayList m_Disliked;
        private Preferences m_Preferences;

        public Mobile Mobile { get { return m_Mobile; } }
        public ArrayList Disliked { get { return m_Disliked; } }

        public PreferencesEntry(Mobile mob, Preferences prefs)
        {
            m_Preferences = prefs;
            m_Mobile = mob;
            m_Disliked = new ArrayList();
        }

        public PreferencesEntry(GenericReader reader, Preferences prefs, int version)
        {
            m_Preferences = prefs;

            switch (version)
            {
                case 0:
                    {
                        m_Mobile = reader.ReadMobile();

                        int count = reader.ReadEncodedInt();

                        m_Disliked = new ArrayList(count);

                        for (int i = 0; i < count; ++i)
                            m_Disliked.Add(reader.ReadString());

                        break;
                    }
            }
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write((Mobile)m_Mobile);

            writer.WriteEncodedInt((int)m_Disliked.Count);

            for (int i = 0; i < m_Disliked.Count; ++i)
                writer.Write((string)m_Disliked[i]);
        }
    }

    public class PreferencesGump : Gump
    {
        private Mobile m_From;
        private PreferencesEntry m_Entry;

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (m_Entry == null)
                return;

            if (info.ButtonID != 1)
                return;

            m_Entry.Disliked.Clear();

            List<Arena> arenas = Arena.Arenas;

            for (int i = 0; i < info.Switches.Length; ++i)
            {
                int idx = info.Switches[i];

                if (idx >= 0 && idx < arenas.Count)
                    m_Entry.Disliked.Add(arenas[idx].Name);
            }
        }

        public PreferencesGump(Mobile from, Preferences prefs)
            : base(50, 50)
        {
            m_From = from;
            m_Entry = prefs.Find(from);

            if (m_Entry == null)
                return;

            List<Arena> arenas = Arena.Arenas;

            AddPage(0);

            int height = 12 + 20 + (arenas.Count * 31) + 24 + 12;

            AddBackground(0, 0, 499 + 40 - 365, height, 0x2436);

            for (int i = 1; i < arenas.Count; i += 2)
                AddImageTiled(12, 32 + (i * 31), 475 + 40 - 365, 30, 0x2430);

            AddAlphaRegion(10, 10, 479 + 40 - 365, height - 20);

            AddColumnHeader(35, null);
            AddColumnHeader(115, "Arena");

            AddButton(499 + 40 - 365 - 12 - 63 - 4 - 63, height - 12 - 24, 247, 248, 1, GumpButtonType.Reply, 0);
            AddButton(499 + 40 - 365 - 12 - 63, height - 12 - 24, 241, 242, 2, GumpButtonType.Reply, 0);

            for (int i = 0; i < arenas.Count; ++i)
            {
                Arena ar = arenas[i];

                string name = ar.Name;

                if (name == null)
                    name = "(no name)";

                int x = 12;
                int y = 32 + (i * 31);

                int color = 0xCCFFCC;

                AddCheck(x + 3, y + 1, 9730, 9727, m_Entry.Disliked.Contains(name), i);
                x += 35;

                AddBorderedText(x + 5, y + 5, 115 - 5, name, color, 0);
                x += 115;
            }
        }

        public string Center(string text)
        {
            return String.Format("<CENTER>{0}</CENTER>", text);
        }

        public string Color(string text, int color)
        {
            return String.Format("<BASEFONT COLOR=#{0:X6}>{1}</BASEFONT>", color, text);
        }

        private void AddBorderedText(int x, int y, int width, string text, int color, int borderColor)
        {
            /*AddColoredText( x - 1, y, width, text, borderColor );
            AddColoredText( x + 1, y, width, text, borderColor );
            AddColoredText( x, y - 1, width, text, borderColor );
            AddColoredText( x, y + 1, width, text, borderColor );*/
            /*AddColoredText( x - 1, y - 1, width, text, borderColor );
            AddColoredText( x + 1, y + 1, width, text, borderColor );*/
            AddColoredText(x, y, width, text, color);
        }

        private void AddColoredText(int x, int y, int width, string text, int color)
        {
            if (color == 0)
                AddHtml(x, y, width, 20, text, false, false);
            else
                AddHtml(x, y, width, 20, Color(text, color), false, false);
        }

        private int m_ColumnX = 12;

        private void AddColumnHeader(int width, string name)
        {
            AddBackground(m_ColumnX, 12, width, 20, 0x242C);
            AddImageTiled(m_ColumnX + 2, 14, width - 4, 16, 0x2430);

            if (name != null)
                AddBorderedText(m_ColumnX, 13, width, Center(name), 0xFFFFFF, 0);

            m_ColumnX += width;
        }
    }

    #region Event Duel Participant

    public class Participant
    {
        private DuelContext m_Context;
        private DuelPlayer[] m_Players;
        private TournyParticipant m_TournyPart;

        public int Count { get { return m_Players.Length; } }
        public DuelPlayer[] Players { get { return m_Players; } }
        public DuelContext Context { get { return m_Context; } }
        public TournyParticipant TournyPart { get { return m_TournyPart; } set { m_TournyPart = value; } }

        public DuelPlayer Find(Mobile mob)
        {
            if (mob is PlayerMobile)
            {
                PlayerMobile pm = (PlayerMobile)mob;

                if (pm.DuelContext == m_Context && pm.DuelPlayer.Participant == this)
                    return pm.DuelPlayer;

                return null;
            }

            for (int i = 0; i < m_Players.Length; ++i)
            {
                if (m_Players[i] != null && m_Players[i].Mobile == mob)
                    return m_Players[i];
            }

            return null;
        }

        public bool Contains(Mobile mob)
        {
            return (Find(mob) != null);
        }

        public void Broadcast(int hue, string message, string nonLocalOverhead, string localOverhead)
        {
            for (int i = 0; i < m_Players.Length; ++i)
            {
                if (m_Players[i] != null)
                {
                    if (message != null)
                        m_Players[i].Mobile.SendMessage(hue, message);

                    if (nonLocalOverhead != null)
                        m_Players[i].Mobile.NonlocalOverheadMessage(Network.MessageType.Regular, hue, false, String.Format(nonLocalOverhead, m_Players[i].Mobile.Name, m_Players[i].Mobile.Female ? "her" : "his"));

                    if (localOverhead != null)
                        m_Players[i].Mobile.LocalOverheadMessage(Network.MessageType.Regular, hue, false, localOverhead);
                }
            }
        }

        public int FilledSlots
        {
            get
            {
                int count = 0;

                for (int i = 0; i < m_Players.Length; ++i)
                {
                    if (m_Players[i] != null)
                        ++count;
                }

                return count;
            }
        }

        public bool HasOpenSlot
        {
            get
            {
                for (int i = 0; i < m_Players.Length; ++i)
                {
                    if (m_Players[i] == null)
                        return true;
                }

                return false;
            }
        }

        public bool Eliminated
        {
            get
            {
                for (int i = 0; i < m_Players.Length; ++i)
                {
                    if (m_Players[i] != null && !m_Players[i].Eliminated)
                        return false;
                }

                return true;
            }
        }

        public string NameList
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < m_Players.Length; ++i)
                {
                    if (m_Players[i] == null)
                        continue;

                    Mobile mob = m_Players[i].Mobile;

                    if (sb.Length > 0)
                        sb.Append(", ");

                    sb.Append(mob.Name);
                }

                if (sb.Length == 0)
                    return "Empty";

                return sb.ToString();
            }
        }

        public void Nullify(DuelPlayer player)
        {
            if (player == null)
                return;

            int index = Array.IndexOf(m_Players, player);

            if (index == -1)
                return;

            m_Players[index] = null;
        }

        public void Remove(DuelPlayer player)
        {
            if (player == null)
                return;

            int index = Array.IndexOf(m_Players, player);

            if (index == -1)
                return;

            DuelPlayer[] old = m_Players;
            m_Players = new DuelPlayer[old.Length - 1];

            for (int i = 0; i < index; ++i)
                m_Players[i] = old[i];

            for (int i = index + 1; i < old.Length; ++i)
                m_Players[i - 1] = old[i];
        }

        public void Remove(Mobile player)
        {
            Remove(Find(player));
        }

        public void Add(Mobile player)
        {
            if (Contains(player))
                return;

            for (int i = 0; i < m_Players.Length; ++i)
            {
                if (m_Players[i] == null)
                {
                    m_Players[i] = new DuelPlayer(player, this);
                    return;
                }
            }

            Resize(m_Players.Length + 1);
            m_Players[m_Players.Length - 1] = new DuelPlayer(player, this);
        }

        public void Resize(int count)
        {
            DuelPlayer[] old = m_Players;
            m_Players = new DuelPlayer[count];

            if (old != null)
            {
                int ct = 0;

                for (int i = 0; i < old.Length; ++i)
                {
                    if (old[i] != null && ct < count)
                        m_Players[ct++] = old[i];
                }
            }
        }

        public Participant(DuelContext context, int count)
        {
            m_Context = context;
            //m_Stakes = new StakesContainer( context, this );
            Resize(count);
        }
    }

    public class ParticipantGump : Gump
    {
        private Mobile m_From;
        private DuelContext m_Context;
        private Participant m_Participant;

        public Mobile From { get { return m_From; } }
        public DuelContext Context { get { return m_Context; } }
        public Participant Participant { get { return m_Participant; } }

        public string Center(string text)
        {
            return String.Format("<CENTER>{0}</CENTER>", text);
        }

        public void AddGoldenButton(int x, int y, int bid)
        {
            AddButton(x, y, 0xD2, 0xD2, bid, GumpButtonType.Reply, 0);
            AddButton(x + 3, y + 3, 0xD8, 0xD8, bid, GumpButtonType.Reply, 0);
        }

        public void AddGoldenButtonLabeled(int x, int y, int bid, string text)
        {
            AddGoldenButton(x, y, bid);
            AddHtml(x + 25, y, 200, 20, text, false, false);
        }

        public ParticipantGump(Mobile from, DuelContext context, Participant p)
            : base(50, 50)
        {
            m_From = from;
            m_Context = context;
            m_Participant = p;

            from.CloseGump(typeof(RulesetGump));
            from.CloseGump(typeof(DuelContextGump));
            from.CloseGump(typeof(ParticipantGump));

            int count = p.Players.Length;

            if (count < 4)
                count = 4;

            AddPage(0);

            int height = 35 + 10 + 22 + 22 + 30 + 22 + 2 + (count * 22) + 2 + 30;

            AddBackground(0, 0, 300, height, 9250);
            AddBackground(10, 10, 280, height - 20, 0xDAC);

            AddButton(240, 25, 0xFB1, 0xFB3, 3, GumpButtonType.Reply, 0);

            //AddButton( 223, 54, 0x265A, 0x265A, 4, GumpButtonType.Reply, 0 );

            AddHtml(35, 25, 230, 20, Center("Participant Setup"), false, false);

            int x = 35;
            int y = 47;

            AddHtml(x, y, 200, 20, String.Format("Team Size: {0}", p.Players.Length), false, false); y += 22;

            AddGoldenButtonLabeled(x + 20, y, 1, "Increase"); y += 22;
            AddGoldenButtonLabeled(x + 20, y, 2, "Decrease"); y += 30;

            AddHtml(35, y, 230, 20, Center("Players"), false, false); y += 22;

            for (int i = 0; i < p.Players.Length; ++i)
            {
                DuelPlayer pl = p.Players[i];

                AddGoldenButtonLabeled(x, y, 5 + i, String.Format("{0}: {1}", 1 + i, pl == null ? "Empty" : pl.Mobile.Name)); y += 22;
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (!m_Context.Registered)
                return;

            int bid = info.ButtonID;

            if (bid == 0)
            {
                m_From.SendGump(new DuelContextGump(m_From, m_Context));
            }
            else if (bid == 1)
            {
                if (m_Participant.Count < 8)
                    m_Participant.Resize(m_Participant.Count + 1);
                else
                    m_From.SendMessage("You may not raise the team size any further.");

                m_From.SendGump(new ParticipantGump(m_From, m_Context, m_Participant));
            }
            else if (bid == 2)
            {
                if (m_Participant.Count > 1 && m_Participant.Count > m_Participant.FilledSlots)
                    m_Participant.Resize(m_Participant.Count - 1);
                else
                    m_From.SendMessage("You may not lower the team size any further.");

                m_From.SendGump(new ParticipantGump(m_From, m_Context, m_Participant));
            }
            else if (bid == 3)
            {
                if (m_Participant.FilledSlots > 0)
                {
                    m_From.SendMessage("There is at least one currently active player. You must remove them first.");
                    m_From.SendGump(new ParticipantGump(m_From, m_Context, m_Participant));
                }
                else if (m_Context.Participants.Count > 2)
                {
                    /*Container cont = m_Participant.Stakes;

                    if ( cont != null )
                        cont.Delete();*/

                    m_Context.Participants.Remove(m_Participant);
                    m_From.SendGump(new DuelContextGump(m_From, m_Context));
                }
                else
                {
                    m_From.SendMessage("Duels must have at least two participating parties.");
                    m_From.SendGump(new ParticipantGump(m_From, m_Context, m_Participant));
                }
            }
            /*else if ( bid == 4 )
            {
                m_From.SendGump( new ParticipantGump( m_From, m_Context, m_Participant ) );

                Container cont = m_Participant.Stakes;

                if ( cont != null && !cont.Deleted )
                {
                    cont.DisplayTo( m_From );

                    Item[] checks = cont.FindItemsByType( typeof( BankCheck ) );

                    int gold = cont.TotalGold;

                    for ( int i = 0; i < checks.Length; ++i )
                        gold += ((BankCheck)checks[i]).Worth;

                    m_From.SendMessage( "This container has {0} item{1} and {2} stone{3}. In gold or check form there is a total of {4:D}gp.", cont.TotalItems, cont.TotalItems==1?"":"s", cont.TotalWeight, cont.TotalWeight==1?"":"s", gold );
                }
            }*/
            else
            {
                bid -= 5;

                if (bid >= 0 && bid < m_Participant.Players.Length)
                {
                    if (m_Participant.Players[bid] == null)
                    {
                        m_From.Target = new ParticipantTarget(m_Context, m_Participant, bid);
                        m_From.SendMessage("Target a player.");
                    }
                    else
                    {
                        m_Participant.Players[bid].Mobile.SendMessage("You have been removed from the duel.");

                        if (m_Participant.Players[bid].Mobile is PlayerMobile)
                            ((PlayerMobile)(m_Participant.Players[bid].Mobile)).DuelPlayer = null;

                        m_Participant.Players[bid] = null;
                        m_From.SendMessage("They have been removed from the duel.");
                        m_From.SendGump(new ParticipantGump(m_From, m_Context, m_Participant));
                    }
                }
            }
        }

        private class ParticipantTarget : Target
        {
            private DuelContext m_Context;
            private Participant m_Participant;
            private int m_Index;

            public ParticipantTarget(DuelContext context, Participant p, int index)
                : base(12, false, TargetFlags.None)
            {
                m_Context = context;
                m_Participant = p;
                m_Index = index;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (!m_Context.Registered)
                    return;

                int index = m_Index;

                if (index < 0 || index >= m_Participant.Players.Length)
                    return;

                Mobile mob = targeted as Mobile;

                if (mob == null)
                {
                    from.SendMessage("That is not a player.");
                }
                else if (!mob.Player)
                {
                    if (mob.Body.IsHuman)
                        mob.SayTo(from, 1005443); // Nay, I would rather stay here and watch a nail rust.
                    else
                        mob.SayTo(from, 1005444); // The creature ignores your offer.
                }
                else if (AcceptDuelGump.IsIgnored(mob, from) || mob.Blessed)
                {
                    from.SendMessage("They ignore your offer.");
                }
                else
                {
                    PlayerMobile pm = mob as PlayerMobile;

                    if (pm == null)
                        return;

                    if (pm.DuelContext != null)
                        from.SendMessage("{0} cannot fight because they are already assigned to another duel.", pm.Name);
                    else if (DuelContext.CheckCombat(pm))
                        from.SendMessage("{0} cannot fight because they have recently been in combat with another player.", pm.Name);
                    else if (mob.HasGump(typeof(AcceptDuelGump)))
                        from.SendMessage("{0} has already been offered a duel.");
                    else
                    {
                        from.SendMessage("You send {0} to {1}.", m_Participant.Find(from) == null ? "a challenge" : "an invitation", mob.Name);
                        mob.SendGump(new AcceptDuelGump(from, mob, m_Context, m_Participant, m_Index));
                    }
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                from.SendGump(new ParticipantGump(from, m_Context, m_Participant));
            }
        }
    }

    public class DuelPlayer
    {
        private Mobile m_Mobile;
        private bool m_Eliminated;
        private bool m_Ready;
        private Participant m_Participant;

        public Mobile Mobile { get { return m_Mobile; } }
        public bool Ready { get { return m_Ready; } set { m_Ready = value; } }
        public bool Eliminated { get { return m_Eliminated; } set { m_Eliminated = value; if (m_Participant.Context.m_Tournament != null && m_Eliminated) { m_Participant.Context.m_Tournament.OnEliminated(this); m_Mobile.SendEverything(); } } }
        public Participant Participant { get { return m_Participant; } set { m_Participant = value; } }

        public DuelPlayer(Mobile mob, Participant p)
        {
            m_Mobile = mob;
            m_Participant = p;

            if (mob is PlayerMobile)
                ((PlayerMobile)mob).DuelPlayer = this;
        }
    }

    #endregion

    #region Event Arena Controller

    public class ArenaController : Item
    {
        private Arena m_Arena;
        private bool m_IsPrivate;

        [CommandProperty(AccessLevel.GameMaster)]
        public Arena Arena { get { return m_Arena; } set { } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsPrivate { get { return m_IsPrivate; } set { m_IsPrivate = value; } }

        public override string DefaultName
        {
            get { return "arena controller"; }
        }

        [Constructable]
        public ArenaController()
            : base(0x1B7A)
        {
            Visible = false;
            Movable = false;

            m_Arena = new Arena();

            m_Instances.Add(this);
        }

        public override void OnDelete()
        {
            base.OnDelete();

            m_Instances.Remove(this);
            m_Arena.Delete();
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.AccessLevel >= AccessLevel.GameMaster)
                from.SendGump(new Gumps.PropertiesGump(from, m_Arena));
        }

        public ArenaController(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1);

            writer.Write((bool)m_IsPrivate);

            m_Arena.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                    {
                        m_IsPrivate = reader.ReadBool();

                        goto case 0;
                    }
                case 0:
                    {
                        m_Arena = new Arena(reader);
                        break;
                    }
            }

            m_Instances.Add(this);
        }

        private static List<ArenaController> m_Instances = new List<ArenaController>();

        public static List<ArenaController> Instances { get { return m_Instances; } set { m_Instances = value; } }
    }

    [PropertyObject]
    public class ArenaStartPoints
    {
        private Point3D[] m_Points;

        public Point3D[] Points { get { return m_Points; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D EdgeWest { get { return m_Points[0]; } set { m_Points[0] = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D EdgeEast { get { return m_Points[1]; } set { m_Points[1] = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D EdgeNorth { get { return m_Points[2]; } set { m_Points[2] = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D EdgeSouth { get { return m_Points[3]; } set { m_Points[3] = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D CornerNW { get { return m_Points[4]; } set { m_Points[4] = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D CornerSE { get { return m_Points[5]; } set { m_Points[5] = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D CornerSW { get { return m_Points[6]; } set { m_Points[6] = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D CornerNE { get { return m_Points[7]; } set { m_Points[7] = value; } }

        public override string ToString()
        {
            return "...";
        }

        public ArenaStartPoints()
            : this(new Point3D[8])
        {
        }

        public ArenaStartPoints(Point3D[] points)
        {
            m_Points = points;
        }

        public ArenaStartPoints(GenericReader reader)
        {
            m_Points = new Point3D[reader.ReadEncodedInt()];

            for (int i = 0; i < m_Points.Length; ++i)
                m_Points[i] = reader.ReadPoint3D();
        }

        public void Serialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)m_Points.Length);

            for (int i = 0; i < m_Points.Length; ++i)
                writer.Write((Point3D)m_Points[i]);
        }
    }

    [PropertyObject]
    public class Arena : IComparable
    {
        private Map m_Facet;
        private Rectangle2D m_Bounds;
        private Rectangle2D m_Zone;
        private Point3D m_Outside;
        private Point3D m_Wall;
        private Point3D m_GateIn;
        private Point3D m_GateOut;
        private ArenaStartPoints m_Points;
        private bool m_Active;
        private string m_Name;

        private bool m_IsGuarded;

        private Item m_Teleporter;

        private List<Mobile> m_Players;

        private TournamentController m_Tournament;
        private Mobile m_Announcer;

        private LadderController m_Ladder;

        [CommandProperty(AccessLevel.GameMaster)]
        public LadderController Ladder
        {
            get { return m_Ladder; }
            set { m_Ladder = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsGuarded
        {
            get { return m_IsGuarded; }
            set
            {
                m_IsGuarded = value;

                if (m_Region != null)
                    m_Region.Disabled = !m_IsGuarded;
            }
        }

        public Ladder AcquireLadder()
        {
            if (m_Ladder != null)
                return m_Ladder.Ladder;

            return Server.Engines.ConPVP.Ladder.Instance;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TournamentController Tournament
        {
            get { return m_Tournament; }
            set
            {
                if (m_Tournament != null)
                    m_Tournament.Tournament.Arenas.Remove(this);

                m_Tournament = value;

                if (m_Tournament != null)
                    m_Tournament.Tournament.Arenas.Add(this);
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Announcer
        {
            get { return m_Announcer; }
            set { m_Announcer = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; if (m_Active) m_Arenas.Sort(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Map Facet
        {
            get { return m_Facet; }
            set
            {
                m_Facet = value;

                if (m_Teleporter != null)
                    m_Teleporter.Map = value;

                if (m_Region != null)
                    m_Region.Unregister();

                if (m_Zone.Start != Point2D.Zero && m_Zone.End != Point2D.Zero && m_Facet != null)
                    m_Region = new SafeZone(m_Zone, m_Outside, m_Facet, m_IsGuarded);
                else
                    m_Region = null;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Rectangle2D Bounds { get { return m_Bounds; } set { m_Bounds = value; } }

        private SafeZone m_Region;

        public int Spectators
        {
            get
            {
                if (m_Region == null)
                    return 0;

                int specs = m_Region.GetPlayerCount() - m_Players.Count;

                if (specs < 0)
                    specs = 0;

                return specs;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Rectangle2D Zone
        {
            get { return m_Zone; }
            set
            {
                m_Zone = value;

                if (m_Zone.Start != Point2D.Zero && m_Zone.End != Point2D.Zero && m_Facet != null)
                {
                    if (m_Region != null)
                        m_Region.Unregister();

                    m_Region = new SafeZone(m_Zone, m_Outside, m_Facet, m_IsGuarded);
                }
                else
                {
                    if (m_Region != null)
                        m_Region.Unregister();

                    m_Region = null;
                }
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D Outside { get { return m_Outside; } set { m_Outside = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D GateIn { get { return m_GateIn; } set { m_GateIn = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D GateOut { get { return m_GateOut; } set { m_GateOut = value; if (m_Teleporter != null) m_Teleporter.Location = m_GateOut; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D Wall { get { return m_Wall; } set { m_Wall = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsOccupied { get { return (m_Players.Count > 0); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public ArenaStartPoints Points { get { return m_Points; } set { } }

        public Item Teleporter { get { return m_Teleporter; } set { m_Teleporter = value; } }

        public List<Mobile> Players { get { return m_Players; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Active
        {
            get { return m_Active; }
            set
            {
                if (m_Active == value)
                    return;

                m_Active = value;

                if (m_Active)
                {
                    m_Arenas.Add(this);
                    m_Arenas.Sort();
                }
                else
                {
                    m_Arenas.Remove(this);
                }
            }
        }

        public void Delete()
        {
            Active = false;

            if (m_Region != null)
                m_Region.Unregister();

            m_Region = null;
        }

        public override string ToString()
        {
            return "...";
        }

        public Point3D GetBaseStartPoint(int index)
        {
            if (index < 0)
                index = 0;

            return m_Points.Points[index % m_Points.Points.Length];
        }

        #region Offsets & Rotation
        private static Point2D[] m_EdgeOffsets = new Point2D[]
			{
				/*
				 *        /\
				 *       /\/\
				 *      /\/\/\
				 *      \/\/\/
				 *       \/\/\
				 *        \/\/
				 */
				new Point2D( 0, 0 ),
				new Point2D( 0, -1 ),
				new Point2D( 0, +1 ),
				new Point2D( 1, 0 ),
				new Point2D( 1, -1 ),
				new Point2D( 1, +1 ),
				new Point2D( 2, 0 ),
				new Point2D( 2, -1 ),
				new Point2D( 2, +1 ),
				new Point2D( 3, 0 )
			};

        // nw corner
        private static Point2D[] m_CornerOffsets = new Point2D[]
			{
				/*
				 *         /\
				 *        /\/\
				 *       /\/\/\
				 *      /\/\/\/\
				 *      \/\/\/\/
				 */
				new Point2D( 0, 0 ),
				new Point2D( 0, 1 ),
				new Point2D( 1, 0 ),
				new Point2D( 1, 1 ),
				new Point2D( 0, 2 ),
				new Point2D( 2, 0 ),
				new Point2D( 2, 1 ),
				new Point2D( 1, 2 ),
				new Point2D( 0, 3 ),
				new Point2D( 3, 0 )
			};

        private static int[][,] m_Rotate = new int[][,]
			{
				new int[,]{ { +1, 0 }, { 0, +1 } }, // west
				new int[,]{ { -1, 0 }, { 0, -1 } }, // east
				new int[,]{ { 0, +1 }, { +1, 0 } }, // north
				new int[,]{ { 0, -1 }, { -1, 0 } }, // south
				new int[,]{ { +1, 0 }, { 0, +1 } }, // nw
				new int[,]{ { -1, 0 }, { 0, -1 } }, // se
				new int[,]{ { 0, +1 }, { +1, 0 } }, // sw
				new int[,]{ { 0, -1 }, { -1, 0 } }, // ne
			};
        #endregion

        public void MoveInside(DuelPlayer[] players, int index)
        {
            if (index < 0)
                index = 0;
            else
                index %= m_Points.Points.Length;

            Point3D start = GetBaseStartPoint(index);

            int offset = 0;

            Point2D[] offsets = (index < 4) ? m_EdgeOffsets : m_CornerOffsets;
            int[,] matrix = m_Rotate[index];

            for (int i = 0; i < players.Length; ++i)
            {
                DuelPlayer pl = players[i];

                if (pl == null)
                    continue;

                Mobile mob = pl.Mobile;

                Point2D p;

                if (offset < offsets.Length)
                    p = offsets[offset++];
                else
                    p = offsets[offsets.Length - 1];

                p.X = (p.X * matrix[0, 0]) + (p.Y * matrix[0, 1]);
                p.Y = (p.X * matrix[1, 0]) + (p.Y * matrix[1, 1]);

                mob.MoveToWorld(new Point3D(start.X + p.X, start.Y + p.Y, start.Z), m_Facet);
                mob.Direction = mob.GetDirectionTo(m_Wall);

                m_Players.Add(mob);
            }
        }

        public Arena()
        {
            m_Points = new ArenaStartPoints();
            m_Players = new List<Mobile>();
        }

        public Arena(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            switch (version)
            {
                case 7:
                    {
                        m_IsGuarded = reader.ReadBool();

                        goto case 6;
                    }
                case 6:
                    {
                        m_Ladder = reader.ReadItem() as LadderController;

                        goto case 5;
                    }
                case 5:
                    {
                        m_Tournament = reader.ReadItem() as TournamentController;
                        m_Announcer = reader.ReadMobile();

                        goto case 4;
                    }
                case 4:
                    {
                        m_Name = reader.ReadString();

                        goto case 3;
                    }
                case 3:
                    {
                        m_Zone = reader.ReadRect2D();

                        goto case 2;
                    }
                case 2:
                    {
                        m_GateIn = reader.ReadPoint3D();
                        m_GateOut = reader.ReadPoint3D();
                        m_Teleporter = reader.ReadItem();

                        goto case 1;
                    }
                case 1:
                    {
                        m_Players = reader.ReadStrongMobileList();

                        goto case 0;
                    }
                case 0:
                    {
                        m_Facet = reader.ReadMap();
                        m_Bounds = reader.ReadRect2D();
                        m_Outside = reader.ReadPoint3D();
                        m_Wall = reader.ReadPoint3D();

                        if (version == 0)
                        {
                            reader.ReadBool();
                            m_Players = new List<Mobile>();
                        }

                        m_Active = reader.ReadBool();
                        m_Points = new ArenaStartPoints(reader);

                        if (m_Active)
                        {
                            m_Arenas.Add(this);
                            m_Arenas.Sort();
                        }

                        break;
                    }
            }

            if (m_Zone.Start != Point2D.Zero && m_Zone.End != Point2D.Zero && m_Facet != null)
                m_Region = new SafeZone(m_Zone, m_Outside, m_Facet, m_IsGuarded);

            if (IsOccupied)
                Timer.DelayCall(TimeSpan.FromSeconds(2.0), new TimerCallback(Evict));

            if (m_Tournament != null)
                Timer.DelayCall(TimeSpan.Zero, new TimerCallback(AttachToTournament_Sandbox));
        }

        private void AttachToTournament_Sandbox()
        {
            if (m_Tournament != null)
                m_Tournament.Tournament.Arenas.Add(this);
        }

        [CommandProperty(AccessLevel.Administrator, AccessLevel.Administrator)]
        public bool ForceEvict { get { return false; } set { if (value) Evict(); } }

        public void Evict()
        {
            Point3D loc;
            Map facet;

            if (m_Facet == null)
            {
                loc = new Point3D(2715, 2165, 0);
                facet = Map.Felucca;
            }
            else
            {
                loc = m_Outside;
                facet = m_Facet;
            }

            bool hasBounds = (m_Bounds.Start != Point2D.Zero && m_Bounds.End != Point2D.Zero);

            for (int i = 0; i < m_Players.Count; ++i)
            {
                Mobile mob = m_Players[i];

                if (mob == null)
                    continue;

                if (mob.Map == Map.Internal)
                {
                    if ((m_Facet == null || mob.LogoutMap == m_Facet) && (!hasBounds || m_Bounds.Contains(mob.LogoutLocation)))
                        mob.LogoutLocation = loc;
                }
                else if ((m_Facet == null || mob.Map == m_Facet) && (!hasBounds || m_Bounds.Contains(mob.Location)))
                {
                    mob.MoveToWorld(loc, facet);
                }

                mob.Combatant = null;
                mob.Frozen = false;
                DuelContext.Debuff(mob);
                DuelContext.CancelSpell(mob);
            }

            if (hasBounds)
            {
                List<Mobile> pets = new List<Mobile>();

                foreach (Mobile mob in facet.GetMobilesInBounds(m_Bounds))
                {
                    BaseCreature pet = mob as BaseCreature;

                    if (pet != null && pet.Controlled && pet.ControlMaster != null)
                    {
                        if (m_Players.Contains(pet.ControlMaster))
                        {
                            pets.Add(pet);
                        }
                    }
                }

                foreach (Mobile pet in pets)
                {
                    pet.Combatant = null;
                    pet.Frozen = false;

                    pet.MoveToWorld(loc, facet);
                }
            }

            m_Players.Clear();
        }

        public void Serialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)7);

            writer.Write((bool)m_IsGuarded);

            writer.Write((Item)m_Ladder);

            writer.Write((Item)m_Tournament);
            writer.Write((Mobile)m_Announcer);

            writer.Write((string)m_Name);

            writer.Write((Rectangle2D)m_Zone);

            writer.Write((Point3D)m_GateIn);
            writer.Write((Point3D)m_GateOut);
            writer.Write((Item)m_Teleporter);

            writer.Write(m_Players);

            writer.Write((Map)m_Facet);
            writer.Write((Rectangle2D)m_Bounds);
            writer.Write((Point3D)m_Outside);
            writer.Write((Point3D)m_Wall);
            writer.Write((bool)m_Active);

            m_Points.Serialize(writer);
        }

        private static List<Arena> m_Arenas = new List<Arena>();

        public static List<Arena> Arenas { get { return m_Arenas; } }

        public static Arena FindArena(List<Mobile> players)
        {
            Preferences prefs = Preferences.Instance;

            if (prefs == null)
                return FindArena();

            if (m_Arenas.Count == 0)
                return null;

            if (players.Count > 0)
            {
                Mobile first = players[0];

                List<ArenaController> allControllers = ArenaController.Instances;

                for (int i = 0; i < allControllers.Count; ++i)
                {
                    ArenaController controller = allControllers[i];

                    if (controller != null && !controller.Deleted && controller.Arena != null && controller.IsPrivate && controller.Map == first.Map && first.InRange(controller, 24))
                    {
                        Multis.BaseHouse house = Multis.BaseHouse.FindHouseAt(controller);
                        bool allNear = true;

                        for (int j = 0; j < players.Count; ++j)
                        {
                            Mobile check = players[j];
                            bool isNear;

                            if (house == null)
                                isNear = (controller.Map == check.Map && check.InRange(controller, 24));
                            else
                                isNear = (Multis.BaseHouse.FindHouseAt(check) == house);

                            if (!isNear)
                            {
                                allNear = false;
                                break;
                            }
                        }

                        if (allNear)
                            return controller.Arena;
                    }
                }
            }

            List<ArenaEntry> arenas = new List<ArenaEntry>();

            for (int i = 0; i < m_Arenas.Count; ++i)
            {
                Arena arena = m_Arenas[i];

                if (!arena.IsOccupied)
                    arenas.Add(new ArenaEntry(arena));
            }

            if (arenas.Count == 0)
                return m_Arenas[0];

            int tc = 0;

            for (int i = 0; i < arenas.Count; ++i)
            {
                ArenaEntry ae = arenas[i];

                for (int j = 0; j < players.Count; ++j)
                {
                    PreferencesEntry pe = prefs.Find(players[j]);

                    if (pe.Disliked.Contains(ae.m_Arena.Name))
                        ++ae.m_VotesAgainst;
                    else
                        ++ae.m_VotesFor;
                }

                tc += ae.Value;
            }

            int rn = Utility.Random(tc);

            for (int i = 0; i < arenas.Count; ++i)
            {
                ArenaEntry ae = arenas[i];

                if (rn < ae.Value)
                    return ae.m_Arena;

                rn -= ae.Value;
            }

            return arenas[Utility.Random(arenas.Count)].m_Arena;
        }

        private class ArenaEntry
        {
            public Arena m_Arena;
            public int m_VotesFor;
            public int m_VotesAgainst;

            public int Value
            {
                get
                {
                    return m_VotesFor;

                    /*if ( m_VotesFor > m_VotesAgainst )
                        return m_VotesFor - m_VotesAgainst;
                    else if ( m_VotesFor > 0 )
                        return 1;
                    else
                        return 0;*/
                }
            }

            public ArenaEntry(Arena arena)
            {
                m_Arena = arena;
            }
        }

        public static Arena FindArena()
        {
            if (m_Arenas.Count == 0)
                return null;

            int offset = Utility.Random(m_Arenas.Count);

            for (int i = 0; i < m_Arenas.Count; ++i)
            {
                Arena arena = m_Arenas[(i + offset) % m_Arenas.Count];

                if (!arena.IsOccupied)
                    return arena;
            }

            return m_Arenas[offset];
        }

        public int CompareTo(object obj)
        {
            Arena c = (Arena)obj;

            string a = m_Name;
            string b = c.m_Name;

            if (a == null && b == null)
                return 0;
            else if (a == null)
                return -1;
            else if (b == null)
                return +1;

            return a.CompareTo(b);
        }
    }

    public class ArenasMoongate : Item
    {
        public override string DefaultName
        {
            get { return "arena moongate"; }
        }

        [Constructable]
        public ArenasMoongate()
            : base(0x1FD4)
        {
            Movable = false;
            Light = LightType.Circle300;
        }

        public ArenasMoongate(Serial serial)
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
            Light = LightType.Circle300;
        }

        public bool UseGate(Mobile from)
        {
            if (DuelContext.CheckCombat(from))
            {
                from.SendMessage(0x22, "You have recently been in combat with another player and cannot use this moongate.");
                return false;
            }
            else if (from.Spell != null)
            {
                from.SendLocalizedMessage(1049616); // You are too busy to do that at the moment.
                return false;
            }
            else
            {
                from.CloseGump(typeof(ArenaGump));
                from.SendGump(new ArenaGump(from, this));

                if (!from.Hidden || from.AccessLevel == AccessLevel.Player)
                    Effects.PlaySound(from.Location, from.Map, 0x20E);

                return true;
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.InRange(GetWorldLocation(), 1))
                UseGate(from);
            else
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that
        }

        public override bool OnMoveOver(Mobile m)
        {
            return (!m.Player || UseGate(m));
        }
    }

    public class ArenaGump : Gump
    {
        private Mobile m_From;
        private ArenasMoongate m_Gate;
        private List<Arena> m_Arenas;

        private void Append(StringBuilder sb, LadderEntry le)
        {
            if (le == null)
                return;

            if (sb.Length > 0)
                sb.Append(", ");

            sb.Append(le.Mobile.Name);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID != 1)
                return;

            int[] switches = info.Switches;

            if (switches.Length == 0)
                return;

            int opt = switches[0];

            if (opt < 0 || opt >= m_Arenas.Count)
                return;

            Arena arena = m_Arenas[opt];

            if (!m_From.InRange(m_Gate.GetWorldLocation(), 1) || m_From.Map != m_Gate.Map)
            {
                m_From.SendLocalizedMessage(1019002); // You are too far away to use the gate.
            }
            else if (DuelContext.CheckCombat(m_From))
            {
                m_From.SendMessage(0x22, "You have recently been in combat with another player and cannot use this moongate.");
            }
            else if (m_From.Spell != null)
            {
                m_From.SendLocalizedMessage(1049616); // You are too busy to do that at the moment.
            }
            else if (m_From.Map == arena.Facet && arena.Zone.Contains(m_From))
            {
                m_From.SendLocalizedMessage(1019003); // You are already there.
            }
            else
            {
                BaseCreature.TeleportPets(m_From, arena.GateIn, arena.Facet);

                m_From.Combatant = null;
                m_From.Warmode = false;
                m_From.Hidden = true;

                m_From.MoveToWorld(arena.GateIn, arena.Facet);

                Effects.PlaySound(arena.GateIn, arena.Facet, 0x1FE);
            }
        }

        public ArenaGump(Mobile from, ArenasMoongate gate)
            : base(50, 50)
        {
            m_From = from;
            m_Gate = gate;
            m_Arenas = Arena.Arenas;

            AddPage(0);

            int height = 12 + 20 + (m_Arenas.Count * 31) + 24 + 12;

            AddBackground(0, 0, 499 + 40, height, 0x2436);

            List<Arena> list = m_Arenas;

            for (int i = 1; i < list.Count; i += 2)
                AddImageTiled(12, 32 + (i * 31), 475 + 40, 30, 0x2430);

            AddAlphaRegion(10, 10, 479 + 40, height - 20);

            AddColumnHeader(35, null);
            AddColumnHeader(115, "Arena");
            AddColumnHeader(325, "Participants");
            AddColumnHeader(40, "Obs");

            AddButton(499 + 40 - 12 - 63 - 4 - 63, height - 12 - 24, 247, 248, 1, GumpButtonType.Reply, 0);
            AddButton(499 + 40 - 12 - 63, height - 12 - 24, 241, 242, 2, GumpButtonType.Reply, 0);

            for (int i = 0; i < list.Count; ++i)
            {
                Arena ar = list[i];

                string name = ar.Name;

                if (name == null)
                    name = "(no name)";

                int x = 12;
                int y = 32 + (i * 31);

                int color = (ar.Players.Count > 0 ? 0xCCFFCC : 0xCCCCCC);

                AddRadio(x + 3, y + 1, 9727, 9730, false, i);
                x += 35;

                AddBorderedText(x + 5, y + 5, 115 - 5, name, color, 0);
                x += 115;

                StringBuilder sb = new StringBuilder();

                if (ar.Players.Count > 0)
                {
                    Ladder ladder = Ladder.Instance;

                    if (ladder == null)
                        continue;

                    LadderEntry p1 = null, p2 = null, p3 = null, p4 = null;

                    for (int j = 0; j < ar.Players.Count; ++j)
                    {
                        Mobile mob = (Mobile)ar.Players[j];
                        LadderEntry c = ladder.Find(mob);

                        if (p1 == null || c.Index < p1.Index)
                        {
                            p4 = p3;
                            p3 = p2;
                            p2 = p1;
                            p1 = c;
                        }
                        else if (p2 == null || c.Index < p2.Index)
                        {
                            p4 = p3;
                            p3 = p2;
                            p2 = c;
                        }
                        else if (p3 == null || c.Index < p3.Index)
                        {
                            p4 = p3;
                            p3 = c;
                        }
                        else if (p4 == null || c.Index < p4.Index)
                        {
                            p4 = c;
                        }
                    }

                    Append(sb, p1);
                    Append(sb, p2);
                    Append(sb, p3);
                    Append(sb, p4);

                    if (ar.Players.Count > 4)
                        sb.Append(", ...");
                }
                else
                {
                    sb.Append("Empty");
                }

                AddBorderedText(x + 5, y + 5, 325 - 5, sb.ToString(), color, 0);
                x += 325;

                AddBorderedText(x, y + 5, 40, Center(ar.Spectators.ToString()), color, 0);
            }
        }

        public string Center(string text)
        {
            return String.Format("<CENTER>{0}</CENTER>", text);
        }

        public string Color(string text, int color)
        {
            return String.Format("<BASEFONT COLOR=#{0:X6}>{1}</BASEFONT>", color, text);
        }

        private void AddBorderedText(int x, int y, int width, string text, int color, int borderColor)
        {
            /*AddColoredText( x - 1, y, width, text, borderColor );
            AddColoredText( x + 1, y, width, text, borderColor );
            AddColoredText( x, y - 1, width, text, borderColor );
            AddColoredText( x, y + 1, width, text, borderColor );*/
            /*AddColoredText( x - 1, y - 1, width, text, borderColor );
            AddColoredText( x + 1, y + 1, width, text, borderColor );*/
            AddColoredText(x, y, width, text, color);
        }

        private void AddColoredText(int x, int y, int width, string text, int color)
        {
            if (color == 0)
                AddHtml(x, y, width, 20, text, false, false);
            else
                AddHtml(x, y, width, 20, Color(text, color), false, false);
        }

        private int m_ColumnX = 12;

        private void AddColumnHeader(int width, string name)
        {
            AddBackground(m_ColumnX, 12, width, 20, 0x242C);
            AddImageTiled(m_ColumnX + 2, 14, width - 4, 16, 0x2430);

            if (name != null)
                AddBorderedText(m_ColumnX, 13, width, Center(name), 0xFFFFFF, 0);

            m_ColumnX += width;
        }
    }

    #endregion

    public class AcceptDuelGump : Gump
    {
        private Mobile m_Challenger, m_Challenged;
        private DuelContext m_Context;
        private Participant m_Participant;
        private int m_Slot;

        public string Center(string text)
        {
            return String.Format("<CENTER>{0}</CENTER>", text);
        }

        public string Color(string text, int color)
        {
            return String.Format("<BASEFONT COLOR=#{0:X6}>{1}</BASEFONT>", color, text);
        }

        private const int LabelColor32 = 0xFFFFFF;
        private const int BlackColor32 = 0x000008;

        private bool m_Active = true;

        public AcceptDuelGump(Mobile challenger, Mobile challenged, DuelContext context, Participant p, int slot)
            : base(50, 50)
        {
            m_Challenger = challenger;
            m_Challenged = challenged;
            m_Context = context;
            m_Participant = p;
            m_Slot = slot;

            challenged.CloseGump(typeof(AcceptDuelGump));

            Closable = false;

            AddPage(0);

            //AddBackground( 0, 0, 400, 220, 9150 );
            AddBackground(1, 1, 398, 218, 3600);
            //AddBackground( 16, 15, 369, 189, 9100 );

            AddImageTiled(16, 15, 369, 189, 3604);
            AddAlphaRegion(16, 15, 369, 189);

            AddImage(215, -43, 0xEE40);
            //AddImage( 330, 141, 0x8BA );

            AddHtml(22 - 1, 22, 294, 20, Color(Center("Duel Challenge"), BlackColor32), false, false);
            AddHtml(22 + 1, 22, 294, 20, Color(Center("Duel Challenge"), BlackColor32), false, false);
            AddHtml(22, 22 - 1, 294, 20, Color(Center("Duel Challenge"), BlackColor32), false, false);
            AddHtml(22, 22 + 1, 294, 20, Color(Center("Duel Challenge"), BlackColor32), false, false);
            AddHtml(22, 22, 294, 20, Color(Center("Duel Challenge"), LabelColor32), false, false);

            string fmt;

            if (p.Contains(challenger))
                fmt = "You have been asked to join sides with {0} in a duel. Do you accept?";
            else
                fmt = "You have been challenged to a duel from {0}. Do you accept?";

            AddHtml(22 - 1, 50, 294, 40, Color(String.Format(fmt, challenger.Name), BlackColor32), false, false);
            AddHtml(22 + 1, 50, 294, 40, Color(String.Format(fmt, challenger.Name), BlackColor32), false, false);
            AddHtml(22, 50 - 1, 294, 40, Color(String.Format(fmt, challenger.Name), BlackColor32), false, false);
            AddHtml(22, 50 + 1, 294, 40, Color(String.Format(fmt, challenger.Name), BlackColor32), false, false);
            AddHtml(22, 50, 294, 40, Color(String.Format(fmt, challenger.Name), 0xB0C868), false, false);

            AddImageTiled(32, 88, 264, 1, 9107);
            AddImageTiled(42, 90, 264, 1, 9157);

            AddRadio(24, 100, 9727, 9730, true, 1);
            AddHtml(60 - 1, 105, 250, 20, Color("Yes, I will fight this duel.", BlackColor32), false, false);
            AddHtml(60 + 1, 105, 250, 20, Color("Yes, I will fight this duel.", BlackColor32), false, false);
            AddHtml(60, 105 - 1, 250, 20, Color("Yes, I will fight this duel.", BlackColor32), false, false);
            AddHtml(60, 105 + 1, 250, 20, Color("Yes, I will fight this duel.", BlackColor32), false, false);
            AddHtml(60, 105, 250, 20, Color("Yes, I will fight this duel.", LabelColor32), false, false);

            AddRadio(24, 135, 9727, 9730, false, 2);
            AddHtml(60 - 1, 140, 250, 20, Color("No, I do not wish to fight.", BlackColor32), false, false);
            AddHtml(60 + 1, 140, 250, 20, Color("No, I do not wish to fight.", BlackColor32), false, false);
            AddHtml(60, 140 - 1, 250, 20, Color("No, I do not wish to fight.", BlackColor32), false, false);
            AddHtml(60, 140 + 1, 250, 20, Color("No, I do not wish to fight.", BlackColor32), false, false);
            AddHtml(60, 140, 250, 20, Color("No, I do not wish to fight.", LabelColor32), false, false);

            AddRadio(24, 170, 9727, 9730, false, 3);
            AddHtml(60 - 1, 175, 250, 20, Color("No, knave. Do not ask again.", BlackColor32), false, false);
            AddHtml(60 + 1, 175, 250, 20, Color("No, knave. Do not ask again.", BlackColor32), false, false);
            AddHtml(60, 175 - 1, 250, 20, Color("No, knave. Do not ask again.", BlackColor32), false, false);
            AddHtml(60, 175 + 1, 250, 20, Color("No, knave. Do not ask again.", BlackColor32), false, false);
            AddHtml(60, 175, 250, 20, Color("No, knave. Do not ask again.", LabelColor32), false, false);

            AddButton(314, 173, 247, 248, 1, GumpButtonType.Reply, 0);

            Timer.DelayCall(TimeSpan.FromSeconds(15.0), new TimerCallback(AutoReject));
        }

        public void AutoReject()
        {
            if (!m_Active)
                return;

            m_Active = false;

            m_Challenged.CloseGump(typeof(AcceptDuelGump));

            m_Challenger.SendMessage("{0} seems unresponsive.", m_Challenged.Name);
            m_Challenged.SendMessage("You decline the challenge.");
        }

        private static Hashtable m_IgnoreLists = new Hashtable();

        private class IgnoreEntry
        {
            public Mobile m_Ignored;
            public DateTime m_Expire;

            public Mobile Ignored { get { return m_Ignored; } }
            public bool Expired { get { return (DateTime.UtcNow >= m_Expire); } }

            private static TimeSpan ExpireDelay = TimeSpan.FromMinutes(15.0);

            public void Refresh()
            {
                m_Expire = DateTime.UtcNow + ExpireDelay;
            }

            public IgnoreEntry(Mobile ignored)
            {
                m_Ignored = ignored;
                Refresh();
            }
        }

        public static void BeginIgnore(Mobile source, Mobile toIgnore)
        {
            ArrayList list = (ArrayList)m_IgnoreLists[source];

            if (list == null)
                m_IgnoreLists[source] = list = new ArrayList();

            for (int i = 0; i < list.Count; ++i)
            {
                IgnoreEntry ie = (IgnoreEntry)list[i];

                if (ie.Ignored == toIgnore)
                {
                    ie.Refresh();
                    return;
                }
                else if (ie.Expired)
                {
                    list.RemoveAt(i--);
                }
            }

            list.Add(new IgnoreEntry(toIgnore));
        }

        public static bool IsIgnored(Mobile source, Mobile check)
        {
            ArrayList list = (ArrayList)m_IgnoreLists[source];

            if (list == null)
                return false;

            for (int i = 0; i < list.Count; ++i)
            {
                IgnoreEntry ie = (IgnoreEntry)list[i];

                if (ie.Expired)
                    list.RemoveAt(i--);
                else if (ie.Ignored == check)
                    return true;
            }

            return false;
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID != 1 || !m_Active || !m_Context.Registered)
                return;

            m_Active = false;

            if (!m_Context.Participants.Contains(m_Participant))
                return;

            if (info.IsSwitched(1))
            {
                PlayerMobile pm = m_Challenged as PlayerMobile;

                if (pm == null)
                    return;

                if (pm.DuelContext != null)
                {
                    if (pm.DuelContext.Initiator == pm)
                        pm.SendMessage(0x22, "You have already started a duel.");
                    else
                        pm.SendMessage(0x22, "You have already been challenged in a duel.");

                    m_Challenger.SendMessage("{0} cannot fight because they are already assigned to another duel.", pm.Name);
                }
                else if (DuelContext.CheckCombat(pm))
                {
                    pm.SendMessage(0x22, "You have recently been in combat with another player and must wait before starting a duel.");
                    m_Challenger.SendMessage("{0} cannot fight because they have recently been in combat with another player.", pm.Name);
                }
                else if (TournamentController.IsActive)
                {
                    pm.SendMessage(0x22, "A tournament is currently active and you may not duel.");
                    m_Challenger.SendMessage(0x22, "A tournament is currently active and you may not duel.");
                }
                else
                {
                    bool added = false;

                    if (m_Slot >= 0 && m_Slot < m_Participant.Players.Length && m_Participant.Players[m_Slot] == null)
                    {
                        added = true;
                        m_Participant.Players[m_Slot] = new DuelPlayer(m_Challenged, m_Participant);
                    }
                    else
                    {
                        for (int i = 0; i < m_Participant.Players.Length; ++i)
                        {
                            if (m_Participant.Players[i] == null)
                            {
                                added = true;
                                m_Participant.Players[i] = new DuelPlayer(m_Challenged, m_Participant);
                                break;
                            }
                        }
                    }

                    if (added)
                    {
                        m_Challenger.SendMessage("{0} has accepted the request.", m_Challenged.Name);
                        m_Challenged.SendMessage("You have accepted the request from {0}.", m_Challenger.Name);

                        NetState ns = m_Challenger.NetState;

                        if (ns != null)
                        {
                            foreach (Gump g in ns.Gumps)
                            {
                                if (g is ParticipantGump)
                                {
                                    ParticipantGump pg = (ParticipantGump)g;

                                    if (pg.Participant == m_Participant)
                                    {
                                        m_Challenger.SendGump(new ParticipantGump(m_Challenger, m_Context, m_Participant));
                                        break;
                                    }
                                }
                                else if (g is DuelContextGump)
                                {
                                    DuelContextGump dcg = (DuelContextGump)g;

                                    if (dcg.Context == m_Context)
                                    {
                                        m_Challenger.SendGump(new DuelContextGump(m_Challenger, m_Context));
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        m_Challenger.SendMessage("The participant list was full and so {0} could not join.", m_Challenged.Name);
                        m_Challenged.SendMessage("The participant list was full and so you could not join the fight {1} {0}.", m_Challenger.Name, m_Participant.Contains(m_Challenger) ? "with" : "against");
                    }
                }
            }
            else
            {
                if (info.IsSwitched(3))
                    BeginIgnore(m_Challenged, m_Challenger);

                m_Challenger.SendMessage("{0} does not wish to fight.", m_Challenged.Name);
                m_Challenged.SendMessage("You chose not to fight {1} {0}.", m_Challenger.Name, m_Participant.Contains(m_Challenger) ? "with" : "against");
            }
        }
    }

    public delegate void CountdownCallback(int count);

    public class DuelContext
    {
        private Mobile m_Initiator;
        private ArrayList m_Participants;
        private Ruleset m_Ruleset;
        private Arena m_Arena;
        private bool m_Registered = true;
        private bool m_Finished, m_Started;

        private bool m_ReadyWait;
        private int m_ReadyCount;

        private bool m_Rematch;

        public bool Rematch { get { return m_Rematch; } }

        public bool ReadyWait { get { return m_ReadyWait; } }
        public int ReadyCount { get { return m_ReadyCount; } }

        public bool Registered { get { return m_Registered; } }
        public bool Finished { get { return m_Finished; } }
        public bool Started { get { return m_Started; } }

        public Mobile Initiator { get { return m_Initiator; } }
        public ArrayList Participants { get { return m_Participants; } }
        public Ruleset Ruleset { get { return m_Ruleset; } }
        public Arena Arena { get { return m_Arena; } }

        private bool CantDoAnything(Mobile mob)
        {
            if (m_EventGame != null)
                return m_EventGame.CantDoAnything(mob);
            else
                return false;
        }

        public static bool IsFreeConsume(Mobile mob)
        {
            PlayerMobile pm = mob as PlayerMobile;

            if (pm == null || pm.DuelContext == null || pm.DuelContext.m_EventGame == null)
                return false;

            return pm.DuelContext.m_EventGame.FreeConsume;
        }

        public void DelayBounce(TimeSpan ts, Mobile mob, Container corpse)
        {
            Timer.DelayCall(ts, new TimerStateCallback(DelayBounce_Callback), new object[] { mob, corpse });
        }

        public static bool AllowSpecialMove(Mobile from, string name, SpecialMove move)
        {
            PlayerMobile pm = from as PlayerMobile;

            if (pm == null)
                return true;

            DuelContext dc = pm.DuelContext;

            return (dc == null || dc.InstAllowSpecialMove(from, name, move));
        }

        public bool InstAllowSpecialMove(Mobile from, string name, SpecialMove move)
        {

            if (!m_StartedBeginCountdown)
                return true;

            DuelPlayer pl = Find(from);

            if (pl == null || pl.Eliminated)
                return true;

            if (CantDoAnything(from))
                return false;

            string title = null;

            if (move is NinjaMove)
                title = "Bushido";
            else if (move is SamuraiMove)
                title = "Ninjitsu";


            if (title == null || name == null || m_Ruleset.GetOption(title, name))
                return true;

            from.SendMessage("The dueling ruleset prevents you from using this move.");
            return false;
        }

        public bool AllowSpellCast(Mobile from, Spell spell)
        {
            if (!m_StartedBeginCountdown)
                return true;

            DuelPlayer pl = Find(from);

            if (pl == null || pl.Eliminated)
                return true;

            if (CantDoAnything(from))
                return false;

            if (spell is Server.Spells.Fourth.RecallSpell)
                from.SendMessage("You may not cast this spell.");

            string title = null, option = null;

            if (spell is ArcanistSpell)
            {
                title = "Spellweaving";
                option = spell.Name;
            }
            else if (spell is PaladinSpell)
            {
                title = "Chivalry";
                option = spell.Name;
            }
            else if (spell is NecromancerSpell)
            {
                title = "Necromancy";
                option = spell.Name;
            }
            else if (spell is NinjaSpell)
            {
                title = "Ninjitsu";
                option = spell.Name;
            }
            else if (spell is SamuraiSpell)
            {
                title = "Bushido";
                option = spell.Name;
            }
            else if (spell is MagerySpell)
            {
                switch (((MagerySpell)spell).Circle)
                {
                    case SpellCircle.First: title = "1st Circle"; break;
                    case SpellCircle.Second: title = "2nd Circle"; break;
                    case SpellCircle.Third: title = "3rd Circle"; break;
                    case SpellCircle.Fourth: title = "4th Circle"; break;
                    case SpellCircle.Fifth: title = "5th Circle"; break;
                    case SpellCircle.Sixth: title = "6th Circle"; break;
                    case SpellCircle.Seventh: title = "7th Circle"; break;
                    case SpellCircle.Eighth: title = "8th Circle"; break;
                }

                option = spell.Name;
            }
            else
            {
                title = "Other Spell";
                option = spell.Name;
            }

            if (title == null || option == null || m_Ruleset.GetOption(title, option))
                return true;

            from.SendMessage("The dueling ruleset prevents you from casting this spell.");
            return false;
        }

        public bool AllowItemEquip(Mobile from, Item item)
        {
            if (!m_StartedBeginCountdown)
                return true;

            DuelPlayer pl = Find(from);

            if (pl == null || pl.Eliminated)
                return true;

            if (item is Dagger || CheckItemEquip(from, item))
                return true;

            from.SendMessage("The dueling ruleset prevents you from equiping this item.");
            return false;
        }

        public static bool AllowSpecialAbility(Mobile from, string name, bool message)
        {
            PlayerMobile pm = from as PlayerMobile;

            if (pm == null)
                return true;

            DuelContext dc = pm.DuelContext;

            return (dc == null || dc.InstAllowSpecialAbility(from, name, message));
        }

        public bool InstAllowSpecialAbility(Mobile from, string name, bool message)
        {
            if (!m_StartedBeginCountdown)
                return true;

            DuelPlayer pl = Find(from);

            if (pl == null || pl.Eliminated)
                return true;

            if (CantDoAnything(from))
                return false;

            if (m_Ruleset.GetOption("Combat Abilities", name))
                return true;

            if (message)
                from.SendMessage("The dueling ruleset prevents you from using this combat ability.");

            return false;
        }

        public bool CheckItemEquip(Mobile from, Item item)
        {
            if (item is Fists)
            {
                if (!m_Ruleset.GetOption("Weapons", "Wrestling"))
                    return false;
            }
            else if (item is BaseArmor)
            {
                BaseArmor armor = (BaseArmor)item;

                if (armor.ProtectionLevel > ArmorProtectionLevel.Regular && !m_Ruleset.GetOption("Armor", "Magical"))
                    return false;

                if (!Core.AOS && armor.Resource != armor.DefaultResource && !m_Ruleset.GetOption("Armor", "Colored"))
                    return false;

                if (armor is BaseShield && !m_Ruleset.GetOption("Armor", "Shields"))
                    return false;
            }
            else if (item is BaseWeapon)
            {
                BaseWeapon weapon = (BaseWeapon)item;

                if ((weapon.DamageLevel > WeaponDamageLevel.Regular || weapon.AccuracyLevel > WeaponAccuracyLevel.Regular) && !m_Ruleset.GetOption("Weapons", "Magical"))
                    return false;

                if (!Core.AOS && weapon.Resource != CraftResource.Iron && weapon.Resource != CraftResource.None && !m_Ruleset.GetOption("Weapons", "Runics"))
                    return false;

                if (weapon is BaseRanged && !m_Ruleset.GetOption("Weapons", "Ranged"))
                    return false;

                if (!(weapon is BaseRanged) && !m_Ruleset.GetOption("Weapons", "Melee"))
                    return false;

                if (weapon.PoisonCharges > 0 && weapon.Poison != null && !m_Ruleset.GetOption("Weapons", "Poisoned"))
                    return false;

                if (weapon is BaseWand && !m_Ruleset.GetOption("Items", "Wands"))
                    return false;
            }

            return true;
        }

        public bool AllowSkillUse(Mobile from, SkillName skill)
        {
            if (!m_StartedBeginCountdown)
                return true;

            DuelPlayer pl = Find(from);

            if (pl == null || pl.Eliminated)
                return true;

            if (CantDoAnything(from))
                return false;

            int id = (int)skill;

            if (id >= 0 && id < SkillInfo.Table.Length)
            {
                if (m_Ruleset.GetOption("Skills", SkillInfo.Table[id].Name))
                    return true;
            }

            from.SendMessage("The dueling ruleset prevents you from using this skill.");
            return false;
        }

        public bool AllowItemUse(Mobile from, Item item)
        {
            if (!m_StartedBeginCountdown)
                return true;

            DuelPlayer pl = Find(from);

            if (pl == null || pl.Eliminated)
                return true;

            if (!(item is BaseRefreshPotion))
            {
                if (CantDoAnything(from))
                    return false;
            }

            string title = null, option = null;

            if (item is BasePotion)
            {
                title = "Potions";

                if (item is BaseAgilityPotion)
                    option = "Agility";
                else if (item is BaseCurePotion)
                    option = "Cure";
                else if (item is BaseHealPotion)
                    option = "Heal";
                else if (item is NightSightPotion)
                    option = "Nightsight";
                else if (item is BasePoisonPotion)
                    option = "Poison";
                else if (item is BaseStrengthPotion)
                    option = "Strength";
                else if (item is BaseExplosionPotion)
                    option = "Explosion";
                else if (item is BaseRefreshPotion)
                    option = "Refresh";
            }
            else if (item is Bandage)
            {
                title = "Items";
                option = "Bandages";
            }
            else if (item is TrapableContainer)
            {
                if (((TrapableContainer)item).TrapType != TrapType.None)
                {
                    title = "Items";
                    option = "Trapped Containers";
                }
            }
            else if (item is Bola)
            {
                title = "Items";
                option = "Bolas";
            }
            else if (item is OrangePetals)
            {
                title = "Items";
                option = "Orange Petals";
            }
            else if (item is EtherealMount || item.Layer == Layer.Mount)
            {
                title = "Items";
                option = "Mounts";
            }
            else if (item is LeatherNinjaBelt)
            {
                title = "Items";
                option = "Shurikens";
            }
            else if (item is Fukiya)
            {
                title = "Items";
                option = "Fukiya Darts";
            }
            else if (item is FireHorn)
            {
                title = "Items";
                option = "Fire Horns";
            }
            else if (item is BaseWand)
            {
                title = "Items";
                option = "Wands";
            }

            if (title != null && option != null && m_StartedBeginCountdown && !m_Started)
            {
                from.SendMessage("You may not use this item before the duel begins.");
                return false;
            }
            else if (item is BasePotion && !(item is BaseExplosionPotion) && !(item is BaseRefreshPotion) && IsSuddenDeath)
            {
                from.SendMessage(0x22, "You may not drink potions in sudden death.");
                return false;
            }
            else if (item is Bandage && IsSuddenDeath)
            {
                from.SendMessage(0x22, "You may not use bandages in sudden death.");
                return false;
            }

            if (title == null || option == null || m_Ruleset.GetOption(title, option))
                return true;

            from.SendMessage("The dueling ruleset prevents you from using this item.");
            return false;
        }

        private void DelayBounce_Callback(object state)
        {
            object[] states = (object[])state;
            Mobile mob = (Mobile)states[0];
            Container corpse = (Container)states[1];

            RemoveAggressions(mob);
            SendOutside(mob);
            Refresh(mob, corpse);
            Debuff(mob);
            CancelSpell(mob);
            mob.Frozen = false;
        }

        public void OnMapChanged(Mobile mob)
        {
            OnLocationChanged(mob);
        }

        public void OnLocationChanged(Mobile mob)
        {
            if (!m_Registered || !m_StartedBeginCountdown || m_Finished)
                return;

            Arena arena = m_Arena;

            if (arena == null)
                return;

            if (mob.Map == arena.Facet && arena.Bounds.Contains(mob.Location))
                return;

            DuelPlayer pl = Find(mob);

            if (pl == null || pl.Eliminated)
                return;

            if (mob.Map == Map.Internal)
            {
                // they've logged out

                if (mob.LogoutMap == arena.Facet && arena.Bounds.Contains(mob.LogoutLocation))
                {
                    // they logged out inside the arena.. set them to eject on login

                    mob.LogoutLocation = arena.Outside;
                }
            }

            pl.Eliminated = true;

            mob.LocalOverheadMessage(MessageType.Regular, 0x22, false, "You have forfeited your position in the duel.");
            mob.NonlocalOverheadMessage(MessageType.Regular, 0x22, false, String.Format("{0} has forfeited by leaving the dueling arena.", mob.Name));

            Participant winner = CheckCompletion();

            if (winner != null)
                Finish(winner);
        }

        private bool m_Yielding;

        public void OnDeath(Mobile mob, Container corpse)
        {
            if (!m_Registered || !m_Started)
                return;

            DuelPlayer pl = Find(mob);

            if (pl != null && !pl.Eliminated)
            {
                if (m_EventGame != null && !m_EventGame.OnDeath(mob, corpse))
                    return;

                pl.Eliminated = true;

                if (mob.Poison != null)
                    mob.Poison = null;

                Requip(mob, corpse);
                DelayBounce(TimeSpan.FromSeconds(4.0), mob, corpse);

                Participant winner = CheckCompletion();

                if (winner != null)
                {
                    Finish(winner);
                }
                else if (!m_Yielding)
                {
                    mob.LocalOverheadMessage(MessageType.Regular, 0x22, false, "You have been defeated.");
                    mob.NonlocalOverheadMessage(MessageType.Regular, 0x22, false, String.Format("{0} has been defeated.", mob.Name));
                }
            }
        }

        public bool CheckFull()
        {
            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant p = (Participant)m_Participants[i];

                if (p.HasOpenSlot)
                    return false;
            }

            return true;
        }

        public void Requip(Mobile from, Container cont)
        {
            Corpse corpse = cont as Corpse;

            if (corpse == null)
                return;

            List<Item> items = new List<Item>(corpse.Items);

            bool gathered = false;
            bool didntFit = false;

            Container pack = from.Backpack;

            for (int i = 0; !didntFit && i < items.Count; ++i)
            {
                Item item = items[i];
                Point3D loc = item.Location;

                if ((item.Layer == Layer.Hair || item.Layer == Layer.FacialHair) || !item.Movable)
                    continue;

                if (pack != null)
                {
                    pack.DropItem(item);
                    gathered = true;
                }
                else
                {
                    didntFit = true;
                }
            }

            corpse.Carved = true;

            if (corpse.ItemID == 0x2006)
            {
                corpse.ProcessDelta();
                corpse.SendRemovePacket();
                corpse.ItemID = Utility.Random(0xECA, 9); // bone graphic
                corpse.Hue = 0;
                corpse.ProcessDelta();

                Mobile killer = from.FindMostRecentDamager(false);

                if (killer != null && killer.Player)
                    killer.AddToBackpack(new Head(m_Tournament == null ? HeadType.Duel : HeadType.Tournament, from.Name));
            }

            from.PlaySound(0x3E3);

            if (gathered && !didntFit)
                from.SendLocalizedMessage(1062471); // You quickly gather all of your belongings.
            else if (gathered && didntFit)
                from.SendLocalizedMessage(1062472); // You gather some of your belongings. The rest remain on the corpse.
        }

        public void Refresh(Mobile mob, Container cont)
        {
            if (!mob.Alive)
            {
                mob.Resurrect();

                DeathRobe robe = mob.FindItemOnLayer(Layer.OuterTorso) as DeathRobe;

                if (robe != null)
                    robe.Delete();

                if (cont is Corpse)
                {
                    Corpse corpse = (Corpse)cont;

                    for (int i = 0; i < corpse.EquipItems.Count; ++i)
                    {
                        Item item = corpse.EquipItems[i];

                        if (item.Movable && item.Layer != Layer.Hair && item.Layer != Layer.FacialHair && item.IsChildOf(mob.Backpack))
                            mob.EquipItem(item);
                    }
                }
            }

            mob.Hits = mob.HitsMax;
            mob.Stam = mob.StamMax;
            mob.Mana = mob.ManaMax;

            mob.Poison = null;
        }

        public void SendOutside(Mobile mob)
        {
            if (m_Arena == null)
                return;

            mob.Combatant = null;
            mob.MoveToWorld(m_Arena.Outside, m_Arena.Facet);
        }

        private Point3D m_GatePoint;
        private Map m_GateFacet;

        public void Finish(Participant winner)
        {
            if (m_Finished)
                return;

            EndAutoTie();
            StopSDTimers();

            m_Finished = true;

            for (int i = 0; i < winner.Players.Length; ++i)
            {
                DuelPlayer pl = winner.Players[i];

                if (pl != null && !pl.Eliminated)
                    DelayBounce(TimeSpan.FromSeconds(8.0), pl.Mobile, null);
            }

            winner.Broadcast(0x59, null, winner.Players.Length == 1 ? "{0} has won the duel." : "{0} and {1} team have won the duel.", winner.Players.Length == 1 ? "You have won the duel." : "Your team has won the duel.");

            if (m_Tournament != null && winner.TournyPart != null)
            {
                m_Match.Winner = winner.TournyPart;
                winner.TournyPart.WonMatch(m_Match);
                m_Tournament.HandleWon(m_Arena, m_Match, winner.TournyPart);
            }

            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant loser = (Participant)m_Participants[i];

                if (loser != winner)
                {
                    loser.Broadcast(0x22, null, loser.Players.Length == 1 ? "{0} has lost the duel." : "{0} and {1} team have lost the duel.", loser.Players.Length == 1 ? "You have lost the duel." : "Your team has lost the duel.");

                    if (m_Tournament != null && loser.TournyPart != null)
                        loser.TournyPart.LostMatch(m_Match);
                }

                for (int j = 0; j < loser.Players.Length; ++j)
                {
                    if (loser.Players[j] != null)
                    {
                        RemoveAggressions(loser.Players[j].Mobile);
                        loser.Players[j].Mobile.Delta(MobileDelta.Noto);
                        loser.Players[j].Mobile.CloseGump(typeof(BeginGump));

                        if (m_Tournament != null)
                            loser.Players[j].Mobile.SendEverything();
                    }
                }
            }

            if (IsOneVsOne)
            {
                DuelPlayer dp1 = ((Participant)m_Participants[0]).Players[0];
                DuelPlayer dp2 = ((Participant)m_Participants[1]).Players[0];

                if (dp1 != null && dp2 != null)
                {
                    Award(dp1.Mobile, dp2.Mobile, dp1.Participant == winner);
                    Award(dp2.Mobile, dp1.Mobile, dp2.Participant == winner);
                }
            }

            if (m_EventGame != null)
                m_EventGame.OnStop();

            Timer.DelayCall(TimeSpan.FromSeconds(9.0), new TimerCallback(UnregisterRematch));
        }

        public void Award(Mobile us, Mobile them, bool won)
        {
            Ladder ladder = (m_Arena == null ? Ladder.Instance : m_Arena.AcquireLadder());

            if (ladder == null)
                return;

            LadderEntry ourEntry = ladder.Find(us);
            LadderEntry theirEntry = ladder.Find(them);

            if (ourEntry == null || theirEntry == null)
                return;

            int xpGain = Ladder.GetExperienceGain(ourEntry, theirEntry, won);

            if (xpGain == 0)
                return;

            if (m_Tournament != null)
                xpGain *= (xpGain > 0 ? 5 : 2);

            if (won)
                ++ourEntry.Wins;
            else
                ++ourEntry.Losses;

            int oldLevel = Ladder.GetLevel(ourEntry.Experience);

            ourEntry.Experience += xpGain;

            if (ourEntry.Experience < 0)
                ourEntry.Experience = 0;

            ladder.UpdateEntry(ourEntry);

            int newLevel = Ladder.GetLevel(ourEntry.Experience);

            if (newLevel > oldLevel)
                us.SendMessage(0x59, "You have achieved level {0}!", newLevel);
            else if (newLevel < oldLevel)
                us.SendMessage(0x22, "You have lost a level. You are now at {0}.", newLevel);
        }

        public void UnregisterRematch()
        {
            Unregister(true);
        }

        public void Unregister()
        {
            Unregister(false);
        }

        public void Unregister(bool queryRematch)
        {
            DestroyWall();

            if (!m_Registered)
                return;

            m_Registered = false;

            if (m_Arena != null)
                m_Arena.Evict();

            StopSDTimers();

            Type[] types = new Type[] { typeof(BeginGump), typeof(DuelContextGump), typeof(ParticipantGump), typeof(PickRulesetGump), typeof(ReadyGump), typeof(ReadyUpGump), typeof(RulesetGump) };

            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant p = (Participant)m_Participants[i];

                for (int j = 0; j < p.Players.Length; ++j)
                {
                    DuelPlayer pl = (DuelPlayer)p.Players[j];

                    if (pl == null)
                        continue;

                    if (pl.Mobile is PlayerMobile)
                        ((PlayerMobile)pl.Mobile).DuelPlayer = null;

                    for (int k = 0; k < types.Length; ++k)
                        pl.Mobile.CloseGump(types[k]);
                }
            }

            if (queryRematch && m_Tournament == null)
                QueryRematch();
        }

        public void QueryRematch()
        {
            DuelContext dc = new DuelContext(m_Initiator, m_Ruleset.Layout, false);

            dc.m_Ruleset = m_Ruleset;
            dc.m_Rematch = true;

            dc.m_Participants.Clear();

            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant oldPart = (Participant)m_Participants[i];
                Participant newPart = new Participant(dc, oldPart.Players.Length);

                for (int j = 0; j < oldPart.Players.Length; ++j)
                {
                    DuelPlayer oldPlayer = oldPart.Players[j];

                    if (oldPlayer != null)
                        newPart.Players[j] = new DuelPlayer(oldPlayer.Mobile, newPart);
                }

                dc.m_Participants.Add(newPart);
            }

            dc.CloseAllGumps();
            dc.SendReadyUpGump();
        }

        public DuelPlayer Find(Mobile mob)
        {
            if (mob is PlayerMobile)
            {
                PlayerMobile pm = (PlayerMobile)mob;

                if (pm.DuelContext == this)
                    return pm.DuelPlayer;

                return null;
            }

            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant p = (Participant)m_Participants[i];
                DuelPlayer pl = p.Find(mob);

                if (pl != null)
                    return pl;
            }

            return null;
        }

        public bool IsAlly(Mobile m1, Mobile m2)
        {
            DuelPlayer pl1 = Find(m1);
            DuelPlayer pl2 = Find(m2);

            return (pl1 != null && pl2 != null && pl1.Participant == pl2.Participant);
        }

        public Participant CheckCompletion()
        {
            Participant winner = null;

            bool hasWinner = false;
            int eliminated = 0;

            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant p = (Participant)m_Participants[i];

                if (p.Eliminated)
                {
                    ++eliminated;

                    if (eliminated == (m_Participants.Count - 1))
                        hasWinner = true;
                }
                else
                {
                    winner = p;
                }
            }

            if (hasWinner)
                return winner == null ? (Participant)m_Participants[0] : winner;

            return null;
        }

        private Timer m_Countdown;

        public void StartCountdown(int count, CountdownCallback cb)
        {
            cb(count);
            m_Countdown = Timer.DelayCall(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0), count, new TimerStateCallback(Countdown_Callback), new object[] { count - 1, cb });
        }

        public void StopCountdown()
        {
            if (m_Countdown != null)
                m_Countdown.Stop();

            m_Countdown = null;
        }

        private void Countdown_Callback(object state)
        {
            object[] states = (object[])state;

            int count = (int)states[0];
            CountdownCallback cb = (CountdownCallback)states[1];

            if (count == 0)
            {
                if (m_Countdown != null)
                    m_Countdown.Stop();

                m_Countdown = null;
            }

            cb(count);

            states[0] = count - 1;
        }

        private Timer m_AutoTieTimer;
        private bool m_Tied;

        public bool Tied { get { return m_Tied; } }

        private bool m_IsSuddenDeath;

        public bool IsSuddenDeath { get { return m_IsSuddenDeath; } set { m_IsSuddenDeath = value; } }

        private Timer m_SDWarnTimer, m_SDActivateTimer;

        public void StopSDTimers()
        {
            if (m_SDWarnTimer != null)
                m_SDWarnTimer.Stop();

            m_SDWarnTimer = null;

            if (m_SDActivateTimer != null)
                m_SDActivateTimer.Stop();

            m_SDActivateTimer = null;
        }

        public void StartSuddenDeath(TimeSpan timeUntilActive)
        {
            if (m_SDWarnTimer != null)
                m_SDWarnTimer.Stop();

            m_SDWarnTimer = Timer.DelayCall(TimeSpan.FromMinutes(timeUntilActive.TotalMinutes * 0.9), new TimerCallback(WarnSuddenDeath));

            if (m_SDActivateTimer != null)
                m_SDActivateTimer.Stop();

            m_SDActivateTimer = Timer.DelayCall(timeUntilActive, new TimerCallback(ActivateSuddenDeath));
        }

        public void WarnSuddenDeath()
        {
            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant p = (Participant)m_Participants[i];

                for (int j = 0; j < p.Players.Length; ++j)
                {
                    DuelPlayer pl = p.Players[j];

                    if (pl == null || pl.Eliminated)
                        continue;

                    pl.Mobile.SendSound(0x1E1);
                    pl.Mobile.SendMessage(0x22, "Warning! Warning! Warning!");
                    pl.Mobile.SendMessage(0x22, "Sudden death will be active soon!");
                }
            }

            if (m_Tournament != null)
                m_Tournament.Alert(m_Arena, "Sudden death will be active soon!");

            if (m_SDWarnTimer != null)
                m_SDWarnTimer.Stop();

            m_SDWarnTimer = null;
        }

        public static bool CheckSuddenDeath(Mobile mob)
        {
            if (mob is PlayerMobile)
            {
                PlayerMobile pm = (PlayerMobile)mob;

                if (pm.DuelPlayer != null && !pm.DuelPlayer.Eliminated && pm.DuelContext != null && pm.DuelContext.IsSuddenDeath)
                    return true;
            }

            return false;
        }

        public void ActivateSuddenDeath()
        {
            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant p = (Participant)m_Participants[i];

                for (int j = 0; j < p.Players.Length; ++j)
                {
                    DuelPlayer pl = p.Players[j];

                    if (pl == null || pl.Eliminated)
                        continue;

                    pl.Mobile.SendSound(0x1E1);
                    pl.Mobile.SendMessage(0x22, "Warning! Warning! Warning!");
                    pl.Mobile.SendMessage(0x22, "Sudden death has ACTIVATED. You are now unable to perform any beneficial actions.");
                }
            }

            if (m_Tournament != null)
                m_Tournament.Alert(m_Arena, "Sudden death has been activated!");

            m_IsSuddenDeath = true;

            if (m_SDActivateTimer != null)
                m_SDActivateTimer.Stop();

            m_SDActivateTimer = null;
        }

        public void BeginAutoTie()
        {
            if (m_AutoTieTimer != null)
                m_AutoTieTimer.Stop();

            TimeSpan ts = (m_Tournament == null || m_Tournament.TournyType == TournyType.Standard)
                ? AutoTieDelay
                : TimeSpan.FromMinutes(90.0);

            m_AutoTieTimer = Timer.DelayCall(ts, new TimerCallback(InvokeAutoTie));
        }

        public void EndAutoTie()
        {
            if (m_AutoTieTimer != null)
                m_AutoTieTimer.Stop();

            m_AutoTieTimer = null;
        }

        public void InvokeAutoTie()
        {
            m_AutoTieTimer = null;

            if (!m_Started || m_Finished)
                return;

            m_Tied = true;
            m_Finished = true;

            StopSDTimers();

            ArrayList remaining = new ArrayList();

            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant p = (Participant)m_Participants[i];

                if (p.Eliminated)
                {
                    p.Broadcast(0x22, null, p.Players.Length == 1 ? "{0} has lost the duel." : "{0} and {1} team have lost the duel.", p.Players.Length == 1 ? "You have lost the duel." : "Your team has lost the duel.");
                }
                else
                {
                    p.Broadcast(0x59, null, p.Players.Length == 1 ? "{0} has tied the duel due to time expiration." : "{0} and {1} team have tied the duel due to time expiration.", p.Players.Length == 1 ? "You have tied the duel due to time expiration." : "Your team has tied the duel due to time expiration.");

                    for (int j = 0; j < p.Players.Length; ++j)
                    {
                        DuelPlayer pl = p.Players[j];

                        if (pl != null && !pl.Eliminated)
                            DelayBounce(TimeSpan.FromSeconds(8.0), pl.Mobile, null);
                    }

                    if (p.TournyPart != null)
                        remaining.Add(p.TournyPart);
                }

                for (int j = 0; j < p.Players.Length; ++j)
                {
                    DuelPlayer pl = p.Players[j];

                    if (pl != null)
                    {
                        pl.Mobile.Delta(MobileDelta.Noto);
                        pl.Mobile.SendEverything();
                    }
                }
            }

            if (m_Tournament != null)
                m_Tournament.HandleTie(m_Arena, m_Match, remaining);

            Timer.DelayCall(TimeSpan.FromSeconds(10.0), new TimerCallback(Unregister));
        }

        public bool IsOneVsOne
        {
            get
            {
                if (m_Participants.Count != 2)
                    return false;

                if (((Participant)m_Participants[0]).Players.Length != 1)
                    return false;

                if (((Participant)m_Participants[1]).Players.Length != 1)
                    return false;

                return true;
            }
        }

        public static void Initialize()
        {
            EventSink.Speech += new SpeechEventHandler(EventSink_Speech);
            EventSink.Login += new LoginEventHandler(EventSink_Login);

            CommandSystem.Register("vli", AccessLevel.GameMaster, new CommandEventHandler(vli_oc));
        }

        private static void vli_oc(CommandEventArgs e)
        {
            e.Mobile.BeginTarget(-1, false, Targeting.TargetFlags.None, new TargetCallback(vli_ot));
        }

        private static void vli_ot(Mobile from, object obj)
        {
            if (obj is PlayerMobile)
            {
                PlayerMobile pm = (PlayerMobile)obj;

                Ladder ladder = Ladder.Instance;

                if (ladder == null)
                    return;

                LadderEntry entry = ladder.Find(pm);

                if (entry != null)
                    from.SendGump(new PropertiesGump(from, entry));
            }
        }

        private static TimeSpan CombatDelay = TimeSpan.FromSeconds(30.0);
        private static TimeSpan AutoTieDelay = TimeSpan.FromMinutes(15.0);

        public static bool CheckCombat(Mobile m)
        {
            for (int i = 0; i < m.Aggressed.Count; ++i)
            {
                AggressorInfo info = m.Aggressed[i];

                if (info.Defender.Player && (DateTime.UtcNow - info.LastCombatTime) < CombatDelay)
                    return true;
            }

            for (int i = 0; i < m.Aggressors.Count; ++i)
            {
                AggressorInfo info = m.Aggressors[i];

                if (info.Attacker.Player && (DateTime.UtcNow - info.LastCombatTime) < CombatDelay)
                    return true;
            }

            return false;
        }

        private static void EventSink_Login(LoginEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;

            if (pm == null)
                return;

            DuelContext dc = pm.DuelContext;

            if (dc == null)
                return;

            if (dc.ReadyWait && pm.DuelPlayer.Ready && !dc.Started && !dc.StartedBeginCountdown && !dc.Finished)
            {
                if (dc.m_Tournament == null)
                    pm.SendGump(new ReadyGump(pm, dc, dc.m_ReadyCount));
            }
            else if (dc.ReadyWait && !dc.StartedBeginCountdown && !dc.Started && !dc.Finished)
            {
                if (dc.m_Tournament == null)
                    pm.SendGump(new ReadyUpGump(pm, dc));
            }
            else if (dc.Initiator == pm && !dc.ReadyWait && !dc.StartedBeginCountdown && !dc.Started && !dc.Finished)
                pm.SendGump(new DuelContextGump(pm, dc));
        }

        private static void ViewLadder_OnTarget(Mobile from, object obj, object state)
        {
            if (obj is PlayerMobile)
            {
                PlayerMobile pm = (PlayerMobile)obj;
                Ladder ladder = (Ladder)state;

                LadderEntry entry = ladder.Find(pm);

                if (entry == null)
                    return; // sanity

                string text = String.Format("{{0}} are ranked {0} at level {1}.", LadderGump.Rank(entry.Index + 1), Ladder.GetLevel(entry.Experience));

                pm.PrivateOverheadMessage(MessageType.Regular, pm.SpeechHue, true, String.Format(text, from == pm ? "You" : "They"), from.NetState);
            }
            else if (obj is Mobile)
            {
                Mobile mob = (Mobile)obj;

                if (mob.Body.IsHuman)
                    mob.PrivateOverheadMessage(MessageType.Regular, mob.SpeechHue, false, "I'm not a duelist, and quite frankly, I resent the implication.", from.NetState);
                else
                    mob.PrivateOverheadMessage(MessageType.Regular, 0x3B2, true, "It's probably better than you.", from.NetState);
            }
            else
            {
                from.SendMessage("That's not a player.");
            }
        }

        private static void EventSink_Speech(SpeechEventArgs e)
        {
            if (e.Handled)
                return;

            PlayerMobile pm = e.Mobile as PlayerMobile;

            if (pm == null)
                return;

            if (Insensitive.Contains(e.Speech, "i wish to duel"))
            {
                if (!pm.CheckAlive())
                {
                }
                else if (pm.Region.IsPartOf(typeof(Regions.Jail)))
                {
                }
                else if (CheckCombat(pm))
                {
                    e.Mobile.SendMessage(0x22, "You have recently been in combat with another player and must wait before starting a duel.");
                }
                else if (pm.DuelContext != null)
                {
                    if (pm.DuelContext.Initiator == pm)
                        e.Mobile.SendMessage(0x22, "You have already started a duel.");
                    else
                        e.Mobile.SendMessage(0x22, "You have already been challenged in a duel.");
                }
                else if (TournamentController.IsActive)
                {
                    e.Mobile.SendMessage(0x22, "You may not start a duel while a tournament is active.");
                }
                else
                {
                    pm.SendGump(new DuelContextGump(pm, new DuelContext(pm, RulesetLayout.Root)));
                    e.Handled = true;
                }
            }
            else if (Insensitive.Equals(e.Speech, "change arena preferences"))
            {
                if (!pm.CheckAlive())
                {
                }
                else
                {
                    Preferences prefs = Preferences.Instance;

                    if (prefs != null)
                    {
                        e.Mobile.CloseGump(typeof(PreferencesGump));
                        e.Mobile.SendGump(new PreferencesGump(e.Mobile, prefs));
                    }
                }
            }
            else if (Insensitive.Equals(e.Speech, "showladder"))
            {
                e.Blocked = true;
                if (!pm.CheckAlive())
                {
                }
                else
                {
                    Ladder instance = Ladder.Instance;

                    if (instance == null)
                    {
                        //pm.SendMessage( "Ladder not yet initialized." );
                    }
                    else
                    {
                        LadderEntry entry = instance.Find(pm);

                        if (entry == null)
                            return; // sanity

                        string text = String.Format("{{0}} {{1}} ranked {0} at level {1}.", LadderGump.Rank(entry.Index + 1), Ladder.GetLevel(entry.Experience));

                        pm.LocalOverheadMessage(MessageType.Regular, pm.SpeechHue, true, String.Format(text, "You", "are"));
                        pm.NonlocalOverheadMessage(MessageType.Regular, pm.SpeechHue, true, String.Format(text, pm.Name, "is"));

                        //pm.PublicOverheadMessage( MessageType.Regular, pm.SpeechHue, true, String.Format( "Level {0} with {1} win{2} and {3} loss{4}.", Ladder.GetLevel( entry.Experience ), entry.Wins, entry.Wins==1?"":"s", entry.Losses, entry.Losses==1?"":"es" ) );
                        //pm.PublicOverheadMessage( MessageType.Regular, pm.SpeechHue, true, String.Format( "Level {0} with {1} win{2} and {3} loss{4}.", Ladder.GetLevel( entry.Experience ), entry.Wins, entry.Wins==1?"":"s", entry.Losses, entry.Losses==1?"":"es" ) );
                    }
                }
            }
            else if (Insensitive.Equals(e.Speech, "viewladder"))
            {
                e.Blocked = true;

                if (!pm.CheckAlive())
                {
                }
                else
                {
                    Ladder instance = Ladder.Instance;

                    if (instance == null)
                    {
                        //pm.SendMessage( "Ladder not yet initialized." );
                    }
                    else
                    {
                        pm.SendMessage("Target a player to view their ranking and level.");
                        pm.BeginTarget(16, false, Targeting.TargetFlags.None, new TargetStateCallback(ViewLadder_OnTarget), instance);
                    }
                }
            }
            else if (Insensitive.Contains(e.Speech, "i yield"))
            {
                if (!pm.CheckAlive())
                {
                }
                else if (pm.DuelContext == null)
                {
                }
                else if (pm.DuelContext.Finished)
                {
                    e.Mobile.SendMessage(0x22, "The duel is already finished.");
                }
                else if (!pm.DuelContext.Started)
                {
                    DuelContext dc = pm.DuelContext;
                    Mobile init = dc.Initiator;

                    if (pm.DuelContext.StartedBeginCountdown)
                    {
                        e.Mobile.SendMessage(0x22, "The duel has not yet started.");
                    }
                    else
                    {
                        DuelPlayer pl = pm.DuelContext.Find(pm);

                        if (pl == null)
                            return;

                        Participant p = pl.Participant;

                        if (!pm.DuelContext.ReadyWait) // still setting stuff up
                        {
                            p.Broadcast(0x22, null, "{0} has yielded.", "You have yielded.");

                            if (init == pm)
                            {
                                dc.Unregister();
                            }
                            else
                            {
                                p.Nullify(pl);
                                pm.DuelPlayer = null;

                                NetState ns = init.NetState;

                                if (ns != null)
                                {
                                    foreach (Gump g in ns.Gumps)
                                    {
                                        if (g is ParticipantGump)
                                        {
                                            ParticipantGump pg = (ParticipantGump)g;

                                            if (pg.Participant == p)
                                            {
                                                init.SendGump(new ParticipantGump(init, dc, p));
                                                break;
                                            }
                                        }
                                        else if (g is DuelContextGump)
                                        {
                                            DuelContextGump dcg = (DuelContextGump)g;

                                            if (dcg.Context == dc)
                                            {
                                                init.SendGump(new DuelContextGump(init, dc));
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (!pm.DuelContext.StartedReadyCountdown) // at ready stage
                        {
                            p.Broadcast(0x22, null, "{0} has yielded.", "You have yielded.");

                            dc.m_Yielding = true;
                            dc.RejectReady(pm, null);
                            dc.m_Yielding = false;

                            if (init == pm)
                            {
                                dc.Unregister();
                            }
                            else if (dc.m_Registered)
                            {
                                p.Nullify(pl);
                                pm.DuelPlayer = null;

                                NetState ns = init.NetState;

                                if (ns != null)
                                {
                                    bool send = true;

                                    foreach (Gump g in ns.Gumps)
                                    {
                                        if (g is ParticipantGump)
                                        {
                                            ParticipantGump pg = (ParticipantGump)g;

                                            if (pg.Participant == p)
                                            {
                                                init.SendGump(new ParticipantGump(init, dc, p));
                                                send = false;
                                                break;
                                            }
                                        }
                                        else if (g is DuelContextGump)
                                        {
                                            DuelContextGump dcg = (DuelContextGump)g;

                                            if (dcg.Context == dc)
                                            {
                                                init.SendGump(new DuelContextGump(init, dc));
                                                send = false;
                                                break;
                                            }
                                        }
                                    }

                                    if (send)
                                        init.SendGump(new DuelContextGump(init, dc));
                                }
                            }
                        }
                        else
                        {
                            if (pm.DuelContext.m_Countdown != null)
                                pm.DuelContext.m_Countdown.Stop();
                            pm.DuelContext.m_Countdown = null;

                            pm.DuelContext.m_StartedReadyCountdown = false;
                            p.Broadcast(0x22, null, "{0} has yielded.", "You have yielded.");

                            dc.m_Yielding = true;
                            dc.RejectReady(pm, null);
                            dc.m_Yielding = false;

                            if (init == pm)
                            {
                                dc.Unregister();
                            }
                            else if (dc.m_Registered)
                            {
                                p.Nullify(pl);
                                pm.DuelPlayer = null;

                                NetState ns = init.NetState;

                                if (ns != null)
                                {
                                    bool send = true;

                                    foreach (Gump g in ns.Gumps)
                                    {
                                        if (g is ParticipantGump)
                                        {
                                            ParticipantGump pg = (ParticipantGump)g;

                                            if (pg.Participant == p)
                                            {
                                                init.SendGump(new ParticipantGump(init, dc, p));
                                                send = false;
                                                break;
                                            }
                                        }
                                        else if (g is DuelContextGump)
                                        {
                                            DuelContextGump dcg = (DuelContextGump)g;

                                            if (dcg.Context == dc)
                                            {
                                                init.SendGump(new DuelContextGump(init, dc));
                                                send = false;
                                                break;
                                            }
                                        }
                                    }

                                    if (send)
                                        init.SendGump(new DuelContextGump(init, dc));
                                }
                            }
                        }
                    }
                }
                else
                {
                    DuelPlayer pl = pm.DuelContext.Find(pm);

                    if (pl != null)
                    {
                        if (pm.DuelContext.IsOneVsOne)
                        {
                            e.Mobile.SendMessage(0x22, "You may not yield a 1 on 1 match.");
                        }
                        else if (pl.Eliminated)
                        {
                            e.Mobile.SendMessage(0x22, "You have already been eliminated.");
                        }
                        else
                        {
                            pm.LocalOverheadMessage(MessageType.Regular, 0x22, false, "You have yielded.");
                            pm.NonlocalOverheadMessage(MessageType.Regular, 0x22, false, String.Format("{0} has yielded.", pm.Name));

                            pm.DuelContext.m_Yielding = true;
                            pm.Kill();
                            pm.DuelContext.m_Yielding = false;

                            if (pm.Alive) // invul, ...
                            {
                                pl.Eliminated = true;

                                pm.DuelContext.RemoveAggressions(pm);
                                pm.DuelContext.SendOutside(pm);
                                pm.DuelContext.Refresh(pm, null);
                                Debuff(pm);
                                CancelSpell(pm);
                                pm.Frozen = false;

                                Participant winner = pm.DuelContext.CheckCompletion();

                                if (winner != null)
                                    pm.DuelContext.Finish(winner);
                            }
                        }
                    }
                    else
                    {
                        e.Mobile.SendMessage(0x22, "BUG: Unable to find duel context.");
                    }
                }
            }
        }

        public DuelContext(Mobile initiator, RulesetLayout layout)
            : this(initiator, layout, true)
        {
        }

        public DuelContext(Mobile initiator, RulesetLayout layout, bool addNew)
        {
            m_Initiator = initiator;
            m_Participants = new ArrayList();
            m_Ruleset = new Ruleset(layout);
            m_Ruleset.ApplyDefault(layout.Defaults[0]);

            if (addNew)
            {
                m_Participants.Add(new Participant(this, 1));
                m_Participants.Add(new Participant(this, 1));

                ((Participant)m_Participants[0]).Add(initiator);
            }
        }

        public void CloseAllGumps()
        {
            Type[] types = new Type[] { typeof(DuelContextGump), typeof(ParticipantGump), typeof(RulesetGump) };
            int[] defs = new int[] { -1, -1, -1 };

            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant p = (Participant)m_Participants[i];

                for (int j = 0; j < p.Players.Length; ++j)
                {
                    DuelPlayer pl = p.Players[j];

                    if (pl == null)
                        continue;

                    Mobile mob = pl.Mobile;

                    for (int k = 0; k < types.Length; ++k)
                        mob.CloseGump(types[k]);
                    //mob.CloseGump( types[k], defs[k] );
                }
            }
        }

        public void RejectReady(Mobile rejector, string page)
        {
            if (m_StartedReadyCountdown)
                return; // sanity

            Type[] types = new Type[] { typeof(DuelContextGump), typeof(ReadyUpGump), typeof(ReadyGump) };
            int[] defs = new int[] { -1, -1, -1 };

            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant p = (Participant)m_Participants[i];

                for (int j = 0; j < p.Players.Length; ++j)
                {
                    DuelPlayer pl = p.Players[j];

                    if (pl == null)
                        continue;

                    pl.Ready = false;

                    Mobile mob = pl.Mobile;

                    if (page == null) // yield
                    {
                        if (mob != rejector)
                            mob.SendMessage(0x22, "{0} has yielded.", rejector.Name);
                    }
                    else
                    {
                        if (mob == rejector)
                            mob.SendMessage(0x22, "You have rejected the {0}.", m_Rematch ? "rematch" : page);
                        else
                            mob.SendMessage(0x22, "{0} has rejected the {1}.", rejector.Name, m_Rematch ? "rematch" : page);
                    }

                    for (int k = 0; k < types.Length; ++k)
                        mob.CloseGump(types[k]);
                    //mob.CloseGump( types[k], defs[k] );
                }
            }

            if (m_Rematch)
                Unregister();
            else if (!m_Yielding)
                m_Initiator.SendGump(new DuelContextGump(m_Initiator, this));

            m_ReadyWait = false;
            m_ReadyCount = 0;
        }

        public void SendReadyGump()
        {
            SendReadyGump(-1);
        }

        public static void Debuff(Mobile mob)
        {
            mob.RemoveStatMod("[Magic] Str Offset");
            mob.RemoveStatMod("[Magic] Dex Offset");
            mob.RemoveStatMod("[Magic] Int Offset");
            mob.RemoveStatMod("Concussion");
            mob.RemoveStatMod("blood-rose");
            mob.RemoveStatMod("clarity-potion");

            OrangePetals.RemoveContext(mob);

            mob.Paralyzed = false;
            mob.Hidden = false;

            if (!Core.AOS)
            {
                mob.MagicDamageAbsorb = 0;
                mob.MeleeDamageAbsorb = 0;
                Spells.Second.ProtectionSpell.Registry.Remove(mob);

                Spells.Fourth.ArchProtectionSpell.RemoveEntry(mob);

                mob.EndAction(typeof(DefensiveSpell));
            }

            TransformationSpellHelper.RemoveContext(mob, true);
            AnimalForm.RemoveContext(mob, true);

            if (DisguiseTimers.IsDisguised(mob))
                DisguiseTimers.StopTimer(mob);

            if (!mob.CanBeginAction(typeof(PolymorphSpell)))
            {
                mob.BodyMod = 0;
                mob.HueMod = -1;
                mob.EndAction(typeof(PolymorphSpell));
            }

            BaseArmor.ValidateMobile(mob);
            BaseClothing.ValidateMobile(mob);

            mob.Hits = mob.HitsMax;
            mob.Stam = mob.StamMax;
            mob.Mana = mob.ManaMax;

            mob.Poison = null;
        }

        public static void CancelSpell(Mobile mob)
        {
            if (mob.Spell is Spells.Spell)
                ((Spells.Spell)mob.Spell).Disturb(Spells.DisturbType.Kill);

            Targeting.Target.Cancel(mob);
        }

        private bool m_StartedBeginCountdown;
        private bool m_StartedReadyCountdown;

        public bool StartedBeginCountdown { get { return m_StartedBeginCountdown; } }
        public bool StartedReadyCountdown { get { return m_StartedReadyCountdown; } }

        private class InternalWall : Item
        {
            public InternalWall()
                : base(0x80)
            {
                Movable = false;
            }

            public void Appear(Point3D loc, Map map)
            {
                MoveToWorld(loc, map);

                Effects.SendLocationParticles(this, 0x376A, 9, 10, 5025);
            }

            public InternalWall(Serial serial)
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

                Delete();
            }
        }

        private ArrayList m_Walls = new ArrayList();

        public void DestroyWall()
        {
            for (int i = 0; i < m_Walls.Count; ++i)
                ((Item)m_Walls[i]).Delete();

            m_Walls.Clear();
        }

        public void CreateWall()
        {
            if (m_Arena == null)
                return;

            Point3D start = m_Arena.Points.EdgeWest;
            Point3D wall = m_Arena.Wall;

            int dx = start.X - wall.X;
            int dy = start.Y - wall.Y;
            int rx = dx - dy;
            int ry = dx + dy;

            bool eastToWest;

            if (rx >= 0 && ry >= 0)
                eastToWest = false;
            else if (rx >= 0)
                eastToWest = true;
            else if (ry >= 0)
                eastToWest = true;
            else
                eastToWest = false;

            Effects.PlaySound(wall, m_Arena.Facet, 0x1F6);

            for (int i = -1; i <= 1; ++i)
            {
                Point3D loc = new Point3D(eastToWest ? wall.X + i : wall.X, eastToWest ? wall.Y : wall.Y + i, wall.Z);

                InternalWall created = new InternalWall();

                created.Appear(loc, m_Arena.Facet);

                m_Walls.Add(created);
            }
        }

        public void BuildParties()
        {
            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant p = (Participant)m_Participants[i];

                if (p.Players.Length > 1)
                {
                    ArrayList players = new ArrayList();

                    for (int j = 0; j < p.Players.Length; ++j)
                    {
                        DuelPlayer dp = p.Players[j];

                        if (dp == null)
                            continue;

                        players.Add(dp.Mobile);
                    }

                    if (players.Count > 1)
                    {
                        for (int leaderIndex = 0; (leaderIndex + 1) < players.Count; leaderIndex += Party.Capacity)
                        {
                            Mobile leader = (Mobile)players[leaderIndex];
                            Party party = Party.Get(leader);

                            if (party == null)
                            {
                                leader.Party = party = new Party(leader);
                            }
                            else if (party.Leader != leader)
                            {
                                party.SendPublicMessage(leader, "I leave this party to fight in a duel.");
                                party.Remove(leader);
                                leader.Party = party = new Party(leader);
                            }

                            for (int j = leaderIndex + 1; j < players.Count && j < leaderIndex + Party.Capacity; ++j)
                            {
                                Mobile player = (Mobile)players[j];
                                Party existing = Party.Get(player);

                                if (existing == party)
                                    continue;

                                if ((party.Members.Count + party.Candidates.Count) >= Party.Capacity)
                                {
                                    player.SendMessage("You could not be added to the team party because it is at full capacity.");
                                    leader.SendMessage("{0} could not be added to the team party because it is at full capacity.");
                                }
                                else
                                {
                                    if (existing != null)
                                    {
                                        existing.SendPublicMessage(player, "I leave this party to fight in a duel.");
                                        existing.Remove(player);
                                    }

                                    party.OnAccept(player, true);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void ClearIllegalItems()
        {
            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant p = (Participant)m_Participants[i];

                for (int j = 0; j < p.Players.Length; ++j)
                {
                    DuelPlayer pl = p.Players[j];

                    if (pl == null)
                        continue;

                    ClearIllegalItems(pl.Mobile);
                }
            }
        }

        public void ClearIllegalItems(Mobile mob)
        {
            if (mob.StunReady && !AllowSpecialAbility(mob, "Stun", false))
                mob.StunReady = false;

            if (mob.DisarmReady && !AllowSpecialAbility(mob, "Disarm", false))
                mob.DisarmReady = false;

            Container pack = mob.Backpack;

            if (pack == null)
                return;

            for (int i = mob.Items.Count - 1; i >= 0; --i)
            {
                if (i >= mob.Items.Count)
                    continue; // sanity

                Item item = mob.Items[i];

                if (!CheckItemEquip(mob, item))
                {
                    pack.DropItem(item);

                    if (item is BaseWeapon)
                        mob.SendLocalizedMessage(1062001, item.Name == null ? "#" + item.LabelNumber.ToString() : item.Name); // You can no longer wield your ~1_WEAPON~
                    else if (item is BaseArmor && !(item is BaseShield))
                        mob.SendLocalizedMessage(1062002, item.Name == null ? "#" + item.LabelNumber.ToString() : item.Name); // You can no longer wear your ~1_ARMOR~
                    else
                        mob.SendLocalizedMessage(1062003, item.Name == null ? "#" + item.LabelNumber.ToString() : item.Name); // You can no longer equip your ~1_SHIELD~
                }
            }

            Item inHand = mob.Holding;

            if (inHand != null && !CheckItemEquip(mob, inHand))
            {
                mob.Holding = null;

                BounceInfo bi = inHand.GetBounce();

                if (bi.m_Parent == mob)
                    pack.DropItem(inHand);
                else
                    inHand.Bounce(mob);

                inHand.ClearBounce();
            }
        }

        private void MessageRuleset(Mobile mob)
        {
            if (m_Ruleset == null)
            {
                return;
            }

            Ruleset ruleset = m_Ruleset;
            Ruleset basedef = ruleset.Base;

            mob.SendMessage("Ruleset: {0}", basedef.Title);

            BitArray defs;

            if (ruleset.Flavors.Count > 0)
            {
                defs = new BitArray(basedef.Options);

                for (int i = 0; i < ruleset.Flavors.Count; ++i)
                {
                    defs.Or(((Ruleset)ruleset.Flavors[i]).Options);

                    mob.SendMessage(" + {0}", ((Ruleset)ruleset.Flavors[i]).Title);
                }
            }
            else
            {
                defs = basedef.Options;
            }

            int changes = 0;

            BitArray opts = ruleset.Options;

            for (int i = 0; i < opts.Length; ++i)
            {
                if (defs[i] != opts[i])
                {
                    string name = ruleset.Layout.FindByIndex(i);

                    if (name != null) // sanity
                    {
                        ++changes;

                        if (changes == 1)
                        {
                            mob.SendMessage("Modifications:");
                        }

                        mob.SendMessage("{0}: {1}", name, opts[i] ? "enabled" : "disabled");
                    }
                }
            }
        }

        public void SendBeginGump(int count)
        {
            if (!m_Registered || m_Finished)
                return;

            if (count == 10)
            {
                CreateWall();
                BuildParties();
                ClearIllegalItems();
            }
            else if (count == 0)
            {
                DestroyWall();
            }

            m_StartedBeginCountdown = true;

            if (count == 0)
            {
                m_Started = true;
                BeginAutoTie();
            }

            Type[] types = new Type[] { typeof(ReadyGump), typeof(ReadyUpGump), typeof(BeginGump) };

            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant p = (Participant)m_Participants[i];

                for (int j = 0; j < p.Players.Length; ++j)
                {
                    DuelPlayer pl = p.Players[j];

                    if (pl == null)
                        continue;

                    Mobile mob = pl.Mobile;

                    if (count > 0)
                    {
                        if (count == 10)
                            CloseAndSendGump(mob, new BeginGump(count), types);

                        mob.Frozen = true;
                    }
                    else
                    {
                        mob.CloseGump(typeof(BeginGump));
                        mob.Frozen = false;
                    }
                }
            }
        }

        private ArrayList m_Entered = new ArrayList();

        private class ReturnEntry
        {
            private Mobile m_Mobile;
            private Point3D m_Location;
            private Map m_Facet;
            private DateTime m_Expire;

            public Mobile Mobile { get { return m_Mobile; } }
            public Point3D Location { get { return m_Location; } }
            public Map Facet { get { return m_Facet; } }

            public void Return()
            {
                if (m_Facet == Map.Internal || m_Facet == null)
                    return;

                if (m_Mobile.Map == Map.Internal)
                {
                    m_Mobile.LogoutLocation = m_Location;
                    m_Mobile.LogoutMap = m_Facet;
                }
                else
                {
                    m_Mobile.Location = m_Location;
                    m_Mobile.Map = m_Facet;
                }
            }

            public ReturnEntry(Mobile mob)
            {
                m_Mobile = mob;

                Update();
            }

            public ReturnEntry(Mobile mob, Point3D loc, Map facet)
            {
                m_Mobile = mob;
                m_Location = loc;
                m_Facet = facet;
                m_Expire = DateTime.UtcNow + TimeSpan.FromMinutes(30.0);
            }

            public bool Expired { get { return (DateTime.UtcNow >= m_Expire); } }

            public void Update()
            {
                m_Expire = DateTime.UtcNow + TimeSpan.FromMinutes(30.0);

                if (m_Mobile.Map == Map.Internal)
                {
                    m_Facet = m_Mobile.LogoutMap;
                    m_Location = m_Mobile.LogoutLocation;
                }
                else
                {
                    m_Facet = m_Mobile.Map;
                    m_Location = m_Mobile.Location;
                }
            }
        }

        private class ExitTeleporter : Item
        {
            private ArrayList m_Entries;

            public override string DefaultName
            {
                get { return "return teleporter"; }
            }

            public ExitTeleporter()
                : base(0x1822)
            {
                m_Entries = new ArrayList();

                Hue = 0x482;
                Movable = false;
            }

            public void Register(Mobile mob)
            {
                ReturnEntry entry = Find(mob);

                if (entry != null)
                {
                    entry.Update();
                    return;
                }

                m_Entries.Add(new ReturnEntry(mob));
            }

            private ReturnEntry Find(Mobile mob)
            {
                for (int i = 0; i < m_Entries.Count; ++i)
                {
                    ReturnEntry entry = (ReturnEntry)m_Entries[i];

                    if (entry.Mobile == mob)
                        return entry;
                    else if (entry.Expired)
                        m_Entries.RemoveAt(i--);
                }

                return null;
            }

            public override bool OnMoveOver(Mobile m)
            {
                if (!base.OnMoveOver(m))
                    return false;

                ReturnEntry entry = Find(m);

                if (entry != null)
                {
                    entry.Return();

                    Effects.PlaySound(GetWorldLocation(), Map, 0x1FE);
                    Effects.PlaySound(m.Location, m.Map, 0x1FE);

                    m_Entries.Remove(entry);

                    return false;
                }
                else
                {
                    m.SendLocalizedMessage(1049383); // The teleporter doesn't seem to work for you.
                    return true;
                }
            }

            public ExitTeleporter(Serial serial)
                : base(serial)
            {
            }

            public override void Serialize(GenericWriter writer)
            {
                base.Serialize(writer);

                writer.Write((int)0);

                writer.WriteEncodedInt((int)m_Entries.Count);

                for (int i = 0; i < m_Entries.Count; ++i)
                {
                    ReturnEntry entry = (ReturnEntry)m_Entries[i];

                    writer.Write((Mobile)entry.Mobile);
                    writer.Write((Point3D)entry.Location);
                    writer.Write((Map)entry.Facet);

                    if (entry.Expired)
                        m_Entries.RemoveAt(i--);
                }
            }

            public override void Deserialize(GenericReader reader)
            {
                base.Deserialize(reader);

                int version = reader.ReadInt();

                switch (version)
                {
                    case 0:
                        {
                            int count = reader.ReadEncodedInt();

                            m_Entries = new ArrayList(count);

                            for (int i = 0; i < count; ++i)
                            {
                                Mobile mob = reader.ReadMobile();
                                Point3D loc = reader.ReadPoint3D();
                                Map map = reader.ReadMap();

                                m_Entries.Add(new ReturnEntry(mob, loc, map));
                            }

                            break;
                        }
                }
            }
        }

        private class ArenaMoongate : ConfirmationMoongate
        {
            private ExitTeleporter m_Teleporter;

            public override string DefaultName
            {
                get { return "spectator moongate"; }
            }

            public ArenaMoongate(Point3D target, Map map, ExitTeleporter tp)
                : base(target, map)
            {
                m_Teleporter = tp;

                ItemID = 0x1FD4;
                Dispellable = false;

                GumpWidth = 300;
                GumpHeight = 150;
                MessageColor = 0xFFC000;
                MessageString = "Are you sure you wish to spectate this duel?";
                TitleColor = 0x7800;
                TitleNumber = 1062051; // Gate Warning

                Timer.DelayCall(TimeSpan.FromSeconds(10.0), new TimerCallback(Delete));
            }

            public override void CheckGate(Mobile m, int range)
            {
                if (DuelContext.CheckCombat(m))
                {
                    m.SendMessage(0x22, "You have recently been in combat with another player and cannot use this moongate.");
                }
                else
                {
                    base.CheckGate(m, range);
                }
            }

            public override void UseGate(Mobile m)
            {
                if (DuelContext.CheckCombat(m))
                {
                    m.SendMessage(0x22, "You have recently been in combat with another player and cannot use this moongate.");
                }
                else
                {
                    if (m_Teleporter != null && !m_Teleporter.Deleted)
                        m_Teleporter.Register(m);

                    base.UseGate(m);
                }
            }

            public void Appear(Point3D loc, Map map)
            {
                Effects.PlaySound(loc, map, 0x20E);
                MoveToWorld(loc, map);
            }

            public ArenaMoongate(Serial serial)
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

                Delete();
            }
        }

        public void RemoveAggressions(Mobile mob)
        {
            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant p = (Participant)m_Participants[i];

                for (int j = 0; j < p.Players.Length; ++j)
                {
                    DuelPlayer dp = (DuelPlayer)p.Players[j];

                    if (dp == null || dp.Mobile == mob)
                        continue;

                    mob.RemoveAggressed(dp.Mobile);
                    mob.RemoveAggressor(dp.Mobile);
                    dp.Mobile.RemoveAggressed(mob);
                    dp.Mobile.RemoveAggressor(mob);
                }
            }
        }

        public void SendReadyUpGump()
        {
            if (!m_Registered)
                return;

            m_ReadyWait = true;
            m_ReadyCount = -1;

            Type[] types = new Type[] { typeof(ReadyUpGump) };

            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant p = (Participant)m_Participants[i];

                for (int j = 0; j < p.Players.Length; ++j)
                {
                    DuelPlayer pl = p.Players[j];

                    if (pl == null)
                        continue;

                    Mobile mob = pl.Mobile;

                    if (mob != null)
                    {
                        if (m_Tournament == null)
                            CloseAndSendGump(mob, new ReadyUpGump(mob, this), types);
                    }
                }
            }
        }

        public string ValidateStart()
        {
            if (m_Tournament == null && TournamentController.IsActive)
                return "a tournament is active";

            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant p = (Participant)m_Participants[i];

                for (int j = 0; j < p.Players.Length; ++j)
                {
                    DuelPlayer dp = p.Players[j];

                    if (dp == null)
                        return "a slot is empty";

                    if (dp.Mobile.Region.IsPartOf(typeof(Regions.Jail)))
                        return String.Format("{0} is in jail", dp.Mobile.Name);

                    if (Sigil.ExistsOn(dp.Mobile))
                        return String.Format("{0} is holding a sigil", dp.Mobile.Name);

                    if (!dp.Mobile.Alive)
                    {
                        if (m_Tournament == null)
                            return String.Format("{0} is dead", dp.Mobile.Name);
                        else
                            dp.Mobile.Resurrect();
                    }

                    if (m_Tournament == null && CheckCombat(dp.Mobile))
                        return String.Format("{0} is in combat", dp.Mobile.Name);

                    if (dp.Mobile.Mounted)
                    {
                        IMount mount = dp.Mobile.Mount;

                        if (m_Tournament != null && mount != null)
                            mount.Rider = null;
                        else
                            return String.Format("{0} is mounted", dp.Mobile.Name);
                    }
                }
            }

            return null;
        }

        public Arena m_OverrideArena;
        public Tournament m_Tournament;
        public TournyMatch m_Match;
        public EventGame m_EventGame;

        public Tournament Tournament { get { return m_Tournament; } }

        public void SendReadyGump(int count)
        {
            if (!m_Registered)
                return;

            if (count != -1)
                m_StartedReadyCountdown = true;

            m_ReadyCount = count;

            if (count == 0)
            {
                string error = ValidateStart();

                if (error != null)
                {
                    for (int i = 0; i < m_Participants.Count; ++i)
                    {
                        Participant p = (Participant)m_Participants[i];

                        for (int j = 0; j < p.Players.Length; ++j)
                        {
                            DuelPlayer dp = p.Players[j];

                            if (dp != null)
                                dp.Mobile.SendMessage("The duel could not be started because {0}.", error);
                        }
                    }

                    StartCountdown(10, new CountdownCallback(SendReadyGump));

                    return;
                }

                m_ReadyWait = false;

                List<Mobile> players = new List<Mobile>();

                for (int i = 0; i < m_Participants.Count; ++i)
                {
                    Participant p = (Participant)m_Participants[i];

                    for (int j = 0; j < p.Players.Length; ++j)
                    {
                        DuelPlayer dp = p.Players[j];

                        if (dp != null)
                            players.Add(dp.Mobile);
                    }
                }

                Arena arena = m_OverrideArena;

                if (arena == null)
                    arena = Arena.FindArena(players);

                if (arena == null)
                {
                    for (int i = 0; i < m_Participants.Count; ++i)
                    {
                        Participant p = (Participant)m_Participants[i];

                        for (int j = 0; j < p.Players.Length; ++j)
                        {
                            DuelPlayer dp = p.Players[j];

                            if (dp != null)
                                dp.Mobile.SendMessage("The duel could not be started because there are no arenas. If you want to stop waiting for a free arena, yield the duel.");
                        }
                    }

                    StartCountdown(10, new CountdownCallback(SendReadyGump));
                    return;
                }

                if (!arena.IsOccupied)
                {
                    m_Arena = arena;

                    if (m_Initiator.Map == Map.Internal)
                    {
                        m_GatePoint = m_Initiator.LogoutLocation;
                        m_GateFacet = m_Initiator.LogoutMap;
                    }
                    else
                    {
                        m_GatePoint = m_Initiator.Location;
                        m_GateFacet = m_Initiator.Map;
                    }

                    ExitTeleporter tp = arena.Teleporter as ExitTeleporter;

                    if (tp == null)
                    {
                        arena.Teleporter = tp = new ExitTeleporter();
                        tp.MoveToWorld(arena.GateOut == Point3D.Zero ? arena.Outside : arena.GateOut, arena.Facet);
                    }

                    ArenaMoongate mg = new ArenaMoongate(arena.GateIn == Point3D.Zero ? arena.Outside : arena.GateIn, arena.Facet, tp);

                    m_StartedBeginCountdown = true;

                    for (int i = 0; i < m_Participants.Count; ++i)
                    {
                        Participant p = (Participant)m_Participants[i];

                        for (int j = 0; j < p.Players.Length; ++j)
                        {
                            DuelPlayer pl = p.Players[j];

                            if (pl == null)
                                continue;

                            tp.Register(pl.Mobile);

                            pl.Mobile.Frozen = false; // reset timer just in case
                            pl.Mobile.Frozen = true;

                            Debuff(pl.Mobile);
                            CancelSpell(pl.Mobile);

                            pl.Mobile.Delta(MobileDelta.Noto);
                        }

                        arena.MoveInside(p.Players, i);
                    }

                    if (m_EventGame != null)
                        m_EventGame.OnStart();

                    StartCountdown(10, new CountdownCallback(SendBeginGump));

                    mg.Appear(m_GatePoint, m_GateFacet);
                }
                else
                {
                    for (int i = 0; i < m_Participants.Count; ++i)
                    {
                        Participant p = (Participant)m_Participants[i];

                        for (int j = 0; j < p.Players.Length; ++j)
                        {
                            DuelPlayer dp = p.Players[j];

                            if (dp != null)
                                dp.Mobile.SendMessage("The duel could not be started because all arenas are full. If you want to stop waiting for a free arena, yield the duel.");
                        }
                    }

                    StartCountdown(10, new CountdownCallback(SendReadyGump));
                }

                return;
            }

            m_ReadyWait = true;

            bool isAllReady = true;

            Type[] types = new Type[] { typeof(ReadyGump) };

            for (int i = 0; i < m_Participants.Count; ++i)
            {
                Participant p = (Participant)m_Participants[i];

                for (int j = 0; j < p.Players.Length; ++j)
                {
                    DuelPlayer pl = p.Players[j];

                    if (pl == null)
                        continue;

                    Mobile mob = pl.Mobile;

                    if (pl.Ready)
                    {
                        if (m_Tournament == null)
                            CloseAndSendGump(mob, new ReadyGump(mob, this, count), types);
                    }
                    else
                    {
                        isAllReady = false;
                    }
                }
            }

            if (count == -1 && isAllReady)
                StartCountdown(3, new CountdownCallback(SendReadyGump));
        }

        public static void CloseAndSendGump(Mobile mob, Gump g, params Type[] types)
        {
            CloseAndSendGump(mob.NetState, g, types);
        }

        public static void CloseAndSendGump(NetState ns, Gump g, params Type[] types)
        {
            if (ns != null)
            {
                Mobile mob = ns.Mobile;

                if (mob != null)
                {
                    foreach (Type type in types)
                    {
                        mob.CloseGump(type);
                    }

                    mob.SendGump(g);
                }
            }

            /*if ( ns == null )
                return;

            for ( int i = 0; i < types.Length; ++i )
                ns.Send( new CloseGump( Gump.GetTypeID( types[i] ), 0 ) );

            g.SendTo( ns );

            ns.AddGump( g );

            Packet[] packets = new Packet[types.Length + 1];

            for ( int i = 0; i < types.Length; ++i )
                packets[i] = new CloseGump( Gump.GetTypeID( types[i] ), 0 );

            packets[types.Length] = (Packet) typeof( Gump ).InvokeMember( "Compile", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, g, null, null );

            bool compress = ns.CompressionEnabled;
            ns.CompressionEnabled = false;
            ns.Send( BindPackets( compress, packets ) );
            ns.CompressionEnabled = compress;*/
        }

        /*public static Packet BindPackets( bool compress, params Packet[] packets )
        {
            if ( packets.Length == 0 )
                throw new ArgumentException( "No packets to bind", "packets" );

            byte[][] compiled = new byte[packets.Length][];
            int[] lengths = new int[packets.Length];

            int length = 0;

            for ( int i = 0; i < packets.Length; ++i )
            {
                compiled[i] = packets[i].Compile( compress, out lengths[i] );
                length += lengths[i];
            }

            return new BoundPackets( length, compiled, lengths );
        }

        private class BoundPackets : Packet
        {
            public BoundPackets( int length, byte[][] compiled, int[] lengths ) : base( 0, length )
            {
                m_Stream.Seek( 0, System.IO.SeekOrigin.Begin );

                for ( int i = 0; i < compiled.Length; ++i )
                    m_Stream.Write( compiled[i], 0, lengths[i] );
            }
        }*/
    }

    public class DuelContextGump : Gump
    {
        private Mobile m_From;
        private DuelContext m_Context;

        public Mobile From { get { return m_From; } }
        public DuelContext Context { get { return m_Context; } }

        public string Center(string text)
        {
            return String.Format("<CENTER>{0}</CENTER>", text);
        }

        public void AddGoldenButton(int x, int y, int bid)
        {
            AddButton(x, y, 0xD2, 0xD2, bid, GumpButtonType.Reply, 0);
            AddButton(x + 3, y + 3, 0xD8, 0xD8, bid, GumpButtonType.Reply, 0);
        }

        public void AddGoldenButtonLabeled(int x, int y, int bid, string text)
        {
            AddGoldenButton(x, y, bid);
            AddHtml(x + 25, y, 200, 20, text, false, false);
        }

        public DuelContextGump(Mobile from, DuelContext context)
            : base(50, 50)
        {
            m_From = from;
            m_Context = context;

            from.CloseGump(typeof(RulesetGump));
            from.CloseGump(typeof(DuelContextGump));
            from.CloseGump(typeof(ParticipantGump));

            int count = context.Participants.Count;

            if (count < 3)
                count = 3;

            int height = 35 + 10 + 22 + 30 + 22 + 22 + 2 + (count * 22) + 2 + 30;

            AddPage(0);

            AddBackground(0, 0, 300, height, 9250);
            AddBackground(10, 10, 280, height - 20, 0xDAC);

            AddHtml(35, 25, 230, 20, Center("Duel Setup"), false, false);

            int x = 35;
            int y = 47;

            AddGoldenButtonLabeled(x, y, 1, "Rules"); y += 22;
            AddGoldenButtonLabeled(x, y, 2, "Start"); y += 22;
            AddGoldenButtonLabeled(x, y, 3, "Add Participant"); y += 30;

            AddHtml(35, y, 230, 20, Center("Participants"), false, false); y += 22;

            for (int i = 0; i < context.Participants.Count; ++i)
            {
                Participant p = (Participant)context.Participants[i];

                AddGoldenButtonLabeled(x, y, 4 + i, String.Format(p.Count == 1 ? "Player {0}: {3}" : "Team {0}: {1}/{2}: {3}", 1 + i, p.FilledSlots, p.Count, p.NameList)); y += 22;
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (!m_Context.Registered)
                return;

            int index = info.ButtonID;

            switch (index)
            {
                case -1: // CloseGump
                    {
                        break;
                    }
                case 0: // closed
                    {
                        m_Context.Unregister();
                        break;
                    }
                case 1: // Rules
                    {
                        //m_From.SendGump( new RulesetGump( m_From, m_Context.Ruleset, m_Context.Ruleset.Layout, m_Context ) );
                        m_From.SendGump(new PickRulesetGump(m_From, m_Context, m_Context.Ruleset));
                        break;
                    }
                case 2: // Start
                    {
                        if (m_Context.CheckFull())
                        {
                            m_Context.CloseAllGumps();
                            m_Context.SendReadyUpGump();
                            //m_Context.SendReadyGump();
                        }
                        else
                        {
                            m_From.SendMessage("You cannot start the duel before all participating players have been assigned.");
                            m_From.SendGump(new DuelContextGump(m_From, m_Context));
                        }

                        break;
                    }
                case 3: // New Participant
                    {
                        if (m_Context.Participants.Count < 10)
                            m_Context.Participants.Add(new Participant(m_Context, 1));
                        else
                            m_From.SendMessage("The number of participating parties may not be increased further.");

                        m_From.SendGump(new DuelContextGump(m_From, m_Context));

                        break;
                    }
                default: // Participant
                    {
                        index -= 4;

                        if (index >= 0 && index < m_Context.Participants.Count)
                            m_From.SendGump(new ParticipantGump(m_From, m_Context, (Participant)m_Context.Participants[index]));

                        break;
                    }
            }
        }
    }

    #region Event Arena Guidelines

    public class Ruleset
    {
        private RulesetLayout m_Layout;
        private BitArray m_Options;
        private string m_Title;

        private Ruleset m_Base;
        private ArrayList m_Flavors = new ArrayList();
        private bool m_Changed;

        public RulesetLayout Layout { get { return m_Layout; } }
        public BitArray Options { get { return m_Options; } }
        public string Title { get { return m_Title; } set { m_Title = value; } }

        public Ruleset Base { get { return m_Base; } }
        public ArrayList Flavors { get { return m_Flavors; } }
        public bool Changed { get { return m_Changed; } set { m_Changed = value; } }

        public void ApplyDefault(Ruleset newDefault)
        {
            m_Base = newDefault;
            m_Changed = false;

            m_Options = new BitArray(newDefault.m_Options);

            ApplyFlavorsTo(this);
        }

        public void ApplyFlavorsTo(Ruleset ruleset)
        {
            for (int i = 0; i < m_Flavors.Count; ++i)
            {
                Ruleset flavor = (Ruleset)m_Flavors[i];

                m_Options.Or(flavor.m_Options);
            }
        }

        public void AddFlavor(Ruleset flavor)
        {
            if (m_Flavors.Contains(flavor))
                return;

            m_Flavors.Add(flavor);
            m_Options.Or(flavor.m_Options);
        }

        public void RemoveFlavor(Ruleset flavor)
        {
            if (!m_Flavors.Contains(flavor))
                return;

            m_Flavors.Remove(flavor);
            m_Options.And(flavor.m_Options.Not());
            flavor.m_Options.Not();
        }

        public void SetOptionRange(string title, bool value)
        {
            RulesetLayout layout = m_Layout.FindByTitle(title);

            if (layout == null)
                return;

            for (int i = 0; i < layout.TotalLength; ++i)
                m_Options[i + layout.Offset] = value;

            m_Changed = true;
        }

        public bool GetOption(string title, string option)
        {
            int index = 0;
            RulesetLayout layout = m_Layout.FindByOption(title, option, ref index);

            if (layout == null)
                return true;

            return m_Options[layout.Offset + index];
        }

        public void SetOption(string title, string option, bool value)
        {
            int index = 0;
            RulesetLayout layout = m_Layout.FindByOption(title, option, ref index);

            if (layout == null)
                return;

            m_Options[layout.Offset + index] = value;

            m_Changed = true;
        }

        public Ruleset(RulesetLayout layout)
        {
            m_Layout = layout;
            m_Options = new BitArray(layout.TotalLength);
        }
    }

    public class RulesetLayout
    {
        private static RulesetLayout m_Root;

        public static RulesetLayout Root
        {
            get
            {
                if (m_Root == null)
                {
                    ArrayList entries = new ArrayList();

                    entries.Add(new RulesetLayout("Spells", new RulesetLayout[]
						{
							new RulesetLayout( "1st Circle", "Spells", new string[]
							{
								"Reactive Armor", "Clumsy",
								"Create Food", "Feeblemind",
								"Heal", "Magic Arrow",
								"Night Sight", "Weaken"
							} ),
							new RulesetLayout( "2nd Circle", "Spells", new string[]
							{
								"Agility", "Cunning",
								"Cure", "Harm",
								"Magic Trap", "Untrap",
								"Protection", "Strength"
							} ),
							new RulesetLayout( "3rd Circle", "Spells", new string[]
							{
								"Bless", "Fireball",
								"Magic Lock", "Poison",
								"Telekinesis", "Teleport",
								"Unlock Spell", "Wall of Stone"
							} ),
							new RulesetLayout( "4th Circle", "Spells", new string[]
							{
								"Arch Cure", "Arch Protection",
								"Curse", "Fire Field",
								"Greater Heal", "Lightning",
								"Mana Drain", "Recall"
							} ),
							new RulesetLayout( "5th Circle", "Spells", new string[]
							{
								"Blade Spirits", "Dispel Field",
								"Incognito", "Magic Reflection",
								"Mind Blast", "Paralyze",
								"Poison Field", "Summon Creature"
							} ),
							new RulesetLayout( "6th Circle", "Spells", new string[]
							{
								"Dispel", "Energy Bolt",
								"Explosion", "Invisibility",
								"Mark", "Mass Curse",
								"Paralyze Field", "Reveal"
							} ),
							new RulesetLayout( "7th Circle", "Spells", new string[]
							{
								"Chain Lightning", "Energy Field",
								"Flame Strike", "Gate Travel",
								"Mana Vampire", "Mass Dispel",
								"Meteor Swarm", "Polymorph"
							} ),
							new RulesetLayout( "8th Circle", "Spells", new string[]
							{
								"Earthquake", "Energy Vortex",
								"Resurrection", "Air Elemental",
								"Summon Daemon", "Earth Elemental",
								"Fire Elemental", "Water Elemental"
							} )
						}));

                    if (Core.AOS)
                    {
                        entries.Add(new RulesetLayout("Chivalry", new string[]
							{
								"Cleanse by Fire",
								"Close Wounds",
								"Consecrate Weapon",
								"Dispel Evil",
								"Divine Fury",
								"Enemy of One",
								"Holy Light",
								"Noble Sacrifice",
								"Remove Curse",
								"Sacred Journey"
							}));

                        entries.Add(new RulesetLayout("Necromancy", new string[]
							{
								"Animate Dead",
								"Blood Oath",
								"Corpse Skin",
								"Curse Weapon",
								"Evil Omen",
								"Horrific Beast",
								"Lich Form",
								"Mind Rot",
								"Pain Spike",
								"Poison Strike",
								"Strangle",
								"Summon Familiar",
								"Vampiric Embrace",
								"Vengeful Spirit",
								"Wither",
								"Wraith Form"
							}));

                        if (Core.SE)
                        {
                            entries.Add(new RulesetLayout("Bushido", new string[]
							{
								"Confidence",
								"Counter Attack",
								"Evasion",
								"Honorable Execution",
								"Lightning Strike",
								"Momentum Strike"
							}));

                            entries.Add(new RulesetLayout("Ninjitsu", new string[]
							{
								"Animal Form",
								"Backstab",
								"Death Strike",
								"Focus Attack",
								"Ki Attack",
								"Mirror Image",
								"Shadow Jump",
								"Suprise Attack"
							}));

                            if (Core.ML)
                            {
                                entries.Add(new RulesetLayout("Spellweaving", new string[]
									{
										"Arcane Circle",
										"Arcane Empowerment",
										"Attune Weapon",
										"Dryad Allure",
										"Essence of Wind",
										"Ethereal Voyage",
										"Gift of Life",
										"Gift of Renewal",
										"Immolating Weapon",
										"Nature's Fury",
										"Reaper Form",
										"Summon Fey",
										"Summon Fiend",
										"Thunderstorm",
										"Wildfire",
										"Word of Death"
									}));
                            }
                        }
                    }

                    if (Core.AOS)
                    {
                        if (Core.SE)
                        {
                            entries.Add(new RulesetLayout("Combat Abilities", new string[]
							{
								"Stun",
								"Disarm",
								"Armor Ignore",
								"Bleed Attack",
								"Concussion Blow",
								"Crushing Blow",
								"Disarm",
								"Dismount",
								"Double Strike",
								"Infectious Strike",
								"Mortal Strike",
								"Moving Shot",
								"Paralyzing Blow",
								"Shadow Strike",
								"Whirlwind Attack",
								"Riding Swipe",
								"Frenzied Whirlwind",
								"Block",
								"Defense Mastery",
								"Nerve Strike",
								"Talon Strike",
								"Feint",
								"Dual Wield",
								"Double Shot",
								"Armor Pierce"
							}));

                            //TODO: ADD ML ABILITIES
                        }
                        else
                        {
                            entries.Add(new RulesetLayout("Combat Abilities", new string[]
							{
								"Stun",
								"Disarm",
								"Armor Ignore",
								"Bleed Attack",
								"Concussion Blow",
								"Crushing Blow",
								"Disarm",
								"Dismount",
								"Double Strike",
								"Infectious Strike",
								"Mortal Strike",
								"Moving Shot",
								"Paralyzing Blow",
								"Shadow Strike",
								"Whirlwind Attack"
							}));
                        }
                    }
                    else
                    {
                        entries.Add(new RulesetLayout("Combat Abilities", new string[]
							{
								"Stun",
								"Disarm",
								"Concussion Blow",
								"Crushing Blow",
								"Paralyzing Blow"
							}));
                    }

                    entries.Add(new RulesetLayout("Skills", new string[]
						{
							"Anatomy",
							"Detect Hidden",
							"Evaluating Intelligence",
							"Hiding",
							"Poisoning",
							"Snooping",
							"Stealing",
							"Spirit Speak",
							"Stealth"
						}));

                    if (Core.AOS)
                    {
                        entries.Add(new RulesetLayout("Weapons", new string[]
						{
							"Magical",
							"Melee",
							"Ranged",
							"Poisoned",
							"Wrestling"
						}));

                        entries.Add(new RulesetLayout("Armor", new string[]
							{
								"Magical",
								"Shields"
							}));
                    }
                    else
                    {
                        entries.Add(new RulesetLayout("Weapons", new string[]
						{
							"Magical",
							"Melee",
							"Ranged",
							"Poisoned",
							"Wrestling",
							"Runics"
						}));

                        entries.Add(new RulesetLayout("Armor", new string[]
							{
								"Magical",
								"Shields",
								"Colored"
							}));
                    }

                    if (Core.SE)
                    {
                        entries.Add(new RulesetLayout("Items", new RulesetLayout[]
						{
							new RulesetLayout( "Potions", new string[]
							{
								"Agility",
								"Cure",
								"Explosion",
								"Heal",
								"Nightsight",
								"Poison",
								"Refresh",
								"Strength"
							} )
						},
                        new string[]
						{
							"Bandages",
							"Wands",
							"Trapped Containers",
							"Bolas",
							"Mounts",
							"Orange Petals",
							"Shurikens",
							"Fukiya Darts",
							"Fire Horns"
						}));
                    }
                    else
                    {
                        entries.Add(new RulesetLayout("Items", new RulesetLayout[]
						{
							new RulesetLayout( "Potions", new string[]
							{
								"Agility",
								"Cure",
								"Explosion",
								"Heal",
								"Nightsight",
								"Poison",
								"Refresh",
								"Strength"
							} )
						},
                            new string[]
						{
							"Bandages",
							"Wands",
							"Trapped Containers",
							"Bolas",
							"Mounts",
							"Orange Petals",
							"Fire Horns"
						}));
                    }

                    m_Root = new RulesetLayout("Rules", (RulesetLayout[])entries.ToArray(typeof(RulesetLayout)));
                    m_Root.ComputeOffsets();

                    // Set up default rulesets

                    if (!Core.AOS)
                    {
                        #region Mage 5x
                        Ruleset m5x = new Ruleset(m_Root);

                        m5x.Title = "Mage 5x";

                        m5x.SetOptionRange("Spells", true);

                        m5x.SetOption("Spells", "Wall of Stone", false);
                        m5x.SetOption("Spells", "Fire Field", false);
                        m5x.SetOption("Spells", "Poison Field", false);
                        m5x.SetOption("Spells", "Energy Field", false);
                        m5x.SetOption("Spells", "Reactive Armor", false);
                        m5x.SetOption("Spells", "Protection", false);
                        m5x.SetOption("Spells", "Teleport", false);
                        m5x.SetOption("Spells", "Wall of Stone", false);
                        m5x.SetOption("Spells", "Arch Protection", false);
                        m5x.SetOption("Spells", "Recall", false);
                        m5x.SetOption("Spells", "Blade Spirits", false);
                        m5x.SetOption("Spells", "Incognito", false);
                        m5x.SetOption("Spells", "Magic Reflection", false);
                        m5x.SetOption("Spells", "Paralyze", false);
                        m5x.SetOption("Spells", "Summon Creature", false);
                        m5x.SetOption("Spells", "Invisibility", false);
                        m5x.SetOption("Spells", "Mark", false);
                        m5x.SetOption("Spells", "Paralyze Field", false);
                        m5x.SetOption("Spells", "Energy Field", false);
                        m5x.SetOption("Spells", "Gate Travel", false);
                        m5x.SetOption("Spells", "Polymorph", false);
                        m5x.SetOption("Spells", "Energy Vortex", false);
                        m5x.SetOption("Spells", "Air Elemental", false);
                        m5x.SetOption("Spells", "Summon Daemon", false);
                        m5x.SetOption("Spells", "Earth Elemental", false);
                        m5x.SetOption("Spells", "Fire Elemental", false);
                        m5x.SetOption("Spells", "Water Elemental", false);
                        m5x.SetOption("Spells", "Earthquake", false);
                        m5x.SetOption("Spells", "Meteor Swarm", false);
                        m5x.SetOption("Spells", "Chain Lightning", false);
                        m5x.SetOption("Spells", "Resurrection", false);

                        m5x.SetOption("Weapons", "Wrestling", true);

                        m5x.SetOption("Skills", "Anatomy", true);
                        m5x.SetOption("Skills", "Detect Hidden", true);
                        m5x.SetOption("Skills", "Evaluating Intelligence", true);

                        m5x.SetOption("Items", "Trapped Containers", true);
                        #endregion

                        #region Mage 7x
                        Ruleset m7x = new Ruleset(m_Root);

                        m7x.Title = "Mage 7x";

                        m7x.SetOptionRange("Spells", true);

                        m7x.SetOption("Spells", "Wall of Stone", false);
                        m7x.SetOption("Spells", "Fire Field", false);
                        m7x.SetOption("Spells", "Poison Field", false);
                        m7x.SetOption("Spells", "Energy Field", false);
                        m7x.SetOption("Spells", "Reactive Armor", false);
                        m7x.SetOption("Spells", "Protection", false);
                        m7x.SetOption("Spells", "Teleport", false);
                        m7x.SetOption("Spells", "Wall of Stone", false);
                        m7x.SetOption("Spells", "Arch Protection", false);
                        m7x.SetOption("Spells", "Recall", false);
                        m7x.SetOption("Spells", "Blade Spirits", false);
                        m7x.SetOption("Spells", "Incognito", false);
                        m7x.SetOption("Spells", "Magic Reflection", false);
                        m7x.SetOption("Spells", "Paralyze", false);
                        m7x.SetOption("Spells", "Summon Creature", false);
                        m7x.SetOption("Spells", "Invisibility", false);
                        m7x.SetOption("Spells", "Mark", false);
                        m7x.SetOption("Spells", "Paralyze Field", false);
                        m7x.SetOption("Spells", "Energy Field", false);
                        m7x.SetOption("Spells", "Gate Travel", false);
                        m7x.SetOption("Spells", "Polymorph", false);
                        m7x.SetOption("Spells", "Energy Vortex", false);
                        m7x.SetOption("Spells", "Air Elemental", false);
                        m7x.SetOption("Spells", "Summon Daemon", false);
                        m7x.SetOption("Spells", "Earth Elemental", false);
                        m7x.SetOption("Spells", "Fire Elemental", false);
                        m7x.SetOption("Spells", "Water Elemental", false);
                        m7x.SetOption("Spells", "Earthquake", false);
                        m7x.SetOption("Spells", "Meteor Swarm", false);
                        m7x.SetOption("Spells", "Chain Lightning", false);
                        m7x.SetOption("Spells", "Resurrection", false);

                        m7x.SetOption("Combat Abilities", "Stun", true);

                        m7x.SetOption("Skills", "Anatomy", true);
                        m7x.SetOption("Skills", "Detect Hidden", true);
                        m7x.SetOption("Skills", "Poisoning", true);
                        m7x.SetOption("Skills", "Evaluating Intelligence", true);

                        m7x.SetOption("Weapons", "Wrestling", true);

                        m7x.SetOption("Potions", "Refresh", true);
                        m7x.SetOption("Items", "Trapped Containers", true);
                        m7x.SetOption("Items", "Bandages", true);
                        #endregion

                        #region Standard 7x
                        Ruleset s7x = new Ruleset(m_Root);

                        s7x.Title = "Standard 7x";

                        s7x.SetOptionRange("Spells", true);

                        s7x.SetOption("Spells", "Wall of Stone", false);
                        s7x.SetOption("Spells", "Fire Field", false);
                        s7x.SetOption("Spells", "Poison Field", false);
                        s7x.SetOption("Spells", "Energy Field", false);
                        s7x.SetOption("Spells", "Teleport", false);
                        s7x.SetOption("Spells", "Wall of Stone", false);
                        s7x.SetOption("Spells", "Arch Protection", false);
                        s7x.SetOption("Spells", "Recall", false);
                        s7x.SetOption("Spells", "Blade Spirits", false);
                        s7x.SetOption("Spells", "Incognito", false);
                        s7x.SetOption("Spells", "Magic Reflection", false);
                        s7x.SetOption("Spells", "Paralyze", false);
                        s7x.SetOption("Spells", "Summon Creature", false);
                        s7x.SetOption("Spells", "Invisibility", false);
                        s7x.SetOption("Spells", "Mark", false);
                        s7x.SetOption("Spells", "Paralyze Field", false);
                        s7x.SetOption("Spells", "Energy Field", false);
                        s7x.SetOption("Spells", "Gate Travel", false);
                        s7x.SetOption("Spells", "Polymorph", false);
                        s7x.SetOption("Spells", "Energy Vortex", false);
                        s7x.SetOption("Spells", "Air Elemental", false);
                        s7x.SetOption("Spells", "Summon Daemon", false);
                        s7x.SetOption("Spells", "Earth Elemental", false);
                        s7x.SetOption("Spells", "Fire Elemental", false);
                        s7x.SetOption("Spells", "Water Elemental", false);
                        s7x.SetOption("Spells", "Earthquake", false);
                        s7x.SetOption("Spells", "Meteor Swarm", false);
                        s7x.SetOption("Spells", "Chain Lightning", false);
                        s7x.SetOption("Spells", "Resurrection", false);

                        s7x.SetOptionRange("Combat Abilities", true);

                        s7x.SetOption("Skills", "Anatomy", true);
                        s7x.SetOption("Skills", "Detect Hidden", true);
                        s7x.SetOption("Skills", "Poisoning", true);
                        s7x.SetOption("Skills", "Evaluating Intelligence", true);

                        s7x.SetOptionRange("Weapons", true);
                        s7x.SetOption("Weapons", "Runics", false);
                        s7x.SetOptionRange("Armor", true);

                        s7x.SetOption("Potions", "Refresh", true);
                        s7x.SetOption("Items", "Bandages", true);
                        s7x.SetOption("Items", "Trapped Containers", true);
                        #endregion

                        m_Root.Defaults = new Ruleset[] { m5x, m7x, s7x };
                    }
                    else
                    {
                        #region Standard All Skills

                        Ruleset all = new Ruleset(m_Root);

                        all.Title = "Standard All Skills";


                        all.SetOptionRange("Spells", true);

                        all.SetOption("Spells", "Wall of Stone", false);
                        all.SetOption("Spells", "Fire Field", false);
                        all.SetOption("Spells", "Poison Field", false);
                        all.SetOption("Spells", "Energy Field", false);
                        all.SetOption("Spells", "Teleport", false);
                        all.SetOption("Spells", "Wall of Stone", false);
                        all.SetOption("Spells", "Arch Protection", false);
                        all.SetOption("Spells", "Recall", false);
                        all.SetOption("Spells", "Blade Spirits", false);
                        all.SetOption("Spells", "Incognito", false);
                        all.SetOption("Spells", "Magic Reflection", false);
                        all.SetOption("Spells", "Paralyze", false);
                        all.SetOption("Spells", "Summon Creature", false);
                        all.SetOption("Spells", "Invisibility", false);
                        all.SetOption("Spells", "Mark", false);
                        all.SetOption("Spells", "Paralyze Field", false);
                        all.SetOption("Spells", "Energy Field", false);
                        all.SetOption("Spells", "Gate Travel", false);
                        all.SetOption("Spells", "Polymorph", false);
                        all.SetOption("Spells", "Energy Vortex", false);
                        all.SetOption("Spells", "Air Elemental", false);
                        all.SetOption("Spells", "Summon Daemon", false);
                        all.SetOption("Spells", "Earth Elemental", false);
                        all.SetOption("Spells", "Fire Elemental", false);
                        all.SetOption("Spells", "Water Elemental", false);
                        all.SetOption("Spells", "Earthquake", false);
                        all.SetOption("Spells", "Meteor Swarm", false);
                        all.SetOption("Spells", "Chain Lightning", false);
                        all.SetOption("Spells", "Resurrection", false);

                        all.SetOptionRange("Necromancy", true);
                        all.SetOption("Necromancy", "Summon Familiar", false);
                        all.SetOption("Necromancy", "Vengeful Spirit", false);
                        all.SetOption("Necromancy", "Animate Dead", false);
                        all.SetOption("Necromancy", "Wither", false);
                        all.SetOption("Necromancy", "Poison Strike", false);

                        all.SetOptionRange("Chivalry", true);
                        all.SetOption("Chivalry", "Sacred Journey", false);
                        all.SetOption("Chivalry", "Enemy of One", false);
                        all.SetOption("Chivalry", "Noble Sacrifice", false);

                        all.SetOptionRange("Combat Abilities", true);
                        all.SetOption("Combat Abilities", "Paralyzing Blow", false);
                        all.SetOption("Combat Abilities", "Shadow Strike", false);

                        all.SetOption("Skills", "Anatomy", true);
                        all.SetOption("Skills", "Detect Hidden", true);
                        all.SetOption("Skills", "Poisoning", true);
                        all.SetOption("Skills", "Spirit Speak", true);
                        all.SetOption("Skills", "Evaluating Intelligence", true);

                        all.SetOptionRange("Weapons", true);
                        all.SetOption("Weapons", "Poisoned", false);

                        all.SetOptionRange("Armor", true);

                        all.SetOptionRange("Ninjitsu", true);
                        all.SetOption("Ninjitsu", "Animal Form", false);
                        all.SetOption("Ninjitsu", "Mirror Image", false);
                        all.SetOption("Ninjitsu", "Backstab", false);
                        all.SetOption("Ninjitsu", "Suprise Attack", false);
                        all.SetOption("Ninjitsu", "Shadow Jump", false);

                        all.SetOptionRange("Bushido", true);

                        all.SetOptionRange("Spellweaving", true);
                        all.SetOption("Spellweaving", "Gift of Life", false);
                        all.SetOption("Spellweaving", "Summon Fey", false);
                        all.SetOption("Spellweaving", "Summon Fiend", false);
                        all.SetOption("Spellweaving", "Nature's Fury", false);

                        all.SetOption("Potions", "Refresh", true);
                        all.SetOption("Items", "Bandages", true);
                        all.SetOption("Items", "Trapped Containers", true);

                        m_Root.Defaults = new Ruleset[] { all };
                        #endregion
                    }

                    // Set up flavors

                    Ruleset pots = new Ruleset(m_Root);

                    pots.Title = "Potions";

                    pots.SetOptionRange("Potions", true);
                    pots.SetOption("Potions", "Explosion", false);

                    Ruleset para = new Ruleset(m_Root);

                    para.Title = "Paralyze";
                    para.SetOption("Spells", "Paralyze", true);
                    para.SetOption("Spells", "Paralyze Field", true);
                    para.SetOption("Combat Abilities", "Paralyzing Blow", true);

                    Ruleset fields = new Ruleset(m_Root);

                    fields.Title = "Fields";
                    fields.SetOption("Spells", "Wall of Stone", true);
                    fields.SetOption("Spells", "Fire Field", true);
                    fields.SetOption("Spells", "Poison Field", true);
                    fields.SetOption("Spells", "Energy Field", true);
                    fields.SetOption("Spells", "Wildfire", true);

                    Ruleset area = new Ruleset(m_Root);

                    area.Title = "Area Effect";
                    area.SetOption("Spells", "Earthquake", true);
                    area.SetOption("Spells", "Meteor Swarm", true);
                    area.SetOption("Spells", "Chain Lightning", true);
                    area.SetOption("Necromancy", "Wither", true);
                    area.SetOption("Necromancy", "Poison Strike", true);

                    Ruleset summons = new Ruleset(m_Root);

                    summons.Title = "Summons";
                    summons.SetOption("Spells", "Blade Spirits", true);
                    summons.SetOption("Spells", "Energy Vortex", true);
                    summons.SetOption("Spells", "Air Elemental", true);
                    summons.SetOption("Spells", "Summon Daemon", true);
                    summons.SetOption("Spells", "Earth Elemental", true);
                    summons.SetOption("Spells", "Fire Elemental", true);
                    summons.SetOption("Spells", "Water Elemental", true);
                    summons.SetOption("Necromancy", "Summon Familiar", true);
                    summons.SetOption("Necromancy", "Vengeful Spirit", true);
                    summons.SetOption("Necromancy", "Animate Dead", true);
                    summons.SetOption("Ninjitsu", "Mirror Image", true);
                    summons.SetOption("Spellweaving", "Summon Fey", true);
                    summons.SetOption("Spellweaving", "Summon Fiend", true);
                    summons.SetOption("Spellweaving", "Nature's Fury", true);

                    m_Root.Flavors = new Ruleset[] { pots, para, fields, area, summons };
                }

                return m_Root;
            }
        }

        private string m_Title, m_Description;
        private string[] m_Options;

        private int m_Offset, m_TotalLength;

        private Ruleset[] m_Defaults;
        private Ruleset[] m_Flavors;

        private RulesetLayout m_Parent;
        private RulesetLayout[] m_Children;

        public string Title { get { return m_Title; } }
        public string Description { get { return m_Description; } }
        public string[] Options { get { return m_Options; } }

        public int Offset { get { return m_Offset; } }
        public int TotalLength { get { return m_TotalLength; } }

        public RulesetLayout Parent { get { return m_Parent; } }
        public RulesetLayout[] Children { get { return m_Children; } }

        public Ruleset[] Defaults { get { return m_Defaults; } set { m_Defaults = value; } }
        public Ruleset[] Flavors { get { return m_Flavors; } set { m_Flavors = value; } }

        public RulesetLayout FindByTitle(string title)
        {
            if (m_Title == title)
                return this;

            for (int i = 0; i < m_Children.Length; ++i)
            {
                RulesetLayout layout = m_Children[i].FindByTitle(title);

                if (layout != null)
                    return layout;
            }

            return null;
        }

        public string FindByIndex(int index)
        {
            if (index >= m_Offset && index < (m_Offset + m_Options.Length))
                return m_Description + ": " + m_Options[index - m_Offset];

            for (int i = 0; i < m_Children.Length; ++i)
            {
                string opt = m_Children[i].FindByIndex(index);

                if (opt != null)
                    return opt;
            }

            return null;
        }

        public RulesetLayout FindByOption(string title, string option, ref int index)
        {
            if (title == null || m_Title == title)
            {
                index = GetOptionIndex(option);

                if (index >= 0)
                    return this;

                title = null;
            }

            for (int i = 0; i < m_Children.Length; ++i)
            {
                RulesetLayout layout = m_Children[i].FindByOption(title, option, ref index);

                if (layout != null)
                    return layout;
            }

            return null;
        }

        public int GetOptionIndex(string option)
        {
            return Array.IndexOf(m_Options, option);
        }

        public void ComputeOffsets()
        {
            int offset = 0;

            RecurseComputeOffsets(ref offset);
        }

        private int RecurseComputeOffsets(ref int offset)
        {
            m_Offset = offset;

            offset += m_Options.Length;
            m_TotalLength += m_Options.Length;

            for (int i = 0; i < m_Children.Length; ++i)
                m_TotalLength += m_Children[i].RecurseComputeOffsets(ref offset);

            return m_TotalLength;
        }

        public RulesetLayout(string title, string[] options)
            : this(title, title, new RulesetLayout[0], options)
        {
        }

        public RulesetLayout(string title, string description, string[] options)
            : this(title, description, new RulesetLayout[0], options)
        {
        }

        public RulesetLayout(string title, RulesetLayout[] children)
            : this(title, title, children, new string[0])
        {
        }

        public RulesetLayout(string title, string description, RulesetLayout[] children)
            : this(title, description, children, new string[0])
        {
        }

        public RulesetLayout(string title, RulesetLayout[] children, string[] options)
            : this(title, title, children, options)
        {
        }

        public RulesetLayout(string title, string description, RulesetLayout[] children, string[] options)
        {
            m_Title = title;
            m_Description = description;
            m_Children = children;
            m_Options = options;

            for (int i = 0; i < children.Length; ++i)
                children[i].m_Parent = this;
        }
    }

    public class RulesetGump : Gump
    {
        private Mobile m_From;
        private Ruleset m_Ruleset;
        private RulesetLayout m_Page;
        private DuelContext m_DuelContext;
        private bool m_ReadOnly;

        public string Center(string text)
        {
            return String.Format("<CENTER>{0}</CENTER>", text);
        }

        public void AddGoldenButton(int x, int y, int bid)
        {
            AddButton(x, y, 0xD2, 0xD2, bid, GumpButtonType.Reply, 0);
            AddButton(x + 3, y + 3, 0xD8, 0xD8, bid, GumpButtonType.Reply, 0);
        }

        public RulesetGump(Mobile from, Ruleset ruleset, RulesetLayout page, DuelContext duelContext)
            : this(from, ruleset, page, duelContext, false)
        {
        }

        public RulesetGump(Mobile from, Ruleset ruleset, RulesetLayout page, DuelContext duelContext, bool readOnly)
            : base(readOnly ? 310 : 50, 50)
        {
            m_From = from;
            m_Ruleset = ruleset;
            m_Page = page;
            m_DuelContext = duelContext;
            m_ReadOnly = readOnly;

            Dragable = !readOnly;

            from.CloseGump(typeof(RulesetGump));
            from.CloseGump(typeof(DuelContextGump));
            from.CloseGump(typeof(ParticipantGump));

            RulesetLayout depthCounter = page;
            int depth = 0;

            while (depthCounter != null)
            {
                ++depth;
                depthCounter = depthCounter.Parent;
            }

            int count = page.Children.Length + page.Options.Length;

            AddPage(0);

            int height = 35 + 10 + 2 + (count * 22) + 2 + 30;

            AddBackground(0, 0, 260, height, 9250);
            AddBackground(10, 10, 240, height - 20, 0xDAC);

            AddHtml(35, 25, 190, 20, Center(page.Title), false, false);

            int x = 35;
            int y = 47;

            for (int i = 0; i < page.Children.Length; ++i)
            {
                AddGoldenButton(x, y, 1 + i);
                AddHtml(x + 25, y, 250, 22, page.Children[i].Title, false, false);

                y += 22;
            }

            for (int i = 0; i < page.Options.Length; ++i)
            {
                bool enabled = ruleset.Options[page.Offset + i];

                if (readOnly)
                    AddImage(x, y, enabled ? 0xD3 : 0xD2);
                else
                    AddCheck(x, y, 0xD2, 0xD3, enabled, i);

                AddHtml(x + 25, y, 250, 22, page.Options[i], false, false);

                y += 22;
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (m_DuelContext != null && !m_DuelContext.Registered)
                return;

            if (!m_ReadOnly)
            {
                BitArray opts = new BitArray(m_Page.Options.Length);

                for (int i = 0; i < info.Switches.Length; ++i)
                {
                    int sid = info.Switches[i];

                    if (sid >= 0 && sid < m_Page.Options.Length)
                        opts[sid] = true;
                }

                for (int i = 0; i < opts.Length; ++i)
                {
                    if (m_Ruleset.Options[m_Page.Offset + i] != opts[i])
                    {
                        m_Ruleset.Options[m_Page.Offset + i] = opts[i];
                        m_Ruleset.Changed = true;
                    }
                }
            }

            int bid = info.ButtonID;

            if (bid == 0)
            {
                if (m_Page.Parent != null)
                    m_From.SendGump(new RulesetGump(m_From, m_Ruleset, m_Page.Parent, m_DuelContext, m_ReadOnly));
                else if (!m_ReadOnly)
                    m_From.SendGump(new PickRulesetGump(m_From, m_DuelContext, m_Ruleset));
            }
            else
            {
                bid -= 1;

                if (bid >= 0 && bid < m_Page.Children.Length)
                    m_From.SendGump(new RulesetGump(m_From, m_Ruleset, m_Page.Children[bid], m_DuelContext, m_ReadOnly));
            }
        }
    }

    public class PickRulesetGump : Gump
    {
        private Mobile m_From;
        private DuelContext m_Context;
        private Ruleset m_Ruleset;
        private Ruleset[] m_Defaults;
        private Ruleset[] m_Flavors;

        public string Center(string text)
        {
            return String.Format("<CENTER>{0}</CENTER>", text);
        }

        public PickRulesetGump(Mobile from, DuelContext context, Ruleset ruleset)
            : base(50, 50)
        {
            m_From = from;
            m_Context = context;
            m_Ruleset = ruleset;
            m_Defaults = ruleset.Layout.Defaults;
            m_Flavors = ruleset.Layout.Flavors;

            int height = 25 + 20 + ((m_Defaults.Length + 1) * 22) + 6 + 20 + (m_Flavors.Length * 22) + 25;

            AddPage(0);

            AddBackground(0, 0, 260, height, 9250);
            AddBackground(10, 10, 240, height - 20, 0xDAC);

            AddHtml(35, 25, 190, 20, Center("Rules"), false, false);

            int y = 25 + 20;

            for (int i = 0; i < m_Defaults.Length; ++i)
            {
                Ruleset cur = m_Defaults[i];

                AddHtml(35 + 14, y, 176, 20, cur.Title, false, false);

                if (ruleset.Base == cur && !ruleset.Changed)
                    AddImage(35, y + 4, 0x939);
                else if (ruleset.Base == cur)
                    AddButton(35, y + 4, 0x93A, 0x939, 2 + i, GumpButtonType.Reply, 0);
                else
                    AddButton(35, y + 4, 0x938, 0x939, 2 + i, GumpButtonType.Reply, 0);

                y += 22;
            }

            AddHtml(35 + 14, y, 176, 20, "Custom", false, false);
            AddButton(35, y + 4, ruleset.Changed ? 0x939 : 0x938, 0x939, 1, GumpButtonType.Reply, 0);

            y += 22;
            y += 6;

            AddHtml(35, y, 190, 20, Center("Flavors"), false, false);
            y += 20;

            for (int i = 0; i < m_Flavors.Length; ++i)
            {
                Ruleset cur = m_Flavors[i];

                AddHtml(35 + 14, y, 176, 20, cur.Title, false, false);

                if (ruleset.Flavors.Contains(cur))
                    AddButton(35, y + 4, 0x939, 0x938, 2 + m_Defaults.Length + i, GumpButtonType.Reply, 0);
                else
                    AddButton(35, y + 4, 0x938, 0x939, 2 + m_Defaults.Length + i, GumpButtonType.Reply, 0);

                y += 22;
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (m_Context != null && !m_Context.Registered)
                return;

            switch (info.ButtonID)
            {
                case 0: // closed
                    {
                        if (m_Context != null)
                            m_From.SendGump(new DuelContextGump(m_From, m_Context));

                        break;
                    }
                case 1: // customize
                    {
                        m_From.SendGump(new RulesetGump(m_From, m_Ruleset, m_Ruleset.Layout, m_Context));
                        break;
                    }
                default:
                    {
                        int idx = info.ButtonID - 2;

                        if (idx >= 0 && idx < m_Defaults.Length)
                        {
                            m_Ruleset.ApplyDefault(m_Defaults[idx]);
                            m_From.SendGump(new PickRulesetGump(m_From, m_Context, m_Ruleset));
                        }
                        else
                        {
                            idx -= m_Defaults.Length;

                            if (idx >= 0 && idx < m_Flavors.Length)
                            {
                                if (m_Ruleset.Flavors.Contains(m_Flavors[idx]))
                                    m_Ruleset.RemoveFlavor(m_Flavors[idx]);
                                else
                                    m_Ruleset.AddFlavor(m_Flavors[idx]);

                                m_From.SendGump(new PickRulesetGump(m_From, m_Context, m_Ruleset));
                            }
                        }

                        break;
                    }
            }
        }
    }

    #endregion

    #region Event Player SafeZones

    public class SafeZone : GuardedRegion
    {
        public static readonly int SafeZonePriority = HouseRegion.HousePriority + 1;

        /*public override bool AllowReds{ get{ return true; } }*/

        public SafeZone(Rectangle2D area, Point3D goloc, Map map, bool isGuarded)
            : base(null, map, SafeZonePriority, area)
        {
            GoLocation = goloc;

            this.Disabled = !isGuarded;

            Register();
        }

        public override bool AllowHousing(Mobile from, Point3D p)
        {
            if (from.AccessLevel < AccessLevel.GameMaster)
                return false;

            return base.AllowHousing(from, p);
        }

        public override bool OnMoveInto(Mobile m, Direction d, Point3D newLocation, Point3D oldLocation)
        {
            if (m.Player && Factions.Sigil.ExistsOn(m))
            {
                m.SendMessage(0x22, "You are holding a sigil and cannot enter this zone.");
                return false;
            }

            PlayerMobile pm = m as PlayerMobile;

            if (pm == null && m is BaseCreature)
            {
                BaseCreature bc = (BaseCreature)m;

                if (bc.Summoned)
                    pm = bc.SummonMaster as PlayerMobile;
            }

            if (pm != null && pm.DuelContext != null && pm.DuelContext.StartedBeginCountdown)
                return true;

            if (DuelContext.CheckCombat(m))
            {
                m.SendMessage(0x22, "You have recently been in combat and cannot enter this zone.");
                return false;
            }

            return base.OnMoveInto(m, d, newLocation, oldLocation);
        }

        public override void OnEnter(Mobile m)
        {
            m.SendMessage("You have entered a dueling safezone. No combat other than duels are allowed in this zone.");
        }

        public override void OnExit(Mobile m)
        {
            m.SendMessage("You have left a dueling safezone. Combat is now unrestricted.");
        }

        public override bool CanUseStuckMenu(Mobile m)
        {
            return false;
        }
    }

    #endregion

    public class ReadyGump : Gump
    {
        private Mobile m_From;
        private DuelContext m_Context;
        private int m_Count;

        public string Center(string text)
        {
            return String.Format("<CENTER>{0}</CENTER>", text);
        }

        public ReadyGump(Mobile from, DuelContext context, int count)
            : base(50, 50)
        {
            m_From = from;
            m_Context = context;
            m_Count = count;

            ArrayList parts = context.Participants;

            int height = 25 + 20;

            for (int i = 0; i < parts.Count; ++i)
            {
                Participant p = (Participant)parts[i];

                height += 4;

                if (p.Players.Length > 1)
                    height += 22;

                height += (p.Players.Length * 22);
            }

            height += 25;

            Closable = false;
            Dragable = false;

            AddPage(0);

            AddBackground(0, 0, 260, height, 9250);
            AddBackground(10, 10, 240, height - 20, 0xDAC);

            if (count == -1)
            {
                AddHtml(35, 25, 190, 20, Center("Ready"), false, false);
            }
            else
            {
                AddHtml(35, 25, 190, 20, Center("Starting"), false, false);
                AddHtml(35, 25, 190, 20, "<DIV ALIGN=RIGHT>" + count.ToString(), false, false);
            }

            int y = 25 + 20;

            for (int i = 0; i < parts.Count; ++i)
            {
                Participant p = (Participant)parts[i];

                y += 4;

                bool isAllReady = true;
                int yStore = y;
                int offset = 0;

                if (p.Players.Length > 1)
                {
                    AddHtml(35 + 14, y, 176, 20, String.Format("Participant #{0}", i + 1), false, false);
                    y += 22;
                    offset = 10;
                }

                for (int j = 0; j < p.Players.Length; ++j)
                {
                    DuelPlayer pl = p.Players[j];

                    if (pl != null && pl.Ready)
                    {
                        AddImage(35 + offset, y + 4, 0x939);
                    }
                    else
                    {
                        AddImage(35 + offset, y + 4, 0x938);
                        isAllReady = false;
                    }

                    string name = (pl == null ? "(Empty)" : pl.Mobile.Name);

                    AddHtml(35 + offset + 14, y, 166, 20, name, false, false);

                    y += 22;
                }

                if (p.Players.Length > 1)
                    AddImage(35, yStore + 4, isAllReady ? 0x939 : 0x938);
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
        }
    }

    public class BeginGump : Gump
    {
        public string Center(string text)
        {
            return String.Format("<CENTER>{0}</CENTER>", text);
        }

        public string Color(string text, int color)
        {
            return String.Format("<BASEFONT COLOR=#{0:X6}>{1}</BASEFONT>", color, text);
        }

        private const int LabelColor32 = 0xFFFFFF;
        private const int BlackColor32 = 0x000008;

        public BeginGump(int count)
            : base(50, 50)
        {
            AddPage(0);

            const int offset = 50;

            AddBackground(1, 1, 398, 202 - offset, 3600);

            AddImageTiled(16, 15, 369, 173 - offset, 3604);
            AddAlphaRegion(16, 15, 369, 173 - offset);

            AddImage(215, -43, 0xEE40);

            AddHtml(22 - 1, 22, 294, 20, Color(Center("Duel Countdown"), BlackColor32), false, false);
            AddHtml(22 + 1, 22, 294, 20, Color(Center("Duel Countdown"), BlackColor32), false, false);
            AddHtml(22, 22 - 1, 294, 20, Color(Center("Duel Countdown"), BlackColor32), false, false);
            AddHtml(22, 22 + 1, 294, 20, Color(Center("Duel Countdown"), BlackColor32), false, false);
            AddHtml(22, 22, 294, 20, Color(Center("Duel Countdown"), LabelColor32), false, false);

            AddHtml(22 - 1, 50, 294, 80, Color("The arranged duel is about to begin. During this countdown period you may not cast spells and you may not move. This message will close automatically when the period ends.", BlackColor32), false, false);
            AddHtml(22 + 1, 50, 294, 80, Color("The arranged duel is about to begin. During this countdown period you may not cast spells and you may not move. This message will close automatically when the period ends.", BlackColor32), false, false);
            AddHtml(22, 50 - 1, 294, 80, Color("The arranged duel is about to begin. During this countdown period you may not cast spells and you may not move. This message will close automatically when the period ends.", BlackColor32), false, false);
            AddHtml(22, 50 + 1, 294, 80, Color("The arranged duel is about to begin. During this countdown period you may not cast spells and you may not move. This message will close automatically when the period ends.", BlackColor32), false, false);
            AddHtml(22, 50, 294, 80, Color("The arranged duel is about to begin. During this countdown period you may not cast spells and you may not move. This message will close automatically when the period ends.", 0xFFCC66), false, false);

            /*AddImageTiled( 32, 128, 264, 1, 9107 );
            AddImageTiled( 42, 130, 264, 1, 9157 );

            AddHtml( 60-1, 140, 250, 20, Color( String.Format( "Duel will begin in <BASEFONT COLOR=#{2:X6}>{0} <BASEFONT COLOR=#{2:X6}>second{1}.", count, count==1?"":"s", BlackColor32 ), BlackColor32 ), false, false );
            AddHtml( 60+1, 140, 250, 20, Color( String.Format( "Duel will begin in <BASEFONT COLOR=#{2:X6}>{0} <BASEFONT COLOR=#{2:X6}>second{1}.", count, count==1?"":"s", BlackColor32 ), BlackColor32 ), false, false );
            AddHtml( 60, 140-1, 250, 20, Color( String.Format( "Duel will begin in <BASEFONT COLOR=#{2:X6}>{0} <BASEFONT COLOR=#{2:X6}>second{1}.", count, count==1?"":"s", BlackColor32 ), BlackColor32 ), false, false );
            AddHtml( 60, 140+1, 250, 20, Color( String.Format( "Duel will begin in <BASEFONT COLOR=#{2:X6}>{0} <BASEFONT COLOR=#{2:X6}>second{1}.", count, count==1?"":"s", BlackColor32 ), BlackColor32 ), false, false );
            AddHtml( 60, 140, 250, 20, Color( String.Format( "Duel will begin in <BASEFONT COLOR=#FF6666>{0} <BASEFONT COLOR=#{2:X6}>second{1}.", count, count==1?"":"s", 0x66AACC ), 0x66AACC ), false, false );*/

            AddButton(314 - 50, 157 - offset, 247, 248, 1, GumpButtonType.Reply, 0);
        }
    }

    #region Event Dueling Tournament

    public enum TournamentStage
    {
        Inactive,
        Signup,
        Fighting
    }

    public enum GroupingType
    {
        HighVsLow,
        Nearest,
        Random
    }

    public enum TieType
    {
        Random,
        Highest,
        Lowest,
        FullElimination,
        FullAdvancement
    }

    public class TournamentRegistrar : Banker
    {
        private TournamentController m_Tournament;

        [CommandProperty(AccessLevel.GameMaster)]
        public TournamentController Tournament { get { return m_Tournament; } set { m_Tournament = value; } }

        [Constructable]
        public TournamentRegistrar()
        {
            Timer.DelayCall(TimeSpan.FromSeconds(30.0), TimeSpan.FromSeconds(30.0), new TimerCallback(Announce_Callback));
        }

        private void Announce_Callback()
        {
            Tournament tourny = null;

            if (m_Tournament != null)
                tourny = m_Tournament.Tournament;

            if (tourny != null && tourny.Stage == TournamentStage.Signup)
                PublicOverheadMessage(MessageType.Regular, 0x35, false, "Come one, come all! Do you aspire to be a fighter of great renown? Join this tournament and show the world your abilities.");
        }

        public override void OnMovement(Mobile m, Point3D oldLocation)
        {
            base.OnMovement(m, oldLocation);

            Tournament tourny = null;

            if (m_Tournament != null)
                tourny = m_Tournament.Tournament;

            if (InRange(m, 4) && !InRange(oldLocation, 4) && tourny != null && tourny.Stage == TournamentStage.Signup && m.CanBeginAction(this))
            {
                Ladder ladder = Ladder.Instance;

                if (ladder != null)
                {
                    LadderEntry entry = ladder.Find(m);

                    if (entry != null && Ladder.GetLevel(entry.Experience) < tourny.LevelRequirement)
                        return;
                }

                if (tourny.IsFactionRestricted && Faction.Find(m) == null)
                {
                    return;
                }

                if (tourny.HasParticipant(m))
                    return;

                PrivateOverheadMessage(MessageType.Regular, 0x35, false, String.Format("Hello m'{0}. Dost thou wish to enter this tournament? You need only to write your name in this book.", m.Female ? "Lady" : "Lord"), m.NetState);
                m.BeginAction(this);
                Timer.DelayCall(TimeSpan.FromSeconds(10.0), new TimerStateCallback(ReleaseLock_Callback), m);
            }
        }

        private void ReleaseLock_Callback(object obj)
        {
            ((Mobile)obj).EndAction(this);
        }

        public TournamentRegistrar(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);

            writer.Write((Item)m_Tournament);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_Tournament = reader.ReadItem() as TournamentController;
                        break;
                    }
            }

            Timer.DelayCall(TimeSpan.FromSeconds(30.0), TimeSpan.FromSeconds(30.0), new TimerCallback(Announce_Callback));
        }
    }

    public class TournamentSignupItem : Item
    {
        private TournamentController m_Tournament;
        private Mobile m_Registrar;

        [CommandProperty(AccessLevel.GameMaster)]
        public TournamentController Tournament { get { return m_Tournament; } set { m_Tournament = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Registrar { get { return m_Registrar; } set { m_Registrar = value; } }

        public override string DefaultName
        {
            get { return "tournament signup book"; }
        }

        [Constructable]
        public TournamentSignupItem()
            : base(4029)
        {
            Movable = false;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(GetWorldLocation(), 2))
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that
            }
            else if (m_Tournament != null)
            {
                Tournament tourny = m_Tournament.Tournament;

                if (tourny != null)
                {
                    if (m_Registrar != null)
                        m_Registrar.Direction = m_Registrar.GetDirectionTo(this);

                    switch (tourny.Stage)
                    {
                        case TournamentStage.Fighting:
                            {
                                if (m_Registrar != null)
                                {
                                    if (tourny.HasParticipant(from))
                                    {
                                        m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                                            0x35, false, "Excuse me? You are already signed up.", from.NetState);
                                    }
                                    else
                                    {
                                        m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                                            0x22, false, "The tournament has already begun. You are too late to signup now.", from.NetState);
                                    }
                                }

                                break;
                            }
                        case TournamentStage.Inactive:
                            {
                                if (m_Registrar != null)
                                    m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                                        0x35, false, "The tournament is closed.", from.NetState);

                                break;
                            }
                        case TournamentStage.Signup:
                            {
                                Ladder ladder = Ladder.Instance;

                                if (ladder != null)
                                {
                                    LadderEntry entry = ladder.Find(from);

                                    if (entry != null && Ladder.GetLevel(entry.Experience) < tourny.LevelRequirement)
                                    {
                                        if (m_Registrar != null)
                                        {
                                            m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                                                0x35, false, "You have not yet proven yourself a worthy dueler.", from.NetState);
                                        }

                                        break;
                                    }
                                }

                                if (tourny.IsFactionRestricted && Faction.Find(from) == null)
                                {
                                    if (m_Registrar != null)
                                    {
                                        m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                                            0x35, false, "Only those who have declared their faction allegiance may participate.", from.NetState);
                                    }

                                    break;
                                }

                                if (from.HasGump(typeof(AcceptTeamGump)))
                                {
                                    if (m_Registrar != null)
                                        m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                                            0x22, false, "You must first respond to the offer I've given you.", from.NetState);
                                }
                                else if (from.HasGump(typeof(AcceptDuelGump)))
                                {
                                    if (m_Registrar != null)
                                        m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                                            0x22, false, "You must first cancel your duel offer.", from.NetState);
                                }
                                else if (from is PlayerMobile && ((PlayerMobile)from).DuelContext != null)
                                {
                                    if (m_Registrar != null)
                                        m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                                            0x22, false, "You are already participating in a duel.", from.NetState);
                                }
                                else if (!tourny.HasParticipant(from))
                                {
                                    ArrayList players = new ArrayList();
                                    players.Add(from);
                                    from.CloseGump(typeof(ConfirmSignupGump));
                                    from.SendGump(new ConfirmSignupGump(from, m_Registrar, tourny, players));
                                }
                                else if (m_Registrar != null)
                                {
                                    m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                                        0x35, false, "You have already entered this tournament.", from.NetState);
                                }

                                break;
                            }
                    }
                }
            }
        }

        public TournamentSignupItem(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);

            writer.Write((Item)m_Tournament);
            writer.Write((Mobile)m_Registrar);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_Tournament = reader.ReadItem() as TournamentController;
                        m_Registrar = reader.ReadMobile();
                        break;
                    }
            }
        }
    }

    public class ConfirmSignupGump : Gump
    {
        private Mobile m_From;
        private Tournament m_Tournament;
        private ArrayList m_Players;
        private Mobile m_Registrar;

        private const int BlackColor32 = 0x000008;
        private const int LabelColor32 = 0xFFFFFF;

        public string Center(string text)
        {
            return String.Format("<CENTER>{0}</CENTER>", text);
        }

        public string Color(string text, int color)
        {
            return String.Format("<BASEFONT COLOR=#{0:X6}>{1}</BASEFONT>", color, text);
        }

        private void AddBorderedText(int x, int y, int width, int height, string text, int color, int borderColor)
        {
            AddColoredText(x - 1, y - 1, width, height, text, borderColor);
            AddColoredText(x - 1, y + 1, width, height, text, borderColor);
            AddColoredText(x + 1, y - 1, width, height, text, borderColor);
            AddColoredText(x + 1, y + 1, width, height, text, borderColor);
            AddColoredText(x, y, width, height, text, color);
        }

        private void AddColoredText(int x, int y, int width, int height, string text, int color)
        {
            if (color == 0)
                AddHtml(x, y, width, height, text, false, false);
            else
                AddHtml(x, y, width, height, Color(text, color), false, false);
        }

        public void AddGoldenButton(int x, int y, int bid)
        {
            AddButton(x, y, 0xD2, 0xD2, bid, GumpButtonType.Reply, 0);
            AddButton(x + 3, y + 3, 0xD8, 0xD8, bid, GumpButtonType.Reply, 0);
        }

        public ConfirmSignupGump(Mobile from, Mobile registrar, Tournament tourny, ArrayList players)
            : base(50, 50)
        {
            m_From = from;
            m_Registrar = registrar;
            m_Tournament = tourny;
            m_Players = players;

            m_From.CloseGump(typeof(AcceptTeamGump));
            m_From.CloseGump(typeof(AcceptDuelGump));
            m_From.CloseGump(typeof(DuelContextGump));
            m_From.CloseGump(typeof(ConfirmSignupGump));

            #region Rules
            Ruleset ruleset = tourny.Ruleset;
            Ruleset basedef = ruleset.Base;

            int height = 185 + 60 + 12;

            int changes = 0;

            BitArray defs;

            if (ruleset.Flavors.Count > 0)
            {
                defs = new BitArray(basedef.Options);

                for (int i = 0; i < ruleset.Flavors.Count; ++i)
                    defs.Or(((Ruleset)ruleset.Flavors[i]).Options);

                height += ruleset.Flavors.Count * 18;
            }
            else
            {
                defs = basedef.Options;
            }

            BitArray opts = ruleset.Options;

            for (int i = 0; i < opts.Length; ++i)
            {
                if (defs[i] != opts[i])
                    ++changes;
            }

            height += (changes * 22);

            height += 10 + 22 + 25 + 25;

            if (tourny.PlayersPerParticipant > 1)
                height += 36 + (tourny.PlayersPerParticipant * 20);
            #endregion

            Closable = false;

            AddPage(0);

            //AddBackground( 0, 0, 400, 220, 9150 );
            AddBackground(1, 1, 398, height, 3600);
            //AddBackground( 16, 15, 369, 189, 9100 );

            AddImageTiled(16, 15, 369, height - 29, 3604);
            AddAlphaRegion(16, 15, 369, height - 29);

            AddImage(215, -43, 0xEE40);
            //AddImage( 330, 141, 0x8BA );

            StringBuilder sb = new StringBuilder();

            if (tourny.TournyType == TournyType.FreeForAll)
            {
                sb.Append("FFA");
            }
            else if (tourny.TournyType == TournyType.RandomTeam)
            {
                sb.Append(tourny.ParticipantsPerMatch);
                sb.Append("-Team");
            }
            else if (tourny.TournyType == TournyType.Faction)
            {
                sb.Append(tourny.ParticipantsPerMatch);
                sb.Append("-Team Faction");
            }
            else if (tourny.TournyType == TournyType.RedVsBlue)
            {
                sb.Append("Red v Blue");
            }
            else
            {
                for (int i = 0; i < tourny.ParticipantsPerMatch; ++i)
                {
                    if (sb.Length > 0)
                        sb.Append('v');

                    sb.Append(tourny.PlayersPerParticipant);
                }
            }

            if (tourny.EventController != null)
                sb.Append(' ').Append(tourny.EventController.Title);

            sb.Append(" Tournament Signup");

            AddBorderedText(22, 22, 294, 20, Center(sb.ToString()), LabelColor32, BlackColor32);
            AddBorderedText(22, 50, 294, 40, "You have requested to join the tournament. Do you accept the rules?", 0xB0C868, BlackColor32);

            AddImageTiled(32, 88, 264, 1, 9107);
            AddImageTiled(42, 90, 264, 1, 9157);

            #region Rules
            int y = 100;

            string groupText = null;

            switch (tourny.GroupType)
            {
                case GroupingType.HighVsLow: groupText = "High vs Low"; break;
                case GroupingType.Nearest: groupText = "Closest opponent"; break;
                case GroupingType.Random: groupText = "Random"; break;
            }

            AddBorderedText(35, y, 190, 20, String.Format("Grouping: {0}", groupText), LabelColor32, BlackColor32);
            y += 20;

            string tieText = null;

            switch (tourny.TieType)
            {
                case TieType.Random: tieText = "Random"; break;
                case TieType.Highest: tieText = "Highest advances"; break;
                case TieType.Lowest: tieText = "Lowest advances"; break;
                case TieType.FullAdvancement: tieText = (tourny.ParticipantsPerMatch == 2 ? "Both advance" : "Everyone advances"); break;
                case TieType.FullElimination: tieText = (tourny.ParticipantsPerMatch == 2 ? "Both eliminated" : "Everyone eliminated"); break;
            }

            AddBorderedText(35, y, 190, 20, String.Format("Tiebreaker: {0}", tieText), LabelColor32, BlackColor32);
            y += 20;

            string sdText = "Off";

            if (tourny.SuddenDeath > TimeSpan.Zero)
            {
                sdText = String.Format("{0}:{1:D2}", (int)tourny.SuddenDeath.TotalMinutes, tourny.SuddenDeath.Seconds);

                if (tourny.SuddenDeathRounds > 0)
                    sdText = String.Format("{0} (first {1} rounds)", sdText, tourny.SuddenDeathRounds);
                else
                    sdText = String.Format("{0} (all rounds)", sdText);
            }

            AddBorderedText(35, y, 240, 20, String.Format("Sudden Death: {0}", sdText), LabelColor32, BlackColor32);
            y += 20;

            y += 6;
            AddImageTiled(32, y - 1, 264, 1, 9107);
            AddImageTiled(42, y + 1, 264, 1, 9157);
            y += 6;

            AddBorderedText(35, y, 190, 20, String.Format("Ruleset: {0}", basedef.Title), LabelColor32, BlackColor32);
            y += 20;

            for (int i = 0; i < ruleset.Flavors.Count; ++i, y += 18)
                AddBorderedText(35, y, 190, 20, String.Format(" + {0}", ((Ruleset)ruleset.Flavors[i]).Title), LabelColor32, BlackColor32);

            y += 4;

            if (changes > 0)
            {
                AddBorderedText(35, y, 190, 20, "Modifications:", LabelColor32, BlackColor32);
                y += 20;

                for (int i = 0; i < opts.Length; ++i)
                {
                    if (defs[i] != opts[i])
                    {
                        string name = ruleset.Layout.FindByIndex(i);

                        if (name != null) // sanity
                        {
                            AddImage(35, y, opts[i] ? 0xD3 : 0xD2);
                            AddBorderedText(60, y, 165, 22, name, LabelColor32, BlackColor32);
                        }

                        y += 22;
                    }
                }
            }
            else
            {
                AddBorderedText(35, y, 190, 20, "Modifications: None", LabelColor32, BlackColor32);
                y += 20;
            }
            #endregion

            #region Team
            if (tourny.PlayersPerParticipant > 1)
            {
                y += 8;
                AddImageTiled(32, y - 1, 264, 1, 9107);
                AddImageTiled(42, y + 1, 264, 1, 9157);
                y += 8;

                AddBorderedText(35, y, 190, 20, "Your Team", LabelColor32, BlackColor32);
                y += 20;

                for (int i = 0; i < players.Count; ++i, y += 20)
                {
                    if (i == 0)
                        AddImage(35, y, 0xD2);
                    else
                        AddGoldenButton(35, y, 1 + i);

                    AddBorderedText(60, y, 200, 20, ((Mobile)players[i]).Name, LabelColor32, BlackColor32);
                }

                for (int i = players.Count; i < tourny.PlayersPerParticipant; ++i, y += 20)
                {
                    if (i == 0)
                        AddImage(35, y, 0xD2);
                    else
                        AddGoldenButton(35, y, 1 + i);

                    AddBorderedText(60, y, 200, 20, "(Empty)", LabelColor32, BlackColor32);
                }
            }
            #endregion

            y += 8;
            AddImageTiled(32, y - 1, 264, 1, 9107);
            AddImageTiled(42, y + 1, 264, 1, 9157);
            y += 8;

            AddRadio(24, y, 9727, 9730, true, 1);
            AddBorderedText(60, y + 5, 250, 20, "Yes, I wish to join the tournament.", LabelColor32, BlackColor32);
            y += 35;

            AddRadio(24, y, 9727, 9730, false, 2);
            AddBorderedText(60, y + 5, 250, 20, "No, I do not wish to join.", LabelColor32, BlackColor32);
            y += 35;

            y -= 3;
            AddButton(314, y, 247, 248, 1, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 1 && info.IsSwitched(1))
            {
                Tournament tourny = m_Tournament;
                Mobile from = m_From;

                switch (tourny.Stage)
                {
                    case TournamentStage.Fighting:
                        {
                            if (m_Registrar != null)
                            {
                                if (m_Tournament.HasParticipant(from))
                                {
                                    m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                                        0x35, false, "Excuse me? You are already signed up.", from.NetState);
                                }
                                else
                                {
                                    m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                                        0x22, false, "The tournament has already begun. You are too late to signup now.", from.NetState);
                                }
                            }

                            break;
                        }
                    case TournamentStage.Inactive:
                        {
                            if (m_Registrar != null)
                                m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                                    0x35, false, "The tournament is closed.", from.NetState);

                            break;
                        }
                    case TournamentStage.Signup:
                        {
                            if (m_Players.Count != tourny.PlayersPerParticipant)
                            {
                                if (m_Registrar != null)
                                {
                                    m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                                        0x35, false, "You have not yet chosen your team.", from.NetState);
                                }

                                m_From.SendGump(new ConfirmSignupGump(m_From, m_Registrar, m_Tournament, m_Players));
                                break;
                            }

                            Ladder ladder = Ladder.Instance;

                            for (int i = 0; i < m_Players.Count; ++i)
                            {
                                Mobile mob = (Mobile)m_Players[i];

                                LadderEntry entry = (ladder == null ? null : ladder.Find(mob));

                                if (entry != null && Ladder.GetLevel(entry.Experience) < tourny.LevelRequirement)
                                {
                                    if (m_Registrar != null)
                                    {
                                        if (mob == from)
                                        {
                                            m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                                                0x35, false, "You have not yet proven yourself a worthy dueler.", from.NetState);
                                        }
                                        else
                                        {
                                            m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                                                0x35, false, String.Format("{0} has not yet proven themselves a worthy dueler.", mob.Name), from.NetState);
                                        }
                                    }

                                    m_From.SendGump(new ConfirmSignupGump(m_From, m_Registrar, m_Tournament, m_Players));
                                    return;
                                }
                                else if (tourny.IsFactionRestricted && Faction.Find(mob) == null)
                                {
                                    if (m_Registrar != null)
                                    {
                                        m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                                            0x35, false, "Only those who have declared their faction allegiance may participate.", from.NetState);
                                    }

                                    m_From.SendGump(new ConfirmSignupGump(m_From, m_Registrar, m_Tournament, m_Players));
                                    return;
                                }
                                else if (tourny.HasParticipant(mob))
                                {
                                    if (m_Registrar != null)
                                    {
                                        if (mob == from)
                                        {
                                            m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                                                0x35, false, "You have already entered this tournament.", from.NetState);
                                        }
                                        else
                                        {
                                            m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                                                0x35, false, String.Format("{0} has already entered this tournament.", mob.Name), from.NetState);
                                        }
                                    }

                                    m_From.SendGump(new ConfirmSignupGump(m_From, m_Registrar, m_Tournament, m_Players));
                                    return;
                                }
                                else if (mob is PlayerMobile && ((PlayerMobile)mob).DuelContext != null)
                                {
                                    if (m_Registrar != null)
                                    {
                                        if (mob == from)
                                        {
                                            m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                                                0x35, false, "You are already assigned to a duel. You must yield it before joining this tournament.", from.NetState);
                                        }
                                        else
                                        {
                                            m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                                                0x35, false, String.Format("{0} is already assigned to a duel. They must yield it before joining this tournament.", mob.Name), from.NetState);
                                        }
                                    }

                                    m_From.SendGump(new ConfirmSignupGump(m_From, m_Registrar, m_Tournament, m_Players));
                                    return;
                                }
                            }

                            if (m_Registrar != null)
                            {
                                string fmt;

                                if (tourny.PlayersPerParticipant == 1)
                                    fmt = "As you say m'{0}. I've written your name to the bracket. The tournament will begin {1}.";
                                else if (tourny.PlayersPerParticipant == 2)
                                    fmt = "As you wish m'{0}. The tournament will begin {1}, but first you must name your partner.";
                                else
                                    fmt = "As you wish m'{0}. The tournament will begin {1}, but first you must name your team.";

                                string timeUntil;
                                int minutesUntil = (int)Math.Round(((tourny.SignupStart + tourny.SignupPeriod) - DateTime.UtcNow).TotalMinutes);

                                if (minutesUntil == 0)
                                    timeUntil = "momentarily";
                                else
                                    timeUntil = String.Format("in {0} minute{1}", minutesUntil, minutesUntil == 1 ? "" : "s");

                                m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                                    0x35, false, String.Format(fmt, from.Female ? "Lady" : "Lord", timeUntil), from.NetState);
                            }

                            TournyParticipant part = new TournyParticipant(from);
                            part.Players.Clear();
                            part.Players.AddRange(m_Players);

                            tourny.Participants.Add(part);

                            break;
                        }
                }
            }
            else if (info.ButtonID > 1)
            {
                int index = info.ButtonID - 1;

                if (index > 0 && index < m_Players.Count)
                {
                    m_Players.RemoveAt(index);
                    m_From.SendGump(new ConfirmSignupGump(m_From, m_Registrar, m_Tournament, m_Players));
                }
                else if (m_Players.Count < m_Tournament.PlayersPerParticipant)
                {
                    m_From.BeginTarget(12, false, TargetFlags.None, new TargetCallback(AddPlayer_OnTarget));
                    m_From.SendGump(new ConfirmSignupGump(m_From, m_Registrar, m_Tournament, m_Players));
                }
            }
        }

        private void AddPlayer_OnTarget(Mobile from, object obj)
        {
            Mobile mob = obj as Mobile;

            if (mob == null || mob == from)
            {
                m_From.SendGump(new ConfirmSignupGump(m_From, m_Registrar, m_Tournament, m_Players));

                if (m_Registrar != null)
                    m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                        0x22, false, "Excuse me?", from.NetState);
            }
            else if (!mob.Player)
            {
                m_From.SendGump(new ConfirmSignupGump(m_From, m_Registrar, m_Tournament, m_Players));

                if (mob.Body.IsHuman)
                    mob.SayTo(from, 1005443); // Nay, I would rather stay here and watch a nail rust.
                else
                    mob.SayTo(from, 1005444); // The creature ignores your offer.
            }
            else if (AcceptDuelGump.IsIgnored(mob, from) || mob.Blessed)
            {
                m_From.SendGump(new ConfirmSignupGump(m_From, m_Registrar, m_Tournament, m_Players));

                if (m_Registrar != null)
                    m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                        0x22, false, "They ignore your invitation.", from.NetState);
            }
            else
            {
                PlayerMobile pm = mob as PlayerMobile;

                if (pm == null)
                    return;

                if (pm.DuelContext != null)
                {
                    m_From.SendGump(new ConfirmSignupGump(m_From, m_Registrar, m_Tournament, m_Players));

                    if (m_Registrar != null)
                        m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                            0x22, false, "They are already assigned to another duel.", from.NetState);
                }
                else if (mob.HasGump(typeof(AcceptTeamGump)))
                {
                    m_From.SendGump(new ConfirmSignupGump(m_From, m_Registrar, m_Tournament, m_Players));

                    if (m_Registrar != null)
                        m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                            0x22, false, "They have already been offered a partnership.", from.NetState);
                }
                else if (mob.HasGump(typeof(ConfirmSignupGump)))
                {
                    m_From.SendGump(new ConfirmSignupGump(m_From, m_Registrar, m_Tournament, m_Players));

                    if (m_Registrar != null)
                        m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                            0x22, false, "They are already trying to join this tournament.", from.NetState);
                }
                else if (m_Players.Contains(mob))
                {
                    m_From.SendGump(new ConfirmSignupGump(m_From, m_Registrar, m_Tournament, m_Players));

                    if (m_Registrar != null)
                        m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                            0x22, false, "You have already named them as a team member.", from.NetState);
                }
                else if (m_Tournament.HasParticipant(mob))
                {
                    m_From.SendGump(new ConfirmSignupGump(m_From, m_Registrar, m_Tournament, m_Players));

                    if (m_Registrar != null)
                        m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                            0x22, false, "They have already entered this tournament.", from.NetState);
                }
                else if (m_Players.Count >= m_Tournament.PlayersPerParticipant)
                {
                    m_From.SendGump(new ConfirmSignupGump(m_From, m_Registrar, m_Tournament, m_Players));

                    if (m_Registrar != null)
                        m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                            0x22, false, "Your team is full.", from.NetState);
                }
                else
                {
                    m_From.SendGump(new ConfirmSignupGump(m_From, m_Registrar, m_Tournament, m_Players));
                    mob.SendGump(new AcceptTeamGump(from, mob, m_Tournament, m_Registrar, m_Players));

                    if (m_Registrar != null)
                        m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                            0x59, false, String.Format("As you command m'{0}. I've given your offer to {1}.", from.Female ? "Lady" : "Lord", mob.Name), from.NetState);
                }
            }
        }
    }

    public class AcceptTeamGump : Gump
    {
        private bool m_Active;

        private Mobile m_From;
        private Mobile m_Requested;
        private Tournament m_Tournament;
        private Mobile m_Registrar;
        private ArrayList m_Players;

        private const int BlackColor32 = 0x000008;
        private const int LabelColor32 = 0xFFFFFF;

        public string Center(string text)
        {
            return String.Format("<CENTER>{0}</CENTER>", text);
        }

        public string Color(string text, int color)
        {
            return String.Format("<BASEFONT COLOR=#{0:X6}>{1}</BASEFONT>", color, text);
        }

        private void AddBorderedText(int x, int y, int width, int height, string text, int color, int borderColor)
        {
            AddColoredText(x - 1, y - 1, width, height, text, borderColor);
            AddColoredText(x - 1, y + 1, width, height, text, borderColor);
            AddColoredText(x + 1, y - 1, width, height, text, borderColor);
            AddColoredText(x + 1, y + 1, width, height, text, borderColor);
            AddColoredText(x, y, width, height, text, color);
        }

        private void AddColoredText(int x, int y, int width, int height, string text, int color)
        {
            if (color == 0)
                AddHtml(x, y, width, height, text, false, false);
            else
                AddHtml(x, y, width, height, Color(text, color), false, false);
        }

        public AcceptTeamGump(Mobile from, Mobile requested, Tournament tourny, Mobile registrar, ArrayList players)
            : base(50, 50)
        {
            m_From = from;
            m_Requested = requested;
            m_Tournament = tourny;
            m_Registrar = registrar;
            m_Players = players;

            m_Active = true;

            #region Rules
            Ruleset ruleset = tourny.Ruleset;
            Ruleset basedef = ruleset.Base;

            int height = 185 + 35 + 60 + 12;

            int changes = 0;

            BitArray defs;

            if (ruleset.Flavors.Count > 0)
            {
                defs = new BitArray(basedef.Options);

                for (int i = 0; i < ruleset.Flavors.Count; ++i)
                    defs.Or(((Ruleset)ruleset.Flavors[i]).Options);

                height += ruleset.Flavors.Count * 18;
            }
            else
            {
                defs = basedef.Options;
            }

            BitArray opts = ruleset.Options;

            for (int i = 0; i < opts.Length; ++i)
            {
                if (defs[i] != opts[i])
                    ++changes;
            }

            height += (changes * 22);

            height += 10 + 22 + 25 + 25;
            #endregion

            Closable = false;

            AddPage(0);

            AddBackground(1, 1, 398, height, 3600);

            AddImageTiled(16, 15, 369, height - 29, 3604);
            AddAlphaRegion(16, 15, 369, height - 29);

            AddImage(215, -43, 0xEE40);

            StringBuilder sb = new StringBuilder();

            if (tourny.TournyType == TournyType.FreeForAll)
            {
                sb.Append("FFA");
            }
            else if (tourny.TournyType == TournyType.RandomTeam)
            {
                sb.Append(tourny.ParticipantsPerMatch);
                sb.Append("-Team");
            }
            else if (tourny.TournyType == TournyType.Faction)
            {
                sb.Append(tourny.ParticipantsPerMatch);
                sb.Append("-Team Faction");
            }
            else if (tourny.TournyType == TournyType.RedVsBlue)
            {
                sb.Append("Red v Blue");
            }
            else
            {
                for (int i = 0; i < tourny.ParticipantsPerMatch; ++i)
                {
                    if (sb.Length > 0)
                        sb.Append('v');

                    sb.Append(tourny.PlayersPerParticipant);
                }
            }

            if (tourny.EventController != null)
                sb.Append(' ').Append(tourny.EventController.Title);

            sb.Append(" Tournament Invitation");

            AddBorderedText(22, 22, 294, 20, Center(sb.ToString()), LabelColor32, BlackColor32);

            AddBorderedText(22, 50, 294, 40,
                String.Format("You have been asked to partner with {0} in a tournament. Do you accept?", from.Name),
                0xB0C868, BlackColor32);

            AddImageTiled(32, 88, 264, 1, 9107);
            AddImageTiled(42, 90, 264, 1, 9157);

            #region Rules
            int y = 100;

            string groupText = null;

            switch (tourny.GroupType)
            {
                case GroupingType.HighVsLow: groupText = "High vs Low"; break;
                case GroupingType.Nearest: groupText = "Closest opponent"; break;
                case GroupingType.Random: groupText = "Random"; break;
            }

            AddBorderedText(35, y, 190, 20, String.Format("Grouping: {0}", groupText), LabelColor32, BlackColor32);
            y += 20;

            string tieText = null;

            switch (tourny.TieType)
            {
                case TieType.Random: tieText = "Random"; break;
                case TieType.Highest: tieText = "Highest advances"; break;
                case TieType.Lowest: tieText = "Lowest advances"; break;
                case TieType.FullAdvancement: tieText = (tourny.ParticipantsPerMatch == 2 ? "Both advance" : "Everyone advances"); break;
                case TieType.FullElimination: tieText = (tourny.ParticipantsPerMatch == 2 ? "Both eliminated" : "Everyone eliminated"); break;
            }

            AddBorderedText(35, y, 190, 20, String.Format("Tiebreaker: {0}", tieText), LabelColor32, BlackColor32);
            y += 20;

            string sdText = "Off";

            if (tourny.SuddenDeath > TimeSpan.Zero)
            {
                sdText = String.Format("{0}:{1:D2}", (int)tourny.SuddenDeath.TotalMinutes, tourny.SuddenDeath.Seconds);

                if (tourny.SuddenDeathRounds > 0)
                    sdText = String.Format("{0} (first {1} rounds)", sdText, tourny.SuddenDeathRounds);
                else
                    sdText = String.Format("{0} (all rounds)", sdText);
            }

            AddBorderedText(35, y, 240, 20, String.Format("Sudden Death: {0}", sdText), LabelColor32, BlackColor32);
            y += 20;

            y += 6;
            AddImageTiled(32, y - 1, 264, 1, 9107);
            AddImageTiled(42, y + 1, 264, 1, 9157);
            y += 6;

            AddBorderedText(35, y, 190, 20, String.Format("Ruleset: {0}", basedef.Title), LabelColor32, BlackColor32);
            y += 20;

            for (int i = 0; i < ruleset.Flavors.Count; ++i, y += 18)
                AddBorderedText(35, y, 190, 20, String.Format(" + {0}", ((Ruleset)ruleset.Flavors[i]).Title), LabelColor32, BlackColor32);

            y += 4;

            if (changes > 0)
            {
                AddBorderedText(35, y, 190, 20, "Modifications:", LabelColor32, BlackColor32);
                y += 20;

                for (int i = 0; i < opts.Length; ++i)
                {
                    if (defs[i] != opts[i])
                    {
                        string name = ruleset.Layout.FindByIndex(i);

                        if (name != null) // sanity
                        {
                            AddImage(35, y, opts[i] ? 0xD3 : 0xD2);
                            AddBorderedText(60, y, 165, 22, name, LabelColor32, BlackColor32);
                        }

                        y += 22;
                    }
                }
            }
            else
            {
                AddBorderedText(35, y, 190, 20, "Modifications: None", LabelColor32, BlackColor32);
                y += 20;
            }
            #endregion

            y += 8;
            AddImageTiled(32, y - 1, 264, 1, 9107);
            AddImageTiled(42, y + 1, 264, 1, 9157);
            y += 8;

            AddRadio(24, y, 9727, 9730, true, 1);
            AddBorderedText(60, y + 5, 250, 20, "Yes, I will join them.", LabelColor32, BlackColor32);
            y += 35;

            AddRadio(24, y, 9727, 9730, false, 2);
            AddBorderedText(60, y + 5, 250, 20, "No, I do not wish to fight.", LabelColor32, BlackColor32);
            y += 35;

            AddRadio(24, y, 9727, 9730, false, 3);
            AddBorderedText(60, y + 5, 270, 20, "No, most certainly not. Do not ask again.", LabelColor32, BlackColor32);
            y += 35;

            y -= 3;
            AddButton(314, y, 247, 248, 1, GumpButtonType.Reply, 0);

            Timer.DelayCall(TimeSpan.FromSeconds(15.0), new TimerCallback(AutoReject));
        }

        public void AutoReject()
        {
            if (!m_Active)
                return;

            m_Active = false;

            m_Requested.CloseGump(typeof(AcceptTeamGump));
            m_From.SendGump(new ConfirmSignupGump(m_From, m_Registrar, m_Tournament, m_Players));

            if (m_Registrar != null)
            {
                m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                    0x22, false, String.Format("{0} seems unresponsive.", m_Requested.Name), m_From.NetState);

                m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                    0x22, false, String.Format("You have declined the partnership with {0}.", m_From.Name), m_Requested.NetState);
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = m_From;
            Mobile mob = m_Requested;

            if (info.ButtonID != 1 || !m_Active)
                return;

            m_Active = false;

            if (info.IsSwitched(1))
            {
                PlayerMobile pm = mob as PlayerMobile;

                if (pm == null)
                    return;

                if (AcceptDuelGump.IsIgnored(mob, from) || mob.Blessed)
                {
                    m_From.SendGump(new ConfirmSignupGump(m_From, m_Registrar, m_Tournament, m_Players));

                    if (m_Registrar != null)
                        m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                            0x22, false, "They ignore your invitation.", from.NetState);
                }
                else if (pm.DuelContext != null)
                {
                    m_From.SendGump(new ConfirmSignupGump(m_From, m_Registrar, m_Tournament, m_Players));

                    if (m_Registrar != null)
                        m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                            0x22, false, "They are already assigned to another duel.", from.NetState);
                }
                else if (m_Players.Contains(mob))
                {
                    m_From.SendGump(new ConfirmSignupGump(m_From, m_Registrar, m_Tournament, m_Players));

                    if (m_Registrar != null)
                        m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                            0x22, false, "You have already named them as a team member.", from.NetState);
                }
                else if (m_Tournament.HasParticipant(mob))
                {
                    m_From.SendGump(new ConfirmSignupGump(m_From, m_Registrar, m_Tournament, m_Players));

                    if (m_Registrar != null)
                        m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                            0x22, false, "They have already entered this tournament.", from.NetState);
                }
                else if (m_Players.Count >= m_Tournament.PlayersPerParticipant)
                {
                    m_From.SendGump(new ConfirmSignupGump(m_From, m_Registrar, m_Tournament, m_Players));

                    if (m_Registrar != null)
                        m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                            0x22, false, "Your team is full.", from.NetState);
                }
                else
                {
                    m_Players.Add(mob);

                    m_From.SendGump(new ConfirmSignupGump(m_From, m_Registrar, m_Tournament, m_Players));

                    if (m_Registrar != null)
                    {
                        m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                            0x59, false, String.Format("{0} has accepted your offer of partnership.", mob.Name), from.NetState);

                        m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                            0x59, false, String.Format("You have accepted the partnership with {0}.", from.Name), mob.NetState);
                    }
                }
            }
            else
            {
                if (info.IsSwitched(3))
                    AcceptDuelGump.BeginIgnore(m_Requested, m_From);

                m_From.SendGump(new ConfirmSignupGump(m_From, m_Registrar, m_Tournament, m_Players));

                if (m_Registrar != null)
                {
                    m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                        0x22, false, String.Format("{0} has declined your offer of partnership.", mob.Name), from.NetState);

                    m_Registrar.PrivateOverheadMessage(MessageType.Regular,
                        0x22, false, String.Format("You have declined the partnership with {0}.", from.Name), mob.NetState);
                }
            }
        }
    }

    public class TournamentController : Item
    {
        private Tournament m_Tournament;

        [CommandProperty(AccessLevel.GameMaster)]
        public Tournament Tournament { get { return m_Tournament; } set { } }

        private static ArrayList m_Instances = new ArrayList();

        public static bool IsActive
        {
            get
            {
                for (int i = 0; i < m_Instances.Count; ++i)
                {
                    TournamentController controller = (TournamentController)m_Instances[i];

                    if (controller != null && !controller.Deleted && controller.Tournament != null && controller.Tournament.Stage != TournamentStage.Inactive)
                        return true;
                }

                return false;
            }
        }

        public override string DefaultName
        {
            get { return "tournament controller"; }
        }

        [Constructable]
        public TournamentController()
            : base(0x1B7A)
        {
            Visible = false;
            Movable = false;

            m_Tournament = new Tournament();
            m_Instances.Add(this);
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);

            if (from.AccessLevel >= AccessLevel.GameMaster && m_Tournament != null)
            {
                list.Add(new EditEntry(m_Tournament));

                if (m_Tournament.CurrentStage == TournamentStage.Inactive)
                    list.Add(new StartEntry(m_Tournament));
            }
        }

        private class EditEntry : ContextMenuEntry
        {
            private Tournament m_Tournament;

            public EditEntry(Tournament tourny)
                : base(5101)
            {
                m_Tournament = tourny;
            }

            public override void OnClick()
            {
                Owner.From.SendGump(new PropertiesGump(Owner.From, m_Tournament));
            }
        }

        private class StartEntry : ContextMenuEntry
        {
            private Tournament m_Tournament;

            public StartEntry(Tournament tourny)
                : base(5113)
            {
                m_Tournament = tourny;
            }

            public override void OnClick()
            {
                if (m_Tournament.Stage == TournamentStage.Inactive)
                {
                    m_Tournament.SignupStart = DateTime.UtcNow;
                    m_Tournament.Stage = TournamentStage.Signup;
                    m_Tournament.Participants.Clear();
                    m_Tournament.Pyramid.Levels.Clear();
                    m_Tournament.Alert("Hear ye! Hear ye!", "Tournament signup has opened. You can enter by signing up with the registrar.");
                }
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.AccessLevel >= AccessLevel.GameMaster && m_Tournament != null)
            {
                from.CloseGump(typeof(PickRulesetGump));
                from.CloseGump(typeof(RulesetGump));
                from.SendGump(new PickRulesetGump(from, null, m_Tournament.Ruleset));
            }
        }

        public TournamentController(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);

            m_Tournament.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_Tournament = new Tournament(reader);
                        break;
                    }
            }

            m_Instances.Add(this);
        }

        public override void OnDelete()
        {
            base.OnDelete();

            m_Instances.Remove(this);
        }
    }

    public enum TournyType
    {
        Standard,
        FreeForAll,
        RandomTeam,
        RedVsBlue,
        Faction
    }

    [PropertyObject]
    public class Tournament
    {
        private int m_ParticipantsPerMatch;
        private int m_PlayersPerParticipant;
        private int m_LevelRequirement;

        private bool m_FactionRestricted;

        private TournyPyramid m_Pyramid;
        private Ruleset m_Ruleset;

        private ArrayList m_Arenas;
        private ArrayList m_Participants;
        private ArrayList m_Undefeated;

        private TimeSpan m_SignupPeriod;
        private DateTime m_SignupStart;

        private TournamentStage m_Stage;

        private GroupingType m_GroupType;
        private TieType m_TieType;
        private TimeSpan m_SuddenDeath;

        private TournyType m_TournyType;

        private int m_SuddenDeathRounds;

        private EventController m_EventController;

        public bool IsNotoRestricted { get { return (m_TournyType != TournyType.Standard); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public EventController EventController
        {
            get { return m_EventController; }
            set { m_EventController = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SuddenDeathRounds
        {
            get { return m_SuddenDeathRounds; }
            set { m_SuddenDeathRounds = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TournyType TournyType
        {
            get { return m_TournyType; }
            set { m_TournyType = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public GroupingType GroupType
        {
            get { return m_GroupType; }
            set { m_GroupType = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TieType TieType
        {
            get { return m_TieType; }
            set { m_TieType = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan SuddenDeath
        {
            get { return m_SuddenDeath; }
            set { m_SuddenDeath = value; }
        }

        public Ruleset Ruleset
        {
            get { return m_Ruleset; }
            set { m_Ruleset = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ParticipantsPerMatch
        {
            get { return m_ParticipantsPerMatch; }
            set { if (value < 2) value = 2; else if (value > 10) value = 10; m_ParticipantsPerMatch = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int PlayersPerParticipant
        {
            get { return m_PlayersPerParticipant; }
            set { if (value < 1) value = 1; else if (value > 10) value = 10; m_PlayersPerParticipant = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int LevelRequirement
        {
            get { return m_LevelRequirement; }
            set { m_LevelRequirement = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool FactionRestricted
        {
            get { return m_FactionRestricted; }
            set { m_FactionRestricted = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan SignupPeriod
        {
            get { return m_SignupPeriod; }
            set { m_SignupPeriod = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime SignupStart
        {
            get { return m_SignupStart; }
            set { m_SignupStart = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TournamentStage CurrentStage
        {
            get { return m_Stage; }
        }

        public TournamentStage Stage
        {
            get { return m_Stage; }
            set { m_Stage = value; }
        }

        public TournyPyramid Pyramid
        {
            get { return m_Pyramid; }
            set { m_Pyramid = value; }
        }

        public ArrayList Arenas
        {
            get { return m_Arenas; }
            set { m_Arenas = value; }
        }

        public ArrayList Participants
        {
            get { return m_Participants; }
            set { m_Participants = value; }
        }

        public ArrayList Undefeated
        {
            get { return m_Undefeated; }
            set { m_Undefeated = value; }
        }

        public bool IsFactionRestricted
        {
            get
            {
                return (m_FactionRestricted || m_TournyType == TournyType.Faction);
            }
        }

        public bool HasParticipant(Mobile mob)
        {
            for (int i = 0; i < m_Participants.Count; ++i)
            {
                TournyParticipant part = (TournyParticipant)m_Participants[i];

                if (part.Players.Contains(mob))
                    return true;
            }

            return false;
        }

        public void Serialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)5); // version

            writer.Write((bool)m_FactionRestricted);

            writer.Write((Item)m_EventController);

            writer.WriteEncodedInt((int)m_SuddenDeathRounds);

            writer.WriteEncodedInt((int)m_TournyType);

            writer.WriteEncodedInt((int)m_GroupType);
            writer.WriteEncodedInt((int)m_TieType);
            writer.Write((TimeSpan)m_SuddenDeath);

            writer.WriteEncodedInt((int)m_ParticipantsPerMatch);
            writer.WriteEncodedInt((int)m_PlayersPerParticipant);
            writer.Write((TimeSpan)m_SignupPeriod);
        }

        public Tournament(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            switch (version)
            {
                case 5:
                    {
                        m_FactionRestricted = reader.ReadBool();

                        goto case 4;
                    }
                case 4:
                    {
                        m_EventController = reader.ReadItem() as EventController;

                        goto case 3;
                    }
                case 3:
                    {
                        m_SuddenDeathRounds = reader.ReadEncodedInt();

                        goto case 2;
                    }
                case 2:
                    {
                        m_TournyType = (TournyType)reader.ReadEncodedInt();

                        goto case 1;
                    }
                case 1:
                    {
                        m_GroupType = (GroupingType)reader.ReadEncodedInt();
                        m_TieType = (TieType)reader.ReadEncodedInt();
                        m_SignupPeriod = reader.ReadTimeSpan();

                        goto case 0;
                    }
                case 0:
                    {
                        if (version < 3)
                            m_SuddenDeathRounds = 3;

                        m_ParticipantsPerMatch = reader.ReadEncodedInt();
                        m_PlayersPerParticipant = reader.ReadEncodedInt();
                        m_SignupPeriod = reader.ReadTimeSpan();
                        m_Stage = TournamentStage.Inactive;
                        m_Pyramid = new TournyPyramid();
                        m_Ruleset = new Ruleset(RulesetLayout.Root);
                        m_Ruleset.ApplyDefault(m_Ruleset.Layout.Defaults[0]);
                        m_Participants = new ArrayList();
                        m_Undefeated = new ArrayList();
                        m_Arenas = new ArrayList();

                        break;
                    }
            }

            Timer.DelayCall(SliceInterval, SliceInterval, new TimerCallback(Slice));
        }

        public Tournament()
        {
            m_ParticipantsPerMatch = 2;
            m_PlayersPerParticipant = 1;
            m_Pyramid = new TournyPyramid();
            m_Ruleset = new Ruleset(RulesetLayout.Root);
            m_Ruleset.ApplyDefault(m_Ruleset.Layout.Defaults[0]);
            m_Participants = new ArrayList();
            m_Undefeated = new ArrayList();
            m_Arenas = new ArrayList();
            m_SignupPeriod = TimeSpan.FromMinutes(10.0);

            Timer.DelayCall(SliceInterval, SliceInterval, new TimerCallback(Slice));
        }

        public void HandleTie(Arena arena, TournyMatch match, ArrayList remaining)
        {
            if (remaining.Count == 1)
                HandleWon(arena, match, (TournyParticipant)remaining[0]);

            if (remaining.Count < 2)
                return;

            StringBuilder sb = new StringBuilder();

            sb.Append("The match has ended in a tie ");

            if (remaining.Count == 2)
                sb.Append("between ");
            else
                sb.Append("among ");

            sb.Append(remaining.Count);

            if (((TournyParticipant)remaining[0]).Players.Count == 1)
                sb.Append(" players: ");
            else
                sb.Append(" teams: ");

            bool hasAppended = false;

            for (int j = 0; j < match.Participants.Count; ++j)
            {
                TournyParticipant part = (TournyParticipant)match.Participants[j];

                if (remaining.Contains(part))
                {
                    if (hasAppended)
                        sb.Append(", ");

                    sb.Append(part.NameList);
                    hasAppended = true;
                }
                else
                {
                    m_Undefeated.Remove(part);
                }
            }

            sb.Append(". ");

            string whole = (remaining.Count == 2 ? "both" : "all");

            TieType tieType = m_TieType;

            if (tieType == TieType.FullElimination && remaining.Count >= m_Undefeated.Count)
                tieType = TieType.FullAdvancement;

            switch (m_TieType)
            {
                case TieType.FullAdvancement:
                    {
                        sb.AppendFormat("In accordance with the rules, {0} parties are advanced.", whole);
                        break;
                    }
                case TieType.FullElimination:
                    {
                        for (int j = 0; j < remaining.Count; ++j)
                            m_Undefeated.Remove(remaining[j]);

                        sb.AppendFormat("In accordance with the rules, {0} parties are eliminated.", whole);
                        break;
                    }
                case TieType.Random:
                    {
                        TournyParticipant advanced = (TournyParticipant)remaining[Utility.Random(remaining.Count)];

                        for (int i = 0; i < remaining.Count; ++i)
                        {
                            if (remaining[i] != advanced)
                                m_Undefeated.Remove(remaining[i]);
                        }

                        if (advanced != null)
                            sb.AppendFormat("In accordance with the rules, {0} {1} advanced.", advanced.NameList, advanced.Players.Count == 1 ? "is" : "are");

                        break;
                    }
                case TieType.Highest:
                    {
                        TournyParticipant advanced = null;

                        for (int i = 0; i < remaining.Count; ++i)
                        {
                            TournyParticipant part = (TournyParticipant)remaining[i];

                            if (advanced == null || part.TotalLadderXP > advanced.TotalLadderXP)
                                advanced = part;
                        }

                        for (int i = 0; i < remaining.Count; ++i)
                        {
                            if (remaining[i] != advanced)
                                m_Undefeated.Remove(remaining[i]);
                        }

                        if (advanced != null)
                            sb.AppendFormat("In accordance with the rules, {0} {1} advanced.", advanced.NameList, advanced.Players.Count == 1 ? "is" : "are");

                        break;
                    }
                case TieType.Lowest:
                    {
                        TournyParticipant advanced = null;

                        for (int i = 0; i < remaining.Count; ++i)
                        {
                            TournyParticipant part = (TournyParticipant)remaining[i];

                            if (advanced == null || part.TotalLadderXP < advanced.TotalLadderXP)
                                advanced = part;
                        }

                        for (int i = 0; i < remaining.Count; ++i)
                        {
                            if (remaining[i] != advanced)
                                m_Undefeated.Remove(remaining[i]);
                        }

                        if (advanced != null)
                            sb.AppendFormat("In accordance with the rules, {0} {1} advanced.", advanced.NameList, advanced.Players.Count == 1 ? "is" : "are");

                        break;
                    }
            }

            Alert(arena, sb.ToString());
        }

        public void OnEliminated(DuelPlayer player)
        {
            Participant part = player.Participant;

            if (!part.Eliminated)
                return;

            if (m_TournyType == TournyType.FreeForAll)
            {
                int rem = 0;

                for (int i = 0; i < part.Context.Participants.Count; ++i)
                {
                    Participant check = (Participant)part.Context.Participants[i];

                    if (check != null && !check.Eliminated)
                        ++rem;
                }

                TournyParticipant tp = part.TournyPart;

                if (tp == null)
                    return;

                if (rem == 1)
                    GiveAwards(tp.Players, TrophyRank.Silver, ComputeCashAward() / 2);
                else if (rem == 2)
                    GiveAwards(tp.Players, TrophyRank.Bronze, ComputeCashAward() / 4);
            }
        }

        public void HandleWon(Arena arena, TournyMatch match, TournyParticipant winner)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("The match is complete. ");
            sb.Append(winner.NameList);

            if (winner.Players.Count > 1)
                sb.Append(" have bested ");
            else
                sb.Append(" has bested ");

            if (match.Participants.Count > 2)
                sb.AppendFormat("{0} other {1}: ", match.Participants.Count - 1, winner.Players.Count == 1 ? "players" : "teams");

            bool hasAppended = false;

            for (int j = 0; j < match.Participants.Count; ++j)
            {
                TournyParticipant part = (TournyParticipant)match.Participants[j];

                if (part == winner)
                    continue;

                m_Undefeated.Remove(part);

                if (hasAppended)
                    sb.Append(", ");

                sb.Append(part.NameList);
                hasAppended = true;
            }

            sb.Append(".");

            if (m_TournyType == TournyType.Standard)
                Alert(arena, sb.ToString());
        }

        private static readonly TimeSpan SliceInterval = TimeSpan.FromSeconds(12.0);

        private int ComputeCashAward()
        {
            return m_Participants.Count * m_PlayersPerParticipant * 2500;
        }

        private void GiveAwards()
        {
            switch (m_TournyType)
            {
                case TournyType.FreeForAll:
                    {
                        if (m_Pyramid.Levels.Count < 1)
                            break;

                        PyramidLevel top = m_Pyramid.Levels[m_Pyramid.Levels.Count - 1] as PyramidLevel;

                        if (top.FreeAdvance != null || top.Matches.Count != 1)
                            break;

                        TournyMatch match = top.Matches[0] as TournyMatch;
                        TournyParticipant winner = match.Winner;

                        if (winner != null)
                            GiveAwards(winner.Players, TrophyRank.Gold, ComputeCashAward());

                        break;
                    }
                case TournyType.Standard:
                    {
                        if (m_Pyramid.Levels.Count < 2)
                            break;

                        PyramidLevel top = m_Pyramid.Levels[m_Pyramid.Levels.Count - 1] as PyramidLevel;

                        if (top.FreeAdvance != null || top.Matches.Count != 1)
                            break;

                        int cash = ComputeCashAward();

                        TournyMatch match = top.Matches[0] as TournyMatch;
                        TournyParticipant winner = match.Winner;

                        for (int i = 0; i < match.Participants.Count; ++i)
                        {
                            TournyParticipant part = (TournyParticipant)match.Participants[i];

                            if (part == winner)
                                GiveAwards(part.Players, TrophyRank.Gold, cash);
                            else
                                GiveAwards(part.Players, TrophyRank.Silver, cash / 2);
                        }

                        PyramidLevel next = m_Pyramid.Levels[m_Pyramid.Levels.Count - 2] as PyramidLevel;

                        if (next.Matches.Count > 2)
                            break;

                        for (int i = 0; i < next.Matches.Count; ++i)
                        {
                            match = (TournyMatch)next.Matches[i];
                            winner = match.Winner;

                            for (int j = 0; j < match.Participants.Count; ++j)
                            {
                                TournyParticipant part = (TournyParticipant)match.Participants[j];

                                if (part != winner)
                                    GiveAwards(part.Players, TrophyRank.Bronze, cash / 4);
                            }
                        }

                        break;
                    }
            }
        }

        private void GiveAwards(ArrayList players, TrophyRank rank, int cash)
        {
            if (players.Count == 0)
                return;

            if (players.Count > 1)
                cash /= (players.Count - 1);

            cash += 500;
            cash /= 1000;
            cash *= 1000;

            StringBuilder sb = new StringBuilder();

            if (m_TournyType == TournyType.FreeForAll)
            {
                sb.Append(m_Participants.Count * m_PlayersPerParticipant);
                sb.Append("-man FFA");
            }
            else if (m_TournyType == TournyType.RandomTeam)
            {
                sb.Append(m_ParticipantsPerMatch);
                sb.Append("-Team");
            }
            else if (m_TournyType == TournyType.Faction)
            {
                sb.Append(m_ParticipantsPerMatch);
                sb.Append("-Team Faction");
            }
            else if (m_TournyType == TournyType.RedVsBlue)
            {
                sb.Append("Red v Blue");
            }
            else
            {
                for (int i = 0; i < m_ParticipantsPerMatch; ++i)
                {
                    if (sb.Length > 0)
                        sb.Append('v');

                    sb.Append(m_PlayersPerParticipant);
                }
            }

            if (m_EventController != null)
                sb.Append(' ').Append(m_EventController.Title);

            sb.Append(" Champion");

            string title = sb.ToString();

            for (int i = 0; i < players.Count; ++i)
            {
                Mobile mob = (Mobile)players[i];

                if (mob == null || mob.Deleted)
                    continue;

                Item item = new Trophy(title, rank);

                if (!mob.PlaceInBackpack(item))
                    mob.BankBox.DropItem(item);

                if (cash > 0)
                {
                    item = new BankCheck(cash);

                    if (!mob.PlaceInBackpack(item))
                        mob.BankBox.DropItem(item);

                    mob.SendMessage("You have been awarded a {0} trophy and {1:N0}gp for your participation in this tournament.", rank.ToString().ToLower(), cash);
                }
                else
                {
                    mob.SendMessage("You have been awarded a {0} trophy for your participation in this tournament.", rank.ToString().ToLower());
                }
            }
        }

        public void Slice()
        {
            if (m_Stage == TournamentStage.Signup)
            {
                TimeSpan until = (m_SignupStart + m_SignupPeriod) - DateTime.UtcNow;

                if (until <= TimeSpan.Zero)
                {
                    for (int i = m_Participants.Count - 1; i >= 0; --i)
                    {
                        TournyParticipant part = (TournyParticipant)m_Participants[i];
                        bool bad = false;

                        for (int j = 0; j < part.Players.Count; ++j)
                        {
                            Mobile check = (Mobile)part.Players[j];

                            if (check.Deleted || check.Map == null || check.Map == Map.Internal || !check.Alive || Factions.Sigil.ExistsOn(check) || check.Region.IsPartOf(typeof(Regions.Jail)))
                            {
                                bad = true;
                                break;
                            }
                        }

                        if (bad)
                        {
                            for (int j = 0; j < part.Players.Count; ++j)
                                ((Mobile)part.Players[j]).SendMessage("You have been disqualified from the tournament.");

                            m_Participants.RemoveAt(i);
                        }
                    }

                    if (m_Participants.Count >= 2)
                    {
                        m_Stage = TournamentStage.Fighting;

                        m_Undefeated.Clear();

                        m_Pyramid.Levels.Clear();
                        m_Pyramid.AddLevel(m_ParticipantsPerMatch, m_Participants, m_GroupType, m_TournyType);

                        PyramidLevel level = (PyramidLevel)m_Pyramid.Levels[0];

                        if (level.FreeAdvance != null)
                            m_Undefeated.Add(level.FreeAdvance);

                        for (int i = 0; i < level.Matches.Count; ++i)
                        {
                            TournyMatch match = (TournyMatch)level.Matches[i];

                            m_Undefeated.AddRange(match.Participants);
                        }

                        Alert("Hear ye! Hear ye!", "The tournament will begin shortly.");
                    }
                    else
                    {
                        /*Alert( "Is this all?", "Pitiful. Signup extended." );
                        m_SignupStart = DateTime.UtcNow;*/

                        Alert("Is this all?", "Pitiful. Tournament cancelled.");
                        m_Stage = TournamentStage.Inactive;
                    }
                }
                else if (Math.Abs(until.TotalSeconds - TimeSpan.FromMinutes(1.0).TotalSeconds) < (SliceInterval.TotalSeconds / 2))
                {
                    Alert("Last call!", "If you wish to enter the tournament, sign up with the registrar now.");
                }
                else if (Math.Abs(until.TotalSeconds - TimeSpan.FromMinutes(5.0).TotalSeconds) < (SliceInterval.TotalSeconds / 2))
                {
                    Alert("The tournament will begin in 5 minutes.", "Sign up now before it's too late.");
                }
            }
            else if (m_Stage == TournamentStage.Fighting)
            {
                if (m_Undefeated.Count == 1)
                {
                    TournyParticipant winner = (TournyParticipant)m_Undefeated[0];

                    try
                    {
                        if (m_EventController != null)
                            Alert("The tournament has completed!", String.Format("Team {0} has won!", m_EventController.GetTeamName(((TournyMatch)((PyramidLevel)m_Pyramid.Levels[0]).Matches[0]).Participants.IndexOf(winner))));
                        else if (m_TournyType == TournyType.RandomTeam)
                            Alert("The tournament has completed!", String.Format("Team {0} has won!", ((TournyMatch)((PyramidLevel)m_Pyramid.Levels[0]).Matches[0]).Participants.IndexOf(winner) + 1));
                        else if (m_TournyType == TournyType.Faction)
                        {
                            if (m_ParticipantsPerMatch == 4)
                            {
                                string name = "(null)";

                                switch (((TournyMatch)((PyramidLevel)m_Pyramid.Levels[0]).Matches[0]).Participants.IndexOf(winner))
                                {
                                    case 0:
                                        {
                                            name = "Minax";
                                            break;
                                        }
                                    case 1:
                                        {
                                            name = "Council of Mages";
                                            break;
                                        }
                                    case 2:
                                        {
                                            name = "True Britannians";
                                            break;
                                        }
                                    case 3:
                                        {
                                            name = "Shadowlords";
                                            break;
                                        }
                                }

                                Alert("The tournament has completed!", String.Format("The {0} team has won!", name));
                            }
                            else if (m_ParticipantsPerMatch == 2)
                            {
                                Alert("The tournament has completed!", String.Format("The {0} team has won!", ((TournyMatch)((PyramidLevel)m_Pyramid.Levels[0]).Matches[0]).Participants.IndexOf(winner) == 0 ? "Evil" : "Hero"));
                            }
                            else
                            {
                                Alert("The tournament has completed!", String.Format("Team {0} has won!", ((TournyMatch)((PyramidLevel)m_Pyramid.Levels[0]).Matches[0]).Participants.IndexOf(winner) + 1));
                            }
                        }
                        else if (m_TournyType == TournyType.RedVsBlue)
                            Alert("The tournament has completed!", String.Format("Team {0} has won!", ((TournyMatch)((PyramidLevel)m_Pyramid.Levels[0]).Matches[0]).Participants.IndexOf(winner) == 0 ? "Red" : "Blue"));
                        else
                            Alert("The tournament has completed!", String.Format("{0} {1} the champion{2}.", winner.NameList, winner.Players.Count > 1 ? "are" : "is", winner.Players.Count == 1 ? "" : "s"));
                    }
                    catch
                    {
                    }

                    GiveAwards();

                    m_Stage = TournamentStage.Inactive;
                    m_Undefeated.Clear();
                }
                else if (m_Pyramid.Levels.Count > 0)
                {
                    PyramidLevel activeLevel = (PyramidLevel)m_Pyramid.Levels[m_Pyramid.Levels.Count - 1];
                    bool stillGoing = false;

                    for (int i = 0; i < activeLevel.Matches.Count; ++i)
                    {
                        TournyMatch match = (TournyMatch)activeLevel.Matches[i];

                        if (match.Winner == null)
                        {
                            stillGoing = true;

                            if (!match.InProgress)
                            {
                                for (int j = 0; j < m_Arenas.Count; ++j)
                                {
                                    Arena arena = (Arena)m_Arenas[j];

                                    if (!arena.IsOccupied)
                                    {
                                        match.Start(arena, this);
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (!stillGoing)
                    {
                        for (int i = m_Undefeated.Count - 1; i >= 0; --i)
                        {
                            TournyParticipant part = (TournyParticipant)m_Undefeated[i];
                            bool bad = false;

                            for (int j = 0; j < part.Players.Count; ++j)
                            {
                                Mobile check = (Mobile)part.Players[j];

                                if (check.Deleted || check.Map == null || check.Map == Map.Internal || !check.Alive || Factions.Sigil.ExistsOn(check) || check.Region.IsPartOf(typeof(Regions.Jail)))
                                {
                                    bad = true;
                                    break;
                                }
                            }

                            if (bad)
                            {
                                for (int j = 0; j < part.Players.Count; ++j)
                                    ((Mobile)part.Players[j]).SendMessage("You have been disqualified from the tournament.");

                                m_Undefeated.RemoveAt(i);

                                if (m_Undefeated.Count == 1)
                                {
                                    TournyParticipant winner = (TournyParticipant)m_Undefeated[0];

                                    try
                                    {
                                        if (m_EventController != null)
                                            Alert("The tournament has completed!", String.Format("Team {0} has won", m_EventController.GetTeamName(((TournyMatch)((PyramidLevel)m_Pyramid.Levels[0]).Matches[0]).Participants.IndexOf(winner))));
                                        else if (m_TournyType == TournyType.RandomTeam)
                                            Alert("The tournament has completed!", String.Format("Team {0} has won!", ((TournyMatch)((PyramidLevel)m_Pyramid.Levels[0]).Matches[0]).Participants.IndexOf(winner) + 1));
                                        else if (m_TournyType == TournyType.Faction)
                                        {
                                            if (m_ParticipantsPerMatch == 4)
                                            {
                                                string name = "(null)";

                                                switch (((TournyMatch)((PyramidLevel)m_Pyramid.Levels[0]).Matches[0]).Participants.IndexOf(winner))
                                                {
                                                    case 0:
                                                        {
                                                            name = "Minax";
                                                            break;
                                                        }
                                                    case 1:
                                                        {
                                                            name = "Council of Mages";
                                                            break;
                                                        }
                                                    case 2:
                                                        {
                                                            name = "True Britannians";
                                                            break;
                                                        }
                                                    case 3:
                                                        {
                                                            name = "Shadowlords";
                                                            break;
                                                        }
                                                }

                                                Alert("The tournament has completed!", String.Format("The {0} team has won!", name));
                                            }
                                            else if (m_ParticipantsPerMatch == 2)
                                            {
                                                Alert("The tournament has completed!", String.Format("The {0} team has won!", ((TournyMatch)((PyramidLevel)m_Pyramid.Levels[0]).Matches[0]).Participants.IndexOf(winner) == 0 ? "Evil" : "Hero"));
                                            }
                                            else
                                            {
                                                Alert("The tournament has completed!", String.Format("Team {0} has won!", ((TournyMatch)((PyramidLevel)m_Pyramid.Levels[0]).Matches[0]).Participants.IndexOf(winner) + 1));
                                            }
                                        }
                                        else if (m_TournyType == TournyType.RedVsBlue)
                                            Alert("The tournament has completed!", String.Format("Team {0} has won!", ((TournyMatch)((PyramidLevel)m_Pyramid.Levels[0]).Matches[0]).Participants.IndexOf(winner) == 0 ? "Red" : "Blue"));
                                        else
                                            Alert("The tournament has completed!", String.Format("{0} {1} the champion{2}.", winner.NameList, winner.Players.Count > 1 ? "are" : "is", winner.Players.Count == 1 ? "" : "s"));
                                    }
                                    catch
                                    {
                                    }

                                    GiveAwards();

                                    m_Stage = TournamentStage.Inactive;
                                    m_Undefeated.Clear();
                                    break;
                                }
                            }
                        }

                        if (m_Undefeated.Count > 1)
                            m_Pyramid.AddLevel(m_ParticipantsPerMatch, m_Undefeated, m_GroupType, m_TournyType);
                    }
                }
            }
        }

        public void Alert(params string[] alerts)
        {
            for (int i = 0; i < m_Arenas.Count; ++i)
                Alert((Arena)m_Arenas[i], alerts);
        }

        public void Alert(Arena arena, params string[] alerts)
        {
            if (arena != null && arena.Announcer != null)
            {
                for (int j = 0; j < alerts.Length; ++j)
                    Timer.DelayCall(TimeSpan.FromSeconds(Math.Max(j - 0.5, 0.0)), new TimerStateCallback(Alert_Callback), new object[] { arena.Announcer, alerts[j] });
            }
        }

        private void Alert_Callback(object state)
        {
            object[] states = (object[])state;

            if (states[0] != null)
                ((Mobile)states[0]).PublicOverheadMessage(MessageType.Regular, 0x35, false, (string)states[1]);
        }
    }

    public class TournyPyramid
    {
        private ArrayList m_Levels;

        public ArrayList Levels
        {
            get { return m_Levels; }
            set { m_Levels = value; }
        }

        public TournyPyramid()
        {
            m_Levels = new ArrayList();
        }

        public void AddLevel(int partsPerMatch, ArrayList participants, GroupingType groupType, TournyType tournyType)
        {
            ArrayList copy = new ArrayList(participants);

            if (groupType == GroupingType.Nearest || groupType == GroupingType.HighVsLow)
                copy.Sort();

            PyramidLevel level = new PyramidLevel();

            switch (tournyType)
            {
                case TournyType.RedVsBlue:
                    {
                        TournyParticipant[] parts = new TournyParticipant[2];

                        for (int i = 0; i < parts.Length; ++i)
                            parts[i] = new TournyParticipant(new ArrayList());

                        for (int i = 0; i < copy.Count; ++i)
                        {
                            ArrayList players = ((TournyParticipant)copy[i]).Players;

                            for (int j = 0; j < players.Count; ++j)
                            {
                                Mobile mob = (Mobile)players[j];

                                if (mob.Kills >= 5)
                                    parts[0].Players.Add(mob);
                                else
                                    parts[1].Players.Add(mob);
                            }
                        }

                        level.Matches.Add(new TournyMatch(new ArrayList(parts)));
                        break;
                    }
                case TournyType.Faction:
                    {
                        TournyParticipant[] parts = new TournyParticipant[partsPerMatch];

                        for (int i = 0; i < parts.Length; ++i)
                            parts[i] = new TournyParticipant(new ArrayList());

                        for (int i = 0; i < copy.Count; ++i)
                        {
                            ArrayList players = ((TournyParticipant)copy[i]).Players;

                            for (int j = 0; j < players.Count; ++j)
                            {
                                Mobile mob = (Mobile)players[j];

                                int index = -1;

                                if (partsPerMatch == 4)
                                {
                                    Faction fac = Faction.Find(mob);

                                    if (fac != null)
                                    {
                                        index = fac.Definition.Sort;
                                    }
                                }
                                else if (partsPerMatch == 2)
                                {
                                    if (Ethic.Evil.IsEligible(mob))
                                    {
                                        index = 0;
                                    }
                                    else if (Ethic.Hero.IsEligible(mob))
                                    {
                                        index = 1;
                                    }
                                }

                                if (index < 0 || index >= partsPerMatch)
                                {
                                    index = i % partsPerMatch;
                                }

                                parts[index].Players.Add(mob);
                            }
                        }

                        level.Matches.Add(new TournyMatch(new ArrayList(parts)));
                        break;
                    }
                case TournyType.RandomTeam:
                    {
                        TournyParticipant[] parts = new TournyParticipant[partsPerMatch];

                        for (int i = 0; i < partsPerMatch; ++i)
                            parts[i] = new TournyParticipant(new ArrayList());

                        for (int i = 0; i < copy.Count; ++i)
                            parts[i % parts.Length].Players.AddRange(((TournyParticipant)copy[i]).Players);

                        level.Matches.Add(new TournyMatch(new ArrayList(parts)));
                        break;
                    }
                case TournyType.FreeForAll:
                    {
                        level.Matches.Add(new TournyMatch(copy));
                        break;
                    }
                case TournyType.Standard:
                    {
                        if (partsPerMatch >= 2 && participants.Count % partsPerMatch == 1)
                        {
                            int lowAdvances = int.MaxValue;

                            for (int i = 0; i < participants.Count; ++i)
                            {
                                TournyParticipant p = (TournyParticipant)participants[i];

                                if (p.FreeAdvances < lowAdvances)
                                    lowAdvances = p.FreeAdvances;
                            }

                            ArrayList toAdvance = new ArrayList();

                            for (int i = 0; i < participants.Count; ++i)
                            {
                                TournyParticipant p = (TournyParticipant)participants[i];

                                if (p.FreeAdvances == lowAdvances)
                                    toAdvance.Add(p);
                            }

                            if (toAdvance.Count == 0)
                                toAdvance = copy; // sanity

                            int idx = Utility.Random(toAdvance.Count);

                            ((TournyParticipant)toAdvance[idx]).AddLog("Advanced automatically due to an odd number of challengers.");
                            level.FreeAdvance = (TournyParticipant)toAdvance[idx];
                            ++level.FreeAdvance.FreeAdvances;
                            copy.Remove(toAdvance[idx]);
                        }

                        while (copy.Count >= partsPerMatch)
                        {
                            ArrayList thisMatch = new ArrayList();

                            for (int i = 0; i < partsPerMatch; ++i)
                            {
                                int idx = 0;

                                switch (groupType)
                                {
                                    case GroupingType.HighVsLow: idx = (i * (copy.Count - 1)) / (partsPerMatch - 1); break;
                                    case GroupingType.Nearest: idx = 0; break;
                                    case GroupingType.Random: idx = Utility.Random(copy.Count); break;
                                }

                                thisMatch.Add(copy[idx]);
                                copy.RemoveAt(idx);
                            }

                            level.Matches.Add(new TournyMatch(thisMatch));
                        }

                        if (copy.Count > 1)
                            level.Matches.Add(new TournyMatch(copy));

                        break;
                    }
            }

            m_Levels.Add(level);
        }
    }

    public class PyramidLevel
    {
        private ArrayList m_Matches;
        private TournyParticipant m_FreeAdvance;

        public ArrayList Matches
        {
            get { return m_Matches; }
            set { m_Matches = value; }
        }

        public TournyParticipant FreeAdvance
        {
            get { return m_FreeAdvance; }
            set { m_FreeAdvance = value; }
        }

        public PyramidLevel()
        {
            m_Matches = new ArrayList();
        }
    }

    public class TournyMatch
    {
        private ArrayList m_Participants;
        private TournyParticipant m_Winner;
        private DuelContext m_Context;

        public ArrayList Participants
        {
            get { return m_Participants; }
            set { m_Participants = value; }
        }

        public TournyParticipant Winner
        {
            get { return m_Winner; }
            set { m_Winner = value; }
        }

        public DuelContext Context
        {
            get { return m_Context; }
            set { m_Context = value; }
        }

        public bool InProgress
        {
            get { return (m_Context != null && m_Context.Registered); }
        }

        public void Start(Arena arena, Tournament tourny)
        {
            TournyParticipant first = (TournyParticipant)m_Participants[0];

            DuelContext dc = new DuelContext((Mobile)first.Players[0], tourny.Ruleset.Layout, false);
            dc.Ruleset.Options.SetAll(false);
            dc.Ruleset.Options.Or(tourny.Ruleset.Options);

            for (int i = 0; i < m_Participants.Count; ++i)
            {
                TournyParticipant tournyPart = (TournyParticipant)m_Participants[i];
                Participant duelPart = new Participant(dc, tournyPart.Players.Count);

                duelPart.TournyPart = tournyPart;

                for (int j = 0; j < tournyPart.Players.Count; ++j)
                    duelPart.Add((Mobile)tournyPart.Players[j]);

                for (int j = 0; j < duelPart.Players.Length; ++j)
                {
                    if (duelPart.Players[j] != null)
                        duelPart.Players[j].Ready = true;
                }

                dc.Participants.Add(duelPart);
            }

            if (tourny.EventController != null)
                dc.m_EventGame = tourny.EventController.Construct(dc);

            dc.m_Tournament = tourny;
            dc.m_Match = this;

            dc.m_OverrideArena = arena;

            if (tourny.SuddenDeath > TimeSpan.Zero && (tourny.SuddenDeathRounds == 0 || tourny.Pyramid.Levels.Count <= tourny.SuddenDeathRounds))
                dc.StartSuddenDeath(tourny.SuddenDeath);

            dc.SendReadyGump(0);

            if (dc.StartedBeginCountdown)
            {
                m_Context = dc;

                for (int i = 0; i < m_Participants.Count; ++i)
                {
                    TournyParticipant p = (TournyParticipant)m_Participants[i];

                    for (int j = 0; j < p.Players.Count; ++j)
                    {
                        Mobile mob = (Mobile)p.Players[j];

                        foreach (Mobile view in mob.GetMobilesInRange(18))
                        {
                            if (!mob.CanSee(view))
                                mob.Send(view.RemovePacket);
                        }

                        mob.LocalOverheadMessage(MessageType.Emote, 0x3B2, false, "* Your mind focuses intently on the fight and all other distractions fade away *");
                    }
                }
            }
            else
            {
                dc.Unregister();
                dc.StopCountdown();
            }
        }

        public TournyMatch(ArrayList participants)
        {
            m_Participants = participants;

            for (int i = 0; i < participants.Count; ++i)
            {
                TournyParticipant part = (TournyParticipant)participants[i];

                StringBuilder sb = new StringBuilder();

                sb.Append("Matched in a duel against ");

                if (participants.Count > 2)
                    sb.AppendFormat("{0} other {1}: ", participants.Count - 1, part.Players.Count == 1 ? "players" : "teams");

                bool hasAppended = false;

                for (int j = 0; j < participants.Count; ++j)
                {
                    if (i == j)
                        continue;

                    if (hasAppended)
                        sb.Append(", ");

                    sb.Append(((TournyParticipant)participants[j]).NameList);
                    hasAppended = true;
                }

                sb.Append(".");

                part.AddLog(sb.ToString());
            }
        }
    }

    public class TournyParticipant : IComparable
    {
        private ArrayList m_Players;
        private ArrayList m_Log;
        private int m_FreeAdvances;

        public ArrayList Players
        {
            get { return m_Players; }
            set { m_Players = value; }
        }

        public ArrayList Log
        {
            get { return m_Log; }
            set { m_Log = value; }
        }

        public int FreeAdvances
        {
            get { return m_FreeAdvances; }
            set { m_FreeAdvances = value; }
        }

        public int TotalLadderXP
        {
            get
            {
                Ladder ladder = Ladder.Instance;

                if (ladder == null)
                    return 0;

                int total = 0;

                for (int i = 0; i < m_Players.Count; ++i)
                {
                    Mobile mob = (Mobile)m_Players[i];
                    LadderEntry entry = ladder.Find(mob);

                    if (entry != null)
                        total += entry.Experience;
                }

                return total;
            }
        }

        public string NameList
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < m_Players.Count; ++i)
                {
                    if (m_Players[i] == null)
                        continue;

                    Mobile mob = (Mobile)m_Players[i];

                    if (sb.Length > 0)
                    {
                        if (m_Players.Count == 2)
                            sb.Append(" and ");
                        else if ((i + 1) < m_Players.Count)
                            sb.Append(", ");
                        else
                            sb.Append(", and ");
                    }

                    sb.Append(mob.Name);
                }

                if (sb.Length == 0)
                    return "Empty";

                return sb.ToString();
            }
        }

        public void AddLog(string text)
        {
            m_Log.Add(text);
        }

        public void AddLog(string format, params object[] args)
        {
            AddLog(String.Format(format, args));
        }

        public void WonMatch(TournyMatch match)
        {
            AddLog("Match won.");
        }

        public void LostMatch(TournyMatch match)
        {
            AddLog("Match lost.");
        }

        public TournyParticipant(Mobile owner)
        {
            m_Log = new ArrayList();
            m_Players = new ArrayList();
            m_Players.Add(owner);
        }

        public TournyParticipant(ArrayList players)
        {
            m_Log = new ArrayList();
            m_Players = players;
        }

        public int CompareTo(object obj)
        {
            TournyParticipant p = (TournyParticipant)obj;

            return p.TotalLadderXP - this.TotalLadderXP;
        }
    }

    public enum TournyBracketGumpType
    {
        Index,
        Rules_Info,
        Participant_List,
        Participant_Info,
        Round_List,
        Round_Info,
        Match_Info,
        Player_Info
    }

    public class TournamentBracketGump : Gump
    {
        private Mobile m_From;
        private Tournament m_Tournament;
        private TournyBracketGumpType m_Type;
        private ArrayList m_List;
        private int m_Page;
        private int m_PerPage;
        private object m_Object;

        private const int BlackColor32 = 0x000008;
        private const int LabelColor32 = 0xFFFFFF;

        public string Center(string text)
        {
            return String.Format("<CENTER>{0}</CENTER>", text);
        }

        public string Color(string text, int color)
        {
            return String.Format("<BASEFONT COLOR=#{0:X6}>{1}</BASEFONT>", color, text);
        }

        private void AddBorderedText(int x, int y, int width, int height, string text, int color, int borderColor)
        {
            AddColoredText(x - 1, y - 1, width, height, text, borderColor);
            AddColoredText(x - 1, y + 1, width, height, text, borderColor);
            AddColoredText(x + 1, y - 1, width, height, text, borderColor);
            AddColoredText(x + 1, y + 1, width, height, text, borderColor);
            AddColoredText(x, y, width, height, text, color);
        }

        private void AddColoredText(int x, int y, int width, int height, string text, int color)
        {
            if (color == 0)
                AddHtml(x, y, width, height, text, false, false);
            else
                AddHtml(x, y, width, height, Color(text, color), false, false);
        }

        public void AddRightArrow(int x, int y, int bid, string text)
        {
            AddButton(x, y, 0x15E1, 0x15E5, bid, GumpButtonType.Reply, 0);

            if (text != null)
                AddHtml(x + 20, y - 1, 230, 20, text, false, false);
        }

        public void AddRightArrow(int x, int y, int bid)
        {
            AddRightArrow(x, y, bid, null);
        }

        public void AddLeftArrow(int x, int y, int bid, string text)
        {
            AddButton(x, y, 0x15E3, 0x15E7, bid, GumpButtonType.Reply, 0);

            if (text != null)
                AddHtml(x + 20, y - 1, 230, 20, text, false, false);
        }

        public void AddLeftArrow(int x, int y, int bid)
        {
            AddLeftArrow(x, y, bid, null);
        }

        public int ToButtonID(int type, int index)
        {
            return 1 + (index * 7) + type;
        }

        public bool FromButtonID(int bid, out int type, out int index)
        {
            type = (bid - 1) % 7;
            index = (bid - 1) / 7;
            return (bid >= 1);
        }

        public void StartPage(out int index, out int count, out int y, int perPage)
        {
            m_PerPage = perPage;

            index = Math.Max(m_Page * perPage, 0);
            count = Math.Max(Math.Min(m_List.Count - index, perPage), 0);

            y = 53 + ((12 - perPage) * 18);

            if (m_Page > 0)
                AddLeftArrow(242, 35, ToButtonID(1, 0));

            if ((m_Page + 1) * perPage < m_List.Count)
                AddRightArrow(260, 35, ToButtonID(1, 1));
        }

        public TournamentBracketGump(Mobile from, Tournament tourny, TournyBracketGumpType type, ArrayList list, int page, object obj)
            : base(50, 50)
        {
            m_From = from;
            m_Tournament = tourny;
            m_Type = type;
            m_List = list;
            m_Page = page;
            m_Object = obj;
            m_PerPage = 12;

            switch (type)
            {
                case TournyBracketGumpType.Index:
                    {
                        AddPage(0);
                        AddBackground(0, 0, 300, 300, 9380);

                        StringBuilder sb = new StringBuilder();

                        if (tourny.TournyType == TournyType.FreeForAll)
                        {
                            sb.Append("FFA");
                        }
                        else if (tourny.TournyType == TournyType.RandomTeam)
                        {
                            sb.Append(tourny.ParticipantsPerMatch);
                            sb.Append("-Team");
                        }
                        else if (tourny.TournyType == TournyType.RedVsBlue)
                        {
                            sb.Append("Red v Blue");
                        }
                        else if (tourny.TournyType == TournyType.Faction)
                        {
                            sb.Append(tourny.ParticipantsPerMatch);
                            sb.Append("-Team Faction");
                        }
                        else
                        {
                            for (int i = 0; i < tourny.ParticipantsPerMatch; ++i)
                            {
                                if (sb.Length > 0)
                                    sb.Append('v');

                                sb.Append(tourny.PlayersPerParticipant);
                            }
                        }

                        if (tourny.EventController != null)
                            sb.Append(' ').Append(tourny.EventController.Title);

                        sb.Append(" Tournament Bracket");

                        AddHtml(25, 35, 250, 20, Center(sb.ToString()), false, false);

                        AddRightArrow(25, 53, ToButtonID(0, 4), "Rules");
                        AddRightArrow(25, 71, ToButtonID(0, 1), "Participants");

                        if (m_Tournament.Stage == TournamentStage.Signup)
                        {
                            TimeSpan until = (m_Tournament.SignupStart + m_Tournament.SignupPeriod) - DateTime.UtcNow;
                            string text;
                            int secs = (int)until.TotalSeconds;

                            if (secs > 0)
                            {
                                int mins = secs / 60;
                                secs %= 60;

                                if (mins > 0 && secs > 0)
                                    text = String.Format("The tournament will begin in {0} minute{1} and {2} second{3}.", mins, mins == 1 ? "" : "s", secs, secs == 1 ? "" : "s");
                                else if (mins > 0)
                                    text = String.Format("The tournament will begin in {0} minute{1}.", mins, mins == 1 ? "" : "s");
                                else if (secs > 0)
                                    text = String.Format("The tournament will begin in {0} second{1}.", secs, secs == 1 ? "" : "s");
                                else
                                    text = "The tournament will begin shortly.";
                            }
                            else
                            {
                                text = "The tournament will begin shortly.";
                            }

                            AddHtml(25, 92, 250, 40, text, false, false);
                        }
                        else
                        {
                            AddRightArrow(25, 89, ToButtonID(0, 2), "Rounds");
                        }

                        break;
                    }
                case TournyBracketGumpType.Rules_Info:
                    {
                        Ruleset ruleset = tourny.Ruleset;
                        Ruleset basedef = ruleset.Base;

                        BitArray defs;

                        if (ruleset.Flavors.Count > 0)
                        {
                            defs = new BitArray(basedef.Options);

                            for (int i = 0; i < ruleset.Flavors.Count; ++i)
                                defs.Or(((Ruleset)ruleset.Flavors[i]).Options);
                        }
                        else
                        {
                            defs = basedef.Options;
                        }

                        int changes = 0;

                        BitArray opts = ruleset.Options;

                        for (int i = 0; i < opts.Length; ++i)
                        {
                            if (defs[i] != opts[i])
                                ++changes;
                        }

                        AddPage(0);
                        AddBackground(0, 0, 300, 60 + 18 + 20 + 20 + 20 + 8 + 20 + (ruleset.Flavors.Count * 18) + 4 + 20 + (changes * 22) + 6, 9380);

                        AddLeftArrow(25, 11, ToButtonID(0, 0));
                        AddHtml(25, 35, 250, 20, Center("Rules"), false, false);

                        int y = 53;

                        string groupText = null;

                        switch (tourny.GroupType)
                        {
                            case GroupingType.HighVsLow: groupText = "High vs Low"; break;
                            case GroupingType.Nearest: groupText = "Closest opponent"; break;
                            case GroupingType.Random: groupText = "Random"; break;
                        }

                        AddHtml(35, y, 190, 20, String.Format("Grouping: {0}", groupText), false, false);
                        y += 20;

                        string tieText = null;

                        switch (tourny.TieType)
                        {
                            case TieType.Random: tieText = "Random"; break;
                            case TieType.Highest: tieText = "Highest advances"; break;
                            case TieType.Lowest: tieText = "Lowest advances"; break;
                            case TieType.FullAdvancement: tieText = (tourny.ParticipantsPerMatch == 2 ? "Both advance" : "Everyone advances"); break;
                            case TieType.FullElimination: tieText = (tourny.ParticipantsPerMatch == 2 ? "Both eliminated" : "Everyone eliminated"); break;
                        }

                        AddHtml(35, y, 190, 20, String.Format("Tiebreaker: {0}", tieText), false, false);
                        y += 20;

                        string sdText = "Off";

                        if (tourny.SuddenDeath > TimeSpan.Zero)
                        {
                            sdText = String.Format("{0}:{1:D2}", (int)tourny.SuddenDeath.TotalMinutes, tourny.SuddenDeath.Seconds);

                            if (tourny.SuddenDeathRounds > 0)
                                sdText = String.Format("{0} (first {1} rounds)", sdText, tourny.SuddenDeathRounds);
                            else
                                sdText = String.Format("{0} (all rounds)", sdText);
                        }

                        AddHtml(35, y, 240, 20, String.Format("Sudden Death: {0}", sdText), false, false);
                        y += 20;

                        y += 8;

                        AddHtml(35, y, 190, 20, String.Format("Ruleset: {0}", basedef.Title), false, false);
                        y += 20;

                        for (int i = 0; i < ruleset.Flavors.Count; ++i, y += 18)
                            AddHtml(35, y, 190, 20, String.Format(" + {0}", ((Ruleset)ruleset.Flavors[i]).Title), false, false);

                        y += 4;

                        if (changes > 0)
                        {
                            AddHtml(35, y, 190, 20, "Modifications:", false, false);
                            y += 20;

                            for (int i = 0; i < opts.Length; ++i)
                            {
                                if (defs[i] != opts[i])
                                {
                                    string name = ruleset.Layout.FindByIndex(i);

                                    if (name != null) // sanity
                                    {
                                        AddImage(35, y, opts[i] ? 0xD3 : 0xD2);
                                        AddHtml(60, y, 165, 22, name, false, false);
                                    }

                                    y += 22;
                                }
                            }
                        }
                        else
                        {
                            AddHtml(35, y, 190, 20, "Modifications: None", false, false);
                            y += 20;
                        }

                        break;
                    }
                case TournyBracketGumpType.Participant_List:
                    {
                        AddPage(0);
                        AddBackground(0, 0, 300, 300, 9380);

                        if (m_List == null)
                            m_List = new ArrayList(tourny.Participants);

                        AddLeftArrow(25, 11, ToButtonID(0, 0));
                        AddHtml(25, 35, 250, 20, Center(String.Format("{0} Participant{1}", m_List.Count, m_List.Count == 1 ? "" : "s")), false, false);

                        int index, count, y;
                        StartPage(out index, out count, out y, 12);

                        for (int i = 0; i < count; ++i, y += 18)
                        {
                            TournyParticipant part = (TournyParticipant)m_List[index + i];
                            string name = part.NameList;

                            if (m_Tournament.TournyType != TournyType.Standard && part.Players.Count == 1)
                            {
                                PlayerMobile pm = part.Players[0] as PlayerMobile;

                                if (pm != null && pm.DuelPlayer != null)
                                    name = Color(name, pm.DuelPlayer.Eliminated ? 0x6633333 : 0x336666);
                            }

                            AddRightArrow(25, y, ToButtonID(2, index + i), name);
                        }

                        break;
                    }
                case TournyBracketGumpType.Participant_Info:
                    {
                        TournyParticipant part = obj as TournyParticipant;

                        if (part == null)
                            break;

                        AddPage(0);
                        AddBackground(0, 0, 300, 60 + 18 + 20 + (part.Players.Count * 18) + 20 + 20 + 160, 9380);

                        AddLeftArrow(25, 11, ToButtonID(0, 1));
                        AddHtml(25, 35, 250, 20, Center("Participants"), false, false);

                        int y = 53;

                        AddHtml(25, y, 200, 20, part.Players.Count == 1 ? "Players" : "Team", false, false);
                        y += 20;

                        for (int i = 0; i < part.Players.Count; ++i)
                        {
                            Mobile mob = (Mobile)part.Players[i];
                            string name = mob.Name;

                            if (m_Tournament.TournyType != TournyType.Standard)
                            {
                                PlayerMobile pm = mob as PlayerMobile;

                                if (pm != null && pm.DuelPlayer != null)
                                    name = Color(name, pm.DuelPlayer.Eliminated ? 0x6633333 : 0x336666);
                            }

                            AddRightArrow(35, y, ToButtonID(4, i), name);
                            y += 18;
                        }

                        AddHtml(25, y, 200, 20, String.Format("Free Advances: {0}", part.FreeAdvances == 0 ? "None" : part.FreeAdvances.ToString()), false, false);
                        y += 20;

                        AddHtml(25, y, 200, 20, "Log:", false, false);
                        y += 20;

                        StringBuilder sb = new StringBuilder();

                        for (int i = 0; i < part.Log.Count; ++i)
                        {
                            if (sb.Length > 0)
                                sb.Append("<br>");

                            sb.Append(part.Log[i]);
                        }

                        if (sb.Length == 0)
                            sb.Append("Nothing logged yet.");

                        AddHtml(25, y, 250, 150, Color(sb.ToString(), BlackColor32), false, true);

                        break;
                    }
                case TournyBracketGumpType.Player_Info:
                    {
                        AddPage(0);
                        AddBackground(0, 0, 300, 300, 9380);

                        AddLeftArrow(25, 11, ToButtonID(0, 3));
                        AddHtml(25, 35, 250, 20, Center("Participants"), false, false);

                        Mobile mob = obj as Mobile;

                        if (mob == null)
                            break;

                        Ladder ladder = Ladder.Instance;
                        LadderEntry entry = (ladder == null ? null : ladder.Find(mob));

                        AddHtml(25, 53, 250, 20, String.Format("Name: {0}", mob.Name), false, false);
                        AddHtml(25, 73, 250, 20, String.Format("Guild: {0}", mob.Guild == null ? "None" : mob.Guild.Name + " [" + mob.Guild.Abbreviation + "]"), false, false);
                        AddHtml(25, 93, 250, 20, String.Format("Rank: {0}", entry == null ? "N/A" : LadderGump.Rank(entry.Index + 1)), false, false);
                        AddHtml(25, 113, 250, 20, String.Format("Level: {0}", entry == null ? 0 : Ladder.GetLevel(entry.Experience)), false, false);
                        AddHtml(25, 133, 250, 20, String.Format("Wins: {0:N0}", entry == null ? 0 : entry.Wins), false, false);
                        AddHtml(25, 153, 250, 20, String.Format("Losses: {0:N0}", entry == null ? 0 : entry.Losses), false, false);

                        break;
                    }
                case TournyBracketGumpType.Round_List:
                    {
                        AddPage(0);
                        AddBackground(0, 0, 300, 300, 9380);

                        AddLeftArrow(25, 11, ToButtonID(0, 0));
                        AddHtml(25, 35, 250, 20, Center("Rounds"), false, false);

                        if (m_List == null)
                            m_List = new ArrayList(tourny.Pyramid.Levels);

                        int index, count, y;
                        StartPage(out index, out count, out y, 12);

                        for (int i = 0; i < count; ++i, y += 18)
                        {
                            PyramidLevel level = (PyramidLevel)m_List[index + i];

                            AddRightArrow(25, y, ToButtonID(3, index + i), "Round #" + (index + i + 1));
                        }

                        break;
                    }
                case TournyBracketGumpType.Round_Info:
                    {
                        AddPage(0);
                        AddBackground(0, 0, 300, 300, 9380);

                        AddLeftArrow(25, 11, ToButtonID(0, 2));
                        AddHtml(25, 35, 250, 20, Center("Rounds"), false, false);

                        PyramidLevel level = m_Object as PyramidLevel;

                        if (level == null)
                            break;

                        if (m_List == null)
                            m_List = new ArrayList(level.Matches);

                        AddRightArrow(25, 53, ToButtonID(5, 0), String.Format("Free Advance: {0}", level.FreeAdvance == null ? "None" : level.FreeAdvance.NameList));

                        AddHtml(25, 73, 200, 20, String.Format("{0} Match{1}", m_List.Count, m_List.Count == 1 ? "" : "es"), false, false);

                        int index, count, y;
                        StartPage(out index, out count, out y, 10);

                        for (int i = 0; i < count; ++i, y += 18)
                        {
                            TournyMatch match = (TournyMatch)m_List[index + i];

                            int color = -1;

                            if (match.InProgress)
                                color = 0x336666;
                            else if (match.Context != null && match.Winner == null)
                                color = 0x666666;

                            StringBuilder sb = new StringBuilder();

                            if (m_Tournament.TournyType == TournyType.Standard)
                            {
                                for (int j = 0; j < match.Participants.Count; ++j)
                                {
                                    if (sb.Length > 0)
                                        sb.Append(" vs ");

                                    TournyParticipant part = (TournyParticipant)match.Participants[j];
                                    string txt = part.NameList;

                                    if (color == -1 && match.Context != null && match.Winner == part)
                                        txt = Color(txt, 0x336633);
                                    else if (color == -1 && match.Context != null)
                                        txt = Color(txt, 0x663333);

                                    sb.Append(txt);
                                }
                            }
                            else if (m_Tournament.EventController != null || m_Tournament.TournyType == TournyType.RandomTeam || m_Tournament.TournyType == TournyType.RedVsBlue || m_Tournament.TournyType == TournyType.Faction)
                            {
                                for (int j = 0; j < match.Participants.Count; ++j)
                                {
                                    if (sb.Length > 0)
                                        sb.Append(" vs ");

                                    TournyParticipant part = (TournyParticipant)match.Participants[j];
                                    string txt;

                                    if (m_Tournament.EventController != null)
                                        txt = String.Format("Team {0} ({1})", m_Tournament.EventController.GetTeamName(j), part.Players.Count);
                                    else if (m_Tournament.TournyType == TournyType.RandomTeam)
                                        txt = String.Format("Team {0} ({1})", j + 1, part.Players.Count);
                                    else if (m_Tournament.TournyType == TournyType.Faction)
                                    {
                                        if (m_Tournament.ParticipantsPerMatch == 4)
                                        {
                                            string name = "(null)";

                                            switch (j)
                                            {
                                                case 0:
                                                    {
                                                        name = "Minax";
                                                        break;
                                                    }
                                                case 1:
                                                    {
                                                        name = "Council of Mages";
                                                        break;
                                                    }
                                                case 2:
                                                    {
                                                        name = "True Britannians";
                                                        break;
                                                    }
                                                case 3:
                                                    {
                                                        name = "Shadowlords";
                                                        break;
                                                    }
                                            }

                                            txt = String.Format("{0} ({1})", name, part.Players.Count);
                                        }
                                        else if (m_Tournament.ParticipantsPerMatch == 2)
                                        {
                                            txt = String.Format("{0} Team ({1})", j == 0 ? "Evil" : "Hero", part.Players.Count);
                                        }
                                        else
                                        {
                                            txt = String.Format("Team {0} ({1})", j + 1, part.Players.Count);
                                        }
                                    }
                                    else
                                        txt = String.Format("Team {0} ({1})", j == 0 ? "Red" : "Blue", part.Players.Count);

                                    if (color == -1 && match.Context != null && match.Winner == part)
                                        txt = Color(txt, 0x336633);
                                    else if (color == -1 && match.Context != null)
                                        txt = Color(txt, 0x663333);

                                    sb.Append(txt);
                                }
                            }
                            else if (m_Tournament.TournyType == TournyType.FreeForAll)
                            {
                                sb.Append("Free For All");
                            }

                            string str = sb.ToString();

                            if (color >= 0)
                                str = Color(str, color);

                            AddRightArrow(25, y, ToButtonID(5, index + i + 1), str);
                        }

                        break;
                    }
                case TournyBracketGumpType.Match_Info:
                    {
                        TournyMatch match = obj as TournyMatch;

                        if (match == null)
                            break;

                        int ct = (m_Tournament.TournyType == TournyType.FreeForAll ? 2 : match.Participants.Count);

                        AddPage(0);
                        AddBackground(0, 0, 300, 60 + 18 + 20 + 20 + 20 + (ct * 18) + 6, 9380);

                        AddLeftArrow(25, 11, ToButtonID(0, 5));
                        AddHtml(25, 35, 250, 20, Center("Rounds"), false, false);

                        AddHtml(25, 53, 250, 20, String.Format("Winner: {0}", match.Winner == null ? "N/A" : match.Winner.NameList), false, false);
                        AddHtml(25, 73, 250, 20, String.Format("State: {0}", match.InProgress ? "In progress" : match.Context != null ? "Complete" : "Waiting"), false, false);
                        AddHtml(25, 93, 250, 20, String.Format("Participants:"), false, false);

                        if (m_Tournament.TournyType == TournyType.Standard)
                        {
                            for (int i = 0; i < match.Participants.Count; ++i)
                            {
                                TournyParticipant part = (TournyParticipant)match.Participants[i];

                                AddRightArrow(25, 113 + (i * 18), ToButtonID(6, i), part.NameList);
                            }
                        }
                        else if (m_Tournament.EventController != null || m_Tournament.TournyType == TournyType.RandomTeam || m_Tournament.TournyType == TournyType.RedVsBlue || m_Tournament.TournyType == TournyType.Faction)
                        {
                            for (int i = 0; i < match.Participants.Count; ++i)
                            {
                                TournyParticipant part = (TournyParticipant)match.Participants[i];

                                if (m_Tournament.EventController != null)
                                    AddRightArrow(25, 113 + (i * 18), ToButtonID(6, i), String.Format("Team {0} ({1})", m_Tournament.EventController.GetTeamName(i), part.Players.Count));
                                else if (m_Tournament.TournyType == TournyType.RandomTeam)
                                    AddRightArrow(25, 113 + (i * 18), ToButtonID(6, i), String.Format("Team {0} ({1})", i + 1, part.Players.Count));
                                else if (m_Tournament.TournyType == TournyType.Faction)
                                {
                                    if (m_Tournament.ParticipantsPerMatch == 4)
                                    {
                                        string name = "(null)";

                                        switch (i)
                                        {
                                            case 0:
                                                {
                                                    name = "Minax";
                                                    break;
                                                }
                                            case 1:
                                                {
                                                    name = "Council of Mages";
                                                    break;
                                                }
                                            case 2:
                                                {
                                                    name = "True Britannians";
                                                    break;
                                                }
                                            case 3:
                                                {
                                                    name = "Shadowlords";
                                                    break;
                                                }
                                        }

                                        AddRightArrow(25, 113 + (i * 18), ToButtonID(6, i), String.Format("{0} ({1})", name, part.Players.Count));
                                    }
                                    else if (m_Tournament.ParticipantsPerMatch == 2)
                                    {
                                        AddRightArrow(25, 113 + (i * 18), ToButtonID(6, i), String.Format("{0} Team ({1})", i == 0 ? "Evil" : "Hero", part.Players.Count));
                                    }
                                    else
                                    {
                                        AddRightArrow(25, 113 + (i * 18), ToButtonID(6, i), String.Format("Team {0} ({1})", i + 1, part.Players.Count));
                                    }
                                }
                                else
                                    AddRightArrow(25, 113 + (i * 18), ToButtonID(6, i), String.Format("Team {0} ({1})", i == 0 ? "Red" : "Blue", part.Players.Count));
                            }
                        }
                        else if (m_Tournament.TournyType == TournyType.FreeForAll)
                        {
                            AddHtml(25, 113, 250, 20, "Free For All", false, false);
                        }

                        break;
                    }
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            int type, index;

            if (!FromButtonID(info.ButtonID, out type, out index))
                return;

            switch (type)
            {
                case 0:
                    {
                        switch (index)
                        {
                            case 0: m_From.SendGump(new TournamentBracketGump(m_From, m_Tournament, TournyBracketGumpType.Index, null, 0, null)); break;
                            case 1: m_From.SendGump(new TournamentBracketGump(m_From, m_Tournament, TournyBracketGumpType.Participant_List, null, 0, null)); break;
                            case 2: m_From.SendGump(new TournamentBracketGump(m_From, m_Tournament, TournyBracketGumpType.Round_List, null, 0, null)); break;
                            case 4: m_From.SendGump(new TournamentBracketGump(m_From, m_Tournament, TournyBracketGumpType.Rules_Info, null, 0, null)); break;
                            case 3:
                                {
                                    Mobile mob = m_Object as Mobile;

                                    for (int i = 0; i < m_Tournament.Participants.Count; ++i)
                                    {
                                        TournyParticipant part = (TournyParticipant)m_Tournament.Participants[i];

                                        if (part.Players.Contains(mob))
                                        {
                                            m_From.SendGump(new TournamentBracketGump(m_From, m_Tournament, TournyBracketGumpType.Participant_Info, null, 0, part));
                                            break;
                                        }
                                    }

                                    break;
                                }
                            case 5:
                                {
                                    TournyMatch match = m_Object as TournyMatch;

                                    if (match == null)
                                        break;

                                    for (int i = 0; i < m_Tournament.Pyramid.Levels.Count; ++i)
                                    {
                                        PyramidLevel level = (PyramidLevel)m_Tournament.Pyramid.Levels[i];

                                        if (level.Matches.Contains(match))
                                            m_From.SendGump(new TournamentBracketGump(m_From, m_Tournament, TournyBracketGumpType.Round_Info, null, 0, level));
                                    }

                                    break;
                                }
                        }

                        break;
                    }
                case 1:
                    {
                        switch (index)
                        {
                            case 0:
                                {
                                    if (m_List != null && m_Page > 0)
                                        m_From.SendGump(new TournamentBracketGump(m_From, m_Tournament, m_Type, m_List, m_Page - 1, m_Object));

                                    break;
                                }
                            case 1:
                                {
                                    if (m_List != null && ((m_Page + 1) * m_PerPage) < m_List.Count)
                                        m_From.SendGump(new TournamentBracketGump(m_From, m_Tournament, m_Type, m_List, m_Page + 1, m_Object));

                                    break;
                                }
                        }

                        break;
                    }
                case 2:
                    {
                        if (m_Type != TournyBracketGumpType.Participant_List)
                            break;

                        if (index >= 0 && index < m_List.Count)
                            m_From.SendGump(new TournamentBracketGump(m_From, m_Tournament, TournyBracketGumpType.Participant_Info, null, 0, m_List[index]));

                        break;
                    }
                case 3:
                    {
                        if (m_Type != TournyBracketGumpType.Round_List)
                            break;

                        if (index >= 0 && index < m_List.Count)
                            m_From.SendGump(new TournamentBracketGump(m_From, m_Tournament, TournyBracketGumpType.Round_Info, null, 0, m_List[index]));

                        break;
                    }
                case 4:
                    {
                        if (m_Type != TournyBracketGumpType.Participant_Info)
                            break;

                        TournyParticipant part = m_Object as TournyParticipant;

                        if (part != null && index >= 0 && index < part.Players.Count)
                            m_From.SendGump(new TournamentBracketGump(m_From, m_Tournament, TournyBracketGumpType.Player_Info, null, 0, part.Players[index]));

                        break;
                    }
                case 5:
                    {
                        if (m_Type != TournyBracketGumpType.Round_Info)
                            break;

                        PyramidLevel level = m_Object as PyramidLevel;

                        if (level == null)
                            break;

                        if (index == 0)
                        {
                            if (level.FreeAdvance != null)
                                m_From.SendGump(new TournamentBracketGump(m_From, m_Tournament, TournyBracketGumpType.Participant_Info, null, 0, level.FreeAdvance));
                            else
                                m_From.SendGump(new TournamentBracketGump(m_From, m_Tournament, m_Type, m_List, m_Page, m_Object));
                        }
                        else if (index >= 1 && index <= level.Matches.Count)
                        {
                            m_From.SendGump(new TournamentBracketGump(m_From, m_Tournament, TournyBracketGumpType.Match_Info, null, 0, level.Matches[index - 1]));
                        }

                        break;
                    }
                case 6:
                    {
                        if (m_Type != TournyBracketGumpType.Match_Info)
                            break;

                        TournyMatch match = m_Object as TournyMatch;

                        if (match != null && index >= 0 && index < match.Participants.Count)
                            m_From.SendGump(new TournamentBracketGump(m_From, m_Tournament, TournyBracketGumpType.Participant_Info, null, 0, match.Participants[index]));

                        break;
                    }
            }
        }
    }

    public class TournamentBracketItem : Item
    {
        private TournamentController m_Tournament;

        [CommandProperty(AccessLevel.GameMaster)]
        public TournamentController Tournament { get { return m_Tournament; } set { m_Tournament = value; } }

        public override string DefaultName
        {
            get { return "tournament bracket"; }
        }

        [Constructable]
        public TournamentBracketItem()
            : base(3774)
        {
            Movable = false;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(GetWorldLocation(), 2))
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that
            }
            else if (m_Tournament != null)
            {
                Tournament tourny = m_Tournament.Tournament;

                if (tourny != null)
                {
                    from.CloseGump(typeof(TournamentBracketGump));
                    from.SendGump(new TournamentBracketGump(from, tourny, TournyBracketGumpType.Index, null, 0, null));

                    /*if ( tourny.Stage == TournamentStage.Fighting && tourny.Pyramid.Levels.Count > 0 )
                        from.SendGump( new TournamentBracketGump( tourny, (PyramidLevel)tourny.Pyramid.Levels[tourny.Pyramid.Levels.Count - 1] ) );
                    else
                        from.SendGump( new TournamentBracketGump( tourny, 0 ) );*/
                }
            }
        }

        public TournamentBracketItem(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);

            writer.Write((Item)m_Tournament);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_Tournament = reader.ReadItem() as TournamentController;
                        break;
                    }
            }
        }
    }

    #endregion

    #region Event Gaming Competition

    public abstract class EventController : Item
    {
        public abstract EventGame Construct(DuelContext dc);

        public abstract string Title { get; }

        public abstract string GetTeamName(int teamID);

        public EventController()
            : base(0x1B7A)
        {
            Visible = false;
            Movable = false;
        }

        public EventController(Serial serial)
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

        public override void OnDoubleClick(Mobile from)
        {
            if (from.AccessLevel >= AccessLevel.GameMaster)
                from.SendGump(new Gumps.PropertiesGump(from, this));
        }
    }

    public abstract class EventGame
    {
        protected DuelContext m_Context;

        public DuelContext Context { get { return m_Context; } }

        public virtual bool FreeConsume { get { return true; } }

        public EventGame(DuelContext context)
        {
            m_Context = context;
        }

        public virtual bool OnDeath(Mobile mob, Container corpse)
        {
            return true;
        }

        public virtual bool CantDoAnything(Mobile mob)
        {
            return false;
        }

        public virtual void OnStart()
        {
        }

        public virtual void OnStop()
        {
        }
    }

    #endregion

    public class ReadyUpGump : Gump
    {
        private Mobile m_From;
        private DuelContext m_Context;

        public string Center(string text)
        {
            return String.Format("<CENTER>{0}</CENTER>", text);
        }

        public void AddGoldenButton(int x, int y, int bid)
        {
            AddButton(x, y, 0xD2, 0xD2, bid, GumpButtonType.Reply, 0);
            AddButton(x + 3, y + 3, 0xD8, 0xD8, bid, GumpButtonType.Reply, 0);
        }

        public ReadyUpGump(Mobile from, DuelContext context)
            : base(50, 50)
        {
            m_From = from;
            m_Context = context;

            Closable = false;
            AddPage(0);

            if (context.Rematch)
            {
                int height = 25 + 20 + 10 + 22 + 25;

                AddBackground(0, 0, 210, height, 9250);
                AddBackground(10, 10, 190, height - 20, 0xDAC);

                AddHtml(35, 25, 140, 20, Center("Rematch?"), false, false);

                AddButton(35, 55, 247, 248, 1, GumpButtonType.Reply, 0);
                AddButton(115, 55, 242, 241, 2, GumpButtonType.Reply, 0);
            }
            else
            {
                #region Participants
                AddPage(1);

                ArrayList parts = context.Participants;

                int height = 25 + 20;

                for (int i = 0; i < parts.Count; ++i)
                {
                    Participant p = (Participant)parts[i];

                    height += 4;

                    if (p.Players.Length > 1)
                        height += 22;

                    height += (p.Players.Length * 22);
                }

                height += 10 + 22 + 25;

                AddBackground(0, 0, 260, height, 9250);
                AddBackground(10, 10, 240, height - 20, 0xDAC);

                AddHtml(35, 25, 190, 20, Center("Participants"), false, false);

                int y = 20 + 25;

                for (int i = 0; i < parts.Count; ++i)
                {
                    Participant p = (Participant)parts[i];

                    y += 4;

                    int offset = 0;

                    if (p.Players.Length > 1)
                    {
                        AddHtml(35, y, 176, 20, String.Format("Team #{0}", i + 1), false, false);
                        y += 22;
                        offset = 10;
                    }

                    for (int j = 0; j < p.Players.Length; ++j)
                    {
                        DuelPlayer pl = p.Players[j];

                        string name = (pl == null ? "(Empty)" : pl.Mobile.Name);

                        AddHtml(35 + offset, y, 166, 20, name, false, false);

                        y += 22;
                    }
                }

                y += 8;

                AddHtml(35, y, 176, 20, "Continue?", false, false);

                y -= 2;

                AddButton(102, y, 247, 248, 0, GumpButtonType.Page, 2);
                AddButton(169, y, 242, 241, 2, GumpButtonType.Reply, 0);
                #endregion

                #region Rules
                AddPage(2);

                Ruleset ruleset = context.Ruleset;
                Ruleset basedef = ruleset.Base;

                height = 25 + 20 + 5 + 20 + 20 + 4;

                int changes = 0;

                BitArray defs;

                if (ruleset.Flavors.Count > 0)
                {
                    defs = new BitArray(basedef.Options);

                    for (int i = 0; i < ruleset.Flavors.Count; ++i)
                        defs.Or(((Ruleset)ruleset.Flavors[i]).Options);

                    height += ruleset.Flavors.Count * 18;
                }
                else
                {
                    defs = basedef.Options;
                }

                BitArray opts = ruleset.Options;

                for (int i = 0; i < opts.Length; ++i)
                {
                    if (defs[i] != opts[i])
                        ++changes;
                }

                height += (changes * 22);

                height += 10 + 22 + 25;

                AddBackground(0, 0, 260, height, 9250);
                AddBackground(10, 10, 240, height - 20, 0xDAC);

                AddHtml(35, 25, 190, 20, Center("Rules"), false, false);

                AddHtml(35, 50, 190, 20, String.Format("Set: {0}", basedef.Title), false, false);

                y = 70;

                for (int i = 0; i < ruleset.Flavors.Count; ++i, y += 18)
                    AddHtml(35, y, 190, 20, String.Format(" + {0}", ((Ruleset)ruleset.Flavors[i]).Title), false, false);

                y += 4;

                if (changes > 0)
                {
                    AddHtml(35, y, 190, 20, "Modifications:", false, false);
                    y += 20;

                    for (int i = 0; i < opts.Length; ++i)
                    {
                        if (defs[i] != opts[i])
                        {
                            string name = ruleset.Layout.FindByIndex(i);

                            if (name != null) // sanity
                            {
                                AddImage(35, y, opts[i] ? 0xD3 : 0xD2);
                                AddHtml(60, y, 165, 22, name, false, false);
                            }

                            y += 22;
                        }
                    }
                }
                else
                {
                    AddHtml(35, y, 190, 20, "Modifications: None", false, false);
                    y += 20;
                }

                y += 8;

                AddHtml(35, y, 176, 20, "Continue?", false, false);

                y -= 2;

                AddButton(102, y, 247, 248, 1, GumpButtonType.Reply, 0);
                AddButton(169, y, 242, 241, 3, GumpButtonType.Reply, 0);
                #endregion
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (!m_Context.Registered || !m_Context.ReadyWait)
                return;

            switch (info.ButtonID)
            {
                case 1: // okay
                    {
                        PlayerMobile pm = m_From as PlayerMobile;

                        if (pm == null)
                            break;

                        pm.DuelPlayer.Ready = true;
                        m_Context.SendReadyGump();

                        break;
                    }
                case 2: // reject participants
                    {
                        m_Context.RejectReady(m_From, "participants");
                        break;
                    }
                case 3: // reject rules
                    {
                        m_Context.RejectReady(m_From, "rules");
                        break;
                    }
            }
        }
    }
}