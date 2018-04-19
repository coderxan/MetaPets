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
    public class BigWorms : MLQuest
    {
        public BigWorms()
        {
            Activated = true;
            Title = 1073088; // Big Worms
            Description = 1073578; // It makes no sense! Cold blooded serpents cannot live in the ice! It's a biological impossibility! They are an abomination against reason! Please, I beg you - kill them! Make them disappear for me! Do this and I will reward you.
            RefusalMessage = 1073580; // I hope you'll reconsider. Until then, farwell.
            InProgressMessage = 1073598; // You wouldn't try and just pretend you murdered 10 Giant Ice Worms, would you?

            Objectives.Add(new KillObjective(10, new Type[] { typeof(IceSerpent) }, "giant ice serpents"));

            Rewards.Add(ItemReward.BagOfTreasure);
        }
    }
}