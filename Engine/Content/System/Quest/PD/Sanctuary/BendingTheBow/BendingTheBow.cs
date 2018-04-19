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
    public class BendingTheBow : MLQuest
    {
        public BendingTheBow()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1074019; // Bending the Bow
            Description = 1074113; // Human craftsmanship! Ha! Why, take an elven bow. It will last for a lifetime, never break and always shoot an arrow straight and true. Can't say the same for a human, can you? Bring me some of these human made bows, and I will show you.
            RefusalMessage = 1074063; // Fine then, I'm shall find another to run my errands then.
            InProgressMessage = 1074064; // Hurry up! I don't have all day to wait for you to bring what I desire!
            CompletionMessage = 1074065; // These human made goods are laughable! It offends so -- I must show you what elven skill is capable of!

            Objectives.Add(new CollectObjective(10, typeof(Bow), 1025041)); // bow

            Rewards.Add(ItemReward.FletchingSatchel);
        }
    }
}