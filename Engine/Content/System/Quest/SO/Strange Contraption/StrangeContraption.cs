using System;
using System.Collections.Generic;

using Server;
using Server.Mobiles;
using Server.Network;

/// <summary>
/// Mutation Cores are obtained from Plague Beast Lords, in a rather unconventional manner: Cutting the beasts open, 
/// and solving a puzzle that consists of messing with their guts.
/// 
/// They were first used during the climax of the Plague of Despair scenario, though now they are a component required 
/// to activate a Strange Contraption located inside the Solen Hives.
/// </summary>

namespace Server.Items
{
    #region Mutation Core Puzzle

    public class PlagueBeastBackpack : BaseContainer
    {
        public override int DefaultMaxWeight { get { return 0; } }
        public override int DefaultMaxItems { get { return 0; } }
        public override int DefaultGumpID { get { return 0x2A63; } }
        public override int DefaultDropSound { get { return 0x23F; } }

        private static int[, ,] m_Positions = new int[,,]
		{
			{ { 275, 85 }, { 360, 111 }, { 375, 184 }, { 332, 228 }, { 141, 105 }, { 189, 75 } },
			{ { 274, 34 }, { 327, 89 }, { 354, 168 }, { 304, 225 }, { 113, 86 }, { 189, 75 } },
			{ { 276, 79 }, { 369, 117 }, { 372, 192 }, { 336, 230 }, { 141, 116 }, { 189, 75 } },
		};

        private static int[] m_BrainHues = new int[]
		{
			0x2B, 0x42, 0x54, 0x60
		};

        public PlagueBeastBackpack()
            : base(0x261B)
        {
            Layer = Layer.Backpack;
        }

        public void Initialize()
        {
            AddInnard(0x1CF6, 0x0, 227, 128);
            AddInnard(0x1D10, 0x0, 251, 128);
            AddInnard(0x1FBE, 0x21, 240, 83);

            AddInnard(new PlagueBeastHeart(), 229, 104);

            AddInnard(0x1D06, 0x0, 283, 91);
            AddInnard(0x1FAF, 0x21, 315, 107);
            AddInnard(0x1FB9, 0x21, 289, 87);
            AddInnard(0x9E7, 0x21, 304, 96);
            AddInnard(0x1B1A, 0x66D, 335, 102);
            AddInnard(0x1D10, 0x0, 338, 146);
            AddInnard(0x1FB3, 0x21, 358, 167);
            AddInnard(0x1D0B, 0x0, 357, 155);
            AddInnard(0x9E7, 0x21, 339, 184);
            AddInnard(0x1B1A, 0x66D, 157, 172);
            AddInnard(0x1D11, 0x0, 147, 157);
            AddInnard(0x1FB9, 0x21, 121, 131);
            AddInnard(0x9E7, 0x21, 166, 176);
            AddInnard(0x1D0B, 0x0, 122, 138);
            AddInnard(0x1D0D, 0x0, 118, 150);
            AddInnard(0x1FB3, 0x21, 97, 123);
            AddInnard(0x1D08, 0x0, 115, 113);
            AddInnard(0x9E7, 0x21, 109, 109);
            AddInnard(0x9E7, 0x21, 91, 122);
            AddInnard(0x9E7, 0x21, 94, 160);
            AddInnard(0x1B19, 0x66D, 170, 121);
            AddInnard(0x1FAF, 0x21, 161, 111);
            AddInnard(0x1D0B, 0x0, 158, 112);
            AddInnard(0x9E7, 0x21, 159, 101);
            AddInnard(0x1D10, 0x0, 132, 177);
            AddInnard(0x1D0E, 0x0, 110, 178);
            AddInnard(0x1FB3, 0x21, 95, 194);
            AddInnard(0x1FAF, 0x21, 154, 203);
            AddInnard(0x1B1A, 0x66D, 110, 237);
            AddInnard(0x9E7, 0x21, 111, 171);
            AddInnard(0x9E7, 0x21, 90, 197);
            AddInnard(0x9E7, 0x21, 166, 205);
            AddInnard(0x9E7, 0x21, 96, 242);
            AddInnard(0x1D10, 0x0, 334, 196);
            AddInnard(0x1D0B, 0x0, 322, 270);

            List<PlagueBeastOrgan> organs = new List<PlagueBeastOrgan>();
            PlagueBeastOrgan organ;

            for (int i = 0; i < 6; i++)
            {
                int random = Utility.Random(3);

                if (i == 5)
                    random = 0;

                switch (random)
                {
                    default:
                    case 0: organ = new PlagueBeastRockOrgan(); break;
                    case 1: organ = new PlagueBeastMaidenOrgan(); break;
                    case 2: organ = new PlagueBeastRubbleOrgan(); break;
                }

                organs.Add(organ);
                AddInnard(organ, m_Positions[random, i, 0], m_Positions[random, i, 1]);
            }

            organ = new PlagueBeastBackupOrgan();
            organs.Add(organ);
            AddInnard(organ, 129, 214);

            for (int i = 0; i < m_BrainHues.Length; i++)
            {
                int random = Utility.Random(organs.Count);
                organ = organs[random];
                organ.BrainHue = m_BrainHues[i];
                organs.RemoveAt(random);
            }

            organs.Clear();

            AddInnard(new PlagueBeastMainOrgan(), 240, 161);
        }

