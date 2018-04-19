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
    public class TheRightToolForTheJob : MLQuest
    {
        public TheRightToolForTheJob()
        {
            Activated = true;
            OneTimeOnly = true;
            Title = 1077741; // The Right Tool for the Job
            Description = 1077744; // Create new scissors and hammers while inside Amelia's workshop. Try making scissors up to 45 skill, the switch to making hammers until 50 skill.<br><center>-----</center><br>Hello! I guess you're here to learn something about Tinkering, eh? You've come to the right place, as Tinkering is what I've dedicated my life to. <br><br>You'll need two things to get started: a supply of ingots and the right tools for the job. You can either buy ingots from the market, or go mine them yourself. As for tools, you can try making your own set of Tinker's Tools, or if you'd prefer to buy them, I have some for sale.<br><br>Working here in my shop will let me give you pointers as you go, so you'll be able to learn faster than anywhere else. Start off making scissors until you reach 45 tinkering skill, then switch to hammers until you've achieved 50. Once you've done that, come talk to me and I'll give you something for your hard work.
            RefusalMessage = 1077745; // I’m disappointed that you aren’t interested in learning more about Tinkering. It’s really such a useful skill!<br><br>*Amelia smiles*<br><br>At least you know where to find me if you change your mind, since I rarely spend time outside of this shop.
            InProgressMessage = 1077746; // Nice going! You're not quite at Apprentice Tinkering yet, though, so you better get back to work. Remember that the quickest way to learn is to make scissors up until 45 skill, and then switch to hammers. Also, don't forget that working here in my shop will let me give you tips so you can learn faster.
            CompletionMessage = 1077748; // You've done it! Look at our brand new Apprentice Tinker! You've still got quite a lot to learn if you want to be a Grandmaster Tinker, but I believe you can do it! Just keep in mind that if you're tinkering just to practice and improve your skill, make items that are moderately difficult (60-80% success chance), and try to stick to ones that use less ingots.  <br><br>Come here, my brand new Apprentice Tinker, I want to give you something special. I created this just for you, so I hope you like it. It's a set of Tinker's Tools that contains a bit of magic. These tools have more charges than any Tinker's Tools a Tinker can make. You can even use them to make a normal set of tools, so that way you won't ever find yourself stuck somewhere with no tools!
            CompletionNotice = 1077747; // You have achieved the rank of Apprentice Tinker. Talk to Amelia Youngstone in New Haven to see what kind of reward she has waiting for you.

            Objectives.Add(new GainSkillObjective(SkillName.Tinkering, 500, true, true));

            Rewards.Add(new ItemReward(1077749, typeof(AmeliasToolbox))); // Amelia’s Toolbox
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "AmeliaYoungstone"), new Point3D(3459, 2529, 53), Map.Trammel);
        }
    }
}