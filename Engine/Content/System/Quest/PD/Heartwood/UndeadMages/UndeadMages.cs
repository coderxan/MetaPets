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
    public class UndeadMages : MLQuest
    {
        public UndeadMages()
        {
            Activated = true;
            Title = 1073080; // Undead Mages
            Description = 1073570; // Why must the dead plague the living? With their foul necromancy and dark sorceries, the undead menace the countryside. I fear what will happen if no one is strong enough to face these nightmare sorcerers and thin their numbers.
            RefusalMessage = 1073580; // I hope you'll reconsider. Until then, farwell.
            InProgressMessage = 1073590; // Surely, a brave soul like yourself can kill 10 Bone Magi and Skeletal Mages?

            Objectives.Add(new KillObjective(10, new Type[] { typeof(BoneMagi), typeof(SkeletalMage) }, "bone mages or skeletal mages"));

            Rewards.Add(ItemReward.BagOfTreasure);
        }
    }
}