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
    public class Spirits : MLQuest
    {
        public Spirits()
        {
            Activated = true;
            Title = 1073076; // Spirits
            Description = 1073566; // It is a piteous thing when the dead continue to walk the earth. Restless spirits are known to inhabit these parts, taking the lives of unwary travelers. It is about time a hero put the dead back in their graves. I'm sure such a hero would be justly rewarded.
            RefusalMessage = 1073580; // I hope you'll reconsider. Until then, farwell.
            InProgressMessage = 1073586; // The restless spirts still walk -- you must kill 15 of them.

            Objectives.Add(new KillObjective(15, new Type[] { typeof(Spectre), typeof(Shade), typeof(Wraith) }, "spectres or shades or wraiths"));

            Rewards.Add(ItemReward.BagOfTreasure);
        }
    }
}