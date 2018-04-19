using System;

using Server;
using Server.Misc;
using Server.Mobiles;

namespace Server.Items
{
    public enum LesserPigmentType
    {
        None,
        PaleOrange,
        FreshRose,
        ChaosBlue,
        Silver,
        NobleGold,
        LightGreen,
        PaleBlue,
        FreshPlum,
        DeepBrown,
        BurntBrown
    }

    public class LesserPigmentsOfTokuno : BasePigmentsOfTokuno
    {

        private static int[][] m_Table = new int[][]
		{
			// Hue, Label
			new int[]{ /*PigmentType.None,*/ 0, -1 },
			new int[]{ /*PigmentType.PaleOrange,*/ 0x02E, 1071458 },
			new int[]{ /*PigmentType.FreshRose,*/ 0x4B9, 1071455 },
			new int[]{ /*PigmentType.ChaosBlue,*/ 0x005, 1071459 },
			new int[]{ /*PigmentType.Silver,*/ 0x3E9, 1071451 },
			new int[]{ /*PigmentType.NobleGold,*/ 0x227, 1071457 },
			new int[]{ /*PigmentType.LightGreen,*/ 0x1C8, 1071454 },
			new int[]{ /*PigmentType.PaleBlue,*/ 0x24F, 1071456 },
			new int[]{ /*PigmentType.FreshPlum,*/ 0x145, 1071450 },
			new int[]{ /*PigmentType.DeepBrown,*/ 0x3F0, 1071452 },
			new int[]{ /*PigmentType.BurntBrown,*/ 0x41A, 1071453 }
		};

        public static int[] GetInfo(LesserPigmentType type)
        {
            int v = (int)type;

            if (v < 0 || v >= m_Table.Length)
                v = 0;

            return m_Table[v];
        }

        private LesserPigmentType m_Type;

        [CommandProperty(AccessLevel.GameMaster)]
        public LesserPigmentType Type
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

        [Constructable]
        public LesserPigmentsOfTokuno()
            : this((LesserPigmentType)Utility.Random(0, 11))
        {
        }

        [Constructable]
        public LesserPigmentsOfTokuno(LesserPigmentType type)
            : base(1)
        {
            Weight = 1.0;
            Type = type;
        }

        public LesserPigmentsOfTokuno(Serial serial)
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
                case 1: Type = (LesserPigmentType)reader.ReadEncodedInt(); break;
                case 0: break;
            }
        }
    }
}