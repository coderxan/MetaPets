using System;
using System.Collections;
using System.Collections.Generic;

using Server.ContextMenus;
using Server.Engines.Craft;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Network;

namespace Server.Items
{
    public abstract class BaseContainer : Container
    {
        public override int DefaultMaxWeight
        {
            get
            {
                if (IsSecure)
                    return 0;

                return base.DefaultMaxWeight;
            }
        }

        public BaseContainer(int itemID)
            : base(itemID)
        {
        }

        public override bool IsAccessibleTo(Mobile m)
        {
            if (!BaseHouse.CheckAccessible(m, this))
                return false;

            return base.IsAccessibleTo(m);
        }

        public override bool CheckHold(Mobile m, Item item, bool message, bool checkItems, int plusItems, int plusWeight)
        {
            if (this.IsSecure && !BaseHouse.CheckHold(m, this, item, message, checkItems, plusItems, plusWeight))
                return false;

            return base.CheckHold(m, item, message, checkItems, plusItems, plusWeight);
        }

        public override bool CheckItemUse(Mobile from, Item item)
        {
            if (IsDecoContainer && item is BaseBook)
                return true;

            return base.CheckItemUse(from, item);
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);
            SetSecureLevelEntry.AddTo(from, this, list);
        }

        public override bool TryDropItem(Mobile from, Item dropped, bool sendFullMessage)
        {
            if (!CheckHold(from, dropped, sendFullMessage, true))
                return false;

            BaseHouse house = BaseHouse.FindHouseAt(this);

            if (house != null && house.IsLockedDown(this))
            {
                if (dropped is VendorRentalContract || (dropped is Container && ((Container)dropped).FindItemByType(typeof(VendorRentalContract)) != null))
                {
                    from.SendLocalizedMessage(1062492); // You cannot place a rental contract in a locked down container.
                    return false;
                }

                if (!house.LockDown(from, dropped, false))
                    return false;
            }

            List<Item> list = this.Items;

            for (int i = 0; i < list.Count; ++i)
            {
                Item item = list[i];

                if (!(item is Container) && item.StackWith(from, dropped, false))
                    return true;
            }

            DropItem(dropped);

            return true;
        }

        public override bool OnDragDropInto(Mobile from, Item item, Point3D p)
        {
            if (!CheckHold(from, item, true, true))
                return false;

            BaseHouse house = BaseHouse.FindHouseAt(this);

            if (house != null && house.IsLockedDown(this))
            {
                if (item is VendorRentalContract || (item is Container && ((Container)item).FindItemByType(typeof(VendorRentalContract)) != null))
                {
                    from.SendLocalizedMessage(1062492); // You cannot place a rental contract in a locked down container.
                    return false;
                }

                if (!house.LockDown(from, item, false))
                    return false;
            }

            item.Location = new Point3D(p.X, p.Y, 0);
            AddItem(item);

            from.SendSound(GetDroppedSound(item), GetWorldLocation());

            return true;
        }

