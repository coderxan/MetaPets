using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

using Server;
using Server.ContextMenus;
using Server.Engines.Quests;
using Server.Engines.Quests.Necro;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Regions;

namespace Server
{
    public class MessageHelper
    {
        public static void SendLocalizedMessageTo(Item from, Mobile to, int number, int hue)
        {
            SendLocalizedMessageTo(from, to, number, "", hue);
        }

        public static void SendLocalizedMessageTo(Item from, Mobile to, int number, string args, int hue)
        {
            to.Send(new MessageLocalized(from.Serial, from.ItemID, MessageType.Regular, hue, 3, number, "", args));
        }

        public static void SendMessageTo(Item from, Mobile to, string text, int hue)
        {
            to.Send(new UnicodeMessage(from.Serial, from.ItemID, MessageType.Regular, hue, 3, "ENU", "", text));
        }
    }
}

namespace Server.Engines.Quests
{
    public delegate void QuestCallback();

    public abstract class QuestSystem
    {
        public static readonly Type[] QuestTypes = new Type[]
			{
				typeof( Doom.TheSummoningQuest ),
				typeof( Necro.DarkTidesQuest ),
				typeof( Haven.UzeraanTurmoilQuest ),
				typeof( Collector.CollectorQuest ),
				typeof( Hag.WitchApprenticeQuest ),
				typeof( Naturalist.StudyOfSolenQuest ),
				typeof( Matriarch.SolenMatriarchQuest ),
				typeof( Ambitious.AmbitiousQueenQuest ),
				typeof( Ninja.EminosUndertakingQuest ),
				typeof( Samurai.HaochisTrialsQuest ),
				typeof( Zento.TerribleHatchlingsQuest )
			};

        public abstract object Name { get; }
        public abstract object OfferMessage { get; }

        public abstract int Picture { get; }

        public abstract bool IsTutorial { get; }
        public abstract TimeSpan RestartDelay { get; }

        public abstract Type[] TypeReferenceTable { get; }

        private PlayerMobile m_From;
        private ArrayList m_Objectives;
        private ArrayList m_Conversations;

        public PlayerMobile From
        {
            get { return m_From; }
            set { m_From = value; }
        }

        public ArrayList Objectives
        {
            get { return m_Objectives; }
            set { m_Objectives = value; }
        }

        public ArrayList Conversations
        {
            get { return m_Conversations; }
            set { m_Conversations = value; }
        }

        private Timer m_Timer;

        public virtual void StartTimer()
        {
            if (m_Timer != null)
                return;

            m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(0.5), new TimerCallback(Slice));
        }

        public virtual void StopTimer()
        {
            if (m_Timer != null)
                m_Timer.Stop();

            m_Timer = null;
        }

        public virtual void Slice()
        {
            for (int i = m_Objectives.Count - 1; i >= 0; --i)
            {
                QuestObjective obj = (QuestObjective)m_Objectives[i];

                if (obj.GetTimerEvent())
                    obj.CheckProgress();
            }
        }

        public virtual void OnKill(BaseCreature creature, Container corpse)
        {
            for (int i = m_Objectives.Count - 1; i >= 0; --i)
            {
                QuestObjective obj = (QuestObjective)m_Objectives[i];

                if (obj.GetKillEvent(creature, corpse))
                    obj.OnKill(creature, corpse);
            }
        }

        public virtual bool IgnoreYoungProtection(Mobile from)
        {
            for (int i = m_Objectives.Count - 1; i >= 0; --i)
            {
                QuestObjective obj = (QuestObjective)m_Objectives[i];

                if (obj.IgnoreYoungProtection(from))
                    return true;
            }

            return false;
        }

        public QuestSystem(PlayerMobile from)
        {
            m_From = from;
            m_Objectives = new ArrayList();
            m_Conversations = new ArrayList();
        }

        public QuestSystem()
        {
        }

        public virtual void BaseDeserialize(GenericReader reader)
        {
            Type[] referenceTable = this.TypeReferenceTable;

            int version = reader.ReadEncodedInt();

            switch (version)
            {
                case 0:
                    {
                        int count = reader.ReadEncodedInt();

                        m_Objectives = new ArrayList(count);

                        for (int i = 0; i < count; ++i)
                        {
                            QuestObjective obj = QuestSerializer.DeserializeObjective(referenceTable, reader);

                            if (obj != null)
                            {
                                obj.System = this;
                                m_Objectives.Add(obj);
                            }
                        }

                        count = reader.ReadEncodedInt();

                        m_Conversations = new ArrayList(count);

                        for (int i = 0; i < count; ++i)
                        {
                            QuestConversation conv = QuestSerializer.DeserializeConversation(referenceTable, reader);

                            if (conv != null)
                            {
                                conv.System = this;
                                m_Conversations.Add(conv);
                            }
                        }

                        break;
                    }
            }

            ChildDeserialize(reader);
        }

        public virtual void ChildDeserialize(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();
        }

        public virtual void BaseSerialize(GenericWriter writer)
        {
            Type[] referenceTable = this.TypeReferenceTable;

            writer.WriteEncodedInt((int)0); // version

            writer.WriteEncodedInt((int)m_Objectives.Count);

            for (int i = 0; i < m_Objectives.Count; ++i)
                QuestSerializer.Serialize(referenceTable, (QuestObjective)m_Objectives[i], writer);

            writer.WriteEncodedInt((int)m_Conversations.Count);

            for (int i = 0; i < m_Conversations.Count; ++i)
                QuestSerializer.Serialize(referenceTable, (QuestConversation)m_Conversations[i], writer);

            ChildSerialize(writer);
        }

