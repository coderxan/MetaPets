using System;
using System.Collections;

using Server;
using Server.Engines.Quests;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Engines.Quests.Collector
{
    public class EnchantedPaints : QuestItem
    {
        [Constructable]
        public EnchantedPaints()
            : base(0xFC1)
        {
            LootType = LootType.Blessed;

            Weight = 1.0;
        }

        public EnchantedPaints(Serial serial)
            : base(serial)
        {
        }

        public override bool CanDrop(PlayerMobile player)
        {
            CollectorQuest qs = player.Quest as CollectorQuest;

            if (qs == null)
                return true;

            /*return !( qs.IsObjectiveInProgress( typeof( CaptureImagesObjective ) )
                || qs.IsObjectiveInProgress( typeof( ReturnImagesObjective ) ) );*/
            return false;
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile player = from as PlayerMobile;

            if (player != null)
            {
                QuestSystem qs = player.Quest;

                if (qs is CollectorQuest)
                {
                    if (qs.IsObjectiveInProgress(typeof(CaptureImagesObjective)))
                    {
                        player.SendAsciiMessage(0x59, "Target the creature whose image you wish to create.");
                        player.Target = new InternalTarget(this);

                        return;
                    }
                }
            }

            from.SendLocalizedMessage(1010085); // You cannot use this.
        }

        private class InternalTarget : Target
        {
            private EnchantedPaints m_Paints;

            public InternalTarget(EnchantedPaints paints)
                : base(-1, false, TargetFlags.None)
            {
                CheckLOS = false;
                m_Paints = paints;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (m_Paints.Deleted || !m_Paints.IsChildOf(from.Backpack))
                    return;

                PlayerMobile player = from as PlayerMobile;

                if (player != null)
                {
                    QuestSystem qs = player.Quest;

                    if (qs is CollectorQuest)
                    {
                        CaptureImagesObjective obj = qs.FindObjective(typeof(CaptureImagesObjective)) as CaptureImagesObjective;

                        if (obj != null && !obj.Completed)
                        {
                            if (targeted is Mobile)
                            {
                                ImageType image;
                                CaptureResponse response = obj.CaptureImage((targeted.GetType().Name == "GreaterMongbat" ? new Mongbat().GetType() : targeted.GetType()), out image);

                                switch (response)
                                {
                                    case CaptureResponse.Valid:
                                        {
                                            player.SendLocalizedMessage(1055125); // The enchanted paints swirl for a moment then an image begins to take shape. *Click*
                                            player.AddToBackpack(new PaintedImage(image));

                                            break;
                                        }
                                    case CaptureResponse.AlreadyDone:
                                        {
                                            player.SendAsciiMessage(0x2C, "You have already captured the image of this creature");

                                            break;
                                        }
                                    case CaptureResponse.Invalid:
                                        {
                                            player.SendLocalizedMessage(1055124); // You have no interest in capturing the image of this creature.

                                            break;
                                        }
                                }
                            }
                            else
                            {
                                player.SendAsciiMessage(0x35, "You have no interest in that.");
                            }

                            return;
                        }
                    }
                }

                from.SendLocalizedMessage(1010085); // You cannot use this.
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public enum ImageType
    {
        Betrayer,
        Bogling,
        BogThing,
        Gazer,
        Beetle,
        GiantBlackWidow,
        Scorpion,
        JukaMage,
        JukaWarrior,
        Lich,
        MeerMage,
        MeerWarrior,
        Mongbat,
        Mummy,
        Pixie,
        PlagueBeast,
        SandVortex,
        StoneGargoyle,
        SwampDragon,
        Wisp,
        Juggernaut
    }

    public class ImageTypeInfo
    {
        private static readonly ImageTypeInfo[] m_Table = new ImageTypeInfo[]
			{
				new ImageTypeInfo( 9734, typeof( Betrayer ), 75, 45 ),
				new ImageTypeInfo( 9735, typeof( Bogling ), 75, 45 ),
				new ImageTypeInfo( 9736, typeof( BogThing ), 60, 47 ),
				new ImageTypeInfo( 9615, typeof( Gazer ), 75, 45 ),
				new ImageTypeInfo( 9743, typeof( Beetle ), 60, 55 ),
				new ImageTypeInfo( 9667, typeof( GiantBlackWidow ), 55, 52 ),
				new ImageTypeInfo( 9657, typeof( Scorpion ), 65, 47 ),
				new ImageTypeInfo( 9758, typeof( JukaMage ), 75, 45 ),
				new ImageTypeInfo( 9759, typeof( JukaWarrior ), 75, 45 ),
				new ImageTypeInfo( 9636, typeof( Lich ), 75, 45 ),
				new ImageTypeInfo( 9756, typeof( MeerMage ), 75, 45 ),
				new ImageTypeInfo( 9757, typeof( MeerWarrior ), 75, 45 ),
				new ImageTypeInfo( 9638, typeof( Mongbat ), 70, 50 ),
				new ImageTypeInfo( 9639, typeof( Mummy ), 75, 45 ),
				new ImageTypeInfo( 9654, typeof( Pixie ), 75, 45 ),
				new ImageTypeInfo( 9747, typeof( PlagueBeast ), 60, 45 ),
				new ImageTypeInfo( 9750, typeof( SandVortex ), 60, 43 ),
				new ImageTypeInfo( 9614, typeof( StoneGargoyle ), 75, 45 ),
				new ImageTypeInfo( 9753, typeof( SwampDragon ), 50, 55 ),
				new ImageTypeInfo( 8448, typeof( Wisp ), 75, 45 ),
				new ImageTypeInfo( 9746, typeof( Juggernaut ), 55, 38 )
			};

        public static ImageTypeInfo Get(ImageType image)
        {
            int index = (int)image;
            if (index >= 0 && index < m_Table.Length)
                return m_Table[index];
            else
                return m_Table[0];
        }

        public static ImageType[] RandomList(int count)
        {
            ArrayList list = new ArrayList(m_Table.Length);
            for (int i = 0; i < m_Table.Length; i++)
                list.Add((ImageType)i);

            ImageType[] images = new ImageType[count];

            for (int i = 0; i < images.Length; i++)
            {
                int index = Utility.Random(list.Count);
                images[i] = (ImageType)list[index];

                list.RemoveAt(index);
            }

            return images;
        }

        private int m_Figurine;
        private Type m_Type;
        private int m_X;
        private int m_Y;

        public int Figurine { get { return m_Figurine; } }
        public Type Type { get { return m_Type; } }
        public int Name { get { return m_Figurine < 0x4000 ? 1020000 + m_Figurine : 1078872 + m_Figurine; } }
        public int X { get { return m_X; } }
        public int Y { get { return m_Y; } }

        public ImageTypeInfo(int figurine, Type type, int x, int y)
        {
            m_Figurine = figurine;
            m_Type = type;
            m_X = x;
            m_Y = y;
        }
    }
}