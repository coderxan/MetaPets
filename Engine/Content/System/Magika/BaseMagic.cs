using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Server;
using Server.Commands;
using Server.ContextMenus;
using Server.Engines.Craft;
using Server.Engines.PartySystem;
using Server.Guilds;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Regions;
using Server.Spells;
using Server.Spells.Bushido;
using Server.Spells.Chivalry;
using Server.Spells.Second;
using Server.Spells.Fifth;
using Server.Spells.Seventh;
using Server.Spells.Necromancy;
using Server.Spells.Ninjitsu;
using Server.Spells.Spellweaving;
using Server.Targeting;

namespace Server
{
    public class DefensiveSpell
    {
        public static void Nullify(Mobile from)
        {
            if (!from.CanBeginAction(typeof(DefensiveSpell)))
                new InternalTimer(from).Start();
        }

        private class InternalTimer : Timer
        {
            private Mobile m_Mobile;

            public InternalTimer(Mobile m)
                : base(TimeSpan.FromMinutes(1.0))
            {
                m_Mobile = m;

                Priority = TimerPriority.OneSecond;
            }

            protected override void OnTick()
            {
                m_Mobile.EndAction(typeof(DefensiveSpell));
            }
        }
    }
}

namespace Server.Items
{
    public enum SpellbookType
    {
        Invalid = -1,
        Regular,
        Necromancer,
        Paladin,
        Ninja,
        Samurai,
        Arcanist,
        Mystic
    }

    public enum BookQuality
    {
        Regular,
        Exceptional,
    }

    public class Spellbook : Item, ICraftable, ISlayer
    {
        private string m_EngravedText;
        private BookQuality m_Quality;

        [CommandProperty(AccessLevel.GameMaster)]
        public string EngravedText
        {
            get { return m_EngravedText; }
            set { m_EngravedText = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public BookQuality Quality
        {
            get { return m_Quality; }
            set { m_Quality = value; InvalidateProperties(); }
        }

        public static void Initialize()
        {
            EventSink.OpenSpellbookRequest += new OpenSpellbookRequestEventHandler(EventSink_OpenSpellbookRequest);
            EventSink.CastSpellRequest += new CastSpellRequestEventHandler(EventSink_CastSpellRequest);

            CommandSystem.Register("AllSpells", AccessLevel.GameMaster, new CommandEventHandler(AllSpells_OnCommand));
        }

        [Usage("AllSpells")]
        [Description("Completely fills a targeted spellbook with scrolls.")]
        private static void AllSpells_OnCommand(CommandEventArgs e)
        {
            e.Mobile.BeginTarget(-1, false, TargetFlags.None, new TargetCallback(AllSpells_OnTarget));
            e.Mobile.SendMessage("Target the spellbook to fill.");
        }

        private static void AllSpells_OnTarget(Mobile from, object obj)
        {
            if (obj is Spellbook)
            {
                Spellbook book = (Spellbook)obj;

                if (book.BookCount == 64)
                    book.Content = ulong.MaxValue;
                else
                    book.Content = (1ul << book.BookCount) - 1;

                from.SendMessage("The spellbook has been filled.");

                CommandLogging.WriteLine(from, "{0} {1} filling spellbook {2}", from.AccessLevel, CommandLogging.Format(from), CommandLogging.Format(book));
            }
            else
            {
                from.BeginTarget(-1, false, TargetFlags.None, new TargetCallback(AllSpells_OnTarget));
                from.SendMessage("That is not a spellbook. Try again.");
            }
        }

        private static void EventSink_OpenSpellbookRequest(OpenSpellbookRequestEventArgs e)
        {
            Mobile from = e.Mobile;

            if (!Multis.DesignContext.Check(from))
                return; // They are customizing

            SpellbookType type;

            switch (e.Type)
            {
                default:
                case 1: type = SpellbookType.Regular; break;
                case 2: type = SpellbookType.Necromancer; break;
                case 3: type = SpellbookType.Paladin; break;
                case 4: type = SpellbookType.Ninja; break;
                case 5: type = SpellbookType.Samurai; break;
                case 6: type = SpellbookType.Arcanist; break;
                case 7: type = SpellbookType.Mystic; break;
            }

            Spellbook book = Spellbook.Find(from, -1, type);

            if (book != null)
                book.DisplayTo(from);
        }

        private static void EventSink_CastSpellRequest(CastSpellRequestEventArgs e)
        {
            Mobile from = e.Mobile;

            if (!Multis.DesignContext.Check(from))
                return; // They are customizing

            Spellbook book = e.Spellbook as Spellbook;
            int spellID = e.SpellID;

            if (book == null || !book.HasSpell(spellID))
                book = Find(from, spellID);

            if (book != null && book.HasSpell(spellID))
            {
                SpecialMove move = SpellRegistry.GetSpecialMove(spellID);

                if (move != null)
                {
                    SpecialMove.SetCurrentMove(from, move);
                }
                else
                {
                    Spell spell = SpellRegistry.NewSpell(spellID, from, null);

                    if (spell != null)
                        spell.Cast();
                    else
                        from.SendLocalizedMessage(502345); // This spell has been temporarily disabled.
                }
            }
            else
            {
                from.SendLocalizedMessage(500015); // You do not have that spell!
            }
        }

        private static Dictionary<Mobile, List<Spellbook>> m_Table = new Dictionary<Mobile, List<Spellbook>>();

        public static SpellbookType GetTypeForSpell(int spellID)
        {
            if (spellID >= 0 && spellID < 64)
                return SpellbookType.Regular;
            else if (spellID >= 100 && spellID < 117)
                return SpellbookType.Necromancer;
            else if (spellID >= 200 && spellID < 210)
                return SpellbookType.Paladin;
            else if (spellID >= 400 && spellID < 406)
                return SpellbookType.Samurai;
            else if (spellID >= 500 && spellID < 508)
                return SpellbookType.Ninja;
            else if (spellID >= 600 && spellID < 617)
                return SpellbookType.Arcanist;
            else if (spellID >= 677 && spellID < 693)
                return SpellbookType.Mystic;

            return SpellbookType.Invalid;
        }

        public static Spellbook FindRegular(Mobile from)
        {
            return Find(from, -1, SpellbookType.Regular);
        }

        public static Spellbook FindNecromancer(Mobile from)
        {
            return Find(from, -1, SpellbookType.Necromancer);
        }

        public static Spellbook FindPaladin(Mobile from)
        {
            return Find(from, -1, SpellbookType.Paladin);
        }

        public static Spellbook FindSamurai(Mobile from)
        {
            return Find(from, -1, SpellbookType.Samurai);
        }

        public static Spellbook FindNinja(Mobile from)
        {
            return Find(from, -1, SpellbookType.Ninja);
        }

        public static Spellbook FindArcanist(Mobile from)
        {
            return Find(from, -1, SpellbookType.Arcanist);
        }

        public static Spellbook FindMystic(Mobile from)
        {
            return Find(from, -1, SpellbookType.Mystic);
        }

        public static Spellbook Find(Mobile from, int spellID)
        {
            return Find(from, spellID, GetTypeForSpell(spellID));
        }

        public static Spellbook Find(Mobile from, int spellID, SpellbookType type)
        {
            if (from == null)
                return null;

            if (from.Deleted)
            {
                m_Table.Remove(from);
                return null;
            }

            List<Spellbook> list = null;

            m_Table.TryGetValue(from, out list);

            bool searchAgain = false;

            if (list == null)
                m_Table[from] = list = FindAllSpellbooks(from);
            else
                searchAgain = true;

            Spellbook book = FindSpellbookInList(list, from, spellID, type);

            if (book == null && searchAgain)
            {
                m_Table[from] = list = FindAllSpellbooks(from);

                book = FindSpellbookInList(list, from, spellID, type);
            }

            return book;
        }

        public static Spellbook FindSpellbookInList(List<Spellbook> list, Mobile from, int spellID, SpellbookType type)
        {
            Container pack = from.Backpack;

            for (int i = list.Count - 1; i >= 0; --i)
            {
                if (i >= list.Count)
                    continue;

                Spellbook book = list[i];

                if (!book.Deleted && (book.Parent == from || (pack != null && book.Parent == pack)) && ValidateSpellbook(book, spellID, type))
                    return book;

                list.RemoveAt(i);
            }

            return null;
        }

        public static List<Spellbook> FindAllSpellbooks(Mobile from)
        {
            List<Spellbook> list = new List<Spellbook>();

            Item item = from.FindItemOnLayer(Layer.OneHanded);

            if (item is Spellbook)
                list.Add((Spellbook)item);

            Container pack = from.Backpack;

            if (pack == null)
                return list;

            for (int i = 0; i < pack.Items.Count; ++i)
            {
                item = pack.Items[i];

                if (item is Spellbook)
                    list.Add((Spellbook)item);
            }

            return list;
        }

        public static Spellbook FindEquippedSpellbook(Mobile from)
        {
            return (from.FindItemOnLayer(Layer.OneHanded) as Spellbook);
        }

        public static bool ValidateSpellbook(Spellbook book, int spellID, SpellbookType type)
        {
            return (book.SpellbookType == type && (spellID == -1 || book.HasSpell(spellID)));
        }

        public override bool DisplayWeight { get { return false; } }

        private AosAttributes m_AosAttributes;
        private AosSkillBonuses m_AosSkillBonuses;

        [CommandProperty(AccessLevel.GameMaster)]
        public AosAttributes Attributes
        {
            get { return m_AosAttributes; }
            set { }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public AosSkillBonuses SkillBonuses
        {
            get { return m_AosSkillBonuses; }
            set { }
        }

        public virtual SpellbookType SpellbookType { get { return SpellbookType.Regular; } }
        public virtual int BookOffset { get { return 0; } }
        public virtual int BookCount { get { return 64; } }

        private ulong m_Content;
        private int m_Count;

        public override bool AllowSecureTrade(Mobile from, Mobile to, Mobile newOwner, bool accepted)
        {
            if (!Ethics.Ethic.CheckTrade(from, to, newOwner, this))
                return false;

            return base.AllowSecureTrade(from, to, newOwner, accepted);
        }

        public override bool CanEquip(Mobile from)
        {
            if (!Ethics.Ethic.CheckEquip(from, this))
            {
                return false;
            }
            else if (!from.CanBeginAction(typeof(BaseWeapon)))
            {
                return false;
            }

            return base.CanEquip(from);
        }

        public override bool AllowEquipedCast(Mobile from)
        {
            return true;
        }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (dropped is SpellScroll && dropped.Amount == 1)
            {
                SpellScroll scroll = (SpellScroll)dropped;

                SpellbookType type = GetTypeForSpell(scroll.SpellID);

                if (type != this.SpellbookType)
                {
                    return false;
                }
                else if (HasSpell(scroll.SpellID))
                {
                    from.SendLocalizedMessage(500179); // That spell is already present in that spellbook.
                    return false;
                }
                else
                {
                    int val = scroll.SpellID - BookOffset;

                    if (val >= 0 && val < BookCount)
                    {
                        m_Content |= (ulong)1 << val;
                        ++m_Count;

                        InvalidateProperties();

                        scroll.Delete();

                        from.Send(new PlaySound(0x249, GetWorldLocation()));
                        return true;
                    }

                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public ulong Content
        {
            get
            {
                return m_Content;
            }
            set
            {
                if (m_Content != value)
                {
                    m_Content = value;

                    m_Count = 0;

                    while (value > 0)
                    {
                        m_Count += (int)(value & 0x1);
                        value >>= 1;
                    }

                    InvalidateProperties();
                }
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SpellCount
        {
            get
            {
                return m_Count;
            }
        }

        [Constructable]
        public Spellbook()
            : this((ulong)0)
        {
        }

        [Constructable]
        public Spellbook(ulong content)
            : this(content, 0xEFA)
        {
        }

        public Spellbook(ulong content, int itemID)
            : base(itemID)
        {
            m_AosAttributes = new AosAttributes(this);
            m_AosSkillBonuses = new AosSkillBonuses(this);

            Weight = 3.0;
            Layer = Layer.OneHanded;
            LootType = LootType.Blessed;

            Content = content;
        }

        public override void OnAfterDuped(Item newItem)
        {
            Spellbook book = newItem as Spellbook;

            if (book == null)
                return;

            book.m_AosAttributes = new AosAttributes(newItem, m_AosAttributes);
            book.m_AosSkillBonuses = new AosSkillBonuses(newItem, m_AosSkillBonuses);
        }

        public override void OnAdded(IEntity parent)
        {
            if (Core.AOS && parent is Mobile)
            {
                Mobile from = (Mobile)parent;

                m_AosSkillBonuses.AddTo(from);

                int strBonus = m_AosAttributes.BonusStr;
                int dexBonus = m_AosAttributes.BonusDex;
                int intBonus = m_AosAttributes.BonusInt;

                if (strBonus != 0 || dexBonus != 0 || intBonus != 0)
                {
                    string modName = this.Serial.ToString();

                    if (strBonus != 0)
                        from.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));

                    if (dexBonus != 0)
                        from.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));

                    if (intBonus != 0)
                        from.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
                }

                from.CheckStatTimers();
            }
        }

        public override void OnRemoved(IEntity parent)
        {
            if (Core.AOS && parent is Mobile)
            {
                Mobile from = (Mobile)parent;

                m_AosSkillBonuses.Remove();

                string modName = this.Serial.ToString();

                from.RemoveStatMod(modName + "Str");
                from.RemoveStatMod(modName + "Dex");
                from.RemoveStatMod(modName + "Int");

                from.CheckStatTimers();
            }
        }

        public bool HasSpell(int spellID)
        {
            spellID -= BookOffset;

            return (spellID >= 0 && spellID < BookCount && (m_Content & ((ulong)1 << spellID)) != 0);
        }

        public Spellbook(Serial serial)
            : base(serial)
        {
        }

        public void DisplayTo(Mobile to)
        {
            // The client must know about the spellbook or it will crash!

            NetState ns = to.NetState;

            if (ns == null)
                return;

            if (Parent == null)
            {
                to.Send(this.WorldPacket);
            }
            else if (Parent is Item)
            {
                // What will happen if the client doesn't know about our parent?
                if (ns.ContainerGridLines)
                    to.Send(new ContainerContentUpdate6017(this));
                else
                    to.Send(new ContainerContentUpdate(this));
            }
            else if (Parent is Mobile)
            {
                // What will happen if the client doesn't know about our parent?
                to.Send(new EquipUpdate(this));
            }

            if (ns.HighSeas)
                to.Send(new DisplaySpellbookHS(this));
            else
                to.Send(new DisplaySpellbook(this));

            if (ObjectPropertyList.Enabled)
            {
                if (ns.NewSpellbook)
                {
                    to.Send(new NewSpellbookContent(this, ItemID, BookOffset + 1, m_Content));
                }
                else
                {
                    if (ns.ContainerGridLines)
                    {
                        to.Send(new SpellbookContent6017(m_Count, BookOffset + 1, m_Content, this));
                    }
                    else
                    {
                        to.Send(new SpellbookContent(m_Count, BookOffset + 1, m_Content, this));
                    }
                }
            }
            else
            {
                if (ns.ContainerGridLines)
                {
                    to.Send(new SpellbookContent6017(m_Count, BookOffset + 1, m_Content, this));
                }
                else
                {
                    to.Send(new SpellbookContent(m_Count, BookOffset + 1, m_Content, this));
                }
            }
        }

        private Mobile m_Crafter;

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Crafter
        {
            get { return m_Crafter; }
            set { m_Crafter = value; InvalidateProperties(); }
        }

        public override bool DisplayLootType { get { return Core.AOS; } }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (m_Quality == BookQuality.Exceptional)
                list.Add(1063341); // exceptional

            if (m_EngravedText != null)
                list.Add(1072305, m_EngravedText); // Engraved: ~1_INSCRIPTION~

            if (m_Crafter != null)
                list.Add(1050043, m_Crafter.Name); // crafted by ~1_NAME~

            m_AosSkillBonuses.GetProperties(list);

            if (m_Slayer != SlayerName.None)
            {
                SlayerEntry entry = SlayerGroup.GetEntryByName(m_Slayer);
                if (entry != null)
                    list.Add(entry.Title);
            }

            if (m_Slayer2 != SlayerName.None)
            {
                SlayerEntry entry = SlayerGroup.GetEntryByName(m_Slayer2);
                if (entry != null)
                    list.Add(entry.Title);
            }

            int prop;

            if ((prop = m_AosAttributes.WeaponDamage) != 0)
                list.Add(1060401, prop.ToString()); // damage increase ~1_val~%

            if ((prop = m_AosAttributes.DefendChance) != 0)
                list.Add(1060408, prop.ToString()); // defense chance increase ~1_val~%

            if ((prop = m_AosAttributes.BonusDex) != 0)
                list.Add(1060409, prop.ToString()); // dexterity bonus ~1_val~

            if ((prop = m_AosAttributes.EnhancePotions) != 0)
                list.Add(1060411, prop.ToString()); // enhance potions ~1_val~%

            if ((prop = m_AosAttributes.CastRecovery) != 0)
                list.Add(1060412, prop.ToString()); // faster cast recovery ~1_val~

            if ((prop = m_AosAttributes.CastSpeed) != 0)
                list.Add(1060413, prop.ToString()); // faster casting ~1_val~

            if ((prop = m_AosAttributes.AttackChance) != 0)
                list.Add(1060415, prop.ToString()); // hit chance increase ~1_val~%

            if ((prop = m_AosAttributes.BonusHits) != 0)
                list.Add(1060431, prop.ToString()); // hit point increase ~1_val~

            if ((prop = m_AosAttributes.BonusInt) != 0)
                list.Add(1060432, prop.ToString()); // intelligence bonus ~1_val~

            if ((prop = m_AosAttributes.LowerManaCost) != 0)
                list.Add(1060433, prop.ToString()); // lower mana cost ~1_val~%

            if ((prop = m_AosAttributes.LowerRegCost) != 0)
                list.Add(1060434, prop.ToString()); // lower reagent cost ~1_val~%

            if ((prop = m_AosAttributes.Luck) != 0)
                list.Add(1060436, prop.ToString()); // luck ~1_val~

            if ((prop = m_AosAttributes.BonusMana) != 0)
                list.Add(1060439, prop.ToString()); // mana increase ~1_val~

            if ((prop = m_AosAttributes.RegenMana) != 0)
                list.Add(1060440, prop.ToString()); // mana regeneration ~1_val~

            if ((prop = m_AosAttributes.NightSight) != 0)
                list.Add(1060441); // night sight

            if ((prop = m_AosAttributes.ReflectPhysical) != 0)
                list.Add(1060442, prop.ToString()); // reflect physical damage ~1_val~%

            if ((prop = m_AosAttributes.RegenStam) != 0)
                list.Add(1060443, prop.ToString()); // stamina regeneration ~1_val~

            if ((prop = m_AosAttributes.RegenHits) != 0)
                list.Add(1060444, prop.ToString()); // hit point regeneration ~1_val~

            if ((prop = m_AosAttributes.SpellChanneling) != 0)
                list.Add(1060482); // spell channeling

            if ((prop = m_AosAttributes.SpellDamage) != 0)
                list.Add(1060483, prop.ToString()); // spell damage increase ~1_val~%

            if ((prop = m_AosAttributes.BonusStam) != 0)
                list.Add(1060484, prop.ToString()); // stamina increase ~1_val~

            if ((prop = m_AosAttributes.BonusStr) != 0)
                list.Add(1060485, prop.ToString()); // strength bonus ~1_val~

            if ((prop = m_AosAttributes.WeaponSpeed) != 0)
                list.Add(1060486, prop.ToString()); // swing speed increase ~1_val~%

            if (Core.ML && (prop = m_AosAttributes.IncreasedKarmaLoss) != 0)
                list.Add(1075210, prop.ToString()); // Increased Karma Loss ~1val~%

            list.Add(1042886, m_Count.ToString()); // ~1_NUMBERS_OF_SPELLS~ Spells
        }

        public override void OnSingleClick(Mobile from)
        {
            base.OnSingleClick(from);

            if (m_Crafter != null)
                this.LabelTo(from, 1050043, m_Crafter.Name); // crafted by ~1_NAME~

            this.LabelTo(from, 1042886, m_Count.ToString());
        }

        public override void OnDoubleClick(Mobile from)
        {
            Container pack = from.Backpack;

            if (Parent == from || (pack != null && Parent == pack))
                DisplayTo(from);
            else
                from.SendLocalizedMessage(500207); // The spellbook must be in your backpack (and not in a container within) to open.
        }


        private SlayerName m_Slayer;
        private SlayerName m_Slayer2;
        //Currently though there are no dual slayer spellbooks, OSI has a habit of putting dual slayer stuff in later

        [CommandProperty(AccessLevel.GameMaster)]
        public SlayerName Slayer
        {
            get { return m_Slayer; }
            set { m_Slayer = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SlayerName Slayer2
        {
            get { return m_Slayer2; }
            set { m_Slayer2 = value; InvalidateProperties(); }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)5); // version

            writer.Write((byte)m_Quality);

            writer.Write((string)m_EngravedText);

            writer.Write(m_Crafter);

            writer.Write((int)m_Slayer);
            writer.Write((int)m_Slayer2);

            m_AosAttributes.Serialize(writer);
            m_AosSkillBonuses.Serialize(writer);

            writer.Write(m_Content);
            writer.Write(m_Count);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 5:
                    {
                        m_Quality = (BookQuality)reader.ReadByte();

                        goto case 4;
                    }
                case 4:
                    {
                        m_EngravedText = reader.ReadString();

                        goto case 3;
                    }
                case 3:
                    {
                        m_Crafter = reader.ReadMobile();
                        goto case 2;
                    }
                case 2:
                    {
                        m_Slayer = (SlayerName)reader.ReadInt();
                        m_Slayer2 = (SlayerName)reader.ReadInt();
                        goto case 1;
                    }
                case 1:
                    {
                        m_AosAttributes = new AosAttributes(this, reader);
                        m_AosSkillBonuses = new AosSkillBonuses(this, reader);

                        goto case 0;
                    }
                case 0:
                    {
                        m_Content = reader.ReadULong();
                        m_Count = reader.ReadInt();

                        break;
                    }
            }

            if (m_AosAttributes == null)
                m_AosAttributes = new AosAttributes(this);

            if (m_AosSkillBonuses == null)
                m_AosSkillBonuses = new AosSkillBonuses(this);

            if (Core.AOS && Parent is Mobile)
                m_AosSkillBonuses.AddTo((Mobile)Parent);

            int strBonus = m_AosAttributes.BonusStr;
            int dexBonus = m_AosAttributes.BonusDex;
            int intBonus = m_AosAttributes.BonusInt;

            if (Parent is Mobile && (strBonus != 0 || dexBonus != 0 || intBonus != 0))
            {
                Mobile m = (Mobile)Parent;

                string modName = Serial.ToString();

                if (strBonus != 0)
                    m.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));

                if (dexBonus != 0)
                    m.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));

