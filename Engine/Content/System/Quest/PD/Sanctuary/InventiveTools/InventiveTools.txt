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
    public class InventiveTools : MLQuest
    {
        public InventiveTools()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1074048; // Inventive Tools
            Description = 1074142; // Bring me some of these tinker's tools! I am certain, in the hands of an elf, they will fashion objects of ingenuity and delight that will shame all human invention! Hurry, do this quickly and I might deign to show you my skill.
            RefusalMessage = 1074063; // Fine then, I'm shall find another to run my errands then.
            InProgressMessage = 1074064; // Hurry up! I don't have all day to wait for you to bring what I desire!
            CompletionMessage = 1074065; // These human made goods are laughable! It offends so -- I must show you what elven skill is capable of!

            Objectives.Add(new CollectObjective(10, typeof(TinkerTools), 1027868)); // tinker's tools

            Rewards.Add(ItemReward.TinkerSatchel);
        }
    }
}