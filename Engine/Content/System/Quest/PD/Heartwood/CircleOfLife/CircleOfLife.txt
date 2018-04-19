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
    public class CircleOfLife : MLQuest
    {
        public CircleOfLife()
        {
            Activated = true;
            Title = 1073656; // Circle of Life
            Description = 1073695; // There's been a bumper crop of evil with the Bog Things in these parts, my friend. Though they are foul creatures, they are also most fecund. Slay one and you make the land more fertile. Even better, slay several and I will give you whatever coin I can spare.
            RefusalMessage = 1073733; // Perhaps you'll change your mind and return at some point.
            InProgressMessage = 1073736; // Continue to seek and kill the Bog Things.

            Objectives.Add(new KillObjective(8, new Type[] { typeof(BogThing) }, "bog things"));

            Rewards.Add(ItemReward.LargeBagOfTreasure);
        }
    }
}