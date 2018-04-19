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
    public class EnGuarde : MLQuest
    {
        public EnGuarde()
        {
            Activated = true;
            OneTimeOnly = true;
            Title = 1078186; // En Guarde!
            Description = 1078190; // Head East out of town to Old Haven. Battle monsters there until you have raised your Fencing skill to 50.<br><center>------</center><br>Well hello there, lad. Fighting with elegance and precision is far more enriching than slugging an enemy with a club or butchering an enemy with a sword. Learn the art of Fencing if you want to master combat and look good doing it!<br><br>The key to being a successful fencer is to be the complement and not the opposition to your opponent's strength. Watch for your opponent to become off balance. Then finish him off with finesse and flair.<br><br>There are some undead that need cleansing out in Old Haven towards the East. Head over there and slay them, but remember, do it with style!<br><br>Come back to me once you have achieved the rank of Apprentice Fencer, and I will reward you with a prize.
            RefusalMessage = 1078191; // I understand, lad. Being a hero isn't for everyone. Run along, then. Come back to me if you change your mind.
            InProgressMessage = 1078192; // You're doing well so far, but you're not quite ready yet. Head back to Old Haven, to the East, and kill some more undead.
            CompletionMessage = 1078194; // Excellent! You are beginning to appreciate the art of Fencing. I told you fighting with elegance and precision is more enriching than fighting like an ogre.<br><br>Since you have returned victorious, please take this war fork and use it well. The war fork is a finesse weapon, and this one is magical! I call it "Recaro's Riposte". With it, you will be able to parry and counterstrike with ease! Your enemies will bask in your greatness and glory! Good luck to you, lad, and keep practicing!
            CompletionNotice = 1078193; // You have achieved the rank of Apprentice Fencer. Return to Recaro in New Haven to claim your reward.

            Objectives.Add(new GainSkillObjective(SkillName.Fencing, 500, true, true));

            Rewards.Add(new ItemReward(1078195, typeof(RecarosRiposte))); // Recaro's Riposte
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Recaro"), new Point3D(3536, 2534, 20), Map.Trammel);
        }
    }
}