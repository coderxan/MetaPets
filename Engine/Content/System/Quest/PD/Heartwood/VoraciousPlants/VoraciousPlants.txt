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
    public class VoraciousPlants : MLQuest
    {
        public VoraciousPlants()
        {
            Activated = true;
            Title = 1073001; // Voracious Plants
            Description = 1073024; // I bet you can't tangle with those nasty plants ... say eight corpsers and two swamp tentacles!  I bet they're too much for you. You may as well confess you can't ...
            RefusalMessage = 1073019; // Hahahaha!  I knew it!
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

            Objectives.Add(new KillObjective(8, new Type[] { typeof(Corpser) }, "corpsers"));
            Objectives.Add(new KillObjective(2, new Type[] { typeof(SwampTentacle) }, "swamp tentacles"));

            Rewards.Add(ItemReward.BagOfTrinkets);
        }
    }
}