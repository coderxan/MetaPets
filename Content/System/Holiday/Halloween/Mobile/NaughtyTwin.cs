using System;
using System.Collections.Generic;

using Server.Events.Halloween;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Engines.Events
{
    public class NaughtyTwin : BaseCreature
    {
        private Mobile m_From;

        private static Point3D[] Felucca_Locations =
		{
			new Point3D( 4467, 1283, 5 ), // Moonglow
			new Point3D( 1336, 1997, 5 ), // Britain
			new Point3D( 1499, 3771, 5 ), // Jhelom
			new Point3D(  771,  752, 5 ), // Yew
			new Point3D( 2701,  692, 5 ), // Minoc
			new Point3D( 1828, 2948,-20), // Trinsic
			new Point3D(  643, 2067, 5 ), // Skara Brae
			new Point3D( 3563, 2139, Map.Trammel.GetAverageZ( 3563, 2139 ) ), // (New) Magincia
		};

        private static Point3D[] Malas_Locations =
		{
			new Point3D(1015, 527, -65), // Luna
			new Point3D(1997, 1386, -85) // Umbra
		};

        private static Point3D[] Ilshenar_Locations =
		{
			new Point3D( 1215,  467, -13 ), // Compassion
			new Point3D(  722, 1366, -60 ), // Honesty
			new Point3D(  744,  724, -28 ), // Honor
			new Point3D(  281, 1016,   0 ), // Humility
			new Point3D(  987, 1011, -32 ), // Justice
			new Point3D( 1174, 1286, -30 ), // Sacrifice
			new Point3D( 1532, 1340, - 3 ), // Spirituality
			new Point3D(  528,  216, -45 ), // Valor
			new Point3D( 1721,  218,  96 )  // Chaos
		};

        private static Point3D[] Tokuno_Locations =
		{
			new Point3D( 1169,  998, 41 ), // Isamu-Jima
			new Point3D(  802, 1204, 25 ), // Makoto-Jima
			new Point3D(  270,  628, 15 )  // Homare-Jima
		};

        public NaughtyTwin(Mobile from)
            : base(AIType.AI_Melee, FightMode.None, 10, 1, 0.2, 0.4)
        {
            if (TrickOrTreat.CheckMobile(from))
            {
                Body = from.Body;

                m_From = from;
                Name = String.Format("{0}\'s Naughty Twin", from.Name);

                Timer.DelayCall<Mobile>(TrickOrTreat.OneSecond, Utility.RandomBool() ? new TimerStateCallback<Mobile>(StealCandy) : new TimerStateCallback<Mobile>(ToGate), m_From);
            }
        }

        public override void OnThink()
        {
            if (m_From == null || m_From.Deleted)
            {
                Delete();
            }
        }

        public static Item FindCandyTypes(Mobile target)
        {
            Type[] types = { typeof(WrappedCandy), typeof(Lollipops), typeof(NougatSwirl), typeof(Taffy), typeof(JellyBeans) };

            if (TrickOrTreat.CheckMobile(target))
            {
                for (int i = 0; i < types.Length; i++)
                {
                    Item item = target.Backpack.FindItemByType(types[i]);

                    if (item != null)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        public static void StealCandy(Mobile target)
        {
            if (TrickOrTreat.CheckMobile(target))
            {
                Item item = FindCandyTypes(target);

                target.SendLocalizedMessage(1113967); /* Your naughty twin steals some of your candy. */

                if (item != null && !item.Deleted)
                {
                    item.Delete();
                }
            }
        }

        public static void ToGate(Mobile target)
        {
            if (TrickOrTreat.CheckMobile(target))
            {
                target.SendLocalizedMessage(1113972); /* Your naughty twin teleports you away with a naughty laugh! */

                target.MoveToWorld(RandomMoongate(target), target.Map);
            }
        }

        public static Point3D RandomMoongate(Mobile target)
        {
            Map map = target.Map;

            switch (target.Map.MapID)
            {
                case 2: return Ilshenar_Locations[Utility.Random(Ilshenar_Locations.Length)];
                case 3: return Malas_Locations[Utility.Random(Malas_Locations.Length)];
                case 4: return Tokuno_Locations[Utility.Random(Tokuno_Locations.Length)];
                default: return Felucca_Locations[Utility.Random(Felucca_Locations.Length)];
            }
        }

        public NaughtyTwin(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}