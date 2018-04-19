using System;
using System.Collections;
using System.Collections.Generic;

using Server;
using Server.ContextMenus;
using Server.Engines.Plants;
using Server.Engines.Quests;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

namespace Server.Engines.Quests.Matriarch
{
    public class SolenMatriarchQuest : QuestSystem
    {
        private static Type[] m_TypeReferenceTable = new Type[]
			{
				typeof( Matriarch.DontOfferConversation ),
				typeof( Matriarch.AcceptConversation ),
				typeof( Matriarch.DuringKillInfiltratorsConversation ),
				typeof( Matriarch.GatherWaterConversation ),
				typeof( Matriarch.DuringWaterGatheringConversation ),
				typeof( Matriarch.ProcessFungiConversation ),
				typeof( Matriarch.DuringFungiProcessConversation ),
				typeof( Matriarch.FullBackpackConversation ),
				typeof( Matriarch.EndConversation ),
				typeof( Matriarch.KillInfiltratorsObjective ),
				typeof( Matriarch.ReturnAfterKillsObjective ),
				typeof( Matriarch.GatherWaterObjective ),
				typeof( Matriarch.ReturnAfterWaterObjective ),
				typeof( Matriarch.ProcessFungiObjective ),
				typeof( Matriarch.GetRewardObjective )
			};

        public override Type[] TypeReferenceTable { get { return m_TypeReferenceTable; } }

        public override object Name
        {
            get
            {
                // Solen Matriarch Quest
                return 1054147;
            }
        }

        public override object OfferMessage
        {
            get
            {
                if (IsFriend(From, RedSolen))
                {
                    /* <I>The Solen Matriarch smiles happily as you greet her.</I><BR><BR>
                     * 
                     * Hello again. It is always good to see a friend of our colony.<BR><BR>
                     * 
                     * Would you like me to process some zoogi fungus into powder of translocation
                     * for you? I would be happy to do so if you will first undertake a couple
                     * tasks for me.<BR><BR>
                     * 
                     * First, I would like for you to eliminate some infiltrators from the other
                     * solen colony. They are spying on my colony, and I fear for the safety of my
                     * people. They must be slain.<BR><BR>
                     * 
                     * After that, I must ask that you gather some water for me. Our water supplies
                     * are inadequate, so we must try to supplement our reserve using water vats here
                     * in our lair.<BR><BR>
                     * 
                     * Will you accept my offer?
                     */
                    return 1054083;
                }
                else
                {
                    /* <I>The Solen Matriarch smiles happily as she eats the seed you offered.</I><BR><BR>
                     * 
                     * I think you for that seed. I was quite delicious. So full of flavor.<BR><BR>
                     * 
                     * Hmm... if you would like, I could make you a friend of my colony. This would stop
                     * the warriors, workers, and queens of my colony from thinking you are an intruder,
                     * thus they would not attack you. In addition, as a friend of my colony I will process
                     * zoogi fungus into powder of translocation for you.<BR><BR>
                     * 
                     * To become a friend of my colony, I ask that you complete a couple tasks for me. These
                     * are the same tasks I will ask of you when you wish me to process zoogi fungus,
                     * by the way.<BR><BR>
                     * 
                     * First, I would like for you to eliminate some infiltrators from the other solen colony.
                     * They are spying on my colony, and I fear for the safety of my people. They must
                     * be slain.<BR><BR>
                     * 
                     * After that, I must ask that you gather some water for me. Our water supplies are
                     * inadequate, so we must try to supplement our reserve using water vats here in our
                     * lair.<BR><BR>
                     * 
                     * Will you accept my offer?
                     */
                    return 1054082;
                }
            }
        }

        public override TimeSpan RestartDelay { get { return TimeSpan.Zero; } }
        public override bool IsTutorial { get { return false; } }

        public override int Picture { get { return 0x15C9; } }

        private bool m_RedSolen;

        public bool RedSolen { get { return m_RedSolen; } }

        public SolenMatriarchQuest(PlayerMobile from, bool redSolen)
            : base(from)
        {
            m_RedSolen = redSolen;
        }

        // Serialization
        public SolenMatriarchQuest()
        {
        }

        public override void ChildDeserialize(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            m_RedSolen = reader.ReadBool();
        }

