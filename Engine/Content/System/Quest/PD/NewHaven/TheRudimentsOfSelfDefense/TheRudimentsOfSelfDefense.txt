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
    public class TheRudimentsOfSelfDefense : MLQuest
    {
        public TheRudimentsOfSelfDefense()
        {
            Activated = true;
            OneTimeOnly = true;
            Title = 1077609; // The Rudiments of Self Defense
            Description = 1077610; // Head East out of town and go to Old Haven. Battle monster there until you have raised your Wrestling skill to 50.Listen up! If you want to learn the rudiments of self-defense, you need toughening up, and there's no better way to toughen up than engaging in combat. Head East out of town to Old Haven and battle the undead there in hand to hand combat. Afraid of dying, you say? Well, you should be! Being an adventurer isn't a bed of posies, or roses, or however that saying goes. If you take a dirt nap, go to one of the nearby wandering healers and they'll get you back on your feet.Come back to me once you feel that you are worthy of the rank Apprentice Wrestler and i will reward you wit a prize.
            RefusalMessage = 1077611; // Ok, featherweight. come back to me if you want to learn the rudiments of self-defense.
            InProgressMessage = 1077630; // You have not achived the rank of Apprentice Wrestler. Come back to me once you feel that you are worthy of the rank Apprentice Wrestler and i will reward you with something useful.
            CompletionMessage = 1077613; // It's about time! Looks like you managed to make it through your self-defense training. As i promised, here's a little something for you. When worn, these Gloves of Safeguarding will increase your awareness and resistances to most elements except poison. Oh yeah, they also increase your natural health regeneration aswell. Pretty handy gloves, indeed. Oh, if you are wondering if your meditation will be hinered while wearing these gloves, it won't be. Mages can wear cloth and leather items without needing to worry about that. Now get out of here and make something of yourself.
            CompletionNotice = 1077612; // You have achieved the rank of Apprentice Wrestler. Return to Dimethro in New Haven to receive your prize.

            Objectives.Add(new GainSkillObjective(SkillName.Wrestling, 500, true, true));

            Rewards.Add(new ItemReward(1077614, typeof(GlovesOfSafeguarding))); // Gloves Of Safeguarding
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Dimethro"), new Point3D(3528, 2520, 25), Map.Trammel);
        }
    }
}