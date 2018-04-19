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
    public class ChannelingTheSupernatural : MLQuest
    {
        public ChannelingTheSupernatural()
        {
            Activated = true;
            OneTimeOnly = true;
            Title = 1078044; // Channeling the Supernatural
            Description = 1078047; // Head East out of town and go to Old Haven. Use Spirit Speak and channel energy from either yourself or nearby corpses there. You can also cast Necromancy spells as well to raise Spirit Speak. Do these activities until you have raised your Spirit Speak skill to 50.<br><center>------</center><br>How do you do? Channeling the supernatural through Spirit Speak allows you heal your wounds. Such channeling expends your mana, so be mindful of this. Spirit Speak enhances the potency of your Necromancy spells. The channeling powers of a Medium are quite useful when practicing the dark magic of Necromancy.<br><br>It is best to practice Spirit Speak where there are a lot of corpses. Head East out of town and go to Old Haven. Undead currently reside there. Use Spirit Speak and channel energy from either yourself or nearby corpses. You can also cast Necromancy spells as well to raise Spirit Speak.<br><br>Come back to me once you feel that you are worthy of the rank of Apprentice Medium and I will reward you with something useful.
            RefusalMessage = 1078048; // Channeling the supernatural isn't for everyone. It is a dark art. See me if you ever wish to pursue the life of a Medium.
            InProgressMessage = 1078049; // Back so soon? You have not achieved the rank of Apprentice Medium. Come back to me once you feel that you are worthy of the rank of Apprentice Medium and I will reward you with something useful.
            CompletionMessage = 1078051; // Well done! Channeling the supernatural is taxing, indeed. As promised, I will reward you with this bag of Necromancer reagents. You will need these if you wish to also pursue the dark magic of Necromancy. Good journey to you.
            CompletionNotice = 1078050; // You have achieved the rank of Apprentice Medium. Return to Morganna in New Haven to receive your reward.

            Objectives.Add(new GainSkillObjective(SkillName.SpiritSpeak, 500, true, true));

            Rewards.Add(new ItemReward(1078053, typeof(BagOfNecromancerReagents))); // Bag of Necromancer Reagents
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Morganna"), new Point3D(3547, 2463, 15), Map.Trammel);
        }
    }
}