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
    public class Momento : MLQuest
    {
        public Momento()
        {
            Activated = true;
            Title = 1074750; // Momento!
            Description = 1074751; // I was going to march right out there and get it myself, but no ... Master Gnosos won't let me.  But you see, that bridle means so much to me.  A momento of happier, less-dead ... well undead horseback riding.  Could you fetch it for me?  I think my horse, formerly known as 'Resolve', may still be wearing it.
            RefusalMessage = 1074752; // Hrmph.
            InProgressMessage = 1074753; // The bridle would be hard to miss on him now ... since he's skeletal.  Please do what you need to do to retreive it for me.
            CompletionMessage = 1074754; // I'd know that jingling sound anywhere!  You have recovered my bridle.  Thank you.

            Objectives.Add(new CollectObjective(1, typeof(ResolvesBridle), "Resolve's Bridle"));

            Rewards.Add(ItemReward.LargeBagOfTreasure);
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 3, "Kia"), new Point3D(87, 1640, 0), Map.Malas);
            PutSpawner(new Spawner(1, 5, 10, 0, 3, "Nythalia"), new Point3D(91, 1639, 0), Map.Malas);
        }
    }
}