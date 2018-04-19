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
    public class ImpishDelights : MLQuest
    {
        public ImpishDelights()
        {
            Activated = true;
            Title = 1073077; // Impish Delights
            Description = 1073567; // Imps! Do you hear me? Imps! They're everywhere! They're in everything! Oh, don't be fooled by their size - they vicious little devils! Half-sized evil incarnate, they are! Somebody needs to send them back to where they came from, if you know what I mean.
            RefusalMessage = 1073580; // I hope you'll reconsider. Until then, farwell.
            InProgressMessage = 1073587; // Don't let the little devils scare you! You  kill 12 imps - then we'll talk reward.

            Objectives.Add(new KillObjective(12, new Type[] { typeof(Imp) }, "imps"));

            Rewards.Add(ItemReward.BagOfTreasure);
        }
    }
}