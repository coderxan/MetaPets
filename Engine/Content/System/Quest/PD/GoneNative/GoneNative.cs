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
    public class GoneNative : MLQuest
    {
        public GoneNative()
        {
            Activated = true;
            Title = 1074855; // Gone Native
            Description = 1074856; // Pathetic really.  I must say, a senior instructor going native -- forgetting about his students and peers and engaging in such disgraceful behavior!  I'm speaking, of course, of Theophilus.  Master Theophilus to you. He may have gone native but he still holds a Mastery Degree from Bedlam College!  But, well, that's neither here nor there.  I need you to take care of my colleague.  Convince him of the error of his ways.  He may resist.  In fact, assume he will and kill him.  We'll get him resurrected and be ready to cure his folly.  What do you say?
            RefusalMessage = 1074857; // I understand.  A Master of Bedlam, even one entirely off his rocker, is too much for you to handle.
            InProgressMessage = 1074858; // You had better get going.  Master Theophilus isn't likely to kill himself just to save me this embarrassment.
            CompletionMessage = 1074859; // You look a bit worse for wear!  He put up a good fight did he?  Hah!  That's the spirit … a Master of Bedlam is a match for most.

            Objectives.Add(new KillObjective(1, new Type[] { typeof(MasterTheophilus) }, "Master Theophilus"));

            Rewards.Add(ItemReward.LargeBagOfTreasure);
        }
    }
}