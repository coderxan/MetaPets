using System;
using System.Collections.Generic;
using System.Text;

using Server;
using Server.Mobiles;
using Server.Network;
using Server.Spells;

namespace Server.Items
{
    public class SkillTeleporter : Teleporter
    {
        private SkillName m_Skill;
        private double m_Required;
        private string m_MessageString;
        private int m_MessageNumber;

        [CommandProperty(AccessLevel.GameMaster)]
        public SkillName Skill
        {
            get { return m_Skill; }
            set { m_Skill = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public double Required
        {
            get { return m_Required; }
            set { m_Required = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string MessageString
        {
            get { return m_MessageString; }
            set { m_MessageString = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MessageNumber
        {
            get { return m_MessageNumber; }
            set { m_MessageNumber = value; InvalidateProperties(); }
        }

        private void EndMessageLock(object state)
        {
            ((Mobile)state).EndAction(this);
        }

        public override bool CanTeleport(Mobile m)
        {
            if (!base.CanTeleport(m))
                return false;

            Skill sk = m.Skills[m_Skill];

            if (sk == null || sk.Base < m_Required)
            {
                if (m.BeginAction(this))
                {
                    if (m_MessageString != null)
                        m.Send(new UnicodeMessage(Serial, ItemID, MessageType.Regular, 0x3B2, 3, "ENU", null, m_MessageString));
                    else if (m_MessageNumber != 0)
                        m.Send(new MessageLocalized(Serial, ItemID, MessageType.Regular, 0x3B2, 3, m_MessageNumber, null, ""));

                    Timer.DelayCall(TimeSpan.FromSeconds(5.0), new TimerStateCallback(EndMessageLock), m);
                }

                return false;
            }

            return true;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            int skillIndex = (int)m_Skill;
            string skillName;

            if (skillIndex >= 0 && skillIndex < SkillInfo.Table.Length)
                skillName = SkillInfo.Table[skillIndex].Name;
            else
                skillName = "(Invalid)";

            list.Add(1060661, "{0}\t{1:F1}", skillName, m_Required);

            if (m_MessageString != null)
                list.Add(1060662, "Message\t{0}", m_MessageString);
            else if (m_MessageNumber != 0)
                list.Add(1060662, "Message\t#{0}", m_MessageNumber);
        }

        [Constructable]
        public SkillTeleporter()
        {
        }

        public SkillTeleporter(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write((int)m_Skill);
            writer.Write((double)m_Required);
            writer.Write((string)m_MessageString);
            writer.Write((int)m_MessageNumber);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_Skill = (SkillName)reader.ReadInt();
                        m_Required = reader.ReadDouble();
                        m_MessageString = reader.ReadString();
                        m_MessageNumber = reader.ReadInt();

                        break;
                    }
            }
        }
    }
}