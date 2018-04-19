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
    public class AnImpressivePlaid : MLQuest
    {
        public AnImpressivePlaid()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1074044; // An Impressive Plaid
            Description = 1074138; // I do not believe humans are so ridiculous as to wear something called a "kilt". Bring for me some of these kilts, if they truly exist, and I will offer you meager reward.
            RefusalMessage = 1074063; // Fine then, I'm shall find another to run my errands then.
            InProgressMessage = 1074064; // Hurry up! I don't have all day to wait for you to bring what I desire!
            CompletionMessage = 1074065; // These human made goods are laughable! It offends so -- I must show you what elven skill is capable of!

            Objectives.Add(new CollectObjective(10, typeof(Kilt), 1025431)); // kilt

            Rewards.Add(ItemReward.TailorSatchel);
        }
    }
}