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
    public class KnowThineEnemy : MLQuest
    {
        public KnowThineEnemy()
        {
            Activated = true;
            OneTimeOnly = true;
            Title = 1077685; // Know Thine Enemy
            Description = 1077688; // Head East out of town to Old Haven. Battle monsters there, or heal yourself and other players, until you have raised your Anatomy skill to 50.<br><center>------</center><br>Hail and well met. You must be here to improve your knowledge of Anatomy. Well, you've come to the right place because I can teach you what you need to know. At least all you'll need to know for now. Haha!<br><br>Knowing about how living things work inside can be a very useful skill. Not only can you learn where to strike an opponent to hurt him the most, but you can use what you learn to heal wounds better as well. Just walking around town, you can even tell if someone is strong or weak or if they happen to be particularly dexterous or not.<BR><BR>If you're interested in learning more, I'd advise you to head out to Old Haven, just to the east, and jump into the fray. You'll learn best by engaging in combat while keeping you and your fellow adventurers healed, or you can even try sizing up your opponents.<br><br>While you're gone, I'll dig up something you may find useful.
            RefusalMessage = 1077689; // It's your choice, but I wouldn't head out there without knowing what makes those things tick inside! If you change your mind, you can find me right here dissecting frogs, cats or even the occasional unlucky adventurer.
            InProgressMessage = 1077690; // I'm surprised to see you back so soon. You've still got a ways to go if you want to really understand the science of Anatomy. Head out to Old Haven and practice combat and healing yourself or other adventurers.
            CompletionMessage = 1077692; // By the Virtues, you've done it! Congratulations mate! You still have quite a ways to go if you want to perfect your knowledge of Anatomy, but I know you'll get there someday. Just keep at it.<br><br>In the meantime, here's a piece of armor that you might find useful. It's not fancy, but it'll serve you well if you choose to wear it.<br><br>Happy adventuring, and remember to keep your cranium separate from your clavicle!
            CompletionNotice = 1077691; // You have achieved the rank of Apprentice Healer (for Anatomy). Return to Andreas Vesalius in New Haven as soon as you can to claim your reward.

            Objectives.Add(new GainSkillObjective(SkillName.Anatomy, 500, true, true));

            Rewards.Add(new ItemReward(1077693, typeof(TunicOfGuarding))); // Tunic of Guarding
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "AndreasVesalius"), new Point3D(3457, 2550, 35), Map.Trammel);
        }
    }
}