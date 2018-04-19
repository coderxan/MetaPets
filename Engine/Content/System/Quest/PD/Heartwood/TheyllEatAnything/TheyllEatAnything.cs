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
    public class TheyllEatAnything : MLQuest
    {
        public TheyllEatAnything()
        {
            Activated = true;
            Title = 1072248; // They'll Eat Anything
            Description = 1072262; // Pork is the fruit of the land!  You can barbeque it, boil it, bake it, sautee it.  There's pork kebabs, pork creole, pork gumbo, pan fried, deep fried, stir fried.  There's apple pork, peppered pork, pork soup, pork salad, pork and potatoes, pork burger, pork sandwich, pork stew, pork chops, pork loins, shredded pork. So, lets get some piggies butchered!
            RefusalMessage = 1072270; // Well, okay. But if you decide you are up for it after all, c'mon back and see me.
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

            Objectives.Add(new KillObjective(10, new Type[] { typeof(Pig) }, "pigs"));

            Rewards.Add(ItemReward.SmallBagOfTrinkets);
        }
    }
}