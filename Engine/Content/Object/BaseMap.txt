using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Server;
using Server.Engines.Craft;
using Server.Network;

namespace Server.Items
{
    public enum PresetMapType
    {
        Britain,
        BritainToSkaraBrae,
        BritainToTrinsic,
        BucsDen,
        BucsDenToMagincia,
        BucsDenToOcllo,
        Jhelom,
        Magincia,
        MaginciaToOcllo,
        Minoc,
        MinocToYew,
        MinocToVesper,
        Moonglow,
        MoonglowToNujelm,
        Nujelm,
        NujelmToMagincia,
        Ocllo,
        SerpentsHold,
        SerpentsHoldToOcllo,
        SkaraBrae,
        TheWorld,
        Trinsic,
        TrinsicToBucsDen,
        TrinsicToJhelom,
        Vesper,
        VesperToNujelm,
        Yew,
        YewToBritain
    }

    [Flipable(0x14EB, 0x14EC)]
    public class MapItem : Item, ICraftable
    {
        private Rectangle2D m_Bounds;

        private int m_Width, m_Height;

        private bool m_Protected;
        private bool m_Editable;

        private List<Point2D> m_Pins = new List<Point2D>();

        private const int MaxUserPins = 50;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Protected
        {
            get { return m_Protected; }
            set { m_Protected = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Rectangle2D Bounds
        {
            get { return m_Bounds; }
            set { m_Bounds = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Width
        {
            get { return m_Width; }
            set { m_Width = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Height
        {
            get { return m_Height; }
            set { m_Height = value; }
        }

        public List<Point2D> Pins
        {
            get { return m_Pins; }
        }

        [Constructable]
        public MapItem()
            : base(0x14EC)
        {
            Weight = 1.0;

            m_Width = 200;
            m_Height = 200;
        }

        public virtual void CraftInit(Mobile from)
        {
        }

        public void SetDisplay(int x1, int y1, int x2, int y2, int w, int h)
        {
            Width = w;
            Height = h;

            if (x1 < 0)
                x1 = 0;

            if (y1 < 0)
                y1 = 0;

            if (x2 >= 5120)
                x2 = 5119;

            if (y2 >= 4096)
                y2 = 4095;

            Bounds = new Rectangle2D(x1, y1, x2 - x1, y2 - y1);
        }

        public MapItem(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.InRange(GetWorldLocation(), 2))
                DisplayTo(from);
            else
                from.SendLocalizedMessage(500446); // That is too far away.
        }

        public virtual void DisplayTo(Mobile from)
        {
            from.Send(new MapDetails(this));
            from.Send(new MapDisplay(this));

            for (int i = 0; i < m_Pins.Count; ++i)
                from.Send(new MapAddPin(this, m_Pins[i]));

            from.Send(new MapSetEditable(this, ValidateEdit(from)));
        }

        public virtual void OnAddPin(Mobile from, int x, int y)
        {
            if (!ValidateEdit(from))
                return;
            else if (m_Pins.Count >= MaxUserPins)
                return;

            Validate(ref x, ref y);
            AddPin(x, y);
        }

        public virtual void OnRemovePin(Mobile from, int number)
        {
            if (!ValidateEdit(from))
                return;

            RemovePin(number);
        }

        public virtual void OnChangePin(Mobile from, int number, int x, int y)
        {
            if (!ValidateEdit(from))
                return;

            Validate(ref x, ref y);
            ChangePin(number, x, y);
        }

        public virtual void OnInsertPin(Mobile from, int number, int x, int y)
        {
            if (!ValidateEdit(from))
                return;
            else if (m_Pins.Count >= MaxUserPins)
                return;

            Validate(ref x, ref y);
            InsertPin(number, x, y);
        }

        public virtual void OnClearPins(Mobile from)
        {
            if (!ValidateEdit(from))
                return;

            ClearPins();
        }

        public virtual void OnToggleEditable(Mobile from)
        {
            if (Validate(from))
                m_Editable = !m_Editable;

            from.Send(new MapSetEditable(this, Validate(from) && m_Editable));
        }

        public virtual void Validate(ref int x, ref int y)
        {
            if (x < 0)
                x = 0;
            else if (x >= m_Width)
                x = m_Width - 1;

            if (y < 0)
                y = 0;
            else if (y >= m_Height)
                y = m_Height - 1;
        }

        public virtual bool ValidateEdit(Mobile from)
        {
            return m_Editable && Validate(from);
        }

        public virtual bool Validate(Mobile from)
        {
            if (!from.CanSee(this) || from.Map != this.Map || !from.Alive || InSecureTrade)
                return false;
            else if (from.AccessLevel >= AccessLevel.GameMaster)
                return true;
            else if (!Movable || m_Protected || !from.InRange(GetWorldLocation(), 2))
                return false;

            object root = RootParent;

            if (root is Mobile && root != from)
                return false;

            return true;
        }

        public void ConvertToWorld(int x, int y, out int worldX, out int worldY)
        {
            worldX = ((m_Bounds.Width * x) / Width) + m_Bounds.X;
            worldY = ((m_Bounds.Height * y) / Height) + m_Bounds.Y;
        }

        public void ConvertToMap(int x, int y, out int mapX, out int mapY)
        {
            mapX = ((x - m_Bounds.X) * Width) / m_Bounds.Width;
            mapY = ((y - m_Bounds.Y) * Width) / m_Bounds.Height;
        }

        public virtual void AddWorldPin(int x, int y)
        {
            int mapX, mapY;
            ConvertToMap(x, y, out mapX, out mapY);

            AddPin(mapX, mapY);
        }

        public virtual void AddPin(int x, int y)
        {
            m_Pins.Add(new Point2D(x, y));
        }

        public virtual void RemovePin(int index)
        {
            if (index > 0 && index < m_Pins.Count)
                m_Pins.RemoveAt(index);
        }

        public virtual void InsertPin(int index, int x, int y)
        {
            if (index < 0 || index >= m_Pins.Count)
                m_Pins.Add(new Point2D(x, y));
            else
                m_Pins.Insert(index, new Point2D(x, y));
        }

        public virtual void ChangePin(int index, int x, int y)
        {
            if (index >= 0 && index < m_Pins.Count)
                m_Pins[index] = new Point2D(x, y);
        }

        public virtual void ClearPins()
        {
            m_Pins.Clear();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);

            writer.Write(m_Bounds);

            writer.Write(m_Width);
            writer.Write(m_Height);

            writer.Write(m_Protected);

            writer.Write(m_Pins.Count);
            for (int i = 0; i < m_Pins.Count; ++i)
                writer.Write(m_Pins[i]);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_Bounds = reader.ReadRect2D();

                        m_Width = reader.ReadInt();
                        m_Height = reader.ReadInt();

                        m_Protected = reader.ReadBool();

                        int count = reader.ReadInt();
                        for (int i = 0; i < count; i++)
                            m_Pins.Add(reader.ReadPoint2D());

                        break;
                    }
            }
        }

        public static void Initialize()
        {
            PacketHandlers.Register(0x56, 11, true, new OnPacketReceive(OnMapCommand));
        }

        private static void OnMapCommand(NetState state, PacketReader pvSrc)
        {
            Mobile from = state.Mobile;
            MapItem map = World.FindItem(pvSrc.ReadInt32()) as MapItem;

            if (map == null)
                return;

            int command = pvSrc.ReadByte();
            int number = pvSrc.ReadByte();

            int x = pvSrc.ReadInt16();
            int y = pvSrc.ReadInt16();

            switch (command)
            {
                case 1: map.OnAddPin(from, x, y); break;
                case 2: map.OnInsertPin(from, number, x, y); break;
                case 3: map.OnChangePin(from, number, x, y); break;
                case 4: map.OnRemovePin(from, number); break;
                case 5: map.OnClearPins(from); break;
                case 6: map.OnToggleEditable(from); break;
            }
        }

        private sealed class MapDetails : Packet
        {
            public MapDetails(MapItem map)
                : base(0x90, 19)
            {
                m_Stream.Write((int)map.Serial);
                m_Stream.Write((short)0x139D);
                m_Stream.Write((short)map.Bounds.Start.X);
                m_Stream.Write((short)map.Bounds.Start.Y);
                m_Stream.Write((short)map.Bounds.End.X);
                m_Stream.Write((short)map.Bounds.End.Y);
                m_Stream.Write((short)map.Width);
                m_Stream.Write((short)map.Height);
            }
        }

        /*
        private sealed class MapDetailsNew : Packet
        {
            public MapDetailsNew( MapItem map ) : base ( 0xF5, 21 )
            {
                m_Stream.Write( (int) map.Serial );
                m_Stream.Write( (short) 0x139D );
                m_Stream.Write( (short) map.Bounds.Start.X );
                m_Stream.Write( (short) map.Bounds.Start.Y );
                m_Stream.Write( (short) map.Bounds.End.X );
                m_Stream.Write( (short) map.Bounds.End.Y );
                m_Stream.Write( (short) map.Width );
                m_Stream.Write( (short) map.Height );
                m_Stream.Write( (short) ( map.Facet == null ? 0 : map.Facet.MapID ) );
            }
        }
        */

        private abstract class MapCommand : Packet
        {
            public MapCommand(MapItem map, int command, int number, int x, int y)
                : base(0x56, 11)
            {
                m_Stream.Write((int)map.Serial);
                m_Stream.Write((byte)command);
                m_Stream.Write((byte)number);
                m_Stream.Write((short)x);
                m_Stream.Write((short)y);
            }
        }

        private sealed class MapDisplay : MapCommand
        {
            public MapDisplay(MapItem map)
                : base(map, 5, 0, 0, 0)
            {
            }
        }

        private sealed class MapAddPin : MapCommand
        {
            public MapAddPin(MapItem map, Point2D point)
                : base(map, 1, 0, point.X, point.Y)
            {
            }
        }

        private sealed class MapSetEditable : MapCommand
        {
            public MapSetEditable(MapItem map, bool editable)
                : base(map, 7, editable ? 1 : 0, 0, 0)
            {
            }
        }
        #region ICraftable Members

        public int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool, CraftItem craftItem, int resHue)
        {
            CraftInit(from);
            return 1;
        }

        #endregion
    }