                if (intBonus != 0)
                    m.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
            }

            if (Parent is Mobile)
                ((Mobile)Parent).CheckStatTimers();
        }

        private static int[] m_LegendPropertyCounts = new int[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,	// 0 properties : 21/52 : 40%
				1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,					// 1 property   : 15/52 : 29%
				2, 2, 2, 2, 2, 2, 2, 2, 2, 2,									// 2 properties : 10/52 : 19%
				3, 3, 3, 3, 3, 3												// 3 properties :  6/52 : 12%

			};

        private static int[] m_ElderPropertyCounts = new int[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,// 0 properties : 15/34 : 44%
				1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 				// 1 property   : 10/34 : 29%
				2, 2, 2, 2, 2, 2,							// 2 properties :  6/34 : 18%
				3, 3, 3										// 3 properties :  3/34 :  9%
			};

        private static int[] m_GrandPropertyCounts = new int[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,	// 0 properties : 10/20 : 50%
				1, 1, 1, 1, 1, 1,				// 1 property   :  6/20 : 30%
				2, 2, 2,						// 2 properties :  3/20 : 15%
				3								// 3 properties :  1/20 :  5%
			};

        private static int[] m_MasterPropertyCounts = new int[]
            {
                0, 0, 0, 0, 0, 0,				// 0 properties : 6/10 : 60%
				1, 1, 1,						// 1 property   : 3/10 : 30%
				2								// 2 properties : 1/10 : 10%
			};

        private static int[] m_AdeptPropertyCounts = new int[]
            {
                0, 0, 0,						// 0 properties : 3/4 : 75%
				1								// 1 property   : 1/4 : 25%
			};

        public virtual int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool, CraftItem craftItem, int resHue)
        {
            int magery = from.Skills.Magery.BaseFixedPoint;

            if (magery >= 800)
            {
                int[] propertyCounts;
                int minIntensity;
                int maxIntensity;

                if (magery >= 1000)
                {
                    if (magery >= 1200)
                        propertyCounts = m_LegendPropertyCounts;
                    else if (magery >= 1100)
                        propertyCounts = m_ElderPropertyCounts;
                    else
                        propertyCounts = m_GrandPropertyCounts;

                    minIntensity = 55;
                    maxIntensity = 75;
                }
                else if (magery >= 900)
                {
                    propertyCounts = m_MasterPropertyCounts;
                    minIntensity = 25;
                    maxIntensity = 45;
                }
                else
                {
                    propertyCounts = m_AdeptPropertyCounts;
                    minIntensity = 0;
                    maxIntensity = 15;
                }

                int propertyCount = propertyCounts[Utility.Random(propertyCounts.Length)];

                BaseRunicTool.ApplyAttributesTo(this, true, 0, propertyCount, minIntensity, maxIntensity);
            }

            if (makersMark)
                Crafter = from;

            m_Quality = (BookQuality)(quality - 1);

            return quality;
        }
    }

    public class SpellScroll : Item, ICommodity
    {
        private int m_SpellID;

        public int SpellID
        {
            get
            {
                return m_SpellID;
            }
        }

        int ICommodity.DescriptionNumber { get { return LabelNumber; } }
        bool ICommodity.IsDeedable { get { return (Core.ML); } }

        public SpellScroll(Serial serial)
            : base(serial)
        {
        }

        [Constructable]
        public SpellScroll(int spellID, int itemID)
            : this(spellID, itemID, 1)
        {
        }

        [Constructable]
        public SpellScroll(int spellID, int itemID, int amount)
            : base(itemID)
        {
            Stackable = true;
            Weight = 1.0;
            Amount = amount;

            m_SpellID = spellID;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write((int)m_SpellID);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_SpellID = reader.ReadInt();

                        break;
                    }
            }
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);

            if (from.Alive && this.Movable)
                list.Add(new ContextMenus.AddToSpellbookEntry());
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!Multis.DesignContext.Check(from))
                return; // They are customizing

            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
                return;
            }

            Spell spell = SpellRegistry.NewSpell(m_SpellID, from, this);

            if (spell != null)
                spell.Cast();
            else
                from.SendLocalizedMessage(502345); // This spell has been temporarily disabled.
        }
    }

    public class TransientItem : Item
    {
        private TimeSpan m_LifeSpan;

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan LifeSpan
        {
            get { return m_LifeSpan; }
            set { m_LifeSpan = value; }
        }

        private DateTime m_CreationTime;

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime CreationTime
        {
            get { return m_CreationTime; }
            set { m_CreationTime = value; }
        }

        private Timer m_Timer;

        public override bool Nontransferable { get { return true; } }
        public override void HandleInvalidTransfer(Mobile from)
        {
            if (InvalidTransferMessage != null)
                TextDefinition.SendMessageTo(from, InvalidTransferMessage);

            this.Delete();
        }

        public virtual TextDefinition InvalidTransferMessage { get { return null; } }


        public virtual void Expire(Mobile parent)
        {
            if (parent != null)
                parent.SendLocalizedMessage(1072515, (this.Name == null ? String.Format("#{0}", LabelNumber) : this.Name)); // The ~1_name~ expired...

            Effects.PlaySound(GetWorldLocation(), Map, 0x201);

            this.Delete();
        }

        public virtual void SendTimeRemainingMessage(Mobile to)
        {
            to.SendLocalizedMessage(1072516, String.Format("{0}\t{1}", (this.Name == null ? String.Format("#{0}", LabelNumber) : this.Name), (int)m_LifeSpan.TotalSeconds)); // ~1_name~ will expire in ~2_val~ seconds!
        }

        public override void OnDelete()
        {
            if (m_Timer != null)
                m_Timer.Stop();

            base.OnDelete();
        }

        public virtual void CheckExpiry()
        {
            if ((m_CreationTime + m_LifeSpan) < DateTime.UtcNow)
                Expire(RootParent as Mobile);
            else
                InvalidateProperties();
        }

        [Constructable]
        public TransientItem(int itemID, TimeSpan lifeSpan)
            : base(itemID)
        {
            m_CreationTime = DateTime.UtcNow;
            m_LifeSpan = lifeSpan;

            m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5), new TimerCallback(CheckExpiry));
        }

        public TransientItem(Serial serial)
            : base(serial)
        {
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            TimeSpan remaining = ((m_CreationTime + m_LifeSpan) - DateTime.UtcNow);

            list.Add(1072517, ((int)remaining.TotalSeconds).ToString()); // Lifespan: ~1_val~ seconds
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);

            writer.Write(m_LifeSpan);
            writer.Write(m_CreationTime);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            m_LifeSpan = reader.ReadTimeSpan();
            m_CreationTime = reader.ReadDateTime();

            m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5), new TimerCallback(CheckExpiry));
        }
    }

    public enum PotionEffect
    {
        Nightsight,
        CureLesser,
        Cure,
        CureGreater,
        Agility,
        AgilityGreater,
        Strength,
        StrengthGreater,
        PoisonLesser,
        Poison,
        PoisonGreater,
        PoisonDeadly,
        Refresh,
        RefreshTotal,
        HealLesser,
        Heal,
        HealGreater,
        ExplosionLesser,
        Explosion,
        ExplosionGreater,
        Conflagration,
        ConflagrationGreater,
        MaskOfDeath,		// Mask of Death is not available in OSI but does exist in cliloc files
        MaskOfDeathGreater,	// included in enumeration for compatability if later enabled by OSI
        ConfusionBlast,
        ConfusionBlastGreater,
        Invisibility,
        Parasitic,
        Darkglow,
    }

    public abstract class BasePotion : Item, ICraftable, ICommodity
    {
        private PotionEffect m_PotionEffect;

        public PotionEffect PotionEffect
        {
            get
            {
                return m_PotionEffect;
            }
            set
            {
                m_PotionEffect = value;
                InvalidateProperties();
            }
        }

        int ICommodity.DescriptionNumber { get { return LabelNumber; } }
        bool ICommodity.IsDeedable { get { return (Core.ML); } }

        public override int LabelNumber { get { return 1041314 + (int)m_PotionEffect; } }

        public BasePotion(int itemID, PotionEffect effect)
            : base(itemID)
        {
            m_PotionEffect = effect;

            Stackable = Core.ML;
            Weight = 1.0;
        }

        public BasePotion(Serial serial)
            : base(serial)
        {
        }

        public virtual bool RequireFreeHand { get { return true; } }

        public static bool HasFreeHand(Mobile m)
        {
            Item handOne = m.FindItemOnLayer(Layer.OneHanded);
            Item handTwo = m.FindItemOnLayer(Layer.TwoHanded);

            if (handTwo is BaseWeapon)
                handOne = handTwo;
            if (handTwo is BaseRanged)
            {
                BaseRanged ranged = (BaseRanged)handTwo;

                if (ranged.Balanced)
                    return true;
            }

            return (handOne == null || handTwo == null);
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!Movable)
                return;

            if (from.InRange(this.GetWorldLocation(), 1))
            {
                if (!RequireFreeHand || HasFreeHand(from))
                {
                    if (this is BaseExplosionPotion && Amount > 1)
                    {
                        BasePotion pot = (BasePotion)Activator.CreateInstance(this.GetType());

                        if (pot != null)
                        {
                            Amount--;

                            if (from.Backpack != null && !from.Backpack.Deleted)
                            {
                                from.Backpack.DropItem(pot);
                            }
                            else
                            {
                                pot.MoveToWorld(from.Location, from.Map);
                            }
                            pot.Drink(from);
                        }
                    }
                    else
                    {
                        this.Drink(from);
                    }
                }
                else
                {
                    from.SendLocalizedMessage(502172); // You must have a free hand to drink a potion.
                }
            }
            else
            {
                from.SendLocalizedMessage(502138); // That is too far away for you to use
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version

            writer.Write((int)m_PotionEffect);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                case 0:
                    {
                        m_PotionEffect = (PotionEffect)reader.ReadInt();
                        break;
                    }
            }

            if (version == 0)
                Stackable = Core.ML;
        }

        public abstract void Drink(Mobile from);

        public static void PlayDrinkEffect(Mobile m)
        {
            m.RevealingAction();

            m.PlaySound(0x2D6);

            #region Dueling
            if (!Engines.ConPVP.DuelContext.IsFreeConsume(m))
                m.AddToBackpack(new Bottle());
            #endregion

            if (m.Body.IsHuman && !m.Mounted)
                m.Animate(34, 5, 1, true, false, 0);
        }

        public static int EnhancePotions(Mobile m)
        {
            int EP = AosAttributes.GetValue(m, AosAttribute.EnhancePotions);
            int skillBonus = m.Skills.Alchemy.Fixed / 330 * 10;

            if (Core.ML && EP > 50 && m.AccessLevel <= AccessLevel.Player)
                EP = 50;

            return (EP + skillBonus);
        }

        public static TimeSpan Scale(Mobile m, TimeSpan v)
        {
            if (!Core.AOS)
                return v;

            double scalar = 1.0 + (0.01 * EnhancePotions(m));

            return TimeSpan.FromSeconds(v.TotalSeconds * scalar);
        }

        public static double Scale(Mobile m, double v)
        {
            if (!Core.AOS)
                return v;

            double scalar = 1.0 + (0.01 * EnhancePotions(m));

            return v * scalar;
        }

        public static int Scale(Mobile m, int v)
        {
            if (!Core.AOS)
                return v;

            return AOS.Scale(v, 100 + EnhancePotions(m));
        }

        public override bool StackWith(Mobile from, Item dropped, bool playSound)
        {
            if (dropped is BasePotion && ((BasePotion)dropped).m_PotionEffect == m_PotionEffect)
                return base.StackWith(from, dropped, playSound);

            return false;
        }

        #region ICraftable Members

        public int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool, CraftItem craftItem, int resHue)
        {
            if (craftSystem is DefAlchemy)
            {
                Container pack = from.Backpack;

                if (pack != null)
                {
                    if ((int)PotionEffect >= (int)PotionEffect.Invisibility)
                        return 1;

                    List<PotionKeg> kegs = pack.FindItemsByType<PotionKeg>();

                    for (int i = 0; i < kegs.Count; ++i)
                    {
                        PotionKeg keg = kegs[i];

                        if (keg == null)
                            continue;

                        if (keg.Held <= 0 || keg.Held >= 100)
                            continue;

                        if (keg.Type != PotionEffect)
                            continue;

                        ++keg.Held;

                        Consume();
                        from.AddToBackpack(new Bottle());

                        return -1; // signal placed in keg
                    }
                }
            }

            return 1;
        }

        #endregion
    }

    #region Potion Types

    /// <summary>
    /// Agility Potions
    /// </summary>
    public abstract class BaseAgilityPotion : BasePotion
    {
        public abstract int DexOffset { get; }
        public abstract TimeSpan Duration { get; }

        public BaseAgilityPotion(PotionEffect effect)
            : base(0xF08, effect)
        {
        }

        public BaseAgilityPotion(Serial serial)
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

        public bool DoAgility(Mobile from)
        {
            // TODO: Verify scaled; is it offset, duration, or both?
            if (Spells.SpellHelper.AddStatOffset(from, StatType.Dex, Scale(from, DexOffset), Duration))
            {
                from.FixedEffect(0x375A, 10, 15);
                from.PlaySound(0x1E7);
                return true;
            }

            from.SendLocalizedMessage(502173); // You are already under a similar effect.
            return false;
        }

        public override void Drink(Mobile from)
        {
            if (DoAgility(from))
            {
                BasePotion.PlayDrinkEffect(from);

                if (!Engines.ConPVP.DuelContext.IsFreeConsume(from))
                    this.Consume();
            }
        }
    }

    /// <summary>
    /// Conflagration Potions
    /// </summary>
    public abstract class BaseConflagrationPotion : BasePotion
    {
        public abstract int MinDamage { get; }
        public abstract int MaxDamage { get; }

        public override bool RequireFreeHand { get { return false; } }

        public BaseConflagrationPotion(PotionEffect effect)
            : base(0xF06, effect)
        {
            Hue = 0x489;
        }

        public BaseConflagrationPotion(Serial serial)
            : base(serial)
        {
        }

        public override void Drink(Mobile from)
        {
            if (Core.AOS && (from.Paralyzed || from.Frozen || (from.Spell != null && from.Spell.IsCasting)))
            {
                from.SendLocalizedMessage(1062725); // You can not use that potion while paralyzed.
                return;
            }

            int delay = GetDelay(from);

            if (delay > 0)
            {
                from.SendLocalizedMessage(1072529, String.Format("{0}\t{1}", delay, delay > 1 ? "seconds." : "second.")); // You cannot use that for another ~1_NUM~ ~2_TIMEUNITS~
                return;
            }

            ThrowTarget targ = from.Target as ThrowTarget;

            if (targ != null && targ.Potion == this)
                return;

            from.RevealingAction();

            if (!m_Users.Contains(from))
                m_Users.Add(from);

            from.Target = new ThrowTarget(this);
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

        private List<Mobile> m_Users = new List<Mobile>();

        public void Explode_Callback(object state)
        {
            object[] states = (object[])state;

            Explode((Mobile)states[0], (Point3D)states[1], (Map)states[2]);
        }

        public virtual void Explode(Mobile from, Point3D loc, Map map)
        {
            if (Deleted || map == null)
                return;

            Consume();

            // Check if any other players are using this potion
            for (int i = 0; i < m_Users.Count; i++)
            {
                ThrowTarget targ = m_Users[i].Target as ThrowTarget;

                if (targ != null && targ.Potion == this)
                    Target.Cancel(from);
            }

            // Effects
            Effects.PlaySound(loc, map, 0x20C);

            for (int i = -2; i <= 2; i++)
            {
                for (int j = -2; j <= 2; j++)
                {
                    Point3D p = new Point3D(loc.X + i, loc.Y + j, loc.Z);

                    if (map.CanFit(p, 12, true, false) && from.InLOS(p))
                        new InternalItem(from, p, map, MinDamage, MaxDamage);
                }
            }
        }

        #region Delay
        private static Hashtable m_Delay = new Hashtable();

        public static void AddDelay(Mobile m)
        {
            Timer timer = m_Delay[m] as Timer;

            if (timer != null)
                timer.Stop();

            m_Delay[m] = Timer.DelayCall(TimeSpan.FromSeconds(30), new TimerStateCallback(EndDelay_Callback), m);
        }

        public static int GetDelay(Mobile m)
        {
            Timer timer = m_Delay[m] as Timer;

            if (timer != null && timer.Next > DateTime.UtcNow)
                return (int)(timer.Next - DateTime.UtcNow).TotalSeconds;

            return 0;
        }

        private static void EndDelay_Callback(object obj)
        {
            if (obj is Mobile)
                EndDelay((Mobile)obj);
        }

        public static void EndDelay(Mobile m)
        {
            Timer timer = m_Delay[m] as Timer;

            if (timer != null)
            {
                timer.Stop();
                m_Delay.Remove(m);
            }
        }
        #endregion

        private class ThrowTarget : Target
        {
            private BaseConflagrationPotion m_Potion;

            public BaseConflagrationPotion Potion
            {
                get { return m_Potion; }
            }

            public ThrowTarget(BaseConflagrationPotion potion)
                : base(12, true, TargetFlags.None)
            {
                m_Potion = potion;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (m_Potion.Deleted || m_Potion.Map == Map.Internal)
                    return;

                IPoint3D p = targeted as IPoint3D;

                if (p == null || from.Map == null)
                    return;

                // Add delay
                BaseConflagrationPotion.AddDelay(from);

                SpellHelper.GetSurfaceTop(ref p);

                from.RevealingAction();

                IEntity to;

                if (p is Mobile)
                    to = (Mobile)p;
                else
                    to = new Entity(Serial.Zero, new Point3D(p), from.Map);

                Effects.SendMovingEffect(from, to, 0xF0D, 7, 0, false, false, m_Potion.Hue, 0);
                Timer.DelayCall(TimeSpan.FromSeconds(1.5), new TimerStateCallback(m_Potion.Explode_Callback), new object[] { from, new Point3D(p), from.Map });
            }
        }

        public class InternalItem : Item
        {
            private Mobile m_From;
            private int m_MinDamage;
            private int m_MaxDamage;
            private DateTime m_End;
            private Timer m_Timer;

            public Mobile From { get { return m_From; } }

            public override bool BlocksFit { get { return true; } }

            public InternalItem(Mobile from, Point3D loc, Map map, int min, int max)
                : base(0x398C)
            {
                Movable = false;
                Light = LightType.Circle300;

                MoveToWorld(loc, map);

                m_From = from;
                m_End = DateTime.UtcNow + TimeSpan.FromSeconds(10);

                SetDamage(min, max);

                m_Timer = new InternalTimer(this, m_End);
                m_Timer.Start();
            }

            public override void OnAfterDelete()
            {
                base.OnAfterDelete();

                if (m_Timer != null)
                    m_Timer.Stop();
            }

            public InternalItem(Serial serial)
                : base(serial)
            {
            }

            public int GetDamage() { return Utility.RandomMinMax(m_MinDamage, m_MaxDamage); }

            private void SetDamage(int min, int max)
            {
                /* 	new way to apply alchemy bonus according to Stratics' calculator.
                    this gives a mean to values 25, 50, 75 and 100. Stratics' calculator is outdated.
                    Those goals will give 2 to alchemy bonus. It's not really OSI-like but it's an approximation. */

                m_MinDamage = min;
                m_MaxDamage = max;

                if (m_From == null)
                    return;

                int alchemySkill = m_From.Skills.Alchemy.Fixed;
                int alchemyBonus = alchemySkill / 125 + alchemySkill / 250;

                m_MinDamage = Scale(m_From, m_MinDamage + alchemyBonus);
                m_MaxDamage = Scale(m_From, m_MaxDamage + alchemyBonus);
            }

            public override void Serialize(GenericWriter writer)
            {
                base.Serialize(writer);

                writer.Write((int)0); // version

                writer.Write((Mobile)m_From);
                writer.Write((DateTime)m_End);
                writer.Write((int)m_MinDamage);
                writer.Write((int)m_MaxDamage);
            }

            public override void Deserialize(GenericReader reader)
            {
                base.Deserialize(reader);

                int version = reader.ReadInt();

                m_From = reader.ReadMobile();
                m_End = reader.ReadDateTime();
                m_MinDamage = reader.ReadInt();
                m_MaxDamage = reader.ReadInt();

                m_Timer = new InternalTimer(this, m_End);
                m_Timer.Start();
            }

            public override bool OnMoveOver(Mobile m)
            {
                if (Visible && m_From != null && (!Core.AOS || m != m_From) && SpellHelper.ValidIndirectTarget(m_From, m) && m_From.CanBeHarmful(m, false))
                {
                    m_From.DoHarmful(m);

                    AOS.Damage(m, m_From, GetDamage(), 0, 100, 0, 0, 0);
                    m.PlaySound(0x208);
                }

                return true;
            }

            private class InternalTimer : Timer
            {
                private InternalItem m_Item;
                private DateTime m_End;

                public InternalTimer(InternalItem item, DateTime end)
                    : base(TimeSpan.Zero, TimeSpan.FromSeconds(1.0))
                {
                    m_Item = item;
                    m_End = end;

                    Priority = TimerPriority.FiftyMS;
                }

                protected override void OnTick()
                {
                    if (m_Item.Deleted)
                        return;

                    if (DateTime.UtcNow > m_End)
                    {
                        m_Item.Delete();
                        Stop();
                        return;
                    }

                    Mobile from = m_Item.From;

                    if (m_Item.Map == null || from == null)
                        return;

                    List<Mobile> mobiles = new List<Mobile>();

                    foreach (Mobile mobile in m_Item.GetMobilesInRange(0))
                        mobiles.Add(mobile);

                    for (int i = 0; i < mobiles.Count; i++)
                    {
                        Mobile m = mobiles[i];

                        if ((m.Z + 16) > m_Item.Z && (m_Item.Z + 12) > m.Z && (!Core.AOS || m != from) && SpellHelper.ValidIndirectTarget(from, m) && from.CanBeHarmful(m, false))
                        {
                            if (from != null)
                                from.DoHarmful(m);

                            AOS.Damage(m, from, m_Item.GetDamage(), 0, 100, 0, 0, 0);
                            m.PlaySound(0x208);
                        }
                    }
                }
            }
        }
    }

    public abstract class BaseConfusionBlastPotion : BasePotion
    {
        public abstract int Radius { get; }

        public override bool RequireFreeHand { get { return false; } }

        public BaseConfusionBlastPotion(PotionEffect effect)
            : base(0xF06, effect)
        {
            Hue = 0x48D;
        }

        public BaseConfusionBlastPotion(Serial serial)
            : base(serial)
        {
        }

        public override void Drink(Mobile from)
        {
            if (Core.AOS && (from.Paralyzed || from.Frozen || (from.Spell != null && from.Spell.IsCasting)))
            {
                from.SendLocalizedMessage(1062725); // You can not use that potion while paralyzed.
                return;
            }

            int delay = GetDelay(from);

            if (delay > 0)
            {
                from.SendLocalizedMessage(1072529, String.Format("{0}\t{1}", delay, delay > 1 ? "seconds." : "second.")); // You cannot use that for another ~1_NUM~ ~2_TIMEUNITS~
                return;
            }

            ThrowTarget targ = from.Target as ThrowTarget;

            if (targ != null && targ.Potion == this)
                return;

            from.RevealingAction();

            if (!m_Users.Contains(from))
                m_Users.Add(from);

            from.Target = new ThrowTarget(this);
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

        private List<Mobile> m_Users = new List<Mobile>();

        public void Explode_Callback(object state)
        {
            object[] states = (object[])state;

            Explode((Mobile)states[0], (Point3D)states[1], (Map)states[2]);
        }

        public virtual void Explode(Mobile from, Point3D loc, Map map)
        {
            if (Deleted || map == null)
                return;

            Consume();

            // Check if any other players are using this potion
            for (int i = 0; i < m_Users.Count; i++)
            {
                ThrowTarget targ = m_Users[i].Target as ThrowTarget;

                if (targ != null && targ.Potion == this)
                    Target.Cancel(from);
            }

            // Effects
            Effects.PlaySound(loc, map, 0x207);

            Geometry.Circle2D(loc, map, Radius, new DoEffect_Callback(BlastEffect), 270, 90);

            Timer.DelayCall(TimeSpan.FromSeconds(0.3), new TimerStateCallback(CircleEffect2), new object[] { loc, map });

            foreach (Mobile mobile in map.GetMobilesInRange(loc, Radius))
            {
                if (mobile is BaseCreature)
                {
                    BaseCreature mon = (BaseCreature)mobile;

                    if (mon.Controlled || mon.Summoned)
                        continue;

                    mon.Pacify(from, DateTime.UtcNow + TimeSpan.FromSeconds(5.0)); // TODO check
                }
            }
        }

        #region Effects
        public virtual void BlastEffect(Point3D p, Map map)
        {
            if (map.CanFit(p, 12, true, false))
                Effects.SendLocationEffect(p, map, 0x376A, 4, 9);
        }

        public void CircleEffect2(object state)
        {
            object[] states = (object[])state;

            Geometry.Circle2D((Point3D)states[0], (Map)states[1], Radius, new DoEffect_Callback(BlastEffect), 90, 270);
        }
        #endregion

        #region Delay
        private static Hashtable m_Delay = new Hashtable();

        public static void AddDelay(Mobile m)
        {
            Timer timer = m_Delay[m] as Timer;

            if (timer != null)
                timer.Stop();

            m_Delay[m] = Timer.DelayCall(TimeSpan.FromSeconds(60), new TimerStateCallback(EndDelay_Callback), m);
        }

        public static int GetDelay(Mobile m)
        {
            Timer timer = m_Delay[m] as Timer;

            if (timer != null && timer.Next > DateTime.UtcNow)
                return (int)(timer.Next - DateTime.UtcNow).TotalSeconds;

            return 0;
        }

        private static void EndDelay_Callback(object obj)
        {
            if (obj is Mobile)
                EndDelay((Mobile)obj);
        }

        public static void EndDelay(Mobile m)
        {
            Timer timer = m_Delay[m] as Timer;

            if (timer != null)
            {
                timer.Stop();
                m_Delay.Remove(m);
            }
        }
        #endregion

        private class ThrowTarget : Target
        {
            private BaseConfusionBlastPotion m_Potion;

            public BaseConfusionBlastPotion Potion
            {
                get { return m_Potion; }
            }

            public ThrowTarget(BaseConfusionBlastPotion potion)
                : base(12, true, TargetFlags.None)
            {
                m_Potion = potion;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (m_Potion.Deleted || m_Potion.Map == Map.Internal)
                    return;

                IPoint3D p = targeted as IPoint3D;

                if (p == null || from.Map == null)
                    return;

                // Add delay
                BaseConfusionBlastPotion.AddDelay(from);

                SpellHelper.GetSurfaceTop(ref p);

                from.RevealingAction();

                IEntity to;

                if (p is Mobile)
                    to = (Mobile)p;
                else
                    to = new Entity(Serial.Zero, new Point3D(p), from.Map);

                Effects.SendMovingEffect(from, to, 0xF0D, 7, 0, false, false, m_Potion.Hue, 0);
                Timer.DelayCall(TimeSpan.FromSeconds(1.0), new TimerStateCallback(m_Potion.Explode_Callback), new object[] { from, new Point3D(p), from.Map });
            }
        }
    }

    /// <summary>
    /// Cure Potions
    /// </summary>
    public class CureLevelInfo
    {
        private Poison m_Poison;
        private double m_Chance;

        public Poison Poison
        {
            get { return m_Poison; }
        }

        public double Chance
        {
            get { return m_Chance; }
        }

        public CureLevelInfo(Poison poison, double chance)
        {
            m_Poison = poison;
            m_Chance = chance;
        }
    }

    public abstract class BaseCurePotion : BasePotion
    {
        public abstract CureLevelInfo[] LevelInfo { get; }

        public BaseCurePotion(PotionEffect effect)
            : base(0xF07, effect)
        {
        }

        public BaseCurePotion(Serial serial)
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

        public void DoCure(Mobile from)
        {
            bool cure = false;

            CureLevelInfo[] info = LevelInfo;

            for (int i = 0; i < info.Length; ++i)
            {
                CureLevelInfo li = info[i];

                if (li.Poison == from.Poison && Scale(from, li.Chance) > Utility.RandomDouble())
                {
                    cure = true;
                    break;
                }
            }

            if (cure && from.CurePoison(from))
            {
                from.SendLocalizedMessage(500231); // You feel cured of poison!

                from.FixedEffect(0x373A, 10, 15);
                from.PlaySound(0x1E0);
            }
            else if (!cure)
            {
                from.SendLocalizedMessage(500232); // That potion was not strong enough to cure your ailment!
            }
        }

        public override void Drink(Mobile from)
        {
            if (TransformationSpellHelper.UnderTransformation(from, typeof(Spells.Necromancy.VampiricEmbraceSpell)))
            {
                from.SendLocalizedMessage(1061652); // The garlic in the potion would surely kill you.
            }
            else if (from.Poisoned)
            {
                DoCure(from);

                BasePotion.PlayDrinkEffect(from);

                from.FixedParticles(0x373A, 10, 15, 5012, EffectLayer.Waist);
                from.PlaySound(0x1E0);

                if (!Engines.ConPVP.DuelContext.IsFreeConsume(from))
                    this.Consume();
            }
            else
            {
                from.SendLocalizedMessage(1042000); // You are not poisoned.
            }
        }
    }

    /// <summary>
    /// Explosion Potions
    /// </summary>
    public abstract class BaseExplosionPotion : BasePotion
    {
        public abstract int MinDamage { get; }
        public abstract int MaxDamage { get; }

        public override bool RequireFreeHand { get { return false; } }

        private static bool LeveledExplosion = false; // Should explosion potions explode other nearby potions?
        private static bool InstantExplosion = false; // Should explosion potions explode on impact?
        private static bool RelativeLocation = false; // Is the explosion target location relative for mobiles?
        private const int ExplosionRange = 2; // How long is the blast radius?

        public BaseExplosionPotion(PotionEffect effect)
            : base(0xF0D, effect)
        {
        }

        public BaseExplosionPotion(Serial serial)
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

        public virtual object FindParent(Mobile from)
        {
            Mobile m = this.HeldBy;

            if (m != null && m.Holding == this)
                return m;

            object obj = this.RootParent;

            if (obj != null)
                return obj;

            if (Map == Map.Internal)
                return from;

            return this;
        }

        private Timer m_Timer;

        public List<Mobile> Users { get { return m_Users; } }

        private List<Mobile> m_Users;

        public override void Drink(Mobile from)
        {
            if (Core.AOS && (from.Paralyzed || from.Frozen || (from.Spell != null && from.Spell.IsCasting)))
            {
                from.SendLocalizedMessage(1062725); // You can not use a purple potion while paralyzed.
                return;
            }

            ThrowTarget targ = from.Target as ThrowTarget;
            this.Stackable = false; // Scavenged explosion potions won't stack with those ones in backpack, and still will explode.

            if (targ != null && targ.Potion == this)
                return;

            from.RevealingAction();

            if (m_Users == null)
                m_Users = new List<Mobile>();

            if (!m_Users.Contains(from))
                m_Users.Add(from);

            from.Target = new ThrowTarget(this);

            if (m_Timer == null)
            {
                from.SendLocalizedMessage(500236); // You should throw it now!

                if (Core.ML)
                    m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.25), 5, new TimerStateCallback(Detonate_OnTick), new object[] { from, 3 }); // 3.6 seconds explosion delay
                else
                    m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(0.75), TimeSpan.FromSeconds(1.0), 4, new TimerStateCallback(Detonate_OnTick), new object[] { from, 3 }); // 2.6 seconds explosion delay
            }
        }

        private void Detonate_OnTick(object state)
        {
            if (Deleted)
                return;

            object[] states = (object[])state;
            Mobile from = (Mobile)states[0];
            int timer = (int)states[1];

            object parent = FindParent(from);

            if (timer == 0)
            {
                Point3D loc;
                Map map;

                if (parent is Item)
                {
                    Item item = (Item)parent;

                    loc = item.GetWorldLocation();
                    map = item.Map;
                }
                else if (parent is Mobile)
                {
                    Mobile m = (Mobile)parent;

                    loc = m.Location;
                    map = m.Map;
                }
                else
                {
                    return;
                }

                Explode(from, true, loc, map);
                m_Timer = null;
            }
            else
            {
                if (parent is Item)
                    ((Item)parent).PublicOverheadMessage(MessageType.Regular, 0x22, false, timer.ToString());
                else if (parent is Mobile)
                    ((Mobile)parent).PublicOverheadMessage(MessageType.Regular, 0x22, false, timer.ToString());

                states[1] = timer - 1;
            }
        }

        private void Reposition_OnTick(object state)
        {
            if (Deleted)
                return;

            object[] states = (object[])state;
            Mobile from = (Mobile)states[0];
            IPoint3D p = (IPoint3D)states[1];
            Map map = (Map)states[2];

            Point3D loc = new Point3D(p);

            if (InstantExplosion)
                Explode(from, true, loc, map);
            else
                MoveToWorld(loc, map);
        }

        private class ThrowTarget : Target
        {
            private BaseExplosionPotion m_Potion;

            public BaseExplosionPotion Potion
            {
                get { return m_Potion; }
            }

            public ThrowTarget(BaseExplosionPotion potion)
                : base(12, true, TargetFlags.None)
            {
                m_Potion = potion;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (m_Potion.Deleted || m_Potion.Map == Map.Internal)
                    return;

                IPoint3D p = targeted as IPoint3D;

                if (p == null)
                    return;

                Map map = from.Map;

                if (map == null)
                    return;

                SpellHelper.GetSurfaceTop(ref p);

                from.RevealingAction();

                IEntity to;

                to = new Entity(Serial.Zero, new Point3D(p), map);

                if (p is Mobile)
                {
                    if (!RelativeLocation) // explosion location = current mob location. 
                        p = ((Mobile)p).Location;
                    else
                        to = (Mobile)p;
                }

                Effects.SendMovingEffect(from, to, m_Potion.ItemID, 7, 0, false, false, m_Potion.Hue, 0);

                if (m_Potion.Amount > 1)
                {
                    Mobile.LiftItemDupe(m_Potion, 1);
                }

                m_Potion.Internalize();
                Timer.DelayCall(TimeSpan.FromSeconds(1.0), new TimerStateCallback(m_Potion.Reposition_OnTick), new object[] { from, p, map });
            }
        }

        public void Explode(Mobile from, bool direct, Point3D loc, Map map)
        {
            if (Deleted)
                return;

            Consume();

            for (int i = 0; m_Users != null && i < m_Users.Count; ++i)
            {
                Mobile m = m_Users[i];
                ThrowTarget targ = m.Target as ThrowTarget;

                if (targ != null && targ.Potion == this)
                    Target.Cancel(m);
            }

            if (map == null)
                return;

            Effects.PlaySound(loc, map, 0x307);

            Effects.SendLocationEffect(loc, map, 0x36B0, 9, 10, 0, 0);
            int alchemyBonus = 0;

            if (direct)
                alchemyBonus = (int)(from.Skills.Alchemy.Value / (Core.AOS ? 5 : 10));

            IPooledEnumerable eable = LeveledExplosion ? (IPooledEnumerable)map.GetObjectsInRange(loc, ExplosionRange) : (IPooledEnumerable)map.GetMobilesInRange(loc, ExplosionRange);
            ArrayList toExplode = new ArrayList();

            int toDamage = 0;

            foreach (object o in eable)
            {
                if (o is Mobile && (from == null || (SpellHelper.ValidIndirectTarget(from, (Mobile)o) && from.CanBeHarmful((Mobile)o, false))))
                {
                    toExplode.Add(o);
                    ++toDamage;
                }
                else if (o is BaseExplosionPotion && o != this)
                {
                    toExplode.Add(o);
                }
            }

            eable.Free();

            int min = Scale(from, MinDamage);
            int max = Scale(from, MaxDamage);

            for (int i = 0; i < toExplode.Count; ++i)
            {
                object o = toExplode[i];

                if (o is Mobile)
                {
                    Mobile m = (Mobile)o;

                    if (from != null)
                        from.DoHarmful(m);

                    int damage = Utility.RandomMinMax(min, max);

                    damage += alchemyBonus;

                    if (!Core.AOS && damage > 40)
                        damage = 40;
                    else if (Core.AOS && toDamage > 2)
                        damage /= toDamage - 1;

                    AOS.Damage(m, from, damage, 0, 100, 0, 0, 0);
                }
                else if (o is BaseExplosionPotion)
                {
                    BaseExplosionPotion pot = (BaseExplosionPotion)o;

                    pot.Explode(from, false, pot.GetWorldLocation(), pot.Map);
                }
            }
        }
    }

    /// <summary>
    /// Heal Potions
    /// </summary>
    public abstract class BaseHealPotion : BasePotion
    {
        public abstract int MinHeal { get; }
        public abstract int MaxHeal { get; }
        public abstract double Delay { get; }

        public BaseHealPotion(PotionEffect effect)
            : base(0xF0C, effect)
        {
        }

        public BaseHealPotion(Serial serial)
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

        public void DoHeal(Mobile from)
        {
            int min = Scale(from, MinHeal);
            int max = Scale(from, MaxHeal);

            from.Heal(Utility.RandomMinMax(min, max));
        }

        public override void Drink(Mobile from)
        {
            if (from.Hits < from.HitsMax)
            {
                if (from.Poisoned || MortalStrike.IsWounded(from))
                {
                    from.LocalOverheadMessage(MessageType.Regular, 0x22, 1005000); // You can not heal yourself in your current state.
                }
                else
                {
                    if (from.BeginAction(typeof(BaseHealPotion)))
                    {
                        DoHeal(from);

                        BasePotion.PlayDrinkEffect(from);

                        if (!Engines.ConPVP.DuelContext.IsFreeConsume(from))
                            this.Consume();

                        Timer.DelayCall(TimeSpan.FromSeconds(Delay), new TimerStateCallback(ReleaseHealLock), from);
                    }
                    else
                    {
                        from.LocalOverheadMessage(MessageType.Regular, 0x22, 500235); // You must wait 10 seconds before using another healing potion.
                    }
                }
            }
            else
            {
                from.SendLocalizedMessage(1049547); // You decide against drinking this potion, as you are already at full health.
            }
        }

        private static void ReleaseHealLock(object state)
        {
            ((Mobile)state).EndAction(typeof(BaseHealPotion));
        }
    }

    /// <summary>
    /// Poison Potions
    /// </summary>
    public abstract class BasePoisonPotion : BasePotion
    {
        public abstract Poison Poison { get; }

        public abstract double MinPoisoningSkill { get; }
        public abstract double MaxPoisoningSkill { get; }

        public BasePoisonPotion(PotionEffect effect)
            : base(0xF0A, effect)
        {
        }

        public BasePoisonPotion(Serial serial)
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

        public void DoPoison(Mobile from)
        {
            from.ApplyPoison(from, Poison);
        }

        public override void Drink(Mobile from)
        {
            DoPoison(from);

            BasePotion.PlayDrinkEffect(from);

            if (!Engines.ConPVP.DuelContext.IsFreeConsume(from))
                this.Consume();
        }
    }

    /// <summary>
    /// Refresh Potions
    /// </summary>
    public abstract class BaseRefreshPotion : BasePotion
    {
        public abstract double Refresh { get; }

        public BaseRefreshPotion(PotionEffect effect)
            : base(0xF0B, effect)
        {
        }

        public BaseRefreshPotion(Serial serial)
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

        public override void Drink(Mobile from)
        {
            if (from.Stam < from.StamMax)
            {
                from.Stam += Scale(from, (int)(Refresh * from.StamMax));

                BasePotion.PlayDrinkEffect(from);

                if (!Engines.ConPVP.DuelContext.IsFreeConsume(from))
                    this.Consume();
            }
            else
            {
                from.SendMessage("You decide against drinking this potion, as you are already at full stamina.");
            }
        }
    }

    /// <summary>
    /// Strength Potions
    /// </summary>
    public abstract class BaseStrengthPotion : BasePotion
    {
        public abstract int StrOffset { get; }
        public abstract TimeSpan Duration { get; }

        public BaseStrengthPotion(PotionEffect effect)
            : base(0xF09, effect)
        {
        }

        public BaseStrengthPotion(Serial serial)
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

        public bool DoStrength(Mobile from)
        {
            // TODO: Verify scaled; is it offset, duration, or both?
            if (Spells.SpellHelper.AddStatOffset(from, StatType.Str, Scale(from, StrOffset), Duration))
            {
                from.FixedEffect(0x375A, 10, 15);
                from.PlaySound(0x1E7);
                return true;
            }

            from.SendLocalizedMessage(502173); // You are already under a similar effect.
            return false;
        }

        public override void Drink(Mobile from)
        {
            if (DoStrength(from))
            {
                BasePotion.PlayDrinkEffect(from);

                if (!Engines.ConPVP.DuelContext.IsFreeConsume(from))
                    this.Consume();
            }
        }
    }

    #endregion
}