        public override bool TryDropItem(Mobile from, Item dropped, bool sendFullMessage)
        {
            if (dropped is PlagueBeastInnard || dropped is PlagueBeastGland)
                return base.TryDropItem(from, dropped, sendFullMessage);

            return false;
        }

        public override bool OnDragDropInto(Mobile from, Item item, Point3D p)
        {
            if (IsAccessibleTo(from) && (item is PlagueBeastInnard || item is PlagueBeastGland))
            {
                Rectangle2D ir = ItemBounds.Table[item.ItemID];
                int x, y;
                int cx = p.X + ir.X + ir.Width / 2;
                int cy = p.Y + ir.Y + ir.Height / 2;

                for (int i = Items.Count - 1; i >= 0; i--)
                {
                    PlagueBeastComponent innard = Items[i] as PlagueBeastComponent;

                    if (innard != null)
                    {
                        Rectangle2D r = ItemBounds.Table[innard.ItemID];

                        x = innard.X + r.X;
                        y = innard.Y + r.Y;

                        if (cx >= x && cx <= x + r.Width && cy >= y && cy <= y + r.Height)
                        {
                            innard.OnDragDrop(from, item);
                            break;
                        }
                    }
                }

                return base.OnDragDropInto(from, item, p);
            }

            return false;
        }

        public void AddInnard(int itemID, int hue, int x, int y)
        {
            AddInnard(new PlagueBeastInnard(itemID, hue), x, y);
        }

        public void AddInnard(PlagueBeastInnard innard, int x, int y)
        {
            AddItem(innard);
            innard.Location = new Point3D(x, y, 0);
            innard.Map = Map;
        }

