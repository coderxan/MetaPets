using System;

using Server;

namespace Server.Items
{
    public enum SnowGlobeTypeTwo
    {
        AncientCitadel,
        BlackthornesCastle,
        CityofMontor,
        CityofMistas,
        ExodusLair,
        LakeofFire,
        Lakeshire,
        PassofKarnaugh,
        TheEtherealFortress,
        TwinOaksTavern,
        ChaosShrine,
        ShrineofHumility,
        ShrineofSacrifice,
        ShrineofCompassion,
        ShrineofHonor,
        ShrineofHonesty,
        ShrineofSpirituality,
        ShrineofJustice,
        ShrineofValor
    }

    public class SnowGlobeTwo : SnowGlobe
    {
        /* Oddly, these are not localized. */
        private static readonly string[] m_PlaceNames = new string[]
		{
			/* AncientCitadel */ 		"Ancient Citadel",
			/* BlackthornesCastle */ 	"Blackthorne's Castle",
			/* CityofMontor */ 			"City of Montor",
			/* CityofMistas */ 			"City of Mistas",
			/* ExodusLair */ 			"Exodus' Lair",
			/* LakeofFire */ 			"Lake of Fire",
			/* Lakeshire */ 			"Lakeshire",
			/* PassofKarnaugh */ 		"Pass of Karnaugh",
			/* TheEtherealFortress */ 	"The Etheral Fortress",
			/* TwinOaksTavern */ 		"Twin Oaks Tavern",
			/* ChaosShrine */ 			"Chaos Shrine",
			/* ShrineofHumility */ 		"Shrine of Humility",
			/* ShrineofSacrifice */ 	"Shrine of Sacrifice",
			/* ShrineofCompassion */ 	"Shrine of Compassion",
			/* ShrineofHonor */ 		"Shrine of Honor",
			/* ShrineofHonesty */ 		"Shrine of Honesty",
			/* ShrineofSpirituality */ 	"Shrine of Spirituality",
			/* ShrineofJustice */ 		"Shrine of Justice",
			/* ShrineofValor */ 		"Shrine of Valor"
		};

        private SnowGlobeTypeTwo m_Type;

        [CommandProperty(AccessLevel.GameMaster)]
        public SnowGlobeTypeTwo Place
        {
            get { return m_Type; }
            set { m_Type = value; InvalidateProperties(); }
        }

        public override string DefaultName
        {
            get
            {
                int idx = (int)m_Type;

                if (idx < 0 || idx >= m_PlaceNames.Length)
                    return "a snowy scene";

                return String.Format("a snowy scene of {0}", m_PlaceNames[idx]);
            }
        }

        [Constructable]
        public SnowGlobeTwo()
            : this((SnowGlobeTypeTwo)Utility.Random(19))
        {
        }

        [Constructable]
        public SnowGlobeTwo(SnowGlobeTypeTwo type)
        {
            m_Type = type;
        }

        public SnowGlobeTwo(Serial serial)
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
                        m_Type = (SnowGlobeTypeTwo)reader.ReadEncodedInt();
                        break;
                    }
            }
        }
    }
}