using System;
using System.Collections;
using System.Collections.Generic;

using Server;
using Server.Engines.CannedEvil;
using Server.Items;
using Server.Network;
using Server.Spells;

namespace Server.Mobiles
{
    public class StainedOoze : Item
    {
        private bool m_Corrosive;
        private Timer m_Timer;
        private int m_Ticks;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Corrosive
        {
            get { return m_Corrosive; }
            set { m_Corrosive = value; }
        }

        [Constructable]
        public StainedOoze()
            : this(false)
        {
        }

        [Constructable]
        public StainedOoze(bool corrosive)
            : base(0x122A)
        {
            Movable = false;
            Hue = 0x95;

            m_Corrosive = corrosive;
            m_Timer = Timer.DelayCall(TimeSpan.Zero, TimeSpan.FromSeconds(1), OnTick);
            m_Ticks = 0;
        }

        public override void OnAfterDelete()
        {
            if (m_Timer != null)
            {
                m_Timer.Stop();
                m_Timer = null;
            }
        }

        private void OnTick()
        {
            List<Mobile> toDamage = new List<Mobile>();

            foreach (Mobile m in GetMobilesInRange(0))
            {
                if (m is BaseCreature)
                {
                    BaseCreature bc = (BaseCreature)m;

                    if (!bc.Controlled && !bc.Summoned)
                        continue;
                }
                else if (!m.Player)
                {
                    continue;
                }

                if (m.Alive && !m.IsDeadBondedPet && m.CanBeDamaged())
                    toDamage.Add(m);
            }

            for (int i = 0; i < toDamage.Count; ++i)
                Damage(toDamage[i]);

            ++m_Ticks;

            if (m_Ticks >= 35)
                Delete();
            else if (m_Ticks == 30)
                ItemID = 0x122B;
        }

        public void Damage(Mobile m)
        {
            if (m_Corrosive)
            {
                List<Item> items = m.Items;
                bool damaged = false;

                for (int i = 0; i < items.Count; ++i)
                {
                    IDurability wearable = items[i] as IDurability;

                    if (wearable != null && wearable.HitPoints >= 10 && Utility.RandomDouble() < 0.25)
                    {
                        wearable.HitPoints -= (wearable.HitPoints == 10) ? Utility.Random(1, 5) : 10;
                        damaged = true;
                    }
                }

                if (damaged)
                {
                    m.LocalOverheadMessage(MessageType.Regular, 0x21, 1072070); // The infernal ooze scorches you, setting you and your equipment ablaze!
                    return;
                }
            }

            AOS.Damage(m, 40, 0, 0, 0, 100, 0);
        }

        public StainedOoze(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write(m_Corrosive);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            m_Corrosive = reader.ReadBool();

            m_Timer = Timer.DelayCall(TimeSpan.Zero, TimeSpan.FromSeconds(1), OnTick);
            m_Ticks = (ItemID == 0x122A) ? 0 : 30;
        }
    }
}