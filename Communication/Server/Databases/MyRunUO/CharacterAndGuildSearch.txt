using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

using Server;
using Server.Accounting;
using Server.Commands;
using Server.Guilds;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Network;

namespace Server.Engines.MyRunUO
{
    public class Config
    {
        // Is MyRunUO enabled?
        public static bool Enabled = false;

        // Details required for database connection string
        public const string DatabaseDriver = "{MySQL ODBC 5.2w Driver}";
        public const string DatabaseServer = "localhost";
        public const string DatabaseName = "MyRunUO";
        public const string DatabaseUserID = "username";
        public const string DatabasePassword = "password";

        // Should the database use transactions? This is recommended
        public static bool UseTransactions = true;

        // Use optimized table loading techniques? (LOAD DATA INFILE)
        public static bool LoadDataInFile = true;

        // This must be enabled if the database server is on a remote machine.
        public static bool DatabaseNonLocal = (DatabaseServer != "localhost");

        // Text encoding used
        public static Encoding EncodingIO = Encoding.ASCII;

        // Database communication is done in a separate thread. This value is the 'priority' of that thread, or, how much CPU it will try to use
        public static ThreadPriority DatabaseThreadPriority = ThreadPriority.BelowNormal;

        // Any character with an AccessLevel equal to or higher than this will not be displayed
        public static AccessLevel HiddenAccessLevel = AccessLevel.Counselor;

        // Export character database every 30 minutes
        public static TimeSpan CharacterUpdateInterval = TimeSpan.FromMinutes(30.0);

        // Export online list database every 5 minutes
        public static TimeSpan StatusUpdateInterval = TimeSpan.FromMinutes(5.0);

        public static string CompileConnectionString()
        {
            string connectionString = String.Format("DRIVER={0};SERVER={1};DATABASE={2};UID={3};PASSWORD={4};",
                DatabaseDriver, DatabaseServer, DatabaseName, DatabaseUserID, DatabasePassword);

            return connectionString;
        }
    }

    public class LayerComparer : IComparer
    {
        private static Layer PlateArms = (Layer)255;
        private static Layer ChainTunic = (Layer)254;
        private static Layer LeatherShorts = (Layer)253;

        private static Layer[] m_DesiredLayerOrder = new Layer[]
		{
			Layer.Cloak,
			Layer.Bracelet,
			Layer.Ring,
			Layer.Shirt,
			Layer.Pants,
			Layer.InnerLegs,
			Layer.Shoes,
			LeatherShorts,
			Layer.Arms,
			Layer.InnerTorso,
			LeatherShorts,
			PlateArms,
			Layer.MiddleTorso,
			Layer.OuterLegs,
			Layer.Neck,
			Layer.Waist,
			Layer.Gloves,
			Layer.OuterTorso,
			Layer.OneHanded,
			Layer.TwoHanded,
			Layer.FacialHair,
			Layer.Hair,
			Layer.Helm,
			Layer.Talisman
		};

        private static int[] m_TranslationTable;

        public static int[] TranslationTable
        {
            get { return m_TranslationTable; }
        }

        static LayerComparer()
        {
            m_TranslationTable = new int[256];

            for (int i = 0; i < m_DesiredLayerOrder.Length; ++i)
                m_TranslationTable[(int)m_DesiredLayerOrder[i]] = m_DesiredLayerOrder.Length - i;
        }

        public static bool IsValid(Item item)
        {
            return (m_TranslationTable[(int)item.Layer] > 0);
        }

        public static readonly IComparer Instance = new LayerComparer();

        public LayerComparer()
        {
        }

        public Layer Fix(int itemID, Layer oldLayer)
        {
            if (itemID == 0x1410 || itemID == 0x1417) // platemail arms
                return PlateArms;

            if (itemID == 0x13BF || itemID == 0x13C4) // chainmail tunic
                return ChainTunic;

            if (itemID == 0x1C08 || itemID == 0x1C09) // leather skirt
                return LeatherShorts;

            if (itemID == 0x1C00 || itemID == 0x1C01) // leather shorts
                return LeatherShorts;

            return oldLayer;
        }

        public int Compare(object x, object y)
        {
            Item a = (Item)x;
            Item b = (Item)y;

            Layer aLayer = a.Layer;
            Layer bLayer = b.Layer;

            aLayer = Fix(a.ItemID, aLayer);
            bLayer = Fix(b.ItemID, bLayer);

            return m_TranslationTable[(int)bLayer] - m_TranslationTable[(int)aLayer];
        }
    }

    public class MyRunUO : Timer
    {
        private static double CpuInterval = 0.1; // Processor runs every 0.1 seconds
        private static double CpuPercent = 0.25; // Processor runs for 25% of Interval, or ~25ms. This should take around 25% cpu

