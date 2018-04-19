using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Server.Commands;
using Server.Items;
using Server.Mobiles;

namespace Server
{
    public class VendorGenerator
    {
        public static void Initialize()
        {
            CommandSystem.Register("VendorGen", AccessLevel.Administrator, new CommandEventHandler(VendorGenerator.VendorGen_OnCommand));
        }

        private static Rectangle2D[] m_BritRegions = new Rectangle2D[]
			{
				new Rectangle2D( new Point2D( 250, 750 ), new Point2D( 775, 1330 ) ),
				new Rectangle2D( new Point2D( 525, 2095 ), new Point2D( 925, 2430 ) ),
				new Rectangle2D( new Point2D( 1025, 2155 ), new Point2D( 1265, 2310 ) ),
				new Rectangle2D( new Point2D( 1635, 2430 ), new Point2D( 1705, 2508 ) ),
				new Rectangle2D( new Point2D( 1775, 2605 ), new Point2D( 2165, 2975 ) ),
				new Rectangle2D( new Point2D( 1055, 3520 ), new Point2D( 1570, 4075 ) ),
				new Rectangle2D( new Point2D( 2860, 3310 ), new Point2D( 3120, 3630 ) ),
				new Rectangle2D( new Point2D( 2470, 1855 ), new Point2D( 3950, 3045 ) ),
				new Rectangle2D( new Point2D( 3425, 990 ), new Point2D( 3900, 1455 ) ),
				new Rectangle2D( new Point2D( 4175, 735 ), new Point2D( 4840, 1600 ) ),
				new Rectangle2D( new Point2D( 2375, 330 ), new Point2D( 3100, 1045 ) ),
				new Rectangle2D( new Point2D( 2100, 1090 ), new Point2D( 2310, 1450 ) ),
				new Rectangle2D( new Point2D( 1495, 1400 ), new Point2D( 1550, 1475 ) ),
				new Rectangle2D( new Point2D( 1085, 1520 ), new Point2D( 1415, 1910 ) ),
				new Rectangle2D( new Point2D( 1410, 1500 ), new Point2D( 1745, 1795 ) ),
				new Rectangle2D( new Point2D( 5120, 2300 ), new Point2D( 6143, 4095 ) )
			};

        private static Rectangle2D[] m_IlshRegions = new Rectangle2D[]
			{
				new Rectangle2D( new Point2D( 0, 0 ), new Point2D( 288*8, 200*8 ) )
			};

        [Usage("VendorGen")]
        [Description("Generates vendors based on display cases and floor plans. Analyzes the map files, slow.")]
        private static void VendorGen_OnCommand(CommandEventArgs e)
        {
            Process(Map.Trammel, m_BritRegions);
            Process(Map.Felucca, m_BritRegions);
            Process(Map.Ilshenar, m_IlshRegions);
        }

        private static bool GetFloorZ(Map map, int x, int y, out int z)
        {
            LandTile lt = map.Tiles.GetLandTile(x, y);

            if (IsFloor(lt.ID) && map.CanFit(x, y, lt.Z, 16, false, false))
            {
                z = lt.Z;
                return true;
            }

            StaticTile[] tiles = map.Tiles.GetStaticTiles(x, y);

            for (int i = 0; i < tiles.Length; ++i)
            {
                StaticTile t = tiles[i];
                ItemData id = TileData.ItemTable[t.ID & TileData.MaxItemValue];

                if (IsStaticFloor(t.ID) && map.CanFit(x, y, t.Z + (id.Surface ? id.CalcHeight : 0), 16, false, false))
                {
                    z = t.Z + (id.Surface ? id.CalcHeight : 0);
                    return true;
                }
            }

            z = 0;
            return false;
        }

        private static bool IsFloor(Map map, int x, int y, bool canFit)
        {
            LandTile lt = map.Tiles.GetLandTile(x, y);

            if (IsFloor(lt.ID) && (canFit || CanFit(map, x, y, lt.Z)))
                return true;

            StaticTile[] tiles = map.Tiles.GetStaticTiles(x, y);

            for (int i = 0; i < tiles.Length; ++i)
            {
                StaticTile t = tiles[i];
                ItemData id = TileData.ItemTable[t.ID & TileData.MaxItemValue];

                if (IsStaticFloor(t.ID) && (canFit || CanFit(map, x, y, t.Z + (id.Surface ? id.CalcHeight : 0))))
                    return true;
            }

            return false;
        }

