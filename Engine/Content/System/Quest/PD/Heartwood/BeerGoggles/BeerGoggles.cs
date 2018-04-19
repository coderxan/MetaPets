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
    public class BeerGoggles : MLQuest
    {
        public BeerGoggles()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1073895; // Beer Goggles
            Description = 1074085; // Oh, the deviltry! Why would humans lock their precious liquors inside a wooden coffin? I understand I need a "keg tap" to access the golden brew within such a wooden abomination. Perhaps, if you could bring me such a tap, we could share a drink and I could teach you.
            RefusalMessage = 1073921; // I will patiently await your reconsideration.
            InProgressMessage = 1073941; // I will be in your debt if you bring me barrel taps.
            CompletionMessage = 1073971; // My thanks for your service.  Here is something for you to enjoy.

            Objectives.Add(new CollectObjective(25, typeof(BarrelTap), 1024100)); // barrel tap

            Rewards.Add(ItemReward.TinkerSatchel);
        }
    }
}