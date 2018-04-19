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
    public class TheAllureOfDarkMagic : MLQuest
    {
        public TheAllureOfDarkMagic()
        {
            Activated = true;
            OneTimeOnly = true;
            Title = 1078036; // The Allure of Dark Magic
            Description = 1078039; // Head East out of town and go to Old Haven. Cast Evil Omen and Pain Spike against monsters there until you have raised your Necromancy skill to 50.<br><center>------</center><br>Welcome! I see you are allured by the dark magic of Necromancy. First, you must prove yourself worthy of such knowledge. Undead currently occupy the town of Old Haven. Practice your harmful Necromancy spells on them such as Evil Omen and Pain Spike.<br><br>Make sure you have plenty of reagents before embarking on your journey. Reagents are required to cast Necromancy spells. You can purchase extra reagents from me, or you can find reagents growing in the nearby wooded areas. You can see which reagents are required for each spell by looking in your spellbook.<br><br>Come back to me once you feel that you are worthy of the rank of Apprentice Necromancer and I will reward you with the knowledge you desire.
            RefusalMessage = 1078040; // You are weak after all. Come back to me when you are ready to practice Necromancy.
            InProgressMessage = 1078041; // You have not achieved the rank of Apprentice Necromancer. Come back to me once you feel that you are worthy of the rank of Apprentice Necromancer and I will reward you with the knowledge you desire.
            CompletionMessage = 1078043; // You have done well, my young apprentice. Behold! I now present to you the knowledge you desire. This spellbook contains all the Necromancer spells. The power is intoxicating, isn't it?
            CompletionNotice = 1078042; // You have achieved the rank of Apprentice Necromancer. Return to Mulcivikh in New Haven to receive the knowledge you desire.

            Objectives.Add(new GainSkillObjective(SkillName.Necromancy, 500, true, true));

            Rewards.Add(new InternalReward());
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Mulcivikh"), new Point3D(3548, 2456, 15), Map.Trammel);
        }

        private class InternalReward : ItemReward
        {
            public InternalReward()
                : base(1078052, typeof(NecromancerSpellbook)) // Complete Necromancer Spellbook
            {
            }

            public override Item CreateItem()
            {
                Item item = base.CreateItem();

                Spellbook book = item as Spellbook;

                if (book != null)
                    book.Content = (1ul << book.BookCount) - 1;

                return item;
            }
        }
    }
}