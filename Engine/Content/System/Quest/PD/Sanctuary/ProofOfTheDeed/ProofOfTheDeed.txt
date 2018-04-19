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
    public class ProofOfTheDeed : MLQuest
    {
        public ProofOfTheDeed()
        {
            Activated = true;
            Title = 1072339; // Proof of the Deed
            Description = 1072340; // These human vermin must be erradicated!  They despoil fair Sosaria with their every footfall upon her soil, every exhalation of breath upon her pristine air.  Prove yourself an ally of Sosaria and bring me 20 human ears as proof of your devotion to our cause.
            RefusalMessage = 1072342; // Do you find the task distasteful?  Are you too weak to shoulder the duty of cleansing Sosaria?  So be it.
            InProgressMessage = 1072343; // Well, where is the proof of your deed?  I will honor your actions when you have brought me the ears of the human scum.
            CompletionMessage = 1072344; // Ah, well done.  You have chosen the path of duty and fulfilled your task with honor.

            Objectives.Add(new CollectObjective(20, typeof(SeveredHumanEars), 1032591)); // severed human ears

            Rewards.Add(ItemReward.BagOfTrinkets);
        }
    }
}