namespace Server.Misc
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DispellableAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class DispellableFieldAttribute : Attribute
    {
    }
}

namespace Server.Mobiles
{
    public class Clone : BaseCreature
    {
        private Mobile m_Caster;

        public Clone(Mobile caster)
            : base(AIType.AI_Melee, FightMode.None, 10, 1, 0.2, 0.4)
        {
            m_Caster = caster;

            Body = caster.Body;

            Hue = caster.Hue;
            Female = caster.Female;

            Name = caster.Name;
            NameHue = caster.NameHue;

            Title = caster.Title;
            Kills = caster.Kills;

            HairItemID = caster.HairItemID;
            HairHue = caster.HairHue;

            FacialHairItemID = caster.FacialHairItemID;
            FacialHairHue = caster.FacialHairHue;

            for (int i = 0; i < caster.Skills.Length; ++i)
            {
                Skills[i].Base = caster.Skills[i].Base;
                Skills[i].Cap = caster.Skills[i].Cap;
            }

            for (int i = 0; i < caster.Items.Count; i++)
            {
                AddItem(CloneItem(caster.Items[i]));
            }

            Warmode = true;

            Summoned = true;
            SummonMaster = caster;

            ControlOrder = OrderType.Follow;
            ControlTarget = caster;

            TimeSpan duration = TimeSpan.FromSeconds(30 + caster.Skills.Ninjitsu.Fixed / 40);

            new UnsummonTimer(caster, this, duration).Start();
            SummonEnd = DateTime.UtcNow + duration;

            MirrorImage.AddClone(m_Caster);
        }

