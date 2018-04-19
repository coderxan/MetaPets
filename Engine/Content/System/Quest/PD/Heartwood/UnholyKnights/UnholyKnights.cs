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
    public class UnholyKnights : MLQuest
    {
        public UnholyKnights()
        {
            Activated = true;
            Title = 1073075; // Unholy Knights
            Description = 1073565; // Please, hear me kind traveler. You know when a knight falls, sometimes they are cursed to roam the earth as undead mockeries of their former glory? That is too grim a fate for even any knight to suffer! Please, put them out of their misery. I will offer you what payment I can if you will end the torment of these undead wretches.
            RefusalMessage = 1073580; // I hope you'll reconsider. Until then, farwell.
            InProgressMessage = 1073585; // Your task is not done. Continue putting the Skeleton and Bone Knights to rest.

            Objectives.Add(new KillObjective(16, new Type[] { typeof(BoneKnight), typeof(SkeletalKnight) }, "bone knights or skeletal knights"));

            Rewards.Add(ItemReward.BagOfTreasure);
        }
    }
}