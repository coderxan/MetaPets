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
    public class CrushingBonesAndTakingNames : MLQuest
    {
        public CrushingBonesAndTakingNames()
        {
            Activated = true;
            OneTimeOnly = true;
            Title = 1078070; // Crushing Bones and Taking Names
            Description = 1078065; // Head East out of town and go to Old Haven. While wielding your mace,battle monster there until you have raised your Mace Fighting skill to 50. I see you want to learn a real weapon skill and not that toothpick training Jockles hasto offer. Real warriors are called Armsmen, and they wield mace weapons. No doubt about it. Nothing is more satisfying than knocking the wind out of your enemies, smashing there armor, crushing their bones, and taking there names. Want to learn how to wield a mace? Well i have an assignment for you. Head East out of town and go to Old Haven. Undead have plagued the town, so there are plenty of bones for you to smash there. Come back to me after you have ahcived the rank of Apprentice Armsman, and i will reward you with a real weapon.
            RefusalMessage = 1078068; // I thought you wanted to be an Armsman and really make something of yourself. You have potential, kid, but if you want to play with toothpicks, run to Jockles and he will teach you how to clean your teeth with a sword. If you change your mind, come back to me, and i will show you how to wield a real weapon.
            InProgressMessage = 1078067; // Listen kid. There are a lot of undead in Old Haven, and you haven't smashed enough of them yet. So get back there and do some more cleansing.
            CompletionMessage = 1078069; // Now that's what I'm talking about! Well done! Don't you like crushing bones and taking names? As i promised, here is a war mace for you. It hits hard. It swings fast. It hits often. What more do you need? Now get out of here and crush some more enemies!
            CompletionNotice = 1078068; // You have achieved the rank of Apprentice Armsman. Return to Churchill in New Haven to claim your reward.

            Objectives.Add(new GainSkillObjective(SkillName.Macing, 500, true, true));

            Rewards.Add(new ItemReward(1078062, typeof(ChurchillsWarMace))); // Churchill's War Mace
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Churchill"), new Point3D(3531, 2531, 20), Map.Trammel);
        }
    }
}