        private static bool IsFloor(int itemID)
        {
            itemID &= TileData.MaxLandValue;

            return (itemID >= 0x406 && itemID <= 0x51A);
        }

        private static bool IsStaticFloor(int itemID)
        {
            return (itemID >= 0x495 && itemID <= 0x514)
                || (itemID >= 0x519 && itemID <= 0x53A);
        }

        private static bool IsDisplayCase(int itemID)
        {
            return (itemID >= 0xB00 && itemID <= 0xB02)
                || (itemID >= 0xB06 && itemID <= 0xB0A)
                || (itemID >= 0xB0D && itemID <= 0xB17);
        }

        private static void Process(Map map, Rectangle2D[] regions)
        {
            m_ShopTable = new Hashtable();
            m_ShopList = new ArrayList();

            World.Broadcast(0x35, true, "Generating vendor spawns for {0}, please wait.", map);

            for (int i = 0; i < regions.Length; ++i)
                for (int x = 0; x < map.Width; ++x)
                    for (int y = 0; y < map.Height; ++y)
                        CheckPoint(map, regions[i].X + x, regions[i].Y + y);

            for (int i = 0; i < m_ShopList.Count; ++i)
            {
                ShopInfo si = (ShopInfo)m_ShopList[i];

                int xTotal = 0;
                int yTotal = 0;

                bool hasSpawner = false;

                for (int j = 0; !hasSpawner && j < si.m_Floor.Count; ++j)
                {
                    Point2D fp = (Point2D)si.m_Floor[j];

                    xTotal += fp.X;
                    yTotal += fp.Y;

                    IPooledEnumerable eable = map.GetItemsInRange(new Point3D(fp.X, fp.Y, 0), 0);

                    foreach (Item item in eable)
                    {
                        if (item is Spawner)
                        {
                            hasSpawner = true;
                            break;
                        }
                    }

                    eable.Free();

                    if (hasSpawner)
                        break;
                }

                if (hasSpawner)
                    continue;

                int xAvg = xTotal / si.m_Floor.Count;
                int yAvg = yTotal / si.m_Floor.Count;

                ArrayList names = new ArrayList();
                ShopFlags flags = si.m_Flags;

                if ((flags & ShopFlags.Armor) != 0)
                    names.Add("armorer");

                if ((flags & ShopFlags.MetalWeapon) != 0)
                    names.Add("weaponsmith");

                if ((flags & ShopFlags.ArcheryWeapon) != 0)
                    names.Add("bowyer");

                if ((flags & ShopFlags.Scroll) != 0)
                    names.Add("mage");

                if ((flags & ShopFlags.Spellbook) != 0)
                    names.Add("mage");

                if ((flags & ShopFlags.Bread) != 0)
                    names.Add("baker");

                if ((flags & ShopFlags.Jewel) != 0)
                    names.Add("jeweler");

                if ((flags & ShopFlags.Potion) != 0)
                {
                    names.Add("herbalist");
                    names.Add("alchemist");
                    names.Add("mage");
                }

                if ((flags & ShopFlags.Reagent) != 0)
                {
                    names.Add("mage");
                    names.Add("herbalist");
                }

                if ((flags & ShopFlags.Clothes) != 0)
                {
                    names.Add("tailor");
                    names.Add("weaver");
                }

                for (int j = 0; j < names.Count; ++j)
                {
                    Point2D cp = Point2D.Zero;
                    int dist = 100000;
                    int tz;

                    for (int k = 0; k < si.m_Floor.Count; ++k)
                    {
                        Point2D fp = (Point2D)si.m_Floor[k];

                        int rx = fp.X - xAvg;
                        int ry = fp.Y - yAvg;
                        int fd = (int)Math.Sqrt(rx * rx + ry * ry);

                        if (fd > 0 && fd < 5)
                            fd -= Utility.Random(10);

                        if (fd < dist && GetFloorZ(map, fp.X, fp.Y, out tz))
                        {
                            dist = fd;
                            cp = fp;
                        }
                    }

                    if (cp == Point2D.Zero)
                        continue;

                    int z;

                    if (!GetFloorZ(map, cp.X, cp.Y, out z))
                        continue;

                    new Spawner(1, 1, 1, 0, 4, (string)names[j]).MoveToWorld(new Point3D(cp.X, cp.Y, z), map);
                }
            }

            World.Broadcast(0x35, true, "Generation complete. {0} spawners generated.", m_ShopList.Count);
        }

