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
    public class CulinaryCrisis : MLQuest
    {
        public CulinaryCrisis()
        {
            Activated = true;
            Title = 1074755; // Culinary Crisis
            Description = 1074756; // You have NO idea how impossible this is.  Simply intolerable!  How can one expect an artiste' like me to create masterpieces of culinary delight without the best, fresh ingredients?  Ever since this whositwhatsit started this uproar, my thrice-daily produce deliveries have ended.  I can't survive another hour without produce!
            RefusalMessage = 1074757; // You have no artistry in your soul.
            InProgressMessage = 1074758; // I must have fresh produce and cheese at once!
            CompletionMessage = 1074759; // Those dates look bruised!  Oh no, and you fetched a soft cheese.  *deep pained sigh*  Well, even I can only do so much with inferior ingredients.  BAM!

            Objectives.Add(new CollectObjective(20, typeof(Dates), 1025927)); // bunch of dates
            Objectives.Add(new CollectObjective(5, typeof(CheeseWheel), 1022430)); // wheel of cheese

            Rewards.Add(ItemReward.BagOfTreasure);
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 3, "Emerillo"), new Point3D(90, 1639, 0), Map.Malas);
        }
    }
}