        public virtual void ChildSerialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)0); // version
        }

        public bool IsObjectiveInProgress(Type type)
        {
            QuestObjective obj = FindObjective(type);

            return (obj != null && !obj.Completed);
        }

        public QuestObjective FindObjective(Type type)
        {
            for (int i = m_Objectives.Count - 1; i >= 0; --i)
            {
                QuestObjective obj = (QuestObjective)m_Objectives[i];

                if (obj.GetType() == type)
                    return obj;
            }

            return null;
        }

        public virtual void SendOffer()
        {
            m_From.SendGump(new QuestOfferGump(this));
        }

        public virtual void GetContextMenuEntries(List<ContextMenuEntry> list)
        {
            if (m_Objectives.Count > 0)
                list.Add(new QuestCallbackEntry(6154, new QuestCallback(ShowQuestLog))); // View Quest Log

            if (m_Conversations.Count > 0)
                list.Add(new QuestCallbackEntry(6156, new QuestCallback(ShowQuestConversation))); // Quest Conversation

            list.Add(new QuestCallbackEntry(6155, new QuestCallback(BeginCancelQuest))); // Cancel Quest
        }

        public virtual void ShowQuestLogUpdated()
        {
            m_From.CloseGump(typeof(QuestLogUpdatedGump));
            m_From.SendGump(new QuestLogUpdatedGump(this));
        }

        public virtual void ShowQuestLog()
        {
            if (m_Objectives.Count > 0)
            {
                m_From.CloseGump(typeof(QuestItemInfoGump));
                m_From.CloseGump(typeof(QuestLogUpdatedGump));
                m_From.CloseGump(typeof(QuestObjectivesGump));
                m_From.CloseGump(typeof(QuestConversationsGump));

                m_From.SendGump(new QuestObjectivesGump(m_Objectives));

                QuestObjective last = (QuestObjective)m_Objectives[m_Objectives.Count - 1];

                if (last.Info != null)
                    m_From.SendGump(new QuestItemInfoGump(last.Info));
            }
        }

        public virtual void ShowQuestConversation()
        {
            if (m_Conversations.Count > 0)
            {
                m_From.CloseGump(typeof(QuestItemInfoGump));
                m_From.CloseGump(typeof(QuestObjectivesGump));
                m_From.CloseGump(typeof(QuestConversationsGump));

                m_From.SendGump(new QuestConversationsGump(m_Conversations));

                QuestConversation last = (QuestConversation)m_Conversations[m_Conversations.Count - 1];

                if (last.Info != null)
                    m_From.SendGump(new QuestItemInfoGump(last.Info));
            }
        }

        public virtual void BeginCancelQuest()
        {
            m_From.SendGump(new QuestCancelGump(this));
        }

        public virtual void EndCancelQuest(bool shouldCancel)
        {
            if (m_From.Quest != this)
                return;

            if (shouldCancel)
            {
                m_From.SendLocalizedMessage(1049015); // You have canceled your quest.
                Cancel();
            }
            else
            {
                m_From.SendLocalizedMessage(1049014); // You have chosen not to cancel your quest.
            }
        }

        public virtual void Cancel()
        {
            ClearQuest(false);
        }

        public virtual void Complete()
        {
            ClearQuest(true);
        }

        public virtual void ClearQuest(bool completed)
        {
            StopTimer();

            if (m_From.Quest == this)
            {
                m_From.Quest = null;

                TimeSpan restartDelay = this.RestartDelay;

                if ((completed && restartDelay > TimeSpan.Zero) || (!completed && restartDelay == TimeSpan.MaxValue))
                {
                    List<QuestRestartInfo> doneQuests = m_From.DoneQuests;

                    if (doneQuests == null)
                        m_From.DoneQuests = doneQuests = new List<QuestRestartInfo>();

                    bool found = false;

                    Type ourQuestType = this.GetType();

                    for (int i = 0; i < doneQuests.Count; ++i)
                    {
                        QuestRestartInfo restartInfo = doneQuests[i];

                        if (restartInfo.QuestType == ourQuestType)
                        {
                            restartInfo.Reset(restartDelay);
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                        doneQuests.Add(new QuestRestartInfo(ourQuestType, restartDelay));
                }
            }
        }

        public virtual void AddConversation(QuestConversation conv)
        {
            conv.System = this;

            if (conv.Logged)
                m_Conversations.Add(conv);

            m_From.CloseGump(typeof(QuestItemInfoGump));
            m_From.CloseGump(typeof(QuestObjectivesGump));
            m_From.CloseGump(typeof(QuestConversationsGump));

            if (conv.Logged)
                m_From.SendGump(new QuestConversationsGump(m_Conversations));
            else
                m_From.SendGump(new QuestConversationsGump(conv));

            if (conv.Info != null)
                m_From.SendGump(new QuestItemInfoGump(conv.Info));
        }

        public virtual void AddObjective(QuestObjective obj)
        {
            obj.System = this;
            m_Objectives.Add(obj);

            ShowQuestLogUpdated();
        }

        public virtual void Accept()
        {
            if (m_From.Quest != null)
                return;

            m_From.Quest = this;
            m_From.SendLocalizedMessage(1049019); // You have accepted the Quest.

            StartTimer();
        }

        public virtual void Decline()
        {
            m_From.SendLocalizedMessage(1049018); // You have declined the Quest.
        }

        public static bool CanOfferQuest(Mobile check, Type questType)
        {
            bool inRestartPeriod;

            return CanOfferQuest(check, questType, out inRestartPeriod);
        }

        public static bool CanOfferQuest(Mobile check, Type questType, out bool inRestartPeriod)
        {
            inRestartPeriod = false;

            PlayerMobile pm = check as PlayerMobile;

            if (pm == null)
                return false;

            if (pm.HasGump(typeof(QuestOfferGump)))
                return false;

            if (questType == typeof(Necro.DarkTidesQuest) && pm.Profession != 4) // necromancer
                return false;

            if (questType == typeof(Haven.UzeraanTurmoilQuest) && pm.Profession != 1 && pm.Profession != 2 && pm.Profession != 5) // warrior / magician / paladin
                return false;

            if (questType == typeof(Samurai.HaochisTrialsQuest) && pm.Profession != 6) // samurai
                return false;

            if (questType == typeof(Ninja.EminosUndertakingQuest) && pm.Profession != 7) // ninja
                return false;

            List<QuestRestartInfo> doneQuests = pm.DoneQuests;

            if (doneQuests != null)
            {
                for (int i = 0; i < doneQuests.Count; ++i)
                {
                    QuestRestartInfo restartInfo = doneQuests[i];

                    if (restartInfo.QuestType == questType)
                    {
                        DateTime endTime = restartInfo.RestartTime;

                        if (DateTime.UtcNow < endTime)
                        {
                            inRestartPeriod = true;
                            return false;
                        }

                        doneQuests.RemoveAt(i--);
                        return true;
                    }
                }
            }

            return true;
        }

        public static void FocusTo(Mobile who, Mobile to)
        {
            if (Utility.RandomBool())
            {
                who.Animate(17, 7, 1, true, false, 0);
            }
            else
            {
                switch (Utility.Random(3))
                {
                    case 0: who.Animate(32, 7, 1, true, false, 0); break;
                    case 1: who.Animate(33, 7, 1, true, false, 0); break;
                    case 2: who.Animate(34, 7, 1, true, false, 0); break;
                }
            }

            who.Direction = who.GetDirectionTo(to);
        }

        public static int RandomBrightHue()
        {
            if (0.1 > Utility.RandomDouble())
                return Utility.RandomList(0x62, 0x71);

            return Utility.RandomList(0x03, 0x0D, 0x13, 0x1C, 0x21, 0x30, 0x37, 0x3A, 0x44, 0x59);
        }
    }

    public class QuestCallbackEntry : ContextMenuEntry
    {
        private QuestCallback m_Callback;

        public QuestCallbackEntry(int number, QuestCallback callback)
            : this(number, -1, callback)
        {
        }

        public QuestCallbackEntry(int number, int range, QuestCallback callback)
            : base(number, range)
        {
            m_Callback = callback;
        }

        public override void OnClick()
        {
            if (m_Callback != null)
                m_Callback();
        }
    }

    public abstract class BaseQuestGump : Gump
    {
        public const int Black = 0x0000;
        public const int White = 0x7FFF;
        public const int DarkGreen = 10000;
        public const int LightGreen = 90000;
        public const int Blue = 19777215;

        public static int C16232(int c16)
        {
            c16 &= 0x7FFF;

            int r = (((c16 >> 10) & 0x1F) << 3);
            int g = (((c16 >> 05) & 0x1F) << 3);
            int b = (((c16 >> 00) & 0x1F) << 3);

            return (r << 16) | (g << 8) | (b << 0);
        }

        public static int C16216(int c16)
        {
            return c16 & 0x7FFF;
        }

        public static int C32216(int c32)
        {
            c32 &= 0xFFFFFF;

            int r = (((c32 >> 16) & 0xFF) >> 3);
            int g = (((c32 >> 08) & 0xFF) >> 3);
            int b = (((c32 >> 00) & 0xFF) >> 3);

            return (r << 10) | (g << 5) | (b << 0);
        }

        public static string Color(string text, int color)
        {
            return String.Format("<BASEFONT COLOR=#{0:X6}>{1}</BASEFONT>", color, text);
        }

        public static ArrayList BuildList(object obj)
        {
            ArrayList list = new ArrayList();

            list.Add(obj);

            return list;
        }

        public void AddHtmlObject(int x, int y, int width, int height, object message, int color, bool back, bool scroll)
        {
            if (message is string)
            {
                string html = (string)message;

                AddHtml(x, y, width, height, Color(html, C16232(color)), back, scroll);
            }
            else if (message is int)
            {
                int html = (int)message;

                AddHtmlLocalized(x, y, width, height, html, C16216(color), back, scroll);
            }
        }

        public BaseQuestGump(int x, int y)
            : base(x, y)
        {
        }
    }

    public class QuestOfferGump : BaseQuestGump
    {
        private QuestSystem m_System;

        public QuestOfferGump(QuestSystem system)
            : base(75, 25)
        {
            m_System = system;

            Closable = false;

            AddPage(0);

            AddImageTiled(50, 20, 400, 400, 2624);
            AddAlphaRegion(50, 20, 400, 400);

            AddImage(90, 33, 9005);
            AddHtmlLocalized(130, 45, 270, 20, 1049010, White, false, false); // Quest Offer
            AddImageTiled(130, 65, 175, 1, 9101);

            AddImage(140, 110, 1209);
            AddHtmlObject(160, 108, 250, 20, system.Name, DarkGreen, false, false);

            AddHtmlObject(98, 140, 312, 200, system.OfferMessage, LightGreen, false, true);

            AddRadio(85, 350, 9720, 9723, true, 1);
            AddHtmlLocalized(120, 356, 280, 20, 1049011, White, false, false); // I accept!

            AddRadio(85, 385, 9720, 9723, false, 0);
            AddHtmlLocalized(120, 391, 280, 20, 1049012, White, false, false); // No thanks, I decline.

            AddButton(340, 390, 247, 248, 1, GumpButtonType.Reply, 0);

            AddImageTiled(50, 29, 30, 390, 10460);
            AddImageTiled(34, 140, 17, 279, 9263);

            AddImage(48, 135, 10411);
            AddImage(-16, 285, 10402);
            AddImage(0, 10, 10421);
            AddImage(25, 0, 10420);

            AddImageTiled(83, 15, 350, 15, 10250);

            AddImage(34, 419, 10306);
            AddImage(442, 419, 10304);
            AddImageTiled(51, 419, 392, 17, 10101);

            AddImageTiled(415, 29, 44, 390, 2605);
            AddImageTiled(415, 29, 30, 390, 10460);
            AddImage(425, 0, 10441);

            AddImage(370, 50, 1417);
            AddImage(379, 60, system.Picture);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 1)
            {
                if (info.IsSwitched(1))
                    m_System.Accept();
                else
                    m_System.Decline();
            }
        }
    }

    public class QuestCancelGump : BaseQuestGump
    {
        private QuestSystem m_System;

        public QuestCancelGump(QuestSystem system)
            : base(120, 50)
        {
            m_System = system;

            Closable = false;

            AddPage(0);

            AddImageTiled(0, 0, 348, 262, 2702);
            AddAlphaRegion(0, 0, 348, 262);

            AddImage(0, 15, 10152);
            AddImageTiled(0, 30, 17, 200, 10151);
            AddImage(0, 230, 10154);

            AddImage(15, 0, 10252);
            AddImageTiled(30, 0, 300, 17, 10250);
            AddImage(315, 0, 10254);

            AddImage(15, 244, 10252);
            AddImageTiled(30, 244, 300, 17, 10250);
            AddImage(315, 244, 10254);

            AddImage(330, 15, 10152);
            AddImageTiled(330, 30, 17, 200, 10151);
            AddImage(330, 230, 10154);

            AddImage(333, 2, 10006);
            AddImage(333, 248, 10006);
            AddImage(2, 248, 10006);
            AddImage(2, 2, 10006);

            AddHtmlLocalized(25, 22, 200, 20, 1049000, 32000, false, false); // Confirm Quest Cancellation
            AddImage(25, 40, 3007);

            if (system.IsTutorial)
            {
                AddHtmlLocalized(25, 55, 300, 120, 1060836, White, false, false); // This quest will give you valuable information, skills and equipment that will help you advance in the game at a quicker pace.<BR><BR>Are you certain you wish to cancel at this time?
            }
            else
            {
                AddHtmlLocalized(25, 60, 300, 20, 1049001, White, false, false); // You have chosen to abort your quest:
                AddImage(25, 81, 0x25E7);
                AddHtmlObject(48, 80, 280, 20, system.Name, DarkGreen, false, false);

                AddHtmlLocalized(25, 120, 280, 20, 1049002, White, false, false); // Can this quest be restarted after quitting?
                AddImage(25, 141, 0x25E7);
                AddHtmlLocalized(48, 140, 280, 20, (system.RestartDelay < TimeSpan.MaxValue) ? 1049016 : 1049017, DarkGreen, false, false); // Yes/No
            }

            AddRadio(25, 175, 9720, 9723, true, 1);
            AddHtmlLocalized(60, 180, 280, 20, 1049005, White, false, false); // Yes, I really want to quit!

            AddRadio(25, 210, 9720, 9723, false, 0);
            AddHtmlLocalized(60, 215, 280, 20, 1049006, White, false, false); // No, I don't want to quit.

            AddButton(265, 220, 247, 248, 1, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 1)
                m_System.EndCancelQuest(info.IsSwitched(1));
        }
    }

    #region Quest Offer Regions

    public class QuestOfferRegion : BaseRegion
    {
        private Type m_Quest;

        public Type Quest { get { return m_Quest; } }

        public QuestOfferRegion(XmlElement xml, Map map, Region parent)
            : base(xml, map, parent)
        {
            ReadType(xml["quest"], "type", ref m_Quest);
        }

        public override void OnEnter(Mobile m)
        {
            base.OnEnter(m);

            if (m_Quest == null)
                return;

            PlayerMobile player = m as PlayerMobile;

            if (player != null && player.Quest == null && QuestSystem.CanOfferQuest(m, m_Quest))
            {
                try
                {
                    QuestSystem qs = (QuestSystem)Activator.CreateInstance(m_Quest, new object[] { player });
                    qs.SendOffer();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error creating quest {0}: {1}", m_Quest, ex);
                }
            }
        }
    }

    public class QuestNoEntryRegion : BaseRegion
    {
        private Type m_Quest;
        private Type m_MinObjective;
        private Type m_MaxObjective;
        private int m_Message;

        public Type Quest { get { return m_Quest; } }
        public Type MinObjective { get { return m_MinObjective; } }
        public Type MaxObjective { get { return m_MaxObjective; } }
        public int Message { get { return m_Message; } }

        public QuestNoEntryRegion(XmlElement xml, Map map, Region parent)
            : base(xml, map, parent)
        {
            XmlElement questEl = xml["quest"];

            ReadType(questEl, "type", ref m_Quest);
            ReadType(questEl, "min", ref m_MinObjective, false);
            ReadType(questEl, "max", ref m_MaxObjective, false);
            ReadInt32(questEl, "message", ref m_Message, false);
        }

        public override bool OnMoveInto(Mobile m, Direction d, Point3D newLocation, Point3D oldLocation)
        {
            if (!base.OnMoveInto(m, d, newLocation, oldLocation))
                return false;

            if (m.AccessLevel > AccessLevel.Player)
                return true;

            if (m is BaseCreature)
            {
                BaseCreature bc = m as BaseCreature;

                if (!bc.Controlled && !bc.Summoned)
                    return true;
            }

            if (m_Quest == null)
                return true;

            PlayerMobile player = m as PlayerMobile;

            if (player != null && player.Quest != null && player.Quest.GetType() == m_Quest
                && (m_MinObjective == null || player.Quest.FindObjective(m_MinObjective) != null)
                && (m_MaxObjective == null || player.Quest.FindObjective(m_MaxObjective) == null))
            {
                return true;
            }
            else
            {
                if (m_Message != 0)
                    m.SendLocalizedMessage(m_Message);

                return false;
            }
        }
    }

    public class QuestCompleteObjectiveRegion : BaseRegion
    {
        private Type m_Quest;
        private Type m_Objective;

        public Type Quest { get { return m_Quest; } }
        public Type Objective { get { return m_Objective; } }

        public QuestCompleteObjectiveRegion(XmlElement xml, Map map, Region parent)
            : base(xml, map, parent)
        {
            XmlElement questEl = xml["quest"];

            ReadType(questEl, "type", ref m_Quest);
            ReadType(questEl, "complete", ref m_Objective);
        }

        public override void OnEnter(Mobile m)
        {
            base.OnEnter(m);

            if (m_Quest != null && m_Objective != null)
            {
                PlayerMobile player = m as PlayerMobile;

                if (player != null && player.Quest != null && player.Quest.GetType() == m_Quest)
                {
                    QuestObjective obj = player.Quest.FindObjective(m_Objective);

                    if (obj != null && !obj.Completed)
                        obj.Complete();
                }
            }
        }
    }

    public class CancelQuestRegion : BaseRegion
    {
        private Type m_Quest;

        public Type Quest { get { return m_Quest; } }

        public CancelQuestRegion(XmlElement xml, Map map, Region parent)
            : base(xml, map, parent)
        {
            ReadType(xml["quest"], "type", ref m_Quest);
        }

        public override bool OnMoveInto(Mobile m, Direction d, Point3D newLocation, Point3D oldLocation)
        {
            if (!base.OnMoveInto(m, d, newLocation, oldLocation))
                return false;

            if (m.AccessLevel > AccessLevel.Player)
                return true;

            if (m_Quest == null)
                return true;

            PlayerMobile player = m as PlayerMobile;

            if (player != null && player.Quest != null && player.Quest.GetType() == m_Quest)
            {
                if (!player.HasGump(typeof(QuestCancelGump)))
                    player.Quest.BeginCancelQuest();

                return false;
            }

            return true;
        }
    }

    #endregion

    #region Questing Objectives

    public abstract class QuestObjective
    {
        private QuestSystem m_System;
        private bool m_HasBeenRead;
        private int m_CurProgress;
        private bool m_HasCompleted;

        public abstract object Message { get; }

        public virtual int MaxProgress { get { return 1; } }
        public virtual QuestItemInfo[] Info { get { return null; } }

        public QuestSystem System
        {
            get { return m_System; }
            set { m_System = value; }
        }

        public bool HasBeenRead
        {
            get { return m_HasBeenRead; }
            set { m_HasBeenRead = value; }
        }

        public int CurProgress
        {
            get { return m_CurProgress; }
            set { m_CurProgress = value; CheckCompletionStatus(); }
        }

        public bool HasCompleted
        {
            get { return m_HasCompleted; }
            set { m_HasCompleted = value; }
        }

        public virtual bool Completed
        {
            get { return m_CurProgress >= MaxProgress; }
        }

        public bool IsSingleObjective
        {
            get { return (MaxProgress == 1); }
        }

        public QuestObjective()
        {
        }

        public virtual void BaseDeserialize(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            switch (version)
            {
                case 1:
                    {
                        m_HasBeenRead = reader.ReadBool();
                        goto case 0;
                    }
                case 0:
                    {
                        m_CurProgress = reader.ReadEncodedInt();
                        m_HasCompleted = reader.ReadBool();

                        break;
                    }
            }

            ChildDeserialize(reader);
        }

        public virtual void ChildDeserialize(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();
        }

        public virtual void BaseSerialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)1); // version

            writer.Write((bool)m_HasBeenRead);
            writer.WriteEncodedInt((int)m_CurProgress);
            writer.Write((bool)m_HasCompleted);

            ChildSerialize(writer);
        }

        public virtual void ChildSerialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)0); // version
        }

        public virtual void Complete()
        {
            CurProgress = MaxProgress;
        }

        public virtual void RenderMessage(BaseQuestGump gump)
        {
            gump.AddHtmlObject(70, 130, 300, 100, this.Message, BaseQuestGump.Blue, false, false);
        }

        public virtual void RenderProgress(BaseQuestGump gump)
        {
            gump.AddHtmlObject(70, 260, 270, 100, this.Completed ? 1049077 : 1049078, BaseQuestGump.Blue, false, false);
        }

        public virtual void CheckCompletionStatus()
        {
            if (Completed && !HasCompleted)
            {
                HasCompleted = true;
                OnComplete();
            }
        }

        public virtual void OnRead()
        {
        }

        public virtual bool GetTimerEvent()
        {
            return !Completed;
        }

        public virtual void CheckProgress()
        {
        }

        public virtual void OnComplete()
        {
        }

        public virtual bool GetKillEvent(BaseCreature creature, Container corpse)
        {
            return !Completed;
        }

        public virtual void OnKill(BaseCreature creature, Container corpse)
        {
        }

        public virtual bool IgnoreYoungProtection(Mobile from)
        {
            return false;
        }
    }

    public class QuestLogUpdatedGump : BaseQuestGump
    {
        private QuestSystem m_System;

        public QuestLogUpdatedGump(QuestSystem system)
            : base(3, 30)
        {
            m_System = system;

            AddPage(0);

            AddImage(20, 5, 1417);

            AddHtmlLocalized(0, 78, 120, 40, 1049079, White, false, false); // Quest Log Updated

            AddImageTiled(0, 78, 120, 40, 2624);
            AddAlphaRegion(0, 78, 120, 40);

            AddButton(30, 15, 5575, 5576, 1, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 1)
                m_System.ShowQuestLog();
        }
    }

    public class QuestObjectivesGump : BaseQuestGump
    {
        private ArrayList m_Objectives;

        public QuestObjectivesGump(QuestObjective obj)
            : this(BuildList(obj))
        {
        }

        public QuestObjectivesGump(ArrayList objectives)
            : base(90, 50)
        {
            m_Objectives = objectives;

            Closable = false;

            AddPage(0);

            AddImage(0, 0, 3600);
            AddImageTiled(0, 14, 15, 375, 3603);
            AddImageTiled(380, 14, 14, 375, 3605);
            AddImage(0, 376, 3606);
            AddImageTiled(15, 376, 370, 16, 3607);
            AddImageTiled(15, 0, 370, 16, 3601);
            AddImage(380, 0, 3602);
            AddImage(380, 376, 3608);

            AddImageTiled(15, 15, 365, 365, 2624);
            AddAlphaRegion(15, 15, 365, 365);

            AddImage(20, 87, 1231);
            AddImage(75, 62, 9307);

            AddHtmlLocalized(117, 35, 230, 20, 1046026, Blue, false, false); // Quest Log

            AddImage(77, 33, 9781);
            AddImage(65, 110, 2104);

            AddHtmlLocalized(79, 106, 230, 20, 1049073, Blue, false, false); // Objective:

            AddImageTiled(68, 125, 120, 1, 9101);
            AddImage(65, 240, 2104);

            AddHtmlLocalized(79, 237, 230, 20, 1049076, Blue, false, false); // Progress details:

            AddImageTiled(68, 255, 120, 1, 9101);
            AddButton(175, 355, 2313, 2312, 1, GumpButtonType.Reply, 0);

            AddImage(341, 15, 10450);
            AddImage(341, 330, 10450);
            AddImage(15, 330, 10450);
            AddImage(15, 15, 10450);

            AddPage(1);

            for (int i = 0; i < objectives.Count; ++i)
            {
                QuestObjective obj = (QuestObjective)objectives[objectives.Count - 1 - i];

                if (i > 0)
                {
                    AddButton(55, 346, 9909, 9911, 0, GumpButtonType.Page, 1 + i);
                    AddHtmlLocalized(82, 347, 50, 20, 1043354, White, false, false); // Previous

                    AddPage(1 + i);
                }

                obj.RenderMessage(this);
                obj.RenderProgress(this);

                if (i > 0)
                {
                    AddButton(317, 346, 9903, 9905, 0, GumpButtonType.Page, i);
                    AddHtmlLocalized(278, 347, 50, 20, 1043353, White, false, false); // Next
                }
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            for (int i = m_Objectives.Count - 1; i >= 0; --i)
            {
                QuestObjective obj = (QuestObjective)m_Objectives[i];

                if (!obj.HasBeenRead)
                {
                    obj.HasBeenRead = true;
                    obj.OnRead();
                }
            }
        }
    }

    #endregion

    #region Mobile Conversation

    public abstract class QuestConversation
    {
        private QuestSystem m_System;
        private bool m_HasBeenRead;

        public abstract object Message { get; }

        public virtual QuestItemInfo[] Info { get { return null; } }
        public virtual bool Logged { get { return true; } }

        public QuestSystem System
        {
            get { return m_System; }
            set { m_System = value; }
        }

        public bool HasBeenRead
        {
            get { return m_HasBeenRead; }
            set { m_HasBeenRead = value; }
        }

        public QuestConversation()
        {
        }

        public virtual void BaseDeserialize(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            switch (version)
            {
                case 0:
                    {
                        m_HasBeenRead = reader.ReadBool();

                        break;
                    }
            }

            ChildDeserialize(reader);
        }

        public virtual void ChildDeserialize(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();
        }

        public virtual void BaseSerialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)0); // version

            writer.Write((bool)m_HasBeenRead);

            ChildSerialize(writer);
        }

        public virtual void ChildSerialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)0); // version
        }

        public virtual void OnRead()
        {
        }
    }

    public class QuestConversationsGump : BaseQuestGump
    {
        private ArrayList m_Conversations;

        public QuestConversationsGump(QuestConversation conv)
            : this(BuildList(conv))
        {
        }

        public QuestConversationsGump(ArrayList conversations)
            : base(30, 50)
        {
            m_Conversations = conversations;

            Closable = false;

            AddPage(0);

            AddImage(349, 10, 9392);
            AddImageTiled(349, 130, 100, 120, 9395);
            AddImageTiled(149, 10, 200, 140, 9391);
            AddImageTiled(149, 250, 200, 140, 9397);
            AddImage(349, 250, 9398);
            AddImage(35, 10, 9390);
            AddImageTiled(35, 150, 120, 100, 9393);
            AddImage(35, 250, 9396);

            AddHtmlLocalized(110, 60, 200, 20, 1049069, White, false, false); // <STRONG>Conversation Event</STRONG>

            AddImage(65, 14, 10102);
            AddImageTiled(81, 14, 349, 17, 10101);
            AddImage(426, 14, 10104);

            AddImageTiled(55, 40, 388, 323, 2624);
            AddAlphaRegion(55, 40, 388, 323);

            AddImageTiled(75, 90, 200, 1, 9101);
            AddImage(75, 58, 9781);
            AddImage(380, 45, 223);

            AddButton(220, 335, 2313, 2312, 1, GumpButtonType.Reply, 0);
            AddImage(0, 0, 10440);

            AddPage(1);

            for (int i = 0; i < conversations.Count; ++i)
            {
                QuestConversation conv = (QuestConversation)conversations[conversations.Count - 1 - i];

                if (i > 0)
                {
                    AddButton(65, 366, 9909, 9911, 0, GumpButtonType.Page, 1 + i);
                    AddHtmlLocalized(90, 367, 50, 20, 1043354, Black, false, false); // Previous

                    AddPage(1 + i);
                }

                AddHtmlObject(70, 110, 365, 220, conv.Message, LightGreen, false, true);

                if (i > 0)
                {
                    AddButton(420, 366, 9903, 9905, 0, GumpButtonType.Page, i);
                    AddHtmlLocalized(370, 367, 50, 20, 1043353, Black, false, false); // Next
                }
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            for (int i = m_Conversations.Count - 1; i >= 0; --i)
            {
                QuestConversation qc = (QuestConversation)m_Conversations[i];

                if (!qc.HasBeenRead)
                {
                    qc.HasBeenRead = true;
                    qc.OnRead();
                }
            }
        }
    }

    #endregion

    #region Quest Mobile Engine

    public class TalkEntry : ContextMenuEntry
    {
        private BaseQuester m_Quester;

        public TalkEntry(BaseQuester quester)
            : base(quester.TalkNumber)
        {
            m_Quester = quester;
        }

        public override void OnClick()
        {
            Mobile from = Owner.From;

            if (from.CheckAlive() && from is PlayerMobile && m_Quester.CanTalkTo((PlayerMobile)from))
                m_Quester.OnTalk((PlayerMobile)from, true);
        }
    }

    public abstract class BaseQuester : BaseVendor
    {
        protected List<SBInfo> m_SBInfos = new List<SBInfo>();
        protected override List<SBInfo> SBInfos { get { return m_SBInfos; } }

        public override bool IsActiveVendor { get { return false; } }
        public override bool IsInvulnerable { get { return true; } }
        public override bool DisallowAllMoves { get { return true; } }
        public override bool ClickTitle { get { return false; } }
        public override bool CanTeach { get { return false; } }

        public virtual int TalkNumber { get { return 6146; } } // Talk

        public override void InitSBInfo()
        {
        }

        public BaseQuester()
            : this(null)
        {
        }

        public BaseQuester(string title)
            : base(title)
        {
        }

        public BaseQuester(Serial serial)
            : base(serial)
        {
        }

        public abstract void OnTalk(PlayerMobile player, bool contextMenu);

        public virtual bool CanTalkTo(PlayerMobile to)
        {
            return true;
        }

        public virtual int GetAutoTalkRange(PlayerMobile m)
        {
            return -1;
        }

        public override bool CanBeDamaged()
        {
            return false;
        }

        protected Item SetHue(Item item, int hue)
        {
            item.Hue = hue;
            return item;
        }

        public override void AddCustomContextEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.AddCustomContextEntries(from, list);

            if (from.Alive && from is PlayerMobile && TalkNumber > 0 && CanTalkTo((PlayerMobile)from))
                list.Add(new TalkEntry(this));
        }

        public override void OnMovement(Mobile m, Point3D oldLocation)
        {
            if (m.Alive && m is PlayerMobile)
            {
                PlayerMobile pm = (PlayerMobile)m;

                int range = GetAutoTalkRange(pm);

                if (m.Alive && range >= 0 && InRange(m, range) && !InRange(oldLocation, range) && CanTalkTo(pm))
                    OnTalk(pm, false);
            }
        }

        public void FocusTo(Mobile to)
        {
            QuestSystem.FocusTo(this, to);
        }

        public static Container GetNewContainer()
        {
            Bag bag = new Bag();
            bag.Hue = QuestSystem.RandomBrightHue();
            return bag;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    #endregion

    #region Quest Object Engine

    public class QuestItemInfo
    {
        private object m_Name;
        private int m_ItemID;

        public object Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public int ItemID
        {
            get { return m_ItemID; }
            set { m_ItemID = value; }
        }

        public QuestItemInfo(object name, int itemID)
        {
            m_Name = name;
            m_ItemID = itemID;
        }
    }

    public class QuestItemInfoGump : BaseQuestGump
    {
        public QuestItemInfoGump(QuestItemInfo[] info)
            : base(485, 75)
        {
            int height = 100 + (info.Length * 75);

            AddPage(0);

            AddBackground(5, 10, 145, height, 5054);

            AddImageTiled(13, 20, 125, 10, 2624);
            AddAlphaRegion(13, 20, 125, 10);

            AddImageTiled(13, height - 10, 128, 10, 2624);
            AddAlphaRegion(13, height - 10, 128, 10);

            AddImageTiled(13, 20, 10, height - 30, 2624);
            AddAlphaRegion(13, 20, 10, height - 30);

            AddImageTiled(131, 20, 10, height - 30, 2624);
            AddAlphaRegion(131, 20, 10, height - 30);

            AddHtmlLocalized(67, 35, 120, 20, 1011233, White, false, false); // INFO

            AddImage(62, 52, 9157);
            AddImage(72, 52, 9157);
            AddImage(82, 52, 9157);

            AddButton(25, 31, 1209, 1210, 777, GumpButtonType.Reply, 0);

            AddPage(1);

            for (int i = 0; i < info.Length; ++i)
            {
                QuestItemInfo cur = info[i];

                AddHtmlObject(25, 65 + (i * 75), 110, 20, cur.Name, 1153, false, false);
                AddItem(45, 85 + (i * 75), cur.ItemID);
            }
        }
    }

    public abstract class QuestItem : Item
    {
        public QuestItem(int itemID)
            : base(itemID)
        {
        }

        public QuestItem(Serial serial)
            : base(serial)
        {
        }

        public abstract bool CanDrop(PlayerMobile pm);

        public virtual bool Accepted { get { return Deleted; } }

        public override bool DropToWorld(Mobile from, Point3D p)
        {
            bool ret = base.DropToWorld(from, p);

            if (ret && !Accepted && Parent != from.Backpack)
            {
                if (from.AccessLevel > AccessLevel.Player)
                {
                    return true;
                }
                else if (!(from is PlayerMobile) || CanDrop((PlayerMobile)from))
                {
                    return true;
                }
                else
                {
                    from.SendLocalizedMessage(1049343); // You can only drop quest items into the top-most level of your backpack while you still need them for your quest.
                    return false;
                }
            }
            else
            {
                return ret;
            }
        }

        public override bool DropToMobile(Mobile from, Mobile target, Point3D p)
        {
            bool ret = base.DropToMobile(from, target, p);

            if (ret && !Accepted && Parent != from.Backpack)
            {
                if (from.AccessLevel > AccessLevel.Player)
                {
                    return true;
                }
                else if (!(from is PlayerMobile) || CanDrop((PlayerMobile)from))
                {
                    return true;
                }
                else
                {
                    from.SendLocalizedMessage(1049344); // You decide against trading the item.  You still need it for your quest.
                    return false;
                }
            }
            else
            {
                return ret;
            }
        }

        public override bool DropToItem(Mobile from, Item target, Point3D p)
        {
            bool ret = base.DropToItem(from, target, p);

            if (ret && !Accepted && Parent != from.Backpack)
            {
                if (from.AccessLevel > AccessLevel.Player)
                {
                    return true;
                }
                else if (!(from is PlayerMobile) || CanDrop((PlayerMobile)from))
                {
                    return true;
                }
                else
                {
                    from.SendLocalizedMessage(1049343); // You can only drop quest items into the top-most level of your backpack while you still need them for your quest.
                    return false;
                }
            }
            else
            {
                return ret;
            }
        }

        public override DeathMoveResult OnParentDeath(Mobile parent)
        {
            if (parent is PlayerMobile && !CanDrop((PlayerMobile)parent))
                return DeathMoveResult.MoveToBackpack;
            else
                return base.OnParentDeath(parent);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    #region Specific BaseItems

    public abstract class DynamicTeleporter : Item
    {
        public override int LabelNumber { get { return 1049382; } } // a magical teleporter

        public DynamicTeleporter()
            : this(0x1822, 0x482)
        {
        }

        public DynamicTeleporter(int itemID, int hue)
            : base(itemID)
        {
            Movable = false;
            Hue = hue;
        }

        public abstract bool GetDestination(PlayerMobile player, ref Point3D loc, ref Map map);

        public virtual int NotWorkingMessage { get { return 500309; } } // Nothing Happens.

        public override bool OnMoveOver(Mobile m)
        {
            PlayerMobile pm = m as PlayerMobile;

            if (pm != null)
            {
                Point3D loc = Point3D.Zero;
                Map map = null;

                if (GetDestination(pm, ref loc, ref map))
                {
                    BaseCreature.TeleportPets(pm, loc, map);

                    pm.PlaySound(0x1FE);
                    pm.MoveToWorld(loc, map);

                    return false;
                }
                else
                {
                    pm.SendLocalizedMessage(this.NotWorkingMessage);
                }
            }

            return base.OnMoveOver(m);
        }

        public DynamicTeleporter(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class HornOfRetreat : Item
    {
        private Point3D m_DestLoc;
        private Map m_DestMap;
        private int m_Charges;

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D DestLoc
        {
            get { return m_DestLoc; }
            set { m_DestLoc = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Map DestMap
        {
            get { return m_DestMap; }
            set { m_DestMap = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Charges
        {
            get { return m_Charges; }
            set { m_Charges = value; InvalidateProperties(); }
        }

        public override int LabelNumber { get { return 1049117; } } // Horn of Retreat

        [Constructable]
        public HornOfRetreat()
            : base(0xFC4)
        {
            Hue = 0x482;
            Weight = 1.0;
            Charges = 10;
        }

        public virtual bool ValidateUse(Mobile from)
        {
            return true;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1060741, m_Charges.ToString()); // charges: ~1_val~
        }

        private Timer m_PlayTimer;

        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack))
            {
                if (!ValidateUse(from))
                {
                    SendLocalizedMessageTo(from, 500309); // Nothing Happens.
                }
                else if (Core.ML && from.Map != Map.Trammel && from.Map != Map.Malas)
                {
                    from.SendLocalizedMessage(1076154); // You can only use this in Trammel and Malas.
                }
                else if (m_PlayTimer != null)
                {
                    SendLocalizedMessageTo(from, 1042144); // This is currently in use.
                }
                else if (Charges > 0)
                {
                    from.Animate(34, 7, 1, true, false, 0);
                    from.PlaySound(0xFF);
                    from.SendLocalizedMessage(1049115); // You play the horn and a sense of peace overcomes you...

                    --Charges;

                    m_PlayTimer = Timer.DelayCall(TimeSpan.FromSeconds(5.0), new TimerStateCallback(PlayTimer_Callback), from);
                }
                else
                {
                    SendLocalizedMessageTo(from, 1042544); // This item is out of charges.
                }
            }
            else
            {
                SendLocalizedMessageTo(from, 1042001); // That must be in your pack for you to use it.
            }
        }

        public virtual void PlayTimer_Callback(object state)
        {
            Mobile from = (Mobile)state;

            m_PlayTimer = null;

            HornOfRetreatMoongate gate = new HornOfRetreatMoongate(this.DestLoc, this.DestMap, from, this.Hue);

            gate.MoveToWorld(from.Location, from.Map);

            from.PlaySound(0x20E);

            gate.SendLocalizedMessageTo(from, 1049102, from.Name); // Quickly ~1_NAME~! Onward through the gate!
        }

        public HornOfRetreat(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write(m_DestLoc);
            writer.Write(m_DestMap);
            writer.Write(m_Charges);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_DestLoc = reader.ReadPoint3D();
                        m_DestMap = reader.ReadMap();
                        m_Charges = reader.ReadInt();
                        break;
                    }
            }
        }
    }

    public class HornOfRetreatMoongate : Moongate
    {
        public override int LabelNumber { get { return 1049114; } } // Sanctuary Gate

        private Mobile m_Caster;

        public HornOfRetreatMoongate(Point3D destLoc, Map destMap, Mobile caster, int hue)
        {
            m_Caster = caster;

            Target = destLoc;
            TargetMap = destMap;

            Hue = hue;
            Light = LightType.Circle300;

            Dispellable = false;

            Timer.DelayCall(TimeSpan.FromSeconds(10.0), new TimerCallback(Delete));
        }

        public override void BeginConfirmation(Mobile from)
        {
            EndConfirmation(from);
        }

        public override void UseGate(Mobile m)
        {
            if (m.Region.IsPartOf(typeof(Regions.Jail)))
            {
                m.SendLocalizedMessage(1114345); // You'll need a better jailbreak plan than that!
            }
            else if (m == m_Caster)
            {
                base.UseGate(m);
                Delete();
            }
        }

        public HornOfRetreatMoongate(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            Delete();
        }
    }

    #endregion

    #endregion

    public class QuestRestartInfo
    {
        private Type m_QuestType;
        private DateTime m_RestartTime;

        public Type QuestType
        {
            get { return m_QuestType; }
            set { m_QuestType = value; }
        }

        public DateTime RestartTime
        {
            get { return m_RestartTime; }
            set { m_RestartTime = value; }
        }

        public void Reset(TimeSpan restartDelay)
        {
            if (restartDelay < TimeSpan.MaxValue)
                m_RestartTime = DateTime.UtcNow + restartDelay;
            else
                m_RestartTime = DateTime.MaxValue;
        }

        public QuestRestartInfo(Type questType, TimeSpan restartDelay)
        {
            m_QuestType = questType;
            Reset(restartDelay);
        }

        public QuestRestartInfo(Type questType, DateTime restartTime)
        {
            m_QuestType = questType;
            m_RestartTime = restartTime;
        }
    }

    public class QuestSerializer
    {
        public static object Construct(Type type)
        {
            try
            {
                return Activator.CreateInstance(type);
            }
            catch
            {
                return null;
            }
        }

        public static void Write(Type type, Type[] referenceTable, GenericWriter writer)
        {
            if (type == null)
            {
                writer.WriteEncodedInt((int)0x00);
            }
            else
            {
                for (int i = 0; i < referenceTable.Length; ++i)
                {
                    if (referenceTable[i] == type)
                    {
                        writer.WriteEncodedInt((int)0x01);
                        writer.WriteEncodedInt((int)i);
                        return;
                    }
                }

                writer.WriteEncodedInt((int)0x02);
                writer.Write(type.FullName);
            }
        }

        public static Type ReadType(Type[] referenceTable, GenericReader reader)
        {
            int encoding = reader.ReadEncodedInt();

            switch (encoding)
            {
                default:
                case 0x00: // null
                    {
                        return null;
                    }
                case 0x01: // indexed
                    {
                        int index = reader.ReadEncodedInt();

                        if (index >= 0 && index < referenceTable.Length)
                            return referenceTable[index];

                        return null;
                    }
                case 0x02: // by name
                    {
                        string fullName = reader.ReadString();

                        if (fullName == null)
                            return null;

                        return ScriptCompiler.FindTypeByFullName(fullName, false);
                    }
            }
        }

        public static QuestSystem DeserializeQuest(GenericReader reader)
        {
            int encoding = reader.ReadEncodedInt();

            switch (encoding)
            {
                default:
                case 0x00: // null
                    {
                        return null;
                    }
                case 0x01:
                    {
                        Type type = ReadType(QuestSystem.QuestTypes, reader);

                        QuestSystem qs = Construct(type) as QuestSystem;

                        if (qs != null)
                            qs.BaseDeserialize(reader);

                        return qs;
                    }
            }
        }

        public static void Serialize(QuestSystem qs, GenericWriter writer)
        {
            if (qs == null)
            {
                writer.WriteEncodedInt(0x00);
            }
            else
            {
                writer.WriteEncodedInt(0x01);

                Write(qs.GetType(), QuestSystem.QuestTypes, writer);

                qs.BaseSerialize(writer);
            }
        }

        public static QuestObjective DeserializeObjective(Type[] referenceTable, GenericReader reader)
        {
            int encoding = reader.ReadEncodedInt();

            switch (encoding)
            {
                default:
                case 0x00: // null
                    {
                        return null;
                    }
                case 0x01:
                    {
                        Type type = ReadType(referenceTable, reader);

                        QuestObjective obj = Construct(type) as QuestObjective;

                        if (obj != null)
                            obj.BaseDeserialize(reader);

                        return obj;
                    }
            }
        }

        public static void Serialize(Type[] referenceTable, QuestObjective obj, GenericWriter writer)
        {
            if (obj == null)
            {
                writer.WriteEncodedInt(0x00);
            }
            else
            {
                writer.WriteEncodedInt(0x01);

                Write(obj.GetType(), referenceTable, writer);

                obj.BaseSerialize(writer);
            }
        }

        public static QuestConversation DeserializeConversation(Type[] referenceTable, GenericReader reader)
        {
            int encoding = reader.ReadEncodedInt();

            switch (encoding)
            {
                default:
                case 0x00: // null
                    {
                        return null;
                    }
                case 0x01:
                    {
                        Type type = ReadType(referenceTable, reader);

                        QuestConversation conv = Construct(type) as QuestConversation;

                        if (conv != null)
                            conv.BaseDeserialize(reader);

                        return conv;
                    }
            }
        }

        public static void Serialize(Type[] referenceTable, QuestConversation conv, GenericWriter writer)
        {
            if (conv == null)
            {
                writer.WriteEncodedInt(0x00);
            }
            else
            {
                writer.WriteEncodedInt(0x01);

                Write(conv.GetType(), referenceTable, writer);

                conv.BaseSerialize(writer);
            }
        }
    }
}