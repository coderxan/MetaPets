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
    public class DefyingTheArcane : MLQuest
    {
        public DefyingTheArcane()
        {
            Activated = true;
            OneTimeOnly = true;
            Title = 1077621; // Defying the Arcane
            Description = 1077623; // Head East out of town and go to Old Haven. Battle spell casting monsters there until you have raised your Resisting Spells skill to 50.<br><center>------</center><br>Hail and well met! To become a true master of the arcane art of Magery, I suggest learning the complementary skill known as Resisting Spells. While the name of this skill may suggest that it helps with resisting all spells, this is not the case. This skill helps you lessen the severity of spells that lower your stats or ones that last for a specific duration of time. It does not lessen damage from spells such as Energy Bolt or Flamestrike.<BR><BR>The Magery spells that can be resisted are Clumsy, Curse, Feeblemind, Mana Drain, Mana Vampire, Paralyze, Paralyze Field, Poison, Poison Field, and Weaken.<BR><BR>The Necromancy spells that can be resisted are Blood Oath, Corpse Skin, Mind Rot, and Pain Spike.<BR><BR>At higher ranks, the Resisting Spells skill also benefits you by adding a bonus to your minimum elemental resists. This bonus is only applied after all other resist modifications - such as from equipment - has been calculated. It's also not cumulative. It compares the number of your minimum resists to the calculated value of your modifications and uses the higher of the two values.<BR><BR>As you can see, Resisting Spells is a difficult skill to understand, and even more difficult to master. This is because in order to improve it, you will have to put yourself in harm's way - as in the path of one of the above spells.<BR><BR>Undead have plagued the town of Old Haven. We need your assistance in cleansing the town of this evil influence. Old Haven is located east of here. Battle the undead spell casters that inhabit there.<BR><BR>Comeback to me once you feel that you are worthy of the rank of Apprentice Mage and I will reward you with an arcane prize.
            RefusalMessage = 1077624; // The ability to resist powerful spells is a taxing experience. I understand your resistance in wanting to pursue it. If you wish to reconsider, feel free to return to me for Resisting Spells training. Good journey to you!
            InProgressMessage = 1077632; // You have not achieved the rank of Apprentice Mage. Come back to me once you feel that you are worthy of the rank of Apprentice Mage and I will reward you with an arcane prize.
            CompletionMessage = 1077626; // You have successfully begun your journey in becoming a true master of Magery. On behalf of the New Haven Mage Council I wish to present you with this bracelet. When worn, the Bracelet of Resilience will enhance your resistances vs. the elements, physical, and poison harm. The Bracelet of Resilience also magically enhances your ability fend off ranged and melee attacks. I hope it serves you well.
            CompletionNotice = 1077625; // You have achieved the rank of Apprentice Mage (for Resisting Spells). Return to Alefian in New Haven to receive your arcane prize.

            Objectives.Add(new GainSkillObjective(SkillName.MagicResist, 500, true, true));

            Rewards.Add(new ItemReward(1077627, typeof(BraceletOfResilience))); // Bracelet of Resilience
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Alefian"), new Point3D(3473, 2497, 72), Map.Trammel);
        }
    }
}