        private static void CheckPoint(Map map, int x, int y)
        {
            if (IsFloor(map, x, y, true))
                CheckFloor(map, x, y);
        }

        private static void CheckFloor(Map map, int x, int y)
        {
            StaticTile[] tiles = map.Tiles.GetStaticTiles(x, y);

            for (int i = 0; i < tiles.Length; ++i)
            {
                if (IsDisplayCase(tiles[i].ID))
                {
                    ProcessDisplayCase(map, tiles, x, y);
                    break;
                }
            }
        }

        [Flags]
        private enum ShopFlags
        {
            None = 0x000,
            Armor = 0x001,
            MetalWeapon = 0x002,
            Jewel = 0x004,
            Reagent = 0x008,
            Potion = 0x010,
            Bread = 0x020,
            Clothes = 0x040,
            ArcheryWeapon = 0x080,
            Scroll = 0x100,
            Spellbook = 0x200
        }

        private static bool IsClothes(int itemID)
        {
            if (itemID >= 0x1515 && itemID <= 0x1518)
                return true;

            if (itemID >= 0x152E && itemID <= 0x1531)
                return true;

            if (itemID >= 0x1537 && itemID <= 0x154C)
                return true;

            if (itemID >= 0x1EFD && itemID <= 0x1F04)
                return true;

            if (itemID >= 0x170B && itemID <= 0x171C)
                return true;

            return false;
        }

        private static bool IsArmor(int itemID)
        {
            if (itemID >= 0x13BB && itemID <= 0x13E2)
                return true;

            if (itemID >= 0x13E5 && itemID <= 0x13F2)
                return true;

            if (itemID >= 0x1408 && itemID <= 0x141A)
                return true;

            if (itemID >= 0x144E && itemID <= 0x1457)
                return true;

            return false;
        }

        private static bool IsMetalWeapon(int itemID)
        {
            if (itemID >= 0xF43 && itemID <= 0xF4E)
                return true;

            if (itemID >= 0xF51 && itemID <= 0xF52)
                return true;

            if (itemID >= 0xF5C && itemID <= 0xF63)
                return true;

            if (itemID >= 0x13AF && itemID <= 0x13B0)
                return true;

            if (itemID >= 0x13B5 && itemID <= 0x13BA)
                return true;

            if (itemID >= 0x13FA && itemID <= 0x13FB)
                return true;

            if (itemID >= 0x13FE && itemID <= 0x1407)
                return true;

            if (itemID >= 0x1438 && itemID <= 0x1443)
                return true;

            return false;
        }

        private static bool IsArcheryWeapon(int itemID)
        {
            if (itemID >= 0xF4F && itemID <= 0xF50)
                return true;

            if (itemID >= 0x13B1 && itemID <= 0x13B2)
                return true;

            if (itemID >= 0x13FC && itemID <= 0x13FD)
                return true;

            return false;
        }

