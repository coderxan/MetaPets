using System;
using System.Collections;
using System.Collections.Generic;

using Server;
using Server.Commands;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Regions;

/// <summary>
/// This BaseScript.cs contains any system engine which affects dungeons server-wide.
/// This includes, but is not limited to, dungeon spawners and hidden item engines that players wouldn't have access to.
/// </summary>

#region Reward Drop Spawner

namespace Server
{
    public static class MondainsLegacy
    {
        public static Type[] Artifacts { get { return m_Artifacts; } }

        private static Type[] m_Artifacts = new Type[]
		{
			typeof( AegisOfGrace ), typeof( BladeDance ), typeof( BloodwoodSpirit ), typeof( Bonesmasher ),
			typeof( Boomstick ), typeof( BrightsightLenses ), typeof( FeyLeggings ), typeof( FleshRipper ),
			typeof( HelmOfSwiftness ), typeof( PadsOfTheCuSidhe ), typeof( QuiverOfRage ), typeof( QuiverOfElements ),
			typeof( RaedsGlory ), typeof( RighteousAnger ), typeof( RobeOfTheEclipse ), typeof( RobeOfTheEquinox ),
			typeof( SoulSeeker ), typeof( TalonBite ), typeof( TotemOfVoid ), typeof( WildfireBow ),
			typeof( Windsong )
		};

        public static bool CheckArtifactChance(Mobile m, BaseCreature bc)
        {
            if (!Core.ML)
                return false;

            return Paragon.CheckArtifactChance(m, bc);
        }

        public static void GiveArtifactTo(Mobile m)
        {
            Item item = Activator.CreateInstance(m_Artifacts[Utility.Random(m_Artifacts.Length)]) as Item;

            if (item == null)
                return;

            if (m.AddToBackpack(item))
            {
                m.SendLocalizedMessage(1072223); // An item has been placed in your backpack.
                m.SendLocalizedMessage(1062317); // For your valor in combating the fallen beast, a special artifact has been bestowed on you.
            }
            else if (m.BankBox.TryDropItem(m, item, false))
            {
                m.SendLocalizedMessage(1072224); // An item has been placed in your bank box.
                m.SendLocalizedMessage(1062317); // For your valor in combating the fallen beast, a special artifact has been bestowed on you.
            }
            else
            {
                // Item was placed at feet by m.AddToBackpack
                m.SendLocalizedMessage(1072523); // You find an artifact, but your backpack and bank are too full to hold it.
            }
        }

        public static bool CheckML(Mobile from)
        {
            return CheckML(from, true);
        }

        public static bool CheckML(Mobile from, bool message)
        {
            if (from == null || from.NetState == null)
                return false;

            if (from.NetState.SupportsExpansion(Expansion.ML))
                return true;

            if (message)
                from.SendLocalizedMessage(1072791); // You must upgrade to Mondain's Legacy in order to use that item.

            return false;
        }

        public static bool IsMLRegion(Region region)
        {
            return region.IsPartOf("Twisted Weald")
                || region.IsPartOf("Sanctuary")
                || region.IsPartOf("The Prism of Light")
                || region.IsPartOf("The Citadel")
                || region.IsPartOf("Bedlam")
                || region.IsPartOf("Blighted Grove")
                || region.IsPartOf("The Painted Caves")
                || region.IsPartOf("The Palace of Paroxysmus")
                || region.IsPartOf("Labyrinth");
        }
    }
}

#endregion

#region Doom Dungeon Engine

namespace Server.Engines.Doom
{
    public enum GauntletSpawnerState
    {
        InSequence,
        InProgress,
        Completed
    }

    public class GauntletSpawner : Item
    {
        public const int PlayersPerSpawn = 5;

        public const int InSequenceItemHue = 0x000;
        public const int InProgressItemHue = 0x676;
        public const int CompletedItemHue = 0x455;

        private GauntletSpawnerState m_State;

        private string m_TypeName;
        private BaseDoor m_Door;
        private BaseAddon m_Addon;
        private GauntletSpawner m_Sequence;
        private List<Mobile> m_Creatures;

        private Rectangle2D m_RegionBounds;
        private List<BaseTrap> m_Traps;

