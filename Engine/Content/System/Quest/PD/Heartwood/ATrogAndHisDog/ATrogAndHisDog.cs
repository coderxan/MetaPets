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
    public class ATrogAndHisDog : MLQuest
    {
        public ATrogAndHisDog()
        {
            Activated = true;
            Title = 1074681; // A Trog and His Dog
            Description = 1074680; // I don't know if you can handle it, but I'll give you a go at it. Troglodyte chief - name of Lurg and his mangy wolf pet need killing. Do the deed and I'll reward you.
            RefusalMessage = 1074655; // Perhaps I thought too highly of you.
            InProgressMessage = 1074682; // The trog chief and his mutt should be easy enough to find. Just kill them and report back.  Easy enough.
            CompletionMessage = 1074683; // Not half bad.  Here's your prize.

            Objectives.Add(new KillObjective(1, new Type[] { typeof(Lurg) }, "Lurg"));
            Objectives.Add(new KillObjective(1, new Type[] { typeof(Grobu) }, "Grobu"));

            Rewards.Add(ItemReward.LargeBagOfTreasure);
        }
    }
}