        public static void Initialize()
        {
            if (Config.Enabled)
            {
                Timer.DelayCall(TimeSpan.FromSeconds(10.0), Config.CharacterUpdateInterval, new TimerCallback(Begin));

                CommandSystem.Register("InitMyRunUO", AccessLevel.Administrator, new CommandEventHandler(InitMyRunUO_OnCommand));
                CommandSystem.Register("UpdateMyRunUO", AccessLevel.Administrator, new CommandEventHandler(UpdateMyRunUO_OnCommand));

                CommandSystem.Register("PublicChar", AccessLevel.Player, new CommandEventHandler(PublicChar_OnCommand));
                CommandSystem.Register("PrivateChar", AccessLevel.Player, new CommandEventHandler(PrivateChar_OnCommand));
            }
        }

        [Usage("PublicChar")]
        [Description("Enables showing extended character stats and skills in MyRunUO.")]
        public static void PublicChar_OnCommand(CommandEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;

            if (pm != null)
            {
                if (pm.PublicMyRunUO)
                {
                    pm.SendMessage("You have already chosen to show your skills and stats.");
                }
                else
                {
                    pm.PublicMyRunUO = true;
                    pm.SendMessage("All of your skills and stats will now be shown publicly in MyRunUO.");
                }
            }
        }

        [Usage("PrivateChar")]
        [Description("Disables showing extended character stats and skills in MyRunUO.")]
        public static void PrivateChar_OnCommand(CommandEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;

            if (pm != null)
            {
                if (!pm.PublicMyRunUO)
                {
                    pm.SendMessage("You have already chosen to not show your skills and stats.");
                }
                else
                {
                    pm.PublicMyRunUO = false;
                    pm.SendMessage("Only a general level of your top three skills will be shown in MyRunUO.");
                }
            }
        }

        [Usage("InitMyRunUO")]
        [Description("Initially creates the database schema.")]
        public static void InitMyRunUO_OnCommand(CommandEventArgs e)
        {
            if (m_Command != null && m_Command.HasCompleted)
            {
                TableCreation();
                e.Mobile.SendMessage("MyRunUO table creation process has been started.");
            }
            else
            {
                e.Mobile.SendMessage("MyRunUO table creation is already running.");
            }
        }

        public static void TableCreation()
        {
            if (m_Command != null && !m_Command.HasCompleted)
                return;
            DateTime start = DateTime.Now;
            Console.WriteLine("MyRunUO: Creating tables");
            try
            {
                m_Command = new DatabaseCommandQueue("MyRunUO: Tables created in {0:F1} seconds", "MyRunUO Status Database Thread");
                m_Command.Enqueue("CREATE TABLE IF NOT EXISTS `myrunuo_characters` ( `char_id` int(10) unsigned NOT NULL, `char_name` varchar(255) NOT NULL, `char_str` smallint(3) unsigned NOT NULL, `char_dex` smallint(3) unsigned NOT NULL, `char_int` smallint(3) unsigned NOT NULL, `char_female` tinyint(1) NOT NULL, `char_counts` smallint(6) NOT NULL, `char_guild` varchar(255) DEFAULT NULL, `char_guildtitle` varchar(255) DEFAULT NULL, `char_nototitle` varchar(255) DEFAULT NULL, `char_bodyhue` smallint(5) unsigned DEFAULT NULL, `char_public` tinyint(1) NOT NULL, PRIMARY KEY (`char_id`))");
                m_Command.Enqueue("CREATE TABLE IF NOT EXISTS `myrunuo_characters_layers` ( `char_id` int(10) unsigned NOT NULL, `layer_id` tinyint(3) unsigned NOT NULL, `item_id` smallint(5) unsigned NOT NULL, `item_hue` smallint(5) unsigned NOT NULL, PRIMARY KEY (`char_id`,`layer_id`))");
                m_Command.Enqueue("CREATE TABLE IF NOT EXISTS `myrunuo_characters_skills` ( `char_id` int(10) unsigned NOT NULL, `skill_id` tinyint(3) NOT NULL, `skill_value` smallint(5) unsigned NOT NULL, PRIMARY KEY (`char_id`,`skill_id`),  KEY `skill_id` (`skill_id`))");
                m_Command.Enqueue("CREATE TABLE IF NOT EXISTS `myrunuo_guilds` ( `guild_id` smallint(5) unsigned NOT NULL, `guild_name` varchar(255) NOT NULL, `guild_abbreviation` varchar(8) DEFAULT NULL, `guild_website` varchar(255) DEFAULT NULL, `guild_charter` varchar(255) DEFAULT NULL, `guild_type` varchar(8) NOT NULL, `guild_wars` smallint(5) unsigned NOT NULL, `guild_members` smallint(5) unsigned NOT NULL, `guild_master` int(10) unsigned NOT NULL, PRIMARY KEY (`guild_id`))");
                m_Command.Enqueue("CREATE TABLE IF NOT EXISTS `myrunuo_guilds_wars` ( `guild_1` smallint(5) unsigned NOT NULL DEFAULT '0', `guild_2` smallint(5) unsigned NOT NULL DEFAULT '0', PRIMARY KEY (`guild_1`,`guild_2`), KEY `guild1` (`guild_1`), KEY `guild2` (`guild_2`))");
                m_Command.Enqueue("CREATE TABLE IF NOT EXISTS `myrunuo_status` ( `char_id` int(10) NOT NULL, PRIMARY KEY (`char_id`))");
            }
            catch (Exception e)
            {
                Console.WriteLine("MyRunUO: Error creating tables.");
                Console.WriteLine(e);
            }
            if (m_Command != null)
                m_Command.Enqueue(null);
        }

