using System;

using Server;

namespace Server.Items
{
    public enum SnowGlobeTypeThree
    {
        Luna,
        Umbra,
        Zento,
        Heartwood,
        Covetous,
        Deceit,
        Destard,
        Hythloth,
        Khaldun,
        Shame,
        Wrong,
        Doom,
        TheCitadel,
        ThePalaceofParoxysmus,
        TheBlightedGrove,
        ThePrismofLight
    }

    public class SnowGlobeThree : SnowGlobe
    {
        private SnowGlobeTypeThree m_Type;

        [CommandProperty(AccessLevel.GameMaster)]
        public SnowGlobeTypeThree Place
        {
            get { return m_Type; }
            set { m_Type = value; InvalidateProperties(); }
        }

        public override int LabelNumber
        {
            get
            {
                if (m_Type >= SnowGlobeTypeThree.Covetous)
                    return 1075440 + ((int)m_Type - 4);

                return 1075294 + (int)m_Type;
            }
        }

        [Constructable]
        public SnowGlobeThree()
            : this((SnowGlobeTypeThree)Utility.Random(16))
        {
        }

        [Constructable]
        public SnowGlobeThree(SnowGlobeTypeThree type)
        {
            m_Type = type;
        }

        public SnowGlobeThree(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
            writer.WriteEncodedInt((int)m_Type);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_Type = (SnowGlobeTypeThree)reader.ReadEncodedInt();
                        break;
                    }
            }
        }
    }
}