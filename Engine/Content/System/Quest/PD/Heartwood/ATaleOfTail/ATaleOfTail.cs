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
    public class ATaleOfTail : MLQuest
    {
        public ATaleOfTail()
        {
            Activated = true;
            Title = 1074726; // A Tale of Tail
            Description = 1074727; // I've heard of you, adventurer.  Your reputation is impressive, and now I'll put it to the test. This is not something I ask lightly, for this task is fraught with danger, but it is vital.  Seek out the vile hydra Abscess, slay it, and return to me with it's tail.
            RefusalMessage = 1074728; // Well, the beast will still be there when you are ready I suppose.
            InProgressMessage = 1074729; // Em, I thought I had explained already.  Abscess, the hydra, you know? Lots of heads but just the one tail. I need the tail. I have my reasons. Go go go.
            CompletionMessage = 1074730; // Ah, the tail.  You did it!  You know the rumours about dried ground hydra tail powder are all true? Thank you so much!

            Objectives.Add(new CollectObjective(1, typeof(AbscessTail), "Abscess' Tail"));

            Rewards.Add(ItemReward.BagOfTreasure);
        }
    }
}