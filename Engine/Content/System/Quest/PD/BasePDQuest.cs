using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Server;
using Server.Commands;
using Server.Commands.Generic;
using Server.Engines.MLQuests;
using Server.Engines.MLQuests.Definitions;
using Server.Engines.MLQuests.Gumps;
using Server.Engines.MLQuests.Items;
using Server.Engines.MLQuests.Mobiles;
using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Gumps;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;
using Server.Spells.Fifth;
using Server.Spells.Ninjitsu;
using Server.Spells.Seventh;

namespace Server.Engines.MLQuests
{
    public enum ObjectiveType
    {
        All,
        Any
    }

    public class MLQuest
    {
        private bool m_Deserialized;
        private bool m_SaveEnabled;

        public bool Deserialized
        {
            get { return m_Deserialized; }
            set { m_Deserialized = value; }
        }

        public bool SaveEnabled
        {
            get { return m_SaveEnabled; }
            set { m_SaveEnabled = value; }
        }

        private bool m_Activated;
        private List<BaseObjective> m_Objectives;
        private ObjectiveType m_ObjectiveType;
        private List<BaseReward> m_Rewards;

        private List<MLQuestInstance> m_Instances;

        private TextDefinition m_Title;
        private TextDefinition m_Description;
        private TextDefinition m_RefuseMessage;
        private TextDefinition m_InProgressMessage;
        private TextDefinition m_CompletionMessage;
        private TextDefinition m_CompletionNotice;

        // TODO: Flags? (Deserialized, SaveEnabled, Activated)
        private bool m_OneTimeOnly;
        private bool m_HasRestartDelay;

        public bool Activated
        {
            get { return m_Activated; }
            set { m_Activated = value; }
        }

        public List<BaseObjective> Objectives
        {
            get { return m_Objectives; }
            set { m_Objectives = value; }
        }

        public ObjectiveType ObjectiveType
        {
            get { return m_ObjectiveType; }
            set { m_ObjectiveType = value; }
        }

        public List<BaseReward> Rewards
        {
            get { return m_Rewards; }
            set { m_Rewards = value; }
        }

        public List<MLQuestInstance> Instances
        {
            get { return m_Instances; }
            set { m_Instances = value; }
        }

        public bool OneTimeOnly
        {
            get { return m_OneTimeOnly; }
            set { m_OneTimeOnly = value; }
        }

        public bool HasRestartDelay
        {
            get { return m_HasRestartDelay; }
            set { m_HasRestartDelay = value; }
        }

        public bool HasObjective<T>() where T : BaseObjective
        {
            foreach (BaseObjective obj in m_Objectives)
            {
                if (obj is T)
                    return true;
            }

            return false;
        }

        public bool IsEscort
        {
            get { return HasObjective<EscortObjective>(); }
        }

        public bool IsSkillTrainer
        {
            get { return HasObjective<GainSkillObjective>(); }
        }

        public bool RequiresCollection
        {
            get { return HasObjective<CollectObjective>() || HasObjective<DeliverObjective>(); }
        }

        public virtual bool RecordCompletion
        {
            get { return (m_OneTimeOnly || m_HasRestartDelay); }
        }

        public virtual bool IsChainTriggered { get { return false; } }
        public virtual Type NextQuest { get { return null; } }

        public TextDefinition Title { get { return m_Title; } set { m_Title = value; } }
        public TextDefinition Description { get { return m_Description; } set { m_Description = value; } }
        public TextDefinition RefusalMessage { get { return m_RefuseMessage; } set { m_RefuseMessage = value; } }
        public TextDefinition InProgressMessage { get { return m_InProgressMessage; } set { m_InProgressMessage = value; } }
        public TextDefinition CompletionMessage { get { return m_CompletionMessage; } set { m_CompletionMessage = value; } }
        public TextDefinition CompletionNotice { get { return m_CompletionNotice; } set { m_CompletionNotice = value; } }

        public static readonly TextDefinition CompletionNoticeDefault = new TextDefinition(1072273); // You've completed a quest!  Don't forget to collect your reward.
        public static readonly TextDefinition CompletionNoticeShort = new TextDefinition(1046258); // Your quest is complete.
        public static readonly TextDefinition CompletionNoticeShortReturn = new TextDefinition(1073775); // Your quest is complete. Return for your reward.
        public static readonly TextDefinition CompletionNoticeCraft = new TextDefinition(1073967); // You obtained what you seek, now receive your reward.

        public MLQuest()
        {
            m_Activated = false;
            m_Objectives = new List<BaseObjective>();
            m_ObjectiveType = ObjectiveType.All;
            m_Rewards = new List<BaseReward>();
            m_CompletionNotice = CompletionNoticeDefault;

            m_Instances = new List<MLQuestInstance>();

            m_SaveEnabled = true;
        }

        public virtual void Generate()
        {
            if (MLQuestSystem.Debug)
                Console.WriteLine("INFO: Generating quest: {0}", GetType());
        }

        #region Generation Methods

        public void PutSpawner(Spawner s, Point3D loc, Map map)
        {
            string name = String.Format("MLQS-{0}", GetType().Name);

            // Auto cleanup on regeneration
            List<Item> toDelete = new List<Item>();

            foreach (Item item in map.GetItemsInRange(loc, 0))
            {
                if (item is Spawner && item.Name == name)
                    toDelete.Add(item);
            }

            foreach (Item item in toDelete)
                item.Delete();

            s.Name = name;
            s.MoveToWorld(loc, map);
        }

        public void PutDeco(Item deco, Point3D loc, Map map)
        {
            // Auto cleanup on regeneration
            List<Item> toDelete = new List<Item>();

            foreach (Item item in map.GetItemsInRange(loc, 0))
            {
                if (item.ItemID == deco.ItemID && item.Z == loc.Z)
                    toDelete.Add(item);
            }

            foreach (Item item in toDelete)
                item.Delete();

            deco.MoveToWorld(loc, map);
        }

        #endregion

        public MLQuestInstance CreateInstance(IQuestGiver quester, PlayerMobile pm)
        {
            return new MLQuestInstance(this, quester, pm);
        }

        public bool CanOffer(IQuestGiver quester, PlayerMobile pm, bool message)
        {
            return CanOffer(quester, pm, MLQuestSystem.GetContext(pm), message);
        }

        public virtual bool CanOffer(IQuestGiver quester, PlayerMobile pm, MLQuestContext context, bool message)
        {
            if (!m_Activated || quester.Deleted)
                return false;

            if (context != null)
            {
                if (context.IsFull)
                {
                    if (message)
                        MLQuestSystem.Tell(quester, pm, 1080107); // I'm sorry, I have nothing for you at this time.

                    return false;
                }

                MLQuest checkQuest = this;

                while (checkQuest != null)
                {
                    DateTime nextAvailable;

                    if (context.HasDoneQuest(checkQuest, out nextAvailable))
                    {
                        if (checkQuest.OneTimeOnly)
                        {
                            if (message)
                                MLQuestSystem.Tell(quester, pm, 1075454); // I cannot offer you the quest again.

                            return false;
                        }
                        else if (nextAvailable > DateTime.UtcNow)
                        {
                            if (message)
                                MLQuestSystem.Tell(quester, pm, 1075575); // I'm sorry, but I don't have anything else for you right now. Could you check back with me in a few minutes?

                            return false;
                        }
                    }

                    if (checkQuest.NextQuest == null)
                        break;

                    checkQuest = MLQuestSystem.FindQuest(checkQuest.NextQuest);
                }
            }

            foreach (BaseObjective obj in m_Objectives)
            {
                if (!obj.CanOffer(quester, pm, message))
                    return false;
            }

            return true;
        }

        public virtual void SendOffer(IQuestGiver quester, PlayerMobile pm)
        {
            pm.SendGump(new QuestOfferGump(this, quester, pm));
        }

        public virtual void OnAccept(IQuestGiver quester, PlayerMobile pm)
        {
            if (!CanOffer(quester, pm, true))
                return;

            MLQuestInstance instance = CreateInstance(quester, pm);

            pm.SendLocalizedMessage(1049019); // You have accepted the Quest.
            pm.SendSound(0x2E7); // private sound

            OnAccepted(instance);

            foreach (BaseObjectiveInstance obj in instance.Objectives)
                obj.OnQuestAccepted();
        }

        public virtual void OnAccepted(MLQuestInstance instance)
        {
        }

        public virtual void OnRefuse(IQuestGiver quester, PlayerMobile pm)
        {
            pm.SendGump(new QuestConversationGump(this, pm, RefusalMessage));
        }

        public virtual void GetRewards(MLQuestInstance instance)
        {
            instance.SendRewardGump();
        }

        public virtual void OnRewardClaimed(MLQuestInstance instance)
        {
        }

        public virtual void OnCancel(MLQuestInstance instance)
        {
        }

        public virtual void OnQuesterDeleted(MLQuestInstance instance)
        {
        }

        public virtual void OnPlayerDeath(MLQuestInstance instance)
        {
        }

        public virtual TimeSpan GetRestartDelay()
        {
            return TimeSpan.FromSeconds(Utility.Random(1, 5) * 30);
        }

        public static void Serialize(GenericWriter writer, MLQuest quest)
        {
            MLQuestSystem.WriteQuestRef(writer, quest);
            writer.Write(quest.Version);
        }

        public static void Deserialize(GenericReader reader, int version)
        {
            MLQuest quest = MLQuestSystem.ReadQuestRef(reader);
            int oldVersion = reader.ReadInt();

            if (quest == null)
                return; // not saved or no longer exists

            quest.Refresh(oldVersion);
            quest.m_Deserialized = true;
        }

        public virtual int Version { get { return 0; } }

        public virtual void Refresh(int oldVersion)
        {
        }
    }

    public class MLQuestPersistence : Item
    {
        private static MLQuestPersistence m_Instance;

        public static void EnsureExistence()
        {
            if (m_Instance == null)
                m_Instance = new MLQuestPersistence();
        }

        public override string DefaultName
        {
            get { return "ML quests persistence - Internal"; }
        }

        private MLQuestPersistence()
            : base(1)
        {
            Movable = false;
        }

        public MLQuestPersistence(Serial serial)
            : base(serial)
        {
            m_Instance = this;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)2); // version
            writer.Write(MLQuestSystem.Contexts.Count);

            foreach (MLQuestContext context in MLQuestSystem.Contexts.Values)
                context.Serialize(writer);

            writer.Write(MLQuestSystem.Quests.Count);

            foreach (MLQuest quest in MLQuestSystem.Quests.Values)
                MLQuest.Serialize(writer, quest);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
            int contexts = reader.ReadInt();

            for (int i = 0; i < contexts; ++i)
            {
                MLQuestContext context = new MLQuestContext(reader, version);

                if (context.Owner != null)
                    MLQuestSystem.Contexts[context.Owner] = context;
            }

            int quests = reader.ReadInt();

