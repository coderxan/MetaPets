using System;

using Server;
using Server.Network;

namespace Server.Items
{
    /// <summary>
    /// Creepy Portraits
    /// </summary>
    [Flipable(0x2A69, 0x2A6D)]
    public class CreepyPortraitComponent : AddonComponent
    {
        public override int LabelNumber { get { return 1074481; } } // Creepy portrait
        public override bool HandlesOnMovement { get { return true; } }

        public CreepyPortraitComponent()
            : base(0x2A69)
        {
        }

        public CreepyPortraitComponent(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (Utility.InRange(Location, from.Location, 2))
                Effects.PlaySound(Location, Map, Utility.RandomMinMax(0x565, 0x566));
            else
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
        }

        public override void OnMovement(Mobile m, Point3D old)
        {
            if (m.Alive && m.Player && (m.AccessLevel == AccessLevel.Player || !m.Hidden))
            {
                if (!Utility.InRange(old, Location, 2) && Utility.InRange(m.Location, Location, 2))
                {
                    if (ItemID == 0x2A69 || ItemID == 0x2A6D)
                    {
                        Up();
                        Timer.DelayCall(TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(0.5), 2, new TimerCallback(Up));
                    }
                }
                else if (Utility.InRange(old, Location, 2) && !Utility.InRange(m.Location, Location, 2))
                {
                    if (ItemID == 0x2A6C || ItemID == 0x2A70)
                    {
                        Down();
                        Timer.DelayCall(TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(0.5), 2, new TimerCallback(Down));
                    }
                }
            }
        }

        private void Up()
        {
            ItemID += 1;
        }

        private void Down()
        {
            ItemID -= 1;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();

            if (version == 0 && ItemID != 0x2A69 && ItemID != 0x2A6D)
                ItemID = 0x2A69;
        }
    }

