using System;

using Server;
using Server.Mobiles;

namespace Server
{
    public partial class OppositionGroup
    {
        private static OppositionGroup m_OrcsAndSavages = new OppositionGroup(new Type[][]
			{
				new Type[]
				{
                    typeof( Orc ),
					typeof( OrcBomber ),
					typeof( OrcBrute ),
					typeof( OrcCaptain ),
					typeof( OrcishLord ),
					typeof( OrcishMage ),
					typeof( SpawnedOrcishLord )		
				},
				new Type[]
				{	
                    typeof( Savage ),
					typeof( SavageRider ),
					typeof( SavageRidgeback ),
					typeof( SavageShaman )
				}
			});

        public static OppositionGroup OrcsAndSavages
        {
            get { return m_OrcsAndSavages; }
        }
    }
}