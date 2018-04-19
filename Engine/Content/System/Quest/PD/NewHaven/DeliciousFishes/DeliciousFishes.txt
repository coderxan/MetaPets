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
    public class DeliciousFishes : MLQuest
    {
        public DeliciousFishes()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1075555; // Delicious Fishes
            Description = 1075556; // Ello there, looking for a good place on the dock to fish? I like the southeast corner meself. What's that? Oh, no, *sighs* me pole is broken and in for fixin'. My grandpappy gave me that pole, means a lot you see. Miss the taste of fish though... Oh say, since you're here, could you catch me a few fish? I can cook a mean fish steak, and I'll split 'em with you! But make sure it's one of the green kind, they're the best for seasoning!
            RefusalMessage = 1075558; // Ah, you're missin' out my friend, you're missing out. My peppercorn fishsteaks are famous on this little isle of ours!
            InProgressMessage = 1075559; // Eh? Find yerself a pole and get close to some water. Just toss the line on in and hopefully you won't snag someone's old boots! Remember, that's twenty of them green fish we'll be needin', so come back when you've got em, 'aight?
            CompletionMessage = 1075560; // Just a moment my friend, just a moment! *rummages in his pack* Here we are! My secret blend of peppers always does the trick, never fails, no not once. These'll fill you up much faster than that tripe they sell in the market!

            Objectives.Add(new CollectObjective(5, typeof(Fish), 1022508)); // fish

            Rewards.Add(new ItemReward(1075557, typeof(PeppercornFishsteak), 3)); // peppercorn fishsteak
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Norton"), new Point3D(3502, 2603, 1), Map.Trammel);
        }
    }
}