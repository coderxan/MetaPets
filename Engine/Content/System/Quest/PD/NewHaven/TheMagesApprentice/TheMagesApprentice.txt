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
    public class TheMagesApprentice : MLQuest
    {
        public TheMagesApprentice()
        {
            Activated = true;
            OneTimeOnly = true;
            Title = 1077576; // The Mage's Apprentice
            Description = 1077577; // Head East out of town and go to Old Haven. Cast fireballs and lightning bolts against monsters there until you have raised your Magery skill to 50. Greetings. You seek to unlock the secrets of the arcane art of Magery. The New Haven Mage Council has an assignment for you. Undead have plagued the town of Old Haven. We need your assistance in cleansing the town of this evil influence. Old Haven is located east of here. I suggest using your offensive Magery spells such as Fireball and Lightning Bolt against the Undead that inhabit there. Make sure you have plenty of reagents before embarking on your journey. Reagents are required to cast Magery spells. You can purchase extra reagents at the nearby Reagent shop, or you can find reagents growing in the nearby wooded areas. You can see which reagents are required for each spell by looking in your spellbook. Come back to me once you feel that you are worthy of the rank of Apprentice Mage and I will reward you with an arcane prize.
            RefusalMessage = 1077578; // Very well, come back to me when you are ready to practice Magery. You have so much arcane potential. 'Tis a shame to see it go to waste. The New Haven Mage Council could really use your help.
            InProgressMessage = 1077579; // You have not achieved the rank of Apprentice Mage. Come back to me once you feel that you are worthy of the rank of Apprentice Mage and I will reward you with an arcane prize.
            CompletionMessage = 1077581; // Well done! On behalf of the New Haven Mage Council I wish to present you with this staff. Normally a mage must unequip weapons before spell casting. While wielding your new Ember Staff, however, you will be able to invoke your Magery spells. Even if you do not currently possess skill in Mace Fighting, the Ember Staff will allow you to fight as if you do. However, your Magery skill will be temporarily reduced while doing so. Finally, the Ember Staff occasionally smites a foe with a Fireball while wielding it in melee combat. I hope the Ember Staff serves you well.
            CompletionNotice = 1077580; // You have achieved the rank of Apprentice Mage. Return to Kaelynna in New Haven to receive your arcane prize.

            Objectives.Add(new GainSkillObjective(SkillName.Magery, 500, true, true));

            Rewards.Add(new ItemReward(1077582, typeof(EmberStaff))); // Ember Staff
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Kaelynna"), new Point3D(3486, 2491, 52), Map.Trammel);
        }
    }
}