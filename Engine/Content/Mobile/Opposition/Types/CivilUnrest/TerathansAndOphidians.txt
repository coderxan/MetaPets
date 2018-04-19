using System;

using Server;
using Server.Mobiles;

namespace Server
{
    public partial class OppositionGroup
    {
        private static OppositionGroup m_TerathansAndOphidians = new OppositionGroup(new Type[][]
			{
				new Type[]
				{
					typeof( TerathanAvenger ),
					typeof( TerathanDrone ),
					typeof( TerathanMatriarch ),
					typeof( TerathanWarrior )
				},
				new Type[]
				{
					typeof( OphidianArchmage ),
					typeof( OphidianKnight ),
					typeof( OphidianMage ),
					typeof( OphidianMatriarch ),
					typeof( OphidianWarrior )
				}
			});

        public static OppositionGroup TerathansAndOphidians
        {
            get { return m_TerathansAndOphidians; }
        }
    }
}