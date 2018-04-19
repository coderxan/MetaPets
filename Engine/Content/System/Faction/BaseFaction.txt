using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;

using Server;
using Server.Accounting;
using Server.Commands;
using Server.Commands.Generic;
using Server.Engines.Craft;
using Server.Factions;
using Server.Factions.AI;
using Server.Guilds;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Prompts;
using Server.Regions;
using Server.Spells;
using Server.Spells.First;
using Server.Spells.Second;
using Server.Spells.Third;
using Server.Spells.Fourth;
using Server.Spells.Fifth;
using Server.Spells.Sixth;
using Server.Spells.Seventh;
using Server.Targeting;

namespace Server.Ethics
{
    public abstract class Ethic
    {
        public static readonly bool Enabled = false;

        public static Ethic Find(Item item)
        {
            if ((item.SavedFlags & 0x100) != 0)
            {
                if (item.Hue == Hero.Definition.PrimaryHue)
                    return Hero;

                item.SavedFlags &= ~0x100;
            }

            if ((item.SavedFlags & 0x200) != 0)
            {
                if (item.Hue == Evil.Definition.PrimaryHue)
                    return Evil;

                item.SavedFlags &= ~0x200;
            }

            return null;
        }

        public static bool CheckTrade(Mobile from, Mobile to, Mobile newOwner, Item item)
        {
            Ethic itemEthic = Find(item);

            if (itemEthic == null || Find(newOwner) == itemEthic)
                return true;

            if (itemEthic == Hero)
                (from == newOwner ? to : from).SendMessage("Only heros may receive this item.");
            else if (itemEthic == Evil)
                (from == newOwner ? to : from).SendMessage("Only the evil may receive this item.");

            return false;
        }

        public static bool CheckEquip(Mobile from, Item item)
        {
            Ethic itemEthic = Find(item);

            if (itemEthic == null || Find(from) == itemEthic)
                return true;

            if (itemEthic == Hero)
                from.SendMessage("Only heros may wear this item.");
            else if (itemEthic == Evil)
                from.SendMessage("Only the evil may wear this item.");

            return false;
        }

        public static bool IsImbued(Item item)
        {
            return IsImbued(item, false);
        }

        public static bool IsImbued(Item item, bool recurse)
        {
            if (Find(item) != null)
                return true;

            if (recurse)
            {
                foreach (Item child in item.Items)
                {
                    if (IsImbued(child, true))
                        return true;
                }
            }

            return false;
        }

        public static void Initialize()
        {
            if (Enabled)
                EventSink.Speech += new SpeechEventHandler(EventSink_Speech);
        }

        public static void EventSink_Speech(SpeechEventArgs e)
        {
            if (e.Blocked || e.Handled)
                return;

            Player pl = Player.Find(e.Mobile);

            if (pl == null)
            {
                for (int i = 0; i < Ethics.Length; ++i)
                {
                    Ethic ethic = Ethics[i];

                    if (!ethic.IsEligible(e.Mobile))
                        continue;

                    if (!Insensitive.Equals(ethic.Definition.JoinPhrase.String, e.Speech))
                        continue;

                    bool isNearAnkh = false;

                    foreach (Item item in e.Mobile.GetItemsInRange(2))
                    {
                        if (item is Items.AnkhNorth || item is Items.AnkhWest)
                        {
                            isNearAnkh = true;
                            break;
                        }
                    }

                    if (!isNearAnkh)
                        continue;

                    pl = new Player(ethic, e.Mobile);

                    pl.Attach();

                    e.Mobile.FixedEffect(0x373A, 10, 30);
                    e.Mobile.PlaySound(0x209);

                    e.Handled = true;
                    break;
                }
            }
            else
            {
                if (e.Mobile is PlayerMobile && (e.Mobile as PlayerMobile).DuelContext != null)
                    return;

                Ethic ethic = pl.Ethic;

                for (int i = 0; i < ethic.Definition.Powers.Length; ++i)
                {
                    Power power = ethic.Definition.Powers[i];

                    if (!Insensitive.Equals(power.Definition.Phrase.String, e.Speech))
                        continue;

                    if (!power.CheckInvoke(pl))
                        continue;

                    power.BeginInvoke(pl);
                    e.Handled = true;

                    break;
                }
            }
        }

        protected EthicDefinition m_Definition;

        protected PlayerCollection m_Players;

        public EthicDefinition Definition
        {
            get { return m_Definition; }
        }

        public PlayerCollection Players
        {
            get { return m_Players; }
        }

        public static Ethic Find(Mobile mob)
        {
            return Find(mob, false, false);
        }

        public static Ethic Find(Mobile mob, bool inherit)
        {
            return Find(mob, inherit, false);
        }

        public static Ethic Find(Mobile mob, bool inherit, bool allegiance)
        {
            Player pl = Player.Find(mob);

            if (pl != null)
                return pl.Ethic;

            if (inherit && mob is BaseCreature)
            {
                BaseCreature bc = (BaseCreature)mob;

                if (bc.Controlled)
                    return Find(bc.ControlMaster, false);
                else if (bc.Summoned)
                    return Find(bc.SummonMaster, false);
                else if (allegiance)
                    return bc.EthicAllegiance;
            }

            return null;
        }

        public Ethic()
        {
            m_Players = new PlayerCollection();
        }

        public abstract bool IsEligible(Mobile mob);

        public virtual void Deserialize(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            switch (version)
            {
                case 0:
                    {
                        int playerCount = reader.ReadEncodedInt();

                        for (int i = 0; i < playerCount; ++i)
                        {
                            Player pl = new Player(this, reader);

                            if (pl.Mobile != null)
                                Timer.DelayCall(TimeSpan.Zero, new TimerCallback(pl.CheckAttach));
                        }

                        break;
                    }
            }
        }

        public virtual void Serialize(GenericWriter writer)
        {
            writer.WriteEncodedInt(0); // version

            writer.WriteEncodedInt(m_Players.Count);

            for (int i = 0; i < m_Players.Count; ++i)
                m_Players[i].Serialize(writer);
        }

        public static readonly Ethic Hero = new Hero.HeroEthic();
        public static readonly Ethic Evil = new Evil.EvilEthic();

        public static readonly Ethic[] Ethics = new Ethic[]
			{
				Hero,
				Evil
			};
    }

    public class EthicDefinition
    {
        private int m_PrimaryHue;

        private TextDefinition m_Title;
        private TextDefinition m_Adjunct;

        private TextDefinition m_JoinPhrase;

        private Power[] m_Powers;

        public int PrimaryHue { get { return m_PrimaryHue; } }

        public TextDefinition Title { get { return m_Title; } }
        public TextDefinition Adjunct { get { return m_Adjunct; } }

        public TextDefinition JoinPhrase { get { return m_JoinPhrase; } }

        public Power[] Powers { get { return m_Powers; } }

        public EthicDefinition(int primaryHue, TextDefinition title, TextDefinition adjunct, TextDefinition joinPhrase, Power[] powers)
        {
            m_PrimaryHue = primaryHue;

            m_Title = title;
            m_Adjunct = adjunct;

            m_JoinPhrase = joinPhrase;

            m_Powers = powers;
        }
    }

    public class EthicsPersistance : Item
    {
        private static EthicsPersistance m_Instance;

        public static EthicsPersistance Instance { get { return m_Instance; } }

        public override string DefaultName
        {
            get { return "Ethics Persistance - Internal"; }
        }

        [Constructable]
        public EthicsPersistance()
            : base(1)
        {
            Movable = false;

            if (m_Instance == null || m_Instance.Deleted)
                m_Instance = this;
            else
                base.Delete();
        }

