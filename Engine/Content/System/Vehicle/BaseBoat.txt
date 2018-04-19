using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Server;
using Server.Engines.CannedEvil;
using Server.Factions;
using Server.Gumps;
using Server.Items;
using Server.Movement;
using Server.Multis;
using Server.Network;
using Server.Prompts;
using Server.Regions;
using Server.Targeting;

namespace Server.Items
{
    #region Boat System Component

    /// <summary>
    /// Boat Hold
    /// </summary>
    public class Hold : Container
    {
        private BaseBoat m_Boat;

        public Hold(BaseBoat boat)
            : base(0x3EAE)
        {
            m_Boat = boat;
            Movable = false;
        }

        public Hold(Serial serial)
            : base(serial)
        {
        }

        public void SetFacing(Direction dir)
        {
            switch (dir)
            {
                case Direction.East: ItemID = 0x3E65; break;
                case Direction.West: ItemID = 0x3E93; break;
                case Direction.North: ItemID = 0x3EAE; break;
                case Direction.South: ItemID = 0x3EB9; break;
            }
        }

        public override bool OnDragDrop(Mobile from, Item item)
        {
            if (m_Boat == null || !m_Boat.Contains(from) || m_Boat.IsMoving)
                return false;

            return base.OnDragDrop(from, item);
        }

        public override bool OnDragDropInto(Mobile from, Item item, Point3D p)
        {
            if (m_Boat == null || !m_Boat.Contains(from) || m_Boat.IsMoving)
                return false;

            return base.OnDragDropInto(from, item, p);
        }

        public override bool CheckItemUse(Mobile from, Item item)
        {
            if (item != this && (m_Boat == null || !m_Boat.Contains(from) || m_Boat.IsMoving))
                return false;

            return base.CheckItemUse(from, item);
        }

        public override bool CheckLift(Mobile from, Item item, ref LRReason reject)
        {
            if (m_Boat == null || !m_Boat.Contains(from) || m_Boat.IsMoving)
                return false;

            return base.CheckLift(from, item, ref reject);
        }

        public override void OnAfterDelete()
        {
            if (m_Boat != null)
                m_Boat.Delete();
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (m_Boat == null || !m_Boat.Contains(from))
            {
                if (m_Boat.TillerMan != null)
                    m_Boat.TillerMan.Say(502490); // You must be on the ship to open the hold.
            }
            else if (m_Boat.IsMoving)
            {
                if (m_Boat.TillerMan != null)
                    m_Boat.TillerMan.Say(502491); // I can not open the hold while the ship is moving.
            }
            else
            {
                base.OnDoubleClick(from);
            }
        }

        public override bool IsDecoContainer
        {
            get { return false; }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);

            writer.Write(m_Boat);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_Boat = reader.ReadItem() as BaseBoat;

                        if (m_Boat == null || Parent != null)
                            Delete();

                        Movable = false;