        protected override BaseAI ForcedAI { get { return new CloneAI(this); } }

        public override bool IsHumanInTown() { return false; }

        private Item CloneItem(Item item)
        {
            Item newItem = new Item(item.ItemID);
            newItem.Hue = item.Hue;
            newItem.Layer = item.Layer;

            return newItem;
        }

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {
            Delete();
        }

        public override bool DeleteCorpseOnDeath { get { return true; } }

        public override void OnDelete()
        {
            Effects.SendLocationParticles(EffectItem.Create(Location, Map, EffectItem.DefaultDuration), 0x3728, 10, 15, 5042);

            base.OnDelete();
        }

        public override void OnAfterDelete()
        {
            MirrorImage.RemoveClone(m_Caster);
            base.OnAfterDelete();
        }

        public override bool IsDispellable { get { return false; } }
        public override bool Commandable { get { return false; } }

        public Clone(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version

            writer.Write(m_Caster);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();

            m_Caster = reader.ReadMobile();

            MirrorImage.AddClone(m_Caster);
        }
    }

    public class CloneAI : BaseAI
    {
        public CloneAI(Clone m)
            : base(m)
        {
            m.CurrentSpeed = m.ActiveSpeed;
        }

        public override bool Think()
        {
            // Clones only follow their owners
            Mobile master = m_Mobile.SummonMaster;

            if (master != null && master.Map == m_Mobile.Map && master.InRange(m_Mobile, m_Mobile.RangePerception))
            {
                int iCurrDist = (int)m_Mobile.GetDistanceToSqrt(master);
                bool bRun = (iCurrDist > 5);

                WalkMobileRange(master, 2, bRun, 0, 1);
            }
            else
                WalkRandom(2, 2, 1);

            return true;
        }

        public override bool CanDetectHidden { get { return false; } }
    }
}

namespace Server.Spells
{
    public enum SpellState
    {
        None = 0,
        Casting = 1,	// We are in the process of casting (that is, waiting GetCastTime() and doing animations). Spell casting may be interupted in this state.
        Sequencing = 2	// Casting completed, but the full spell sequence isn't. Usually waiting for a target response. Some actions are restricted in this state (using skills for example).
    }

    public enum TravelCheckType
    {
        RecallFrom,
        RecallTo,
        GateFrom,
        GateTo,
        Mark,
        TeleportFrom,
        TeleportTo
    }

    public enum DisturbType
    {
        Unspecified,
        EquipRequest,
        UseRequest,
        Hurt,
        Kill,
        NewCast
    }

    public class Initializer
    {
        public static void Initialize()
        {
            // First circle
            Register(00, typeof(First.ClumsySpell));
            Register(01, typeof(First.CreateFoodSpell));
            Register(02, typeof(First.FeeblemindSpell));
            Register(03, typeof(First.HealSpell));
            Register(04, typeof(First.MagicArrowSpell));
            Register(05, typeof(First.NightSightSpell));
            Register(06, typeof(First.ReactiveArmorSpell));
            Register(07, typeof(First.WeakenSpell));

            // Second circle
            Register(08, typeof(Second.AgilitySpell));
            Register(09, typeof(Second.CunningSpell));
            Register(10, typeof(Second.CureSpell));
            Register(11, typeof(Second.HarmSpell));
            Register(12, typeof(Second.MagicTrapSpell));
            Register(13, typeof(Second.RemoveTrapSpell));
            Register(14, typeof(Second.ProtectionSpell));
            Register(15, typeof(Second.StrengthSpell));

            // Third circle
            Register(16, typeof(Third.BlessSpell));
            Register(17, typeof(Third.FireballSpell));
            Register(18, typeof(Third.MagicLockSpell));
            Register(19, typeof(Third.PoisonSpell));
            Register(20, typeof(Third.TelekinesisSpell));
            Register(21, typeof(Third.TeleportSpell));
            Register(22, typeof(Third.UnlockSpell));
            Register(23, typeof(Third.WallOfStoneSpell));

            // Fourth circle
            Register(24, typeof(Fourth.ArchCureSpell));
            Register(25, typeof(Fourth.ArchProtectionSpell));
            Register(26, typeof(Fourth.CurseSpell));
            Register(27, typeof(Fourth.FireFieldSpell));
            Register(28, typeof(Fourth.GreaterHealSpell));
            Register(29, typeof(Fourth.LightningSpell));
            Register(30, typeof(Fourth.ManaDrainSpell));
            Register(31, typeof(Fourth.RecallSpell));

            // Fifth circle
            Register(32, typeof(Fifth.BladeSpiritsSpell));
            Register(33, typeof(Fifth.DispelFieldSpell));
            Register(34, typeof(Fifth.IncognitoSpell));
            Register(35, typeof(Fifth.MagicReflectSpell));
            Register(36, typeof(Fifth.MindBlastSpell));
            Register(37, typeof(Fifth.ParalyzeSpell));
            Register(38, typeof(Fifth.PoisonFieldSpell));
            Register(39, typeof(Fifth.SummonCreatureSpell));

            // Sixth circle
            Register(40, typeof(Sixth.DispelSpell));
            Register(41, typeof(Sixth.EnergyBoltSpell));
            Register(42, typeof(Sixth.ExplosionSpell));
            Register(43, typeof(Sixth.InvisibilitySpell));
            Register(44, typeof(Sixth.MarkSpell));
            Register(45, typeof(Sixth.MassCurseSpell));
            Register(46, typeof(Sixth.ParalyzeFieldSpell));
            Register(47, typeof(Sixth.RevealSpell));

            // Seventh circle
            Register(48, typeof(Seventh.ChainLightningSpell));
            Register(49, typeof(Seventh.EnergyFieldSpell));
            Register(50, typeof(Seventh.FlameStrikeSpell));
            Register(51, typeof(Seventh.GateTravelSpell));
            Register(52, typeof(Seventh.ManaVampireSpell));
            Register(53, typeof(Seventh.MassDispelSpell));
            Register(54, typeof(Seventh.MeteorSwarmSpell));
            Register(55, typeof(Seventh.PolymorphSpell));

            // Eighth circle
            Register(56, typeof(Eighth.EarthquakeSpell));
            Register(57, typeof(Eighth.EnergyVortexSpell));
            Register(58, typeof(Eighth.ResurrectionSpell));
            Register(59, typeof(Eighth.AirElementalSpell));
            Register(60, typeof(Eighth.SummonDaemonSpell));
            Register(61, typeof(Eighth.EarthElementalSpell));
            Register(62, typeof(Eighth.FireElementalSpell));
            Register(63, typeof(Eighth.WaterElementalSpell));

            if (Core.AOS)
            {
                // Necromancy spells
                Register(100, typeof(Necromancy.AnimateDeadSpell));
                Register(101, typeof(Necromancy.BloodOathSpell));
                Register(102, typeof(Necromancy.CorpseSkinSpell));
                Register(103, typeof(Necromancy.CurseWeaponSpell));
                Register(104, typeof(Necromancy.EvilOmenSpell));
                Register(105, typeof(Necromancy.HorrificBeastSpell));
                Register(106, typeof(Necromancy.LichFormSpell));
                Register(107, typeof(Necromancy.MindRotSpell));
                Register(108, typeof(Necromancy.PainSpikeSpell));
                Register(109, typeof(Necromancy.PoisonStrikeSpell));
                Register(110, typeof(Necromancy.StrangleSpell));
                Register(111, typeof(Necromancy.SummonFamiliarSpell));
                Register(112, typeof(Necromancy.VampiricEmbraceSpell));
                Register(113, typeof(Necromancy.VengefulSpiritSpell));
                Register(114, typeof(Necromancy.WitherSpell));
                Register(115, typeof(Necromancy.WraithFormSpell));

                if (Core.SE)
                    Register(116, typeof(Necromancy.ExorcismSpell));

                // Paladin abilities
                Register(200, typeof(Chivalry.CleanseByFireSpell));
                Register(201, typeof(Chivalry.CloseWoundsSpell));
                Register(202, typeof(Chivalry.ConsecrateWeaponSpell));
                Register(203, typeof(Chivalry.DispelEvilSpell));
                Register(204, typeof(Chivalry.DivineFurySpell));
                Register(205, typeof(Chivalry.EnemyOfOneSpell));
                Register(206, typeof(Chivalry.HolyLightSpell));
                Register(207, typeof(Chivalry.NobleSacrificeSpell));
                Register(208, typeof(Chivalry.RemoveCurseSpell));
                Register(209, typeof(Chivalry.SacredJourneySpell));

                if (Core.SE)
                {
                    // Samurai abilities
                    Register(400, typeof(Bushido.HonorableExecution));
                    Register(401, typeof(Bushido.Confidence));
                    Register(402, typeof(Bushido.Evasion));
                    Register(403, typeof(Bushido.CounterAttack));
                    Register(404, typeof(Bushido.LightningStrike));
                    Register(405, typeof(Bushido.MomentumStrike));

                    // Ninja abilities
                    Register(500, typeof(Ninjitsu.FocusAttack));
                    Register(501, typeof(Ninjitsu.DeathStrike));
                    Register(502, typeof(Ninjitsu.AnimalForm));
                    Register(503, typeof(Ninjitsu.KiAttack));
                    Register(504, typeof(Ninjitsu.SurpriseAttack));
                    Register(505, typeof(Ninjitsu.Backstab));
                    Register(506, typeof(Ninjitsu.Shadowjump));
                    Register(507, typeof(Ninjitsu.MirrorImage));
                }

                if (Core.ML)
                {
                    Register(600, typeof(Spellweaving.ArcaneCircleSpell));
                    Register(601, typeof(Spellweaving.GiftOfRenewalSpell));
                    Register(602, typeof(Spellweaving.ImmolatingWeaponSpell));
                    Register(603, typeof(Spellweaving.AttuneWeaponSpell));
                    Register(604, typeof(Spellweaving.ThunderstormSpell));
                    Register(605, typeof(Spellweaving.NatureFurySpell));
                    Register(606, typeof(Spellweaving.SummonFeySpell));
                    Register(607, typeof(Spellweaving.SummonFiendSpell));
                    Register(608, typeof(Spellweaving.ReaperFormSpell));
                    //Register( 609, typeof( Spellweaving.WildfireSpell ) );
                    Register(610, typeof(Spellweaving.EssenceOfWindSpell));
                    //Register( 611, typeof( Spellweaving.DryadAllureSpell ) );
                    Register(612, typeof(Spellweaving.EtherealVoyageSpell));
                    Register(613, typeof(Spellweaving.WordOfDeathSpell));
                    Register(614, typeof(Spellweaving.GiftOfLifeSpell));
                    //Register( 615, typeof( Spellweaving.ArcaneEmpowermentSpell ) );
                }

                if (Core.SA)
                {
                    // Mysticism spells
                    //Register( 677, typeof( Mysticism.NetherBoltSpell ) );
                    //Register( 678, typeof( Mysticism.HealingStoneSpell ) );
                    //Register( 679, typeof( Mysticism.PurgeMagicSpell ) );
                    //Register( 680, typeof( Mysticism.EnchantSpell ) );
                    //Register( 681, typeof( Mysticism.SleepSpell ) );
                    Register(682, typeof(Mysticism.EagleStrikeSpell));
                    Register(683, typeof(Mysticism.AnimatedWeaponSpell));
                    Register(684, typeof(Mysticism.StoneFormSpell));
                    //Register( 685, typeof( Mysticism.SpellTriggerSpell ) );
                    //Register( 686, typeof( Mysticism.MassSleepSpell ) );
                    //Register( 687, typeof( Mysticism.CleansingWindsSpell ) );
                    //Register( 688, typeof( Mysticism.BombardSpell ) );
                    Register(689, typeof(Mysticism.SpellPlagueSpell));
                    Register(690, typeof(Mysticism.HailStormSpell));
                    Register(691, typeof(Mysticism.NetherCycloneSpell));
                    //Register( 692, typeof( Mysticism.RisingColossusSpell ) );
                }
            }
        }

        public static void Register(int spellId, Type type)
        {
            SpellRegistry.Register(spellId, type);
        }
    }

    public class SpellRegistry
    {
        private static Type[] m_Types = new Type[700];
        private static int m_Count;

        public static Type[] Types
        {
            get
            {
                m_Count = -1;
                return m_Types;
            }
        }

        //What IS this used for anyways.
        public static int Count
        {
            get
            {
                if (m_Count == -1)
                {
                    m_Count = 0;

                    for (int i = 0; i < m_Types.Length; ++i)
                        if (m_Types[i] != null)
                            ++m_Count;
                }

                return m_Count;
            }
        }

        private static Dictionary<Type, Int32> m_IDsFromTypes = new Dictionary<Type, Int32>(m_Types.Length);

        private static Dictionary<Int32, SpecialMove> m_SpecialMoves = new Dictionary<Int32, SpecialMove>();
        public static Dictionary<Int32, SpecialMove> SpecialMoves { get { return m_SpecialMoves; } }

        public static int GetRegistryNumber(ISpell s)
        {
            return GetRegistryNumber(s.GetType());
        }

        public static int GetRegistryNumber(SpecialMove s)
        {
            return GetRegistryNumber(s.GetType());
        }

        public static int GetRegistryNumber(Type type)
        {
            if (m_IDsFromTypes.ContainsKey(type))
                return m_IDsFromTypes[type];

            return -1;
        }

        public static void Register(int spellID, Type type)
        {
            if (spellID < 0 || spellID >= m_Types.Length)
                return;

            if (m_Types[spellID] == null)
                ++m_Count;

            m_Types[spellID] = type;

            if (!m_IDsFromTypes.ContainsKey(type))
                m_IDsFromTypes.Add(type, spellID);

            if (type.IsSubclassOf(typeof(SpecialMove)))
            {
                SpecialMove spm = null;

                try
                {
                    spm = Activator.CreateInstance(type) as SpecialMove;
                }
                catch
                {
                }

                if (spm != null)
                    m_SpecialMoves.Add(spellID, spm);
            }
        }

        public static SpecialMove GetSpecialMove(int spellID)
        {
            if (spellID < 0 || spellID >= m_Types.Length)
                return null;

            Type t = m_Types[spellID];

            if (t == null || !t.IsSubclassOf(typeof(SpecialMove)) || !m_SpecialMoves.ContainsKey(spellID))
                return null;

            return m_SpecialMoves[spellID];
        }

        private static object[] m_Params = new object[2];

        public static Spell NewSpell(int spellID, Mobile caster, Item scroll)
        {
            if (spellID < 0 || spellID >= m_Types.Length)
                return null;

            Type t = m_Types[spellID];

            if (t != null && !t.IsSubclassOf(typeof(SpecialMove)))
            {
                m_Params[0] = caster;
                m_Params[1] = scroll;

                try
                {
                    return (Spell)Activator.CreateInstance(t, m_Params);
                }
                catch
                {
                }
            }

            return null;
        }

        private static string[] m_CircleNames = new string[]
            {
                "First",
                "Second",
                "Third",
                "Fourth",
                "Fifth",
                "Sixth",
                "Seventh",
                "Eighth",
                "Necromancy",
                "Chivalry",
                "Bushido",
                "Ninjitsu",
                "Spellweaving"
            };

        public static Spell NewSpell(string name, Mobile caster, Item scroll)
        {
            for (int i = 0; i < m_CircleNames.Length; ++i)
            {
                Type t = ScriptCompiler.FindTypeByFullName(String.Format("Server.Spells.{0}.{1}", m_CircleNames[i], name));

                if (t != null && !t.IsSubclassOf(typeof(SpecialMove)))
                {
                    m_Params[0] = caster;
                    m_Params[1] = scroll;

                    try
                    {
                        return (Spell)Activator.CreateInstance(t, m_Params);
                    }
                    catch
                    {
                    }
                }
            }

            return null;
        }
    }

    public class Reagent
    {
        private static Type[] m_Types = 
        {
			typeof( BlackPearl ),
			typeof( Bloodmoss ),
			typeof( Garlic ),
			typeof( Ginseng ),
			typeof( MandrakeRoot ),
			typeof( Nightshade ),
			typeof( SulfurousAsh ),
			typeof( SpidersSilk ),
			typeof( BatWing ),
			typeof( GraveDust ),
			typeof( DaemonBlood ),
			typeof( NoxCrystal ),
			typeof( PigIron ),
			typeof( Bone ),
			typeof( FertileDirt ),
			typeof( DragonsBlood ),
			typeof( DaemonBone )
		};

        public Type[] Types
        {
            get { return m_Types; }
        }

        public static Type BlackPearl
        {
            get { return m_Types[0]; }
            set { m_Types[0] = value; }
        }

        public static Type Bloodmoss
        {
            get { return m_Types[1]; }
            set { m_Types[1] = value; }
        }

        public static Type Garlic
        {
            get { return m_Types[2]; }
            set { m_Types[2] = value; }
        }

        public static Type Ginseng
        {
            get { return m_Types[3]; }
            set { m_Types[3] = value; }
        }

        public static Type MandrakeRoot
        {
            get { return m_Types[4]; }
            set { m_Types[4] = value; }
        }

        public static Type Nightshade
        {
            get { return m_Types[5]; }
            set { m_Types[5] = value; }
        }

        public static Type SulfurousAsh
        {
            get { return m_Types[6]; }
            set { m_Types[6] = value; }
        }

        public static Type SpidersSilk
        {
            get { return m_Types[7]; }
            set { m_Types[7] = value; }
        }

        public static Type BatWing
        {
            get { return m_Types[8]; }
            set { m_Types[8] = value; }
        }

        public static Type GraveDust
        {
            get { return m_Types[9]; }
            set { m_Types[9] = value; }
        }

        public static Type DaemonBlood
        {
            get { return m_Types[10]; }
            set { m_Types[10] = value; }
        }

