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
    public class AClockworkPuzzle : MLQuest
    {
        public AClockworkPuzzle()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1075535; // A clockwork puzzle
            Description = 1075534; // 'Tis a riddle, you see! "What kind of clock is only right twice per day? A broken one!" *laughs heartily* Ah, yes *wipes eye*, that's one of my favorites! Ah... to business. Could you fashion me some clock parts? I wish my own clocks to be right all the day long! You'll need some tinker's tools and some iron ingots, I think, but from there it should be just a matter of working the metal.
            RefusalMessage = 1072981; // Or perhaps you'd rather not.
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!
            CompletionMessage = 1075536; // Wonderful! Tick tock, tick tock, soon all shall be well with grandfather's clock!

            Objectives.Add(new CollectObjective(5, typeof(ClockParts), 1024175)); // clock parts

            Rewards.Add(new ItemReward(1074282, typeof(NibbetSatchel))); // Craftsmans's Satchel
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Nibbet"), new Point3D(3459, 2525, 53), Map.Trammel);
        }
    }
}