                        break;
                    }
            }
        }
    }

    /// <summary>
    /// Boat Plank
    /// </summary>
    public enum PlankSide { Port, Starboard }

    public class Plank : Item, ILockable
    {
        private BaseBoat m_Boat;
        private PlankSide m_Side;
        private bool m_Locked;
        private uint m_KeyValue;

        private Timer m_CloseTimer;

        public Plank(BaseBoat boat, PlankSide side, uint keyValue)
            : base(0x3EB1 + (int)side)
        {
            m_Boat = boat;
            m_Side = side;
            m_KeyValue = keyValue;
            m_Locked = true;

            Movable = false;
        }

        public Plank(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);//version

            writer.Write(m_Boat);
            writer.Write((int)m_Side);
            writer.Write(m_Locked);
            writer.Write(m_KeyValue);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_Boat = reader.ReadItem() as BaseBoat;
                        m_Side = (PlankSide)reader.ReadInt();
                        m_Locked = reader.ReadBool();
                        m_KeyValue = reader.ReadUInt();

                        if (m_Boat == null)
                            Delete();

                        break;
                    }
            }

            if (IsOpen)
            {
                m_CloseTimer = new CloseTimer(this);
                m_CloseTimer.Start();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public BaseBoat Boat { get { return m_Boat; } set { m_Boat = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public PlankSide Side { get { return m_Side; } set { m_Side = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Locked { get { return m_Locked; } set { m_Locked = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public uint KeyValue { get { return m_KeyValue; } set { m_KeyValue = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsOpen { get { return (ItemID == 0x3ED5 || ItemID == 0x3ED4 || ItemID == 0x3E84 || ItemID == 0x3E89); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Starboard { get { return (m_Side == PlankSide.Starboard); } }

        public void SetFacing(Direction dir)
        {
            if (IsOpen)
            {
                switch (dir)
                {
                    case Direction.North: ItemID = Starboard ? 0x3ED4 : 0x3ED5; break;
                    case Direction.East: ItemID = Starboard ? 0x3E84 : 0x3E89; break;
                    case Direction.South: ItemID = Starboard ? 0x3ED5 : 0x3ED4; break;
                    case Direction.West: ItemID = Starboard ? 0x3E89 : 0x3E84; break;
                }
            }
            else
            {
                switch (dir)
                {
                    case Direction.North: ItemID = Starboard ? 0x3EB2 : 0x3EB1; break;
                    case Direction.East: ItemID = Starboard ? 0x3E85 : 0x3E8A; break;
                    case Direction.South: ItemID = Starboard ? 0x3EB1 : 0x3EB2; break;
                    case Direction.West: ItemID = Starboard ? 0x3E8A : 0x3E85; break;
                }
            }
        }

        public void Open()
        {
            if (IsOpen || Deleted)
                return;

            if (m_CloseTimer != null)
                m_CloseTimer.Stop();

            m_CloseTimer = new CloseTimer(this);
            m_CloseTimer.Start();

            switch (ItemID)
            {
                case 0x3EB1: ItemID = 0x3ED5; break;
                case 0x3E8A: ItemID = 0x3E89; break;
                case 0x3EB2: ItemID = 0x3ED4; break;
                case 0x3E85: ItemID = 0x3E84; break;
            }

            if (m_Boat != null)
                m_Boat.Refresh();
        }

        public override bool OnMoveOver(Mobile from)
        {
            if (IsOpen)
            {
                if (from is BaseFactionGuard)
                    return false;

                if ((from.Direction & Direction.Running) != 0 || (m_Boat != null && !m_Boat.Contains(from)))
                    return true;

                Map map = Map;

                if (map == null)
                    return false;

                int rx = 0, ry = 0;

                if (ItemID == 0x3ED4)
                    rx = 1;
                else if (ItemID == 0x3ED5)
                    rx = -1;
                else if (ItemID == 0x3E84)
                    ry = 1;
                else if (ItemID == 0x3E89)
                    ry = -1;

                for (int i = 1; i <= 6; ++i)
                {
                    int x = X + (i * rx);
                    int y = Y + (i * ry);
                    int z;

                    for (int j = -8; j <= 8; ++j)
                    {
                        z = from.Z + j;

                        if (map.CanFit(x, y, z, 16, false, false) && !Server.Spells.SpellHelper.CheckMulti(new Point3D(x, y, z), map) && !Region.Find(new Point3D(x, y, z), map).IsPartOf(typeof(Factions.StrongholdRegion)))
                        {
                            if (i == 1 && j >= -2 && j <= 2)
                                return true;

                            from.Location = new Point3D(x, y, z);
                            return false;
                        }
                    }

                    z = map.GetAverageZ(x, y);

                    if (map.CanFit(x, y, z, 16, false, false) && !Server.Spells.SpellHelper.CheckMulti(new Point3D(x, y, z), map) && !Region.Find(new Point3D(x, y, z), map).IsPartOf(typeof(Factions.StrongholdRegion)))
                    {
                        if (i == 1)
                            return true;

                        from.Location = new Point3D(x, y, z);
                        return false;
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CanClose()
        {
            Map map = Map;

            if (map == null || Deleted)
                return false;

            foreach (object o in this.GetObjectsInRange(0))
            {
                if (o != this)
                    return false;
            }

            return true;
        }

        public void Close()
        {
            if (!IsOpen || !CanClose() || Deleted)
                return;

            if (m_CloseTimer != null)
                m_CloseTimer.Stop();

            m_CloseTimer = null;

            switch (ItemID)
            {
                case 0x3ED5: ItemID = 0x3EB1; break;
                case 0x3E89: ItemID = 0x3E8A; break;
                case 0x3ED4: ItemID = 0x3EB2; break;
                case 0x3E84: ItemID = 0x3E85; break;
            }

            if (m_Boat != null)
                m_Boat.Refresh();
        }

        public override void OnDoubleClickDead(Mobile from)
        {
            OnDoubleClick(from);
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (m_Boat == null)
                return;

            if (from.InRange(GetWorldLocation(), 8))
            {
                if (m_Boat.Contains(from))
                {
                    if (IsOpen)
                        Close();
                    else
                        Open();
                }
                else
                {
                    if (!IsOpen)
                    {
                        if (!Locked)
                        {
                            Open();
                        }
                        else if (from.AccessLevel >= AccessLevel.GameMaster)
                        {
                            from.LocalOverheadMessage(Network.MessageType.Regular, 0x00, 502502); // That is locked but your godly powers allow access
                            Open();
                        }
                        else
                        {
                            from.LocalOverheadMessage(Network.MessageType.Regular, 0x00, 502503); // That is locked.
                        }
                    }
                    else if (!Locked)
                    {
                        from.Location = new Point3D(this.X, this.Y, this.Z + 3);
                    }
                    else if (from.AccessLevel >= AccessLevel.GameMaster)
                    {
                        from.LocalOverheadMessage(Network.MessageType.Regular, 0x00, 502502); // That is locked but your godly powers allow access
                        from.Location = new Point3D(this.X, this.Y, this.Z + 3);
                    }
                    else
                    {
                        from.LocalOverheadMessage(Network.MessageType.Regular, 0x00, 502503); // That is locked.
                    }
                }
            }
        }

        private class CloseTimer : Timer
        {
            private Plank m_Plank;

            public CloseTimer(Plank plank)
                : base(TimeSpan.FromSeconds(5.0), TimeSpan.FromSeconds(5.0))
            {
                m_Plank = plank;
                Priority = TimerPriority.OneSecond;
            }

            protected override void OnTick()
            {
                m_Plank.Close();
            }
        }
    }

    /// <summary>
    /// Boat Tillerman
    /// </summary>
    public class TillerMan : Item
    {
        private BaseBoat m_Boat;

        public TillerMan(BaseBoat boat)
            : base(0x3E4E)
        {
            m_Boat = boat;
            Movable = false;
        }

        public TillerMan(Serial serial)
            : base(serial)
        {
        }

        public void SetFacing(Direction dir)
        {
            switch (dir)
            {
                case Direction.South: ItemID = 0x3E4B; break;
                case Direction.North: ItemID = 0x3E4E; break;
                case Direction.West: ItemID = 0x3E50; break;
                case Direction.East: ItemID = 0x3E55; break;
            }
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(m_Boat.Status);
        }

        public void Say(int number)
        {
            PublicOverheadMessage(MessageType.Regular, 0x3B2, number);
        }

        public void Say(int number, string args)
        {
            PublicOverheadMessage(MessageType.Regular, 0x3B2, number, args);
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            if (m_Boat != null && m_Boat.ShipName != null)
                list.Add(1042884, m_Boat.ShipName); // the tiller man of the ~1_SHIP_NAME~
            else
                base.AddNameProperty(list);
        }

        public override void OnSingleClick(Mobile from)
        {
            if (m_Boat != null && m_Boat.ShipName != null)
                LabelTo(from, 1042884, m_Boat.ShipName); // the tiller man of the ~1_SHIP_NAME~
            else
                base.OnSingleClick(from);
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (m_Boat != null && m_Boat.Contains(from))
                m_Boat.BeginRename(from);
            else if (m_Boat != null)
                m_Boat.BeginDryDock(from);
        }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (dropped is MapItem && m_Boat != null && m_Boat.CanCommand(from) && m_Boat.Contains(from))
            {
                m_Boat.AssociateMap((MapItem)dropped);
            }

            return false;
        }

        public override void OnAfterDelete()
        {
            if (m_Boat != null)
                m_Boat.Delete();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);//version

            writer.Write(m_Boat);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_Boat = reader.ReadItem() as BaseBoat;

                        if (m_Boat == null)
                            Delete();

                        break;
                    }
            }
        }
    }

    #endregion
}

namespace Server.Misc
{
    public class Strandedness
    {
        private static Point2D[] m_Felucca = new Point2D[]
			{
				new Point2D( 2528, 3568 ), new Point2D( 2376, 3400 ), new Point2D( 2528, 3896 ),
				new Point2D( 2168, 3904 ), new Point2D( 1136, 3416 ), new Point2D( 1432, 3648 ),
				new Point2D( 1416, 4000 ), new Point2D( 4512, 3936 ), new Point2D( 4440, 3120 ),
				new Point2D( 4192, 3672 ), new Point2D( 4720, 3472 ), new Point2D( 3744, 2768 ),
				new Point2D( 3480, 2432 ), new Point2D( 3560, 2136 ), new Point2D( 3792, 2112 ),
				new Point2D( 2800, 2296 ), new Point2D( 2736, 2016 ), new Point2D( 4576, 1456 ),
				new Point2D( 4680, 1152 ), new Point2D( 4304, 1104 ), new Point2D( 4496, 984 ),
				new Point2D( 4248, 696 ), new Point2D( 4040, 616 ), new Point2D( 3896, 248 ),
				new Point2D( 4176, 384 ), new Point2D( 3672, 1104 ), new Point2D( 3520, 1152 ),
				new Point2D( 3720, 1360 ), new Point2D( 2184, 2152 ), new Point2D( 1952, 2088 ),
				new Point2D( 2056, 1936 ), new Point2D( 1720, 1992 ), new Point2D( 472, 2064 ),
				new Point2D( 656, 2096 ), new Point2D( 3008, 3592 ), new Point2D( 2784, 3472 ),
				new Point2D( 5456, 2400 ), new Point2D( 5976, 2424 ), new Point2D( 5328, 3112 ),
				new Point2D( 5792, 3152 ), new Point2D( 2120, 3616 ), new Point2D( 2136, 3128 ),
				new Point2D( 1632, 3528 ), new Point2D( 1328, 3160 ), new Point2D( 1072, 3136 ),
				new Point2D( 1128, 2976 ), new Point2D( 960, 2576 ), new Point2D( 752, 1832 ),
				new Point2D( 184, 1488 ), new Point2D( 592, 1440 ), new Point2D( 368, 1216 ),
				new Point2D( 232, 752 ), new Point2D( 696, 744 ), new Point2D( 304, 1000 ),
				new Point2D( 840, 376 ), new Point2D( 1192, 624 ), new Point2D( 1200, 192 ),
				new Point2D( 1512, 240 ), new Point2D( 1336, 456 ), new Point2D( 1536, 648 ),
				new Point2D( 1104, 952 ), new Point2D( 1864, 264 ), new Point2D( 2136, 200 ),
				new Point2D( 2160, 528 ), new Point2D( 1904, 512 ), new Point2D( 2240, 784 ),
				new Point2D( 2536, 776 ), new Point2D( 2488, 216 ), new Point2D( 2336, 72 ),
				new Point2D( 2648, 288 ), new Point2D( 2680, 576 ), new Point2D( 2896, 88 ),
				new Point2D( 2840, 344 ), new Point2D( 3136, 72 ), new Point2D( 2968, 520 ),
				new Point2D( 3192, 328 ), new Point2D( 3448, 208 ), new Point2D( 3432, 608 ),
				new Point2D( 3184, 752 ), new Point2D( 2800, 704 ), new Point2D( 2768, 1016 ),
				new Point2D( 2448, 1232 ), new Point2D( 2272, 920 ), new Point2D( 2072, 1080 ),
				new Point2D( 2048, 1264 ), new Point2D( 1808, 1528 ), new Point2D( 1496, 1880 ),
				new Point2D( 1656, 2168 ), new Point2D( 2096, 2320 ), new Point2D( 1816, 2528 ),
				new Point2D( 1840, 2640 ), new Point2D( 1928, 2952 ), new Point2D( 2120, 2712 )
			};

        private static Point2D[] m_Trammel = m_Felucca;

        private static Point2D[] m_Ilshenar = new Point2D[]
			{
				new Point2D( 1252, 1180 ), new Point2D( 1562, 1090 ), new Point2D( 1444, 1016 ),
				new Point2D( 1324, 968 ), new Point2D( 1418, 806 ), new Point2D( 1722, 874 ),
				new Point2D( 1456, 684 ), new Point2D( 1036, 866 ), new Point2D( 612, 476 ),
				new Point2D( 1476, 372 ), new Point2D( 762, 472 ), new Point2D( 812, 1162 ),
				new Point2D( 1422, 1144 ), new Point2D( 1254, 1066 ), new Point2D( 1598, 870 ),
				new Point2D( 1358, 866 ), new Point2D( 510, 302 ), new Point2D( 510, 392 )
			};

        private static Point2D[] m_Tokuno = new Point2D[]
			{
				//Makoto-Jima
				new Point2D( 837, 1351 ), new Point2D( 941, 1241 ), new Point2D( 959, 1185 ),
				new Point2D( 923, 1091 ), new Point2D( 904, 983 ), new Point2D( 845, 944 ),
				new Point2D( 829, 896 ), new Point2D( 794, 852 ), new Point2D( 766, 821 ),
				new Point2D( 695, 814 ), new Point2D( 576, 835 ), new Point2D( 518, 840 ),
				new Point2D( 519, 902 ), new Point2D( 502, 950 ), new Point2D( 503, 1045 ),
				new Point2D( 547, 1131 ), new Point2D( 518, 1204 ), new Point2D( 506, 1243 ),
				new Point2D( 526, 1271 ), new Point2D( 562, 1295 ), new Point2D( 616, 1335 ),
				new Point2D( 789, 1347 ), new Point2D( 712, 1359 ),

				//Homare-Jima
				new Point2D( 202, 498 ), new Point2D( 116, 600 ), new Point2D( 107, 699 ),
				new Point2D( 162, 799 ), new Point2D( 158, 889 ), new Point2D( 169, 989 ),
				new Point2D( 194, 1101 ), new Point2D( 250, 1163 ), new Point2D( 295, 1176 ),
				new Point2D( 280, 1194 ), new Point2D( 286, 1102 ), new Point2D( 250, 1000 ),
				new Point2D( 260, 906 ), new Point2D( 360, 838 ), new Point2D( 389, 763 ),
				new Point2D( 415, 662 ), new Point2D( 500, 597 ), new Point2D( 570, 572 ),
				new Point2D( 631, 577 ), new Point2D( 692, 500 ), new Point2D( 723, 445 ),
				new Point2D( 672, 379 ), new Point2D( 626, 332 ), new Point2D( 494, 291 ),
				new Point2D( 371, 336 ), new Point2D( 324, 334 ), new Point2D( 270, 362 ),

				//Isamu-Jima
				new Point2D( 1240, 1076 ), new Point2D( 1189, 1115 ), new Point2D( 1046, 1039 ),
				new Point2D( 1025, 885 ), new Point2D( 907, 809 ), new Point2D( 840, 506 ),
				new Point2D( 799, 396 ), new Point2D( 720, 258 ), new Point2D( 744, 158 ),
				new Point2D( 904, 37 ), new Point2D( 974, 91 ), new Point2D( 1020, 187 ),
				new Point2D( 1035, 288 ), new Point2D( 1104, 395 ), new Point2D( 1215, 462 ),
				new Point2D( 1275, 488 ), new Point2D( 1348, 611 ), new Point2D( 1363, 739 ),
				new Point2D( 1364, 765 ), new Point2D( 1364, 876 ), new Point2D( 1300, 936 ),
				new Point2D( 1240, 1003 )


			};

        public static void Initialize()
        {
            EventSink.Login += new LoginEventHandler(EventSink_Login);
        }

        private static bool IsStranded(Mobile from)
        {
            Map map = from.Map;

            if (map == null)
                return false;

            object surface = map.GetTopSurface(from.Location);

            if (surface is LandTile)
            {
                int id = ((LandTile)surface).ID;

                return (id >= 168 && id <= 171)
                    || (id >= 310 && id <= 311);
            }
            else if (surface is StaticTile)
            {
                int id = ((StaticTile)surface).ID;

                return (id >= 0x1796 && id <= 0x17B2);
            }

            return false;
        }

        public static void EventSink_Login(LoginEventArgs e)
        {
            Mobile from = e.Mobile;

            if (!IsStranded(from))
                return;

            Map map = from.Map;

            Point2D[] list;

            if (map == Map.Felucca)
                list = m_Felucca;
            else if (map == Map.Trammel)
                list = m_Trammel;
            else if (map == Map.Ilshenar)
                list = m_Ilshenar;
            else if (map == Map.Tokuno)
                list = m_Tokuno;
            else
                return;

            Point2D p = Point2D.Zero;
            double pdist = double.MaxValue;

            for (int i = 0; i < list.Length; ++i)
            {
                double dist = from.GetDistanceToSqrt(list[i]);

                if (dist < pdist)
                {
                    p = list[i];
                    pdist = dist;
                }
            }

            int x = p.X, y = p.Y;
            int z;
            bool canFit = false;

            z = map.GetAverageZ(x, y);
            canFit = map.CanSpawnMobile(x, y, z);

            for (int i = 1; !canFit && i <= 40; i += 2)
            {
                for (int xo = -1; !canFit && xo <= 1; ++xo)
                {
                    for (int yo = -1; !canFit && yo <= 1; ++yo)
                    {
                        if (xo == 0 && yo == 0)
                            continue;

                        x = p.X + (xo * i);
                        y = p.Y + (yo * i);
                        z = map.GetAverageZ(x, y);
                        canFit = map.CanSpawnMobile(x, y, z);
                    }
                }
            }

            if (canFit)
                from.Location = new Point3D(x, y, z);
        }
    }
}

namespace Server.Multis
{
    public enum BoatOrder
    {
        Move,
        Course,
        Single
    }

    public abstract class BaseBoat : BaseMulti
    {
        private static Rectangle2D[] m_BritWrap = new Rectangle2D[] { new Rectangle2D(16, 16, 5120 - 32, 4096 - 32), new Rectangle2D(5136, 2320, 992, 1760) };
        private static Rectangle2D[] m_IlshWrap = new Rectangle2D[] { new Rectangle2D(16, 16, 2304 - 32, 1600 - 32) };
        private static Rectangle2D[] m_TokunoWrap = new Rectangle2D[] { new Rectangle2D(16, 16, 1448 - 32, 1448 - 32) };

        private static TimeSpan BoatDecayDelay = TimeSpan.FromDays(9.0);

        public static BaseBoat FindBoatAt(IPoint2D loc, Map map)
        {
            Sector sector = map.GetSector(loc);

            for (int i = 0; i < sector.Multis.Count; i++)
            {
                BaseBoat boat = sector.Multis[i] as BaseBoat;

                if (boat != null && boat.Contains(loc.X, loc.Y))
                    return boat;
            }

            return null;
        }

        private Hold m_Hold;
        private TillerMan m_TillerMan;
        private Mobile m_Owner;

        private Direction m_Facing;

        private Direction m_Moving;
        private int m_Speed;
        private int m_ClientSpeed;

        private bool m_Anchored;
        private string m_ShipName;

        private BoatOrder m_Order;

        private MapItem m_MapItem;
        private int m_NextNavPoint;

        private Plank m_PPlank, m_SPlank;

        private DateTime m_DecayTime;

        private Timer m_TurnTimer;
        private Timer m_MoveTimer;

        [CommandProperty(AccessLevel.GameMaster)]
        public Hold Hold { get { return m_Hold; } set { m_Hold = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public TillerMan TillerMan { get { return m_TillerMan; } set { m_TillerMan = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public Plank PPlank { get { return m_PPlank; } set { m_PPlank = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public Plank SPlank { get { return m_SPlank; } set { m_SPlank = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Owner { get { return m_Owner; } set { m_Owner = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public Direction Facing { get { return m_Facing; } set { SetFacing(value); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public Direction Moving { get { return m_Moving; } set { m_Moving = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsMoving { get { return (m_MoveTimer != null); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Speed { get { return m_Speed; } set { m_Speed = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Anchored { get { return m_Anchored; } set { m_Anchored = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ShipName { get { return m_ShipName; } set { m_ShipName = value; if (m_TillerMan != null) m_TillerMan.InvalidateProperties(); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public BoatOrder Order { get { return m_Order; } set { m_Order = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public MapItem MapItem { get { return m_MapItem; } set { m_MapItem = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int NextNavPoint { get { return m_NextNavPoint; } set { m_NextNavPoint = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime TimeOfDecay { get { return m_DecayTime; } set { m_DecayTime = value; if (m_TillerMan != null) m_TillerMan.InvalidateProperties(); } }

        public int Status
        {
            get
            {
                DateTime start = TimeOfDecay - BoatDecayDelay;

                if (DateTime.UtcNow - start < TimeSpan.FromHours(1.0))
                    return 1043010; // This structure is like new.

                if (DateTime.UtcNow - start < TimeSpan.FromDays(2.0))
                    return 1043011; // This structure is slightly worn.

                if (DateTime.UtcNow - start < TimeSpan.FromDays(3.0))
                    return 1043012; // This structure is somewhat worn.

                if (DateTime.UtcNow - start < TimeSpan.FromDays(4.0))
                    return 1043013; // This structure is fairly worn.

                if (DateTime.UtcNow - start < TimeSpan.FromDays(5.0))
                    return 1043014; // This structure is greatly worn.

                return 1043015; // This structure is in danger of collapsing.
            }
        }

        public virtual int NorthID { get { return 0; } }
        public virtual int EastID { get { return 0; } }
        public virtual int SouthID { get { return 0; } }
        public virtual int WestID { get { return 0; } }

        public virtual int HoldDistance { get { return 0; } }
        public virtual int TillerManDistance { get { return 0; } }
        public virtual Point2D StarboardOffset { get { return Point2D.Zero; } }
        public virtual Point2D PortOffset { get { return Point2D.Zero; } }
        public virtual Point3D MarkOffset { get { return Point3D.Zero; } }

        public virtual BaseDockedBoat DockedBoat { get { return null; } }

        private static List<BaseBoat> m_Instances = new List<BaseBoat>();

        public static List<BaseBoat> Boats { get { return m_Instances; } }

        public BaseBoat()
            : base(0x0)
        {
            m_DecayTime = DateTime.UtcNow + BoatDecayDelay;

            m_TillerMan = new TillerMan(this);
            m_Hold = new Hold(this);

            m_PPlank = new Plank(this, PlankSide.Port, 0);
            m_SPlank = new Plank(this, PlankSide.Starboard, 0);

            m_PPlank.MoveToWorld(new Point3D(X + PortOffset.X, Y + PortOffset.Y, Z), Map);
            m_SPlank.MoveToWorld(new Point3D(X + StarboardOffset.X, Y + StarboardOffset.Y, Z), Map);

            Facing = Direction.North;

            m_NextNavPoint = -1;

            Movable = false;

            m_Instances.Add(this);
        }

        public BaseBoat(Serial serial)
            : base(serial)
        {
        }

        public Point3D GetRotatedLocation(int x, int y)
        {
            Point3D p = new Point3D(X + x, Y + y, Z);

            return Rotate(p, (int)m_Facing / 2);
        }

        public void UpdateComponents()
        {
            if (m_PPlank != null)
            {
                m_PPlank.MoveToWorld(GetRotatedLocation(PortOffset.X, PortOffset.Y), Map);
                m_PPlank.SetFacing(m_Facing);
            }

            if (m_SPlank != null)
            {
                m_SPlank.MoveToWorld(GetRotatedLocation(StarboardOffset.X, StarboardOffset.Y), Map);
                m_SPlank.SetFacing(m_Facing);
            }

            int xOffset = 0, yOffset = 0;
            Movement.Movement.Offset(m_Facing, ref xOffset, ref yOffset);

            if (m_TillerMan != null)
            {
                m_TillerMan.Location = new Point3D(X + (xOffset * TillerManDistance) + (m_Facing == Direction.North ? 1 : 0), Y + (yOffset * TillerManDistance), m_TillerMan.Z);
                m_TillerMan.SetFacing(m_Facing);
                m_TillerMan.InvalidateProperties();
            }

            if (m_Hold != null)
            {
                m_Hold.Location = new Point3D(X + (xOffset * HoldDistance), Y + (yOffset * HoldDistance), m_Hold.Z);
                m_Hold.SetFacing(m_Facing);
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)3);

            writer.Write((Item)m_MapItem);
            writer.Write((int)m_NextNavPoint);

            writer.Write((int)m_Facing);

            writer.WriteDeltaTime(m_DecayTime);

            writer.Write(m_Owner);
            writer.Write(m_PPlank);
            writer.Write(m_SPlank);
            writer.Write(m_TillerMan);
            writer.Write(m_Hold);
            writer.Write(m_Anchored);
            writer.Write(m_ShipName);

            CheckDecay();
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 3:
                    {
                        m_MapItem = (MapItem)reader.ReadItem();
                        m_NextNavPoint = reader.ReadInt();

                        goto case 2;
                    }
                case 2:
                    {
                        m_Facing = (Direction)reader.ReadInt();

                        goto case 1;
                    }
                case 1:
                    {
                        m_DecayTime = reader.ReadDeltaTime();

                        goto case 0;
                    }
                case 0:
                    {
                        if (version < 3)
                            m_NextNavPoint = -1;

                        if (version < 2)
                        {
                            if (ItemID == NorthID)
                                m_Facing = Direction.North;
                            else if (ItemID == SouthID)
                                m_Facing = Direction.South;
                            else if (ItemID == EastID)
                                m_Facing = Direction.East;
                            else if (ItemID == WestID)
                                m_Facing = Direction.West;
                        }

                        m_Owner = reader.ReadMobile();
                        m_PPlank = reader.ReadItem() as Plank;
                        m_SPlank = reader.ReadItem() as Plank;
                        m_TillerMan = reader.ReadItem() as TillerMan;
                        m_Hold = reader.ReadItem() as Hold;
                        m_Anchored = reader.ReadBool();
                        m_ShipName = reader.ReadString();

                        if (version < 1)
                            Refresh();

                        break;
                    }
            }

            m_Instances.Add(this);
        }

        public void RemoveKeys(Mobile m)
        {
            uint keyValue = 0;

            if (m_PPlank != null)
                keyValue = m_PPlank.KeyValue;

            if (keyValue == 0 && m_SPlank != null)
                keyValue = m_SPlank.KeyValue;

            Key.RemoveKeys(m, keyValue);
        }

        public uint CreateKeys(Mobile m)
        {
            uint value = Key.RandomValue();

            Key packKey = new Key(KeyType.Gold, value, this);
            Key bankKey = new Key(KeyType.Gold, value, this);

            packKey.MaxRange = 10;
            bankKey.MaxRange = 10;

            packKey.Name = "a ship key";
            bankKey.Name = "a ship key";

            BankBox box = m.BankBox;

            if (!box.TryDropItem(m, bankKey, false))
                bankKey.Delete();
            else
                m.LocalOverheadMessage(MessageType.Regular, 0x3B2, 502484); // A ship's key is now in my safety deposit box.

            if (m.AddToBackpack(packKey))
                m.LocalOverheadMessage(MessageType.Regular, 0x3B2, 502485); // A ship's key is now in my backpack.
            else
                m.LocalOverheadMessage(MessageType.Regular, 0x3B2, 502483); // A ship's key is now at my feet.

            return value;
        }

        public override void OnAfterDelete()
        {
            if (m_TillerMan != null)
                m_TillerMan.Delete();

            if (m_Hold != null)
                m_Hold.Delete();

            if (m_PPlank != null)
                m_PPlank.Delete();

            if (m_SPlank != null)
                m_SPlank.Delete();

            if (m_TurnTimer != null)
                m_TurnTimer.Stop();

            if (m_MoveTimer != null)
                m_MoveTimer.Stop();

            m_Instances.Remove(this);
        }

        public override void OnLocationChange(Point3D old)
        {
            if (m_TillerMan != null)
                m_TillerMan.Location = new Point3D(X + (m_TillerMan.X - old.X), Y + (m_TillerMan.Y - old.Y), Z + (m_TillerMan.Z - old.Z));

            if (m_Hold != null)
                m_Hold.Location = new Point3D(X + (m_Hold.X - old.X), Y + (m_Hold.Y - old.Y), Z + (m_Hold.Z - old.Z));

            if (m_PPlank != null)
                m_PPlank.Location = new Point3D(X + (m_PPlank.X - old.X), Y + (m_PPlank.Y - old.Y), Z + (m_PPlank.Z - old.Z));

            if (m_SPlank != null)
                m_SPlank.Location = new Point3D(X + (m_SPlank.X - old.X), Y + (m_SPlank.Y - old.Y), Z + (m_SPlank.Z - old.Z));
        }

        public override void OnMapChange()
        {
            if (m_TillerMan != null)
                m_TillerMan.Map = Map;

            if (m_Hold != null)
                m_Hold.Map = Map;

            if (m_PPlank != null)
                m_PPlank.Map = Map;

            if (m_SPlank != null)
                m_SPlank.Map = Map;
        }

        public bool CanCommand(Mobile m)
        {
            return true;
        }

        public Point3D GetMarkedLocation()
        {
            Point3D p = new Point3D(X + MarkOffset.X, Y + MarkOffset.Y, Z + MarkOffset.Z);

            return Rotate(p, (int)m_Facing / 2);
        }

        public bool CheckKey(uint keyValue)
        {
            if (m_SPlank != null && m_SPlank.KeyValue == keyValue)
                return true;

            if (m_PPlank != null && m_PPlank.KeyValue == keyValue)
                return true;

            return false;
        }

        /*
         * Intervals:
         *       drift forward
         * fast | 0.25|   0.25
         * slow | 0.50|   0.50
         *
         * Speed:
         *       drift forward
         * fast |  0x4|    0x4
         * slow |  0x3|    0x3
         *
         * Tiles (per interval):
         *       drift forward
         * fast |    1|      1
         * slow |    1|      1
         *
         * 'walking' in piloting mode has a 1s interval, speed 0x2
         */

        private static bool NewBoatMovement { get { return Core.HS; } }

        private static TimeSpan SlowInterval = TimeSpan.FromSeconds(NewBoatMovement ? 0.50 : 0.75);
        private static TimeSpan FastInterval = TimeSpan.FromSeconds(NewBoatMovement ? 0.25 : 0.75);

        private static int SlowSpeed = 1;
        private static int FastSpeed = NewBoatMovement ? 1 : 3;

        private static TimeSpan SlowDriftInterval = TimeSpan.FromSeconds(NewBoatMovement ? 0.50 : 1.50);
        private static TimeSpan FastDriftInterval = TimeSpan.FromSeconds(NewBoatMovement ? 0.25 : 0.75);

        private static int SlowDriftSpeed = 1;
        private static int FastDriftSpeed = 1;

        private static Direction Forward = Direction.North;
        private static Direction ForwardLeft = Direction.Up;
        private static Direction ForwardRight = Direction.Right;
        private static Direction Backward = Direction.South;
        private static Direction BackwardLeft = Direction.Left;
        private static Direction BackwardRight = Direction.Down;
        private static Direction Left = Direction.West;
        private static Direction Right = Direction.East;
        private static Direction Port = Left;
        private static Direction Starboard = Right;

        private bool m_Decaying;

        public void Refresh()
        {
            m_DecayTime = DateTime.UtcNow + BoatDecayDelay;

            if (m_TillerMan != null)
                m_TillerMan.InvalidateProperties();
        }

        private class DecayTimer : Timer
        {
            private BaseBoat m_Boat;
            private int m_Count;

            public DecayTimer(BaseBoat boat)
                : base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(5.0))
            {
                m_Boat = boat;

                Priority = TimerPriority.TwoFiftyMS;
            }

            protected override void OnTick()
            {
                if (m_Count == 5)
                {
                    m_Boat.Delete();
                    Stop();
                }
                else
                {
                    m_Boat.Location = new Point3D(m_Boat.X, m_Boat.Y, m_Boat.Z - 1);

                    if (m_Boat.TillerMan != null)
                        m_Boat.TillerMan.Say(1007168 + m_Count);

                    ++m_Count;
                }
            }
        }

        public bool CheckDecay()
        {
            if (m_Decaying)
                return true;

            if (!IsMoving && DateTime.UtcNow >= m_DecayTime)
            {
                new DecayTimer(this).Start();

                m_Decaying = true;

                return true;
            }

            return false;
        }

        public bool LowerAnchor(bool message)
        {
            if (CheckDecay())
                return false;

            if (m_Anchored)
            {
                if (message && m_TillerMan != null)
                    m_TillerMan.Say(501445); // Ar, the anchor was already dropped sir.

                return false;
            }

            StopMove(false);

            m_Anchored = true;

            if (message && m_TillerMan != null)
                m_TillerMan.Say(501444); // Ar, anchor dropped sir.

            return true;
        }

        public bool RaiseAnchor(bool message)
        {
            if (CheckDecay())
                return false;

            if (!m_Anchored)
            {
                if (message && m_TillerMan != null)
                    m_TillerMan.Say(501447); // Ar, the anchor has not been dropped sir.

                return false;
            }

            m_Anchored = false;

            if (message && m_TillerMan != null)
                m_TillerMan.Say(501446); // Ar, anchor raised sir.

            return true;
        }

        public bool StartMove(Direction dir, bool fast)
        {
            if (CheckDecay())
                return false;

            bool drift = (dir != Forward && dir != ForwardLeft && dir != ForwardRight);
            TimeSpan interval = (fast ? (drift ? FastDriftInterval : FastInterval) : (drift ? SlowDriftInterval : SlowInterval));
            int speed = (fast ? (drift ? FastDriftSpeed : FastSpeed) : (drift ? SlowDriftSpeed : SlowSpeed));
            int clientSpeed = fast ? 0x4 : 0x3;

            if (StartMove(dir, speed, clientSpeed, interval, false, true))
            {
                if (m_TillerMan != null)
                    m_TillerMan.Say(501429); // Aye aye sir.

                return true;
            }

            return false;
        }

        public bool OneMove(Direction dir)
        {
            if (CheckDecay())
                return false;

            bool drift = (dir != Forward);
            TimeSpan interval = drift ? FastDriftInterval : FastInterval;
            int speed = drift ? FastDriftSpeed : FastSpeed;

            if (StartMove(dir, speed, 0x1, interval, true, true))
            {
                if (m_TillerMan != null)
                    m_TillerMan.Say(501429); // Aye aye sir.

                return true;
            }

            return false;
        }

        public void BeginRename(Mobile from)
        {
            if (CheckDecay())
                return;

            if (from.AccessLevel < AccessLevel.GameMaster && from != m_Owner)
            {
                if (m_TillerMan != null)
                    m_TillerMan.Say(Utility.Random(1042876, 4)); // Arr, don't do that! | Arr, leave me alone! | Arr, watch what thour'rt doing, matey! | Arr! Do that again and I’ll throw ye overhead!

                return;
            }

            if (m_TillerMan != null)
                m_TillerMan.Say(502580); // What dost thou wish to name thy ship?

            from.Prompt = new RenameBoatPrompt(this);
        }

        public void EndRename(Mobile from, string newName)
        {
            if (Deleted || CheckDecay())
                return;

            if (from.AccessLevel < AccessLevel.GameMaster && from != m_Owner)
            {
                if (m_TillerMan != null)
                    m_TillerMan.Say(1042880); // Arr! Only the owner of the ship may change its name!

                return;
            }
            else if (!from.Alive)
            {
                if (m_TillerMan != null)
                    m_TillerMan.Say(502582); // You appear to be dead.

                return;
            }

            newName = newName.Trim();

            if (newName.Length == 0)
                newName = null;

            Rename(newName);
        }

        public enum DryDockResult { Valid, Dead, NoKey, NotAnchored, Mobiles, Items, Hold, Decaying }

        public DryDockResult CheckDryDock(Mobile from)
        {
            if (CheckDecay())
                return DryDockResult.Decaying;

            if (!from.Alive)
                return DryDockResult.Dead;

            Container pack = from.Backpack;
            if ((m_SPlank == null || !Key.ContainsKey(pack, m_SPlank.KeyValue)) && (m_PPlank == null || !Key.ContainsKey(pack, m_PPlank.KeyValue)))
                return DryDockResult.NoKey;

            if (!m_Anchored)
                return DryDockResult.NotAnchored;

            if (m_Hold != null && m_Hold.Items.Count > 0)
                return DryDockResult.Hold;

            Map map = Map;

            if (map == null || map == Map.Internal)
                return DryDockResult.Items;

            List<IEntity> ents = GetMovingEntities();

            if (ents.Count >= 1)
                return (ents[0] is Mobile) ? DryDockResult.Mobiles : DryDockResult.Items;

            return DryDockResult.Valid;
        }

        public void BeginDryDock(Mobile from)
        {
            if (CheckDecay())
                return;

            DryDockResult result = CheckDryDock(from);

            if (result == DryDockResult.Dead)
                from.SendLocalizedMessage(502493); // You appear to be dead.
            else if (result == DryDockResult.NoKey)
                from.SendLocalizedMessage(502494); // You must have a key to the ship to dock the boat.
            else if (result == DryDockResult.NotAnchored)
                from.SendLocalizedMessage(1010570); // You must lower the anchor to dock the boat.
            else if (result == DryDockResult.Mobiles)
                from.SendLocalizedMessage(502495); // You cannot dock the ship with beings on board!
            else if (result == DryDockResult.Items)
                from.SendLocalizedMessage(502496); // You cannot dock the ship with a cluttered deck.
            else if (result == DryDockResult.Hold)
                from.SendLocalizedMessage(502497); // Make sure your hold is empty, and try again!
            else if (result == DryDockResult.Valid)
                from.SendGump(new ConfirmDryDockGump(from, this));
        }

        public void EndDryDock(Mobile from)
        {
            if (Deleted || CheckDecay())
                return;

            DryDockResult result = CheckDryDock(from);

            if (result == DryDockResult.Dead)
                from.SendLocalizedMessage(502493); // You appear to be dead.
            else if (result == DryDockResult.NoKey)
                from.SendLocalizedMessage(502494); // You must have a key to the ship to dock the boat.
            else if (result == DryDockResult.NotAnchored)
                from.SendLocalizedMessage(1010570); // You must lower the anchor to dock the boat.
            else if (result == DryDockResult.Mobiles)
                from.SendLocalizedMessage(502495); // You cannot dock the ship with beings on board!
            else if (result == DryDockResult.Items)
                from.SendLocalizedMessage(502496); // You cannot dock the ship with a cluttered deck.
            else if (result == DryDockResult.Hold)
                from.SendLocalizedMessage(502497); // Make sure your hold is empty, and try again!

            if (result != DryDockResult.Valid)
                return;

            BaseDockedBoat boat = DockedBoat;

            if (boat == null)
                return;

            RemoveKeys(from);

            from.AddToBackpack(boat);
            Delete();
        }

        public void SetName(SpeechEventArgs e)
        {
            if (CheckDecay())
                return;

            if (e.Mobile.AccessLevel < AccessLevel.GameMaster && e.Mobile != m_Owner)
            {
                if (m_TillerMan != null)
                    m_TillerMan.Say(1042880); // Arr! Only the owner of the ship may change its name!

                return;
            }
            else if (!e.Mobile.Alive)
            {
                if (m_TillerMan != null)
                    m_TillerMan.Say(502582); // You appear to be dead.

                return;
            }

            if (e.Speech.Length > 8)
            {
                string newName = e.Speech.Substring(8).Trim();

                if (newName.Length == 0)
                    newName = null;

                Rename(newName);
            }
        }

        public void Rename(string newName)
        {
            if (CheckDecay())
                return;

            if (newName != null && newName.Length > 40)
                newName = newName.Substring(0, 40);

            if (m_ShipName == newName)
            {
                if (m_TillerMan != null)
                    m_TillerMan.Say(502531); // Yes, sir.

                return;
            }

            ShipName = newName;

            if (m_TillerMan != null && m_ShipName != null)
                m_TillerMan.Say(1042885, m_ShipName); // This ship is now called the ~1_NEW_SHIP_NAME~.
            else if (m_TillerMan != null)
                m_TillerMan.Say(502534); // This ship now has no name.
        }

        public void RemoveName(Mobile m)
        {
            if (CheckDecay())
                return;

            if (m.AccessLevel < AccessLevel.GameMaster && m != m_Owner)
            {
                if (m_TillerMan != null)
                    m_TillerMan.Say(1042880); // Arr! Only the owner of the ship may change its name!

                return;
            }
            else if (!m.Alive)
            {
                if (m_TillerMan != null)
                    m_TillerMan.Say(502582); // You appear to be dead.

                return;
            }

            if (m_ShipName == null)
            {
                if (m_TillerMan != null)
                    m_TillerMan.Say(502526); // Ar, this ship has no name.

                return;
            }

            ShipName = null;

            if (m_TillerMan != null)
                m_TillerMan.Say(502534); // This ship now has no name.
        }

        public void GiveName(Mobile m)
        {
            if (m_TillerMan == null || CheckDecay())
                return;

            if (m_ShipName == null)
                m_TillerMan.Say(502526); // Ar, this ship has no name.
            else
                m_TillerMan.Say(1042881, m_ShipName); // This is the ~1_BOAT_NAME~.
        }

        public void GiveNavPoint()
        {
            if (TillerMan == null || CheckDecay())
                return;

            if (NextNavPoint < 0)
                TillerMan.Say(1042882); // I have no current nav point.
            else
                TillerMan.Say(1042883, (NextNavPoint + 1).ToString()); // My current destination navpoint is nav ~1_NAV_POINT_NUM~.
        }

        public void AssociateMap(MapItem map)
        {
            if (CheckDecay())
                return;

            if (map is BlankMap)
            {
                if (TillerMan != null)
                    TillerMan.Say(502575); // Ar, that is not a map, tis but a blank piece of paper!
            }
            else if (map.Pins.Count == 0)
            {
                if (TillerMan != null)
                    TillerMan.Say(502576); // Arrrr, this map has no course on it!
            }
            else
            {
                StopMove(false);

                MapItem = map;
                NextNavPoint = -1;

                if (TillerMan != null)
                    TillerMan.Say(502577); // A map!
            }
        }

        public bool StartCourse(string navPoint, bool single, bool message)
        {
            int number = -1;

            int start = -1;
            for (int i = 0; i < navPoint.Length; i++)
            {
                if (Char.IsDigit(navPoint[i]))
                {
                    start = i;
                    break;
                }
            }

            if (start != -1)
            {
                string sNumber = navPoint.Substring(start);

                if (!int.TryParse(sNumber, out number))
                    number = -1;

                if (number != -1)
                {
                    number--;

                    if (MapItem == null || number < 0 || number >= MapItem.Pins.Count)
                    {
                        number = -1;
                    }
                }
            }

            if (number == -1)
            {
                if (message && TillerMan != null)
                    TillerMan.Say(1042551); // I don't see that navpoint, sir.

                return false;
            }

            NextNavPoint = number;
            return StartCourse(single, message);
        }

        public bool StartCourse(bool single, bool message)
        {
            if (CheckDecay())
                return false;

            if (Anchored)
            {
                if (message && TillerMan != null)
                    TillerMan.Say(501419); // Ar, the anchor is down sir!

                return false;
            }
            else if (MapItem == null || MapItem.Deleted)
            {
                if (message && TillerMan != null)
                    TillerMan.Say(502513); // I have seen no map, sir.

                return false;
            }
            else if (this.Map != MapItem.Map || !this.Contains(MapItem.GetWorldLocation()))
            {
                if (message && TillerMan != null)
                    TillerMan.Say(502514); // The map is too far away from me, sir.

                return false;
            }
            else if ((this.Map != Map.Trammel && this.Map != Map.Felucca) || NextNavPoint < 0 || NextNavPoint >= MapItem.Pins.Count)
            {
                if (message && TillerMan != null)
                    TillerMan.Say(1042551); // I don't see that navpoint, sir.

                return false;
            }

            Speed = FastSpeed;
            Order = single ? BoatOrder.Single : BoatOrder.Course;

            if (m_MoveTimer != null)
                m_MoveTimer.Stop();

            m_MoveTimer = new MoveTimer(this, FastInterval, false);
            m_MoveTimer.Start();

            if (message && TillerMan != null)
                TillerMan.Say(501429); // Aye aye sir.

            return true;
        }

        public override bool HandlesOnSpeech { get { return true; } }

        public override void OnSpeech(SpeechEventArgs e)
        {
            if (CheckDecay())
                return;

            Mobile from = e.Mobile;

            if (CanCommand(from) && Contains(from))
            {
                for (int i = 0; i < e.Keywords.Length; ++i)
                {
                    int keyword = e.Keywords[i];

                    if (keyword >= 0x42 && keyword <= 0x6B)
                    {
                        switch (keyword)
                        {
                            case 0x42: SetName(e); break;
                            case 0x43: RemoveName(e.Mobile); break;
                            case 0x44: GiveName(e.Mobile); break;
                            case 0x45: StartMove(Forward, true); break;
                            case 0x46: StartMove(Backward, true); break;
                            case 0x47: StartMove(Left, true); break;
                            case 0x48: StartMove(Right, true); break;
                            case 0x4B: StartMove(ForwardLeft, true); break;
                            case 0x4C: StartMove(ForwardRight, true); break;
                            case 0x4D: StartMove(BackwardLeft, true); break;
                            case 0x4E: StartMove(BackwardRight, true); break;
                            case 0x4F: StopMove(true); break;
                            case 0x50: StartMove(Left, false); break;
                            case 0x51: StartMove(Right, false); break;
                            case 0x52: StartMove(Forward, false); break;
                            case 0x53: StartMove(Backward, false); break;
                            case 0x54: StartMove(ForwardLeft, false); break;
                            case 0x55: StartMove(ForwardRight, false); break;
                            case 0x56: StartMove(BackwardRight, false); break;
                            case 0x57: StartMove(BackwardLeft, false); break;
                            case 0x58: OneMove(Left); break;
                            case 0x59: OneMove(Right); break;
                            case 0x5A: OneMove(Forward); break;
                            case 0x5B: OneMove(Backward); break;
                            case 0x5C: OneMove(ForwardLeft); break;
                            case 0x5D: OneMove(ForwardRight); break;
                            case 0x5E: OneMove(BackwardRight); break;
                            case 0x5F: OneMove(BackwardLeft); break;
                            case 0x49:
                            case 0x65: StartTurn(2, true); break; // turn right
                            case 0x4A:
                            case 0x66: StartTurn(-2, true); break; // turn left
                            case 0x67: StartTurn(-4, true); break; // turn around, come about
                            case 0x68: StartMove(Forward, true); break;
                            case 0x69: StopMove(true); break;
                            case 0x6A: LowerAnchor(true); break;
                            case 0x6B: RaiseAnchor(true); break;
                            case 0x60: GiveNavPoint(); break; // nav
                            case 0x61: NextNavPoint = 0; StartCourse(false, true); break; // start
                            case 0x62: StartCourse(false, true); break; // continue
                            case 0x63: StartCourse(e.Speech, false, true); break; // goto*
                            case 0x64: StartCourse(e.Speech, true, true); break; // single*
                        }

                        break;
                    }
                }
            }
        }

        public bool StartTurn(int offset, bool message)
        {
            if (CheckDecay())
                return false;

            if (m_Anchored)
            {
                if (message)
                    m_TillerMan.Say(501419); // Ar, the anchor is down sir!

                return false;
            }
            else
            {
                if (m_MoveTimer != null && this.Order != BoatOrder.Move)
                {
                    m_MoveTimer.Stop();
                    m_MoveTimer = null;
                }

                if (m_TurnTimer != null)
                    m_TurnTimer.Stop();

                m_TurnTimer = new TurnTimer(this, offset);
                m_TurnTimer.Start();

                if (message && TillerMan != null)
                    TillerMan.Say(501429); // Aye aye sir.

                return true;
            }
        }

        public bool Turn(int offset, bool message)
        {
            if (m_TurnTimer != null)
            {
                m_TurnTimer.Stop();
                m_TurnTimer = null;
            }

            if (CheckDecay())
                return false;

            if (m_Anchored)
            {
                if (message)
                    m_TillerMan.Say(501419); // Ar, the anchor is down sir!

                return false;
            }
            else if (SetFacing((Direction)(((int)m_Facing + offset) & 0x7)))
            {
                return true;
            }
            else
            {
                if (message)
                    m_TillerMan.Say(501423); // Ar, can't turn sir.

                return false;
            }
        }

        private class TurnTimer : Timer
        {
            private BaseBoat m_Boat;
            private int m_Offset;

            public TurnTimer(BaseBoat boat, int offset)
                : base(TimeSpan.FromSeconds(0.5))
            {
                m_Boat = boat;
                m_Offset = offset;

                Priority = TimerPriority.TenMS;
            }

            protected override void OnTick()
            {
                if (!m_Boat.Deleted)
                    m_Boat.Turn(m_Offset, true);
            }
        }

        public bool StartMove(Direction dir, int speed, int clientSpeed, TimeSpan interval, bool single, bool message)
        {
            if (CheckDecay())
                return false;

            if (m_Anchored)
            {
                if (message && m_TillerMan != null)
                    m_TillerMan.Say(501419); // Ar, the anchor is down sir!

                return false;
            }

            m_Moving = dir;
            m_Speed = speed;
            m_ClientSpeed = clientSpeed;
            m_Order = BoatOrder.Move;

            if (m_MoveTimer != null)
                m_MoveTimer.Stop();

            m_MoveTimer = new MoveTimer(this, interval, single);
            m_MoveTimer.Start();

            return true;
        }

        public bool StopMove(bool message)
        {
            if (CheckDecay())
                return false;

            if (m_MoveTimer == null)
            {
                if (message && m_TillerMan != null)
                    m_TillerMan.Say(501443); // Er, the ship is not moving sir.

                return false;
            }

            m_Moving = Direction.North;
            m_Speed = 0;
            m_ClientSpeed = 0;
            m_MoveTimer.Stop();
            m_MoveTimer = null;

            if (message && m_TillerMan != null)
                m_TillerMan.Say(501429); // Aye aye sir.

            return true;
        }

        public bool CanFit(Point3D p, Map map, int itemID)
        {
            if (map == null || map == Map.Internal || Deleted || CheckDecay())
                return false;

            MultiComponentList newComponents = MultiData.GetComponents(itemID);

            for (int x = 0; x < newComponents.Width; ++x)
            {
                for (int y = 0; y < newComponents.Height; ++y)
                {
                    int tx = p.X + newComponents.Min.X + x;
                    int ty = p.Y + newComponents.Min.Y + y;

                    if (newComponents.Tiles[x][y].Length == 0 || Contains(tx, ty))
                        continue;

                    LandTile landTile = map.Tiles.GetLandTile(tx, ty);
                    StaticTile[] tiles = map.Tiles.GetStaticTiles(tx, ty, true);

                    bool hasWater = false;

                    if (landTile.Z == p.Z && ((landTile.ID >= 168 && landTile.ID <= 171) || (landTile.ID >= 310 && landTile.ID <= 311)))
                        hasWater = true;

                    int z = p.Z;

                    //int landZ = 0, landAvg = 0, landTop = 0;

                    //map.GetAverageZ( tx, ty, ref landZ, ref landAvg, ref landTop );

                    //if ( !landTile.Ignored && top > landZ && landTop > z )
                    //	return false;

                    for (int i = 0; i < tiles.Length; ++i)
                    {
                        StaticTile tile = tiles[i];
                        bool isWater = (tile.ID >= 0x1796 && tile.ID <= 0x17B2);

                        if (tile.Z == p.Z && isWater)
                            hasWater = true;
                        else if (tile.Z >= p.Z && !isWater)
                            return false;
                    }

                    if (!hasWater)
                        return false;
                }
            }

            IPooledEnumerable eable = map.GetItemsInBounds(new Rectangle2D(p.X + newComponents.Min.X, p.Y + newComponents.Min.Y, newComponents.Width, newComponents.Height));

            foreach (Item item in eable)
            {
                if (item is BaseMulti || item.ItemID > TileData.MaxItemValue || item.Z < p.Z || !item.Visible)
                    continue;

                int x = item.X - p.X + newComponents.Min.X;
                int y = item.Y - p.Y + newComponents.Min.Y;

                if (x >= 0 && x < newComponents.Width && y >= 0 && y < newComponents.Height && newComponents.Tiles[x][y].Length == 0)
                    continue;
                else if (Contains(item))
                    continue;

                eable.Free();
                return false;
            }

            eable.Free();

            return true;
        }

        public Point3D Rotate(Point3D p, int count)
        {
            int rx = p.X - Location.X;
            int ry = p.Y - Location.Y;

            for (int i = 0; i < count; ++i)
            {
                int temp = rx;
                rx = -ry;
                ry = temp;
            }

            return new Point3D(Location.X + rx, Location.Y + ry, p.Z);
        }

        public override bool Contains(int x, int y)
        {
            if (base.Contains(x, y))
                return true;

            if (m_TillerMan != null && x == m_TillerMan.X && y == m_TillerMan.Y)
                return true;

            if (m_Hold != null && x == m_Hold.X && y == m_Hold.Y)
                return true;

            if (m_PPlank != null && x == m_PPlank.X && y == m_PPlank.Y)
                return true;

            if (m_SPlank != null && x == m_SPlank.X && y == m_SPlank.Y)
                return true;

            return false;
        }

        public static bool IsValidLocation(Point3D p, Map map)
        {
            Rectangle2D[] wrap = GetWrapFor(map);

            for (int i = 0; i < wrap.Length; ++i)
            {
                if (wrap[i].Contains(p))
                    return true;
            }

            return false;
        }

        public static Rectangle2D[] GetWrapFor(Map m)
        {
            if (m == Map.Ilshenar)
                return m_IlshWrap;
            else if (m == Map.Tokuno)
                return m_TokunoWrap;
            else
                return m_BritWrap;
        }

        public Direction GetMovementFor(int x, int y, out int maxSpeed)
        {
            int dx = x - this.X;
            int dy = y - this.Y;

            int adx = Math.Abs(dx);
            int ady = Math.Abs(dy);

            Direction dir = Utility.GetDirection(this, new Point2D(x, y));
            int iDir = (int)dir;

            // Compute the maximum distance we can travel without going too far away
            if (iDir % 2 == 0) // North, East, South and West
                maxSpeed = Math.Abs(adx - ady);
            else // Right, Down, Left and Up
                maxSpeed = Math.Min(adx, ady);

            return (Direction)((iDir - (int)Facing) & 0x7);
        }

        public bool DoMovement(bool message)
        {
            Direction dir;
            int speed, clientSpeed;

            if (this.Order == BoatOrder.Move)
            {
                dir = m_Moving;
                speed = m_Speed;
                clientSpeed = m_ClientSpeed;
            }
            else if (MapItem == null || MapItem.Deleted)
            {
                if (message && TillerMan != null)
                    TillerMan.Say(502513); // I have seen no map, sir.

                return false;
            }
            else if (this.Map != MapItem.Map || !this.Contains(MapItem.GetWorldLocation()))
            {
                if (message && TillerMan != null)
                    TillerMan.Say(502514); // The map is too far away from me, sir.

                return false;
            }
            else if ((this.Map != Map.Trammel && this.Map != Map.Felucca) || NextNavPoint < 0 || NextNavPoint >= MapItem.Pins.Count)
            {
                if (message && TillerMan != null)
                    TillerMan.Say(1042551); // I don't see that navpoint, sir.

                return false;
            }
            else
            {
                Point2D dest = (Point2D)MapItem.Pins[NextNavPoint];

                int x, y;
                MapItem.ConvertToWorld(dest.X, dest.Y, out x, out y);

                int maxSpeed;
                dir = GetMovementFor(x, y, out maxSpeed);

                if (maxSpeed == 0)
                {
                    if (message && this.Order == BoatOrder.Single && TillerMan != null)
                        TillerMan.Say(1042874, (NextNavPoint + 1).ToString()); // We have arrived at nav point ~1_POINT_NUM~ , sir.

                    if (NextNavPoint + 1 < MapItem.Pins.Count)
                    {
                        NextNavPoint++;

                        if (this.Order == BoatOrder.Course)
                        {
                            if (message && TillerMan != null)
                                TillerMan.Say(1042875, (NextNavPoint + 1).ToString()); // Heading to nav point ~1_POINT_NUM~, sir.

                            return true;
                        }

                        return false;
                    }
                    else
                    {
                        NextNavPoint = -1;

                        if (message && this.Order == BoatOrder.Course && TillerMan != null)
                            TillerMan.Say(502515); // The course is completed, sir.

                        return false;
                    }
                }

                if (dir == Left || dir == BackwardLeft || dir == Backward)
                    return Turn(-2, true);
                else if (dir == Right || dir == BackwardRight)
                    return Turn(2, true);

                speed = Math.Min(this.Speed, maxSpeed);
                clientSpeed = 0x4;
            }

            return Move(dir, speed, clientSpeed, true);
        }

        public bool Move(Direction dir, int speed, int clientSpeed, bool message)
        {
            Map map = Map;

            if (map == null || Deleted || CheckDecay())
                return false;

            if (m_Anchored)
            {
                if (message && m_TillerMan != null)
                    m_TillerMan.Say(501419); // Ar, the anchor is down sir!

                return false;
            }

            int rx = 0, ry = 0;
            Direction d = (Direction)(((int)m_Facing + (int)dir) & 0x7);
            Movement.Movement.Offset(d, ref rx, ref ry);

            for (int i = 1; i <= speed; ++i)
            {
                if (!CanFit(new Point3D(X + (i * rx), Y + (i * ry), Z), Map, ItemID))
                {
                    if (i == 1)
                    {
                        if (message && m_TillerMan != null)
                            m_TillerMan.Say(501424); // Ar, we've stopped sir.

                        return false;
                    }

                    speed = i - 1;
                    break;
                }
            }

            int xOffset = speed * rx;
            int yOffset = speed * ry;

            int newX = X + xOffset;
            int newY = Y + yOffset;

            Rectangle2D[] wrap = GetWrapFor(map);

            for (int i = 0; i < wrap.Length; ++i)
            {
                Rectangle2D rect = wrap[i];

                if (rect.Contains(new Point2D(X, Y)) && !rect.Contains(new Point2D(newX, newY)))
                {
                    if (newX < rect.X)
                        newX = rect.X + rect.Width - 1;
                    else if (newX >= rect.X + rect.Width)
                        newX = rect.X;

                    if (newY < rect.Y)
                        newY = rect.Y + rect.Height - 1;
                    else if (newY >= rect.Y + rect.Height)
                        newY = rect.Y;

                    for (int j = 1; j <= speed; ++j)
                    {
                        if (!CanFit(new Point3D(newX + (j * rx), newY + (j * ry), Z), Map, ItemID))
                        {
                            if (message && m_TillerMan != null)
                                m_TillerMan.Say(501424); // Ar, we've stopped sir.

                            return false;
                        }
                    }

                    xOffset = newX - X;
                    yOffset = newY - Y;
                }
            }

            if (!NewBoatMovement || Math.Abs(xOffset) > 1 || Math.Abs(yOffset) > 1)
            {
                Teleport(xOffset, yOffset, 0);
            }
            else
            {
                List<IEntity> toMove = GetMovingEntities();

                SafeAdd(m_TillerMan, toMove);
                SafeAdd(m_Hold, toMove);
                SafeAdd(m_PPlank, toMove);
                SafeAdd(m_SPlank, toMove);

                // Packet must be sent before actual locations are changed
                foreach (NetState ns in Map.GetClientsInRange(Location, GetMaxUpdateRange()))
                {
                    Mobile m = ns.Mobile;

                    if (ns.HighSeas && m.CanSee(this) && m.InRange(Location, GetUpdateRange(m)))
                        ns.Send(new MoveBoatHS(m, this, d, clientSpeed, toMove, xOffset, yOffset));
                }

                foreach (IEntity e in toMove)
                {
                    if (e is Item)
                    {
                        Item item = (Item)e;

                        item.NoMoveHS = true;

                        if (!(item is TillerMan || item is Hold || item is Plank))
                            item.Location = new Point3D(item.X + xOffset, item.Y + yOffset, item.Z);
                    }
                    else if (e is Mobile)
                    {
                        Mobile m = (Mobile)e;

                        m.NoMoveHS = true;
                        m.Location = new Point3D(m.X + xOffset, m.Y + yOffset, m.Z);
                    }
                }

                NoMoveHS = true;
                Location = new Point3D(X + xOffset, Y + yOffset, Z);

                foreach (IEntity e in toMove)
                {
                    if (e is Item)
                        ((Item)e).NoMoveHS = false;
                    else if (e is Mobile)
                        ((Mobile)e).NoMoveHS = false;
                }

                NoMoveHS = false;
            }

            return true;
        }

        private static void SafeAdd(Item item, List<IEntity> toMove)
        {
            if (item != null)
                toMove.Add(item);
        }

        public void Teleport(int xOffset, int yOffset, int zOffset)
        {
            List<IEntity> toMove = GetMovingEntities();

            for (int i = 0; i < toMove.Count; ++i)
            {
                IEntity e = toMove[i];

                if (e is Item)
                {
                    Item item = (Item)e;

                    item.Location = new Point3D(item.X + xOffset, item.Y + yOffset, item.Z + zOffset);
                }
                else if (e is Mobile)
                {
                    Mobile m = (Mobile)e;

                    m.Location = new Point3D(m.X + xOffset, m.Y + yOffset, m.Z + zOffset);
                }
            }

            Location = new Point3D(X + xOffset, Y + yOffset, Z + zOffset);
        }

        public List<IEntity> GetMovingEntities()
        {
            List<IEntity> list = new List<IEntity>();

            Map map = Map;

            if (map == null || map == Map.Internal)
                return list;

            MultiComponentList mcl = Components;

            foreach (object o in map.GetObjectsInBounds(new Rectangle2D(X + mcl.Min.X, Y + mcl.Min.Y, mcl.Width, mcl.Height)))
            {
                if (o == this || o is TillerMan || o is Hold || o is Plank)
                    continue;

                if (o is Item)
                {
                    Item item = (Item)o;

                    if (Contains(item) && item.Visible && item.Z >= Z)
                        list.Add(item);
                }
                else if (o is Mobile)
                {
                    Mobile m = (Mobile)o;

                    if (Contains(m))
                        list.Add(m);
                }
            }

            return list;
        }

        public bool SetFacing(Direction facing)
        {
            if (Parent != null || this.Map == null)
                return false;

            if (CheckDecay())
                return false;

            if (Map != Map.Internal)
            {
                switch (facing)
                {
                    case Direction.North: if (!CanFit(Location, Map, NorthID)) return false; break;
                    case Direction.East: if (!CanFit(Location, Map, EastID)) return false; break;
                    case Direction.South: if (!CanFit(Location, Map, SouthID)) return false; break;
                    case Direction.West: if (!CanFit(Location, Map, WestID)) return false; break;
                }
            }

            Direction old = m_Facing;

            m_Facing = facing;

            if (m_TillerMan != null)
                m_TillerMan.SetFacing(facing);

            if (m_Hold != null)
                m_Hold.SetFacing(facing);

            if (m_PPlank != null)
                m_PPlank.SetFacing(facing);

            if (m_SPlank != null)
                m_SPlank.SetFacing(facing);

            List<IEntity> toMove = GetMovingEntities();

            toMove.Add(m_PPlank);
            toMove.Add(m_SPlank);

            int xOffset = 0, yOffset = 0;
            Movement.Movement.Offset(facing, ref xOffset, ref yOffset);

            if (m_TillerMan != null)
                m_TillerMan.Location = new Point3D(X + (xOffset * TillerManDistance) + (facing == Direction.North ? 1 : 0), Y + (yOffset * TillerManDistance), m_TillerMan.Z);

            if (m_Hold != null)
                m_Hold.Location = new Point3D(X + (xOffset * HoldDistance), Y + (yOffset * HoldDistance), m_Hold.Z);

            int count = (int)(m_Facing - old) & 0x7;
            count /= 2;

            for (int i = 0; i < toMove.Count; ++i)
            {
                IEntity e = toMove[i];

                if (e is Item)
                {
                    Item item = (Item)e;

                    item.Location = Rotate(item.Location, count);
                }
                else if (e is Mobile)
                {
                    Mobile m = (Mobile)e;

                    m.Direction = (m.Direction - old + facing) & Direction.Mask;
                    m.Location = Rotate(m.Location, count);
                }
            }

            switch (facing)
            {
                case Direction.North: ItemID = NorthID; break;
                case Direction.East: ItemID = EastID; break;
                case Direction.South: ItemID = SouthID; break;
                case Direction.West: ItemID = WestID; break;
            }

            return true;
        }

        private class MoveTimer : Timer
        {
            private BaseBoat m_Boat;

            public MoveTimer(BaseBoat boat, TimeSpan interval, bool single)
                : base(interval, interval, single ? 1 : 0)
            {
                m_Boat = boat;
                Priority = TimerPriority.TwentyFiveMS;
            }

            protected override void OnTick()
            {
                if (!m_Boat.DoMovement(true))
                    m_Boat.StopMove(false);
            }
        }

        public static void UpdateAllComponents()
        {
            for (int i = m_Instances.Count - 1; i >= 0; --i)
                m_Instances[i].UpdateComponents();
        }

        public static void Initialize()
        {
            new UpdateAllTimer().Start();
            EventSink.WorldSave += new WorldSaveEventHandler(EventSink_WorldSave);
        }

        private static void EventSink_WorldSave(WorldSaveEventArgs e)
        {
            new UpdateAllTimer().Start();
        }

        public class UpdateAllTimer : Timer
        {
            public UpdateAllTimer()
                : base(TimeSpan.FromSeconds(1.0))
            {
            }

            protected override void OnTick()
            {
                UpdateAllComponents();
            }
        }

        #region High Seas

        public override bool AllowsRelativeDrop
        {
            get { return true; }
        }

        /*
         * OSI sends the 0xF7 packet instead, holding 0xF3 packets
         * for every entity on the boat. Though, the regular 0xF3
         * packets are still being sent as well as entities come
         * into sight. Do we really need it?
         */
        /*
        protected override Packet GetWorldPacketFor( NetState state )
        {
            if ( NewBoatMovement && state.HighSeas )
                return new DisplayBoatHS( state.Mobile, this );
            else
                return base.GetWorldPacketFor( state );
        }
        */

        public sealed class MoveBoatHS : Packet
        {
            public MoveBoatHS(Mobile beholder, BaseBoat boat, Direction d, int speed, List<IEntity> ents, int xOffset, int yOffset)
                : base(0xF6)
            {
                EnsureCapacity(3 + 15 + ents.Count * 10);

                m_Stream.Write((int)boat.Serial);
                m_Stream.Write((byte)speed);
                m_Stream.Write((byte)d);
                m_Stream.Write((byte)boat.Facing);
                m_Stream.Write((short)(boat.X + xOffset));
                m_Stream.Write((short)(boat.Y + yOffset));
                m_Stream.Write((short)boat.Z);
                m_Stream.Write((short)0); // count placeholder

                int count = 0;

                foreach (IEntity ent in ents)
                {
                    if (!beholder.CanSee(ent))
                        continue;

                    m_Stream.Write((int)ent.Serial);
                    m_Stream.Write((short)(ent.X + xOffset));
                    m_Stream.Write((short)(ent.Y + yOffset));
                    m_Stream.Write((short)ent.Z);
                    ++count;
                }

                m_Stream.Seek(16, System.IO.SeekOrigin.Begin);
                m_Stream.Write((short)count);
            }
        }

        public sealed class DisplayBoatHS : Packet
        {
            public DisplayBoatHS(Mobile beholder, BaseBoat boat)
                : base(0xF7)
            {
                List<IEntity> ents = boat.GetMovingEntities();

                SafeAdd(boat.TillerMan, ents);
                SafeAdd(boat.Hold, ents);
                SafeAdd(boat.PPlank, ents);
                SafeAdd(boat.SPlank, ents);

                ents.Add(boat);

                EnsureCapacity(3 + 2 + ents.Count * 26);

                m_Stream.Write((short)0); // count placeholder

                int count = 0;

                foreach (IEntity ent in ents)
                {
                    if (!beholder.CanSee(ent))
                        continue;

                    // Embedded WorldItemHS packets
                    m_Stream.Write((byte)0xF3);
                    m_Stream.Write((short)0x1);

                    if (ent is BaseMulti)
                    {
                        BaseMulti bm = (BaseMulti)ent;

                        m_Stream.Write((byte)0x02);
                        m_Stream.Write((int)bm.Serial);
                        // TODO: Mask no longer needed, merge with Item case?
                        m_Stream.Write((ushort)(bm.ItemID & 0x3FFF));
                        m_Stream.Write((byte)0);

                        m_Stream.Write((short)bm.Amount);
                        m_Stream.Write((short)bm.Amount);

                        m_Stream.Write((short)(bm.X & 0x7FFF));
                        m_Stream.Write((short)(bm.Y & 0x3FFF));
                        m_Stream.Write((sbyte)bm.Z);

                        m_Stream.Write((byte)bm.Light);
                        m_Stream.Write((short)bm.Hue);
                        m_Stream.Write((byte)bm.GetPacketFlags());
                    }
                    else if (ent is Mobile)
                    {
                        Mobile m = (Mobile)ent;

                        m_Stream.Write((byte)0x01);
                        m_Stream.Write((int)m.Serial);
                        m_Stream.Write((short)m.Body);
                        m_Stream.Write((byte)0);

                        m_Stream.Write((short)1);
                        m_Stream.Write((short)1);

                        m_Stream.Write((short)(m.X & 0x7FFF));
                        m_Stream.Write((short)(m.Y & 0x3FFF));
                        m_Stream.Write((sbyte)m.Z);

                        m_Stream.Write((byte)m.Direction);
                        m_Stream.Write((short)m.Hue);
                        m_Stream.Write((byte)m.GetPacketFlags());
                    }
                    else if (ent is Item)
                    {
                        Item item = (Item)ent;

                        m_Stream.Write((byte)0x00);
                        m_Stream.Write((int)item.Serial);
                        m_Stream.Write((ushort)(item.ItemID & 0xFFFF));
                        m_Stream.Write((byte)0);

                        m_Stream.Write((short)item.Amount);
                        m_Stream.Write((short)item.Amount);

                        m_Stream.Write((short)(item.X & 0x7FFF));
                        m_Stream.Write((short)(item.Y & 0x3FFF));
                        m_Stream.Write((sbyte)item.Z);

                        m_Stream.Write((byte)item.Light);
                        m_Stream.Write((short)item.Hue);
                        m_Stream.Write((byte)item.GetPacketFlags());
                    }

                    m_Stream.Write((short)0x00);
                    ++count;
                }

                m_Stream.Seek(3, System.IO.SeekOrigin.Begin);
                m_Stream.Write((short)count);
            }
        }

        #endregion
    }

    public class RenameBoatPrompt : Prompt
    {
        private BaseBoat m_Boat;

        public RenameBoatPrompt(BaseBoat boat)
        {
            m_Boat = boat;
        }

        public override void OnResponse(Mobile from, string text)
        {
            m_Boat.EndRename(from, text);
        }
    }

    public abstract class BaseDockedBoat : Item
    {
        private int m_MultiID;
        private Point3D m_Offset;
        private string m_ShipName;

        [CommandProperty(AccessLevel.GameMaster)]
        public int MultiID { get { return m_MultiID; } set { m_MultiID = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D Offset { get { return m_Offset; } set { m_Offset = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public string ShipName { get { return m_ShipName; } set { m_ShipName = value; InvalidateProperties(); } }

        public BaseDockedBoat(int id, Point3D offset, BaseBoat boat)
            : base(0x14F4)
        {
            Weight = 1.0;
            LootType = LootType.Blessed;

            m_MultiID = id;
            m_Offset = offset;

            m_ShipName = boat.ShipName;
        }

        public BaseDockedBoat(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version

            writer.Write(m_MultiID);
            writer.Write(m_Offset);
            writer.Write(m_ShipName);
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
                        m_MultiID = reader.ReadInt();
                        m_Offset = reader.ReadPoint3D();
                        m_ShipName = reader.ReadString();

                        if (version == 0)
                            reader.ReadUInt();

                        break;
                    }
            }

            if (LootType == LootType.Newbied)
                LootType = LootType.Blessed;

            if (Weight == 0.0)
                Weight = 1.0;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
            }
            else
            {
                from.SendLocalizedMessage(502482); // Where do you wish to place the ship?

                from.Target = new InternalTarget(this);
            }
        }

        public abstract BaseBoat Boat { get; }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            if (m_ShipName != null)
                list.Add(m_ShipName);
            else
                base.AddNameProperty(list);
        }

        public override void OnSingleClick(Mobile from)
        {
            if (m_ShipName != null)
                LabelTo(from, m_ShipName);
            else
                base.OnSingleClick(from);
        }

        public void OnPlacement(Mobile from, Point3D p)
        {
            if (Deleted)
            {
                return;
            }
            else if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
            }
            else
            {
                Map map = from.Map;

                if (map == null)
                    return;

                BaseBoat boat = Boat;

                if (boat == null)
                    return;

                p = new Point3D(p.X - m_Offset.X, p.Y - m_Offset.Y, p.Z - m_Offset.Z);

                if (BaseBoat.IsValidLocation(p, map) && boat.CanFit(p, map, boat.ItemID) && map != Map.Ilshenar && map != Map.Malas)
                {
                    Delete();

                    boat.Owner = from;
                    boat.Anchored = true;
                    boat.ShipName = m_ShipName;

                    uint keyValue = boat.CreateKeys(from);

                    if (boat.PPlank != null)
                        boat.PPlank.KeyValue = keyValue;

                    if (boat.SPlank != null)
                        boat.SPlank.KeyValue = keyValue;

                    boat.MoveToWorld(p, map);
                }
                else
                {
                    boat.Delete();
                    from.SendLocalizedMessage(1043284); // A ship can not be created here.
                }
            }
        }

        private class InternalTarget : MultiTarget
        {
            private BaseDockedBoat m_Model;

            public InternalTarget(BaseDockedBoat model)
                : base(model.MultiID, model.Offset)
            {
                m_Model = model;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                IPoint3D ip = o as IPoint3D;

                if (ip != null)
                {
                    if (ip is Item)
                        ip = ((Item)ip).GetWorldTop();

                    Point3D p = new Point3D(ip);

                    Region region = Region.Find(p, from.Map);

                    if (region.IsPartOf(typeof(DungeonRegion)))
                        from.SendLocalizedMessage(502488); // You can not place a ship inside a dungeon.
                    else if (region.IsPartOf(typeof(HouseRegion)) || region.IsPartOf(typeof(ChampionSpawnRegion)))
                        from.SendLocalizedMessage(1042549); // A boat may not be placed in this area.
                    else
                        m_Model.OnPlacement(from, p);
                }
            }
        }
    }

    public class ConfirmDryDockGump : Gump
    {
        private Mobile m_From;
        private BaseBoat m_Boat;

        public ConfirmDryDockGump(Mobile from, BaseBoat boat)
            : base(150, 200)
        {
            m_From = from;
            m_Boat = boat;

            m_From.CloseGump(typeof(ConfirmDryDockGump));

            AddPage(0);

            AddBackground(0, 0, 220, 170, 5054);
            AddBackground(10, 10, 200, 150, 3000);

            AddHtmlLocalized(20, 20, 180, 80, 1018319, true, false); // Do you wish to dry dock this boat?

            AddHtmlLocalized(55, 100, 140, 25, 1011011, false, false); // CONTINUE
            AddButton(20, 100, 4005, 4007, 2, GumpButtonType.Reply, 0);

            AddHtmlLocalized(55, 125, 140, 25, 1011012, false, false); // CANCEL
            AddButton(20, 125, 4005, 4007, 1, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            if (info.ButtonID == 2)
                m_Boat.EndDryDock(m_From);
        }
    }

    public abstract class BaseBoatDeed : Item
    {
        private int m_MultiID;
        private Point3D m_Offset;

        [CommandProperty(AccessLevel.GameMaster)]
        public int MultiID { get { return m_MultiID; } set { m_MultiID = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D Offset { get { return m_Offset; } set { m_Offset = value; } }

        public BaseBoatDeed(int id, Point3D offset)
            : base(0x14F2)
        {
            Weight = 1.0;

            if (!Core.AOS)
                LootType = LootType.Newbied;

            m_MultiID = id;
            m_Offset = offset;
        }

        public BaseBoatDeed(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write(m_MultiID);
            writer.Write(m_Offset);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_MultiID = reader.ReadInt();
                        m_Offset = reader.ReadPoint3D();

                        break;
                    }
            }

            if (Weight == 0.0)
                Weight = 1.0;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
            }
            else if (from.AccessLevel < AccessLevel.GameMaster && (from.Map == Map.Ilshenar || from.Map == Map.Malas))
            {
                from.SendLocalizedMessage(1010567, null, 0x25); // You may not place a boat from this location.
            }
            else
            {
                if (Core.SE)
                    from.SendLocalizedMessage(502482); // Where do you wish to place the ship?
                else
                    from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 502482); // Where do you wish to place the ship?

                from.Target = new InternalTarget(this);
            }
        }

        public abstract BaseBoat Boat { get; }

        public void OnPlacement(Mobile from, Point3D p)
        {
            if (Deleted)
            {
                return;
            }
            else if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
            }
            else
            {
                Map map = from.Map;

                if (map == null)
                    return;

                if (from.AccessLevel < AccessLevel.GameMaster && (map == Map.Ilshenar || map == Map.Malas))
                {
                    from.SendLocalizedMessage(1043284); // A ship can not be created here.
                    return;
                }

                if (from.Region.IsPartOf(typeof(HouseRegion)) || BaseBoat.FindBoatAt(from, from.Map) != null)
                {
                    from.SendLocalizedMessage(1010568, null, 0x25); // You may not place a ship while on another ship or inside a house.
                    return;
                }

                BaseBoat boat = Boat;

                if (boat == null)
                    return;

                p = new Point3D(p.X - m_Offset.X, p.Y - m_Offset.Y, p.Z - m_Offset.Z);

                if (BaseBoat.IsValidLocation(p, map) && boat.CanFit(p, map, boat.ItemID))
                {
                    Delete();

                    boat.Owner = from;
                    boat.Anchored = true;

                    uint keyValue = boat.CreateKeys(from);

                    if (boat.PPlank != null)
                        boat.PPlank.KeyValue = keyValue;

                    if (boat.SPlank != null)
                        boat.SPlank.KeyValue = keyValue;

                    boat.MoveToWorld(p, map);
                }
                else
                {
                    boat.Delete();
                    from.SendLocalizedMessage(1043284); // A ship can not be created here.
                }
            }
        }

        private class InternalTarget : MultiTarget
        {
            private BaseBoatDeed m_Deed;

            public InternalTarget(BaseBoatDeed deed)
                : base(deed.MultiID, deed.Offset)
            {
                m_Deed = deed;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                IPoint3D ip = o as IPoint3D;

                if (ip != null)
                {
                    if (ip is Item)
                        ip = ((Item)ip).GetWorldTop();

                    Point3D p = new Point3D(ip);

                    Region region = Region.Find(p, from.Map);

                    if (region.IsPartOf(typeof(DungeonRegion)))
                        from.SendLocalizedMessage(502488); // You can not place a ship inside a dungeon.
                    else if (region.IsPartOf(typeof(HouseRegion)) || region.IsPartOf(typeof(ChampionSpawnRegion)))
                        from.SendLocalizedMessage(1042549); // A boat may not be placed in this area.
                    else
                        m_Deed.OnPlacement(from, p);
                }
            }
        }
    }
}