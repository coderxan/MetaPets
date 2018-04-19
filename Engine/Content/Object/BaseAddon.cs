﻿using System;
using System.Collections;
using System.Collections.Generic;

using Server;
using Server.ContextMenus;
using Server.Engines.Craft;
using Server.Multis;
using Server.Regions;
using Server.Targeting;

namespace Server.Items
{
    public enum AddonFitResult
    {
        Valid,
        Blocked,
        NotInHouse,
        DoorTooClose,
        NoWall,
        DoorsNotClosed
    }

    public interface IAddon
    {
        Item Deed { get; }

        bool CouldFit(IPoint3D p, Map map);
    }

    /// <summary>
    /// Addon Static Objects
    /// </summary>
    public abstract class BaseAddon : Item, IChopable, IAddon
    {
        #region Mondain's Legacy
        private CraftResource m_Resource;

        [CommandProperty(AccessLevel.GameMaster)]
        public CraftResource Resource
        {
            get { return m_Resource; }
            set
            {
                if (m_Resource != value)
                {
                    m_Resource = value;
                    Hue = CraftResources.GetHue(m_Resource);

                    InvalidateProperties();
                }
            }
        }
        #endregion
        private List<AddonComponent> m_Components;

        public void AddComponent(AddonComponent c, int x, int y, int z)
        {
            if (Deleted)
                return;

            m_Components.Add(c);

            c.Addon = this;
            c.Offset = new Point3D(x, y, z);
            c.MoveToWorld(new Point3D(X + x, Y + y, Z + z), Map);
        }

        public BaseAddon()
            : base(1)
        {
            Movable = false;
            Visible = false;

            m_Components = new List<AddonComponent>();
        }

        public virtual bool RetainDeedHue { get { return false; } }

        public virtual void OnChop(Mobile from)
        {
            BaseHouse house = BaseHouse.FindHouseAt(this);

            if (house != null && house.IsOwner(from) && house.Addons.Contains(this))
            {
                Effects.PlaySound(GetWorldLocation(), Map, 0x3B3);
                from.SendLocalizedMessage(500461); // You destroy the item.

                int hue = 0;

                if (RetainDeedHue)
                {
                    for (int i = 0; hue == 0 && i < m_Components.Count; ++i)
                    {
                        AddonComponent c = m_Components[i];

                        if (c.Hue != 0)
                            hue = c.Hue;
                    }
                }

                Delete();

                house.Addons.Remove(this);

                BaseAddonDeed deed = Deed;

                if (deed != null)
                {
                    if (RetainDeedHue)
                        deed.Hue = hue;

                    from.AddToBackpack(deed);
                }
            }
        }

        public virtual BaseAddonDeed Deed { get { return null; } }

        Item IAddon.Deed
        {
            get { return this.Deed; }
        }

        public List<AddonComponent> Components
        {
            get
            {
                return m_Components;
            }
        }

        public BaseAddon(Serial serial)
            : base(serial)
        {
        }

        public bool CouldFit(IPoint3D p, Map map)
        {
            BaseHouse h = null;
            return (CouldFit(p, map, null, ref h) == AddonFitResult.Valid);
        }

