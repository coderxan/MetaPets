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
    public class NewCloak : MLQuest
    {
        public NewCloak()
        {
            Activated = true;
            Title = 1074684; // New Cloak
            Description = 1074685; // I have created a masterpiece!  And all I need to finish it off is the soft fur of a wolf.  But not just ANY wolf -- oh no, no, that wouldn't do.  I've heard tales of a mighty beast, Grobu, who is bonded to the leader of the troglodytes.  Only Grobu's fur will do.  Will you retrieve it for me?
            RefusalMessage = 1074655; // Perhaps I thought too highly of you.
            InProgressMessage = 1074686; // I've told you all I know of the creature.  Until you return with Grobu's fur I can't finish my cloak.
            CompletionMessage = 1074687; // Ah! So soft, so supple.  What a wonderful texture.  Here you are ... my thanks.

            Objectives.Add(new CollectObjective(1, typeof(GrobusFur), "Grobu's Fur"));

            Rewards.Add(ItemReward.TailorSatchel);
        }
    }
}