        [Usage("UpdateMyRunUO")]
        [Description("Starts the process of updating the MyRunUO character and guild database.")]
        public static void UpdateMyRunUO_OnCommand(CommandEventArgs e)
        {
            if (m_Command != null && m_Command.HasCompleted)
                m_Command = null;

            if (m_Timer == null && m_Command == null)
            {
                Begin();
                e.Mobile.SendMessage("MyRunUO update process has been started.");
            }
            else
            {
                e.Mobile.SendMessage("MyRunUO database is already being updated.");
            }
        }

        public static void Begin()
        {
            if (m_Command != null && m_Command.HasCompleted)
                m_Command = null;

            if (m_Timer != null || m_Command != null)
                return;

            m_Timer = new MyRunUO();
            m_Timer.Start();
        }

        private static Timer m_Timer;

        private Stage m_Stage;
        private ArrayList m_List;
        private List<IAccount> m_Collecting;
        private int m_Index;

        private static DatabaseCommandQueue m_Command;

        private string m_SkillsPath;
        private string m_LayersPath;
        private string m_MobilesPath;

        private StreamWriter m_OpSkills;
        private StreamWriter m_OpLayers;
        private StreamWriter m_OpMobiles;

        private DateTime m_StartTime;

        public MyRunUO()
            : base(TimeSpan.FromSeconds(CpuInterval), TimeSpan.FromSeconds(CpuInterval))
        {
            m_List = new ArrayList();
            m_Collecting = new List<IAccount>();

            m_StartTime = DateTime.UtcNow;
            Console.WriteLine("MyRunUO: Updating character database");
        }

        protected override void OnTick()
        {
            bool shouldExit = false;

            try
            {
                shouldExit = Process(DateTime.UtcNow + TimeSpan.FromSeconds(CpuInterval * CpuPercent));

                if (shouldExit)
                    Console.WriteLine("MyRunUO: Database statements compiled in {0:F2} seconds", (DateTime.UtcNow - m_StartTime).TotalSeconds);
            }
            catch (Exception e)
            {
                Console.WriteLine("MyRunUO: {0}: Exception cought while processing", m_Stage);
                Console.WriteLine(e);
                shouldExit = true;
            }

            if (shouldExit)
            {
                m_Command.Enqueue(null);

                Stop();
                m_Timer = null;
            }
        }

        private enum Stage
        {
            CollectingMobiles,
            DumpingMobiles,
            CollectingGuilds,
            DumpingGuilds,
            Complete
        }

        public bool Process(DateTime endTime)
        {
            switch (m_Stage)
            {
                case Stage.CollectingMobiles: CollectMobiles(endTime); break;
                case Stage.DumpingMobiles: DumpMobiles(endTime); break;
                case Stage.CollectingGuilds: CollectGuilds(endTime); break;
                case Stage.DumpingGuilds: DumpGuilds(endTime); break;
            }

            return (m_Stage == Stage.Complete);
        }

        private static ArrayList m_MobilesToUpdate = new ArrayList();

        public static void QueueMobileUpdate(Mobile m)
        {
            if (!Config.Enabled || Config.LoadDataInFile)
                return;

            m_MobilesToUpdate.Add(m);
        }

        public void CollectMobiles(DateTime endTime)
        {
            if (Config.LoadDataInFile)
            {
                if (m_Index == 0)
                    m_Collecting.AddRange(Accounts.GetAccounts());

                for (int i = m_Index; i < m_Collecting.Count; ++i)
                {
                    IAccount acct = m_Collecting[i];

                    for (int j = 0; j < acct.Length; ++j)
                    {
                        Mobile mob = acct[j];

                        if (mob != null && mob.AccessLevel < Config.HiddenAccessLevel)
                            m_List.Add(mob);
                    }

                    ++m_Index;

                    if (DateTime.UtcNow >= endTime)
                        break;
                }

                if (m_Index == m_Collecting.Count)
                {
                    m_Collecting = new List<IAccount>();
                    m_Stage = Stage.DumpingMobiles;
                    m_Index = 0;
                }
            }
            else
            {
                m_List = m_MobilesToUpdate;
                m_MobilesToUpdate = new ArrayList();
                m_Stage = Stage.DumpingMobiles;
                m_Index = 0;
            }
        }

        public void CheckConnection()
        {
            if (m_Command == null)
            {
                m_Command = new DatabaseCommandQueue("MyRunUO: Characeter database updated in {0:F1} seconds", "MyRunUO Character Database Thread");

                if (Config.LoadDataInFile)
                {
                    m_OpSkills = GetUniqueWriter("skills", out m_SkillsPath);
                    m_OpLayers = GetUniqueWriter("layers", out m_LayersPath);
                    m_OpMobiles = GetUniqueWriter("mobiles", out m_MobilesPath);

                    m_Command.Enqueue("TRUNCATE TABLE myrunuo_characters");
                    m_Command.Enqueue("TRUNCATE TABLE myrunuo_characters_layers");
                    m_Command.Enqueue("TRUNCATE TABLE myrunuo_characters_skills");
                }

                m_Command.Enqueue("TRUNCATE TABLE myrunuo_guilds");
                m_Command.Enqueue("TRUNCATE TABLE myrunuo_guilds_wars");
            }
        }

