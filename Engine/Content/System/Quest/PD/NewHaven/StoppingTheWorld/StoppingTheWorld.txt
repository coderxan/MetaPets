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
    public class StoppingTheWorld : MLQuest
    {
        public StoppingTheWorld()
        {
            Activated = true;
            OneTimeOnly = true;
            Title = 1077597; // Stopping the World
            Description = 1077598; // Head East out of town and go to Old Haven. Use spells and abilites to deplete your mana and meditate there until you have raised your Meditation skill to 50.	Well met! I can teach you how to 'Stop the World' around you and focus your inner energies on replenishing you mana. What is mana? Mana is the life force for everyone who practices arcane arts. When a practitioner of magic invokes a spell or scribes a scroll. It consumes mana. Having a abundant supply of mana is vital to excelling as a practitioner of the arcane. Those of us who study the art of Meditation are also known as stotics. The Meditation skill allows stoics to increase the rate at which they regenerate mana A Stoic needs to perform abilities or cast spells to deplete mana before he can meditate to replenish it. Meditation can occur passively or actively. Actively Meditation is more difficult to master but allows for the stoic to replenish mana at a significantly faster rate. Metal armor inerferes with the regenerative properties of Meditation. It is wise to wear leather or cloth protection when meditating. Head east out of town and go to Old Haven. Use spells and abilities to deplete your mana and actively meditate to replenish it.	Come back once you feel you are at the worthy rank of Apprentice Stoic and i will reward you with a arcane prize.
            RefusalMessage = 1077599; // Seek me out if you ever wish to study the art of Meditation. Good journey.
            InProgressMessage = 1077628; // You have not achived the rank of Apprentice Stoic. Come back to me once you feel that you are worthy of the rank Apprentice Stoic and i will reward you with a arcane prize.
            CompletionMessage = 1077626; // You have successfully begun your journey in becoming a true master of Magery. On behalf of the New Haven Mage Council I wish to present you with this bracelet. When worn, the Bracelet of Resilience will enhance your resistances vs. the elements, physical, and poison harm. The Bracelet of Resilience also magically enhances your ability fend off ranged and melee attacks. I hope it serves you well.
            CompletionNotice = 1077600; // You have achieved the rank of Apprentice Stoic (for Meditation). Return to Gustar in New Haven to receive your arcane prize.

            Objectives.Add(new GainSkillObjective(SkillName.Meditation, 500, true, true));

            Rewards.Add(new ItemReward(1077602, typeof(PhilosophersHat))); // Philosopher's Hat
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Gustar"), new Point3D(3474, 2492, 91), Map.Trammel);
        }
    }
}