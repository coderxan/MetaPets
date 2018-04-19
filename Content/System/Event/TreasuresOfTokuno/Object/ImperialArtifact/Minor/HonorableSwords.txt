using System;

using Server;
using Server.Misc;
using Server.Mobiles;

namespace Server.Items
{
    public class HonorableSwords : Item
    {
        private string m_SwordsName;

        [CommandProperty(AccessLevel.GameMaster)]
        public string SwordsName
        {
            get { return m_SwordsName; }
            set { m_SwordsName = value; }
        }

        public override int LabelNumber { get { return 1071015; } } // Honorable Swords

        [Constructable]
        public HonorableSwords(string swordsName)
            : base(0x2853)
        {
            m_SwordsName = swordsName;

            Weight = 5.0;
        }

        [Constructable]
        public HonorableSwords()
            : this(AncientUrn.Names[Utility.Random(AncientUrn.Names.Length)])
        {
        }

        public HonorableSwords(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
            writer.Write(m_SwordsName);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
            m_SwordsName = reader.ReadString();

            Utility.Intern(ref m_SwordsName);
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            list.Add(1070936, m_SwordsName); // Honorable Swords of ~1_name~
        }

        public override void OnSingleClick(Mobile from)
        {
            LabelTo(from, 1070936, m_SwordsName); // Honorable Swords of ~1_name~
        }
    }
}