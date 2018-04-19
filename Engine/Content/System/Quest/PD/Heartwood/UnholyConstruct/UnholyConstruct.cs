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
    public class UnholyConstruct : MLQuest
    {
        public UnholyConstruct()
        {
            Activated = true;
            Title = 1073666; // Unholy Construct
            Description = 1073705; // They're unholy, I say. Golems, a walking mockery of all life, born of blackest magic. They're not truly alive, so destroying them isn't a crime, it's a service. A service I will gladly pay for.
            RefusalMessage = 1073733; // Perhaps you'll change your mind and return at some point.
            InProgressMessage = 1073746; // The unholy brutes, the Golems, must be smited!
            CompletionMessage = 1073787; // Reduced those Golems to component parts? Good, then -- you deserve this reward!

            Objectives.Add(new KillObjective(10, new Type[] { typeof(Golem) }, "golems"));

            Rewards.Add(ItemReward.LargeBagOfTreasure);
        }
    }
}