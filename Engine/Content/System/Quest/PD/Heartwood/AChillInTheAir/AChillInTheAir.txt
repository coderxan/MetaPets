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
    public class AChillInTheAir : MLQuest
    {
        public AChillInTheAir()
        {
            Activated = true;
            Title = 1073663; // A Chill in the Air
            Description = 1073702; // Feel that chill in the air? It means an icy death for the unwary, for deadly Ice Elementals are about. Who knows what magic summoned them, what's important now is getting rid of them. I don't have much, but I'll give all I can if you'd only stop the cold-hearted monsters.
            RefusalMessage = 1073733; // Perhaps you'll change your mind and return at some point.
            InProgressMessage = 1073746; // The chill won't lift until you eradicate a few Ice Elemenals.

            Objectives.Add(new KillObjective(15, new Type[] { typeof(IceElemental) }, "ice elementals"));

            Rewards.Add(ItemReward.LargeBagOfTreasure);
        }
    }
}