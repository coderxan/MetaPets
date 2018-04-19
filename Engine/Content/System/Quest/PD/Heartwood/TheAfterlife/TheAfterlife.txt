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
    public class TheAfterlife : MLQuest
    {
        public TheAfterlife()
        {
            Activated = true;
            Title = 1073073; // The Afterlife
            Description = 1073563; // Nobody told me about the Mummy's Curse. How was I supposed to know you shouldn't disturb the tombs? Oh, sure, now all I hear about is the curse of the vengeful dead. I'll tell you what - make a few of these mummies go away and we'll keep this between you and me.
            RefusalMessage = 1073580; // I hope you'll reconsider. Until then, farwell.
            InProgressMessage = 1073583; // Uh, I don't think you're quite done killing Mummies yet.

            Objectives.Add(new KillObjective(15, new Type[] { typeof(Mummy) }, "mummies"));

            Rewards.Add(ItemReward.BagOfTreasure);
        }
    }
}