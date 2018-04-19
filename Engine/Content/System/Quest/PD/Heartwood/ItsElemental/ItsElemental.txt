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
    public class ItsElemental : MLQuest
    {
        public ItsElemental()
        {
            Activated = true;
            Title = 1073089; // It's Elemental
            Description = 1073579; // The universe is all about balance my friend. Tip one end, you must balance the other. That's why I must ask you to kill not just one kind of elemental, but three kinds. Snuff out some Fire, douse a few Water, and crush some Earth elementals and I'll pay you for your trouble.
            RefusalMessage = 1073580; // I hope you'll reconsider. Until then, farwell.
            InProgressMessage = 1073599; // Four of each, that's all I ask. Water, earth and fire.

            Objectives.Add(new KillObjective(4, new Type[] { typeof(FireElemental) }, "fire elementals"));
            Objectives.Add(new KillObjective(4, new Type[] { typeof(WaterElemental) }, "water elementals"));
            Objectives.Add(new KillObjective(4, new Type[] { typeof(EarthElemental) }, "earth elementals"));

            Rewards.Add(ItemReward.BagOfTreasure);
        }
    }
}