        public void ExecuteNonQuery(string text)
        {
            m_Command.Enqueue(text);
        }

        public void ExecuteNonQuery(string format, params string[] args)
        {
            ExecuteNonQuery(String.Format(format, args));
        }

        public void ExecuteNonQueryIfNull(string select, string insert)
        {
            m_Command.Enqueue(new string[] { select, insert });
        }

        private void AppendCharEntity(string input, int charIndex, ref StringBuilder sb, char c)
        {
            if (sb == null)
            {
                if (charIndex > 0)
                    sb = new StringBuilder(input, 0, charIndex, input.Length + 20);
                else
                    sb = new StringBuilder(input.Length + 20);
            }

            sb.Append("&#");
            sb.Append((int)c);
            sb.Append(";");
        }

        private void AppendEntityRef(string input, int charIndex, ref StringBuilder sb, string ent)
        {
            if (sb == null)
            {
                if (charIndex > 0)
                    sb = new StringBuilder(input, 0, charIndex, input.Length + 20);
                else
                    sb = new StringBuilder(input.Length + 20);
            }

            sb.Append(ent);
        }

        private string SafeString(string input)
        {
            if (input == null)
                return "";

            StringBuilder sb = null;

            for (int i = 0; i < input.Length; ++i)
            {
                char c = input[i];

                if (c < 0x20 || c >= 0x7F)
                {
                    AppendCharEntity(input, i, ref sb, c);
                }
                else
                {
                    switch (c)
                    {
                        case '&': AppendEntityRef(input, i, ref sb, "&amp;"); break;
                        case '>': AppendEntityRef(input, i, ref sb, "&gt;"); break;
                        case '<': AppendEntityRef(input, i, ref sb, "&lt;"); break;
                        case '"': AppendEntityRef(input, i, ref sb, "&quot;"); break;
                        case '\'':
                        case ':':
                        case '/':
                        case '\\': AppendCharEntity(input, i, ref sb, c); break;
                        default:
                            {
                                if (sb != null)
                                    sb.Append(c);

                                break;
                            }
                    }
                }
            }

            if (sb != null)
                return sb.ToString();

            return input;
        }

        public const char LineStart = '\"';
        public const string EntrySep = "\",\"";
        public const string LineEnd = "\"\n";

        public void InsertMobile(Mobile mob)
        {
            string guildTitle = mob.GuildTitle;

            if (guildTitle == null || (guildTitle = guildTitle.Trim()).Length == 0)
                guildTitle = "NULL";
            else
                guildTitle = SafeString(guildTitle);

            string notoTitle = SafeString(Titles.ComputeTitle(null, mob));
            string female = (mob.Female ? "1" : "0");

            bool pubBool = (mob is PlayerMobile) && (((PlayerMobile)mob).PublicMyRunUO);

            string pubString = (pubBool ? "1" : "0");

            string guildId = (mob.Guild == null ? "NULL" : mob.Guild.Id.ToString());

            if (Config.LoadDataInFile)
            {
                m_OpMobiles.Write(LineStart);
                m_OpMobiles.Write(mob.Serial.Value);
                m_OpMobiles.Write(EntrySep);
                m_OpMobiles.Write(SafeString(mob.Name));
                m_OpMobiles.Write(EntrySep);
                m_OpMobiles.Write(mob.RawStr);
                m_OpMobiles.Write(EntrySep);
                m_OpMobiles.Write(mob.RawDex);
                m_OpMobiles.Write(EntrySep);
                m_OpMobiles.Write(mob.RawInt);
                m_OpMobiles.Write(EntrySep);
                m_OpMobiles.Write(female);
                m_OpMobiles.Write(EntrySep);
                m_OpMobiles.Write(mob.Kills);
                m_OpMobiles.Write(EntrySep);
                m_OpMobiles.Write(guildId);
                m_OpMobiles.Write(EntrySep);
                m_OpMobiles.Write(guildTitle);
                m_OpMobiles.Write(EntrySep);
                m_OpMobiles.Write(notoTitle);
                m_OpMobiles.Write(EntrySep);
                m_OpMobiles.Write(mob.Hue);
                m_OpMobiles.Write(EntrySep);
                m_OpMobiles.Write(pubString);
                m_OpMobiles.Write(LineEnd);
            }
            else
            {
                ExecuteNonQuery("INSERT INTO myrunuo_characters (char_id, char_name, char_str, char_dex, char_int, char_female, char_counts, char_guild, char_guildtitle, char_nototitle, char_bodyhue, char_public ) VALUES ({0}, '{1}', {2}, {3}, {4}, {5}, {6}, {7}, {8}, '{9}', {10}, {11})", mob.Serial.Value.ToString(), SafeString(mob.Name), mob.RawStr.ToString(), mob.RawDex.ToString(), mob.RawInt.ToString(), female, mob.Kills.ToString(), guildId, guildTitle, notoTitle, mob.Hue.ToString(), pubString);
            }
        }