        public override void ChildSerialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)0); // version

            writer.Write((bool)m_RedSolen);
        }

        public override void Accept()
        {
            base.Accept();

            AddConversation(new AcceptConversation());
        }

        public static bool IsFriend(PlayerMobile player, bool redSolen)
        {
            if (redSolen)
                return player.SolenFriendship == SolenFriendship.Red;
            else
                return player.SolenFriendship == SolenFriendship.Black;
        }

        public static bool GiveRewardTo(PlayerMobile player)
        {
            Gold gold = new Gold(Utility.RandomMinMax(250, 350));

            if (player.PlaceInBackpack(gold))
            {
                player.SendLocalizedMessage(1054076); // You have been given some gold.
                return true;
            }
            else
            {
                gold.Delete();
                return false;
            }
        }
    }

    #region Questing Objectives

    public class KillInfiltratorsObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                // Kill 7 black/red solen infiltrators.
                return ((SolenMatriarchQuest)System).RedSolen ? 1054086 : 1054085;
            }
        }

        public override int MaxProgress { get { return 7; } }

        public KillInfiltratorsObjective()
        {
        }

        public override void RenderProgress(BaseQuestGump gump)
        {
            if (!Completed)
            {
                // Black/Red Solen Infiltrators killed:
                gump.AddHtmlLocalized(70, 260, 270, 100, ((SolenMatriarchQuest)System).RedSolen ? 1054088 : 1054087, BaseQuestGump.Blue, false, false);
                gump.AddLabel(70, 280, 0x64, CurProgress.ToString());
                gump.AddLabel(100, 280, 0x64, "/");
                gump.AddLabel(130, 280, 0x64, MaxProgress.ToString());
            }
            else
            {
                base.RenderProgress(gump);
            }
        }

        public override bool IgnoreYoungProtection(Mobile from)
        {
            if (Completed)
                return false;

            bool redSolen = ((SolenMatriarchQuest)System).RedSolen;

            if (redSolen)
                return from is BlackSolenInfiltratorWarrior || from is BlackSolenInfiltratorQueen;
            else
                return from is RedSolenInfiltratorWarrior || from is RedSolenInfiltratorQueen;
        }

        public override void OnKill(BaseCreature creature, Container corpse)
        {
            bool redSolen = ((SolenMatriarchQuest)System).RedSolen;

            if (redSolen)
            {
                if (creature is BlackSolenInfiltratorWarrior || creature is BlackSolenInfiltratorQueen)
                    CurProgress++;
            }
            else
            {
                if (creature is RedSolenInfiltratorWarrior || creature is RedSolenInfiltratorQueen)
                    CurProgress++;
            }
        }

        public override void OnComplete()
        {
            System.AddObjective(new ReturnAfterKillsObjective());
        }
    }

    public class ReturnAfterKillsObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* You've completed your task of slaying solen infiltrators. Return to the
                 * Matriarch who gave you this task.
                 */
                return 1054090;
            }
        }

        public ReturnAfterKillsObjective()
        {
        }

        public override void OnComplete()
        {
            System.AddConversation(new GatherWaterConversation());
        }
    }

    public class GatherWaterObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                // Gather 8 gallons of water for the water vats of the solen ant lair.
                return 1054092;
            }
        }

        public override int MaxProgress { get { return 40; } }

        public GatherWaterObjective()
        {
        }

        public override void RenderProgress(BaseQuestGump gump)
        {
            if (!Completed)
            {
                gump.AddHtmlLocalized(70, 260, 270, 100, 1054093, BaseQuestGump.Blue, false, false); // Gallons of Water gathered:
                gump.AddLabel(70, 280, 0x64, (CurProgress / 5).ToString());
                gump.AddLabel(100, 280, 0x64, "/");
                gump.AddLabel(130, 280, 0x64, (MaxProgress / 5).ToString());
            }
            else
            {
                base.RenderProgress(gump);
            }
        }

        public override void OnComplete()
        {
            System.AddObjective(new ReturnAfterWaterObjective());
        }
    }

    public class ReturnAfterWaterObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                // You've completed your task of gathering water. Return to the Matriarch who gave you this task.
                return 1054095;
            }
        }

        public ReturnAfterWaterObjective()
        {
        }

        public override void OnComplete()
        {
            PlayerMobile player = System.From;
            bool redSolen = ((SolenMatriarchQuest)System).RedSolen;

            bool friend = SolenMatriarchQuest.IsFriend(player, redSolen);

            System.AddConversation(new ProcessFungiConversation(friend));

            if (redSolen)
                player.SolenFriendship = SolenFriendship.Red;
            else
                player.SolenFriendship = SolenFriendship.Black;
        }
    }

    public class ProcessFungiObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                // Give the Solen Matriarch a stack of zoogi fungus to process into powder of translocation.
                return 1054098;
            }
        }

        public ProcessFungiObjective()
        {
        }

        public override void OnComplete()
        {
            if (SolenMatriarchQuest.GiveRewardTo(System.From))
            {
                System.Complete();
            }
            else
            {
                System.AddConversation(new FullBackpackConversation(true));
            }
        }
    }

    public class GetRewardObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                // Return to the solen matriarch for your reward.
                return 1054149;
            }
        }

        public GetRewardObjective()
        {
        }

        public override void OnComplete()
        {
            System.AddConversation(new EndConversation());
        }
    }

    #endregion

    #region Mobile Conversation

    public class DontOfferConversation : QuestConversation
    {
        private bool m_Friend;

        public override object Message
        {
            get
            {
                if (m_Friend)
                {
                    /* <I>The Solen Matriarch smiles as you greet her.</I><BR><BR>
                     * 
                     * It is good to see you again. I would offer to process some zoogi fungus for you,
                     * but you seem to be busy with another task at the moment. Perhaps you should
                     * finish whatever is occupying your attention at the moment and return to me once
                     * you're done.
                     */
                    return 1054081;
                }
                else
                {
                    /* <I>The Solen Matriarch smiles as she eats the seed you offered.</I><BR><BR>
                     * 
                     * Thank you for that seed. It was quite delicious.  <BR><BR>
                     * 
                     * I would offer to make you a friend of my colony, but you seem to be busy with
                     * another task at the moment. Perhaps you should finish whatever is occupying
                     * your attention at the moment and return to me once you're done.
                     */
                    return 1054079;
                }
            }
        }

        public override bool Logged { get { return false; } }

        public DontOfferConversation(bool friend)
        {
            m_Friend = friend;
        }

        public DontOfferConversation()
        {
        }

        public override void ChildDeserialize(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            m_Friend = reader.ReadBool();
        }

        public override void ChildSerialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)0); // version

            writer.Write((bool)m_Friend);
        }
    }

    public class AcceptConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>The Solen Matriarch looks pleased that you've accepted.</I><BR><BR>
                 * 
                 * Very good. Please start by hunting some infiltrators from the other solen
                 * colony and eliminating them. Slay 7 of them and then return to me.<BR><BR>
                 * 
                 * Farewell for now and good hunting.
                 */
                return 1054084;
            }
        }

        public AcceptConversation()
        {
        }

        public override void OnRead()
        {
            System.AddObjective(new KillInfiltratorsObjective());
        }
    }

    public class DuringKillInfiltratorsConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>The Solen Matriarch looks up as you approach.</I><BR><BR>
                 * 
                 * You're back, but you have not yet eliminated 7 infiltrators from the enemy
                 * colony. Return when you have completed this task.<BR><BR>
                 * 
                 * Carry on. I'll be waiting for your return.
                 */
                return 1054089;
            }
        }

        public override bool Logged { get { return false; } }

        public DuringKillInfiltratorsConversation()
        {
        }
    }

    public class GatherWaterConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>The Solen Matriarch nods favorably as you approach her.</I><BR><BR>
                 * 
                 * Marvelous! I'm impressed at your ability to hunt and kill enemies for me.
                 * My colony is thankful.<BR><BR>
                 * 
                 * Now I must ask that you gather some water for me. A standard pitcher of water
                 * holds approximately one gallon. Please decant 8 gallons of fresh water
                 * into our water vats.<BR><BR>
                 * 
                 * Farewell for now.
                 */
                return 1054091;
            }
        }

        public GatherWaterConversation()
        {
        }

        public override void OnRead()
        {
            System.AddObjective(new GatherWaterObjective());
        }
    }

    public class DuringWaterGatheringConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>The Solen Matriarch looks up as you approach.</I><BR><BR>
                 * 
                 * You're back, but you have not yet gathered 8 gallons of water. Return when
                 * you have completed this task.<BR><BR>
                 * 
                 * Carry on. I'll be waiting for your return.
                 */
                return 1054094;
            }
        }

        public override bool Logged { get { return false; } }

        public DuringWaterGatheringConversation()
        {
        }
    }

    public class ProcessFungiConversation : QuestConversation
    {
        private bool m_Friend;

        public override object Message
        {
            get
            {
                if (m_Friend)
                {
                    /* <I>The Solen Matriarch listens as you report the completion of your
                     * tasks to her.</I><BR><BR>
                     * 
                     * I give you my thanks for your help, and I will gladly process some zoogi
                     * fungus into powder of translocation for you. Two of the zoogi fungi are
                     * required for each measure of the powder. I will process up to 200 zoogi fungi
                     * into 100 measures of powder of translocation.<BR><BR>
                     * 
                     * I will also give you some gold for assisting me and my colony, but first let's
                     * take care of your zoogi fungus.
                     */
                    return 1054097;
                }
                else
                {
                    /* <I>The Solen Matriarch listens as you report the completion of your
                     * tasks to her.</I><BR><BR>
                     * 
                     * I give you my thanks for your help, and I will gladly make you a friend of my
                     * solen colony. My warriors, workers, and queens will not longer look at you
                     * as an intruder and attack you when you enter our lair.<BR><BR>
                     * 
                     * I will also process some zoogi fungus into powder of translocation for you.
                     * Two of the zoogi fungi are required for each measure of the powder. I will
                     * process up to 200 zoogi fungi into 100 measures of powder of translocation.<BR><BR>
                     * 
                     * I will also give you some gold for assisting me and my colony, but first let's
                     * take care of your zoogi fungus.
                     */
                    return 1054096;
                }
            }
        }

        public ProcessFungiConversation(bool friend)
        {
            m_Friend = friend;
        }

        public override void OnRead()
        {
            System.AddObjective(new ProcessFungiObjective());
        }

        public ProcessFungiConversation()
        {
        }

        public override void ChildDeserialize(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            m_Friend = reader.ReadBool();
        }

        public override void ChildSerialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)0); // version

            writer.Write((bool)m_Friend);
        }
    }

    public class DuringFungiProcessConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>The Solen Matriarch smiles as you greet her.</I><BR><BR>
                 * 
                 * I will gladly process some zoogi fungus into powder of translocation for you.
                 * Two of the zoogi fungi are required for each measure of the powder.
                 * I will process up to 200 zoogi fungi into 100 measures of powder of translocation.
                 */
                return 1054099;
            }
        }

        public override bool Logged { get { return false; } }

        public DuringFungiProcessConversation()
        {
        }
    }

    public class FullBackpackConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>The Solen Matriarch looks at you with a smile.</I><BR><BR>
                 * 
                 * While I'd like to finish conducting our business, it seems that you're a
                 * bit overloaded with equipment at the moment.<BR><BR>
                 * 
                 * Perhaps you should free some room in your backpack before we proceed.
                 */
                return 1054102;
            }
        }

        private bool m_Logged;

        public override bool Logged { get { return m_Logged; } }

        public FullBackpackConversation(bool logged)
        {
            m_Logged = logged;
        }

        public FullBackpackConversation()
        {
            m_Logged = true;
        }

        public override void OnRead()
        {
            if (m_Logged)
                System.AddObjective(new GetRewardObjective());
        }
    }

    public class EndConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>The Solen Matriarch smiles as you greet her.</I><BR><BR>
                 * 
                 * Ah good, you've returned. I will conclude our business by giving you
                 * gold I owe you for aiding me.
                 */
                return 1054101;
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

    #region Quest Mobile Engine

    public abstract class BaseSolenMatriarch : BaseQuester
    {
        public abstract bool RedSolen { get; }

        public override bool DisallowAllMoves { get { return false; } }

        public BaseSolenMatriarch()
        {
            Name = "the solen matriarch";

            Body = 0x328;

            if (!RedSolen)
                Hue = 0x44E;

            SpeechHue = 0;
        }

        public override int GetIdleSound()
        {
            return 0x10D;
        }

        public override bool CanTalkTo(PlayerMobile to)
        {
            if (SolenMatriarchQuest.IsFriend(to, this.RedSolen))
                return true;

            SolenMatriarchQuest qs = to.Quest as SolenMatriarchQuest;

            return qs != null && qs.RedSolen == this.RedSolen;
        }

        public override void OnTalk(PlayerMobile player, bool contextMenu)
        {
            this.Direction = GetDirectionTo(player);

            SolenMatriarchQuest qs = player.Quest as SolenMatriarchQuest;

            if (qs != null && qs.RedSolen == this.RedSolen)
            {
                if (qs.IsObjectiveInProgress(typeof(KillInfiltratorsObjective)))
                {
                    qs.AddConversation(new DuringKillInfiltratorsConversation());
                }
                else
                {
                    QuestObjective obj = qs.FindObjective(typeof(ReturnAfterKillsObjective));

                    if (obj != null && !obj.Completed)
                    {
                        obj.Complete();
                    }
                    else if (qs.IsObjectiveInProgress(typeof(GatherWaterObjective)))
                    {
                        qs.AddConversation(new DuringWaterGatheringConversation());
                    }
                    else
                    {
                        obj = qs.FindObjective(typeof(ReturnAfterWaterObjective));

                        if (obj != null && !obj.Completed)
                        {
                            obj.Complete();
                        }
                        else if (qs.IsObjectiveInProgress(typeof(ProcessFungiObjective)))
                        {
                            qs.AddConversation(new DuringFungiProcessConversation());
                        }
                        else
                        {
                            obj = qs.FindObjective(typeof(GetRewardObjective));

                            if (obj != null && !obj.Completed)
                            {
                                if (SolenMatriarchQuest.GiveRewardTo(player))
                                {
                                    obj.Complete();
                                }
                                else
                                {
                                    qs.AddConversation(new FullBackpackConversation(false));
                                }
                            }
                        }
                    }
                }
            }
            else if (SolenMatriarchQuest.IsFriend(player, this.RedSolen))
            {
                QuestSystem newQuest = new SolenMatriarchQuest(player, this.RedSolen);

                if (player.Quest == null && QuestSystem.CanOfferQuest(player, typeof(SolenMatriarchQuest)))
                {
                    newQuest.SendOffer();
                }
                else
                {
                    newQuest.AddConversation(new DontOfferConversation(true));
                }
            }
        }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            PlayerMobile player = from as PlayerMobile;

            if (player != null)
            {
                if (dropped is Seed)
                {
                    SolenMatriarchQuest qs = player.Quest as SolenMatriarchQuest;

                    if (qs != null && qs.RedSolen == this.RedSolen)
                    {
                        SayTo(player, 1054080); // Thank you for that plant seed. Those have such wonderful flavor.
                    }
                    else
                    {
                        QuestSystem newQuest = new SolenMatriarchQuest(player, this.RedSolen);

                        if (player.Quest == null && QuestSystem.CanOfferQuest(player, typeof(SolenMatriarchQuest)))
                        {
                            newQuest.SendOffer();
                        }
                        else
                        {
                            newQuest.AddConversation(new DontOfferConversation(SolenMatriarchQuest.IsFriend(player, this.RedSolen)));
                        }
                    }

                    dropped.Delete();
                    return true;
                }
                else if (dropped is ZoogiFungus)
                {
                    OnGivenFungi(player, (ZoogiFungus)dropped);

                    return dropped.Deleted;
                }
            }

            return base.OnDragDrop(from, dropped);
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);

            if (from.Alive)
            {
                PlayerMobile pm = from as PlayerMobile;

                if (pm != null)
                {
                    SolenMatriarchQuest qs = pm.Quest as SolenMatriarchQuest;

                    if (qs != null && qs.RedSolen == this.RedSolen)
                    {
                        if (qs.IsObjectiveInProgress(typeof(ProcessFungiObjective)))
                        {
                            list.Add(new ProcessZoogiFungusEntry(this, pm));
                        }
                    }
                }
            }
        }

        private class ProcessZoogiFungusEntry : ContextMenuEntry
        {
            private BaseSolenMatriarch m_Matriarch;
            private PlayerMobile m_From;

            public ProcessZoogiFungusEntry(BaseSolenMatriarch matriarch, PlayerMobile from)
                : base(6184)
            {
                m_Matriarch = matriarch;
                m_From = from;
            }

            public override void OnClick()
            {
                if (m_From.Alive)
                    m_From.Target = new ProcessFungiTarget(m_Matriarch, m_From);
            }
        }

        private class ProcessFungiTarget : Target
        {
            private BaseSolenMatriarch m_Matriarch;
            private PlayerMobile m_From;

            public ProcessFungiTarget(BaseSolenMatriarch matriarch, PlayerMobile from)
                : base(-1, false, TargetFlags.None)
            {
                m_Matriarch = matriarch;
                m_From = from;
            }

            protected override void OnTargetCancel(Mobile from, TargetCancelType cancelType)
            {
                from.SendLocalizedMessage(1042021, "", 0x59); // Cancelled.
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is ZoogiFungus)
                {
                    ZoogiFungus fungus = (ZoogiFungus)targeted;

                    if (fungus.IsChildOf(m_From.Backpack))
                        m_Matriarch.OnGivenFungi(m_From, (ZoogiFungus)targeted);
                    else
                        m_From.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
                }
            }
        }

        public void OnGivenFungi(PlayerMobile player, ZoogiFungus fungi)
        {
            this.Direction = GetDirectionTo(player);

            SolenMatriarchQuest qs = player.Quest as SolenMatriarchQuest;

            if (qs != null && qs.RedSolen == this.RedSolen)
            {
                QuestObjective obj = qs.FindObjective(typeof(ProcessFungiObjective));

                if (obj != null && !obj.Completed)
                {
                    int amount = fungi.Amount / 2;

                    if (amount > 100)
                        amount = 100;

                    if (amount > 0)
                    {
                        if (amount * 2 >= fungi.Amount)
                            fungi.Delete();
                        else
                            fungi.Amount -= amount * 2;

                        PowderOfTranslocation powder = new PowderOfTranslocation(amount);
                        player.AddToBackpack(powder);

                        player.SendLocalizedMessage(1054100); // You receive some powder of translocation.

                        obj.Complete();
                    }
                }
            }
        }

        public BaseSolenMatriarch(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }

    #endregion
}

