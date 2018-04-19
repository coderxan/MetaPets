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
    public class TheSongOfTheWind : MLQuest
    {
        public TheSongOfTheWind()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1073910; // The Song of the Wind
            Description = 1074100; // To give voice to the passing wind, this is an idea worthy of an elf! Friend, bring me some of the amazing fancy wind chimes so that I may listen to the song of the passing breeze. Do this, and I will share with you treasured elven secrets.
            RefusalMessage = 1073921; // I will patiently await your reconsideration.
            InProgressMessage = 1073956; // I will be in your debt if you bring me fancy wind chimes.
            CompletionMessage = 1073980; // Such a delightful sound, I think I shall never tire of it.

            Objectives.Add(new CollectObjective(10, typeof(FancyWindChimes), "fancy wind chimes"));

            Rewards.Add(ItemReward.TinkerSatchel);
        }
    }
}