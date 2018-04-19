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
    public class ThePuffyShirt : MLQuest
    {
        public ThePuffyShirt()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1073903; // The Puffy Shirt
            Description = 1074093; // We elves believe that beauty is expressed in all things, including the garments we wear. I wish to understand more about human aesthetics, so please kind traveler - could you bring to me magnificent examples of human fancy shirts? For my thanks, I could teach you more about the beauty of elven vestements.
            RefusalMessage = 1073921; // I will patiently await your reconsideration.
            InProgressMessage = 1073949; // I will be in your debt if you bring me fancy shirts.
            CompletionMessage = 1073973; // I appreciate your service. Now, see what elven hands can create.

            Objectives.Add(new CollectObjective(10, typeof(FancyShirt), 1027933)); // fancy shirt

            Rewards.Add(ItemReward.TailorSatchel);
        }
    }
}