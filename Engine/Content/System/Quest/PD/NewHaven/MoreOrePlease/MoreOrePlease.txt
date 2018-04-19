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
    public class MoreOrePlease : MLQuest
    {
        public MoreOrePlease()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1075530; // More Ore Please
            Description = 1075529; // Have a pickaxe? My supplier is late and I need some iron ore so I can complete a bulk order for another merchant. If you can get me some soon I'll pay you double what it's worth on the market. Just find a cave or mountainside and try to use your pickaxe there, maybe you'll strike a good vein! 5 large pieces should do it.
            RefusalMessage = 1075531; // Not feeling strong enough today? Its alright, I didn't need a bucket of rocks anyway.
            InProgressMessage = 1075532; // Hmmm… we need some more Ore. Try finding a mountain or cave, and give it a whack.
            CompletionMessage = 1075533; // I see you found a good vien! Great!  This will help get this order out on time. Good work!

            Objectives.Add(new InternalObjective());

            Rewards.Add(new ItemReward(1074282, typeof(MuggSatchel))); // Craftsmans's Satchel
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Mugg"), new Point3D(3507, 2747, 0), Map.Trammel);
        }

        private class InternalObjective : CollectObjective
        {
            // Any type of ore is allowed
            public InternalObjective()
                : base(5, typeof(BaseOre), 1026585) // ore
            {
            }

            public override bool CheckItem(Item item)
            {
                return (item.ItemID == 6585); // Only large pieces count
            }
        }
    }
}