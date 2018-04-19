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
    public class TheWayOfTheBlade : MLQuest
    {
        public TheWayOfTheBlade()
        {
            Activated = true;
            OneTimeOnly = true;
            Title = 1077658; // The way of The Blade
            Description = 1077661; // Head East out of town and go to Old Haven. While wielding your sword, battle monster there until you have raised your Swordsmanship skill to 50. *as you approach, you notice Jockles sizing you up with a skeptical look on his face* i can see you want to learn how to handle a blade. It's a lot harder than it looks, and you're going to have to put alot of time and effort if you ever want to be half as good as i am. I'll tell you what, kid, I'll help you get started, but you're going to have to do all the work if you want to learn something. East of here, outside of town, is Old Haven. It's been overrun with the nastiest of undead you've seen, which makes it a perfect place for you to turn that sloppy grin on your face into actual skill at handling a sword. Make sure you have a sturdy Swordsmanship weapon in good repair before you leave. 'tis no fun to travel all the way down there just to find out you forgot your blade! When you feel that you've cut down enough of those foul smelling things to learn how to handle a blade without hurting yourself, come back to me. If i think you've improved enough, I'll give you something suited for a real warrior.
            RefusalMessage = 1077662; // Ha! I had a feeling you were a lily-livered pansy. You might have potential, but you're scared by a few smelly undead, maybe it's better that you stay away from sharp objects. After all, you wouldn't want to hurt yourself swinging a sword. If you change your mind, I might give you another chance...maybe.
            InProgressMessage = 1077663; // *Jockles looks you up and down* Come on! You've got to work harder than that to get better. Now get out of here, go kill some more of those undead to the east in Old Haven, and don't come back till you've got real skill.
            CompletionMessage = 1077665; // Well, well, look at what we have here! You managed to do it after all. I have to say, I'm a little surprised that you came back in one piece, but since you did. I've got a little something for you. This is a fine blade that served me well in my younger days. Of course I've got much better swords at my disposal now, so I'll let you go ahead and use it under one condition. Take goodcare of it and treat it with the respect that a fine sword deserves. You're one of the quickers learners I've seen, but you still have a long way to go. Keep at it, and you'll get there someday. Happy hunting, kid.
            CompletionNotice = 1077664; // You have achieved the rank of Apprentice Swordsman. Return to Jockles in New Haven to see what kind of reward he has waiting for you. Hopefully he'll be a little nicer this time!

            Objectives.Add(new GainSkillObjective(SkillName.Swords, 500, true, true));

            Rewards.Add(new ItemReward(1077666, typeof(JocklesQuicksword))); // Jockles' Quicksword
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Jockles"), new Point3D(3535, 2544, 20), Map.Trammel);
        }
    }
}