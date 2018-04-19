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
    public class TheKingOfClothing : MLQuest
    {
        public TheKingOfClothing()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1073902; // The King of Clothing
            Description = 1074092; // I have heard noble tales of a fine and proud human garment. An article of clothing fit for both man and god alike. It is called a "kilt" I believe? Could you fetch for me some of these kilts so I that I might revel in their majesty and glory?
            RefusalMessage = 1073921; // I will patiently await your reconsideration.
            InProgressMessage = 1073948; // I will be in your debt if you bring me kilts.
            CompletionMessage = 1073974; // I say truly - that is a magnificent garment! You have more than earned a reward.

            Objectives.Add(new CollectObjective(10, typeof(Kilt), 1025431)); // kilt

            Rewards.Add(ItemReward.TailorSatchel);
        }
    }
}