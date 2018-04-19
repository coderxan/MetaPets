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
    public class TheFarEye : MLQuest
    {
        public TheFarEye()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1073908; // The Far Eye
            Description = 1074098; // The wonders of human invention! Turning sand and metal into a far-seeing eye! This is something I must experience for myself. Bring me some of these spyglasses friend human.
            RefusalMessage = 1073921; // I will patiently await your reconsideration.
            InProgressMessage = 1073954; // I will be in your debt if you bring me spyglasses.
            CompletionMessage = 1073978; // Enjoy my thanks for your service.

            Objectives.Add(new CollectObjective(20, typeof(Spyglass), 1025365)); // spyglass

            Rewards.Add(ItemReward.TinkerSatchel);
        }
    }
}