        private static ShopFlags ProcessDisplayedItem(int itemID)
        {
            itemID &= TileData.MaxItemValue;

            ShopFlags res = ShopFlags.None;

            ItemData id = TileData.ItemTable[itemID];
            TileFlag flags = id.Flags;

            if ((flags & TileFlag.Wearable) != 0)
            {
                if (IsClothes(itemID))
                    res |= ShopFlags.Clothes;
                else if (IsArmor(itemID))
                    res |= ShopFlags.Armor;
                else if (IsMetalWeapon(itemID))
                    res |= ShopFlags.MetalWeapon;
                else if (IsArcheryWeapon(itemID))
                    res |= ShopFlags.ArcheryWeapon;
            }

            if (itemID == 0x98C || itemID == 0x103B || itemID == 0x103C)
                res |= ShopFlags.Bread;

            if (itemID >= 0xF0F && itemID <= 0xF30)
                res |= ShopFlags.Jewel;

            if (itemID >= 0xEFB && itemID <= 0xF0D)
                res |= ShopFlags.Potion;

            if (itemID >= 0xF78 && itemID <= 0xF91)
                res |= ShopFlags.Reagent;

            if ((itemID >= 0xE35 && itemID <= 0xE3A) || (itemID >= 0xEF4 && itemID <= 0xEF9) || (itemID >= 0x1F2D && itemID <= 0x1F72))
                res |= ShopFlags.Scroll;

            if (itemID == 0xE38 || itemID == 0xEFA)
                res |= ShopFlags.Spellbook;

            return res;
        }

        private static void ProcessDisplayCase(Map map, StaticTile[] tiles, int x, int y)
        {
            ShopFlags flags = ShopFlags.None;

            for (int i = 0; i < tiles.Length; ++i)
                flags |= ProcessDisplayedItem(tiles[i].ID);

            if (flags != ShopFlags.None)
            {
                Point2D p = new Point2D(x, y);
                ShopInfo si = (ShopInfo)m_ShopTable[p];

                if (si == null)
                {
                    ArrayList floor = new ArrayList();

                    RecurseFindFloor(map, x, y, floor);

                    if (floor.Count == 0)
                        return;

                    si = new ShopInfo();
                    si.m_Flags = flags;
                    si.m_Floor = floor;
                    m_ShopList.Add(si);

                    for (int i = 0; i < floor.Count; ++i)
                        m_ShopTable[(Point2D)floor[i]] = si;
                }
                else
                {
                    si.m_Flags |= flags;
                }
            }
        }

        private static Hashtable m_ShopTable;
        private static ArrayList m_ShopList;

        private class ShopInfo
        {
            public ShopFlags m_Flags;
            public ArrayList m_Floor;
        }

        private static bool CanFit(Map map, int x, int y, int z)
        {
            bool hasSurface = false;

            LandTile lt = map.Tiles.GetLandTile(x, y);
            int lowZ = 0, avgZ = 0, topZ = 0;

            map.GetAverageZ(x, y, ref lowZ, ref avgZ, ref topZ);
            TileFlag landFlags = TileData.LandTable[lt.ID & TileData.MaxLandValue].Flags;

            if ((landFlags & TileFlag.Impassable) != 0 && topZ > z && (z + 16) > lowZ)
                return false;
            else if ((landFlags & TileFlag.Impassable) == 0 && z == avgZ && !lt.Ignored)
                hasSurface = true;

            StaticTile[] staticTiles = map.Tiles.GetStaticTiles(x, y);

            bool surface, impassable;

            for (int i = 0; i < staticTiles.Length; ++i)
            {
                if (IsDisplayCase(staticTiles[i].ID))
                    continue;

                ItemData id = TileData.ItemTable[staticTiles[i].ID & TileData.MaxItemValue];

                surface = id.Surface;
                impassable = id.Impassable;

                if ((surface || impassable) && (staticTiles[i].Z + id.CalcHeight) > z && (z + 16) > staticTiles[i].Z)
                    return false;
                else if (surface && !impassable && z == (staticTiles[i].Z + id.CalcHeight))
                    hasSurface = true;
            }

            Sector sector = map.GetSector(x, y);
            List<Item> items = sector.Items;

            for (int i = 0; i < items.Count; ++i)
            {
                Item item = items[i];

                if (item.AtWorldPoint(x, y))
                {
                    ItemData id = item.ItemData;
                    surface = id.Surface;
                    impassable = id.Impassable;

                    if ((surface || impassable) && (item.Z + id.CalcHeight) > z && (z + 16) > item.Z)
                        return false;
                    else if (surface && !impassable && z == (item.Z + id.CalcHeight))
                        hasSurface = true;
                }
            }

            return hasSurface;
        }

