using System;
using System.Collections.Generic;
using System.Text;

using Server;
using Server.Mobiles;
using Server.Network;
using Server.Spells;

namespace Server.Items
{
    public class TimeoutTeleporter : Teleporter
    {
        private TimeSpan m_TimeoutDelay;
        private Dictionary<Mobile, Timer> m_Teleporting;

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan TimeoutDelay
        {
            get { return m_TimeoutDelay; }
            set { m_TimeoutDelay = value; }
        }

        [Constructable]
        public TimeoutTeleporter()
            : this(new Point3D(0, 0, 0), null, false)
        {
        }

        [Constructable]
        public TimeoutTeleporter(Point3D pointDest, Map mapDest)
            : this(pointDest, mapDest, false)
        {
        }

        [Constructable]
        public TimeoutTeleporter(Point3D pointDest, Map mapDest, bool creatures)
            : base(pointDest, mapDest, creatures)
        {
            m_Teleporting = new Dictionary<Mobile, Timer>();
        }

        public void StartTimer(Mobile m)
        {
            StartTimer(m, m_TimeoutDelay);
        }

        private void StartTimer(Mobile m, TimeSpan delay)
        {
            Timer t;

            if (m_Teleporting.TryGetValue(m, out t))
                t.Stop();

            m_Teleporting[m] = Timer.DelayCall<Mobile>(delay, StartTeleport, m);
        }

        public void StopTimer(Mobile m)
        {
            Timer t;

            if (m_Teleporting.TryGetValue(m, out t))
            {
                t.Stop();
                m_Teleporting.Remove(m);
            }
        }

        public override void DoTeleport(Mobile m)
        {
            m_Teleporting.Remove(m);

            base.DoTeleport(m);
        }

        public override bool OnMoveOver(Mobile m)
        {
            if (Active)
            {
                if (!CanTeleport(m))
                    return false;

                StartTimer(m);
            }

            return true;
        }

        public TimeoutTeleporter(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write(m_TimeoutDelay);
            writer.Write(m_Teleporting.Count);

            foreach (KeyValuePair<Mobile, Timer> kvp in m_Teleporting)
            {
                writer.Write(kvp.Key);
                writer.Write(kvp.Value.Next);
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            m_TimeoutDelay = reader.ReadTimeSpan();
            m_Teleporting = new Dictionary<Mobile, Timer>();

            int count = reader.ReadInt();

            for (int i = 0; i < count; ++i)
            {
                Mobile m = reader.ReadMobile();
                DateTime end = reader.ReadDateTime();

                StartTimer(m, end - DateTime.UtcNow);
            }
        }
    }

    public class TimeoutGoal : Item
    {
        private TimeoutTeleporter m_Teleporter;

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeoutTeleporter Teleporter
        {
            get { return m_Teleporter; }
            set { m_Teleporter = value; }
        }

        [Constructable]
        public TimeoutGoal()
            : base(0x1822)
        {
            Movable = false;
            Visible = false;

            Hue = 1154;
        }

        public override bool OnMoveOver(Mobile m)
        {
            if (m_Teleporter != null)
                m_Teleporter.StopTimer(m);

            return true;
        }

        public override string DefaultName
        {
            get { return "timeout teleporter goal"; }
        }

        public TimeoutGoal(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.WriteItem<TimeoutTeleporter>(m_Teleporter);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            m_Teleporter = reader.ReadItem<TimeoutTeleporter>();
        }
    }
}