        public void InsertSkills(Mobile mob)
        {
            Skills skills = mob.Skills;
            string serial = mob.Serial.Value.ToString();

            for (int i = 0; i < skills.Length; ++i)
            {
                Skill skill = skills[i];

                if (skill.BaseFixedPoint > 0)
                {
                    if (Config.LoadDataInFile)
                    {
                        m_OpSkills.Write(LineStart);
                        m_OpSkills.Write(serial);
                        m_OpSkills.Write(EntrySep);
                        m_OpSkills.Write(i);
                        m_OpSkills.Write(EntrySep);
                        m_OpSkills.Write(skill.BaseFixedPoint);
                        m_OpSkills.Write(LineEnd);
                    }
                    else
                    {
                        ExecuteNonQuery("INSERT INTO myrunuo_characters_skills (char_id, skill_id, skill_value) VALUES ({0}, {1}, {2})", serial, i.ToString(), skill.BaseFixedPoint.ToString());
                    }
                }
            }
        }

        private ArrayList m_Items = new ArrayList();

        private void InsertItem(string serial, int index, int itemID, int hue)
        {
            if (Config.LoadDataInFile)
            {
                m_OpLayers.Write(LineStart);
                m_OpLayers.Write(serial);
                m_OpLayers.Write(EntrySep);
                m_OpLayers.Write(index);
                m_OpLayers.Write(EntrySep);
                m_OpLayers.Write(itemID);
                m_OpLayers.Write(EntrySep);
                m_OpLayers.Write(hue);
                m_OpLayers.Write(LineEnd);
            }
            else
            {
                ExecuteNonQuery("INSERT INTO myrunuo_characters_layers (char_id, layer_id, item_id, item_hue) VALUES ({0}, {1}, {2}, {3})", serial, index.ToString(), itemID.ToString(), hue.ToString());
            }
        }

        public void InsertItems(Mobile mob)
        {
            ArrayList items = m_Items;
            items.AddRange(mob.Items);
            string serial = mob.Serial.Value.ToString();

            items.Sort(LayerComparer.Instance);

            int index = 0;

            bool hidePants = false;
            bool alive = mob.Alive;
            bool hideHair = !alive;

            for (int i = 0; i < items.Count; ++i)
            {
                Item item = (Item)items[i];

                if (!LayerComparer.IsValid(item))
                    break;

                if (!alive && item.ItemID != 8270)
                    continue;

                if (item.ItemID == 0x1411 || item.ItemID == 0x141A) // plate legs
                    hidePants = true;
                else if (hidePants && item.Layer == Layer.Pants)
                    continue;

                if (!hideHair && item.Layer == Layer.Helm)
                    hideHair = true;

                InsertItem(serial, index++, item.ItemID, item.Hue);
            }

            if (mob.FacialHairItemID != 0 && alive)
                InsertItem(serial, index++, mob.FacialHairItemID, mob.FacialHairHue);

            if (mob.HairItemID != 0 && !hideHair)
                InsertItem(serial, index++, mob.HairItemID, mob.HairHue);

            items.Clear();
        }

        public void DeleteMobile(Mobile mob)
        {
            ExecuteNonQuery("DELETE FROM myrunuo_characters WHERE char_id = {0}", mob.Serial.Value.ToString());
            ExecuteNonQuery("DELETE FROM myrunuo_characters_skills WHERE char_id = {0}", mob.Serial.Value.ToString());
            ExecuteNonQuery("DELETE FROM myrunuo_characters_layers WHERE char_id = {0}", mob.Serial.Value.ToString());
        }

        public StreamWriter GetUniqueWriter(string type, out string filePath)
        {
            filePath = Path.Combine(Core.BaseDirectory, String.Format("myrunuodb_{0}.txt", type)).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            try
            {
                return new StreamWriter(filePath);
            }
            catch
            {
                for (int i = 0; i < 100; ++i)
                {
                    try
                    {
                        filePath = Path.Combine(Core.BaseDirectory, String.Format("myrunuodb_{0}_{1}.txt", type, i)).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                        return new StreamWriter(filePath);
                    }
                    catch
                    {
                    }
                }
            }

            return null;
        }

