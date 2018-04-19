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
    public class EmbracingHumanity : MLQuest
    {
        public EmbracingHumanity()
        {
            Activated = true;
            OneTimeOnly = true; // OSI has no limit or delay, VERY exploitable
            Title = 1074349; // Embracing Humanity
            Description = 1074357; // Well, I don't mind saying it -- I'm flabbergasted!  Absolutely astonished.  I just heard that some elves want to convert themselves to humans through some magical process.  My cousin Nedrick does whatever needs doing.  I guess you could check it out for yourself if you're curious.  Anyway, I wonder if you'll bring my cousin, Drithen, this here treat my wife baked up for him special.
            RefusalMessage = 1074459; // That's okay, I'll find someone else to make the delivery.
            InProgressMessage = 1074460; // If I knew where my cousin was, I'd make the delivery myself.
            CompletionMessage = 1074461; // Oh, hello there.  What do you have for me?

            Objectives.Add(new DeliverObjective(typeof(SpecialTreatForDrithen), 1, "treat for Drithen", typeof(Drithen)));

            Rewards.Add(ItemReward.BagOfTreasure);
        }
    }
}