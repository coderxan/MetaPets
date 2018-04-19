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
    public class StirringTheNest : MLQuest
    {
        public StirringTheNest()
        {
            Activated = true;
            Title = 1073087; // Stirring the Nest
            Description = 1073577; // Were you the sort of child that enjoyed knocking over anthills? Well, perhaps you'd like to try something a little bigger? There's a Solen nest nearby and I bet if you killed a queen or two, it would be quite the sight to behold.  I'd even pay to see that - what do you say?
            RefusalMessage = 1073580; // I hope you'll reconsider. Until then, farwell.
            InProgressMessage = 1073597; // Dead Solen Queens isn't too much to ask, is it?

            ObjectiveType = ObjectiveType.Any;

            Objectives.Add(new KillObjective(3, new Type[] { typeof(RedSolenQueen) }, "red solen queens"));
            Objectives.Add(new KillObjective(3, new Type[] { typeof(BlackSolenQueen) }, "black solen queens"));

            Rewards.Add(ItemReward.BagOfTreasure);
        }
    }
}