        public virtual AddonFitResult CouldFit(IPoint3D p, Map map, Mobile from, ref BaseHouse house)
        {
            if (Deleted)
                return AddonFitResult.Blocked;

            foreach (AddonComponent c in m_Components)
            {
                Point3D p3D = new Point3D(p.X + c.Offset.X, p.Y + c.Offset.Y, p.Z + c.Offset.Z);

                if (!map.CanFit(p3D.X, p3D.Y, p3D.Z, c.ItemData.Height, false, true, (c.Z == 0)))
                    return AddonFitResult.Blocked;
                else if (!CheckHouse(from, p3D, map, c.ItemData.Height, ref house))
                    return AddonFitResult.NotInHouse;

                if (c.NeedsWall)
                {
                    Point3D wall = c.WallPosition;

                    if (!IsWall(p3D.X + wall.X, p3D.Y + wall.Y, p3D.Z + wall.Z, map))
                        return AddonFitResult.NoWall;
                }
            }

            ArrayList doors = house.Doors;

            for (int i = 0; i < doors.Count; ++i)
            {
                BaseDoor door = doors[i] as BaseDoor;

                Point3D doorLoc = door.GetWorldLocation();
                int doorHeight = door.ItemData.CalcHeight;

                foreach (AddonComponent c in m_Components)
                {
                    Point3D addonLoc = new Point3D(p.X + c.Offset.X, p.Y + c.Offset.Y, p.Z + c.Offset.Z);
                    int addonHeight = c.ItemData.CalcHeight;

                    if (Utility.InRange(doorLoc, addonLoc, 1) && (addonLoc.Z == doorLoc.Z || ((addonLoc.Z + addonHeight) > doorLoc.Z && (doorLoc.Z + doorHeight) > addonLoc.Z)))
                        return AddonFitResult.DoorTooClose;
                }
            }

            return AddonFitResult.Valid;
        }

        public static bool CheckHouse(Mobile from, Point3D p, Map map, int height, ref BaseHouse house)
        {
            house = BaseHouse.FindHouseAt(p, map, height);

            if (house == null || (from != null && !house.IsOwner(from)))
                return false;

            return true;
        }

        public static bool IsWall(int x, int y, int z, Map map)
        {
            if (map == null)
                return false;

            StaticTile[] tiles = map.Tiles.GetStaticTiles(x, y, true);

            for (int i = 0; i < tiles.Length; ++i)
            {
                StaticTile t = tiles[i];
                ItemData id = TileData.ItemTable[t.ID & TileData.MaxItemValue];

                if ((id.Flags & TileFlag.Wall) != 0 && (z + 16) > t.Z && (t.Z + t.Height) > z)
                    return true;
            }

            return false;
        }

        public virtual void OnComponentLoaded(AddonComponent c)
        {
        }

        public virtual void OnComponentUsed(AddonComponent c, Mobile from)
        {
        }

        public override void OnLocationChange(Point3D oldLoc)
        {
            if (Deleted)
                return;

            foreach (AddonComponent c in m_Components)
                c.Location = new Point3D(X + c.Offset.X, Y + c.Offset.Y, Z + c.Offset.Z);
        }

        public override void OnMapChange()
        {
            if (Deleted)
                return;

            foreach (AddonComponent c in m_Components)
                c.Map = Map;
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            foreach (AddonComponent c in m_Components)
                c.Delete();
        }

        public virtual bool ShareHue { get { return true; } }

        [Hue, CommandProperty(AccessLevel.GameMaster)]
        public override int Hue
        {
            get
            {
                return base.Hue;
            }
            set
            {
                if (base.Hue != value)
                {
                    base.Hue = value;

                    if (!Deleted && this.ShareHue && m_Components != null)
                    {
                        foreach (AddonComponent c in m_Components)
                            c.Hue = value;
                    }
                }
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version

            writer.WriteItemList<AddonComponent>(m_Components);
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
                        m_Components = reader.ReadStrongItemList<AddonComponent>();
                        break;
                    }
            }

