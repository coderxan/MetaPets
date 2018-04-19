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
    public class NecessitysMother : MLQuest
    {
        public NecessitysMother()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1073906; // Necessity's Mother
            Description = 1074096; // What a thing, this human need to tinker. It seems there is no end to what might be produced with a set of Tinker's Tools. Who knows what an elf might build with some? Could you obtain some tinker's tools and bring them to me? In exchange, I offer you elven lore and knowledge.
            RefusalMessage = 1073921; // I will patiently await your reconsideration.
            InProgressMessage = 1073952; // I will be in your debt if you bring me tinker's tools.
            CompletionMessage = 1073977; // Now, I shall see what an elf can invent!

            Objectives.Add(new CollectObjective(10, typeof(TinkerTools), 1027868)); // tinker's tools

            Rewards.Add(ItemReward.TinkerSatchel);
        }
    }
}