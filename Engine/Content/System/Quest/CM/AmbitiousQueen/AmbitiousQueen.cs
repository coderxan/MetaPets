using System;

using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests.Ambitious
{
    public class AmbitiousQueenQuest : QuestSystem
    {
        private static Type[] m_TypeReferenceTable = new Type[]
			{
				typeof( Ambitious.DontOfferConversation ),
				typeof( Ambitious.AcceptConversation ),
				typeof( Ambitious.DuringKillQueensConversation ),
				typeof( Ambitious.GatherFungiConversation ),
				typeof( Ambitious.DuringFungiGatheringConversation ),
				typeof( Ambitious.EndConversation ),
				typeof( Ambitious.FullBackpackConversation ),
				typeof( Ambitious.End2Conversation ),
				typeof( Ambitious.KillQueensObjective ),
				typeof( Ambitious.ReturnAfterKillsObjective ),
				typeof( Ambitious.GatherFungiObjective ),
				typeof( Ambitious.GetRewardObjective )
			};

        public override Type[] TypeReferenceTable { get { return m_TypeReferenceTable; } }

        public override object Name
        {
            get
            {
                // Ambitious Solen Queen Quest
                return 1054146;
            }
        }

        public override object OfferMessage
        {
            get
            {
                /* <I>The Solen queen considers you eagerly for a moment then says,</I><BR><BR>
                 * 
                 * Yes. Yes, I think you could be of use. Normally, of course, I would handle
                 * these things on my own, but these are busy times. Much to do, much to do.
                 * And besides, if I am to one day become the Matriarch, then it will be good to
                 * have experience trusting others to carry out various tasks for me. Yes.<BR><BR>
                 * 
                 * That is my plan, you see - I will become the next Matriarch. Our current
                 * Matriarch is fine and all, but she won't be around forever. And when she steps
                 * down, I intend to be the next in line. Ruling others is my destiny, you see.<BR><BR>
                 * 
                 * What I ask of you is quite simple. First, I need you to remove some of the
                 * - well - competition, I suppose. Though I dare say most are hardly competent to
                 * live up to such a title. I'm referring to the other queens of this colony,
                 * of course. My dear sisters, so to speak. If you could remove 5 of them, I would
                 * be most pleased. *sighs* By remove, I mean kill them. Don't make that face
                 * at me - this is how things work in a proper society, and ours has been more proper
                 * than most since the dawn of time. It's them or me, and whenever I give it
                 * any thought, I'm quite sure I'd prefer it to be them.<BR><BR>
                 * 
                 * I also need you to gather some zoogi fungus for me - 50 should do the trick.<BR><BR>
                 * 
                 * Will you accept my offer?
                 */
                return 1054060;
            }
        }

        public override TimeSpan RestartDelay { get { return TimeSpan.Zero; } }
        public override bool IsTutorial { get { return false; } }

        public override int Picture { get { return 0x15C9; } }

        private bool m_RedSolen;

        public bool RedSolen { get { return m_RedSolen; } }

        public AmbitiousQueenQuest(PlayerMobile from, bool redSolen)
            : base(from)
        {
            m_RedSolen = redSolen;
        }

        // Serialization
        public AmbitiousQueenQuest()
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

        public static void GiveRewardTo(PlayerMobile player, ref bool bagOfSending, ref bool powderOfTranslocation, ref bool gold)
        {
            if (bagOfSending)
            {
                Item reward = new BagOfSending();

                if (player.PlaceInBackpack(reward))
                {
                    player.SendLocalizedMessage(1054074, "", 0x59); // You have been given a bag of sending.
                    bagOfSending = false;
                }
                else
                {
                    reward.Delete();
                }
            }

            if (powderOfTranslocation)
            {
                Item reward = new PowderOfTranslocation(Utility.RandomMinMax(10, 12));

                if (player.PlaceInBackpack(reward))
                {
                    player.SendLocalizedMessage(1054075, "", 0x59); // You have been given some powder of translocation.
                    powderOfTranslocation = false;
                }
                else
                {
                    reward.Delete();
                }
            }

            if (gold)
            {
                Item reward = new Gold(Utility.RandomMinMax(250, 350));

                if (player.PlaceInBackpack(reward))
                {
                    player.SendLocalizedMessage(1054076, "", 0x59); // You have been given some gold.
                    gold = false;
                }
                else
                {
                    reward.Delete();
                }
            }
        }
    }

    #region Questing Objectives

    public class KillQueensObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                // Kill 5 red/black solen queens.
                return ((AmbitiousQueenQuest)System).RedSolen ? 1054062 : 1054063;
            }
        }

        public override int MaxProgress { get { return 5; } }

        public KillQueensObjective()
        {
        }

        public override void RenderProgress(BaseQuestGump gump)
        {
            if (!Completed)
            {
                // Red/Black Solen Queens killed:
                gump.AddHtmlLocalized(70, 260, 270, 100, ((AmbitiousQueenQuest)System).RedSolen ? 1054064 : 1054065, BaseQuestGump.Blue, false, false);
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

            bool redSolen = ((AmbitiousQueenQuest)System).RedSolen;

            if (redSolen)
                return from is RedSolenQueen;
            else
                return from is BlackSolenQueen;
        }

        public override void OnKill(BaseCreature creature, Container corpse)
        {
            bool redSolen = ((AmbitiousQueenQuest)System).RedSolen;

            if (redSolen)
            {
                if (creature is RedSolenQueen)
                    CurProgress++;
            }
            else
            {
                if (creature is BlackSolenQueen)
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
                /* You've completed your task of slaying solen queens. Return to
                 * the ambitious queen who asked for your help.
                 */
                return 1054067;
            }
        }

        public ReturnAfterKillsObjective()
        {
        }

        public override void OnComplete()
        {
            System.AddConversation(new GatherFungiConversation());
        }
    }

    public class GatherFungiObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                /* Gather zoogi fungus until you have 50 of them, then give them
                 * to the ambitious queen you are helping.
                 */
                return 1054069;
            }
        }

        public GatherFungiObjective()
        {
        }

        public override void OnComplete()
        {
            System.AddConversation(new EndConversation());
        }
    }

    public class GetRewardObjective : QuestObjective
    {
        public override object Message
        {
            get
            {
                // Return to the ambitious solen queen for your reward.
                return 1054148;
            }
        }

        private bool m_BagOfSending;
        private bool m_PowderOfTranslocation;
        private bool m_Gold;

        public bool BagOfSending { get { return m_BagOfSending; } set { m_BagOfSending = value; } }
        public bool PowderOfTranslocation { get { return m_PowderOfTranslocation; } set { m_PowderOfTranslocation = value; } }
        public bool Gold { get { return m_Gold; } set { m_Gold = value; } }

        public GetRewardObjective(bool bagOfSending, bool powderOfTranslocation, bool gold)
        {
            m_BagOfSending = bagOfSending;
            m_PowderOfTranslocation = powderOfTranslocation;
            m_Gold = gold;
        }

        public GetRewardObjective()
        {
        }

        public override void OnComplete()
        {
            System.AddConversation(new End2Conversation());
        }

        public override void ChildDeserialize(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            m_BagOfSending = reader.ReadBool();
            m_PowderOfTranslocation = reader.ReadBool();
            m_Gold = reader.ReadBool();
        }

        public override void ChildSerialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)0); // version

            writer.Write((bool)m_BagOfSending);
            writer.Write((bool)m_PowderOfTranslocation);
            writer.Write((bool)m_Gold);
        }
    }

    #endregion

    #region Mobile Conversation

    public class DontOfferConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>The Solen queen considers you for a moment then says,</I><BR><BR>
                 * 
                 * Hmmm... I could perhaps benefit from your assistance, but you seem to be
                 * busy with another task at the moment. Return to me when you complete whatever
                 * it is that you're working on and maybe I can still put you to good use.
                 */
                return 1054059;
            }
        }

        public override bool Logged { get { return false; } }

        public DontOfferConversation()
        {
        }
    }

    public class AcceptConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>The Solen queen smiles as you decide to help her.</I><BR><BR>
                 * 
                 * Excellent. We'll worry about the zoogi fungus later - start by eliminating
                 * 5 queens from my colony.<BR><BR>That part's important, by the way; they must
                 * be queens from my colony. Killing queens from the other solen colony does
                 * little to help me become Matriarch of this colony and will not count
                 * toward your task.<BR><BR>
                 * 
                 * Oh, and none of those nasty infiltrator queens either. They perform a necessary
                 * duty, I suppose, spying on the other colony. I fail to see why that couldn't be
                 * left totally to the warriors, though. Nevertheless, they do not count as well.<BR><BR>
                 * 
                 * Very well. Carry on. I'll be waiting for your return.
                 */
                return 1054061;
            }
        }

        public AcceptConversation()
        {
        }

        public override void OnRead()
        {
            System.AddObjective(new KillQueensObjective());
        }
    }

    public class DuringKillQueensConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>The Solen queen looks up as you approach.</I><BR><BR>
                 * 
                 * You're back, but you have not yet eliminated 5 queens from my colony.
                 * Return when you have completed this task.<BR><BR>
                 * 
                 * Remember, by the way, that queens from the other solen colony and
                 * infiltrator queens do not count toward your task.<BR><BR>
                 * 
                 * Very well. Carry on. I'll be waiting for your return.
                 */
                return 1054066;
            }
        }

        public override bool Logged { get { return false; } }

        public DuringKillQueensConversation()
        {
        }
    }

    public class GatherFungiConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>The Solen queen looks pleased to see you.</I><BR><BR>
                 * 
                 * Splendid! You've done quite well in reducing my competition to become
                 * the next Matriarch. Now I must ask that you gather some zoogi fungus for me.
                 * I must practice processing it into powder of translocation.<BR><BR>
                 * 
                 * I believe the amount we agreed upon earlier was 50. Please return when
                 * you have that amount and then give them to me.<BR><BR>
                 * 
                 * Farewell for now.
                 */
                return 1054068;
            }
        }

        public GatherFungiConversation()
        {
        }

        public override void OnRead()
        {
            System.AddObjective(new GatherFungiObjective());
        }
    }

    public class DuringFungiGatheringConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>The Solen queen looks up as you approach.</I><BR><BR>
                 * 
                 * Do you have the zoogi fungus?<BR><BR>
                 * 
                 * If so, give them to me. Otherwise, go gather some and then return to me.
                 */
                return 1054070;
            }
        }

        public override bool Logged { get { return false; } }

        public DuringFungiGatheringConversation()
        {
        }
    }

    public class EndConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>The Solen queen smiles as she takes the zoogi fungus from you.</I><BR><BR>
                 * 
                 * Wonderful! I greatly appreciate your help with these tasks. My plans are beginning
                 * to take shape ensuring that I will be the next Matriarch. But there is still
                 * much to be done until then.<BR><BR>
                 * 
                 * You've done what I've asked of you and for that I thank you. Please accept this
                 * bag of sending and some powder of translocation as a reward. Oh, and I suppose
                 * I should give you some gold as well. Yes, yes. Of course.
                 */
                return 1054073;
            }
        }

        public EndConversation()
        {
        }

        public override void OnRead()
        {
            bool bagOfSending = true;
            bool powderOfTranslocation = true;
            bool gold = true;

            AmbitiousQueenQuest.GiveRewardTo(System.From, ref bagOfSending, ref powderOfTranslocation, ref gold);

            if (!bagOfSending && !powderOfTranslocation && !gold)
            {
                System.Complete();
            }
            else
            {
                System.AddConversation(new FullBackpackConversation(true, bagOfSending, powderOfTranslocation, gold));
            }
        }
    }

    public class FullBackpackConversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>The Solen queen looks at you with a smile.</I><BR><BR>
                 * 
                 * While I'd like to finish conducting our business, it seems that you're a
                 * bit overloaded with equipment at the moment.<BR><BR>
                 * 
                 * Perhaps you should free some room in your backpack before we proceed.
                 */
                return 1054077;
            }
        }

        private bool m_Logged;
        private bool m_BagOfSending;
        private bool m_PowderOfTranslocation;
        private bool m_Gold;

        public override bool Logged { get { return m_Logged; } }

        public FullBackpackConversation(bool logged, bool bagOfSending, bool powderOfTranslocation, bool gold)
        {
            m_Logged = logged;

            m_BagOfSending = bagOfSending;
            m_PowderOfTranslocation = powderOfTranslocation;
            m_Gold = gold;
        }

        public FullBackpackConversation()
        {
            m_Logged = true;
        }

        public override void OnRead()
        {
            if (m_Logged)
                System.AddObjective(new GetRewardObjective(m_BagOfSending, m_PowderOfTranslocation, m_Gold));
        }

        public override void ChildDeserialize(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            m_BagOfSending = reader.ReadBool();
            m_PowderOfTranslocation = reader.ReadBool();
            m_Gold = reader.ReadBool();
        }

        public override void ChildSerialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)0); // version

            writer.Write((bool)m_BagOfSending);
            writer.Write((bool)m_PowderOfTranslocation);
            writer.Write((bool)m_Gold);
        }
    }

    public class End2Conversation : QuestConversation
    {
        public override object Message
        {
            get
            {
                /* <I>The Solen queen looks up as you approach.</I><BR><BR>
                 * 
                 * Ah good, you've returned. I will conclude our business by giving you any
                 * remaining rewards I owe you for aiding me.
                 */
                return 1054078;
            }
        }

        public End2Conversation()
        {
        }

        public override void OnRead()
        {
            System.Complete();
        }
    }

    #endregion

    #region Quest Mobile Engine

    public abstract class BaseAmbitiousSolenQueen : BaseQuester
    {
        public abstract bool RedSolen { get; }

        public override bool DisallowAllMoves { get { return false; } }

        public BaseAmbitiousSolenQueen()
        {
        }

        public override void InitBody()
        {
            Name = "an ambitious solen queen";

            Body = 0x30F;

            if (!RedSolen)
                Hue = 0x453;

            SpeechHue = 0;
        }

        public override int GetIdleSound()
        {
            return 0x10D;
        }

        public override void OnTalk(PlayerMobile player, bool contextMenu)
        {
            this.Direction = GetDirectionTo(player);

            AmbitiousQueenQuest qs = player.Quest as AmbitiousQueenQuest;

            if (qs != null && qs.RedSolen == this.RedSolen)
            {
                if (qs.IsObjectiveInProgress(typeof(KillQueensObjective)))
                {
                    qs.AddConversation(new DuringKillQueensConversation());
                }
                else
                {
                    QuestObjective obj = qs.FindObjective(typeof(ReturnAfterKillsObjective));

                    if (obj != null && !obj.Completed)
                    {
                        obj.Complete();
                    }
                    else if (qs.IsObjectiveInProgress(typeof(GatherFungiObjective)))
                    {
                        qs.AddConversation(new DuringFungiGatheringConversation());
                    }
                    else
                    {
                        GetRewardObjective lastObj = qs.FindObjective(typeof(GetRewardObjective)) as GetRewardObjective;

                        if (lastObj != null && !lastObj.Completed)
                        {
                            bool bagOfSending = lastObj.BagOfSending;
                            bool powderOfTranslocation = lastObj.PowderOfTranslocation;
                            bool gold = lastObj.Gold;

                            AmbitiousQueenQuest.GiveRewardTo(player, ref bagOfSending, ref powderOfTranslocation, ref gold);

                            lastObj.BagOfSending = bagOfSending;
                            lastObj.PowderOfTranslocation = powderOfTranslocation;
                            lastObj.Gold = gold;

                            if (!bagOfSending && !powderOfTranslocation && !gold)
                            {
                                lastObj.Complete();
                            }
                            else
                            {
                                qs.AddConversation(new FullBackpackConversation(false, lastObj.BagOfSending, lastObj.PowderOfTranslocation, lastObj.Gold));
                            }
                        }
                    }
                }
            }
            else
            {
                QuestSystem newQuest = new AmbitiousQueenQuest(player, this.RedSolen);

                if (player.Quest == null && QuestSystem.CanOfferQuest(player, typeof(AmbitiousQueenQuest)))
                {
                    newQuest.SendOffer();
                }
                else
                {
                    newQuest.AddConversation(new DontOfferConversation());
                }
            }
        }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            this.Direction = GetDirectionTo(from);

            PlayerMobile player = from as PlayerMobile;

            if (player != null)
            {
                AmbitiousQueenQuest qs = player.Quest as AmbitiousQueenQuest;

                if (qs != null && qs.RedSolen == this.RedSolen)
                {
                    QuestObjective obj = qs.FindObjective(typeof(GatherFungiObjective));

                    if (obj != null && !obj.Completed)
                    {
                        if (dropped is ZoogiFungus)
                        {
                            ZoogiFungus fungi = (ZoogiFungus)dropped;

                            if (fungi.Amount >= 50)
                            {
                                obj.Complete();

                                fungi.Amount -= 50;

                                if (fungi.Amount == 0)
                                {
                                    fungi.Delete();
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                SayTo(player, 1054072); // Our arrangement was for 50 of the zoogi fungus. Please return to me when you have that amount.
                                return false;
                            }
                        }
                    }
                }
            }

            return base.OnDragDrop(from, dropped);
        }

        public BaseAmbitiousSolenQueen(Serial serial)
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