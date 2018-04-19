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
    public class FilthyPests : MLQuest
    {
        public FilthyPests()
        {
            Activated = true;
            Title = 1072242; // Filthy Pests!
            Description = 1072253; // They're everywhere I tell you!  They crawl in the walls, they scurry in the bushes.  Disgusting critters. Say ... I don't suppose you're up for some sewer rat killing?  Sewer rats now, not any other kind of squeaker will do.
            RefusalMessage = 1072270; // Well, okay. But if you decide you are up for it after all, c'mon back and see me.
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

            Objectives.Add(new KillObjective(10, new Type[] { typeof(Sewerrat) }, "sewer rats"));

            Rewards.Add(ItemReward.SmallBagOfTrinkets);
        }
    }
}