        public static Type NoxCrystal
        {
            get { return m_Types[11]; }
            set { m_Types[11] = value; }
        }

        public static Type PigIron
        {
            get { return m_Types[12]; }
            set { m_Types[12] = value; }
        }

        public static Type Bone
        {
            get { return m_Types[13]; }
            set { m_Types[13] = value; }
        }

        public static Type FertileDirt
        {
            get { return m_Types[14]; }
            set { m_Types[14] = value; }
        }

        public static Type DragonsBlood
        {
            get { return m_Types[15]; }
            set { m_Types[15] = value; }
        }

        public static Type DaemonBone
        {
            get { return m_Types[16]; }
            set { m_Types[16] = value; }
        }
    }

    public abstract class Spell : ISpell
    {
        private Mobile m_Caster;
        private Item m_Scroll;
        private SpellInfo m_Info;
        private SpellState m_State;
        private long m_StartCastTime;

        public SpellState State { get { return m_State; } set { m_State = value; } }
        public Mobile Caster { get { return m_Caster; } }
        public SpellInfo Info { get { return m_Info; } }
        public string Name { get { return m_Info.Name; } }
        public string Mantra { get { return m_Info.Mantra; } }
        public Type[] Reagents { get { return m_Info.Reagents; } }
        public Item Scroll { get { return m_Scroll; } }
        public long StartCastTime { get { return m_StartCastTime; } }

        private static TimeSpan NextSpellDelay = TimeSpan.FromSeconds(0.75);
        private static TimeSpan AnimateDelay = TimeSpan.FromSeconds(1.5);

        public virtual SkillName CastSkill { get { return SkillName.Magery; } }
        public virtual SkillName DamageSkill { get { return SkillName.EvalInt; } }

        public virtual bool RevealOnCast { get { return true; } }
        public virtual bool ClearHandsOnCast { get { return true; } }
        public virtual bool ShowHandMovement { get { return true; } }

        public virtual bool DelayedDamage { get { return false; } }

        public virtual bool DelayedDamageStacking { get { return true; } }
        //In reality, it's ANY delayed Damage spell Post-AoS that can't stack, but, only 
        //Expo & Magic Arrow have enough delay and a short enough cast time to bring up 
        //the possibility of stacking 'em.  Note that a MA & an Explosion will stack, but
        //of course, two MA's won't.

        private static Dictionary<Type, DelayedDamageContextWrapper> m_ContextTable = new Dictionary<Type, DelayedDamageContextWrapper>();

        private class DelayedDamageContextWrapper
        {
            private Dictionary<Mobile, Timer> m_Contexts = new Dictionary<Mobile, Timer>();

            public void Add(Mobile m, Timer t)
            {
                Timer oldTimer;
                if (m_Contexts.TryGetValue(m, out oldTimer))
                {
                    oldTimer.Stop();
                    m_Contexts.Remove(m);
                }

                m_Contexts.Add(m, t);
            }

            public void Remove(Mobile m)
            {
                m_Contexts.Remove(m);
            }
        }

        public void StartDelayedDamageContext(Mobile m, Timer t)
        {
            if (DelayedDamageStacking)
                return; //Sanity

            DelayedDamageContextWrapper contexts;

            if (!m_ContextTable.TryGetValue(GetType(), out contexts))
            {
                contexts = new DelayedDamageContextWrapper();
                m_ContextTable.Add(GetType(), contexts);
            }

            contexts.Add(m, t);
        }

        public void RemoveDelayedDamageContext(Mobile m)
        {
            DelayedDamageContextWrapper contexts;

            if (!m_ContextTable.TryGetValue(GetType(), out contexts))
                return;

            contexts.Remove(m);
        }

        public void HarmfulSpell(Mobile m)
        {
            if (m is BaseCreature)
                ((BaseCreature)m).OnHarmfulSpell(m_Caster);
        }

        public Spell(Mobile caster, Item scroll, SpellInfo info)
        {
            m_Caster = caster;
            m_Scroll = scroll;
            m_Info = info;
        }

        public virtual int GetNewAosDamage(int bonus, int dice, int sides, Mobile singleTarget)
        {
            if (singleTarget != null)
            {
                return GetNewAosDamage(bonus, dice, sides, (Caster.Player && singleTarget.Player), GetDamageScalar(singleTarget));
            }
            else
            {
                return GetNewAosDamage(bonus, dice, sides, false);
            }
        }

        public virtual int GetNewAosDamage(int bonus, int dice, int sides, bool playerVsPlayer)
        {
            return GetNewAosDamage(bonus, dice, sides, playerVsPlayer, 1.0);
        }

        public virtual int GetNewAosDamage(int bonus, int dice, int sides, bool playerVsPlayer, double scalar)
        {
            int damage = Utility.Dice(dice, sides, bonus) * 100;
            int damageBonus = 0;

            int inscribeSkill = GetInscribeFixed(m_Caster);
            int inscribeBonus = (inscribeSkill + (1000 * (inscribeSkill / 1000))) / 200;
            damageBonus += inscribeBonus;

            int intBonus = Caster.Int / 10;
            damageBonus += intBonus;

            int sdiBonus = AosAttributes.GetValue(m_Caster, AosAttribute.SpellDamage);
            // PvP spell damage increase cap of 15% from an item’s magic property
            if (playerVsPlayer && sdiBonus > 15)
                sdiBonus = 15;

            damageBonus += sdiBonus;

            TransformContext context = TransformationSpellHelper.GetContext(Caster);

            if (context != null && context.Spell is ReaperFormSpell)
                damageBonus += ((ReaperFormSpell)context.Spell).SpellDamageBonus;

            damage = AOS.Scale(damage, 100 + damageBonus);

            int evalSkill = GetDamageFixed(m_Caster);
            int evalScale = 30 + ((9 * evalSkill) / 100);

            damage = AOS.Scale(damage, evalScale);

            damage = AOS.Scale(damage, (int)(scalar * 100));

            return damage / 100;
        }

        public virtual bool IsCasting { get { return m_State == SpellState.Casting; } }

        public virtual void OnCasterHurt()
        {
            //Confirm: Monsters and pets cannot be disturbed.
            if (!Caster.Player)
                return;

            if (IsCasting)
            {
                object o = ProtectionSpell.Registry[m_Caster];
                bool disturb = true;

                if (o != null && o is double)
                {
                    if (((double)o) > Utility.RandomDouble() * 100.0)
                        disturb = false;
                }

                if (disturb)
                    Disturb(DisturbType.Hurt, false, true);
            }
        }

        public virtual void OnCasterKilled()
        {
            Disturb(DisturbType.Kill);
        }

        public virtual void OnConnectionChanged()
        {
            FinishSequence();
        }

        public virtual bool OnCasterMoving(Direction d)
        {
            if (IsCasting && BlocksMovement)
            {
                m_Caster.SendLocalizedMessage(500111); // You are frozen and can not move.
                return false;
            }

            return true;
        }

        public virtual bool OnCasterEquiping(Item item)
        {
            if (IsCasting)
                Disturb(DisturbType.EquipRequest);

            return true;
        }

        public virtual bool OnCasterUsingObject(object o)
        {
            if (m_State == SpellState.Sequencing)
                Disturb(DisturbType.UseRequest);

            return true;
        }

        public virtual bool OnCastInTown(Region r)
        {
            return m_Info.AllowTown;
        }

        public virtual bool ConsumeReagents()
        {
            if (m_Scroll != null || !m_Caster.Player)
                return true;

            if (AosAttributes.GetValue(m_Caster, AosAttribute.LowerRegCost) > Utility.Random(100))
                return true;

            if (Engines.ConPVP.DuelContext.IsFreeConsume(m_Caster))
                return true;

            Container pack = m_Caster.Backpack;

            if (pack == null)
                return false;

            if (pack.ConsumeTotal(m_Info.Reagents, m_Info.Amounts) == -1)
                return true;

            return false;
        }

        public virtual double GetInscribeSkill(Mobile m)
        {
            // There is no chance to gain
            // m.CheckSkill( SkillName.Inscribe, 0.0, 120.0 );

            return m.Skills[SkillName.Inscribe].Value;
        }

        public virtual int GetInscribeFixed(Mobile m)
        {
            // There is no chance to gain
            // m.CheckSkill( SkillName.Inscribe, 0.0, 120.0 );

            return m.Skills[SkillName.Inscribe].Fixed;
        }

        public virtual int GetDamageFixed(Mobile m)
        {
            //m.CheckSkill( DamageSkill, 0.0, m.Skills[DamageSkill].Cap );

            return m.Skills[DamageSkill].Fixed;
        }

        public virtual double GetDamageSkill(Mobile m)
        {
            //m.CheckSkill( DamageSkill, 0.0, m.Skills[DamageSkill].Cap );

            return m.Skills[DamageSkill].Value;
        }

        public virtual double GetResistSkill(Mobile m)
        {
            return m.Skills[SkillName.MagicResist].Value;
        }

        public virtual double GetDamageScalar(Mobile target)
        {
            double scalar = 1.0;

            if (!Core.AOS)	//EvalInt stuff for AoS is handled elsewhere
            {
                double casterEI = m_Caster.Skills[DamageSkill].Value;
                double targetRS = target.Skills[SkillName.MagicResist].Value;

                /*
                if( Core.AOS )
                    targetRS = 0;
                */

                //m_Caster.CheckSkill( DamageSkill, 0.0, 120.0 );

                if (casterEI > targetRS)
                    scalar = (1.0 + ((casterEI - targetRS) / 500.0));
                else
                    scalar = (1.0 + ((casterEI - targetRS) / 200.0));

                // magery damage bonus, -25% at 0 skill, +0% at 100 skill, +5% at 120 skill
                scalar += (m_Caster.Skills[CastSkill].Value - 100.0) / 400.0;

                if (!target.Player && !target.Body.IsHuman /*&& !Core.AOS*/ )
                    scalar *= 2.0; // Double magery damage to monsters/animals if not AOS
            }

            if (target is BaseCreature)
                ((BaseCreature)target).AlterDamageScalarFrom(m_Caster, ref scalar);

            if (m_Caster is BaseCreature)
                ((BaseCreature)m_Caster).AlterDamageScalarTo(target, ref scalar);

            if (Core.SE)
                scalar *= GetSlayerDamageScalar(target);

            target.Region.SpellDamageScalar(m_Caster, target, ref scalar);

            if (Evasion.CheckSpellEvasion(target))	//Only single target spells an be evaded
                scalar = 0;

            return scalar;
        }

        public virtual double GetSlayerDamageScalar(Mobile defender)
        {
            Spellbook atkBook = Spellbook.FindEquippedSpellbook(m_Caster);

            double scalar = 1.0;
            if (atkBook != null)
            {
                SlayerEntry atkSlayer = SlayerGroup.GetEntryByName(atkBook.Slayer);
                SlayerEntry atkSlayer2 = SlayerGroup.GetEntryByName(atkBook.Slayer2);

                if (atkSlayer != null && atkSlayer.Slays(defender) || atkSlayer2 != null && atkSlayer2.Slays(defender))
                {
                    defender.FixedEffect(0x37B9, 10, 5);	//TODO: Confirm this displays on OSIs
                    scalar = 2.0;
                }


                TransformContext context = TransformationSpellHelper.GetContext(defender);

                if ((atkBook.Slayer == SlayerName.Silver || atkBook.Slayer2 == SlayerName.Silver) && context != null && context.Type != typeof(HorrificBeastSpell))
                    scalar += .25; // Every necromancer transformation other than horrific beast take an additional 25% damage

                if (scalar != 1.0)
                    return scalar;
            }

            ISlayer defISlayer = Spellbook.FindEquippedSpellbook(defender);

            if (defISlayer == null)
                defISlayer = defender.Weapon as ISlayer;

            if (defISlayer != null)
            {
                SlayerEntry defSlayer = SlayerGroup.GetEntryByName(defISlayer.Slayer);
                SlayerEntry defSlayer2 = SlayerGroup.GetEntryByName(defISlayer.Slayer2);

                if (defSlayer != null && defSlayer.Group.OppositionSuperSlays(m_Caster) || defSlayer2 != null && defSlayer2.Group.OppositionSuperSlays(m_Caster))
                    scalar = 2.0;
            }

            return scalar;
        }

        public virtual void DoFizzle()
        {
            m_Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, 502632); // The spell fizzles.

            if (m_Caster.Player)
            {
                if (Core.AOS)
                    m_Caster.FixedParticles(0x3735, 1, 30, 9503, EffectLayer.Waist);
                else
                    m_Caster.FixedEffect(0x3735, 6, 30);

                m_Caster.PlaySound(0x5C);
            }
        }

        private CastTimer m_CastTimer;
        private AnimTimer m_AnimTimer;

        public void Disturb(DisturbType type)
        {
            Disturb(type, true, false);
        }

        public virtual bool CheckDisturb(DisturbType type, bool firstCircle, bool resistable)
        {
            if (resistable && m_Scroll is BaseWand)
                return false;

            return true;
        }

        public void Disturb(DisturbType type, bool firstCircle, bool resistable)
        {
            if (!CheckDisturb(type, firstCircle, resistable))
                return;

            if (m_State == SpellState.Casting)
            {
                if (!firstCircle && !Core.AOS && this is MagerySpell && ((MagerySpell)this).Circle == SpellCircle.First)
                    return;

                m_State = SpellState.None;
                m_Caster.Spell = null;

                OnDisturb(type, true);

                if (m_CastTimer != null)
                    m_CastTimer.Stop();

                if (m_AnimTimer != null)
                    m_AnimTimer.Stop();

                if (Core.AOS && m_Caster.Player && type == DisturbType.Hurt)
                    DoHurtFizzle();

                m_Caster.NextSpellTime = Core.TickCount + (int)GetDisturbRecovery().TotalMilliseconds;
            }
            else if (m_State == SpellState.Sequencing)
            {
                if (!firstCircle && !Core.AOS && this is MagerySpell && ((MagerySpell)this).Circle == SpellCircle.First)
                    return;

                m_State = SpellState.None;
                m_Caster.Spell = null;

                OnDisturb(type, false);

                Targeting.Target.Cancel(m_Caster);

                if (Core.AOS && m_Caster.Player && type == DisturbType.Hurt)
                    DoHurtFizzle();
            }
        }

        public virtual void DoHurtFizzle()
        {
            m_Caster.FixedEffect(0x3735, 6, 30);
            m_Caster.PlaySound(0x5C);
        }

        public virtual void OnDisturb(DisturbType type, bool message)
        {
            if (message)
                m_Caster.SendLocalizedMessage(500641); // Your concentration is disturbed, thus ruining thy spell.
        }

        public virtual bool CheckCast()
        {
            return true;
        }

        public virtual void SayMantra()
        {
            if (m_Scroll is BaseWand)
                return;

            if (m_Info.Mantra != null && m_Info.Mantra.Length > 0 && m_Caster.Player)
                m_Caster.PublicOverheadMessage(MessageType.Spell, m_Caster.SpeechHue, true, m_Info.Mantra, false);
        }

        public virtual bool BlockedByHorrificBeast { get { return true; } }
        public virtual bool BlockedByAnimalForm { get { return true; } }
        public virtual bool BlocksMovement { get { return true; } }

        public virtual bool CheckNextSpellTime { get { return !(m_Scroll is BaseWand); } }

