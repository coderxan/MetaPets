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
    public class ShakingThingsUp : MLQuest
    {
        public ShakingThingsUp()
        {
            Activated = true;
            Title = 1073083; // Shaking Things Up
            Description = 1073573; // A Solen hive is a fascinating piece of ecology. It's put together like a finely crafted clock. Who knows what happens if you remove something? So let's find out. Exterminate a few of the warriors and I'll make it worth your while.
            RefusalMessage = 1073580; // I hope you'll reconsider. Until then, farwell.
            InProgressMessage = 1073593; // I don't think you've gotten their attention yet -- you need to kill at least 10 Solen Warriors.

            ObjectiveType = ObjectiveType.Any;

            Objectives.Add(new KillObjective(10, new Type[] { typeof(RedSolenWarrior) }, "red solen warriors"));
            Objectives.Add(new KillObjective(10, new Type[] { typeof(BlackSolenWarrior) }, "black solen warriors"));

            Rewards.Add(ItemReward.BagOfTreasure);
        }
    }
}