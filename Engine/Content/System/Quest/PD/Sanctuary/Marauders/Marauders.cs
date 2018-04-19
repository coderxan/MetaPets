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
    public class Marauders : MLQuest
    {
        public override Type NextQuest { get { return typeof(TheBrainsOfTheOperation); } }

        public Marauders()
        {
            Activated = true;
            Title = 1072374; // Marauders
            Description = 1072686; // What a miserable place we live in.  Look around you at the changes we've wrought. The trees are sprouting leaves once more and the grass is reclaiming the blood-soaked soil.  Who would have imagined we'd find ourselves here?  Our "neighbors" are anything but friendly and those ogres are the worst of the lot. Maybe you'd be interested in helping our community by disposing of some of our least amiable neighbors?
            RefusalMessage = 1072687; // I quite understand your reluctance.  If you reconsider, I'll be here.
            InProgressMessage = 1072688; // You can't miss those ogres, they're huge and just outside the gates here.

            Objectives.Add(new KillObjective(10, new Type[] { typeof(Ogre) }, "ogres", new QuestArea(1074807, "Sanctuary"))); // Sanctuary

            Rewards.Add(ItemReward.BagOfTreasure);
        }
    }

    public class TheBrainsOfTheOperation : MLQuest
    {
        public override Type NextQuest { get { return typeof(TheBrawn); } }
        public override bool IsChainTriggered { get { return true; } }

        public TheBrainsOfTheOperation()
        {
            Activated = true;
            Title = 1072692; // The Brains of the Operation
            Description = 1072707; // *sigh*  We have so much to do to clean this area up.  Even the fine work you did on those ogres didn't have much of an impact on the community.  It's the ogre lords that direct the actions of the other ogres, let's strike at the leaders and perhaps that will thwart the miserable curs.
            RefusalMessage = 1072708; // Reluctance doesn't become a hero like you.  But, as you wish.
            InProgressMessage = 1072709; // Ogre Lords are pretty easy to recognize.  They're the ones ordering the other ogres about in a lordly manner.  Striking down their leadership will throw the ogres into confusion and dismay!

            Objectives.Add(new KillObjective(10, new Type[] { typeof(OgreLord) }, "ogre lords", new QuestArea(1074807, "Sanctuary"))); // Sanctuary

            Rewards.Add(ItemReward.LargeBagOfTreasure);
        }
    }

    public class TheBrawn : MLQuest
    {
        public override Type NextQuest { get { return typeof(TheBiggerTheyAre); } }
        public override bool IsChainTriggered { get { return true; } }

        public TheBrawn()
        {
            Activated = true;
            Title = 1072693; // The Brawn
            Description = 1072710; // Inconceiveable!  We've learned that the ogre leadership has recruited some heavy-duty guards to their cause.  I've never personally fought a cyclopian warrior, but I'm sure you could easily best a few and report back how much trouble they'll cause to our growing community?
            RefusalMessage = 1072711; // Oh, I see.  *sigh*  Perhaps I overestimated your abilities.
            InProgressMessage = 1072712; // Make sure you fully assess all of the cyclopian tactical abilities!

            Objectives.Add(new KillObjective(6, new Type[] { typeof(Cyclops) }, "cyclops", new QuestArea(1074807, "Sanctuary"))); // Sanctuary

            Rewards.Add(ItemReward.LargeBagOfTreasure);
        }
    }

    public class TheBiggerTheyAre : MLQuest
    {
        public override bool IsChainTriggered { get { return true; } }

        public TheBiggerTheyAre()
        {
            Activated = true;
            Title = 1072694; // The Bigger They Are ...
            Description = 1072713; // The ogre insurgency has taken a turn for the worse! I've just been advised that the titans have concluded their discussions with the ogres and they've allied. We have virtually no information about titans.  Engage them and appraise their mettle.
            RefusalMessage = 1072714; // Certainly.  You've done enough to merit a breather.  When you're ready for more, report back to me.
            InProgressMessage = 1072715; // Those titans don't skulk very well.  You should be able to track them easily ... their footsteps are easily the largest around.

            Objectives.Add(new KillObjective(3, new Type[] { typeof(Titan) }, "titans", new QuestArea(1074807, "Sanctuary"))); // Sanctuary

            Rewards.Add(ItemReward.LargeBagOfTreasure);
        }
    }
}