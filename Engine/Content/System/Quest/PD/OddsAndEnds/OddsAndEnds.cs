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
    public class OddsAndEnds : MLQuest
    {
        public OddsAndEnds()
        {
            Activated = true;
            Title = 1074354; // Odds and Ends
            Description = 1074677; // I've always been fascinated by primitive cultures -- especially the artifacts.  I'm a collector, you see.  I'm working on building my troglodyte display and I'm saddened to say that I'm short on examples of religion and superstition amongst the creatures.  If you come across any primitive fetishes, I'd be happy to trade you something interesting for them.
            RefusalMessage = 1072270; // Well, okay. But if you decide you are up for it after all, c'mon back and see me.
            InProgressMessage = 1074678; // I don't really want to know where you get the primitive fetishes, as I can't support the destruction of their lifestyle and culture. That would be wrong.
            CompletionMessage = 1074679; // Bravo!  These fetishes are just what I needed.  You've earned this reward.

            Objectives.Add(new CollectObjective(12, typeof(PrimitiveFetish), "Primitive Fetishes"));

            Rewards.Add(ItemReward.BagOfTreasure);
        }
    }
}