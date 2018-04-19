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
    public class TheBulwark : MLQuest
    {
        public TheBulwark()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1073912; // The Bulwark
            Description = 1074102; // The clank of human iron and steel is strange to elven ears. For instance, the metallic heater shield which human warriors carry into battle. It is odd to an elf, but nevertheless intriguing. Tell me friend, could you bring me such an example of human smithing skill?
            RefusalMessage = 1073921; // I will patiently await your reconsideration.
            InProgressMessage = 1073958; // I will be in your debt if you bring me heater shields.
            CompletionMessage = 1073978; // Enjoy my thanks for your service.

            Objectives.Add(new CollectObjective(10, typeof(HeaterShield), 1027030)); // heater shield

            Rewards.Add(ItemReward.BlacksmithSatchel);
        }
    }
}