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
    public class BecomingOneWithTheShadows : MLQuest
    {
        public BecomingOneWithTheShadows()
        {
            Activated = true;
            OneTimeOnly = true;
            Title = 1078164; // Becoming One with the Shadows
            Description = 1078168; // Practice hiding in the Ninja Dojo until you reach 50 Hiding skill.<br><center>------</center><br>Come closer. Don't be afraid. The shadows will not harm you. To be a successful Ninja, you must learn to become one with the shadows. The Ninja Dojo is the ideal place to learn the art of concealment. Practice hiding here.<br><br>Talk to me once you have achieved the rank of Apprentice Rogue (for Hiding), and I shall reward you.
            RefusalMessage = 1078169; // If you wish to become one with the shadows, come back and talk to me.
            InProgressMessage = 1078170; // You have not achieved the rank of Apprentice Rogue (for Hiding). Talk to me when you feel you have accomplished this.
            CompletionMessage = 1078172; // Not bad at all. You have learned to control your fear of the dark and you are becoming one with the shadows. If you haven't already talked to Jun, I advise you do so. Jun can teach you how to stealth undetected. Hiding and Stealth are essential skills to master when becoming a Ninja.<br><br>As promised, I have a reward for you. Here are some smokebombs. As long as you are an Apprentice Ninja and have mana available you will be able to use them. They will allow you to hide while in the middle of combat. I hope these serve you well.
            CompletionNotice = 1078171; // You have achieved the rank of Apprentice Rogue (for Hiding). Return to Chiyo in New Haven to claim your reward.

            Objectives.Add(new GainSkillObjective(SkillName.Hiding, 500, true, true));

            Rewards.Add(new ItemReward(1078173, typeof(BagOfSmokeBombs))); // Bag of Smoke Bombs
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Chiyo"), new Point3D(3420, 2516, 21), Map.Trammel);
        }
    }
}