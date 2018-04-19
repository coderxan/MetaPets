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
    public class TheInnerWarrior : MLQuest
    {
        public TheInnerWarrior()
        {
            Activated = true;
            OneTimeOnly = true;
            Title = 1077696; // The Inner Warrior
            Description = 1077699; // Head East out of town to Old Haven. Expend stamina and mana until you have raised your Focus skill to 50.<br><center>------</center><br>Well, hello there. Don't you look like quite the adventurer!<br><br>You want to learn more about Focus, do you? I can teach you something about that, but first you should know that not everyone can be disciplined enough to excel at it. Focus is the ability to achieve inner balance in both body and spirit, so that you recover from physical and mental exertion faster than you otherwise would.<br><br>If you want to practice Focus, the best place to do that is east of here, in Old Haven, where you'll find an undead infestation. Exert yourself physically by engaging in combat and moving quickly. For testing your mental balance, expend mana in whatever way you find most suitable to your abilities. Casting spells and using abilities work well for consuming your mana.<br><br>Go. Train hard, and you will find that your concentration will improve naturally. When you've improved your ability to focus yourself at an Apprentice level, come back to me and I shall give you something worthy of your new ability.
            RefusalMessage = 1077700; // I'm disappointed. You have a lot of inner potential, and it would pain me greatly to see you waste that. Oh well. If you change your mind, I'll be right here.
            InProgressMessage = 1077701; // Hello again. I see you've returned, but it seems that your Focus skill hasn't improved as much as it could have. Just head east, to Old Haven, and exert yourself physically and mentally as much as possible. To do this physically, engage in combat and move as quickly as you can. For exerting yourself mentally, expend mana in whatever way you find most suitable to your abilities. Casting spells and using abilities work well for consuming your mana.<br><br>Return to me when you have gained enough Focus skill to be considered an Apprentice Stoic.
            CompletionMessage = 1077703; // Look who it is! I knew you could do it if you just had the discipline to apply yourself. It feels good to recover from battle so quickly, doesn't it? Just wait until you become a Grandmaster, it's amazing!<br><br>Please take this gift, as you've more than earned it with your hard work. It will help you recover even faster during battle, and provides a bit of protection as well.<br><br>You have so much more potential, so don't stop trying to improve your Focus now! Safe travels!
            CompletionNotice = 1077702; // You have achieved the rank of Apprentice Stoic (for Focus). Return to Sarsmea Smythe in New Haven to see what kind of reward she has waiting for you.

            Objectives.Add(new GainSkillObjective(SkillName.Focus, 500, true, true));

            Rewards.Add(new ItemReward(1077695, typeof(ClaspOfConcentration))); // Clasp of Concentration
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "SarsmeaSmythe"), new Point3D(3492, 2577, 15), Map.Trammel);
        }
    }
}