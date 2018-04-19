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
    public class AFeatherInYerCap : MLQuest
    {
        public AFeatherInYerCap()
        {
            Activated = true;
            Title = 1074738; // A Feather in Yer Cap
            Description = 1074737; // I've seen how you strut about, as if you were something special. I have some news for you, you don't impress me at all. It's not enough to have a fancy hat you know.  That may impress people in the big city, but not here. If you want a reputation you have to climb a mountain, slay some great beast, and then write about it. Trust me, it's a long process.  The first step is doing a great feat. If I were you, I'd go pluck a feather from the harpy Saliva, that would give you a good start.
            RefusalMessage = 1074736; // The path to greatness isn't for everyone obviously.
            InProgressMessage = 1074735; // If you're going to get anywhere in the adventuring game, you have to take some risks.  A harpy, well, it's bad, but it's not a dragon.
            CompletionMessage = 1074734; // The hero returns from the glorious battle and - oh, such a small feather?

            Objectives.Add(new CollectObjective(1, typeof(SalivasFeather), "Saliva's Feather"));

            Rewards.Add(ItemReward.BagOfTreasure);
        }
    }
}