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
    public class ThePenIsMightier : MLQuest
    {
        public ThePenIsMightier()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1075542; // The Pen is Mightier
            Description = 1075543; // Do you know anything about 'Inscription?' I've been trying to get my hands on some hand crafted Recall scrolls for a while now, and I could really use some help. I don't have a scribe's pen, let alone a spellbook with Recall in it, or blank scrolls, so there's no way I can do it on my own. How about you though? I could trade you one of my old leather bound books for some.
            RefusalMessage = 1075546; // Hmm, thought I had your interest there for a moment. It's not everyday you see a book made from real daemon skin, after all!
            InProgressMessage = 1075547; // Inscribing... yes, you'll need a scribe's pen, some reagents, some blank scroll, and of course your own magery book. You might want to visit the magery shop if you're lacking some materials.
            CompletionMessage = 1075548; // Ha! Finally! I've had a rune to the waterfalls near Justice Isle that I've been wanting to use for the longest time, and now I can visit at last. Here's that book I promised you... glad to be rid of it, to be honest.

            Objectives.Add(new CollectObjective(5, typeof(RecallScroll), "recall scroll"));

            Rewards.Add(new ItemReward(1075545, typeof(RedLeatherBook))); // a book bound in red leather
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Lyle"), new Point3D(3503, 2584, 14), Map.Trammel);
        }
    }
}