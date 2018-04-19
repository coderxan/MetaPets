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
    public class TheArtOfWar : MLQuest
    {
        public TheArtOfWar()
        {
            Activated = true;
            OneTimeOnly = true;
            Title = 1077667; // The Art of War
            Description = 1077670; // Head East out of town to Old Haven. Battle monsters there until you have raised your Tactics skill to 50.<br><center>------</center><br>Knowing how to hold a weapon is only half of the battle. The other half is knowing how to use it against an opponent. It's one thing to kill a few bunnies now and then for fun, but a true warrior knows that the right moves to use against a lich will pretty much get your arse fried by a dragon.<br><br>I'll help teach you how to fight so that when you do come up against that dragon, maybe you won't have to walk out of there "OooOOooOOOooOO'ing" and looking for a healer.<br><br>There are some undead that need cleaning out in Old Haven towards the east. Why don't you head on over there and practice killing things?<br><br>When you feel like you've got the basics down, come back to me and I'll see if I can scrounge up an item to help you in your adventures later on.
            RefusalMessage = 1077671; // That's too bad. I really thought you had it in you. Well, I'm sure those undead will still be there later, so if you change your mind, feel free to stop on by and I'll help you the best I can.
            InProgressMessage = 1077672; // You're making some progress, that i can tell, but you're not quite good enough to last for very long out there by yourself. Head back to Old Haven, to the east, and kill some more undead.
            CompletionMessage = 1077674; // Hey, good job killing those undead! Hopefully someone will come along and clean up the mess. All that blood and guts tends to stink after a few days, and when the wind blows in from the east, it can raise a mighty stink!<br><br>Since you performed valiantly, please take these arms and use them well. I've seen a few too many harvests to be running around out there myself, so you might as well take it.<br><br>There is a lot left for you to learn, but I think you'll do fine. Remember to keep your elbows in and stick'em where it hurts the most!
            CompletionNotice = 1077673; // You have achieved the rank of Apprentice Warrior. Return to Alden Armstrong in New Haven to claim your reward.

            Objectives.Add(new GainSkillObjective(SkillName.Tactics, 500, true, true));

            Rewards.Add(new ItemReward(1077675, typeof(ArmsOfArmstrong))); // Arms of Armstrong
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "AldenArmstrong"), new Point3D(3535, 2538, 20), Map.Trammel);
        }
    }
}