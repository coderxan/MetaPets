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
    public class PixieDustToDust : MLQuest
    {
        public PixieDustToDust()
        {
            Activated = true;
            Title = 1073661; // Pixie dust to dust
            Description = 1073700; // Is there anything more foul than a pixie? They have cruel eyes and a mind for mischief, I say. I don't care if some think they're cute -- I say kill them and let the Avatar sort them out. In fact, if you were to kill a few pixies, I'd make sure you had a few coins to rub together, if you get my meaning.
            RefusalMessage = 1073733; // Perhaps you'll change your mind and return at some point.
            InProgressMessage = 1073741; // There's too much cuteness in the world -- kill those pixies!

            Objectives.Add(new KillObjective(10, new Type[] { typeof(Pixie) }, "pixies"));

            Rewards.Add(ItemReward.LargeBagOfTreasure);
        }
    }
}