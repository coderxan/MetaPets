using System;

using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests.Zento
{
    public class TerribleHatchlingsQuest : QuestSystem
    {
        private static Type[] m_TypeReferenceTable = new Type[]
			{
				typeof( AcceptConversation ),
				typeof( DirectionConversation ),
				typeof( TakeCareConversation ),
				typeof( EndConversation ),
				typeof( FirstKillObjective ),
				typeof( SecondKillObjective ),
				typeof( ThirdKillObjective ),
				typeof( ReturnObjective )
			};

        public override Type[] TypeReferenceTable { get { return m_TypeReferenceTable; } }

        public override object Name
        {
            get
            {
                // Terrible Hatchlings
                return 1063314;
            }
        }

        public override object OfferMessage
        {
            get
            {
                /* The Deathwatch Beetle Hatchlings have trampled through my fields
                 * again, what a nuisance! Please help me get rid of the terrible
                 * hatchlings. If you kill 10 of them, you will be rewarded.
                 * The Deathwatch Beetle Hatchlings live in The Waste -
                 * the desert close to this city.<BR><BR>
                 * 
                 * Will you accept this challenge?
                 */
                return 1063315;
            }
        }

        public override TimeSpan RestartDelay { get { return TimeSpan.MaxValue; } }
        public override bool IsTutorial { get { return true; } }

        public override int Picture { get { return 0x15CF; } }

        public TerribleHatchlingsQuest(PlayerMobile from)
            : base(from)
        {
        }

        // Serialization
        public TerribleHatchlingsQuest()
        {
        }

        public override void Accept()
        {
            base.Accept();

            AddConversation(new AcceptConversation());
        }
    }

    #region Questing Objectives

    public class FirstKillObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                // Kill 10 Deathwatch Beetle Hatchlings and return to Ansella Gryen.
                return 1063316;
            }
        }

        public override void RenderProgress(BaseQuestGump gump)
        {
            if (!Completed)
            {
                // Deathwatch Beetle Hatchlings killed:
                gump.AddHtmlLocalized(70, 260, 270, 100, 1063318, 0x12DC6BF, false, false);

                gump.AddLabel(70, 280, 0x64, "0");
                gump.AddLabel(100, 280, 0x64, "/");
                gump.AddLabel(130, 280, 0x64, "10");
            }
            else
            {
                base.RenderProgress(gump);
            }
        }

        public FirstKillObjective()
        {
        }

        public override void OnKill(BaseCreature creature, Container corpse)
        {
            if (creature is DeathwatchBeetleHatchling)
                Complete();
        }

        public override void OnComplete()
        {
            System.AddObjective(new SecondKillObjective());
        }
    }

    public class SecondKillObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* Great job! One less terrible hatchling in the Waste!<BR><BR>
                 * 
                 * Once you've killed 10 of the Deathwatch Beetle Hatchlings,
                 * return to Ansella for your reward!
                 */
                return 1063320;
            }
        }

        public override void RenderProgress(BaseQuestGump gump)
        {
            if (!Completed)
            {
                // Deathwatch Beetle Hatchlings killed:
                gump.AddHtmlLocalized(70, 260, 270, 100, 1063318, 0x12DC6BF, false, false);

                gump.AddLabel(70, 280, 0x64, "1");
                gump.AddLabel(100, 280, 0x64, "/");
                gump.AddLabel(130, 280, 0x64, "10");
            }
            else
            {
                base.RenderProgress(gump);
            }
        }

        public SecondKillObjective()
        {
        }

        public override void OnKill(BaseCreature creature, Container corpse)
        {
            if (creature is DeathwatchBeetleHatchling)
            {
                Complete();
                System.AddObjective(new ThirdKillObjective(2));
            }
        }

        public override void OnRead()
        {
            if (!Completed)
            {
                Complete();
                System.AddObjective(new ThirdKillObjective(1));
            }
        }
    }

    public class ThirdKillObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                // Continue killing Deathwatch Beetle Hatchlings.
                return 1063319;
            }
        }

        public override void RenderProgress(BaseQuestGump gump)
        {
            if (!Completed)
            {
                // Deathwatch Beetle Hatchlings killed:
                gump.AddHtmlLocalized(70, 260, 270, 100, 1063318, 0x12DC6BF, false, false);

                gump.AddLabel(70, 280, 0x64, CurProgress.ToString());
                gump.AddLabel(100, 280, 0x64, "/");
                gump.AddLabel(130, 280, 0x64, "10");
            }
            else
            {
                base.RenderProgress(gump);
            }
        }

        public override int MaxProgress { get { return 10; } }

        public ThirdKillObjective(int startingProgress)
        {
            CurProgress = startingProgress;
        }

        public ThirdKillObjective()
        {
        }

        public override void OnKill(BaseCreature creature, Container corpse)
        {
            if (creature is DeathwatchBeetleHatchling)
                CurProgress++;
        }

        public override void OnComplete()
        {
            System.AddObjective(new ReturnObjective());
        }
    }

    public class ReturnObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                // Return to Ansella Gryen for your reward.
                return 1063313;
            }
        }

        public ReturnObjective()
        {
        }

        public override void OnComplete()
        {
            System.AddConversation(new EndConversation());
        }
    }

    #endregion

    #region Mobile Conversation

    public class AcceptConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I><U>Important Quest Information</U></I><BR><BR>
                 * 
                 * During your quest, any important information that a
                 * <a href = "?ForceTopic31">NPC</a> gives you, will appear in a window
                 * such as this one.  You can review the information at any time during your
                 * quest.<BR><BR>
                 * 
                 * <U>Getting Help</U><BR><BR>
                 * 
                 * Some of the text you will come across during your quest,
                 * will be underlined <a href = "?ForceTopic73">links to the codex of wisdom</a>,
                 * or online help system.  You can click on the text to get detailed information
                 * on the underlined subject.  You may also access the Codex Of Wisdom by
                 * pressing "F1" or by clicking on the "?" on the toolbar at the top of
                 * your screen.<BR><BR><U>Context Menus</U><BR><BR>
                 * 
                 * Context menus can be called up by single left-clicking (or Shift + single
                 * left-click, if you changed it) on most objects or NPCs in the world.
                 * Nearly everything, including your own avatar will have context menus available.
                 * Bringing up your avatar's context menu will give you options to cancel your quest
                 * and review various quest information.<BR><BR>
                 */
                return 1049092;
            }
        }

        public AcceptConversation()
        {
        }

        public override void OnRead()
        {
            System.AddObjective(new FirstKillObjective());
        }
    }

    public class DirectionConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                // The Deathwatch Beetle Hatchlings live in The Waste - the desert close to this city.
                return 1063323;
            }
        }

        public override bool Logged { get { return false; } }

        public DirectionConversation()
        {
        }
    }

    public class TakeCareConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                // I know you can take care of those nasty Deathwatch Beetle Hatchlings! No get to it!
                return 1063324;
            }
        }

        public override bool Logged { get { return false; } }

        public TakeCareConversation()
        {
        }
    }

    public class EndConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* Thank you for helping me get rid of these vile beasts!
                 * You have been rewarded for your good deeds. If you wish to
                 * help me in the future, visit me again.<br><br>
                 * 
                 * Farewell.
                 */
                return 1063321;
            }
        }

        public EndConversation()
        {
        }

        public override void OnRead()
        {
            System.Complete();
        }
    }

    #endregion
}