        public PlagueBeastBackpack(Serial serial)
            : base(serial)
        {
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

    public class PlagueBeastInnard : Item, IScissorable, ICarvable
    {
        public PlagueBeastLord Owner
        {
            get { return RootParent as PlagueBeastLord; }
        }

        public PlagueBeastInnard(int itemID, int hue)
            : base(itemID)
        {
            Name = "plague beast innards";
            Hue = hue;
            Movable = false;
            Weight = 1.0;
        }

        public virtual bool Scissor(Mobile from, Scissors scissors)
        {
            return false;
        }

        public virtual void Carve(Mobile from, Item with)
        {
        }

        public virtual bool OnBandage(Mobile from)
        {
            return false;
        }

        public override bool IsAccessibleTo(Mobile check)
        {
            if ((int)check.AccessLevel >= (int)AccessLevel.GameMaster)
                return true;

            PlagueBeastLord owner = Owner;

            if (owner == null)
                return false;

            if (!owner.InRange(check, 2))
                owner.PrivateOverheadMessage(MessageType.Label, 0x3B2, 500446, check.NetState); // That is too far away.
            else if (owner.OpenedBy != null && owner.OpenedBy != check) // TODO check
                owner.PrivateOverheadMessage(MessageType.Label, 0x3B2, 500365, check.NetState); // That is being used by someone else
            else if (owner.Frozen)
                return true;

            return false;
        }

        public PlagueBeastInnard(Serial serial)
            : base(serial)
        {
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

            PlagueBeastLord owner = Owner;

            if (owner == null || !owner.Alive)
                Delete();
        }
    }

    public class PlagueBeastComponent : PlagueBeastInnard
    {
        private PlagueBeastOrgan m_Organ;

        public PlagueBeastOrgan Organ
        {
            get { return m_Organ; }
            set { m_Organ = value; }
        }

        public bool IsBrain
        {
            get { return ItemID == 0x1CF0; }
        }

        public bool IsGland
        {
            get { return ItemID == 0x1CEF; }
        }

        public bool IsReceptacle
        {
            get { return ItemID == 0x9DF; }
        }

        public PlagueBeastComponent(int itemID, int hue)
            : this(itemID, hue, false)
        {
        }

        public PlagueBeastComponent(int itemID, int hue, bool movable)
            : base(itemID, hue)
        {
            Movable = movable;
        }

        public override bool DropToItem(Mobile from, Item target, Point3D p)
        {
            if (target is PlagueBeastBackpack)
                return base.DropToItem(from, target, p);

            return false;
        }

        public override bool AllowSecureTrade(Mobile from, Mobile to, Mobile newOwner, bool accepted)
        {
            return false;
        }

        public override bool DropToMobile(Mobile from, Mobile target, Point3D p)
        {
            return false;
        }

        public override bool DropToWorld(Mobile from, Point3D p)
        {
            return false;
        }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (m_Organ != null && m_Organ.OnDropped(from, dropped, this))
            {
                if (dropped is PlagueBeastComponent)
                    m_Organ.Components.Add((PlagueBeastComponent)dropped);
            }

            return true;
        }

        public override bool OnDragLift(Mobile from)
        {
            if (IsAccessibleTo(from))
            {
                if (m_Organ != null && m_Organ.OnLifted(from, this))
                {
                    from.SendLocalizedMessage(IsGland ? 1071895 : 1071914, null, 0x3B2); // * You rip the organ out of the plague beast's flesh *

                    if (m_Organ.Components.Contains(this))
                        m_Organ.Components.Remove(this);

                    m_Organ = null;
                    from.PlaySound(0x1CA);
                }

                return true;
            }

            return false;
        }

        public PlagueBeastComponent(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version

            writer.WriteItem<PlagueBeastOrgan>(m_Organ);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();

            m_Organ = reader.ReadItem<PlagueBeastOrgan>();
        }
    }

    public class PlagueBeastBlood : PlagueBeastComponent
    {
        public bool Patched
        {
            get { return ItemID == 0x1765; }
        }

        public bool Starting
        {
            get { return ItemID == 0x122C; }
        }

        private Timer m_Timer;

        public PlagueBeastBlood()
            : base(0x122C, 0)
        {
            m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(1.5), 3, new TimerCallback(Hemorrhage));
        }

        public override void OnAfterDelete()
        {
            if (m_Timer != null && m_Timer.Running)
                m_Timer.Stop();
        }

