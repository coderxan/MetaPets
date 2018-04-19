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
    public class ScholarlyTask : MLQuest
    {
        public ScholarlyTask()
        {
            Activated = true;
            OneTimeOnly = true;
            Title = 1077603; // A Scholarly Task
            Description = 1077604; // Head East out of town and go to Old Haven. Use Evaluating Intelligence on all creatures you see there. You can also cast Magery spells as well to raise Evaluating Intelligence. Do these activities until you have raised your Evaluating Intelligence skill to 50.<br><center>------</center><br>Hello. Truly knowing your opponent is essential for landing your offensive spells with precision. I can teach you how to enhance the effectiveness of your offensive spells, but first you must learn how to size up your opponents intellectually. I have a scholarly task for you. Head East out of town and go to Old Haven. Use Evaluating Intelligence on all creatures you see there. You can also cast Magery spells as well to raise Evaluating Intelligence.<BR><BR>Come back to me once you feel that you are worthy of the rank of Apprentice Scholar and I will reward you with an arcane prize.
            RefusalMessage = 1077605; // Return to me if you reconsider and wish to become an Apprentice Scholar.
            InProgressMessage = 1077629; // You have not achieved the rank of Apprentice Scholar. Come back to me once you feel that you are worthy of the rank of Apprentice Scholar and I will reward you with an arcane prize.
            CompletionMessage = 1077607; // You have completed the task. Well done. On behalf of the New Haven Mage Council I wish to present you with this ring. When worn, the Ring of the Savant enhances your intellectual aptitude and increases your mana pool. Your spell casting abilities will take less time to invoke and recovering from such spell casting will be hastened. I hope the Ring of the Savant serves you well.
            CompletionNotice = 1077606; // You have achieved the rank of Apprentice Scholar. Return to Mithneral in New Haven to receive your arcane prize.

            Objectives.Add(new GainSkillObjective(SkillName.EvalInt, 500, true, true));

            Rewards.Add(new ItemReward(1077608, typeof(RingOfTheSavant))); // Ring of the Savant
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Mithneral"), new Point3D(3485, 2491, 71), Map.Trammel);
        }
    }
}