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
    public class PointyEars : MLQuest
    {
        public PointyEars()
        {
            Activated = true;
            Title = 1074640; // Pointy Ears
            Description = 1074641; // I've heard ... there's some that will pay a good bounty for pointed ears, much like we used to pay for each wolf skin.  I've got nothing personal against these elves.  It's just business.  You want in on this?  I'm not fussy who I work with.
            RefusalMessage = 1074642; // Suit yourself.
            InProgressMessage = 1074643; // I can't pay a bounty if you don't bring bag the ears.
            CompletionMessage = 1074644; // Here to collect on a bounty?

            Objectives.Add(new CollectObjective(20, typeof(SeveredElfEars), 1032590)); // severed elf ears

            Rewards.Add(ItemReward.BagOfTrinkets);
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 4, "Drithen"), new Point3D(1983, 1364, -80), Map.Malas);
        }
    }
}