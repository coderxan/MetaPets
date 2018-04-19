using System;
using Server;
using Server.Mobiles;

namespace Server
{
    public partial class OppositionGroup
    {
        private static OppositionGroup m_SavagesAndOrcs = new OppositionGroup(new Type[][]
			{
				new Type[]
				{
                    typeof( Savage ),
					typeof( SavageRider ),
					typeof( SavageRidgeback ),
					typeof( SavageShaman )
				},
				new Type[]
				{
                    typeof( Orc ),
					typeof( OrcBomber ),
					typeof( OrcBrute ),
					typeof( OrcCaptain ),
					typeof( OrcishLord ),
					typeof( OrcishMage ),
					typeof( SpawnedOrcishLord )
				}
			});

        public static OppositionGroup SavagesAndOrcs
        {
            get { return m_SavagesAndOrcs; }
        }
    }
}