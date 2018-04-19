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
    public class ImprovedCrossbows : MLQuest
    {
        public ImprovedCrossbows()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1074021; // Improved Crossbows
            Description = 1074115; // How lazy is man! You cannot even be bothered to pull your own drawstring and hold an arrow ready? You must invent a device to do it for you? I cannot understand, but perhaps if I examine a heavy crossbow for myself, I will see their appeal. Go and bring me such a device and I will repay your meager favor.
            RefusalMessage = 1074063; // Fine then, I'm shall find another to run my errands then.
            InProgressMessage = 1074064; // Hurry up! I don't have all day to wait for you to bring what I desire!
            CompletionMessage = 1074065; // These human made goods are laughable! It offends so -- I must show you what elven skill is capable of!

            Objectives.Add(new CollectObjective(10, typeof(HeavyCrossbow), 1025116)); // heavy crossbow

            Rewards.Add(ItemReward.FletchingSatchel);
        }
    }
}