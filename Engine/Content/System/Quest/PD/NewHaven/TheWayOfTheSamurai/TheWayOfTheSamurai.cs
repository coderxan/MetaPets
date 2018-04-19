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
    public class TheWayOfTheSamurai : MLQuest
    {
        public TheWayOfTheSamurai()
        {
            Activated = true;
            OneTimeOnly = true;
            Title = 1078007; // The Way of the Samurai
            Description = 1078010; // Head East out of town and go to Old Haven. use the Confidence defensive stance and attempt to honorably execute monsters there until you have raised your Bushido skill to 50.<br><center>------</center><br>Greetings. I see you wish to learn the Way of the Samurai. Wielding a blade is easy. Anyone can grasp a sword's hilt. Learning how to fight properly and skillfully is to become an Armsman. Learning how to master weapons, and even more importantly when not to use them, is the Way of the Warrior. The Way of the Samurai. The Code of the Bushido. That is why you are here.<br><br>Adventure East to Old Haven. Use the Confidence defensive stance and attempt to honorably execute the undead that inhabit there. You will need a book of Bushido to perform these abilities. If you do not possess a book of Bushido, you can purchase one from me. <br><br>If you fail to honorably execute the undead, your defenses will be greatly weakened: Resistances will suffer and Resisting Spells will suffer. A successful parry instantly ends the weakness. If you succeed, however, you will be infused with strength and healing. Your swing speed will also be boosted for a short duration. With practice, you will learn how to master your Bushido abilities.<br><br>Return to me once you feel that you have become an Apprentice Samurai.
            RefusalMessage = 1078011; // Good journey to you. Return to me if you wish to live the life of a Samurai.
            InProgressMessage = 1078012; // You are not ready to become an Apprentice Samurai. There are still more undead to lay to rest. Return to me once you have done so.
            CompletionMessage = 1078014; // You have proven yourself young one. You will continue to improve as your skills are honed with age. You are an honorable warrior, worthy of the rank of Apprentice Samurai.  Please accept this no-dachi as a gift. It is called "The Dragon's Tail". Upon a successful strike in combat, there is a chance this mighty weapon will replenish your stamina equal to the damage of your attack. I hope "The Dragon's Tail" serves you well. You have earned it. Farewell for now.
            CompletionNotice = 1078013; // You have achieved the rank of Apprentice Samurai. Return to Hamato in New Haven to report your progress.

            Objectives.Add(new GainSkillObjective(SkillName.Bushido, 500, true, true));

            Rewards.Add(new ItemReward(1078015, typeof(TheDragonsTail))); // The Dragon's Tail
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Hamato"), new Point3D(3493, 2414, 55), Map.Trammel);
        }
    }
}