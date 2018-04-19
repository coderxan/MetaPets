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
    public class Troglodytes : MLQuest
    {
        public Troglodytes()
        {
            Activated = true;
            Title = 1074688; // Troglodytes!
            Description = 1074689; // Oh nevermind, you don't look capable of my task afterall.  Haha! What was I thinking - you could never handle killing troglodytes.  It'd be suicide.  What?  I don't know, I don't want to be responsible ... well okay if you're really sure?
            RefusalMessage = 1074690; // Probably the wiser course of action.
            InProgressMessage = 1074691; // You still need to kill those troglodytes, remember?

            Objectives.Add(new KillObjective(12, new Type[] { typeof(Troglodyte) }, "troglodytes"));

            Rewards.Add(ItemReward.BagOfTrinkets);
        }
    }
}