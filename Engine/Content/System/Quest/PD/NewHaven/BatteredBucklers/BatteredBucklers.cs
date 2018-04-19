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
    public class BatteredBucklers : MLQuest
    {
        public BatteredBucklers()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1075511; // Battered Bucklers
            Description = 1075512; // Hey there! Yeah... you! Ya' any good with a hammer? Tell ya what, if yer thinking about tryin' some metal work, and have a bit of skill, I can show ya how to bend it into shape. Just get some of those ingots there, and grab a hammer and use it over here at this forge. I need a few more bucklers hammered out to fill this here order with...  hmmm about ten more. that'll give some taste of how to work the metal.
            RefusalMessage = 1075514; // Not enough muscle on yer bones to use it? hmph, probably afraid of the sparks markin' up yer loverly skin... to good for some honest labor... ha!... off with ya!
            InProgressMessage = 1075515; // Come On! Whats that... a bucket? We need ten bucklers... not spitoons.
            CompletionMessage = 1075516; // Thanks for the help. Here's something for ya to remember me by.

            Objectives.Add(new CollectObjective(10, typeof(Buckler), 1027027)); // buckler

            Rewards.Add(new ItemReward(1074282, typeof(GervisSatchel))); // Craftsmans's Satchel
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Gervis"), new Point3D(3505, 2749, 0), Map.Trammel);
        }
    }
}