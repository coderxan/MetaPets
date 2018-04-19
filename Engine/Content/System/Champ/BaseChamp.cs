using System;
using System.Collections;
using System.Collections.Generic;

using Server;
using Server.Engines.CannedEvil;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Regions;
using Server.Targeting;

namespace Server.Engines.CannedEvil
{
    #region Champ Spawn Platform And Altar

    public class ChampionPlatform : BaseAddon
    {
        private ChampionSpawn m_Spawn;

        public ChampionPlatform(ChampionSpawn spawn)
        {
            m_Spawn = spawn;

            for (int x = -2; x <= 2; ++x)
                for (int y = -2; y <= 2; ++y)
                    AddComponent(0x750, x, y, -5);

            for (int x = -1; x <= 1; ++x)
                for (int y = -1; y <= 1; ++y)
                    AddComponent(0x750, x, y, 0);

            for (int i = -1; i <= 1; ++i)
            {
                AddComponent(0x751, i, 2, 0);
                AddComponent(0x752, 2, i, 0);

                AddComponent(0x753, i, -2, 0);
                AddComponent(0x754, -2, i, 0);
            }

            AddComponent(0x759, -2, -2, 0);
            AddComponent(0x75A, 2, 2, 0);
            AddComponent(0x75B, -2, 2, 0);
            AddComponent(0x75C, 2, -2, 0);
        }

        public void AddComponent(int id, int x, int y, int z)
        {
            AddonComponent ac = new AddonComponent(id);

            ac.Hue = 0x497;

            AddComponent(ac, x, y, z);
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            if (m_Spawn != null)
                m_Spawn.Delete();
        }

