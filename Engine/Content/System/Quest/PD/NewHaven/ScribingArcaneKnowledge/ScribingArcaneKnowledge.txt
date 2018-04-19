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
    public class ScribingArcaneKnowledge : MLQuest
    {
        public ScribingArcaneKnowledge()
        {
            Activated = true;
            OneTimeOnly = true;
            Title = 1077615; // Scribing Arcane Knowledge
            Description = 1077616; // While Here ar the New Haven Magery Library, use scribe's pen and scribe 3rd and 4th circle Magery scrolls that you have in your spellbook. Remeber, you will need blank scrolls aswell. Do this until you have raised your Inscription skill to 50. Greetings and welcome to the New Haven Magery Library! You wish to learn how to scribe spell scrolls? You have come to the right place! Inscribeed in a steady hand and imbued with te power of reagents, a scroll can mean the difference between life and death in a perilous situation. Those knowledgeable in Inscription man transcribe spells to create useful and valuale magical scrolls. Before you can inscribe a spell, you must first be able to cast the spell without the aid of a scroll. This means that you need the appropriate level of proficiency as a mage, the required mana, and the required reagents. Second, you will need a blank scroll to write on and a scribe's pen. Then, you will need to decide which particular spell you wish to scribe. It may sound easy, but there is a bit more to it. As with the development of all skills, you need to practice Inscription of lower level spells before you can move onto the more difficult ones. The most important aspect of Inscription is mana. Inscribing a scroll with a magic spell drains your mana. When inscribing 3rd or lower spells this is will not be much of a problem for these spells consume a small amount of mana. However, when you are inscribing higher circle spells, you may see your mana drain rapidly. When this happens, pause or meditate before continuing.I suggest you begin scribing any 3rd and 4th circle spells that you know. If you don't possess ant, you can alwayers barter with one of the local mage merchants or a fellow adventurer that is a seasoned Scribe. Come back to me once you feel that you are the worthy rankof Apprentice Scribe and i will reward you with an arcane prize.
            RefusalMessage = 1077617; // I understand. When you are ready, feel free to return to me for Inscription training. Thanks for stopping by!
            InProgressMessage = 1077631; // You have not achived the rank of Apprentice Scribe. Come back to me once you feel that you are worthy of the rank Apprentice Scribe and i will reward you with a arcane prize.
            CompletionMessage = 1077619; // Scribing is a very fulfilling pursuit. I am please to see you embark on this journey. You sling a pen well! On behalf of the New Haven Mage Council I wish to present you with this spellbook. When equipped, the Hallowed Spellbook greatly enhanced the potency of your offensive soells when used against Undead. Be mindful, though. While this book is equiped you invoke powerful spells and abilities vs Humanoids, such as other humans, orcs, ettins, and trolls. Your offensive spells will diminish in effectiveness. I suggest unequipping the Hallowed Spellbook when battling Humanoids. I hope this spellbook serves you well.
            CompletionNotice = 1077618; // You have achieved the rank of Apprentice Scribe. Return to Jillian in New Haven to receive your arcane prize.

            Objectives.Add(new GainSkillObjective(SkillName.Inscribe, 500, true, true));

            Rewards.Add(new ItemReward(1077620, typeof(HallowedSpellbook))); // Hallowed Spellbook
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Jillian"), new Point3D(3465, 2490, 71), Map.Trammel);
        }
    }
}