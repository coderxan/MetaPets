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
    public class IShotAnArrowIntoTheAir : MLQuest
    {
        public IShotAnArrowIntoTheAir()
        {
            Activated = true;
            Title = 1075486; // I Shot an Arrow Into the Air...
            Description = 1075482; // Truth be told, the only way to get a feel for the bow is to shoot one and there's no better practice target than a sheep. If ye can shoot ten of them I think ye will have proven yer abilities. Just grab a bow and make sure to take enough ammunition. Bows tend to use arrows and crossbows use bolts. Ye can buy 'em or have someone craft 'em. How about it then? Come back here when ye are done.
            RefusalMessage = 1075483; // Fair enough, the bow isn't for everyone. Good day then.
            InProgressMessage = 1075484; // Return once ye have killed ten sheep with a bow and not a moment before.

            Objectives.Add(new KillObjective(10, new Type[] { typeof(Sheep) }, 1018270)); // sheep

            Rewards.Add(ItemReward.BagOfTrinkets);
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Kashiel"), new Point3D(3744, 2586, 40), Map.Trammel);
        }
    }
}