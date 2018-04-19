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
    public class CleansingOldHaven : MLQuest
    {
        public CleansingOldHaven()
        {
            Activated = true;
            OneTimeOnly = true;
            Title = 1077719; // Cleansing Old Haven
            Description = 1077722; // Head East out of town to Old Haven. Consecrate your weapon, cast Divine Fury, and battle monsters there until you have raised your Chivalry skill to 50.<br><center>------</center><br>Hail, friend. The life of a Paladin is a life of much sacrifice, humility, bravery, and righteousness. If you wish to pursue such a life, I have an assignment for you. Adventure east to Old Haven, consecrate your weapon, and lay to rest the undead that inhabit there.<br><br>Each ability a Paladin wishes to invoke will require a certain amount of "tithing points" to use. A Paladin can earn these tithing points by donating gold at a shrine or holy place. You may tithe at this shrine.<br><br>Return to me once you feel that you are worthy of the rank of Apprentice Paladin.
            RefusalMessage = 1077723; // Farewell to you my friend. Return to me if you wish to live the life of a Paladin.
            InProgressMessage = 1077724; // There are still more undead to lay to rest. You still have more to learn. Return to me once you have done so.
            CompletionMessage = 1077726; // Well done, friend. While I know you understand Chivalry is its own reward, I would like to reward you with something that will protect you in battle. It was passed down to me when I was a lad. Now, I am passing it on you. It is called the Bulwark Leggings. Thank you for your service.
            CompletionNotice = 1077725; // You have achieved the rank of Apprentice Paladin. Return to Aelorn in New Haven to report your progress.

            Objectives.Add(new GainSkillObjective(SkillName.Chivalry, 500, true, true));

            Rewards.Add(new ItemReward(1077727, typeof(BulwarkLeggings))); // Bulwark Leggings
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Aelorn"), new Point3D(3527, 2516, 45), Map.Trammel);
        }
    }
}