            for (int i = 0; i < quests; ++i)
                MLQuest.Deserialize(reader, version);
        }
    }

    public static class MLQuestSystem
    {
        public static bool Enabled { get { return Core.ML; } }

        public const int MaxConcurrentQuests = 10;
        public const int SpeechColor = 0x3B2;

        public static readonly bool AutoGenerateNew = true;
        public static readonly bool Debug = false;

        private static Dictionary<Type, MLQuest> m_Quests;
        private static Dictionary<Type, List<MLQuest>> m_QuestGivers;
        private static Dictionary<PlayerMobile, MLQuestContext> m_Contexts;

        public static readonly List<MLQuest> EmptyList = new List<MLQuest>();

        public static Dictionary<Type, MLQuest> Quests
        {
            get { return m_Quests; }
        }

        public static Dictionary<Type, List<MLQuest>> QuestGivers
        {
            get { return m_QuestGivers; }
        }

        public static Dictionary<PlayerMobile, MLQuestContext> Contexts
        {
            get { return m_Contexts; }
        }

        static MLQuestSystem()
        {
            m_Quests = new Dictionary<Type, MLQuest>();
            m_QuestGivers = new Dictionary<Type, List<MLQuest>>();
            m_Contexts = new Dictionary<PlayerMobile, MLQuestContext>();

            string cfgPath = Path.Combine(Core.BaseDirectory, Path.Combine("Data", "MLQuests.cfg"));

            Type baseQuestType = typeof(MLQuest);
            Type baseQuesterType = typeof(IQuestGiver);

            if (File.Exists(cfgPath))
            {
                using (StreamReader sr = new StreamReader(cfgPath))
                {
                    string line;

                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Length == 0 || line.StartsWith("#"))
                            continue;

                        string[] split = line.Split('\t');

                        Type type = ScriptCompiler.FindTypeByName(split[0]);

                        if (type == null || !baseQuestType.IsAssignableFrom(type))
                        {
                            if (Debug)
                                Console.WriteLine("Warning: {1} quest type '{0}'", split[0], (type == null) ? "Unknown" : "Invalid");

                            continue;
                        }

                        MLQuest quest = null;

                        try
                        {
                            quest = Activator.CreateInstance(type) as MLQuest;
                        }
                        catch { }

                        if (quest == null)
                            continue;

                        Register(type, quest);

                        for (int i = 1; i < split.Length; ++i)
                        {
                            Type questerType = ScriptCompiler.FindTypeByName(split[i]);

                            if (questerType == null || !baseQuesterType.IsAssignableFrom(questerType))
                            {
                                if (Debug)
                                    Console.WriteLine("Warning: {1} quester type '{0}'", split[i], (questerType == null) ? "Unknown" : "Invalid");

                                continue;
                            }

                            RegisterQuestGiver(quest, questerType);
                        }
                    }
                }
            }
        }

        private static void Register(Type type, MLQuest quest)
        {
            m_Quests[type] = quest;
        }

        private static void RegisterQuestGiver(MLQuest quest, Type questerType)
        {
            List<MLQuest> questList;

            if (!m_QuestGivers.TryGetValue(questerType, out questList))
                m_QuestGivers[questerType] = questList = new List<MLQuest>();

            questList.Add(quest);
        }

        public static void Register(MLQuest quest, params Type[] questerTypes)
        {
            Register(quest.GetType(), quest);

            foreach (Type questerType in questerTypes)
                RegisterQuestGiver(quest, questerType);
        }

        public static void Initialize()
        {
            if (!Enabled)
                return;

            if (AutoGenerateNew)
            {
                foreach (MLQuest quest in m_Quests.Values)
                {
                    if (quest != null && !quest.Deserialized)
                        quest.Generate();
                }
            }

            MLQuestPersistence.EnsureExistence();

            CommandSystem.Register("MLQuestsInfo", AccessLevel.Administrator, new CommandEventHandler(MLQuestsInfo_OnCommand));
            CommandSystem.Register("SaveQuest", AccessLevel.Administrator, new CommandEventHandler(SaveQuest_OnCommand));
            CommandSystem.Register("SaveAllQuests", AccessLevel.Administrator, new CommandEventHandler(SaveAllQuests_OnCommand));
            CommandSystem.Register("InvalidQuestItems", AccessLevel.Administrator, new CommandEventHandler(InvalidQuestItems_OnCommand));

            TargetCommands.Register(new ViewQuestsCommand());
            TargetCommands.Register(new ViewContextCommand());

            EventSink.QuestGumpRequest += new QuestGumpRequestHandler(EventSink_QuestGumpRequest);
        }

        [Usage("MLQuestsInfo")]
        [Description("Displays general information about the ML quest system, or a quest by type name.")]
        public static void MLQuestsInfo_OnCommand(CommandEventArgs e)
        {
            Mobile m = e.Mobile;

            if (e.Length == 0)
            {
                m.SendMessage("Quest table length: {0}", m_Quests.Count);
                return;
            }

            Type index = ScriptCompiler.FindTypeByName(e.GetString(0));
            MLQuest quest;

            if (index == null || !m_Quests.TryGetValue(index, out quest))
            {
                m.SendMessage("Invalid quest type name.");
                return;
            }

            m.SendMessage("Activated: {0}", quest.Activated);
            m.SendMessage("Number of objectives: {0}", quest.Objectives.Count);
            m.SendMessage("Objective type: {0}", quest.ObjectiveType);
            m.SendMessage("Number of active instances: {0}", quest.Instances.Count);
        }

        public class ViewQuestsCommand : BaseCommand
        {
            public ViewQuestsCommand()
            {
                AccessLevel = AccessLevel.GameMaster;
                Supports = CommandSupport.Simple;
                Commands = new string[] { "ViewQuests" };
                ObjectTypes = ObjectTypes.Mobiles;
                Usage = "ViewQuests";
                Description = "Displays a targeted mobile's quest overview.";
            }

            public override void Execute(CommandEventArgs e, object obj)
            {
                Mobile from = e.Mobile;
                PlayerMobile pm = obj as PlayerMobile;

                if (pm == null)
                {
                    LogFailure("That is not a player.");
                    return;
                }

                CommandLogging.WriteLine(from, "{0} {1} viewing quest overview of {2}", from.AccessLevel, CommandLogging.Format(from), CommandLogging.Format(pm));
                from.SendGump(new QuestLogGump(pm, false));
            }
        }

        private class ViewContextCommand : BaseCommand
        {
            public ViewContextCommand()
            {
                AccessLevel = AccessLevel.GameMaster;
                Supports = CommandSupport.Simple;
                Commands = new string[] { "ViewMLContext" };
                ObjectTypes = ObjectTypes.Mobiles;
                Usage = "ViewMLContext";
                Description = "Opens the ML quest context for a targeted mobile.";
            }

            public override void Execute(CommandEventArgs e, object obj)
            {
                PlayerMobile pm = obj as PlayerMobile;

                if (pm == null)
                    LogFailure("They have no ML quest context.");
                else
                    e.Mobile.SendGump(new PropertiesGump(e.Mobile, GetOrCreateContext(pm)));
            }
        }

        [Usage("SaveQuest <type> [saveEnabled=true]")]
        [Description("Allows serialization for a specific quest to be turned on or off.")]
        public static void SaveQuest_OnCommand(CommandEventArgs e)
        {
            Mobile m = e.Mobile;

            if (e.Length == 0 || e.Length > 2)
            {
                m.SendMessage("Syntax: SaveQuest <id> [saveEnabled=true]");
                return;
            }

            Type index = ScriptCompiler.FindTypeByName(e.GetString(0));
            MLQuest quest;

            if (index == null || !m_Quests.TryGetValue(index, out quest))
            {
                m.SendMessage("Invalid quest type name.");
                return;
            }

            bool enable = (e.Length == 2) ? e.GetBoolean(1) : true;

            quest.SaveEnabled = enable;
            m.SendMessage("Serialization for quest {0} is now {1}.", quest.GetType().Name, enable ? "enabled" : "disabled");

            if (AutoGenerateNew && !enable)
                m.SendMessage("Please note that automatic generation of new quests is ON. This quest will be regenerated on the next server start.");
        }

        [Usage("SaveAllQuests [saveEnabled=true]")]
        [Description("Allows serialization for all quests to be turned on or off.")]
        public static void SaveAllQuests_OnCommand(CommandEventArgs e)
        {
            Mobile m = e.Mobile;

            if (e.Length > 1)
            {
                m.SendMessage("Syntax: SaveAllQuests [saveEnabled=true]");
                return;
            }

            bool enable = (e.Length == 1) ? e.GetBoolean(0) : true;

            foreach (MLQuest quest in m_Quests.Values)
                quest.SaveEnabled = enable;

            m.SendMessage("Serialization for all quests is now {0}.", enable ? "enabled" : "disabled");

            if (AutoGenerateNew && !enable)
                m.SendMessage("Please note that automatic generation of new quests is ON. All quests will be regenerated on the next server start.");
        }

        [Usage("InvalidQuestItems")]
        [Description("Provides an overview of all quest items not located in the top-level of a player's backpack.")]
        public static void InvalidQuestItems_OnCommand(CommandEventArgs e)
        {
            Mobile m = e.Mobile;

            ArrayList found = new ArrayList();

            foreach (Item item in World.Items.Values)
            {
                if (item.QuestItem)
                {
                    Backpack pack = item.Parent as Backpack;

                    if (pack != null)
                    {
                        PlayerMobile player = pack.Parent as PlayerMobile;

                        if (player != null && player.Backpack == pack)
                            continue;
                    }

                    found.Add(item);
                }
            }

            if (found.Count == 0)
                m.SendMessage("No matching objects found.");
            else
                m.SendGump(new InterfaceGump(m, new string[] { "Object" }, found, 0, null));
        }

        private static bool FindQuest(IQuestGiver quester, PlayerMobile pm, MLQuestContext context, out MLQuest quest, out MLQuestInstance entry)
        {
            quest = null;
            entry = null;

            List<MLQuest> quests = quester.MLQuests;
            Type questerType = quester.GetType();

            // 1. Check quests in progress with this NPC (overriding deliveries is intended)
            if (context != null)
            {
                foreach (MLQuest questEntry in quests)
                {
                    MLQuestInstance instance = context.FindInstance(questEntry);

                    if (instance != null && (instance.Quester == quester || (!questEntry.IsEscort && instance.QuesterType == questerType)))
                    {
                        entry = instance;
                        quest = questEntry;
                        return true;
                    }
                }
            }

            // 2. Check deliveries (overriding chain offers is intended)
            if ((entry = HandleDelivery(pm, quester, questerType)) != null)
            {
                quest = entry.Quest;
                return true;
            }

            // 3. Check chain quest offers
            if (context != null)
            {
                foreach (MLQuest questEntry in quests)
                {
                    if (questEntry.IsChainTriggered && context.ChainOffers.Contains(questEntry))
                    {
                        quest = questEntry;
                        return true;
                    }
                }
            }

            // 4. Random quest
            quest = RandomStarterQuest(quester, pm, context);

            return (quest != null);
        }

        public static void OnDoubleClick(IQuestGiver quester, PlayerMobile pm)
        {
            if (quester.Deleted || !pm.Alive)
                return;

            MLQuestContext context = GetContext(pm);

            MLQuest quest;
            MLQuestInstance entry;

            if (!FindQuest(quester, pm, context, out quest, out entry))
            {
                Tell(quester, pm, 1080107); // I'm sorry, I have nothing for you at this time.
                return;
            }

            if (entry != null)
            {
                TurnToFace(quester, pm);

                if (entry.Failed)
                    return; // Note: OSI sends no gump at all for failed quests, they have to be cancelled in the quest overview
                else if (entry.ClaimReward)
                    entry.SendRewardOffer();
                else if (entry.IsCompleted())
                    entry.SendReportBackGump();
                else
                    entry.SendProgressGump();
            }
            else if (quest.CanOffer(quester, pm, context, true))
            {
                TurnToFace(quester, pm);

                quest.SendOffer(quester, pm);
            }
        }

        public static bool CanMarkQuestItem(PlayerMobile pm, Item item, Type type)
        {
            MLQuestContext context = GetContext(pm);

            if (context != null)
            {
                foreach (MLQuestInstance quest in context.QuestInstances)
                {
                    if (!quest.ClaimReward && quest.AllowsQuestItem(item, type))
                        return true;
                }
            }

            return false;
        }

        private static void OnMarkQuestItem(PlayerMobile pm, Item item, Type type)
        {
            MLQuestContext context = GetContext(pm);

            if (context == null)
                return;

            List<MLQuestInstance> instances = context.QuestInstances;

            // We don't foreach because CheckComplete() can potentially modify the MLQuests list
            for (int i = instances.Count - 1; i >= 0; --i)
            {
                MLQuestInstance instance = instances[i];

                if (instance.ClaimReward)
                    continue;

                foreach (BaseObjectiveInstance objective in instance.Objectives)
                {
                    if (!objective.Expired && objective.AllowsQuestItem(item, type))
                    {
                        objective.CheckComplete(); // yes, this can happen multiple times (for multiple quests)
                        break;
                    }
                }
            }
        }

        public static bool MarkQuestItem(PlayerMobile pm, Item item)
        {
            Type type = item.GetType();

            if (CanMarkQuestItem(pm, item, type))
            {
                item.QuestItem = true;
                OnMarkQuestItem(pm, item, type);

                return true;
            }

            return false;
        }

        public static void HandleSkillGain(PlayerMobile pm, SkillName skill)
        {
            MLQuestContext context = GetContext(pm);

            if (context == null)
                return;

            List<MLQuestInstance> instances = context.QuestInstances;

            for (int i = instances.Count - 1; i >= 0; --i)
            {
                MLQuestInstance instance = instances[i];

                if (instance.ClaimReward)
                    continue;

                foreach (BaseObjectiveInstance objective in instance.Objectives)
                {
                    if (!objective.Expired && objective is GainSkillObjectiveInstance && ((GainSkillObjectiveInstance)objective).Handles(skill))
                    {
                        objective.CheckComplete();
                        break;
                    }
                }
            }
        }

        public static void HandleKill(PlayerMobile pm, Mobile mob)
        {
            MLQuestContext context = GetContext(pm);

            if (context == null)
                return;

            List<MLQuestInstance> instances = context.QuestInstances;

            Type type = null;

            for (int i = instances.Count - 1; i >= 0; --i)
            {
                MLQuestInstance instance = instances[i];

                if (instance.ClaimReward)
                    continue;

                /* A kill only counts for a single objective within a quest,
                 * but it can count for multiple quests. This is something not
                 * currently observable on OSI, so it is assumed behavior.
                 */
                foreach (BaseObjectiveInstance objective in instance.Objectives)
                {
                    if (!objective.Expired && objective is KillObjectiveInstance)
                    {
                        KillObjectiveInstance kill = (KillObjectiveInstance)objective;

                        if (type == null)
                            type = mob.GetType();

                        if (kill.AddKill(mob, type))
                        {
                            kill.CheckComplete();
                            break;
                        }
                    }
                }
            }
        }

        public static MLQuestInstance HandleDelivery(PlayerMobile pm, IQuestGiver quester, Type questerType)
        {
            MLQuestContext context = GetContext(pm);

            if (context == null)
                return null;

            List<MLQuestInstance> instances = context.QuestInstances;
            MLQuestInstance deliverInstance = null;

            for (int i = instances.Count - 1; i >= 0; --i)
            {
                MLQuestInstance instance = instances[i];

                // Do NOT skip quests on ClaimReward, because the quester still needs the quest ref!
                //if ( instance.ClaimReward )
                //	continue;

                foreach (BaseObjectiveInstance objective in instance.Objectives)
                {
                    // Note: On OSI, expired deliveries can still be completed. Bug?
                    if (!objective.Expired && objective is DeliverObjectiveInstance)
                    {
                        DeliverObjectiveInstance deliver = (DeliverObjectiveInstance)objective;

                        if (deliver.IsDestination(quester, questerType))
                        {
                            if (!deliver.HasCompleted) // objective completes only once
                            {
                                deliver.HasCompleted = true;
                                deliver.CheckComplete();

                                // The quest is continued with this NPC (important for chains)
                                instance.Quester = quester;
                            }

                            if (deliverInstance == null)
                                deliverInstance = instance;

                            break; // don't return, we may have to complete more deliveries
                        }
                    }
                }
            }

            return deliverInstance;
        }

        public static MLQuestContext GetContext(PlayerMobile pm)
        {
            MLQuestContext context;
            m_Contexts.TryGetValue(pm, out context);

            return context;
        }

        public static MLQuestContext GetOrCreateContext(PlayerMobile pm)
        {
            MLQuestContext context;

            if (!m_Contexts.TryGetValue(pm, out context))
                m_Contexts[pm] = context = new MLQuestContext(pm);

            return context;
        }

        public static void HandleDeath(PlayerMobile pm)
        {
            MLQuestContext context = GetContext(pm);

            if (context != null)
                context.HandleDeath();
        }

        public static void HandleDeletion(PlayerMobile pm)
        {
            MLQuestContext context = GetContext(pm);

            if (context != null)
            {
                context.HandleDeletion();
                m_Contexts.Remove(pm);
            }
        }

        public static void HandleDeletion(IQuestGiver quester)
        {
            foreach (MLQuest quest in quester.MLQuests)
            {
                List<MLQuestInstance> instances = quest.Instances;

                for (int i = instances.Count - 1; i >= 0; --i)
                {
                    MLQuestInstance instance = instances[i];

                    if (instance.Quester == quester)
                        instance.OnQuesterDeleted();
                }
            }
        }

        public static void EventSink_QuestGumpRequest(QuestGumpRequestArgs args)
        {
            PlayerMobile pm = args.Mobile as PlayerMobile;

            if (!Enabled || pm == null)
                return;

            pm.SendGump(new QuestLogGump(pm));
        }

        private static List<MLQuest> m_EligiblePool = new List<MLQuest>();

        public static MLQuest RandomStarterQuest(IQuestGiver quester, PlayerMobile pm, MLQuestContext context)
        {
            List<MLQuest> quests = quester.MLQuests;

            if (quests.Count == 0)
                return null;

            m_EligiblePool.Clear();
            MLQuest fallback = null;

            foreach (MLQuest quest in quests)
            {
                if (quest.IsChainTriggered || (context != null && context.IsDoingQuest(quest)))
                    continue;

                /*
                 * Save first quest that reaches the CanOffer call.
                 * If no quests are valid at all, return this quest for displaying the CanOffer error message.
                 */
                if (fallback == null)
                    fallback = quest;

                if (quest.CanOffer(quester, pm, context, false))
                    m_EligiblePool.Add(quest);
            }

            if (m_EligiblePool.Count == 0)
                return fallback;

            return m_EligiblePool[Utility.Random(m_EligiblePool.Count)];
        }

        public static void TurnToFace(IQuestGiver quester, Mobile mob)
        {
            if (quester is Mobile)
            {
                Mobile m = (Mobile)quester;
                m.Direction = m.GetDirectionTo(mob);
            }
        }

        public static void Tell(IQuestGiver quester, PlayerMobile pm, int cliloc)
        {
            TurnToFace(quester, pm);

            if (quester is Mobile)
                ((Mobile)quester).PrivateOverheadMessage(MessageType.Regular, SpeechColor, cliloc, pm.NetState);
            else if (quester is Item)
                MessageHelper.SendLocalizedMessageTo((Item)quester, pm, cliloc, SpeechColor);
            else
                pm.SendLocalizedMessage(cliloc, "", SpeechColor);
        }

        public static void Tell(IQuestGiver quester, PlayerMobile pm, int cliloc, string args)
        {
            TurnToFace(quester, pm);

            if (quester is Mobile)
                ((Mobile)quester).PrivateOverheadMessage(MessageType.Regular, SpeechColor, cliloc, args, pm.NetState);
            else if (quester is Item)
                MessageHelper.SendLocalizedMessageTo((Item)quester, pm, cliloc, args, SpeechColor);
            else
                pm.SendLocalizedMessage(cliloc, args, SpeechColor);
        }

        public static void Tell(IQuestGiver quester, PlayerMobile pm, string message)
        {
            TurnToFace(quester, pm);

            if (quester is Mobile)
                ((Mobile)quester).PrivateOverheadMessage(MessageType.Regular, SpeechColor, false, message, pm.NetState);
            else if (quester is Item)
                MessageHelper.SendMessageTo((Item)quester, pm, message, SpeechColor);
            else
                pm.SendMessage(SpeechColor, message);
        }

        public static void TellDef(IQuestGiver quester, PlayerMobile pm, TextDefinition def)
        {
            if (def == null)
                return;

            if (def.Number > 0)
                Tell(quester, pm, def.Number);
            else if (def.String != null)
                Tell(quester, pm, def.String);
        }

        public static void WriteQuestRef(GenericWriter writer, MLQuest quest)
        {
            writer.Write((quest != null && quest.SaveEnabled) ? quest.GetType().FullName : null);
        }

        public static MLQuest ReadQuestRef(GenericReader reader)
        {
            string typeName = reader.ReadString();

            if (typeName == null)
                return null; // not serialized

            Type questType = ScriptCompiler.FindTypeByFullName(typeName);

            if (questType == null)
                return null; // no longer a type

            return FindQuest(questType);
        }

        public static MLQuest FindQuest(Type questType)
        {
            MLQuest result;
            m_Quests.TryGetValue(questType, out result);

            return result;
        }

        public static List<MLQuest> FindQuestList(Type questerType)
        {
            List<MLQuest> result;

            if (m_QuestGivers.TryGetValue(questerType, out result))
                return result;

            return EmptyList;
        }
    }

    [Flags]
    public enum MLQuestFlag
    {
        None = 0x00,
        Spellweaving = 0x01,
        SummonFey = 0x02,
        SummonFiend = 0x04,
        BedlamAccess = 0x08
    }

    [PropertyObject]
    public class MLQuestContext
    {
        private class MLDoneQuestInfo
        {
            public MLQuest m_Quest;
            public DateTime m_NextAvailable;

            public MLDoneQuestInfo(MLQuest quest, DateTime nextAvailable)
            {
                m_Quest = quest;
                m_NextAvailable = nextAvailable;
            }

            public void Serialize(GenericWriter writer)
            {
                MLQuestSystem.WriteQuestRef(writer, m_Quest);
                writer.Write(m_NextAvailable);
            }

            public static MLDoneQuestInfo Deserialize(GenericReader reader, int version)
            {
                MLQuest quest = MLQuestSystem.ReadQuestRef(reader);
                DateTime nextAvailable = reader.ReadDateTime();

                if (quest == null || !quest.RecordCompletion)
                    return null; // forget about this record

                return new MLDoneQuestInfo(quest, nextAvailable);
            }
        }

        private PlayerMobile m_Owner;
        private List<MLQuestInstance> m_QuestInstances;
        private List<MLDoneQuestInfo> m_DoneQuests;
        private List<MLQuest> m_ChainOffers;
        private MLQuestFlag m_Flags;

        public PlayerMobile Owner
        {
            get { return m_Owner; }
        }

        public List<MLQuestInstance> QuestInstances
        {
            get { return m_QuestInstances; }
        }

        public List<MLQuest> ChainOffers
        {
            get { return m_ChainOffers; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsFull
        {
            get { return m_QuestInstances.Count >= MLQuestSystem.MaxConcurrentQuests; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Spellweaving
        {
            get { return GetFlag(MLQuestFlag.Spellweaving); }
            set { SetFlag(MLQuestFlag.Spellweaving, value); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool SummonFey
        {
            get { return GetFlag(MLQuestFlag.SummonFey); }
            set { SetFlag(MLQuestFlag.SummonFey, value); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool SummonFiend
        {
            get { return GetFlag(MLQuestFlag.SummonFiend); }
            set { SetFlag(MLQuestFlag.SummonFiend, value); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool BedlamAccess
        {
            get { return GetFlag(MLQuestFlag.BedlamAccess); }
            set { SetFlag(MLQuestFlag.BedlamAccess, value); }
        }

        public MLQuestContext(PlayerMobile owner)
        {
            m_Owner = owner;
            m_QuestInstances = new List<MLQuestInstance>();
            m_DoneQuests = new List<MLDoneQuestInfo>();
            m_ChainOffers = new List<MLQuest>();
            m_Flags = MLQuestFlag.None;
        }

        public bool HasDoneQuest(Type questType)
        {
            MLQuest quest = MLQuestSystem.FindQuest(questType);

            return (quest != null && HasDoneQuest(quest));
        }

        public bool HasDoneQuest(MLQuest quest)
        {
            foreach (MLDoneQuestInfo info in m_DoneQuests)
            {
                if (info.m_Quest == quest)
                    return true;
            }

            return false;
        }

        public bool HasDoneQuest(MLQuest quest, out DateTime nextAvailable)
        {
            nextAvailable = DateTime.MinValue;

            foreach (MLDoneQuestInfo info in m_DoneQuests)
            {
                if (info.m_Quest == quest)
                {
                    nextAvailable = info.m_NextAvailable;
                    return true;
                }
            }

            return false;
        }

        public void SetDoneQuest(MLQuest quest)
        {
            SetDoneQuest(quest, DateTime.MinValue);
        }

        public void SetDoneQuest(MLQuest quest, DateTime nextAvailable)
        {
            foreach (MLDoneQuestInfo info in m_DoneQuests)
            {
                if (info.m_Quest == quest)
                {
                    info.m_NextAvailable = nextAvailable;
                    return;
                }
            }

            m_DoneQuests.Add(new MLDoneQuestInfo(quest, nextAvailable));
        }

        public void RemoveDoneQuest(MLQuest quest)
        {
            for (int i = m_DoneQuests.Count - 1; i >= 0; --i)
            {
                MLDoneQuestInfo info = m_DoneQuests[i];

                if (info.m_Quest == quest)
                    m_DoneQuests.RemoveAt(i);
            }
        }

        public void HandleDeath()
        {
            for (int i = m_QuestInstances.Count - 1; i >= 0; --i)
                m_QuestInstances[i].OnPlayerDeath();
        }

        public void HandleDeletion()
        {
            for (int i = m_QuestInstances.Count - 1; i >= 0; --i)
                m_QuestInstances[i].Remove();
        }

        public MLQuestInstance FindInstance(Type questType)
        {
            MLQuest quest = MLQuestSystem.FindQuest(questType);

            if (quest == null)
                return null;

            return FindInstance(quest);
        }

        public MLQuestInstance FindInstance(MLQuest quest)
        {
            foreach (MLQuestInstance instance in m_QuestInstances)
            {
                if (instance.Quest == quest)
                    return instance;
            }

            return null;
        }

        public bool IsDoingQuest(Type questType)
        {
            MLQuest quest = MLQuestSystem.FindQuest(questType);

            return (quest != null && IsDoingQuest(quest));
        }

        public bool IsDoingQuest(MLQuest quest)
        {
            return (FindInstance(quest) != null);
        }

        public void Serialize(GenericWriter writer)
        {
            // Version info is written in MLQuestPersistence.Serialize

            writer.WriteMobile<PlayerMobile>(m_Owner);
            writer.Write(m_QuestInstances.Count);

            foreach (MLQuestInstance instance in m_QuestInstances)
                instance.Serialize(writer);

            writer.Write(m_DoneQuests.Count);

            foreach (MLDoneQuestInfo info in m_DoneQuests)
                info.Serialize(writer);

            writer.Write(m_ChainOffers.Count);

            foreach (MLQuest quest in m_ChainOffers)
                MLQuestSystem.WriteQuestRef(writer, quest);

            writer.WriteEncodedInt((int)m_Flags);
        }

        public MLQuestContext(GenericReader reader, int version)
        {
            m_Owner = reader.ReadMobile<PlayerMobile>();
            m_QuestInstances = new List<MLQuestInstance>();
            m_DoneQuests = new List<MLDoneQuestInfo>();
            m_ChainOffers = new List<MLQuest>();

            int instances = reader.ReadInt();

            for (int i = 0; i < instances; ++i)
            {
                MLQuestInstance instance = MLQuestInstance.Deserialize(reader, version, m_Owner);

                if (instance != null)
                    m_QuestInstances.Add(instance);
            }

            int doneQuests = reader.ReadInt();

            for (int i = 0; i < doneQuests; ++i)
            {
                MLDoneQuestInfo info = MLDoneQuestInfo.Deserialize(reader, version);

                if (info != null)
                    m_DoneQuests.Add(info);
            }

            int chainOffers = reader.ReadInt();

            for (int i = 0; i < chainOffers; ++i)
            {
                MLQuest quest = MLQuestSystem.ReadQuestRef(reader);

                if (quest != null && quest.IsChainTriggered)
                    m_ChainOffers.Add(quest);
            }

            m_Flags = (MLQuestFlag)reader.ReadEncodedInt();
        }

        public bool GetFlag(MLQuestFlag flag)
        {
            return ((m_Flags & flag) != 0);
        }

        public void SetFlag(MLQuestFlag flag, bool value)
        {
            if (value)
                m_Flags |= flag;
            else
                m_Flags &= ~flag;
        }
    }

    [Flags]
    public enum MLQuestInstanceFlags : byte
    {
        None = 0x00,
        ClaimReward = 0x01,
        Removed = 0x02,
        Failed = 0x04
    }

    public class MLQuestInstance
    {
        private MLQuest m_Quest;

        private IQuestGiver m_Quester;
        private Type m_QuesterType;
        private PlayerMobile m_Player;

        private DateTime m_Accepted;
        private MLQuestInstanceFlags m_Flags;

        private BaseObjectiveInstance[] m_ObjectiveInstances;

        private Timer m_Timer;

        public MLQuestInstance(MLQuest quest, IQuestGiver quester, PlayerMobile player)
        {
            m_Quest = quest;

            m_Quester = quester;
            m_QuesterType = (quester == null) ? null : quester.GetType();
            m_Player = player;

            m_Accepted = DateTime.UtcNow;
            m_Flags = MLQuestInstanceFlags.None;

            m_ObjectiveInstances = new BaseObjectiveInstance[quest.Objectives.Count];

            BaseObjectiveInstance obj;
            bool timed = false;

            for (int i = 0; i < quest.Objectives.Count; ++i)
            {
                m_ObjectiveInstances[i] = obj = quest.Objectives[i].CreateInstance(this);

                if (obj.IsTimed)
                    timed = true;
            }

            Register();

            if (timed)
                m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5), Slice);
        }

        private void Register()
        {
            if (m_Quest != null && m_Quest.Instances != null)
                m_Quest.Instances.Add(this);

            if (m_Player != null)
                PlayerContext.QuestInstances.Add(this);
        }

        private void Unregister()
        {
            if (m_Quest != null && m_Quest.Instances != null)
                m_Quest.Instances.Remove(this);

            if (m_Player != null)
                PlayerContext.QuestInstances.Remove(this);

            Removed = true;
        }

        public MLQuest Quest
        {
            get { return m_Quest; }
            set { m_Quest = value; }
        }

        public IQuestGiver Quester
        {
            get { return m_Quester; }
            set
            {
                m_Quester = value;
                m_QuesterType = (value == null) ? null : value.GetType();
            }
        }

        public Type QuesterType
        {
            get { return m_QuesterType; }
        }

        public PlayerMobile Player
        {
            get { return m_Player; }
            set { m_Player = value; }
        }

        public MLQuestContext PlayerContext
        {
            get { return MLQuestSystem.GetOrCreateContext(m_Player); }
        }

        public DateTime Accepted
        {
            get { return m_Accepted; }
            set { m_Accepted = value; }
        }

        public bool ClaimReward
        {
            get { return GetFlag(MLQuestInstanceFlags.ClaimReward); }
            set { SetFlag(MLQuestInstanceFlags.ClaimReward, value); }
        }

        public bool Removed
        {
            get { return GetFlag(MLQuestInstanceFlags.Removed); }
            set { SetFlag(MLQuestInstanceFlags.Removed, value); }
        }

        public bool Failed
        {
            get { return GetFlag(MLQuestInstanceFlags.Failed); }
            set { SetFlag(MLQuestInstanceFlags.Failed, value); }
        }

        public BaseObjectiveInstance[] Objectives
        {
            get { return m_ObjectiveInstances; }
            set { m_ObjectiveInstances = value; }
        }

        public bool AllowsQuestItem(Item item, Type type)
        {
            foreach (BaseObjectiveInstance objective in m_ObjectiveInstances)
            {
                if (!objective.Expired && objective.AllowsQuestItem(item, type))
                    return true;
            }

            return false;
        }

        public bool IsCompleted()
        {
            bool requiresAll = (m_Quest.ObjectiveType == ObjectiveType.All);

            foreach (BaseObjectiveInstance obj in m_ObjectiveInstances)
            {
                bool complete = obj.IsCompleted();

                if (complete && !requiresAll)
                    return true;
                else if (!complete && requiresAll)
                    return false;
            }

            return requiresAll;
        }

        public void CheckComplete()
        {
            if (IsCompleted())
            {
                m_Player.PlaySound(0x5B5); // public sound

                foreach (BaseObjectiveInstance obj in m_ObjectiveInstances)
                    obj.OnQuestCompleted();

                TextDefinition.SendMessageTo(m_Player, m_Quest.CompletionNotice, 0x23);

                /*
                 * Advance to the ClaimReward=true stage if this quest has no
                 * completion message to show anyway. This suppresses further
                 * triggers of CheckComplete.
                 *
                 * For quests that require collections, this is done later when
                 * the player double clicks the quester.
                 */
                if (!Removed && SkipReportBack && !m_Quest.RequiresCollection) // An OnQuestCompleted can potentially have removed this instance already
                    ContinueReportBack(false);
            }
        }

        public void Fail()
        {
            Failed = true;
        }

        private void Slice()
        {
            if (ClaimReward || Removed)
            {
                StopTimer();
                return;
            }

            bool hasAnyFails = false;
            bool hasAnyLeft = false;

            foreach (BaseObjectiveInstance obj in m_ObjectiveInstances)
            {
                if (!obj.Expired)
                {
                    if (obj.IsTimed && obj.EndTime <= DateTime.UtcNow)
                    {
                        m_Player.SendLocalizedMessage(1072258); // You failed to complete an objective in time!

                        obj.Expired = true;
                        obj.OnExpire();

                        hasAnyFails = true;
                    }
                    else
                    {
                        hasAnyLeft = true;
                    }
                }
            }

            if ((m_Quest.ObjectiveType == ObjectiveType.All && hasAnyFails) || !hasAnyLeft)
                Fail();

            if (!hasAnyLeft)
                StopTimer();
        }

        public void SendProgressGump()
        {
            m_Player.SendGump(new QuestConversationGump(m_Quest, m_Player, m_Quest.InProgressMessage));
        }

        public void SendRewardOffer()
        {
            m_Quest.GetRewards(this);
        }

        // TODO: Split next quest stuff from SendRewardGump stuff?
        public void SendRewardGump()
        {
            Type nextQuestType = m_Quest.NextQuest;

            if (nextQuestType != null)
            {
                ClaimRewards(); // skip reward gump

                if (Removed) // rewards were claimed successfully
                {
                    MLQuest nextQuest = MLQuestSystem.FindQuest(nextQuestType);

                    if (nextQuest != null)
                        nextQuest.SendOffer(m_Quester, m_Player);
                }
            }
            else
            {
                m_Player.SendGump(new QuestRewardGump(this));
            }
        }

        public bool SkipReportBack
        {
            get { return TextDefinition.IsNullOrEmpty(m_Quest.CompletionMessage); }
        }

        public void SendReportBackGump()
        {
            if (SkipReportBack)
                ContinueReportBack(true); // skip ahead
            else
                m_Player.SendGump(new QuestReportBackGump(this));
        }

        public void ContinueReportBack(bool sendRewardGump)
        {
            // There is a backpack check here on OSI for the rewards as well (even though it's not needed...)

            if (m_Quest.ObjectiveType == ObjectiveType.All)
            {
                // TODO: 1115877 - You no longer have the required items to complete this quest.
                foreach (BaseObjectiveInstance objective in m_ObjectiveInstances)
                {
                    if (!objective.IsCompleted())
                        return;
                }

                foreach (BaseObjectiveInstance objective in m_ObjectiveInstances)
                {
                    if (!objective.OnBeforeClaimReward())
                        return;
                }

                foreach (BaseObjectiveInstance objective in m_ObjectiveInstances)
                    objective.OnClaimReward();
            }
            else
            {
                /* The following behavior is unverified, as OSI (currently) has no collect quest requiring
                 * only one objective to be completed. It is assumed that only one objective is claimed
                 * (the first completed one), even when multiple are complete.
                 */
                bool complete = false;

                foreach (BaseObjectiveInstance objective in m_ObjectiveInstances)
                {
                    if (objective.IsCompleted())
                    {
                        if (objective.OnBeforeClaimReward())
                        {
                            complete = true;
                            objective.OnClaimReward();
                        }

                        break;
                    }
                }

                if (!complete)
                    return;
            }

            ClaimReward = true;

            if (m_Quest.HasRestartDelay)
                PlayerContext.SetDoneQuest(m_Quest, DateTime.UtcNow + m_Quest.GetRestartDelay());

            // This is correct for ObjectiveType.Any as well
            foreach (BaseObjectiveInstance objective in m_ObjectiveInstances)
                objective.OnAfterClaimReward();

            if (sendRewardGump)
                SendRewardOffer();
        }

        public void ClaimRewards()
        {
            if (m_Quest == null || m_Player == null || m_Player.Deleted || !ClaimReward || Removed)
                return;

            List<Item> rewards = new List<Item>();

            foreach (BaseReward reward in m_Quest.Rewards)
                reward.AddRewardItems(m_Player, rewards);

            if (rewards.Count != 0)
            {
                // On OSI a more naive method of checking is used.
                // For containers, only the actual container item counts.
                bool canFit = true;

                foreach (Item rewardItem in rewards)
                {
                    if (!m_Player.AddToBackpack(rewardItem))
                    {
                        canFit = false;
                        break;
                    }
                }

                if (!canFit)
                {
                    foreach (Item rewardItem in rewards)
                        rewardItem.Delete();

                    m_Player.SendLocalizedMessage(1078524); // Your backpack is full. You cannot complete the quest and receive your reward.
                    return;
                }

                foreach (Item rewardItem in rewards)
                {
                    string rewardName = (rewardItem.Name != null) ? rewardItem.Name : String.Concat("#", rewardItem.LabelNumber);

                    if (rewardItem.Stackable)
                        m_Player.SendLocalizedMessage(1115917, String.Concat(rewardItem.Amount, "\t", rewardName)); // You receive a reward: ~1_QUANTITY~ ~2_ITEM~
                    else
                        m_Player.SendLocalizedMessage(1074360, rewardName); // You receive a reward: ~1_REWARD~
                }
            }

            foreach (BaseObjectiveInstance objective in m_ObjectiveInstances)
                objective.OnRewardClaimed();

            m_Quest.OnRewardClaimed(this);

            MLQuestContext context = PlayerContext;

            if (m_Quest.RecordCompletion && !m_Quest.HasRestartDelay) // Quests with restart delays are logged earlier as per OSI
                context.SetDoneQuest(m_Quest);

            if (m_Quest.IsChainTriggered)
                context.ChainOffers.Remove(m_Quest);

            Type nextQuestType = m_Quest.NextQuest;

            if (nextQuestType != null)
            {
                MLQuest nextQuest = MLQuestSystem.FindQuest(nextQuestType);

                if (nextQuest != null && !context.ChainOffers.Contains(nextQuest))
                    context.ChainOffers.Add(nextQuest);
            }

            Remove();
        }

        public void Cancel()
        {
            Cancel(false);
        }

        public void Cancel(bool removeChain)
        {
            Remove();

            m_Player.SendSound(0x5B3); // private sound

            foreach (BaseObjectiveInstance obj in m_ObjectiveInstances)
                obj.OnQuestCancelled();

            m_Quest.OnCancel(this);

            if (removeChain)
                PlayerContext.ChainOffers.Remove(m_Quest);
        }

        public void Remove()
        {
            Unregister();
            StopTimer();
        }

        private void StopTimer()
        {
            if (m_Timer != null)
            {
                m_Timer.Stop();
                m_Timer = null;
            }
        }

        public void OnQuesterDeleted()
        {
            foreach (BaseObjectiveInstance obj in m_ObjectiveInstances)
                obj.OnQuesterDeleted();

            m_Quest.OnQuesterDeleted(this);
        }

        public void OnPlayerDeath()
        {
            foreach (BaseObjectiveInstance obj in m_ObjectiveInstances)
                obj.OnPlayerDeath();

            m_Quest.OnPlayerDeath(this);
        }

        private bool GetFlag(MLQuestInstanceFlags flag)
        {
            return ((m_Flags & flag) != 0);
        }

        private void SetFlag(MLQuestInstanceFlags flag, bool value)
        {
            if (value)
                m_Flags |= flag;
            else
                m_Flags &= ~flag;
        }

        public void Serialize(GenericWriter writer)
        {
            // Version info is written in MLQuestPersistence.Serialize

            MLQuestSystem.WriteQuestRef(writer, m_Quest);

            if (m_Quester == null || m_Quester.Deleted)
                writer.Write(Serial.MinusOne);
            else
                writer.Write(m_Quester.Serial);

            writer.Write(ClaimReward);
            writer.Write(m_ObjectiveInstances.Length);

            foreach (BaseObjectiveInstance objInstance in m_ObjectiveInstances)
                objInstance.Serialize(writer);
        }

        public static MLQuestInstance Deserialize(GenericReader reader, int version, PlayerMobile pm)
        {
            MLQuest quest = MLQuestSystem.ReadQuestRef(reader);

            // TODO: Serialize quester TYPE too, the quest giver reference then becomes optional (only for escorts)
            IQuestGiver quester = World.FindEntity(reader.ReadInt()) as IQuestGiver;

            bool claimReward = reader.ReadBool();
            int objectives = reader.ReadInt();

            MLQuestInstance instance;

            if (quest != null && quester != null && pm != null)
            {
                instance = quest.CreateInstance(quester, pm);
                instance.ClaimReward = claimReward;
            }
            else
            {
                instance = null;
            }

            for (int i = 0; i < objectives; ++i)
                BaseObjectiveInstance.Deserialize(reader, version, (instance != null && i < instance.Objectives.Length) ? instance.Objectives[i] : null);

            if (instance != null)
                instance.Slice();

            return instance;
        }
    }

    #region Quest Offer Regions

    public class QuestArea
    {
        private TextDefinition m_Name; // So we can add custom names, different from the Region name
        private string m_RegionName;
        private Map m_ForceMap;

        public TextDefinition Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public string RegionName
        {
            get { return m_RegionName; }
            set { m_RegionName = value; }
        }

        public Map ForceMap
        {
            get { return m_ForceMap; }
            set { m_ForceMap = value; }
        }

        public QuestArea(TextDefinition name, string region)
            : this(name, region, null)
        {
        }

        public QuestArea(TextDefinition name, string region, Map forceMap)
        {
            m_Name = name;
            m_RegionName = region;
            m_ForceMap = forceMap;

            if (MLQuestSystem.Debug)
                ValidationQueue<QuestArea>.Add(this);
        }

        public bool Contains(Mobile mob)
        {
            return Contains(mob.Region);
        }

        public bool Contains(Region reg)
        {
            if (reg == null || (m_ForceMap != null && reg.Map != m_ForceMap))
                return false;

            return reg.IsPartOf(m_RegionName);
        }

        // Debug method
        public void Validate()
        {
            bool found = false;

            foreach (Region r in Region.Regions)
            {
                if (r.Name == m_RegionName && (m_ForceMap == null || r.Map == m_ForceMap))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
                Console.WriteLine("Warning: QuestArea region '{0}' does not exist (ForceMap = {1})", m_RegionName, (m_ForceMap == null) ? "-null-" : m_ForceMap.ToString());
        }
    }

    #endregion

    #region Quest Mobile Engine

    public interface IQuestGiver
    {
        List<MLQuest> MLQuests { get; }

        Serial Serial { get; }
        bool Deleted { get; }

        Type GetType();
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class QuesterNameAttribute : Attribute
    {
        private string m_QuesterName;

        public string QuesterName { get { return m_QuesterName; } }

        public QuesterNameAttribute(string questerName)
        {
            m_QuesterName = questerName;
        }

        private static readonly Type m_Type = typeof(QuesterNameAttribute);
        private static readonly Dictionary<Type, string> m_Cache = new Dictionary<Type, string>();

        public static string GetQuesterNameFor(Type t)
        {
            if (t == null)
                return "";

            string result;

            if (m_Cache.TryGetValue(t, out result))
                return result;

            object[] attributes = t.GetCustomAttributes(m_Type, false);

            if (attributes.Length != 0)
                result = ((QuesterNameAttribute)attributes[0]).QuesterName;
            else
                result = t.Name;

            return (m_Cache[t] = result);
        }
    }

    #endregion
}

namespace Server.Engines.MLQuests.Definitions
{
    /// <summary>
    /// Base Class For Escorts Providing The AwardHumanInNeed Option.
    /// </summary>
    public class BaseEscort : MLQuest
    {
        public virtual bool AwardHumanInNeed { get { return true; } }

        public BaseEscort()
        {
            CompletionNotice = CompletionNoticeShort;
        }

        public override void GetRewards(MLQuestInstance instance)
        {
            if (AwardHumanInNeed)
                HumanInNeed.AwardTo(instance.Player);

            base.GetRewards(instance);
        }
    }

    public static class Spellweaving
    {
        public static void AwardTo(PlayerMobile pm)
        {
            if (pm == null)
                return;

            MLQuestContext context = MLQuestSystem.GetOrCreateContext(pm);

            if (!context.Spellweaving)
            {
                context.Spellweaving = true;

                Effects.SendLocationParticles(EffectItem.Create(pm.Location, pm.Map, EffectItem.DefaultDuration), 0, 0, 0, 0, 0, 5060, 0);
                Effects.PlaySound(pm.Location, pm.Map, 0x243);

                Effects.SendMovingParticles(new Entity(Server.Serial.Zero, new Point3D(pm.X - 6, pm.Y - 6, pm.Z + 15), pm.Map), pm, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
                Effects.SendMovingParticles(new Entity(Server.Serial.Zero, new Point3D(pm.X - 4, pm.Y - 6, pm.Z + 15), pm.Map), pm, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
                Effects.SendMovingParticles(new Entity(Server.Serial.Zero, new Point3D(pm.X - 6, pm.Y - 4, pm.Z + 15), pm.Map), pm, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);

                Effects.SendTargetParticles(pm, 0x375A, 35, 90, 0x00, 0x00, 9502, (EffectLayer)255, 0x100);
            }
        }
    }
}

namespace Server.Engines.MLQuests.Gumps
{
    public enum ButtonPosition : byte
    {
        Left,
        Right
    }

    public enum ButtonGraphic : ushort
    {
        Invalid,
        Accept = 0x2EE0,
        Clear = 0x2EE3,
        Close = 0x2EE6,
        Continue = 0x2EE9,
        Okay = 0x2EEC,
        Previous = 0x2EEF,
        Refuse = 0x2EF2,
        Resign = 0x2EF5
    }

    public abstract class BaseQuestGump : Gump
    {
        private struct ButtonInfo
        {
            private ButtonPosition m_Position;
            private ButtonGraphic m_Graphic;
            private int m_ButtonID;

            public ButtonPosition Position { get { return m_Position; } }
            public ButtonGraphic Graphic { get { return m_Graphic; } }
            public int ButtonID { get { return m_ButtonID; } }

            public ButtonInfo(ButtonPosition position, ButtonGraphic graphic, int buttonID)
            {
                m_Position = position;
                m_Graphic = graphic;
                m_ButtonID = buttonID;
            }
        }

        private int m_Page;
        private int m_MaxPages;
        private int m_Label;
        private string m_Title;
        private List<ButtonInfo> m_Buttons;

#if false
		// OSI clone, inefficient layout
		public BaseQuestGump( int label ) : base( 75, 25 )
		{
			m_Page = 0;
			m_MaxPages = 0;
			m_Label = label;
			m_Title = null;
			m_Buttons = new List<ButtonInfo>( 2 );
		}

		public void BuildPage()
		{
			AddPage( ++m_Page );

			Closable = false;
			AddImageTiled( 50, 20, 400, 460, 0x1404 );
			AddImageTiled( 50, 29, 30, 450, 0x28DC );
			AddImageTiled( 34, 140, 17, 339, 0x242F );
			AddImage( 48, 135, 0x28AB );
			AddImage( -16, 285, 0x28A2 );
			AddImage( 0, 10, 0x28B5 );
			AddImage( 25, 0, 0x28B4 );
			AddImageTiled( 83, 15, 350, 15, 0x280A );
			AddImage( 34, 479, 0x2842 );
			AddImage( 442, 479, 0x2840 );
			AddImageTiled( 51, 479, 392, 17, 0x2775 );
			AddImageTiled( 415, 29, 44, 450, 0xA2D );
			AddImageTiled( 415, 29, 30, 450, 0x28DC );
			AddLabel( 100, 50, 0x481, "" );
			AddImage( 370, 50, 0x589 );
			AddImage( 379, 60, 0x15A9 );
			AddImage( 425, 0, 0x28C9 );
			AddImage( 90, 33, 0x232D );
			AddHtmlLocalized( 130, 45, 270, 16, m_Label, 0xFFFFFF, false, false );
			AddImageTiled( 130, 65, 175, 1, 0x238D );

			if ( m_Page > 1 )
				AddButton( 130, 430, (int)ButtonGraphic.Previous, (int)ButtonGraphic.Previous + 2, 0, GumpButtonType.Page, m_Page - 1 );

			if ( m_Page < m_MaxPages )
				AddButton( 275, 430, (int)ButtonGraphic.Continue, (int)ButtonGraphic.Continue + 2, 0, GumpButtonType.Page, m_Page + 1 );

			foreach ( ButtonInfo button in m_Buttons )
				AddButton( button.Position == ButtonPosition.Left ? 95 : 313, 455, (int)button.Graphic, (int)button.Graphic + 2, button.ButtonID, GumpButtonType.Reply, 0 );

			if ( m_Title != null )
				AddHtmlLocalized( 130, 68, 220, 48, 1114513, m_Title, 0x2710, false, false ); // <DIV ALIGN=CENTER>~1_TOKEN~</DIV>
		}
#else
        // RunUO optimized version
        public BaseQuestGump(int label)
            : base(75, 25)
        {
            m_Page = 0;
            m_MaxPages = 0;
            m_Label = label;
            m_Title = null;
            m_Buttons = new List<ButtonInfo>(2);

            Closable = false;

            AddPage(0);

            AddImageTiled(50, 20, 400, 460, 0x1404);
            AddImageTiled(50, 29, 30, 450, 0x28DC);
            AddImageTiled(34, 140, 17, 339, 0x242F);
            AddImage(48, 135, 0x28AB);
            AddImage(-16, 285, 0x28A2);
            AddImage(0, 10, 0x28B5);
            AddImage(25, 0, 0x28B4);
            AddImageTiled(83, 15, 350, 15, 0x280A);
            AddImage(34, 479, 0x2842);
            AddImage(442, 479, 0x2840);
            AddImageTiled(51, 479, 392, 17, 0x2775);
            AddImageTiled(415, 29, 44, 450, 0xA2D);
            AddImageTiled(415, 29, 30, 450, 0x28DC);
            //AddLabel( 100, 50, 0x481, "" );
            AddImage(370, 50, 0x589);
            AddImage(379, 60, 0x15A9);
            AddImage(425, 0, 0x28C9);
            AddImage(90, 33, 0x232D);
            AddHtmlLocalized(130, 45, 270, 16, label, 0xFFFFFF, false, false);
            AddImageTiled(130, 65, 175, 1, 0x238D);
        }

        public void BuildPage()
        {
            AddPage(++m_Page);

            if (m_Page > 1)
                AddButton(130, 430, (int)ButtonGraphic.Previous, (int)ButtonGraphic.Previous + 2, 0, GumpButtonType.Page, m_Page - 1);

            if (m_Page < m_MaxPages)
                AddButton(275, 430, (int)ButtonGraphic.Continue, (int)ButtonGraphic.Continue + 2, 0, GumpButtonType.Page, m_Page + 1);

            foreach (ButtonInfo button in m_Buttons)
                AddButton(button.Position == ButtonPosition.Left ? 95 : 313, 455, (int)button.Graphic, (int)button.Graphic + 2, button.ButtonID, GumpButtonType.Reply, 0);

            if (m_Title != null)
                AddHtmlLocalized(130, 68, 220, 48, 1114513, m_Title, 0x2710, false, false); // <DIV ALIGN=CENTER>~1_TOKEN~</DIV>
        }
#endif

        public void SetPageCount(int maxPages)
        {
            m_MaxPages = maxPages;
        }

        public void SetTitle(TextDefinition def)
        {
            if (def.Number > 0)
                m_Title = String.Format("#{0}", def.Number); // OSI does "@@#{0}" instead, why? KR client related?
            else
                m_Title = def.String;
        }

        public void RegisterButton(ButtonPosition position, ButtonGraphic graphic, int buttonID)
        {
            m_Buttons.Add(new ButtonInfo(position, graphic, buttonID));
        }

        #region Elaborate Formatting Shortcuts

        public void AddDescription(MLQuest quest)
        {
            AddHtmlLocalized(98, 140, 312, 16, (quest.IsChainTriggered || quest.NextQuest != null) ? 1075024 : 1072202, 0x2710, false, false); // Description [(quest chain)]
            TextDefinition.AddHtmlText(this, 98, 156, 312, 240, quest.Description, false, true, 0x15F90, 0xBDE784);
        }

        public void AddObjectives(MLQuest quest)
        {
            AddHtmlLocalized(98, 140, 312, 16, 1049073, 0x2710, false, false); // Objective:
            AddHtmlLocalized(98, 156, 312, 16, (quest.ObjectiveType == ObjectiveType.All) ? 1072208 : 1072209, 0x2710, false, false); // All of the following / Only one of the following

            int y = 172;

            foreach (BaseObjective objective in quest.Objectives)
            {
                objective.WriteToGump(this, ref y);

                if (objective.IsTimed)
                {
                    if (objective is CollectObjective)
                        y -= 16;

                    BaseObjectiveInstance.WriteTimeRemaining(this, ref y, objective.Duration);
                }
            }
        }

        public void AddObjectivesProgress(MLQuestInstance instance)
        {
            MLQuest quest = instance.Quest;

            AddHtmlLocalized(98, 140, 312, 16, 1049073, 0x2710, false, false); // Objective:
            AddHtmlLocalized(98, 156, 312, 16, (quest.ObjectiveType == ObjectiveType.All) ? 1072208 : 1072209, 0x2710, false, false); // All of the following / Only one of the following

            int y = 172;

            foreach (BaseObjectiveInstance objInstance in instance.Objectives)
                objInstance.WriteToGump(this, ref y);
        }

        public void AddRewardsPage(MLQuest quest) // For the quest log/offer gumps
        {
            AddHtmlLocalized(98, 140, 312, 16, 1072201, 0x2710, false, false); // Reward

            int y = 162;

            if (quest.Rewards.Count > 1)
            {
                // TODO: Is this what this is for? Does "Only one of the following" occur?
                AddHtmlLocalized(98, 156, 312, 16, 1072208, 0x2710, false, false); // All of the following
                y += 16;
            }

            AddRewards(quest, 105, y, 16);
        }

        public void AddRewards(MLQuest quest) // For the claim rewards gump
        {
            int y = 146;

            if (quest.Rewards.Count > 1)
            {
                // TODO: Is this what this is for? Does "Only one of the following" occur?
                AddHtmlLocalized(100, 140, 312, 16, 1072208, 0x2710, false, false); // All of the following
                y += 16;
            }

            AddRewards(quest, 107, y, 26);
        }

        public void AddRewards(MLQuest quest, int x, int y, int spacing)
        {
            int xReward = x + 28;

            foreach (BaseReward reward in quest.Rewards)
            {
                AddImage(x, y + 1, 0x4B9);
                reward.WriteToGump(this, xReward, ref y);
                y += spacing;
            }
        }

        public void AddConversation(TextDefinition text)
        {
            TextDefinition.AddHtmlText(this, 98, 140, 312, 180, text, false, true, 0x15F90, 0xBDE784);
        }

        #endregion

        /* OSI gump IDs:
		 * 800 - QuestOfferGump
		 * 801 - QuestCancelConfirmGump
		 * 802 - ?? (gets closed by Toggle Quest Item)
		 * 803 - QuestRewardGump
		 * 804 - ?? (gets closed by Toggle Quest Item)
		 * 805 - QuestLogGump
		 * 806 - QuestConversationGump (refuse / in progress)
		 * 807 - ?? (gets closed by Toggle Quest Item and most quest gumps)
		 * 808 - InfoNPCGump
		 * 809 - QuestLogDetailedGump
		 * 810 - QuestReportBackGump
		 */
        public static void CloseOtherGumps(PlayerMobile pm)
        {
            pm.CloseGump(typeof(InfoNPCGump));
            pm.CloseGump(typeof(QuestRewardGump));
            pm.CloseGump(typeof(QuestConversationGump));
            pm.CloseGump(typeof(QuestReportBackGump));
            //pm.CloseGump( typeof( UnknownGump807 ) );
            pm.CloseGump(typeof(QuestCancelConfirmGump));
        }
    }

    public class QuestOfferGump : BaseQuestGump
    {
        private MLQuest m_Quest;
        private IQuestGiver m_Quester;

        public QuestOfferGump(MLQuest quest, IQuestGiver quester, PlayerMobile pm)
            : base(1049010) // Quest Offer
        {
            m_Quest = quest;
            m_Quester = quester;

            CloseOtherGumps(pm);
            pm.CloseGump(typeof(QuestOfferGump));

            SetTitle(quest.Title);
            RegisterButton(ButtonPosition.Left, ButtonGraphic.Accept, 1);
            RegisterButton(ButtonPosition.Right, ButtonGraphic.Refuse, 2);

            SetPageCount(3);

            BuildPage();
            AddDescription(quest);

            BuildPage();
            AddObjectives(quest);

            BuildPage();
            AddRewardsPage(quest);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile pm = sender.Mobile as PlayerMobile;

            if (pm == null)
                return;

            switch (info.ButtonID)
            {
                case 1: // Accept
                    {
                        m_Quest.OnAccept(m_Quester, pm);
                        break;
                    }
                case 2: // Refuse
                    {
                        m_Quest.OnRefuse(m_Quester, pm);
                        break;
                    }
            }
        }
    }

    public class QuestCancelConfirmGump : Gump
    {
        private MLQuestInstance m_Instance;
        private bool m_CloseGumps;

        public QuestCancelConfirmGump(MLQuestInstance instance)
            : this(instance, true)
        {
        }

        public QuestCancelConfirmGump(MLQuestInstance instance, bool closeGumps)
            : base(120, 50)
        {
            m_Instance = instance;
            m_CloseGumps = closeGumps;

            if (closeGumps)
                BaseQuestGump.CloseOtherGumps(instance.Player);

            AddPage(0);

            Closable = false;

            AddImageTiled(0, 0, 348, 262, 0xA8E);
            AddAlphaRegion(0, 0, 348, 262);

            AddImage(0, 15, 0x27A8);
            AddImageTiled(0, 30, 17, 200, 0x27A7);
            AddImage(0, 230, 0x27AA);

            AddImage(15, 0, 0x280C);
            AddImageTiled(30, 0, 300, 17, 0x280A);
            AddImage(315, 0, 0x280E);

            AddImage(15, 244, 0x280C);
            AddImageTiled(30, 244, 300, 17, 0x280A);
            AddImage(315, 244, 0x280E);

            AddImage(330, 15, 0x27A8);
            AddImageTiled(330, 30, 17, 200, 0x27A7);
            AddImage(330, 230, 0x27AA);

            AddImage(333, 2, 0x2716);
            AddImage(333, 248, 0x2716);
            AddImage(2, 248, 0x2716);
            AddImage(2, 2, 0x2716);

            AddHtmlLocalized(25, 22, 200, 20, 1049000, 0x7D00, false, false); // Confirm Quest Cancellation
            AddImage(25, 40, 0xBBF);

            /*
             * This quest will give you valuable information, skills
             * and equipment that will help you advance in the
             * game at a quicker pace.<BR>
             * <BR>
             * Are you certain you wish to cancel at this time?
             */
            AddHtmlLocalized(25, 55, 300, 120, 1060836, 0xFFFFFF, false, false);

            MLQuest quest = instance.Quest;

            if (quest.IsChainTriggered || quest.NextQuest != null)
            {
                AddRadio(25, 145, 0x25F8, 0x25FB, false, 2);
                AddHtmlLocalized(60, 150, 280, 20, 1075023, 0xFFFFFF, false, false); // Yes, I want to quit this entire chain!
            }

            AddRadio(25, 180, 0x25F8, 0x25FB, true, 1);
            AddHtmlLocalized(60, 185, 280, 20, 1049005, 0xFFFFFF, false, false); // Yes, I really want to quit this quest!

            AddRadio(25, 215, 0x25F8, 0x25FB, false, 0);
            AddHtmlLocalized(60, 220, 280, 20, 1049006, 0xFFFFFF, false, false); // No, I don't want to quit.

            AddButton(265, 220, 0xF7, 0xF8, 7, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (m_Instance.Removed)
                return;

            switch (info.ButtonID)
            {
                case 7: // Okay
                    {
                        if (info.IsSwitched(2))
                            m_Instance.Cancel(true);
                        else if (info.IsSwitched(1))
                            m_Instance.Cancel(false);

                        sender.Mobile.SendGump(new QuestLogGump(m_Instance.Player, m_CloseGumps));
                        break;
                    }
            }
        }
    }

    #region Mobile Conversation

    public class QuestConversationGump : BaseQuestGump
    {
        public QuestConversationGump(MLQuest quest, PlayerMobile pm, TextDefinition text)
            : base(3006156) // Quest Conversation
        {
            CloseOtherGumps(pm);

            SetTitle(quest.Title);
            RegisterButton(ButtonPosition.Right, ButtonGraphic.Close, 3);

            SetPageCount(1);

            BuildPage();
            AddConversation(text);
        }
    }

    #endregion

    #region Confirm Race Change

    public interface IRaceChanger
    {
        bool CheckComplete(PlayerMobile from);
        void ConsumeNeeded(PlayerMobile from);
        void OnCancel(PlayerMobile from);
    }

    public class RaceChangeConfirmGump : Gump
    {
        public static readonly Type Type = typeof(RaceChangeConfirmGump);

        private IRaceChanger m_Owner;
        private PlayerMobile m_From;
        private Race m_Race;

        public RaceChangeConfirmGump(IRaceChanger owner, PlayerMobile from, Race targetRace)
            : base(50, 50)
        {
            from.CloseGump(Type);

            m_Owner = owner;
            m_From = from;
            m_Race = targetRace;

            AddPage(0);
            AddBackground(0, 0, 240, 135, 0x2422);

            if (targetRace == Race.Human)
                AddHtmlLocalized(15, 15, 210, 75, 1073643, 0, false, false); // Are you sure you wish to embrace your humanity?
            else if (targetRace == Race.Elf)
                AddHtmlLocalized(15, 15, 210, 75, 1073642, 0, false, false); // Are you sure you want to follow the elven ways?
            else
                AddHtml(15, 15, 210, 75, String.Format("Are you sure you want to change your race to {0}?", targetRace.Name), false, false);

            AddButton(160, 95, 0xF7, 0xF8, 1, GumpButtonType.Reply, 0);
            AddButton(90, 95, 0xF2, 0xF1, 0, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            switch (info.ButtonID)
            {
                case 0: // Cancel
                    {
                        if (m_Owner != null)
                            m_Owner.OnCancel(m_From);

                        break;
                    }
                case 1: // Okay
                    {
                        if (m_Owner == null || m_Owner.CheckComplete(m_From))
                            Offer(m_Owner, m_From, m_Race);

                        break;
                    }
            }
        }

        private class RaceChangeState
        {
            private static readonly TimeSpan m_TimeoutDelay = TimeSpan.FromMinutes(1);
            private static readonly TimerStateCallback<NetState> m_TimeoutCallback = new TimerStateCallback<NetState>(Timeout);

            public IRaceChanger m_Owner;
            public Race m_TargetRace;
            public Timer m_Timeout;

            public RaceChangeState(IRaceChanger owner, NetState ns, Race targetRace)
            {
                m_Owner = owner;
                m_TargetRace = targetRace;
                m_Timeout = Timer.DelayCall<NetState>(m_TimeoutDelay, m_TimeoutCallback, ns);
            }
        }

        private static Dictionary<NetState, RaceChangeState> m_Pending;

        public static void Initialize()
        {
            m_Pending = new Dictionary<NetState, RaceChangeState>();

            PacketHandlers.RegisterExtended(0x2A, true, new OnPacketReceive(RaceChangeReply));
        }

        public static bool IsPending(NetState state)
        {
            return (state != null && m_Pending.ContainsKey(state));
        }

        private static void Offer(IRaceChanger owner, PlayerMobile from, Race targetRace)
        {
            NetState ns = from.NetState;

            if (ns == null || !CanChange(from, targetRace))
                return;

            CloseCurrent(ns);

            m_Pending[ns] = new RaceChangeState(owner, ns, targetRace);
            ns.Send(new RaceChanger(from.Female, targetRace));
        }

        private static void CloseCurrent(NetState ns)
        {
            RaceChangeState state;

            if (m_Pending.TryGetValue(ns, out state))
            {
                state.m_Timeout.Stop();
                m_Pending.Remove(ns);
            }

            ns.Send(CloseRaceChanger.Instance);
        }

        private static void Timeout(NetState ns)
        {
            if (m_Pending.ContainsKey(ns))
            {
                m_Pending.Remove(ns);
                ns.Send(CloseRaceChanger.Instance);
            }
        }

        public static bool IsWearingEquipment(Mobile from)
        {
            foreach (Item item in from.Items)
            {
                switch (item.Layer)
                {
                    case Layer.Hair:
                    case Layer.FacialHair:
                    case Layer.Backpack:
                    case Layer.Mount:
                    case Layer.Bank:
                        {
                            continue; // ignore
                        }
                    default:
                        {
                            return true;
                        }
                }
            }

            return false;
        }

        private static bool CanChange(PlayerMobile from, Race targetRace)
        {
            if (from.Deleted)
                return false;

            if (from.Race == targetRace)
                from.SendLocalizedMessage(1111918); // You are already that race.
            else if (!MondainsLegacy.CheckML(from, false))
                from.SendLocalizedMessage(1073651); // You must have Mondain's Legacy before proceeding...
            else if (!from.Alive)
                from.SendLocalizedMessage(1073646); // Only the living may proceed...
            else if (from.Mounted)
                from.SendLocalizedMessage(1073647); // You may not continue while mounted...
            else if (!from.CanBeginAction(typeof(PolymorphSpell)) || DisguiseTimers.IsDisguised(from) || AnimalForm.UnderTransformation(from) || !from.CanBeginAction(typeof(IncognitoSpell)) || from.IsBodyMod) // TODO: Does this cover everything?
                from.SendLocalizedMessage(1073648); // You may only proceed while in your original state...
            else if (from.Spell != null && from.Spell.IsCasting)
                from.SendLocalizedMessage(1073649); // One may not proceed while embracing magic...
            else if (from.Poisoned)
                from.SendLocalizedMessage(1073652); // You must be healthy to proceed...
            else if (IsWearingEquipment(from))
                from.SendLocalizedMessage(1073650); // To proceed you must be unburdened by equipment...
            else
                return true;

            return false;
        }

        private static void RaceChangeReply(NetState state, PacketReader pvSrc)
        {
            RaceChangeState raceChangeState;

            if (!m_Pending.TryGetValue(state, out raceChangeState))
                return;

            CloseCurrent(state);

            PlayerMobile pm = state.Mobile as PlayerMobile;

            if (pm == null)
                return;

            IRaceChanger owner = raceChangeState.m_Owner;
            Race targetRace = raceChangeState.m_TargetRace;

            if (pvSrc.Size == 5)
            {
                if (owner != null)
                    owner.OnCancel(pm);

                return;
            }

            if (!CanChange(pm, targetRace) || (owner != null && !owner.CheckComplete(pm)))
                return;

            int hue = pvSrc.ReadUInt16();
            int hairItemId = pvSrc.ReadUInt16();
            int hairHue = pvSrc.ReadUInt16();
            int facialHairItemId = pvSrc.ReadUInt16();
            int facialHairHue = pvSrc.ReadUInt16();

            pm.Race = targetRace;
            pm.Hue = targetRace.ClipSkinHue(hue) | 0x8000;

            if (targetRace.ValidateHair(pm, hairItemId))
            {
                pm.HairItemID = hairItemId;
                pm.HairHue = targetRace.ClipHairHue(hairHue);
            }
            else
            {
                pm.HairItemID = 0;
            }

            if (targetRace.ValidateFacialHair(pm, facialHairItemId))
            {
                pm.FacialHairItemID = facialHairItemId;
                pm.FacialHairHue = targetRace.ClipHairHue(facialHairHue);
            }
            else
            {
                pm.FacialHairItemID = 0;
            }

            if (targetRace == Race.Human)
                pm.SendLocalizedMessage(1073654); // You are now fully human.
            else if (targetRace == Race.Elf)
                pm.SendLocalizedMessage(1073653); // You are now fully initiated into the Elven culture.
            else
                pm.SendMessage("You have fully changed your race to {0}.", targetRace.Name);

            if (owner != null)
                owner.ConsumeNeeded(pm);
        }
    }

    public sealed class RaceChanger : Packet
    {
        public RaceChanger(bool female, Race targetRace)
            : base(0xBF)
        {
            EnsureCapacity(7);

            m_Stream.Write((short)0x2A);
            m_Stream.Write((byte)(female ? 1 : 0));
            m_Stream.Write((byte)(targetRace.RaceID + 1));
        }
    }

    public sealed class CloseRaceChanger : Packet
    {
        public static readonly Packet Instance = Packet.SetStatic(new CloseRaceChanger());

        private CloseRaceChanger()
            : base(0xBF)
        {
            EnsureCapacity(7);

            m_Stream.Write((short)0x2A);
            m_Stream.Write((byte)0);
            m_Stream.Write((byte)0xFF);
        }
    }

    public class RaceChangeDeed : Item, IRaceChanger
    {
        public override string DefaultName { get { return "a race change deed"; } }

        [Constructable]
        public RaceChangeDeed()
            : base(0x14F0)
        {
            LootType = LootType.Blessed;
        }

        public bool CheckComplete(PlayerMobile pm)
        {
            if (Deleted)
                return false;

            if (!IsChildOf(pm.Backpack))
            {
                pm.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
                return false;
            }

            return true;
        }

        public void ConsumeNeeded(PlayerMobile pm)
        {
            Consume();
        }

        public void OnCancel(PlayerMobile pm)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;

            if (pm == null)
                return;

            if (CheckComplete(pm))
                pm.SendGump(new RaceChangeConfirmGump(this, pm, (pm.Race == Race.Human) ? Race.Elf : Race.Human));
        }

        public RaceChangeDeed(Serial serial)
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

    #endregion

    #region Mobile Questing Log

    public class QuestLogGump : BaseQuestGump
    {
        private PlayerMobile m_Owner;
        private bool m_CloseGumps;

        public QuestLogGump(PlayerMobile pm)
            : this(pm, true)
        {
        }

        public QuestLogGump(PlayerMobile pm, bool closeGumps)
            : base(1046026) // Quest Log
        {
            m_Owner = pm;
            m_CloseGumps = closeGumps;

            if (closeGumps)
            {
                pm.CloseGump(typeof(QuestLogGump));
                pm.CloseGump(typeof(QuestLogDetailedGump));
            }

            RegisterButton(ButtonPosition.Right, ButtonGraphic.Okay, 3);

            SetPageCount(1);

            BuildPage();

            int numberColor, stringColor;

            MLQuestContext context = MLQuestSystem.GetContext(pm);

            if (context != null)
            {
                List<MLQuestInstance> instances = context.QuestInstances;

                for (int i = 0; i < instances.Count; ++i)
                {
                    if (instances[i].Failed)
                    {
                        numberColor = 0x3C00;
                        stringColor = 0x7B0000;
                    }
                    else
                    {
                        numberColor = stringColor = 0xFFFFFF;
                    }

                    TextDefinition.AddHtmlText(this, 98, 140 + 21 * i, 270, 21, instances[i].Quest.Title, false, false, numberColor, stringColor);
                    AddButton(368, 140 + 21 * i, 0x26B0, 0x26B1, 6 + i, GumpButtonType.Reply, 1);
                }
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID < 6)
                return;

            MLQuestContext context = MLQuestSystem.GetContext(m_Owner);

            if (context == null)
                return;

            List<MLQuestInstance> instances = context.QuestInstances;
            int index = info.ButtonID - 6;

            if (index >= instances.Count)
                return;

            sender.Mobile.SendGump(new QuestLogDetailedGump(instances[index], m_CloseGumps));
        }
    }

    public class QuestLogDetailedGump : BaseQuestGump
    {
        private MLQuestInstance m_Instance;
        private bool m_CloseGumps;

        public QuestLogDetailedGump(MLQuestInstance instance)
            : this(instance, true)
        {
        }

        public QuestLogDetailedGump(MLQuestInstance instance, bool closeGumps)
            : base(1046026) // Quest Log
        {
            m_Instance = instance;
            m_CloseGumps = closeGumps;

            PlayerMobile pm = instance.Player;
            MLQuest quest = instance.Quest;

            if (closeGumps)
            {
                CloseOtherGumps(pm);
                pm.CloseGump(typeof(QuestLogDetailedGump));
            }

            SetTitle(quest.Title);
            RegisterButton(ButtonPosition.Left, ButtonGraphic.Resign, 1);
            RegisterButton(ButtonPosition.Right, ButtonGraphic.Okay, 2);

            SetPageCount(3);

            BuildPage();
            AddDescription(quest);

            if (instance.Failed) // only displayed on the first page
                AddHtmlLocalized(160, 80, 250, 16, 500039, 0x3C00, false, false); // Failed!

            BuildPage();
            AddObjectivesProgress(instance);

            BuildPage();
            AddRewardsPage(quest);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (m_Instance.Removed)
                return;

            switch (info.ButtonID)
            {
                case 1: // Resign
                    {
                        // TODO: Custom reward loss protection? OSI doesn't have this
                        //if ( m_Instance.ClaimReward )
                        //	pm.SendMessage( "You cannot cancel a quest with rewards pending." );
                        //else

                        sender.Mobile.SendGump(new QuestCancelConfirmGump(m_Instance, m_CloseGumps));

                        break;
                    }
                case 2: // Okay
                    {
                        sender.Mobile.SendGump(new QuestLogGump(m_Instance.Player, m_CloseGumps));

                        break;
                    }
            }
        }
    }

    #endregion

    #region Mobile Quest Report

    public class QuestReportBackGump : BaseQuestGump
    {
        private MLQuestInstance m_Instance;

        public QuestReportBackGump(MLQuestInstance instance)
            : base(3006156) // Quest Conversation
        {
            m_Instance = instance;

            MLQuest quest = instance.Quest;
            PlayerMobile pm = instance.Player;

            // TODO: Check close sequence
            CloseOtherGumps(pm);

            SetTitle(quest.Title);
            RegisterButton(ButtonPosition.Left, ButtonGraphic.Continue, 4);
            RegisterButton(ButtonPosition.Right, ButtonGraphic.Close, 3);

            SetPageCount(1);

            BuildPage();
            AddConversation(quest.CompletionMessage);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 4)
                m_Instance.ContinueReportBack(true);
        }
    }

    #endregion

    #region Achievement Rewards

    public class QuestRewardGump : BaseQuestGump
    {
        private MLQuestInstance m_Instance;

        public QuestRewardGump(MLQuestInstance instance)
            : base(1072201) // Reward
        {
            m_Instance = instance;

            MLQuest quest = instance.Quest;
            PlayerMobile pm = instance.Player;

            CloseOtherGumps(pm);

            SetTitle(quest.Title);
            RegisterButton(ButtonPosition.Left, ButtonGraphic.Accept, 1);

            SetPageCount(1);

            BuildPage();
            AddRewards(quest);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 1)
                m_Instance.ClaimRewards();
        }
    }

    #endregion
}

namespace Server.Engines.MLQuests.Items
{
    #region Quest Object Engine

    public abstract class QuestGiverItem : Item, IQuestGiver
    {
        private List<MLQuest> m_MLQuests;

        public List<MLQuest> MLQuests
        {
            get
            {
                if (m_MLQuests == null)
                {
                    m_MLQuests = MLQuestSystem.FindQuestList(GetType());

                    if (m_MLQuests == null)
                        m_MLQuests = MLQuestSystem.EmptyList;
                }

                return m_MLQuests;
            }
        }

        public bool CanGiveMLQuest { get { return (MLQuests.Count != 0); } }

        public QuestGiverItem(int itemId)
            : base(itemId)
        {
        }

        public override bool Nontransferable { get { return true; } }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            AddQuestItemProperty(list);

            if (CanGiveMLQuest)
                list.Add(1072269); // Quest Giver
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(GetWorldLocation(), 2))
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
            else if (!IsChildOf(from.Backpack))
                from.SendLocalizedMessage(1042593); // That is not in your backpack.
            else if (MLQuestSystem.Enabled && CanGiveMLQuest && from is PlayerMobile)
                MLQuestSystem.OnDoubleClick(this, (PlayerMobile)from);
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            if (MLQuestSystem.Enabled)
                MLQuestSystem.HandleDeletion(this);
        }

        public QuestGiverItem(Serial serial)
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

    public abstract class TransientQuestGiverItem : TransientItem, IQuestGiver
    {
        private List<MLQuest> m_MLQuests;

        public List<MLQuest> MLQuests
        {
            get
            {
                if (m_MLQuests == null)
                {
                    m_MLQuests = MLQuestSystem.FindQuestList(GetType());

                    if (m_MLQuests == null)
                        m_MLQuests = MLQuestSystem.EmptyList;
                }

                return m_MLQuests;
            }
        }

        public bool CanGiveMLQuest { get { return (MLQuests.Count != 0); } }

        public TransientQuestGiverItem(int itemId, TimeSpan lifeSpan)
            : base(itemId, lifeSpan)
        {
        }

        public override bool Nontransferable { get { return true; } }

        public override void HandleInvalidTransfer(Mobile from)
        {
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            AddQuestItemProperty(list);

            if (CanGiveMLQuest)
                list.Add(1072269); // Quest Giver
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(GetWorldLocation(), 2))
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
            else if (!IsChildOf(from.Backpack))
                from.SendLocalizedMessage(1042593); // That is not in your backpack.
            else if (MLQuestSystem.Enabled && CanGiveMLQuest && from is PlayerMobile)
                MLQuestSystem.OnDoubleClick(this, (PlayerMobile)from);
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            if (MLQuestSystem.Enabled)
                MLQuestSystem.HandleDeletion(this);
        }

        public TransientQuestGiverItem(Serial serial)
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

    #endregion
}

namespace Server.Engines.MLQuests.Mobiles
{
    #region Heritage Collectors

    public abstract class DoneQuestCollector : BaseCreature, IRaceChanger
    {
        public override bool IsInvulnerable { get { return true; } }

        public abstract TextDefinition[] Offer { get; }
        public abstract TextDefinition[] Incomplete { get; }
        public abstract TextDefinition[] Complete { get; }
        public abstract Type[] Needed { get; }

        private InternalTimer m_Timer;

        public DoneQuestCollector()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            TryTalkTo(from, true);
        }

        public override void OnDoubleClickDead(Mobile from)
        {
            TryTalkTo(from, true);
        }

        public override void OnMovement(Mobile m, Point3D oldLocation)
        {
            if (m.Player && InRange(m, 6) && !InRange(oldLocation, 6))
                TryTalkTo(m, false);

            base.OnMovement(m, oldLocation);
        }

        public void TryTalkTo(Mobile from, bool fromClick)
        {
            if (!from.Hidden && !from.HasGump(typeOfRaceChangeConfirmGump) && !RaceChangeConfirmGump.IsPending(from.NetState) && CanTalkTo(from))
                TalkTo(from as PlayerMobile);
            else if (fromClick)
                DenyTalk(from);
        }

        public virtual bool CanTalkTo(Mobile from)
        {
            return true;
        }

        public virtual void DenyTalk(Mobile from)
        {
        }

        private static Type typeOfRaceChangeConfirmGump = typeof(RaceChangeConfirmGump);

        public void TalkTo(PlayerMobile pm)
        {
            if (pm == null || (m_Timer != null && m_Timer.Running))
                return;

            int completed = CompletedCount(pm);

            if (completed == Needed.Length)
            {
                m_Timer = new InternalTimer(this, pm, Complete, true);
            }
            else if (completed == 0)
            {
                m_Timer = new InternalTimer(this, pm, Offer, false);
            }
            else
            {
                List<TextDefinition> conversation = new List<TextDefinition>();
                conversation.AddRange(Incomplete);

                MLQuestContext context = MLQuestSystem.GetContext(pm);

                if (context != null)
                {
                    foreach (Type type in Needed)
                    {
                        MLQuest quest = MLQuestSystem.FindQuest(type);

                        if (quest == null || context.HasDoneQuest(quest))
                            continue;

                        conversation.Add(quest.Title);
                    }
                }

                m_Timer = new InternalTimer(this, pm, conversation, false);
            }

            m_Timer.Start();
        }

        private int CompletedCount(PlayerMobile pm)
        {
            MLQuestContext context = MLQuestSystem.GetContext(pm);

            if (context == null)
                return 0;

            int result = 0;

            foreach (Type type in Needed)
            {
                MLQuest quest = MLQuestSystem.FindQuest(type);

                if (quest == null || context.HasDoneQuest(quest))
                    ++result;
            }

            return result;
        }

        public bool CheckComplete(PlayerMobile pm)
        {
            if (CompletedCount(pm) == Needed.Length)
                return true;

            pm.SendLocalizedMessage(1073644); // You must complete all the tasks before proceeding...
            return false;
        }

        public void ConsumeNeeded(PlayerMobile pm)
        {
            MLQuestContext context = MLQuestSystem.GetContext(pm);

            if (context != null)
            {
                foreach (Type type in Needed)
                {
                    MLQuest quest = MLQuestSystem.FindQuest(type);

                    if (quest != null)
                        context.RemoveDoneQuest(quest);
                }
            }
        }

        public void OnCancel(PlayerMobile pm)
        {
            pm.SendLocalizedMessage(1073645); // You may try this again later...
        }

        public virtual void OnComplete(PlayerMobile pm)
        {
        }

        public DoneQuestCollector(Serial serial)
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

        private class InternalTimer : Timer
        {
            private DoneQuestCollector m_Owner;
            private PlayerMobile m_Target;
            private IList<TextDefinition> m_Conversation;
            private bool m_IsComplete;
            private int m_Index;

            private static TimeSpan GetDelay()
            {
                return TimeSpan.FromSeconds(Utility.RandomBool() ? 3 : 4);
            }

            public InternalTimer(DoneQuestCollector owner, PlayerMobile target, IList<TextDefinition> conversation, bool isComplete)
                : base(TimeSpan.Zero, GetDelay())
            {
                m_Owner = owner;
                m_Target = target;
                m_Conversation = conversation;
                m_IsComplete = isComplete;
                m_Index = 0;
            }

            protected override void OnTick()
            {
                if (m_Owner.Deleted)
                {
                    Stop();
                    return;
                }

                if (m_Index >= m_Conversation.Count)
                {
                    if (m_IsComplete)
                        m_Owner.OnComplete(m_Target);

                    Stop();
                }
                else
                {
                    if (m_Index == 0)
                    {
                        if (m_Target.ShowFameTitle && m_Target.Fame >= 10000)
                            m_Owner.Say(true, String.Format("{0} {1}", m_Target.Female ? "Lady" : "Lord", m_Target.Name));
                        else
                            m_Owner.Say(true, m_Target.Name);
                    }

                    TextDefinition.PublicOverheadMessage(m_Owner, MessageType.Regular, 0x3B2, m_Conversation[m_Index++]);
                    Interval = GetDelay();
                }
            }
        }
    }

    #endregion
}

namespace Server.Engines.MLQuests.Objectives
{
    public abstract class BaseObjective
    {
        public virtual bool IsTimed { get { return false; } }
        public virtual TimeSpan Duration { get { return TimeSpan.Zero; } }

        public BaseObjective()
        {
        }

        public virtual bool CanOffer(IQuestGiver quester, PlayerMobile pm, bool message)
        {
            return true;
        }

        public abstract void WriteToGump(Gump g, ref int y);

        public virtual BaseObjectiveInstance CreateInstance(MLQuestInstance instance)
        {
            return null;
        }
    }

    public abstract class BaseObjectiveInstance
    {
        private MLQuestInstance m_Instance;
        private DateTime m_EndTime;
        private bool m_Expired;

        public MLQuestInstance Instance
        {
            get { return m_Instance; }
        }

        public bool IsTimed
        {
            get { return (m_EndTime != DateTime.MinValue); }
        }

        public DateTime EndTime
        {
            get { return m_EndTime; }
            set { m_EndTime = value; }
        }

        public bool Expired
        {
            get { return m_Expired; }
            set { m_Expired = value; }
        }

        public BaseObjectiveInstance(MLQuestInstance instance, BaseObjective obj)
        {
            m_Instance = instance;

            if (obj.IsTimed)
                m_EndTime = DateTime.UtcNow + obj.Duration;
        }

        public virtual void WriteToGump(Gump g, ref int y)
        {
            if (IsTimed)
                WriteTimeRemaining(g, ref y, (m_EndTime > DateTime.UtcNow) ? (m_EndTime - DateTime.UtcNow) : TimeSpan.Zero);
        }

        public static void WriteTimeRemaining(Gump g, ref int y, TimeSpan timeRemaining)
        {
            g.AddHtmlLocalized(103, y, 120, 16, 1062379, 0x15F90, false, false); // Est. time remaining:
            g.AddLabel(223, y, 0x481, timeRemaining.TotalSeconds.ToString("F0"));
            y += 16;
        }

        public virtual bool AllowsQuestItem(Item item, Type type)
        {
            return false;
        }

        public virtual bool IsCompleted()
        {
            return false;
        }

        public virtual void CheckComplete()
        {
            if (IsCompleted())
            {
                m_Instance.Player.PlaySound(0x5B6); // public sound
                m_Instance.CheckComplete();
            }
        }

        public virtual void OnQuestAccepted()
        {
        }

        public virtual void OnQuestCancelled()
        {
        }

        public virtual void OnQuestCompleted()
        {
        }

        public virtual bool OnBeforeClaimReward()
        {
            return true;
        }

        public virtual void OnClaimReward()
        {
        }

        public virtual void OnAfterClaimReward()
        {
        }

        public virtual void OnRewardClaimed()
        {
        }

        public virtual void OnQuesterDeleted()
        {
        }

        public virtual void OnPlayerDeath()
        {
        }

        public virtual void OnExpire()
        {
        }

        public enum DataType : byte
        {
            None,
            EscortObjective,
            KillObjective,
            DeliverObjective
        }

        public virtual DataType ExtraDataType { get { return DataType.None; } }

        public virtual void Serialize(GenericWriter writer)
        {
            // Version info is written in MLQuestPersistence.Serialize

            if (IsTimed)
            {
                writer.Write(true);
                writer.WriteDeltaTime(m_EndTime);
            }
            else
            {
                writer.Write(false);
            }

            // For type checks on deserialization
            // (This way quest objectives can be changed without breaking serialization)
            writer.Write((byte)ExtraDataType);
        }

        public static void Deserialize(GenericReader reader, int version, BaseObjectiveInstance objInstance)
        {
            if (reader.ReadBool())
            {
                DateTime endTime = reader.ReadDeltaTime();

                if (objInstance != null)
                    objInstance.EndTime = endTime;
            }

            DataType extraDataType = (DataType)reader.ReadByte();

            switch (extraDataType)
            {
                case DataType.EscortObjective:
                    {
                        bool completed = reader.ReadBool();

                        if (objInstance is EscortObjectiveInstance)
                            ((EscortObjectiveInstance)objInstance).HasCompleted = completed;

                        break;
                    }
                case DataType.KillObjective:
                    {
                        int slain = reader.ReadInt();

                        if (objInstance is KillObjectiveInstance)
                            ((KillObjectiveInstance)objInstance).Slain = slain;

                        break;
                    }
                case DataType.DeliverObjective:
                    {
                        bool completed = reader.ReadBool();

                        if (objInstance is DeliverObjectiveInstance)
                            ((DeliverObjectiveInstance)objInstance).HasCompleted = completed;

                        break;
                    }
            }
        }
    }

    #region Questing Objectives

    /// <summary>
    /// Objective: Collect Objects
    /// </summary>
    public class CollectObjective : BaseObjective
    {
        private int m_DesiredAmount;
        private Type m_AcceptedType;
        private TextDefinition m_Name;

        public int DesiredAmount
        {
            get { return m_DesiredAmount; }
            set { m_DesiredAmount = value; }
        }

        public Type AcceptedType
        {
            get { return m_AcceptedType; }
            set { m_AcceptedType = value; }
        }

        public TextDefinition Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public virtual bool ShowDetailed
        {
            get { return true; }
        }

        public CollectObjective()
            : this(0, null, null)
        {
        }

        public CollectObjective(int amount, Type type, TextDefinition name)
        {
            m_DesiredAmount = amount;
            m_AcceptedType = type;
            m_Name = name;

            if (MLQuestSystem.Debug && ShowDetailed && name.Number > 0)
            {
                int itemid = LabelToItemID(name.Number);

                if (itemid <= 0 || itemid > 0x4000)
                    Console.WriteLine("Warning: cliloc {0} is likely giving the wrong item ID", name.Number);
            }
        }

        public bool CheckType(Type type)
        {
            return (m_AcceptedType != null && m_AcceptedType.IsAssignableFrom(type));
        }

        public virtual bool CheckItem(Item item)
        {
            return true;
        }

        public static int LabelToItemID(int label)
        {
            if (label < 1078872)
                return (label - 1020000);
            else
                return (label - 1078872);
        }

        public override void WriteToGump(Gump g, ref int y)
        {
            if (ShowDetailed)
            {
                string amount = m_DesiredAmount.ToString();

                g.AddHtmlLocalized(98, y, 350, 16, 1072205, 0x15F90, false, false); // Obtain
                g.AddLabel(143, y, 0x481, amount);

                if (m_Name.Number > 0)
                {
                    g.AddHtmlLocalized(143 + amount.Length * 15, y, 190, 18, m_Name.Number, 0x77BF, false, false);
                    g.AddItem(350, y, LabelToItemID(m_Name.Number));
                }
                else if (m_Name.String != null)
                {
                    g.AddLabel(143 + amount.Length * 15, y, 0x481, m_Name.String);
                }
            }
            else
            {
                if (m_Name.Number > 0)
                    g.AddHtmlLocalized(98, y, 312, 32, m_Name.Number, 0x15F90, false, false);
                else if (m_Name.String != null)
                    g.AddLabel(98, y, 0x481, m_Name.String);
            }

            y += 32;
        }

        public override BaseObjectiveInstance CreateInstance(MLQuestInstance instance)
        {
            return new CollectObjectiveInstance(this, instance);
        }
    }

    public class TimedCollectObjective : CollectObjective
    {
        private TimeSpan m_Duration;

        public override bool IsTimed { get { return true; } }
        public override TimeSpan Duration { get { return m_Duration; } }

        public TimedCollectObjective(TimeSpan duration, int amount, Type type, TextDefinition name)
            : base(amount, type, name)
        {
            m_Duration = duration;
        }
    }

    public class CollectObjectiveInstance : BaseObjectiveInstance
    {
        private CollectObjective m_Objective;

        public CollectObjective Objective
        {
            get { return m_Objective; }
            set { m_Objective = value; }
        }

        public CollectObjectiveInstance(CollectObjective objective, MLQuestInstance instance)
            : base(instance, objective)
        {
            m_Objective = objective;
        }

        private int GetCurrentTotal()
        {
            Container pack = Instance.Player.Backpack;

            if (pack == null)
                return 0;

            Item[] items = pack.FindItemsByType(m_Objective.AcceptedType, false); // Note: subclasses are included
            int total = 0;

            foreach (Item item in items)
            {
                if (item.QuestItem && m_Objective.CheckItem(item))
                    total += item.Amount;
            }

            return total;
        }

        public override bool AllowsQuestItem(Item item, Type type)
        {
            return (m_Objective.CheckType(type) && m_Objective.CheckItem(item));
        }

        public override bool IsCompleted()
        {
            return (GetCurrentTotal() >= m_Objective.DesiredAmount);
        }

        public override void OnQuestCancelled()
        {
            PlayerMobile pm = Instance.Player;
            Container pack = pm.Backpack;

            if (pack == null)
                return;

            Type checkType = m_Objective.AcceptedType;
            Item[] items = pack.FindItemsByType(checkType, false);

            foreach (Item item in items)
            {
                if (item.QuestItem && !MLQuestSystem.CanMarkQuestItem(pm, item, checkType)) // does another quest still need this item? (OSI just unmarks everything)
                    item.QuestItem = false;
            }
        }

        // Should only be called after IsComplete() is checked to be true
        public override void OnClaimReward()
        {
            Container pack = Instance.Player.Backpack;

            if (pack == null)
                return;

            // TODO: OSI also counts the item in the cursor?

            Item[] items = pack.FindItemsByType(m_Objective.AcceptedType, false);
            int left = m_Objective.DesiredAmount;

            foreach (Item item in items)
            {
                if (item.QuestItem && m_Objective.CheckItem(item))
                {
                    if (left == 0)
                        return;

                    if (item.Amount > left)
                    {
                        item.Consume(left);
                        left = 0;
                    }
                    else
                    {
                        item.Delete();
                        left -= item.Amount;
                    }
                }
            }
        }

        public override void OnAfterClaimReward()
        {
            OnQuestCancelled(); // same thing, clear other quest items
        }

        public override void OnExpire()
        {
            OnQuestCancelled();

            // No message
        }

        public override void WriteToGump(Gump g, ref int y)
        {
            m_Objective.WriteToGump(g, ref y);
            y -= 16;

            if (m_Objective.ShowDetailed)
            {
                base.WriteToGump(g, ref y);

                g.AddHtmlLocalized(103, y, 120, 16, 3000087, 0x15F90, false, false); // Total
                g.AddLabel(223, y, 0x481, GetCurrentTotal().ToString());
                y += 16;

                g.AddHtmlLocalized(103, y, 120, 16, 1074782, 0x15F90, false, false); // Return to
                g.AddLabel(223, y, 0x481, QuesterNameAttribute.GetQuesterNameFor(Instance.QuesterType));
                y += 16;
            }
        }
    }

    /// <summary>
    /// Objective: Deliver Objects
    /// </summary>
    public class DeliverObjective : BaseObjective
    {
        private Type m_Delivery;
        private int m_Amount;
        private TextDefinition m_Name;
        private Type m_Destination;
        private bool m_SpawnsDelivery;

        public Type Delivery
        {
            get { return m_Delivery; }
            set { m_Delivery = value; }
        }

        public int Amount
        {
            get { return m_Amount; }
            set { m_Amount = value; }
        }

        public TextDefinition Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public Type Destination
        {
            get { return m_Destination; }
            set { m_Destination = value; }
        }

        public bool SpawnsDelivery
        {
            get { return m_SpawnsDelivery; }
            set { m_SpawnsDelivery = value; }
        }

        public DeliverObjective(Type delivery, int amount, TextDefinition name, Type destination)
            : this(delivery, amount, name, destination, true)
        {
        }

        public DeliverObjective(Type delivery, int amount, TextDefinition name, Type destination, bool spawnsDelivery)
        {
            m_Delivery = delivery;
            m_Amount = amount;
            m_Name = name;
            m_Destination = destination;
            m_SpawnsDelivery = spawnsDelivery;

            if (MLQuestSystem.Debug && name.Number > 0)
            {
                int itemid = CollectObjective.LabelToItemID(name.Number);

                if (itemid <= 0 || itemid > 0x4000)
                    Console.WriteLine("Warning: cliloc {0} is likely giving the wrong item ID", name.Number);
            }
        }

        public virtual void SpawnDelivery(Container pack)
        {
            if (!m_SpawnsDelivery || pack == null)
                return;

            List<Item> delivery = new List<Item>();

            for (int i = 0; i < m_Amount; ++i)
            {
                Item item = Activator.CreateInstance(m_Delivery) as Item;

                if (item == null)
                    continue;

                delivery.Add(item);

                if (item.Stackable && m_Amount > 1)
                {
                    item.Amount = m_Amount;
                    break;
                }
            }

            foreach (Item item in delivery)
                pack.DropItem(item); // Confirmed: on OSI items are added even if your pack is full
        }

        public override void WriteToGump(Gump g, ref int y)
        {
            string amount = m_Amount.ToString();

            g.AddHtmlLocalized(98, y, 312, 16, 1072207, 0x15F90, false, false); // Deliver
            g.AddLabel(143, y, 0x481, amount);

            if (m_Name.Number > 0)
            {
                g.AddHtmlLocalized(143 + amount.Length * 15, y, 190, 18, m_Name.Number, 0x77BF, false, false);
                g.AddItem(350, y, CollectObjective.LabelToItemID(m_Name.Number));
            }
            else if (m_Name.String != null)
            {
                g.AddLabel(143 + amount.Length * 15, y, 0x481, m_Name.String);
            }

            y += 32;

            g.AddHtmlLocalized(103, y, 120, 16, 1072379, 0x15F90, false, false); // Deliver to
            g.AddLabel(223, y, 0x481, QuesterNameAttribute.GetQuesterNameFor(m_Destination));

            y += 16;
        }

        public override BaseObjectiveInstance CreateInstance(MLQuestInstance instance)
        {
            return new DeliverObjectiveInstance(this, instance);
        }
    }

    public class TimedDeliverObjective : DeliverObjective
    {
        private TimeSpan m_Duration;

        public override bool IsTimed { get { return true; } }
        public override TimeSpan Duration { get { return m_Duration; } }

        public TimedDeliverObjective(TimeSpan duration, Type delivery, int amount, TextDefinition name, Type destination)
            : this(duration, delivery, amount, name, destination, true)
        {
        }

        public TimedDeliverObjective(TimeSpan duration, Type delivery, int amount, TextDefinition name, Type destination, bool spawnsDelivery)
            : base(delivery, amount, name, destination, spawnsDelivery)
        {
            m_Duration = duration;
        }
    }

    public class DeliverObjectiveInstance : BaseObjectiveInstance
    {
        private DeliverObjective m_Objective;
        private bool m_HasCompleted;

        public DeliverObjective Objective
        {
            get { return m_Objective; }
            set { m_Objective = value; }
        }

        public bool HasCompleted
        {
            get { return m_HasCompleted; }
            set { m_HasCompleted = value; }
        }

        public DeliverObjectiveInstance(DeliverObjective objective, MLQuestInstance instance)
            : base(instance, objective)
        {
            m_Objective = objective;
        }

        public virtual bool IsDestination(IQuestGiver quester, Type type)
        {
            Type destType = m_Objective.Destination;

            return (destType != null && destType.IsAssignableFrom(type));
        }

        public override bool IsCompleted()
        {
            return m_HasCompleted;
        }

        public override void OnQuestAccepted()
        {
            m_Objective.SpawnDelivery(Instance.Player.Backpack);
        }

        // This is VERY similar to CollectObjective.GetCurrentTotal
        private int GetCurrentTotal()
        {
            Container pack = Instance.Player.Backpack;

            if (pack == null)
                return 0;

            Item[] items = pack.FindItemsByType(m_Objective.Delivery, false); // Note: subclasses are included
            int total = 0;

            foreach (Item item in items)
                total += item.Amount;

            return total;
        }

        public override bool OnBeforeClaimReward()
        {
            PlayerMobile pm = Instance.Player;

            int total = GetCurrentTotal();
            int desired = m_Objective.Amount;

            if (total < desired)
            {
                pm.SendLocalizedMessage(1074861); // You do not have everything you need!
                pm.SendLocalizedMessage(1074885, String.Format("{0}\t{1}", total, desired)); // You have ~1_val~ item(s) but require ~2_val~
                return false;
            }

            return true;
        }

        // TODO: This is VERY similar to CollectObjective.OnClaimReward
        public override void OnClaimReward()
        {
            Container pack = Instance.Player.Backpack;

            if (pack == null)
                return;

            Item[] items = pack.FindItemsByType(m_Objective.Delivery, false);
            int left = m_Objective.Amount;

            foreach (Item item in items)
            {
                if (left == 0)
                    break;

                if (item.Amount > left)
                {
                    item.Consume(left);
                    left = 0;
                }
                else
                {
                    item.Delete();
                    left -= item.Amount;
                }
            }
        }

        public override void OnQuestCancelled()
        {
            OnClaimReward(); // same effect
        }

        public override void OnExpire()
        {
            OnQuestCancelled();

            Instance.Player.SendLocalizedMessage(1074813); // You have failed to complete your delivery.
        }

        public override void WriteToGump(Gump g, ref int y)
        {
            m_Objective.WriteToGump(g, ref y);

            base.WriteToGump(g, ref y);

            // No extra instance stuff printed for this objective
        }

        public override DataType ExtraDataType { get { return DataType.DeliverObjective; } }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(m_HasCompleted);
        }
    }

    /// <summary>
    /// Objective: Escort Mobiles
    /// </summary>
    public class EscortObjective : BaseObjective
    {
        private QuestArea m_Destination;

        public QuestArea Destination
        {
            get { return m_Destination; }
            set { m_Destination = value; }
        }

        public EscortObjective()
            : this(null)
        {
        }

        public EscortObjective(QuestArea destination)
        {
            m_Destination = destination;
        }

        public override bool CanOffer(IQuestGiver quester, PlayerMobile pm, bool message)
        {
            if ((quester is BaseCreature && ((BaseCreature)quester).Controlled) || (quester is BaseEscortable && ((BaseEscortable)quester).IsBeingDeleted))
                return false;

            MLQuestContext context = MLQuestSystem.GetContext(pm);

            if (context != null)
            {
                foreach (MLQuestInstance instance in context.QuestInstances)
                {
                    if (instance.Quest.IsEscort)
                    {
                        if (message)
                            MLQuestSystem.Tell(quester, pm, 500896); // I see you already have an escort.

                        return false;
                    }
                }
            }

            DateTime nextEscort = pm.LastEscortTime + BaseEscortable.EscortDelay;

            if (nextEscort > DateTime.UtcNow)
            {
                if (message)
                {
                    int minutes = (int)Math.Ceiling((nextEscort - DateTime.UtcNow).TotalMinutes);

                    if (minutes == 1)
                        MLQuestSystem.Tell(quester, pm, "You must rest 1 minute before we set out on this journey.");
                    else
                        MLQuestSystem.Tell(quester, pm, 1071195, minutes.ToString()); // You must rest ~1_minsleft~ minutes before we set out on this journey.
                }

                return false;
            }

            return true;
        }

        public override void WriteToGump(Gump g, ref int y)
        {
            g.AddHtmlLocalized(98, y, 312, 16, 1072206, 0x15F90, false, false); // Escort to

            if (m_Destination.Name.Number > 0)
                g.AddHtmlLocalized(173, y, 312, 20, m_Destination.Name.Number, 0xFFFFFF, false, false);
            else if (m_Destination.Name.String != null)
                g.AddLabel(173, y, 0x481, m_Destination.Name.String);

            y += 16;
        }

        public override BaseObjectiveInstance CreateInstance(MLQuestInstance instance)
        {
            if (instance == null || m_Destination == null)
                return null;

            return new EscortObjectiveInstance(this, instance);
        }
    }

    public class EscortObjectiveInstance : BaseObjectiveInstance
    {
        private EscortObjective m_Objective;
        private bool m_HasCompleted;
        private Timer m_Timer;
        private DateTime m_LastSeenEscorter;
        private BaseCreature m_Escort;

        public bool HasCompleted
        {
            get { return m_HasCompleted; }
            set { m_HasCompleted = value; }
        }

        public EscortObjectiveInstance(EscortObjective objective, MLQuestInstance instance)
            : base(instance, objective)
        {
            m_Objective = objective;
            m_HasCompleted = false;
            m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5), new TimerCallback(CheckDestination));
            m_LastSeenEscorter = DateTime.UtcNow;
            m_Escort = instance.Quester as BaseCreature;

            if (MLQuestSystem.Debug && m_Escort == null && instance.Quester != null)
                Console.WriteLine("Warning: EscortObjective is not supported for type '{0}'", instance.Quester.GetType().Name);
        }

        public override bool IsCompleted()
        {
            return m_HasCompleted;
        }

        private void CheckDestination()
        {
            if (m_Escort == null || m_HasCompleted) // Completed by deserialization
            {
                StopTimer();
                return;
            }

            MLQuestInstance instance = Instance;
            PlayerMobile pm = instance.Player;

            if (instance.Removed)
            {
                Abandon();
            }
            else if (m_Objective.Destination.Contains(m_Escort))
            {
                m_Escort.Say(1042809, pm.Name); // We have arrived! I thank thee, ~1_PLAYER_NAME~! I have no further need of thy services. Here is thy pay.

                if (pm.Young || m_Escort.Region.IsPartOf("Haven Island"))
                    Titles.AwardFame(pm, 10, true);
                else
                    VirtueHelper.AwardVirtue(pm, VirtueName.Compassion, (m_Escort is BaseEscortable && ((BaseEscortable)m_Escort).IsPrisoner) ? 400 : 200);

                EndFollow(m_Escort);
                StopTimer();

                m_HasCompleted = true;
                CheckComplete();

                // Auto claim reward
                MLQuestSystem.OnDoubleClick(m_Escort, pm);
            }
            else if (pm.Map != m_Escort.Map || !pm.InRange(m_Escort, 30)) // TODO: verify range
            {
                if (m_LastSeenEscorter + BaseEscortable.AbandonDelay <= DateTime.UtcNow)
                    Abandon();
            }
            else
            {
                m_LastSeenEscorter = DateTime.UtcNow;
            }
        }

        private void StopTimer()
        {
            if (m_Timer != null)
            {
                m_Timer.Stop();
                m_Timer = null;
            }
        }

        public static void BeginFollow(BaseCreature quester, PlayerMobile pm)
        {
            quester.ControlSlots = 0;
            quester.SetControlMaster(pm);

            quester.ActiveSpeed = 0.1;
            quester.PassiveSpeed = 0.2;

            quester.ControlOrder = OrderType.Follow;
            quester.ControlTarget = pm;

            quester.CantWalk = false;
            quester.CurrentSpeed = 0.1;
        }

        public static void EndFollow(BaseCreature quester)
        {
            quester.ActiveSpeed = 0.2;
            quester.PassiveSpeed = 1.0;

            quester.ControlOrder = OrderType.None;
            quester.ControlTarget = null;

            quester.CurrentSpeed = 1.0;

            quester.SetControlMaster(null);

            if (quester is BaseEscortable)
                ((BaseEscortable)quester).BeginDelete();
        }

        public override void OnQuestAccepted()
        {
            MLQuestInstance instance = Instance;
            PlayerMobile pm = instance.Player;

            pm.LastEscortTime = DateTime.UtcNow;

            if (m_Escort != null)
                BeginFollow(m_Escort, pm);
        }

        public void Abandon()
        {
            StopTimer();

            MLQuestInstance instance = Instance;
            PlayerMobile pm = instance.Player;

            if (m_Escort != null && !m_Escort.Deleted)
            {
                if (!pm.Alive)
                    m_Escort.Say(500901); // Ack!  My escort has come to haunt me!
                else
                    m_Escort.Say(500902); // My escort seems to have abandoned me!

                EndFollow(m_Escort);
            }

            // Note: this sound is sent twice on OSI (once here and once in Cancel())
            //m_Player.SendSound( 0x5B3 ); // private sound
            pm.SendLocalizedMessage(1071194); // You have failed your escort quest...

            if (!instance.Removed)
                instance.Cancel();
        }

        public override void OnQuesterDeleted()
        {
            if (IsCompleted() || Instance.Removed)
                return;

            Abandon();
        }

        public override void OnPlayerDeath()
        {
            // Note: OSI also cancels it when the quest is already complete
            if ( /*IsCompleted() ||*/ Instance.Removed)
                return;

            Instance.Cancel();
        }

        public override void OnExpire()
        {
            Abandon();
        }

        public override void WriteToGump(Gump g, ref int y)
        {
            m_Objective.WriteToGump(g, ref y);

            base.WriteToGump(g, ref y);

            // No extra instance stuff printed for this objective
        }

        public override DataType ExtraDataType { get { return DataType.EscortObjective; } }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(m_HasCompleted);
        }
    }

    /// <summary>
    /// Objective: Skill Gaining
    /// </summary>
    public enum GainSkillObjectiveFlags : byte
    {
        None = 0x00,
        UseReal = 0x01,
        Accelerate = 0x02
    }

    public class GainSkillObjective : BaseObjective
    {
        private SkillName m_Skill;
        private int m_ThresholdFixed;
        private GainSkillObjectiveFlags m_Flags;

        public SkillName Skill
        {
            get { return m_Skill; }
            set { m_Skill = value; }
        }

        public int ThresholdFixed
        {
            get { return m_ThresholdFixed; }
            set { m_ThresholdFixed = value; }
        }

        public bool UseReal
        {
            get { return GetFlag(GainSkillObjectiveFlags.UseReal); }
            set { SetFlag(GainSkillObjectiveFlags.UseReal, value); }
        }

        public bool Accelerate
        {
            get { return GetFlag(GainSkillObjectiveFlags.Accelerate); }
            set { SetFlag(GainSkillObjectiveFlags.Accelerate, value); }
        }

        public GainSkillObjective()
            : this(SkillName.Alchemy, 0)
        {
        }

        public GainSkillObjective(SkillName skill, int thresholdFixed)
            : this(skill, thresholdFixed, false, false)
        {
        }

        public GainSkillObjective(SkillName skill, int thresholdFixed, bool useReal, bool accelerate)
        {
            m_Skill = skill;
            m_ThresholdFixed = thresholdFixed;
            m_Flags = GainSkillObjectiveFlags.None;

            if (useReal)
                m_Flags |= GainSkillObjectiveFlags.UseReal;

            if (accelerate)
                m_Flags |= GainSkillObjectiveFlags.Accelerate;
        }

        public override bool CanOffer(IQuestGiver quester, PlayerMobile pm, bool message)
        {
            Skill skill = pm.Skills[m_Skill];

            if ((UseReal ? skill.Fixed : skill.BaseFixedPoint) >= m_ThresholdFixed)
            {
                if (message)
                    MLQuestSystem.Tell(quester, pm, 1077772); // I cannot teach you, for you know all I can teach!

                return false;
            }

            return true;
        }

        public override void WriteToGump(Gump g, ref int y)
        {
            int skillLabel = AosSkillBonuses.GetLabel(m_Skill);
            string args;

            if (m_ThresholdFixed % 10 == 0)
                args = String.Format("#{0}\t{1}", skillLabel, m_ThresholdFixed / 10); // as seen on OSI
            else
                args = String.Format("#{0}\t{1:0.0}", skillLabel, (double)m_ThresholdFixed / 10); // for non-integer skill levels

            g.AddHtmlLocalized(98, y, 312, 16, 1077485, args, 0x15F90, false, false); // Increase ~1_SKILL~ to ~2_VALUE~
            y += 16;
        }

        public override BaseObjectiveInstance CreateInstance(MLQuestInstance instance)
        {
            return new GainSkillObjectiveInstance(this, instance);
        }

        private bool GetFlag(GainSkillObjectiveFlags flag)
        {
            return ((m_Flags & flag) != 0);
        }

        private void SetFlag(GainSkillObjectiveFlags flag, bool value)
        {
            if (value)
                m_Flags |= flag;
            else
                m_Flags &= ~flag;
        }
    }

    /// <summary>
    ///  On OSI, Once This Is Complete, It Will *Stay* Complete, Even If You Lower Your Skill Again.
    /// </summary>
    public class GainSkillObjectiveInstance : BaseObjectiveInstance
    {
        private GainSkillObjective m_Objective;

        public GainSkillObjective Objective
        {
            get { return m_Objective; }
            set { m_Objective = value; }
        }

        public GainSkillObjectiveInstance(GainSkillObjective objective, MLQuestInstance instance)
            : base(instance, objective)
        {
            m_Objective = objective;
        }

        public bool Handles(SkillName skill)
        {
            return (m_Objective.Skill == skill);
        }

        public override bool IsCompleted()
        {
            PlayerMobile pm = Instance.Player;

            int valueFixed = m_Objective.UseReal ? pm.Skills[m_Objective.Skill].Fixed : pm.Skills[m_Objective.Skill].BaseFixedPoint;

            return (valueFixed >= m_Objective.ThresholdFixed);
        }

        // TODO: This may interfere with scrolls, or even quests among each other
        // How does OSI deal with this?
        public override void OnQuestAccepted()
        {
            if (!m_Objective.Accelerate)
                return;

            PlayerMobile pm = Instance.Player;

            pm.AcceleratedSkill = m_Objective.Skill;
            pm.AcceleratedStart = DateTime.UtcNow + TimeSpan.FromMinutes(15); // TODO: Is there a max duration?
        }

        public override void OnQuestCancelled()
        {
            if (!m_Objective.Accelerate)
                return;

            PlayerMobile pm = Instance.Player;

            pm.AcceleratedStart = DateTime.UtcNow;
            pm.PlaySound(0x100);
        }

        public override void OnQuestCompleted()
        {
            OnQuestCancelled();
        }

        public override void WriteToGump(Gump g, ref int y)
        {
            m_Objective.WriteToGump(g, ref y);

            base.WriteToGump(g, ref y);

            if (IsCompleted())
            {
                g.AddHtmlLocalized(113, y, 312, 20, 1055121, 0xFFFFFF, false, false); // Complete
                y += 16;
            }
        }
    }

    /// <summary>
    /// Objective: Kill A Target
    /// </summary>
    public class KillObjective : BaseObjective
    {
        private int m_DesiredAmount;
        private Type[] m_AcceptedTypes; // Example of Type[] requirement on OSI: killing X bone magis or skeletal mages (probably the same type on OSI though?)
        private TextDefinition m_Name;
        private QuestArea m_Area;

        public int DesiredAmount
        {
            get { return m_DesiredAmount; }
            set { m_DesiredAmount = value; }
        }

        public Type[] AcceptedTypes
        {
            get { return m_AcceptedTypes; }
            set { m_AcceptedTypes = value; }
        }

        public TextDefinition Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public QuestArea Area
        {
            get { return m_Area; }
            set { m_Area = value; }
        }

        public KillObjective()
            : this(0, null, null, null)
        {
        }

        public KillObjective(int amount, Type[] types, TextDefinition name)
            : this(amount, types, name, null)
        {
        }

        public KillObjective(int amount, Type[] types, TextDefinition name, QuestArea area)
        {
            m_DesiredAmount = amount;
            m_AcceptedTypes = types;
            m_Name = name;
            m_Area = area;
        }

        public override void WriteToGump(Gump g, ref int y)
        {
            string amount = m_DesiredAmount.ToString();

            g.AddHtmlLocalized(98, y, 312, 16, 1072204, 0x15F90, false, false); // Slay
            g.AddLabel(133, y, 0x481, amount);

            if (m_Name.Number > 0)
                g.AddHtmlLocalized(133 + amount.Length * 15, y, 190, 18, m_Name.Number, 0x77BF, false, false);
            else if (m_Name.String != null)
                g.AddLabel(133 + amount.Length * 15, y, 0x481, m_Name.String);

            y += 16;

            #region Location
            if (m_Area != null)
            {
                g.AddHtmlLocalized(103, y, 312, 20, 1018327, 0x15F90, false, false); // Location

                if (m_Area.Name.Number > 0)
                    g.AddHtmlLocalized(223, y, 312, 20, m_Area.Name.Number, 0xFFFFFF, false, false);
                else if (m_Area.Name.String != null)
                    g.AddLabel(223, y, 0x481, m_Area.Name.String);

                y += 16;
            }
            #endregion
        }

        public override BaseObjectiveInstance CreateInstance(MLQuestInstance instance)
        {
            return new KillObjectiveInstance(this, instance);
        }
    }

    public class TimedKillObjective : KillObjective
    {
        private TimeSpan m_Duration;

        public override bool IsTimed { get { return true; } }
        public override TimeSpan Duration { get { return m_Duration; } }

        public TimedKillObjective(TimeSpan duration, int amount, Type[] types, TextDefinition name)
            : this(duration, amount, types, name, null)
        {
        }

        public TimedKillObjective(TimeSpan duration, int amount, Type[] types, TextDefinition name, QuestArea area)
            : base(amount, types, name, area)
        {
            m_Duration = duration;
        }
    }

    public class KillObjectiveInstance : BaseObjectiveInstance
    {
        private KillObjective m_Objective;
        private int m_Slain;

        public KillObjective Objective
        {
            get { return m_Objective; }
            set { m_Objective = value; }
        }

        public int Slain
        {
            get { return m_Slain; }
            set { m_Slain = value; }
        }

        public KillObjectiveInstance(KillObjective objective, MLQuestInstance instance)
            : base(instance, objective)
        {
            m_Objective = objective;
            m_Slain = 0;
        }

        public bool AddKill(Mobile mob, Type type)
        {
            int desired = m_Objective.DesiredAmount;

            foreach (Type acceptedType in m_Objective.AcceptedTypes)
            {
                if (acceptedType.IsAssignableFrom(type))
                {
                    if (m_Objective.Area != null && !m_Objective.Area.Contains(mob))
                        return false;

                    PlayerMobile pm = Instance.Player;

                    if (++m_Slain >= desired)
                        pm.SendLocalizedMessage(1075050); // You have killed all the required quest creatures of this type.
                    else
                        pm.SendLocalizedMessage(1075051, (desired - m_Slain).ToString()); // You have killed a quest creature. ~1_val~ more left.

                    return true;
                }
            }

            return false;
        }

        public override bool IsCompleted()
        {
            return (m_Slain >= m_Objective.DesiredAmount);
        }

        public override void WriteToGump(Gump g, ref int y)
        {
            m_Objective.WriteToGump(g, ref y);

            base.WriteToGump(g, ref y);

            g.AddHtmlLocalized(103, y, 120, 16, 3000087, 0x15F90, false, false); // Total
            g.AddLabel(223, y, 0x481, m_Slain.ToString());
            y += 16;

            g.AddHtmlLocalized(103, y, 120, 16, 1074782, 0x15F90, false, false); // Return to
            g.AddLabel(223, y, 0x481, QuesterNameAttribute.GetQuesterNameFor(Instance.QuesterType));
            y += 16;
        }

        public override DataType ExtraDataType { get { return DataType.KillObjective; } }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(m_Slain);
        }
    }

    #endregion
}

namespace Server.Engines.MLQuests.Rewards
{
    public abstract class BaseReward
    {
        private TextDefinition m_Name;

        public TextDefinition Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public BaseReward(TextDefinition name)
        {
            m_Name = name;
        }

        protected virtual int LabelHeight { get { return 16; } }

        public void WriteToGump(Gump g, int x, ref int y)
        {
            TextDefinition.AddHtmlText(g, x, y, 280, LabelHeight, m_Name, false, false, 0x15F90, 0xBDE784);
        }

        public abstract void AddRewardItems(PlayerMobile pm, List<Item> rewards);
    }

    public class ItemReward : BaseReward
    {
        public static readonly ItemReward SmallBagOfTrinkets = new ItemReward(1072268, typeof(SmallBagOfTrinkets)); // A small bag of trinkets.
        public static readonly ItemReward BagOfTrinkets = new ItemReward(1072341, typeof(BagOfTrinkets)); // A bag of trinkets.
        public static readonly ItemReward BagOfTreasure = new ItemReward(1072583, typeof(BagOfTreasure)); // A bag of treasure.
        public static readonly ItemReward LargeBagOfTreasure = new ItemReward(1072706, typeof(LargeBagOfTreasure)); // A large bag of treasure.
        public static readonly ItemReward Strongbox = new ItemReward(1072584, typeof(RewardStrongbox)); // A strongbox.

        public static readonly ItemReward TailorSatchel = new ItemReward(1074282, typeof(TailorSatchel)); // Craftsman's Satchel
        public static readonly ItemReward BlacksmithSatchel = new ItemReward(1074282, typeof(BlacksmithSatchel)); // Craftsman's Satchel
        public static readonly ItemReward FletchingSatchel = new ItemReward(1074282, typeof(FletchingSatchel)); // Craftsman's Satchel
        public static readonly ItemReward CarpentrySatchel = new ItemReward(1074282, typeof(CarpentrySatchel)); // Craftsman's Satchel
        public static readonly ItemReward TinkerSatchel = new ItemReward(1074282, typeof(TinkerSatchel)); // Craftsman's Satchel

        private Type m_Type;
        private int m_Amount;

        public ItemReward()
            : this(null, null)
        {
        }

        public ItemReward(TextDefinition name, Type type)
            : this(name, type, 1)
        {
        }

        public ItemReward(TextDefinition name, Type type, int amount)
            : base(name)
        {
            m_Type = type;
            m_Amount = amount;
        }

        public virtual Item CreateItem()
        {
            Item spawnedItem = null;

            try
            {
                spawnedItem = Activator.CreateInstance(m_Type) as Item;
            }
            catch (Exception e)
            {
                if (MLQuestSystem.Debug)
                    Console.WriteLine("WARNING: ItemReward.CreateItem failed for {0}: {1}", m_Type, e);
            }

            return spawnedItem;
        }

        public override void AddRewardItems(PlayerMobile pm, List<Item> rewards)
        {
            Item reward = CreateItem();

            if (reward == null)
                return;

            if (reward.Stackable)
            {
                if (m_Amount > 1)
                    reward.Amount = m_Amount;

                rewards.Add(reward);
            }
            else
            {
                for (int i = 0; i < m_Amount; ++i)
                {
                    rewards.Add(reward);

                    if (i < m_Amount - 1)
                    {
                        reward = CreateItem();

                        if (reward == null)
                            return;
                    }
                }
            }
        }
    }

    public class DummyReward : BaseReward
    {
        public DummyReward(TextDefinition name)
            : base(name)
        {
        }

        protected override int LabelHeight { get { return 180; } }

        public override void AddRewardItems(PlayerMobile pm, List<Item> rewards)
        {
        }
    }
}