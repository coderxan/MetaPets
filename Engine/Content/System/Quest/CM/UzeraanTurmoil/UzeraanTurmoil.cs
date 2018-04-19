using System;

using Server;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Engines.Quests.Haven
{
    public class UzeraanTurmoilQuest : QuestSystem
    {
        private static Type[] m_TypeReferenceTable = new Type[]
			{
				typeof( Haven.AcceptConversation ),
				typeof( Haven.UzeraanTitheConversation ),
				typeof( Haven.UzeraanFirstTaskConversation ),
				typeof( Haven.UzeraanReportConversation ),
				typeof( Haven.SchmendrickConversation ),
				typeof( Haven.UzeraanScrollOfPowerConversation ),
				typeof( Haven.DryadConversation ),
				typeof( Haven.UzeraanFertileDirtConversation ),
				typeof( Haven.UzeraanDaemonBloodConversation ),
				typeof( Haven.UzeraanDaemonBoneConversation ),
				typeof( Haven.BankerConversation ),
				typeof( Haven.RadarConversation ),
				typeof( Haven.LostScrollOfPowerConversation ),
				typeof( Haven.LostFertileDirtConversation ),
				typeof( Haven.DryadAppleConversation ),
				typeof( Haven.LostDaemonBloodConversation ),
				typeof( Haven.LostDaemonBoneConversation ),
				typeof( Haven.FindUzeraanBeginObjective ),
				typeof( Haven.TitheGoldObjective ),
				typeof( Haven.FindUzeraanFirstTaskObjective ),
				typeof( Haven.KillHordeMinionsObjective ),
				typeof( Haven.FindUzeraanAboutReportObjective ),
				typeof( Haven.FindSchmendrickObjective ),
				typeof( Haven.FindApprenticeObjective ),
				typeof( Haven.ReturnScrollOfPowerObjective ),
				typeof( Haven.FindDryadObjective ),
				typeof( Haven.ReturnFertileDirtObjective ),
				typeof( Haven.GetDaemonBloodObjective ),
				typeof( Haven.ReturnDaemonBloodObjective ),
				typeof( Haven.GetDaemonBoneObjective ),
				typeof( Haven.ReturnDaemonBoneObjective ),
				typeof( Haven.CashBankCheckObjective ),
				typeof( Haven.FewReagentsConversation )
			};

        public override Type[] TypeReferenceTable { get { return m_TypeReferenceTable; } }

        public override object Name
        {
            get
            {
                // "Uzeraan's Turmoil"
                return 1049007;
            }
        }

        public override object OfferMessage
        {
            get
            {
                /* <I>The guard speaks to you as you come closer... </I><BR><BR>
                 * 
                 * Greetings traveler! <BR><BR>
                 * 
                 * Uzeraan, the lord of this house and overseer of this city -
                 * <a href="?ForceTopic72">Haven</a>, has requested an audience with you. <BR><BR>
                 * 
                 * Hordes of gruesome hell spawn are beginning to overrun the
                 * city and terrorize the inhabitants.  No one seems to be able
                 * to stop them.<BR><BR>
                 * 
                 * Our fine city militia is falling to the evil creatures
                 * one battalion after the other.<BR><BR>
                 * 
                 * Uzeraan, whom you can find through these doors, is looking to
                 * hire mercenaries to aid in the battle. <BR><BR>
                 * 
                 * Will you assist us?
                 */
                return 1049008;
            }
        }

        public override TimeSpan RestartDelay { get { return TimeSpan.MaxValue; } }
        public override bool IsTutorial { get { return true; } }

        public override int Picture
        {
            get
            {
                switch (From.Profession)
                {
                    case 1: return 0x15C9; // warrior
                    case 2: return 0x15C1; // magician
                    default: return 0x15D3; // paladin
                }
            }
        }

        private bool m_HasLeftTheMansion;

        public override void Slice()
        {
            if (!m_HasLeftTheMansion && (From.Map != Map.Trammel || From.X < 3573 || From.X > 3611 || From.Y < 2568 || From.Y > 2606))
            {
                m_HasLeftTheMansion = true;
                AddConversation(new RadarConversation());
            }

            base.Slice();
        }

        public UzeraanTurmoilQuest(PlayerMobile from)
            : base(from)
        {
        }

        // Serialization
        public UzeraanTurmoilQuest()
        {
        }

        public override void Accept()
        {
            base.Accept();

            AddConversation(new AcceptConversation());
        }

        public override void ChildDeserialize(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            m_HasLeftTheMansion = reader.ReadBool();
        }

        public override void ChildSerialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)0); // version

            writer.Write((bool)m_HasLeftTheMansion);
        }

        public static bool HasLostScrollOfPower(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;

            if (pm == null)
                return false;

            QuestSystem qs = pm.Quest;

            if (qs is UzeraanTurmoilQuest)
            {
                if (qs.IsObjectiveInProgress(typeof(ReturnScrollOfPowerObjective)))
                {
                    Container pack = from.Backpack;

                    return (pack == null || pack.FindItemByType(typeof(SchmendrickScrollOfPower)) == null);
                }
            }

            return false;
        }

        public static bool HasLostFertileDirt(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;

            if (pm == null)
                return false;

            QuestSystem qs = pm.Quest;

            if (qs is UzeraanTurmoilQuest)
            {
                if (qs.IsObjectiveInProgress(typeof(ReturnFertileDirtObjective)))
                {
                    Container pack = from.Backpack;

                    return (pack == null || pack.FindItemByType(typeof(QuestFertileDirt)) == null);
                }
            }

            return false;
        }

        public static bool HasLostDaemonBlood(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;

            if (pm == null)
                return false;

            QuestSystem qs = pm.Quest;

            if (qs is UzeraanTurmoilQuest)
            {
                if (qs.IsObjectiveInProgress(typeof(ReturnDaemonBloodObjective)))
                {
                    Container pack = from.Backpack;

                    return (pack == null || pack.FindItemByType(typeof(QuestDaemonBlood)) == null);
                }
            }

            return false;
        }

        public static bool HasLostDaemonBone(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;

            if (pm == null)
                return false;

            QuestSystem qs = pm.Quest;

            if (qs is UzeraanTurmoilQuest)
            {
                if (qs.IsObjectiveInProgress(typeof(ReturnDaemonBoneObjective)))
                {
                    Container pack = from.Backpack;

                    return (pack == null || pack.FindItemByType(typeof(QuestDaemonBone)) == null);
                }
            }

            return false;
        }
    }

    #region Questing Objectives

    public class FindUzeraanBeginObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                // Find Uzeraan.  Uzeraan will explain what you need to do next.
                return 1046039;
            }
        }

        public FindUzeraanBeginObjective()
        {
        }

        public override void OnComplete()
        {
            if (System.From.Profession == 5) // paladin
                System.AddConversation(new UzeraanTitheConversation());
            else
                System.AddConversation(new UzeraanFirstTaskConversation());
        }
    }

    public class TitheGoldObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* Go to the shrine inside of Uzeraan's Mansion, near the front doors and
                 * <a href = "?ForceTopic109">tithe</a> at least 500 gold.<BR><BR>
                 * 
                 * Return to Uzeraan when you are done.
                 */
                return 1060386;
            }
        }

        private int m_OldTithingPoints;

        public TitheGoldObjective()
        {
            m_OldTithingPoints = -1;
        }

        public override void CheckProgress()
        {
            PlayerMobile pm = System.From;
            int curTithingPoints = pm.TithingPoints;

            if (curTithingPoints >= 500)
                Complete();
            else if (curTithingPoints > m_OldTithingPoints && m_OldTithingPoints >= 0)
                pm.SendLocalizedMessage(1060240, "", 0x41); // You must have at least 500 tithing points before you can continue in your quest.

            m_OldTithingPoints = curTithingPoints;
        }

        public override void OnComplete()
        {
            System.AddObjective(new FindUzeraanFirstTaskObjective());
        }
    }

    public class FindUzeraanFirstTaskObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                // Return to Uzeraan, now that you have enough tithing points to continue your quest.
                return 1060387;
            }
        }

        public FindUzeraanFirstTaskObjective()
        {
        }

        public override void OnComplete()
        {
            System.AddConversation(new UzeraanFirstTaskConversation());
        }
    }

    public enum KillHordeMinionsStep
    {
        First,
        LearnKarma,
        Others
    }

    public class KillHordeMinionsObjective : QuestObjective
    {
        private KillHordeMinionsStep m_Step;

        public KillHordeMinionsStep Step { get { return m_Step; } }

        public override object Message
        {
            get
            {
                switch (m_Step)
                {
                    case KillHordeMinionsStep.First:
                        /* Find the mountain pass beyond the house which lies at the
                         * end of the runic road.<BR><BR>
                         * 
                         * Assist the city Militia by slaying <I>Horde Minions</I>
                         */
                        return 1049089;

                    case KillHordeMinionsStep.LearnKarma:
                        /* You have just gained some <a href="?ForceTopic45">Karma</a>
                         * for killing the horde minion. <a href="?ForceTopic134">Learn</a>
                         * how this affects your Paladin abilities.
                         */
                        return 1060389;

                    default:
                        // Continue driving back the Horde Minions, as Uzeraan instructed you to do.
                        return 1060507;
                }
            }
        }

        public override int MaxProgress
        {
            get
            {
                if (System.From.Profession == 5) // paladin
                {
                    switch (m_Step)
                    {
                        case KillHordeMinionsStep.First: return 1;
                        case KillHordeMinionsStep.LearnKarma: return 2;
                        default: return 5;
                    }
                }
                else
                {
                    return 5;
                }
            }
        }

        public override bool Completed
        {
            get
            {
                if (m_Step == KillHordeMinionsStep.LearnKarma && HasBeenRead)
                    return true;
                else
                    return base.Completed;
            }
        }

        public KillHordeMinionsObjective()
        {
        }

        public KillHordeMinionsObjective(KillHordeMinionsStep step)
        {
            m_Step = step;
        }

        public override void RenderProgress(BaseQuestGump gump)
        {
            if (!Completed)
            {
                gump.AddHtmlObject(70, 260, 270, 100, 1049090, BaseQuestGump.Blue, false, false); // Horde Minions killed:
                gump.AddLabel(70, 280, 0x64, CurProgress.ToString());
            }
            else
            {
                base.RenderProgress(gump);
            }
        }

        public override void OnRead()
        {
            CheckCompletionStatus();
        }

        public override bool IgnoreYoungProtection(Mobile from)
        {
            // This restriction continues until the quest is ended
            if (from is HordeMinion && from.Map == Map.Trammel && from.X >= 3314 && from.X <= 3814 && from.Y >= 2345 && from.Y <= 3095) // Haven island
                return true;

            return false;
        }

        public override void OnKill(BaseCreature creature, Container corpse)
        {
            if (creature is HordeMinion && corpse.Map == Map.Trammel && corpse.X >= 3314 && corpse.X <= 3814 && corpse.Y >= 2345 && corpse.Y <= 3095) // Haven island
            {
                if (CurProgress == 0)
                    System.From.Send(new DisplayHelpTopic(29, false)); // HEALING

                CurProgress++;
            }
        }

        public override void OnComplete()
        {
            if (System.From.Profession == 5)
            {
                switch (m_Step)
                {
                    case KillHordeMinionsStep.First:
                        {
                            QuestObjective obj = new KillHordeMinionsObjective(KillHordeMinionsStep.LearnKarma);
                            System.AddObjective(obj);
                            obj.CurProgress = CurProgress;
                            break;
                        }
                    case KillHordeMinionsStep.LearnKarma:
                        {
                            QuestObjective obj = new KillHordeMinionsObjective(KillHordeMinionsStep.Others);
                            System.AddObjective(obj);
                            obj.CurProgress = CurProgress;
                            break;
                        }
                    default:
                        {
                            System.AddObjective(new FindUzeraanAboutReportObjective());
                            break;
                        }
                }
            }
            else
            {
                System.AddObjective(new FindUzeraanAboutReportObjective());
            }
        }

        public override void ChildDeserialize(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            m_Step = (KillHordeMinionsStep)reader.ReadEncodedInt();
        }

        public override void ChildSerialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)0); // version

            writer.WriteEncodedInt((int)m_Step);
        }
    }

    public class FindUzeraanAboutReportObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* It's no use... The <I>Horde Minions</I> are too many.
                 * They are appearing out of nowhere.<BR><BR>
                 * 
                 * Return to Uzeraan and report your findings.
                 */
                return 1049091;
            }
        }

        public FindUzeraanAboutReportObjective()
        {
        }

        public override void OnComplete()
        {
            System.AddConversation(new UzeraanReportConversation());
        }
    }

    public class FindSchmendrickObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* Prepare for battle and step onto the teleporter,
                 * located against the wall in the main hall of Uzeraan's mansion.<BR><BR>
                 * 
                 * Find Schmendrick within the mines.
                 */
                return 1049120;
            }
        }

        public FindSchmendrickObjective()
        {
        }

        public override bool IgnoreYoungProtection(Mobile from)
        {
            // This restriction begins when this objective is completed, and continues until the quest is ended
            if (Completed && from is RestlessSoul && from.Map == Map.Trammel && from.X >= 5199 && from.X <= 5271 && from.Y >= 1812 && from.Y <= 1865) // Schmendrick's cave
                return true;

            return false;
        }

        public override void OnComplete()
        {
            System.AddConversation(new SchmendrickConversation());
        }
    }

    public class FindApprenticeObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* Find Schmendrick's apprentice who is somewhere in the mining cave.
                 * The apprentice has the scroll of power needed by Uzeraan.
                 */
                return 1049323;
            }
        }

        public FindApprenticeObjective()
        {
        }

        public override void OnComplete()
        {
            System.AddObjective(new ReturnScrollOfPowerObjective());
        }
    }

    public class ReturnScrollOfPowerObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* You have obtained the scroll of power!  Find your way out of the cave.<BR><BR>
                 * 
                 * Hand the scroll to Uzeraan (drag and drop) once you arrive in his mansion.
                 */
                return 1049324;
            }
        }

        public ReturnScrollOfPowerObjective()
        {
        }

        public override void OnComplete()
        {
            System.AddConversation(new UzeraanScrollOfPowerConversation());
        }
    }

    public class FindDryadObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* Find the Dryad in the woods of Haven and get a patch
                 * of fertile dirt from her.<BR><BR>
                 * 
                 * Use Uzeraan's teleporter to get there if necessary.
                 */
                return 1049358;
            }
        }

        public FindDryadObjective()
        {
        }

        public override void OnComplete()
        {
            System.AddConversation(new DryadConversation());
        }
    }

    public class ReturnFertileDirtObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* You have acquired the <I>Fertile Dirt</I>!<BR><BR>
                 * 
                 * Return to the mansion (<a href = "?ForceTopic13">North-East</a>
                 * of the Dryad's Grove) and hand it to Uzeraan.
                 */
                return 1049327;
            }
        }

        public ReturnFertileDirtObjective()
        {
        }

        public override void OnComplete()
        {
            System.AddConversation(new UzeraanFertileDirtConversation());
        }
    }

    public class GetDaemonBloodObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* Bring back a vial of blood.<BR><BR>
                 * 
                 * Follow the road leading north from the mansion and walk into the hut
                 * to find the chest that contains the vial
                 */
                return 1049361;
            }
        }

        private bool m_Ambushed;

        public GetDaemonBloodObjective()
        {
        }

        public override void CheckProgress()
        {
            PlayerMobile player = System.From;

            if (!m_Ambushed && player.Map == Map.Trammel && player.InRange(new Point3D(3456, 2558, 50), 30))
            {
                int x = player.X - 1;
                int y = player.Y - 2;
                int z = Map.Trammel.GetAverageZ(x, y);

                if (Map.Trammel.CanSpawnMobile(x, y, z))
                {
                    m_Ambushed = true;

                    player.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1049330); // You have been ambushed! Fight for your honor!!!

                    BaseCreature creature = new HordeMinion();
                    creature.MoveToWorld(new Point3D(x, y, z), Map.Trammel);
                    creature.Combatant = player;
                }
            }
        }

        public override void OnComplete()
        {
            System.AddObjective(new ReturnDaemonBloodObjective());
        }

        public override void ChildDeserialize(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            m_Ambushed = reader.ReadBool();
        }

        public override void ChildSerialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)0); // version

            writer.Write((bool)m_Ambushed);
        }
    }

    public class ReturnDaemonBloodObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* You have the vial of blood!<BR><BR>
                 * 
                 * Return to Uzeraan's mansion and hand him the vial.
                 */
                return 1049332;
            }
        }

        public ReturnDaemonBloodObjective()
        {
        }

        public override void OnComplete()
        {
            System.AddConversation(new UzeraanDaemonBloodConversation());
        }
    }

    public class GetDaemonBoneObjective : QuestObjective
    {
        private Container m_CorpseWithBone;

        public Container CorpseWithBone
        {
            get { return m_CorpseWithBone; }
            set { m_CorpseWithBone = value; }
        }

        public override object Message
        {
            get
            {
                if (System.From.Profession == 5)
                {
                    /* Use your <a href="?ForceTopic108">Sacred Journey</a>
                     * ability on the rune to the <a href="?ForceTopic13">North</a>
                     * of Uzeraan to travel to the graveyard.
                     */
                    return 1060755;
                }
                else
                {
                    /* Use Uzeraan's teleporter to get to the Haunted graveyard.<BR><BR>
                     * 
                     * Slay the undead until you find a <I>Daemon Bone</I>.
                     */
                    return 1049362;
                }
            }
        }

        public GetDaemonBoneObjective()
        {
        }

        public override void OnComplete()
        {
            System.AddObjective(new ReturnDaemonBoneObjective());
        }

        public override bool IgnoreYoungProtection(Mobile from)
        {
            // This restriction continues until the end of the quest
            if ((from is Zombie || from is Skeleton) && from.Map == Map.Trammel && from.X >= 3391 && from.X <= 3424 && from.Y >= 2639 && from.Y <= 2664) // Haven graveyard
                return true;

            return false;
        }

        public override bool GetKillEvent(BaseCreature creature, Container corpse)
        {
            if (base.GetKillEvent(creature, corpse))
                return true;

            return UzeraanTurmoilQuest.HasLostDaemonBone(System.From);
        }

        public override void OnKill(BaseCreature creature, Container corpse)
        {
            if ((creature is Zombie || creature is Skeleton) && corpse.Map == Map.Trammel && corpse.X >= 3391 && corpse.X <= 3424 && corpse.Y >= 2639 && corpse.Y <= 2664) // Haven graveyard
            {
                if (Utility.RandomDouble() < 0.25)
                    m_CorpseWithBone = corpse;
            }
        }

        public override void ChildDeserialize(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            m_CorpseWithBone = (Container)reader.ReadItem();
        }

        public override void ChildSerialize(GenericWriter writer)
        {
            if (m_CorpseWithBone != null && m_CorpseWithBone.Deleted)
                m_CorpseWithBone = null;

            writer.WriteEncodedInt((int)0); // version

            writer.Write((Item)m_CorpseWithBone);
        }
    }

    public class ReturnDaemonBoneObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* Head <a href = "?ForceTopic13">East</a> of here (or use the Horn of Retreat)
                 * to return to Uzeraan's Mansion and deliver the bone to Uzeraan.
                 */
                return 1049334;
            }
        }

        public ReturnDaemonBoneObjective()
        {
        }

        public override void OnComplete()
        {
            System.AddConversation(new UzeraanDaemonBoneConversation());
        }
    }

    public class CashBankCheckObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* Locate the Haven Bank (use the teleporter in Uzeraan's Mansion
                 * if necessary), which lies <a href = "?ForceTopic13">South-East</a>
                 * of Uzeraan's Mansion.  Once there, <a href="?ForceTopic86">cash your check</a>.
                 */
                return 1049336;
            }
        }

        public CashBankCheckObjective()
        {
        }

        public override void OnComplete()
        {
            System.AddConversation(new BankerConversation());
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
                 * <a href = "?ForceTopic31">NPC</a> gives you, will appear
                 * in a window such as this one.  You can review the information
                 * at any time during your quest.<BR><BR>
                 * 
                 * <U>Getting Help</U><BR><BR>
                 * 
                 * Some of the text you will come across during your quest,
                 * will be underlined <a href = "?ForceTopic73">links to the codex of wisdom</a>,
                 * or online help system.  You can click on the text to get detailed
                 * information on the underlined subject.  You may also access the
                 * Codex Of Wisdom by pressing "F1" or by clicking on the "?" on the toolbar
                 * at the top of your screen.<BR><BR>
                 * 
                 * <U>Context Menus</U><BR><BR>
                 * 
                 * Context menus can be called up by single left-clicking
                 * (or Shift + single left-click, if you changed it) on most objects
                 * or NPCs in the world.  Nearly everything, including your own avatar
                 * will have context menus available.  Bringing up your avatar's
                 * context menu will give you options to cancel your quest and review
                 * various quest information.<BR><BR>
                 */
                return 1049092;
            }
        }

        public AcceptConversation()
        {
        }

        public override void OnRead()
        {
            System.AddObjective(new FindUzeraanBeginObjective());
        }
    }

    public class UzeraanTitheConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>Uzeraan greets you as you approach...</I><BR><BR>
                 * 
                 * Greetings traveler!<BR><BR>
                 * 
                 * I am Uzeraan, the lord of this house and overseer of this fine city, Haven.
                 * I know we have just met, but time is short and we need to reinforce
                 * the troops in the mountain pass, so I will not waste your time
                 * with pleasantries.<BR><BR>
                 * 
                 * We have been trying to fight back the wicked <I>Horde Minions</I> which
                 * have recently begun attacking our cities - but to no avail.
                 * We desperately need help!<BR><BR>
                 * 
                 * Your first task will be to assess the situation in the mountain pass,
                 * and help our troops defeat the Horde Minions there.<BR><BR>
                 * 
                 * Before I send you into battle however, it is time that I teach you a thing
                 * or two about the way of the Paladin.<BR><BR>
                 * 
                 * <U>The Paladin</U><BR><BR>
                 * 
                 * Paladins are the holy warriors of the realm who have dedicated themselves
                 * as protectors of the virtues and vanquishers of all that is evil.<BR><BR>
                 * 
                 * Paladins have several <a href = "?ForceTopic111">special abilities</a> that
                 * are not available to the ordinary warrior. Due to the spiritual nature of
                 * these abilities, the Paladin requires some amount of mana to activate them.
                 * In addition to mana, the Paladin is also required to spend a certain amount
                 * of <a href = "?ForceTopic109">tithing points</a> each time a special ability
                 * is used.<BR><BR>
                 * 
                 * Tithing points and mana are automatically consumed when a special Paladin
                 * ability is activated. All Paladin abilities are activated through the
                 * <a href = "?ForceTopic114">Book of Chivalry</a><BR><BR>
                 * 
                 * Go now, to the shrine just East of here, just before the doors and
                 * <a href = "?ForceTopic109">tithe</a> at least 500 gold.<BR><BR>
                 * 
                 * Return here when you are done.
                 */
                return 1060209;
            }
        }

        public UzeraanTitheConversation()
        {
        }

        public override void OnRead()
        {
            System.AddObjective(new TitheGoldObjective());
        }
    }

    public class UzeraanFirstTaskConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                if (System.From.Profession == 1) // warrior
                {
                    /* <I>Uzeraan greets you as you approach...</I><BR><BR>
                     * 
                     * Greetings traveler!<BR><BR>
                     * 
                     * I am Uzeraan, the lord of this house and overseer of this fine city, Haven.
                     * I know we have just met, but time is short and we need to reinforce the troops
                     * in the mountain pass, so I will not waste your time with pleasantries.<BR><BR>
                     * 
                     * We have been trying to fight back the wicked <I>Horde Minions</I> which have
                     * recently begun attacking our cities - but to no avail.  We desperately need
                     * help!<BR><BR>
                     * 
                     * Your first task will be to assess the situation in the mountain pass, and
                     * help our troops defeat the Horde Minions there.<BR><BR>
                     * 
                     * Take the road marked with glowing runes, that starts just outside of this
                     * mansion. Before you go into battle, it would be prudent to
                     * <a href="?ForceTopic27">review combat techniques</a> as well as
                     * <a href = "?ForceTopic29">information on healing yourself</a>.<BR><BR>
                     * 
                     * To aid you in your fight, you may also wish to
                     * <a href = "?ForceTopic33">purchase equipment</a> from Frank the Blacksmith,
                     * who is standing just <a href = "?ForceTopic13">South</a> of here.<BR><BR>
                     * 
                     * Good luck young warrior.
                     */
                    return 1049088;
                }
                else if (System.From.Profession == 2) // magician
                {
                    /* <I>Uzeraan greets you as you approach...</I><BR><BR>
                     * 
                     * Greetings traveler!<BR><BR>
                     * 
                     * I am Uzeraan, the lord of this house and overseer of this fine city, Haven.
                     * I know we have just met, but time is short and we need to reinforce
                     * the troops in the mountain pass, so I will not waste your time with
                     * pleasantries.<BR><BR>
                     * 
                     * We have been trying to fight back the wicked <I>Horde Minions</I> which have
                     * recently begun attacking our cities - but to no avail.  We desperately
                     * need help!<BR><BR>
                     * 
                     * Your first task will be to assess the situation in the mountain pass,
                     * and help our troops defeat the Horde Minions there.<BR><BR>
                     * 
                     * Take the road marked with glowing runes, that starts just outside of this
                     * mansion. Before you go into battle, it would be prudent to
                     * <a href="?ForceTopic35">review your magic skills</a> as well as
                     * <a href = "?ForceTopic29">information on healing yourself</a>.<BR><BR>
                     * 
                     * To aid you in your fight, you may also wish to
                     * <a href = "?ForceTopic33">purchase equipment</a> from Frank the Blacksmith,
                     * who is standing just <a href = "?ForceTopic13">South</a> of here.<BR><BR>
                     * 
                     * Good luck young mage.
                     */
                    return 1049386;
                }
                else
                {
                    /* <I>Uzeraan nods at you with approval and begins to speak...</I><BR><BR>
                     * 
                     * Now that you are ready, let me give you your first task.<BR><BR>
                     * 
                     * As I mentioned earlier, we have been trying to fight back the wicked
                     * <I>Horde Minions</I> which have recently begun attacking our cities
                     * - but to no avail. Our need is great!<BR><BR>
                     * 
                     * Your first task will be to assess the situation in the mountain pass,
                     * and help our troops defeat the Horde Minions there.<BR><BR>
                     * 
                     * Take the road marked with glowing runes, that starts just outside of this mansion.
                     * Before you go into battle, it would be prudent to
                     * <a href="?ForceTopic27">review combat techniques</a> as well as
                     * <a href = "?ForceTopic29">information on healing yourself,
                     * using your Paladin ability 'Close Wounds'</a>.<BR><BR>
                     * 
                     * To aid you in your fight, you may also wish to
                     * <a href = "?ForceTopic33">purchase equipment</a> from Frank the Blacksmith,
                     * who is standing just <a href = "?ForceTopic13">South</a> of here.<BR><BR>
                     * 
                     * Good luck young Paladin!
                     */
                    return 1060388;
                }
            }
        }

        private static QuestItemInfo[] m_Info = new QuestItemInfo[]
			{
				new QuestItemInfo( 1023676, 0xE68 ) // glowing rune
			};

        public override QuestItemInfo[] Info { get { return m_Info; } }

        public UzeraanFirstTaskConversation()
        {
        }

        public override void OnRead()
        {
            System.AddObjective(new KillHordeMinionsObjective());
        }
    }

    public class UzeraanReportConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                if (System.From.Profession == 2) // magician
                {
                    /* <I>You give your report to Uzeraan and after a while, he begins to
                     * speak...</I><BR><BR>
                     * 
                     * Your report is grim,  but all hope is not lost!  It has become apparent
                     * that our swords and spells will not drive the evil from Haven.<BR><BR>
                     * 
                     * The head of my order, the High Mage Schmendrick, arrived here shortly after
                     * you went into battle with the <I>Horde Minions</I>.  He has brought with him a
                     * scroll of great power, that should aid us greatly in our battle.<BR><BR>
                     * 
                     * Unfortunately, the entrance to one of our mining caves collapsed recently,
                     * trapping our miners inside.<BR><BR>
                     * 
                     * Schmendrick went to install magical teleporters inside the mines so that
                     * the miners would have a way out.  The miners have since returned, but Schmendrick has not.
                     * Those who have returned, all seem to have lost their minds to madness;
                     * mumbling strange things of "the souls of the dead seeking revenge".<BR><BR>
                     * 
                     * No matter. We must find Schmendrick.<BR><BR>
                     * 
                     * Step onto the teleporter, located against the wall, and seek Schmendrick in the mines.<BR><BR>
                     * 
                     * I've given you a bag with some <a href="?ForceTopic93">Travel Spells</a>,
                     * in case you need to make a quick escape. In addition, you may wish to cast
                     * the <a href="?ForceTopic92">Night Sight</a> spell on yourself before going
                     * into the cave, as it it's pretty dark in there.<BR><BR>
                     * 
                     * Now please go. Good luck, friend.
                     */
                    return 1049387;
                }
                else
                {
                    /* <I>You give your report to Uzeraan and after a while,
                     * he begins to speak...</I><BR><BR>
                     * 
                     * Your report is grim, but all hope is not lost!  It has become apparent
                     * that our swords and spells will not drive the evil from Haven.<BR><BR>
                     * 
                     * The head of my order, the High Mage Schmendrick, arrived here shortly after
                     * you went into battle with the <I>Horde Minions</I>.  He has brought with him a
                     * scroll of great power, that should aid us greatly in our battle.<BR><BR>
                     * 
                     * Unfortunately, the entrance to one of our mining caves collapsed recently,
                     * trapping our miners inside.<BR><BR>
                     * 
                     * Schmendrick went to install magical teleporters inside the mines so that
                     * the miners would have a way out.  The miners have since returned, but Schmendrick has not.
                     * Those who have returned, all seem to have lost their minds to madness;
                     * mumbling strange things of "the souls of the dead seeking revenge".<BR><BR>
                     * 
                     * No matter. We must find Schmendrick.<BR><BR>
                     * 
                     * Step onto the teleporter, located against the wall, and seek Schmendrick in the mines.<BR><BR>
                     * 
                     * I've given you a bag with some <a href="?ForceTopic75">Night Sight</a>
                     * and <a href="?ForceTopic76">Healing</a> <a href="?ForceTopic74">potions</a>
                     * to help you out along the way.  Good luck.
                     */
                    return 1049119;
                }
            }
        }

        private static QuestItemInfo[] m_Info = new QuestItemInfo[]
			{
				new QuestItemInfo( 1026153, 0x1822 ), // teleporter
				new QuestItemInfo( 1048032, 0xE76 ) // a bag
			};

        public override QuestItemInfo[] Info { get { return m_Info; } }

        public UzeraanReportConversation()
        {
        }

        public override void OnRead()
        {
            System.AddObjective(new FindSchmendrickObjective());
        }
    }

    public class SchmendrickConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                if (System.From.Profession == 5) // paladin
                {
                    /* <I>Schmendrick barely pays you any attention as you approach him.
                     * His mind seems to be occupied with something else. You explain to him that
                     * you came for the scroll of power and after a long while he begins to speak,
                     * but apparently still not giving you his full attention...</I><BR><BR>
                     * 
                     * Hmmm.. peculiar indeed. Very strange activity here indeed... I wonder...<BR><BR>
                     * 
                     * Hmmm. Oh yes! Scroll, you say? I don't have it, sorry. My apprentice
                     * was carrying it, and he ran off to somewhere in this cave. Find him and
                     * you will find the scroll.<BR><BR>
                     * 
                     * Be sure to bring the scroll to Uzeraan once you have it. He's the only person
                     * aside from myself who can read the ancient markings on the scroll.
                     * I need to figure out what's going on down here before I can leave.
                     * Strange activity indeed...<BR><BR>
                     * 
                     * One more thing...<BR><BR>
                     * 
                     * Be careful of the restless souls wandering about. They seem to be in the habit
                     * of spontaneously attacking people.  Perhaps using your paladin ability
                     * <a href="?ForceTopic104">Enemy of One</a> might help you overcome the perils
                     * of these halls.<BR><BR>
                     * 
                     * <I>Schmendrick goes back to his work and you seem to completely fade from his awareness...
                     */
                    return 1060749;
                }
                else
                {
                    /* <I>Schmendrick barely pays you any attention as you approach him.  His
                     * mind seems to be occupied with something else.  You explain to him that
                     * you came for the scroll of power and after a long while he begins to speak,
                     * but apparently still not giving you his full attention...</I><BR><BR>
                     * 
                     * Hmmm.. peculiar indeed.  Very strange activity here indeed... I wonder...<BR><BR>
                     * 
                     * Hmmm.  Oh yes! Scroll, you say?  I don't have it, sorry. My apprentice was
                     * carrying it, and he ran off to somewhere in this cave.  Find him and you will
                     * find the scroll.<BR><BR>Be sure to bring the scroll to Uzeraan once you
                     * have it. He's the only person aside from myself who can read the ancient
                     * markings on the scroll.  I need to figure out what's going on down here before
                     * I can leave.  Strange activity indeed...<BR><BR>
                     * 
                     * <I>Schmendrick goes back to his work and you seem to completely fade from his
                     * awareness...
                     */
                    return 1049322;
                }
            }
        }

        private static QuestItemInfo[] m_Info = new QuestItemInfo[]
			{
				new QuestItemInfo( 1023637, 0xE34 ) // scroll
			};

        public override QuestItemInfo[] Info { get { return m_Info; } }

        public SchmendrickConversation()
        {
        }

        public override void OnRead()
        {
            System.AddObjective(new FindApprenticeObjective());
        }
    }

    public class UzeraanScrollOfPowerConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>Uzeraan carefully unravels the scroll and begins to read...
                 * after a short while his face lights up with a smile and he speaks to you...</I><BR><BR>
                 * 
                 * This is wonderful, friend!  For your troubles I've given you a treasure map
                 * I had laying about, along with a shovel to <a href = "?ForceTopic91">dig up the treasure</a>.
                 * Feel free to find the treasure at your leisure.<BR><BR>
                 * 
                 * Now let us get back to the business of this scroll. The only trouble,
                 * is that this scroll calls for some special ingredients that I do
                 * not have on hand.<BR><BR>
                 * 
                 * Though it may involve some danger, I will ask of you to find
                 * these reagents for me.  <BR><BR>
                 * 
                 * There are three reagents I need to complete the spell.<BR><BR>
                 * 
                 * The first thing I need is some <I>Fertile Dirt</I>.<BR><BR>
                 * 
                 * There lives a Dryad on this island who I know would have such a thing on hand.
                 * I have recalibrated the teleporter to transport you to the Dryad's grove,
                 * which lies <a href = "?ForceTopic13">South-West</a> of this mansion.<BR><BR>
                 * 
                 * Tell her Uzeraan sent you, and she should cooperate.<BR><BR>
                 * 
                 * Should you get into trouble out there or should you lose your way, do not worry.
                 * I have also given you a magical horn - a <I>Horn of Retreat</I>.
                 * Play the horn at any time to open a magical gateway that leads back to this mansion.<BR><BR>
                 * 
                 * Should your horn run out of <a href = "?ForceTopic83">charges</a>,
                 * simply hand me or any of my mansion guards the horn to have it recharged.<BR><BR>
                 * 
                 * Good luck friend.
                 */
                return 1049325;
            }
        }

        private static QuestItemInfo[] m_Info = new QuestItemInfo[]
			{
				new QuestItemInfo( 1048030, 0x14EB ), // a Treasure Map
				new QuestItemInfo( 1023969, 0xF81 ), // Fertile Dirt
				new QuestItemInfo( 1049117, 0xFC4 ) // Horn of Retreat
			};

        public override QuestItemInfo[] Info { get { return m_Info; } }

        public UzeraanScrollOfPowerConversation()
        {
        }

        public override void OnRead()
        {
            System.AddObjective(new FindDryadObjective());
        }
    }

    public class DryadConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>The Dryad watches hungrily as you approach, giving you an
                 * uneasy feeling in the pit of your stomach.  You explain that
                 * Uzeraan has sent you for a quantity of fertile dirt.  With a wide grin
                 * and in a slightly hoarse voice she replies...</I><BR><BR>
                 * 
                 * <I>Fertile Dirt</I>, eh?  Well, I have a few patches here...but what have
                 * you brought me in return?  Came empty-handed did you?  That's unfortunate
                 * indeed... but since you were sent by my dear friend Uzeraan, I supposed
                 * I could oblige you.<BR><BR>
                 * 
                 * <I>The Dryad digs around in the ground and hands you a patch of Fertile Dirt.<BR><BR>
                 * 
                 * With a smile she goes back to her work...</I>
                 */
                return 1049326;
            }
        }

        public DryadConversation()
        {
        }

        public override void OnRead()
        {
            System.AddObjective(new ReturnFertileDirtObjective());
        }
    }

    public class UzeraanFertileDirtConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                if (System.From.Profession == 2) // magician
                {
                    /* <I>Uzeraan takes the dirt from you and smiles...<BR><BR></I>
                     * 
                     * Wonderful!  I knew I could count on you.  As a token of my appreciation
                     * I've given you a bag with some <a href = "?ForceTopic37">reagents</a>
                     * as well as some <a href="?ForceTopic35">spell scrolls</a>.  They should
                     * help out a bit.<BR><BR>
                     * 
                     * The next item I need is a <I>Vial of Blood</I>.  I know it seems strange,
                     * but that's what the formula asks for.  I have some locked away in a chest
                     * not far from here.  It's only a short distance from the mansion.  Let me
                     * give you directions...<BR><BR>Exit the front door to the East.  Then follow
                     * the path to the North.  You will pass by several pedestals with lanterns on
                     * them.  Continue on this path until you run into a small hut.  Walk up the
                     * stairs and through the door.  Inside you will find a chest.  Open it and
                     * bring me a <I>Vial of Blood</I> from inside the chest.  It's very easy to find.
                     * Just follow the road and you can't miss it.<BR><BR>
                     * 
                     * Good luck!
                     */
                    return 1049388;
                }
                else
                {
                    /* <I>Uzeraan takes the dirt from you and smiles...<BR><BR></I>
                     * 
                     * Wonderful!  I knew I could count on you.  As a token of my appreciation
                     * I've given you a bag with some bandages as well as some healing potions.
                     * They should help out a bit.<BR><BR>
                     * 
                     * The next item I need is a <I>Vial of Blood</I>.  I know it seems strange,
                     * but that's what the formula asks for.  I have some locked away in a chest
                     * not far from here.  It's only a short distance from the mansion.  Let me give
                     * you directions...<BR><BR>
                     * 
                     * Exit the front door to the East.  Then follow the path to the North.
                     * You will pass by several pedestals with lanterns on them.  Continue on this
                     * path until you run into a small hut.  Walk up the stairs and through the door.
                     * Inside you will find a chest.  Open it and bring me a <I>Vial of Blood</I>
                     * from inside the chest.  It's very easy to find.  Just follow the road and you
                     * can't miss it.<BR><BR>
                     * 
                     * Good luck!
                     */
                    return 1049329;
                }
            }
        }

        private static QuestItemInfo[] m_Info = new QuestItemInfo[]
			{
				new QuestItemInfo( 1023965, 0xF7D ), // Daemon Blood
				new QuestItemInfo( 1022581, 0xA22 ), // lantern
			};

        public override QuestItemInfo[] Info { get { return m_Info; } }

        public UzeraanFertileDirtConversation()
        {
        }

        public override void OnRead()
        {
            System.AddObjective(new GetDaemonBloodObjective());
        }
    }

    public class UzeraanDaemonBloodConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                if (System.From.Profession == 2) // magician
                {
                    //return 1049389; // localized message is bugged
                    return "<I>You hand Uzeraan the Vial of Blood, which he hastily accepts...</I><BR>"
                        + "<BR>"
                        + "Excellent work!  Only one reagent remains and the spell is complete!  The final "
                        + "requirement is a <I>Daemon Bone</I>, which will not be as easily acquired as the "
                        + "previous two components.<BR>"
                        + "<BR>"
                        + "There is a haunted graveyard on this island, which is the home to many undead "
                        + "creatures.   Dispose of the undead as you see fit.  Be sure to search their remains "
                        + "after you have smitten them, to check for a <I>Daemon Bone</I>.  I'm quite sure "
                        + "that you will find what we seek, if you are thorough enough with your "
                        + "extermination.<BR>"
                        + "<BR>"
                        + "Take these explosion spell scrolls and  magical wizard's hat to aid you in your "
                        + "battle.  The scrolls should help you make short work of the undead.<BR>"
                        + "<BR>"
                        + "Return here when you have found a <I>Daemon Bone</I>.";
                }
                else
                {
                    /* <I>You hand Uzeraan the Vial of Blood, which he hastily accepts...</I><BR><BR>
                     * 
                     * Excellent work!  Only one reagent remains and the spell is complete!
                     * The final requirement is a <I>Daemon Bone</I>, which will not be as easily
                     * acquired as the previous two components.<BR><BR>
                     * 
                     * There is a haunted graveyard on this island, which is the home to many
                     * undead creatures.   Dispose of the undead as you see fit.  Be sure to search
                     * their remains after you have smitten them, to check for a <I>Daemon Bone</I>.
                     * I'm quite sure that you will find what we seek, if you are thorough enough
                     * with your extermination.<BR><BR>
                     * 
                     * Take this magical silver sword to aid you in your battle.  Silver weapons
                     * will damage the undead twice as much as your regular weapon.<BR><BR>
                     * 
                     * Return here when you have found a <I>Daemon Bone</I>.
                     */
                    return 1049333;
                }
            }
        }

        private static QuestItemInfo[] m_Info = new QuestItemInfo[]
			{
				new QuestItemInfo( 1017412, 0xF80 ), // Daemon Bone
			};

        private static QuestItemInfo[] m_InfoPaladin = new QuestItemInfo[]
			{
				new QuestItemInfo( 1017412, 0xF80 ), // Daemon Bone
				new QuestItemInfo( 1060577, 0x1F14 ), // Recall Rune
			};

        public override QuestItemInfo[] Info
        {
            get
            {
                if (System.From.Profession == 5) // paladin
                    return m_InfoPaladin;
                else
                    return m_Info;
            }
        }

        public UzeraanDaemonBloodConversation()
        {
        }

        public override void OnRead()
        {
            System.AddObjective(new GetDaemonBoneObjective());
        }
    }

    public class UzeraanDaemonBoneConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>As you hand Uzeraan the final reagent, he nods at you with approval
                 * and starts searching through the pockets of his robe...<BR><BR>
                 * 
                 * After a short while he hands you a small pouch...</I><BR><BR>
                 * 
                 * There you are.  Your contract of employment with me has expired and so here
                 * is your pay.  2000 gold in the form of a check and a magical sextant that
                 * will help you find <a href = "?ForceTopic47">Moongates</a> and Banks.<BR><BR>
                 * 
                 * Before you can actually spend the money I have given you, however, you must
                 * <a href="?ForceTopic86">cash the check</a>.<BR><BR>
                 * 
                 * I have recalibrated the teleporter to take you to the Haven
                 * <a href="?ForceTopic38">Bank</a>.  Step onto the teleporter to be taken
                 * to the bank, which lies <a href = "?ForceTopic13">South-East</a> of here.<BR><BR>
                 * 
                 * Thank you for all your help friend.  I hope we shall meet
                 * each other again in the future.<BR><BR>
                 * 
                 * Farewell.
                 */
                return 1049335;
            }
        }

        public UzeraanDaemonBoneConversation()
        {
        }

        public override void OnRead()
        {
            System.AddObjective(new CashBankCheckObjective());
        }
    }

    public class BankerConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>The banker smiles at you and greets you in a loud and robust voice...</I><BR><BR>
                 * 
                 * Well hello there adventurer! I see you've learned how to cash checks. Wonderful!
                 * Let me tell you a bit about the banks in this world...<BR><BR>
                 * 
                 * Anything that you place into any bank box, can be retrieved from any other
                 * bank box in the land. For instance, if you place an item into a bank box in
                 * Britain, it can be retrieved from your bank box in Moonglow or any other city.<BR><BR>
                 * 
                 * Bank boxes are very secure. So secure, in fact, that no one can ever get into
                 * your bank box except for yourself. Security is hard to come by these days,
                 * but you can trust in the banking system of Britannia! We shall not let you down!<BR><BR>
                 * 
                 * I hope to be seeing much more of you as your riches grow! May your bank box overflow
                 * with the spoils of your adventures.<BR><BR>Farewell adventurer, you are now free to
                 * explore the world on your own.
                 */
                return 1060137;
            }
        }

        public BankerConversation()
        {
        }

        public override void OnRead()
        {
            System.Complete();
        }
    }

    public class RadarConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* If you are leaving the mansion, you should learn about the Radar Map.<BR><BR>
                 * 
                 * The Radar Map (or Overhead View) can be opened by pressing 'ALT-R' on your
                 * keyboard. It shows your immediate surroundings from a bird's eye view.<BR><BR>
                 * 
                 * Pressing ALT-R twice, will enlarge the Radar Map a little.  Use the Radar Map
                 * often as you travel throughout the world to familiarize yourself with your surroundings.
                 */
                return 1049660;
            }
        }

        public override bool Logged { get { return false; } }

        public RadarConversation()
        {
        }
    }

    public class LostScrollOfPowerConversation : QuestConversation
    {
        private bool m_FromUzeraan;

        public override object Message
        {
            get
            {
                if (m_FromUzeraan)
                {
                    /* You return without the scroll???<BR><BR>
                     * 
                     * All hope is lost without it, friend.  Return to the mines and talk to
                     * Schmendrick to see if he can help us out of this predicament.
                     */
                    return 1049377;
                }
                else
                {
                    /* You've lost the scroll?  Argh!  I will have to try and re-construct
                     * the scroll from memory.  Bring me a blank scroll, which you can
                     * <a href = "?ForceTopic33">purchase from the mage shop</a> just
                     * <a href = "?ForceTopic13">East</a> of Uzeraan's mansion in Haven.<BR><BR>
                     * 
                     * Return the scroll to me and I will try to make another scroll for you.<BR><BR>
                     * 
                     * When you return, be sure to hand me the scroll (drag and drop).
                     */
                    return 1049345;
                }
            }
        }

        public override bool Logged { get { return false; } }

        public LostScrollOfPowerConversation(bool fromUzeraan)
        {
            m_FromUzeraan = fromUzeraan;
        }

        public LostScrollOfPowerConversation()
        {
        }

        public override void ChildDeserialize(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            m_FromUzeraan = reader.ReadBool();
        }

        public override void ChildSerialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)0); // version

            writer.Write((bool)m_FromUzeraan);
        }
    }

    public class LostFertileDirtConversation : QuestConversation
    {
        private bool m_FromUzeraan;

        public override object Message
        {
            get
            {
                if (m_FromUzeraan)
                {
                    /* You return without <I>Fertile Dirt</I>?  It is imperative that we
                     * get all of the ingredients friend.<BR><BR>
                     * 
                     * Seek out the Dryad and ask her to help you again.
                     */
                    return 1049374;
                }
                else
                {
                    /* You've lost the dirt I gave you?<BR><BR>
                     * 
                     * My, my, my... What ever shall we do now?<BR><BR>
                     * 
                     * I can try to make you some more, but I will need something
                     * that I can transform.  Bring me an <I>apple</I>, and I shall
                     * see what I can do.<BR><BR>
                     * 
                     * You can <a href = "?ForceTopic33">buy</a> apples from the
                     * Provisioner's Shop, which is located a ways <a href = "?ForceTopic13">East</a>
                     * of Uzeraan's mansion.<BR><BR>
                     * 
                     * Hand me the apple when you have it, and I shall see about transforming
                     * it for you.<BR><BR>
                     * 
                     * Good luck.<BR><BR>
                     */
                    return 1049359;
                }
            }
        }

        public override bool Logged { get { return false; } }

        public LostFertileDirtConversation(bool fromUzeraan)
        {
            m_FromUzeraan = fromUzeraan;
        }

        public LostFertileDirtConversation()
        {
        }

        public override void ChildDeserialize(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            m_FromUzeraan = reader.ReadBool();
        }

        public override void ChildSerialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)0); // version

            writer.Write((bool)m_FromUzeraan);
        }
    }

    public class DryadAppleConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>The Dryad sticks the apple into the ground and you watch it
                 * rot before your eyes.<BR><BR>
                 * 
                 * She pulls the now fertile dirt out of the ground and hands
                 * it to you.</I><BR><BR>
                 * 
                 * There you go friend.  Try not to lose it again this time, eh?
                 */
                return 1049360;
            }
        }

        public override bool Logged { get { return false; } }

        public DryadAppleConversation()
        {
        }
    }

    public class LostDaemonBloodConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* You return without <I>a Vial of Blood</I>?  It is imperative that we
                 * get all of the ingredients friend.<BR><BR>
                 * 
                 * Go back to the chest and fetch another vial.  Please hurry.
                 */
                return 1049375;
            }
        }

        public override bool Logged { get { return false; } }

        public LostDaemonBloodConversation()
        {
        }
    }

    public class LostDaemonBoneConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* You return without <I>a Daemon Bone</I>?  It is imperative that we
                 * get all of the ingredients friend.<BR><BR>
                 * 
                 * Go back to the graveyard and continue hunting the undead until you
                 * find another one.  Please hurry.
                 */
                return 1049376;
            }
        }

        public override bool Logged { get { return false; } }

        public LostDaemonBoneConversation()
        {
        }
    }

    public class FewReagentsConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* I don't feel comfortable sending you into a potentially dangerous situation
                 * with as few <a href = "?ForceTopic37">reagents</a> as you have in your pack.<BR><BR>
                 * 
                 * Before going on, please acquire at least 30 of each reagent.  You can
                 * <a href ="?ForceTopic33">purchase</a> reagents from the Mage shop, which is
                 * located just <a href ="?ForceTopic13">East</a> this mansion.<BR><BR>
                 * 
                 * Remember that there are eight (8) different reagents: Black Pearl, Mandrake Root,
                 * Sulfurous Ash, Garlic, Ginseng, Blood Moss, Nightshade and Spider's Silk.<BR><BR>
                 * 
                 * Come back here when you are ready to go.
                 */
                return 1049390;
            }
        }

        public override bool Logged { get { return false; } }

        public FewReagentsConversation()
        {
        }
    }

    #endregion
}