        public bool Cast()
        {
            m_StartCastTime = Core.TickCount;

            if (Core.AOS && m_Caster.Spell is Spell && ((Spell)m_Caster.Spell).State == SpellState.Sequencing)
                ((Spell)m_Caster.Spell).Disturb(DisturbType.NewCast);

            if (!m_Caster.CheckAlive())
            {
                return false;
            }
            else if (m_Scroll is BaseWand && m_Caster.Spell != null && m_Caster.Spell.IsCasting)
            {
                m_Caster.SendLocalizedMessage(502643); // You can not cast a spell while frozen.
            }
            else if (m_Caster.Spell != null && m_Caster.Spell.IsCasting)
            {
                m_Caster.SendLocalizedMessage(502642); // You are already casting a spell.
            }
            else if (BlockedByHorrificBeast && TransformationSpellHelper.UnderTransformation(m_Caster, typeof(HorrificBeastSpell)) || (BlockedByAnimalForm && AnimalForm.UnderTransformation(m_Caster)))
            {
                m_Caster.SendLocalizedMessage(1061091); // You cannot cast that spell in this form.
            }
            else if (!(m_Scroll is BaseWand) && (m_Caster.Paralyzed || m_Caster.Frozen))
            {
                m_Caster.SendLocalizedMessage(502643); // You can not cast a spell while frozen.
            }
            else if (CheckNextSpellTime && Core.TickCount - m_Caster.NextSpellTime < 0)
            {
                m_Caster.SendLocalizedMessage(502644); // You have not yet recovered from casting a spell.
            }
            else if (m_Caster is PlayerMobile && ((PlayerMobile)m_Caster).PeacedUntil > DateTime.UtcNow)
            {
                m_Caster.SendLocalizedMessage(1072060); // You cannot cast a spell while calmed.
            }
            #region Dueling
            else if (m_Caster is PlayerMobile && ((PlayerMobile)m_Caster).DuelContext != null && !((PlayerMobile)m_Caster).DuelContext.AllowSpellCast(m_Caster, this))
            {
            }
            #endregion
            else if (m_Caster.Mana >= ScaleMana(GetMana()))
            {
                if (m_Caster.Spell == null && m_Caster.CheckSpellCast(this) && CheckCast() && m_Caster.Region.OnBeginSpellCast(m_Caster, this))
                {
                    m_State = SpellState.Casting;
                    m_Caster.Spell = this;

                    if (!(m_Scroll is BaseWand) && RevealOnCast)
                        m_Caster.RevealingAction();

                    SayMantra();

                    TimeSpan castDelay = this.GetCastDelay();

                    if (ShowHandMovement && (m_Caster.Body.IsHuman || (m_Caster.Player && m_Caster.Body.IsMonster)))
                    {
                        int count = (int)Math.Ceiling(castDelay.TotalSeconds / AnimateDelay.TotalSeconds);

                        if (count != 0)
                        {
                            m_AnimTimer = new AnimTimer(this, count);
                            m_AnimTimer.Start();
                        }

                        if (m_Info.LeftHandEffect > 0)
                            Caster.FixedParticles(0, 10, 5, m_Info.LeftHandEffect, EffectLayer.LeftHand);

                        if (m_Info.RightHandEffect > 0)
                            Caster.FixedParticles(0, 10, 5, m_Info.RightHandEffect, EffectLayer.RightHand);
                    }

                    if (ClearHandsOnCast)
                        m_Caster.ClearHands();

                    if (Core.ML)
                        WeaponAbility.ClearCurrentAbility(m_Caster);

                    m_CastTimer = new CastTimer(this, castDelay);
                    //m_CastTimer.Start();

                    OnBeginCast();

                    if (castDelay > TimeSpan.Zero)
                    {
                        m_CastTimer.Start();
                    }
                    else
                    {
                        m_CastTimer.Tick();
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                m_Caster.LocalOverheadMessage(MessageType.Regular, 0x22, 502625); // Insufficient mana
            }

            return false;
        }

        public abstract void OnCast();

        public virtual void OnBeginCast()
        {
        }

        public virtual void GetCastSkills(out double min, out double max)
        {
            min = max = 0;	//Intended but not required for overriding.
        }

        public virtual bool CheckFizzle()
        {
            if (m_Scroll is BaseWand)
                return true;

            double minSkill, maxSkill;

            GetCastSkills(out minSkill, out maxSkill);

            if (DamageSkill != CastSkill)
                Caster.CheckSkill(DamageSkill, 0.0, Caster.Skills[DamageSkill].Cap);

            return Caster.CheckSkill(CastSkill, minSkill, maxSkill);
        }

        public abstract int GetMana();

        public virtual int ScaleMana(int mana)
        {
            double scalar = 1.0;

            if (!Necromancy.MindRotSpell.GetMindRotScalar(Caster, ref scalar))
                scalar = 1.0;

            // Lower Mana Cost = 40%
            int lmc = AosAttributes.GetValue(m_Caster, AosAttribute.LowerManaCost);
            if (lmc > 40)
                lmc = 40;

            scalar -= (double)lmc / 100;

            return (int)(mana * scalar);
        }

        public virtual TimeSpan GetDisturbRecovery()
        {
            if (Core.AOS)
                return TimeSpan.Zero;

            double delay = 1.0 - Math.Sqrt((Core.TickCount - m_StartCastTime) / 1000.0 / GetCastDelay().TotalSeconds);

            if (delay < 0.2)
                delay = 0.2;

            return TimeSpan.FromSeconds(delay);
        }

        public virtual int CastRecoveryBase { get { return 6; } }
        public virtual int CastRecoveryFastScalar { get { return 1; } }
        public virtual int CastRecoveryPerSecond { get { return 4; } }
        public virtual int CastRecoveryMinimum { get { return 0; } }

        public virtual TimeSpan GetCastRecovery()
        {
            if (!Core.AOS)
                return NextSpellDelay;

            int fcr = AosAttributes.GetValue(m_Caster, AosAttribute.CastRecovery);

            fcr -= ThunderstormSpell.GetCastRecoveryMalus(m_Caster);

            int fcrDelay = -(CastRecoveryFastScalar * fcr);

            int delay = CastRecoveryBase + fcrDelay;

            if (delay < CastRecoveryMinimum)
                delay = CastRecoveryMinimum;

            return TimeSpan.FromSeconds((double)delay / CastRecoveryPerSecond);
        }

        public abstract TimeSpan CastDelayBase { get; }

        public virtual double CastDelayFastScalar { get { return 1; } }
        public virtual double CastDelaySecondsPerTick { get { return 0.25; } }
        public virtual TimeSpan CastDelayMinimum { get { return TimeSpan.FromSeconds(0.25); } }

        //public virtual int CastDelayBase{ get{ return 3; } }
        //public virtual int CastDelayFastScalar{ get{ return 1; } }
        //public virtual int CastDelayPerSecond{ get{ return 4; } }
        //public virtual int CastDelayMinimum{ get{ return 1; } }

        public virtual TimeSpan GetCastDelay()
        {
            if (m_Scroll is BaseWand)
                return Core.ML ? CastDelayBase : TimeSpan.Zero; // TODO: Should FC apply to wands?

            // Faster casting cap of 2 (if not using the protection spell) 
            // Faster casting cap of 0 (if using the protection spell) 
            // Paladin spells are subject to a faster casting cap of 4 
            // Paladins with magery of 70.0 or above are subject to a faster casting cap of 2 
            int fcMax = 4;

            if (CastSkill == SkillName.Magery || CastSkill == SkillName.Necromancy || (CastSkill == SkillName.Chivalry && m_Caster.Skills[SkillName.Magery].Value >= 70.0))
                fcMax = 2;

            int fc = AosAttributes.GetValue(m_Caster, AosAttribute.CastSpeed);

            if (fc > fcMax)
                fc = fcMax;

            if (ProtectionSpell.Registry.Contains(m_Caster))
                fc -= 2;

            if (EssenceOfWindSpell.IsDebuffed(m_Caster))
                fc -= EssenceOfWindSpell.GetFCMalus(m_Caster);

            TimeSpan baseDelay = CastDelayBase;

            TimeSpan fcDelay = TimeSpan.FromSeconds(-(CastDelayFastScalar * fc * CastDelaySecondsPerTick));

            //int delay = CastDelayBase + circleDelay + fcDelay;
            TimeSpan delay = baseDelay + fcDelay;

            if (delay < CastDelayMinimum)
                delay = CastDelayMinimum;

            //return TimeSpan.FromSeconds( (double)delay / CastDelayPerSecond );
            return delay;
        }

        public virtual void FinishSequence()
        {
            m_State = SpellState.None;

            if (m_Caster.Spell == this)
                m_Caster.Spell = null;
        }

        public virtual int ComputeKarmaAward()
        {
            return 0;
        }

        public virtual bool CheckSequence()
        {
            int mana = ScaleMana(GetMana());

            if (m_Caster.Deleted || !m_Caster.Alive || m_Caster.Spell != this || m_State != SpellState.Sequencing)
            {
                DoFizzle();
            }
            else if (m_Scroll != null && !(m_Scroll is Runebook) && (m_Scroll.Amount <= 0 || m_Scroll.Deleted || m_Scroll.RootParent != m_Caster || (m_Scroll is BaseWand && (((BaseWand)m_Scroll).Charges <= 0 || m_Scroll.Parent != m_Caster))))
            {
                DoFizzle();
            }
            else if (!ConsumeReagents())
            {
                m_Caster.LocalOverheadMessage(MessageType.Regular, 0x22, 502630); // More reagents are needed for this spell.
            }
            else if (m_Caster.Mana < mana)
            {
                m_Caster.LocalOverheadMessage(MessageType.Regular, 0x22, 502625); // Insufficient mana for this spell.
            }
            else if (Core.AOS && (m_Caster.Frozen || m_Caster.Paralyzed))
            {
                m_Caster.SendLocalizedMessage(502646); // You cannot cast a spell while frozen.
                DoFizzle();
            }
            else if (m_Caster is PlayerMobile && ((PlayerMobile)m_Caster).PeacedUntil > DateTime.UtcNow)
            {
                m_Caster.SendLocalizedMessage(1072060); // You cannot cast a spell while calmed.
                DoFizzle();
            }
            else if (CheckFizzle())
            {
                m_Caster.Mana -= mana;

                if (m_Scroll is SpellScroll)
                    m_Scroll.Consume();
                else if (m_Scroll is BaseWand)
                {
                    ((BaseWand)m_Scroll).ConsumeCharge(m_Caster);
                    m_Caster.RevealingAction();
                }

                if (m_Scroll is BaseWand)
                {
                    bool m = m_Scroll.Movable;

                    m_Scroll.Movable = false;

                    if (ClearHandsOnCast)
                        m_Caster.ClearHands();

                    m_Scroll.Movable = m;
                }
                else
                {
                    if (ClearHandsOnCast)
                        m_Caster.ClearHands();
                }

                int karma = ComputeKarmaAward();

                if (karma != 0)
                    Misc.Titles.AwardKarma(Caster, karma, true);

                if (TransformationSpellHelper.UnderTransformation(m_Caster, typeof(VampiricEmbraceSpell)))
                {
                    bool garlic = false;

                    for (int i = 0; !garlic && i < m_Info.Reagents.Length; ++i)
                        garlic = (m_Info.Reagents[i] == Reagent.Garlic);

                    if (garlic)
                    {
                        m_Caster.SendLocalizedMessage(1061651); // The garlic burns you!
                        AOS.Damage(m_Caster, Utility.RandomMinMax(17, 23), 100, 0, 0, 0, 0);
                    }
                }

                return true;
            }
            else
            {
                DoFizzle();
            }

            return false;
        }

        public bool CheckBSequence(Mobile target)
        {
            return CheckBSequence(target, false);
        }

        public bool CheckBSequence(Mobile target, bool allowDead)
        {
            if (!target.Alive && !allowDead)
            {
                m_Caster.SendLocalizedMessage(501857); // This spell won't work on that!
                return false;
            }
            else if (Caster.CanBeBeneficial(target, true, allowDead) && CheckSequence())
            {
                Caster.DoBeneficial(target);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckHSequence(Mobile target)
        {
            if (!target.Alive)
            {
                m_Caster.SendLocalizedMessage(501857); // This spell won't work on that!
                return false;
            }
            else if (Caster.CanBeHarmful(target) && CheckSequence())
            {
                Caster.DoHarmful(target);
                return true;
            }
            else
            {
                return false;
            }
        }

        private class AnimTimer : Timer
        {
            private Spell m_Spell;

            public AnimTimer(Spell spell, int count)
                : base(TimeSpan.Zero, AnimateDelay, count)
            {
                m_Spell = spell;

                Priority = TimerPriority.FiftyMS;
            }

            protected override void OnTick()
            {
                if (m_Spell.State != SpellState.Casting || m_Spell.m_Caster.Spell != m_Spell)
                {
                    Stop();
                    return;
                }

                if (!m_Spell.Caster.Mounted && m_Spell.m_Info.Action >= 0)
                {
                    if (m_Spell.Caster.Body.IsHuman)
                        m_Spell.Caster.Animate(m_Spell.m_Info.Action, 7, 1, true, false, 0);
                    else if (m_Spell.Caster.Player && m_Spell.Caster.Body.IsMonster)
                        m_Spell.Caster.Animate(12, 7, 1, true, false, 0);
                }

                if (!Running)
                    m_Spell.m_AnimTimer = null;
            }
        }

        private class CastTimer : Timer
        {
            private Spell m_Spell;

            public CastTimer(Spell spell, TimeSpan castDelay)
                : base(castDelay)
            {
                m_Spell = spell;

                Priority = TimerPriority.TwentyFiveMS;
            }

            protected override void OnTick()
            {
                if (m_Spell == null || m_Spell.m_Caster == null)
                {
                    return;
                }
                else if (m_Spell.m_State == SpellState.Casting && m_Spell.m_Caster.Spell == m_Spell)
                {
                    m_Spell.m_State = SpellState.Sequencing;
                    m_Spell.m_CastTimer = null;
                    m_Spell.m_Caster.OnSpellCast(m_Spell);
                    if (m_Spell.m_Caster.Region != null)
                        m_Spell.m_Caster.Region.OnSpellCast(m_Spell.m_Caster, m_Spell);
                    m_Spell.m_Caster.NextSpellTime = Core.TickCount + (int)m_Spell.GetCastRecovery().TotalMilliseconds; // Spell.NextSpellDelay;

                    Target originalTarget = m_Spell.m_Caster.Target;

                    m_Spell.OnCast();

                    if (m_Spell.m_Caster.Player && m_Spell.m_Caster.Target != originalTarget && m_Spell.Caster.Target != null)
                        m_Spell.m_Caster.Target.BeginTimeout(m_Spell.m_Caster, TimeSpan.FromSeconds(30.0));

                    m_Spell.m_CastTimer = null;
                }
            }

            public void Tick()
            {
                OnTick();
            }
        }
    }

    public class SpellInfo
    {
        private string m_Name;
        private string m_Mantra;
        private Type[] m_Reagents;
        private int[] m_Amounts;
        private int m_Action;
        private bool m_AllowTown;
        private int m_LeftHandEffect, m_RightHandEffect;

        public SpellInfo(string name, string mantra, params Type[] regs)
            : this(name, mantra, 16, 0, 0, true, regs)
        {
        }

        public SpellInfo(string name, string mantra, bool allowTown, params Type[] regs)
            : this(name, mantra, 16, 0, 0, allowTown, regs)
        {
        }

        public SpellInfo(string name, string mantra, int action, params Type[] regs)
            : this(name, mantra, action, 0, 0, true, regs)
        {
        }

        public SpellInfo(string name, string mantra, int action, bool allowTown, params Type[] regs)
            : this(name, mantra, action, 0, 0, allowTown, regs)
        {
        }

        public SpellInfo(string name, string mantra, int action, int handEffect, params Type[] regs)
            : this(name, mantra, action, handEffect, handEffect, true, regs)
        {
        }

        public SpellInfo(string name, string mantra, int action, int handEffect, bool allowTown, params Type[] regs)
            : this(name, mantra, action, handEffect, handEffect, allowTown, regs)
        {
        }

        public SpellInfo(string name, string mantra, int action, int leftHandEffect, int rightHandEffect, bool allowTown, params Type[] regs)
        {
            m_Name = name;
            m_Mantra = mantra;
            m_Action = action;
            m_Reagents = regs;
            m_AllowTown = allowTown;

            m_LeftHandEffect = leftHandEffect;
            m_RightHandEffect = rightHandEffect;

            m_Amounts = new int[regs.Length];

            for (int i = 0; i < regs.Length; ++i)
                m_Amounts[i] = 1;
        }

        public int Action { get { return m_Action; } set { m_Action = value; } }
        public bool AllowTown { get { return m_AllowTown; } set { m_AllowTown = value; } }
        public int[] Amounts { get { return m_Amounts; } set { m_Amounts = value; } }
        public string Mantra { get { return m_Mantra; } set { m_Mantra = value; } }
        public string Name { get { return m_Name; } set { m_Name = value; } }
        public Type[] Reagents { get { return m_Reagents; } set { m_Reagents = value; } }
        public int LeftHandEffect { get { return m_LeftHandEffect; } set { m_LeftHandEffect = value; } }
        public int RightHandEffect { get { return m_RightHandEffect; } set { m_RightHandEffect = value; } }
    }

    class UnsummonTimer : Timer
    {
        private BaseCreature m_Creature;
        private Mobile m_Caster;

        public UnsummonTimer(Mobile caster, BaseCreature creature, TimeSpan delay)
            : base(delay)
        {
            m_Caster = caster;
            m_Creature = creature;
            Priority = TimerPriority.OneSecond;
        }

        protected override void OnTick()
        {
            if (!m_Creature.Deleted)
                m_Creature.Delete();
        }
    }

    public class SpellHelper
    {
        private static TimeSpan AosDamageDelay = TimeSpan.FromSeconds(1.0);
        private static TimeSpan OldDamageDelay = TimeSpan.FromSeconds(0.5);

        public static TimeSpan GetDamageDelayForSpell(Spell sp)
        {
            if (!sp.DelayedDamage)
                return TimeSpan.Zero;

            return (Core.AOS ? AosDamageDelay : OldDamageDelay);
        }

        public static bool CheckMulti(Point3D p, Map map)
        {
            return CheckMulti(p, map, true, 0);
        }

        public static bool CheckMulti(Point3D p, Map map, bool houses)
        {
            return CheckMulti(p, map, houses, 0);
        }

        public static bool CheckMulti(Point3D p, Map map, bool houses, int housingrange)
        {
            if (map == null || map == Map.Internal)
                return false;

            Sector sector = map.GetSector(p.X, p.Y);

            for (int i = 0; i < sector.Multis.Count; ++i)
            {
                BaseMulti multi = sector.Multis[i];

                if (multi is BaseHouse)
                {
                    BaseHouse bh = (BaseHouse)multi;

                    if ((houses && bh.IsInside(p, 16)) || (housingrange > 0 && bh.InRange(p, housingrange)))
                        return true;
                }
                else if (multi.Contains(p))
                {
                    return true;
                }
            }

            return false;
        }

        public static void Turn(Mobile from, object to)
        {
            IPoint3D target = to as IPoint3D;

            if (target == null)
                return;

            if (target is Item)
            {
                Item item = (Item)target;

                if (item.RootParent != from)
                    from.Direction = from.GetDirectionTo(item.GetWorldLocation());
            }
            else if (from != target)
            {
                from.Direction = from.GetDirectionTo(target);
            }
        }

        private static TimeSpan CombatHeatDelay = TimeSpan.FromSeconds(30.0);
        private static bool RestrictTravelCombat = true;

        public static bool CheckCombat(Mobile m)
        {
            if (!RestrictTravelCombat)
                return false;

            for (int i = 0; i < m.Aggressed.Count; ++i)
            {
                AggressorInfo info = m.Aggressed[i];

                if (info.Defender.Player && (DateTime.UtcNow - info.LastCombatTime) < CombatHeatDelay)
                    return true;
            }

            if (Core.Expansion == Expansion.AOS)
            {
                for (int i = 0; i < m.Aggressors.Count; ++i)
                {
                    AggressorInfo info = m.Aggressors[i];

                    if (info.Attacker.Player && (DateTime.UtcNow - info.LastCombatTime) < CombatHeatDelay)
                        return true;
                }
            }

            return false;
        }

        public static bool AdjustField(ref Point3D p, Map map, int height, bool mobsBlock)
        {
            if (map == null)
                return false;

            for (int offset = 0; offset < 10; ++offset)
            {
                Point3D loc = new Point3D(p.X, p.Y, p.Z - offset);

                if (map.CanFit(loc, height, true, mobsBlock))
                {
                    p = loc;
                    return true;
                }
            }

            return false;
        }

        public static bool CanRevealCaster(Mobile m)
        {
            if (m is BaseCreature)
            {
                BaseCreature c = (BaseCreature)m;

                if (!c.Controlled)
                    return true;
            }

            return false;
        }

        public static void GetSurfaceTop(ref IPoint3D p)
        {
            if (p is Item)
            {
                p = ((Item)p).GetSurfaceTop();
            }
            else if (p is StaticTarget)
            {
                StaticTarget t = (StaticTarget)p;
                int z = t.Z;

                if ((t.Flags & TileFlag.Surface) == 0)
                    z -= TileData.ItemTable[t.ItemID & TileData.MaxItemValue].CalcHeight;

                p = new Point3D(t.X, t.Y, z);
            }
        }

        public static bool AddStatOffset(Mobile m, StatType type, int offset, TimeSpan duration)
        {
            if (offset > 0)
                return AddStatBonus(m, m, type, offset, duration);
            else if (offset < 0)
                return AddStatCurse(m, m, type, -offset, duration);

            return true;
        }

        public static bool AddStatBonus(Mobile caster, Mobile target, StatType type)
        {
            return AddStatBonus(caster, target, type, GetOffset(caster, target, type, false), GetDuration(caster, target));
        }

        public static bool AddStatBonus(Mobile caster, Mobile target, StatType type, int bonus, TimeSpan duration)
        {
            int offset = bonus;
            string name = String.Format("[Magic] {0} Offset", type);

            StatMod mod = target.GetStatMod(name);

            if (mod != null && mod.Offset < 0)
            {
                target.AddStatMod(new StatMod(type, name, mod.Offset + offset, duration));
                return true;
            }
            else if (mod == null || mod.Offset < offset)
            {
                target.AddStatMod(new StatMod(type, name, offset, duration));
                return true;
            }

            return false;
        }

        public static bool AddStatCurse(Mobile caster, Mobile target, StatType type)
        {
            return AddStatCurse(caster, target, type, GetOffset(caster, target, type, true), GetDuration(caster, target));
        }

        public static bool AddStatCurse(Mobile caster, Mobile target, StatType type, int curse, TimeSpan duration)
        {
            int offset = -curse;
            string name = String.Format("[Magic] {0} Offset", type);

            StatMod mod = target.GetStatMod(name);

            if (mod != null && mod.Offset > 0)
            {
                target.AddStatMod(new StatMod(type, name, mod.Offset + offset, duration));
                return true;
            }
            else if (mod == null || mod.Offset > offset)
            {
                target.AddStatMod(new StatMod(type, name, offset, duration));
                return true;
            }

            return false;
        }

        public static TimeSpan GetDuration(Mobile caster, Mobile target)
        {
            if (Core.AOS)
                return TimeSpan.FromSeconds(((6 * caster.Skills.EvalInt.Fixed) / 50) + 1);

            return TimeSpan.FromSeconds(caster.Skills[SkillName.Magery].Value * 1.2);
        }

        private static bool m_DisableSkillCheck;

        public static bool DisableSkillCheck
        {
            get { return m_DisableSkillCheck; }
            set { m_DisableSkillCheck = value; }
        }

        public static double GetOffsetScalar(Mobile caster, Mobile target, bool curse)
        {
            double percent;

            if (curse)
                percent = 8 + (caster.Skills.EvalInt.Fixed / 100) - (target.Skills.MagicResist.Fixed / 100);
            else
                percent = 1 + (caster.Skills.EvalInt.Fixed / 100);

            percent *= 0.01;

            if (percent < 0)
                percent = 0;

            return percent;
        }

        public static int GetOffset(Mobile caster, Mobile target, StatType type, bool curse)
        {
            if (Core.AOS)
            {
                if (!m_DisableSkillCheck)
                {
                    caster.CheckSkill(SkillName.EvalInt, 0.0, 120.0);

                    if (curse)
                        target.CheckSkill(SkillName.MagicResist, 0.0, 120.0);
                }

                double percent = GetOffsetScalar(caster, target, curse);

                switch (type)
                {
                    case StatType.Str:
                        return (int)(target.RawStr * percent);
                    case StatType.Dex:
                        return (int)(target.RawDex * percent);
                    case StatType.Int:
                        return (int)(target.RawInt * percent);
                }
            }

            return 1 + (int)(caster.Skills[SkillName.Magery].Value * 0.1);
        }

        public static Guild GetGuildFor(Mobile m)
        {
            Guild g = m.Guild as Guild;

            if (g == null && m is BaseCreature)
            {
                BaseCreature c = (BaseCreature)m;
                m = c.ControlMaster;

                if (m != null)
                    g = m.Guild as Guild;

                if (g == null)
                {
                    m = c.SummonMaster;

                    if (m != null)
                        g = m.Guild as Guild;
                }
            }

            return g;
        }

        public static bool ValidIndirectTarget(Mobile from, Mobile to)
        {
            if (from == to)
                return true;

            if (to.Hidden && to.AccessLevel > from.AccessLevel)
                return false;

            #region Dueling
            PlayerMobile pmFrom = from as PlayerMobile;
            PlayerMobile pmTarg = to as PlayerMobile;

            if (pmFrom == null && from is BaseCreature)
            {
                BaseCreature bcFrom = (BaseCreature)from;

                if (bcFrom.Summoned)
                    pmFrom = bcFrom.SummonMaster as PlayerMobile;
            }

            if (pmTarg == null && to is BaseCreature)
            {
                BaseCreature bcTarg = (BaseCreature)to;

                if (bcTarg.Summoned)
                    pmTarg = bcTarg.SummonMaster as PlayerMobile;
            }

            if (pmFrom != null && pmTarg != null)
            {
                if (pmFrom.DuelContext != null && pmFrom.DuelContext == pmTarg.DuelContext && pmFrom.DuelContext.Started && pmFrom.DuelPlayer != null && pmTarg.DuelPlayer != null)
                    return (pmFrom.DuelPlayer.Participant != pmTarg.DuelPlayer.Participant);
            }
            #endregion

            Guild fromGuild = GetGuildFor(from);
            Guild toGuild = GetGuildFor(to);

            if (fromGuild != null && toGuild != null && (fromGuild == toGuild || fromGuild.IsAlly(toGuild)))
                return false;

            Party p = Party.Get(from);

            if (p != null && p.Contains(to))
                return false;

            if (to is BaseCreature)
            {
                BaseCreature c = (BaseCreature)to;

                if (c.Controlled || c.Summoned)
                {
                    if (c.ControlMaster == from || c.SummonMaster == from)
                        return false;

                    if (p != null && (p.Contains(c.ControlMaster) || p.Contains(c.SummonMaster)))
                        return false;
                }
            }

            if (from is BaseCreature)
            {
                BaseCreature c = (BaseCreature)from;

                if (c.Controlled || c.Summoned)
                {
                    if (c.ControlMaster == to || c.SummonMaster == to)
                        return false;

                    p = Party.Get(to);

                    if (p != null && (p.Contains(c.ControlMaster) || p.Contains(c.SummonMaster)))
                        return false;
                }
            }

            if (to is BaseCreature && !((BaseCreature)to).Controlled && ((BaseCreature)to).InitialInnocent)
                return true;

            int noto = Notoriety.Compute(from, to);

            return (noto != Notoriety.Innocent || from.Kills >= 5);
        }

        private static int[] m_Offsets = new int[]
            {
                -1, -1,
                -1,  0,
                -1,  1,
                0, -1,
                0,  1,
                1, -1,
                1,  0,
                1,  1
            };

        public static void Summon(BaseCreature creature, Mobile caster, int sound, TimeSpan duration, bool scaleDuration, bool scaleStats)
        {
            Map map = caster.Map;

            if (map == null)
                return;

            double scale = 1.0 + ((caster.Skills[SkillName.Magery].Value - 100.0) / 200.0);

            if (scaleDuration)
                duration = TimeSpan.FromSeconds(duration.TotalSeconds * scale);

            if (scaleStats)
            {
                creature.RawStr = (int)(creature.RawStr * scale);
                creature.Hits = creature.HitsMax;

                creature.RawDex = (int)(creature.RawDex * scale);
                creature.Stam = creature.StamMax;

                creature.RawInt = (int)(creature.RawInt * scale);
                creature.Mana = creature.ManaMax;
            }

            Point3D p = new Point3D(caster);

            if (SpellHelper.FindValidSpawnLocation(map, ref p, true))
            {
                BaseCreature.Summon(creature, caster, p, sound, duration);
                return;
            }


            /*
            int offset = Utility.Random( 8 ) * 2;

            for( int i = 0; i < m_Offsets.Length; i += 2 )
            {
                int x = caster.X + m_Offsets[(offset + i) % m_Offsets.Length];
                int y = caster.Y + m_Offsets[(offset + i + 1) % m_Offsets.Length];

                if( map.CanSpawnMobile( x, y, caster.Z ) )
                {
                    BaseCreature.Summon( creature, caster, new Point3D( x, y, caster.Z ), sound, duration );
                    return;
                }
                else
                {
                    int z = map.GetAverageZ( x, y );

                    if( map.CanSpawnMobile( x, y, z ) )
                    {
                        BaseCreature.Summon( creature, caster, new Point3D( x, y, z ), sound, duration );
                        return;
                    }
                }
            }
             * */

            creature.Delete();
            caster.SendLocalizedMessage(501942); // That location is blocked.
        }

        public static bool FindValidSpawnLocation(Map map, ref Point3D p, bool surroundingsOnly)
        {
            if (map == null)	//sanity
                return false;

            if (!surroundingsOnly)
            {
                if (map.CanSpawnMobile(p))	//p's fine.
                {
                    p = new Point3D(p);
                    return true;
                }

                int z = map.GetAverageZ(p.X, p.Y);

                if (map.CanSpawnMobile(p.X, p.Y, z))
                {
                    p = new Point3D(p.X, p.Y, z);
                    return true;
                }
            }

            int offset = Utility.Random(8) * 2;

            for (int i = 0; i < m_Offsets.Length; i += 2)
            {
                int x = p.X + m_Offsets[(offset + i) % m_Offsets.Length];
                int y = p.Y + m_Offsets[(offset + i + 1) % m_Offsets.Length];

                if (map.CanSpawnMobile(x, y, p.Z))
                {
                    p = new Point3D(x, y, p.Z);
                    return true;
                }
                else
                {
                    int z = map.GetAverageZ(x, y);

                    if (map.CanSpawnMobile(x, y, z))
                    {
                        p = new Point3D(x, y, z);
                        return true;
                    }
                }
            }

            return false;
        }

        private delegate bool TravelValidator(Map map, Point3D loc);

        private static TravelValidator[] m_Validators = new TravelValidator[]
            {
                new TravelValidator( IsFeluccaT2A ),
                new TravelValidator( IsKhaldun ),
                new TravelValidator( IsIlshenar ),
                new TravelValidator( IsTrammelWind ),
                new TravelValidator( IsFeluccaWind ),
                new TravelValidator( IsFeluccaDungeon ),
                new TravelValidator( IsTrammelSolenHive ),
                new TravelValidator( IsFeluccaSolenHive ),
                new TravelValidator( IsCrystalCave ),
                new TravelValidator( IsDoomGauntlet ),
                new TravelValidator( IsDoomFerry ),
                new TravelValidator( IsSafeZone ),
                new TravelValidator( IsFactionStronghold ),
                new TravelValidator( IsChampionSpawn ),
                new TravelValidator( IsTokunoDungeon ),
                new TravelValidator( IsLampRoom ),
                new TravelValidator( IsGuardianRoom ),
                new TravelValidator( IsHeartwood ),
                new TravelValidator( IsMLDungeon )
            };

        private static bool[,] m_Rules = new bool[,]
            {
					/*T2A(Fel),	Khaldun,	Ilshenar,	Wind(Tram),	Wind(Fel),	Dungeons(Fel),	Solen(Tram),	Solen(Fel),	CrystalCave(Malas),	Gauntlet(Malas),	Gauntlet(Ferry),	SafeZone,	Stronghold,	ChampionSpawn,	Dungeons(Tokuno[Malas]),	LampRoom(Doom),	GuardianRoom(Doom),	Heartwood,	MLDungeons */
/* Recall From */	{ false,    false,      true,       true,       false,      false,          true,           false,      false,              false,              false,              true,       true,       false,          true,                       false,          false,              false,      false },
/* Recall To */		{ false,    false,      false,      false,      false,      false,          false,          false,      false,              false,              false,              false,      false,      false,          false,                      false,          false,              false,      false },
/* Gate From */		{ false,    false,      false,      false,      false,      false,          false,          false,      false,              false,              false,              false,      false,      false,          false,                      false,          false,              false,      false },
/* Gate To */		{ false,    false,      false,      false,      false,      false,          false,          false,      false,              false,              false,              false,      false,      false,          false,                      false,          false,              false,      false },
/* Mark In */		{ false,    false,      false,      false,      false,      false,          false,          false,      false,              false,              false,              false,      false,      false,          false,                      false,          false,              false,      false },
/* Tele From */		{ true,     true,       true,       true,       true,       true,           true,           true,       false,              true,               true,               true,       false,      true,           true,                       true,           true,               false,      true },
/* Tele To */		{ true,     true,       true,       true,       true,       true,           true,           true,       false,              true,               false,              false,      false,      true,           true,                       true,           true,               false,      false },
            };

        public static void SendInvalidMessage(Mobile caster, TravelCheckType type)
        {
            if (type == TravelCheckType.RecallTo || type == TravelCheckType.GateTo)
                caster.SendLocalizedMessage(1019004); // You are not allowed to travel there.
            else if (type == TravelCheckType.TeleportTo)
                caster.SendLocalizedMessage(501035); // You cannot teleport from here to the destination.
            else
                caster.SendLocalizedMessage(501802); // Thy spell doth not appear to work...
        }

        public static bool CheckTravel(Mobile caster, TravelCheckType type)
        {
            return CheckTravel(caster, caster.Map, caster.Location, type);
        }

        public static bool CheckTravel(Map map, Point3D loc, TravelCheckType type)
        {
            return CheckTravel(null, map, loc, type);
        }

        private static Mobile m_TravelCaster;
        private static TravelCheckType m_TravelType;

        public static bool CheckTravel(Mobile caster, Map map, Point3D loc, TravelCheckType type)
        {
            if (IsInvalid(map, loc)) // null, internal, out of bounds
            {
                if (caster != null)
                    SendInvalidMessage(caster, type);

                return false;
            }

            if (caster != null && caster.AccessLevel == AccessLevel.Player && caster.Region.IsPartOf(typeof(Regions.Jail)))
            {
                caster.SendLocalizedMessage(1114345); // You'll need a better jailbreak plan than that!
                return false;
            }

            // Always allow monsters to teleport
            if (caster is BaseCreature && (type == TravelCheckType.TeleportTo || type == TravelCheckType.TeleportFrom))
            {
                BaseCreature bc = (BaseCreature)caster;

                if (!bc.Controlled && !bc.Summoned)
                    return true;
            }

            m_TravelCaster = caster;
            m_TravelType = type;

            int v = (int)type;
            bool isValid = true;

            for (int i = 0; isValid && i < m_Validators.Length; ++i)
                isValid = (m_Rules[v, i] || !m_Validators[i](map, loc));

            if (!isValid && caster != null)
                SendInvalidMessage(caster, type);

            return isValid;
        }

        public static bool IsWindLoc(Point3D loc)
        {
            int x = loc.X, y = loc.Y;

            return (x >= 5120 && y >= 0 && x < 5376 && y < 256);
        }

        public static bool IsFeluccaWind(Map map, Point3D loc)
        {
            return (map == Map.Felucca && IsWindLoc(loc));
        }

        public static bool IsTrammelWind(Map map, Point3D loc)
        {
            return (map == Map.Trammel && IsWindLoc(loc));
        }

        public static bool IsIlshenar(Map map, Point3D loc)
        {
            return (map == Map.Ilshenar);
        }

        public static bool IsSolenHiveLoc(Point3D loc)
        {
            int x = loc.X, y = loc.Y;

            return (x >= 5640 && y >= 1776 && x < 5935 && y < 2039);
        }

        public static bool IsTrammelSolenHive(Map map, Point3D loc)
        {
            return (map == Map.Trammel && IsSolenHiveLoc(loc));
        }

        public static bool IsFeluccaSolenHive(Map map, Point3D loc)
        {
            return (map == Map.Felucca && IsSolenHiveLoc(loc));
        }

        public static bool IsFeluccaT2A(Map map, Point3D loc)
        {
            int x = loc.X, y = loc.Y;

            return (map == Map.Felucca && x >= 5120 && y >= 2304 && x < 6144 && y < 4096);
        }

        public static bool IsAnyT2A(Map map, Point3D loc)
        {
            int x = loc.X, y = loc.Y;

            return ((map == Map.Trammel || map == Map.Felucca) && x >= 5120 && y >= 2304 && x < 6144 && y < 4096);
        }

        public static bool IsFeluccaDungeon(Map map, Point3D loc)
        {
            Region region = Region.Find(loc, map);
            return (region.IsPartOf(typeof(DungeonRegion)) && region.Map == Map.Felucca);
        }

        public static bool IsKhaldun(Map map, Point3D loc)
        {
            return (Region.Find(loc, map).Name == "Khaldun");
        }

        public static bool IsCrystalCave(Map map, Point3D loc)
        {
            if (map != Map.Malas || loc.Z >= -80)
                return false;

            int x = loc.X, y = loc.Y;

            return (x >= 1182 && y >= 437 && x < 1211 && y < 470)
                || (x >= 1156 && y >= 470 && x < 1211 && y < 503)
                || (x >= 1176 && y >= 503 && x < 1208 && y < 509)
                || (x >= 1188 && y >= 509 && x < 1201 && y < 513);
        }

        public static bool IsSafeZone(Map map, Point3D loc)
        {
            #region Duels
            if (Region.Find(loc, map).IsPartOf(typeof(Engines.ConPVP.SafeZone)))
            {
                if (m_TravelType == TravelCheckType.TeleportTo || m_TravelType == TravelCheckType.TeleportFrom)
                {
                    PlayerMobile pm = m_TravelCaster as PlayerMobile;

                    if (pm != null && pm.DuelPlayer != null && !pm.DuelPlayer.Eliminated)
                        return true;
                }

                return true;
            }
            #endregion

            return false;
        }

        public static bool IsFactionStronghold(Map map, Point3D loc)
        {
            /*// Teleporting is allowed, but only for faction members
            if ( !Core.AOS && m_TravelCaster != null && (m_TravelType == TravelCheckType.TeleportTo || m_TravelType == TravelCheckType.TeleportFrom) )
            {
                if ( Factions.Faction.Find( m_TravelCaster, true, true ) != null )
                    return false;
            }*/

            return (Region.Find(loc, map).IsPartOf(typeof(Factions.StrongholdRegion)));
        }

        public static bool IsChampionSpawn(Map map, Point3D loc)
        {
            return (Region.Find(loc, map).IsPartOf(typeof(Engines.CannedEvil.ChampionSpawnRegion)));
        }

        public static bool IsDoomFerry(Map map, Point3D loc)
        {
            if (map != Map.Malas)
                return false;

            int x = loc.X, y = loc.Y;

            if (x >= 426 && y >= 314 && x <= 430 && y <= 331)
                return true;

            if (x >= 406 && y >= 247 && x <= 410 && y <= 264)
                return true;

            return false;
        }

        public static bool IsTokunoDungeon(Map map, Point3D loc)
        {
            //The tokuno dungeons are really inside malas
            if (map != Map.Malas)
                return false;

            int x = loc.X, y = loc.Y, z = loc.Z;

            bool r1 = (x >= 0 && y >= 0 && x <= 128 && y <= 128);
            bool r2 = (x >= 45 && y >= 320 && x < 195 && y < 710);

            return (r1 || r2);
        }

        public static bool IsDoomGauntlet(Map map, Point3D loc)
        {
            if (map != Map.Malas)
                return false;

            int x = loc.X - 256, y = loc.Y - 304;

            return (x >= 0 && y >= 0 && x < 256 && y < 256);
        }

        public static bool IsLampRoom(Map map, Point3D loc)
        {
            if (map != Map.Malas)
                return false;

            int x = loc.X, y = loc.Y;

            return (x >= 465 && y >= 92 && x < 474 && y < 102);
        }

        public static bool IsGuardianRoom(Map map, Point3D loc)
        {
            if (map != Map.Malas)
                return false;

            int x = loc.X, y = loc.Y;

            return (x >= 356 && y >= 5 && x < 375 && y < 25);
        }

        public static bool IsHeartwood(Map map, Point3D loc)
        {
            int x = loc.X, y = loc.Y;

            return (map == Map.Trammel || map == Map.Felucca) && (x >= 6911 && y >= 254 && x < 7167 && y < 511);
        }

        public static bool IsMLDungeon(Map map, Point3D loc)
        {
            return MondainsLegacy.IsMLRegion(Region.Find(loc, map));
        }

        public static bool IsInvalid(Map map, Point3D loc)
        {
            if (map == null || map == Map.Internal)
                return true;

            int x = loc.X, y = loc.Y;

            return (x < 0 || y < 0 || x >= map.Width || y >= map.Height);
        }

        //towns
        public static bool IsTown(IPoint3D loc, Mobile caster)
        {
            if (loc is Item)
                loc = ((Item)loc).GetWorldLocation();

            return IsTown(new Point3D(loc), caster);
        }

        public static bool IsTown(Point3D loc, Mobile caster)
        {
            Map map = caster.Map;

            if (map == null)
                return false;

            #region Dueling
            Engines.ConPVP.SafeZone sz = (Engines.ConPVP.SafeZone)Region.Find(loc, map).GetRegion(typeof(Engines.ConPVP.SafeZone));

            if (sz != null)
            {
                PlayerMobile pm = (PlayerMobile)caster;

                if (pm == null || pm.DuelContext == null || !pm.DuelContext.Started || pm.DuelPlayer == null || pm.DuelPlayer.Eliminated)
                    return true;
            }
            #endregion

            GuardedRegion reg = (GuardedRegion)Region.Find(loc, map).GetRegion(typeof(GuardedRegion));

            return (reg != null && !reg.IsDisabled());
        }

        public static bool CheckTown(IPoint3D loc, Mobile caster)
        {
            if (loc is Item)
                loc = ((Item)loc).GetWorldLocation();

            return CheckTown(new Point3D(loc), caster);
        }

        public static bool CheckTown(Point3D loc, Mobile caster)
        {
            if (IsTown(loc, caster))
            {
                caster.SendLocalizedMessage(500946); // You cannot cast this in town!
                return false;
            }

            return true;
        }

        //magic reflection
        public static void CheckReflect(int circle, Mobile caster, ref Mobile target)
        {
            CheckReflect(circle, ref caster, ref target);
        }

        public static void CheckReflect(int circle, ref Mobile caster, ref Mobile target)
        {
            if (target.MagicDamageAbsorb > 0)
            {
                ++circle;

                target.MagicDamageAbsorb -= circle;

                // This order isn't very intuitive, but you have to nullify reflect before target gets switched

                bool reflect = (target.MagicDamageAbsorb >= 0);

                if (target is BaseCreature)
                    ((BaseCreature)target).CheckReflect(caster, ref reflect);

                if (target.MagicDamageAbsorb <= 0)
                {
                    target.MagicDamageAbsorb = 0;
                    DefensiveSpell.Nullify(target);
                }

                if (reflect)
                {
                    target.FixedEffect(0x37B9, 10, 5);

                    Mobile temp = caster;
                    caster = target;
                    target = temp;
                }
            }
            else if (target is BaseCreature)
            {
                bool reflect = false;

                ((BaseCreature)target).CheckReflect(caster, ref reflect);

                if (reflect)
                {
                    target.FixedEffect(0x37B9, 10, 5);

                    Mobile temp = caster;
                    caster = target;
                    target = temp;
                }
            }
        }

        public static void Damage(Spell spell, Mobile target, double damage)
        {
            TimeSpan ts = GetDamageDelayForSpell(spell);

            Damage(spell, ts, target, spell.Caster, damage);
        }

        public static void Damage(TimeSpan delay, Mobile target, double damage)
        {
            Damage(delay, target, null, damage);
        }

        public static void Damage(TimeSpan delay, Mobile target, Mobile from, double damage)
        {
            Damage(null, delay, target, from, damage);
        }

        public static void Damage(Spell spell, TimeSpan delay, Mobile target, Mobile from, double damage)
        {
            int iDamage = (int)damage;

            if (delay == TimeSpan.Zero)
            {
                if (from is BaseCreature)
                    ((BaseCreature)from).AlterSpellDamageTo(target, ref iDamage);

                if (target is BaseCreature)
                    ((BaseCreature)target).AlterSpellDamageFrom(from, ref iDamage);

                target.Damage(iDamage, from);
            }
            else
            {
                new SpellDamageTimer(spell, target, from, iDamage, delay).Start();
            }

            if (target is BaseCreature && from != null && delay == TimeSpan.Zero)
            {
                BaseCreature c = (BaseCreature)target;

                c.OnHarmfulSpell(from);
                c.OnDamagedBySpell(from);
            }
        }

        public static void Damage(Spell spell, Mobile target, double damage, int phys, int fire, int cold, int pois, int nrgy)
        {
            TimeSpan ts = GetDamageDelayForSpell(spell);

            Damage(spell, ts, target, spell.Caster, damage, phys, fire, cold, pois, nrgy, DFAlgorithm.Standard);
        }

        public static void Damage(Spell spell, Mobile target, double damage, int phys, int fire, int cold, int pois, int nrgy, DFAlgorithm dfa)
        {
            TimeSpan ts = GetDamageDelayForSpell(spell);

            Damage(spell, ts, target, spell.Caster, damage, phys, fire, cold, pois, nrgy, dfa);
        }

        public static void Damage(TimeSpan delay, Mobile target, double damage, int phys, int fire, int cold, int pois, int nrgy)
        {
            Damage(delay, target, null, damage, phys, fire, cold, pois, nrgy);
        }

        public static void Damage(TimeSpan delay, Mobile target, Mobile from, double damage, int phys, int fire, int cold, int pois, int nrgy)
        {
            Damage(delay, target, from, damage, phys, fire, cold, pois, nrgy, DFAlgorithm.Standard);
        }

        public static void Damage(TimeSpan delay, Mobile target, Mobile from, double damage, int phys, int fire, int cold, int pois, int nrgy, DFAlgorithm dfa)
        {
            Damage(null, delay, target, from, damage, phys, fire, cold, pois, nrgy, dfa);
        }

        public static void Damage(Spell spell, TimeSpan delay, Mobile target, Mobile from, double damage, int phys, int fire, int cold, int pois, int nrgy, DFAlgorithm dfa)
        {
            int iDamage = (int)damage;

            if (delay == TimeSpan.Zero)
            {
                if (from is BaseCreature)
                    ((BaseCreature)from).AlterSpellDamageTo(target, ref iDamage);

                if (target is BaseCreature)
                    ((BaseCreature)target).AlterSpellDamageFrom(from, ref iDamage);

                WeightOverloading.DFA = dfa;

                int damageGiven = AOS.Damage(target, from, iDamage, phys, fire, cold, pois, nrgy);

                if (from != null) // sanity check
                {
                    DoLeech(damageGiven, from, target);
                }

                WeightOverloading.DFA = DFAlgorithm.Standard;
            }
            else
            {
                new SpellDamageTimerAOS(spell, target, from, iDamage, phys, fire, cold, pois, nrgy, delay, dfa).Start();
            }

            if (target is BaseCreature && from != null && delay == TimeSpan.Zero)
            {
                BaseCreature c = (BaseCreature)target;

                c.OnHarmfulSpell(from);
                c.OnDamagedBySpell(from);
            }
        }

        public static void DoLeech(int damageGiven, Mobile from, Mobile target)
        {
            TransformContext context = TransformationSpellHelper.GetContext(from);

            if (context != null) /* cleanup */
            {
                if (context.Type == typeof(WraithFormSpell))
                {
                    int wraithLeech = (5 + (int)((15 * from.Skills.SpiritSpeak.Value) / 100)); // Wraith form gives 5-20% mana leech
                    int manaLeech = AOS.Scale(damageGiven, wraithLeech);
                    if (manaLeech != 0)
                    {
                        from.Mana += manaLeech;
                        from.PlaySound(0x44D);
                    }
                }
                else if (context.Type == typeof(VampiricEmbraceSpell))
                {
                    from.Hits += AOS.Scale(damageGiven, 20);
                    from.PlaySound(0x44D);
                }
            }
        }

        public static void Heal(int amount, Mobile target, Mobile from)
        {
            Heal(amount, target, from, true);
        }
        public static void Heal(int amount, Mobile target, Mobile from, bool message)
        {
            //TODO: All Healing *spells* go through ArcaneEmpowerment
            target.Heal(amount, from, message);
        }

        private class SpellDamageTimer : Timer
        {
            private Mobile m_Target, m_From;
            private int m_Damage;
            private Spell m_Spell;

            public SpellDamageTimer(Spell s, Mobile target, Mobile from, int damage, TimeSpan delay)
                : base(delay)
            {
                m_Target = target;
                m_From = from;
                m_Damage = damage;
                m_Spell = s;

                if (m_Spell != null && m_Spell.DelayedDamage && !m_Spell.DelayedDamageStacking)
                    m_Spell.StartDelayedDamageContext(target, this);

                Priority = TimerPriority.TwentyFiveMS;
            }

            protected override void OnTick()
            {
                if (m_From is BaseCreature)
                    ((BaseCreature)m_From).AlterSpellDamageTo(m_Target, ref m_Damage);

                if (m_Target is BaseCreature)
                    ((BaseCreature)m_Target).AlterSpellDamageFrom(m_From, ref m_Damage);

                m_Target.Damage(m_Damage);
                if (m_Spell != null)
                    m_Spell.RemoveDelayedDamageContext(m_Target);
            }
        }

        private class SpellDamageTimerAOS : Timer
        {
            private Mobile m_Target, m_From;
            private int m_Damage;
            private int m_Phys, m_Fire, m_Cold, m_Pois, m_Nrgy;
            private DFAlgorithm m_DFA;
            private Spell m_Spell;

            public SpellDamageTimerAOS(Spell s, Mobile target, Mobile from, int damage, int phys, int fire, int cold, int pois, int nrgy, TimeSpan delay, DFAlgorithm dfa)
                : base(delay)
            {
                m_Target = target;
                m_From = from;
                m_Damage = damage;
                m_Phys = phys;
                m_Fire = fire;
                m_Cold = cold;
                m_Pois = pois;
                m_Nrgy = nrgy;
                m_DFA = dfa;
                m_Spell = s;
                if (m_Spell != null && m_Spell.DelayedDamage && !m_Spell.DelayedDamageStacking)
                    m_Spell.StartDelayedDamageContext(target, this);

                Priority = TimerPriority.TwentyFiveMS;
            }

            protected override void OnTick()
            {
                if (m_From is BaseCreature && m_Target != null)
                    ((BaseCreature)m_From).AlterSpellDamageTo(m_Target, ref m_Damage);

                if (m_Target is BaseCreature && m_From != null)
                    ((BaseCreature)m_Target).AlterSpellDamageFrom(m_From, ref m_Damage);

                WeightOverloading.DFA = m_DFA;

                int damageGiven = AOS.Damage(m_Target, m_From, m_Damage, m_Phys, m_Fire, m_Cold, m_Pois, m_Nrgy);

                if (m_From != null) // sanity check
                {
                    DoLeech(damageGiven, m_From, m_Target);
                }

                WeightOverloading.DFA = DFAlgorithm.Standard;

                if (m_Target is BaseCreature && m_From != null)
                {
                    BaseCreature c = (BaseCreature)m_Target;

                    c.OnHarmfulSpell(m_From);
                    c.OnDamagedBySpell(m_From);
                }

                if (m_Spell != null)
                    m_Spell.RemoveDelayedDamageContext(m_Target);

            }
        }
    }

    public abstract class SpecialMove
    {
        public virtual int BaseMana { get { return 0; } }

        public virtual SkillName MoveSkill { get { return SkillName.Bushido; } }
        public virtual double RequiredSkill { get { return 0.0; } }

        public virtual TextDefinition AbilityMessage { get { return 0; } }

        public virtual bool BlockedByAnimalForm { get { return true; } }
        public virtual bool DelayedContext { get { return false; } }

        public virtual int GetAccuracyBonus(Mobile attacker)
        {
            return 0;
        }

        public virtual double GetDamageScalar(Mobile attacker, Mobile defender)
        {
            return 1.0;
        }

        // Called before swinging, to make sure the accuracy scalar is to be computed.
        public virtual bool OnBeforeSwing(Mobile attacker, Mobile defender)
        {
            return true;
        }

        // Called when a hit connects, but before damage is calculated.
        public virtual bool OnBeforeDamage(Mobile attacker, Mobile defender)
        {
            return true;
        }

        // Called as soon as the ability is used.
        public virtual void OnUse(Mobile from)
        {
        }

        // Called when a hit connects, at the end of the weapon.OnHit() method.
        public virtual void OnHit(Mobile attacker, Mobile defender, int damage)
        {
        }

        // Called when a hit misses.
        public virtual void OnMiss(Mobile attacker, Mobile defender)
        {
        }

        // Called when the move is cleared.
        public virtual void OnClearMove(Mobile from)
        {
        }

        public virtual bool IgnoreArmor(Mobile attacker)
        {
            return false;
        }

        public virtual double GetPropertyBonus(Mobile attacker)
        {
            return 1.0;
        }

        public virtual bool CheckSkills(Mobile m)
        {
            if (m.Skills[MoveSkill].Value < RequiredSkill)
            {
                string args = String.Format("{0}\t{1}\t ", RequiredSkill.ToString("F1"), MoveSkill.ToString());
                m.SendLocalizedMessage(1063013, args); // You need at least ~1_SKILL_REQUIREMENT~ ~2_SKILL_NAME~ skill to use that ability.
                return false;
            }

            return true;
        }

        public virtual int ScaleMana(Mobile m, int mana)
        {
            double scalar = 1.0;

            if (!Server.Spells.Necromancy.MindRotSpell.GetMindRotScalar(m, ref scalar))
                scalar = 1.0;

            // Lower Mana Cost = 40%
            int lmc = Math.Min(AosAttributes.GetValue(m, AosAttribute.LowerManaCost), 40);

            scalar -= (double)lmc / 100;

            int total = (int)(mana * scalar);

            if (m.Skills[MoveSkill].Value < 50.0 && GetContext(m) != null)
                total *= 2;

            return total;
        }

        public virtual bool CheckMana(Mobile from, bool consume)
        {
            int mana = ScaleMana(from, BaseMana);

            if (from.Mana < mana)
            {
                from.SendLocalizedMessage(1060181, mana.ToString()); // You need ~1_MANA_REQUIREMENT~ mana to perform that attack
                return false;
            }

            if (consume)
            {
                if (!DelayedContext)
                    SetContext(from);

                from.Mana -= mana;
            }

            return true;
        }

        public virtual void SetContext(Mobile from)
        {
            if (GetContext(from) == null)
            {
                if (DelayedContext || from.Skills[MoveSkill].Value < 50.0)
                {
                    Timer timer = new SpecialMoveTimer(from);
                    timer.Start();

                    AddContext(from, new SpecialMoveContext(timer, this.GetType()));
                }
            }
        }

        public virtual bool Validate(Mobile from)
        {
            if (!from.Player)
                return true;

            if (Bushido.HonorableExecution.IsUnderPenalty(from))
            {
                from.SendLocalizedMessage(1063024); // You cannot perform this special move right now.
                return false;
            }

            if (Ninjitsu.AnimalForm.UnderTransformation(from))
            {
                from.SendLocalizedMessage(1063024); // You cannot perform this special move right now.
                return false;
            }

            #region Dueling
            string option = null;

            if (this is Backstab)
                option = "Backstab";
            else if (this is DeathStrike)
                option = "Death Strike";
            else if (this is FocusAttack)
                option = "Focus Attack";
            else if (this is KiAttack)
                option = "Ki Attack";
            else if (this is SurpriseAttack)
                option = "Surprise Attack";
            else if (this is HonorableExecution)
                option = "Honorable Execution";
            else if (this is LightningStrike)
                option = "Lightning Strike";
            else if (this is MomentumStrike)
                option = "Momentum Strike";

            if (option != null && !Engines.ConPVP.DuelContext.AllowSpecialMove(from, option, this))
                return false;
            #endregion

            return CheckSkills(from) && CheckMana(from, false);
        }

        public virtual void CheckGain(Mobile m)
        {
            m.CheckSkill(MoveSkill, RequiredSkill, RequiredSkill + 37.5);
        }

        private static Dictionary<Mobile, SpecialMove> m_Table = new Dictionary<Mobile, SpecialMove>();

        public static Dictionary<Mobile, SpecialMove> Table { get { return m_Table; } }

        public static void ClearAllMoves(Mobile m)
        {
            foreach (KeyValuePair<Int32, SpecialMove> kvp in SpellRegistry.SpecialMoves)
            {
                int moveID = kvp.Key;

                if (moveID != -1)
                    m.Send(new ToggleSpecialAbility(moveID + 1, false));
            }
        }

        public virtual bool ValidatesDuringHit { get { return true; } }

        public static SpecialMove GetCurrentMove(Mobile m)
        {
            if (m == null)
                return null;

            if (!Core.SE)
            {
                ClearCurrentMove(m);
                return null;
            }

            SpecialMove move = null;
            m_Table.TryGetValue(m, out move);

            if (move != null && move.ValidatesDuringHit && !move.Validate(m))
            {
                ClearCurrentMove(m);
                return null;
            }

            return move;
        }

        public static bool SetCurrentMove(Mobile m, SpecialMove move)
        {
            if (!Core.SE)
            {
                ClearCurrentMove(m);
                return false;
            }

            if (move != null && !move.Validate(m))
            {
                ClearCurrentMove(m);
                return false;
            }

            bool sameMove = (move == GetCurrentMove(m));

            ClearCurrentMove(m);

            if (sameMove)
                return true;

            if (move != null)
            {
                WeaponAbility.ClearCurrentAbility(m);

                m_Table[m] = move;

                move.OnUse(m);

                int moveID = SpellRegistry.GetRegistryNumber(move);

                if (moveID > 0)
                    m.Send(new ToggleSpecialAbility(moveID + 1, true));

                TextDefinition.SendMessageTo(m, move.AbilityMessage);
            }

            return true;
        }

        public static void ClearCurrentMove(Mobile m)
        {
            SpecialMove move = null;
            m_Table.TryGetValue(m, out move);

            if (move != null)
            {
                move.OnClearMove(m);

                int moveID = SpellRegistry.GetRegistryNumber(move);

                if (moveID > 0)
                    m.Send(new ToggleSpecialAbility(moveID + 1, false));
            }

            m_Table.Remove(m);
        }

        public SpecialMove()
        {
        }


        private static Dictionary<Mobile, SpecialMoveContext> m_PlayersTable = new Dictionary<Mobile, SpecialMoveContext>();

        private static void AddContext(Mobile m, SpecialMoveContext context)
        {
            m_PlayersTable[m] = context;
        }

        private static void RemoveContext(Mobile m)
        {
            SpecialMoveContext context = GetContext(m);

            if (context != null)
            {
                m_PlayersTable.Remove(m);

                context.Timer.Stop();
            }
        }

        private static SpecialMoveContext GetContext(Mobile m)
        {
            return (m_PlayersTable.ContainsKey(m) ? m_PlayersTable[m] : null);
        }

        public static bool GetContext(Mobile m, Type type)
        {
            SpecialMoveContext context = null;
            m_PlayersTable.TryGetValue(m, out context);

            if (context == null)
                return false;

            return (context.Type == type);
        }

        private class SpecialMoveTimer : Timer
        {
            private Mobile m_Mobile;

            public SpecialMoveTimer(Mobile from)
                : base(TimeSpan.FromSeconds(3.0))
            {
                m_Mobile = from;

                Priority = TimerPriority.TwentyFiveMS;
            }

            protected override void OnTick()
            {
                RemoveContext(m_Mobile);
            }
        }

        public class SpecialMoveContext
        {
            private Timer m_Timer;
            private Type m_Type;

            public Timer Timer { get { return m_Timer; } }
            public Type Type { get { return m_Type; } }

            public SpecialMoveContext(Timer timer, Type type)
            {
                m_Timer = timer;
                m_Type = type;
            }
        }
    }

    public class TransformationSpellHelper
    {
        #region Context Stuff
        private static Dictionary<Mobile, TransformContext> m_Table = new Dictionary<Mobile, TransformContext>();

        public static void AddContext(Mobile m, TransformContext context)
        {
            m_Table[m] = context;
        }

        public static void RemoveContext(Mobile m, bool resetGraphics)
        {
            TransformContext context = GetContext(m);

            if (context != null)
                RemoveContext(m, context, resetGraphics);
        }

        public static void RemoveContext(Mobile m, TransformContext context, bool resetGraphics)
        {
            if (m_Table.ContainsKey(m))
            {
                m_Table.Remove(m);

                List<ResistanceMod> mods = context.Mods;

                for (int i = 0; i < mods.Count; ++i)
                    m.RemoveResistanceMod(mods[i]);

                if (resetGraphics)
                {
                    m.HueMod = -1;
                    m.BodyMod = 0;
                }

                context.Timer.Stop();
                context.Spell.RemoveEffect(m);
            }
        }

        public static TransformContext GetContext(Mobile m)
        {
            TransformContext context = null;

            m_Table.TryGetValue(m, out context);

            return context;
        }

        public static bool UnderTransformation(Mobile m)
        {
            return (GetContext(m) != null);
        }

        public static bool UnderTransformation(Mobile m, Type type)
        {
            TransformContext context = GetContext(m);

            return (context != null && context.Type == type);
        }
        #endregion

        public static bool CheckCast(Mobile caster, Spell spell)
        {
            if (Factions.Sigil.ExistsOn(caster))
            {
                caster.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
                return false;
            }
            else if (!caster.CanBeginAction(typeof(PolymorphSpell)))
            {
                caster.SendLocalizedMessage(1061628); // You can't do that while polymorphed.
                return false;
            }
            else if (AnimalForm.UnderTransformation(caster))
            {
                caster.SendLocalizedMessage(1061091); // You cannot cast that spell in this form.
                return false;
            }

            return true;
        }

        public static bool OnCast(Mobile caster, Spell spell)
        {
            ITransformationSpell transformSpell = spell as ITransformationSpell;

            if (transformSpell == null)
                return false;

            if (Factions.Sigil.ExistsOn(caster))
            {
                caster.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
            }
            else if (!caster.CanBeginAction(typeof(PolymorphSpell)))
            {
                caster.SendLocalizedMessage(1061628); // You can't do that while polymorphed.
            }
            else if (DisguiseTimers.IsDisguised(caster))
            {
                caster.SendLocalizedMessage(1061631); // You can't do that while disguised.
                return false;
            }
            else if (AnimalForm.UnderTransformation(caster))
            {
                caster.SendLocalizedMessage(1061091); // You cannot cast that spell in this form.
            }
            else if (!caster.CanBeginAction(typeof(IncognitoSpell)) || (caster.IsBodyMod && GetContext(caster) == null))
            {
                spell.DoFizzle();
            }
            else if (spell.CheckSequence())
            {
                TransformContext context = GetContext(caster);
                Type ourType = spell.GetType();

                bool wasTransformed = (context != null);
                bool ourTransform = (wasTransformed && context.Type == ourType);

                if (wasTransformed)
                {
                    RemoveContext(caster, context, ourTransform);

                    if (ourTransform)
                    {
                        caster.PlaySound(0xFA);
                        caster.FixedParticles(0x3728, 1, 13, 5042, EffectLayer.Waist);
                    }
                }

                if (!ourTransform)
                {
                    List<ResistanceMod> mods = new List<ResistanceMod>();

                    if (transformSpell.PhysResistOffset != 0)
                        mods.Add(new ResistanceMod(ResistanceType.Physical, transformSpell.PhysResistOffset));

                    if (transformSpell.FireResistOffset != 0)
                        mods.Add(new ResistanceMod(ResistanceType.Fire, transformSpell.FireResistOffset));

                    if (transformSpell.ColdResistOffset != 0)
                        mods.Add(new ResistanceMod(ResistanceType.Cold, transformSpell.ColdResistOffset));

                    if (transformSpell.PoisResistOffset != 0)
                        mods.Add(new ResistanceMod(ResistanceType.Poison, transformSpell.PoisResistOffset));

                    if (transformSpell.NrgyResistOffset != 0)
                        mods.Add(new ResistanceMod(ResistanceType.Energy, transformSpell.NrgyResistOffset));

                    if (!((Body)transformSpell.Body).IsHuman)
                    {
                        Mobiles.IMount mt = caster.Mount;

                        if (mt != null)
                            mt.Rider = null;
                    }

                    caster.BodyMod = transformSpell.Body;
                    caster.HueMod = transformSpell.Hue;

                    for (int i = 0; i < mods.Count; ++i)
                        caster.AddResistanceMod(mods[i]);

                    transformSpell.DoEffect(caster);

                    Timer timer = new TransformTimer(caster, transformSpell);
                    timer.Start();

                    AddContext(caster, new TransformContext(timer, mods, ourType, transformSpell));
                    return true;
                }
            }

            return false;
        }
    }

    public interface ITransformationSpell
    {
        int Body { get; }
        int Hue { get; }

        int PhysResistOffset { get; }
        int FireResistOffset { get; }
        int ColdResistOffset { get; }
        int PoisResistOffset { get; }
        int NrgyResistOffset { get; }

        double TickRate { get; }
        void OnTick(Mobile m);

        void DoEffect(Mobile m);
        void RemoveEffect(Mobile m);
    }

    public class TransformContext
    {
        private Timer m_Timer;
        private List<ResistanceMod> m_Mods;
        private Type m_Type;
        private ITransformationSpell m_Spell;

        public Timer Timer { get { return m_Timer; } }
        public List<ResistanceMod> Mods { get { return m_Mods; } }
        public Type Type { get { return m_Type; } }
        public ITransformationSpell Spell { get { return m_Spell; } }

        public TransformContext(Timer timer, List<ResistanceMod> mods, Type type, ITransformationSpell spell)
        {
            m_Timer = timer;
            m_Mods = mods;
            m_Type = type;
            m_Spell = spell;
        }
    }

    public class TransformTimer : Timer
    {
        private Mobile m_Mobile;
        private ITransformationSpell m_Spell;

        public TransformTimer(Mobile from, ITransformationSpell spell)
            : base(TimeSpan.FromSeconds(spell.TickRate), TimeSpan.FromSeconds(spell.TickRate))
        {
            m_Mobile = from;
            m_Spell = spell;

            Priority = TimerPriority.TwoFiftyMS;
        }

        protected override void OnTick()
        {
            if (m_Mobile.Deleted || !m_Mobile.Alive || m_Mobile.Body != m_Spell.Body || m_Mobile.Hue != m_Spell.Hue)
            {
                TransformationSpellHelper.RemoveContext(m_Mobile, true);
                Stop();
            }
            else
            {
                m_Spell.OnTick(m_Mobile);
            }
        }
    }
}