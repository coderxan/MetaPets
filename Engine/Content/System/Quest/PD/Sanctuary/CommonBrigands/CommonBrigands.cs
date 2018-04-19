using System;
using System.Collections.Generic;

using Server;
using Server.ContextMenus;
using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Items;
using Server.Misc;
using Server.Mobiles;

namespace Server.Engines.MLQuests.Definitions
{
    public class CommonBrigands : MLQuest
    {
        public CommonBrigands()
        {
            Activated = true;
            Title = 1073082; // Common Brigands
            Description = 1073572; // Thank goodness, a hero like you has arrived! Brigands have descended upon this area like locusts, stealing and looting where ever they go. We need someone to put these vile curs where they belong -- in their graves. Are you up to the task? 
            RefusalMessage = 1073580; // I hope you'll reconsider. Until then, farwell.
            InProgressMessage = 1073592; // The Brigands still plague us. Have you killed 20 of their number?<br>

            Objectives.Add(new KillObjective(20, new Type[] { typeof(Brigand) }, 1074894)); // Common brigands

            Rewards.Add(ItemReward.BagOfTreasure);
        }
    }
}