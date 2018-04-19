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
    public class BruisesBandagesAndBlood : MLQuest
    {
        public BruisesBandagesAndBlood()
        {
            Activated = true;
            OneTimeOnly = true;
            Title = 1077676; // Bruises, Bandages and Blood
            Description = 1077679; // Head East out of town and go to Old Haven. Heal yourself and other players until you have raised your Healing skill to 50.<br><center>------</center><br>Ah, welcome to my humble practice. I am Avicenna, New Haven's resident Healer. A lot of adventurers head out into the wild from here, so I keep rather busy when they come back bruised, bleeding, or worse.<br><br>I can teach you how to bandage a wound, sure, but it's not a job for the queasy! For some folks, the mere sight of blood is too much for them, but it's something you'll get used to over time. It is one thing to cut open a living thing, but it's quite another to sew it back up and save it from sure death. 'Tis noble work, healing.<br><br>Best way for you to practice fixing up wounds is to head east out to Old Haven and either practice binding up your own wounds, or practice on someone else. Surely they'll be grateful for the assistance.<br><br>Make sure to take enough bandages with you! You don't want to run out in the middle of a tough fight.
            RefusalMessage = 1077680; // No? Are you sure? Well, when you feel that you're ready to practice your healing, come back to me. I'll be right here, fixing up adventurers and curing the occasional cold!
            InProgressMessage = 1077681; // Hail! 'Tis good to see you again. Unfortunately, you're not quite ready to call yourself an Apprentice Healer quite yet. Head back out to Old Haven, due east from here, and bandage up some wounds. Yours or someone else's, it doesn't much matter.
            CompletionMessage = 1077683; // Hello there, friend. I see you've returned in one piece, and you're an Apprentice Healer to boot! You should be proud of your accomplishment, as not everyone has "the touch" when it comes to healing.<br><br>I can't stand to see such good work go unrewarded, so I have something I'd like you to have. It's not much, but it'll help you heal just a little faster, and maybe keep you alive.<br><br>Good luck out there, friend, and don't forget to help your fellow adventurer whenever possible!
            CompletionNotice = 1077682; // You have achieved the rank of Apprentice Healer. Return to Avicenna in New Haven as soon as you can to claim your reward.

            Objectives.Add(new GainSkillObjective(SkillName.Healing, 500, true, true));

            Rewards.Add(new ItemReward(1077684, typeof(HealersTouch))); // Healer's Touch
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Avicenna"), new Point3D(3464, 2558, 35), Map.Trammel);
        }
    }
}