    public class PresetMap : MapItem
    {
        private int m_LabelNumber;

        [Constructable]
        public PresetMap(PresetMapType type)
        {
            int v = (int)type;

            if (v >= 0 && v < PresetMapEntry.Table.Length)
                InitEntry(PresetMapEntry.Table[v]);
        }

        public PresetMap(PresetMapEntry entry)
        {
            InitEntry(entry);
        }

        public void InitEntry(PresetMapEntry entry)
        {
            m_LabelNumber = entry.Name;

            Width = entry.Width;
            Height = entry.Height;

            Bounds = entry.Bounds;
        }

        public override int LabelNumber { get { return (m_LabelNumber == 0 ? base.LabelNumber : m_LabelNumber); } }

        public PresetMap(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);

            writer.Write((int)m_LabelNumber);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_LabelNumber = reader.ReadInt();
                        break;
                    }
            }
        }
    }

    public class PresetMapEntry
    {
        private int m_Name;
        private int m_Width, m_Height;
        private Rectangle2D m_Bounds;

        public int Name { get { return m_Name; } }
        public int Width { get { return m_Width; } }
        public int Height { get { return m_Height; } }
        public Rectangle2D Bounds { get { return m_Bounds; } }

        public PresetMapEntry(int name, int width, int height, int xLeft, int yTop, int xRight, int yBottom)
        {
            m_Name = name;
            m_Width = width;
            m_Height = height;
            m_Bounds = new Rectangle2D(xLeft, yTop, xRight - xLeft, yBottom - yTop);
        }

        private static PresetMapEntry[] m_Table = new PresetMapEntry[]
			{
				new PresetMapEntry( 1041189, 200, 200, 1092, 1396, 1736, 1924 ), // map of Britain
				new PresetMapEntry( 1041203, 200, 200, 0256, 1792, 1736, 2560 ), // map of Britain to Skara Brae
				new PresetMapEntry( 1041192, 200, 200, 1024, 1280, 2304, 3072 ), // map of Britain to Trinsic
				new PresetMapEntry( 1041183, 200, 200, 2500, 1900, 3000, 2400 ), // map of Buccaneer's Den
				new PresetMapEntry( 1041198, 200, 200, 2560, 1792, 3840, 2560 ), // map of Buccaneer's Den to Magincia
				new PresetMapEntry( 1041194, 200, 200, 2560, 1792, 3840, 3072 ), // map of Buccaneer's Den to Ocllo
				new PresetMapEntry( 1041181, 200, 200, 1088, 3572, 1528, 4056 ), // map of Jhelom
				new PresetMapEntry( 1041186, 200, 200, 3530, 2022, 3818, 2298 ), // map of Magincia
				new PresetMapEntry( 1041199, 200, 200, 3328, 1792, 3840, 2304 ), // map of Magincia to Ocllo
				new PresetMapEntry( 1041182, 200, 200, 2360, 0356, 2706, 0702 ), // map of Minoc
				new PresetMapEntry( 1041190, 200, 200, 0000, 0256, 2304, 3072 ), // map of Minoc to Yew
				new PresetMapEntry( 1041191, 200, 200, 2467, 0572, 2878, 0746 ), // map of Minoc to Vesper
				new PresetMapEntry( 1041188, 200, 200, 4156, 0808, 4732, 1528 ), // map of Moonglow
				new PresetMapEntry( 1041201, 200, 200, 3328, 0768, 4864, 1536 ), // map of Moonglow to Nujelm
				new PresetMapEntry( 1041185, 200, 200, 3446, 1030, 3832, 1424 ), // map of Nujelm
				new PresetMapEntry( 1041197, 200, 200, 3328, 1024, 3840, 2304 ), // map of Nujelm to Magincia
				new PresetMapEntry( 1041187, 200, 200, 3582, 2456, 3770, 2742 ), // map of Ocllo
				new PresetMapEntry( 1041184, 200, 200, 2714, 3329, 3100, 3639 ), // map of Serpent's Hold
				new PresetMapEntry( 1041200, 200, 200, 2560, 2560, 3840, 3840 ), // map of Serpent's Hold to Ocllo
				new PresetMapEntry( 1041180, 200, 200, 0524, 2064, 0960, 2452 ), // map of Skara Brae
				new PresetMapEntry( 1041204, 200, 200, 0000, 0000, 5199, 4095 ), // map of The World
				new PresetMapEntry( 1041177, 200, 200, 1792, 2630, 2118, 2952 ), // map of Trinsic
				new PresetMapEntry( 1041193, 200, 200, 1792, 1792, 3072, 3072 ), // map of Trinsic to Buccaneer's Den
				new PresetMapEntry( 1041195, 200, 200, 0256, 1792, 2304, 4095 ), // map of Trinsic to Jhelom
				new PresetMapEntry( 1041178, 200, 200, 2636, 0592, 3064, 1012 ), // map of Vesper
				new PresetMapEntry( 1041196, 200, 200, 2636, 0592, 3840, 1536 ), // map of Vesper to Nujelm
				new PresetMapEntry( 1041179, 200, 200, 0236, 0741, 0766, 1269 ), // map of Yew
				new PresetMapEntry( 1041202, 200, 200, 0000, 0512, 1792, 2048 )  // map of Yew to Britain
			};

        public static PresetMapEntry[] Table { get { return m_Table; } }
    }

    public class IndecipherableMap : MapItem
    {
        public override int LabelNumber { get { return 1070799; } } // indecipherable map

        [Constructable]
        public IndecipherableMap()
        {
            if (Utility.RandomDouble() < 0.2)
                Hue = 0x965;
            else
                Hue = 0x961;
        }

        public IndecipherableMap(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            from.SendLocalizedMessage(1070801); // You cannot decipher this ruined map.
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }
}