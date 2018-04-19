using System;

using Server;
using Server.Mobiles;

namespace Server
{
    public partial class OppositionGroup
    {
        private static OppositionGroup m_FeyAndUndead = new OppositionGroup(new Type[][]
			{
				new Type[]
				{
					typeof( Centaur ),
					typeof( EtherealWarrior ),
					typeof( Kirin ),
					typeof( LordOaks ),
					typeof( Pixie ),
					typeof( Silvani ),
					typeof( Unicorn ),
					typeof( Wisp ),
					typeof( Treefellow ),
					typeof( MLDryad ),
					typeof( Satyr )
				},
				new Type[]
				{
					typeof( AncientLich ),
					typeof( Bogle ),
					typeof( LichLord ),
					typeof( Shade ),
					typeof( Spectre ),
					typeof( Wraith ),
					typeof( BoneKnight ),
					typeof( Ghoul ),
					typeof( Mummy ),
					typeof( SkeletalKnight ),
					typeof( Skeleton ),
					typeof( Zombie ),
					typeof( ShadowKnight ),
					typeof( DarknightCreeper ),
					typeof( RevenantLion ),
					typeof( LadyOfTheSnow ),
					typeof( RottingCorpse ),
					typeof( SkeletalDragon ),
					typeof( Lich )
				}
			});

        public static OppositionGroup FeyAndUndead
        {
            get { return m_FeyAndUndead; }
        }
    }
}