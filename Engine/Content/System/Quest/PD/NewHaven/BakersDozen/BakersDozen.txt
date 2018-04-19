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
    public class BakersDozen : MLQuest
    {
        public BakersDozen()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1075478; // Baker's Dozen
            Description = 1075479; // You there! Do you know much about the ways of cooking? If you help me out, I'll show you a thing or two about how it's done. Bring me some cookie mix, about 5 batches will do it, and I will reward you. Although, I don't think you can buy it, you can make some in a snap! First get a rolling pin or frying pan or even a flour sifter. Then you mix one pinch of flour with some water and you've got some dough! Take that dough and add one dollop of honey and you've got sweet dough. add one more drop of honey and you've got cookie mix. See? Nothing to it! Now get to work!
            RefusalMessage = 1075480; // Argh, I absolutely must have more of these 'cookies!' Come back if you change your mind.
            InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!
            CompletionMessage = 1075481; // Thank you! I haven't been this excited about food in months!

            Objectives.Add(new CollectObjective(5, typeof(CookieMix), 1024159)); // cookie mix

            Rewards.Add(new ItemReward(1074282, typeof(AsandosSatchel))); // Craftsmans's Satchel
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Asandos"), new Point3D(3505, 2513, 27), Map.Trammel);
        }
    }
}