        public EthicsPersistance(Serial serial)
            : base(serial)
        {
            m_Instance = this;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            for (int i = 0; i < Ethics.Ethic.Ethics.Length; ++i)
                Ethics.Ethic.Ethics[i].Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        for (int i = 0; i < Ethics.Ethic.Ethics.Length; ++i)
                            Ethics.Ethic.Ethics[i].Deserialize(reader);

                        break;
                    }
            }
        }

        public override void Delete()
        {
        }
    }

    public class PlayerCollection : System.Collections.ObjectModel.Collection<Player>
    {
    }

    [PropertyObject]
    public class Player
    {
        public static Player Find(Mobile mob)
        {
            return Find(mob, false);
        }

        public static Player Find(Mobile mob, bool inherit)
        {
            PlayerMobile pm = mob as PlayerMobile;

            if (pm == null)
            {
                if (inherit && mob is BaseCreature)
                {
                    BaseCreature bc = mob as BaseCreature;

                    if (bc != null && bc.Controlled)
                        pm = bc.ControlMaster as PlayerMobile;
                    else if (bc != null && bc.Summoned)
                        pm = bc.SummonMaster as PlayerMobile;
                }

                if (pm == null)
                    return null;
            }

            Player pl = pm.EthicPlayer;

            if (pl != null && !pl.Ethic.IsEligible(pl.Mobile))
                pm.EthicPlayer = pl = null;

            return pl;
        }

        private Ethic m_Ethic;
        private Mobile m_Mobile;

        private int m_Power;
        private int m_History;

        private Mobile m_Steed;
        private Mobile m_Familiar;

        private DateTime m_Shield;

        public Ethic Ethic { get { return m_Ethic; } }
        public Mobile Mobile { get { return m_Mobile; } }

        [CommandProperty(AccessLevel.GameMaster, AccessLevel.Administrator)]
        public int Power { get { return m_Power; } set { m_Power = value; } }

        [CommandProperty(AccessLevel.GameMaster, AccessLevel.Administrator)]
        public int History { get { return m_History; } set { m_History = value; } }

        [CommandProperty(AccessLevel.GameMaster, AccessLevel.Administrator)]
        public Mobile Steed { get { return m_Steed; } set { m_Steed = value; } }

        [CommandProperty(AccessLevel.GameMaster, AccessLevel.Administrator)]
        public Mobile Familiar { get { return m_Familiar; } set { m_Familiar = value; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsShielded
        {
            get
            {
                if (m_Shield == DateTime.MinValue)
                    return false;

                if (DateTime.UtcNow < (m_Shield + TimeSpan.FromHours(1.0)))
                    return true;

                FinishShield();
                return false;
            }
        }

        public void BeginShield()
        {
            m_Shield = DateTime.UtcNow;
        }

        public void FinishShield()
        {
            m_Shield = DateTime.MinValue;
        }

        public Player(Ethic ethic, Mobile mobile)
        {
            m_Ethic = ethic;
            m_Mobile = mobile;

            m_Power = 5;
            m_History = 5;
        }

        public void CheckAttach()
        {
            if (m_Ethic.IsEligible(m_Mobile))
                Attach();
        }

        public void Attach()
        {
            if (m_Mobile is PlayerMobile)
                (m_Mobile as PlayerMobile).EthicPlayer = this;

            m_Ethic.Players.Add(this);
        }

        public void Detach()
        {
            if (m_Mobile is PlayerMobile)
                (m_Mobile as PlayerMobile).EthicPlayer = null;

            m_Ethic.Players.Remove(this);
        }

        public Player(Ethic ethic, GenericReader reader)
        {
            m_Ethic = ethic;

            int version = reader.ReadEncodedInt();

            switch (version)
            {
                case 0:
                    {
                        m_Mobile = reader.ReadMobile();

                        m_Power = reader.ReadEncodedInt();
                        m_History = reader.ReadEncodedInt();

                        m_Steed = reader.ReadMobile();
                        m_Familiar = reader.ReadMobile();

                        m_Shield = reader.ReadDeltaTime();

                        break;
                    }
            }
        }

        public void Serialize(GenericWriter writer)
        {
            writer.WriteEncodedInt(0); // version

            writer.Write(m_Mobile);

            writer.WriteEncodedInt(m_Power);
            writer.WriteEncodedInt(m_History);

            writer.Write(m_Steed);
            writer.Write(m_Familiar);

            writer.WriteDeltaTime(m_Shield);
        }
    }

    public abstract class Power
    {
        protected PowerDefinition m_Definition;

        public PowerDefinition Definition { get { return m_Definition; } }

        public virtual bool CheckInvoke(Player from)
        {
            if (!from.Mobile.CheckAlive())
                return false;

            if (from.Power < m_Definition.Power)
            {
                from.Mobile.LocalOverheadMessage(Server.Network.MessageType.Regular, 0x3B2, false, "You lack the power to invoke this ability.");
                return false;
            }

            return true;
        }

        public abstract void BeginInvoke(Player from);

        public virtual void FinishInvoke(Player from)
        {
            from.Power -= m_Definition.Power;
        }
    }

    public class PowerDefinition
    {
        private int m_Power;

        private TextDefinition m_Name;
        private TextDefinition m_Phrase;
        private TextDefinition m_Description;

        public int Power { get { return m_Power; } }

        public TextDefinition Name { get { return m_Name; } }
        public TextDefinition Phrase { get { return m_Phrase; } }
        public TextDefinition Description { get { return m_Description; } }

        public PowerDefinition(int power, TextDefinition name, TextDefinition phrase, TextDefinition description)
        {
            m_Power = power;

            m_Name = name;
            m_Phrase = phrase;
            m_Description = description;
        }
    }
}

namespace Server.Ethics.Evil
{
    public sealed class EvilEthic : Ethic
    {
        public EvilEthic()
        {
            m_Definition = new EthicDefinition(
                    0x455,
                    "Evil", "(Evil)",
                    "I am evil incarnate",
                    new Power[]
					{
						new UnholySense(),
						new UnholyItem(),
						new SummonFamiliar(),
						new VileBlade(),
						new Blight(),
						new UnholyShield(),
						new UnholySteed(),
						new UnholyWord()
					}
                );
        }

        public override bool IsEligible(Mobile mob)
        {
            Faction fac = Faction.Find(mob);

            return (fac is Minax || fac is Shadowlords);
        }
    }
}

namespace Server.Ethics.Hero
{
    public sealed class HeroEthic : Ethic
    {
        public HeroEthic()
        {
            m_Definition = new EthicDefinition(
                    0x482,
                    "Hero", "(Hero)",
                    "I will defend the virtues",
                    new Power[]
					{
						new HolySense(),
						new HolyItem(),
						new SummonFamiliar(),
						new HolyBlade(),
						new Bless(),
						new HolyShield(),
						new HolySteed(),
						new HolyWord()
					}
                );
        }

        public override bool IsEligible(Mobile mob)
        {
            if (mob.Kills >= 5)
                return false;

            Faction fac = Faction.Find(mob);

            return (fac is TrueBritannians || fac is CouncilOfMages);
        }
    }
}

namespace Server.Factions
{
    public class FactionDefinition
    {
        private int m_Sort;

        private int m_HuePrimary;
        private int m_HueSecondary;
        private int m_HueJoin;
        private int m_HueBroadcast;

        private int m_WarHorseBody;
        private int m_WarHorseItem;

        private string m_FriendlyName;
        private string m_Keyword;
        private string m_Abbreviation;

        private TextDefinition m_Name;
        private TextDefinition m_PropName;
        private TextDefinition m_Header;
        private TextDefinition m_About;
        private TextDefinition m_CityControl;
        private TextDefinition m_SigilControl;
        private TextDefinition m_SignupName;
        private TextDefinition m_FactionStoneName;
        private TextDefinition m_OwnerLabel;

        private TextDefinition m_GuardIgnore, m_GuardWarn, m_GuardAttack;

        private StrongholdDefinition m_Stronghold;

        private RankDefinition[] m_Ranks;
        private GuardDefinition[] m_Guards;

        public int Sort { get { return m_Sort; } }

        public int HuePrimary { get { return m_HuePrimary; } }
        public int HueSecondary { get { return m_HueSecondary; } }
        public int HueJoin { get { return m_HueJoin; } }
        public int HueBroadcast { get { return m_HueBroadcast; } }

        public int WarHorseBody { get { return m_WarHorseBody; } }
        public int WarHorseItem { get { return m_WarHorseItem; } }

        public string FriendlyName { get { return m_FriendlyName; } }
        public string Keyword { get { return m_Keyword; } }
        public string Abbreviation { get { return m_Abbreviation; } }

        public TextDefinition Name { get { return m_Name; } }
        public TextDefinition PropName { get { return m_PropName; } }
        public TextDefinition Header { get { return m_Header; } }
        public TextDefinition About { get { return m_About; } }
        public TextDefinition CityControl { get { return m_CityControl; } }
        public TextDefinition SigilControl { get { return m_SigilControl; } }
        public TextDefinition SignupName { get { return m_SignupName; } }
        public TextDefinition FactionStoneName { get { return m_FactionStoneName; } }
        public TextDefinition OwnerLabel { get { return m_OwnerLabel; } }

        public TextDefinition GuardIgnore { get { return m_GuardIgnore; } }
        public TextDefinition GuardWarn { get { return m_GuardWarn; } }
        public TextDefinition GuardAttack { get { return m_GuardAttack; } }

        public StrongholdDefinition Stronghold { get { return m_Stronghold; } }

        public RankDefinition[] Ranks { get { return m_Ranks; } }
        public GuardDefinition[] Guards { get { return m_Guards; } }

        public FactionDefinition(int sort, int huePrimary, int hueSecondary, int hueJoin, int hueBroadcast, int warHorseBody, int warHorseItem, string friendlyName, string keyword, string abbreviation, TextDefinition name, TextDefinition propName, TextDefinition header, TextDefinition about, TextDefinition cityControl, TextDefinition sigilControl, TextDefinition signupName, TextDefinition factionStoneName, TextDefinition ownerLabel, TextDefinition guardIgnore, TextDefinition guardWarn, TextDefinition guardAttack, StrongholdDefinition stronghold, RankDefinition[] ranks, GuardDefinition[] guards)
        {
            m_Sort = sort;
            m_HuePrimary = huePrimary;
            m_HueSecondary = hueSecondary;
            m_HueJoin = hueJoin;
            m_HueBroadcast = hueBroadcast;
            m_WarHorseBody = warHorseBody;
            m_WarHorseItem = warHorseItem;
            m_FriendlyName = friendlyName;
            m_Keyword = keyword;
            m_Abbreviation = abbreviation;
            m_Name = name;
            m_PropName = propName;
            m_Header = header;
            m_About = about;
            m_CityControl = cityControl;
            m_SigilControl = sigilControl;
            m_SignupName = signupName;
            m_FactionStoneName = factionStoneName;
            m_OwnerLabel = ownerLabel;
            m_GuardIgnore = guardIgnore;
            m_GuardWarn = guardWarn;
            m_GuardAttack = guardAttack;
            m_Stronghold = stronghold;
            m_Ranks = ranks;
            m_Guards = guards;
        }
    }

    [CustomEnum(new string[] { "Minax", "Council of Mages", "True Britannians", "Shadowlords" })]
    public abstract class Faction : IComparable
    {
        public int ZeroRankOffset;

        private FactionDefinition m_Definition;
        private FactionState m_State;
        private StrongholdRegion m_StrongholdRegion;

        public StrongholdRegion StrongholdRegion
        {
            get { return m_StrongholdRegion; }
            set { m_StrongholdRegion = value; }
        }

        public FactionDefinition Definition
        {
            get { return m_Definition; }
            set
            {
                m_Definition = value;
                m_StrongholdRegion = new StrongholdRegion(this);
            }
        }

        public FactionState State
        {
            get { return m_State; }
            set { m_State = value; }
        }

        public Election Election
        {
            get { return m_State.Election; }
            set { m_State.Election = value; }
        }

        public Mobile Commander
        {
            get { return m_State.Commander; }
            set { m_State.Commander = value; }
        }

        public int Tithe
        {
            get { return m_State.Tithe; }
            set { m_State.Tithe = value; }
        }

        public int Silver
        {
            get { return m_State.Silver; }
            set { m_State.Silver = value; }
        }

        public List<PlayerState> Members
        {
            get { return m_State.Members; }
            set { m_State.Members = value; }
        }

        public static readonly TimeSpan LeavePeriod = TimeSpan.FromDays(3.0);

        public bool FactionMessageReady
        {
            get { return m_State.FactionMessageReady; }
        }

        public void Broadcast(string text)
        {
            Broadcast(0x3B2, text);
        }

        public void Broadcast(int hue, string text)
        {
            List<PlayerState> members = Members;

            for (int i = 0; i < members.Count; ++i)
                members[i].Mobile.SendMessage(hue, text);
        }

        public void Broadcast(int number)
        {
            List<PlayerState> members = Members;

            for (int i = 0; i < members.Count; ++i)
                members[i].Mobile.SendLocalizedMessage(number);
        }

        public void Broadcast(string format, params object[] args)
        {
            Broadcast(String.Format(format, args));
        }

        public void Broadcast(int hue, string format, params object[] args)
        {
            Broadcast(hue, String.Format(format, args));
        }

        public void BeginBroadcast(Mobile from)
        {
            from.SendLocalizedMessage(1010265); // Enter Faction Message
            from.Prompt = new BroadcastPrompt(this);
        }

        public void EndBroadcast(Mobile from, string text)
        {
            if (from.AccessLevel == AccessLevel.Player)
                m_State.RegisterBroadcast();

            Broadcast(Definition.HueBroadcast, "{0} [Commander] {1} : {2}", from.Name, Definition.FriendlyName, text);
        }

        private class BroadcastPrompt : Prompt
        {
            private Faction m_Faction;

            public BroadcastPrompt(Faction faction)
            {
                m_Faction = faction;
            }

            public override void OnResponse(Mobile from, string text)
            {
                m_Faction.EndBroadcast(from, text);
            }
        }

        public static void HandleAtrophy()
        {
            foreach (Faction f in Factions)
            {
                if (!f.State.IsAtrophyReady)
                    return;
            }

            List<PlayerState> activePlayers = new List<PlayerState>();

            foreach (Faction f in Factions)
            {
                foreach (PlayerState ps in f.Members)
                {
                    if (ps.KillPoints > 0 && ps.IsActive)
                        activePlayers.Add(ps);
                }
            }

            int distrib = 0;

            foreach (Faction f in Factions)
                distrib += f.State.CheckAtrophy();

            if (activePlayers.Count == 0)
                return;

            for (int i = 0; i < distrib; ++i)
                activePlayers[Utility.Random(activePlayers.Count)].KillPoints++;
        }

        public static void DistributePoints(int distrib)
        {
            List<PlayerState> activePlayers = new List<PlayerState>();

            foreach (Faction f in Factions)
            {
                foreach (PlayerState ps in f.Members)
                {
                    if (ps.KillPoints > 0 && ps.IsActive)
                    {
                        activePlayers.Add(ps);
                    }
                }
            }

            if (activePlayers.Count > 0)
            {
                for (int i = 0; i < distrib; ++i)
                {
                    activePlayers[Utility.Random(activePlayers.Count)].KillPoints++;
                }
            }
        }

        public void BeginHonorLeadership(Mobile from)
        {
            from.SendLocalizedMessage(502090); // Click on the player whom you wish to honor.
            from.BeginTarget(12, false, TargetFlags.None, new TargetCallback(HonorLeadership_OnTarget));
        }

        public void HonorLeadership_OnTarget(Mobile from, object obj)
        {
            if (obj is Mobile)
            {
                Mobile recv = (Mobile)obj;

                PlayerState giveState = PlayerState.Find(from);
                PlayerState recvState = PlayerState.Find(recv);

                if (giveState == null)
                    return;

                if (recvState == null || recvState.Faction != giveState.Faction)
                {
                    from.SendLocalizedMessage(1042497); // Only faction mates can be honored this way.
                }
                else if (giveState.KillPoints < 5)
                {
                    from.SendLocalizedMessage(1042499); // You must have at least five kill points to honor them.
                }
                else
                {
                    recvState.LastHonorTime = DateTime.UtcNow;
                    giveState.KillPoints -= 5;
                    recvState.KillPoints += 4;

                    // TODO: Confirm no message sent to giver
                    recv.SendLocalizedMessage(1042500); // You have been honored with four kill points.
                }
            }
            else
            {
                from.SendLocalizedMessage(1042496); // You may only honor another player.
            }
        }

        public virtual void AddMember(Mobile mob)
        {
            Members.Insert(ZeroRankOffset, new PlayerState(mob, this, Members));

            mob.AddToBackpack(FactionItem.Imbue(new Robe(), this, false, Definition.HuePrimary));
            mob.SendLocalizedMessage(1010374); // You have been granted a robe which signifies your faction

            mob.InvalidateProperties();
            mob.Delta(MobileDelta.Noto);

            mob.FixedEffect(0x373A, 10, 30);
            mob.PlaySound(0x209);
        }

        public static bool IsNearType(Mobile mob, Type type, int range)
        {
            bool mobs = type.IsSubclassOf(typeof(Mobile));
            bool items = type.IsSubclassOf(typeof(Item));

            IPooledEnumerable eable;

            if (mobs)
                eable = mob.GetMobilesInRange(range);
            else if (items)
                eable = mob.GetItemsInRange(range);
            else
                return false;

            foreach (object obj in eable)
            {
                if (type.IsAssignableFrom(obj.GetType()))
                {
                    eable.Free();
                    return true;
                }
            }

            eable.Free();
            return false;
        }

        public static bool IsNearType(Mobile mob, Type[] types, int range)
        {
            IPooledEnumerable eable = mob.GetObjectsInRange(range);

            foreach (object obj in eable)
            {
                Type objType = obj.GetType();

                for (int i = 0; i < types.Length; i++)
                {
                    if (types[i].IsAssignableFrom(objType))
                    {
                        eable.Free();
                        return true;
                    }
                }
            }

            eable.Free();
            return false;
        }

        public void RemovePlayerState(PlayerState pl)
        {
            if (pl == null || !Members.Contains(pl))
                return;

            int killPoints = pl.KillPoints;

            if (pl.RankIndex != -1)
            {
                while ((pl.RankIndex + 1) < ZeroRankOffset)
                {
                    PlayerState pNext = Members[pl.RankIndex + 1] as PlayerState;
                    Members[pl.RankIndex + 1] = pl;
                    Members[pl.RankIndex] = pNext;
                    pl.RankIndex++;
                    pNext.RankIndex--;
                }

                ZeroRankOffset--;
            }

            Members.Remove(pl);

            PlayerMobile pm = (PlayerMobile)pl.Mobile;
            if (pm == null)
                return;

            Mobile mob = pl.Mobile;
            if (pm.FactionPlayerState == pl)
            {
                pm.FactionPlayerState = null;

                mob.InvalidateProperties();
                mob.Delta(MobileDelta.Noto);

                if (Election.IsCandidate(mob))
                    Election.RemoveCandidate(mob);

                if (pl.Finance != null)
                    pl.Finance.Finance = null;

                if (pl.Sheriff != null)
                    pl.Sheriff.Sheriff = null;

                Election.RemoveVoter(mob);

                if (Commander == mob)
                    Commander = null;

                pm.ValidateEquipment();
            }

            if (killPoints > 0)
                DistributePoints(killPoints);
        }

        public void RemoveMember(Mobile mob)
        {
            PlayerState pl = PlayerState.Find(mob);

            if (pl == null || !Members.Contains(pl))
                return;

            int killPoints = pl.KillPoints;

            if (mob.Backpack != null)
            {
                //Ordinarily, through normal faction removal, this will never find any sigils.
                //Only with a leave delay less than the ReturnPeriod or a Faction Kick/Ban, will this ever do anything
                Item[] sigils = mob.Backpack.FindItemsByType(typeof(Sigil));

                for (int i = 0; i < sigils.Length; ++i)
                    ((Sigil)sigils[i]).ReturnHome();
            }

            if (pl.RankIndex != -1)
            {
                while ((pl.RankIndex + 1) < ZeroRankOffset)
                {
                    PlayerState pNext = Members[pl.RankIndex + 1];
                    Members[pl.RankIndex + 1] = pl;
                    Members[pl.RankIndex] = pNext;
                    pl.RankIndex++;
                    pNext.RankIndex--;
                }

                ZeroRankOffset--;
            }

            Members.Remove(pl);

            if (mob is PlayerMobile)
                ((PlayerMobile)mob).FactionPlayerState = null;

            mob.InvalidateProperties();
            mob.Delta(MobileDelta.Noto);

            if (Election.IsCandidate(mob))
                Election.RemoveCandidate(mob);

            Election.RemoveVoter(mob);

            if (pl.Finance != null)
                pl.Finance.Finance = null;

            if (pl.Sheriff != null)
                pl.Sheriff.Sheriff = null;

            if (Commander == mob)
                Commander = null;

            if (mob is PlayerMobile)
                ((PlayerMobile)mob).ValidateEquipment();

            if (killPoints > 0)
                DistributePoints(killPoints);
        }

        public void JoinGuilded(PlayerMobile mob, Guild guild)
        {
            if (mob.Young)
            {
                guild.RemoveMember(mob);
                mob.SendLocalizedMessage(1042283); // You have been kicked out of your guild!  Young players may not remain in a guild which is allied with a faction.
            }
            else if (AlreadyHasCharInFaction(mob))
            {
                guild.RemoveMember(mob);
                mob.SendLocalizedMessage(1005281); // You have been kicked out of your guild due to factional overlap
            }
            else if (IsFactionBanned(mob))
            {
                guild.RemoveMember(mob);
                mob.SendLocalizedMessage(1005052); // You are currently banned from the faction system
            }
            else
            {
                AddMember(mob);
                mob.SendLocalizedMessage(1042756, true, " " + m_Definition.FriendlyName); // You are now joining a faction:
            }
        }

        public void JoinAlone(Mobile mob)
        {
            AddMember(mob);
            mob.SendLocalizedMessage(1005058); // You have joined the faction
        }

        private bool AlreadyHasCharInFaction(Mobile mob)
        {
            Account acct = mob.Account as Account;

            if (acct != null)
            {
                for (int i = 0; i < acct.Length; ++i)
                {
                    Mobile c = acct[i];

                    if (Find(c) != null)
                        return true;
                }
            }

            return false;
        }

        public static bool IsFactionBanned(Mobile mob)
        {
            Account acct = mob.Account as Account;

            if (acct == null)
                return false;

            return (acct.GetTag("FactionBanned") != null);
        }

        public void OnJoinAccepted(Mobile mob)
        {
            PlayerMobile pm = mob as PlayerMobile;

            if (pm == null)
                return; // sanity

            PlayerState pl = PlayerState.Find(pm);

            if (pm.Young)
                pm.SendLocalizedMessage(1010104); // You cannot join a faction as a young player
            else if (pl != null && pl.IsLeaving)
                pm.SendLocalizedMessage(1005051); // You cannot use the faction stone until you have finished quitting your current faction
            else if (AlreadyHasCharInFaction(pm))
                pm.SendLocalizedMessage(1005059); // You cannot join a faction because you already declared your allegiance with another character
            else if (IsFactionBanned(mob))
                pm.SendLocalizedMessage(1005052); // You are currently banned from the faction system
            else if (pm.Guild != null)
            {
                Guild guild = pm.Guild as Guild;

                if (guild.Leader != pm)
                    pm.SendLocalizedMessage(1005057); // You cannot join a faction because you are in a guild and not the guildmaster
                else if (guild.Type != GuildType.Regular)
                    pm.SendLocalizedMessage(1042161); // You cannot join a faction because your guild is an Order or Chaos type.
                else if (!Guild.NewGuildSystem && guild.Enemies != null && guild.Enemies.Count > 0)	//CAN join w/wars in new system
                    pm.SendLocalizedMessage(1005056); // You cannot join a faction with active Wars
                else if (Guild.NewGuildSystem && guild.Alliance != null)
                    pm.SendLocalizedMessage(1080454); // Your guild cannot join a faction while in alliance with non-factioned guilds.
                else if (!CanHandleInflux(guild.Members.Count))
                    pm.SendLocalizedMessage(1018031); // In the interest of faction stability, this faction declines to accept new members for now.
                else
                {
                    List<Mobile> members = new List<Mobile>(guild.Members);

                    for (int i = 0; i < members.Count; ++i)
                    {
                        PlayerMobile member = members[i] as PlayerMobile;

                        if (member == null)
                            continue;

                        JoinGuilded(member, guild);
                    }
                }
            }
            else if (!CanHandleInflux(1))
            {
                pm.SendLocalizedMessage(1018031); // In the interest of faction stability, this faction declines to accept new members for now.
            }
            else
            {
                JoinAlone(mob);
            }
        }

        public bool IsCommander(Mobile mob)
        {
            if (mob == null)
                return false;

            return (mob.AccessLevel >= AccessLevel.GameMaster || mob == Commander);
        }

        public Faction()
        {
            m_State = new FactionState(this);
        }

        public override string ToString()
        {
            return m_Definition.FriendlyName;
        }

        public int CompareTo(object obj)
        {
            return m_Definition.Sort - ((Faction)obj).m_Definition.Sort;
        }

        public static bool CheckLeaveTimer(Mobile mob)
        {
            PlayerState pl = PlayerState.Find(mob);

            if (pl == null || !pl.IsLeaving)
                return false;

            if ((pl.Leaving + LeavePeriod) >= DateTime.UtcNow)
                return false;

            mob.SendLocalizedMessage(1005163); // You have now quit your faction

            pl.Faction.RemoveMember(mob);

            return true;
        }

        public static void Initialize()
        {
            EventSink.Login += new LoginEventHandler(EventSink_Login);
            EventSink.Logout += new LogoutEventHandler(EventSink_Logout);

            Timer.DelayCall(TimeSpan.FromMinutes(1.0), TimeSpan.FromMinutes(10.0), new TimerCallback(HandleAtrophy));

            Timer.DelayCall(TimeSpan.FromSeconds(30.0), TimeSpan.FromSeconds(30.0), new TimerCallback(ProcessTick));

            CommandSystem.Register("FactionElection", AccessLevel.GameMaster, new CommandEventHandler(FactionElection_OnCommand));
            CommandSystem.Register("FactionCommander", AccessLevel.Administrator, new CommandEventHandler(FactionCommander_OnCommand));
            CommandSystem.Register("FactionItemReset", AccessLevel.Administrator, new CommandEventHandler(FactionItemReset_OnCommand));
            CommandSystem.Register("FactionReset", AccessLevel.Administrator, new CommandEventHandler(FactionReset_OnCommand));
            CommandSystem.Register("FactionTownReset", AccessLevel.Administrator, new CommandEventHandler(FactionTownReset_OnCommand));
        }

        public static void FactionTownReset_OnCommand(CommandEventArgs e)
        {
            List<BaseMonolith> monoliths = BaseMonolith.Monoliths;

            for (int i = 0; i < monoliths.Count; ++i)
                monoliths[i].Sigil = null;

            List<Town> towns = Town.Towns;

            for (int i = 0; i < towns.Count; ++i)
            {
                towns[i].Silver = 0;
                towns[i].Sheriff = null;
                towns[i].Finance = null;
                towns[i].Tax = 0;
                towns[i].Owner = null;
            }

            List<Sigil> sigils = Sigil.Sigils;

            for (int i = 0; i < sigils.Count; ++i)
            {
                sigils[i].Corrupted = null;
                sigils[i].Corrupting = null;
                sigils[i].LastStolen = DateTime.MinValue;
                sigils[i].GraceStart = DateTime.MinValue;
                sigils[i].CorruptionStart = DateTime.MinValue;
                sigils[i].PurificationStart = DateTime.MinValue;
                sigils[i].LastMonolith = null;
                sigils[i].ReturnHome();
            }

            List<Faction> factions = Faction.Factions;

            for (int i = 0; i < factions.Count; ++i)
            {
                Faction f = factions[i];

                List<FactionItem> list = new List<FactionItem>(f.State.FactionItems);

                for (int j = 0; j < list.Count; ++j)
                {
                    FactionItem fi = list[j];

                    if (fi.Expiration == DateTime.MinValue)
                        fi.Item.Delete();
                    else
                        fi.Detach();
                }
            }
        }

        public static void FactionReset_OnCommand(CommandEventArgs e)
        {
            List<BaseMonolith> monoliths = BaseMonolith.Monoliths;

            for (int i = 0; i < monoliths.Count; ++i)
                monoliths[i].Sigil = null;

            List<Town> towns = Town.Towns;

            for (int i = 0; i < towns.Count; ++i)
            {
                towns[i].Silver = 0;
                towns[i].Sheriff = null;
                towns[i].Finance = null;
                towns[i].Tax = 0;
                towns[i].Owner = null;
            }

            List<Sigil> sigils = Sigil.Sigils;

            for (int i = 0; i < sigils.Count; ++i)
            {
                sigils[i].Corrupted = null;
                sigils[i].Corrupting = null;
                sigils[i].LastStolen = DateTime.MinValue;
                sigils[i].GraceStart = DateTime.MinValue;
                sigils[i].CorruptionStart = DateTime.MinValue;
                sigils[i].PurificationStart = DateTime.MinValue;
                sigils[i].LastMonolith = null;
                sigils[i].ReturnHome();
            }

            List<Faction> factions = Faction.Factions;

            for (int i = 0; i < factions.Count; ++i)
            {
                Faction f = factions[i];

                List<PlayerState> playerStateList = new List<PlayerState>(f.Members);

                for (int j = 0; j < playerStateList.Count; ++j)
                    f.RemoveMember(playerStateList[j].Mobile);

                List<FactionItem> factionItemList = new List<FactionItem>(f.State.FactionItems);

                for (int j = 0; j < factionItemList.Count; ++j)
                {
                    FactionItem fi = (FactionItem)factionItemList[j];

                    if (fi.Expiration == DateTime.MinValue)
                        fi.Item.Delete();
                    else
                        fi.Detach();
                }

                List<BaseFactionTrap> factionTrapList = new List<BaseFactionTrap>(f.Traps);

                for (int j = 0; j < factionTrapList.Count; ++j)
                    factionTrapList[j].Delete();
            }
        }

        public static void FactionItemReset_OnCommand(CommandEventArgs e)
        {
            ArrayList pots = new ArrayList();

            foreach (Item item in World.Items.Values)
            {
                if (item is IFactionItem && !(item is HoodedShroudOfShadows))
                    pots.Add(item);
            }

            int[] hues = new int[Factions.Count * 2];

            for (int i = 0; i < Factions.Count; ++i)
            {
                hues[0 + (i * 2)] = Factions[i].Definition.HuePrimary;
                hues[1 + (i * 2)] = Factions[i].Definition.HueSecondary;
            }

            int count = 0;

            for (int i = 0; i < pots.Count; ++i)
            {
                Item item = (Item)pots[i];
                IFactionItem fci = (IFactionItem)item;

                if (fci.FactionItemState != null || item.LootType != LootType.Blessed)
                    continue;

                bool isHued = false;

                for (int j = 0; j < hues.Length; ++j)
                {
                    if (item.Hue == hues[j])
                    {
                        isHued = true;
                        break;
                    }
                }

                if (isHued)
                {
                    fci.FactionItemState = null;
                    ++count;
                }
            }

            e.Mobile.SendMessage("{0} items reset", count);
        }

        public static void FactionCommander_OnCommand(CommandEventArgs e)
        {
            e.Mobile.SendMessage("Target a player to make them the faction commander.");
            e.Mobile.BeginTarget(-1, false, TargetFlags.None, new TargetCallback(FactionCommander_OnTarget));
        }

        public static void FactionCommander_OnTarget(Mobile from, object obj)
        {
            if (obj is PlayerMobile)
            {
                Mobile targ = (Mobile)obj;
                PlayerState pl = PlayerState.Find(targ);

                if (pl != null)
                {
                    pl.Faction.Commander = targ;
                    from.SendMessage("You have appointed them as the faction commander.");
                }
                else
                {
                    from.SendMessage("They are not in a faction.");
                }
            }
            else
            {
                from.SendMessage("That is not a player.");
            }
        }

        public static void FactionElection_OnCommand(CommandEventArgs e)
        {
            e.Mobile.SendMessage("Target a faction stone to open its election properties.");
            e.Mobile.BeginTarget(-1, false, TargetFlags.None, new TargetCallback(FactionElection_OnTarget));
        }

        public static void FactionElection_OnTarget(Mobile from, object obj)
        {
            if (obj is FactionStone)
            {
                Faction faction = ((FactionStone)obj).Faction;

                if (faction != null)
                    from.SendGump(new ElectionManagementGump(faction.Election));
                //from.SendGump( new Gumps.PropertiesGump( from, faction.Election ) );
                else
                    from.SendMessage("That stone has no faction assigned.");
            }
            else
            {
                from.SendMessage("That is not a faction stone.");
            }
        }

        public static void FactionKick_OnCommand(CommandEventArgs e)
        {
            e.Mobile.SendMessage("Target a player to remove them from their faction.");
            e.Mobile.BeginTarget(-1, false, TargetFlags.None, new TargetCallback(FactionKick_OnTarget));
        }

        public static void FactionKick_OnTarget(Mobile from, object obj)
        {
            if (obj is Mobile)
            {
                Mobile mob = (Mobile)obj;
                PlayerState pl = PlayerState.Find((Mobile)mob);

                if (pl != null)
                {
                    pl.Faction.RemoveMember(mob);

                    mob.SendMessage("You have been kicked from your faction.");
                    from.SendMessage("They have been kicked from their faction.");
                }
                else
                {
                    from.SendMessage("They are not in a faction.");
                }
            }
            else
            {
                from.SendMessage("That is not a player.");
            }
        }

        public static void ProcessTick()
        {
            List<Sigil> sigils = Sigil.Sigils;

            for (int i = 0; i < sigils.Count; ++i)
            {
                Sigil sigil = sigils[i];

                if (!sigil.IsBeingCorrupted && sigil.GraceStart != DateTime.MinValue && (sigil.GraceStart + Sigil.CorruptionGrace) < DateTime.UtcNow)
                {
                    if (sigil.LastMonolith is StrongholdMonolith && (sigil.Corrupted == null || sigil.LastMonolith.Faction != sigil.Corrupted))
                    {
                        sigil.Corrupting = sigil.LastMonolith.Faction;
                        sigil.CorruptionStart = DateTime.UtcNow;
                    }
                    else
                    {
                        sigil.Corrupting = null;
                        sigil.CorruptionStart = DateTime.MinValue;
                    }

                    sigil.GraceStart = DateTime.MinValue;
                }

                if (sigil.LastMonolith == null || sigil.LastMonolith.Sigil == null)
                {
                    if ((sigil.LastStolen + Sigil.ReturnPeriod) < DateTime.UtcNow)
                        sigil.ReturnHome();
                }
                else
                {
                    if (sigil.IsBeingCorrupted && (sigil.CorruptionStart + Sigil.CorruptionPeriod) < DateTime.UtcNow)
                    {
                        sigil.Corrupted = sigil.Corrupting;
                        sigil.Corrupting = null;
                        sigil.CorruptionStart = DateTime.MinValue;
                        sigil.GraceStart = DateTime.MinValue;
                    }
                    else if (sigil.IsPurifying && (sigil.PurificationStart + Sigil.PurificationPeriod) < DateTime.UtcNow)
                    {
                        sigil.PurificationStart = DateTime.MinValue;
                        sigil.Corrupted = null;
                        sigil.Corrupting = null;
                        sigil.CorruptionStart = DateTime.MinValue;
                        sigil.GraceStart = DateTime.MinValue;
                    }
                }
            }
        }

        public static void HandleDeath(Mobile mob)
        {
            HandleDeath(mob, null);
        }

        #region Skill Loss
        public const double SkillLossFactor = 1.0 / 3;
        public static readonly TimeSpan SkillLossPeriod = TimeSpan.FromMinutes(20.0);

        private static Dictionary<Mobile, SkillLossContext> m_SkillLoss = new Dictionary<Mobile, SkillLossContext>();

        private class SkillLossContext
        {
            public Timer m_Timer;
            public List<SkillMod> m_Mods;
        }

        public static bool InSkillLoss(Mobile mob)
        {
            return m_SkillLoss.ContainsKey(mob);
        }

        public static void ApplySkillLoss(Mobile mob)
        {
            if (InSkillLoss(mob))
                return;

            SkillLossContext context = new SkillLossContext();
            m_SkillLoss[mob] = context;

            List<SkillMod> mods = context.m_Mods = new List<SkillMod>();

            for (int i = 0; i < mob.Skills.Length; ++i)
            {
                Skill sk = mob.Skills[i];
                double baseValue = sk.Base;

                if (baseValue > 0)
                {
                    SkillMod mod = new DefaultSkillMod(sk.SkillName, true, -(baseValue * SkillLossFactor));

                    mods.Add(mod);
                    mob.AddSkillMod(mod);
                }
            }

            context.m_Timer = Timer.DelayCall(SkillLossPeriod, new TimerStateCallback(ClearSkillLoss_Callback), mob);
        }

        private static void ClearSkillLoss_Callback(object state)
        {
            ClearSkillLoss((Mobile)state);
        }

        public static bool ClearSkillLoss(Mobile mob)
        {
            SkillLossContext context;

            if (!m_SkillLoss.TryGetValue(mob, out context))
                return false;

            m_SkillLoss.Remove(mob);

            List<SkillMod> mods = context.m_Mods;

            for (int i = 0; i < mods.Count; ++i)
                mob.RemoveSkillMod(mods[i]);

            context.m_Timer.Stop();

            return true;
        }
        #endregion

        public int AwardSilver(Mobile mob, int silver)
        {
            if (silver <= 0)
                return 0;

            int tithed = (silver * Tithe) / 100;

            Silver += tithed;

            silver = silver - tithed;

            if (silver > 0)
                mob.AddToBackpack(new Silver(silver));

            return silver;
        }

        public virtual int MaximumTraps { get { return 15; } }

        public List<BaseFactionTrap> Traps
        {
            get { return m_State.Traps; }
            set { m_State.Traps = value; }
        }

        public const int StabilityFactor = 300; // 300% greater (3 times) than smallest faction
        public const int StabilityActivation = 200; // Stablity code goes into effect when largest faction has > 200 people

        public static Faction FindSmallestFaction()
        {
            List<Faction> factions = Factions;
            Faction smallest = null;

            for (int i = 0; i < factions.Count; ++i)
            {
                Faction faction = factions[i];

                if (smallest == null || faction.Members.Count < smallest.Members.Count)
                    smallest = faction;
            }

            return smallest;
        }

        public static bool StabilityActive()
        {
            List<Faction> factions = Factions;

            for (int i = 0; i < factions.Count; ++i)
            {
                Faction faction = factions[i];

                if (faction.Members.Count > StabilityActivation)
                    return true;
            }

            return false;
        }

        public bool CanHandleInflux(int influx)
        {
            if (!StabilityActive())
                return true;

            Faction smallest = FindSmallestFaction();

            if (smallest == null)
                return true; // sanity

            if (StabilityFactor > 0 && (((this.Members.Count + influx) * 100) / StabilityFactor) > smallest.Members.Count)
                return false;

            return true;
        }

        public static void HandleDeath(Mobile victim, Mobile killer)
        {
            if (killer == null)
                killer = victim.FindMostRecentDamager(true);

            PlayerState killerState = PlayerState.Find(killer);

            Container pack = victim.Backpack;

            if (pack != null)
            {
                Container killerPack = (killer == null ? null : killer.Backpack);
                Item[] sigils = pack.FindItemsByType(typeof(Sigil));

                for (int i = 0; i < sigils.Length; ++i)
                {
                    Sigil sigil = (Sigil)sigils[i];

                    if (killerState != null && killerPack != null)
                    {
                        if (killer.GetDistanceToSqrt(victim) > 64)
                        {
                            sigil.ReturnHome();
                            killer.SendLocalizedMessage(1042230); // The sigil has gone back to its home location.
                        }
                        else if (Sigil.ExistsOn(killer))
                        {
                            sigil.ReturnHome();
                            killer.SendLocalizedMessage(1010258); // The sigil has gone back to its home location because you already have a sigil.
                        }
                        else if (!killerPack.TryDropItem(killer, sigil, false))
                        {
                            sigil.ReturnHome();
                            killer.SendLocalizedMessage(1010259); // The sigil has gone home because your backpack is full.
                        }
                    }
                    else
                    {
                        sigil.ReturnHome();
                    }
                }
            }

            if (killerState == null)
                return;

            if (victim is BaseCreature)
            {
                BaseCreature bc = (BaseCreature)victim;
                Faction victimFaction = bc.FactionAllegiance;

                if (bc.Map == Faction.Facet && victimFaction != null && killerState.Faction != victimFaction)
                {
                    int silver = killerState.Faction.AwardSilver(killer, bc.FactionSilverWorth);

                    if (silver > 0)
                        killer.SendLocalizedMessage(1042748, silver.ToString("N0")); // Thou hast earned ~1_AMOUNT~ silver for vanquishing the vile creature.
                }

                #region Ethics
                if (bc.Map == Faction.Facet && bc.GetEthicAllegiance(killer) == BaseCreature.Allegiance.Enemy)
                {
                    Ethics.Player killerEPL = Ethics.Player.Find(killer);

                    if (killerEPL != null && (100 - killerEPL.Power) > Utility.Random(100))
                    {
                        ++killerEPL.Power;
                        ++killerEPL.History;
                    }
                }
                #endregion

                return;
            }

            PlayerState victimState = PlayerState.Find(victim);

            if (victimState == null)
                return;

            #region Dueling
            if (victim.Region.IsPartOf(typeof(Engines.ConPVP.SafeZone)))
                return;
            #endregion

            if (killer == victim || killerState.Faction != victimState.Faction)
                ApplySkillLoss(victim);

            if (killerState.Faction != victimState.Faction)
            {
                if (victimState.KillPoints <= -6)
                {
                    killer.SendLocalizedMessage(501693); // This victim is not worth enough to get kill points from. 

                    #region Ethics
                    Ethics.Player killerEPL = Ethics.Player.Find(killer);
                    Ethics.Player victimEPL = Ethics.Player.Find(victim);

                    if (killerEPL != null && victimEPL != null && victimEPL.Power > 0 && victimState.CanGiveSilverTo(killer))
                    {
                        int powerTransfer = Math.Max(1, victimEPL.Power / 5);

                        if (powerTransfer > (100 - killerEPL.Power))
                            powerTransfer = 100 - killerEPL.Power;

                        if (powerTransfer > 0)
                        {
                            victimEPL.Power -= (powerTransfer + 1) / 2;
                            killerEPL.Power += powerTransfer;

                            killerEPL.History += powerTransfer;

                            victimState.OnGivenSilverTo(killer);
                        }
                    }
                    #endregion
                }
                else
                {
                    int award = Math.Max(victimState.KillPoints / 10, 1);

                    if (award > 40)
                        award = 40;

                    if (victimState.CanGiveSilverTo(killer))
                    {
                        PowerFactionItem.CheckSpawn(killer, victim);

                        if (victimState.KillPoints > 0)
                        {
                            victimState.IsActive = true;

                            if (1 > Utility.Random(3))
                                killerState.IsActive = true;

                            int silver = 0;

                            silver = killerState.Faction.AwardSilver(killer, award * 40);

                            if (silver > 0)
                                killer.SendLocalizedMessage(1042736, String.Format("{0:N0} silver\t{1}", silver, victim.Name)); // You have earned ~1_SILVER_AMOUNT~ pieces for vanquishing ~2_PLAYER_NAME~!
                        }

                        victimState.KillPoints -= award;
                        killerState.KillPoints += award;

                        int offset = (award != 1 ? 0 : 2); // for pluralization

                        string args = String.Format("{0}\t{1}\t{2}", award, victim.Name, killer.Name);

                        killer.SendLocalizedMessage(1042737 + offset, args); // Thou hast been honored with ~1_KILL_POINTS~ kill point(s) for vanquishing ~2_DEAD_PLAYER~!
                        victim.SendLocalizedMessage(1042738 + offset, args); // Thou has lost ~1_KILL_POINTS~ kill point(s) to ~3_ATTACKER_NAME~ for being vanquished!

                        #region Ethics
                        Ethics.Player killerEPL = Ethics.Player.Find(killer);
                        Ethics.Player victimEPL = Ethics.Player.Find(victim);

                        if (killerEPL != null && victimEPL != null && victimEPL.Power > 0)
                        {
                            int powerTransfer = Math.Max(1, victimEPL.Power / 5);

                            if (powerTransfer > (100 - killerEPL.Power))
                                powerTransfer = 100 - killerEPL.Power;

                            if (powerTransfer > 0)
                            {
                                victimEPL.Power -= (powerTransfer + 1) / 2;
                                killerEPL.Power += powerTransfer;

                                killerEPL.History += powerTransfer;
                            }
                        }
                        #endregion

                        victimState.OnGivenSilverTo(killer);
                    }
                    else
                    {
                        killer.SendLocalizedMessage(1042231); // You have recently defeated this enemy and thus their death brings you no honor.
                    }
                }
            }
        }

        private static void EventSink_Logout(LogoutEventArgs e)
        {
            Mobile mob = e.Mobile;

            Container pack = mob.Backpack;

            if (pack == null)
                return;

            Item[] sigils = pack.FindItemsByType(typeof(Sigil));

            for (int i = 0; i < sigils.Length; ++i)
                ((Sigil)sigils[i]).ReturnHome();
        }

        private static void EventSink_Login(LoginEventArgs e)
        {
            Mobile mob = e.Mobile;

            CheckLeaveTimer(mob);
        }

        public static readonly Map Facet = Map.Felucca;

        public static void WriteReference(GenericWriter writer, Faction fact)
        {
            int idx = Factions.IndexOf(fact);

            writer.WriteEncodedInt((int)(idx + 1));
        }

        public static List<Faction> Factions { get { return Reflector.Factions; } }

        public static Faction ReadReference(GenericReader reader)
        {
            int idx = reader.ReadEncodedInt() - 1;

            if (idx >= 0 && idx < Factions.Count)
                return Factions[idx];

            return null;
        }

        public static Faction Find(Mobile mob)
        {
            return Find(mob, false, false);
        }

        public static Faction Find(Mobile mob, bool inherit)
        {
            return Find(mob, inherit, false);
        }

        public static Faction Find(Mobile mob, bool inherit, bool creatureAllegiances)
        {
            PlayerState pl = PlayerState.Find(mob);

            if (pl != null)
                return pl.Faction;

            if (inherit && mob is BaseCreature)
            {
                BaseCreature bc = (BaseCreature)mob;

                if (bc.Controlled)
                    return Find(bc.ControlMaster, false);
                else if (bc.Summoned)
                    return Find(bc.SummonMaster, false);
                else if (creatureAllegiances && mob is BaseFactionGuard)
                    return ((BaseFactionGuard)mob).Faction;
                else if (creatureAllegiances)
                    return bc.FactionAllegiance;
            }

            return null;
        }

        public static Faction Parse(string name)
        {
            List<Faction> factions = Factions;

            for (int i = 0; i < factions.Count; ++i)
            {
                Faction faction = factions[i];

                if (Insensitive.Equals(faction.Definition.FriendlyName, name))
                    return faction;
            }

            return null;
        }
    }

    public abstract class FactionGump : Gump
    {
        public virtual int ButtonTypes { get { return 10; } }

        public int ToButtonID(int type, int index)
        {
            return 1 + (index * ButtonTypes) + type;
        }

        public bool FromButtonID(int buttonID, out int type, out int index)
        {
            int offset = buttonID - 1;

            if (offset >= 0)
            {
                type = offset % ButtonTypes;
                index = offset / ButtonTypes;
                return true;
            }
            else
            {
                type = index = 0;
                return false;
            }
        }

        public static bool Exists(Mobile mob)
        {
            return (mob.FindGump(typeof(FactionGump)) != null);
        }

        public void AddHtmlText(int x, int y, int width, int height, TextDefinition text, bool back, bool scroll)
        {
            if (text != null && text.Number > 0)
                AddHtmlLocalized(x, y, width, height, text.Number, back, scroll);
            else if (text != null && text.String != null)
                AddHtml(x, y, width, height, text.String, back, scroll);
        }

        public FactionGump(int x, int y)
            : base(x, y)
        {
        }
    }

    #region Faction Generator

    public class Generator
    {
        public static void Initialize()
        {
            CommandSystem.Register("GenerateFactions", AccessLevel.Administrator, new CommandEventHandler(GenerateFactions_OnCommand));
        }

        public static void GenerateFactions_OnCommand(CommandEventArgs e)
        {
            new FactionPersistance();

            List<Faction> factions = Faction.Factions;

            foreach (Faction faction in factions)
                Generate(faction);

            List<Town> towns = Town.Towns;

            foreach (Town town in towns)
                Generate(town);
        }

        public static void Generate(Town town)
        {
            Map facet = Faction.Facet;

            TownDefinition def = town.Definition;

            if (!CheckExistance(def.Monolith, facet, typeof(TownMonolith)))
            {
                TownMonolith mono = new TownMonolith(town);
                mono.MoveToWorld(def.Monolith, facet);
                mono.Sigil = new Sigil(town);
            }

            if (!CheckExistance(def.TownStone, facet, typeof(TownStone)))
                new TownStone(town).MoveToWorld(def.TownStone, facet);
        }

        public static void Generate(Faction faction)
        {
            Map facet = Faction.Facet;

            List<Town> towns = Town.Towns;

            StrongholdDefinition stronghold = faction.Definition.Stronghold;

            if (!CheckExistance(stronghold.JoinStone, facet, typeof(JoinStone)))
                new JoinStone(faction).MoveToWorld(stronghold.JoinStone, facet);

            if (!CheckExistance(stronghold.FactionStone, facet, typeof(FactionStone)))
                new FactionStone(faction).MoveToWorld(stronghold.FactionStone, facet);

            for (int i = 0; i < stronghold.Monoliths.Length; ++i)
            {
                Point3D monolith = stronghold.Monoliths[i];

                if (!CheckExistance(monolith, facet, typeof(StrongholdMonolith)))
                    new StrongholdMonolith(towns[i], faction).MoveToWorld(monolith, facet);
            }
        }

        private static bool CheckExistance(Point3D loc, Map facet, Type type)
        {
            foreach (Item item in facet.GetItemsInRange(loc, 0))
            {
                if (type.IsAssignableFrom(item.GetType()))
                    return true;
            }

            return false;
        }
    }

    #endregion

    public class RankDefinition
    {
        private int m_Rank;
        private int m_Required;
        private int m_MaxWearables;
        private TextDefinition m_Title;

        public int Rank { get { return m_Rank; } }
        public int Required { get { return m_Required; } }
        public int MaxWearables { get { return m_MaxWearables; } }
        public TextDefinition Title { get { return m_Title; } }

        public RankDefinition(int rank, int required, int maxWearables, TextDefinition title)
        {
            m_Rank = rank;
            m_Required = required;
            m_Title = title;
            m_MaxWearables = maxWearables;
        }
    }

    public class FactionPersistance : Item
    {
        private static FactionPersistance m_Instance;

        public static FactionPersistance Instance { get { return m_Instance; } }

        public override string DefaultName
        {
            get { return "Faction Persistance - Internal"; }
        }

        public FactionPersistance()
            : base(1)
        {
            Movable = false;

            if (m_Instance == null || m_Instance.Deleted)
                m_Instance = this;
            else
                base.Delete();
        }

        private enum PersistedType
        {
            Terminator,
            Faction,
            Town
        }

        public FactionPersistance(Serial serial)
            : base(serial)
        {
            m_Instance = this;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            List<Faction> factions = Faction.Factions;

            for (int i = 0; i < factions.Count; ++i)
            {
                writer.WriteEncodedInt((int)PersistedType.Faction);
                factions[i].State.Serialize(writer);
            }

            List<Town> towns = Town.Towns;

            for (int i = 0; i < towns.Count; ++i)
            {
                writer.WriteEncodedInt((int)PersistedType.Town);
                towns[i].State.Serialize(writer);
            }

            writer.WriteEncodedInt((int)PersistedType.Terminator);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        PersistedType type;

                        while ((type = (PersistedType)reader.ReadEncodedInt()) != PersistedType.Terminator)
                        {
                            switch (type)
                            {
                                case PersistedType.Faction: new FactionState(reader); break;
                                case PersistedType.Town: new TownState(reader); break;
                            }
                        }

                        break;
                    }
            }
        }

        public override void Delete()
        {
        }
    }

    public class FactionState
    {
        private Faction m_Faction;
        private Mobile m_Commander;
        private int m_Tithe;
        private int m_Silver;
        private List<PlayerState> m_Members;
        private Election m_Election;
        private List<FactionItem> m_FactionItems;
        private List<BaseFactionTrap> m_FactionTraps;
        private DateTime m_LastAtrophy;

        private const int BroadcastsPerPeriod = 2;
        private static readonly TimeSpan BroadcastPeriod = TimeSpan.FromHours(1.0);

        private DateTime[] m_LastBroadcasts = new DateTime[BroadcastsPerPeriod];

        public DateTime LastAtrophy { get { return m_LastAtrophy; } set { m_LastAtrophy = value; } }

        public bool FactionMessageReady
        {
            get
            {
                for (int i = 0; i < m_LastBroadcasts.Length; ++i)
                {
                    if (DateTime.UtcNow >= (m_LastBroadcasts[i] + BroadcastPeriod))
                        return true;
                }

                return false;
            }
        }

        public bool IsAtrophyReady { get { return DateTime.UtcNow >= (m_LastAtrophy + TimeSpan.FromHours(47.0)); } }

        public int CheckAtrophy()
        {
            if (DateTime.UtcNow < (m_LastAtrophy + TimeSpan.FromHours(47.0)))
                return 0;

            int distrib = 0;
            m_LastAtrophy = DateTime.UtcNow;

            List<PlayerState> members = new List<PlayerState>(m_Members);

            for (int i = 0; i < members.Count; ++i)
            {
                PlayerState ps = members[i];

                if (ps.IsActive)
                {
                    ps.IsActive = false;
                    continue;
                }
                else if (ps.KillPoints > 0)
                {
                    int atrophy = (ps.KillPoints + 9) / 10;
                    ps.KillPoints -= atrophy;
                    distrib += atrophy;
                }
            }

            return distrib;
        }

        public void RegisterBroadcast()
        {
            for (int i = 0; i < m_LastBroadcasts.Length; ++i)
            {
                if (DateTime.UtcNow >= (m_LastBroadcasts[i] + BroadcastPeriod))
                {
                    m_LastBroadcasts[i] = DateTime.UtcNow;
                    break;
                }
            }
        }

        public List<FactionItem> FactionItems
        {
            get { return m_FactionItems; }
            set { m_FactionItems = value; }
        }

        public List<BaseFactionTrap> Traps
        {
            get { return m_FactionTraps; }
            set { m_FactionTraps = value; }
        }

        public Election Election
        {
            get { return m_Election; }
            set { m_Election = value; }
        }

        public Mobile Commander
        {
            get { return m_Commander; }
            set
            {
                if (m_Commander != null)
                    m_Commander.InvalidateProperties();

                m_Commander = value;

                if (m_Commander != null)
                {
                    m_Commander.SendLocalizedMessage(1042227); // You have been elected Commander of your faction

                    m_Commander.InvalidateProperties();

                    PlayerState pl = PlayerState.Find(m_Commander);

                    if (pl != null && pl.Finance != null)
                        pl.Finance.Finance = null;

                    if (pl != null && pl.Sheriff != null)
                        pl.Sheriff.Sheriff = null;
                }
            }
        }

        public int Tithe
        {
            get { return m_Tithe; }
            set { m_Tithe = value; }
        }

        public int Silver
        {
            get { return m_Silver; }
            set { m_Silver = value; }
        }

        public List<PlayerState> Members
        {
            get { return m_Members; }
            set { m_Members = value; }
        }

        public FactionState(Faction faction)
        {
            m_Faction = faction;
            m_Tithe = 50;
            m_Members = new List<PlayerState>();
            m_Election = new Election(faction);
            m_FactionItems = new List<FactionItem>();
            m_FactionTraps = new List<BaseFactionTrap>();
        }

        public FactionState(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            switch (version)
            {
                case 5:
                    {
                        m_LastAtrophy = reader.ReadDateTime();
                        goto case 4;
                    }
                case 4:
                    {
                        int count = reader.ReadEncodedInt();

                        for (int i = 0; i < count; ++i)
                        {
                            DateTime time = reader.ReadDateTime();

                            if (i < m_LastBroadcasts.Length)
                                m_LastBroadcasts[i] = time;
                        }

                        goto case 3;
                    }
                case 3:
                case 2:
                case 1:
                    {
                        m_Election = new Election(reader);

                        goto case 0;
                    }
                case 0:
                    {
                        m_Faction = Faction.ReadReference(reader);

                        m_Commander = reader.ReadMobile();

                        if (version < 5)
                            m_LastAtrophy = DateTime.UtcNow;

                        if (version < 4)
                        {
                            DateTime time = reader.ReadDateTime();

                            if (m_LastBroadcasts.Length > 0)
                                m_LastBroadcasts[0] = time;
                        }

                        m_Tithe = reader.ReadEncodedInt();
                        m_Silver = reader.ReadEncodedInt();

                        int memberCount = reader.ReadEncodedInt();

                        m_Members = new List<PlayerState>();

                        for (int i = 0; i < memberCount; ++i)
                        {
                            PlayerState pl = new PlayerState(reader, m_Faction, m_Members);

                            if (pl.Mobile != null)
                                m_Members.Add(pl);
                        }

                        m_Faction.State = this;

                        m_Faction.ZeroRankOffset = m_Members.Count;
                        m_Members.Sort();

                        for (int i = m_Members.Count - 1; i >= 0; i--)
                        {
                            PlayerState player = m_Members[i];

                            if (player.KillPoints <= 0)
                                m_Faction.ZeroRankOffset = i;
                            else
                                player.RankIndex = i;
                        }

                        m_FactionItems = new List<FactionItem>();

                        if (version >= 2)
                        {
                            int factionItemCount = reader.ReadEncodedInt();

                            for (int i = 0; i < factionItemCount; ++i)
                            {
                                FactionItem factionItem = new FactionItem(reader, m_Faction);

                                Timer.DelayCall(TimeSpan.Zero, new TimerCallback(factionItem.CheckAttach)); // sandbox attachment
                            }
                        }

                        m_FactionTraps = new List<BaseFactionTrap>();

                        if (version >= 3)
                        {
                            int factionTrapCount = reader.ReadEncodedInt();

                            for (int i = 0; i < factionTrapCount; ++i)
                            {
                                BaseFactionTrap trap = reader.ReadItem() as BaseFactionTrap;

                                if (trap != null && !trap.CheckDecay())
                                    m_FactionTraps.Add(trap);
                            }
                        }

                        break;
                    }
            }

            if (version < 1)
                m_Election = new Election(m_Faction);
        }

        public void Serialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)5); // version

            writer.Write(m_LastAtrophy);

            writer.WriteEncodedInt((int)m_LastBroadcasts.Length);

            for (int i = 0; i < m_LastBroadcasts.Length; ++i)
                writer.Write((DateTime)m_LastBroadcasts[i]);

            m_Election.Serialize(writer);

            Faction.WriteReference(writer, m_Faction);

            writer.Write((Mobile)m_Commander);

            writer.WriteEncodedInt((int)m_Tithe);
            writer.WriteEncodedInt((int)m_Silver);

            writer.WriteEncodedInt((int)m_Members.Count);

            for (int i = 0; i < m_Members.Count; ++i)
            {
                PlayerState pl = (PlayerState)m_Members[i];

                pl.Serialize(writer);
            }

            writer.WriteEncodedInt((int)m_FactionItems.Count);

            for (int i = 0; i < m_FactionItems.Count; ++i)
                m_FactionItems[i].Serialize(writer);

            writer.WriteEncodedInt((int)m_FactionTraps.Count);

            for (int i = 0; i < m_FactionTraps.Count; ++i)
                writer.Write((Item)m_FactionTraps[i]);
        }
    }

    public class PlayerState : IComparable
    {
        private Mobile m_Mobile;
        private Faction m_Faction;
        private List<PlayerState> m_Owner;
        private int m_KillPoints;
        private DateTime m_Leaving;
        private MerchantTitle m_MerchantTitle;
        private RankDefinition m_Rank;
        private List<SilverGivenEntry> m_SilverGiven;
        private bool m_IsActive;

        private Town m_Sheriff;
        private Town m_Finance;

        private DateTime m_LastHonorTime;

        public Mobile Mobile { get { return m_Mobile; } }
        public Faction Faction { get { return m_Faction; } }
        public List<PlayerState> Owner { get { return m_Owner; } }
        public MerchantTitle MerchantTitle { get { return m_MerchantTitle; } set { m_MerchantTitle = value; Invalidate(); } }
        public Town Sheriff { get { return m_Sheriff; } set { m_Sheriff = value; Invalidate(); } }
        public Town Finance { get { return m_Finance; } set { m_Finance = value; Invalidate(); } }
        public List<SilverGivenEntry> SilverGiven { get { return m_SilverGiven; } }

        public int KillPoints
        {
            get { return m_KillPoints; }
            set
            {
                if (m_KillPoints != value)
                {
                    if (value > m_KillPoints)
                    {
                        if (m_KillPoints <= 0)
                        {
                            if (value <= 0)
                            {
                                m_KillPoints = value;
                                Invalidate();
                                return;
                            }

                            m_Owner.Remove(this);
                            m_Owner.Insert(m_Faction.ZeroRankOffset, this);

                            m_RankIndex = m_Faction.ZeroRankOffset;
                            m_Faction.ZeroRankOffset++;
                        }
                        while ((m_RankIndex - 1) >= 0)
                        {
                            PlayerState p = m_Owner[m_RankIndex - 1] as PlayerState;
                            if (value > p.KillPoints)
                            {
                                m_Owner[m_RankIndex] = p;
                                m_Owner[m_RankIndex - 1] = this;
                                RankIndex--;
                                p.RankIndex++;
                            }
                            else
                                break;
                        }
                    }
                    else
                    {
                        if (value <= 0)
                        {
                            if (m_KillPoints <= 0)
                            {
                                m_KillPoints = value;
                                Invalidate();
                                return;
                            }

                            while ((m_RankIndex + 1) < m_Faction.ZeroRankOffset)
                            {
                                PlayerState p = m_Owner[m_RankIndex + 1] as PlayerState;
                                m_Owner[m_RankIndex + 1] = this;
                                m_Owner[m_RankIndex] = p;
                                RankIndex++;
                                p.RankIndex--;
                            }

                            m_RankIndex = -1;
                            m_Faction.ZeroRankOffset--;
                        }
                        else
                        {
                            while ((m_RankIndex + 1) < m_Faction.ZeroRankOffset)
                            {
                                PlayerState p = m_Owner[m_RankIndex + 1] as PlayerState;
                                if (value < p.KillPoints)
                                {
                                    m_Owner[m_RankIndex + 1] = this;
                                    m_Owner[m_RankIndex] = p;
                                    RankIndex++;
                                    p.RankIndex--;
                                }
                                else
                                    break;
                            }
                        }
                    }

                    m_KillPoints = value;
                    Invalidate();
                }
            }
        }

        private bool m_InvalidateRank = true;
        private int m_RankIndex = -1;

        public int RankIndex { get { return m_RankIndex; } set { if (m_RankIndex != value) { m_RankIndex = value; m_InvalidateRank = true; } } }

        public RankDefinition Rank
        {
            get
            {
                if (m_InvalidateRank)
                {
                    RankDefinition[] ranks = m_Faction.Definition.Ranks;
                    int percent;

                    if (m_Owner.Count == 1)
                        percent = 1000;
                    else if (m_RankIndex == -1)
                        percent = 0;
                    else
                        percent = ((m_Faction.ZeroRankOffset - m_RankIndex) * 1000) / m_Faction.ZeroRankOffset;

                    for (int i = 0; i < ranks.Length; i++)
                    {
                        RankDefinition check = ranks[i];

                        if (percent >= check.Required)
                        {
                            m_Rank = check;
                            m_InvalidateRank = false;
                            break;
                        }
                    }

                    Invalidate();
                }

                return m_Rank;
            }
        }

        public DateTime LastHonorTime { get { return m_LastHonorTime; } set { m_LastHonorTime = value; } }
        public DateTime Leaving { get { return m_Leaving; } set { m_Leaving = value; } }
        public bool IsLeaving { get { return (m_Leaving > DateTime.MinValue); } }

        public bool IsActive { get { return m_IsActive; } set { m_IsActive = value; } }

        public bool CanGiveSilverTo(Mobile mob)
        {
            if (m_SilverGiven == null)
                return true;

            for (int i = 0; i < m_SilverGiven.Count; ++i)
            {
                SilverGivenEntry sge = m_SilverGiven[i];

                if (sge.IsExpired)
                    m_SilverGiven.RemoveAt(i--);
                else if (sge.GivenTo == mob)
                    return false;
            }

            return true;
        }

        public void OnGivenSilverTo(Mobile mob)
        {
            if (m_SilverGiven == null)
                m_SilverGiven = new List<SilverGivenEntry>();

            m_SilverGiven.Add(new SilverGivenEntry(mob));
        }

        public void Invalidate()
        {
            if (m_Mobile is PlayerMobile)
            {
                PlayerMobile pm = (PlayerMobile)m_Mobile;
                pm.InvalidateProperties();
                pm.InvalidateMyRunUO();
            }
        }

        public void Attach()
        {
            if (m_Mobile is PlayerMobile)
                ((PlayerMobile)m_Mobile).FactionPlayerState = this;
        }

        public PlayerState(Mobile mob, Faction faction, List<PlayerState> owner)
        {
            m_Mobile = mob;
            m_Faction = faction;
            m_Owner = owner;

            Attach();
            Invalidate();
        }

        public PlayerState(GenericReader reader, Faction faction, List<PlayerState> owner)
        {
            m_Faction = faction;
            m_Owner = owner;

            int version = reader.ReadEncodedInt();

            switch (version)
            {
                case 1:
                    {
                        m_IsActive = reader.ReadBool();
                        m_LastHonorTime = reader.ReadDateTime();
                        goto case 0;
                    }
                case 0:
                    {
                        m_Mobile = reader.ReadMobile();

                        m_KillPoints = reader.ReadEncodedInt();
                        m_MerchantTitle = (MerchantTitle)reader.ReadEncodedInt();

                        m_Leaving = reader.ReadDateTime();

                        break;
                    }
            }

            Attach();
        }

        public void Serialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)1); // version

            writer.Write(m_IsActive);
            writer.Write(m_LastHonorTime);

            writer.Write((Mobile)m_Mobile);

            writer.WriteEncodedInt((int)m_KillPoints);
            writer.WriteEncodedInt((int)m_MerchantTitle);

            writer.Write((DateTime)m_Leaving);
        }

        public static PlayerState Find(Mobile mob)
        {
            if (mob is PlayerMobile)
                return ((PlayerMobile)mob).FactionPlayerState;

            return null;
        }

        public int CompareTo(object obj)
        {
            return ((PlayerState)obj).m_KillPoints - m_KillPoints;
        }
    }

    public class LeaveFactionGump : FactionGump
    {
        private PlayerMobile m_From;
        private Faction m_Faction;

        public LeaveFactionGump(PlayerMobile from, Faction faction)
            : base(20, 30)
        {
            m_From = from;
            m_Faction = faction;

            AddBackground(0, 0, 270, 120, 5054);
            AddBackground(10, 10, 250, 100, 3000);

            if (from.Guild is Guild && ((Guild)from.Guild).Leader == from)
                AddHtmlLocalized(20, 15, 230, 60, 1018057, true, true); // Are you sure you want your entire guild to leave this faction?
            else
                AddHtmlLocalized(20, 15, 230, 60, 1018063, true, true); // Are you sure you want to leave this faction?

            AddHtmlLocalized(55, 80, 75, 20, 1011011, false, false); // CONTINUE
            AddButton(20, 80, 4005, 4007, 1, GumpButtonType.Reply, 0);

            AddHtmlLocalized(170, 80, 75, 20, 1011012, false, false); // CANCEL
            AddButton(135, 80, 4005, 4007, 2, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            switch (info.ButtonID)
            {
                case 1: // continue
                    {
                        Guild guild = m_From.Guild as Guild;

                        if (guild == null)
                        {
                            PlayerState pl = PlayerState.Find(m_From);

                            if (pl != null)
                            {
                                pl.Leaving = DateTime.UtcNow;

                                if (Faction.LeavePeriod == TimeSpan.FromDays(3.0))
                                    m_From.SendLocalizedMessage(1005065); // You will be removed from the faction in 3 days
                                else
                                    m_From.SendMessage("You will be removed from the faction in {0} days.", Faction.LeavePeriod.TotalDays);
                            }
                        }
                        else if (guild.Leader != m_From)
                        {
                            m_From.SendLocalizedMessage(1005061); // You cannot quit the faction because you are not the guild master
                        }
                        else
                        {
                            m_From.SendLocalizedMessage(1042285); // Your guild is now quitting the faction.

                            for (int i = 0; i < guild.Members.Count; ++i)
                            {
                                Mobile mob = (Mobile)guild.Members[i];
                                PlayerState pl = PlayerState.Find(mob);

                                if (pl != null)
                                {
                                    pl.Leaving = DateTime.UtcNow;

                                    if (Faction.LeavePeriod == TimeSpan.FromDays(3.0))
                                        mob.SendLocalizedMessage(1005060); // Your guild will quit the faction in 3 days
                                    else
                                        mob.SendMessage("Your guild will quit the faction in {0} days.", Faction.LeavePeriod.TotalDays);
                                }
                            }
                        }

                        break;
                    }
                case 2: // cancel
                    {
                        m_From.SendLocalizedMessage(500737); // Canceled resignation.
                        break;
                    }
            }
        }
    }

    public enum FactionKickType
    {
        Kick,
        Ban,
        Unban
    }

    public class FactionKickCommand : BaseCommand
    {
        private FactionKickType m_KickType;

        public FactionKickCommand(FactionKickType kickType)
        {
            m_KickType = kickType;

            AccessLevel = AccessLevel.GameMaster;
            Supports = CommandSupport.AllMobiles;
            ObjectTypes = ObjectTypes.Mobiles;

            switch (m_KickType)
            {
                case FactionKickType.Kick:
                    {
                        Commands = new string[] { "FactionKick" };
                        Usage = "FactionKick";
                        Description = "Kicks the targeted player out of his current faction. This does not prevent them from rejoining.";
                        break;
                    }
                case FactionKickType.Ban:
                    {
                        Commands = new string[] { "FactionBan" };
                        Usage = "FactionBan";
                        Description = "Bans the account of a targeted player from joining factions. All players on the account are removed from their current faction, if any.";
                        break;
                    }
                case FactionKickType.Unban:
                    {
                        Commands = new string[] { "FactionUnban" };
                        Usage = "FactionUnban";
                        Description = "Unbans the account of a targeted player from joining factions.";
                        break;
                    }
            }
        }

        public override void Execute(CommandEventArgs e, object obj)
        {
            Mobile mob = (Mobile)obj;

            switch (m_KickType)
            {
                case FactionKickType.Kick:
                    {
                        PlayerState pl = PlayerState.Find(mob);

                        if (pl != null)
                        {
                            pl.Faction.RemoveMember(mob);
                            mob.SendMessage("You have been kicked from your faction.");
                            AddResponse("They have been kicked from their faction.");
                        }
                        else
                        {
                            LogFailure("They are not in a faction.");
                        }

                        break;
                    }
                case FactionKickType.Ban:
                    {
                        Account acct = mob.Account as Account;

                        if (acct != null)
                        {
                            if (acct.GetTag("FactionBanned") == null)
                            {
                                acct.SetTag("FactionBanned", "true");
                                AddResponse("The account has been banned from joining factions.");
                            }
                            else
                            {
                                AddResponse("The account is already banned from joining factions.");
                            }

                            for (int i = 0; i < acct.Length; ++i)
                            {
                                mob = acct[i];

                                if (mob != null)
                                {
                                    PlayerState pl = PlayerState.Find(mob);

                                    if (pl != null)
                                    {
                                        pl.Faction.RemoveMember(mob);
                                        mob.SendMessage("You have been kicked from your faction.");
                                        AddResponse("They have been kicked from their faction.");
                                    }
                                }
                            }
                        }
                        else
                        {
                            LogFailure("They have no assigned account.");
                        }

                        break;
                    }
                case FactionKickType.Unban:
                    {
                        Account acct = mob.Account as Account;

                        if (acct != null)
                        {
                            if (acct.GetTag("FactionBanned") == null)
                            {
                                AddResponse("The account is not already banned from joining factions.");
                            }
                            else
                            {
                                acct.RemoveTag("FactionBanned");
                                AddResponse("The account may now freely join factions.");
                            }
                        }
                        else
                        {
                            LogFailure("They have no assigned account.");
                        }

                        break;
                    }
            }
        }
    }

    #region Region And Towns

    public class StrongholdDefinition
    {
        private Rectangle2D[] m_Area;
        private Point3D m_JoinStone;
        private Point3D m_FactionStone;
        private Point3D[] m_Monoliths;

        public Rectangle2D[] Area { get { return m_Area; } }

        public Point3D JoinStone { get { return m_JoinStone; } }
        public Point3D FactionStone { get { return m_FactionStone; } }

        public Point3D[] Monoliths { get { return m_Monoliths; } }

        public StrongholdDefinition(Rectangle2D[] area, Point3D joinStone, Point3D factionStone, Point3D[] monoliths)
        {
            m_Area = area;
            m_JoinStone = joinStone;
            m_FactionStone = factionStone;
            m_Monoliths = monoliths;
        }
    }

    public class StrongholdRegion : BaseRegion
    {
        private Faction m_Faction;

        public Faction Faction
        {
            get { return m_Faction; }
            set { m_Faction = value; }
        }

        public StrongholdRegion(Faction faction)
            : base(faction.Definition.FriendlyName, Faction.Facet, Region.DefaultPriority, faction.Definition.Stronghold.Area)
        {
            m_Faction = faction;

            Register();
        }

        public override bool OnMoveInto(Mobile m, Direction d, Point3D newLocation, Point3D oldLocation)
        {
            if (!base.OnMoveInto(m, d, newLocation, oldLocation))
                return false;

            if (m.AccessLevel >= AccessLevel.Counselor || Contains(oldLocation))
                return true;

            if (m is PlayerMobile)
            {
                PlayerMobile pm = (PlayerMobile)m;

                if (pm.DuelContext != null)
                {
                    m.SendMessage("You may not enter this area while participating in a duel or a tournament.");
                    return false;
                }
            }

            return (Faction.Find(m, true, true) != null);
        }

        public override bool AllowHousing(Mobile from, Point3D p)
        {
            return false;
        }
    }

    public class TownDefinition
    {
        private int m_Sort;
        private int m_SigilID;

        private string m_Region;

        private string m_FriendlyName;

        private TextDefinition m_TownName;
        private TextDefinition m_TownStoneHeader;
        private TextDefinition m_StrongholdMonolithName;
        private TextDefinition m_TownMonolithName;
        private TextDefinition m_TownStoneName;
        private TextDefinition m_SigilName;
        private TextDefinition m_CorruptedSigilName;

        private Point3D m_Monolith;
        private Point3D m_TownStone;

        public int Sort { get { return m_Sort; } }
        public int SigilID { get { return m_SigilID; } }

        public string Region { get { return m_Region; } }
        public string FriendlyName { get { return m_FriendlyName; } }

        public TextDefinition TownName { get { return m_TownName; } }
        public TextDefinition TownStoneHeader { get { return m_TownStoneHeader; } }
        public TextDefinition StrongholdMonolithName { get { return m_StrongholdMonolithName; } }
        public TextDefinition TownMonolithName { get { return m_TownMonolithName; } }
        public TextDefinition TownStoneName { get { return m_TownStoneName; } }
        public TextDefinition SigilName { get { return m_SigilName; } }
        public TextDefinition CorruptedSigilName { get { return m_CorruptedSigilName; } }

        public Point3D Monolith { get { return m_Monolith; } }
        public Point3D TownStone { get { return m_TownStone; } }

        public TownDefinition(int sort, int sigilID, string region, string friendlyName, TextDefinition townName, TextDefinition townStoneHeader, TextDefinition strongholdMonolithName, TextDefinition townMonolithName, TextDefinition townStoneName, TextDefinition sigilName, TextDefinition corruptedSigilName, Point3D monolith, Point3D townStone)
        {
            m_Sort = sort;
            m_SigilID = sigilID;
            m_Region = region;
            m_FriendlyName = friendlyName;
            m_TownName = townName;
            m_TownStoneHeader = townStoneHeader;
            m_StrongholdMonolithName = strongholdMonolithName;
            m_TownMonolithName = townMonolithName;
            m_TownStoneName = townStoneName;
            m_SigilName = sigilName;
            m_CorruptedSigilName = corruptedSigilName;
            m_Monolith = monolith;
            m_TownStone = townStone;
        }
    }

    [CustomEnum(new string[] { "Britain", "Magincia", "Minoc", "Moonglow", "Skara Brae", "Trinsic", "Vesper", "Yew" })]
    public abstract class Town : IComparable
    {
        private TownDefinition m_Definition;
        private TownState m_State;

        public TownDefinition Definition
        {
            get { return m_Definition; }
            set { m_Definition = value; }
        }

        public TownState State
        {
            get { return m_State; }
            set { m_State = value; ConstructGuardLists(); }
        }

        public int Silver
        {
            get { return m_State.Silver; }
            set { m_State.Silver = value; }
        }

        public Faction Owner
        {
            get { return m_State.Owner; }
            set { Capture(value); }
        }

        public Mobile Sheriff
        {
            get { return m_State.Sheriff; }
            set { m_State.Sheriff = value; }
        }

        public Mobile Finance
        {
            get { return m_State.Finance; }
            set { m_State.Finance = value; }
        }

        public int Tax
        {
            get { return m_State.Tax; }
            set { m_State.Tax = value; }
        }

        public DateTime LastTaxChange
        {
            get { return m_State.LastTaxChange; }
            set { m_State.LastTaxChange = value; }
        }

        public static readonly TimeSpan TaxChangePeriod = TimeSpan.FromHours(12.0);
        public static readonly TimeSpan IncomePeriod = TimeSpan.FromDays(1.0);

        public bool TaxChangeReady
        {
            get { return (m_State.LastTaxChange + TaxChangePeriod) < DateTime.UtcNow; }
        }

        public static Town FromRegion(Region reg)
        {
            if (reg.Map != Faction.Facet)
                return null;

            List<Town> towns = Towns;

            for (int i = 0; i < towns.Count; ++i)
            {
                Town town = towns[i];

                if (reg.IsPartOf(town.Definition.Region))
                    return town;
            }

            return null;
        }

        public int FinanceUpkeep
        {
            get
            {
                List<VendorList> vendorLists = VendorLists;
                int upkeep = 0;

                for (int i = 0; i < vendorLists.Count; ++i)
                    upkeep += vendorLists[i].Vendors.Count * vendorLists[i].Definition.Upkeep;

                return upkeep;
            }
        }

        public int SheriffUpkeep
        {
            get
            {
                List<GuardList> guardLists = GuardLists;
                int upkeep = 0;

                for (int i = 0; i < guardLists.Count; ++i)
                    upkeep += guardLists[i].Guards.Count * guardLists[i].Definition.Upkeep;

                return upkeep;
            }
        }

        public int DailyIncome
        {
            get { return (10000 * (100 + m_State.Tax)) / 100; }
        }

        public int NetCashFlow
        {
            get { return DailyIncome - FinanceUpkeep - SheriffUpkeep; }
        }

        public TownMonolith Monolith
        {
            get
            {
                List<BaseMonolith> monoliths = BaseMonolith.Monoliths;

                foreach (BaseMonolith monolith in monoliths)
                {
                    if (monolith is TownMonolith)
                    {
                        TownMonolith townMonolith = (TownMonolith)monolith;

                        if (townMonolith.Town == this)
                            return townMonolith;
                    }
                }

                return null;
            }
        }

        public DateTime LastIncome
        {
            get { return m_State.LastIncome; }
            set { m_State.LastIncome = value; }
        }

        public void BeginOrderFiring(Mobile from)
        {
            bool isFinance = IsFinance(from);
            bool isSheriff = IsSheriff(from);
            string type = null;

            // NOTE: Messages not OSI-accurate, intentional
            if (isFinance && isSheriff) // GM only
                type = "vendor or guard";
            else if (isFinance)
                type = "vendor";
            else if (isSheriff)
                type = "guard";

            from.SendMessage("Target the {0} you wish to dismiss.", type);
            from.BeginTarget(12, false, TargetFlags.None, new TargetCallback(EndOrderFiring));
        }

        public void EndOrderFiring(Mobile from, object obj)
        {
            bool isFinance = IsFinance(from);
            bool isSheriff = IsSheriff(from);
            string type = null;

            if (isFinance && isSheriff) // GM only
                type = "vendor or guard";
            else if (isFinance)
                type = "vendor";
            else if (isSheriff)
                type = "guard";

            if (obj is BaseFactionVendor)
            {
                BaseFactionVendor vendor = (BaseFactionVendor)obj;

                if (vendor.Town == this && isFinance)
                    vendor.Delete();
            }
            else if (obj is BaseFactionGuard)
            {
                BaseFactionGuard guard = (BaseFactionGuard)obj;

                if (guard.Town == this && isSheriff)
                    guard.Delete();
            }
            else
            {
                from.SendMessage("That is not a {0}!", type);
            }
        }

        private Timer m_IncomeTimer;

        public void StartIncomeTimer()
        {
            if (m_IncomeTimer != null)
                m_IncomeTimer.Stop();

            m_IncomeTimer = Timer.DelayCall(TimeSpan.FromMinutes(1.0), TimeSpan.FromMinutes(1.0), new TimerCallback(CheckIncome));
        }

        public void StopIncomeTimer()
        {
            if (m_IncomeTimer != null)
                m_IncomeTimer.Stop();

            m_IncomeTimer = null;
        }

        public void CheckIncome()
        {
            if ((LastIncome + IncomePeriod) > DateTime.UtcNow || Owner == null)
                return;

            ProcessIncome();
        }

        public void ProcessIncome()
        {
            LastIncome = DateTime.UtcNow;

            int flow = NetCashFlow;

            if ((Silver + flow) < 0)
            {
                ArrayList toDelete = BuildFinanceList();

                while ((Silver + flow) < 0 && toDelete.Count > 0)
                {
                    int index = Utility.Random(toDelete.Count);
                    Mobile mob = (Mobile)toDelete[index];

                    mob.Delete();

                    toDelete.RemoveAt(index);
                    flow = NetCashFlow;
                }
            }

            Silver += flow;
        }

        public ArrayList BuildFinanceList()
        {
            ArrayList list = new ArrayList();

            List<VendorList> vendorLists = VendorLists;

            for (int i = 0; i < vendorLists.Count; ++i)
                list.AddRange(vendorLists[i].Vendors);

            List<GuardList> guardLists = GuardLists;

            for (int i = 0; i < guardLists.Count; ++i)
                list.AddRange(guardLists[i].Guards);

            return list;
        }

        private List<VendorList> m_VendorLists;
        private List<GuardList> m_GuardLists;

        public List<VendorList> VendorLists
        {
            get { return m_VendorLists; }
            set { m_VendorLists = value; }
        }

        public List<GuardList> GuardLists
        {
            get { return m_GuardLists; }
            set { m_GuardLists = value; }
        }

        public void ConstructGuardLists()
        {
            GuardDefinition[] defs = (Owner == null ? new GuardDefinition[0] : Owner.Definition.Guards);

            m_GuardLists = new List<GuardList>();

            for (int i = 0; i < defs.Length; ++i)
                m_GuardLists.Add(new GuardList(defs[i]));
        }

        public GuardList FindGuardList(Type type)
        {
            List<GuardList> guardLists = GuardLists;

            for (int i = 0; i < guardLists.Count; ++i)
            {
                GuardList guardList = guardLists[i];

                if (guardList.Definition.Type == type)
                    return guardList;
            }

            return null;
        }

        public void ConstructVendorLists()
        {
            VendorDefinition[] defs = VendorDefinition.Definitions;

            m_VendorLists = new List<VendorList>();

            for (int i = 0; i < defs.Length; ++i)
                m_VendorLists.Add(new VendorList(defs[i]));
        }

        public VendorList FindVendorList(Type type)
        {
            List<VendorList> vendorLists = VendorLists;

            for (int i = 0; i < vendorLists.Count; ++i)
            {
                VendorList vendorList = vendorLists[i];

                if (vendorList.Definition.Type == type)
                    return vendorList;
            }

            return null;
        }

        public bool RegisterGuard(BaseFactionGuard guard)
        {
            if (guard == null)
                return false;

            GuardList guardList = FindGuardList(guard.GetType());

            if (guardList == null)
                return false;

            guardList.Guards.Add(guard);
            return true;
        }

        public bool UnregisterGuard(BaseFactionGuard guard)
        {
            if (guard == null)
                return false;

            GuardList guardList = FindGuardList(guard.GetType());

            if (guardList == null)
                return false;

            if (!guardList.Guards.Contains(guard))
                return false;

            guardList.Guards.Remove(guard);
            return true;
        }

        public bool RegisterVendor(BaseFactionVendor vendor)
        {
            if (vendor == null)
                return false;

            VendorList vendorList = FindVendorList(vendor.GetType());

            if (vendorList == null)
                return false;

            vendorList.Vendors.Add(vendor);
            return true;
        }

        public bool UnregisterVendor(BaseFactionVendor vendor)
        {
            if (vendor == null)
                return false;

            VendorList vendorList = FindVendorList(vendor.GetType());

            if (vendorList == null)
                return false;

            if (!vendorList.Vendors.Contains(vendor))
                return false;

            vendorList.Vendors.Remove(vendor);
            return true;
        }

        public static void Initialize()
        {
            List<Town> towns = Towns;

            for (int i = 0; i < towns.Count; ++i)
            {
                towns[i].Sheriff = towns[i].Sheriff;
                towns[i].Finance = towns[i].Finance;
            }

            CommandSystem.Register("GrantTownSilver", AccessLevel.Administrator, new CommandEventHandler(GrantTownSilver_OnCommand));
        }

        public Town()
        {
            m_State = new TownState(this);
            ConstructVendorLists();
            ConstructGuardLists();
            StartIncomeTimer();
        }

        public bool IsSheriff(Mobile mob)
        {
            if (mob == null || mob.Deleted)
                return false;

            return (mob.AccessLevel >= AccessLevel.GameMaster || mob == Sheriff);
        }

        public bool IsFinance(Mobile mob)
        {
            if (mob == null || mob.Deleted)
                return false;

            return (mob.AccessLevel >= AccessLevel.GameMaster || mob == Finance);
        }

        public static List<Town> Towns { get { return Reflector.Towns; } }

        public const int SilverCaptureBonus = 10000;

        public void Capture(Faction f)
        {
            if (m_State.Owner == f)
                return;

            if (m_State.Owner == null) // going from unowned to owned
            {
                LastIncome = DateTime.UtcNow;
                f.Silver += SilverCaptureBonus;
            }
            else if (f == null) // going from owned to unowned
            {
                LastIncome = DateTime.MinValue;
            }
            else // otherwise changing hands, income timer doesn't change
            {
                f.Silver += SilverCaptureBonus;
            }

            m_State.Owner = f;

            Sheriff = null;
            Finance = null;

            TownMonolith monolith = this.Monolith;

            if (monolith != null)
                monolith.Faction = f;

            List<VendorList> vendorLists = VendorLists;

            for (int i = 0; i < vendorLists.Count; ++i)
            {
                VendorList vendorList = vendorLists[i];
                List<BaseFactionVendor> vendors = vendorList.Vendors;

                for (int j = vendors.Count - 1; j >= 0; --j)
                    vendors[j].Delete();
            }

            List<GuardList> guardLists = GuardLists;

            for (int i = 0; i < guardLists.Count; ++i)
            {
                GuardList guardList = guardLists[i];
                List<BaseFactionGuard> guards = guardList.Guards;

                for (int j = guards.Count - 1; j >= 0; --j)
                    guards[j].Delete();
            }

            ConstructGuardLists();
        }

        public int CompareTo(object obj)
        {
            return m_Definition.Sort - ((Town)obj).m_Definition.Sort;
        }

        public override string ToString()
        {
            return m_Definition.FriendlyName;
        }

        public static void WriteReference(GenericWriter writer, Town town)
        {
            int idx = Towns.IndexOf(town);

            writer.WriteEncodedInt((int)(idx + 1));
        }

        public static Town ReadReference(GenericReader reader)
        {
            int idx = reader.ReadEncodedInt() - 1;

            if (idx >= 0 && idx < Towns.Count)
                return Towns[idx];

            return null;
        }

        public static Town Parse(string name)
        {
            List<Town> towns = Towns;

            for (int i = 0; i < towns.Count; ++i)
            {
                Town town = towns[i];

                if (Insensitive.Equals(town.Definition.FriendlyName, name))
                    return town;
            }

            return null;
        }

        public static void GrantTownSilver_OnCommand(CommandEventArgs e)
        {
            Town town = FromRegion(e.Mobile.Region);

            if (town == null)
                e.Mobile.SendMessage("You are not in a faction town.");
            else if (e.Length == 0)
                e.Mobile.SendMessage("Format: GrantTownSilver <amount>");
            else
            {
                town.Silver += e.GetInt32(0);
                e.Mobile.SendMessage("You have granted {0:N0} silver to the town. It now has {1:N0} silver.", e.GetInt32(0), town.Silver);
            }
        }
    }

    #region Town Monoliths And Stones

    public abstract class BaseSystemController : Item
    {
        private int m_LabelNumber;

        public virtual int DefaultLabelNumber { get { return base.LabelNumber; } }
        public new virtual string DefaultName { get { return null; } }

        public override int LabelNumber
        {
            get
            {
                if (m_LabelNumber > 0)
                    return m_LabelNumber;

                return DefaultLabelNumber;
            }
        }

        public virtual void AssignName(TextDefinition name)
        {
            if (name != null && name.Number > 0)
            {
                m_LabelNumber = name.Number;
                Name = null;
            }
            else if (name != null && name.String != null)
            {
                m_LabelNumber = 0;
                Name = name.String;
            }
            else
            {
                m_LabelNumber = 0;
                Name = DefaultName;
            }

            InvalidateProperties();
        }

        public BaseSystemController(int itemID)
            : base(itemID)
        {
        }

        public BaseSystemController(Serial serial)
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

    public abstract class BaseMonolith : BaseSystemController
    {
        private Town m_Town;
        private Faction m_Faction;
        private Sigil m_Sigil;

        [CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
        public Sigil Sigil
        {
            get { return m_Sigil; }
            set
            {
                if (m_Sigil == value)
                    return;

                m_Sigil = value;

                if (m_Sigil != null && m_Sigil.LastMonolith != null && m_Sigil.LastMonolith != this && m_Sigil.LastMonolith.Sigil == m_Sigil)
                    m_Sigil.LastMonolith.Sigil = null;

                if (m_Sigil != null)
                    m_Sigil.LastMonolith = this;

                UpdateSigil();
            }
        }

        [CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
        public Town Town
        {
            get { return m_Town; }
            set
            {
                m_Town = value;
                OnTownChanged();
            }
        }

        [CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
        public Faction Faction
        {
            get { return m_Faction; }
            set
            {
                m_Faction = value;
                Hue = (m_Faction == null ? 0 : m_Faction.Definition.HuePrimary);
            }
        }

        public override void OnLocationChange(Point3D oldLocation)
        {
            base.OnLocationChange(oldLocation);
            UpdateSigil();
        }

        public override void OnMapChange()
        {
            base.OnMapChange();
            UpdateSigil();
        }

        public virtual void UpdateSigil()
        {
            if (m_Sigil == null || m_Sigil.Deleted)
                return;

            m_Sigil.MoveToWorld(new Point3D(X, Y, Z + 18), Map);
        }

        public virtual void OnTownChanged()
        {
        }

        public BaseMonolith(Town town, Faction faction)
            : base(0x1183)
        {
            Movable = false;
            Town = town;
            Faction = faction;
            m_Monoliths.Add(this);
        }

        public BaseMonolith(Serial serial)
            : base(serial)
        {
            m_Monoliths.Add(this);
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();
            m_Monoliths.Remove(this);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            Town.WriteReference(writer, m_Town);
            Faction.WriteReference(writer, m_Faction);

            writer.Write((Item)m_Sigil);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        Town = Town.ReadReference(reader);
                        Faction = Faction.ReadReference(reader);
                        m_Sigil = reader.ReadItem() as Sigil;
                        break;
                    }
            }
        }

        private static List<BaseMonolith> m_Monoliths = new List<BaseMonolith>();

        public static List<BaseMonolith> Monoliths
        {
            get { return m_Monoliths; }
            set { m_Monoliths = value; }
        }
    }

    #endregion

    public class Britain : Town
    {
        public Britain()
        {
            Definition =
                new TownDefinition(
                    0,
                    0x1869,
                    "Britain",
                    "Britain",
                    new TextDefinition(1011433, "BRITAIN"),
                    new TextDefinition(1011561, "TOWN STONE FOR BRITAIN"),
                    new TextDefinition(1041034, "The Faction Sigil Monolith of Britain"),
                    new TextDefinition(1041404, "The Faction Town Sigil Monolith of Britain"),
                    new TextDefinition(1041413, "Faction Town Stone of Britain"),
                    new TextDefinition(1041395, "Faction Town Sigil of Britain"),
                    new TextDefinition(1041386, "Corrupted Faction Town Sigil of Britain"),
                    new Point3D(1592, 1680, 10),
                    new Point3D(1588, 1676, 10));
        }
    }

    public class Magincia : Town
    {
        public Magincia()
        {
            Definition =
                new TownDefinition(
                    7,
                    0x1870,
                    "Magincia",
                    "Magincia",
                    new TextDefinition(1011440, "MAGINCIA"),
                    new TextDefinition(1011568, "TOWN STONE FOR MAGINCIA"),
                    new TextDefinition(1041041, "The Faction Sigil Monolith of Magincia"),
                    new TextDefinition(1041411, "The Faction Town Sigil Monolith of Magincia"),
                    new TextDefinition(1041420, "Faction Town Stone of Magincia"),
                    new TextDefinition(1041402, "Faction Town Sigil of Magincia"),
                    new TextDefinition(1041393, "Corrupted Faction Town Sigil of Magincia"),
                    new Point3D(3714, 2235, 20),
                    new Point3D(3712, 2230, 20));
        }
    }

    public class Minoc : Town
    {
        public Minoc()
        {
            Definition =
                new TownDefinition(
                    2,
                    0x186B,
                    "Minoc",
                    "Minoc",
                    new TextDefinition(1011437, "MINOC"),
                    new TextDefinition(1011564, "TOWN STONE FOR MINOC"),
                    new TextDefinition(1041036, "The Faction Sigil Monolith of Minoc"),
                    new TextDefinition(1041406, "The Faction Town Sigil Monolith Minoc"),
                    new TextDefinition(1041415, "Faction Town Stone of Minoc"),
                    new TextDefinition(1041397, "Faction Town Sigil of Minoc"),
                    new TextDefinition(1041388, "Corrupted Faction Town Sigil of Minoc"),
                    new Point3D(2471, 439, 15),
                    new Point3D(2469, 445, 15));
        }
    }

    public class Moonglow : Town
    {
        public Moonglow()
        {
            Definition =
                new TownDefinition(
                    3,
                    0x186C,
                    "Moonglow",
                    "Moonglow",
                    new TextDefinition(1011435, "MOONGLOW"),
                    new TextDefinition(1011563, "TOWN STONE FOR MOONGLOW"),
                    new TextDefinition(1041037, "The Faction Sigil Monolith of Moonglow"),
                    new TextDefinition(1041407, "The Faction Town Sigil Monolith of Moonglow"),
                    new TextDefinition(1041416, "Faction Town Stone of Moonglow"),
                    new TextDefinition(1041398, "Faction Town Sigil of Moonglow"),
                    new TextDefinition(1041389, "Corrupted Faction Town Sigil of Moonglow"),
                    new Point3D(4436, 1083, 0),
                    new Point3D(4432, 1086, 0));
        }
    }

    public class SkaraBrae : Town
    {
        public SkaraBrae()
        {
            Definition =
                new TownDefinition(
                    6,
                    0x186F,
                    "Skara Brae",
                    "Skara Brae",
                    new TextDefinition(1011439, "SKARA BRAE"),
                    new TextDefinition(1011567, "TOWN STONE FOR SKARA BRAE"),
                    new TextDefinition(1041040, "The Faction Sigil Monolith of Skara Brae"),
                    new TextDefinition(1041410, "The Faction Town Sigil Monolith of Skara Brae"),
                    new TextDefinition(1041419, "Faction Town Stone of Skara Brae"),
                    new TextDefinition(1041401, "Faction Town Sigil of Skara Brae"),
                    new TextDefinition(1041392, "Corrupted Faction Town Sigil of Skara Brae"),
                    new Point3D(576, 2200, 0),
                    new Point3D(572, 2196, 0));
        }
    }

    public class Trinsic : Town
    {
        public Trinsic()
        {
            Definition =
                new TownDefinition(
                    1,
                    0x186A,
                    "Trinsic",
                    "Trinsic",
                    new TextDefinition(1011434, "TRINSIC"),
                    new TextDefinition(1011562, "TOWN STONE FOR TRINSIC"),
                    new TextDefinition(1041035, "The Faction Sigil Monolith of Trinsic"),
                    new TextDefinition(1041405, "The Faction Town Sigil Monolith of Trinsic"),
                    new TextDefinition(1041414, "Faction Town Stone of Trinsic"),
                    new TextDefinition(1041396, "Faction Town Sigil of Trinsic"),
                    new TextDefinition(1041387, "Corrupted Faction Town Sigil of Trinsic"),
                    new Point3D(1914, 2717, 20),
                    new Point3D(1909, 2720, 20));
        }
    }

    public class Vesper : Town
    {
        public Vesper()
        {
            Definition =
                new TownDefinition(
                    5,
                    0x186E,
                    "Vesper",
                    "Vesper",
                    new TextDefinition(1016413, "VESPER"),
                    new TextDefinition(1011566, "TOWN STONE FOR VESPER"),
                    new TextDefinition(1041039, "The Faction Sigil Monolith of Vesper"),
                    new TextDefinition(1041409, "The Faction Town Sigil Monolith of Vesper"),
                    new TextDefinition(1041418, "Faction Town Stone of Vesper"),
                    new TextDefinition(1041400, "Faction Town Sigil of Vesper"),
                    new TextDefinition(1041391, "Corrupted Faction Town Sigil of Vesper"),
                    new Point3D(2982, 818, 0),
                    new Point3D(2985, 821, 0));
        }
    }

    public class Yew : Town
    {
        public Yew()
        {
            Definition =
                new TownDefinition(
                    4,
                    0x186D,
                    "Yew",
                    "Yew",
                    new TextDefinition(1011438, "YEW"),
                    new TextDefinition(1011565, "TOWN STONE FOR YEW"),
                    new TextDefinition(1041038, "The Faction Sigil Monolith of Yew"),
                    new TextDefinition(1041408, "The Faction Town Sigil Monolith of Yew"),
                    new TextDefinition(1041417, "Faction Town Stone of Yew"),
                    new TextDefinition(1041399, "Faction Town Sigil of Yew"),
                    new TextDefinition(1041390, "Corrupted Faction Town Sigil of Yew"),
                    new Point3D(548, 979, 0),
                    new Point3D(542, 980, 0));
        }
    }

    public class TownState
    {
        private Town m_Town;
        private Faction m_Owner;

        private Mobile m_Sheriff;
        private Mobile m_Finance;

        private int m_Silver;
        private int m_Tax;

        private DateTime m_LastTaxChange;
        private DateTime m_LastIncome;

        public Town Town
        {
            get { return m_Town; }
            set { m_Town = value; }
        }

        public Faction Owner
        {
            get { return m_Owner; }
            set { m_Owner = value; }
        }

        public Mobile Sheriff
        {
            get { return m_Sheriff; }
            set
            {
                if (m_Sheriff != null)
                {
                    PlayerState pl = PlayerState.Find(m_Sheriff);

                    if (pl != null)
                        pl.Sheriff = null;
                }

                m_Sheriff = value;

                if (m_Sheriff != null)
                {
                    PlayerState pl = PlayerState.Find(m_Sheriff);

                    if (pl != null)
                        pl.Sheriff = m_Town;
                }
            }
        }

        public Mobile Finance
        {
            get { return m_Finance; }
            set
            {
                if (m_Finance != null)
                {
                    PlayerState pl = PlayerState.Find(m_Finance);

                    if (pl != null)
                        pl.Finance = null;
                }

                m_Finance = value;

                if (m_Finance != null)
                {
                    PlayerState pl = PlayerState.Find(m_Finance);

                    if (pl != null)
                        pl.Finance = m_Town;
                }
            }
        }

        public int Silver
        {
            get { return m_Silver; }
            set { m_Silver = value; }
        }

        public int Tax
        {
            get { return m_Tax; }
            set { m_Tax = value; }
        }

        public DateTime LastTaxChange
        {
            get { return m_LastTaxChange; }
            set { m_LastTaxChange = value; }
        }

        public DateTime LastIncome
        {
            get { return m_LastIncome; }
            set { m_LastIncome = value; }
        }

        public TownState(Town town)
        {
            m_Town = town;
        }

        public TownState(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            switch (version)
            {
                case 3:
                    {
                        m_LastIncome = reader.ReadDateTime();

                        goto case 2;
                    }
                case 2:
                    {
                        m_Tax = reader.ReadEncodedInt();
                        m_LastTaxChange = reader.ReadDateTime();

                        goto case 1;
                    }
                case 1:
                    {
                        m_Silver = reader.ReadEncodedInt();

                        goto case 0;
                    }
                case 0:
                    {
                        m_Town = Town.ReadReference(reader);
                        m_Owner = Faction.ReadReference(reader);

                        m_Sheriff = reader.ReadMobile();
                        m_Finance = reader.ReadMobile();

                        m_Town.State = this;

                        break;
                    }
            }
        }

        public void Serialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)3); // version

            writer.Write((DateTime)m_LastIncome);

            writer.WriteEncodedInt((int)m_Tax);
            writer.Write((DateTime)m_LastTaxChange);

            writer.WriteEncodedInt((int)m_Silver);

            Town.WriteReference(writer, m_Town);
            Faction.WriteReference(writer, m_Owner);

            writer.Write((Mobile)m_Sheriff);
            writer.Write((Mobile)m_Finance);
        }
    }

    public class Reflector
    {
        private static List<Town> m_Towns;

        public static List<Town> Towns
        {
            get
            {
                if (m_Towns == null)
                    ProcessTypes();

                return m_Towns;
            }
        }

        private static List<Faction> m_Factions;

        public static List<Faction> Factions
        {
            get
            {
                if (m_Factions == null)
                    Reflector.ProcessTypes();

                return m_Factions;
            }
        }

        private static object Construct(Type type)
        {
            try { return Activator.CreateInstance(type); }
            catch { return null; }
        }

        private static void ProcessTypes()
        {
            m_Factions = new List<Faction>();
            m_Towns = new List<Town>();

            Assembly[] asms = ScriptCompiler.Assemblies;

            for (int i = 0; i < asms.Length; ++i)
            {
                Assembly asm = asms[i];
                TypeCache tc = ScriptCompiler.GetTypeCache(asm);
                Type[] types = tc.Types;

                for (int j = 0; j < types.Length; ++j)
                {
                    Type type = types[j];

                    if (type.IsSubclassOf(typeof(Faction)))
                    {
                        Faction faction = Construct(type) as Faction;

                        if (faction != null)
                            Faction.Factions.Add(faction);
                    }
                    else if (type.IsSubclassOf(typeof(Town)))
                    {
                        Town town = Construct(type) as Town;

                        if (town != null)
                            Town.Towns.Add(town);
                    }
                }
            }
        }
    }

    #endregion

    #region Faction Election

    public class Election
    {
        public static readonly TimeSpan PendingPeriod = TimeSpan.FromDays(5.0);
        public static readonly TimeSpan CampaignPeriod = TimeSpan.FromDays(1.0);
        public static readonly TimeSpan VotingPeriod = TimeSpan.FromDays(3.0);

        public const int MaxCandidates = 10;
        public const int CandidateRank = 5;

        private Faction m_Faction;
        private List<Candidate> m_Candidates;

        private ElectionState m_State;
        private DateTime m_LastStateTime;

        public Faction Faction { get { return m_Faction; } }

        public List<Candidate> Candidates { get { return m_Candidates; } }

        public ElectionState State { get { return m_State; } set { m_State = value; m_LastStateTime = DateTime.UtcNow; } }
        public DateTime LastStateTime { get { return m_LastStateTime; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public ElectionState CurrentState { get { return m_State; } }

        [CommandProperty(AccessLevel.GameMaster, AccessLevel.Administrator)]
        public TimeSpan NextStateTime
        {
            get
            {
                TimeSpan period;

                switch (m_State)
                {
                    default:
                    case ElectionState.Pending: period = PendingPeriod; break;
                    case ElectionState.Election: period = VotingPeriod; break;
                    case ElectionState.Campaign: period = CampaignPeriod; break;
                }

                TimeSpan until = (m_LastStateTime + period) - DateTime.UtcNow;

                if (until < TimeSpan.Zero)
                    until = TimeSpan.Zero;

                return until;
            }
            set
            {
                TimeSpan period;

                switch (m_State)
                {
                    default:
                    case ElectionState.Pending: period = PendingPeriod; break;
                    case ElectionState.Election: period = VotingPeriod; break;
                    case ElectionState.Campaign: period = CampaignPeriod; break;
                }

                m_LastStateTime = DateTime.UtcNow - period + value;
            }
        }

        private Timer m_Timer;

        public void StartTimer()
        {
            m_Timer = Timer.DelayCall(TimeSpan.FromMinutes(1.0), TimeSpan.FromMinutes(1.0), new TimerCallback(Slice));
        }

        public Election(Faction faction)
        {
            m_Faction = faction;
            m_Candidates = new List<Candidate>();

            StartTimer();
        }

        public Election(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            switch (version)
            {
                case 0:
                    {
                        m_Faction = Faction.ReadReference(reader);

                        m_LastStateTime = reader.ReadDateTime();
                        m_State = (ElectionState)reader.ReadEncodedInt();

                        m_Candidates = new List<Candidate>();

                        int count = reader.ReadEncodedInt();

                        for (int i = 0; i < count; ++i)
                        {
                            Candidate cd = new Candidate(reader);

                            if (cd.Mobile != null)
                                m_Candidates.Add(cd);
                        }

                        break;
                    }
            }

            StartTimer();
        }

        public void Serialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)0); // version

            Faction.WriteReference(writer, m_Faction);

            writer.Write((DateTime)m_LastStateTime);
            writer.WriteEncodedInt((int)m_State);

            writer.WriteEncodedInt(m_Candidates.Count);

            for (int i = 0; i < m_Candidates.Count; ++i)
                m_Candidates[i].Serialize(writer);
        }

        public void AddCandidate(Mobile mob)
        {
            if (IsCandidate(mob))
                return;

            m_Candidates.Add(new Candidate(mob));
            mob.SendLocalizedMessage(1010117); // You are now running for office.
        }

        public void RemoveVoter(Mobile mob)
        {
            if (m_State == ElectionState.Election)
            {
                for (int i = 0; i < m_Candidates.Count; ++i)
                {
                    List<Voter> voters = m_Candidates[i].Voters;

                    for (int j = 0; j < voters.Count; ++j)
                    {
                        Voter voter = voters[j];

                        if (voter.From == mob)
                            voters.RemoveAt(j--);
                    }
                }
            }
        }

        public void RemoveCandidate(Mobile mob)
        {
            Candidate cd = FindCandidate(mob);

            if (cd == null)
                return;

            m_Candidates.Remove(cd);
            mob.SendLocalizedMessage(1038031);

            if (m_State == ElectionState.Election)
            {
                if (m_Candidates.Count == 1)
                {
                    m_Faction.Broadcast(1038031); // There are no longer any valid candidates in the Faction Commander election.

                    Candidate winner = m_Candidates[0];

                    Mobile winMob = winner.Mobile;
                    PlayerState pl = PlayerState.Find(winMob);

                    if (pl == null || pl.Faction != m_Faction || winMob == m_Faction.Commander)
                    {
                        m_Faction.Broadcast(1038026); // Faction leadership has not changed.
                    }
                    else
                    {
                        m_Faction.Broadcast(1038028); // The faction has a new commander.
                        m_Faction.Commander = winMob;
                    }

                    m_Candidates.Clear();
                    State = ElectionState.Pending;
                }
                else if (m_Candidates.Count == 0) // well, I guess this'll never happen
                {
                    m_Faction.Broadcast(1038031); // There are no longer any valid candidates in the Faction Commander election.

                    m_Candidates.Clear();
                    State = ElectionState.Pending;
                }
            }
        }

        public bool IsCandidate(Mobile mob)
        {
            return (FindCandidate(mob) != null);
        }

        public bool CanVote(Mobile mob)
        {
            return (m_State == ElectionState.Election && !HasVoted(mob));
        }

        public bool HasVoted(Mobile mob)
        {
            return (FindVoter(mob) != null);
        }

        public Candidate FindCandidate(Mobile mob)
        {
            for (int i = 0; i < m_Candidates.Count; ++i)
            {
                if (m_Candidates[i].Mobile == mob)
                    return m_Candidates[i];
            }

            return null;
        }

        public Candidate FindVoter(Mobile mob)
        {
            for (int i = 0; i < m_Candidates.Count; ++i)
            {
                List<Voter> voters = m_Candidates[i].Voters;

                for (int j = 0; j < voters.Count; ++j)
                {
                    Voter voter = voters[j];

                    if (voter.From == mob)
                        return m_Candidates[i];
                }
            }

            return null;
        }

        public bool CanBeCandidate(Mobile mob)
        {
            if (IsCandidate(mob))
                return false;

            if (m_Candidates.Count >= MaxCandidates)
                return false;

            if (m_State != ElectionState.Campaign)
                return false; // sanity..

            PlayerState pl = PlayerState.Find(mob);

            return (pl != null && pl.Faction == m_Faction && pl.Rank.Rank >= CandidateRank);
        }

        public void Slice()
        {
            if (m_Faction.Election != this)
            {
                if (m_Timer != null)
                    m_Timer.Stop();

                m_Timer = null;

                return;
            }

            switch (m_State)
            {
                case ElectionState.Pending:
                    {
                        if ((m_LastStateTime + PendingPeriod) > DateTime.UtcNow)
                            break;

                        m_Faction.Broadcast(1038023); // Campaigning for the Faction Commander election has begun.

                        m_Candidates.Clear();
                        State = ElectionState.Campaign;

                        break;
                    }
                case ElectionState.Campaign:
                    {
                        if ((m_LastStateTime + CampaignPeriod) > DateTime.UtcNow)
                            break;

                        if (m_Candidates.Count == 0)
                        {
                            m_Faction.Broadcast(1038025); // Nobody ran for office.
                            State = ElectionState.Pending;
                        }
                        else if (m_Candidates.Count == 1)
                        {
                            m_Faction.Broadcast(1038029); // Only one member ran for office.

                            Candidate winner = m_Candidates[0];

                            Mobile mob = winner.Mobile;
                            PlayerState pl = PlayerState.Find(mob);

                            if (pl == null || pl.Faction != m_Faction || mob == m_Faction.Commander)
                            {
                                m_Faction.Broadcast(1038026); // Faction leadership has not changed.
                            }
                            else
                            {
                                m_Faction.Broadcast(1038028); // The faction has a new commander.
                                m_Faction.Commander = mob;
                            }

                            m_Candidates.Clear();
                            State = ElectionState.Pending;
                        }
                        else
                        {
                            m_Faction.Broadcast(1038030);
                            State = ElectionState.Election;
                        }

                        break;
                    }
                case ElectionState.Election:
                    {
                        if ((m_LastStateTime + VotingPeriod) > DateTime.UtcNow)
                            break;

                        m_Faction.Broadcast(1038024); // The results for the Faction Commander election are in

                        Candidate winner = null;

                        for (int i = 0; i < m_Candidates.Count; ++i)
                        {
                            Candidate cd = m_Candidates[i];

                            PlayerState pl = PlayerState.Find(cd.Mobile);

                            if (pl == null || pl.Faction != m_Faction)
                                continue;

                            //cd.CleanMuleVotes();

                            if (winner == null || cd.Votes > winner.Votes)
                                winner = cd;
                        }

                        if (winner == null)
                        {
                            m_Faction.Broadcast(1038026); // Faction leadership has not changed.
                        }
                        else if (winner.Mobile == m_Faction.Commander)
                        {
                            m_Faction.Broadcast(1038027); // The incumbent won the election.
                        }
                        else
                        {
                            m_Faction.Broadcast(1038028); // The faction has a new commander.
                            m_Faction.Commander = winner.Mobile;
                        }

                        m_Candidates.Clear();
                        State = ElectionState.Pending;

                        break;
                    }
            }
        }
    }

    public class ElectionGump : FactionGump
    {
        private PlayerMobile m_From;
        private Election m_Election;

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            switch (info.ButtonID)
            {
                case 0: // back
                    {
                        m_From.SendGump(new FactionStoneGump(m_From, m_Election.Faction));
                        break;
                    }
                case 1: // vote
                    {
                        if (m_Election.State == ElectionState.Election)
                            m_From.SendGump(new VoteGump(m_From, m_Election));

                        break;
                    }
                case 2: // campaign
                    {
                        if (m_Election.CanBeCandidate(m_From))
                            m_Election.AddCandidate(m_From);

                        break;
                    }
            }
        }

        public ElectionGump(PlayerMobile from, Election election)
            : base(50, 50)
        {
            m_From = from;
            m_Election = election;

            AddPage(0);

            AddBackground(0, 0, 420, 180, 5054);
            AddBackground(10, 10, 400, 160, 3000);

            AddHtmlText(20, 20, 380, 20, election.Faction.Definition.Header, false, false);

            // NOTE: Gump not entirely OSI-accurate, intentionally so

            switch (election.State)
            {
                case ElectionState.Pending:
                    {
                        TimeSpan toGo = (election.LastStateTime + Election.PendingPeriod) - DateTime.UtcNow;
                        int days = (int)(toGo.TotalDays + 0.5);

                        AddHtmlLocalized(20, 40, 380, 20, 1038034, false, false); // A new election campaign is pending

                        if (days > 0)
                        {
                            AddHtmlLocalized(20, 60, 280, 20, 1018062, false, false); // Days until next election :
                            AddLabel(300, 60, 0, days.ToString());
                        }
                        else
                        {
                            AddHtmlLocalized(20, 60, 280, 20, 1018059, false, false); // Election campaigning begins tonight.
                        }

                        break;
                    }
                case ElectionState.Campaign:
                    {
                        TimeSpan toGo = (election.LastStateTime + Election.CampaignPeriod) - DateTime.UtcNow;
                        int days = (int)(toGo.TotalDays + 0.5);

                        AddHtmlLocalized(20, 40, 380, 20, 1018058, false, false); // There is an election campaign in progress.

                        if (days > 0)
                        {
                            AddHtmlLocalized(20, 60, 280, 20, 1038033, false, false); // Days to go:
                            AddLabel(300, 60, 0, days.ToString());
                        }
                        else
                        {
                            AddHtmlLocalized(20, 60, 280, 20, 1018061, false, false); // Campaign in progress. Voting begins tonight.
                        }

                        if (m_Election.CanBeCandidate(m_From))
                        {
                            AddButton(20, 110, 4005, 4007, 2, GumpButtonType.Reply, 0);
                            AddHtmlLocalized(55, 110, 350, 20, 1011427, false, false); // CAMPAIGN FOR LEADERSHIP
                        }
                        else
                        {
                            PlayerState pl = PlayerState.Find(m_From);

                            if (pl == null || pl.Rank.Rank < Election.CandidateRank)
                                AddHtmlLocalized(20, 100, 380, 20, 1010118, false, false); // You must have a higher rank to run for office
                        }

                        break;
                    }
                case ElectionState.Election:
                    {
                        TimeSpan toGo = (election.LastStateTime + Election.VotingPeriod) - DateTime.UtcNow;
                        int days = (int)Math.Ceiling(toGo.TotalDays);

                        AddHtmlLocalized(20, 40, 380, 20, 1018060, false, false); // There is an election vote in progress.

                        AddHtmlLocalized(20, 60, 280, 20, 1038033, false, false);
                        AddLabel(300, 60, 0, days.ToString());

                        AddHtmlLocalized(55, 100, 380, 20, 1011428, false, false); // VOTE FOR LEADERSHIP
                        AddButton(20, 100, 4005, 4007, 1, GumpButtonType.Reply, 0);

                        break;
                    }
            }

            AddButton(20, 140, 4005, 4007, 0, GumpButtonType.Reply, 0);
            AddHtmlLocalized(55, 140, 350, 20, 1011012, false, false); // CANCEL
        }
    }

    public class ElectionManagementGump : Gump
    {
        public string Right(string text)
        {
            return String.Format("<DIV ALIGN=RIGHT>{0}</DIV>", text);
        }

        public string Center(string text)
        {
            return String.Format("<CENTER>{0}</CENTER>", text);
        }

        public string Color(string text, int color)
        {
            return String.Format("<BASEFONT COLOR=#{0:X6}>{1}</BASEFONT>", color, text);
        }

        public static string FormatTimeSpan(TimeSpan ts)
        {
            return String.Format("{0:D2}:{1:D2}:{2:D2}:{3:D2}", ts.Days, ts.Hours % 24, ts.Minutes % 60, ts.Seconds % 60);
        }

        public const int LabelColor = 0xFFFFFF;

        private Election m_Election;
        private Candidate m_Candidate;
        private int m_Page;

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;
            int bid = info.ButtonID;

            if (m_Candidate == null)
            {
                if (bid == 0)
                {
                }
                else if (bid == 1)
                {
                }
                else
                {
                    bid -= 2;

                    if (bid >= 0 && bid < m_Election.Candidates.Count)
                        from.SendGump(new ElectionManagementGump(m_Election, m_Election.Candidates[bid], 0));
                }
            }
            else
            {
                if (bid == 0)
                {
                    from.SendGump(new ElectionManagementGump(m_Election));
                }
                else if (bid == 1)
                {
                    m_Election.RemoveCandidate(m_Candidate.Mobile);
                    from.SendGump(new ElectionManagementGump(m_Election));
                }
                else if (bid == 2 && m_Page > 0)
                {
                    from.SendGump(new ElectionManagementGump(m_Election, m_Candidate, m_Page - 1));
                }
                else if (bid == 3 && (m_Page + 1) * 10 < m_Candidate.Voters.Count)
                {
                    from.SendGump(new ElectionManagementGump(m_Election, m_Candidate, m_Page + 1));
                }
                else
                {
                    bid -= 4;

                    if (bid >= 0 && bid < m_Candidate.Voters.Count)
                    {
                        m_Candidate.Voters.RemoveAt(bid);
                        from.SendGump(new ElectionManagementGump(m_Election, m_Candidate, m_Page));
                    }
                }
            }
        }

        public ElectionManagementGump(Election election)
            : this(election, null, 0)
        {
        }

        public ElectionManagementGump(Election election, Candidate candidate, int page)
            : base(40, 40)
        {
            m_Election = election;
            m_Candidate = candidate;
            m_Page = page;

            AddPage(0);

            if (candidate != null)
            {
                AddBackground(0, 0, 448, 354, 9270);
                AddAlphaRegion(10, 10, 428, 334);

                AddHtml(10, 10, 428, 20, Color(Center("Candidate Management"), LabelColor), false, false);

                AddHtml(45, 35, 100, 20, Color("Player Name:", LabelColor), false, false);
                AddHtml(145, 35, 100, 20, Color(candidate.Mobile == null ? "null" : candidate.Mobile.Name, LabelColor), false, false);

                AddHtml(45, 55, 100, 20, Color("Vote Count:", LabelColor), false, false);
                AddHtml(145, 55, 100, 20, Color(candidate.Votes.ToString(), LabelColor), false, false);

                AddButton(12, 73, 4005, 4007, 1, GumpButtonType.Reply, 0);
                AddHtml(45, 75, 100, 20, Color("Drop Candidate", LabelColor), false, false);

                AddImageTiled(13, 99, 422, 242, 9264);
                AddImageTiled(14, 100, 420, 240, 9274);
                AddAlphaRegion(14, 100, 420, 240);

                AddHtml(14, 100, 420, 20, Color(Center("Voters"), LabelColor), false, false);

                if (page > 0)
                    AddButton(397, 104, 0x15E3, 0x15E7, 2, GumpButtonType.Reply, 0);
                else
                    AddImage(397, 104, 0x25EA);

                if ((page + 1) * 10 < candidate.Voters.Count)
                    AddButton(414, 104, 0x15E1, 0x15E5, 3, GumpButtonType.Reply, 0);
                else
                    AddImage(414, 104, 0x25E6);


                AddHtml(14, 120, 30, 20, Color(Center("DEL"), LabelColor), false, false);
                AddHtml(47, 120, 150, 20, Color("Name", LabelColor), false, false);
                AddHtml(195, 120, 100, 20, Color(Center("Address"), LabelColor), false, false);
                AddHtml(295, 120, 80, 20, Color(Center("Time"), LabelColor), false, false);
                AddHtml(355, 120, 60, 20, Color(Center("Legit"), LabelColor), false, false);

                int idx = 0;

                for (int i = page * 10; i >= 0 && i < candidate.Voters.Count && i < (page + 1) * 10; ++i, ++idx)
                {
                    Voter voter = (Voter)candidate.Voters[i];

                    AddButton(13, 138 + (idx * 20), 4002, 4004, 4 + i, GumpButtonType.Reply, 0);

                    object[] fields = voter.AcquireFields();

                    int x = 45;

                    for (int j = 0; j < fields.Length; ++j)
                    {
                        object obj = fields[j];

                        if (obj is Mobile)
                        {
                            AddHtml(x + 2, 140 + (idx * 20), 150, 20, Color(((Mobile)obj).Name, LabelColor), false, false);
                            x += 150;
                        }
                        else if (obj is System.Net.IPAddress)
                        {
                            AddHtml(x, 140 + (idx * 20), 100, 20, Color(Center(obj.ToString()), LabelColor), false, false);
                            x += 100;
                        }
                        else if (obj is DateTime)
                        {
                            AddHtml(x, 140 + (idx * 20), 80, 20, Color(Center(FormatTimeSpan(((DateTime)obj) - election.LastStateTime)), LabelColor), false, false);
                            x += 80;
                        }
                        else if (obj is int)
                        {
                            AddHtml(x, 140 + (idx * 20), 60, 20, Color(Center((int)obj + "%"), LabelColor), false, false);
                            x += 60;
                        }
                    }
                }
            }
            else
            {
                AddBackground(0, 0, 288, 334, 9270);
                AddAlphaRegion(10, 10, 268, 314);

                AddHtml(10, 10, 268, 20, Color(Center("Election Management"), LabelColor), false, false);

                AddHtml(45, 35, 100, 20, Color("Current State:", LabelColor), false, false);
                AddHtml(145, 35, 100, 20, Color(election.State.ToString(), LabelColor), false, false);

                AddButton(12, 53, 4005, 4007, 1, GumpButtonType.Reply, 0);
                AddHtml(45, 55, 100, 20, Color("Transition Time:", LabelColor), false, false);
                AddHtml(145, 55, 100, 20, Color(FormatTimeSpan(election.NextStateTime), LabelColor), false, false);

                AddImageTiled(13, 79, 262, 242, 9264);
                AddImageTiled(14, 80, 260, 240, 9274);
                AddAlphaRegion(14, 80, 260, 240);

                AddHtml(14, 80, 260, 20, Color(Center("Candidates"), LabelColor), false, false);
                AddHtml(14, 100, 30, 20, Color(Center("-->"), LabelColor), false, false);
                AddHtml(47, 100, 150, 20, Color("Name", LabelColor), false, false);
                AddHtml(195, 100, 80, 20, Color(Center("Votes"), LabelColor), false, false);

                for (int i = 0; i < election.Candidates.Count; ++i)
                {
                    Candidate cd = election.Candidates[i];
                    Mobile mob = cd.Mobile;

                    if (mob == null)
                        continue;

                    AddButton(13, 118 + (i * 20), 4005, 4007, 2 + i, GumpButtonType.Reply, 0);
                    AddHtml(47, 120 + (i * 20), 150, 20, Color(mob.Name, LabelColor), false, false);
                    AddHtml(195, 120 + (i * 20), 80, 20, Color(Center(cd.Votes.ToString()), LabelColor), false, false);
                }
            }
        }
    }

    public class Voter
    {
        private Mobile m_From;
        private Mobile m_Candidate;

        private IPAddress m_Address;
        private DateTime m_Time;

        public Mobile From
        {
            get { return m_From; }
        }

        public Mobile Candidate
        {
            get { return m_Candidate; }
        }

        public IPAddress Address
        {
            get { return m_Address; }
        }

        public DateTime Time
        {
            get { return m_Time; }
        }

        public object[] AcquireFields()
        {
            TimeSpan gameTime = TimeSpan.Zero;

            if (m_From is PlayerMobile)
                gameTime = ((PlayerMobile)m_From).GameTime;

            int kp = 0;

            PlayerState pl = PlayerState.Find(m_From);

            if (pl != null)
                kp = pl.KillPoints;

            int sk = m_From.Skills.Total;

            int factorSkills = 50 + ((sk * 100) / 10000);
            int factorKillPts = 100 + (kp * 2);
            int factorGameTime = 50 + (int)((gameTime.Ticks * 100) / TimeSpan.TicksPerDay);

            int totalFactor = (factorSkills * factorKillPts * Math.Max(factorGameTime, 100)) / 10000;

            if (totalFactor > 100)
                totalFactor = 100;
            else if (totalFactor < 0)
                totalFactor = 0;

            return new object[] { m_From, m_Address, m_Time, totalFactor };
        }

        public Voter(Mobile from, Mobile candidate)
        {
            m_From = from;
            m_Candidate = candidate;

            if (m_From.NetState != null)
                m_Address = m_From.NetState.Address;
            else
                m_Address = IPAddress.None;

            m_Time = DateTime.UtcNow;
        }

        public Voter(GenericReader reader, Mobile candidate)
        {
            m_Candidate = candidate;

            int version = reader.ReadEncodedInt();

            switch (version)
            {
                case 0:
                    {
                        m_From = reader.ReadMobile();
                        m_Address = Utility.Intern(reader.ReadIPAddress());
                        m_Time = reader.ReadDateTime();

                        break;
                    }
            }
        }

        public void Serialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)0);

            writer.Write((Mobile)m_From);
            writer.Write((IPAddress)m_Address);
            writer.Write((DateTime)m_Time);
        }
    }

    public class VoteGump : FactionGump
    {
        private PlayerMobile m_From;
        private Election m_Election;

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 0)
            {
                m_From.SendGump(new FactionStoneGump(m_From, m_Election.Faction));
            }
            else
            {
                if (!m_Election.CanVote(m_From))
                    return;

                int index = info.ButtonID - 1;

                if (index >= 0 && index < m_Election.Candidates.Count)
                    m_Election.Candidates[index].Voters.Add(new Voter(m_From, m_Election.Candidates[index].Mobile));

                m_From.SendGump(new VoteGump(m_From, m_Election));
            }
        }

        public VoteGump(PlayerMobile from, Election election)
            : base(50, 50)
        {
            m_From = from;
            m_Election = election;

            bool canVote = election.CanVote(from);

            AddPage(0);

            AddBackground(0, 0, 420, 350, 5054);
            AddBackground(10, 10, 400, 330, 3000);

            AddHtmlText(20, 20, 380, 20, election.Faction.Definition.Header, false, false);

            if (canVote)
                AddHtmlLocalized(20, 60, 380, 20, 1011428, false, false); // VOTE FOR LEADERSHIP
            else
                AddHtmlLocalized(20, 60, 380, 20, 1038032, false, false); // You have already voted in this election.

            for (int i = 0; i < election.Candidates.Count; ++i)
            {
                Candidate cd = election.Candidates[i];

                if (canVote)
                    AddButton(20, 100 + (i * 20), 4005, 4007, i + 1, GumpButtonType.Reply, 0);

                AddLabel(55, 100 + (i * 20), 0, cd.Mobile.Name);
                AddLabel(300, 100 + (i * 20), 0, cd.Votes.ToString());
            }

            AddButton(20, 310, 4005, 4007, 0, GumpButtonType.Reply, 0);
            AddHtmlLocalized(55, 310, 100, 20, 1011012, false, false); // CANCEL
        }
    }

    public class Candidate
    {
        private Mobile m_Mobile;
        private List<Voter> m_Voters;

        public Mobile Mobile { get { return m_Mobile; } }
        public List<Voter> Voters { get { return m_Voters; } }

        public int Votes { get { return m_Voters.Count; } }

        public void CleanMuleVotes()
        {
            for (int i = 0; i < m_Voters.Count; ++i)
            {
                Voter voter = (Voter)m_Voters[i];

                if ((int)voter.AcquireFields()[3] < 90)
                    m_Voters.RemoveAt(i--);
            }
        }

        public Candidate(Mobile mob)
        {
            m_Mobile = mob;
            m_Voters = new List<Voter>();
        }

        public Candidate(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            switch (version)
            {
                case 1:
                    {
                        m_Mobile = reader.ReadMobile();

                        int count = reader.ReadEncodedInt();
                        m_Voters = new List<Voter>(count);

                        for (int i = 0; i < count; ++i)
                        {
                            Voter voter = new Voter(reader, m_Mobile);

                            if (voter.From != null)
                                m_Voters.Add(voter);
                        }

                        break;
                    }
                case 0:
                    {
                        m_Mobile = reader.ReadMobile();

                        List<Mobile> mobs = reader.ReadStrongMobileList();
                        m_Voters = new List<Voter>(mobs.Count);

                        for (int i = 0; i < mobs.Count; ++i)
                            m_Voters.Add(new Voter(mobs[i], m_Mobile));

                        break;
                    }
            }
        }

        public void Serialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)1); // version

            writer.Write((Mobile)m_Mobile);

            writer.WriteEncodedInt((int)m_Voters.Count);

            for (int i = 0; i < m_Voters.Count; ++i)
                ((Voter)m_Voters[i]).Serialize(writer);
        }
    }

    public enum ElectionState
    {
        Pending,
        Campaign,
        Election
    }

    #endregion

    #region Faction Keywords

    public class Keywords
    {
        public static void Initialize()
        {
            EventSink.Speech += new SpeechEventHandler(EventSink_Speech);
        }

        private static void ShowScore_Sandbox(object state)
        {
            PlayerState pl = (PlayerState)state;

            if (pl != null)
                pl.Mobile.PublicOverheadMessage(MessageType.Regular, pl.Mobile.SpeechHue, true, pl.KillPoints.ToString("N0")); // NOTE: Added 'N0'
        }

        private static void EventSink_Speech(SpeechEventArgs e)
        {
            Mobile from = e.Mobile;
            int[] keywords = e.Keywords;

            for (int i = 0; i < keywords.Length; ++i)
            {
                switch (keywords[i])
                {
                    case 0x00E4: // *i wish to access the city treasury*
                        {
                            Town town = Town.FromRegion(from.Region);

                            if (town == null || !town.IsFinance(from) || !from.Alive)
                                break;

                            if (FactionGump.Exists(from))
                                from.SendLocalizedMessage(1042160); // You already have a faction menu open.
                            else if (town.Owner != null && from is PlayerMobile)
                                from.SendGump(new FinanceGump((PlayerMobile)from, town.Owner, town));

                            break;
                        }
                    case 0x0ED: // *i am sheriff*
                        {
                            Town town = Town.FromRegion(from.Region);

                            if (town == null || !town.IsSheriff(from) || !from.Alive)
                                break;

                            if (FactionGump.Exists(from))
                                from.SendLocalizedMessage(1042160); // You already have a faction menu open.
                            else if (town.Owner != null)
                                from.SendGump(new SheriffGump((PlayerMobile)from, town.Owner, town));

                            break;
                        }
                    case 0x00EF: // *you are fired*
                        {
                            Town town = Town.FromRegion(from.Region);

                            if (town == null)
                                break;

                            if (town.IsFinance(from) || town.IsSheriff(from))
                                town.BeginOrderFiring(from);

                            break;
                        }
                    case 0x00E5: // *i wish to resign as finance minister*
                        {
                            PlayerState pl = PlayerState.Find(from);

                            if (pl != null && pl.Finance != null)
                            {
                                pl.Finance.Finance = null;
                                from.SendLocalizedMessage(1005081); // You have been fired as Finance Minister
                            }

                            break;
                        }
                    case 0x00EE: // *i wish to resign as sheriff*
                        {
                            PlayerState pl = PlayerState.Find(from);

                            if (pl != null && pl.Sheriff != null)
                            {
                                pl.Sheriff.Sheriff = null;
                                from.SendLocalizedMessage(1010270); // You have been fired as Sheriff
                            }

                            break;
                        }
                    case 0x00E9: // *what is my faction term status*
                        {
                            PlayerState pl = PlayerState.Find(from);

                            if (pl != null && pl.IsLeaving)
                            {
                                if (Faction.CheckLeaveTimer(from))
                                    break;

                                TimeSpan remaining = (pl.Leaving + Faction.LeavePeriod) - DateTime.UtcNow;

                                if (remaining.TotalDays >= 1)
                                    from.SendLocalizedMessage(1042743, remaining.TotalDays.ToString("N0"));// Your term of service will come to an end in ~1_DAYS~ days.
                                else if (remaining.TotalHours >= 1)
                                    from.SendLocalizedMessage(1042741, remaining.TotalHours.ToString("N0")); // Your term of service will come to an end in ~1_HOURS~ hours.
                                else
                                    from.SendLocalizedMessage(1042742); // Your term of service will come to an end in less than one hour.
                            }
                            else if (pl != null)
                            {
                                from.SendLocalizedMessage(1042233); // You are not in the process of quitting the faction.
                            }

                            break;
                        }
                    case 0x00EA: // *message faction*
                        {
                            Faction faction = Faction.Find(from);

                            if (faction == null || !faction.IsCommander(from))
                                break;

                            if (from.AccessLevel == AccessLevel.Player && !faction.FactionMessageReady)
                                from.SendLocalizedMessage(1010264); // The required time has not yet passed since the last message was sent
                            else
                                faction.BeginBroadcast(from);

                            break;
                        }
                    case 0x00EC: // *showscore*
                        {
                            PlayerState pl = PlayerState.Find(from);

                            if (pl != null)
                                Timer.DelayCall(TimeSpan.Zero, new TimerStateCallback(ShowScore_Sandbox), pl);

                            break;
                        }
                    case 0x0178: // i honor your leadership
                        {
                            Faction faction = Faction.Find(from);

                            if (faction != null)
                                faction.BeginHonorLeadership(from);

                            break;
                        }
                }
            }
        }
    }

    #endregion

    #region Faction Crafting

    public class FactionImbueGump : FactionGump
    {
        private Item m_Item;
        private Mobile m_Mobile;
        private Faction m_Faction;
        private CraftSystem m_CraftSystem;
        private BaseTool m_Tool;
        private object m_Notice;
        private int m_Quality;

        private FactionItemDefinition m_Definition;

        public FactionImbueGump(int quality, Item item, Mobile from, CraftSystem craftSystem, BaseTool tool, object notice, int availableSilver, Faction faction, FactionItemDefinition def)
            : base(100, 200)
        {
            m_Item = item;
            m_Mobile = from;
            m_Faction = faction;
            m_CraftSystem = craftSystem;
            m_Tool = tool;
            m_Notice = notice;
            m_Quality = quality;
            m_Definition = def;

            AddPage(0);

            AddBackground(0, 0, 320, 270, 5054);
            AddBackground(10, 10, 300, 250, 3000);

            AddHtmlLocalized(20, 20, 210, 25, 1011569, false, false); // Imbue with Faction properties?


            AddHtmlLocalized(20, 60, 170, 25, 1018302, false, false); // Item quality: 
            AddHtmlLocalized(175, 60, 100, 25, 1018305 - m_Quality, false, false); //	Exceptional, Average, Low

            AddHtmlLocalized(20, 80, 170, 25, 1011572, false, false); // Item Cost : 
            AddLabel(175, 80, 0x34, def.SilverCost.ToString("N0")); // NOTE: Added 'N0'

            AddHtmlLocalized(20, 100, 170, 25, 1011573, false, false); // Your Silver : 
            AddLabel(175, 100, 0x34, availableSilver.ToString("N0")); // NOTE: Added 'N0'


            AddRadio(20, 140, 210, 211, true, 1);
            AddLabel(55, 140, m_Faction.Definition.HuePrimary - 1, "*****");
            AddHtmlLocalized(150, 140, 150, 25, 1011570, false, false); // Primary Color

            AddRadio(20, 160, 210, 211, false, 2);
            AddLabel(55, 160, m_Faction.Definition.HueSecondary - 1, "*****");
            AddHtmlLocalized(150, 160, 150, 25, 1011571, false, false); // Secondary Color


            AddHtmlLocalized(55, 200, 200, 25, 1011011, false, false); // CONTINUE
            AddButton(20, 200, 4005, 4007, 1, GumpButtonType.Reply, 0);

            AddHtmlLocalized(55, 230, 200, 25, 1011012, false, false); // CANCEL
            AddButton(20, 230, 4005, 4007, 0, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 1)
            {
                Container pack = m_Mobile.Backpack;

                if (pack != null && m_Item.IsChildOf(pack))
                {
                    if (pack.ConsumeTotal(typeof(Silver), m_Definition.SilverCost))
                    {
                        int hue;

                        if (m_Item is SpellScroll)
                            hue = 0;
                        else if (info.IsSwitched(1))
                            hue = m_Faction.Definition.HuePrimary;
                        else
                            hue = m_Faction.Definition.HueSecondary;

                        FactionItem.Imbue(m_Item, m_Faction, true, hue);
                    }
                    else
                    {
                        m_Mobile.SendLocalizedMessage(1042204); // You do not have enough silver.
                    }
                }
            }

            if (m_Tool != null && !m_Tool.Deleted && m_Tool.UsesRemaining > 0)
                m_Mobile.SendGump(new CraftGump(m_Mobile, m_CraftSystem, m_Tool, m_Notice));
            else if (m_Notice is string)
                m_Mobile.SendMessage((string)m_Notice);
            else if (m_Notice is int && ((int)m_Notice) > 0)
                m_Mobile.SendLocalizedMessage((int)m_Notice);
        }
    }

    #endregion

    #region Faction MobileAI

    public enum GuardAI
    {
        Bless = 0x01, // heal, cure, +stats
        Curse = 0x02, // poison, -stats
        Melee = 0x04, // weapons
        Magic = 0x08, // damage spells
        Smart = 0x10  // smart weapons/damage spells
    }

    public class ComboEntry
    {
        private Type m_Spell;
        private TimeSpan m_Hold;
        private int m_Chance;

        public Type Spell { get { return m_Spell; } }
        public TimeSpan Hold { get { return m_Hold; } }
        public int Chance { get { return m_Chance; } }

        public ComboEntry(Type spell)
            : this(spell, 100, TimeSpan.Zero)
        {
        }

        public ComboEntry(Type spell, int chance)
            : this(spell, chance, TimeSpan.Zero)
        {
        }

        public ComboEntry(Type spell, int chance, TimeSpan hold)
        {
            m_Spell = spell;
            m_Chance = chance;
            m_Hold = hold;
        }
    }

    public class SpellCombo
    {
        private int m_Mana;
        private ComboEntry[] m_Entries;

        public int Mana { get { return m_Mana; } }
        public ComboEntry[] Entries { get { return m_Entries; } }

        public SpellCombo(int mana, params ComboEntry[] entries)
        {
            m_Mana = mana;
            m_Entries = entries;
        }

        public static readonly SpellCombo Simple = new SpellCombo(50,
            new ComboEntry(typeof(ParalyzeSpell), 20),
            new ComboEntry(typeof(ExplosionSpell), 100, TimeSpan.FromSeconds(2.8)),
            new ComboEntry(typeof(PoisonSpell), 30),
            new ComboEntry(typeof(EnergyBoltSpell))
        );

        public static readonly SpellCombo Strong = new SpellCombo(90,
            new ComboEntry(typeof(ParalyzeSpell), 20),
            new ComboEntry(typeof(ExplosionSpell), 50, TimeSpan.FromSeconds(2.8)),
            new ComboEntry(typeof(PoisonSpell), 30),
            new ComboEntry(typeof(ExplosionSpell), 100, TimeSpan.FromSeconds(2.8)),
            new ComboEntry(typeof(EnergyBoltSpell)),
            new ComboEntry(typeof(PoisonSpell), 30),
            new ComboEntry(typeof(EnergyBoltSpell))
        );

        public static Spell Process(Mobile mob, Mobile targ, ref SpellCombo combo, ref int index, ref DateTime releaseTime)
        {
            while (++index < combo.m_Entries.Length)
            {
                ComboEntry entry = combo.m_Entries[index];

                if (entry.Spell == typeof(PoisonSpell) && targ.Poisoned)
                    continue;

                if (entry.Chance > Utility.Random(100))
                {
                    releaseTime = DateTime.UtcNow + entry.Hold;
                    return (Spell)Activator.CreateInstance(entry.Spell, new object[] { mob, null });
                }
            }

            combo = null;
            index = -1;
            return null;
        }
    }

    public class FactionGuardAI : BaseAI
    {
        private BaseFactionGuard m_Guard;

        private BandageContext m_Bandage;
        private DateTime m_BandageStart;

        private SpellCombo m_Combo;
        private int m_ComboIndex = -1;
        private DateTime m_ReleaseTarget;

        private const int ManaReserve = 30;

        public bool IsAllowed(GuardAI flag)
        {
            return ((m_Guard.GuardAI & flag) == flag);
        }

        public bool IsDamaged
        {
            get { return (m_Guard.Hits < m_Guard.HitsMax); }
        }

        public bool IsPoisoned
        {
            get { return m_Guard.Poisoned; }
        }

        public TimeSpan TimeUntilBandage
        {
            get
            {
                if (m_Bandage != null && m_Bandage.Timer == null)
                    m_Bandage = null;

                if (m_Bandage == null)
                    return TimeSpan.MaxValue;

                TimeSpan ts = (m_BandageStart + m_Bandage.Timer.Delay) - DateTime.UtcNow;

                if (ts < TimeSpan.FromSeconds(-1.0))
                {
                    m_Bandage = null;
                    return TimeSpan.MaxValue;
                }

                if (ts < TimeSpan.Zero)
                    ts = TimeSpan.Zero;

                return ts;
            }
        }

        public bool DequipWeapon()
        {
            Container pack = m_Guard.Backpack;

            if (pack == null)
                return false;

            Item weapon = m_Guard.Weapon as Item;

            if (weapon != null && weapon.Parent == m_Guard && !(weapon is Fists))
            {
                pack.DropItem(weapon);
                return true;
            }

            return false;
        }

        public bool EquipWeapon()
        {
            Container pack = m_Guard.Backpack;

            if (pack == null)
                return false;

            Item weapon = pack.FindItemByType(typeof(BaseWeapon));

            if (weapon == null)
                return false;

            return m_Guard.EquipItem(weapon);
        }

        public bool StartBandage()
        {
            m_Bandage = null;

            Container pack = m_Guard.Backpack;

            if (pack == null)
                return false;

            Item bandage = pack.FindItemByType(typeof(Bandage));

            if (bandage == null)
                return false;

            m_Bandage = BandageContext.BeginHeal(m_Guard, m_Guard);
            m_BandageStart = DateTime.UtcNow;
            return (m_Bandage != null);
        }

        public bool UseItemByType(Type type)
        {
            Container pack = m_Guard.Backpack;

            if (pack == null)
                return false;

            Item item = pack.FindItemByType(type);

            if (item == null)
                return false;

            bool requip = DequipWeapon();

            item.OnDoubleClick(m_Guard);

            if (requip)
                EquipWeapon();

            return true;
        }

        public int GetStatMod(Mobile mob, StatType type)
        {
            StatMod mod = mob.GetStatMod(String.Format("[Magic] {0} Offset", type));

            if (mod == null)
                return 0;

            return mod.Offset;
        }

        public Spell RandomOffenseSpell()
        {
            int maxCircle = (int)((m_Guard.Skills.Magery.Value + 20.0) / (100.0 / 7.0));

            if (maxCircle < 1)
                maxCircle = 1;

            switch (Utility.Random(maxCircle * 2))
            {
                case 0:
                case 1: return new MagicArrowSpell(m_Guard, null);
                case 2:
                case 3: return new HarmSpell(m_Guard, null);
                case 4:
                case 5: return new FireballSpell(m_Guard, null);
                case 6:
                case 7: return new LightningSpell(m_Guard, null);
                case 8: return new MindBlastSpell(m_Guard, null);
                case 9: return new ParalyzeSpell(m_Guard, null);
                case 10: return new EnergyBoltSpell(m_Guard, null);
                case 11: return new ExplosionSpell(m_Guard, null);
                default: return new FlameStrikeSpell(m_Guard, null);
            }
        }

        public Mobile FindDispelTarget(bool activeOnly)
        {
            if (m_Mobile.Deleted || m_Mobile.Int < 95 || CanDispel(m_Mobile) || m_Mobile.AutoDispel)
                return null;

            if (activeOnly)
            {
                List<AggressorInfo> aggressed = m_Mobile.Aggressed;
                List<AggressorInfo> aggressors = m_Mobile.Aggressors;

                Mobile active = null;
                double activePrio = 0.0;

                Mobile comb = m_Mobile.Combatant;

                if (comb != null && !comb.Deleted && comb.Alive && !comb.IsDeadBondedPet && m_Mobile.InRange(comb, 12) && CanDispel(comb))
                {
                    active = comb;
                    activePrio = m_Mobile.GetDistanceToSqrt(comb);

                    if (activePrio <= 2)
                        return active;
                }

                for (int i = 0; i < aggressed.Count; ++i)
                {
                    AggressorInfo info = aggressed[i];
                    Mobile m = info.Defender;

                    if (m != comb && m.Combatant == m_Mobile && m_Mobile.InRange(m, 12) && CanDispel(m))
                    {
                        double prio = m_Mobile.GetDistanceToSqrt(m);

                        if (active == null || prio < activePrio)
                        {
                            active = m;
                            activePrio = prio;

                            if (activePrio <= 2)
                                return active;
                        }
                    }
                }

                for (int i = 0; i < aggressors.Count; ++i)
                {
                    AggressorInfo info = aggressors[i];
                    Mobile m = info.Attacker;

                    if (m != comb && m.Combatant == m_Mobile && m_Mobile.InRange(m, 12) && CanDispel(m))
                    {
                        double prio = m_Mobile.GetDistanceToSqrt(m);

                        if (active == null || prio < activePrio)
                        {
                            active = m;
                            activePrio = prio;

                            if (activePrio <= 2)
                                return active;
                        }
                    }
                }

                return active;
            }
            else
            {
                Map map = m_Mobile.Map;

                if (map != null)
                {
                    Mobile active = null, inactive = null;
                    double actPrio = 0.0, inactPrio = 0.0;

                    Mobile comb = m_Mobile.Combatant;

                    if (comb != null && !comb.Deleted && comb.Alive && !comb.IsDeadBondedPet && CanDispel(comb))
                    {
                        active = inactive = comb;
                        actPrio = inactPrio = m_Mobile.GetDistanceToSqrt(comb);
                    }

                    foreach (Mobile m in m_Mobile.GetMobilesInRange(12))
                    {
                        if (m != m_Mobile && CanDispel(m))
                        {
                            double prio = m_Mobile.GetDistanceToSqrt(m);

                            if (!activeOnly && (inactive == null || prio < inactPrio))
                            {
                                inactive = m;
                                inactPrio = prio;
                            }

                            if ((m_Mobile.Combatant == m || m.Combatant == m_Mobile) && (active == null || prio < actPrio))
                            {
                                active = m;
                                actPrio = prio;
                            }
                        }
                    }

                    return active != null ? active : inactive;
                }
            }

            return null;
        }

        public bool CanDispel(Mobile m)
        {
            return (m is BaseCreature && ((BaseCreature)m).Summoned && m_Mobile.CanBeHarmful(m, false) && !((BaseCreature)m).IsAnimatedDead);
        }

        public void RunTo(Mobile m)
        {
            /*if ( m.Paralyzed || m.Frozen )
            {
                if ( m_Mobile.InRange( m, 1 ) )
                    RunFrom( m );
                else if ( !m_Mobile.InRange( m, m_Mobile.RangeFight > 2 ? m_Mobile.RangeFight : 2 ) && !MoveTo( m, true, 1 ) )
                    OnFailedMove();
            }
            else
            {*/
            if (!m_Mobile.InRange(m, m_Mobile.RangeFight))
            {
                if (!MoveTo(m, true, 1))
                    OnFailedMove();
            }
            else if (m_Mobile.InRange(m, m_Mobile.RangeFight - 1))
            {
                RunFrom(m);
            }
            /*}*/
        }

        public void RunFrom(Mobile m)
        {
            Run((m_Mobile.GetDirectionTo(m) - 4) & Direction.Mask);
        }

        public void OnFailedMove()
        {
            /*if ( !m_Mobile.DisallowAllMoves && 20 > Utility.Random( 100 ) && IsAllowed( GuardAI.Magic ) )
            {
                if ( m_Mobile.Target != null )
                    m_Mobile.Target.Cancel( m_Mobile, TargetCancelType.Canceled );

                new TeleportSpell( m_Mobile, null ).Cast();

                m_Mobile.DebugSay( "I am stuck, I'm going to try teleporting away" );
            }
            else*/
            if (AcquireFocusMob(m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true))
            {
                if (m_Mobile.Debug)
                    m_Mobile.DebugSay("My move is blocked, so I am going to attack {0}", m_Mobile.FocusMob.Name);

                m_Mobile.Combatant = m_Mobile.FocusMob;
                Action = ActionType.Combat;
            }
            else
            {
                m_Mobile.DebugSay("I am stuck");
            }
        }

        public void Run(Direction d)
        {
            if ((m_Mobile.Spell != null && m_Mobile.Spell.IsCasting) || m_Mobile.Paralyzed || m_Mobile.Frozen || m_Mobile.DisallowAllMoves)
                return;

            m_Mobile.Direction = d | Direction.Running;

            if (!DoMove(m_Mobile.Direction, true))
                OnFailedMove();
        }

        public FactionGuardAI(BaseFactionGuard guard)
            : base(guard)
        {
            m_Guard = guard;
        }

        public override bool Think()
        {
            if (m_Mobile.Deleted)
                return false;

            Mobile combatant = m_Guard.Combatant;

            if (combatant == null || combatant.Deleted || !combatant.Alive || combatant.IsDeadBondedPet || !m_Mobile.CanSee(combatant) || !m_Mobile.CanBeHarmful(combatant, false) || combatant.Map != m_Mobile.Map)
            {
                // Our combatant is deleted, dead, hidden, or we cannot hurt them
                // Try to find another combatant

                if (AcquireFocusMob(m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true))
                {
                    m_Mobile.Combatant = combatant = m_Mobile.FocusMob;
                    m_Mobile.FocusMob = null;
                }
                else
                {
                    m_Mobile.Combatant = combatant = null;
                }
            }

            if (combatant != null && (!m_Mobile.InLOS(combatant) || !m_Mobile.InRange(combatant, 12)))
            {
                if (AcquireFocusMob(m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true))
                {
                    m_Mobile.Combatant = combatant = m_Mobile.FocusMob;
                    m_Mobile.FocusMob = null;
                }
                else if (!m_Mobile.InRange(combatant, 36))
                {
                    m_Mobile.Combatant = combatant = null;
                }
            }

            Mobile dispelTarget = FindDispelTarget(true);

            if (m_Guard.Target != null && m_ReleaseTarget == DateTime.MinValue)
                m_ReleaseTarget = DateTime.UtcNow + TimeSpan.FromSeconds(10.0);

            if (m_Guard.Target != null && DateTime.UtcNow > m_ReleaseTarget)
            {
                Target targ = m_Guard.Target;

                Mobile toHarm = (dispelTarget == null ? combatant : dispelTarget);

                if ((targ.Flags & TargetFlags.Harmful) != 0 && toHarm != null)
                {
                    if (m_Guard.Map == toHarm.Map && (targ.Range < 0 || m_Guard.InRange(toHarm, targ.Range)) && m_Guard.CanSee(toHarm) && m_Guard.InLOS(toHarm))
                        targ.Invoke(m_Guard, toHarm);
                    else if (targ is DispelSpell.InternalTarget)
                        targ.Cancel(m_Guard, TargetCancelType.Canceled);
                }
                else if ((targ.Flags & TargetFlags.Beneficial) != 0)
                {
                    targ.Invoke(m_Guard, m_Guard);
                }
                else
                {
                    targ.Cancel(m_Guard, TargetCancelType.Canceled);
                }

                m_ReleaseTarget = DateTime.MinValue;
            }

            if (dispelTarget != null)
            {
                if (Action != ActionType.Combat)
                    Action = ActionType.Combat;

                m_Guard.Warmode = true;

                RunFrom(dispelTarget);
            }
            else if (combatant != null)
            {
                if (Action != ActionType.Combat)
                    Action = ActionType.Combat;

                m_Guard.Warmode = true;

                RunTo(combatant);
            }
            else if (m_Guard.Orders.Movement != MovementType.Stand)
            {
                Mobile toFollow = null;

                if (m_Guard.Town != null && m_Guard.Orders.Movement == MovementType.Follow)
                {
                    toFollow = m_Guard.Orders.Follow;

                    if (toFollow == null)
                        toFollow = m_Guard.Town.Sheriff;
                }

                if (toFollow != null && toFollow.Map == m_Guard.Map && toFollow.InRange(m_Guard, m_Guard.RangePerception * 3) && Town.FromRegion(toFollow.Region) == m_Guard.Town)
                {
                    if (Action != ActionType.Combat)
                        Action = ActionType.Combat;

                    if (m_Mobile.CurrentSpeed != m_Mobile.ActiveSpeed)
                        m_Mobile.CurrentSpeed = m_Mobile.ActiveSpeed;

                    m_Guard.Warmode = true;

                    RunTo(toFollow);
                }
                else
                {
                    if (Action != ActionType.Wander)
                        Action = ActionType.Wander;

                    if (m_Mobile.CurrentSpeed != m_Mobile.PassiveSpeed)
                        m_Mobile.CurrentSpeed = m_Mobile.PassiveSpeed;

                    m_Guard.Warmode = false;

                    WalkRandomInHome(2, 2, 1);
                }
            }
            else
            {
                if (Action != ActionType.Wander)
                    Action = ActionType.Wander;

                m_Guard.Warmode = false;
            }

            if ((IsDamaged || IsPoisoned) && m_Guard.Skills.Healing.Base > 20.0)
            {
                TimeSpan ts = TimeUntilBandage;

                if (ts == TimeSpan.MaxValue)
                    StartBandage();
            }

            if (m_Mobile.Spell == null && Core.TickCount - m_Mobile.NextSpellTime >= 0)
            {
                Spell spell = null;

                DateTime toRelease = DateTime.MinValue;

                if (IsPoisoned)
                {
                    Poison p = m_Guard.Poison;

                    TimeSpan ts = TimeUntilBandage;

                    if (p != Poison.Lesser || ts == TimeSpan.MaxValue || TimeUntilBandage < TimeSpan.FromSeconds(1.5) || (m_Guard.HitsMax - m_Guard.Hits) > Utility.Random(250))
                    {
                        if (IsAllowed(GuardAI.Bless))
                            spell = new CureSpell(m_Guard, null);
                        else
                            UseItemByType(typeof(BaseCurePotion));
                    }
                }
                else if (IsDamaged && (m_Guard.HitsMax - m_Guard.Hits) > Utility.Random(200))
                {
                    if (IsAllowed(GuardAI.Magic) && ((m_Guard.Hits * 100) / Math.Max(m_Guard.HitsMax, 1)) < 10 && m_Guard.Home != Point3D.Zero && !Utility.InRange(m_Guard.Location, m_Guard.Home, 15) && m_Guard.Mana >= 11)
                    {
                        spell = new RecallSpell(m_Guard, null, new RunebookEntry(m_Guard.Home, m_Guard.Map, "Guard's Home", null), null);
                    }
                    else if (IsAllowed(GuardAI.Bless))
                    {
                        if (m_Guard.Mana >= 11 && (m_Guard.Hits + 30) < m_Guard.HitsMax)
                            spell = new GreaterHealSpell(m_Guard, null);
                        else if ((m_Guard.Hits + 10) < m_Guard.HitsMax && (m_Guard.Mana < 11 || (m_Guard.NextCombatTime - Core.TickCount) > 2000))
                            spell = new HealSpell(m_Guard, null);
                    }
                    else if (m_Guard.CanBeginAction(typeof(BaseHealPotion)))
                    {
                        UseItemByType(typeof(BaseHealPotion));
                    }
                }
                else if (dispelTarget != null && (IsAllowed(GuardAI.Magic) || IsAllowed(GuardAI.Bless) || IsAllowed(GuardAI.Curse)))
                {
                    if (!dispelTarget.Paralyzed && m_Guard.Mana > (ManaReserve + 20) && 40 > Utility.Random(100))
                        spell = new ParalyzeSpell(m_Guard, null);
                    else
                        spell = new DispelSpell(m_Guard, null);
                }

                if (combatant != null)
                {
                    if (m_Combo != null)
                    {
                        if (spell == null)
                        {
                            spell = SpellCombo.Process(m_Guard, combatant, ref m_Combo, ref m_ComboIndex, ref toRelease);
                        }
                        else
                        {
                            m_Combo = null;
                            m_ComboIndex = -1;
                        }
                    }
                    else if (20 > Utility.Random(100) && IsAllowed(GuardAI.Magic))
                    {
                        if (80 > Utility.Random(100))
                        {
                            m_Combo = (IsAllowed(GuardAI.Smart) ? SpellCombo.Simple : SpellCombo.Strong);
                            m_ComboIndex = -1;

                            if (m_Guard.Mana >= (ManaReserve + m_Combo.Mana))
                                spell = SpellCombo.Process(m_Guard, combatant, ref m_Combo, ref m_ComboIndex, ref toRelease);
                            else
                            {
                                m_Combo = null;

                                if (m_Guard.Mana >= (ManaReserve + 40))
                                    spell = RandomOffenseSpell();
                            }
                        }
                        else if (m_Guard.Mana >= (ManaReserve + 40))
                        {
                            spell = RandomOffenseSpell();
                        }
                    }

                    if (spell == null && 2 > Utility.Random(100) && m_Guard.Mana >= (ManaReserve + 10))
                    {
                        int strMod = GetStatMod(m_Guard, StatType.Str);
                        int dexMod = GetStatMod(m_Guard, StatType.Dex);
                        int intMod = GetStatMod(m_Guard, StatType.Int);

                        List<Type> types = new List<Type>();

                        if (strMod <= 0)
                            types.Add(typeof(StrengthSpell));

                        if (dexMod <= 0 && IsAllowed(GuardAI.Melee))
                            types.Add(typeof(AgilitySpell));

                        if (intMod <= 0 && IsAllowed(GuardAI.Magic))
                            types.Add(typeof(CunningSpell));

                        if (IsAllowed(GuardAI.Bless))
                        {
                            if (types.Count > 1)
                                spell = new BlessSpell(m_Guard, null);
                            else if (types.Count == 1)
                                spell = (Spell)Activator.CreateInstance(types[0], new object[] { m_Guard, null });
                        }
                        else if (types.Count > 0)
                        {
                            if (types[0] == typeof(StrengthSpell))
                                UseItemByType(typeof(BaseStrengthPotion));
                            else if (types[0] == typeof(AgilitySpell))
                                UseItemByType(typeof(BaseAgilityPotion));
                        }
                    }

                    if (spell == null && 2 > Utility.Random(100) && m_Guard.Mana >= (ManaReserve + 10) && IsAllowed(GuardAI.Curse))
                    {
                        if (!combatant.Poisoned && 40 > Utility.Random(100))
                        {
                            spell = new PoisonSpell(m_Guard, null);
                        }
                        else
                        {
                            int strMod = GetStatMod(combatant, StatType.Str);
                            int dexMod = GetStatMod(combatant, StatType.Dex);
                            int intMod = GetStatMod(combatant, StatType.Int);

                            List<Type> types = new List<Type>();

                            if (strMod >= 0)
                                types.Add(typeof(WeakenSpell));

                            if (dexMod >= 0 && IsAllowed(GuardAI.Melee))
                                types.Add(typeof(ClumsySpell));

                            if (intMod >= 0 && IsAllowed(GuardAI.Magic))
                                types.Add(typeof(FeeblemindSpell));

                            if (types.Count > 1)
                                spell = new CurseSpell(m_Guard, null);
                            else if (types.Count == 1)
                                spell = (Spell)Activator.CreateInstance(types[0], new object[] { m_Guard, null });
                        }
                    }
                }

                if (spell != null && (m_Guard.HitsMax - m_Guard.Hits + 10) > Utility.Random(100))
                {
                    Type type = null;

                    if (spell is GreaterHealSpell)
                        type = typeof(BaseHealPotion);
                    else if (spell is CureSpell)
                        type = typeof(BaseCurePotion);
                    else if (spell is StrengthSpell)
                        type = typeof(BaseStrengthPotion);
                    else if (spell is AgilitySpell)
                        type = typeof(BaseAgilityPotion);

                    if (type == typeof(BaseHealPotion) && !m_Guard.CanBeginAction(type))
                        type = null;

                    if (type != null && m_Guard.Target == null && UseItemByType(type))
                    {
                        if (spell is GreaterHealSpell)
                        {
                            if ((m_Guard.Hits + 30) > m_Guard.HitsMax && (m_Guard.Hits + 10) < m_Guard.HitsMax)
                                spell = new HealSpell(m_Guard, null);
                        }
                        else
                        {
                            spell = null;
                        }
                    }
                }
                else if (spell == null && m_Guard.Stam < (m_Guard.StamMax / 3) && IsAllowed(GuardAI.Melee))
                {
                    UseItemByType(typeof(BaseRefreshPotion));
                }

                if (spell == null || !spell.Cast())
                    EquipWeapon();
            }
            else if (m_Mobile.Spell is Spell && ((Spell)m_Mobile.Spell).State == SpellState.Sequencing)
            {
                EquipWeapon();
            }

            return true;
        }
    }

    #endregion

    #region Faction npcActor

    public class GuardDefinition
    {
        private Type m_Type;

        private int m_Price;
        private int m_Upkeep;
        private int m_Maximum;

        private int m_ItemID;

        private TextDefinition m_Header;
        private TextDefinition m_Label;

        public Type Type { get { return m_Type; } }

        public int Price { get { return m_Price; } }
        public int Upkeep { get { return m_Upkeep; } }
        public int Maximum { get { return m_Maximum; } }
        public int ItemID { get { return m_ItemID; } }

        public TextDefinition Header { get { return m_Header; } }
        public TextDefinition Label { get { return m_Label; } }

        public GuardDefinition(Type type, int itemID, int price, int upkeep, int maximum, TextDefinition header, TextDefinition label)
        {
            m_Type = type;

            m_Price = price;
            m_Upkeep = upkeep;
            m_Maximum = maximum;
            m_ItemID = itemID;

            m_Header = header;
            m_Label = label;
        }
    }

    public class GuardList
    {
        private GuardDefinition m_Definition;
        private List<BaseFactionGuard> m_Guards;

        public GuardDefinition Definition { get { return m_Definition; } }
        public List<BaseFactionGuard> Guards { get { return m_Guards; } }

        public BaseFactionGuard Construct()
        {
            try { return Activator.CreateInstance(m_Definition.Type) as BaseFactionGuard; }
            catch { return null; }
        }

        public GuardList(GuardDefinition definition)
        {
            m_Definition = definition;
            m_Guards = new List<BaseFactionGuard>();
        }
    }

    public abstract class BaseFactionGuard : BaseCreature
    {
        private Faction m_Faction;
        private Town m_Town;
        private Orders m_Orders;

        public override bool BardImmune { get { return true; } }

        [CommandProperty(AccessLevel.GameMaster, AccessLevel.Administrator)]
        public Faction Faction
        {
            get { return m_Faction; }
            set { Unregister(); m_Faction = value; Register(); }
        }

        public Orders Orders
        {
            get { return m_Orders; }
        }

        [CommandProperty(AccessLevel.GameMaster, AccessLevel.Administrator)]
        public Town Town
        {
            get { return m_Town; }
            set { Unregister(); m_Town = value; Register(); }
        }

        public void Register()
        {
            if (m_Town != null && m_Faction != null)
                m_Town.RegisterGuard(this);
        }

        public void Unregister()
        {
            if (m_Town != null)
                m_Town.UnregisterGuard(this);
        }

        public abstract GuardAI GuardAI { get; }

        protected override BaseAI ForcedAI
        {
            get { return new FactionGuardAI(this); }
        }

        public override TimeSpan ReacquireDelay { get { return TimeSpan.FromSeconds(2.0); } }

        public override bool IsEnemy(Mobile m)
        {
            Faction ourFaction = m_Faction;
            Faction theirFaction = Faction.Find(m);

            if (theirFaction == null && m is BaseFactionGuard)
                theirFaction = ((BaseFactionGuard)m).Faction;

            if (ourFaction != null && theirFaction != null && ourFaction != theirFaction)
            {
                ReactionType reactionType = Orders.GetReaction(theirFaction).Type;

                if (reactionType == ReactionType.Attack)
                    return true;

                if (theirFaction != null)
                {
                    List<AggressorInfo> list = m.Aggressed;

                    for (int i = 0; i < list.Count; ++i)
                    {
                        AggressorInfo ai = list[i];

                        if (ai.Defender is BaseFactionGuard)
                        {
                            BaseFactionGuard bf = (BaseFactionGuard)ai.Defender;

                            if (bf.Faction == ourFaction)
                                return true;
                        }
                    }
                }
            }

            return false;
        }

        public override void OnMovement(Mobile m, Point3D oldLocation)
        {
            if (m.Player && m.Alive && InRange(m, 10) && !InRange(oldLocation, 10) && InLOS(m) && m_Orders.GetReaction(Faction.Find(m)).Type == ReactionType.Warn)
            {
                Direction = GetDirectionTo(m);

                string warning = null;

                switch (Utility.Random(6))
                {
                    case 0: warning = "I warn you, {0}, you would do well to leave this area before someone shows you the world of gray."; break;
                    case 1: warning = "It would be wise to leave this area, {0}, lest your head become my commanders' trophy."; break;
                    case 2: warning = "You are bold, {0}, for one of the meager {1}. Leave now, lest you be taught the taste of dirt."; break;
                    case 3: warning = "Your presence here is an insult, {0}. Be gone now, knave."; break;
                    case 4: warning = "Dost thou wish to be hung by your toes, {0}? Nay? Then come no closer."; break;
                    case 5: warning = "Hey, {0}. Yeah, you. Get out of here before I beat you with a stick."; break;
                }

                Faction faction = Faction.Find(m);

                Say(warning, m.Name, faction == null ? "civilians" : faction.Definition.FriendlyName);
            }
        }

        private const int ListenRange = 12;

        public override bool HandlesOnSpeech(Mobile from)
        {
            if (InRange(from, ListenRange))
                return true;

            return base.HandlesOnSpeech(from);
        }

        private DateTime m_OrdersEnd;

        private void ChangeReaction(Faction faction, ReactionType type)
        {
            if (faction == null)
            {
                switch (type)
                {
                    case ReactionType.Ignore: Say(1005179); break; // Civilians will now be ignored.
                    case ReactionType.Warn: Say(1005180); break; // Civilians will now be warned of their impending deaths.
                    case ReactionType.Attack: return;
                }
            }
            else
            {
                TextDefinition def = null;

                switch (type)
                {
                    case ReactionType.Ignore: def = faction.Definition.GuardIgnore; break;
                    case ReactionType.Warn: def = faction.Definition.GuardWarn; break;
                    case ReactionType.Attack: def = faction.Definition.GuardAttack; break;
                }

                if (def != null && def.Number > 0)
                    Say(def.Number);
                else if (def != null && def.String != null)
                    Say(def.String);
            }

            m_Orders.SetReaction(faction, type);
        }

        private bool WasNamed(string speech)
        {
            string name = this.Name;

            return (name != null && Insensitive.StartsWith(speech, name));
        }

        public override void OnSpeech(SpeechEventArgs e)
        {
            base.OnSpeech(e);

            Mobile from = e.Mobile;

            if (!e.Handled && InRange(from, ListenRange) && from.Alive)
            {
                if (e.HasKeyword(0xE6) && (Insensitive.Equals(e.Speech, "orders") || WasNamed(e.Speech))) // *orders*
                {
                    if (m_Town == null || !m_Town.IsSheriff(from))
                    {
                        this.Say(1042189); // I don't work for you!
                    }
                    else if (Town.FromRegion(this.Region) == m_Town)
                    {
                        this.Say(1042180); // Your orders, sire?
                        m_OrdersEnd = DateTime.UtcNow + TimeSpan.FromSeconds(10.0);
                    }
                }
                else if (DateTime.UtcNow < m_OrdersEnd)
                {
                    if (m_Town != null && m_Town.IsSheriff(from) && Town.FromRegion(this.Region) == m_Town)
                    {
                        m_OrdersEnd = DateTime.UtcNow + TimeSpan.FromSeconds(10.0);

                        bool understood = true;
                        ReactionType newType = 0;

                        if (Insensitive.Contains(e.Speech, "attack"))
                            newType = ReactionType.Attack;
                        else if (Insensitive.Contains(e.Speech, "warn"))
                            newType = ReactionType.Warn;
                        else if (Insensitive.Contains(e.Speech, "ignore"))
                            newType = ReactionType.Ignore;
                        else
                            understood = false;

                        if (understood)
                        {
                            understood = false;

                            if (Insensitive.Contains(e.Speech, "civil"))
                            {
                                ChangeReaction(null, newType);
                                understood = true;
                            }

                            List<Faction> factions = Faction.Factions;

                            for (int i = 0; i < factions.Count; ++i)
                            {
                                Faction faction = factions[i];

                                if (faction != m_Faction && Insensitive.Contains(e.Speech, faction.Definition.Keyword))
                                {
                                    ChangeReaction(faction, newType);
                                    understood = true;
                                }
                            }
                        }
                        else if (Insensitive.Contains(e.Speech, "patrol"))
                        {
                            Home = Location;
                            RangeHome = 6;
                            Combatant = null;
                            m_Orders.Movement = MovementType.Patrol;
                            Say(1005146); // This spot looks like it needs protection!  I shall guard it with my life.
                            understood = true;
                        }
                        else if (Insensitive.Contains(e.Speech, "follow"))
                        {
                            Home = Location;
                            RangeHome = 6;
                            Combatant = null;
                            m_Orders.Follow = from;
                            m_Orders.Movement = MovementType.Follow;
                            Say(1005144); // Yes, Sire.
                            understood = true;
                        }

                        if (!understood)
                            Say(1042183); // I'm sorry, I don't understand your orders...
                    }
                }
            }
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (m_Faction != null && Map == Faction.Facet)
                list.Add(1060846, m_Faction.Definition.PropName); // Guard: ~1_val~
        }

        public override void OnSingleClick(Mobile from)
        {
            if (m_Faction != null && Map == Faction.Facet)
            {
                string text = String.Concat("(Guard, ", m_Faction.Definition.FriendlyName, ")");

                int hue = (Faction.Find(from) == m_Faction ? 98 : 38);

                PrivateOverheadMessage(MessageType.Label, hue, true, text, from.NetState);
            }

            base.OnSingleClick(from);
        }

        public virtual void GenerateRandomHair()
        {
            Utility.AssignRandomHair(this);
            Utility.AssignRandomFacialHair(this, HairHue);
        }

        private static Type[] m_StrongPotions = new Type[]
		{
			typeof( GreaterHealPotion ), typeof( GreaterHealPotion ), typeof( GreaterHealPotion ),
			typeof( GreaterCurePotion ), typeof( GreaterCurePotion ), typeof( GreaterCurePotion ),
			typeof( GreaterStrengthPotion ), typeof( GreaterStrengthPotion ),
			typeof( GreaterAgilityPotion ), typeof( GreaterAgilityPotion ),
			typeof( TotalRefreshPotion ), typeof( TotalRefreshPotion ),
			typeof( GreaterExplosionPotion )
		};

        private static Type[] m_WeakPotions = new Type[]
		{
			typeof( HealPotion ), typeof( HealPotion ), typeof( HealPotion ),
			typeof( CurePotion ), typeof( CurePotion ), typeof( CurePotion ),
			typeof( StrengthPotion ), typeof( StrengthPotion ),
			typeof( AgilityPotion ), typeof( AgilityPotion ),
			typeof( RefreshPotion ), typeof( RefreshPotion ),
			typeof( ExplosionPotion )
		};

        public void PackStrongPotions(int min, int max)
        {
            PackStrongPotions(Utility.RandomMinMax(min, max));
        }

        public void PackStrongPotions(int count)
        {
            for (int i = 0; i < count; ++i)
                PackStrongPotion();
        }

        public void PackStrongPotion()
        {
            PackItem(Loot.Construct(m_StrongPotions));
        }

        public void PackWeakPotions(int min, int max)
        {
            PackWeakPotions(Utility.RandomMinMax(min, max));
        }

        public void PackWeakPotions(int count)
        {
            for (int i = 0; i < count; ++i)
                PackWeakPotion();
        }

        public void PackWeakPotion()
        {
            PackItem(Loot.Construct(m_WeakPotions));
        }

        public Item Immovable(Item item)
        {
            item.Movable = false;
            return item;
        }

        public Item Newbied(Item item)
        {
            item.LootType = LootType.Newbied;
            return item;
        }

        public Item Rehued(Item item, int hue)
        {
            item.Hue = hue;
            return item;
        }

        public Item Layered(Item item, Layer layer)
        {
            item.Layer = layer;
            return item;
        }

        public Item Resourced(BaseWeapon weapon, CraftResource resource)
        {
            weapon.Resource = resource;
            return weapon;
        }

        public Item Resourced(BaseArmor armor, CraftResource resource)
        {
            armor.Resource = resource;
            return armor;
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();
            Unregister();
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            c.Delete();
        }

        public virtual void GenerateBody(bool isFemale, bool randomHair)
        {
            Hue = Utility.RandomSkinHue();

            if (isFemale)
            {
                Female = true;
                Body = 401;
                Name = NameList.RandomName("female");
            }
            else
            {
                Female = false;
                Body = 400;
                Name = NameList.RandomName("male");
            }

            if (randomHair)
                GenerateRandomHair();
        }

        public override bool ClickTitle { get { return false; } }

        public BaseFactionGuard(string title)
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            m_Orders = new Orders(this);
            Title = title;

            RangeHome = 6;
        }

        public BaseFactionGuard(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            Faction.WriteReference(writer, m_Faction);
            Town.WriteReference(writer, m_Town);

            m_Orders.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            m_Faction = Faction.ReadReference(reader);
            m_Town = Town.ReadReference(reader);
            m_Orders = new Orders(this, reader);

            Timer.DelayCall(TimeSpan.Zero, new TimerCallback(Register));
        }
    }

    public class VirtualMount : IMount
    {
        private VirtualMountItem m_Item;

        public Mobile Rider
        {
            get { return m_Item.Rider; }
            set { }
        }

        public VirtualMount(VirtualMountItem item)
        {
            m_Item = item;
        }

        public virtual void OnRiderDamaged(int amount, Mobile from, bool willKill)
        {
        }
    }

    public class VirtualMountItem : Item, IMountItem
    {
        private Mobile m_Rider;
        private VirtualMount m_Mount;

        public Mobile Rider { get { return m_Rider; } }

        public VirtualMountItem(Mobile mob)
            : base(0x3EA0)
        {
            Layer = Layer.Mount;

            m_Rider = mob;
            m_Mount = new VirtualMount(this);
        }

        public IMount Mount
        {
            get { return m_Mount; }
        }

        public VirtualMountItem(Serial serial)
            : base(serial)
        {
            m_Mount = new VirtualMount(this);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write((Mobile)m_Rider);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            m_Rider = reader.ReadMobile();

            if (m_Rider == null)
                Delete();
        }
    }

    public class SheriffGump : FactionGump
    {
        private PlayerMobile m_From;
        private Faction m_Faction;
        private Town m_Town;

        private void CenterItem(int itemID, int x, int y, int w, int h)
        {
            Rectangle2D rc = ItemBounds.Table[itemID];
            AddItem(x + ((w - rc.Width) / 2) - rc.X, y + ((h - rc.Height) / 2) - rc.Y, itemID);
        }

        public SheriffGump(PlayerMobile from, Faction faction, Town town)
            : base(50, 50)
        {
            m_From = from;
            m_Faction = faction;
            m_Town = town;


            AddPage(0);

            AddBackground(0, 0, 320, 410, 5054);
            AddBackground(10, 10, 300, 390, 3000);

            #region General
            AddPage(1);

            AddHtmlLocalized(20, 30, 260, 25, 1011431, false, false); // Sheriff

            AddHtmlLocalized(55, 90, 200, 25, 1011494, false, false); // HIRE GUARDS
            AddButton(20, 90, 4005, 4007, 0, GumpButtonType.Page, 3);

            AddHtmlLocalized(55, 120, 200, 25, 1011495, false, false); // VIEW FINANCES
            AddButton(20, 120, 4005, 4007, 0, GumpButtonType.Page, 2);

            AddHtmlLocalized(55, 360, 200, 25, 1011441, false, false); // Exit
            AddButton(20, 360, 4005, 4007, 0, GumpButtonType.Reply, 0);
            #endregion

            #region Finances
            AddPage(2);

            int financeUpkeep = town.FinanceUpkeep;
            int sheriffUpkeep = town.SheriffUpkeep;
            int dailyIncome = town.DailyIncome;
            int netCashFlow = town.NetCashFlow;

            AddHtmlLocalized(20, 30, 300, 25, 1011524, false, false); // FINANCE STATEMENT

            AddHtmlLocalized(20, 80, 300, 25, 1011538, false, false); // Current total money for town : 
            AddLabel(20, 100, 0x44, town.Silver.ToString("N0")); // NOTE: Added 'N0'

            AddHtmlLocalized(20, 130, 300, 25, 1011520, false, false); // Finance Minister Upkeep : 
            AddLabel(20, 150, 0x44, financeUpkeep.ToString("N0")); // NOTE: Added 'N0'

            AddHtmlLocalized(20, 180, 300, 25, 1011521, false, false); // Sheriff Upkeep : 
            AddLabel(20, 200, 0x44, sheriffUpkeep.ToString("N0")); // NOTE: Added 'N0'

            AddHtmlLocalized(20, 230, 300, 25, 1011522, false, false); // Town Income : 
            AddLabel(20, 250, 0x44, dailyIncome.ToString("N0")); // NOTE: Added 'N0'

            AddHtmlLocalized(20, 280, 300, 25, 1011523, false, false); // Net Cash flow per day : 
            AddLabel(20, 300, 0x44, netCashFlow.ToString("N0")); // NOTE: Added 'N0'

            AddHtmlLocalized(55, 360, 200, 25, 1011067, false, false); // Previous page
            AddButton(20, 360, 4005, 4007, 0, GumpButtonType.Page, 1);
            #endregion

            #region Hire Guards
            AddPage(3);

            AddHtmlLocalized(20, 30, 300, 25, 1011494, false, false); // HIRE GUARDS

            List<GuardList> guardLists = town.GuardLists;

            for (int i = 0; i < guardLists.Count; ++i)
            {
                GuardList guardList = guardLists[i];
                int y = 90 + (i * 60);

                AddButton(20, y, 4005, 4007, 0, GumpButtonType.Page, 4 + i);
                CenterItem(guardList.Definition.ItemID, 50, y - 20, 70, 60);
                AddHtmlText(120, y, 200, 25, guardList.Definition.Header, false, false);
            }

            AddHtmlLocalized(55, 360, 200, 25, 1011067, false, false); // Previous page
            AddButton(20, 360, 4005, 4007, 0, GumpButtonType.Page, 1);
            #endregion

            #region Guard Pages
            for (int i = 0; i < guardLists.Count; ++i)
            {
                GuardList guardList = guardLists[i];

                AddPage(4 + i);

                AddHtmlText(90, 30, 300, 25, guardList.Definition.Header, false, false);
                CenterItem(guardList.Definition.ItemID, 10, 10, 80, 80);

                AddHtmlLocalized(20, 90, 200, 25, 1011514, false, false); // You have : 
                AddLabel(230, 90, 0x26, guardList.Guards.Count.ToString());

                AddHtmlLocalized(20, 120, 200, 25, 1011515, false, false); // Maximum : 
                AddLabel(230, 120, 0x12A, guardList.Definition.Maximum.ToString());

                AddHtmlLocalized(20, 150, 200, 25, 1011516, false, false); // Cost : 
                AddLabel(230, 150, 0x44, guardList.Definition.Price.ToString("N0")); // NOTE: Added 'N0'

                AddHtmlLocalized(20, 180, 200, 25, 1011517, false, false); // Daily Pay :
                AddLabel(230, 180, 0x37, guardList.Definition.Upkeep.ToString("N0")); // NOTE: Added 'N0'

                AddHtmlLocalized(20, 210, 200, 25, 1011518, false, false); // Current Silver : 
                AddLabel(230, 210, 0x44, town.Silver.ToString("N0")); // NOTE: Added 'N0'

                AddHtmlLocalized(20, 240, 200, 25, 1011519, false, false); // Current Payroll : 
                AddLabel(230, 240, 0x44, sheriffUpkeep.ToString("N0")); // NOTE: Added 'N0'

                AddHtmlText(55, 300, 200, 25, guardList.Definition.Label, false, false);
                AddButton(20, 300, 4005, 4007, 1 + i, GumpButtonType.Reply, 0);

                AddHtmlLocalized(55, 360, 200, 25, 1011067, false, false); // Previous page
                AddButton(20, 360, 4005, 4007, 0, GumpButtonType.Page, 3);
            }
            #endregion
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (!m_Town.IsSheriff(m_From) || m_Town.Owner != m_Faction)
            {
                m_From.SendLocalizedMessage(1010339); // You no longer control this city
                return;
            }

            int index = info.ButtonID - 1;

            if (index >= 0 && index < m_Town.GuardLists.Count)
            {
                GuardList guardList = m_Town.GuardLists[index];
                Town town = Town.FromRegion(m_From.Region);

                if (Town.FromRegion(m_From.Region) != m_Town)
                {
                    m_From.SendLocalizedMessage(1010305); // You must be in your controlled city to buy Items
                }
                else if (guardList.Guards.Count >= guardList.Definition.Maximum)
                {
                    m_From.SendLocalizedMessage(1010306); // You currently have too many of this enhancement type to place another
                }
                else if (BaseBoat.FindBoatAt(m_From.Location, m_From.Map) != null)
                {
                    m_From.SendMessage("You cannot place a guard here");
                }
                else if (m_Town.Silver >= guardList.Definition.Price)
                {
                    BaseFactionGuard guard = guardList.Construct();

                    if (guard != null)
                    {
                        guard.Faction = m_Faction;
                        guard.Town = m_Town;

                        m_Town.Silver -= guardList.Definition.Price;

                        guard.MoveToWorld(m_From.Location, m_From.Map);
                        guard.Home = guard.Location;
                    }
                }
            }
        }
    }

    public class VendorDefinition
    {
        private Type m_Type;

        private int m_Price;
        private int m_Upkeep;
        private int m_Maximum;

        private int m_ItemID;

        private TextDefinition m_Header;
        private TextDefinition m_Label;

        public Type Type { get { return m_Type; } }

        public int Price { get { return m_Price; } }
        public int Upkeep { get { return m_Upkeep; } }
        public int Maximum { get { return m_Maximum; } }
        public int ItemID { get { return m_ItemID; } }

        public TextDefinition Header { get { return m_Header; } }
        public TextDefinition Label { get { return m_Label; } }

        public VendorDefinition(Type type, int itemID, int price, int upkeep, int maximum, TextDefinition header, TextDefinition label)
        {
            m_Type = type;

            m_Price = price;
            m_Upkeep = upkeep;
            m_Maximum = maximum;
            m_ItemID = itemID;

            m_Header = header;
            m_Label = label;
        }

        private static VendorDefinition[] m_Definitions = new VendorDefinition[]
			{
				new VendorDefinition( typeof( FactionBottleVendor ), 0xF0E,
					5000,
					1000,
					10,
					new TextDefinition( 1011549, "POTION BOTTLE VENDOR" ),
					new TextDefinition( 1011544, "Buy Potion Bottle Vendor" )
				),
				new VendorDefinition( typeof( FactionBoardVendor ), 0x1BD7,
					3000,
					500,
					10,
					new TextDefinition( 1011552, "WOOD VENDOR" ),
					new TextDefinition( 1011545, "Buy Wooden Board Vendor" )
				),
				new VendorDefinition( typeof( FactionOreVendor ), 0x19B8,
					3000,
					500,
					10,
					new TextDefinition( 1011553, "IRON ORE VENDOR" ),
					new TextDefinition( 1011546, "Buy Iron Ore Vendor" )
				),
				new VendorDefinition( typeof( FactionReagentVendor ), 0xF86,
					5000,
					1000,
					10,
					new TextDefinition( 1011554, "REAGENT VENDOR" ),
					new TextDefinition( 1011547, "Buy Reagent Vendor" )
				),
				new VendorDefinition( typeof( FactionHorseVendor ), 0x20DD,
					5000,
					1000,
					1,
					new TextDefinition( 1011556, "HORSE BREEDER" ),
					new TextDefinition( 1011555, "Buy Horse Breeder" )
				)
			};

        public static VendorDefinition[] Definitions { get { return m_Definitions; } }
    }

    public class VendorList
    {
        private VendorDefinition m_Definition;
        private List<BaseFactionVendor> m_Vendors;

        public VendorDefinition Definition { get { return m_Definition; } }
        public List<BaseFactionVendor> Vendors { get { return m_Vendors; } }

        public BaseFactionVendor Construct(Town town, Faction faction)
        {
            try { return Activator.CreateInstance(m_Definition.Type, new object[] { town, faction }) as BaseFactionVendor; }
            catch { return null; }
        }

        public VendorList(VendorDefinition definition)
        {
            m_Definition = definition;
            m_Vendors = new List<BaseFactionVendor>();
        }
    }

    public abstract class BaseFactionVendor : BaseVendor
    {
        private Town m_Town;
        private Faction m_Faction;

        [CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
        public Town Town
        {
            get { return m_Town; }
            set { Unregister(); m_Town = value; Register(); }
        }

        [CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
        public Faction Faction
        {
            get { return m_Faction; }
            set { Unregister(); m_Faction = value; Register(); }
        }

        public void Register()
        {
            if (m_Town != null && m_Faction != null)
                m_Town.RegisterVendor(this);
        }

        public override bool OnMoveOver(Mobile m)
        {
            if (Core.ML)
                return true;

            return base.OnMoveOver(m);
        }

        public void Unregister()
        {
            if (m_Town != null)
                m_Town.UnregisterVendor(this);
        }

        private List<SBInfo> m_SBInfos = new List<SBInfo>();
        protected override List<SBInfo> SBInfos { get { return m_SBInfos; } }

        public override void InitSBInfo()
        {
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            Unregister();
        }

        public override bool CheckVendorAccess(Mobile from)
        {
            return true;
        }

        public BaseFactionVendor(Town town, Faction faction, string title)
            : base(title)
        {
            Frozen = true;
            CantWalk = true;
            Female = false;
            BodyValue = 400;
            Name = NameList.RandomName("male");

            RangeHome = 0;

            m_Town = town;
            m_Faction = faction;
            Register();
        }

        public BaseFactionVendor(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            Town.WriteReference(writer, m_Town);
            Faction.WriteReference(writer, m_Faction);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_Town = Town.ReadReference(reader);
                        m_Faction = Faction.ReadReference(reader);
                        Register();
                        break;
                    }
            }

            Frozen = true;
        }
    }

    #endregion

    #region Faction Creature

    [CorpseName("a war horse corpse")]
    public class FactionWarHorse : BaseMount
    {
        private Faction m_Faction;

        [CommandProperty(AccessLevel.GameMaster, AccessLevel.Administrator)]
        public Faction Faction
        {
            get { return m_Faction; }
            set
            {
                m_Faction = value;

                Body = (m_Faction == null ? 0xE2 : m_Faction.Definition.WarHorseBody);
                ItemID = (m_Faction == null ? 0x3EA0 : m_Faction.Definition.WarHorseItem);
            }
        }

        public const int SilverPrice = 500;
        public const int GoldPrice = 3000;

        [Constructable]
        public FactionWarHorse()
            : this(null)
        {
        }

        public FactionWarHorse(Faction faction)
            : base("a war horse", 0xE2, 0x3EA0, AIType.AI_Melee, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            BaseSoundID = 0xA8;

            SetStr(400);
            SetDex(125);
            SetInt(51, 55);

            SetHits(240);
            SetMana(0);

            SetDamage(5, 8);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 40, 50);
            SetResistance(ResistanceType.Fire, 30, 40);
            SetResistance(ResistanceType.Cold, 30, 40);
            SetResistance(ResistanceType.Poison, 30, 40);
            SetResistance(ResistanceType.Energy, 30, 40);

            SetSkill(SkillName.MagicResist, 25.1, 30.0);
            SetSkill(SkillName.Tactics, 29.3, 44.0);
            SetSkill(SkillName.Wrestling, 29.3, 44.0);

            Fame = 300;
            Karma = 300;

            Tamable = true;
            ControlSlots = 1;

            Faction = faction;
        }

        public override FoodType FavoriteFood { get { return FoodType.FruitsAndVegies | FoodType.GrainsAndHay; } }

        public FactionWarHorse(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            PlayerState pl = PlayerState.Find(from);

            if (pl == null)
                from.SendLocalizedMessage(1010366); // You cannot mount a faction war horse!
            else if (pl.Faction != this.Faction)
                from.SendLocalizedMessage(1010367); // You cannot ride an opposing faction's war horse!
            else if (pl.Rank.Rank < 2)
                from.SendLocalizedMessage(1010368); // You must achieve a faction rank of at least two before riding a war horse!
            else
                base.OnDoubleClick(from);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            Faction.WriteReference(writer, m_Faction);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        Faction = Faction.ReadReference(reader);
                        break;
                    }
            }
        }
    }

    public class HorseBreederGump : FactionGump
    {
        private PlayerMobile m_From;
        private Faction m_Faction;

        public HorseBreederGump(PlayerMobile from, Faction faction)
            : base(20, 30)
        {
            m_From = from;
            m_Faction = faction;

            AddPage(0);

            AddBackground(0, 0, 320, 280, 5054);
            AddBackground(10, 10, 300, 260, 3000);

            AddHtmlText(20, 30, 300, 25, faction.Definition.Header, false, false);

            AddHtmlLocalized(20, 60, 300, 25, 1018306, false, false); // Purchase a Faction War Horse
            AddItem(70, 120, 0x3FFE);

            AddItem(150, 120, 0xEF2);
            AddLabel(190, 122, 0x3E3, FactionWarHorse.SilverPrice.ToString("N0")); // NOTE: Added 'N0'

            AddItem(150, 150, 0xEEF);
            AddLabel(190, 152, 0x3E3, FactionWarHorse.GoldPrice.ToString("N0")); // NOTE: Added 'N0'

            AddHtmlLocalized(55, 210, 200, 25, 1011011, false, false); // CONTINUE
            AddButton(20, 210, 4005, 4007, 1, GumpButtonType.Reply, 0);

            AddHtmlLocalized(55, 240, 200, 25, 1011012, false, false); // CANCEL
            AddButton(20, 240, 4005, 4007, 0, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID != 1)
                return;

            if (Faction.Find(m_From) != m_Faction)
                return;

            Container pack = m_From.Backpack;

            if (pack == null)
                return;

            FactionWarHorse horse = new FactionWarHorse(m_Faction);

            if ((m_From.Followers + horse.ControlSlots) > m_From.FollowersMax)
            {
                // TODO: Message?
                horse.Delete();
            }
            else
            {
                if (pack.GetAmount(typeof(Silver)) < FactionWarHorse.SilverPrice)
                {
                    sender.Mobile.SendLocalizedMessage(1042204); // You do not have enough silver.
                    horse.Delete();
                }
                else if (pack.GetAmount(typeof(Gold)) < FactionWarHorse.GoldPrice)
                {
                    sender.Mobile.SendLocalizedMessage(1042205); // You do not have enough gold.
                    horse.Delete();
                }
                else if (pack.ConsumeTotal(typeof(Silver), FactionWarHorse.SilverPrice) && pack.ConsumeTotal(typeof(Gold), FactionWarHorse.GoldPrice))
                {
                    horse.Controlled = true;
                    horse.ControlMaster = m_From;

                    horse.ControlOrder = OrderType.Follow;
                    horse.ControlTarget = m_From;

                    horse.MoveToWorld(m_From.Location, m_From.Map);
                }
                else
                {
                    horse.Delete();
                }
            }
        }
    }

    #endregion

    #region Occupation Title

    public enum MerchantTitle
    {
        None,
        Scribe,
        Carpenter,
        Blacksmith,
        Bowyer,
        Tialor
    }

    public class MerchantTitleInfo
    {
        private SkillName m_Skill;
        private double m_Requirement;
        private TextDefinition m_Title;
        private TextDefinition m_Label;
        private TextDefinition m_Assigned;

        public SkillName Skill { get { return m_Skill; } }
        public double Requirement { get { return m_Requirement; } }
        public TextDefinition Title { get { return m_Title; } }
        public TextDefinition Label { get { return m_Label; } }
        public TextDefinition Assigned { get { return m_Assigned; } }

        public MerchantTitleInfo(SkillName skill, double requirement, TextDefinition title, TextDefinition label, TextDefinition assigned)
        {
            m_Skill = skill;
            m_Requirement = requirement;
            m_Title = title;
            m_Label = label;
            m_Assigned = assigned;
        }
    }

    public class MerchantTitles
    {
        private static MerchantTitleInfo[] m_Info = new MerchantTitleInfo[]
			{
				new MerchantTitleInfo( SkillName.Inscribe,		90.0,	new TextDefinition( 1060773, "Scribe" ),		new TextDefinition( 1011468, "SCRIBE" ),		new TextDefinition( 1010121, "You now have the faction title of scribe" ) ),
				new MerchantTitleInfo( SkillName.Carpentry,		90.0,	new TextDefinition( 1060774, "Carpenter" ),		new TextDefinition( 1011469, "CARPENTER" ),		new TextDefinition( 1010122, "You now have the faction title of carpenter" ) ),
				new MerchantTitleInfo( SkillName.Tinkering,		90.0,	new TextDefinition( 1022984, "Tinker" ),		new TextDefinition( 1011470, "TINKER" ),		new TextDefinition( 1010123, "You now have the faction title of tinker" ) ),
				new MerchantTitleInfo( SkillName.Blacksmith,	90.0,	new TextDefinition( 1023016, "Blacksmith" ),	new TextDefinition( 1011471, "BLACKSMITH" ),	new TextDefinition( 1010124, "You now have the faction title of blacksmith" ) ),
				new MerchantTitleInfo( SkillName.Fletching,		90.0,	new TextDefinition( 1023022, "Bowyer" ),		new TextDefinition( 1011472, "BOWYER" ),		new TextDefinition( 1010125, "You now have the faction title of Bowyer" ) ),
				new MerchantTitleInfo( SkillName.Tailoring,		90.0,	new TextDefinition( 1022982, "Tailor" ),		new TextDefinition( 1018300, "TAILOR" ),		new TextDefinition( 1042162, "You now have the faction title of Tailor" ) ),
			};

        public static MerchantTitleInfo[] Info { get { return m_Info; } }

        public static MerchantTitleInfo GetInfo(MerchantTitle title)
        {
            int idx = (int)title - 1;

            if (idx >= 0 && idx < m_Info.Length)
                return m_Info[idx];

            return null;
        }

        public static bool HasMerchantQualifications(Mobile mob)
        {
            for (int i = 0; i < m_Info.Length; ++i)
            {
                if (IsQualified(mob, m_Info[i]))
                    return true;
            }

            return false;
        }

        public static bool IsQualified(Mobile mob, MerchantTitle title)
        {
            return IsQualified(mob, GetInfo(title));
        }

        public static bool IsQualified(Mobile mob, MerchantTitleInfo info)
        {
            if (mob == null || info == null)
                return false;

            return (mob.Skills[info.Skill].Value >= info.Requirement);
        }
    }

    #endregion

    #region IFaction Objects

    public class FactionItemDefinition
    {
        private int m_SilverCost;
        private Type m_VendorType;

        public int SilverCost { get { return m_SilverCost; } }
        public Type VendorType { get { return m_VendorType; } }

        public FactionItemDefinition(int silverCost, Type vendorType)
        {
            m_SilverCost = silverCost;
            m_VendorType = vendorType;
        }

        private static FactionItemDefinition m_MetalArmor = new FactionItemDefinition(1000, typeof(Blacksmith));
        private static FactionItemDefinition m_Weapon = new FactionItemDefinition(1000, typeof(Blacksmith));
        private static FactionItemDefinition m_RangedWeapon = new FactionItemDefinition(1000, typeof(Bowyer));
        private static FactionItemDefinition m_LeatherArmor = new FactionItemDefinition(750, typeof(Tailor));
        private static FactionItemDefinition m_Clothing = new FactionItemDefinition(200, typeof(Tailor));
        private static FactionItemDefinition m_Scroll = new FactionItemDefinition(500, typeof(Mage));

        public static FactionItemDefinition Identify(Item item)
        {
            if (item is BaseArmor)
            {
                if (CraftResources.GetType(((BaseArmor)item).Resource) == CraftResourceType.Leather)
                    return m_LeatherArmor;

                return m_MetalArmor;
            }

            if (item is BaseRanged)
                return m_RangedWeapon;
            else if (item is BaseWeapon)
                return m_Weapon;
            else if (item is BaseClothing)
                return m_Clothing;
            else if (item is SpellScroll)
                return m_Scroll;

            return null;
        }
    }

    public interface IFactionItem
    {
        FactionItem FactionItemState { get; set; }
    }

    public class FactionItem
    {
        public static readonly TimeSpan ExpirationPeriod = TimeSpan.FromDays(21.0);

        private Item m_Item;
        private Faction m_Faction;
        private DateTime m_Expiration;

        public Item Item { get { return m_Item; } }
        public Faction Faction { get { return m_Faction; } }
        public DateTime Expiration { get { return m_Expiration; } }

        public bool HasExpired
        {
            get
            {
                if (m_Item == null || m_Item.Deleted)
                    return true;

                return (m_Expiration != DateTime.MinValue && DateTime.UtcNow >= m_Expiration);
            }
        }

        public void StartExpiration()
        {
            m_Expiration = DateTime.UtcNow + ExpirationPeriod;
        }

        public void CheckAttach()
        {
            if (!HasExpired)
                Attach();
            else
                Detach();
        }

        public void Attach()
        {
            if (m_Item is IFactionItem)
                ((IFactionItem)m_Item).FactionItemState = this;

            if (m_Faction != null)
                m_Faction.State.FactionItems.Add(this);
        }

        public void Detach()
        {
            if (m_Item is IFactionItem)
                ((IFactionItem)m_Item).FactionItemState = null;

            if (m_Faction != null && m_Faction.State.FactionItems.Contains(this))
                m_Faction.State.FactionItems.Remove(this);
        }

        public FactionItem(Item item, Faction faction)
        {
            m_Item = item;
            m_Faction = faction;
        }

        public FactionItem(GenericReader reader, Faction faction)
        {
            int version = reader.ReadEncodedInt();

            switch (version)
            {
                case 0:
                    {
                        m_Item = reader.ReadItem();
                        m_Expiration = reader.ReadDateTime();
                        break;
                    }
            }

            m_Faction = faction;
        }

        public void Serialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)0);

            writer.Write((Item)m_Item);
            writer.Write((DateTime)m_Expiration);
        }

        public static int GetMaxWearables(Mobile mob)
        {
            PlayerState pl = PlayerState.Find(mob);

            if (pl == null)
                return 0;

            if (pl.Faction.IsCommander(mob))
                return 9;

            return pl.Rank.MaxWearables;
        }

        public static FactionItem Find(Item item)
        {
            if (item is IFactionItem)
            {
                FactionItem state = ((IFactionItem)item).FactionItemState;

                if (state != null && state.HasExpired)
                {
                    state.Detach();
                    state = null;
                }

                return state;
            }

            return null;
        }

        public static Item Imbue(Item item, Faction faction, bool expire, int hue)
        {
            if (!(item is IFactionItem))
                return item;

            FactionItem state = Find(item);

            if (state == null)
            {
                state = new FactionItem(item, faction);
                state.Attach();
            }

            if (expire)
                state.StartExpiration();

            item.Hue = hue;
            return item;
        }
    }

    public abstract class PowerFactionItem : Item
    {
        public abstract bool Use(Mobile mob);

        private sealed class DestructionTimer : Timer
        {
            private Mobile _mobile;

            private bool _screamed;

            public DestructionTimer(Mobile mob)
                : base(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(0.1), 10)
            {
                _mobile = mob;
            }

            protected override void OnTick()
            {
                if (_mobile.Alive)
                {
                    if (!_screamed)
                    {
                        _screamed = true;

                        _mobile.PlaySound(_mobile.Female ? 814 : 1088);
                        _mobile.PublicOverheadMessage(Server.Network.MessageType.Regular, 2118, false, "Aaaaah!");
                    }

                    _mobile.Damage(Utility.Dice(2, 6, 0));
                }
            }
        }

        private sealed class WeightedItem
        {
            private int _weight;
            private Type _type;

            public int Weight
            {
                get
                {
                    return _weight;
                }
            }

            public Type Type
            {
                get
                {
                    return _type;
                }
            }

            public WeightedItem(int weight, Type type)
            {
                _weight = weight;
                _type = type;
            }

            public Item Construct()
            {
                return Activator.CreateInstance(_type) as Item;
            }
        }

        private static WeightedItem[] _items = {
			new WeightedItem( 30, typeof( GemOfEmpowerment ) ),
			new WeightedItem( 25, typeof( BloodRose ) ),
			new WeightedItem( 20, typeof( ClarityPotion ) ),
			new WeightedItem( 15, typeof( UrnOfAscension ) ),
			new WeightedItem( 10, typeof( StormsEye ) )
		};

        public static void CheckSpawn(Mobile killer, Mobile victim)
        {
            if (killer != null && victim != null)
            {
                PlayerState ps = PlayerState.Find(victim);

                if (ps != null)
                {
                    int chance = ps.Rank.Rank;

                    if (chance > Utility.Random(100))
                    {
                        int weight = 0;

                        foreach (WeightedItem item in _items)
                        {
                            weight += item.Weight;
                        }

                        weight = Utility.Random(weight);

                        foreach (WeightedItem item in _items)
                        {
                            if (weight < item.Weight)
                            {
                                Item obj = item.Construct();

                                if (obj != null)
                                {
                                    killer.AddToBackpack(obj);

                                    killer.SendSound(1470);
                                    killer.LocalOverheadMessage(
                                        Server.Network.MessageType.Regular, 2119, false,
                                        "You notice a strange item on the corpse, and decide to pick it up."
                                    );

                                    try
                                    {
                                        using (StreamWriter op = new StreamWriter("faction-power-items.log", true))
                                        {
                                            op.WriteLine("{0}\t{1}\t{2}\t{3}", DateTime.UtcNow, killer, victim, obj);
                                        }
                                    }
                                    catch
                                    {
                                    }
                                }

                                break;
                            }
                            else
                            {
                                weight -= item.Weight;
                            }
                        }
                    }
                }
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042038); // You must have the object in your backpack to use it.
            }
            else if (from is PlayerMobile && ((PlayerMobile)from).DuelContext != null)
            {
                from.SendMessage("You can't use that.");
            }
            else if (Faction.Find(from) == null)
            {
                from.LocalOverheadMessage(Server.Network.MessageType.Regular, 2119, false, "The object vanishes from your hands as you touch it.");

                Timer.DelayCall(TimeSpan.FromSeconds(1.0), delegate()
                {
                    from.LocalOverheadMessage(Server.Network.MessageType.Regular, 2118, false, "You feel a strange tingling sensation throughout your body.");
                });

                Timer.DelayCall(TimeSpan.FromSeconds(4.0), delegate()
                {
                    from.LocalOverheadMessage(Server.Network.MessageType.Regular, 2118, false, "Your skin begins to burn.");
                });

                new DestructionTimer(from).Start();
                Delete();

                //from.SendMessage( "You must be in a faction to use this item." );
            }
            else if (Use(from))
            {
                from.RevealingAction();
                Consume();
            }
        }

        public PowerFactionItem(int itemId)
            : base(itemId)
        {
        }

        public PowerFactionItem(Serial serial)
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

    public enum AllowedPlacing
    {
        Everywhere,

        AnyFactionTown,
        ControlledFactionTown,
        FactionStronghold
    }

    public abstract class BaseFactionTrap : BaseTrap
    {
        private Faction m_Faction;
        private Mobile m_Placer;
        private DateTime m_TimeOfPlacement;

        private Timer m_Concealing;

        [CommandProperty(AccessLevel.GameMaster)]
        public Faction Faction
        {
            get { return m_Faction; }
            set { m_Faction = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Placer
        {
            get { return m_Placer; }
            set { m_Placer = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime TimeOfPlacement
        {
            get { return m_TimeOfPlacement; }
            set { m_TimeOfPlacement = value; }
        }

        public virtual int EffectSound { get { return 0; } }

        public virtual int SilverFromDisarm { get { return 100; } }

        public virtual int MessageHue { get { return 0; } }

        public virtual int AttackMessage { get { return 0; } }
        public virtual int DisarmMessage { get { return 0; } }

        public virtual AllowedPlacing AllowedPlacing { get { return AllowedPlacing.Everywhere; } }

        public virtual TimeSpan ConcealPeriod
        {
            get { return TimeSpan.FromMinutes(1.0); }
        }

        public virtual TimeSpan DecayPeriod
        {
            get
            {
                if (Core.AOS)
                    return TimeSpan.FromDays(1.0);

                return TimeSpan.MaxValue; // no decay
            }
        }

        public override void OnTrigger(Mobile from)
        {
            if (!IsEnemy(from))
                return;

            Conceal();

            DoVisibleEffect();
            Effects.PlaySound(this.Location, this.Map, this.EffectSound);
            DoAttackEffect(from);

            int silverToAward = (from.Alive ? 20 : 40);

            if (silverToAward > 0 && m_Placer != null && m_Faction != null)
            {
                PlayerState victimState = PlayerState.Find(from);

                if (victimState != null && victimState.CanGiveSilverTo(m_Placer) && victimState.KillPoints > 0)
                {
                    int silverGiven = m_Faction.AwardSilver(m_Placer, silverToAward);

                    if (silverGiven > 0)
                    {
                        // TODO: Get real message
                        if (from.Alive)
                            m_Placer.SendMessage("You have earned {0} silver pieces because {1} fell for your trap.", silverGiven, from.Name);
                        else
                            m_Placer.SendLocalizedMessage(1042736, String.Format("{0} silver\t{1}", silverGiven, from.Name)); // You have earned ~1_SILVER_AMOUNT~ pieces for vanquishing ~2_PLAYER_NAME~!
                    }

                    victimState.OnGivenSilverTo(m_Placer);
                }
            }

            from.LocalOverheadMessage(MessageType.Regular, MessageHue, AttackMessage);
        }

        public abstract void DoVisibleEffect();
        public abstract void DoAttackEffect(Mobile m);

        public virtual int IsValidLocation()
        {
            return IsValidLocation(GetWorldLocation(), Map);
        }

        public virtual int IsValidLocation(Point3D p, Map m)
        {
            if (m == null)
                return 502956; // You cannot place a trap on that.

            if (Core.ML)
            {
                foreach (Item item in m.GetItemsInRange(p, 0))
                {
                    if (item is BaseFactionTrap && ((BaseFactionTrap)item).Faction == this.Faction)
                        return 1075263; // There is already a trap belonging to your faction at this location.;
                }
            }

            switch (AllowedPlacing)
            {
                case AllowedPlacing.FactionStronghold:
                    {
                        StrongholdRegion region = (StrongholdRegion)Region.Find(p, m).GetRegion(typeof(StrongholdRegion));

                        if (region != null && region.Faction == m_Faction)
                            return 0;

                        return 1010355; // This trap can only be placed in your stronghold
                    }
                case AllowedPlacing.AnyFactionTown:
                    {
                        Town town = Town.FromRegion(Region.Find(p, m));

                        if (town != null)
                            return 0;

                        return 1010356; // This trap can only be placed in a faction town
                    }
                case AllowedPlacing.ControlledFactionTown:
                    {
                        Town town = Town.FromRegion(Region.Find(p, m));

                        if (town != null && town.Owner == m_Faction)
                            return 0;

                        return 1010357; // This trap can only be placed in a town your faction controls
                    }
            }

            return 0;
        }

        public override void OnMovement(Mobile m, Point3D oldLocation)
        {
            base.OnMovement(m, oldLocation);

            if (!CheckDecay() && CheckRange(m.Location, oldLocation, 6))
            {
                if (Faction.Find(m) != null && ((m.Skills[SkillName.DetectHidden].Value - 80.0) / 20.0) > Utility.RandomDouble())
                    PrivateOverheadLocalizedMessage(m, 1010154, MessageHue, "", ""); // [Faction Trap]
            }
        }

        public void PrivateOverheadLocalizedMessage(Mobile to, int number, int hue, string name, string args)
        {
            if (to == null)
                return;

            NetState ns = to.NetState;

            if (ns != null)
                ns.Send(new MessageLocalized(Serial, ItemID, MessageType.Regular, hue, 3, number, name, args));
        }

        public BaseFactionTrap(Faction f, Mobile m, int itemID)
            : base(itemID)
        {
            Visible = false;

            m_Faction = f;
            m_TimeOfPlacement = DateTime.UtcNow;
            m_Placer = m;
        }

        public BaseFactionTrap(Serial serial)
            : base(serial)
        {
        }

        public virtual bool CheckDecay()
        {
            TimeSpan decayPeriod = DecayPeriod;

            if (decayPeriod == TimeSpan.MaxValue)
                return false;

            if ((m_TimeOfPlacement + decayPeriod) < DateTime.UtcNow)
            {
                Timer.DelayCall(TimeSpan.Zero, new TimerCallback(Delete));
                return true;
            }

            return false;
        }

        public virtual void BeginConceal()
        {
            if (m_Concealing != null)
                m_Concealing.Stop();

            m_Concealing = Timer.DelayCall(ConcealPeriod, new TimerCallback(Conceal));
        }

        public virtual void Conceal()
        {
            if (m_Concealing != null)
                m_Concealing.Stop();

            m_Concealing = null;

            if (!Deleted)
                Visible = false;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            Faction.WriteReference(writer, m_Faction);
            writer.Write((Mobile)m_Placer);
            writer.Write((DateTime)m_TimeOfPlacement);

            if (Visible)
                BeginConceal();
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            m_Faction = Faction.ReadReference(reader);
            m_Placer = reader.ReadMobile();
            m_TimeOfPlacement = reader.ReadDateTime();

            if (Visible)
                BeginConceal();

            CheckDecay();
        }

        public override void OnDelete()
        {
            if (m_Faction != null && m_Faction.Traps.Contains(this))
                m_Faction.Traps.Remove(this);

            base.OnDelete();
        }

        public virtual bool IsEnemy(Mobile mob)
        {
            if (mob.Hidden && mob.AccessLevel > AccessLevel.Player)
                return false;

            if (!mob.Alive || mob.IsDeadBondedPet)
                return false;

            Faction faction = Faction.Find(mob, true);

            if (faction == null && mob is BaseFactionGuard)
                faction = ((BaseFactionGuard)mob).Faction;

            if (faction == null)
                return false;

            return (faction != m_Faction);
        }
    }

    public abstract class BaseFactionTrapDeed : Item, ICraftable
    {
        public abstract Type TrapType { get; }

        private Faction m_Faction;

        [CommandProperty(AccessLevel.GameMaster)]
        public Faction Faction
        {
            get { return m_Faction; }
            set
            {
                m_Faction = value;

                if (m_Faction != null)
                    Hue = m_Faction.Definition.HuePrimary;
            }
        }

        public BaseFactionTrapDeed(int itemID)
            : base(itemID)
        {
            Weight = 1.0;
            LootType = LootType.Blessed;
        }

        public BaseFactionTrapDeed(bool createdFromDeed)
            : this(0x14F0)
        {
        }

        public BaseFactionTrapDeed(Serial serial)
            : base(serial)
        {
        }

        public virtual BaseFactionTrap Construct(Mobile from)
        {
            try { return Activator.CreateInstance(TrapType, new object[] { m_Faction, from }) as BaseFactionTrap; }
            catch { return null; }
        }

        public override void OnDoubleClick(Mobile from)
        {
            Faction faction = Faction.Find(from);

            if (faction == null)
                from.SendLocalizedMessage(1010353, "", 0x23); // Only faction members may place faction traps
            else if (faction != m_Faction)
                from.SendLocalizedMessage(1010354, "", 0x23); // You may only place faction traps created by your faction
            else if (faction.Traps.Count >= faction.MaximumTraps)
                from.SendLocalizedMessage(1010358, "", 0x23); // Your faction already has the maximum number of traps placed
            else
            {
                BaseFactionTrap trap = Construct(from);

                if (trap == null)
                    return;

                int message = trap.IsValidLocation(from.Location, from.Map);

                if (message > 0)
                {
                    from.SendLocalizedMessage(message, "", 0x23);
                    trap.Delete();
                }
                else
                {
                    from.SendLocalizedMessage(1010360); // You arm the trap and carefully hide it from view
                    trap.MoveToWorld(from.Location, from.Map);
                    faction.Traps.Add(trap);
                    Delete();
                }
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            Faction.WriteReference(writer, m_Faction);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            m_Faction = Faction.ReadReference(reader);
        }
        #region ICraftable Members

        public int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool, CraftItem craftItem, int resHue)
        {
            ItemID = 0x14F0;
            Faction = Faction.Find(from);

            return 1;
        }

        #endregion
    }

    #endregion

    #region Faction Currency

    public class FinanceGump : FactionGump
    {
        private PlayerMobile m_From;
        private Faction m_Faction;
        private Town m_Town;

        private static int[] m_PriceOffsets = new int[]
			{
				-30, -25, -20, -15, -10, -5,
				+50, +100, +150, +200, +250, +300
			};

        public override int ButtonTypes { get { return 2; } }

        public FinanceGump(PlayerMobile from, Faction faction, Town town)
            : base(50, 50)
        {
            m_From = from;
            m_Faction = faction;
            m_Town = town;


            AddPage(0);

            AddBackground(0, 0, 320, 410, 5054);
            AddBackground(10, 10, 300, 390, 3000);

            #region General
            AddPage(1);

            AddHtmlLocalized(20, 30, 260, 25, 1011541, false, false); // FINANCE MINISTER


            AddHtmlLocalized(55, 90, 200, 25, 1011539, false, false); // CHANGE PRICES
            AddButton(20, 90, 4005, 4007, 0, GumpButtonType.Page, 2);

            AddHtmlLocalized(55, 120, 200, 25, 1011540, false, false); // BUY SHOPKEEPERS	
            AddButton(20, 120, 4005, 4007, 0, GumpButtonType.Page, 3);

            AddHtmlLocalized(55, 150, 200, 25, 1011495, false, false); // VIEW FINANCES
            AddButton(20, 150, 4005, 4007, 0, GumpButtonType.Page, 4);

            AddHtmlLocalized(55, 360, 200, 25, 1011441, false, false); // EXIT
            AddButton(20, 360, 4005, 4007, 0, GumpButtonType.Reply, 0);
            #endregion

            #region Change Prices
            AddPage(2);

            AddHtmlLocalized(20, 30, 200, 25, 1011539, false, false); // CHANGE PRICES

            for (int i = 0; i < m_PriceOffsets.Length; ++i)
            {
                int ofs = m_PriceOffsets[i];

                int x = 20 + ((i / 6) * 150);
                int y = 90 + ((i % 6) * 30);

                AddRadio(x, y, 208, 209, (town.Tax == ofs), i + 1);

                if (ofs < 0)
                    AddLabel(x + 35, y, 0x26, String.Concat("- ", -ofs, "%"));
                else
                    AddLabel(x + 35, y, 0x12A, String.Concat("+ ", ofs, "%"));
            }

            AddRadio(20, 270, 208, 209, (town.Tax == 0), 0);
            AddHtmlLocalized(55, 270, 90, 25, 1011542, false, false); // normal

            AddHtmlLocalized(55, 330, 200, 25, 1011509, false, false); // Set Prices
            AddButton(20, 330, 4005, 4007, ToButtonID(0, 0), GumpButtonType.Reply, 0);

            AddHtmlLocalized(55, 360, 200, 25, 1011067, false, false); // Previous page
            AddButton(20, 360, 4005, 4007, 0, GumpButtonType.Page, 1);
            #endregion

            #region Buy Shopkeepers
            AddPage(3);

            AddHtmlLocalized(20, 30, 200, 25, 1011540, false, false); // BUY SHOPKEEPERS

            List<VendorList> vendorLists = town.VendorLists;

            for (int i = 0; i < vendorLists.Count; ++i)
            {
                VendorList list = vendorLists[i];

                AddButton(20, 90 + (i * 40), 4005, 4007, 0, GumpButtonType.Page, 5 + i);
                AddItem(55, 90 + (i * 40), list.Definition.ItemID);
                AddHtmlText(100, 90 + (i * 40), 200, 25, list.Definition.Label, false, false);
            }

            AddHtmlLocalized(55, 360, 200, 25, 1011067, false, false);	//	Previous page
            AddButton(20, 360, 4005, 4007, 0, GumpButtonType.Page, 1);
            #endregion

            #region View Finances
            AddPage(4);

            int financeUpkeep = town.FinanceUpkeep;
            int sheriffUpkeep = town.SheriffUpkeep;
            int dailyIncome = town.DailyIncome;
            int netCashFlow = town.NetCashFlow;


            AddHtmlLocalized(20, 30, 300, 25, 1011524, false, false); // FINANCE STATEMENT

            AddHtmlLocalized(20, 80, 300, 25, 1011538, false, false); // Current total money for town : 
            AddLabel(20, 100, 0x44, town.Silver.ToString());

            AddHtmlLocalized(20, 130, 300, 25, 1011520, false, false); // Finance Minister Upkeep : 
            AddLabel(20, 150, 0x44, financeUpkeep.ToString("N0")); // NOTE: Added 'N0'

            AddHtmlLocalized(20, 180, 300, 25, 1011521, false, false); // Sheriff Upkeep : 
            AddLabel(20, 200, 0x44, sheriffUpkeep.ToString("N0")); // NOTE: Added 'N0'

            AddHtmlLocalized(20, 230, 300, 25, 1011522, false, false); // Town Income : 
            AddLabel(20, 250, 0x44, dailyIncome.ToString("N0")); // NOTE: Added 'N0'

            AddHtmlLocalized(20, 280, 300, 25, 1011523, false, false); // Net Cash flow per day : 
            AddLabel(20, 300, 0x44, netCashFlow.ToString("N0")); // NOTE: Added 'N0'

            AddHtmlLocalized(55, 360, 200, 25, 1011067, false, false); // Previous page
            AddButton(20, 360, 4005, 4007, 0, GumpButtonType.Page, 1);
            #endregion

            #region Shopkeeper Pages
            for (int i = 0; i < vendorLists.Count; ++i)
            {
                VendorList vendorList = vendorLists[i];

                AddPage(5 + i);

                AddHtmlText(60, 30, 300, 25, vendorList.Definition.Header, false, false);
                AddItem(20, 30, vendorList.Definition.ItemID);

                AddHtmlLocalized(20, 90, 200, 25, 1011514, false, false); // You have : 
                AddLabel(230, 90, 0x26, vendorList.Vendors.Count.ToString());

                AddHtmlLocalized(20, 120, 200, 25, 1011515, false, false); // Maximum : 
                AddLabel(230, 120, 0x256, vendorList.Definition.Maximum.ToString());

                AddHtmlLocalized(20, 150, 200, 25, 1011516, false, false); // Cost :
                AddLabel(230, 150, 0x44, vendorList.Definition.Price.ToString("N0")); // NOTE: Added 'N0'

                AddHtmlLocalized(20, 180, 200, 25, 1011517, false, false); // Daily Pay :
                AddLabel(230, 180, 0x37, vendorList.Definition.Upkeep.ToString("N0")); // NOTE: Added 'N0'

                AddHtmlLocalized(20, 210, 200, 25, 1011518, false, false); // Current Silver :
                AddLabel(230, 210, 0x44, town.Silver.ToString("N0")); // NOTE: Added 'N0'

                AddHtmlLocalized(20, 240, 200, 25, 1011519, false, false); // Current Payroll :
                AddLabel(230, 240, 0x44, financeUpkeep.ToString("N0")); // NOTE: Added 'N0'

                AddHtmlText(55, 300, 200, 25, vendorList.Definition.Label, false, false);
                if (town.Silver >= vendorList.Definition.Price)
                    AddButton(20, 300, 4005, 4007, ToButtonID(1, i), GumpButtonType.Reply, 0);
                else
                    AddImage(20, 300, 4020);

                AddHtmlLocalized(55, 360, 200, 25, 1011067, false, false); // Previous page
                AddButton(20, 360, 4005, 4007, 0, GumpButtonType.Page, 3);
            }
            #endregion
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (!m_Town.IsFinance(m_From) || m_Town.Owner != m_Faction)
            {
                m_From.SendLocalizedMessage(1010339); // You no longer control this city
                return;
            }

            int type, index;

            if (!FromButtonID(info.ButtonID, out type, out index))
                return;

            switch (type)
            {
                case 0: // general
                    {
                        switch (index)
                        {
                            case 0: // set price
                                {
                                    int[] switches = info.Switches;

                                    if (switches.Length == 0)
                                        break;

                                    int opt = switches[0];
                                    int newTax = 0;

                                    if (opt >= 1 && opt <= m_PriceOffsets.Length)
                                        newTax = m_PriceOffsets[opt - 1];

                                    if (m_Town.Tax == newTax)
                                        break;

                                    if (m_From.AccessLevel == AccessLevel.Player && !m_Town.TaxChangeReady)
                                    {
                                        TimeSpan remaining = DateTime.UtcNow - (m_Town.LastTaxChange + Town.TaxChangePeriod);

                                        if (remaining.TotalMinutes < 4)
                                            m_From.SendLocalizedMessage(1042165); // You must wait a short while before changing prices again.
                                        else if (remaining.TotalMinutes < 10)
                                            m_From.SendLocalizedMessage(1042166); // You must wait several minutes before changing prices again.
                                        else if (remaining.TotalHours < 1)
                                            m_From.SendLocalizedMessage(1042167); // You must wait up to an hour before changing prices again.
                                        else if (remaining.TotalHours < 4)
                                            m_From.SendLocalizedMessage(1042168); // You must wait a few hours before changing prices again.
                                        else
                                            m_From.SendLocalizedMessage(1042169); // You must wait several hours before changing prices again.
                                    }
                                    else
                                    {
                                        m_Town.Tax = newTax;

                                        if (m_From.AccessLevel == AccessLevel.Player)
                                            m_Town.LastTaxChange = DateTime.UtcNow;
                                    }

                                    break;
                                }
                        }

                        break;
                    }
                case 1: // make vendor
                    {
                        List<VendorList> vendorLists = m_Town.VendorLists;

                        if (index >= 0 && index < vendorLists.Count)
                        {
                            VendorList vendorList = vendorLists[index];

                            Town town = Town.FromRegion(m_From.Region);

                            if (Town.FromRegion(m_From.Region) != m_Town)
                            {
                                m_From.SendLocalizedMessage(1010305); // You must be in your controlled city to buy Items
                            }
                            else if (vendorList.Vendors.Count >= vendorList.Definition.Maximum)
                            {
                                m_From.SendLocalizedMessage(1010306); // You currently have too many of this enhancement type to place another
                            }
                            else if (BaseBoat.FindBoatAt(m_From.Location, m_From.Map) != null)
                            {
                                m_From.SendMessage("You cannot place a vendor here");
                            }
                            else if (m_Town.Silver >= vendorList.Definition.Price)
                            {
                                BaseFactionVendor vendor = vendorList.Construct(m_Town, m_Faction);

                                if (vendor != null)
                                {
                                    m_Town.Silver -= vendorList.Definition.Price;

                                    vendor.MoveToWorld(m_From.Location, m_From.Map);
                                    vendor.Home = vendor.Location;
                                }
                            }
                        }

                        break;
                    }
            }
        }
    }

    public class SilverGivenEntry
    {
        public static readonly TimeSpan ExpirePeriod = TimeSpan.FromHours(3.0);

        private Mobile m_GivenTo;
        private DateTime m_TimeOfGift;

        public Mobile GivenTo { get { return m_GivenTo; } }
        public DateTime TimeOfGift { get { return m_TimeOfGift; } }

        public bool IsExpired { get { return (m_TimeOfGift + ExpirePeriod) < DateTime.UtcNow; } }

        public SilverGivenEntry(Mobile givenTo)
        {
            m_GivenTo = givenTo;
            m_TimeOfGift = DateTime.UtcNow;
        }
    }

    #endregion
}

namespace Server.Factions.AI
{
    public enum ReactionType
    {
        Ignore,
        Warn,
        Attack
    }

    public enum MovementType
    {
        Stand,
        Patrol,
        Follow
    }

    public class Reaction
    {
        private Faction m_Faction;
        private ReactionType m_Type;

        public Faction Faction { get { return m_Faction; } }
        public ReactionType Type { get { return m_Type; } set { m_Type = value; } }

        public Reaction(Faction faction, ReactionType type)
        {
            m_Faction = faction;
            m_Type = type;
        }

        public Reaction(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            switch (version)
            {
                case 0:
                    {
                        m_Faction = Faction.ReadReference(reader);
                        m_Type = (ReactionType)reader.ReadEncodedInt();

                        break;
                    }
            }
        }

        public void Serialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)0); // version

            Faction.WriteReference(writer, m_Faction);
            writer.WriteEncodedInt((int)m_Type);
        }
    }

    public class Orders
    {
        private BaseFactionGuard m_Guard;

        private List<Reaction> m_Reactions;
        private MovementType m_Movement;
        private Mobile m_Follow;

        public BaseFactionGuard Guard { get { return m_Guard; } }

        public MovementType Movement { get { return m_Movement; } set { m_Movement = value; } }
        public Mobile Follow { get { return m_Follow; } set { m_Follow = value; } }

        public Reaction GetReaction(Faction faction)
        {
            Reaction reaction;

            for (int i = 0; i < m_Reactions.Count; ++i)
            {
                reaction = m_Reactions[i];

                if (reaction.Faction == faction)
                    return reaction;
            }

            reaction = new Reaction(faction, (faction == null || faction == m_Guard.Faction) ? ReactionType.Ignore : ReactionType.Attack);
            m_Reactions.Add(reaction);

            return reaction;
        }

        public void SetReaction(Faction faction, ReactionType type)
        {
            Reaction reaction = GetReaction(faction);

            reaction.Type = type;
        }

        public Orders(BaseFactionGuard guard)
        {
            m_Guard = guard;
            m_Reactions = new List<Reaction>();
            m_Movement = MovementType.Patrol;
        }

        public Orders(BaseFactionGuard guard, GenericReader reader)
        {
            m_Guard = guard;

            int version = reader.ReadEncodedInt();

            switch (version)
            {
                case 1:
                    {
                        m_Follow = reader.ReadMobile();
                        goto case 0;
                    }
                case 0:
                    {
                        int count = reader.ReadEncodedInt();
                        m_Reactions = new List<Reaction>(count);

                        for (int i = 0; i < count; ++i)
                            m_Reactions.Add(new Reaction(reader));

                        m_Movement = (MovementType)reader.ReadEncodedInt();

                        break;
                    }
            }
        }

        public void Serialize(GenericWriter writer)
        {
            writer.WriteEncodedInt((int)1); // version

            writer.Write((Mobile)m_Follow);

            writer.WriteEncodedInt((int)m_Reactions.Count);

            for (int i = 0; i < m_Reactions.Count; ++i)
                m_Reactions[i].Serialize(writer);

            writer.WriteEncodedInt((int)m_Movement);
        }
    }
}