        public ChampionPlatform(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write(m_Spawn);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_Spawn = reader.ReadItem() as ChampionSpawn;

                        if (m_Spawn == null)
                            Delete();

                        break;
                    }
            }
        }
    }

    public class ChampionAltar : PentagramAddon
    {
        private ChampionSpawn m_Spawn;

        public ChampionAltar(ChampionSpawn spawn)
        {
            m_Spawn = spawn;
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            if (m_Spawn != null)
                m_Spawn.Delete();
        }

        public ChampionAltar(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write(m_Spawn);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_Spawn = reader.ReadItem() as ChampionSpawn;

                        if (m_Spawn == null)
                            Delete();

                        break;
                    }
            }
        }
    }

    public class ChampionSkullPlatform : BaseAddon
    {
        private ChampionSkullBrazier m_Power, m_Enlightenment, m_Venom, m_Pain, m_Greed, m_Death;

        [Constructable]
        public ChampionSkullPlatform()
        {
            AddComponent(new AddonComponent(0x71A), -1, -1, -1);
            AddComponent(new AddonComponent(0x709), 0, -1, -1);
            AddComponent(new AddonComponent(0x709), 1, -1, -1);
            AddComponent(new AddonComponent(0x709), -1, 0, -1);
            AddComponent(new AddonComponent(0x709), 0, 0, -1);
            AddComponent(new AddonComponent(0x709), 1, 0, -1);
            AddComponent(new AddonComponent(0x709), -1, 1, -1);
            AddComponent(new AddonComponent(0x709), 0, 1, -1);
            AddComponent(new AddonComponent(0x71B), 1, 1, -1);

            AddComponent(new AddonComponent(0x50F), 0, -1, 4);
            AddComponent(m_Power = new ChampionSkullBrazier(this, ChampionSkullType.Power), 0, -1, 5);

            AddComponent(new AddonComponent(0x50F), 1, -1, 4);
            AddComponent(m_Enlightenment = new ChampionSkullBrazier(this, ChampionSkullType.Enlightenment), 1, -1, 5);

            AddComponent(new AddonComponent(0x50F), -1, 0, 4);
            AddComponent(m_Venom = new ChampionSkullBrazier(this, ChampionSkullType.Venom), -1, 0, 5);

            AddComponent(new AddonComponent(0x50F), 1, 0, 4);
            AddComponent(m_Pain = new ChampionSkullBrazier(this, ChampionSkullType.Pain), 1, 0, 5);

            AddComponent(new AddonComponent(0x50F), -1, 1, 4);
            AddComponent(m_Greed = new ChampionSkullBrazier(this, ChampionSkullType.Greed), -1, 1, 5);

            AddComponent(new AddonComponent(0x50F), 0, 1, 4);
            AddComponent(m_Death = new ChampionSkullBrazier(this, ChampionSkullType.Death), 0, 1, 5);

            AddonComponent comp = new LocalizedAddonComponent(0x20D2, 1049495);
            comp.Hue = 0x482;
            AddComponent(comp, 0, 0, 5);

            comp = new LocalizedAddonComponent(0x0BCF, 1049496);
            comp.Hue = 0x482;
            AddComponent(comp, 0, 2, -7);

            comp = new LocalizedAddonComponent(0x0BD0, 1049497);
            comp.Hue = 0x482;
            AddComponent(comp, 2, 0, -7);
        }

        public void Validate()
        {
            if (Validate(m_Power) && Validate(m_Enlightenment) && Validate(m_Venom) && Validate(m_Pain) && Validate(m_Greed) && Validate(m_Death))
            {
                Mobile harrower = Harrower.Spawn(new Point3D(X, Y, Z + 6), this.Map);

                if (harrower == null)
                    return;

                Clear(m_Power);
                Clear(m_Enlightenment);
                Clear(m_Venom);
                Clear(m_Pain);
                Clear(m_Greed);
                Clear(m_Death);
            }
        }

        public void Clear(ChampionSkullBrazier brazier)
        {
            if (brazier != null)
            {
                Effects.SendBoltEffect(brazier);

                if (brazier.Skull != null)
                    brazier.Skull.Delete();
            }
        }

        public bool Validate(ChampionSkullBrazier brazier)
        {
            return (brazier != null && brazier.Skull != null && !brazier.Skull.Deleted);
        }

        public ChampionSkullPlatform(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write(m_Power);
            writer.Write(m_Enlightenment);
            writer.Write(m_Venom);
            writer.Write(m_Pain);
            writer.Write(m_Greed);
            writer.Write(m_Death);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_Power = reader.ReadItem() as ChampionSkullBrazier;
                        m_Enlightenment = reader.ReadItem() as ChampionSkullBrazier;
                        m_Venom = reader.ReadItem() as ChampionSkullBrazier;
                        m_Pain = reader.ReadItem() as ChampionSkullBrazier;
                        m_Greed = reader.ReadItem() as ChampionSkullBrazier;
                        m_Death = reader.ReadItem() as ChampionSkullBrazier;

                        break;
                    }
            }
        }
    }

    #endregion

    public enum ChampionSkullType
    {
        Power,
        Enlightenment,
        Venom,
        Pain,
        Greed,
        Death
    }

    public class ChampionSkullBrazier : AddonComponent
    {
        private ChampionSkullPlatform m_Platform;
        private ChampionSkullType m_Type;
        private Item m_Skull;

        [CommandProperty(AccessLevel.GameMaster)]
        public ChampionSkullPlatform Platform { get { return m_Platform; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public ChampionSkullType Type { get { return m_Type; } set { m_Type = value; InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public Item Skull { get { return m_Skull; } set { m_Skull = value; if (m_Platform != null) m_Platform.Validate(); } }

        public override int LabelNumber { get { return 1049489 + (int)m_Type; } }

        public ChampionSkullBrazier(ChampionSkullPlatform platform, ChampionSkullType type)
            : base(0x19BB)
        {
            Hue = 0x455;
            Light = LightType.Circle300;

            m_Platform = platform;
            m_Type = type;
        }

        public ChampionSkullBrazier(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (m_Platform != null)
                m_Platform.Validate();

            BeginSacrifice(from);
        }

        public void BeginSacrifice(Mobile from)
        {
            if (Deleted)
                return;

            if (m_Skull != null && m_Skull.Deleted)
                Skull = null;

            if (from.Map != this.Map || !from.InRange(GetWorldLocation(), 3))
            {
                from.SendLocalizedMessage(500446); // That is too far away.
            }
            else if (!Harrower.CanSpawn)
            {
                from.SendMessage("The harrower has already been spawned.");
            }
            else if (m_Skull == null)
            {
                from.SendLocalizedMessage(1049485); // What would you like to sacrifice?
                from.Target = new SacrificeTarget(this);
            }
            else
            {
                SendLocalizedMessageTo(from, 1049487, ""); // I already have my champions awakening skull!
            }
        }

        public void EndSacrifice(Mobile from, ChampionSkull skull)
        {
            if (Deleted)
                return;

            if (m_Skull != null && m_Skull.Deleted)
                Skull = null;

            if (from.Map != this.Map || !from.InRange(GetWorldLocation(), 3))
            {
                from.SendLocalizedMessage(500446); // That is too far away.
            }
            else if (!Harrower.CanSpawn)
            {
                from.SendMessage("The harrower has already been spawned.");
            }
            else if (skull == null)
            {
                SendLocalizedMessageTo(from, 1049488, ""); // That is not my champions awakening skull!
            }
            else if (m_Skull != null)
            {
                SendLocalizedMessageTo(from, 1049487, ""); // I already have my champions awakening skull!
            }
            else if (!skull.IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1049486); // You can only sacrifice items that are in your backpack!
            }
            else
            {
                if (skull.Type == this.Type)
                {
                    skull.Movable = false;
                    skull.MoveToWorld(GetWorldTop(), this.Map);

                    this.Skull = skull;
                }
                else
                {
                    SendLocalizedMessageTo(from, 1049488, ""); // That is not my champions awakening skull!
                }
            }
        }

        private class SacrificeTarget : Target
        {
            private ChampionSkullBrazier m_Brazier;

            public SacrificeTarget(ChampionSkullBrazier brazier)
                : base(12, false, TargetFlags.None)
            {
                m_Brazier = brazier;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                m_Brazier.EndSacrifice(from, targeted as ChampionSkull);
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write((int)m_Type);
            writer.Write(m_Platform);
            writer.Write(m_Skull);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_Type = (ChampionSkullType)reader.ReadInt();
                        m_Platform = reader.ReadItem() as ChampionSkullPlatform;
                        m_Skull = reader.ReadItem();

                        if (m_Platform == null)
                            Delete();

                        break;
                    }
            }

            if (Hue == 0x497)
                Hue = 0x455;

            if (Light != LightType.Circle300)
                Light = LightType.Circle300;
        }
    }

    public enum ChampionSpawnType
    {
        Abyss,
        Arachnid,
        ColdBlood,
        ForestLord,
        VerminHorde,
        UnholyTerror,
        SleepingDragon,
        Glade,
        Pestilence
    }

    public class ChampionSpawnInfo
    {
        private string m_Name;
        private Type m_Champion;
        private Type[][] m_SpawnTypes;
        private string[] m_LevelNames;

        public string Name { get { return m_Name; } }
        public Type Champion { get { return m_Champion; } }
        public Type[][] SpawnTypes { get { return m_SpawnTypes; } }
        public string[] LevelNames { get { return m_LevelNames; } }

        public ChampionSpawnInfo(string name, Type champion, string[] levelNames, Type[][] spawnTypes)
        {
            m_Name = name;
            m_Champion = champion;
            m_LevelNames = levelNames;
            m_SpawnTypes = spawnTypes;
        }

        public static ChampionSpawnInfo[] Table { get { return m_Table; } }

        private static readonly ChampionSpawnInfo[] m_Table = new ChampionSpawnInfo[]
			{
				new ChampionSpawnInfo( "Abyss", typeof( Semidar ), new string[]{ "Foe", "Assassin", "Conqueror" }, new Type[][]	// Abyss
				{																											// Abyss
					new Type[]{ typeof( GreaterMongbat ), typeof( Imp ) },													// Level 1
					new Type[]{ typeof( Gargoyle ), typeof( Harpy ) },														// Level 2
					new Type[]{ typeof( FireGargoyle ), typeof( StoneGargoyle ) },											// Level 3
					new Type[]{ typeof( Daemon ), typeof( Succubus ) }														// Level 4
				} ),
				new ChampionSpawnInfo( "Arachnid", typeof( Mephitis ), new string[]{ "Bane", "Killer", "Vanquisher" }, new Type[][]	// Arachnid
				{																											// Arachnid
					new Type[]{ typeof( Scorpion ), typeof( GiantSpider ) },												// Level 1
					new Type[]{ typeof( TerathanDrone ), typeof( TerathanWarrior ) },										// Level 2
					new Type[]{ typeof( DreadSpider ), typeof( TerathanMatriarch ) },										// Level 3
					new Type[]{ typeof( PoisonElemental ), typeof( TerathanAvenger ) }										// Level 4
				} ),
				new ChampionSpawnInfo( "Cold Blood", typeof( Rikktor ), new string[]{ "Blight", "Slayer", "Destroyer" }, new Type[][]	// Cold Blood
				{																											// Cold Blood
					new Type[]{ typeof( Lizardman ), typeof( Snake ) },														// Level 1
					new Type[]{ typeof( LavaLizard ), typeof( OphidianWarrior ) },											// Level 2
					new Type[]{ typeof( Drake ), typeof( OphidianArchmage ) },												// Level 3
					new Type[]{ typeof( Dragon ), typeof( OphidianKnight ) }												// Level 4
				} ),
				new ChampionSpawnInfo( "Forest Lord", typeof( LordOaks ), new string[]{ "Enemy", "Curse", "Slaughterer" }, new Type[][]	// Forest Lord
				{																											// Forest Lord
					new Type[]{ typeof( Pixie ), typeof( ShadowWisp ) },													// Level 1
					new Type[]{ typeof( Kirin ), typeof( Wisp ) },															// Level 2
					new Type[]{ typeof( Centaur ), typeof( Unicorn ) },														// Level 3
					new Type[]{ typeof( EtherealWarrior ), typeof( SerpentineDragon ) }										// Level 4
				} ),
				new ChampionSpawnInfo( "Vermin Horde", typeof( Barracoon ), new string[]{ "Adversary", "Subjugator", "Eradicator" }, new Type[][]	// Vermin Horde
				{																											// Vermin Horde
					new Type[]{ typeof( GiantRat ), typeof( Slime ) },														// Level 1
					new Type[]{ typeof( DireWolf ), typeof( Ratman ) },														// Level 2
					new Type[]{ typeof( HellHound ), typeof( RatmanMage ) },												// Level 3
					new Type[]{ typeof( RatmanArcher ), typeof( SilverSerpent ) }											// Level 4
				} ),
				new ChampionSpawnInfo( "Unholy Terror", typeof( Neira ), new string[]{ "Scourge", "Punisher", "Nemesis" }, new Type[][]	// Unholy Terror
				{																											// Unholy Terror
					(Core.AOS ? 
					new Type[]{ typeof( Bogle ), typeof( Ghoul ), typeof( Shade ), typeof( Spectre ), typeof( Wraith ) }	// Level 1 (Pre-AoS)
					: new Type[]{ typeof( Ghoul ), typeof( Shade ), typeof( Spectre ), typeof( Wraith ) } ),				// Level 1

					new Type[]{ typeof( BoneMagi ), typeof( Mummy ), typeof( SkeletalMage ) },								// Level 2
					new Type[]{ typeof( BoneKnight ), typeof( Lich ), typeof( SkeletalKnight ) },							// Level 3
					new Type[]{ typeof( LichLord ), typeof( RottingCorpse ) }												// Level 4
				} ),
				new ChampionSpawnInfo( "Sleeping Dragon", typeof( Serado ), new string[]{ "Rival", "Challenger", "Antagonist" } , new Type[][]
				{																											// Unholy Terror
					new Type[]{ typeof( DeathwatchBeetleHatchling ), typeof( Lizardman ) },
					new Type[]{ typeof( DeathwatchBeetle ), typeof( Kappa ) },
					new Type[]{ typeof( LesserHiryu ), typeof( RevenantLion ) },
					new Type[]{ typeof( Hiryu ), typeof( Oni ) }
				} ),
				new ChampionSpawnInfo( "Glade", typeof( Twaulo ), new string[]{ "Banisher", "Enforcer", "Eradicator" } , new Type[][]
				{																											// Glade
					new Type[]{ typeof( Pixie ), typeof( ShadowWisp ) },
					new Type[]{ typeof( Centaur ), typeof( MLDryad ) },
					new Type[]{ typeof( Satyr ), typeof( CuSidhe ) },
					new Type[]{ typeof( FerelTreefellow ), typeof( EnragedGrizzlyBear ) }
				} ),
				new ChampionSpawnInfo( "The Corrupt", typeof( Ilhenir ), new string[]{ "Cleanser", "Expunger", "Depurator" } , new Type[][]
				{																											// Unholy Terror
					new Type[]{ typeof( PlagueSpawn ), typeof( Bogling ) },
					new Type[]{ typeof( PlagueBeast ), typeof( BogThing ) },
					new Type[]{ typeof( PlagueBeastLord ), typeof( InterredGrizzle ) },
					new Type[]{ typeof( FetidEssence ), typeof( PestilentBandage ) }
				} )
			};

        public static ChampionSpawnInfo GetInfo(ChampionSpawnType type)
        {
            int v = (int)type;

            if (v < 0 || v >= m_Table.Length)
                v = 0;

            return m_Table[v];
        }
    }

    public class ChampionSpawn : Item
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public int SpawnSzMod
        {
            get
            {
                return (m_SPawnSzMod < 1 || m_SPawnSzMod > 12) ? 12 : m_SPawnSzMod;
            }
            set
            {
                m_SPawnSzMod = (value < 1 || value > 12) ? 12 : value;
            }
        }
        private int m_SPawnSzMod;

        private bool m_Active;
        private bool m_RandomizeType;
        private ChampionSpawnType m_Type;
        private List<Mobile> m_Creatures;
        private List<Item> m_RedSkulls;
        private List<Item> m_WhiteSkulls;
        private ChampionPlatform m_Platform;
        private ChampionAltar m_Altar;
        private int m_Kills;
        private Mobile m_Champion;

        //private int m_SpawnRange;
        private Rectangle2D m_SpawnArea;
        private ChampionSpawnRegion m_Region;

        private TimeSpan m_ExpireDelay;
        private DateTime m_ExpireTime;

        private TimeSpan m_RestartDelay;
        private DateTime m_RestartTime;

        private Timer m_Timer, m_RestartTimer;

        private IdolOfTheChampion m_Idol;

        private bool m_HasBeenAdvanced;
        private bool m_ConfinedRoaming;

        private Dictionary<Mobile, int> m_DamageEntries;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ConfinedRoaming
        {
            get { return m_ConfinedRoaming; }
            set { m_ConfinedRoaming = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool HasBeenAdvanced
        {
            get { return m_HasBeenAdvanced; }
            set { m_HasBeenAdvanced = value; }
        }

        [Constructable]
        public ChampionSpawn()
            : base(0xBD2)
        {
            Movable = false;
            Visible = false;

            m_Creatures = new List<Mobile>();
            m_RedSkulls = new List<Item>();
            m_WhiteSkulls = new List<Item>();

            m_Platform = new ChampionPlatform(this);
            m_Altar = new ChampionAltar(this);
            m_Idol = new IdolOfTheChampion(this);

            m_ExpireDelay = TimeSpan.FromMinutes(10.0);
            m_RestartDelay = TimeSpan.FromMinutes(10.0);

            m_DamageEntries = new Dictionary<Mobile, int>();

            Timer.DelayCall(TimeSpan.Zero, new TimerCallback(SetInitialSpawnArea));
        }

        public void SetInitialSpawnArea()
        {
            //Previous default used to be 24;
            SpawnArea = new Rectangle2D(new Point2D(X - 24, Y - 24), new Point2D(X + 24, Y + 24));
        }

        public void UpdateRegion()
        {
            if (m_Region != null)
                m_Region.Unregister();

            if (!Deleted && this.Map != Map.Internal)
            {
                m_Region = new ChampionSpawnRegion(this);
                m_Region.Register();
            }

            /*
            if( m_Region == null )
            {
                m_Region = new ChampionSpawnRegion( this );
            }
            else
            {
                m_Region.Unregister();
                //Why doesn't Region allow me to set it's map/Area meself? ><
                m_Region = new ChampionSpawnRegion( this );
            }
            */
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool RandomizeType
        {
            get { return m_RandomizeType; }
            set { m_RandomizeType = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Kills
        {
            get
            {
                return m_Kills;
            }
            set
            {
                m_Kills = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Rectangle2D SpawnArea
        {
            get
            {
                return m_SpawnArea;
            }
            set
            {
                m_SpawnArea = value;
                InvalidateProperties();
                UpdateRegion();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan RestartDelay
        {
            get
            {
                return m_RestartDelay;
            }
            set
            {
                m_RestartDelay = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime RestartTime
        {
            get
            {
                return m_RestartTime;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan ExpireDelay
        {
            get
            {
                return m_ExpireDelay;
            }
            set
            {
                m_ExpireDelay = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime ExpireTime
        {
            get
            {
                return m_ExpireTime;
            }
            set
            {
                m_ExpireTime = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public ChampionSpawnType Type
        {
            get
            {
                return m_Type;
            }
            set
            {
                m_Type = value;
                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Active
        {
            get
            {
                return m_Active;
            }
            set
            {
                if (value)
                    Start();
                else
                    Stop();

                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Champion
        {
            get
            {
                return m_Champion;
            }
            set
            {
                m_Champion = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Level
        {
            get
            {
                return m_RedSkulls.Count;
            }
            set
            {
                for (int i = m_RedSkulls.Count - 1; i >= value; --i)
                {
                    m_RedSkulls[i].Delete();
                    m_RedSkulls.RemoveAt(i);
                }

                for (int i = m_RedSkulls.Count; i < value; ++i)
                {
                    Item skull = new Item(0x1854);

                    skull.Hue = 0x26;
                    skull.Movable = false;
                    skull.Light = LightType.Circle150;

                    skull.MoveToWorld(GetRedSkullLocation(i), Map);

                    m_RedSkulls.Add(skull);
                }

                InvalidateProperties();
            }
        }

        public int MaxKills
        {
            get
            {
                return (m_SPawnSzMod * (250 / 12)) - (Level * m_SPawnSzMod);
            }
        }

        public bool IsChampionSpawn(Mobile m)
        {
            return m_Creatures.Contains(m);
        }

        public void SetWhiteSkullCount(int val)
        {
            for (int i = m_WhiteSkulls.Count - 1; i >= val; --i)
            {
                m_WhiteSkulls[i].Delete();
                m_WhiteSkulls.RemoveAt(i);
            }

            for (int i = m_WhiteSkulls.Count; i < val; ++i)
            {
                Item skull = new Item(0x1854);

                skull.Movable = false;
                skull.Light = LightType.Circle150;

                skull.MoveToWorld(GetWhiteSkullLocation(i), Map);

                m_WhiteSkulls.Add(skull);

                Effects.PlaySound(skull.Location, skull.Map, 0x29);
                Effects.SendLocationEffect(new Point3D(skull.X + 1, skull.Y + 1, skull.Z), skull.Map, 0x3728, 10);
            }
        }

        public void Start()
        {
            if (m_Active || Deleted)
                return;

            m_Active = true;
            m_HasBeenAdvanced = false;

            if (m_Timer != null)
                m_Timer.Stop();

            m_Timer = new SliceTimer(this);
            m_Timer.Start();

            if (m_RestartTimer != null)
                m_RestartTimer.Stop();

            m_RestartTimer = null;

            if (m_Altar != null)
            {
                if (m_Champion != null)
                    m_Altar.Hue = 0x26;
                else
                    m_Altar.Hue = 0;
            }

            if (m_Platform != null)
                m_Platform.Hue = 0x452;
        }

        public void Stop()
        {
            if (!m_Active || Deleted)
                return;

            m_Active = false;
            m_HasBeenAdvanced = false;

            if (m_Timer != null)
                m_Timer.Stop();

            m_Timer = null;

            if (m_RestartTimer != null)
                m_RestartTimer.Stop();

            m_RestartTimer = null;

            if (m_Altar != null)
                m_Altar.Hue = 0;

            if (m_Platform != null)
                m_Platform.Hue = 0x497;
        }

        public void BeginRestart(TimeSpan ts)
        {
            if (m_RestartTimer != null)
                m_RestartTimer.Stop();

            m_RestartTime = DateTime.UtcNow + ts;

            m_RestartTimer = new RestartTimer(this, ts);
            m_RestartTimer.Start();
        }

        public void EndRestart()
        {
            if (RandomizeType)
            {
                switch (Utility.Random(5))
                {
                    case 0: Type = ChampionSpawnType.VerminHorde; break;
                    case 1: Type = ChampionSpawnType.UnholyTerror; break;
                    case 2: Type = ChampionSpawnType.ColdBlood; break;
                    case 3: Type = ChampionSpawnType.Abyss; break;
                    case 4: Type = ChampionSpawnType.Arachnid; break;
                }
            }

            m_HasBeenAdvanced = false;

            Start();
        }

        #region Scroll of Transcendence
        private ScrollofTranscendence CreateRandomSoT(bool felucca)
        {
            int level = Utility.RandomMinMax(1, 5);

            if (felucca)
                level += 5;

            return ScrollofTranscendence.CreateRandom(level, level);
        }
        #endregion

        public static void GiveScrollTo(Mobile killer, SpecialScroll scroll)
        {
            if (scroll == null || killer == null)	//sanity
                return;

            if (scroll is ScrollofTranscendence)
                killer.SendLocalizedMessage(1094936); // You have received a Scroll of Transcendence!
            else
                killer.SendLocalizedMessage(1049524); // You have received a scroll of power!

            if (killer.Alive)
                killer.AddToBackpack(scroll);
            else
            {
                if (killer.Corpse != null && !killer.Corpse.Deleted)
                    killer.Corpse.DropItem(scroll);
                else
                    killer.AddToBackpack(scroll);
            }

            // Justice reward
            PlayerMobile pm = (PlayerMobile)killer;
            for (int j = 0; j < pm.JusticeProtectors.Count; ++j)
            {
                Mobile prot = (Mobile)pm.JusticeProtectors[j];

                if (prot.Map != killer.Map || prot.Kills >= 5 || prot.Criminal || !JusticeVirtue.CheckMapRegion(killer, prot))
                    continue;

                int chance = 0;

                switch (VirtueHelper.GetLevel(prot, VirtueName.Justice))
                {
                    case VirtueLevel.Seeker: chance = 60; break;
                    case VirtueLevel.Follower: chance = 80; break;
                    case VirtueLevel.Knight: chance = 100; break;
                }

                if (chance > Utility.Random(100))
                {
                    try
                    {
                        prot.SendLocalizedMessage(1049368); // You have been rewarded for your dedication to Justice!

                        SpecialScroll scrollDupe = Activator.CreateInstance(scroll.GetType()) as SpecialScroll;

                        if (scrollDupe != null)
                        {
                            scrollDupe.Skill = scroll.Skill;
                            scrollDupe.Value = scroll.Value;
                            prot.AddToBackpack(scrollDupe);
                        }
                    }
                    catch { }
                }
            }
        }

        public void OnSlice()
        {
            if (!m_Active || Deleted)
                return;

            if (m_Champion != null)
            {
                if (m_Champion.Deleted)
                {
                    RegisterDamageTo(m_Champion);

                    if (m_Champion is BaseChampion)
                        AwardArtifact(((BaseChampion)m_Champion).GetArtifact());

                    m_DamageEntries.Clear();

                    if (m_Platform != null)
                        m_Platform.Hue = 0x497;

                    if (m_Altar != null)
                    {
                        m_Altar.Hue = 0;

                        if (!Core.ML || Map == Map.Felucca)
                        {
                            new StarRoomGate(true, m_Altar.Location, m_Altar.Map);
                        }
                    }

                    m_Champion = null;
                    Stop();

                    BeginRestart(m_RestartDelay);
                }
            }
            else
            {
                int kills = m_Kills;

                for (int i = 0; i < m_Creatures.Count; ++i)
                {
                    Mobile m = m_Creatures[i];

                    if (m.Deleted)
                    {
                        if (m.Corpse != null && !m.Corpse.Deleted)
                        {
                            ((Corpse)m.Corpse).BeginDecay(TimeSpan.FromMinutes(1));
                        }
                        m_Creatures.RemoveAt(i);
                        --i;
                        ++m_Kills;

                        Mobile killer = m.FindMostRecentDamager(false);

                        RegisterDamageTo(m);

                        if (killer is BaseCreature)
                            killer = ((BaseCreature)killer).GetMaster();

                        if (killer is PlayerMobile)
                        {
                            #region Scroll of Transcendence
                            if (Core.ML)
                            {
                                if (Map == Map.Felucca)
                                {
                                    if (Utility.RandomDouble() < 0.001)
                                    {
                                        PlayerMobile pm = (PlayerMobile)killer;
                                        double random = Utility.Random(49);

                                        if (random <= 24)
                                        {
                                            ScrollofTranscendence SoTF = CreateRandomSoT(true);
                                            GiveScrollTo(pm, (SpecialScroll)SoTF);
                                        }
                                        else
                                        {
                                            PowerScroll PS = PowerScroll.CreateRandomNoCraft(5, 5);
                                            GiveScrollTo(pm, (SpecialScroll)PS);
                                        }
                                    }
                                }

                                if (Map == Map.Ilshenar || Map == Map.Tokuno || Map == Map.Malas)
                                {
                                    if (Utility.RandomDouble() < 0.0015)
                                    {
                                        killer.SendLocalizedMessage(1094936); // You have received a Scroll of Transcendence!
                                        ScrollofTranscendence SoTT = CreateRandomSoT(false);
                                        killer.AddToBackpack(SoTT);
                                    }
                                }
                            }
                            #endregion

                            int mobSubLevel = GetSubLevelFor(m) + 1;

                            if (mobSubLevel >= 0)
                            {
                                bool gainedPath = false;

                                int pointsToGain = mobSubLevel * 40;

                                if (VirtueHelper.Award(killer, VirtueName.Valor, pointsToGain, ref gainedPath))
                                {
                                    if (gainedPath)
                                        m.SendLocalizedMessage(1054032); // You have gained a path in Valor!
                                    else
                                        m.SendLocalizedMessage(1054030); // You have gained in Valor!

                                    //No delay on Valor gains
                                }

                                PlayerMobile.ChampionTitleInfo info = ((PlayerMobile)killer).ChampionTitles;

                                info.Award(m_Type, mobSubLevel);
                            }
                        }
                    }
                }

                // Only really needed once.
                if (m_Kills > kills)
                    InvalidateProperties();

                double n = m_Kills / (double)MaxKills;
                int p = (int)(n * 100);

                if (p >= 90)
                    AdvanceLevel();
                else if (p > 0)
                    SetWhiteSkullCount(p / 20);

                if (DateTime.UtcNow >= m_ExpireTime)
                    Expire();

                Respawn();
            }
        }

        public void AdvanceLevel()
        {
            m_ExpireTime = DateTime.UtcNow + m_ExpireDelay;

            if (Level < 16)
            {
                m_Kills = 0;
                ++Level;
                InvalidateProperties();
                SetWhiteSkullCount(0);

                if (m_Altar != null)
                {
                    Effects.PlaySound(m_Altar.Location, m_Altar.Map, 0x29);
                    Effects.SendLocationEffect(new Point3D(m_Altar.X + 1, m_Altar.Y + 1, m_Altar.Z), m_Altar.Map, 0x3728, 10);
                }
            }
            else
            {
                SpawnChampion();
            }
        }

        public void SpawnChampion()
        {
            if (m_Altar != null)
                m_Altar.Hue = 0x26;

            if (m_Platform != null)
                m_Platform.Hue = 0x452;

            m_Kills = 0;
            Level = 0;
            InvalidateProperties();
            SetWhiteSkullCount(0);

            try
            {
                m_Champion = Activator.CreateInstance(ChampionSpawnInfo.GetInfo(m_Type).Champion) as Mobile;
            }
            catch { }

            if (m_Champion != null)
                m_Champion.MoveToWorld(new Point3D(X, Y, Z - 15), Map);
        }

        public void Respawn()
        {
            if (!m_Active || Deleted || m_Champion != null)
                return;

            while (m_Creatures.Count < ((m_SPawnSzMod * (200 / 12))) - (GetSubLevel() * (m_SPawnSzMod * (40 / 12))))
            {
                Mobile m = Spawn();

                if (m == null)
                    return;

                Point3D loc = GetSpawnLocation();

                // Allow creatures to turn into Paragons at Ilshenar champions.
                m.OnBeforeSpawn(loc, Map);

                m_Creatures.Add(m);
                m.MoveToWorld(loc, Map);

                if (m is BaseCreature)
                {
                    BaseCreature bc = m as BaseCreature;
                    bc.Tamable = false;

                    if (!m_ConfinedRoaming)
                    {
                        bc.Home = this.Location;
                        bc.RangeHome = (int)(Math.Sqrt(m_SpawnArea.Width * m_SpawnArea.Width + m_SpawnArea.Height * m_SpawnArea.Height) / 2);
                    }
                    else
                    {
                        bc.Home = bc.Location;

                        Point2D xWall1 = new Point2D(m_SpawnArea.X, bc.Y);
                        Point2D xWall2 = new Point2D(m_SpawnArea.X + m_SpawnArea.Width, bc.Y);
                        Point2D yWall1 = new Point2D(bc.X, m_SpawnArea.Y);
                        Point2D yWall2 = new Point2D(bc.X, m_SpawnArea.Y + m_SpawnArea.Height);

                        double minXDist = Math.Min(bc.GetDistanceToSqrt(xWall1), bc.GetDistanceToSqrt(xWall2));
                        double minYDist = Math.Min(bc.GetDistanceToSqrt(yWall1), bc.GetDistanceToSqrt(yWall2));

                        bc.RangeHome = (int)Math.Min(minXDist, minYDist);
                    }
                }
            }
        }

        public Point3D GetSpawnLocation()
        {
            Map map = Map;

            if (map == null)
                return Location;

            // Try 20 times to find a spawnable location.
            for (int i = 0; i < 20; i++)
            {
                /*
                int x = Location.X + (Utility.Random( (m_SpawnRange * 2) + 1 ) - m_SpawnRange);
                int y = Location.Y + (Utility.Random( (m_SpawnRange * 2) + 1 ) - m_SpawnRange);
                */

                int x = Utility.Random(m_SpawnArea.X, m_SpawnArea.Width);
                int y = Utility.Random(m_SpawnArea.Y, m_SpawnArea.Height);

                int z = Map.GetAverageZ(x, y);

                if (Map.CanSpawnMobile(new Point2D(x, y), z))
                    return new Point3D(x, y, z);

                /* try @ platform Z if map z fails */
                else if (Map.CanSpawnMobile(new Point2D(x, y), m_Platform.Location.Z))
                    return new Point3D(x, y, m_Platform.Location.Z);
            }

            return Location;
        }

        private const int Level1 = 4;  // First spawn level from 0-4 red skulls
        private const int Level2 = 8;  // Second spawn level from 5-8 red skulls
        private const int Level3 = 12; // Third spawn level from 9-12 red skulls

        public int GetSubLevel()
        {
            int level = this.Level;

            if (level <= Level1)
                return 0;
            else if (level <= Level2)
                return 1;
            else if (level <= Level3)
                return 2;

            return 3;
        }

        public int GetSubLevelFor(Mobile m)
        {
            Type[][] types = ChampionSpawnInfo.GetInfo(m_Type).SpawnTypes;
            Type t = m.GetType();

            for (int i = 0; i < types.GetLength(0); i++)
            {
                Type[] individualTypes = types[i];

                for (int j = 0; j < individualTypes.Length; j++)
                {
                    if (t == individualTypes[j])
                        return i;
                }
            }

            return -1;
        }

        public Mobile Spawn()
        {
            Type[][] types = ChampionSpawnInfo.GetInfo(m_Type).SpawnTypes;

            int v = GetSubLevel();

            if (v >= 0 && v < types.Length)
                return Spawn(types[v]);

            return null;
        }

        public Mobile Spawn(params Type[] types)
        {
            try
            {
                return Activator.CreateInstance(types[Utility.Random(types.Length)]) as Mobile;
            }
            catch
            {
                return null;
            }
        }

        public void Expire()
        {
            m_Kills = 0;

            if (m_WhiteSkulls.Count == 0)
            {
                // They didn't even get 20%, go back a level

                if (Level > 0)
                    --Level;

                InvalidateProperties();
            }
            else
            {
                SetWhiteSkullCount(0);
            }

            m_ExpireTime = DateTime.UtcNow + m_ExpireDelay;
        }

        public Point3D GetRedSkullLocation(int index)
        {
            int x, y;

            if (index < 5)
            {
                x = index - 2;
                y = -2;
            }
            else if (index < 9)
            {
                x = 2;
                y = index - 6;
            }
            else if (index < 13)
            {
                x = 10 - index;
                y = 2;
            }
            else
            {
                x = -2;
                y = 14 - index;
            }

            return new Point3D(X + x, Y + y, Z - 15);
        }

        public Point3D GetWhiteSkullLocation(int index)
        {
            int x, y;

            switch (index)
            {
                default:
                case 0: x = -1; y = -1; break;
                case 1: x = 1; y = -1; break;
                case 2: x = 1; y = 1; break;
                case 3: x = -1; y = 1; break;
            }

            return new Point3D(X + x, Y + y, Z - 15);
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            list.Add("champion spawn");
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (m_Active)
            {
                list.Add(1060742); // active
                list.Add(1060658, "Type\t{0}", m_Type); // ~1_val~: ~2_val~
                list.Add(1060659, "Level\t{0}", Level); // ~1_val~: ~2_val~
                list.Add(1060660, "Kills\t{0} of {1} ({2:F1}%)", m_Kills, MaxKills, 100.0 * ((double)m_Kills / MaxKills)); // ~1_val~: ~2_val~
                //list.Add( 1060661, "Spawn Range\t{0}", m_SpawnRange ); // ~1_val~: ~2_val~
            }
            else
            {
                list.Add(1060743); // inactive
            }
        }

        public override void OnSingleClick(Mobile from)
        {
            if (m_Active)
                LabelTo(from, "{0} (Active; Level: {1}; Kills: {2}/{3})", m_Type, Level, m_Kills, MaxKills);
            else
                LabelTo(from, "{0} (Inactive)", m_Type);
        }

        public override void OnDoubleClick(Mobile from)
        {
            from.SendGump(new PropertiesGump(from, this));
        }

        public override void OnLocationChange(Point3D oldLoc)
        {
            if (Deleted)
                return;

            if (m_Platform != null)
                m_Platform.Location = new Point3D(X, Y, Z - 20);

            if (m_Altar != null)
                m_Altar.Location = new Point3D(X, Y, Z - 15);

            if (m_Idol != null)
                m_Idol.Location = new Point3D(X, Y, Z - 15);

            if (m_RedSkulls != null)
            {
                for (int i = 0; i < m_RedSkulls.Count; ++i)
                    m_RedSkulls[i].Location = GetRedSkullLocation(i);
            }

            if (m_WhiteSkulls != null)
            {
                for (int i = 0; i < m_WhiteSkulls.Count; ++i)
                    m_WhiteSkulls[i].Location = GetWhiteSkullLocation(i);
            }

            m_SpawnArea.X += Location.X - oldLoc.X;
            m_SpawnArea.Y += Location.Y - oldLoc.Y;

            UpdateRegion();
        }

        public override void OnMapChange()
        {
            if (Deleted)
                return;

            if (m_Platform != null)
                m_Platform.Map = Map;

            if (m_Altar != null)
                m_Altar.Map = Map;

            if (m_Idol != null)
                m_Idol.Map = Map;

            if (m_RedSkulls != null)
            {
                for (int i = 0; i < m_RedSkulls.Count; ++i)
                    m_RedSkulls[i].Map = Map;
            }

            if (m_WhiteSkulls != null)
            {
                for (int i = 0; i < m_WhiteSkulls.Count; ++i)
                    m_WhiteSkulls[i].Map = Map;
            }

            UpdateRegion();
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            if (m_Platform != null)
                m_Platform.Delete();

            if (m_Altar != null)
                m_Altar.Delete();

            if (m_Idol != null)
                m_Idol.Delete();

            if (m_RedSkulls != null)
            {
                for (int i = 0; i < m_RedSkulls.Count; ++i)
                    m_RedSkulls[i].Delete();

                m_RedSkulls.Clear();
            }

            if (m_WhiteSkulls != null)
            {
                for (int i = 0; i < m_WhiteSkulls.Count; ++i)
                    m_WhiteSkulls[i].Delete();

                m_WhiteSkulls.Clear();
            }

            if (m_Creatures != null)
            {
                for (int i = 0; i < m_Creatures.Count; ++i)
                {
                    Mobile mob = m_Creatures[i];

                    if (!mob.Player)
                        mob.Delete();
                }

                m_Creatures.Clear();
            }

            if (m_Champion != null && !m_Champion.Player)
                m_Champion.Delete();

            Stop();

            UpdateRegion();
        }

        public ChampionSpawn(Serial serial)
            : base(serial)
        {
        }

        public virtual void RegisterDamageTo(Mobile m)
        {
            if (m == null)
                return;

            foreach (DamageEntry de in m.DamageEntries)
            {
                if (de.HasExpired)
                    continue;

                Mobile damager = de.Damager;

                Mobile master = damager.GetDamageMaster(m);

                if (master != null)
                    damager = master;

                RegisterDamage(damager, de.DamageGiven);
            }
        }

        public void RegisterDamage(Mobile from, int amount)
        {
            if (from == null || !from.Player)
                return;

            if (m_DamageEntries.ContainsKey(from))
                m_DamageEntries[from] += amount;
            else
                m_DamageEntries.Add(from, amount);
        }

        public void AwardArtifact(Item artifact)
        {
            if (artifact == null)
                return;

            int totalDamage = 0;

            Dictionary<Mobile, int> validEntries = new Dictionary<Mobile, int>();

            foreach (KeyValuePair<Mobile, int> kvp in m_DamageEntries)
            {
                if (IsEligible(kvp.Key, artifact))
                {
                    validEntries.Add(kvp.Key, kvp.Value);
                    totalDamage += kvp.Value;
                }
            }

            int randomDamage = Utility.RandomMinMax(1, totalDamage);

            totalDamage = 0;

            foreach (KeyValuePair<Mobile, int> kvp in validEntries)
            {
                totalDamage += kvp.Value;

                if (totalDamage >= randomDamage)
                {
                    GiveArtifact(kvp.Key, artifact);
                    return;
                }
            }

            artifact.Delete();
        }

        public void GiveArtifact(Mobile to, Item artifact)
        {
            if (to == null || artifact == null)
                return;

            Container pack = to.Backpack;

            if (pack == null || !pack.TryDropItem(to, artifact, false))
                artifact.Delete();
            else
                to.SendLocalizedMessage(1062317); // For your valor in combating the fallen beast, a special artifact has been bestowed on you.
        }

        public bool IsEligible(Mobile m, Item Artifact)
        {
            return m.Player && m.Alive && m.Region != null && m.Region == m_Region && m.Backpack != null && m.Backpack.CheckHold(m, Artifact, false);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)6); // version

            writer.Write((int)m_SPawnSzMod);
            writer.Write(m_DamageEntries.Count);
            foreach (KeyValuePair<Mobile, int> kvp in m_DamageEntries)
            {
                writer.Write(kvp.Key);
                writer.Write(kvp.Value);
            }

            writer.Write(m_ConfinedRoaming);
            writer.WriteItem<IdolOfTheChampion>(m_Idol);
            writer.Write(m_HasBeenAdvanced);
            writer.Write(m_SpawnArea);

            writer.Write(m_RandomizeType);

            //			writer.Write( m_SpawnRange );
            writer.Write(m_Kills);

            writer.Write((bool)m_Active);
            writer.Write((int)m_Type);
            writer.Write(m_Creatures, true);
            writer.Write(m_RedSkulls, true);
            writer.Write(m_WhiteSkulls, true);
            writer.WriteItem<ChampionPlatform>(m_Platform);
            writer.WriteItem<ChampionAltar>(m_Altar);
            writer.Write(m_ExpireDelay);
            writer.WriteDeltaTime(m_ExpireTime);
            writer.Write(m_Champion);
            writer.Write(m_RestartDelay);

            writer.Write(m_RestartTimer != null);

            if (m_RestartTimer != null)
                writer.WriteDeltaTime(m_RestartTime);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            m_DamageEntries = new Dictionary<Mobile, int>();

            int version = reader.ReadInt();

            switch (version)
            {
                case 6:
                    {
                        m_SPawnSzMod = reader.ReadInt();
                        goto case 5;
                    }
                case 5:
                    {
                        int entries = reader.ReadInt();
                        Mobile m;
                        int damage;
                        for (int i = 0; i < entries; ++i)
                        {
                            m = reader.ReadMobile();
                            damage = reader.ReadInt();

                            if (m == null)
                                continue;

                            m_DamageEntries.Add(m, damage);
                        }

                        goto case 4;
                    }
                case 4:
                    {
                        m_ConfinedRoaming = reader.ReadBool();
                        m_Idol = reader.ReadItem<IdolOfTheChampion>();
                        m_HasBeenAdvanced = reader.ReadBool();

                        goto case 3;
                    }
                case 3:
                    {
                        m_SpawnArea = reader.ReadRect2D();

                        goto case 2;
                    }
                case 2:
                    {
                        m_RandomizeType = reader.ReadBool();

                        goto case 1;
                    }
                case 1:
                    {
                        if (version < 3)
                        {
                            int oldRange = reader.ReadInt();

                            m_SpawnArea = new Rectangle2D(new Point2D(X - oldRange, Y - oldRange), new Point2D(X + oldRange, Y + oldRange));
                        }

                        m_Kills = reader.ReadInt();

                        goto case 0;
                    }
                case 0:
                    {
                        if (version < 1)
                            m_SpawnArea = new Rectangle2D(new Point2D(X - 24, Y - 24), new Point2D(X + 24, Y + 24));	//Default was 24

                        bool active = reader.ReadBool();
                        m_Type = (ChampionSpawnType)reader.ReadInt();
                        m_Creatures = reader.ReadStrongMobileList();
                        m_RedSkulls = reader.ReadStrongItemList();
                        m_WhiteSkulls = reader.ReadStrongItemList();
                        m_Platform = reader.ReadItem<ChampionPlatform>();
                        m_Altar = reader.ReadItem<ChampionAltar>();
                        m_ExpireDelay = reader.ReadTimeSpan();
                        m_ExpireTime = reader.ReadDeltaTime();
                        m_Champion = reader.ReadMobile();
                        m_RestartDelay = reader.ReadTimeSpan();

                        if (reader.ReadBool())
                        {
                            m_RestartTime = reader.ReadDeltaTime();
                            BeginRestart(m_RestartTime - DateTime.UtcNow);
                        }

                        if (version < 4)
                        {
                            m_Idol = new IdolOfTheChampion(this);
                            m_Idol.MoveToWorld(new Point3D(X, Y, Z - 15), Map);
                        }

                        if (m_Platform == null || m_Altar == null || m_Idol == null)
                            Delete();
                        else if (active)
                            Start();

                        break;
                    }
            }

            Timer.DelayCall(TimeSpan.Zero, new TimerCallback(UpdateRegion));
        }
    }

    public class ChampionSpawnRegion : BaseRegion
    {
        public override bool YoungProtected { get { return false; } }

        private ChampionSpawn m_Spawn;

        public ChampionSpawn ChampionSpawn
        {
            get { return m_Spawn; }
        }

        public ChampionSpawnRegion(ChampionSpawn spawn)
            : base(null, spawn.Map, Region.Find(spawn.Location, spawn.Map), spawn.SpawnArea)
        {
            m_Spawn = spawn;
        }

        public override bool AllowHousing(Mobile from, Point3D p)
        {
            return false;
        }

        public override void AlterLightLevel(Mobile m, ref int global, ref int personal)
        {
            base.AlterLightLevel(m, ref global, ref personal);
            global = Math.Max(global, 1 + m_Spawn.Level);	//This is a guesstimate.  TODO: Verify & get exact values // OSI testing: at 2 red skulls, light = 0x3 ; 1 red = 0x3.; 3 = 8; 9 = 0xD 8 = 0xD 12 = 0x12 10 = 0xD
        }
    }

    public class IdolOfTheChampion : Item
    {
        private ChampionSpawn m_Spawn;

        public ChampionSpawn Spawn { get { return m_Spawn; } }

        public override string DefaultName
        {
            get { return "Idol of the Champion"; }
        }


        public IdolOfTheChampion(ChampionSpawn spawn)
            : base(0x1F18)
        {
            m_Spawn = spawn;
            Movable = false;
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            if (m_Spawn != null)
                m_Spawn.Delete();
        }

        public IdolOfTheChampion(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write(m_Spawn);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_Spawn = reader.ReadItem() as ChampionSpawn;

                        if (m_Spawn == null)
                            Delete();

                        break;
                    }
            }
        }
    }

    public class SliceTimer : Timer
    {
        private ChampionSpawn m_Spawn;

        public SliceTimer(ChampionSpawn spawn)
            : base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
        {
            m_Spawn = spawn;
            Priority = TimerPriority.OneSecond;
        }

        protected override void OnTick()
        {
            m_Spawn.OnSlice();
        }
    }

    public class RestartTimer : Timer
    {
        private ChampionSpawn m_Spawn;

        public RestartTimer(ChampionSpawn spawn, TimeSpan delay)
            : base(delay)
        {
            m_Spawn = spawn;
            Priority = TimerPriority.FiveSeconds;
        }

        protected override void OnTick()
        {
            m_Spawn.EndRestart();
        }
    }
}

namespace Server.Items
{
    public class ChampionSkull : Item
    {
        private ChampionSkullType m_Type;

        [CommandProperty(AccessLevel.GameMaster)]
        public ChampionSkullType Type { get { return m_Type; } set { m_Type = value; InvalidateProperties(); } }

        public override int LabelNumber { get { return 1049479 + (int)m_Type; } }

        [Constructable]
        public ChampionSkull(ChampionSkullType type)
            : base(0x1AE1)
        {
            m_Type = type;
            LootType = LootType.Cursed;

            // TODO: All hue values
            switch (type)
            {
                case ChampionSkullType.Power: Hue = 0x159; break;
                case ChampionSkullType.Venom: Hue = 0x172; break;
                case ChampionSkullType.Greed: Hue = 0x1EE; break;
                case ChampionSkullType.Death: Hue = 0x025; break;
                case ChampionSkullType.Pain: Hue = 0x035; break;
            }
        }

        public ChampionSkull(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version

            writer.Write((int)m_Type);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                case 0:
                    {
                        m_Type = (ChampionSkullType)reader.ReadInt();

                        break;
                    }
            }

            if (version == 0)
            {
                if (LootType != LootType.Cursed)
                    LootType = LootType.Cursed;

                if (Insured)
                    Insured = false;
            }
        }
    }

    public class HarrowerGate : Moongate
    {
        private Mobile m_Harrower;

        public override int LabelNumber { get { return 1049498; } } // dark moongate

        public HarrowerGate(Mobile harrower, Point3D loc, Map map, Point3D targLoc, Map targMap)
            : base(targLoc, targMap)
        {
            m_Harrower = harrower;

            Dispellable = false;
            ItemID = 0x1FD4;
            Light = LightType.Circle300;

            MoveToWorld(loc, map);
        }

        public HarrowerGate(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write(m_Harrower);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_Harrower = reader.ReadMobile();

                        if (m_Harrower == null)
                            Delete();

                        break;
                    }
            }

            if (Light != LightType.Circle300)
                Light = LightType.Circle300;
        }
    }

    public abstract class SpecialScroll : Item
    {
        private SkillName m_Skill;
        private double m_Value;

        #region Old Item Serialization Vars
        /* DO NOT USE! Only used in serialization of special scrolls that originally derived from Item */
        private bool m_InheritsItem;

        protected bool InheritsItem
        {
            get { return m_InheritsItem; }
        }
        #endregion

        public abstract int Message { get; }
        public virtual int Title { get { return 0; } }
        public abstract string DefaultTitle { get; }

        public SpecialScroll(SkillName skill, double value)
            : base(0x14F0)
        {
            LootType = LootType.Cursed;
            Weight = 1.0;

            m_Skill = skill;
            m_Value = value;
        }

        public SpecialScroll(Serial serial)
            : base(serial)
        {
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SkillName Skill
        {
            get { return m_Skill; }
            set { m_Skill = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public double Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        public virtual string GetNameLocalized()
        {
            return String.Concat("#", AosSkillBonuses.GetLabel(m_Skill).ToString());
        }

        public virtual string GetName()
        {
            int index = (int)m_Skill;
            SkillInfo[] table = SkillInfo.Table;

            if (index >= 0 && index < table.Length)
                return table[index].Name.ToLower();
            else
                return "???";
        }

        public virtual bool CanUse(Mobile from)
        {
            if (Deleted)
                return false;

            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
                return false;
            }

            return true;
        }

        public virtual void Use(Mobile from)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!CanUse(from))
                return;

            from.CloseGump(typeof(SpecialScroll.InternalGump));
            from.SendGump(new InternalGump(from, this));
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version

            writer.Write((int)m_Skill);
            writer.Write((double)m_Value);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                    {
                        m_Skill = (SkillName)reader.ReadInt();
                        m_Value = reader.ReadDouble();
                        break;
                    }
                case 0:
                    {
                        m_InheritsItem = true;

                        if (!(this is StatCapScroll))
                            m_Skill = (SkillName)reader.ReadInt();
                        else
                            m_Skill = SkillName.Alchemy;

                        if (this is ScrollofAlacrity)
                            m_Value = 0.0;
                        else if (this is StatCapScroll)
                            m_Value = (double)reader.ReadInt();
                        else
                            m_Value = reader.ReadDouble();

                        break;
                    }
            }
        }

        public class InternalGump : Gump
        {
            private Mobile m_Mobile;
            private SpecialScroll m_Scroll;

            public InternalGump(Mobile mobile, SpecialScroll scroll)
                : base(25, 50)
            {
                m_Mobile = mobile;
                m_Scroll = scroll;

                AddPage(0);

                AddBackground(25, 10, 420, 200, 5054);

                AddImageTiled(33, 20, 401, 181, 2624);
                AddAlphaRegion(33, 20, 401, 181);

                AddHtmlLocalized(40, 48, 387, 100, m_Scroll.Message, true, true);

                AddHtmlLocalized(125, 148, 200, 20, 1049478, 0xFFFFFF, false, false); // Do you wish to use this scroll?

                AddButton(100, 172, 4005, 4007, 1, GumpButtonType.Reply, 0);
                AddHtmlLocalized(135, 172, 120, 20, 1046362, 0xFFFFFF, false, false); // Yes

                AddButton(275, 172, 4005, 4007, 0, GumpButtonType.Reply, 0);
                AddHtmlLocalized(310, 172, 120, 20, 1046363, 0xFFFFFF, false, false); // No

                if (m_Scroll.Title != 0)
                    AddHtmlLocalized(40, 20, 260, 20, m_Scroll.Title, 0xFFFFFF, false, false);
                else
                    AddHtml(40, 20, 260, 20, m_Scroll.DefaultTitle, false, false);

                if (m_Scroll is StatCapScroll)
                    AddHtmlLocalized(310, 20, 120, 20, 1038019, 0xFFFFFF, false, false); // Power
                else
                    AddHtmlLocalized(310, 20, 120, 20, AosSkillBonuses.GetLabel(m_Scroll.Skill), 0xFFFFFF, false, false);
            }

            public override void OnResponse(NetState state, RelayInfo info)
            {
                if (info.ButtonID == 1)
                    m_Scroll.Use(m_Mobile);
            }
        }
    }

    public class StarRoomGate : Moongate
    {
        private bool m_Decays;
        private DateTime m_DecayTime;
        private Timer m_Timer;

        public override int LabelNumber { get { return 1049498; } } // dark moongate

        [Constructable]
        public StarRoomGate()
            : this(false)
        {
        }

        [Constructable]
        public StarRoomGate(bool decays, Point3D loc, Map map)
            : this(decays)
        {
            MoveToWorld(loc, map);
            Effects.PlaySound(loc, map, 0x20E);
        }

        [Constructable]
        public StarRoomGate(bool decays)
            : base(new Point3D(5143, 1774, 0), Map.Felucca)
        {
            Dispellable = false;
            ItemID = 0x1FD4;

            if (decays)
            {
                m_Decays = true;
                m_DecayTime = DateTime.UtcNow + TimeSpan.FromMinutes(2.0);

                m_Timer = new InternalTimer(this, m_DecayTime);
                m_Timer.Start();
            }
        }

        public StarRoomGate(Serial serial)
            : base(serial)
        {
        }

        public override void OnAfterDelete()
        {
            if (m_Timer != null)
                m_Timer.Stop();

            base.OnAfterDelete();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write(m_Decays);

            if (m_Decays)
                writer.WriteDeltaTime(m_DecayTime);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_Decays = reader.ReadBool();

                        if (m_Decays)
                        {
                            m_DecayTime = reader.ReadDeltaTime();

                            m_Timer = new InternalTimer(this, m_DecayTime);
                            m_Timer.Start();
                        }

                        break;
                    }
            }
        }

        private class InternalTimer : Timer
        {
            private Item m_Item;

            public InternalTimer(Item item, DateTime end)
                : base(end - DateTime.UtcNow)
            {
                m_Item = item;
            }

            protected override void OnTick()
            {
                m_Item.Delete();
            }
        }
    }
}

namespace Server.Mobiles
{
    public abstract class BaseChampion : BaseCreature
    {
        public override bool CanMoveOverObstacles { get { return true; } }
        public override bool CanDestroyObstacles { get { return true; } }

        public BaseChampion(AIType aiType)
            : this(aiType, FightMode.Closest)
        {
        }

        public BaseChampion(AIType aiType, FightMode mode)
            : base(aiType, mode, 18, 1, 0.1, 0.2)
        {
        }

        public BaseChampion(Serial serial)
            : base(serial)
        {
        }

        public abstract ChampionSkullType SkullType { get; }

        public abstract Type[] UniqueList { get; }
        public abstract Type[] SharedList { get; }
        public abstract Type[] DecorativeList { get; }
        public abstract MonsterStatuetteType[] StatueTypes { get; }

        public virtual bool NoGoodies { get { return false; } }

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

        public Item GetArtifact()
        {
            double random = Utility.RandomDouble();
            if (0.05 >= random)
                return CreateArtifact(UniqueList);
            else if (0.15 >= random)
                return CreateArtifact(SharedList);
            else if (0.30 >= random)
                return CreateArtifact(DecorativeList);
            return null;
        }

        public Item CreateArtifact(Type[] list)
        {
            if (list.Length == 0)
                return null;

            int random = Utility.Random(list.Length);

            Type type = list[random];

            Item artifact = Loot.Construct(type);

            if (artifact is MonsterStatuette && StatueTypes.Length > 0)
            {
                ((MonsterStatuette)artifact).Type = StatueTypes[Utility.Random(StatueTypes.Length)];
                ((MonsterStatuette)artifact).LootType = LootType.Regular;
            }

            return artifact;
        }

        private PowerScroll CreateRandomPowerScroll()
        {
            int level;
            double random = Utility.RandomDouble();

            if (0.05 >= random)
                level = 20;
            else if (0.4 >= random)
                level = 15;
            else
                level = 10;

            return PowerScroll.CreateRandomNoCraft(level, level);
        }

        public void GivePowerScrolls()
        {
            if (Map != Map.Felucca)
                return;

            List<Mobile> toGive = new List<Mobile>();
            List<DamageStore> rights = BaseCreature.GetLootingRights(this.DamageEntries, this.HitsMax);

            for (int i = rights.Count - 1; i >= 0; --i)
            {
                DamageStore ds = rights[i];

                if (ds.m_HasRight)
                    toGive.Add(ds.m_Mobile);
            }

            if (toGive.Count == 0)
                return;

            for (int i = 0; i < toGive.Count; i++)
            {
                Mobile m = toGive[i];

                if (!(m is PlayerMobile))
                    continue;

                bool gainedPath = false;

                int pointsToGain = 800;

                if (VirtueHelper.Award(m, VirtueName.Valor, pointsToGain, ref gainedPath))
                {
                    if (gainedPath)
                        m.SendLocalizedMessage(1054032); // You have gained a path in Valor!
                    else
                        m.SendLocalizedMessage(1054030); // You have gained in Valor!

                    //No delay on Valor gains
                }
            }

            // Randomize
            for (int i = 0; i < toGive.Count; ++i)
            {
                int rand = Utility.Random(toGive.Count);
                Mobile hold = toGive[i];
                toGive[i] = toGive[rand];
                toGive[rand] = hold;
            }

            for (int i = 0; i < 6; ++i)
            {
                Mobile m = toGive[i % toGive.Count];

                PowerScroll ps = CreateRandomPowerScroll();

                GivePowerScrollTo(m, ps);
            }
        }

        public static void GivePowerScrollTo(Mobile m, PowerScroll ps)
        {
            if (ps == null || m == null)	//sanity
                return;

            m.SendLocalizedMessage(1049524); // You have received a scroll of power!

            if (!Core.SE || m.Alive)
                m.AddToBackpack(ps);
            else
            {
                if (m.Corpse != null && !m.Corpse.Deleted)
                    m.Corpse.DropItem(ps);
                else
                    m.AddToBackpack(ps);
            }

            if (m is PlayerMobile)
            {
                PlayerMobile pm = (PlayerMobile)m;

                for (int j = 0; j < pm.JusticeProtectors.Count; ++j)
                {
                    Mobile prot = pm.JusticeProtectors[j];

                    if (prot.Map != m.Map || prot.Kills >= 5 || prot.Criminal || !JusticeVirtue.CheckMapRegion(m, prot))
                        continue;

                    int chance = 0;

                    switch (VirtueHelper.GetLevel(prot, VirtueName.Justice))
                    {
                        case VirtueLevel.Seeker: chance = 60; break;
                        case VirtueLevel.Follower: chance = 80; break;
                        case VirtueLevel.Knight: chance = 100; break;
                    }

                    if (chance > Utility.Random(100))
                    {
                        PowerScroll powerScroll = new PowerScroll(ps.Skill, ps.Value);

                        prot.SendLocalizedMessage(1049368); // You have been rewarded for your dedication to Justice!

                        if (!Core.SE || prot.Alive)
                            prot.AddToBackpack(powerScroll);
                        else
                        {
                            if (prot.Corpse != null && !prot.Corpse.Deleted)
                                prot.Corpse.DropItem(powerScroll);
                            else
                                prot.AddToBackpack(powerScroll);
                        }
                    }
                }
            }
        }

        public override bool OnBeforeDeath()
        {
            if (!NoKillAwards)
            {
                GivePowerScrolls();

                if (NoGoodies)
                    return base.OnBeforeDeath();

                Map map = this.Map;

                if (map != null)
                {
                    for (int x = -12; x <= 12; ++x)
                    {
                        for (int y = -12; y <= 12; ++y)
                        {
                            double dist = Math.Sqrt(x * x + y * y);

                            if (dist <= 12)
                                new GoodiesTimer(map, X + x, Y + y).Start();
                        }
                    }
                }
            }

            return base.OnBeforeDeath();
        }

        public override void OnDeath(Container c)
        {
            if (Map == Map.Felucca)
            {
                //TODO: Confirm SE change or AoS one too?
                List<DamageStore> rights = BaseCreature.GetLootingRights(this.DamageEntries, this.HitsMax);
                List<Mobile> toGive = new List<Mobile>();

                for (int i = rights.Count - 1; i >= 0; --i)
                {
                    DamageStore ds = rights[i];

                    if (ds.m_HasRight)
                        toGive.Add(ds.m_Mobile);
                }

                if (toGive.Count > 0)
                    toGive[Utility.Random(toGive.Count)].AddToBackpack(new ChampionSkull(SkullType));
                else
                    c.DropItem(new ChampionSkull(SkullType));
            }

            base.OnDeath(c);
        }

        private class GoodiesTimer : Timer
        {
            private Map m_Map;
            private int m_X, m_Y;

            public GoodiesTimer(Map map, int x, int y)
                : base(TimeSpan.FromSeconds(Utility.RandomDouble() * 10.0))
            {
                m_Map = map;
                m_X = x;
                m_Y = y;
            }

            protected override void OnTick()
            {
                int z = m_Map.GetAverageZ(m_X, m_Y);
                bool canFit = m_Map.CanFit(m_X, m_Y, z, 6, false, false);

                for (int i = -3; !canFit && i <= 3; ++i)
                {
                    canFit = m_Map.CanFit(m_X, m_Y, z + i, 6, false, false);

                    if (canFit)
                        z += i;
                }

                if (!canFit)
                    return;

                Gold g = new Gold(500, 1000);

                g.MoveToWorld(new Point3D(m_X, m_Y, z), m_Map);

                if (0.5 >= Utility.RandomDouble())
                {
                    switch (Utility.Random(3))
                    {
                        case 0: // Fire column
                            {
                                Effects.SendLocationParticles(EffectItem.Create(g.Location, g.Map, EffectItem.DefaultDuration), 0x3709, 10, 30, 5052);
                                Effects.PlaySound(g, g.Map, 0x208);

                                break;
                            }
                        case 1: // Explosion
                            {
                                Effects.SendLocationParticles(EffectItem.Create(g.Location, g.Map, EffectItem.DefaultDuration), 0x36BD, 20, 10, 5044);
                                Effects.PlaySound(g, g.Map, 0x307);

                                break;
                            }
                        case 2: // Ball of fire
                            {
                                Effects.SendLocationParticles(EffectItem.Create(g.Location, g.Map, EffectItem.DefaultDuration), 0x36FE, 10, 10, 5052);

                                break;
                            }
                    }
                }
            }
        }
    }
}