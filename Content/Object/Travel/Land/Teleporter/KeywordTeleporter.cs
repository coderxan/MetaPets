using System;
using System.Collections.Generic;
using System.Text;

using Server;
using Server.Mobiles;
using Server.Network;
using Server.Spells;

namespace Server.Items
{
    public class KeywordTeleporter : Teleporter
    {
        private string m_Substring;
        private int m_Keyword;
        private int m_Range;

        [CommandProperty(AccessLevel.GameMaster)]
        public string Substring
        {
            get { return m_Substring; }
            set { m_Substring = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Keyword
        {
            get { return m_Keyword; }
            set { m_Keyword = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Range
        {
            get { return m_Range; }
            set { m_Range = value; InvalidateProperties(); }
        }

        public override bool HandlesOnSpeech { get { return true; } }

        public override void OnSpeech(SpeechEventArgs e)
        {
            if (!e.Handled && Active)
            {
                Mobile m = e.Mobile;

                if (!m.InRange(GetWorldLocation(), m_Range))
                    return;

                bool isMatch = false;

                if (m_Keyword >= 0 && e.HasKeyword(m_Keyword))
                    isMatch = true;
                else if (m_Substring != null && e.Speech.ToLower().IndexOf(m_Substring.ToLower()) >= 0)
                    isMatch = true;

                if (!isMatch || !CanTeleport(m))
                    return;

                e.Handled = true;
                StartTeleport(m);
            }
        }

        public override void DoTeleport(Mobile m)
        {
            if (!m.InRange(GetWorldLocation(), m_Range) || m.Map != Map)
                return;

            base.DoTeleport(m);
        }

        public override bool OnMoveOver(Mobile m)
        {
            return true;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1060661, "Range\t{0}", m_Range);

            if (m_Keyword >= 0)
                list.Add(1060662, "Keyword\t{0}", m_Keyword);

            if (m_Substring != null)
                list.Add(1060663, "Substring\t{0}", m_Substring);
        }

        [Constructable]
        public KeywordTeleporter()
        {
            m_Keyword = -1;
            m_Substring = null;
        }

        public KeywordTeleporter(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write(m_Substring);
            writer.Write(m_Keyword);
            writer.Write(m_Range);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_Substring = reader.ReadString();
                        m_Keyword = reader.ReadInt();
                        m_Range = reader.ReadInt();

                        break;
                    }
            }
        }
    }
}