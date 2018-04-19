using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Server;
using Server.Accounting;
using Server.Commands;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

namespace Server.Engines.Help
{
    public class SpeechLog : IEnumerable<SpeechLogEntry>
    {
        // Are speech logs enabled?
        public static readonly bool Enabled = true;

        // How long should we maintain each speech entry?
        public static readonly TimeSpan EntryDuration = TimeSpan.FromMinutes(20.0);

        // What is the maximum number of entries a log can contain? (0 -> no limit)
        public static readonly int MaxLength = 0;

        public static void Initialize()
        {
            CommandSystem.Register("SpeechLog", AccessLevel.Counselor, new CommandEventHandler(SpeechLog_OnCommand));
        }

        [Usage("SpeechLog")]
        [Description("Opens the speech log of a given target.")]
        private static void SpeechLog_OnCommand(CommandEventArgs e)
        {
            Mobile from = e.Mobile;

            from.SendMessage("Target a player to view his speech log.");
            e.Mobile.Target = new SpeechLogTarget();
        }

        private class SpeechLogTarget : Target
        {
            public SpeechLogTarget()
                : base(-1, false, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                PlayerMobile pm = targeted as PlayerMobile;

                if (pm == null)
                {
                    from.SendMessage("Speech logs aren't supported on that target.");
                }
                else if (from != targeted && from.AccessLevel <= pm.AccessLevel && from.AccessLevel != AccessLevel.Owner)
                {
                    from.SendMessage("You don't have the required access level to view {0} speech log.", pm.Female ? "her" : "his");
                }
                else if (pm.SpeechLog == null)
                {
                    from.SendMessage("{0} has no speech log.", pm.Female ? "She" : "He");
                }
                else
                {
                    CommandLogging.WriteLine(from, "{0} {1} viewing speech log of {2}", from.AccessLevel, CommandLogging.Format(from), CommandLogging.Format(targeted));

                    Gump gump = new SpeechLogGump(pm, pm.SpeechLog);
                    from.SendGump(gump);
                }
            }
        }

        private Queue<SpeechLogEntry> m_Queue;

        public int Count { get { return m_Queue.Count; } }

        public SpeechLog()
        {
            m_Queue = new Queue<SpeechLogEntry>();
        }

        public void Add(Mobile from, string speech)
        {
            Add(new SpeechLogEntry(from, speech));
        }

        public void Add(SpeechLogEntry entry)
        {
            if (MaxLength > 0 && m_Queue.Count >= MaxLength)
                m_Queue.Dequeue();

            Clean();

            m_Queue.Enqueue(entry);
        }

        public void Clean()
        {
            while (m_Queue.Count > 0)
            {
                SpeechLogEntry entry = (SpeechLogEntry)m_Queue.Peek();

                if (DateTime.UtcNow - entry.Created > EntryDuration)
                    m_Queue.Dequeue();
                else
                    break;
            }
        }

        public void CopyTo(SpeechLogEntry[] array, int index)
        {
            m_Queue.CopyTo(array, index);
        }

        #region IEnumerable<SpeechLogEntry> Members

        IEnumerator<SpeechLogEntry> IEnumerable<SpeechLogEntry>.GetEnumerator()
        {
            return m_Queue.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_Queue.GetEnumerator();
        }

        #endregion
    }

    public class SpeechLogEntry
    {
        private Mobile m_From;
        private string m_Speech;
        private DateTime m_Created;

        public Mobile From { get { return m_From; } }
        public string Speech { get { return m_Speech; } }
        public DateTime Created { get { return m_Created; } }

        public SpeechLogEntry(Mobile from, string speech)
        {
            m_From = from;
            m_Speech = speech;
            m_Created = DateTime.UtcNow;
        }
    }

    public class SpeechLogGump : Gump
    {
        public static readonly int MaxEntriesPerPage = 30;

        private Mobile m_Player;
        private List<SpeechLogEntry> m_Log;
        private int m_Page;

        public SpeechLogGump(Mobile player, SpeechLog log)
            : this(player, new List<SpeechLogEntry>(log))
        {
        }

        public SpeechLogGump(Mobile player, List<SpeechLogEntry> log)
            : this(player, log, (log.Count - 1) / MaxEntriesPerPage)
        {
        }

        public SpeechLogGump(Mobile player, List<SpeechLogEntry> log, int page)
            : base(500, 30)
        {
            m_Player = player;
            m_Log = log;
            m_Page = page;

            AddImageTiled(0, 0, 300, 425, 0xA40);
            AddAlphaRegion(1, 1, 298, 423);

            string playerName = player.Name;
            string playerAccount = player.Account is Account ? player.Account.Username : "???";

            AddHtml(10, 10, 280, 20, String.Format("<basefont color=#A0A0FF><center>SPEECH LOG - {0} (<i>{1}</i>)</center></basefont>", playerName, Utility.FixHtml(playerAccount)), false, false);

            int lastPage = (log.Count - 1) / MaxEntriesPerPage;

            string sLog;

            if (page < 0 || page > lastPage)
            {
                sLog = "";
            }
            else
            {
                int max = log.Count - (lastPage - page) * MaxEntriesPerPage;
                int min = Math.Max(max - MaxEntriesPerPage, 0);

                StringBuilder builder = new StringBuilder();

                for (int i = min; i < max; i++)
                {
                    SpeechLogEntry entry = log[i];

                    Mobile m = entry.From;

                    string name = m.Name;
                    string account = m.Account is Account ? m.Account.Username : "???";
                    string speech = entry.Speech;

                    if (i != min)
                        builder.Append("<br>");

                    builder.AppendFormat("<u>{0}</u> (<i>{1}</i>): {2}", name, Utility.FixHtml(account), Utility.FixHtml(speech));
                }

                sLog = builder.ToString();
            }

            AddHtml(10, 40, 280, 350, sLog, false, true);

            if (page > 0)
                AddButton(10, 395, 0xFAE, 0xFB0, 1, GumpButtonType.Reply, 0); // Previous page

            AddLabel(45, 395, 0x481, String.Format("Current page: {0}/{1}", page + 1, lastPage + 1));

            if (page < lastPage)
                AddButton(261, 395, 0xFA5, 0xFA7, 2, GumpButtonType.Reply, 0); // Next page
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            switch (info.ButtonID)
            {
                case 1: // Previous page
                    {
                        if (m_Page - 1 >= 0)
                            from.SendGump(new SpeechLogGump(m_Player, m_Log, m_Page - 1));

                        break;
                    }
                case 2: // Next page
                    {
                        if ((m_Page + 1) * MaxEntriesPerPage < m_Log.Count)
                            from.SendGump(new SpeechLogGump(m_Player, m_Log, m_Page + 1));

                        break;
                    }
            }
        }
    }
}