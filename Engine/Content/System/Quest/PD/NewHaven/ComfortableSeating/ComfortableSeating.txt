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
    public class ComfortableSeating : MLQuest
    {
        public ComfortableSeating()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1075517; // Comfortable Seating
            Description = 1075518; // Hail friend, hast thou a moment? A mishap with a saw hath left me in a sorry state, for it shall be a while before I canst return to carpentry. In the meantime, I need a comfortable chair that I may rest. Could thou craft a straw chair?  Only a tool, such as a dovetail saw, a few boards, and some skill as a carpenter is needed. Remember, this is a piece of furniture, so please pay attention to detail.
            RefusalMessage = 1072687; // I quite understand your reluctance.  If you reconsider, I'll be here.
            InProgressMessage = 1075509; // Is all going well? I look forward to the simple comforts in my very own home.
            CompletionMessage = 1074720; // This is perfect!

            Objectives.Add(new CollectObjective(1, typeof(BambooChair), "straw chair"));

            Rewards.Add(new ItemReward(1074282, typeof(LowelSatchel))); // Craftsmans's Satchel
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Lowel"), new Point3D(3440, 2645, 27), Map.Trammel);
        }
    }
}