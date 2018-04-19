using System;

using Server;

namespace Server.Items
{
    public enum SnowGlobeTypeOne
    {
        Britain,
        Moonglow,
        Minoc,
        Magincia,
        BuccaneersDen,
        Trinsic,
        Yew,
        SkaraBrae,
        Jhelom,
        Nujelm,
        Papua,
        Delucia,
        Cove,
        Ocllo,
        SerpentsHold,
        EmpathAbbey,
        TheLycaeum,
        Vesper,
        Wind
    }

    public class SnowGlobeOne : SnowGlobe
    {
        private SnowGlobeTypeOne m_Type;

        [CommandProperty(AccessLevel.GameMaster)]
        public SnowGlobeTypeOne Place
        {
            get { return m_Type; }
            set { m_Type = value; InvalidateProperties(); }
        }

        public override int LabelNumber { get { return 1041454 + (int)m_Type; } }

        [Constructable]
        public SnowGlobeOne()
            : this((SnowGlobeTypeOne)Utility.Random(19))
        {
        }

        [Constructable]
        public SnowGlobeOne(SnowGlobeTypeOne type)
        {
            m_Type = type;
        }

        public SnowGlobeOne(Serial serial)
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
                        m_Type = (SnowGlobeTypeOne)reader.ReadEncodedInt();
                        break;
                    }
            }
        }
    }
}