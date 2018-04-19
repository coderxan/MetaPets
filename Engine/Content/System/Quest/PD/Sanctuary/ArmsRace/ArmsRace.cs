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
    public class ArmsRace : MLQuest
    {
        public ArmsRace()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1074020; // Arms Race
            Description = 1074114; // Leave it to a human to try and improve upon perfection. To take a bow and turn it into a mechanical contraption like a crossbow. I wish to see more of this sort of "invention". Fetch for me a crossbow, human.
            RefusalMessage = 1074063; // Fine then, I'm shall find another to run my errands then.
            InProgressMessage = 1074064; // Hurry up! I don't have all day to wait for you to bring what I desire!
            CompletionMessage = 1074065; // These human made goods are laughable! It offends so -- I must show you what elven skill is capable of!

            Objectives.Add(new CollectObjective(10, typeof(Crossbow), 1023919)); // crossbow

            Rewards.Add(ItemReward.FletchingSatchel);
        }
    }
}