        public void DumpMobiles(DateTime endTime)
        {
            CheckConnection();

            for (int i = m_Index; i < m_List.Count; ++i)
            {
                Mobile mob = (Mobile)m_List[i];

                if (mob is PlayerMobile)
                    ((PlayerMobile)mob).ChangedMyRunUO = false;

                if (!mob.Deleted && mob.AccessLevel < Config.HiddenAccessLevel)
                {
                    if (!Config.LoadDataInFile)
                        DeleteMobile(mob);

                    InsertMobile(mob);
                    InsertSkills(mob);
                    InsertItems(mob);
                }
                else if (!Config.LoadDataInFile)
                {
                    DeleteMobile(mob);
                }

                ++m_Index;

                if (DateTime.UtcNow >= endTime)
                    break;
            }

            if (m_Index == m_List.Count)
            {
                m_List.Clear();
                m_Stage = Stage.CollectingGuilds;
                m_Index = 0;

                if (Config.LoadDataInFile)
                {
                    m_OpSkills.Close();
                    m_OpLayers.Close();
                    m_OpMobiles.Close();

                    ExecuteNonQuery("LOAD DATA {0}INFILE '{1}' INTO TABLE myrunuo_characters FIELDS TERMINATED BY ',' ENCLOSED BY '\"' LINES TERMINATED BY '\n'", Config.DatabaseNonLocal ? "LOCAL " : "", m_MobilesPath);
                    ExecuteNonQuery("LOAD DATA {0}INFILE '{1}' INTO TABLE myrunuo_characters_skills FIELDS TERMINATED BY ',' ENCLOSED BY '\"' LINES TERMINATED BY '\n'", Config.DatabaseNonLocal ? "LOCAL " : "", m_SkillsPath);
                    ExecuteNonQuery("LOAD DATA {0}INFILE '{1}' INTO TABLE myrunuo_characters_layers FIELDS TERMINATED BY ',' ENCLOSED BY '\"' LINES TERMINATED BY '\n'", Config.DatabaseNonLocal ? "LOCAL " : "", m_LayersPath);
                }
            }
        }

        public void CollectGuilds(DateTime endTime)
        {
            m_List.AddRange(Guild.List.Values);
            m_Stage = Stage.DumpingGuilds;
            m_Index = 0;
        }

        public void InsertGuild(Guild guild)
        {
            string guildType = "Standard";

            switch (guild.Type)
            {
                case GuildType.Chaos: guildType = "Chaos"; break;
                case GuildType.Order: guildType = "Order"; break;
            }

            ExecuteNonQuery("INSERT INTO myrunuo_guilds (guild_id, guild_name, guild_abbreviation, guild_website, guild_charter, guild_type, guild_wars, guild_members, guild_master) VALUES ({0}, '{1}', {2}, {3}, {4}, '{5}', {6}, {7}, {8})", guild.Id.ToString(), SafeString(guild.Name), guild.Abbreviation == "none" ? "NULL" : "'" + SafeString(guild.Abbreviation) + "'", guild.Website == null ? "NULL" : "'" + SafeString(guild.Website) + "'", guild.Charter == null ? "NULL" : "'" + SafeString(guild.Charter) + "'", guildType, guild.Enemies.Count.ToString(), guild.Members.Count.ToString(), guild.Leader.Serial.Value.ToString());
        }

        public void InsertWars(Guild guild)
        {
            List<Guild> wars = guild.Enemies;

            string ourId = guild.Id.ToString();

            for (int i = 0; i < wars.Count; ++i)
            {
                Guild them = wars[i];
                string theirId = them.Id.ToString();

                ExecuteNonQueryIfNull(
                    String.Format("SELECT guild_1 FROM myrunuo_guilds_wars WHERE (guild_1={0} AND guild_2={1}) OR (guild_1={1} AND guild_2={0})", ourId, theirId),
                    String.Format("INSERT INTO myrunuo_guilds_wars (guild_1, guild_2) VALUES ({0}, {1})", ourId, theirId));
            }
        }

        public void DumpGuilds(DateTime endTime)
        {
            CheckConnection();

            for (int i = m_Index; i < m_List.Count; ++i)
            {
                Guild guild = (Guild)m_List[i];

                if (!guild.Disbanded)
                {
                    InsertGuild(guild);
                    InsertWars(guild);
                }

                ++m_Index;

                if (DateTime.UtcNow >= endTime)
                    break;
            }

            if (m_Index == m_List.Count)
            {
                m_List.Clear();
                m_Stage = Stage.Complete;
                m_Index = 0;
            }
        }
    }

    public class DatabaseCommandQueue
    {
        private Queue m_Queue;
        private ManualResetEvent m_Sync;
        private Thread m_Thread;

        private bool m_HasCompleted;

        private string m_CompletionString;
        private string m_ConnectionString;

        public bool HasCompleted
        {
            get { return m_HasCompleted; }
        }

        public void Enqueue(object obj)
        {
            lock (m_Queue.SyncRoot)
            {
                m_Queue.Enqueue(obj);
                try { m_Sync.Set(); }
                catch { }
            }
        }

        public DatabaseCommandQueue(string completionString, string threadName)
            : this(Config.CompileConnectionString(), completionString, threadName)
        {
        }

        public DatabaseCommandQueue(string connectionString, string completionString, string threadName)
        {
            m_CompletionString = completionString;
            m_ConnectionString = connectionString;

            m_Queue = Queue.Synchronized(new Queue());

            m_Queue.Enqueue(null); // signal connect

            /*m_Queue.Enqueue( "DELETE FROM myrunuo_characters" );
            m_Queue.Enqueue( "DELETE FROM myrunuo_characters_layers" );
            m_Queue.Enqueue( "DELETE FROM myrunuo_characters_skills" );
            m_Queue.Enqueue( "DELETE FROM myrunuo_guilds" );
            m_Queue.Enqueue( "DELETE FROM myrunuo_guilds_wars" );*/

            m_Sync = new ManualResetEvent(true);

            m_Thread = new Thread(new ThreadStart(Thread_Start));
            m_Thread.Name = threadName;//"MyRunUO Database Command Queue";
            m_Thread.Priority = Config.DatabaseThreadPriority;
            m_Thread.Start();
        }

