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
    public class MusicToMyEars : MLQuest
    {
        public MusicToMyEars()
        {
            Activated = true;
            HasRestartDelay = true;
            Title = 1074023; // Music to my Ears
            Description = 1074117; // You think you know something of music? Laughable! Take your lap harp. Crude, indelicate instruments that make a noise not unlike the wailing of a choleric child or a dying cat. I will show you - bring lap harps, and I will demonstrate.
            RefusalMessage = 1074063; // Fine then, I'm shall find another to run my errands then.
            InProgressMessage = 1074064; // Hurry up! I don't have all day to wait for you to bring what I desire!
            CompletionMessage = 1074065; // These human made goods are laughable! It offends so -- I must show you what elven skill is capable of!

            Objectives.Add(new CollectObjective(10, typeof(LapHarp), 1023762)); // lap harp

            Rewards.Add(ItemReward.CarpentrySatchel);
        }
    }
}