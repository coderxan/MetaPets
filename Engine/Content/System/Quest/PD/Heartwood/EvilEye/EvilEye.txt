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
    public class EvilEye : MLQuest
    {
        public EvilEye()
        {
            Activated = true;
            Title = 1073084; // Evil Eye
            Description = 1073574; // Kind traveler, hear my plea. You know of the evil orbs? The wrathful eyes? Some call them gazers? They must be a nest nearby, for they are tormenting us poor folk. We need to drive back their numbers. But we are not strong enough to face such horrors ourselves, we need a true hero.
            RefusalMessage = 1073580; // I hope you'll reconsider. Until then, farwell.
            InProgressMessage = 1073594; // Have you annihilated a dozen Gazers yet, kind traveler?

            Objectives.Add(new KillObjective(12, new Type[] { typeof(Gazer) }, "gazers"));

            Rewards.Add(ItemReward.BagOfTreasure);
        }
    }
}