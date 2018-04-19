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
    public class ASimpleBow : MLQuest
    {
        public ASimpleBow()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1073877; // A Simple Bow
            Description = 1074067; // I wish to try a bow crafted in the human style. Is it possible for you to bring me such a weapon? I would be happy to return this favor.
            RefusalMessage = 1073921; // I will patiently await your reconsideration.
            InProgressMessage = 1073923; // I will be in your debt if you bring me bows.
            CompletionMessage = 1073968; // My thanks for your service. Now, I shall teach you of elven archery.
            CompletionNotice = CompletionNoticeCraft;

            Objectives.Add(new CollectObjective(10, typeof(Bow), 1025041)); // bow

            Rewards.Add(ItemReward.FletchingSatchel);
        }
    }
}