            if (version < 1 && Weight == 0)
                Weight = -1;
        }
    }

    public class AddonComponent : Item, IChopable
    {
        private Point3D m_Offset;
        private BaseAddon m_Addon;

        [CommandProperty(AccessLevel.GameMaster)]
        public BaseAddon Addon
        {
            get
            {
                return m_Addon;
            }
            set
            {
                m_Addon = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D Offset
        {
            get
            {
                return m_Offset;
            }
            set
            {
                m_Offset = value;
            }
        }

        [Hue, CommandProperty(AccessLevel.GameMaster)]
        public override int Hue
        {
            get
            {
                return base.Hue;
            }
            set
            {
                base.Hue = value;

                if (m_Addon != null && m_Addon.ShareHue)
                    m_Addon.Hue = value;
            }
        }

        public virtual bool NeedsWall { get { return false; } }
        public virtual Point3D WallPosition { get { return Point3D.Zero; } }

        [Constructable]
        public AddonComponent(int itemID)
            : base(itemID)
        {
            Movable = false;
            ApplyLightTo(this);
        }

        public AddonComponent(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (m_Addon != null)
                m_Addon.OnComponentUsed(this, from);
        }

        public void OnChop(Mobile from)
        {
            if (m_Addon != null && from.InRange(GetWorldLocation(), 3))
                m_Addon.OnChop(from);
            else
                from.SendLocalizedMessage(500446); // That is too far away.
        }

        public override void OnLocationChange(Point3D old)
        {
            if (m_Addon != null)
                m_Addon.Location = new Point3D(X - m_Offset.X, Y - m_Offset.Y, Z - m_Offset.Z);
        }

        public override void OnMapChange()
        {
            if (m_Addon != null)
                m_Addon.Map = Map;
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            if (m_Addon != null)
                m_Addon.Delete();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version

            writer.Write(m_Addon);
            writer.Write(m_Offset);
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
                        m_Addon = reader.ReadItem() as BaseAddon;
                        m_Offset = reader.ReadPoint3D();

                        if (m_Addon != null)
                            m_Addon.OnComponentLoaded(this);

                        ApplyLightTo(this);

                        break;
                    }
            }

            if (version < 1 && Weight == 0)
                Weight = -1;
        }

        public static void ApplyLightTo(Item item)
        {
            if ((item.ItemData.Flags & TileFlag.LightSource) == 0)
                return; // not a light source

            int itemID = item.ItemID;

            for (int i = 0; i < m_Entries.Length; ++i)
            {
                LightEntry entry = m_Entries[i];
                int[] toMatch = entry.m_ItemIDs;
                bool contains = false;

                for (int j = 0; !contains && j < toMatch.Length; ++j)
                    contains = (itemID == toMatch[j]);

                if (contains)
                {
                    item.Light = entry.m_Light;
                    return;
                }
            }
        }

        private static LightEntry[] m_Entries = new LightEntry[]
			{
				new LightEntry( LightType.WestSmall, 1122, 1123, 1124, 1141, 1142, 1143, 1144, 1145, 1146, 2347, 2359, 2360, 2361, 2362, 2363, 2364, 2387, 2388, 2389, 2390, 2391, 2392 ),
				new LightEntry( LightType.NorthSmall, 1131, 1133, 1134, 1147, 1148, 1149, 1150, 1151, 1152, 2352, 2373, 2374, 2375, 2376, 2377, 2378, 2401, 2402, 2403, 2404, 2405, 2406 ),
				new LightEntry( LightType.Circle300, 6526, 6538, 6571 ),
				new LightEntry( LightType.Circle150, 5703, 6587 )
			};

        private class LightEntry
        {
            public LightType m_Light;
            public int[] m_ItemIDs;

            public LightEntry(LightType light, params int[] itemIDs)
            {
                m_Light = light;
                m_ItemIDs = itemIDs;
            }
        }
    }

    public class LocalizedAddonComponent : AddonComponent
    {
        private int m_LabelNumber;

        [CommandProperty(AccessLevel.GameMaster)]
        public int Number
        {
            get { return m_LabelNumber; }
            set { m_LabelNumber = value; InvalidateProperties(); }
        }

        public override int LabelNumber { get { return m_LabelNumber; } }

        [Constructable]
        public LocalizedAddonComponent(int itemID, int labelNumber)
            : base(itemID)
        {
            m_LabelNumber = labelNumber;
        }

        public LocalizedAddonComponent(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

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

    [Flipable(0x14F0, 0x14EF)]
    public abstract class BaseAddonDeed : Item
    {
        public abstract BaseAddon Addon
        {
            get;
        }

        #region Mondain's Legacy
        private CraftResource m_Resource;

        [CommandProperty(AccessLevel.GameMaster)]
        public CraftResource Resource
        {
            get { return m_Resource; }
            set
            {
                if (m_Resource != value)
                {
                    m_Resource = value;
                    Hue = CraftResources.GetHue(m_Resource);

                    InvalidateProperties();
                }
            }
        }
        #endregion

        public BaseAddonDeed()
            : base(0x14F0)
        {
            Weight = 1.0;

            if (!Core.AOS)
                LootType = LootType.Newbied;
        }

        public BaseAddonDeed(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (Weight == 0.0)
                Weight = 1.0;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack))
                from.Target = new InternalTarget(this);
            else
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
        }

        private class InternalTarget : Target
        {
            private BaseAddonDeed m_Deed;

            public InternalTarget(BaseAddonDeed deed)
                : base(-1, true, TargetFlags.None)
            {
                m_Deed = deed;

                CheckLOS = false;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                IPoint3D p = targeted as IPoint3D;
                Map map = from.Map;

                if (p == null || map == null || m_Deed.Deleted)
                    return;

                if (m_Deed.IsChildOf(from.Backpack))
                {
                    BaseAddon addon = m_Deed.Addon;

                    Server.Spells.SpellHelper.GetSurfaceTop(ref p);

                    BaseHouse house = null;

                    AddonFitResult res = addon.CouldFit(p, map, from, ref house);

                    if (res == AddonFitResult.Valid)
                        addon.MoveToWorld(new Point3D(p), map);
                    else if (res == AddonFitResult.Blocked)
                        from.SendLocalizedMessage(500269); // You cannot build that there.
                    else if (res == AddonFitResult.NotInHouse)
                        from.SendLocalizedMessage(500274); // You can only place this in a house that you own!
                    else if (res == AddonFitResult.DoorTooClose)
                        from.SendLocalizedMessage(500271); // You cannot build near the door.
                    else if (res == AddonFitResult.NoWall)
                        from.SendLocalizedMessage(500268); // This object needs to be mounted on something.

                    if (res == AddonFitResult.Valid)
                    {
                        m_Deed.Delete();
                        house.Addons.Add(addon);
                    }
                    else
                    {
                        addon.Delete();
                    }
                }
                else
                {
                    from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
                }
            }
        }
    }

    /// <summary>
    /// Addon Container Objects
    /// </summary>
    public abstract class BaseAddonContainer : BaseContainer, IChopable, IAddon
    {
        public override bool DisplayWeight { get { return false; } }

        [Hue, CommandProperty(AccessLevel.GameMaster)]
        public override int Hue
        {
            get
            {
                return base.Hue;
            }
            set
            {
                if (base.Hue != value)
                {
                    base.Hue = value;

                    if (!Deleted && this.ShareHue && m_Components != null)
                    {
                        Hue = value;

                        foreach (AddonContainerComponent c in m_Components)
                            c.Hue = value;
                    }
                }
            }
        }

        private CraftResource m_Resource;

        [CommandProperty(AccessLevel.GameMaster)]
        public CraftResource Resource
        {
            get { return m_Resource; }
            set
            {
                if (m_Resource != value)
                {
                    m_Resource = value;
                    Hue = CraftResources.GetHue(m_Resource);

                    InvalidateProperties();
                }
            }
        }

        Item IAddon.Deed
        {
            get { return this.Deed; }
        }

        public virtual bool RetainDeedHue { get { return false; } }
        public virtual bool NeedsWall { get { return false; } }
        public virtual bool ShareHue { get { return true; } }
        public virtual Point3D WallPosition { get { return Point3D.Zero; } }
        public virtual BaseAddonContainerDeed Deed { get { return null; } }

        private List<AddonContainerComponent> m_Components;

        public List<AddonContainerComponent> Components
        {
            get { return m_Components; }
        }

        public BaseAddonContainer(int itemID)
            : base(itemID)
        {
            AddonComponent.ApplyLightTo(this);

            m_Components = new List<AddonContainerComponent>();
        }

        public BaseAddonContainer(Serial serial)
            : base(serial)
        {
        }

        public override void OnLocationChange(Point3D oldLoc)
        {
            base.OnLocationChange(oldLoc);

            if (Deleted)
                return;

            foreach (AddonContainerComponent c in m_Components)
                c.Location = new Point3D(X + c.Offset.X, Y + c.Offset.Y, Z + c.Offset.Z);
        }

        public override void OnMapChange()
        {
            base.OnMapChange();

            if (Deleted)
                return;

            foreach (AddonContainerComponent c in m_Components)
                c.Map = Map;
        }

        public override void OnDelete()
        {
            BaseHouse house = BaseHouse.FindHouseAt(this);

            if (house != null)
                house.Addons.Remove(this);

            base.OnDelete();
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (!CraftResources.IsStandard(m_Resource))
                list.Add(CraftResources.GetLocalizationNumber(m_Resource));
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            foreach (AddonContainerComponent c in m_Components)
                c.Delete();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.WriteItemList<AddonContainerComponent>(m_Components);
            writer.Write((int)m_Resource);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            m_Components = reader.ReadStrongItemList<AddonContainerComponent>();
            m_Resource = (CraftResource)reader.ReadInt();

            AddonComponent.ApplyLightTo(this);
        }

        public virtual void DropItemsToGround()
        {
            for (int i = Items.Count - 1; i >= 0; i--)
                Items[i].MoveToWorld(Location);
        }

        public void AddComponent(AddonContainerComponent c, int x, int y, int z)
        {
            if (Deleted)
                return;

            m_Components.Add(c);

            c.Addon = this;
            c.Offset = new Point3D(x, y, z);
            c.MoveToWorld(new Point3D(X + x, Y + y, Z + z), Map);
        }

        public AddonFitResult CouldFit(IPoint3D p, Map map, Mobile from, ref BaseHouse house)
        {
            if (Deleted)
                return AddonFitResult.Blocked;

            foreach (AddonContainerComponent c in m_Components)
            {
                Point3D p3D = new Point3D(p.X + c.Offset.X, p.Y + c.Offset.Y, p.Z + c.Offset.Z);

                if (!map.CanFit(p3D.X, p3D.Y, p3D.Z, c.ItemData.Height, false, true, (c.Z == 0)))
                    return AddonFitResult.Blocked;
                else if (!BaseAddon.CheckHouse(from, p3D, map, c.ItemData.Height, ref house))
                    return AddonFitResult.NotInHouse;

                if (c.NeedsWall)
                {
                    Point3D wall = c.WallPosition;

                    if (!BaseAddon.IsWall(p3D.X + wall.X, p3D.Y + wall.Y, p3D.Z + wall.Z, map))
                        return AddonFitResult.NoWall;
                }
            }

            Point3D p3 = new Point3D(p.X, p.Y, p.Z);

            if (!map.CanFit(p3.X, p3.Y, p3.Z, ItemData.Height, false, true, (Z == 0)))
                return AddonFitResult.Blocked;
            else if (!BaseAddon.CheckHouse(from, p3, map, ItemData.Height, ref house))
                return AddonFitResult.NotInHouse;

            if (NeedsWall)
            {
                Point3D wall = WallPosition;

                if (!BaseAddon.IsWall(p3.X + wall.X, p3.Y + wall.Y, p3.Z + wall.Z, map))
                    return AddonFitResult.NoWall;
            }

            if (house != null)
            {
                ArrayList doors = house.Doors;

                for (int i = 0; i < doors.Count; ++i)
                {
                    BaseDoor door = doors[i] as BaseDoor;

                    if (door != null && door.Open)
                        return AddonFitResult.DoorsNotClosed;

                    Point3D doorLoc = door.GetWorldLocation();
                    int doorHeight = door.ItemData.CalcHeight;

                    foreach (AddonContainerComponent c in m_Components)
                    {
                        Point3D addonLoc = new Point3D(p.X + c.Offset.X, p.Y + c.Offset.Y, p.Z + c.Offset.Z);
                        int addonHeight = c.ItemData.CalcHeight;

                        if (Utility.InRange(doorLoc, addonLoc, 1) && (addonLoc.Z == doorLoc.Z || ((addonLoc.Z + addonHeight) > doorLoc.Z && (doorLoc.Z + doorHeight) > addonLoc.Z)))
                            return AddonFitResult.DoorTooClose;
                    }

                    Point3D addonLo = new Point3D(p.X, p.Y, p.Z);
                    int addonHeigh = ItemData.CalcHeight;

                    if (Utility.InRange(doorLoc, addonLo, 1) && (addonLo.Z == doorLoc.Z || ((addonLo.Z + addonHeigh) > doorLoc.Z && (doorLoc.Z + doorHeight) > addonLo.Z)))
                        return AddonFitResult.DoorTooClose;
                }
            }

            return AddonFitResult.Valid;
        }

        public bool CouldFit(IPoint3D p, Map map)
        {
            BaseHouse house = null;

            return (CouldFit(p, map, null, ref house) == AddonFitResult.Valid);
        }

        public virtual void OnChop(Mobile from)
        {
            BaseHouse house = BaseHouse.FindHouseAt(this);

            if (house != null && house.IsOwner(from))
            {
                if (!IsSecure)
                {
                    Effects.PlaySound(GetWorldLocation(), Map, 0x3B3);
                    from.SendLocalizedMessage(500461); // You destroy the item.

                    int hue = 0;

                    if (RetainDeedHue)
                    {
                        for (int i = 0; hue == 0 && i < m_Components.Count; ++i)
                        {
                            AddonContainerComponent c = m_Components[i];

                            if (c.Hue != 0)
                                hue = c.Hue;
                        }
                    }

                    DropItemsToGround();

                    Delete();

                    house.Addons.Remove(this);

                    BaseAddonContainerDeed deed = Deed;

                    if (deed != null)
                    {
                        deed.Resource = Resource;

                        if (RetainDeedHue)
                            deed.Hue = hue;

                        from.AddToBackpack(deed);
                    }
                }
                else
                    from.SendLocalizedMessage(1074870); // This item must be unlocked/unsecured before re-deeding it.
            }
        }

        public virtual void OnComponentLoaded(AddonContainerComponent c)
        {
        }

        public virtual void OnComponentUsed(AddonContainerComponent c, Mobile from)
        {
        }
    }

    public class AddonContainerComponent : Item, IChopable
    {
        public virtual bool NeedsWall { get { return false; } }
        public virtual Point3D WallPosition { get { return Point3D.Zero; } }

        private Point3D m_Offset;
        private BaseAddonContainer m_Addon;

        [CommandProperty(AccessLevel.GameMaster)]
        public BaseAddonContainer Addon
        {
            get { return m_Addon; }
            set { m_Addon = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D Offset
        {
            get { return m_Offset; }
            set { m_Offset = value; }
        }

        [Hue, CommandProperty(AccessLevel.GameMaster)]
        public override int Hue
        {
            get { return base.Hue; }
            set
            {
                base.Hue = value;

                if (m_Addon != null && m_Addon.ShareHue)
                    m_Addon.Hue = value;
            }
        }

        [Constructable]
        public AddonContainerComponent(int itemID)
            : base(itemID)
        {
            Movable = false;

            AddonComponent.ApplyLightTo(this);
        }

        public AddonContainerComponent(Serial serial)
            : base(serial)
        {
        }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (Addon != null)
                return Addon.OnDragDrop(from, dropped);

            return false;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (m_Addon != null)
                m_Addon.OnComponentUsed(this, from);
        }

        public override void OnLocationChange(Point3D old)
        {
            if (m_Addon != null)
                m_Addon.Location = new Point3D(X - m_Offset.X, Y - m_Offset.Y, Z - m_Offset.Z);
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            if (m_Addon != null)
                m_Addon.GetContextMenuEntries(from, list);
        }

        public override void OnMapChange()
        {
            if (m_Addon != null)
                m_Addon.Map = Map;
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            if (m_Addon != null)
                m_Addon.Delete();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write(m_Addon);
            writer.Write(m_Offset);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            m_Addon = reader.ReadItem() as BaseAddonContainer;
            m_Offset = reader.ReadPoint3D();

            if (m_Addon != null)
                m_Addon.OnComponentLoaded(this);

            AddonComponent.ApplyLightTo(this);
        }

        public virtual void OnChop(Mobile from)
        {
            if (m_Addon != null && from.InRange(GetWorldLocation(), 3))
                m_Addon.OnChop(from);
            else
                from.SendLocalizedMessage(500446); // That is too far away.
        }
    }

    public class LocalizedContainerComponent : AddonContainerComponent
    {
        private int m_LabelNumber;

        public override int LabelNumber
        {
            get
            {
                if (m_LabelNumber > 0)
                    return m_LabelNumber;

                return base.LabelNumber;
            }
        }

        public LocalizedContainerComponent(int itemID, int labelNumber)
            : base(itemID)
        {
            m_LabelNumber = labelNumber;
        }

        public LocalizedContainerComponent(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write(m_LabelNumber);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            m_LabelNumber = reader.ReadInt();
        }
    }

    [Flipable(0x14F0, 0x14EF)]
    public abstract class BaseAddonContainerDeed : Item, ICraftable
    {
        public abstract BaseAddonContainer Addon { get; }

        private CraftResource m_Resource;

        [CommandProperty(AccessLevel.GameMaster)]
        public CraftResource Resource
        {
            get { return m_Resource; }
            set
            {
                if (m_Resource != value)
                {
                    m_Resource = value;
                    Hue = CraftResources.GetHue(m_Resource);

                    InvalidateProperties();
                }
            }
        }

        public BaseAddonContainerDeed()
            : base(0x14F0)
        {
            Weight = 1.0;

            if (!Core.AOS)
                LootType = LootType.Newbied;
        }

        public BaseAddonContainerDeed(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version

            // version 1
            writer.Write((int)m_Resource);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                    m_Resource = (CraftResource)reader.ReadInt();
                    break;
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack))
                from.Target = new InternalTarget(this);
            else
                from.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (!CraftResources.IsStandard(m_Resource))
                list.Add(CraftResources.GetLocalizationNumber(m_Resource));
        }

        #region ICraftable
        public virtual int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool, CraftItem craftItem, int resHue)
        {
            Type resourceType = typeRes;

            if (resourceType == null)
                resourceType = craftItem.Resources.GetAt(0).ItemType;

            Resource = CraftResources.GetFromType(resourceType);

            CraftContext context = craftSystem.GetContext(from);

            if (context != null && context.DoNotColor)
                Hue = 0;

            return quality;
        }
        #endregion

        private class InternalTarget : Target
        {
            private BaseAddonContainerDeed m_Deed;

            public InternalTarget(BaseAddonContainerDeed deed)
                : base(-1, true, TargetFlags.None)
            {
                m_Deed = deed;

                CheckLOS = false;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                IPoint3D p = targeted as IPoint3D;
                Map map = from.Map;

                if (p == null || map == null || m_Deed.Deleted)
                    return;

                if (m_Deed.IsChildOf(from.Backpack))
                {
                    BaseAddonContainer addon = m_Deed.Addon;
                    addon.Resource = m_Deed.Resource;

                    Server.Spells.SpellHelper.GetSurfaceTop(ref p);

                    BaseHouse house = null;

                    AddonFitResult res = addon.CouldFit(p, map, from, ref house);

                    if (res == AddonFitResult.Valid)
                        addon.MoveToWorld(new Point3D(p), map);
                    else if (res == AddonFitResult.Blocked)
                        from.SendLocalizedMessage(500269); // You cannot build that there.
                    else if (res == AddonFitResult.NotInHouse)
                        from.SendLocalizedMessage(500274); // You can only place this in a house that you own!
                    else if (res == AddonFitResult.DoorsNotClosed)
                        from.SendMessage("You must close all house doors before placing this.");
                    else if (res == AddonFitResult.DoorTooClose)
                        from.SendLocalizedMessage(500271); // You cannot build near the door.
                    else if (res == AddonFitResult.NoWall)
                        from.SendLocalizedMessage(500268); // This object needs to be mounted on something.

                    if (res == AddonFitResult.Valid)
                    {
                        m_Deed.Delete();
                        house.Addons.Add(addon);
                        house.AddSecure(from, addon);
                    }
                    else
                    {
                        addon.Delete();
                    }
                }
                else
                {
                    from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
                }
            }
        }
    }
}