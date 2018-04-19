using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using Server;
using Server.Commands;
using Server.Items;
using Server.Mobiles;

namespace Server.Regions
{
    public abstract class SpawnDefinition
    {
        protected SpawnDefinition()
        {
        }

        public abstract ISpawnable Spawn(SpawnEntry entry);

        public abstract bool CanSpawn(params Type[] types);

        public static SpawnDefinition GetSpawnDefinition(XmlElement xml)
        {
            switch (xml.Name)
            {
                case "object":
                    {
                        Type type = null;
                        if (!Region.ReadType(xml, "type", ref type))
                            return null;

                        if (typeof(Mobile).IsAssignableFrom(type))
                        {
                            return SpawnMobile.Get(type);
                        }
                        else if (typeof(Item).IsAssignableFrom(type))
                        {
                            return SpawnItem.Get(type);
                        }
                        else
                        {
                            Console.WriteLine("Invalid type '{0}' in a SpawnDefinition", type.FullName);
                            return null;
                        }
                    }
                case "group":
                    {
                        string group = null;
                        if (!Region.ReadString(xml, "name", ref group))
                            return null;

                        SpawnDefinition def = (SpawnDefinition)SpawnGroup.Table[group];

                        if (def == null)
                        {
                            Console.WriteLine("Could not find group '{0}' in a SpawnDefinition", group);
                            return null;
                        }
                        else
                        {
                            return def;
                        }
                    }
                case "treasureChest":
                    {
                        int itemID = 0xE43;
                        Region.ReadInt32(xml, "itemID", ref itemID, false);

                        BaseTreasureChest.TreasureLevel level = BaseTreasureChest.TreasureLevel.Level2;

                        Region.ReadEnum(xml, "level", ref level, false);

                        return new SpawnTreasureChest(itemID, level);
                    }
                default:
                    {
                        return null;
                    }
            }
        }
    }

    public abstract class SpawnType : SpawnDefinition
    {
        private Type m_Type;
        private bool m_Init;

        public Type Type { get { return m_Type; } }

        public abstract int Height { get; }
        public abstract bool Land { get; }
        public abstract bool Water { get; }

        protected SpawnType(Type type)
        {
            m_Type = type;
            m_Init = false;
        }

        protected void EnsureInit()
        {
            if (m_Init)
                return;

            Init();
            m_Init = true;
        }

        protected virtual void Init()
        {
        }

        public override ISpawnable Spawn(SpawnEntry entry)
        {
            BaseRegion region = entry.Region;
            Map map = region.Map;

            Point3D loc = entry.RandomSpawnLocation(this.Height, this.Land, this.Water);

            if (loc == Point3D.Zero)
                return null;

            return Construct(entry, loc, map);
        }

        protected abstract ISpawnable Construct(SpawnEntry entry, Point3D loc, Map map);