        public override void UpdateTotal(Item sender, TotalType type, int delta)
        {
            base.UpdateTotal(sender, type, delta);

            if (type == TotalType.Weight && RootParent is Mobile)
                ((Mobile)RootParent).InvalidateProperties();
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.AccessLevel > AccessLevel.Player || from.InRange(this.GetWorldLocation(), 2) || this.RootParent is PlayerVendor)
                Open(from);
            else
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
        }

        public virtual void Open(Mobile from)
        {
            DisplayTo(from);
        }

        public BaseContainer(Serial serial)
            : base(serial)
        {
        }

        /* Note: base class insertion; we cannot serialize anything here */
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    public abstract class LockableContainer : TrapableContainer, ILockable, ILockpickable, ICraftable, IShipwreckedItem
    {
        private bool m_Locked;
        private int m_LockLevel, m_MaxLockLevel, m_RequiredSkill;
        private uint m_KeyValue;
        private Mobile m_Picker;
        private bool m_TrapOnLockpick;

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Picker
        {
            get
            {
                return m_Picker;
            }
            set
            {
                m_Picker = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxLockLevel
        {
            get
            {
                return m_MaxLockLevel;
            }
            set
            {
                m_MaxLockLevel = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int LockLevel
        {
            get
            {
                return m_LockLevel;
            }
            set
            {
                m_LockLevel = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int RequiredSkill
        {
            get
            {
                return m_RequiredSkill;
            }
            set
            {
                m_RequiredSkill = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public virtual bool Locked
        {
            get
            {
                return m_Locked;
            }
            set
            {
                m_Locked = value;

                if (m_Locked)
                    m_Picker = null;

                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public uint KeyValue
        {
            get
            {
                return m_KeyValue;
            }
            set
            {
                m_KeyValue = value;
            }
        }

        public override bool TrapOnOpen
        {
            get
            {
                return !m_TrapOnLockpick;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool TrapOnLockpick
        {
            get
            {
                return m_TrapOnLockpick;
            }
            set
            {
                m_TrapOnLockpick = value;
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)6); // version

            writer.Write(m_IsShipwreckedItem);

            writer.Write((bool)m_TrapOnLockpick);

            writer.Write((int)m_RequiredSkill);

            writer.Write((int)m_MaxLockLevel);

            writer.Write(m_KeyValue);
            writer.Write((int)m_LockLevel);
            writer.Write((bool)m_Locked);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 6:
                    {
                        m_IsShipwreckedItem = reader.ReadBool();

                        goto case 5;
                    }
                case 5:
                    {
                        m_TrapOnLockpick = reader.ReadBool();

                        goto case 4;
                    }
                case 4:
                    {
                        m_RequiredSkill = reader.ReadInt();

                        goto case 3;
                    }
                case 3:
                    {
                        m_MaxLockLevel = reader.ReadInt();

                        goto case 2;
                    }
                case 2:
                    {
                        m_KeyValue = reader.ReadUInt();

                        goto case 1;
                    }
                case 1:
                    {
                        m_LockLevel = reader.ReadInt();

                        goto case 0;
                    }
                case 0:
                    {
                        if (version < 3)
                            m_MaxLockLevel = 100;

                        if (version < 4)
                        {
                            if ((m_MaxLockLevel - m_LockLevel) == 40)
                            {
                                m_RequiredSkill = m_LockLevel + 6;
                                m_LockLevel = m_RequiredSkill - 10;
                                m_MaxLockLevel = m_RequiredSkill + 39;
                            }
                            else
                            {
                                m_RequiredSkill = m_LockLevel;
                            }
                        }

                        m_Locked = reader.ReadBool();

                        break;
                    }
            }
        }

        public LockableContainer(int itemID)
            : base(itemID)
        {
            m_MaxLockLevel = 100;
        }

        public LockableContainer(Serial serial)
            : base(serial)
        {
        }

        public override bool CheckContentDisplay(Mobile from)
        {
            return !m_Locked && base.CheckContentDisplay(from);
        }

        public override bool TryDropItem(Mobile from, Item dropped, bool sendFullMessage)
        {
            if (from.AccessLevel < AccessLevel.GameMaster && m_Locked)
            {
                from.SendLocalizedMessage(501747); // It appears to be locked.
                return false;
            }

            return base.TryDropItem(from, dropped, sendFullMessage);
        }

        public override bool OnDragDropInto(Mobile from, Item item, Point3D p)
        {
            if (from.AccessLevel < AccessLevel.GameMaster && m_Locked)
            {
                from.SendLocalizedMessage(501747); // It appears to be locked.
                return false;
            }

            return base.OnDragDropInto(from, item, p);
        }

        public override bool CheckLift(Mobile from, Item item, ref LRReason reject)
        {
            if (!base.CheckLift(from, item, ref reject))
                return false;

            if (item != this && from.AccessLevel < AccessLevel.GameMaster && m_Locked)
                return false;

            return true;
        }

        public override bool CheckItemUse(Mobile from, Item item)
        {
            if (!base.CheckItemUse(from, item))
                return false;

            if (item != this && from.AccessLevel < AccessLevel.GameMaster && m_Locked)
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
                return false;
            }

            return true;
        }

        public override bool DisplaysContent { get { return !m_Locked; } }

        public virtual bool CheckLocked(Mobile from)
        {
            bool inaccessible = false;

            if (m_Locked)
            {
                int number;

                if (from.AccessLevel >= AccessLevel.GameMaster)
                {
                    number = 502502; // That is locked, but you open it with your godly powers.
                }
                else
                {
                    number = 501747; // It appears to be locked.
                    inaccessible = true;
                }

                from.Send(new MessageLocalized(Serial, ItemID, MessageType.Regular, 0x3B2, 3, number, "", ""));
            }

            return inaccessible;
        }

        public override void OnTelekinesis(Mobile from)
        {
            if (CheckLocked(from))
            {
                Effects.SendLocationParticles(EffectItem.Create(Location, Map, EffectItem.DefaultDuration), 0x376A, 9, 32, 5022);
                Effects.PlaySound(Location, Map, 0x1F5);
                return;
            }

            base.OnTelekinesis(from);
        }

        public override void OnDoubleClickSecureTrade(Mobile from)
        {
            if (CheckLocked(from))
                return;

            base.OnDoubleClickSecureTrade(from);
        }

        public override void Open(Mobile from)
        {
            if (CheckLocked(from))
                return;

            base.Open(from);
        }

        public override void OnSnoop(Mobile from)
        {
            if (CheckLocked(from))
                return;

            base.OnSnoop(from);
        }

        public virtual void LockPick(Mobile from)
        {
            Locked = false;
            Picker = from;

            if (this.TrapOnLockpick && ExecuteTrap(from))
            {
                this.TrapOnLockpick = false;
            }
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            if (m_IsShipwreckedItem)
                list.Add(1041645); // recovered from a shipwreck
        }

        public override void OnSingleClick(Mobile from)
        {
            base.OnSingleClick(from);

            if (m_IsShipwreckedItem)
                LabelTo(from, 1041645);	//recovered from a shipwreck
        }

        #region ICraftable Members

        public int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool, CraftItem craftItem, int resHue)
        {
            if (from.CheckSkill(SkillName.Tinkering, -5.0, 15.0))
            {
                from.SendLocalizedMessage(500636); // Your tinker skill was sufficient to make the item lockable.

                Key key = new Key(KeyType.Copper, Key.RandomValue());

                KeyValue = key.KeyValue;
                DropItem(key);

                double tinkering = from.Skills[SkillName.Tinkering].Value;
                int level = (int)(tinkering * 0.8);

                RequiredSkill = level - 4;
                LockLevel = level - 14;
                MaxLockLevel = level + 35;

                if (LockLevel == 0)
                    LockLevel = -1;
                else if (LockLevel > 95)
                    LockLevel = 95;

                if (RequiredSkill > 95)
                    RequiredSkill = 95;

                if (MaxLockLevel > 95)
                    MaxLockLevel = 95;
            }
            else
            {
                from.SendLocalizedMessage(500637); // Your tinker skill was insufficient to make the item lockable.
            }

            return 1;
        }

        #endregion

        #region IShipwreckedItem Members

        private bool m_IsShipwreckedItem;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsShipwreckedItem
        {
            get { return m_IsShipwreckedItem; }
            set { m_IsShipwreckedItem = value; }
        }
        #endregion

    }

    public enum TrapType
    {
        None,
        MagicTrap,
        ExplosionTrap,
        DartTrap,
        PoisonTrap
    }

    public abstract class TrapableContainer : BaseContainer, ITelekinesisable
    {
        private TrapType m_TrapType;
        private int m_TrapPower;
        private int m_TrapLevel;

        [CommandProperty(AccessLevel.GameMaster)]
        public TrapType TrapType
        {
            get
            {
                return m_TrapType;
            }
            set
            {
                m_TrapType = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int TrapPower
        {
            get
            {
                return m_TrapPower;
            }
            set
            {
                m_TrapPower = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int TrapLevel
        {
            get
            {
                return m_TrapLevel;
            }
            set
            {
                m_TrapLevel = value;
            }
        }

        public virtual bool TrapOnOpen { get { return true; } }

        public TrapableContainer(int itemID)
            : base(itemID)
        {
        }

        public TrapableContainer(Serial serial)
            : base(serial)
        {
        }

        private void SendMessageTo(Mobile to, int number, int hue)
        {
            if (Deleted || !to.CanSee(this))
                return;

            to.Send(new Network.MessageLocalized(Serial, ItemID, Network.MessageType.Regular, hue, 3, number, "", ""));
        }

        private void SendMessageTo(Mobile to, string text, int hue)
        {
            if (Deleted || !to.CanSee(this))
                return;

            to.Send(new Network.UnicodeMessage(Serial, ItemID, Network.MessageType.Regular, hue, 3, "ENU", "", text));
        }

        public virtual bool ExecuteTrap(Mobile from)
        {
            if (m_TrapType != TrapType.None)
            {
                Point3D loc = this.GetWorldLocation();
                Map facet = this.Map;

                if (from.AccessLevel >= AccessLevel.GameMaster)
                {
                    SendMessageTo(from, "That is trapped, but you open it with your godly powers.", 0x3B2);
                    return false;
                }

                switch (m_TrapType)
                {
                    case TrapType.ExplosionTrap:
                        {
                            SendMessageTo(from, 502999, 0x3B2); // You set off a trap!

                            if (from.InRange(loc, 3))
                            {
                                int damage;

                                if (m_TrapLevel > 0)
                                    damage = Utility.RandomMinMax(10, 30) * m_TrapLevel;
                                else
                                    damage = m_TrapPower;

                                AOS.Damage(from, damage, 0, 100, 0, 0, 0);

                                // Your skin blisters from the heat!
                                from.LocalOverheadMessage(Network.MessageType.Regular, 0x2A, 503000);
                            }

                            Effects.SendLocationEffect(loc, facet, 0x36BD, 15, 10);
                            Effects.PlaySound(loc, facet, 0x307);

                            break;
                        }
                    case TrapType.MagicTrap:
                        {
                            if (from.InRange(loc, 1))
                                from.Damage(m_TrapPower);
                            //AOS.Damage( from, m_TrapPower, 0, 100, 0, 0, 0 );

                            Effects.PlaySound(loc, Map, 0x307);

                            Effects.SendLocationEffect(new Point3D(loc.X - 1, loc.Y, loc.Z), Map, 0x36BD, 15);
                            Effects.SendLocationEffect(new Point3D(loc.X + 1, loc.Y, loc.Z), Map, 0x36BD, 15);

                            Effects.SendLocationEffect(new Point3D(loc.X, loc.Y - 1, loc.Z), Map, 0x36BD, 15);
                            Effects.SendLocationEffect(new Point3D(loc.X, loc.Y + 1, loc.Z), Map, 0x36BD, 15);

                            Effects.SendLocationEffect(new Point3D(loc.X + 1, loc.Y + 1, loc.Z + 11), Map, 0x36BD, 15);

                            break;
                        }
                    case TrapType.DartTrap:
                        {
                            SendMessageTo(from, 502999, 0x3B2); // You set off a trap!

                            if (from.InRange(loc, 3))
                            {
                                int damage;

                                if (m_TrapLevel > 0)
                                    damage = Utility.RandomMinMax(5, 15) * m_TrapLevel;
                                else
                                    damage = m_TrapPower;

                                AOS.Damage(from, damage, 100, 0, 0, 0, 0);

                                // A dart imbeds itself in your flesh!
                                from.LocalOverheadMessage(Network.MessageType.Regular, 0x62, 502998);
                            }

                            Effects.PlaySound(loc, facet, 0x223);

                            break;
                        }
                    case TrapType.PoisonTrap:
                        {
                            SendMessageTo(from, 502999, 0x3B2); // You set off a trap!

                            if (from.InRange(loc, 3))
                            {
                                Poison poison;

                                if (m_TrapLevel > 0)
                                {
                                    poison = Poison.GetPoison(Math.Max(0, Math.Min(4, m_TrapLevel - 1)));
                                }
                                else
                                {
                                    AOS.Damage(from, m_TrapPower, 0, 0, 0, 100, 0);
                                    poison = Poison.Greater;
                                }

                                from.ApplyPoison(from, poison);

                                // You are enveloped in a noxious green cloud!
                                from.LocalOverheadMessage(Network.MessageType.Regular, 0x44, 503004);
                            }

                            Effects.SendLocationEffect(loc, facet, 0x113A, 10, 20);
                            Effects.PlaySound(loc, facet, 0x231);

                            break;
                        }
                }

                m_TrapType = TrapType.None;
                m_TrapPower = 0;
                m_TrapLevel = 0;
                return true;
            }

            return false;
        }

        public virtual void OnTelekinesis(Mobile from)
        {
            Effects.SendLocationParticles(EffectItem.Create(Location, Map, EffectItem.DefaultDuration), 0x376A, 9, 32, 5022);
            Effects.PlaySound(Location, Map, 0x1F5);

            if (this.TrapOnOpen)
            {
                ExecuteTrap(from);
            }
        }

        public override void Open(Mobile from)
        {
            if (!this.TrapOnOpen || !ExecuteTrap(from))
                base.Open(from);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)2); // version

            writer.Write((int)m_TrapLevel);

            writer.Write((int)m_TrapPower);
            writer.Write((int)m_TrapType);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 2:
                    {
                        m_TrapLevel = reader.ReadInt();
                        goto case 1;
                    }
                case 1:
                    {
                        m_TrapPower = reader.ReadInt();
                        goto case 0;
                    }
                case 0:
                    {
                        m_TrapType = (TrapType)reader.ReadInt();
                        break;
                    }
            }
        }
    }

    public abstract class BaseBagBall : BaseContainer, IDyable
    {
        public BaseBagBall(int itemID)
            : base(itemID)
        {
            Weight = 1.0;
        }

        public BaseBagBall(Serial serial)
            : base(serial)
        {
        }

        public bool Dye(Mobile from, DyeTub sender)
        {
            if (Deleted)
                return false;

            Hue = sender.DyedHue;

            return true;
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
        }
    }

    public abstract class FillableContainer : LockableContainer
    {
        public virtual int MinRespawnMinutes { get { return 60; } }
        public virtual int MaxRespawnMinutes { get { return 90; } }

        public virtual bool IsLockable { get { return true; } }
        public virtual bool IsTrapable { get { return IsLockable; } }

        public virtual int SpawnThreshold { get { return 2; } }

        protected FillableContent m_Content;

        protected DateTime m_NextRespawnTime;
        protected Timer m_RespawnTimer;

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextRespawnTime { get { return m_NextRespawnTime; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public FillableContentType ContentType
        {
            get { return FillableContent.Lookup(m_Content); }
            set { Content = FillableContent.Lookup(value); }
        }

        public FillableContent Content
        {
            get { return m_Content; }
            set
            {
                if (m_Content == value)
                    return;

                m_Content = value;

                for (int i = Items.Count - 1; i >= 0; --i)
                {
                    if (i < Items.Count)
                        Items[i].Delete();
                }

                Respawn();
            }
        }

        public FillableContainer(int itemID)
            : base(itemID)
        {
            Movable = false;
        }

        public override void OnMapChange()
        {
            base.OnMapChange();
            AcquireContent();
        }

        public override void OnLocationChange(Point3D oldLocation)
        {
            base.OnLocationChange(oldLocation);
            AcquireContent();
        }

        public virtual void AcquireContent()
        {
            if (m_Content != null)
                return;

            m_Content = FillableContent.Acquire(this.GetWorldLocation(), this.Map);

            if (m_Content != null)
                Respawn();
        }

        public override void OnItemRemoved(Item item)
        {
            CheckRespawn();
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            if (m_RespawnTimer != null)
            {
                m_RespawnTimer.Stop();
                m_RespawnTimer = null;
            }
        }

        public int GetItemsCount()
        {
            int count = 0;

            foreach (Item item in this.Items)
            {
                count += item.Amount;
            }

            return count;
        }

        public void CheckRespawn()
        {
            bool canSpawn = (m_Content != null && !Deleted && GetItemsCount() <= SpawnThreshold && !Movable && Parent == null && !IsLockedDown && !IsSecure);

            if (canSpawn)
            {
                if (m_RespawnTimer == null)
                {
                    int mins = Utility.RandomMinMax(this.MinRespawnMinutes, this.MaxRespawnMinutes);
                    TimeSpan delay = TimeSpan.FromMinutes(mins);

                    m_NextRespawnTime = DateTime.UtcNow + delay;
                    m_RespawnTimer = Timer.DelayCall(delay, new TimerCallback(Respawn));
                }
            }
            else if (m_RespawnTimer != null)
            {
                m_RespawnTimer.Stop();
                m_RespawnTimer = null;
            }
        }

        public void Respawn()
        {
            if (m_RespawnTimer != null)
            {
                m_RespawnTimer.Stop();
                m_RespawnTimer = null;
            }

            if (m_Content == null || Deleted)
                return;

            GenerateContent();

            if (IsLockable)
            {
                Locked = true;

                int difficulty = (m_Content.Level - 1) * 30;

                LockLevel = difficulty - 10;
                MaxLockLevel = difficulty + 30;
                RequiredSkill = difficulty;
            }

            if (IsTrapable && (m_Content.Level > 1 || 4 > Utility.Random(5)))
            {
                if (m_Content.Level > Utility.Random(5))
                    TrapType = TrapType.PoisonTrap;
                else
                    TrapType = TrapType.ExplosionTrap;

                TrapPower = m_Content.Level * Utility.RandomMinMax(10, 30);
                TrapLevel = m_Content.Level;
            }
            else
            {
                TrapType = TrapType.None;
                TrapPower = 0;
                TrapLevel = 0;
            }

            CheckRespawn();
        }

        protected virtual int GetSpawnCount()
        {
            int itemsCount = GetItemsCount();

            if (itemsCount > SpawnThreshold)
                return 0;

            int maxSpawnCount = (1 + SpawnThreshold - itemsCount) * 2;

            return Utility.RandomMinMax(0, maxSpawnCount);
        }

        public virtual void GenerateContent()
        {
            if (m_Content == null || Deleted)
                return;

            int toSpawn = GetSpawnCount();

            for (int i = 0; i < toSpawn; ++i)
            {
                Item item = m_Content.Construct();

                if (item != null)
                {
                    List<Item> list = this.Items;

                    for (int j = 0; j < list.Count; ++j)
                    {
                        Item subItem = list[j];

                        if (!(subItem is Container) && subItem.StackWith(null, item, false))
                            break;
                    }

                    if (item != null && !item.Deleted)
                        DropItem(item);
                }
            }
        }

        public FillableContainer(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(1); // version

            writer.Write((int)ContentType);

            if (m_RespawnTimer != null)
            {
                writer.Write(true);
                writer.WriteDeltaTime((DateTime)m_NextRespawnTime);
            }
            else
            {
                writer.Write(false);
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();

            switch (version)
            {
                case 1:
                    {
                        m_Content = FillableContent.Lookup((FillableContentType)reader.ReadInt());
                        goto case 0;
                    }
                case 0:
                    {
                        if (reader.ReadBool())
                        {
                            m_NextRespawnTime = reader.ReadDeltaTime();

                            TimeSpan delay = m_NextRespawnTime - DateTime.UtcNow;
                            m_RespawnTimer = Timer.DelayCall(delay > TimeSpan.Zero ? delay : TimeSpan.Zero, new TimerCallback(Respawn));
                        }
                        else
                        {
                            CheckRespawn();
                        }

                        break;
                    }
            }
        }
    }

    public abstract class BaseWaterContainer : Container, IHasQuantity
    {
        public abstract int voidItem_ID { get; }
        public abstract int fullItem_ID { get; }
        public abstract int MaxQuantity { get; }

        public override int DefaultGumpID { get { return 0x3e; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public virtual bool IsEmpty { get { return (m_Quantity <= 0); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public virtual bool IsFull { get { return (m_Quantity >= MaxQuantity); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public virtual int Quantity
        {
            get
            {
                return m_Quantity;
            }
            set
            {
                if (value != m_Quantity)
                {
                    m_Quantity = (value < 1) ? 0 : (value > MaxQuantity) ? MaxQuantity : value;

                    Movable = (!IsLockedDown) ? IsEmpty : false;

                    ItemID = (IsEmpty) ? voidItem_ID : fullItem_ID;

                    if (!IsEmpty)
                    {
                        IEntity rootParent = RootParent;

                        if (rootParent != null && rootParent.Map != null && rootParent.Map != Map.Internal)
                            MoveToWorld(rootParent.Location, rootParent.Map);
                    }

                    InvalidateProperties();
                }
            }
        }

        private int m_Quantity;

        public BaseWaterContainer(int Item_Id, bool filled)
            : base(Item_Id)
        {
            m_Quantity = (filled) ? MaxQuantity : 0;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsEmpty)
            {
                base.OnDoubleClick(from);
            }
        }

        public override void OnSingleClick(Mobile from)
        {
            if (IsEmpty)
            {
                base.OnSingleClick(from);
            }
            else
            {
                if (Name == null)
                    LabelTo(from, LabelNumber);
                else
                    LabelTo(from, Name);
            }
        }

        public override void OnAosSingleClick(Mobile from)
        {
            if (IsEmpty)
            {
                base.OnAosSingleClick(from);
            }
            else
            {
                if (Name == null)
                    LabelTo(from, LabelNumber);
                else
                    LabelTo(from, Name);
            }
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            if (IsEmpty)
            {
                base.GetProperties(list);
            }
        }

        public override bool OnDragDropInto(Mobile from, Item item, Point3D p)
        {
            if (!IsEmpty)
            {
                return false;
            }

            return base.OnDragDropInto(from, item, p);
        }

        public BaseWaterContainer(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
            writer.Write((int)m_Quantity);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
            m_Quantity = reader.ReadInt();
        }
    }

    public class FillableEntry
    {
        protected Type[] m_Types;
        protected int m_Weight;

        public Type[] Types { get { return m_Types; } }
        public int Weight { get { return m_Weight; } }

        public FillableEntry(Type type)
            : this(1, new Type[] { type })
        {
        }

        public FillableEntry(int weight, Type type)
            : this(weight, new Type[] { type })
        {
        }

        public FillableEntry(Type[] types)
            : this(1, types)
        {
        }

        public FillableEntry(int weight, Type[] types)
        {
            m_Weight = weight;
            m_Types = types;
        }

        public FillableEntry(int weight, Type[] types, int offset, int count)
        {
            m_Weight = weight;
            m_Types = new Type[count];

            for (int i = 0; i < m_Types.Length; ++i)
                m_Types[i] = types[offset + i];
        }

        public virtual Item Construct()
        {
            Item item = Loot.Construct(m_Types);

            if (item is Key)
                ((Key)item).ItemID = Utility.RandomList((int)KeyType.Copper, (int)KeyType.Gold, (int)KeyType.Iron, (int)KeyType.Rusty);
            else if (item is Arrow || item is Bolt)
                item.Amount = Utility.RandomMinMax(2, 6);
            else if (item is Bandage || item is Lockpick)
                item.Amount = Utility.RandomMinMax(1, 3);

            return item;
        }
    }

    public class FillableBvrge : FillableEntry
    {
        private BeverageType m_Content;

        public BeverageType Content { get { return m_Content; } }

        public FillableBvrge(Type type, BeverageType content)
            : this(1, type, content)
        {
        }

        public FillableBvrge(int weight, Type type, BeverageType content)
            : base(weight, type)
        {
            m_Content = content;
        }

        public override Item Construct()
        {
            Item item;

            int index = Utility.Random(m_Types.Length);

            if (m_Types[index] == typeof(BeverageBottle))
            {
                item = new BeverageBottle(m_Content);
            }
            else if (m_Types[index] == typeof(Jug))
            {
                item = new Jug(m_Content);
            }
            else
            {
                item = base.Construct();

                if (item is BaseBeverage)
                {
                    BaseBeverage bev = (BaseBeverage)item;

                    bev.Content = m_Content;
                    bev.Quantity = bev.MaxQuantity;
                }
            }

            return item;
        }
    }

    public enum FillableContentType
    {
        None = -1,
        Weaponsmith, Provisioner, Mage,
        Alchemist, Armorer, ArtisanGuild,
        Baker, Bard, Blacksmith,
        Bowyer, Butcher, Carpenter,
        Clothier, Cobbler, Docks,
        Farm, FighterGuild, Guard,
        Healer, Herbalist, Inn,
        Jeweler, Library, Merchant,
        Mill, Mine, Observatory,
        Painter, Ranger, Stables,
        Tanner, Tavern, ThiefGuild,
        Tinker, Veterinarian
    }

    public class FillableContent
    {
        private int m_Level;
        private Type[] m_Vendors;

        private FillableEntry[] m_Entries;
        private int m_Weight;

        public int Level { get { return m_Level; } }
        public Type[] Vendors { get { return m_Vendors; } }

        public FillableContentType TypeID { get { return Lookup(this); } }

        public FillableContent(int level, Type[] vendors, FillableEntry[] entries)
        {
            m_Level = level;
            m_Vendors = vendors;
            m_Entries = entries;

            for (int i = 0; i < entries.Length; ++i)
                m_Weight += entries[i].Weight;
        }

        public virtual Item Construct()
        {
            int index = Utility.Random(m_Weight);

            for (int i = 0; i < m_Entries.Length; ++i)
            {
                FillableEntry entry = m_Entries[i];

                if (index < entry.Weight)
                    return entry.Construct();

                index -= entry.Weight;
            }

            return null;
        }

        public static FillableContent Alchemist = new FillableContent(
            1,
            new Type[]
			{
				typeof( Mobiles.Alchemist )
			},
            new FillableEntry[]
			{
				new FillableEntry( typeof( NightSightPotion ) ),
				new FillableEntry( typeof( LesserCurePotion ) ),
				new FillableEntry( typeof( AgilityPotion ) ),
				new FillableEntry( typeof( StrengthPotion ) ),
				new FillableEntry( typeof( LesserPoisonPotion ) ),
				new FillableEntry( typeof( RefreshPotion ) ),
				new FillableEntry( typeof( LesserHealPotion ) ),
				new FillableEntry( typeof( LesserExplosionPotion ) ),
				new FillableEntry( typeof( MortarPestle ) )
			});

        public static FillableContent Armorer = new FillableContent(
            2,
            new Type[]
			{
				typeof( Armorer )
			},
            new FillableEntry[]
			{
				new FillableEntry( 2, typeof( ChainCoif ) ),
				new FillableEntry( 1, typeof( PlateGorget ) ),
				new FillableEntry( 1, typeof( BronzeShield ) ),
				new FillableEntry( 1, typeof( Buckler ) ),
				new FillableEntry( 2, typeof( MetalKiteShield ) ),
				new FillableEntry( 2, typeof( HeaterShield ) ),
				new FillableEntry( 1, typeof( WoodenShield ) ),
				new FillableEntry( 1, typeof( MetalShield ) )
			});

        public static FillableContent ArtisanGuild = new FillableContent(
            1,
            new Type[]
			{
			},
            new FillableEntry[]
			{
				new FillableEntry( 1, typeof( PaintsAndBrush ) ),
				new FillableEntry( 1, typeof( SledgeHammer ) ),
				new FillableEntry( 2, typeof( SmithHammer ) ),
				new FillableEntry( 2, typeof( Tongs ) ),
				new FillableEntry( 4, typeof( Lockpick ) ),
				new FillableEntry( 4, typeof( TinkerTools ) ),
				new FillableEntry( 1, typeof( MalletAndChisel ) ),
				new FillableEntry( 1, typeof( StatueEast2 ) ),
				new FillableEntry( 1, typeof( StatueSouth ) ),
				new FillableEntry( 1, typeof( StatueSouthEast ) ),
				new FillableEntry( 1, typeof( StatueWest ) ),
				new FillableEntry( 1, typeof( StatueNorth ) ),
				new FillableEntry( 1, typeof( StatueEast ) ),
				new FillableEntry( 1, typeof( BustEast ) ),
				new FillableEntry( 1, typeof( BustSouth ) ),
				new FillableEntry( 1, typeof( BearMask ) ),
				new FillableEntry( 1, typeof( DeerMask ) ),
				new FillableEntry( 4, typeof( OrcHelm ) ),
				new FillableEntry( 1, typeof( TribalMask ) ),
				new FillableEntry( 1, typeof( HornedTribalMask ) )
			});

        public static FillableContent Baker = new FillableContent(
            1,
            new Type[]
			{
				typeof( Baker ),
			},
            new FillableEntry[]
			{
				new FillableEntry( 1, typeof( RollingPin ) ),
				new FillableEntry( 2, typeof( SackFlour ) ),
				new FillableEntry( 2, typeof( BreadLoaf ) ),
				new FillableEntry( 1, typeof( FrenchBread ) )
			});

        public static FillableContent Bard = new FillableContent(
            1,
            new Type[]
			{
				typeof( Bard ),
				typeof( BardGuildmaster )
			},
            new FillableEntry[]
			{
				new FillableEntry( 1, typeof( LapHarp ) ),
				new FillableEntry( 2, typeof( Lute ) ),
				new FillableEntry( 1, typeof( Drums ) ),
				new FillableEntry( 1, typeof( Tambourine ) ),
				new FillableEntry( 1, typeof( TambourineTassel ) )
			});

        public static FillableContent Blacksmith = new FillableContent(
            2,
            new Type[]
			{
				typeof( Blacksmith ),
				typeof( BlacksmithGuildmaster )
			},
            new FillableEntry[]
			{
				new FillableEntry( 8, typeof( SmithHammer ) ),
				new FillableEntry( 8, typeof( Tongs ) ),
				new FillableEntry( 8, typeof( SledgeHammer ) ),
				//new FillableEntry( 8, typeof( IronOre ) ), TODO: Smaller ore
				new FillableEntry( 8, typeof( IronIngot ) ),
				new FillableEntry( 1, typeof( IronWire ) ),
				new FillableEntry( 1, typeof( SilverWire ) ),
				new FillableEntry( 1, typeof( GoldWire ) ),
				new FillableEntry( 1, typeof( CopperWire ) ),
				new FillableEntry( 1, typeof( HorseShoes ) ),
				new FillableEntry( 1, typeof( ForgedMetal ) )
			});

        public static FillableContent Bowyer = new FillableContent(
            2,
            new Type[]
			{
				typeof( Bowyer )
			},
            new FillableEntry[]
			{
				new FillableEntry( 2, typeof( Bow ) ),
				new FillableEntry( 2, typeof( Crossbow ) ),
				new FillableEntry( 1, typeof( Arrow ) )
			});

        public static FillableContent Butcher = new FillableContent(
            1,
            new Type[]
			{
				typeof( Butcher ),
			},
            new FillableEntry[]
			{
				new FillableEntry( 2, typeof( Cleaver ) ),
				new FillableEntry( 2, typeof( SlabOfBacon ) ),
				new FillableEntry( 2, typeof( Bacon ) ),
				new FillableEntry( 1, typeof( RawFishSteak ) ),
				new FillableEntry( 1, typeof( FishSteak ) ),
				new FillableEntry( 2, typeof( CookedBird ) ),
				new FillableEntry( 2, typeof( RawBird ) ),
				new FillableEntry( 2, typeof( Ham ) ),
				new FillableEntry( 1, typeof( RawLambLeg ) ),
				new FillableEntry( 1, typeof( LambLeg ) ),
				new FillableEntry( 1, typeof( Ribs ) ),
				new FillableEntry( 1, typeof( RawRibs ) ),
				new FillableEntry( 2, typeof( Sausage ) ),
				new FillableEntry( 1, typeof( RawChickenLeg ) ),
				new FillableEntry( 1, typeof( ChickenLeg ) )
			});

        public static FillableContent Carpenter = new FillableContent(
            1,
            new Type[]
			{
				typeof( Carpenter ),
				typeof( Architect ),
				typeof( RealEstateBroker )
			},
            new FillableEntry[]
			{
				new FillableEntry( 1, typeof( ChiselsNorth ) ),
				new FillableEntry( 1, typeof( ChiselsWest ) ),
				new FillableEntry( 2, typeof( DovetailSaw ) ),
				new FillableEntry( 2, typeof( Hammer ) ),
				new FillableEntry( 2, typeof( MouldingPlane ) ),
				new FillableEntry( 2, typeof( Nails ) ),
				new FillableEntry( 2, typeof( JointingPlane ) ),
				new FillableEntry( 2, typeof( SmoothingPlane ) ),
				new FillableEntry( 2, typeof( Saw ) ),
				new FillableEntry( 2, typeof( DrawKnife ) ),
				new FillableEntry( 1, typeof( Log ) ),
				new FillableEntry( 1, typeof( Froe ) ),
				new FillableEntry( 1, typeof( Inshave ) ),
				new FillableEntry( 1, typeof( Scorp ) )
			});

        public static FillableContent Clothier = new FillableContent(
            1,
            new Type[]
			{
				typeof( Tailor ),
				typeof( Weaver ),
				typeof( TailorGuildmaster )
			},
            new FillableEntry[]
			{
				new FillableEntry( 1, typeof( Cotton ) ),
				new FillableEntry( 1, typeof( Wool ) ),
				new FillableEntry( 1, typeof( DarkYarn ) ),
				new FillableEntry( 1, typeof( LightYarn ) ),
				new FillableEntry( 1, typeof( LightYarnUnraveled ) ),
				new FillableEntry( 1, typeof( SpoolOfThread ) ),
				// Four different types
				//new FillableEntry( 1, typeof( FoldedCloth ) ),
				//new FillableEntry( 1, typeof( FoldedCloth ) ),
				//new FillableEntry( 1, typeof( FoldedCloth ) ),
				//new FillableEntry( 1, typeof( FoldedCloth ) ),
				new FillableEntry( 1, typeof( Dyes ) ),
				new FillableEntry( 2, typeof( Leather ) )
			});

        public static FillableContent Cobbler = new FillableContent(
            1,
            new Type[]
			{
				typeof( Cobbler )
			},
            new FillableEntry[]
			{
				new FillableEntry( 1, typeof( Boots ) ),
				new FillableEntry( 2, typeof( Shoes ) ),
				new FillableEntry( 2, typeof( Sandals ) ),
				new FillableEntry( 1, typeof( ThighBoots ) )
			});

        public static FillableContent Docks = new FillableContent(
            1,
            new Type[]
			{
				typeof( Fisherman ),
				typeof( FisherGuildmaster )
			},
            new FillableEntry[]
			{
				new FillableEntry( 1, typeof( FishingPole ) ),
				// Two different types
				//new FillableEntry( 1, typeof( SmallFish ) ),
				//new FillableEntry( 1, typeof( SmallFish ) ),
				new FillableEntry( 4, typeof( Fish ) )
			});

        public static FillableContent Farm = new FillableContent(
            1,
            new Type[]
			{
				typeof( Farmer ),
				typeof( Rancher )
			},
            new FillableEntry[]
			{
				new FillableEntry( 1, typeof( Shirt ) ),
				new FillableEntry( 1, typeof( ShortPants ) ),
				new FillableEntry( 1, typeof( Skirt ) ),
				new FillableEntry( 1, typeof( PlainDress ) ),
				new FillableEntry( 1, typeof( Cap ) ),
				new FillableEntry( 2, typeof( Sandals ) ),
				new FillableEntry( 2, typeof( GnarledStaff ) ),
				new FillableEntry( 2, typeof( Pitchfork ) ),
				new FillableEntry( 1, typeof( Bag ) ),
				new FillableEntry( 1, typeof( Kindling ) ),
				new FillableEntry( 1, typeof( Lettuce ) ),
				new FillableEntry( 1, typeof( Onion ) ),
				new FillableEntry( 1, typeof( Turnip ) ),
				new FillableEntry( 1, typeof( Ham ) ),
				new FillableEntry( 1, typeof( Bacon ) ),
				new FillableEntry( 1, typeof( RawLambLeg ) ),
				new FillableEntry( 1, typeof( SheafOfHay ) ),
				new FillableBvrge( 1, typeof( Pitcher ), BeverageType.Milk )
			});

        public static FillableContent FighterGuild = new FillableContent(
            3,
            new Type[]
			{
				typeof( WarriorGuildmaster )
			},
            new FillableEntry[]
			{
				new FillableEntry( 12, Loot.ArmorTypes ),
				new FillableEntry(  8, Loot.WeaponTypes ),
				new FillableEntry(  3, Loot.ShieldTypes ),
				new FillableEntry(  1, typeof( Arrow ) )
			});

        public static FillableContent Guard = new FillableContent(
            3,
            new Type[]
			{
			},
            new FillableEntry[]
			{
				new FillableEntry( 12, Loot.ArmorTypes ),
				new FillableEntry(  8, Loot.WeaponTypes ),
				new FillableEntry(  3, Loot.ShieldTypes ),
				new FillableEntry(  1, typeof( Arrow ) )
			});

        public static FillableContent Healer = new FillableContent(
            1,
            new Type[]
			{
				typeof( Healer ),
				typeof( HealerGuildmaster )
			},
            new FillableEntry[]
			{
				new FillableEntry( 1, typeof( Bandage ) ),
				new FillableEntry( 1, typeof( MortarPestle ) ),
				new FillableEntry( 1, typeof( LesserHealPotion ) )
			});

        public static FillableContent Herbalist = new FillableContent(
            1,
            new Type[]
			{
				typeof( Herbalist )
			},
            new FillableEntry[]
			{
				new FillableEntry( 10, typeof( Garlic ) ),
				new FillableEntry( 10, typeof( Ginseng ) ),
				new FillableEntry( 10, typeof( MandrakeRoot ) ),
				new FillableEntry(  1, typeof( DeadWood ) ),
				new FillableEntry(  1, typeof( WhiteDriedFlowers ) ),
				new FillableEntry(  1, typeof( GreenDriedFlowers ) ),
				new FillableEntry(  1, typeof( DriedOnions ) ),
				new FillableEntry(  1, typeof( DriedHerbs ) )
			});

        public static FillableContent Inn = new FillableContent(
            1,
            new Type[]
			{
			},
            new FillableEntry[]
			{
				new FillableEntry( 1, typeof( Candle ) ),
				new FillableEntry( 1, typeof( Torch ) ),
				new FillableEntry( 1, typeof( Lantern ) )
			});

        public static FillableContent Jeweler = new FillableContent(
            2,
            new Type[]
			{
				typeof( Jeweler )
			},
            new FillableEntry[]
			{
				new FillableEntry( 1, typeof( GoldRing ) ),
				new FillableEntry( 1, typeof( GoldBracelet ) ),
				new FillableEntry( 1, typeof( GoldEarrings ) ),
				new FillableEntry( 1, typeof( GoldNecklace ) ),
				new FillableEntry( 1, typeof( GoldBeadNecklace ) ),
				new FillableEntry( 1, typeof( Necklace ) ),
				new FillableEntry( 1, typeof( Beads ) ),
				new FillableEntry( 9, Loot.GemTypes )
			});

        public static FillableContent Library = new FillableContent(
            1,
            new Type[]
			{
				typeof( Scribe )
			},
            new FillableEntry[]
			{
				new FillableEntry( 8, Loot.LibraryBookTypes ),
				new FillableEntry( 1, typeof( RedBook ) ),
				new FillableEntry( 1, typeof( BlueBook ) )
			});

        public static FillableContent Mage = new FillableContent(
            2,
            new Type[]
			{
				typeof( Mage ),
				typeof( HolyMage ),
				typeof( MageGuildmaster )
			},
            new FillableEntry[]
			{
				new FillableEntry( 16, typeof( BlankScroll ) ),
				new FillableEntry( 14, typeof( Spellbook ) ),
				new FillableEntry( 12, Loot.RegularScrollTypes,  0, 8 ),
				new FillableEntry( 11, Loot.RegularScrollTypes,  8, 8 ),
				new FillableEntry( 10, Loot.RegularScrollTypes, 16, 8 ),
				new FillableEntry(  9, Loot.RegularScrollTypes, 24, 8 ),
				new FillableEntry(  8, Loot.RegularScrollTypes, 32, 8 ),
				new FillableEntry(  7, Loot.RegularScrollTypes, 40, 8 ),
				new FillableEntry(  6, Loot.RegularScrollTypes, 48, 8 ),
				new FillableEntry(  5, Loot.RegularScrollTypes, 56, 8 )
			});

        public static FillableContent Merchant = new FillableContent(
            1,
            new Type[]
			{
				typeof( MerchantGuildmaster )
			},
            new FillableEntry[]
			{
				new FillableEntry( 1, typeof( CheeseWheel ) ),
				new FillableEntry( 1, typeof( CheeseWedge ) ),
				new FillableEntry( 1, typeof( CheeseSlice ) ),
				new FillableEntry( 1, typeof( Eggs ) ),
				new FillableEntry( 4, typeof( Fish ) ),
				new FillableEntry( 2, typeof( RawFishSteak ) ),
				new FillableEntry( 2, typeof( FishSteak ) ),
				new FillableEntry( 1, typeof( Apple ) ),
				new FillableEntry( 2, typeof( Banana ) ),
				new FillableEntry( 2, typeof( Bananas ) ),
				new FillableEntry( 2, typeof( OpenCoconut ) ),
				new FillableEntry( 1, typeof( SplitCoconut ) ),
				new FillableEntry( 1, typeof( Coconut ) ),
				new FillableEntry( 1, typeof( Dates ) ),
				new FillableEntry( 1, typeof( Grapes ) ),
				new FillableEntry( 1, typeof( Lemon ) ),
				new FillableEntry( 1, typeof( Lemons ) ),
				new FillableEntry( 1, typeof( Lime ) ),
				new FillableEntry( 1, typeof( Limes ) ),
				new FillableEntry( 1, typeof( Peach ) ),
				new FillableEntry( 1, typeof( Pear ) ),
				new FillableEntry( 2, typeof( SlabOfBacon ) ),
				new FillableEntry( 2, typeof( Bacon ) ),
				new FillableEntry( 2, typeof( CookedBird ) ),
				new FillableEntry( 2, typeof( RawBird ) ),
				new FillableEntry( 2, typeof( Ham ) ),
				new FillableEntry( 1, typeof( RawLambLeg ) ),
				new FillableEntry( 1, typeof( LambLeg ) ),
				new FillableEntry( 1, typeof( Ribs ) ),
				new FillableEntry( 1, typeof( RawRibs ) ),
				new FillableEntry( 2, typeof( Sausage ) ),
				new FillableEntry( 1, typeof( RawChickenLeg ) ),
				new FillableEntry( 1, typeof( ChickenLeg ) ),
				new FillableEntry( 1, typeof( Watermelon ) ),
				new FillableEntry( 1, typeof( SmallWatermelon ) ),
				new FillableEntry( 3, typeof( Turnip ) ),
				new FillableEntry( 2, typeof( YellowGourd ) ),
				new FillableEntry( 2, typeof( GreenGourd ) ),
				new FillableEntry( 2, typeof( Pumpkin ) ),
				new FillableEntry( 1, typeof( SmallPumpkin ) ),
				new FillableEntry( 2, typeof( Onion ) ),
				new FillableEntry( 2, typeof( Lettuce ) ),
				new FillableEntry( 2, typeof( Squash ) ),
				new FillableEntry( 2, typeof( HoneydewMelon ) ),
				new FillableEntry( 1, typeof( Carrot ) ),
				new FillableEntry( 2, typeof( Cantaloupe ) ),
				new FillableEntry( 2, typeof( Cabbage ) ),
				new FillableEntry( 4, typeof( EarOfCorn ) )
			});

        public static FillableContent Mill = new FillableContent(
            1,
            new Type[]
			{
			},
            new FillableEntry[]
			{
				new FillableEntry( 1, typeof( SackFlour ) )
			});

        public static FillableContent Mine = new FillableContent(
            1,
            new Type[]
			{
				typeof( Miner )
			},
            new FillableEntry[]
			{
				new FillableEntry( 2, typeof( Pickaxe ) ),
				new FillableEntry( 2, typeof( Shovel ) ),
				new FillableEntry( 2, typeof( IronIngot ) ),
				//new FillableEntry( 2, typeof( IronOre ) ),	TODO: Smaller Ore
				new FillableEntry( 1, typeof( ForgedMetal ) )
			});

        public static FillableContent Observatory = new FillableContent(
            1,
            new Type[]
			{
			},
            new FillableEntry[]
			{
				new FillableEntry( 2, typeof( Sextant ) ),
				new FillableEntry( 2, typeof( Clock ) ),
				new FillableEntry( 1, typeof( Spyglass ) )
			});

        public static FillableContent Painter = new FillableContent(
            1,
            new Type[]
			{
			},
            new FillableEntry[]
			{
				new FillableEntry( 1, typeof( PaintsAndBrush ) ),
				new FillableEntry( 2, typeof( PenAndInk ) )
			});

        public static FillableContent Provisioner = new FillableContent(
            1,
            new Type[]
			{
				typeof( Provisioner )
			},
            new FillableEntry[]
			{
				new FillableEntry( 1, typeof( CheeseWheel ) ),
				new FillableEntry( 1, typeof( CheeseWedge ) ),
				new FillableEntry( 1, typeof( CheeseSlice ) ),
				new FillableEntry( 1, typeof( Eggs ) ),
				new FillableEntry( 4, typeof( Fish ) ),
				new FillableEntry( 1, typeof( DirtyFrypan ) ),
				new FillableEntry( 1, typeof( DirtyPan ) ),
				new FillableEntry( 1, typeof( DirtyKettle ) ),
				new FillableEntry( 1, typeof( DirtySmallRoundPot ) ),
				new FillableEntry( 1, typeof( DirtyRoundPot ) ),
				new FillableEntry( 1, typeof( DirtySmallPot ) ),
				new FillableEntry( 1, typeof( DirtyPot ) ),
				new FillableEntry( 1, typeof( Apple ) ),
				new FillableEntry( 2, typeof( Banana ) ),
				new FillableEntry( 2, typeof( Bananas ) ),
				new FillableEntry( 2, typeof( OpenCoconut ) ),
				new FillableEntry( 1, typeof( SplitCoconut ) ),
				new FillableEntry( 1, typeof( Coconut ) ),
				new FillableEntry( 1, typeof( Dates ) ),
				new FillableEntry( 1, typeof( Grapes ) ),
				new FillableEntry( 1, typeof( Lemon ) ),
				new FillableEntry( 1, typeof( Lemons ) ),
				new FillableEntry( 1, typeof( Lime ) ),
				new FillableEntry( 1, typeof( Limes ) ),
				new FillableEntry( 1, typeof( Peach ) ),
				new FillableEntry( 1, typeof( Pear ) ),
				new FillableEntry( 2, typeof( SlabOfBacon ) ),
				new FillableEntry( 2, typeof( Bacon ) ),
				new FillableEntry( 1, typeof( RawFishSteak ) ),
				new FillableEntry( 1, typeof( FishSteak ) ),
				new FillableEntry( 2, typeof( CookedBird ) ),
				new FillableEntry( 2, typeof( RawBird ) ),
				new FillableEntry( 2, typeof( Ham ) ),
				new FillableEntry( 1, typeof( RawLambLeg ) ),
				new FillableEntry( 1, typeof( LambLeg ) ),
				new FillableEntry( 1, typeof( Ribs ) ),
				new FillableEntry( 1, typeof( RawRibs ) ),
				new FillableEntry( 2, typeof( Sausage ) ),
				new FillableEntry( 1, typeof( RawChickenLeg ) ),
				new FillableEntry( 1, typeof( ChickenLeg ) ),
				new FillableEntry( 1, typeof( Watermelon ) ),
				new FillableEntry( 1, typeof( SmallWatermelon ) ),
				new FillableEntry( 3, typeof( Turnip ) ),
				new FillableEntry( 2, typeof( YellowGourd ) ),
				new FillableEntry( 2, typeof( GreenGourd ) ),
				new FillableEntry( 2, typeof( Pumpkin ) ),
				new FillableEntry( 1, typeof( SmallPumpkin ) ),
				new FillableEntry( 2, typeof( Onion ) ),
				new FillableEntry( 2, typeof( Lettuce ) ),
				new FillableEntry( 2, typeof( Squash ) ),
				new FillableEntry( 2, typeof( HoneydewMelon ) ),
				new FillableEntry( 1, typeof( Carrot ) ),
				new FillableEntry( 2, typeof( Cantaloupe ) ),
				new FillableEntry( 2, typeof( Cabbage ) ),
				new FillableEntry( 4, typeof( EarOfCorn ) )
			});

        public static FillableContent Ranger = new FillableContent(
            2,
            new Type[]
			{
				typeof( Ranger ),
				typeof( RangerGuildmaster )
			},
            new FillableEntry[]
			{
				new FillableEntry( 2, typeof( StuddedChest ) ),
				new FillableEntry( 2, typeof( StuddedLegs ) ),
				new FillableEntry( 2, typeof( StuddedArms ) ),
				new FillableEntry( 2, typeof( StuddedGloves ) ),
				new FillableEntry( 1, typeof( StuddedGorget ) ),

				new FillableEntry( 2, typeof( LeatherChest ) ),
				new FillableEntry( 2, typeof( LeatherLegs ) ),
				new FillableEntry( 2, typeof( LeatherArms ) ),
				new FillableEntry( 2, typeof( LeatherGloves ) ),
				new FillableEntry( 1, typeof( LeatherGorget ) ),

				new FillableEntry( 2, typeof( FeatheredHat ) ),
				new FillableEntry( 1, typeof( CloseHelm ) ),
				new FillableEntry( 1, typeof( TallStrawHat ) ),
				new FillableEntry( 1, typeof( Bandana ) ),
				new FillableEntry( 1, typeof( Cloak ) ),
				new FillableEntry( 2, typeof( Boots ) ),
				new FillableEntry( 2, typeof( ThighBoots ) ),

				new FillableEntry( 2, typeof( GnarledStaff ) ),
				new FillableEntry( 1, typeof( Whip ) ),

				new FillableEntry( 2, typeof( Bow ) ),
				new FillableEntry( 2, typeof( Crossbow ) ),
				new FillableEntry( 2, typeof( HeavyCrossbow ) ),
				new FillableEntry( 4, typeof( Arrow ) )
			});

        public static FillableContent Stables = new FillableContent(
            1,
            new Type[]
			{
				typeof( AnimalTrainer ),
				typeof( GypsyAnimalTrainer )
			},
            new FillableEntry[]
			{
				//new FillableEntry( 1, typeof( Wheat ) ),
				new FillableEntry( 1, typeof( Carrot ) )
			});

        public static FillableContent Tanner = new FillableContent(
            2,
            new Type[]
			{
				typeof( Tanner ),
				typeof( LeatherWorker ),
				typeof( Furtrader )
			},
            new FillableEntry[]
			{
				new FillableEntry( 1, typeof( FeatheredHat ) ),
				new FillableEntry( 1, typeof( LeatherArms ) ),
				new FillableEntry( 2, typeof( LeatherLegs ) ),
				new FillableEntry( 2, typeof( LeatherChest ) ),
				new FillableEntry( 2, typeof( LeatherGloves ) ),
				new FillableEntry( 1, typeof( LeatherGorget ) ),
				new FillableEntry( 2, typeof( Leather ) )
			});

        public static FillableContent Tavern = new FillableContent(
            1,
            new Type[]
			{
				typeof( TavernKeeper ),
				typeof( Barkeeper ),
				typeof( Waiter ),
				typeof( Cook )
			},
            new FillableEntry[]
			{
				new FillableBvrge( 1, typeof( BeverageBottle ), BeverageType.Ale ),
				new FillableBvrge( 1, typeof( BeverageBottle ), BeverageType.Wine ),
				new FillableBvrge( 1, typeof( BeverageBottle ), BeverageType.Liquor ),
				new FillableBvrge( 1, typeof( Jug ), BeverageType.Cider )
			});

        public static FillableContent ThiefGuild = new FillableContent(
            1,
            new Type[]
			{
				typeof( Thief ),
				typeof( ThiefGuildmaster )
			},
            new FillableEntry[]
			{
				new FillableEntry( 1, typeof( Lockpick ) ),
				new FillableEntry( 1, typeof( BearMask ) ),
				new FillableEntry( 1, typeof( DeerMask ) ),
				new FillableEntry( 1, typeof( TribalMask ) ),
				new FillableEntry( 1, typeof( HornedTribalMask ) ),
				new FillableEntry( 4, typeof( OrcHelm ) )
			});

        public static FillableContent Tinker = new FillableContent(
            1,
            new Type[]
			{
				typeof( Tinker ),
				typeof( TinkerGuildmaster )
			},
            new FillableEntry[]
			{
				new FillableEntry( 1, typeof( Lockpick ) ),
				//new FillableEntry( 1, typeof( KeyRing ) ),
				new FillableEntry( 2, typeof( Clock ) ),
				new FillableEntry( 2, typeof( ClockParts ) ),
				new FillableEntry( 2, typeof( AxleGears ) ),
				new FillableEntry( 2, typeof( Gears ) ),
				new FillableEntry( 2, typeof( Hinge ) ),
				//new FillableEntry( 1, typeof( ArrowShafts ) ),
				new FillableEntry( 2, typeof( Sextant ) ),
				new FillableEntry( 2, typeof( SextantParts ) ),
				new FillableEntry( 2, typeof( Axle ) ),
				new FillableEntry( 2, typeof( Springs ) ),
				new FillableEntry( 5, typeof( TinkerTools ) ),
				new FillableEntry( 4, typeof( Key ) ),
				new FillableEntry( 1, typeof( DecoArrowShafts )),
				new FillableEntry( 1, typeof( Lockpicks )),
				new FillableEntry( 1, typeof( ToolKit ))
			});

        public static FillableContent Veterinarian = new FillableContent(
            1,
            new Type[]
			{
				typeof( Veterinarian )
			},
            new FillableEntry[]
			{
				new FillableEntry( 1, typeof( Bandage ) ),
				new FillableEntry( 1, typeof( MortarPestle ) ),
				new FillableEntry( 1, typeof( LesserHealPotion ) ),
				//new FillableEntry( 1, typeof( Wheat ) ),
				new FillableEntry( 1, typeof( Carrot ) )
			});

        public static FillableContent Weaponsmith = new FillableContent(
            2,
            new Type[]
			{
				typeof( Weaponsmith )
			},
            new FillableEntry[]
			{
				new FillableEntry( 8, Loot.WeaponTypes ),
				new FillableEntry( 1, typeof( Arrow ) )
			});

        public static FillableContent Lookup(FillableContentType type)
        {
            int v = (int)type;

            if (v >= 0 && v < m_ContentTypes.Length)
                return m_ContentTypes[v];

            return null;
        }

        public static FillableContentType Lookup(FillableContent content)
        {
            if (content == null)
                return FillableContentType.None;

            return (FillableContentType)Array.IndexOf(m_ContentTypes, content);
        }

        private static Hashtable m_AcquireTable;

        private static FillableContent[] m_ContentTypes = new FillableContent[]
			{
				Weaponsmith,	Provisioner,	Mage,
				Alchemist,		Armorer,		ArtisanGuild,
				Baker,			Bard,			Blacksmith,
				Bowyer,			Butcher,		Carpenter,
				Clothier,		Cobbler,		Docks,
				Farm,			FighterGuild,	Guard,
				Healer,			Herbalist,		Inn,
				Jeweler,		Library,		Merchant,
				Mill,			Mine,			Observatory,
				Painter,		Ranger,			Stables,
				Tanner,			Tavern,			ThiefGuild,
				Tinker,			Veterinarian
			};

        public static FillableContent Acquire(Point3D loc, Map map)
        {
            if (map == null || map == Map.Internal)
                return null;

            if (m_AcquireTable == null)
            {
                m_AcquireTable = new Hashtable();

                for (int i = 0; i < m_ContentTypes.Length; ++i)
                {
                    FillableContent fill = m_ContentTypes[i];

                    for (int j = 0; j < fill.m_Vendors.Length; ++j)
                        m_AcquireTable[fill.m_Vendors[j]] = fill;
                }
            }

            Mobile nearest = null;
            FillableContent content = null;

            foreach (Mobile mob in map.GetMobilesInRange(loc, 20))
            {
                if (nearest != null && mob.GetDistanceToSqrt(loc) > nearest.GetDistanceToSqrt(loc) && !(nearest is Cobbler && mob is Provisioner))
                    continue;

                FillableContent check = m_AcquireTable[mob.GetType()] as FillableContent;

                if (check != null)
                {
                    nearest = mob;
                    content = check;
                }
            }

            return content;
        }
    }
}