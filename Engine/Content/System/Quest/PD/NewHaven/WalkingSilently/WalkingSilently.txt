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
    public class WalkingSilently : MLQuest
    {
        public WalkingSilently()
        {
            Activated = true;
            OneTimeOnly = true;
            Title = 1078174; // Walking Silently
            Description = 1078178; // Head East out of town and go to Old Haven. While wearing normal clothes, practice Stealth there until you reach 50 Stealth skill.<br><center>------</center><br>You there. You're not very quiet in your movements. I can help you with that. Not only must you must learn to become one with the shadows, but also you must learn to quiet your movements. Old Haven is the ideal place to learn how to Stealth.<br><br>Head East out of town and go to Old Haven. While wearing normal clothes, practice Stealth there. Stealth becomes more difficult as you wear heavier pieces of armor, so for now, only wear clothes while practicing Stealth.<br><br>You can only Stealth once you are hidden.  If you become visible, use your Hiding skill, and begin slowing walking.<br><br>Come back to me once you have achieved the rank of Apprentice Rogue (for Stealth), and I will reward you with something useful.
            RefusalMessage = 1078179; // If you want to learn to quiet your movements, talk to me, and I will help you.
            InProgressMessage = 1078180; // You have not achieved the rank of Apprentice Rogue (for Stealth). Come back to me when you feel you have accomplished this.
            CompletionMessage = 1078182; // Good. You have learned to quiet your movements. If you haven't already talked to Chiyo, I advise you do so. Chiyo can teach you how to become one with the shadows. Hiding and Stealth are essential skills to master when becoming a Ninja.<br><br>Here is your reward. This leather Ninja jacket is called "Twilight Jacket". It will offer greater protection to you. I hope this serve you well.
            CompletionNotice = 1078181; // You have achieved the rank of Apprentice Rogue (for Stealth). Return to Jun in New Haven to claim your reward.

            Objectives.Add(new GainSkillObjective(SkillName.Stealth, 500, true, true));

            Rewards.Add(new ItemReward(1078183, typeof(TwilightJacket))); // Twilight Jacket
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Jun"), new Point3D(3422, 2516, 21), Map.Trammel);
        }
    }
}