    public class CreepyPortraitAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new CreepyPortraitDeed(); } }

        [Constructable]
        public CreepyPortraitAddon()
            : base()
        {
            AddComponent(new CreepyPortraitComponent(), 0, 0, 0);
        }

        public CreepyPortraitAddon(Serial serial)
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

    public class CreepyPortraitDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new CreepyPortraitAddon(); } }
        public override int LabelNumber { get { return 1074481; } } // Creepy portrait

        [Constructable]
        public CreepyPortraitDeed()
            : base()
        {
            LootType = LootType.Blessed;
        }

        public CreepyPortraitDeed(Serial serial)
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

    /// <summary>
    /// Distrubing Portrait
    /// </summary>
    [Flipable(0x2A5D, 0x2A61)]
    public class DisturbingPortraitComponent : AddonComponent
    {
        public override int LabelNumber { get { return 1074479; } } // Disturbing portrait

        private Timer m_Timer;

        public DisturbingPortraitComponent()
            : base(0x2A5D)
        {
            m_Timer = Timer.DelayCall(TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3), new TimerCallback(Change));
        }

        public DisturbingPortraitComponent(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (Utility.InRange(Location, from.Location, 2))
                Effects.PlaySound(Location, Map, Utility.RandomMinMax(0x567, 0x568));
            else
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            if (m_Timer != null && m_Timer.Running)
                m_Timer.Stop();
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

            m_Timer = Timer.DelayCall(TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3), new TimerCallback(Change));
        }

        private void Change()
        {
            if (ItemID < 0x2A61)
                ItemID = Utility.RandomMinMax(0x2A5D, 0x2A60);
            else
                ItemID = Utility.RandomMinMax(0x2A61, 0x2A64);
        }
    }

    public class DisturbingPortraitAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new DisturbingPortraitDeed(); } }

        [Constructable]
        public DisturbingPortraitAddon()
            : base()
        {
            AddComponent(new DisturbingPortraitComponent(), 0, 0, 0);
        }

        public DisturbingPortraitAddon(Serial serial)
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

    public class DisturbingPortraitDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new DisturbingPortraitAddon(); } }
        public override int LabelNumber { get { return 1074479; } } // Disturbing portrait

        [Constructable]
        public DisturbingPortraitDeed()
            : base()
        {
            LootType = LootType.Blessed;
        }

        public DisturbingPortraitDeed(Serial serial)
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

    /// <summary>
    /// Awesome Distrubing Portrait
    /// </summary>
    [Flipable(0x2A5D, 0x2A61)]
    public class AwesomeDisturbingPortraitComponent : AddonComponent
    {
        public override int LabelNumber { get { return 1074479; } } // Disturbing portrait
        public bool FacingSouth { get { return ItemID < 0x2A61; } }

        private InternalTimer m_Timer;

        public AwesomeDisturbingPortraitComponent()
            : base(0x2A5D)
        {
            m_Timer = new InternalTimer(this, TimeSpan.FromSeconds(1));
            m_Timer.Start();
        }

        public AwesomeDisturbingPortraitComponent(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (Utility.InRange(Location, from.Location, 2))
            {
                int hours;
                int minutes;

                Clock.GetTime(Map, X, Y, out hours, out minutes);

                if (hours < 4 || hours > 20)
                    Effects.PlaySound(Location, Map, 0x569);

                UpdateImage();
            }
            else
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            if (m_Timer != null && m_Timer.Running)
                m_Timer.Stop();
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

            m_Timer = new InternalTimer(this, TimeSpan.Zero);
            m_Timer.Start();
        }

        private void UpdateImage()
        {
            int hours;
            int minutes;

            Clock.GetTime(Map, X, Y, out hours, out minutes);

            if (FacingSouth)
            {
                if (hours < 4)
                    ItemID = 0x2A60;
                else if (hours < 6)
                    ItemID = 0x2A5F;
                else if (hours < 8)
                    ItemID = 0x2A5E;
                else if (hours < 16)
                    ItemID = 0x2A5D;
                else if (hours < 18)
                    ItemID = 0x2A5E;
                else if (hours < 20)
                    ItemID = 0x2A5F;
                else
                    ItemID = 0x2A60;
            }
            else
            {
                if (hours < 4)
                    ItemID = 0x2A64;
                else if (hours < 6)
                    ItemID = 0x2A63;
                else if (hours < 8)
                    ItemID = 0x2A62;
                else if (hours < 16)
                    ItemID = 0x2A61;
                else if (hours < 18)
                    ItemID = 0x2A62;
                else if (hours < 20)
                    ItemID = 0x2A63;
                else
                    ItemID = 0x2A64;
            }
        }

        private class InternalTimer : Timer
        {
            private AwesomeDisturbingPortraitComponent m_Component;

            public InternalTimer(AwesomeDisturbingPortraitComponent c, TimeSpan delay)
                : base(delay, TimeSpan.FromMinutes(10))
            {
                m_Component = c;

                Priority = TimerPriority.OneMinute;
            }

            protected override void OnTick()
            {
                if (m_Component != null && !m_Component.Deleted)
                    m_Component.UpdateImage();
            }
        }
    }

    public class AwesomeDisturbingPortraitAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new AwesomeDisturbingPortraitDeed(); } }

        [Constructable]
        public AwesomeDisturbingPortraitAddon()
            : base()
        {
            AddComponent(new AwesomeDisturbingPortraitComponent(), 0, 0, 0);
        }

        public AwesomeDisturbingPortraitAddon(Serial serial)
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

    public class AwesomeDisturbingPortraitDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new AwesomeDisturbingPortraitAddon(); } }
        public override int LabelNumber { get { return 1074479; } } // Disturbing portrait

        [Constructable]
        public AwesomeDisturbingPortraitDeed()
            : base()
        {
            LootType = LootType.Blessed;
        }

        public AwesomeDisturbingPortraitDeed(Serial serial)
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

    /// <summary>
    /// Unsettling Portrait
    /// </summary>
    [Flipable(0x2A65, 0x2A67)]
    public class UnsettlingPortraitComponent : AddonComponent
    {
        public override int LabelNumber { get { return 1074480; } } // Unsettling portrait

        private Timer m_Timer;

        public UnsettlingPortraitComponent()
            : base(0x2A65)
        {
            m_Timer = Timer.DelayCall(TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3), new TimerCallback(ChangeDirection));
        }

        public UnsettlingPortraitComponent(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (Utility.InRange(Location, from.Location, 2))
                Effects.PlaySound(Location, Map, Utility.RandomMinMax(0x567, 0x568));
            else
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            if (m_Timer != null)
                m_Timer.Stop();
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

            m_Timer = Timer.DelayCall(TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3), new TimerCallback(ChangeDirection));
        }

        private void ChangeDirection()
        {
            if (ItemID == 0x2A65)
                ItemID += 1;
            else if (ItemID == 0x2A66)
                ItemID -= 1;
            else if (ItemID == 0x2A67)
                ItemID += 1;
            else if (ItemID == 0x2A68)
                ItemID -= 1;
        }
    }

    public class UnsettlingPortraitAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new UnsettlingPortraitDeed(); } }

        [Constructable]
        public UnsettlingPortraitAddon()
            : base()
        {
            AddComponent(new UnsettlingPortraitComponent(), 0, 0, 0);
        }

        public UnsettlingPortraitAddon(Serial serial)
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

    public class UnsettlingPortraitDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new UnsettlingPortraitAddon(); } }
        public override int LabelNumber { get { return 1074480; } } // Unsettling portrait

        [Constructable]
        public UnsettlingPortraitDeed()
            : base()
        {
            LootType = LootType.Blessed;
        }

        public UnsettlingPortraitDeed(Serial serial)
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
}