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
    public class ItsHammerTime : MLQuest
    {
        public ItsHammerTime()
        {
            Activated = true;
            OneTimeOnly = true;
            Title = 1077732; // It’s Hammer Time!
            Description = 1077735; // Create new daggers and maces using the forge and anvil in George's shop. Try making daggers up to 45 skill, the switch to making maces until 50 skill.<br><center>-----</center><br>Hail, and welcome to my humble shop. I'm George Hephaestus, New Haven's blacksmith. I assume that you're here to ask me to train you to be an Apprentice Blacksmith. I certainly can do that, but you're going to have to supply your own ingots.<br><br>You can always buy them at the market, but I highly suggest that you mine your own. That way, any items you sell will be pure profit!<br><br>So, once you have a supply of ingots, use my forge and anvil here to create items. You'll also need a supply of the proper tools; you can use a smith's hammer, a sledgehammer or tongs. You can either make them yourself if you have the tinkering skill, or buy them from a tinker at the market.<br><br>Since I'll be around to give you advice, you'll learn faster here than anywhere else. Start off making daggers until you reach 45 blacksmithing skill, then switch to maces until you've achieved 50. Once you've done that, come talk to me and I'll give you something for your hard work.
            RefusalMessage = 1077736; // You're not interested in learning to be a smith, eh? I thought for sure that's why you were here. Oh well, if you change your mind, you can always come back and talk to me.
            InProgressMessage = 1077737; // You’re doing well, but you’re not quite there yet. Remember that the quickest way to learn is to make daggers up until 45 skill, and then switch to maces. Also, don’t forget that using my forge and anvil will help you learn faster.
            CompletionMessage = 1077739; // I've been watching you get better and better as you've been smithing, and I have to say, you're a natural! It's a long road to being a Grandmaster Blacksmith, but I have no doubt that if you put your mind to it you'll get there someday. Let me give you one final piece of advice. If you're smithing just to practice and improve your skill, make items that are moderately difficult (60-80% success chance), and try to stick to ones that use less ingots.<br><br>Now that you're an Apprentice Blacksmith, I have something for you. While you were busy practicing, I was crafting this hammer for you. It's finely balanced, and has a bit of magic imbued within that will help you craft better items. However, that magic needs to restore itself over time, so you can only use it so many times per day. I hope you find it useful!
            CompletionNotice = 1077738; // You have achieved the rank of Apprentice Blacksmith. Return to George Hephaestus in New Haven to see what kind of reward he has waiting for you.

            Objectives.Add(new GainSkillObjective(SkillName.Blacksmith, 500, true, true));

            Rewards.Add(new ItemReward(1077740, typeof(HammerOfHephaestus))); // Hammer of Hephaestus
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "GeorgeHephaestus"), new Point3D(3471, 2542, 36), Map.Trammel);
        }
    }
}