using System;

using Server;
using Server.Engines.MLQuests;
using Server.Engines.MLQuests.Items;
using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;

namespace Server.Engines.MLQuests.Definitions
{
    // TODO: Assassination Contract, Evidence, Lost in Transit, Last Words
    public class LostAndFound : MLQuest
    {
        public LostAndFound()
        {
            Activated = true;
            Title = 1072370; // Lost and Found
            Description = 1072589; // The battered, old bucket is inscribed with barely legible writing that indicates it belongs to someone named "Dallid".  Maybe they'd pay for its return?
            RefusalMessage = 1072590; // You're right, who cares if Dallid might pay for his battered old bucket back.  This way you can carry it around with you!
            InProgressMessage = 1072591; // Whoever this "Dallid" might be, he's probably looking for his bucket.
            CompletionMessage = 1074580; // Is that my bucket? I had to ditch my favorite bucket when a group of ratmen jumped me!

            Objectives.Add(new TimedDeliverObjective(TimeSpan.FromSeconds(600), typeof(BatteredBucket), 1, "battered bucket", typeof(Dallid), false));

            Rewards.Add(ItemReward.BagOfTrinkets);
        }
    }
}