        private Region m_Region;

        [CommandProperty(AccessLevel.GameMaster)]
        public string TypeName
        {
            get { return m_TypeName; }
            set { m_TypeName = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public BaseDoor Door
        {
            get { return m_Door; }
            set { m_Door = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public BaseAddon Addon
        {
            get { return m_Addon; }
            set { m_Addon = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public GauntletSpawner Sequence
        {
            get { return m_Sequence; }
            set { m_Sequence = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool HasCompleted
        {
            get
            {
                if (m_Creatures.Count == 0)
                    return false;

                for (int i = 0; i < m_Creatures.Count; ++i)
                {
                    Mobile mob = m_Creatures[i];

                    if (!mob.Deleted)
                        return false;
                }

                return true;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Rectangle2D RegionBounds
        {
            get { return m_RegionBounds; }
            set { m_RegionBounds = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public GauntletSpawnerState State
        {
            get { return m_State; }
            set
            {
                if (m_State == value)
                    return;

                m_State = value;

                int hue = 0;
                bool lockDoors = (m_State == GauntletSpawnerState.InProgress);

                switch (m_State)
                {
                    case GauntletSpawnerState.InSequence: hue = InSequenceItemHue; break;
                    case GauntletSpawnerState.InProgress: hue = InProgressItemHue; break;
                    case GauntletSpawnerState.Completed: hue = CompletedItemHue; break;
                }

                if (m_Door != null)
                {
                    m_Door.Hue = hue;
                    m_Door.Locked = lockDoors;

                    if (lockDoors)
                    {
                        m_Door.KeyValue = Key.RandomValue();
                        m_Door.Open = false;
                    }

                    if (m_Door.Link != null)
                    {
                        m_Door.Link.Hue = hue;
                        m_Door.Link.Locked = lockDoors;

                        if (lockDoors)
                        {
                            m_Door.Link.KeyValue = Key.RandomValue();
                            m_Door.Open = false;
                        }
                    }
                }

                if (m_Addon != null)
                    m_Addon.Hue = hue;

                if (m_State == GauntletSpawnerState.InProgress)
                {
                    CreateRegion();
                    FullSpawn();

                    m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0), new TimerCallback(Slice));
                }
                else
                {
                    ClearCreatures();
                    ClearTraps();
                    DestroyRegion();

                    if (m_Timer != null)
                        m_Timer.Stop();

                    m_Timer = null;
                }
            }
        }

        private Timer m_Timer;

        public List<Mobile> Creatures
        {
            get { return m_Creatures; }
            set { m_Creatures = value; }
        }

        public List<BaseTrap> Traps
        {
            get { return m_Traps; }
            set { m_Traps = value; }
        }

        public Region Region
        {
            get { return m_Region; }
            set { m_Region = value; }
        }

        public virtual void CreateRegion()
        {
            if (m_Region != null)
                return;

            Map map = this.Map;

            if (map == null || map == Map.Internal)
                return;

            m_Region = new GauntletRegion(this, map);
        }

        public virtual void DestroyRegion()
        {
            if (m_Region != null)
                m_Region.Unregister();

            m_Region = null;
        }

        public virtual int ComputeTrapCount()
        {
            int area = m_RegionBounds.Width * m_RegionBounds.Height;

            return area / 100;
        }

        public virtual void ClearTraps()
        {
            for (int i = 0; i < m_Traps.Count; ++i)
                m_Traps[i].Delete();

            m_Traps.Clear();
        }

        public virtual void SpawnTrap()
        {
            Map map = this.Map;

            if (map == null)
                return;

            BaseTrap trap = null;

            int random = Utility.Random(100);

            if (22 > random)
                trap = new SawTrap(Utility.RandomBool() ? SawTrapType.WestFloor : SawTrapType.NorthFloor);
            else if (44 > random)
                trap = new SpikeTrap(Utility.RandomBool() ? SpikeTrapType.WestFloor : SpikeTrapType.NorthFloor);
            else if (66 > random)
                trap = new GasTrap(Utility.RandomBool() ? GasTrapType.NorthWall : GasTrapType.WestWall);
            else if (88 > random)
                trap = new FireColumnTrap();
            else
                trap = new MushroomTrap();

            if (trap == null)
                return;

            if (trap is FireColumnTrap || trap is MushroomTrap)
                trap.Hue = 0x451;

            // try 10 times to find a valid location
            for (int i = 0; i < 10; ++i)
            {
                int x = Utility.Random(m_RegionBounds.X, m_RegionBounds.Width);
                int y = Utility.Random(m_RegionBounds.Y, m_RegionBounds.Height);
                int z = this.Z;

                if (!map.CanFit(x, y, z, 16, false, false))
                    z = map.GetAverageZ(x, y);

                if (!map.CanFit(x, y, z, 16, false, false))
                    continue;

                trap.MoveToWorld(new Point3D(x, y, z), map);
                m_Traps.Add(trap);

                return;
            }

            trap.Delete();
        }

        public virtual int ComputeSpawnCount()
        {
            int playerCount = 0;

            Map map = this.Map;

            if (map != null)
            {
                Point3D loc = GetWorldLocation();

                Region reg = Region.Find(loc, map).GetRegion("Doom Gauntlet");

                if (reg != null)
                    playerCount = reg.GetPlayerCount();
            }

            if (playerCount == 0 && m_Region != null)
                playerCount = m_Region.GetPlayerCount();

            int count = (playerCount + PlayersPerSpawn - 1) / PlayersPerSpawn;

            if (count < 1)
                count = 1;

            return count;
        }

        public virtual void ClearCreatures()
        {
            for (int i = 0; i < m_Creatures.Count; ++i)
                m_Creatures[i].Delete();

            m_Creatures.Clear();
        }

        public virtual void FullSpawn()
        {
            ClearCreatures();

            int count = ComputeSpawnCount();

            for (int i = 0; i < count; ++i)
                Spawn();

            ClearTraps();

            count = ComputeTrapCount();

            for (int i = 0; i < count; ++i)
                SpawnTrap();
        }

        public virtual void Spawn()
        {
            try
            {
                if (m_TypeName == null)
                    return;

                Type type = ScriptCompiler.FindTypeByName(m_TypeName, true);

                if (type == null)
                    return;

                object obj = Activator.CreateInstance(type);

                if (obj == null)
                    return;

                if (obj is Item)
                {
                    ((Item)obj).Delete();
                }
                else if (obj is Mobile)
                {
                    Mobile mob = (Mobile)obj;

                    mob.MoveToWorld(GetWorldLocation(), this.Map);

                    m_Creatures.Add(mob);
                }
            }
            catch
            {
            }
        }

        public virtual void RecurseReset()
        {
            if (m_State != GauntletSpawnerState.InSequence)
            {
                State = GauntletSpawnerState.InSequence;

                if (m_Sequence != null && !m_Sequence.Deleted)
                    m_Sequence.RecurseReset();
            }
        }

        public virtual void Slice()
        {
            if (m_State != GauntletSpawnerState.InProgress)
                return;

            int count = ComputeSpawnCount();

            for (int i = m_Creatures.Count; i < count; ++i)
                Spawn();

            if (HasCompleted)
            {
                State = GauntletSpawnerState.Completed;

                if (m_Sequence != null && !m_Sequence.Deleted)
                {
                    if (m_Sequence.State == GauntletSpawnerState.Completed)
                        RecurseReset();

                    m_Sequence.State = GauntletSpawnerState.InProgress;
                }
            }
        }

        public override string DefaultName
        {
            get { return "doom spawner"; }
        }

        [Constructable]
        public GauntletSpawner()
            : this(null)
        {
        }

        [Constructable]
        public GauntletSpawner(string typeName)
            : base(0x36FE)
        {
            Visible = false;
            Movable = false;

            m_TypeName = typeName;
            m_Creatures = new List<Mobile>();
            m_Traps = new List<BaseTrap>();
        }

        public GauntletSpawner(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version

            writer.Write(m_RegionBounds);

            writer.WriteItemList<BaseTrap>(m_Traps, false);

            writer.Write(m_Creatures, false);

            writer.Write(m_TypeName);
            writer.WriteItem<BaseDoor>(m_Door);
            writer.WriteItem<BaseAddon>(m_Addon);
            writer.WriteItem<GauntletSpawner>(m_Sequence);

            writer.Write((int)m_State);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                    {
                        m_RegionBounds = reader.ReadRect2D();
                        m_Traps = reader.ReadStrongItemList<BaseTrap>();

                        goto case 0;
                    }
                case 0:
                    {
                        if (version < 1)
                        {
                            m_Traps = new List<BaseTrap>();
                            m_RegionBounds = new Rectangle2D(X - 40, Y - 40, 80, 80);
                        }

                        m_Creatures = reader.ReadStrongMobileList();

                        m_TypeName = reader.ReadString();
                        m_Door = reader.ReadItem<BaseDoor>(); ;
                        m_Addon = reader.ReadItem<BaseAddon>(); ;
                        m_Sequence = reader.ReadItem<GauntletSpawner>();

                        State = (GauntletSpawnerState)reader.ReadInt();

                        break;
                    }
            }
        }

        public static void Initialize()
        {
            CommandSystem.Register("GenGauntlet", AccessLevel.Administrator, new CommandEventHandler(GenGauntlet_OnCommand));
        }

        public static void CreateTeleporter(int xFrom, int yFrom, int xTo, int yTo)
        {
            Static telePad = new Static(0x1822);
            Teleporter teleItem = new Teleporter(new Point3D(xTo, yTo, -1), Map.Malas, false);

            telePad.Hue = 0x482;
            telePad.MoveToWorld(new Point3D(xFrom, yFrom, -1), Map.Malas);

            teleItem.MoveToWorld(new Point3D(xFrom, yFrom, -1), Map.Malas);

            teleItem.SourceEffect = true;
            teleItem.DestEffect = true;
            teleItem.SoundID = 0x1FE;
        }

        public static BaseDoor CreateDoorSet(int xDoor, int yDoor, bool doorEastToWest, int hue)
        {
            BaseDoor hiDoor = new MetalDoor(doorEastToWest ? DoorFacing.NorthCCW : DoorFacing.WestCW);
            BaseDoor loDoor = new MetalDoor(doorEastToWest ? DoorFacing.SouthCW : DoorFacing.EastCCW);

            hiDoor.MoveToWorld(new Point3D(xDoor, yDoor, -1), Map.Malas);
            loDoor.MoveToWorld(new Point3D(xDoor + (doorEastToWest ? 0 : 1), yDoor + (doorEastToWest ? 1 : 0), -1), Map.Malas);

            hiDoor.Link = loDoor;
            loDoor.Link = hiDoor;

            hiDoor.Hue = hue;
            loDoor.Hue = hue;

            return hiDoor;
        }

        public static GauntletSpawner CreateSpawner(string typeName, int xSpawner, int ySpawner, int xDoor, int yDoor, int xPentagram, int yPentagram, bool doorEastToWest, int xStart, int yStart, int xWidth, int yHeight)
        {
            GauntletSpawner spawner = new GauntletSpawner(typeName);

            spawner.MoveToWorld(new Point3D(xSpawner, ySpawner, -1), Map.Malas);

            if (xDoor > 0 && yDoor > 0)
                spawner.Door = CreateDoorSet(xDoor, yDoor, doorEastToWest, 0);

            spawner.RegionBounds = new Rectangle2D(xStart, yStart, xWidth, yHeight);

            if (xPentagram > 0 && yPentagram > 0)
            {
                PentagramAddon pentagram = new PentagramAddon();

                pentagram.MoveToWorld(new Point3D(xPentagram, yPentagram, -1), Map.Malas);

                spawner.Addon = pentagram;
            }

            return spawner;
        }

        public static void CreatePricedHealer(int price, int x, int y)
        {
            PricedHealer healer = new PricedHealer(price);

            healer.MoveToWorld(new Point3D(x, y, -1), Map.Malas);

            healer.Home = healer.Location;
            healer.RangeHome = 5;
        }

        public static void CreateMorphItem(int x, int y, int inactiveItemID, int activeItemID, int range, int hue)
        {
            MorphItem item = new MorphItem(inactiveItemID, activeItemID, range);

            item.Hue = hue;
            item.MoveToWorld(new Point3D(x, y, -1), Map.Malas);
        }

        public static void CreateVarietyDealer(int x, int y)
        {
            VarietyDealer dealer = new VarietyDealer();

            /* Begin outfit */
            dealer.Name = "Nix";
            dealer.Title = "the Variety Dealer";

            dealer.Body = 400;
            dealer.Female = false;
            dealer.Hue = 0x8835;

            List<Item> items = new List<Item>(dealer.Items);

            for (int i = 0; i < items.Count; ++i)
            {
                Item item = items[i];

                if (item.Layer != Layer.ShopBuy && item.Layer != Layer.ShopResale && item.Layer != Layer.ShopSell)
                    item.Delete();
            }

            dealer.HairItemID = 0x2049; // Pig Tails
            dealer.HairHue = 0x482;

            dealer.FacialHairItemID = 0x203E;
            dealer.FacialHairHue = 0x482;

            dealer.AddItem(new FloppyHat(1));
            dealer.AddItem(new Robe(1));

            dealer.AddItem(new LanternOfSouls());

            dealer.AddItem(new Sandals(0x482));
            /* End outfit */

            dealer.MoveToWorld(new Point3D(x, y, -1), Map.Malas);

            dealer.Home = dealer.Location;
            dealer.RangeHome = 2;
        }

        public static void GenGauntlet_OnCommand(CommandEventArgs e)
        {
            /* Begin healer room */
            CreatePricedHealer(5000, 387, 400);
            CreateTeleporter(390, 407, 394, 405);

            BaseDoor healerDoor = CreateDoorSet(393, 404, true, 0x44E);

            healerDoor.Locked = true;
            healerDoor.KeyValue = Key.RandomValue();

            if (healerDoor.Link != null)
            {
                healerDoor.Link.Locked = true;
                healerDoor.Link.KeyValue = Key.RandomValue();
            }
            /* End healer room */

            /* Begin supply room */
            CreateMorphItem(433, 371, 0x29F, 0x116, 3, 0x44E);
            CreateMorphItem(433, 372, 0x29F, 0x115, 3, 0x44E);

            CreateVarietyDealer(492, 369);

            for (int x = 434; x <= 478; ++x)
            {
                for (int y = 371; y <= 372; ++y)
                {
                    Static item = new Static(0x524);

                    item.Hue = 1;
                    item.MoveToWorld(new Point3D(x, y, -1), Map.Malas);
                }
            }
            /* End supply room */

            /* Begin gauntlet cycle */
            CreateTeleporter(471, 428, 474, 428);
            CreateTeleporter(462, 494, 462, 498);
            CreateTeleporter(403, 502, 399, 506);
            CreateTeleporter(357, 476, 356, 480);
            CreateTeleporter(361, 433, 357, 434);

            GauntletSpawner sp1 = CreateSpawner("DarknightCreeper", 491, 456, 473, 432, 417, 426, true, 473, 412, 39, 60);
            GauntletSpawner sp2 = CreateSpawner("FleshRenderer", 482, 520, 468, 496, 426, 422, false, 448, 496, 56, 48);
            GauntletSpawner sp3 = CreateSpawner("Impaler", 406, 538, 408, 504, 432, 430, false, 376, 504, 64, 48);
            GauntletSpawner sp4 = CreateSpawner("ShadowKnight", 335, 512, 360, 478, 424, 439, false, 300, 478, 72, 64);
            GauntletSpawner sp5 = CreateSpawner("AbysmalHorror", 326, 433, 360, 429, 416, 435, true, 300, 408, 60, 56);
            GauntletSpawner sp6 = CreateSpawner("DemonKnight", 423, 430, 0, 0, 423, 430, true, 392, 392, 72, 96);

            sp1.Sequence = sp2;
            sp2.Sequence = sp3;
            sp3.Sequence = sp4;
            sp4.Sequence = sp5;
            sp5.Sequence = sp6;
            sp6.Sequence = sp1;

            sp1.State = GauntletSpawnerState.InProgress;
            /* End gauntlet cycle */

            /* Begin exit gate */
            ConfirmationMoongate gate = new ConfirmationMoongate();

            gate.Dispellable = false;

            gate.Target = new Point3D(2350, 1270, -85);
            gate.TargetMap = Map.Malas;

            gate.GumpWidth = 420;
            gate.GumpHeight = 280;

            gate.MessageColor = 0x7F00;
            gate.MessageNumber = 1062109; // You are about to exit Dungeon Doom.  Do you wish to continue?

            gate.TitleColor = 0x7800;
            gate.TitleNumber = 1062108; // Please verify...

            gate.Hue = 0x44E;

            gate.MoveToWorld(new Point3D(433, 326, 4), Map.Malas);
            /* End exit gate */
        }
    }

    public class GauntletRegion : BaseRegion
    {
        private GauntletSpawner m_Spawner;

        public GauntletRegion(GauntletSpawner spawner, Map map)
            : base(null, map, Region.Find(spawner.Location, spawner.Map), spawner.RegionBounds)
        {
            m_Spawner = spawner;

            GoLocation = spawner.Location;

            Register();
        }

        public override void AlterLightLevel(Mobile m, ref int global, ref int personal)
        {
            global = 12;
        }

        public override void OnEnter(Mobile m)
        {
        }

        public override void OnExit(Mobile m)
        {
        }
    }
}

#endregion

#region Generic Item Engine

namespace Server.Items
{
    public class MorphItem : Item
    {
        private int m_InactiveItemID;
        private int m_ActiveItemID;
        private int m_InRange;
        private int m_OutRange;

        [CommandProperty(AccessLevel.GameMaster)]
        public int InactiveItemID
        {
            get { return m_InactiveItemID; }
            set { m_InactiveItemID = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ActiveItemID
        {
            get { return m_ActiveItemID; }
            set { m_ActiveItemID = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int InRange
        {
            get { return m_InRange; }
            set { if (value > 18) value = 18; m_InRange = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int OutRange
        {
            get { return m_OutRange; }
            set { if (value > 18) value = 18; m_OutRange = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int CurrentRange { get { return ItemID == InactiveItemID ? InRange : OutRange; } }

        [Constructable]
        public MorphItem(int inactiveItemID, int activeItemID, int range)
            : this(inactiveItemID, activeItemID, range, range)
        {
        }

        [Constructable]
        public MorphItem(int inactiveItemID, int activeItemID, int inRange, int outRange)
            : base(inactiveItemID)
        {
            Movable = false;

            InactiveItemID = inactiveItemID;
            ActiveItemID = activeItemID;
            InRange = inRange;
            OutRange = outRange;
        }

        public MorphItem(Serial serial)
            : base(serial)
        {
        }

        public override bool HandlesOnMovement { get { return true; } }

        public override void OnMovement(Mobile m, Point3D oldLocation)
        {
            if (Utility.InRange(m.Location, Location, CurrentRange) || Utility.InRange(oldLocation, Location, CurrentRange))
                Refresh();
        }

        public override void OnMapChange()
        {
            if (!Deleted)
                Refresh();
        }

        public override void OnLocationChange(Point3D oldLoc)
        {
            if (!Deleted)
                Refresh();
        }

        public void Refresh()
        {
            bool found = false;

            foreach (Mobile mob in GetMobilesInRange(CurrentRange))
            {
                if (mob.Hidden && mob.AccessLevel > AccessLevel.Player)
                    continue;

                found = true;
                break;
            }

            if (found)
                ItemID = ActiveItemID;
            else
                ItemID = InactiveItemID;

            Visible = (ItemID != 0x1);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version

            writer.Write((int)m_OutRange);

            writer.Write((int)m_InactiveItemID);
            writer.Write((int)m_ActiveItemID);
            writer.Write((int)m_InRange);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                    {
                        m_OutRange = reader.ReadInt();
                        goto case 0;
                    }
                case 0:
                    {
                        m_InactiveItemID = reader.ReadInt();
                        m_ActiveItemID = reader.ReadInt();
                        m_InRange = reader.ReadInt();

                        if (version < 1)
                            m_OutRange = m_InRange;

                        break;
                    }
            }

            Timer.DelayCall(TimeSpan.Zero, new TimerCallback(Refresh));
        }
    }

    public class RaisableItem : Item
    {
        private int m_MaxElevation;
        private int m_MoveSound;
        private int m_StopSound;
        private TimeSpan m_CloseDelay;

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxElevation
        {
            get { return m_MaxElevation; }
            set
            {
                if (value <= 0)
                    m_MaxElevation = 0;
                else if (value >= 60)
                    m_MaxElevation = 60;
                else
                    m_MaxElevation = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MoveSound { get { return m_MoveSound; } set { m_MoveSound = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int StopSound { get { return m_StopSound; } set { m_StopSound = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan CloseDelay { get { return m_CloseDelay; } set { m_CloseDelay = value; } }

        [Constructable]
        public RaisableItem(int itemID)
            : this(itemID, 20, -1, -1, TimeSpan.FromMinutes(1.0))
        {
        }

        [Constructable]
        public RaisableItem(int itemID, int maxElevation, TimeSpan closeDelay)
            : this(itemID, maxElevation, -1, -1, closeDelay)
        {
        }

        [Constructable]
        public RaisableItem(int itemID, int maxElevation, int moveSound, int stopSound, TimeSpan closeDelay)
            : base(itemID)
        {
            Movable = false;

            m_MaxElevation = maxElevation;
            m_MoveSound = moveSound;
            m_StopSound = stopSound;
            m_CloseDelay = closeDelay;
        }

        private int m_Elevation;
        private RaiseTimer m_RaiseTimer;

        public bool IsRaisable
        {
            get { return m_RaiseTimer == null; }
        }

        public void Raise()
        {
            if (!IsRaisable)
                return;

            m_RaiseTimer = new RaiseTimer(this);
            m_RaiseTimer.Start();
        }

        private class RaiseTimer : Timer
        {
            private RaisableItem m_Item;
            private DateTime m_CloseTime;
            private bool m_Up;
            private int m_Step;

            public RaiseTimer(RaisableItem item)
                : base(TimeSpan.Zero, TimeSpan.FromSeconds(0.5))
            {
                m_Item = item;
                m_CloseTime = DateTime.UtcNow + item.CloseDelay;
                m_Up = true;

                Priority = TimerPriority.TenMS;
            }

            protected override void OnTick()
            {
                if (m_Item.Deleted)
                {
                    Stop();
                    return;
                }

                if (m_Step++ % 3 == 0)
                {
                    if (m_Up)
                    {
                        m_Item.Z++;

                        if (++m_Item.m_Elevation >= m_Item.MaxElevation)
                        {
                            Stop();

                            if (m_Item.StopSound >= 0)
                                Effects.PlaySound(m_Item.Location, m_Item.Map, m_Item.StopSound);

                            m_Up = false;
                            m_Step = 0;

                            TimeSpan delay = m_CloseTime - DateTime.UtcNow;
                            Timer.DelayCall(delay > TimeSpan.Zero ? delay : TimeSpan.Zero, new TimerCallback(Start));

                            return;
                        }
                    }
                    else
                    {
                        m_Item.Z--;

                        if (--m_Item.m_Elevation <= 0)
                        {
                            Stop();

                            if (m_Item.StopSound >= 0)
                                Effects.PlaySound(m_Item.Location, m_Item.Map, m_Item.StopSound);

                            m_Item.m_RaiseTimer = null;

                            return;
                        }
                    }
                }

                if (m_Item.MoveSound >= 0)
                    Effects.PlaySound(m_Item.Location, m_Item.Map, m_Item.MoveSound);
            }
        }

        public RaisableItem(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt((int)0); // version

            writer.WriteEncodedInt((int)m_MaxElevation);
            writer.WriteEncodedInt((int)m_MoveSound);
            writer.WriteEncodedInt((int)m_StopSound);
            writer.Write((TimeSpan)m_CloseDelay);

            writer.WriteEncodedInt((int)m_Elevation);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();

            m_MaxElevation = reader.ReadEncodedInt();
            m_MoveSound = reader.ReadEncodedInt();
            m_StopSound = reader.ReadEncodedInt();
            m_CloseDelay = reader.ReadTimeSpan();

            int elevation = reader.ReadEncodedInt();
            this.Z -= elevation;
        }
    }
}

#endregion