namespace Server.Mobiles
{
    public class SolenHelper
    {
        public static void PackPicnicBasket(BaseCreature solen)
        {
            if (1 > Utility.Random(100))
            {
                PicnicBasket basket = new PicnicBasket();

                basket.DropItem(new BeverageBottle(BeverageType.Wine));
                basket.DropItem(new CheeseWedge());

                solen.PackItem(basket);
            }
        }

        public static bool CheckRedFriendship(Mobile m)
        {
            if (m is BaseCreature)
            {
                BaseCreature bc = (BaseCreature)m;

                if (bc.Controlled && bc.ControlMaster is PlayerMobile)
                    return CheckRedFriendship(bc.ControlMaster);
                else if (bc.Summoned && bc.SummonMaster is PlayerMobile)
                    return CheckRedFriendship(bc.SummonMaster);
            }

            PlayerMobile player = m as PlayerMobile;

            return player != null && player.SolenFriendship == SolenFriendship.Red;
        }

        public static bool CheckBlackFriendship(Mobile m)
        {
            if (m is BaseCreature)
            {
                BaseCreature bc = (BaseCreature)m;

                if (bc.Controlled && bc.ControlMaster is PlayerMobile)
                    return CheckBlackFriendship(bc.ControlMaster);
                else if (bc.Summoned && bc.SummonMaster is PlayerMobile)
                    return CheckBlackFriendship(bc.SummonMaster);
            }

            PlayerMobile player = m as PlayerMobile;

            return player != null && player.SolenFriendship == SolenFriendship.Black;
        }

