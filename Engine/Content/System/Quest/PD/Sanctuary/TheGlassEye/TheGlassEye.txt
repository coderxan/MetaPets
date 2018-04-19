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
    public class TheGlassEye : MLQuest
    {
        public TheGlassEye()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1074050; // The Glass Eye
            Description = 1074144; // Humans are so pathetically weak, they must be augmented by glass and metal! Imagine such a thing! I must see one of these spyglasses for myself, to understand the pathetic limits of human sight!
            RefusalMessage = 1074063; // Fine then, I'm shall find another to run my errands then.
            InProgressMessage = 1074064; // Hurry up! I don't have all day to wait for you to bring what I desire!
            CompletionMessage = 1074065; // These human made goods are laughable! It offends so -- I must show you what elven skill is capable of!

            Objectives.Add(new CollectObjective(10, typeof(Spyglass), 1025365)); // spyglass

            Rewards.Add(ItemReward.TinkerSatchel);
        }
    }
}