        private static void RecurseFindFloor(Map map, int x, int y, ArrayList floor)
        {
            Point2D p = new Point2D(x, y);

            if (floor.Contains(p))
                return;

            floor.Add(p);

            for (int xo = -1; xo <= 1; ++xo)
            {
                for (int yo = -1; yo <= 1; ++yo)
                {
                    if ((xo != 0 || yo != 0) && IsFloor(map, x + xo, y + yo, false))
                        RecurseFindFloor(map, x + xo, y + yo, floor);
                }
            }
        }
    }

    public class UOAMVendorGenerator
    {
        private static int m_Count;

        //configuration
        private const int NPCCount = 2;//2 npcs per type (so a mage spawner will spawn 2 npcs, a alchemist and herbalist spawner will spawn 4 npcs total)
        private const int HomeRange = 5;//How far should they wander?
        private const bool TotalRespawn = true;//Should we spawn them up right away?
        private static TimeSpan MinTime = TimeSpan.FromMinutes(2.5);//min spawn time
        private static TimeSpan MaxTime = TimeSpan.FromMinutes(10.0);//max spawn time
        private const int Team = 0;//"team" the npcs are on

        public static void Initialize()
        {
            CommandSystem.Register("UOAMVendors", AccessLevel.Administrator, new CommandEventHandler(Generate_OnCommand));
        }

        [Usage("UOAMVendors")]
        [Description("Generates vendor spawners from Data/Common.MAP (taken from UOAutoMap)")]
        private static void Generate_OnCommand(CommandEventArgs e)
        {
            Parse(e.Mobile);
        }

