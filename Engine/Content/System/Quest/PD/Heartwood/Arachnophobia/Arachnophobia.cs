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
    public class Arachnophobia : MLQuest
    {
        public Arachnophobia()
        {
            Activated = true;
            Title = 1073079; // Arachnophobia
            Description = 1073569; // I've seen them hiding in their webs among the woods. Glassy eyes, spindly legs, poisonous fangs. Monsters, I say! Deadly horrors, these black widows. Someone must exterminate the abominations! If only I could find a worthy hero for such a task, then I could give them this considerable reward.
            RefusalMessage = 1073580; // I hope you'll reconsider. Until then, farwell.
            InProgressMessage = 1073589; // You've got a good start, but to stop the black-eyed fiends, you need to kill a dozen.

            Objectives.Add(new KillObjective(12, new Type[] { typeof(GiantBlackWidow) }, "giant black widows"));

            Rewards.Add(ItemReward.BagOfTreasure);
        }
    }
}