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
    public class TheArtOfStealth : MLQuest
    {
        public TheArtOfStealth()
        {
            Activated = true;
            OneTimeOnly = true;
            Title = 1078154; // The Art of Stealth
            Description = 1078158; // Head East out of town and go to Old Haven. While wielding your fencing weapon, battle monsters with focus attack and summon mirror images up to 40 Ninjitsu skill, and continue practicing focus attack on monsters until 50 Ninjitsu skill.<br><center>------</center><br>Welcome, young one. You seek to learn Ninjitsu. With it, and the book of Ninjitsu, a Ninja can evoke a number of special abilities including transforming into a variety of creatures that give unique bonuses, using stealth to attack unsuspecting opponents or just plain disappear into thin air! If you do not have a book of Ninjitsu, you can purchase one from me.<br><br>I have an assignment for you. Head East out of town and go to Old Haven. While wielding your fencing weapon, battle monsters with focus attack and summon mirror images up to Novice rank, and continue focusing your attacks for greater damage on monsters until you become an Apprentice Ninja. Each image will absorb one attack. The art of deception is a strong defense. Use it wisely.<br><br>Come back to me once you have achieved the rank of Apprentice Ninja, and I shall reward you with something useful.
            RefusalMessage = 1078159; // Come back to me if you with to learn Ninjitsu in the future.
            InProgressMessage = 1078160; // You have not achieved the rank of Apprentice Ninja. Come back to me once you have done so.
            CompletionMessage = 1078162; // You have done well, young one. Please accept this kryss as a gift. It is called the "Silver Serpent Blade". With it, you will strike with precision and power. This should aid you in your journey as a Ninja. Farewell.
            CompletionNotice = 1078161; // You have achieved the rank of Apprentice Ninja. Return to Ryuichi in New Haven to see what kind of reward he has waiting for you.

            Objectives.Add(new GainSkillObjective(SkillName.Ninjitsu, 500, true, true));

            Rewards.Add(new ItemReward(1078163, typeof(SilverSerpentBlade))); // Silver Serpent Blade
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Ryuichi"), new Point3D(3422, 2520, 21), Map.Trammel);
        }
    }
}