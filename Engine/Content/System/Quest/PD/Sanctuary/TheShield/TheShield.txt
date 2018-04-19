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
    public class TheShield : MLQuest
    {
        public TheShield()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1074054; // The Shield
            Description = 1074148; // I doubt very much a human shield would stop a good stout elven arrow. You doubt me? I will show you - get me some of these heater shields and I will piece them with sharp elven arrows!
            RefusalMessage = 1074063; // Fine then, I'm shall find another to run my errands then.
            InProgressMessage = 1074064; // Hurry up! I don't have all day to wait for you to bring what I desire!
            CompletionMessage = 1074065; // These human made goods are laughable! It offends so -- I must show you what elven skill is capable of!

            Objectives.Add(new CollectObjective(10, typeof(HeaterShield), 1027030)); // heater shield

            Rewards.Add(ItemReward.BlacksmithSatchel);
        }
    }
}