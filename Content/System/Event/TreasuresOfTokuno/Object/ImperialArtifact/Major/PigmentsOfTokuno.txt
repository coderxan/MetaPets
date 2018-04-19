using System;

using Server;
using Server.Misc;
using Server.Mobiles;

namespace Server.Items
{
    public enum PigmentType
    {
        None,
        ParagonGold,
        VioletCouragePurple,
        InvulnerabilityBlue,
        LunaWhite,
        DryadGreen,
        ShadowDancerBlack,
        BerserkerRed,
        NoxGreen,
        RumRed,
        FireOrange,
        FadedCoal,
        Coal,
        FadedGold,
        StormBronze,
        Rose,
        MidnightCoal,
        FadedBronze,
        FadedRose,
        DeepRose
    }

    public class PigmentsOfTokuno : BasePigmentsOfTokuno
    {
        private static int[][] m_Table = new int[][]
		{
			// Hue, Label
			new int[]{ /*PigmentType.None,*/ 0, -1 },
			new int[]{ /*PigmentType.ParagonGold,*/ 0x501, 1070987 },
			new int[]{ /*PigmentType.VioletCouragePurple,*/ 0x486, 1070988 },
			new int[]{ /*PigmentType.InvulnerabilityBlue,*/ 0x4F2, 1070989 },
			new int[]{ /*PigmentType.LunaWhite,*/ 0x47E, 1070990 },
			new int[]{ /*PigmentType.DryadGreen,*/ 0x48F, 1070991 },
			new int[]{ /*PigmentType.ShadowDancerBlack,*/ 0x455, 1070992 },
			new int[]{ /*PigmentType.BerserkerRed,*/ 0x21, 1070993 },
			new int[]{ /*PigmentType.NoxGreen,*/ 0x58C, 1070994 },
			new int[]{ /*PigmentType.RumRed,*/ 0x66C, 1070995 },
			new int[]{ /*PigmentType.FireOrange,*/ 0x54F, 1070996 },
			new int[]{ /*PigmentType.Fadedcoal,*/ 0x96A, 1079579 },
			new int[]{ /*PigmentType.Coal,*/ 0x96B, 1079580 },
			new int[]{ /*PigmentType.FadedGold,*/ 0x972, 1079581 },
			new int[]{ /*PigmentType.StormBronze,*/ 0x977, 1079582 },
			new int[]{ /*PigmentType.Rose,*/ 0x97C, 1079583 },
			new int[]{ /*PigmentType.MidnightCoal,*/ 0x96C, 1079584 },
			new int[]{ /*PigmentType.FadedBronze,*/ 0x975, 1079585 },
			new int[]{ /*PigmentType.FadedRose,*/ 0x97B, 1079586 },
			new int[]{ /*PigmentType.DeepRose,*/ 0x97E, 1079587 }
		};

        public static int[] GetInfo(PigmentType type)
        {
            int v = (int)type;

            if (v < 0 || v >= m_Table.Length)
                v = 0;

            return m_Table[v];
        }

        private PigmentType m_Type;

        [CommandProperty(AccessLevel.GameMaster)]
        public PigmentType Type
        {
            get { return m_Type; }
            set
            {
                m_Type = value;

                int v = (int)m_Type;

                if (v >= 0 && v < m_Table.Length)
                {
                    Hue = m_Table[v][0];
                    Label = m_Table[v][1];
                }
                else
                {
                    Hue = 0;
                    Label = -1;
                }
            }
        }

        public override int LabelNumber { get { return 1070933; } } // Pigments of Tokuno

        [Constructable]
        public PigmentsOfTokuno()
            : this(PigmentType.None, 10)
        {
        }

        [Constructable]
        public PigmentsOfTokuno(PigmentType type)
            : this(type, (type == PigmentType.None || type >= PigmentType.FadedCoal) ? 10 : 50)
        {
        }

        [Constructable]
        public PigmentsOfTokuno(PigmentType type, int uses)
            : base(uses)
        {
            Weight = 1.0;
            Type = type;
        }

        public PigmentsOfTokuno(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1);

            writer.WriteEncodedInt((int)m_Type);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = (InheritsItem ? 0 : reader.ReadInt()); // Required for BasePigmentsOfTokuno insertion

            switch (version)
            {
                case 1: Type = (PigmentType)reader.ReadEncodedInt(); break;
                case 0: break;
            }
        }
    }
}