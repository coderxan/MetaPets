using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using Server;
using Server.Accounting;
using Server.Engines.ConPVP;
using Server.Factions;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Engines.Reports
{
    public class Reports
    {
        public static bool Enabled = false;

        public static void Initialize()
        {
            if (!Enabled)
                return;

            m_StatsHistory = new SnapshotHistory();
            m_StatsHistory.Load();

            m_StaffHistory = new StaffHistory();
            m_StaffHistory.Load();

            DateTime now = DateTime.UtcNow;

            DateTime date = now.Date;
            TimeSpan timeOfDay = now.TimeOfDay;

            m_GenerateTime = date + TimeSpan.FromHours(Math.Ceiling(timeOfDay.TotalHours));

            Timer.DelayCall(TimeSpan.FromMinutes(0.5), TimeSpan.FromMinutes(0.5), new TimerCallback(CheckRegenerate));
        }

        private static DateTime m_GenerateTime;

        public static void CheckRegenerate()
        {
            if (DateTime.UtcNow < m_GenerateTime)
                return;

            Generate();
            m_GenerateTime += TimeSpan.FromHours(1.0);
        }

        private static SnapshotHistory m_StatsHistory;
        private static StaffHistory m_StaffHistory;

        public static StaffHistory StaffHistory { get { return m_StaffHistory; } }

        public static void Generate()
        {
            Snapshot ss = new Snapshot();

            ss.TimeStamp = DateTime.UtcNow;

            FillSnapshot(ss);

            m_StatsHistory.Snapshots.Add(ss);
            m_StaffHistory.QueueStats.Add(new QueueStatus(Engines.Help.PageQueue.List.Count));

            ThreadPool.QueueUserWorkItem(new WaitCallback(UpdateOutput), ss);
        }

        private static void UpdateOutput(object state)
        {
            m_StatsHistory.Save();
            m_StaffHistory.Save();

            HtmlRenderer renderer = new HtmlRenderer("stats", (Snapshot)state, m_StatsHistory);
            renderer.Render();
            renderer.Upload();

            renderer = new HtmlRenderer("staff", m_StaffHistory);
            renderer.Render();
            renderer.Upload();
        }

        public static void FillSnapshot(Snapshot ss)
        {
            ss.Children.Add(CompileGeneralStats());
            ss.Children.Add(CompilePCByDL());
            ss.Children.Add(CompileTop15());
            ss.Children.Add(CompileDislikedArenas());
            ss.Children.Add(CompileStatChart());

            PersistableObject[] obs = CompileSkillReports();

            for (int i = 0; i < obs.Length; ++i)
                ss.Children.Add(obs[i]);

            obs = CompileFactionReports();

            for (int i = 0; i < obs.Length; ++i)
                ss.Children.Add(obs[i]);
        }

        public static Report CompileGeneralStats()
        {
            Report report = new Report("General Stats", "200");

            report.Columns.Add("50%", "left");
            report.Columns.Add("50%", "left");

            int npcs = 0, players = 0;

            foreach (Mobile mob in World.Mobiles.Values)
            {
                if (mob.Player)
                    ++players;
                else
                    ++npcs;
            }

            report.Items.Add("NPCs", npcs, "N0");
            report.Items.Add("Players", players, "N0");
            report.Items.Add("Clients", NetState.Instances.Count, "N0");
            report.Items.Add("Accounts", Accounts.Count, "N0");
            report.Items.Add("Items", World.Items.Count, "N0");

            return report;
        }

        private static Chart CompilePCByDL()
        {
            BarGraph chart = new BarGraph("Player Count By Dueling Level", "graphs_pc_by_dl", 5, "Dueling Level", "Players", BarGraphRenderMode.Bars);

            int lastLevel = -1;
            ChartItem lastItem = null;

            Ladder ladder = Ladder.Instance;

            if (ladder != null)
            {
                ArrayList entries = ladder.ToArrayList();

                for (int i = entries.Count - 1; i >= 0; --i)
                {
                    LadderEntry entry = (LadderEntry)entries[i];
                    int level = Ladder.GetLevel(entry.Experience);

                    if (lastItem == null || level != lastLevel)
                    {
                        chart.Items.Add(lastItem = new ChartItem(level.ToString(), 1));
                        lastLevel = level;
                    }
                    else
                    {
                        lastItem.Value++;
                    }
                }
            }

            return chart;
        }

        private static Report CompileTop15()
        {
            Report report = new Report("Top 15 Duelists", "80%");

            report.Columns.Add("6%", "center", "Rank");
            report.Columns.Add("6%", "center", "Level");
            report.Columns.Add("6%", "center", "Guild");
            report.Columns.Add("70%", "left", "Name");
            report.Columns.Add("6%", "center", "Wins");
            report.Columns.Add("6%", "center", "Losses");

            Ladder ladder = Ladder.Instance;

            if (ladder != null)
            {
                ArrayList entries = ladder.ToArrayList();

                for (int i = 0; i < entries.Count && i < 15; ++i)
                {
                    LadderEntry entry = (LadderEntry)entries[i];
                    int level = Ladder.GetLevel(entry.Experience);
                    string guild = "";

                    if (entry.Mobile.Guild != null)
                        guild = entry.Mobile.Guild.Abbreviation;

                    ReportItem item = new ReportItem();

                    item.Values.Add(LadderGump.Rank(entry.Index + 1));
                    item.Values.Add(level.ToString(), "N0");
                    item.Values.Add(guild);
                    item.Values.Add(entry.Mobile.Name);
                    item.Values.Add(entry.Wins.ToString(), "N0");
                    item.Values.Add(entry.Losses.ToString(), "N0");

                    report.Items.Add(item);
                }
            }

            return report;
        }

        private static Chart CompileDislikedArenas()
        {
            PieChart chart = new PieChart("Most Disliked Arenas", "graphs_arenas_disliked", false);

            Preferences prefs = Preferences.Instance;

            if (prefs != null)
            {
                List<Arena> arenas = Arena.Arenas;

                for (int i = 0; i < arenas.Count; ++i)
                {
                    Arena arena = arenas[i];

                    string name = arena.Name;

                    if (name != null)
                        chart.Items.Add(name, 0);
                }

                ArrayList entries = prefs.Entries;

                for (int i = 0; i < entries.Count; ++i)
                {
                    PreferencesEntry entry = (PreferencesEntry)entries[i];
                    ArrayList list = entry.Disliked;

                    for (int j = 0; j < list.Count; ++j)
                    {
                        string disliked = (string)list[j];

                        for (int k = 0; k < chart.Items.Count; ++k)
                        {
                            ChartItem item = chart.Items[k];

                            if (item.Name == disliked)
                            {
                                ++item.Value;
                                break;
                            }
                        }
                    }
                }
            }

            return chart;
        }

        public static Chart CompileStatChart()
        {
            PieChart chart = new PieChart("Stat Distribution", "graphs_strdexint_distrib", true);

            ChartItem strItem = new ChartItem("Strength", 0);
            ChartItem dexItem = new ChartItem("Dexterity", 0);
            ChartItem intItem = new ChartItem("Intelligence", 0);

            foreach (Mobile mob in World.Mobiles.Values)
            {
                if (mob.RawStatTotal == mob.StatCap && mob is PlayerMobile)
                {
                    strItem.Value += mob.RawStr;
                    dexItem.Value += mob.RawDex;
                    intItem.Value += mob.RawInt;
                }
            }

            chart.Items.Add(strItem);
            chart.Items.Add(dexItem);
            chart.Items.Add(intItem);

            return chart;
        }

        public class SkillDistribution : IComparable
        {
            public SkillInfo m_Skill;
            public int m_NumberOfGMs;

            public SkillDistribution(SkillInfo skill)
            {
                m_Skill = skill;
            }

            public int CompareTo(object obj)
            {
                return (((SkillDistribution)obj).m_NumberOfGMs - m_NumberOfGMs);
            }
        }

        public static SkillDistribution[] GetSkillDistribution()
        {
            int skip = (Core.ML ? 0 : Core.SE ? 1 : Core.AOS ? 3 : 6);

            SkillDistribution[] distribs = new SkillDistribution[SkillInfo.Table.Length - skip];

            for (int i = 0; i < distribs.Length; ++i)
                distribs[i] = new SkillDistribution(SkillInfo.Table[i]);

            foreach (Mobile mob in World.Mobiles.Values)
            {
                if (mob.SkillsTotal >= 1500 && mob.SkillsTotal <= 7200 && mob is PlayerMobile)
                {
                    Skills skills = mob.Skills;

                    for (int i = 0; i < skills.Length - skip; ++i)
                    {
                        Skill skill = skills[i];

                        if (skill.BaseFixedPoint >= 1000)
                            distribs[i].m_NumberOfGMs++;
                    }
                }
            }

            return distribs;
        }

        public static PersistableObject[] CompileSkillReports()
        {
            SkillDistribution[] distribs = GetSkillDistribution();

            Array.Sort(distribs);

            return new PersistableObject[] { CompileSkillChart(distribs), CompileSkillReport(distribs) };
        }

        public static Report CompileSkillReport(SkillDistribution[] distribs)
        {
            Report report = new Report("Skill Report", "300");

            report.Columns.Add("70%", "left", "Name");
            report.Columns.Add("30%", "center", "GMs");

            for (int i = 0; i < distribs.Length; ++i)
                report.Items.Add(distribs[i].m_Skill.Name, distribs[i].m_NumberOfGMs, "N0");

            return report;
        }

        public static Chart CompileSkillChart(SkillDistribution[] distribs)
        {
            PieChart chart = new PieChart("GM Skill Distribution", "graphs_skill_distrib", true);

            for (int i = 0; i < 12; ++i)
                chart.Items.Add(distribs[i].m_Skill.Name, distribs[i].m_NumberOfGMs);

            int rem = 0;

            for (int i = 12; i < distribs.Length; ++i)
                rem += distribs[i].m_NumberOfGMs;

            chart.Items.Add("Other", rem);

            return chart;
        }

        public static PersistableObject[] CompileFactionReports()
        {
            return new PersistableObject[] { CompileFactionMembershipChart(), CompileFactionReport(), CompileFactionTownReport(), CompileSigilReport(), CompileFactionLeaderboard() };
        }

        public static Chart CompileFactionMembershipChart()
        {
            PieChart chart = new PieChart("Faction Membership", "graphs_faction_membership", true);

            List<Faction> factions = Faction.Factions;

            for (int i = 0; i < factions.Count; ++i)
                chart.Items.Add(factions[i].Definition.FriendlyName, factions[i].Members.Count);

            return chart;
        }

        public static Report CompileFactionLeaderboard()
        {
            Report report = new Report("Faction Leaderboard", "60%");

            report.Columns.Add("28%", "center", "Name");
            report.Columns.Add("28%", "center", "Faction");
            report.Columns.Add("28%", "center", "Office");
            report.Columns.Add("16%", "center", "Kill Points");

            ArrayList list = new ArrayList();

            List<Faction> factions = Faction.Factions;

            for (int i = 0; i < factions.Count; ++i)
            {
                Faction faction = factions[i];

                list.AddRange(faction.Members);
            }

            list.Sort();
            list.Reverse();

            for (int i = 0; i < list.Count && i < 15; ++i)
            {
                PlayerState pl = (PlayerState)list[i];

                string office;

                if (pl.Faction.Commander == pl.Mobile)
                    office = "Commanding Lord";
                else if (pl.Finance != null)
                    office = String.Format("{0} Finance Minister", pl.Finance.Definition.FriendlyName);
                else if (pl.Sheriff != null)
                    office = String.Format("{0} Sheriff", pl.Sheriff.Definition.FriendlyName);
                else
                    office = "";

                ReportItem item = new ReportItem();

                item.Values.Add(pl.Mobile.Name);
                item.Values.Add(pl.Faction.Definition.FriendlyName);
                item.Values.Add(office);
                item.Values.Add(pl.KillPoints.ToString(), "N0");

                report.Items.Add(item);
            }

            return report;
        }

        public static Report CompileFactionReport()
        {
            Report report = new Report("Faction Statistics", "80%");

            report.Columns.Add("20%", "center", "Name");
            report.Columns.Add("20%", "center", "Commander");
            report.Columns.Add("15%", "center", "Members");
            report.Columns.Add("15%", "center", "Merchants");
            report.Columns.Add("15%", "center", "Kill Points");
            report.Columns.Add("15%", "center", "Silver");

            List<Faction> factions = Faction.Factions;

            for (int i = 0; i < factions.Count; ++i)
            {
                Faction faction = factions[i];
                List<PlayerState> members = faction.Members;

                int totalKillPoints = 0;
                int totalMerchants = 0;

                for (int j = 0; j < members.Count; ++j)
                {
                    totalKillPoints += members[j].KillPoints;

                    if (members[j].MerchantTitle != MerchantTitle.None)
                        ++totalMerchants;
                }

                ReportItem item = new ReportItem();

                item.Values.Add(faction.Definition.FriendlyName);
                item.Values.Add(faction.Commander == null ? "" : faction.Commander.Name);
                item.Values.Add(faction.Members.Count.ToString(), "N0");
                item.Values.Add(totalMerchants.ToString(), "N0");
                item.Values.Add(totalKillPoints.ToString(), "N0");
                item.Values.Add(faction.Silver.ToString(), "N0");

                report.Items.Add(item);
            }

            return report;
        }

        public static Report CompileSigilReport()
        {
            Report report = new Report("Faction Town Sigils", "50%");

            report.Columns.Add("35%", "center", "Town");
            report.Columns.Add("35%", "center", "Controller");
            report.Columns.Add("30%", "center", "Capturable");

            List<Sigil> sigils = Sigil.Sigils;

            for (int i = 0; i < sigils.Count; ++i)
            {
                Sigil sigil = sigils[i];

                string controller = "Unknown";

                Mobile mob = sigil.RootParent as Mobile;

                if (mob != null)
                {
                    Faction faction = Faction.Find(mob);

                    if (faction != null)
                        controller = faction.Definition.FriendlyName;
                }
                else if (sigil.LastMonolith != null && sigil.LastMonolith.Faction != null)
                {
                    controller = sigil.LastMonolith.Faction.Definition.FriendlyName;
                }

                ReportItem item = new ReportItem();

                item.Values.Add(sigil.Town == null ? "" : sigil.Town.Definition.FriendlyName);
                item.Values.Add(controller);
                item.Values.Add(sigil.IsPurifying ? "No" : "Yes");

                report.Items.Add(item);
            }

            return report;
        }

        public static Report CompileFactionTownReport()
        {
            Report report = new Report("Faction Towns", "80%");

            report.Columns.Add("20%", "center", "Name");
            report.Columns.Add("20%", "center", "Owner");
            report.Columns.Add("17%", "center", "Sheriff");
            report.Columns.Add("17%", "center", "Finance Minister");
            report.Columns.Add("13%", "center", "Silver");
            report.Columns.Add("13%", "center", "Prices");

            List<Town> towns = Town.Towns;

            for (int i = 0; i < towns.Count; ++i)
            {
                Town town = towns[i];

                string prices = "Normal";

                if (town.Tax < 0)
                    prices = town.Tax.ToString() + "%";
                else if (town.Tax > 0)
                    prices = "+" + town.Tax.ToString() + "%";

                ReportItem item = new ReportItem();

                item.Values.Add(town.Definition.FriendlyName);
                item.Values.Add(town.Owner == null ? "Neutral" : town.Owner.Definition.FriendlyName);
                item.Values.Add(town.Sheriff == null ? "" : town.Sheriff.Name);
                item.Values.Add(town.Finance == null ? "" : town.Finance.Name);
                item.Values.Add(town.Silver.ToString(), "N0");
                item.Values.Add(prices);

                report.Items.Add(item);
            }

            return report;
        }
    }

    public class Report : PersistableObject
    {
        #region Type Identification
        public static readonly PersistableType ThisTypeID = new PersistableType("rp", new ConstructCallback(Construct));

        private static PersistableObject Construct()
        {
            return new Report();
        }

        public override PersistableType TypeID { get { return ThisTypeID; } }
        #endregion

        private string m_Name;
        private string m_Width;
        private ReportColumnCollection m_Columns;
        private ReportItemCollection m_Items;

        public string Name { get { return m_Name; } set { m_Name = value; } }
        public string Width { get { return m_Width; } set { m_Width = value; } }
        public ReportColumnCollection Columns { get { return m_Columns; } }
        public ReportItemCollection Items { get { return m_Items; } }

        private Report()
            : this(null, null)
        {
        }

        public Report(string name, string width)
        {
            m_Name = name;
            m_Width = width;
            m_Columns = new ReportColumnCollection();
            m_Items = new ReportItemCollection();
        }

        public override void SerializeAttributes(PersistanceWriter op)
        {
            op.SetString("n", m_Name);
            op.SetString("w", m_Width);
        }

        public override void DeserializeAttributes(PersistanceReader ip)
        {
            m_Name = Utility.Intern(ip.GetString("n"));
            m_Width = Utility.Intern(ip.GetString("w"));
        }

        public override void SerializeChildren(PersistanceWriter op)
        {
            for (int i = 0; i < m_Columns.Count; ++i)
                m_Columns[i].Serialize(op);

            for (int i = 0; i < m_Items.Count; ++i)
                m_Items[i].Serialize(op);
        }

        public override void DeserializeChildren(PersistanceReader ip)
        {
            while (ip.HasChild)
            {
                PersistableObject child = ip.GetChild();

                if (child is ReportColumn)
                    m_Columns.Add((ReportColumn)child);
                else if (child is ReportItem)
                    m_Items.Add((ReportItem)child);
            }
        }
    }

    /// <summary>
    /// Strongly typed collection of Server.Engines.Reports.ReportItem.
    /// </summary>
    public class ReportItemCollection : System.Collections.CollectionBase
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ReportItemCollection() :
            base()
        {
        }

        /// <summary>
        /// Gets or sets the value of the Server.Engines.Reports.ReportItem at a specific position in the ReportItemCollection.
        /// </summary>
        public Server.Engines.Reports.ReportItem this[int index]
        {
            get
            {
                return ((Server.Engines.Reports.ReportItem)(this.List[index]));
            }
            set
            {
                this.List[index] = value;
            }
        }

        public int Add(string name, object value)
        {
            return Add(name, value, null);
        }

        public int Add(string name, object value, string format)
        {
            ReportItem item = new ReportItem();

            item.Values.Add(name);
            item.Values.Add(value == null ? "" : value.ToString(), format);

            return Add(item);
        }

        /// <summary>
        /// Append a Server.Engines.Reports.ReportItem entry to this collection.
        /// </summary>
        /// <param name="value">Server.Engines.Reports.ReportItem instance.</param>
        /// <returns>The position into which the new element was inserted.</returns>
        public int Add(Server.Engines.Reports.ReportItem value)
        {
            return this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specified Server.Engines.Reports.ReportItem instance is in this collection.
        /// </summary>
        /// <param name="value">Server.Engines.Reports.ReportItem instance to search for.</param>
        /// <returns>True if the Server.Engines.Reports.ReportItem instance is in the collection; otherwise false.</returns>
        public bool Contains(Server.Engines.Reports.ReportItem value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Retrieve the index a specified Server.Engines.Reports.ReportItem instance is in this collection.
        /// </summary>
        /// <param name="value">Server.Engines.Reports.ReportItem instance to find.</param>
        /// <returns>The zero-based index of the specified Server.Engines.Reports.ReportItem instance. If the object is not found, the return value is -1.</returns>
        public int IndexOf(Server.Engines.Reports.ReportItem value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Removes a specified Server.Engines.Reports.ReportItem instance from this collection.
        /// </summary>
        /// <param name="value">The Server.Engines.Reports.ReportItem instance to remove.</param>
        public void Remove(Server.Engines.Reports.ReportItem value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Returns an enumerator that can iterate through the Server.Engines.Reports.ReportItem instance.
        /// </summary>
        /// <returns>An Server.Engines.Reports.ReportItem's enumerator.</returns>
        public new ReportItemCollectionEnumerator GetEnumerator()
        {
            return new ReportItemCollectionEnumerator(this);
        }

        /// <summary>
        /// Insert a Server.Engines.Reports.ReportItem instance into this collection at a specified index.
        /// </summary>
        /// <param name="index">Zero-based index.</param>
        /// <param name="value">The Server.Engines.Reports.ReportItem instance to insert.</param>
        public void Insert(int index, Server.Engines.Reports.ReportItem value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Strongly typed enumerator of Server.Engines.Reports.ReportItem.
        /// </summary>
        public class ReportItemCollectionEnumerator : System.Collections.IEnumerator
        {

            /// <summary>
            /// Current index
            /// </summary>
            private int _index;

            /// <summary>
            /// Current element pointed to.
            /// </summary>
            private Server.Engines.Reports.ReportItem _currentElement;

            /// <summary>
            /// Collection to enumerate.
            /// </summary>
            private ReportItemCollection _collection;

            /// <summary>
            /// Default constructor for enumerator.
            /// </summary>
            /// <param name="collection">Instance of the collection to enumerate.</param>
            internal ReportItemCollectionEnumerator(ReportItemCollection collection)
            {
                _index = -1;
                _collection = collection;
            }

            /// <summary>
            /// Gets the Server.Engines.Reports.ReportItem object in the enumerated ReportItemCollection currently indexed by this instance.
            /// </summary>
            public Server.Engines.Reports.ReportItem Current
            {
                get
                {
                    if (((_index == -1)
                                || (_index >= _collection.Count)))
                    {
                        throw new System.IndexOutOfRangeException("Enumerator not started.");
                    }
                    else
                    {
                        return _currentElement;
                    }
                }
            }

            /// <summary>
            /// Gets the current element in the collection.
            /// </summary>
            object IEnumerator.Current
            {
                get
                {
                    if (((_index == -1)
                                || (_index >= _collection.Count)))
                    {
                        throw new System.IndexOutOfRangeException("Enumerator not started.");
                    }
                    else
                    {
                        return _currentElement;
                    }
                }
            }

            /// <summary>
            /// Reset the cursor, so it points to the beginning of the enumerator.
            /// </summary>
            public void Reset()
            {
                _index = -1;
                _currentElement = null;
            }

            /// <summary>
            /// Advances the enumerator to the next queue of the enumeration, if one is currently available.
            /// </summary>
            /// <returns>true, if the enumerator was succesfully advanced to the next queue; false, if the enumerator has reached the end of the enumeration.</returns>
            public bool MoveNext()
            {
                if ((_index
                            < (_collection.Count - 1)))
                {
                    _index = (_index + 1);
                    _currentElement = this._collection[_index];
                    return true;
                }
                _index = _collection.Count;
                return false;
            }
        }
    }

    public class ReportItem : PersistableObject
    {
        #region Type Identification
        public static readonly PersistableType ThisTypeID = new PersistableType("ri", new ConstructCallback(Construct));

        private static PersistableObject Construct()
        {
            return new ReportItem();
        }

        public override PersistableType TypeID { get { return ThisTypeID; } }
        #endregion

        private ItemValueCollection m_Values;

        public ItemValueCollection Values { get { return m_Values; } }

        public ReportItem()
        {
            m_Values = new ItemValueCollection();
        }

        public override void SerializeChildren(PersistanceWriter op)
        {
            for (int i = 0; i < m_Values.Count; ++i)
                m_Values[i].Serialize(op);
        }

        public override void DeserializeChildren(PersistanceReader ip)
        {
            while (ip.HasChild)
                m_Values.Add(ip.GetChild() as ItemValue);
        }
    }

    /// <summary>
    /// Strongly typed collection of Server.Engines.Reports.ReportColumn.
    /// </summary>
    public class ReportColumnCollection : System.Collections.CollectionBase
    {

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ReportColumnCollection() :
            base()
        {
        }

        /// <summary>
        /// Gets or sets the value of the Server.Engines.Reports.ReportColumn at a specific position in the ReportColumnCollection.
        /// </summary>
        public Server.Engines.Reports.ReportColumn this[int index]
        {
            get
            {
                return ((Server.Engines.Reports.ReportColumn)(this.List[index]));
            }
            set
            {
                this.List[index] = value;
            }
        }

        public int Add(string width, string align)
        {
            return Add(new ReportColumn(width, align));
        }

        public int Add(string width, string align, string name)
        {
            return Add(new ReportColumn(width, align, name));
        }

        /// <summary>
        /// Append a Server.Engines.Reports.ReportColumn entry to this collection.
        /// </summary>
        /// <param name="value">Server.Engines.Reports.ReportColumn instance.</param>
        /// <returns>The position into which the new element was inserted.</returns>
        public int Add(Server.Engines.Reports.ReportColumn value)
        {
            return this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specified Server.Engines.Reports.ReportColumn instance is in this collection.
        /// </summary>
        /// <param name="value">Server.Engines.Reports.ReportColumn instance to search for.</param>
        /// <returns>True if the Server.Engines.Reports.ReportColumn instance is in the collection; otherwise false.</returns>
        public bool Contains(Server.Engines.Reports.ReportColumn value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Retrieve the index a specified Server.Engines.Reports.ReportColumn instance is in this collection.
        /// </summary>
        /// <param name="value">Server.Engines.Reports.ReportColumn instance to find.</param>
        /// <returns>The zero-based index of the specified Server.Engines.Reports.ReportColumn instance. If the object is not found, the return value is -1.</returns>
        public int IndexOf(Server.Engines.Reports.ReportColumn value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Removes a specified Server.Engines.Reports.ReportColumn instance from this collection.
        /// </summary>
        /// <param name="value">The Server.Engines.Reports.ReportColumn instance to remove.</param>
        public void Remove(Server.Engines.Reports.ReportColumn value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Returns an enumerator that can iterate through the Server.Engines.Reports.ReportColumn instance.
        /// </summary>
        /// <returns>An Server.Engines.Reports.ReportColumn's enumerator.</returns>
        public new ReportColumnCollectionEnumerator GetEnumerator()
        {
            return new ReportColumnCollectionEnumerator(this);
        }

        /// <summary>
        /// Insert a Server.Engines.Reports.ReportColumn instance into this collection at a specified index.
        /// </summary>
        /// <param name="index">Zero-based index.</param>
        /// <param name="value">The Server.Engines.Reports.ReportColumn instance to insert.</param>
        public void Insert(int index, Server.Engines.Reports.ReportColumn value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Strongly typed enumerator of Server.Engines.Reports.ReportColumn.
        /// </summary>
        public class ReportColumnCollectionEnumerator : System.Collections.IEnumerator
        {

            /// <summary>
            /// Current index
            /// </summary>
            private int _index;

            /// <summary>
            /// Current element pointed to.
            /// </summary>
            private Server.Engines.Reports.ReportColumn _currentElement;

            /// <summary>
            /// Collection to enumerate.
            /// </summary>
            private ReportColumnCollection _collection;

            /// <summary>
            /// Default constructor for enumerator.
            /// </summary>
            /// <param name="collection">Instance of the collection to enumerate.</param>
            internal ReportColumnCollectionEnumerator(ReportColumnCollection collection)
            {
                _index = -1;
                _collection = collection;
            }

            /// <summary>
            /// Gets the Server.Engines.Reports.ReportColumn object in the enumerated ReportColumnCollection currently indexed by this instance.
            /// </summary>
            public Server.Engines.Reports.ReportColumn Current
            {
                get
                {
                    if (((_index == -1)
                                || (_index >= _collection.Count)))
                    {
                        throw new System.IndexOutOfRangeException("Enumerator not started.");
                    }
                    else
                    {
                        return _currentElement;
                    }
                }
            }

            /// <summary>
            /// Gets the current element in the collection.
            /// </summary>
            object IEnumerator.Current
            {
                get
                {
                    if (((_index == -1)
                                || (_index >= _collection.Count)))
                    {
                        throw new System.IndexOutOfRangeException("Enumerator not started.");
                    }
                    else
                    {
                        return _currentElement;
                    }
                }
            }

            /// <summary>
            /// Reset the cursor, so it points to the beginning of the enumerator.
            /// </summary>
            public void Reset()
            {
                _index = -1;
                _currentElement = null;
            }

            /// <summary>
            /// Advances the enumerator to the next queue of the enumeration, if one is currently available.
            /// </summary>
            /// <returns>true, if the enumerator was succesfully advanced to the next queue; false, if the enumerator has reached the end of the enumeration.</returns>
            public bool MoveNext()
            {
                if ((_index
                            < (_collection.Count - 1)))
                {
                    _index = (_index + 1);
                    _currentElement = this._collection[_index];
                    return true;
                }
                _index = _collection.Count;
                return false;
            }
        }
    }

    public class ReportColumn : PersistableObject
    {
        #region Type Identification
        public static readonly PersistableType ThisTypeID = new PersistableType("rc", new ConstructCallback(Construct));

        private static PersistableObject Construct()
        {
            return new ReportColumn();
        }

        public override PersistableType TypeID { get { return ThisTypeID; } }
        #endregion

        private string m_Width;
        private string m_Align;
        private string m_Name;

        public string Width { get { return m_Width; } set { m_Width = value; } }
        public string Align { get { return m_Align; } set { m_Align = value; } }
        public string Name { get { return m_Name; } set { m_Name = value; } }

        private ReportColumn()
        {
        }

        public ReportColumn(string width, string align)
            : this(width, align, null)
        {
        }

        public ReportColumn(string width, string align, string name)
        {
            m_Width = width;
            m_Align = align;
            m_Name = name;
        }

        public override void SerializeAttributes(PersistanceWriter op)
        {
            op.SetString("w", m_Width);
            op.SetString("a", m_Align);
            op.SetString("n", m_Name);
        }

        public override void DeserializeAttributes(PersistanceReader ip)
        {
            m_Width = Utility.Intern(ip.GetString("w"));
            m_Align = Utility.Intern(ip.GetString("a"));
            m_Name = Utility.Intern(ip.GetString("n"));
        }
    }

    /// <summary>
    /// Strongly typed collection of Server.Engines.Reports.ItemValue.
    /// </summary>
    public class ItemValueCollection : System.Collections.CollectionBase
    {

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ItemValueCollection() :
            base()
        {
        }

        /// <summary>
        /// Gets or sets the value of the Server.Engines.Reports.ItemValue at a specific position in the ItemValueCollection.
        /// </summary>
        public Server.Engines.Reports.ItemValue this[int index]
        {
            get
            {
                return ((Server.Engines.Reports.ItemValue)(this.List[index]));
            }
            set
            {
                this.List[index] = value;
            }
        }

        public int Add(string value)
        {
            return Add(new ItemValue(value));
        }

        public int Add(string value, string format)
        {
            return Add(new ItemValue(value, format));
        }

        /// <summary>
        /// Append a Server.Engines.Reports.ItemValue entry to this collection.
        /// </summary>
        /// <param name="value">Server.Engines.Reports.ItemValue instance.</param>
        /// <returns>The position into which the new element was inserted.</returns>
        public int Add(Server.Engines.Reports.ItemValue value)
        {
            return this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specified Server.Engines.Reports.ItemValue instance is in this collection.
        /// </summary>
        /// <param name="value">Server.Engines.Reports.ItemValue instance to search for.</param>
        /// <returns>True if the Server.Engines.Reports.ItemValue instance is in the collection; otherwise false.</returns>
        public bool Contains(Server.Engines.Reports.ItemValue value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Retrieve the index a specified Server.Engines.Reports.ItemValue instance is in this collection.
        /// </summary>
        /// <param name="value">Server.Engines.Reports.ItemValue instance to find.</param>
        /// <returns>The zero-based index of the specified Server.Engines.Reports.ItemValue instance. If the object is not found, the return value is -1.</returns>
        public int IndexOf(Server.Engines.Reports.ItemValue value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Removes a specified Server.Engines.Reports.ItemValue instance from this collection.
        /// </summary>
        /// <param name="value">The Server.Engines.Reports.ItemValue instance to remove.</param>
        public void Remove(Server.Engines.Reports.ItemValue value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Returns an enumerator that can iterate through the Server.Engines.Reports.ItemValue instance.
        /// </summary>
        /// <returns>An Server.Engines.Reports.ItemValue's enumerator.</returns>
        public new ItemValueCollectionEnumerator GetEnumerator()
        {
            return new ItemValueCollectionEnumerator(this);
        }

        /// <summary>
        /// Insert a Server.Engines.Reports.ItemValue instance into this collection at a specified index.
        /// </summary>
        /// <param name="index">Zero-based index.</param>
        /// <param name="value">The Server.Engines.Reports.ItemValue instance to insert.</param>
        public void Insert(int index, Server.Engines.Reports.ItemValue value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Strongly typed enumerator of Server.Engines.Reports.ItemValue.
        /// </summary>
        public class ItemValueCollectionEnumerator : System.Collections.IEnumerator
        {

            /// <summary>
            /// Current index
            /// </summary>
            private int _index;

            /// <summary>
            /// Current element pointed to.
            /// </summary>
            private Server.Engines.Reports.ItemValue _currentElement;

            /// <summary>
            /// Collection to enumerate.
            /// </summary>
            private ItemValueCollection _collection;

            /// <summary>
            /// Default constructor for enumerator.
            /// </summary>
            /// <param name="collection">Instance of the collection to enumerate.</param>
            internal ItemValueCollectionEnumerator(ItemValueCollection collection)
            {
                _index = -1;
                _collection = collection;
            }

            /// <summary>
            /// Gets the Server.Engines.Reports.ItemValue object in the enumerated ItemValueCollection currently indexed by this instance.
            /// </summary>
            public Server.Engines.Reports.ItemValue Current
            {
                get
                {
                    if (((_index == -1)
                                || (_index >= _collection.Count)))
                    {
                        throw new System.IndexOutOfRangeException("Enumerator not started.");
                    }
                    else
                    {
                        return _currentElement;
                    }
                }
            }

            /// <summary>
            /// Gets the current element in the collection.
            /// </summary>
            object IEnumerator.Current
            {
                get
                {
                    if (((_index == -1)
                                || (_index >= _collection.Count)))
                    {
                        throw new System.IndexOutOfRangeException("Enumerator not started.");
                    }
                    else
                    {
                        return _currentElement;
                    }
                }
            }

            /// <summary>
            /// Reset the cursor, so it points to the beginning of the enumerator.
            /// </summary>
            public void Reset()
            {
                _index = -1;
                _currentElement = null;
            }

            /// <summary>
            /// Advances the enumerator to the next queue of the enumeration, if one is currently available.
            /// </summary>
            /// <returns>true, if the enumerator was succesfully advanced to the next queue; false, if the enumerator has reached the end of the enumeration.</returns>
            public bool MoveNext()
            {
                if ((_index
                            < (_collection.Count - 1)))
                {
                    _index = (_index + 1);
                    _currentElement = this._collection[_index];
                    return true;
                }
                _index = _collection.Count;
                return false;
            }
        }
    }

    public class ItemValue : PersistableObject
    {
        #region Type Identification
        public static readonly PersistableType ThisTypeID = new PersistableType("iv", new ConstructCallback(Construct));

        private static PersistableObject Construct()
        {
            return new ItemValue();
        }

        public override PersistableType TypeID { get { return ThisTypeID; } }
        #endregion

        private string m_Value;
        private string m_Format;

        public string Value { get { return m_Value; } set { m_Value = value; } }
        public string Format { get { return m_Format; } set { m_Format = value; } }

        private ItemValue()
        {
        }

        public ItemValue(string value)
            : this(value, null)
        {
        }

        public ItemValue(string value, string format)
        {
            m_Value = value;
            m_Format = format;
        }

        public override void SerializeAttributes(PersistanceWriter op)
        {
            op.SetString("v", m_Value);
            op.SetString("f", m_Format);
        }

        public override void DeserializeAttributes(PersistanceReader ip)
        {
            m_Value = ip.GetString("v");
            m_Format = Utility.Intern(ip.GetString("f"));

            if (m_Format == null)
                Utility.Intern(ref m_Value);
        }
    }
}