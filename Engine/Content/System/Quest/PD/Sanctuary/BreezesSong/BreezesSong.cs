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
    public class BreezesSong : MLQuest
    {
        public BreezesSong()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1074052; // Breeze's Song
            Description = 1074146; // I understand humans cruely enslave the very wind to their selfish whims! Fancy wind chimes, what a monstrous idea! You must bring me proof of this terrible depredation - hurry, bring me wind chimes!
            RefusalMessage = 1074063; // Fine then, I'm shall find another to run my errands then.
            InProgressMessage = 1074064; // Hurry up! I don't have all day to wait for you to bring what I desire!
            CompletionMessage = 1074065; // These human made goods are laughable! It offends so -- I must show you what elven skill is capable of!

            Objectives.Add(new CollectObjective(10, typeof(FancyWindChimes), 1030291)); // fancy wind chimes

            Rewards.Add(ItemReward.TinkerSatchel);
        }
    }
}