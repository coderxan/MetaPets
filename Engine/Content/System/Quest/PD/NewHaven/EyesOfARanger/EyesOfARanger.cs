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
    public class EyesOfARanger : MLQuest
    {
        public EyesOfARanger()
        {
            Activated = true;
            OneTimeOnly = true;
            Title = 1078211; // Eyes of a Ranger
            Description = 1078217; // Track animals, monsters, and people on Haven Island until you have raised your Tracking skill to 50.<br><center>------</center><br>Hello friend. I am Walker, Grandmaster Ranger. An adventurer needs to keep alive in the wilderness. Being able to track those around you is essential to surviving in dangerous places. Certain Ninja abilities are more potent when the Ninja possesses Tracking knowledge. If you want to be a Ninja, or if you simply want to get a leg up on the creatures that habit these parts, I advise you learn how to track them.<br><br>You can track any animals, monsters, or people on Haven Island. Clear your mind, focus, and note any tracks in the ground or sounds in the air that can help you find your mark. You can do it, friend. I have faith in you.<br><br>Come back to me once you have achieved the rank of Apprentice Ranger (for Tracking), and I will give you something that may help you in your travels. Take care, friend.
            RefusalMessage = 1078218; // Farewell, friend. Be careful out here. If you change your mind and want to learn Tracking, come back and talk to me.
            InProgressMessage = 1078219; // So far so good, kid. You are still alive, and you are getting the hang of Tracking. There are many more animals, monsters, and people to track. Come back to me once you have tracked them.
            CompletionMessage = 1078221; // I knew you could do it! You have become a fine Ranger. Just keep practicing, and one day you will become a Grandmaster Ranger. Just like me.<br><br>I have a little something for you that will hopefully aid you in your journeys. These leggings offer some resistances that will hopefully protect you from harm. I hope these serve you well. Farewell, friend.
            CompletionNotice = 1078220; // You have achieved the rank of Apprentice Ranger (for Tracking). Return to Walker in New Haven to claim your reward.

            Objectives.Add(new GainSkillObjective(SkillName.Tracking, 500, true, true));

            Rewards.Add(new ItemReward(1078222, typeof(WalkersLeggings))); // Walker's Leggings
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Walker"), new Point3D(3429, 2518, 19), Map.Trammel);
        }
    }
}