        public static void OnRedDamage(Mobile from)
        {
            if (from is BaseCreature)
            {
                BaseCreature bc = (BaseCreature)from;

                if (bc.Controlled && bc.ControlMaster is PlayerMobile)
                    OnRedDamage(bc.ControlMaster);
                else if (bc.Summoned && bc.SummonMaster is PlayerMobile)
                    OnRedDamage(bc.SummonMaster);
            }

            PlayerMobile player = from as PlayerMobile;

            if (player != null && player.SolenFriendship == SolenFriendship.Red)
            {
                player.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1054103); // The solen revoke their friendship. You will now be considered an intruder.

                player.SolenFriendship = SolenFriendship.None;
            }
        }

        public static void OnBlackDamage(Mobile from)
        {
            if (from is BaseCreature)
            {
                BaseCreature bc = (BaseCreature)from;

                if (bc.Controlled && bc.ControlMaster is PlayerMobile)
                    OnBlackDamage(bc.ControlMaster);
                else if (bc.Summoned && bc.SummonMaster is PlayerMobile)
                    OnBlackDamage(bc.SummonMaster);
            }

            PlayerMobile player = from as PlayerMobile;

            if (player != null && player.SolenFriendship == SolenFriendship.Black)
            {
                player.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1054103); // The solen revoke their friendship. You will now be considered an intruder.

                player.SolenFriendship = SolenFriendship.None;
            }
        }
    }
}