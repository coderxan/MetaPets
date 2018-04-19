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
    public class FleeAndFatigue : MLQuest
    {
        public FleeAndFatigue()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1075487; // Flee and Fatigue
            Description = 1075488; // I was just *coughs* ambushed near the moongate. *wheeze* Why do I pay my taxes? Where were the guards? You then, you an Alchemist? If you can make me a few Refresh potions, I will be back on my feet and can give those lizards the what for! Find a mortar and pestle, a good amount of black pearl, and ten empty bottles to store the finished potions in. Just use the mortar and pestle and the rest will surely come to you. When you return, the favor will be repaid.
            RefusalMessage = 1075489; // Fine fine, off with *cough* thee then! The next time you see a lizardman though, give him a whallop for me, eh?
            InProgressMessage = 1075490; // Just remember you need to use your mortar and pestle while you have empty bottles and some black pearl. Refresh potions are what I need.
            CompletionMessage = 1075491; // *glug* *glug* Ahh... Yes! Yes! That feels great! Those lizardmen will never know what hit 'em! Here, take this, I can get more from the lizards.

            Objectives.Add(new CollectObjective(10, typeof(RefreshPotion), "refresh potions"));

            Rewards.Add(new ItemReward(1074282, typeof(SadrahSatchel))); // Craftsmans's Satchel
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Sadrah"), new Point3D(3742, 2731, 7), Map.Trammel);
        }
    }
}