        public override bool CanSpawn(params Type[] types)
        {
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i] == m_Type)
                    return true;
            }

            return false;
        }
    }

    public class SpawnEntry : ISpawner
    {
        public static readonly TimeSpan DefaultMinSpawnTime = TimeSpan.FromMinutes(2.0);
        public static readonly TimeSpan DefaultMaxSpawnTime = TimeSpan.FromMinutes(5.0);

        private static Hashtable m_Table = new Hashtable();

        public static Hashtable Table { get { return m_Table; } }


        // When a creature's AI is deactivated (PlayerRangeSensitive optimization) does it return home?
        public bool ReturnOnDeactivate { get { return true; } }

        // Are creatures unlinked on taming (true) or should they also go out of the region (false)?
        public bool UnlinkOnTaming { get { return false; } }

        // Are unlinked and untamed creatures removed after 20 hours?
        public bool RemoveIfUntamed { get { return true; } }


        public static readonly Direction InvalidDirection = Direction.Running;

        private int m_ID;
        private BaseRegion m_Region;
        private Point3D m_Home;
        private int m_Range;
        private Direction m_Direction;
        private SpawnDefinition m_Definition;
        private List<ISpawnable> m_SpawnedObjects;
        private int m_Max;
        private TimeSpan m_MinSpawnTime;
        private TimeSpan m_MaxSpawnTime;
        private bool m_Running;

        private DateTime m_NextSpawn;
        private Timer m_SpawnTimer;

        public int ID { get { return m_ID; } }
        public BaseRegion Region { get { return m_Region; } }
        public Point3D HomeLocation { get { return m_Home; } }
        public int HomeRange { get { return m_Range; } }
        public Direction Direction { get { return m_Direction; } }
        public SpawnDefinition Definition { get { return m_Definition; } }
        public List<ISpawnable> SpawnedObjects { get { return m_SpawnedObjects; } }
        public int Max { get { return m_Max; } }
        public TimeSpan MinSpawnTime { get { return m_MinSpawnTime; } }
        public TimeSpan MaxSpawnTime { get { return m_MaxSpawnTime; } }
        public bool Running { get { return m_Running; } }

        public bool Complete { get { return m_SpawnedObjects.Count >= m_Max; } }
        public bool Spawning { get { return m_Running && !this.Complete; } }

        public SpawnEntry(int id, BaseRegion region, Point3D home, int range, Direction direction, SpawnDefinition definition, int max, TimeSpan minSpawnTime, TimeSpan maxSpawnTime)
        {
            m_ID = id;
            m_Region = region;
            m_Home = home;
            m_Range = range;
            m_Direction = direction;
            m_Definition = definition;
            m_SpawnedObjects = new List<ISpawnable>();
            m_Max = max;
            m_MinSpawnTime = minSpawnTime;
            m_MaxSpawnTime = maxSpawnTime;
            m_Running = false;

            if (m_Table.Contains(id))
                Console.WriteLine("Warning: double SpawnEntry ID '{0}'", id);
            else
                m_Table[id] = this;
        }

        public Point3D RandomSpawnLocation(int spawnHeight, bool land, bool water)
        {
            return m_Region.RandomSpawnLocation(spawnHeight, land, water, m_Home, m_Range);
        }

        public void Start()
        {
            if (m_Running)
                return;

            m_Running = true;
            CheckTimer();
        }

        public void Stop()
        {
            if (!m_Running)
                return;

            m_Running = false;
            CheckTimer();
        }

        private void Spawn()
        {
            ISpawnable spawn = m_Definition.Spawn(this);

            if (spawn != null)
                Add(spawn);
        }

        private void Add(ISpawnable spawn)
        {
            m_SpawnedObjects.Add(spawn);

            spawn.Spawner = this;

            if (spawn is BaseCreature)
                ((BaseCreature)spawn).RemoveIfUntamed = this.RemoveIfUntamed;
        }

        void ISpawner.Remove(ISpawnable spawn)
        {
            m_SpawnedObjects.Remove(spawn);

            CheckTimer();
        }

        private TimeSpan RandomTime()
        {
            int min = (int)m_MinSpawnTime.TotalSeconds;
            int max = (int)m_MaxSpawnTime.TotalSeconds;

            int rand = Utility.RandomMinMax(min, max);
            return TimeSpan.FromSeconds(rand);
        }

        private void CheckTimer()
        {
            if (this.Spawning)
            {
                if (m_SpawnTimer == null)
                {
                    TimeSpan time = RandomTime();
                    m_SpawnTimer = Timer.DelayCall(time, new TimerCallback(TimerCallback));
                    m_NextSpawn = DateTime.UtcNow + time;
                }
            }
            else if (m_SpawnTimer != null)
            {
                m_SpawnTimer.Stop();
                m_SpawnTimer = null;
            }
        }

        private void TimerCallback()
        {
            int amount = Math.Max((m_Max - m_SpawnedObjects.Count) / 3, 1);

            for (int i = 0; i < amount; i++)
                Spawn();

            m_SpawnTimer = null;
            CheckTimer();
        }

        public void DeleteSpawnedObjects()
        {
            InternalDeleteSpawnedObjects();

            m_Running = false;
            CheckTimer();
        }

        private void InternalDeleteSpawnedObjects()
        {
            foreach (ISpawnable spawnable in m_SpawnedObjects)
            {
                spawnable.Spawner = null;

                bool uncontrolled = !(spawnable is BaseCreature) || !((BaseCreature)spawnable).Controlled;

                if (uncontrolled)
                    spawnable.Delete();
            }

            m_SpawnedObjects.Clear();
        }

        public void Respawn()
        {
            InternalDeleteSpawnedObjects();

            for (int i = 0; !this.Complete && i < m_Max; i++)
                Spawn();

            m_Running = true;
            CheckTimer();
        }

        public void Delete()
        {
            m_Max = 0;
            InternalDeleteSpawnedObjects();

            if (m_SpawnTimer != null)
            {
                m_SpawnTimer.Stop();
                m_SpawnTimer = null;
            }

            if (m_Table[m_ID] == this)
                m_Table.Remove(m_ID);
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write((int)m_SpawnedObjects.Count);

            for (int i = 0; i < m_SpawnedObjects.Count; i++)
            {
                ISpawnable spawn = m_SpawnedObjects[i];

                int serial = spawn.Serial;

                writer.Write((int)serial);
            }

            writer.Write((bool)m_Running);

            if (m_SpawnTimer != null)
            {
                writer.Write(true);
                writer.WriteDeltaTime((DateTime)m_NextSpawn);
            }
            else
            {
                writer.Write(false);
            }
        }

        public void Deserialize(GenericReader reader, int version)
        {
            int count = reader.ReadInt();

            for (int i = 0; i < count; i++)
            {
                int serial = reader.ReadInt();
                ISpawnable spawnableEntity = World.FindEntity(serial) as ISpawnable;

                if (spawnableEntity != null)
                    Add(spawnableEntity);
            }

            m_Running = reader.ReadBool();

            if (reader.ReadBool())
            {
                m_NextSpawn = reader.ReadDeltaTime();

                if (this.Spawning)
                {
                    if (m_SpawnTimer != null)
                        m_SpawnTimer.Stop();

                    TimeSpan delay = m_NextSpawn - DateTime.UtcNow;
                    m_SpawnTimer = Timer.DelayCall(delay > TimeSpan.Zero ? delay : TimeSpan.Zero, new TimerCallback(TimerCallback));
                }
            }

            CheckTimer();
        }

        private static List<IEntity> m_RemoveList;

        public static void Remove(GenericReader reader, int version)
        {
            int count = reader.ReadInt();

            for (int i = 0; i < count; i++)
            {
                int serial = reader.ReadInt();
                IEntity entity = World.FindEntity(serial);

                if (entity != null)
                {
                    if (m_RemoveList == null)
                        m_RemoveList = new List<IEntity>();

                    m_RemoveList.Add(entity);
                }
            }

            reader.ReadBool(); // m_Running

            if (reader.ReadBool())
                reader.ReadDeltaTime(); // m_NextSpawn
        }

        public static void Initialize()
        {
            if (m_RemoveList != null)
            {
                foreach (IEntity ent in m_RemoveList)
                {
                    ent.Delete();
                }

                m_RemoveList = null;
            }

            SpawnPersistence.EnsureExistence();

            CommandSystem.Register("RespawnAllRegions", AccessLevel.Administrator, new CommandEventHandler(RespawnAllRegions_OnCommand));
            CommandSystem.Register("RespawnRegion", AccessLevel.GameMaster, new CommandEventHandler(RespawnRegion_OnCommand));
            CommandSystem.Register("DelAllRegionSpawns", AccessLevel.Administrator, new CommandEventHandler(DelAllRegionSpawns_OnCommand));
            CommandSystem.Register("DelRegionSpawns", AccessLevel.GameMaster, new CommandEventHandler(DelRegionSpawns_OnCommand));
            CommandSystem.Register("StartAllRegionSpawns", AccessLevel.Administrator, new CommandEventHandler(StartAllRegionSpawns_OnCommand));
            CommandSystem.Register("StartRegionSpawns", AccessLevel.GameMaster, new CommandEventHandler(StartRegionSpawns_OnCommand));
            CommandSystem.Register("StopAllRegionSpawns", AccessLevel.Administrator, new CommandEventHandler(StopAllRegionSpawns_OnCommand));
            CommandSystem.Register("StopRegionSpawns", AccessLevel.GameMaster, new CommandEventHandler(StopRegionSpawns_OnCommand));
        }

        private static BaseRegion GetCommandData(CommandEventArgs args)
        {
            Mobile from = args.Mobile;

            Region reg;
            if (args.Length == 0)
            {
                reg = from.Region;
            }
            else
            {
                string name = args.GetString(0);
                //reg = (Region) from.Map.Regions[name];

                if (!from.Map.Regions.TryGetValue(name, out reg))
                {
                    from.SendMessage("Could not find region '{0}'.", name);
                    return null;
                }
            }

            BaseRegion br = reg as BaseRegion;

            if (br == null || br.Spawns == null)
            {
                from.SendMessage("There are no spawners in region '{0}'.", reg);
                return null;
            }

            return br;
        }

        [Usage("RespawnAllRegions")]
        [Description("Respawns all regions and sets the spawners as running.")]
        private static void RespawnAllRegions_OnCommand(CommandEventArgs args)
        {
            foreach (SpawnEntry entry in m_Table.Values)
            {
                entry.Respawn();
            }

            args.Mobile.SendMessage("All regions have respawned.");
        }

        [Usage("RespawnRegion [<region name>]")]
        [Description("Respawns the region in which you are (or that you provided) and sets the spawners as running.")]
        private static void RespawnRegion_OnCommand(CommandEventArgs args)
        {
            BaseRegion region = GetCommandData(args);

            if (region == null)
                return;

            for (int i = 0; i < region.Spawns.Length; i++)
                region.Spawns[i].Respawn();

            args.Mobile.SendMessage("Region '{0}' has respawned.", region);
        }

        [Usage("DelAllRegionSpawns")]
        [Description("Deletes all spawned objects of every regions and sets the spawners as not running.")]
        private static void DelAllRegionSpawns_OnCommand(CommandEventArgs args)
        {
            foreach (SpawnEntry entry in m_Table.Values)
            {
                entry.DeleteSpawnedObjects();
            }

            args.Mobile.SendMessage("All region spawned objects have been deleted.");
        }

        [Usage("DelRegionSpawns [<region name>]")]
        [Description("Deletes all spawned objects of the region in which you are (or that you provided) and sets the spawners as not running.")]
        private static void DelRegionSpawns_OnCommand(CommandEventArgs args)
        {
            BaseRegion region = GetCommandData(args);

            if (region == null)
                return;

            for (int i = 0; i < region.Spawns.Length; i++)
                region.Spawns[i].DeleteSpawnedObjects();

            args.Mobile.SendMessage("Spawned objects of region '{0}' have been deleted.", region);
        }

        [Usage("StartAllRegionSpawns")]
        [Description("Sets the region spawners of all regions as running.")]
        private static void StartAllRegionSpawns_OnCommand(CommandEventArgs args)
        {
            foreach (SpawnEntry entry in m_Table.Values)
            {
                entry.Start();
            }

            args.Mobile.SendMessage("All region spawners have started.");
        }

        [Usage("StartRegionSpawns [<region name>]")]
        [Description("Sets the region spawners of the region in which you are (or that you provided) as running.")]
        private static void StartRegionSpawns_OnCommand(CommandEventArgs args)
        {
            BaseRegion region = GetCommandData(args);

            if (region == null)
                return;

            for (int i = 0; i < region.Spawns.Length; i++)
                region.Spawns[i].Start();

            args.Mobile.SendMessage("Spawners of region '{0}' have started.", region);
        }

        [Usage("StopAllRegionSpawns")]
        [Description("Sets the region spawners of all regions as not running.")]
        private static void StopAllRegionSpawns_OnCommand(CommandEventArgs args)
        {
            foreach (SpawnEntry entry in m_Table.Values)
            {
                entry.Stop();
            }

            args.Mobile.SendMessage("All region spawners have stopped.");
        }

        [Usage("StopRegionSpawns [<region name>]")]
        [Description("Sets the region spawners of the region in which you are (or that you provided) as not running.")]
        private static void StopRegionSpawns_OnCommand(CommandEventArgs args)
        {
            BaseRegion region = GetCommandData(args);

            if (region == null)
                return;

            for (int i = 0; i < region.Spawns.Length; i++)
                region.Spawns[i].Stop();

            args.Mobile.SendMessage("Spawners of region '{0}' have stopped.", region);
        }
    }

    public class SpawnGroupElement
    {
        private SpawnDefinition m_SpawnDefinition;
        private int m_Weight;

        public SpawnDefinition SpawnDefinition { get { return m_SpawnDefinition; } }
        public int Weight { get { return m_Weight; } }

        public SpawnGroupElement(SpawnDefinition spawnDefinition, int weight)
        {
            m_SpawnDefinition = spawnDefinition;
            m_Weight = weight;
        }
    }

    public class SpawnGroup : SpawnDefinition
    {
        private static Hashtable m_Table = new Hashtable();

        public static Hashtable Table { get { return m_Table; } }

        public static void Register(SpawnGroup group)
        {
            if (m_Table.Contains(group.Name))
                Console.WriteLine("Warning: Double SpawnGroup name '{0}'", group.Name);
            else
                m_Table[group.Name] = group;
        }

        static SpawnGroup()
        {
            string path = Path.Combine(Core.BaseDirectory, "Data/SpawnDefinitions.xml");
            if (!File.Exists(path))
                return;

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);

                XmlElement root = doc["spawnDefinitions"];
                if (root == null)
                    return;

                foreach (XmlElement xmlDef in root.SelectNodes("spawnGroup"))
                {
                    string name = null;
                    if (!Region.ReadString(xmlDef, "name", ref name))
                        continue;

                    List<SpawnGroupElement> list = new List<SpawnGroupElement>();
                    foreach (XmlNode node in xmlDef.ChildNodes)
                    {
                        XmlElement el = node as XmlElement;

                        if (el != null)
                        {
                            SpawnDefinition def = GetSpawnDefinition(el);
                            if (def == null)
                                continue;

                            int weight = 1;
                            Region.ReadInt32(el, "weight", ref weight, false);

                            SpawnGroupElement groupElement = new SpawnGroupElement(def, weight);
                            list.Add(groupElement);
                        }
                    }

                    SpawnGroupElement[] elements = list.ToArray();
                    SpawnGroup group = new SpawnGroup(name, elements);
                    Register(group);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not load SpawnDefinitions.xml: " + ex.Message);
            }
        }

        private string m_Name;
        private SpawnGroupElement[] m_Elements;
        private int m_TotalWeight;

        public string Name { get { return m_Name; } }
        public SpawnGroupElement[] Elements { get { return m_Elements; } }

        public SpawnGroup(string name, SpawnGroupElement[] elements)
        {
            m_Name = name;
            m_Elements = elements;

            m_TotalWeight = 0;
            for (int i = 0; i < elements.Length; i++)
                m_TotalWeight += elements[i].Weight;
        }

        public override ISpawnable Spawn(SpawnEntry entry)
        {
            int index = Utility.Random(m_TotalWeight);

            for (int i = 0; i < m_Elements.Length; i++)
            {
                SpawnGroupElement element = m_Elements[i];

                if (index < element.Weight)
                    return element.SpawnDefinition.Spawn(entry);

                index -= element.Weight;
            }

            return null;
        }

        public override bool CanSpawn(params Type[] types)
        {
            for (int i = 0; i < m_Elements.Length; i++)
            {
                if (m_Elements[i].SpawnDefinition.CanSpawn(types))
                    return true;
            }

            return false;
        }
    }

    public class SpawnPersistence : Item
    {
        private static SpawnPersistence m_Instance;

        public SpawnPersistence Instance { get { return m_Instance; } }

        public static void EnsureExistence()
        {
            if (m_Instance == null)
                m_Instance = new SpawnPersistence();
        }

        public override string DefaultName
        {
            get { return "Region spawn persistence - Internal"; }
        }

        private SpawnPersistence()
            : base(1)
        {
            Movable = false;
        }

        public SpawnPersistence(Serial serial)
            : base(serial)
        {
            m_Instance = this;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version

            writer.Write((int)SpawnEntry.Table.Values.Count);
            foreach (SpawnEntry entry in SpawnEntry.Table.Values)
            {
                writer.Write((int)entry.ID);

                entry.Serialize(writer);
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();

            int count = reader.ReadInt();
            for (int i = 0; i < count; i++)
            {
                int id = reader.ReadInt();

                SpawnEntry entry = (SpawnEntry)SpawnEntry.Table[id];

                if (entry != null)
                    entry.Deserialize(reader, version);
                else
                    SpawnEntry.Remove(reader, version);
            }
        }
    }

    public class SpawnMobile : SpawnType
    {
        private static Hashtable m_Table = new Hashtable();

        public static SpawnMobile Get(Type type)
        {
            SpawnMobile sm = (SpawnMobile)m_Table[type];

            if (sm == null)
            {
                sm = new SpawnMobile(type);
                m_Table[type] = sm;
            }

            return sm;
        }

        protected bool m_Land;
        protected bool m_Water;

        public override int Height { get { return 16; } }
        public override bool Land { get { EnsureInit(); return m_Land; } }
        public override bool Water { get { EnsureInit(); return m_Water; } }

        protected SpawnMobile(Type type)
            : base(type)
        {
        }

        protected override void Init()
        {
            Mobile mob = (Mobile)Activator.CreateInstance(Type);

            m_Land = !mob.CantWalk;
            m_Water = mob.CanSwim;

            mob.Delete();
        }

        protected override ISpawnable Construct(SpawnEntry entry, Point3D loc, Map map)
        {
            Mobile mobile = CreateMobile();

            BaseCreature creature = mobile as BaseCreature;

            if (creature != null)
            {
                creature.Home = entry.HomeLocation;
                creature.RangeHome = entry.HomeRange;
            }

            if (entry.Direction != SpawnEntry.InvalidDirection)
                mobile.Direction = entry.Direction;

            mobile.OnBeforeSpawn(loc, map);
            mobile.MoveToWorld(loc, map);
            mobile.OnAfterSpawn();

            return mobile;
        }

        protected virtual Mobile CreateMobile()
        {
            return (Mobile)Activator.CreateInstance(Type);
        }
    }

    public class SpawnItem : SpawnType
    {
        private static Hashtable m_Table = new Hashtable();

        public static SpawnItem Get(Type type)
        {
            SpawnItem si = (SpawnItem)m_Table[type];

            if (si == null)
            {
                si = new SpawnItem(type);
                m_Table[type] = si;
            }

            return si;
        }

        protected int m_Height;

        public override int Height { get { EnsureInit(); return m_Height; } }
        public override bool Land { get { return true; } }
        public override bool Water { get { return false; } }

        protected SpawnItem(Type type)
            : base(type)
        {
        }

        protected override void Init()
        {
            Item item = (Item)Activator.CreateInstance(Type);

            m_Height = item.ItemData.Height;

            item.Delete();
        }

        protected override ISpawnable Construct(SpawnEntry entry, Point3D loc, Map map)
        {
            Item item = CreateItem();

            item.OnBeforeSpawn(loc, map);
            item.MoveToWorld(loc, map);
            item.OnAfterSpawn();

            return item;
        }

        protected virtual Item CreateItem()
        {
            return (Item)Activator.CreateInstance(Type);
        }
    }

    public class SpawnTreasureChest : SpawnItem
    {
        private int m_ItemID;
        private BaseTreasureChest.TreasureLevel m_Level;

        public int ItemID { get { return m_ItemID; } }
        public BaseTreasureChest.TreasureLevel Level { get { return m_Level; } }

        public SpawnTreasureChest(int itemID, BaseTreasureChest.TreasureLevel level)
            : base(typeof(BaseTreasureChest))
        {
            m_ItemID = itemID;
            m_Level = level;
        }

        protected override void Init()
        {
            m_Height = TileData.ItemTable[m_ItemID & TileData.MaxItemValue].Height;
        }

        protected override Item CreateItem()
        {
            return new BaseTreasureChest(m_ItemID, m_Level);
        }
    }
}