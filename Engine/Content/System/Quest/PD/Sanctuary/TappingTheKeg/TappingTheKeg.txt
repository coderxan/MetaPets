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
    public class TappingTheKeg : MLQuest
    {
        public TappingTheKeg()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1074037; // Tapping the Keg
            Description = 1074131; // I have acquired a barrel of human brewed beer. I am loathe to drink it, but how else to prove how inferior it is? I suppose I shall need a barrel tap to drink. Go, bring me a barrel tap quickly, so I might get this over with.
            RefusalMessage = 1074063; // Fine then, I'm shall find another to run my errands then.
            InProgressMessage = 1074064; // Hurry up! I don't have all day to wait for you to bring what I desire!
            CompletionMessage = 1074065; // These human made goods are laughable! It offends so -- I must show you what elven skill is capable of!

            Objectives.Add(new CollectObjective(10, typeof(BarrelTap), 1024100)); // barrel tap

            Rewards.Add(ItemReward.TinkerSatchel);
        }
    }
}