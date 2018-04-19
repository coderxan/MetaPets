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
    public class GargoylesWrath : MLQuest
    {
        public GargoylesWrath()
        {
            Activated = true;
            Title = 1073658; // Gargoyle's Wrath
            Description = 1073697; // It is regretable that the Gargoyles insist upon warring with us. Their Enforcers attack men on sight, despite all efforts at reason. To help maintain order in this region, I have been authorized to encourage bounty hunters to reduce their numbers. Eradicate their number and I will reward you handsomely.
            RefusalMessage = 1073733; // Perhaps you'll change your mind and return at some point.
            InProgressMessage = 1073738; // I won't be able to pay you until you've gotten enough Gargoyle Enforcers.

            Objectives.Add(new KillObjective(6, new Type[] { typeof(GargoyleEnforcer) }, "gargoyle enforcers"));

            Rewards.Add(ItemReward.LargeBagOfTreasure);
        }
    }
}