        private void Thread_Start()
        {
            bool connected = false;

            OdbcConnection connection = null;
            OdbcCommand command = null;
            OdbcTransaction transact = null;

            DateTime start = DateTime.UtcNow;

            bool shouldWriteException = true;

            while (true)
            {
                m_Sync.WaitOne();

                while (m_Queue.Count > 0)
                {
                    try
                    {
                        object obj = m_Queue.Dequeue();

                        if (obj == null)
                        {
                            if (connected)
                            {
                                if (transact != null)
                                {
                                    try { transact.Commit(); }
                                    catch (Exception commitException)
                                    {
                                        Console.WriteLine("MyRunUO: Exception caught when committing transaction");
                                        Console.WriteLine(commitException);

                                        try
                                        {
                                            transact.Rollback();
                                            Console.WriteLine("MyRunUO: Transaction has been rolled back");
                                        }
                                        catch (Exception rollbackException)
                                        {
                                            Console.WriteLine("MyRunUO: Exception caught when rolling back transaction");
                                            Console.WriteLine(rollbackException);
                                        }
                                    }
                                }

                                try { connection.Close(); }
                                catch { }

                                try { connection.Dispose(); }
                                catch { }

                                try { command.Dispose(); }
                                catch { }

                                try { m_Sync.Close(); }
                                catch { }

                                Console.WriteLine(m_CompletionString, (DateTime.UtcNow - start).TotalSeconds);
                                m_HasCompleted = true;

                                return;
                            }
                            else
                            {
                                try
                                {
                                    connected = true;
                                    connection = new OdbcConnection(m_ConnectionString);
                                    connection.Open();
                                    command = connection.CreateCommand();

                                    if (Config.UseTransactions)
                                    {
                                        transact = connection.BeginTransaction();
                                        command.Transaction = transact;
                                    }
                                }
                                catch (Exception e)
                                {
                                    try { if (transact != null) transact.Rollback(); }
                                    catch { }

                                    try { if (connection != null) connection.Close(); }
                                    catch { }

                                    try { if (connection != null) connection.Dispose(); }
                                    catch { }

                                    try { if (command != null) command.Dispose(); }
                                    catch { }

                                    try { m_Sync.Close(); }
                                    catch { }

                                    Console.WriteLine("MyRunUO: Unable to connect to the database");
                                    Console.WriteLine(e);
                                    m_HasCompleted = true;
                                    return;
                                }
                            }
                        }
                        else if (obj is string)
                        {
                            command.CommandText = (string)obj;
                            command.ExecuteNonQuery();
                        }
                        else
                        {
                            string[] parms = (string[])obj;

                            command.CommandText = parms[0];

                            if (command.ExecuteScalar() == null)
                            {
                                command.CommandText = parms[1];
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (shouldWriteException)
                        {
                            Console.WriteLine("MyRunUO: Exception caught in database thread");
                            Console.WriteLine(e);
                            shouldWriteException = false;
                        }
                    }
                }

                lock (m_Queue.SyncRoot)
                {
                    if (m_Queue.Count == 0)
                        m_Sync.Reset();
                }
            }
        }
    }

    public class MyRunUOStatus
    {
        public static void Initialize()
        {
            if (Config.Enabled)
            {
                Timer.DelayCall(TimeSpan.FromSeconds(20.0), Config.StatusUpdateInterval, new TimerCallback(Begin));

                CommandSystem.Register("UpdateWebStatus", AccessLevel.Administrator, new CommandEventHandler(UpdateWebStatus_OnCommand));
            }
        }

        [Usage("UpdateWebStatus")]
        [Description("Starts the process of updating the MyRunUO online status database.")]
        public static void UpdateWebStatus_OnCommand(CommandEventArgs e)
        {
            if (m_Command == null || m_Command.HasCompleted)
            {
                Begin();
                e.Mobile.SendMessage("Web status update process has been started.");
            }
            else
            {
                e.Mobile.SendMessage("Web status database is already being updated.");
            }
        }

        private static DatabaseCommandQueue m_Command;

        public static void Begin()
        {
            if (m_Command != null && !m_Command.HasCompleted)
                return;

            DateTime start = DateTime.UtcNow;
            Console.WriteLine("MyRunUO: Updating status database");

            try
            {
                m_Command = new DatabaseCommandQueue("MyRunUO: Status database updated in {0:F1} seconds", "MyRunUO Status Database Thread");

                m_Command.Enqueue("DELETE FROM myrunuo_status");

                List<NetState> online = NetState.Instances;

                for (int i = 0; i < online.Count; ++i)
                {
                    NetState ns = online[i];
                    Mobile mob = ns.Mobile;

                    if (mob != null)
                        m_Command.Enqueue(String.Format("INSERT INTO myrunuo_status (char_id) VALUES ({0})", mob.Serial.Value.ToString()));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("MyRunUO: Error updating status database");
                Console.WriteLine(e);
            }

            if (m_Command != null)
                m_Command.Enqueue(null);
        }
    }
}

namespace Server.Misc
{
    public class StatusPage : Timer
    {
        public static readonly bool Enabled = false;

        private static HttpListener _Listener;

        private static string _StatusPage = String.Empty;
        private static byte[] _StatusBuffer = new byte[0];

        private static readonly object _StatusLock = new object();

        public static void Initialize()
        {
            if (!Enabled)
            {
                return;
            }

            new StatusPage().Start();

            Listen();
        }

        private static void Listen()
        {
            if (!HttpListener.IsSupported)
            {
                return;
            }

            if (_Listener == null)
            {
                _Listener = new HttpListener();
                _Listener.Prefixes.Add("http://*:80/status/");
                _Listener.Start();
            }
            else if (!_Listener.IsListening)
            {
                _Listener.Start();
            }

            if (_Listener.IsListening)
            {
                _Listener.BeginGetContext(ListenerCallback, null);
            }
        }

        private static void ListenerCallback(IAsyncResult result)
        {
            try
            {
                var context = _Listener.EndGetContext(result);

                byte[] buffer;

                lock (_StatusLock)
                {
                    buffer = _StatusBuffer;
                }

                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.OutputStream.Close();
            }
            catch
            { }

            Listen();
        }

        private static string Encode(string input)
        {
            var sb = new StringBuilder(input);

            sb.Replace("&", "&amp;");
            sb.Replace("<", "&lt;");
            sb.Replace(">", "&gt;");
            sb.Replace("\"", "&quot;");
            sb.Replace("'", "&apos;");

            return sb.ToString();
        }

        public StatusPage()
            : base(TimeSpan.FromSeconds(5.0), TimeSpan.FromSeconds(60.0))
        {
            Priority = TimerPriority.FiveSeconds;
        }

        protected override void OnTick()
        {
            if (!Directory.Exists("web"))
            {
                Directory.CreateDirectory("web");
            }

            using (var op = new StreamWriter("web/status.html"))
            {
                op.WriteLine("<!DOCTYPE html>");
                op.WriteLine("<html>");
                op.WriteLine("   <head>");
                op.WriteLine("      <title>" + ServerList.ServerName + " Server Status</title>");
                op.WriteLine("   </head>");
                op.WriteLine("   <style type=\"text/css\">");
                op.WriteLine("   body { background: #999; }");
                op.WriteLine("   table { width: 100%; }");
                op.WriteLine("   tr.ruo-header td { background: #000; color: #FFF; }");
                op.WriteLine("   tr.odd td { background: #222; color: #DDD; }");
                op.WriteLine("   tr.even td { background: #DDD; color: #222; }");
                op.WriteLine("   </style>");
                op.WriteLine("   <body>");
                op.WriteLine("      <h1>RunUO Server Status</h1>");
                op.WriteLine("      <h3>Online clients</h3>");
                op.WriteLine("      <table cellpadding=\"0\" cellspacing=\"0\">");
                op.WriteLine("         <tr class=\"ruo-header\"><td>Name</td><td>Location</td><td>Kills</td><td>Karma/Fame</td></tr>");

                var index = 0;

                foreach (var m in NetState.Instances.Where(state => state.Mobile != null).Select(state => state.Mobile))
                {
                    ++index;

                    var g = m.Guild as Guild;

                    op.Write("         <tr class=\"ruo-result " + (index % 2 == 0 ? "even" : "odd") + "\"><td>");

                    if (g != null)
                    {
                        op.Write(Encode(m.Name));
                        op.Write(" [");

                        var title = m.GuildTitle;

                        title = title != null ? title.Trim() : String.Empty;

                        if (title.Length > 0)
                        {
                            op.Write(Encode(title));
                            op.Write(", ");
                        }

                        op.Write(Encode(g.Abbreviation));

                        op.Write(']');
                    }
                    else
                    {
                        op.Write(Encode(m.Name));
                    }

                    op.Write("</td><td>");
                    op.Write(m.X);
                    op.Write(", ");
                    op.Write(m.Y);
                    op.Write(", ");
                    op.Write(m.Z);
                    op.Write(" (");
                    op.Write(m.Map);
                    op.Write(")</td><td>");
                    op.Write(m.Kills);
                    op.Write("</td><td>");
                    op.Write(m.Karma);
                    op.Write(" / ");
                    op.Write(m.Fame);
                    op.WriteLine("</td></tr>");
                }

                op.WriteLine("         <tr>");
                op.WriteLine("      </table>");
                op.WriteLine("   </body>");
                op.WriteLine("</html>");
            }

            lock (_StatusLock)
            {
                _StatusPage = File.ReadAllText("web/status.html");
                _StatusBuffer = Encoding.UTF8.GetBytes(_StatusPage);
            }
        }
    }
}