        public static void Parse(Mobile from)
        {
            string vendor_path = Path.Combine(Core.BaseDirectory, "Data/Common.map");
            m_Count = 0;

            if (File.Exists(vendor_path))
            {
                ArrayList list = new ArrayList();
                from.SendMessage("Generating Vendors...");

                using (StreamReader ip = new StreamReader(vendor_path))
                {
                    string line;

                    while ((line = ip.ReadLine()) != null)
                    {
                        int indexOf = line.IndexOf(':');

                        if (indexOf == -1)
                            continue;

                        string type = line.Substring(0, ++indexOf).Trim();
                        string sub = line.Substring(indexOf).Trim();

                        string[] split = sub.Split(' ');

                        if (split.Length < 3)
                            continue;

                        split = new string[] { type, split[0], split[1], split[2] };

                        switch (split[0].ToLower())
                        {
                            case "-healer:":
                                PlaceNPC(split[1], split[2], split[3], "Healer", "HealerGuildmaster");
                                break;
                            case "-baker:":
                                PlaceNPC(split[1], split[2], split[3], "Baker");
                                break;
                            case "-vet:":
                                PlaceNPC(split[1], split[2], split[3], "Veterinarian");
                                break;
                            case "-gypsymaiden:":
                                PlaceNPC(split[1], split[2], split[3], "GypsyMaiden");
                                break;
                            case "-gypsybank:":
                                PlaceNPC(split[1], split[2], split[3], "GypsyBanker");
                                break;
                            case "-bank:":
                                PlaceNPC(split[1], split[2], split[3], "Banker", "Minter");
                                break;
                            case "-inn:":
                                PlaceNPC(split[1], split[2], split[3], "Innkeeper");
                                break;
                            case "-provisioner:":
                                PlaceNPC(split[1], split[2], split[3], "Provisioner", "Cobbler");
                                break;
                            case "-tailor:":
                                PlaceNPC(split[1], split[2], split[3], "Tailor", "Weaver", "TailorGuildmaster");
                                break;
                            case "-tavern:":
                                PlaceNPC(split[1], split[2], split[3], "Tavernkeeper", "Waiter", "Cook", "Barkeeper");
                                break;
                            case "-reagents:":
                                PlaceNPC(split[1], split[2], split[3], "Herbalist", "Alchemist", "CustomHairstylist");
                                break;
                            case "-fortuneteller:":
                                PlaceNPC(split[1], split[2], split[3], "FortuneTeller");
                                break;
                            case "-holymage:":
                                PlaceNPC(split[1], split[2], split[3], "HolyMage");
                                break;
                            case "-chivalrykeeper:":
                                PlaceNPC(split[1], split[2], split[3], "KeeperOfChivalry");
                                break;
                            case "-mage:":
                                PlaceNPC(split[1], split[2], split[3], "Mage", "Alchemist", "MageGuildmaster");
                                break;
                            case "-arms:":
                                PlaceNPC(split[1], split[2], split[3], "Armorer", "Weaponsmith");
                                break;
                            case "-tinker:":
                                PlaceNPC(split[1], split[2], split[3], "Tinker", "TinkerGuildmaster");
                                break;
                            case "-gypsystable:":
                                PlaceNPC(split[1], split[2], split[3], "GypsyAnimalTrainer");
                                break;
                            case "-stable:":
                                PlaceNPC(split[1], split[2], split[3], "AnimalTrainer");
                                break;
                            case "-blacksmith:":
                                PlaceNPC(split[1], split[2], split[3], "Blacksmith", "BlacksmithGuildmaster");
                                break;
                            case "-bowyer:":
                            case "-fletcher:":
                                PlaceNPC(split[1], split[2], split[3], "Bowyer");
                                break;
                            case "-carpenter:":
                                PlaceNPC(split[1], split[2], split[3], "Carpenter", "Architect", "RealEstateBroker");
                                break;
                            case "-butcher:":
                                PlaceNPC(split[1], split[2], split[3], "Butcher");
                                break;
                            case "-jeweler:":
                                PlaceNPC(split[1], split[2], split[3], "Jeweler");
                                break;
                            case "-tanner:":
                                PlaceNPC(split[1], split[2], split[3], "Tanner", "Furtrader");
                                break;
                            case "-bard:":
                                PlaceNPC(split[1], split[2], split[3], "Bard", "BardGuildmaster");
                                break;
                            case "-market:":
                                PlaceNPC(split[1], split[2], split[3], "Butcher", "Farmer");
                                break;
                            case "-library:":
                                PlaceNPC(split[1], split[2], split[3], "Scribe");
                                break;
                            case "-shipwright:":
                                PlaceNPC(split[1], split[2], split[3], "Shipwright", "Mapmaker");
                                break;
                            case "-docks:":
                                PlaceNPC(split[1], split[2], split[3], "Fisherman");
                                break;

                            case "-beekeeper:":
                                PlaceNPC(split[1], split[2], split[3], "Beekeeper");
                                break;


                            // Guilds & Misc
                            case "-tinkers guild:":
                                PlaceNPC(split[1], split[2], split[3], "TinkerGuildmaster");
                                break;
                            case "-blacksmiths guild:":
                                PlaceNPC(split[1], split[2], split[3], "BlacksmithGuildmaster");
                                break;
                            case "-sorcerors guild:":
                                PlaceNPC(split[1], split[2], split[3], "MageGuildmaster");
                                break;
                            case "-customs:": break;
                            case "-painter:": break;
                            case "-theater:": break;
                            case "-warriors guild:":
                                PlaceNPC(split[1], split[2], split[3], "WarriorGuildmaster");
                                break;
                            case "-archers guild:":
                                PlaceNPC(split[1], split[2], split[3], "RangerGuildmaster");
                                break;
                            case "-thieves guild:":
                                PlaceNPC(split[1], split[2], split[3], "ThiefGuildmaster");
                                break;
                            case "-miners guild:":
                                PlaceNPC(split[1], split[2], split[3], "MinerGuildmaster");
                                break;
                            case "-fishermans guild:":
                                PlaceNPC(split[1], split[2], split[3], "FisherGuildmaster");
                                break;
                            case "-merchants guild:":
                                PlaceNPC(split[1], split[2], split[3], "MerchantGuildmaster");
                                break;
                            case "-illusionists guild:": break;
                            case "-armourers guild:": break;
                            case "-sorcerers guild:": break;
                            case "-mages guild:":
                                PlaceNPC(split[1], split[2], split[3], "MageGuildmaster");
                                break;
                            case "-weapons guild:": break;
                            case "-bardic guild:":
                                PlaceNPC(split[1], split[2], split[3], "BardGuildmaster");
                                break;
                            case "-rogues guild:":
                                break;

                            // Skip
                            case "+landmark:":
                            case "-point of interest:":
                            case "+shrine:":
                            case "+moongate:":
                            case "+dungeon:":
                            case "+scenic:":
                            case "-gate:":
                            case "+Body of Water:":
                            case "+ruins:":
                            case "+teleporter:":
                            case "+Terrain:":
                            case "-exit:":
                            case "-bridge:":
                            case "-other:":
                            case "+champion:":
                            case "-stairs:":
                            case "-guild:":
                            case "+graveyard:":
                            case "+Island:":
                            case "+town:":
                                break;
                            /*default:
                                Console.WriteLine(split[0]);
                                break;*/
                        }
                    }
                }
                from.SendMessage("Done, added {0} spawners", m_Count);
            }
            else
            {
                from.SendMessage("{0} not found!", vendor_path);
            }
        }

