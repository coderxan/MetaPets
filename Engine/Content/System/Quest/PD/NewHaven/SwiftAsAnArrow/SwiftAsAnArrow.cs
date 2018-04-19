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
    public class SwiftAsAnArrow : MLQuest
    {
        public SwiftAsAnArrow()
        {
            Activated = true;
            OneTimeOnly = true;
            Title = 1078201; // Swift as an Arrow
            Description = 1078205; // Head East out of town and go to Old Haven. While wielding your bow or crossbow, battle monster there until you have raised your Archery skill to 50. Well met, friend. Imagine yourself in a distant grove of trees, You raise your bow, take slow, careful aim, and with the twitch of a finger, you impale your prey with a deadly arrow. You look like you would make a excellent archer, but you will need practice. There is no better way to practice Archery than when you life is on the line. I have a challenge for you. Head East out of town and go to Old Haven. While wielding your bow or crossbow, battle the undead that reside there. Make sure you bring a healthy supply of arrows (or bolts if you prefer a crossbow). If you wish to purchase a bow, crossbow, arrows, or bolts, you can purchase them from me or the Archery shop in town. You can also make your own arrows with the Bowcraft/Fletching skill. You will need fletcher's tools, wood to turn into sharft's, and feathers to make arrows or bolts. Come back to me after you have achived the rank of Apprentice Archer, and i will reward you with a fine Archery weapon.
            RefusalMessage = 1078206; // I understand that Archery may not be for you. Feel free to visit me in the future if you change your mind.
            InProgressMessage = 1078207; // You're doing great as an Archer! however, you need more practice.
            CompletionMessage = 1078209; // Congratulation! I want to reward you for your accomplishment. Take this composite bow. It is called " Heartseeker". With it, you will shoot with swiftness, precision, and power. I hope "Heartseeker" serves you well.
            CompletionNotice = 1078208; // You have achieved the rank of Apprentice Archer. Return to Robyn in New Haven to claim your reward.

            Objectives.Add(new GainSkillObjective(SkillName.Archery, 500, true, true));

            Rewards.Add(new ItemReward(1078210, typeof(Heartseeker))); // Heartseeker
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Robyn"), new Point3D(3535, 2531, 20), Map.Trammel);
        }
    }
}