using System;

using Server;
using Server.Mobiles;

namespace Server
{
    public partial class OppositionGroup
    {
        private static OppositionGroup m_OphidiansAndTerathans = new OppositionGroup(new Type[][]
			{
				new Type[]
				{
					typeof( OphidianArchmage ),
					typeof( OphidianKnight ),
					typeof( OphidianMage ),
					typeof( OphidianMatriarch ),
					typeof( OphidianWarrior )
				},
				new Type[]
				{
					typeof( TerathanAvenger ),
					typeof( TerathanDrone ),
					typeof( TerathanMatriarch ),
					typeof( TerathanWarrior )
				}
			});

        public static OppositionGroup OphidiansAndTerathans
        {
            get { return m_OphidiansAndTerathans; }
        }
    }
}