        public override bool OnBandage(Mobile from)
        {
            if (IsAccessibleTo(from) && !Patched)
            {
                if (m_Timer != null && m_Timer.Running)
                    m_Timer.Stop();

                if (Starting)
                {
                    X += 2;
                    Y -= 9;

                    if (Organ is PlagueBeastRubbleOrgan)
                        Y -= 5;
                    else if (Organ is PlagueBeastBackupOrgan)
                        X += 7;
                }
                else
                {
                    X -= 4;
                    Y -= 2;
                }

                ItemID = 0x1765;

                if (Owner != null)
                {
                    Container pack = Owner.Backpack;

                    if (pack != null)
                    {
                        for (int i = 0; i < pack.Items.Count; i++)
                        {
                            PlagueBeastMainOrgan main = pack.Items[i] as PlagueBeastMainOrgan;

                            if (main != null && main.Complete)
                                main.FinishOpening(from);
                        }
                    }
                }

                PublicOverheadMessage(MessageType.Regular, 0x3B2, 1071916); // * You patch up the wound with a bandage *

                return true;
            }

            return false;
        }

        private void Hemorrhage()
        {
            if (Patched)
                return;

            if (Owner != null)
                Owner.PlaySound(0x25);

            if (ItemID == 0x122A)
            {
                if (Owner != null)
                {
                    Owner.Unfreeze();
                    Owner.Kill();
                }
            }
            else
            {
                if (Starting)
                {
                    X += 8;
                    Y -= 10;
                }

                ItemID--;
            }
        }

