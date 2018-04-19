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
    public class GlassyFoe : MLQuest
    {
        public GlassyFoe()
        {
            Activated = true;
            Title = 1073055; // Glassy Foe
            Description = 1074669; // Good, you're here.  The presence of a twisted creature deep under the earth near Nu'Jelm has corrupted the natural growth of crystals in that region.  They've become infused with the twisting energy - they've come to a sort of life.  This is an abomination that festers within Sosaria.  You must eradicate the crystal lattice seekers.
            RefusalMessage = 1074671; // These abominations must not be permitted to fester!
            InProgressMessage = 1074672; // You must not waste time. Do not suffer these crystalline abominations to live.
            CompletionMessage = 1074673; // You have done well.  Enjoy this reward.

            Objectives.Add(new KillObjective(5, new Type[] { typeof(CrystalLatticeSeeker) }, "crystal lattice seekers", new QuestArea(1074805, "The Prism of Light"))); // The Prism of Light

            Rewards.Add(ItemReward.LargeBagOfTreasure);
        }
    }
}