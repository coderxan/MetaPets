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
    public class ReptilianDentist : MLQuest
    {
        public ReptilianDentist()
        {
            Activated = true;
            Title = 1074280; // Reptilian Dentist
            Description = 1074710; // I'm working on a striking necklace -- something really unique -- and I know just what I need to finish it up.  A huge fang!  Won't that catch the eye?  I would like to employ you to find me such an item, perhaps a snake would make the ideal donor.  I'll make it worth your while, of course.
            RefusalMessage = 1074723; // I understand.  I don't like snakes much either.  They're so creepy.
            InProgressMessage = 1074722; // Those really big snakes like swamps, I've heard.  You might try the blighted grove.
            CompletionMessage = 1074721; // Do you have it?  *gasp* What a tooth!  Here … I must get right to work.

            Objectives.Add(new CollectObjective(1, typeof(CoilsFang), "coil's fang"));

            Rewards.Add(ItemReward.TinkerSatchel);
        }
    }
}