        public PlagueBeastBlood(Serial serial)
            : base(serial)
        {
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

    public class PlagueBeastGland : Item
    {
        [Constructable]
        public PlagueBeastGland()
            : base(0x1CEF)
        {
            Name = "A Healthy Gland";
            Weight = 1.0;
            Hue = 0x6;
        }

        public PlagueBeastGland(Serial serial)
            : base(serial)
        {
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

    public class PlagueBeastHeart : PlagueBeastInnard
    {
        private Timer m_Timer;

        public PlagueBeastHeart()
            : base(0x1363, 0x21)
        {
            m_Timer = new InternalTimer(this);
            m_Timer.Start();
        }

        public override void OnAfterDelete()
        {
            if (m_Timer != null && m_Timer.Running)
                m_Timer.Stop();
        }

        public PlagueBeastHeart(Serial serial)
            : base(serial)
        {
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

            m_Timer = new InternalTimer(this);
            m_Timer.Start();
        }

        private class InternalTimer : Timer
        {
            private PlagueBeastHeart m_Heart;
            private bool m_Delay;

            public InternalTimer(PlagueBeastHeart heart)
                : base(TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(0.5))
            {
                m_Heart = heart;
            }

            protected override void OnTick()
            {
                if (m_Heart == null || m_Heart.Deleted || m_Heart.Owner == null || !m_Heart.Owner.Alive)
                {
                    Stop();
                    return;
                }

                if (m_Heart.ItemID == 0x1363)
                {
                    if (m_Delay)
                    {
                        m_Heart.ItemID = 0x1367;
                        m_Heart.Owner.PlaySound(0x11F);
                    }

                    m_Delay = !m_Delay;
                }
                else
                {
                    m_Heart.ItemID = 0x1363;
                    m_Heart.Owner.PlaySound(0x120);
                    m_Delay = false;
                }
            }
        }
    }

    #region Plague Beast Organs

    public class PlagueBeastOrgan : PlagueBeastInnard
    {
        public virtual bool IsCuttable { get { return false; } }

        private List<PlagueBeastComponent> m_Components;

        public List<PlagueBeastComponent> Components
        {
            get { return m_Components; }
        }

        private int m_BrainHue;

        public int BrainHue
        {
            get { return m_BrainHue; }
            set { m_BrainHue = value; }
        }

        private bool m_Opened;

        public bool Opened
        {
            get { return m_Opened; }
            set { m_Opened = value; }
        }

        private Timer m_Timer;

        public PlagueBeastOrgan()
            : this(1, 0)
        {
            Visible = false;
        }

        public PlagueBeastOrgan(int itemID, int hue)
            : base(itemID, hue)
        {
            m_Components = new List<PlagueBeastComponent>();
            m_Opened = false;

            Movable = false;

            Timer.DelayCall(TimeSpan.Zero, new TimerCallback(Initialize));
        }

        public virtual void Initialize()
        {
        }

        public void AddComponent(PlagueBeastComponent c, int x, int y)
        {
            Container pack = Parent as Container;

            if (pack != null)
                pack.DropItem(c);

            c.Organ = this;
            c.Location = new Point3D(X + x, Y + y, Z);
            c.Map = Map;

            m_Components.Add(c);
        }

        public override bool Scissor(Mobile from, Scissors scissors)
        {
            if (IsCuttable && IsAccessibleTo(from))
            {
                if (!m_Opened && m_Timer == null)
                {
                    m_Timer = Timer.DelayCall<Mobile>(TimeSpan.FromSeconds(3), new TimerStateCallback<Mobile>(FinishOpening), from);
                    scissors.PublicOverheadMessage(MessageType.Regular, 0x3B2, 1071897); // You carefully cut into the organ.
                    return true;
                }
                else
                    scissors.PublicOverheadMessage(MessageType.Regular, 0x3B2, 1071898); // You have already cut this organ open.
            }

            return false;
        }

        public override void OnAfterDelete()
        {
            if (m_Timer != null && m_Timer.Running)
                m_Timer.Stop();
        }

        public virtual bool OnLifted(Mobile from, PlagueBeastComponent c)
        {
            return c.IsGland || c.IsBrain;
        }

        public virtual bool OnDropped(Mobile from, Item item, PlagueBeastComponent to)
        {
            return false;
        }

        public virtual void FinishOpening(Mobile from)
        {
            m_Opened = true;

            if (Owner != null)
                Owner.PlaySound(0x50);
        }

        public PlagueBeastOrgan(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version

            writer.WriteItemList<PlagueBeastComponent>(m_Components);
            writer.Write((int)m_BrainHue);
            writer.Write((bool)m_Opened);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();

            m_Components = reader.ReadStrongItemList<PlagueBeastComponent>();
            m_BrainHue = reader.ReadInt();
            m_Opened = reader.ReadBool();
        }
    }

    public class PlagueBeastMaidenOrgan : PlagueBeastOrgan
    {
        public PlagueBeastMaidenOrgan()
            : base(0x124D, 0x0)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!Opened)
                FinishOpening(from);
        }

        public override void FinishOpening(Mobile from)
        {
            ItemID = 0x1249;

            if (Owner != null)
                Owner.PlaySound(0x187);

            AddComponent(new PlagueBeastComponent(0x1D0D, 0x0), 22, 3);
            AddComponent(new PlagueBeastComponent(0x1D12, 0x0), 15, 18);
            AddComponent(new PlagueBeastComponent(0x1DA3, 0x21), 26, 46);

            if (BrainHue > 0)
                AddComponent(new PlagueBeastComponent(0x1CF0, BrainHue, true), 22, 29);

            Opened = true;
        }

        public PlagueBeastMaidenOrgan(Serial serial)
            : base(serial)
        {
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

    public class PlagueBeastRockOrgan : PlagueBeastOrgan
    {
        public override bool IsCuttable { get { return true; } }

        public PlagueBeastRockOrgan()
            : base(0x177A, 0x60)
        {
        }

        public override void Carve(Mobile from, Item with)
        {
            if (IsAccessibleTo(from))
                with.PublicOverheadMessage(MessageType.Regular, 0x3B2, 1071896); // This is too crude an implement for such a procedure. 
        }

        public override bool OnLifted(Mobile from, PlagueBeastComponent c)
        {
            base.OnLifted(from, c);

            if (c.IsBrain)
            {
                AddComponent(new PlagueBeastBlood(), -7, 24);
                return true;
            }

            return false;
        }

        public override void FinishOpening(Mobile from)
        {
            base.FinishOpening(from);

            AddComponent(new PlagueBeastComponent(0x1775, 0x60), 3, 5);
            AddComponent(new PlagueBeastComponent(0x1777, 0x1), 10, 14);

            if (BrainHue > 0)
                AddComponent(new PlagueBeastComponent(0x1CF0, BrainHue, true), 1, 24); // 22, 29 
            else
                AddComponent(new PlagueBeastBlood(), -7, 24);
        }

        public PlagueBeastRockOrgan(Serial serial)
            : base(serial)
        {
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

    public class PlagueBeastRubbleOrgan : PlagueBeastOrgan
    {
        private int m_Veins;

        public PlagueBeastRubbleOrgan()
            : base()
        {
            m_Veins = 3;
        }

        public override void Initialize()
        {
            Hue = Utility.RandomList(m_Hues);

            AddComponent(new PlagueBeastComponent(0x3BB, Hue), 0, 0);
            AddComponent(new PlagueBeastComponent(0x3BA, Hue), 4, 6);
            AddComponent(new PlagueBeastComponent(0x3BA, Hue), -6, 17);

            int v = Utility.Random(4);

            AddComponent(new PlagueBeastVein(0x1B1B, v == 0 ? Hue : RandomHue(Hue)), -23, -3);
            AddComponent(new PlagueBeastVein(0x1B1C, v == 1 ? Hue : RandomHue(Hue)), 19, 4);
            AddComponent(new PlagueBeastVein(0x1B1B, v == 2 ? Hue : RandomHue(Hue)), 21, 27);
            AddComponent(new PlagueBeastVein(0x1B1B, v == 3 ? Hue : RandomHue(Hue)), 10, 40);
        }

        public override bool OnLifted(Mobile from, PlagueBeastComponent c)
        {
            if (c.IsBrain)
            {
                AddComponent(new PlagueBeastBlood(), -13, 25);
                return true;
            }

            return false;
        }

        public override void FinishOpening(Mobile from)
        {
            AddComponent(new PlagueBeastComponent(0x1777, 0x1), 5, 14);

            if (BrainHue > 0)
                AddComponent(new PlagueBeastComponent(0x1CF0, BrainHue, true), -5, 22);
            else
                AddComponent(new PlagueBeastBlood(), -13, 25);

            Opened = true;
        }

        private static int[] m_Hues = new int[]
		{
			0xD, 0x17, 0x2B, 0x42, 0x54, 0x5D
		};

        private static int RandomHue(int exculde)
        {
            for (int i = 0; i < 20; i++)
            {
                int hue = Utility.RandomList(m_Hues);

                if (hue != exculde)
                    return hue;
            }

            return 0xD;
        }

        public virtual void OnVeinCut(Mobile from, PlagueBeastVein vein)
        {
            if (vein.Hue != Hue)
            {
                if (!Opened && m_Veins > 0 && --m_Veins == 0)
                    FinishOpening(from);
            }
            else
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1071901); // * As you cut the vein, a cloud of poison is expelled from the plague beast's organ, and the plague beast dissolves into a puddle of goo *
                from.ApplyPoison(from, Poison.Greater);
                from.PlaySound(0x22F);

                if (Owner != null)
                {
                    Owner.Unfreeze();
                    Owner.Kill();
                }
            }
        }

        public PlagueBeastRubbleOrgan(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version

            writer.Write((int)m_Veins);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();

            m_Veins = reader.ReadInt();
        }
    }

    public class PlagueBeastBackupOrgan : PlagueBeastOrgan
    {
        public override bool IsCuttable { get { return true; } }

        private Timer m_Timer;
        private Item m_Gland;

        public PlagueBeastBackupOrgan()
            : base(0x1362, 0x6)
        {
        }

        public override void Initialize()
        {
            AddComponent(new PlagueBeastComponent(0x1B1B, 0x42), 16, 39);
            AddComponent(new PlagueBeastComponent(0x1B1B, 0x42), 39, 49);
            AddComponent(new PlagueBeastComponent(0x1B1B, 0x42), 39, 48);
            AddComponent(new PlagueBeastComponent(0x1B1B, 0x42), 44, 42);
            AddComponent(new PlagueBeastComponent(0x1CF2, 0x42), 20, 34);
            AddComponent(new PlagueBeastComponent(0x135F, 0x42), 47, 58);
            AddComponent(new PlagueBeastComponent(0x1360, 0x42), 70, 68);
        }

        public override void Carve(Mobile from, Item with)
        {
            if (IsAccessibleTo(from))
                with.PublicOverheadMessage(MessageType.Regular, 0x3B2, 1071896); // This is too crude an implement for such a procedure. 
        }

        public override bool OnLifted(Mobile from, PlagueBeastComponent c)
        {
            if (c.IsBrain)
            {
                AddComponent(new PlagueBeastBlood(), 47, 72);
                return true;
            }
            else if (c.IsGland)
            {
                m_Gland = null;
                return true;
            }

            return c.IsGland;
        }

        public override bool OnDropped(Mobile from, Item item, PlagueBeastComponent to)
        {
            if (to.Hue == 0x1 && m_Gland == null && item is PlagueBeastGland)
            {
                m_Gland = item;
                m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(3), new TimerCallback(FinishHealing));
                from.SendAsciiMessage(0x3B2, "* You place the healthy gland inside the organ sac *");
                item.Movable = false;

                if (Owner != null)
                    Owner.PlaySound(0x20);

                return true;
            }

            return false;
        }

        public override void FinishOpening(Mobile from)
        {
            base.FinishOpening(from);

            AddComponent(new PlagueBeastComponent(0x1363, 0xF), -3, 3);
            AddComponent(new PlagueBeastComponent(0x1365, 0x1), -3, 10);

            m_Gland = new PlagueBeastComponent(0x1CEF, 0x3F, true);
            AddComponent((PlagueBeastComponent)m_Gland, -4, 16);
        }

        public void FinishHealing()
        {
            for (int i = 0; i < 7 && i < Components.Count; i++)
                Components[i].Hue = 0x6;

            m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(2), new TimerCallback(OpenOrgan));
        }

        public void OpenOrgan()
        {
            AddComponent(new PlagueBeastComponent(0x1367, 0xF), 55, 61);
            AddComponent(new PlagueBeastComponent(0x1366, 0x1), 57, 66);

            if (BrainHue > 0)
                AddComponent(new PlagueBeastComponent(0x1CF0, BrainHue, true), 55, 69);
        }

        public PlagueBeastBackupOrgan(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version

            writer.Write((Item)m_Gland);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();

            m_Gland = reader.ReadItem();
        }
    }

    public class PlagueBeastMainOrgan : PlagueBeastOrgan
    {
        private int m_Brains;

        public bool Complete
        {
            get { return m_Brains >= 4; }
        }

        public PlagueBeastMainOrgan()
            : base()
        {
            m_Brains = 0;
        }

        public override void Initialize()
        {
            // receptacles
            AddComponent(new PlagueBeastComponent(0x1B1B, 0x42), -36, -2);
            AddComponent(new PlagueBeastComponent(0x1FB3, 0x42), -42, 0);
            AddComponent(new PlagueBeastComponent(0x9DF, 0x42), -53, -7);

            AddComponent(new PlagueBeastComponent(0x1B1C, 0x54), 29, 9);
            AddComponent(new PlagueBeastComponent(0x1D06, 0x54), 18, -2);
            AddComponent(new PlagueBeastComponent(0x9DF, 0x54), 36, -1);

            AddComponent(new PlagueBeastComponent(0x1D10, 0x2B), -36, 47);
            AddComponent(new PlagueBeastComponent(0x1B1C, 0x2B), -24, 62);
            AddComponent(new PlagueBeastComponent(0x9DF, 0x2B), -41, 74);

            AddComponent(new PlagueBeastComponent(0x1B1B, 0x60), 39, 56);
            AddComponent(new PlagueBeastComponent(0x1FB4, 0x60), 34, 52);
            AddComponent(new PlagueBeastComponent(0x9DF, 0x60), 45, 71);

            // main part
            AddComponent(new PlagueBeastComponent(0x1351, 0x15), 23, 0);
            AddComponent(new PlagueBeastComponent(0x134F, 0x15), -22, 0);
            AddComponent(new PlagueBeastComponent(0x1350, 0x15), 0, 0);
        }

        public override bool OnLifted(Mobile from, PlagueBeastComponent c)
        {
            if (c.IsBrain)
                m_Brains--;

            return true;
        }

        public override bool OnDropped(Mobile from, Item item, PlagueBeastComponent to)
        {
            if (!Opened && to.IsReceptacle && item.Hue == to.Hue)
            {
                to.Organ = this;
                m_Brains++;
                from.LocalOverheadMessage(MessageType.Regular, 0x34, 1071913); // You place the organ in the fleshy receptacle near the core.

                if (Owner != null)
                {
                    Owner.PlaySound(0x1BA);

                    if (Owner.IsBleeding)
                    {
                        from.LocalOverheadMessage(MessageType.Regular, 0x34, 1071922); // The plague beast is still bleeding from open wounds.  You must seal any bleeding wounds before the core will open!
                        return true;
                    }
                }

                if (m_Brains == 4)
                    FinishOpening(from);

                return true;
            }

            return false;
        }

        public override void FinishOpening(Mobile from)
        {
            AddComponent(new PlagueBeastComponent(0x1363, 0x1), 0, 22);
            AddComponent(new PlagueBeastComponent(0x1D04, 0xD), 0, 22);

            if (Owner != null && Owner.Backpack != null)
            {
                PlagueBeastMutationCore core = new PlagueBeastMutationCore();
                Owner.Backpack.AddItem(core);
                core.Movable = false;
                core.Cut = false;
                core.X = X;
                core.Y = Y + 34;

                Owner.PlaySound(0x21);
                Owner.PlaySound(0x166);
            }

            Opened = true;
        }

        public PlagueBeastMainOrgan(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version

            writer.Write((int)m_Brains);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();

            m_Brains = reader.ReadInt();
        }
    }

    #endregion

    public class PlagueBeastVein : PlagueBeastComponent
    {
        private bool m_Cut;

        public bool Cut
        {
            get { return m_Cut; }
        }

        private Timer m_Timer;

        public PlagueBeastVein(int itemID, int hue)
            : base(itemID, hue)
        {
            m_Cut = false;
        }

        public override bool Scissor(Mobile from, Scissors scissors)
        {
            if (IsAccessibleTo(from))
            {
                if (!m_Cut && m_Timer == null)
                {
                    m_Timer = Timer.DelayCall<Mobile>(TimeSpan.FromSeconds(3), new TimerStateCallback<Mobile>(CuttingDone), from);
                    scissors.PublicOverheadMessage(MessageType.Regular, 0x3B2, 1071899); // You begin cutting through the vein.
                    return true;
                }
                else
                    scissors.PublicOverheadMessage(MessageType.Regular, 0x3B2, 1071900); // // This vein has already been cut.
            }

            return false;
        }

        public override void OnAfterDelete()
        {
            if (m_Timer != null && m_Timer.Running)
                m_Timer.Stop();
        }

        private void CuttingDone(Mobile from)
        {
            m_Cut = true;

            if (ItemID == 0x1B1C)
                ItemID = 0x1B1B;
            else
                ItemID = 0x1B1C;

            if (Owner != null)
                Owner.PlaySound(0x199);

            PlagueBeastRubbleOrgan organ = Organ as PlagueBeastRubbleOrgan;

            if (organ != null)
                organ.OnVeinCut(from, this);
        }

        public PlagueBeastVein(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version

            writer.Write((bool)m_Cut);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();

            m_Cut = reader.ReadBool();
        }
    }

    #endregion
}