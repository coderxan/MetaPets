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
    public class MougGuurMustDie : MLQuest
    {
        public override Type NextQuest { get { return typeof(LeaderOfThePack); } }

        public MougGuurMustDie()
        {
            Activated = true;
            Title = 1072368; // Moug-Guur Must Die
            Description = 1072561; // You there!  Yes, you.  Kill Moug-Guur, the leader of the orcs in this depressing place, and I'll make it worth your while.
            RefusalMessage = 1072571; // Fine. It's no skin off my teeth.
            InProgressMessage = 1072572; // Small words.  Kill Moug-Guur.  Go.  Now!
            CompletionMessage = 1072573; // You're better than I thought you'd be.  Not particularly bad, but not entirely inept.

            Objectives.Add(new KillObjective(1, new Type[] { typeof(MougGuur) }, "Moug-Guur", new QuestArea(1074807, "Sanctuary"))); // Sanctuary

            Rewards.Add(ItemReward.BagOfTreasure);
        }
    }

    public class LeaderOfThePack : MLQuest
    {
        public override Type NextQuest { get { return typeof(SayonaraSzavetra); } }
        public override bool IsChainTriggered { get { return true; } }

        public LeaderOfThePack()
        {
            Activated = true;
            Title = 1072560; // Leader of the Pack
            Description = 1072574; // Well now that Moug-Guur is no more -- and I can't say I'm weeping for his demise -- it's time for the ratmen to experience a similar loss of leadership.  Slay Chiikkaha.  In return, I'll satisfy your greed temporarily.
            RefusalMessage = 1072575; // Alright, if you'd rather not, then run along and do whatever worthless things you do when I'm not giving you direction.
            InProgressMessage = 1072576; // How difficult is this?  The rats live in the tunnels.  Go into the tunnels and find the biggest, meanest rat and execute him.  Loitering around here won't get the task done.
            CompletionMessage = 1072577; // It's about time!  Could you have taken longer?

            Objectives.Add(new KillObjective(1, new Type[] { typeof(Chiikkaha) }, "Chiikkaha", new QuestArea(1074807, "Sanctuary"))); // Sanctuary

            Rewards.Add(ItemReward.BagOfTreasure);
        }
    }

    public class SayonaraSzavetra : MLQuest
    {
        public override bool IsChainTriggered { get { return true; } }

        public SayonaraSzavetra()
        {
            Activated = true;
            Title = 1072375; // Sayonara, Szavetra
            Description = 1072578; // Hmm, maybe you aren't entirely worthless.  I suspect a demoness of Szavetra's calibre will tear you apart ...  We might as well find out.  Kill the succubus, yada yada, and you'll be richly rewarded.
            RefusalMessage = 1072579; // Hah!  I knew you couldn't handle it.
            InProgressMessage = 1072581; // Hahahaha!  I can see the fear in your eyes.  Pathetic.  Szavetra is waiting for you.
            CompletionMessage = 1072582; // Amazing!  Simply astonishing ... you survived.  Well, I supposed I should indulge your avarice with a reward.

            Objectives.Add(new KillObjective(1, new Type[] { typeof(Szavetra) }, "Szavetra", new QuestArea(1074807, "Sanctuary"))); // Sanctuary

            Rewards.Add(ItemReward.Strongbox);
        }
    }
}