        public static void PlaceNPC(string sx, string sy, string sm, params string[] types)
        {
            if (types.Length == 0)
                return;

            int x = Utility.ToInt32(sx);
            int y = Utility.ToInt32(sy);
            int map = Utility.ToInt32(sm);

            switch (map)
            {
                case 0://Trammel and Felucca
                    MakeSpawner(types, x, y, Map.Felucca);
                    MakeSpawner(types, x, y, Map.Trammel);
                    break;
                case 1://Felucca
                    MakeSpawner(types, x, y, Map.Felucca);
                    break;
                case 2:
                    MakeSpawner(types, x, y, Map.Trammel);
                    break;
                case 3:
                    MakeSpawner(types, x, y, Map.Ilshenar);
                    break;
                case 4:
                    MakeSpawner(types, x, y, Map.Malas);
                    break;
                default:
                    Console.WriteLine("UOAM Vendor Parser: Warning, unknown map {0}", map);
                    break;
            }
        }

        public static int GetSpawnerZ(int x, int y, Map map)
        {
            int z = map.GetAverageZ(x, y);

            if (map.CanFit(x, y, z, 16, false, false, true))
                return z;

            for (int i = 1; i <= 20; ++i)
            {
                if (map.CanFit(x, y, z + i, 16, false, false, true))
                    return z + i;

                if (map.CanFit(x, y, z - i, 16, false, false, true))
                    return z - i;
            }

            return z;
        }

        private static Queue m_ToDelete = new Queue();

        public static void ClearSpawners(int x, int y, int z, Map map)
        {
            IPooledEnumerable eable = map.GetItemsInRange(new Point3D(x, y, z), 0);

            foreach (Item item in eable)
            {
                if (item is Spawner && item.Z == z)
                    m_ToDelete.Enqueue(item);
            }

            eable.Free();

            while (m_ToDelete.Count > 0)
                ((Item)m_ToDelete.Dequeue()).Delete();
        }

        private static void MakeSpawner(string[] types, int x, int y, Map map)
        {
            if (types.Length == 0)
                return;

            int z = GetSpawnerZ(x, y, map);

            ClearSpawners(x, y, z, map);

            for (int i = 0; i < types.Length; ++i)
            {
                bool isGuildmaster = (types[i].EndsWith("Guildmaster"));

                Spawner sp = new Spawner(types[i]);

                if (isGuildmaster)
                    sp.Count = 1;
                else
                    sp.Count = NPCCount;

                sp.MinDelay = MinTime;
                sp.MaxDelay = MaxTime;
                sp.Team = Team;
                sp.HomeRange = HomeRange;

                sp.MoveToWorld(new Point3D(x, y, z), map);

                if (TotalRespawn)
                {
                    sp.Respawn();
                    sp.BringToHome();
                }

                ++m_Count;
            }
        }
    }
}