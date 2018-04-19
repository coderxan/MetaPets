using System;

using Server;
using Server.Mobiles;

namespace Server
{
    public partial class OppositionGroup
    {
        private static OppositionGroup m_UndeadAndFey = new OppositionGroup(new Type[][]
			{
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
				},
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
				}
			});

        public static OppositionGroup UndeadAndFey
        {
            get { return m_UndeadAndFey; }
        }
    }
}