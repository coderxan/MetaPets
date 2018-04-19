using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Server;
using Server.ContextMenus;
using Server.Engines.Craft;
using Server.Engines.Harvest;
using Server.Engines.PartySystem;
using Server.Engines.Quests;
using Server.Engines.Quests.Hag;
using Server.Factions;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Spells;
using Server.Spells.Bushido;
using Server.Spells.Necromancy;
using Server.Spells.Ninjitsu;
using Server.Spells.Spellweaving;
using Server.Targeting;
using Server.Targets;

namespace Server.Items
{
    public enum WeaponQuality
    {
        Low,
        Regular,
        Exceptional
    }

    public enum WeaponType
    {
        Axe,		// Axes, Hatches, etc. These can give concussion blows
        Slashing,	// Katana, Broadsword, Longsword, etc. Slashing weapons are poisonable
        Staff,		// Staves
        Bashing,	// War Hammers, Maces, Mauls, etc. Two-handed bashing delivers crushing blows
        Piercing,	// Spears, Warforks, Daggers, etc. Two-handed piercing delivers paralyzing blows
        Polearm,	// Halberd, Bardiche
        Ranged,		// Bow, Crossbows
        Fists		// Fists
    }

    public enum SlayerName
    {
        None,
        Silver,
        OrcSlaying,
        TrollSlaughter,
        OgreTrashing,
        Repond,
        DragonSlaying,
        Terathan,
        SnakesBane,
        LizardmanSlaughter,
        ReptilianDeath,
        DaemonDismissal,
        GargoylesFoe,
        BalronDamnation,
        Exorcism,
        Ophidian,
        SpidersDeath,
        ScorpionsBane,
        ArachnidDoom,
        FlameDousing,
        WaterDissipation,
        Vacuum,
        ElementalHealth,
        EarthShatter,
        BloodDrinking,
        SummerWind,
        ElementalBan, // Bane?
        Fey
    }

    public enum WeaponDamageLevel
    {
        Regular,
        Ruin,
        Might,
        Force,
        Power,
        Vanq
    }

    public enum WeaponAccuracyLevel
    {
        Regular,
        Accurate,
        Surpassingly,
        Eminently,
        Exceedingly,
        Supremely
    }

    public enum WeaponDurabilityLevel
    {
        Regular,
        Durable,
        Substantial,
        Massive,
        Fortified,
        Indestructible
    }

    public enum WeaponAnimation
    {
        Slash1H = 9,
        Pierce1H = 10,
        Bash1H = 11,
        Bash2H = 12,
        Slash2H = 13,
        Pierce2H = 14,
        ShootBow = 18,
        ShootXBow = 19,
        Wrestle = 31
    }

    public enum CheckSlayerResult
    {
        None,
        Slayer,
        Opposition
    }

    public interface ISlayer
    {
        SlayerName Slayer { get; set; }
        SlayerName Slayer2 { get; set; }
    }

    public abstract class BaseWeapon : Item, IWeapon, IFactionItem, ICraftable, ISlayer, IDurability
    {
        private string m_EngravedText;

        [CommandProperty(AccessLevel.GameMaster)]
        public string EngravedText
        {
            get { return m_EngravedText; }
            set { m_EngravedText = value; InvalidateProperties(); }
        }

        #region Factions
        private FactionItem m_FactionState;

        public FactionItem FactionItemState
        {
            get { return m_FactionState; }
            set
            {
                m_FactionState = value;

                if (m_FactionState == null)
                    Hue = CraftResources.GetHue(Resource);

                LootType = (m_FactionState == null ? LootType.Regular : LootType.Blessed);
            }
        }
        #endregion

        /* Weapon internals work differently now (Mar 13 2003)
		 * 
		 * The attributes defined below default to -1.
		 * If the value is -1, the corresponding virtual 'Aos/Old' property is used.
		 * If not, the attribute value itself is used. Here's the list:
		 *  - MinDamage
		 *  - MaxDamage
		 *  - Speed
		 *  - HitSound
		 *  - MissSound
		 *  - StrRequirement, DexRequirement, IntRequirement
		 *  - WeaponType
		 *  - WeaponAnimation
		 *  - MaxRange
		 */

        #region Var declarations

        // Instance values. These values are unique to each weapon.
        private WeaponDamageLevel m_DamageLevel;
        private WeaponAccuracyLevel m_AccuracyLevel;
        private WeaponDurabilityLevel m_DurabilityLevel;
        private WeaponQuality m_Quality;
        private Mobile m_Crafter;
        private Poison m_Poison;
        private int m_PoisonCharges;
        private bool m_Identified;
        private int m_Hits;
        private int m_MaxHits;
        private SlayerName m_Slayer;
        private SlayerName m_Slayer2;
        private SkillMod m_SkillMod, m_MageMod;
        private CraftResource m_Resource;
        private bool m_PlayerConstructed;

        private bool m_Cursed; // Is this weapon cursed via Curse Weapon necromancer spell? Temporary; not serialized.
        private bool m_Consecrated; // Is this weapon blessed via Consecrate Weapon paladin ability? Temporary; not serialized.

        private AosAttributes m_AosAttributes;
        private AosWeaponAttributes m_AosWeaponAttributes;
        private AosSkillBonuses m_AosSkillBonuses;
        private AosElementAttributes m_AosElementDamages;

        // Overridable values. These values are provided to override the defaults which get defined in the individual weapon scripts.
        private int m_StrReq, m_DexReq, m_IntReq;
        private int m_MinDamage, m_MaxDamage;
        private int m_HitSound, m_MissSound;
        private float m_Speed;
        private int m_MaxRange;
        private SkillName m_Skill;
        private WeaponType m_Type;
        private WeaponAnimation m_Animation;
        #endregion

        #region Virtual Properties
        public virtual WeaponAbility PrimaryAbility { get { return null; } }
        public virtual WeaponAbility SecondaryAbility { get { return null; } }

        public virtual int DefMaxRange { get { return 1; } }
        public virtual int DefHitSound { get { return 0; } }
        public virtual int DefMissSound { get { return 0; } }
        public virtual SkillName DefSkill { get { return SkillName.Swords; } }
        public virtual WeaponType DefType { get { return WeaponType.Slashing; } }
        public virtual WeaponAnimation DefAnimation { get { return WeaponAnimation.Slash1H; } }

        public virtual int AosStrengthReq { get { return 0; } }
        public virtual int AosDexterityReq { get { return 0; } }
        public virtual int AosIntelligenceReq { get { return 0; } }
        public virtual int AosMinDamage { get { return 0; } }
        public virtual int AosMaxDamage { get { return 0; } }
        public virtual int AosSpeed { get { return 0; } }
        public virtual float MlSpeed { get { return 0.0f; } }
        public virtual int AosMaxRange { get { return DefMaxRange; } }
        public virtual int AosHitSound { get { return DefHitSound; } }
        public virtual int AosMissSound { get { return DefMissSound; } }
        public virtual SkillName AosSkill { get { return DefSkill; } }
        public virtual WeaponType AosType { get { return DefType; } }
        public virtual WeaponAnimation AosAnimation { get { return DefAnimation; } }

        public virtual int OldStrengthReq { get { return 0; } }
        public virtual int OldDexterityReq { get { return 0; } }
        public virtual int OldIntelligenceReq { get { return 0; } }
        public virtual int OldMinDamage { get { return 0; } }
        public virtual int OldMaxDamage { get { return 0; } }
        public virtual int OldSpeed { get { return 0; } }
        public virtual int OldMaxRange { get { return DefMaxRange; } }
        public virtual int OldHitSound { get { return DefHitSound; } }
        public virtual int OldMissSound { get { return DefMissSound; } }
        public virtual SkillName OldSkill { get { return DefSkill; } }
        public virtual WeaponType OldType { get { return DefType; } }
        public virtual WeaponAnimation OldAnimation { get { return DefAnimation; } }

        public virtual int InitMinHits { get { return 0; } }
        public virtual int InitMaxHits { get { return 0; } }

        public virtual bool CanFortify { get { return true; } }

        public override int PhysicalResistance { get { return m_AosWeaponAttributes.ResistPhysicalBonus; } }
        public override int FireResistance { get { return m_AosWeaponAttributes.ResistFireBonus; } }
        public override int ColdResistance { get { return m_AosWeaponAttributes.ResistColdBonus; } }
        public override int PoisonResistance { get { return m_AosWeaponAttributes.ResistPoisonBonus; } }
        public override int EnergyResistance { get { return m_AosWeaponAttributes.ResistEnergyBonus; } }

        public virtual SkillName AccuracySkill { get { return SkillName.Tactics; } }
        #endregion

        #region Getters & Setters
        [CommandProperty(AccessLevel.GameMaster)]
        public AosAttributes Attributes
        {
            get { return m_AosAttributes; }
            set { }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public AosWeaponAttributes WeaponAttributes
        {
            get { return m_AosWeaponAttributes; }
            set { }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public AosSkillBonuses SkillBonuses
        {
            get { return m_AosSkillBonuses; }
            set { }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public AosElementAttributes AosElementDamages
        {
            get { return m_AosElementDamages; }
            set { }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Cursed
        {
            get { return m_Cursed; }
            set { m_Cursed = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Consecrated
        {
            get { return m_Consecrated; }
            set { m_Consecrated = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Identified
        {
            get { return m_Identified; }
            set { m_Identified = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitPoints
        {
            get { return m_Hits; }
            set
            {
                if (m_Hits == value)
                    return;

                if (value > m_MaxHits)
                    value = m_MaxHits;

                m_Hits = value;

                InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxHitPoints
        {
            get { return m_MaxHits; }
            set { m_MaxHits = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int PoisonCharges
        {
            get { return m_PoisonCharges; }
            set { m_PoisonCharges = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Poison Poison
        {
            get { return m_Poison; }
            set { m_Poison = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public WeaponQuality Quality
        {
            get { return m_Quality; }
            set { UnscaleDurability(); m_Quality = value; ScaleDurability(); InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Crafter
        {
            get { return m_Crafter; }
            set { m_Crafter = value; InvalidateProperties(); }
        }

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

        [CommandProperty(AccessLevel.GameMaster)]
        public CraftResource Resource
        {
            get { return m_Resource; }
            set { UnscaleDurability(); m_Resource = value; Hue = CraftResources.GetHue(m_Resource); InvalidateProperties(); ScaleDurability(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public WeaponDamageLevel DamageLevel
        {
            get { return m_DamageLevel; }
            set { m_DamageLevel = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public WeaponDurabilityLevel DurabilityLevel
        {
            get { return m_DurabilityLevel; }
            set { UnscaleDurability(); m_DurabilityLevel = value; InvalidateProperties(); ScaleDurability(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool PlayerConstructed
        {
            get { return m_PlayerConstructed; }
            set { m_PlayerConstructed = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxRange
        {
            get { return (m_MaxRange == -1 ? Core.AOS ? AosMaxRange : OldMaxRange : m_MaxRange); }
            set { m_MaxRange = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public WeaponAnimation Animation
        {
            get { return (m_Animation == (WeaponAnimation)(-1) ? Core.AOS ? AosAnimation : OldAnimation : m_Animation); }
            set { m_Animation = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public WeaponType Type
        {
            get { return (m_Type == (WeaponType)(-1) ? Core.AOS ? AosType : OldType : m_Type); }
            set { m_Type = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SkillName Skill
        {
            get { return (m_Skill == (SkillName)(-1) ? Core.AOS ? AosSkill : OldSkill : m_Skill); }
            set { m_Skill = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitSound
        {
            get { return (m_HitSound == -1 ? Core.AOS ? AosHitSound : OldHitSound : m_HitSound); }
            set { m_HitSound = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MissSound
        {
            get { return (m_MissSound == -1 ? Core.AOS ? AosMissSound : OldMissSound : m_MissSound); }
            set { m_MissSound = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MinDamage
        {
            get { return (m_MinDamage == -1 ? Core.AOS ? AosMinDamage : OldMinDamage : m_MinDamage); }
            set { m_MinDamage = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxDamage
        {
            get { return (m_MaxDamage == -1 ? Core.AOS ? AosMaxDamage : OldMaxDamage : m_MaxDamage); }
            set { m_MaxDamage = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public float Speed
        {
            get
            {
                if (m_Speed != -1)
                    return m_Speed;

                if (Core.ML)
                    return MlSpeed;
                else if (Core.AOS)
                    return AosSpeed;

                return OldSpeed;
            }
            set { m_Speed = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int StrRequirement
        {
            get { return (m_StrReq == -1 ? Core.AOS ? AosStrengthReq : OldStrengthReq : m_StrReq); }
            set { m_StrReq = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int DexRequirement
        {
            get { return (m_DexReq == -1 ? Core.AOS ? AosDexterityReq : OldDexterityReq : m_DexReq); }
            set { m_DexReq = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int IntRequirement
        {
            get { return (m_IntReq == -1 ? Core.AOS ? AosIntelligenceReq : OldIntelligenceReq : m_IntReq); }
            set { m_IntReq = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public WeaponAccuracyLevel AccuracyLevel
        {
            get
            {
                return m_AccuracyLevel;
            }
            set
            {
                if (m_AccuracyLevel != value)
                {
                    m_AccuracyLevel = value;

                    if (UseSkillMod)
                    {
                        if (m_AccuracyLevel == WeaponAccuracyLevel.Regular)
                        {
                            if (m_SkillMod != null)
                                m_SkillMod.Remove();

                            m_SkillMod = null;
                        }
                        else if (m_SkillMod == null && Parent is Mobile)
                        {
                            m_SkillMod = new DefaultSkillMod(AccuracySkill, true, (int)m_AccuracyLevel * 5);
                            ((Mobile)Parent).AddSkillMod(m_SkillMod);
                        }
                        else if (m_SkillMod != null)
                        {
                            m_SkillMod.Value = (int)m_AccuracyLevel * 5;
                        }
                    }

                    InvalidateProperties();
                }
            }
        }

        #endregion

        public override void OnAfterDuped(Item newItem)
        {
            BaseWeapon weap = newItem as BaseWeapon;

            if (weap == null)
                return;

            weap.m_AosAttributes = new AosAttributes(newItem, m_AosAttributes);
            weap.m_AosElementDamages = new AosElementAttributes(newItem, m_AosElementDamages);
            weap.m_AosSkillBonuses = new AosSkillBonuses(newItem, m_AosSkillBonuses);
            weap.m_AosWeaponAttributes = new AosWeaponAttributes(newItem, m_AosWeaponAttributes);
        }

        public virtual void UnscaleDurability()
        {
            int scale = 100 + GetDurabilityBonus();

            m_Hits = ((m_Hits * 100) + (scale - 1)) / scale;
            m_MaxHits = ((m_MaxHits * 100) + (scale - 1)) / scale;
            InvalidateProperties();
        }

        public virtual void ScaleDurability()
        {
            int scale = 100 + GetDurabilityBonus();

            m_Hits = ((m_Hits * scale) + 99) / 100;
            m_MaxHits = ((m_MaxHits * scale) + 99) / 100;
            InvalidateProperties();
        }

        public int GetDurabilityBonus()
        {
            int bonus = 0;

            if (m_Quality == WeaponQuality.Exceptional)
                bonus += 20;

            switch (m_DurabilityLevel)
            {
                case WeaponDurabilityLevel.Durable: bonus += 20; break;
                case WeaponDurabilityLevel.Substantial: bonus += 50; break;
                case WeaponDurabilityLevel.Massive: bonus += 70; break;
                case WeaponDurabilityLevel.Fortified: bonus += 100; break;
                case WeaponDurabilityLevel.Indestructible: bonus += 120; break;
            }

            if (Core.AOS)
            {
                bonus += m_AosWeaponAttributes.DurabilityBonus;

                CraftResourceInfo resInfo = CraftResources.GetInfo(m_Resource);
                CraftAttributeInfo attrInfo = null;

                if (resInfo != null)
                    attrInfo = resInfo.AttributeInfo;

                if (attrInfo != null)
                    bonus += attrInfo.WeaponDurability;
            }

            return bonus;
        }

        public int GetLowerStatReq()
        {
            if (!Core.AOS)
                return 0;

            int v = m_AosWeaponAttributes.LowerStatReq;

            CraftResourceInfo info = CraftResources.GetInfo(m_Resource);

            if (info != null)
            {
                CraftAttributeInfo attrInfo = info.AttributeInfo;

                if (attrInfo != null)
                    v += attrInfo.WeaponLowerRequirements;
            }

            if (v > 100)
                v = 100;

            return v;
        }

        public static void BlockEquip(Mobile m, TimeSpan duration)
        {
            if (m.BeginAction(typeof(BaseWeapon)))
                new ResetEquipTimer(m, duration).Start();
        }

        private class ResetEquipTimer : Timer
        {
            private Mobile m_Mobile;

            public ResetEquipTimer(Mobile m, TimeSpan duration)
                : base(duration)
            {
                m_Mobile = m;
            }

            protected override void OnTick()
            {
                m_Mobile.EndAction(typeof(BaseWeapon));
            }
        }

        public override bool CheckConflictingLayer(Mobile m, Item item, Layer layer)
        {
            if (base.CheckConflictingLayer(m, item, layer))
                return true;

            if (this.Layer == Layer.TwoHanded && layer == Layer.OneHanded)
            {
                m.SendLocalizedMessage(500214); // You already have something in both hands.
                return true;
            }
            else if (this.Layer == Layer.OneHanded && layer == Layer.TwoHanded && !(item is BaseShield) && !(item is BaseEquipableLight))
            {
                m.SendLocalizedMessage(500215); // You can only wield one weapon at a time.
                return true;
            }

            return false;
        }

        public override bool AllowSecureTrade(Mobile from, Mobile to, Mobile newOwner, bool accepted)
        {
            if (!Ethics.Ethic.CheckTrade(from, to, newOwner, this))
                return false;

            return base.AllowSecureTrade(from, to, newOwner, accepted);
        }

        public virtual Race RequiredRace { get { return null; } }	//On OSI, there are no weapons with race requirements, this is for custom stuff

        public override bool CanEquip(Mobile from)
        {
            if (!Ethics.Ethic.CheckEquip(from, this))
                return false;

            if (RequiredRace != null && from.Race != RequiredRace)
            {
                if (RequiredRace == Race.Elf)
                    from.SendLocalizedMessage(1072203); // Only Elves may use this.
                else
                    from.SendMessage("Only {0} may use this.", RequiredRace.PluralName);

                return false;
            }
            else if (from.Dex < DexRequirement)
            {
                from.SendMessage("You are not nimble enough to equip that.");
                return false;
            }
            else if (from.Str < AOS.Scale(StrRequirement, 100 - GetLowerStatReq()))
            {
                from.SendLocalizedMessage(500213); // You are not strong enough to equip that.
                return false;
            }
            else if (from.Int < IntRequirement)
            {
                from.SendMessage("You are not smart enough to equip that.");
                return false;
            }
            else if (!from.CanBeginAction(typeof(BaseWeapon)))
            {
                return false;
            }
            else
            {
                return base.CanEquip(from);
            }
        }

        public virtual bool UseSkillMod { get { return !Core.AOS; } }

        public override bool OnEquip(Mobile from)
        {
            int strBonus = m_AosAttributes.BonusStr;
            int dexBonus = m_AosAttributes.BonusDex;
            int intBonus = m_AosAttributes.BonusInt;

            if ((strBonus != 0 || dexBonus != 0 || intBonus != 0))
            {
                Mobile m = from;

                string modName = this.Serial.ToString();

                if (strBonus != 0)
                    m.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));

                if (dexBonus != 0)
                    m.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));

                if (intBonus != 0)
                    m.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
            }

            from.NextCombatTime = Core.TickCount + (int)GetDelay(from).TotalMilliseconds;

            if (UseSkillMod && m_AccuracyLevel != WeaponAccuracyLevel.Regular)
            {
                if (m_SkillMod != null)
                    m_SkillMod.Remove();

                m_SkillMod = new DefaultSkillMod(AccuracySkill, true, (int)m_AccuracyLevel * 5);
                from.AddSkillMod(m_SkillMod);
            }

            if (Core.AOS && m_AosWeaponAttributes.MageWeapon != 0 && m_AosWeaponAttributes.MageWeapon != 30)
            {
                if (m_MageMod != null)
                    m_MageMod.Remove();

                m_MageMod = new DefaultSkillMod(SkillName.Magery, true, -30 + m_AosWeaponAttributes.MageWeapon);
                from.AddSkillMod(m_MageMod);
            }

            return true;
        }

        public override void OnAdded(IEntity parent)
        {
            base.OnAdded(parent);

            if (parent is Mobile)
            {
                Mobile from = (Mobile)parent;

                if (Core.AOS)
                    m_AosSkillBonuses.AddTo(from);

                from.CheckStatTimers();
                from.Delta(MobileDelta.WeaponDamage);
            }
        }

        public override void OnRemoved(IEntity parent)
        {
            if (parent is Mobile)
            {
                Mobile m = (Mobile)parent;
                BaseWeapon weapon = m.Weapon as BaseWeapon;

                string modName = this.Serial.ToString();

                m.RemoveStatMod(modName + "Str");
                m.RemoveStatMod(modName + "Dex");
                m.RemoveStatMod(modName + "Int");

                if (weapon != null)
                    m.NextCombatTime = Core.TickCount + (int)weapon.GetDelay(m).TotalMilliseconds;

                if (UseSkillMod && m_SkillMod != null)
                {
                    m_SkillMod.Remove();
                    m_SkillMod = null;
                }

                if (m_MageMod != null)
                {
                    m_MageMod.Remove();
                    m_MageMod = null;
                }

                if (Core.AOS)
                    m_AosSkillBonuses.Remove();

                ImmolatingWeaponSpell.StopImmolating(this);

                m.CheckStatTimers();

                m.Delta(MobileDelta.WeaponDamage);
            }
        }

        public virtual SkillName GetUsedSkill(Mobile m, bool checkSkillAttrs)
        {
            SkillName sk;

            if (checkSkillAttrs && m_AosWeaponAttributes.UseBestSkill != 0)
            {
                double swrd = m.Skills[SkillName.Swords].Value;
                double fenc = m.Skills[SkillName.Fencing].Value;
                double mcng = m.Skills[SkillName.Macing].Value;
                double val;

                sk = SkillName.Swords;
                val = swrd;

                if (fenc > val) { sk = SkillName.Fencing; val = fenc; }
                if (mcng > val) { sk = SkillName.Macing; val = mcng; }
            }
            else if (m_AosWeaponAttributes.MageWeapon != 0)
            {
                if (m.Skills[SkillName.Magery].Value > m.Skills[Skill].Value)
                    sk = SkillName.Magery;
                else
                    sk = Skill;
            }
            else
            {
                sk = Skill;

                if (sk != SkillName.Wrestling && !m.Player && !m.Body.IsHuman && m.Skills[SkillName.Wrestling].Value > m.Skills[sk].Value)
                    sk = SkillName.Wrestling;
            }

            return sk;
        }

        public virtual double GetAttackSkillValue(Mobile attacker, Mobile defender)
        {
            return attacker.Skills[GetUsedSkill(attacker, true)].Value;
        }

        public virtual double GetDefendSkillValue(Mobile attacker, Mobile defender)
        {
            return defender.Skills[GetUsedSkill(defender, true)].Value;
        }

        private static bool CheckAnimal(Mobile m, Type type)
        {
            return AnimalForm.UnderTransformation(m, type);
        }

        public virtual bool CheckHit(Mobile attacker, Mobile defender)
        {
            BaseWeapon atkWeapon = attacker.Weapon as BaseWeapon;
            BaseWeapon defWeapon = defender.Weapon as BaseWeapon;

            Skill atkSkill = attacker.Skills[atkWeapon.Skill];
            Skill defSkill = defender.Skills[defWeapon.Skill];

            double atkValue = atkWeapon.GetAttackSkillValue(attacker, defender);
            double defValue = defWeapon.GetDefendSkillValue(attacker, defender);

            double ourValue, theirValue;

            int bonus = GetHitChanceBonus();

            if (Core.AOS)
            {
                if (atkValue <= -20.0)
                    atkValue = -19.9;

                if (defValue <= -20.0)
                    defValue = -19.9;

                bonus += AosAttributes.GetValue(attacker, AosAttribute.AttackChance);

                if (Spells.Chivalry.DivineFurySpell.UnderEffect(attacker))
                    bonus += 10; // attacker gets 10% bonus when they're under divine fury

                if (CheckAnimal(attacker, typeof(GreyWolf)) || CheckAnimal(attacker, typeof(BakeKitsune)))
                    bonus += 20; // attacker gets 20% bonus when under Wolf or Bake Kitsune form

                if (HitLower.IsUnderAttackEffect(attacker))
                    bonus -= 25; // Under Hit Lower Attack effect -> 25% malus

                WeaponAbility ability = WeaponAbility.GetCurrentAbility(attacker);

                if (ability != null)
                    bonus += ability.AccuracyBonus;

                SpecialMove move = SpecialMove.GetCurrentMove(attacker);

                if (move != null)
                    bonus += move.GetAccuracyBonus(attacker);

                // Max Hit Chance Increase = 45%
                if (bonus > 45)
                    bonus = 45;

                ourValue = (atkValue + 20.0) * (100 + bonus);

                bonus = AosAttributes.GetValue(defender, AosAttribute.DefendChance);

                if (Spells.Chivalry.DivineFurySpell.UnderEffect(defender))
                    bonus -= 20; // defender loses 20% bonus when they're under divine fury

                if (HitLower.IsUnderDefenseEffect(defender))
                    bonus -= 25; // Under Hit Lower Defense effect -> 25% malus

                int blockBonus = 0;

                if (Block.GetBonus(defender, ref blockBonus))
                    bonus += blockBonus;

                int surpriseMalus = 0;

                if (SurpriseAttack.GetMalus(defender, ref surpriseMalus))
                    bonus -= surpriseMalus;

                int discordanceEffect = 0;

                // Defender loses -0/-28% if under the effect of Discordance.
                if (SkillHandlers.Discordance.GetEffect(attacker, ref discordanceEffect))
                    bonus -= discordanceEffect;

                // Defense Chance Increase = 45%
                if (bonus > 45)
                    bonus = 45;

                theirValue = (defValue + 20.0) * (100 + bonus);

                bonus = 0;
            }
            else
            {
                if (atkValue <= -50.0)
                    atkValue = -49.9;

                if (defValue <= -50.0)
                    defValue = -49.9;

                ourValue = (atkValue + 50.0);
                theirValue = (defValue + 50.0);
            }

            double chance = ourValue / (theirValue * 2.0);

            chance *= 1.0 + ((double)bonus / 100);

            if (Core.AOS && chance < 0.02)
                chance = 0.02;

            return attacker.CheckSkill(atkSkill.SkillName, chance);
        }

        public virtual TimeSpan GetDelay(Mobile m)
        {
            double speed = this.Speed;

            if (speed == 0)
                return TimeSpan.FromHours(1.0);

            double delayInSeconds;

            if (Core.SE)
            {
                /*
                 * This is likely true for Core.AOS as well... both guides report the same
                 * formula, and both are wrong.
                 * The old formula left in for AOS for legacy & because we aren't quite 100%
                 * Sure that AOS has THIS formula
                 */
                int bonus = AosAttributes.GetValue(m, AosAttribute.WeaponSpeed);

                if (Spells.Chivalry.DivineFurySpell.UnderEffect(m))
                    bonus += 10;

                // Bonus granted by successful use of Honorable Execution.
                bonus += HonorableExecution.GetSwingBonus(m);

                if (DualWield.Registry.Contains(m))
                    bonus += ((DualWield.DualWieldTimer)DualWield.Registry[m]).BonusSwingSpeed;

                if (Feint.Registry.Contains(m))
                    bonus -= ((Feint.FeintTimer)Feint.Registry[m]).SwingSpeedReduction;

                TransformContext context = TransformationSpellHelper.GetContext(m);

                if (context != null && context.Spell is ReaperFormSpell)
                    bonus += ((ReaperFormSpell)context.Spell).SwingSpeedBonus;

                int discordanceEffect = 0;

                // Discordance gives a malus of -0/-28% to swing speed.
                if (SkillHandlers.Discordance.GetEffect(m, ref discordanceEffect))
                    bonus -= discordanceEffect;

                if (EssenceOfWindSpell.IsDebuffed(m))
                    bonus -= EssenceOfWindSpell.GetSSIMalus(m);

                if (bonus > 60)
                    bonus = 60;

                double ticks;

                if (Core.ML)
                {
                    int stamTicks = m.Stam / 30;

                    ticks = speed * 4;
                    ticks = Math.Floor((ticks - stamTicks) * (100.0 / (100 + bonus)));
                }
                else
                {
                    speed = Math.Floor(speed * (bonus + 100.0) / 100.0);

                    if (speed <= 0)
                        speed = 1;

                    ticks = Math.Floor((80000.0 / ((m.Stam + 100) * speed)) - 2);
                }

                // Swing speed currently capped at one swing every 1.25 seconds (5 ticks).
                if (ticks < 5)
                    ticks = 5;

                delayInSeconds = ticks * 0.25;
            }
            else if (Core.AOS)
            {
                int v = (m.Stam + 100) * (int)speed;

                int bonus = AosAttributes.GetValue(m, AosAttribute.WeaponSpeed);

                if (Spells.Chivalry.DivineFurySpell.UnderEffect(m))
                    bonus += 10;

                int discordanceEffect = 0;

                // Discordance gives a malus of -0/-28% to swing speed.
                if (SkillHandlers.Discordance.GetEffect(m, ref discordanceEffect))
                    bonus -= discordanceEffect;

                v += AOS.Scale(v, bonus);

                if (v <= 0)
                    v = 1;

                delayInSeconds = Math.Floor(40000.0 / v) * 0.5;

                // Maximum swing rate capped at one swing per second 
                // OSI dev said that it has and is supposed to be 1.25
                if (delayInSeconds < 1.25)
                    delayInSeconds = 1.25;
            }
            else
            {
                int v = (m.Stam + 100) * (int)speed;

                if (v <= 0)
                    v = 1;

                delayInSeconds = 15000.0 / v;
            }

            return TimeSpan.FromSeconds(delayInSeconds);
        }

        public virtual void OnBeforeSwing(Mobile attacker, Mobile defender)
        {
            WeaponAbility a = WeaponAbility.GetCurrentAbility(attacker);

            if (a != null && !a.OnBeforeSwing(attacker, defender))
                WeaponAbility.ClearCurrentAbility(attacker);

            SpecialMove move = SpecialMove.GetCurrentMove(attacker);

            if (move != null && !move.OnBeforeSwing(attacker, defender))
                SpecialMove.ClearCurrentMove(attacker);
        }

        public virtual TimeSpan OnSwing(Mobile attacker, Mobile defender)
        {
            return OnSwing(attacker, defender, 1.0);
        }

        public virtual TimeSpan OnSwing(Mobile attacker, Mobile defender, double damageBonus)
        {
            bool canSwing = true;

            if (Core.AOS)
            {
                canSwing = (!attacker.Paralyzed && !attacker.Frozen);

                if (canSwing)
                {
                    Spell sp = attacker.Spell as Spell;

                    canSwing = (sp == null || !sp.IsCasting || !sp.BlocksMovement);
                }

                if (canSwing)
                {
                    PlayerMobile p = attacker as PlayerMobile;

                    canSwing = (p == null || p.PeacedUntil <= DateTime.UtcNow);
                }
            }

            #region Dueling
            if (attacker is PlayerMobile)
            {
                PlayerMobile pm = (PlayerMobile)attacker;

                if (pm.DuelContext != null && !pm.DuelContext.CheckItemEquip(attacker, this))
                    canSwing = false;
            }
            #endregion

            if (canSwing && attacker.HarmfulCheck(defender))
            {
                attacker.DisruptiveAction();

                if (attacker.NetState != null)
                    attacker.Send(new Swing(0, attacker, defender));

                if (attacker is BaseCreature)
                {
                    BaseCreature bc = (BaseCreature)attacker;
                    WeaponAbility ab = bc.GetWeaponAbility();

                    if (ab != null)
                    {
                        if (bc.WeaponAbilityChance > Utility.RandomDouble())
                            WeaponAbility.SetCurrentAbility(bc, ab);
                        else
                            WeaponAbility.ClearCurrentAbility(bc);
                    }
                }

                if (CheckHit(attacker, defender))
                    OnHit(attacker, defender, damageBonus);
                else
                    OnMiss(attacker, defender);
            }

            return GetDelay(attacker);
        }

        #region Sounds
        public virtual int GetHitAttackSound(Mobile attacker, Mobile defender)
        {
            int sound = attacker.GetAttackSound();

            if (sound == -1)
                sound = HitSound;

            return sound;
        }

        public virtual int GetHitDefendSound(Mobile attacker, Mobile defender)
        {
            return defender.GetHurtSound();
        }

        public virtual int GetMissAttackSound(Mobile attacker, Mobile defender)
        {
            if (attacker.GetAttackSound() == -1)
                return MissSound;
            else
                return -1;
        }

        public virtual int GetMissDefendSound(Mobile attacker, Mobile defender)
        {
            return -1;
        }
        #endregion

        public static bool CheckParry(Mobile defender)
        {
            if (defender == null)
                return false;

            BaseShield shield = defender.FindItemOnLayer(Layer.TwoHanded) as BaseShield;

            double parry = defender.Skills[SkillName.Parry].Value;
            double bushidoNonRacial = defender.Skills[SkillName.Bushido].NonRacialValue;
            double bushido = defender.Skills[SkillName.Bushido].Value;

            if (shield != null)
            {
                double chance = (parry - bushidoNonRacial) / 400.0;	// As per OSI, no negitive effect from the Racial stuffs, ie, 120 parry and '0' bushido with humans

                if (chance < 0) // chance shouldn't go below 0
                    chance = 0;

                // Parry/Bushido over 100 grants a 5% bonus.
                if (parry >= 100.0 || bushido >= 100.0)
                    chance += 0.05;

                // Evasion grants a variable bonus post ML. 50% prior.
                if (Evasion.IsEvading(defender))
                    chance *= Evasion.GetParryScalar(defender);

                // Low dexterity lowers the chance.
                if (defender.Dex < 80)
                    chance = chance * (20 + defender.Dex) / 100;

                return defender.CheckSkill(SkillName.Parry, chance);
            }
            else if (!(defender.Weapon is Fists) && !(defender.Weapon is BaseRanged))
            {
                BaseWeapon weapon = defender.Weapon as BaseWeapon;

                double divisor = (weapon.Layer == Layer.OneHanded) ? 48000.0 : 41140.0;

                double chance = (parry * bushido) / divisor;

                double aosChance = parry / 800.0;

                // Parry or Bushido over 100 grant a 5% bonus.
                if (parry >= 100.0)
                {
                    chance += 0.05;
                    aosChance += 0.05;
                }
                else if (bushido >= 100.0)
                {
                    chance += 0.05;
                }

                // Evasion grants a variable bonus post ML. 50% prior.
                if (Evasion.IsEvading(defender))
                    chance *= Evasion.GetParryScalar(defender);

                // Low dexterity lowers the chance.
                if (defender.Dex < 80)
                    chance = chance * (20 + defender.Dex) / 100;

                if (chance > aosChance)
                    return defender.CheckSkill(SkillName.Parry, chance);
                else
                    return (aosChance > Utility.RandomDouble()); // Only skillcheck if wielding a shield & there's no effect from Bushido
            }

            return false;
        }

        public virtual int AbsorbDamageAOS(Mobile attacker, Mobile defender, int damage)
        {
            bool blocked = false;

            if (defender.Player || defender.Body.IsHuman)
            {
                blocked = CheckParry(defender);

                if (blocked)
                {
                    defender.FixedEffect(0x37B9, 10, 16);
                    damage = 0;

                    // Successful block removes the Honorable Execution penalty.
                    HonorableExecution.RemovePenalty(defender);

                    if (CounterAttack.IsCountering(defender))
                    {
                        BaseWeapon weapon = defender.Weapon as BaseWeapon;

                        if (weapon != null)
                        {
                            defender.FixedParticles(0x3779, 1, 15, 0x158B, 0x0, 0x3, EffectLayer.Waist);
                            weapon.OnSwing(defender, attacker);
                        }

                        CounterAttack.StopCountering(defender);
                    }

                    if (Confidence.IsConfident(defender))
                    {
                        defender.SendLocalizedMessage(1063117); // Your confidence reassures you as you successfully block your opponent's blow.

                        double bushido = defender.Skills.Bushido.Value;

                        defender.Hits += Utility.RandomMinMax(1, (int)(bushido / 12));
                        defender.Stam += Utility.RandomMinMax(1, (int)(bushido / 5));
                    }

                    BaseShield shield = defender.FindItemOnLayer(Layer.TwoHanded) as BaseShield;

                    if (shield != null)
                    {
                        shield.OnHit(this, damage);
                    }
                }
            }

            if (!blocked)
            {
                double positionChance = Utility.RandomDouble();

                Item armorItem;

                if (positionChance < 0.07)
                    armorItem = defender.NeckArmor;
                else if (positionChance < 0.14)
                    armorItem = defender.HandArmor;
                else if (positionChance < 0.28)
                    armorItem = defender.ArmsArmor;
                else if (positionChance < 0.43)
                    armorItem = defender.HeadArmor;
                else if (positionChance < 0.65)
                    armorItem = defender.LegsArmor;
                else
                    armorItem = defender.ChestArmor;

                IWearableDurability armor = armorItem as IWearableDurability;

                if (armor != null)
                    armor.OnHit(this, damage); // call OnHit to lose durability
            }

            return damage;
        }

        public virtual int AbsorbDamage(Mobile attacker, Mobile defender, int damage)
        {
            if (Core.AOS)
                return AbsorbDamageAOS(attacker, defender, damage);

            BaseShield shield = defender.FindItemOnLayer(Layer.TwoHanded) as BaseShield;
            if (shield != null)
                damage = shield.OnHit(this, damage);

            double chance = Utility.RandomDouble();

            Item armorItem;

            if (chance < 0.07)
                armorItem = defender.NeckArmor;
            else if (chance < 0.14)
                armorItem = defender.HandArmor;
            else if (chance < 0.28)
                armorItem = defender.ArmsArmor;
            else if (chance < 0.43)
                armorItem = defender.HeadArmor;
            else if (chance < 0.65)
                armorItem = defender.LegsArmor;
            else
                armorItem = defender.ChestArmor;

            IWearableDurability armor = armorItem as IWearableDurability;

            if (armor != null)
                damage = armor.OnHit(this, damage);

            int virtualArmor = defender.VirtualArmor + defender.VirtualArmorMod;

            if (virtualArmor > 0)
            {
                double scalar;

                if (chance < 0.14)
                    scalar = 0.07;
                else if (chance < 0.28)
                    scalar = 0.14;
                else if (chance < 0.43)
                    scalar = 0.15;
                else if (chance < 0.65)
                    scalar = 0.22;
                else
                    scalar = 0.35;

                int from = (int)(virtualArmor * scalar) / 2;
                int to = (int)(virtualArmor * scalar);

                damage -= Utility.Random(from, (to - from) + 1);
            }

            return damage;
        }

        public virtual int GetPackInstinctBonus(Mobile attacker, Mobile defender)
        {
            if (attacker.Player || defender.Player)
                return 0;

            BaseCreature bc = attacker as BaseCreature;

            if (bc == null || bc.PackInstinct == PackInstinct.None || (!bc.Controlled && !bc.Summoned))
                return 0;

            Mobile master = bc.ControlMaster;

            if (master == null)
                master = bc.SummonMaster;

            if (master == null)
                return 0;

            int inPack = 1;

            IPooledEnumerable eable = defender.GetMobilesInRange(1);
            foreach (Mobile m in eable)
            {
                if (m != attacker && m is BaseCreature)
                {
                    BaseCreature tc = (BaseCreature)m;

                    if ((tc.PackInstinct & bc.PackInstinct) == 0 || (!tc.Controlled && !tc.Summoned))
                        continue;

                    Mobile theirMaster = tc.ControlMaster;

                    if (theirMaster == null)
                        theirMaster = tc.SummonMaster;

                    if (master == theirMaster && tc.Combatant == defender)
                        ++inPack;
                }
            }
            eable.Free();

            if (inPack >= 5)
                return 100;
            else if (inPack >= 4)
                return 75;
            else if (inPack >= 3)
                return 50;
            else if (inPack >= 2)
                return 25;

            return 0;
        }

        private static bool m_InDoubleStrike;

        public static bool InDoubleStrike
        {
            get { return m_InDoubleStrike; }
            set { m_InDoubleStrike = value; }
        }

        public void OnHit(Mobile attacker, Mobile defender)
        {
            OnHit(attacker, defender, 1.0);
        }

        public virtual void OnHit(Mobile attacker, Mobile defender, double damageBonus)
        {
            if (MirrorImage.HasClone(defender) && (defender.Skills.Ninjitsu.Value / 150.0) > Utility.RandomDouble())
            {
                Clone bc;

                IPooledEnumerable eable = defender.GetMobilesInRange(4);
                foreach (Mobile m in eable)
                {
                    bc = m as Clone;

                    if (bc != null && bc.Summoned && bc.SummonMaster == defender)
                    {
                        attacker.SendLocalizedMessage(1063141); // Your attack has been diverted to a nearby mirror image of your target!
                        defender.SendLocalizedMessage(1063140); // You manage to divert the attack onto one of your nearby mirror images.

                        /*
                         * TODO: What happens if the Clone parries a blow?
                         * And what about if the attacker is using Honorable Execution
                         * and kills it?
                         */

                        defender = m;
                        break;
                    }
                }
                eable.Free();
            }

            PlaySwingAnimation(attacker);
            PlayHurtAnimation(defender);

            attacker.PlaySound(GetHitAttackSound(attacker, defender));
            defender.PlaySound(GetHitDefendSound(attacker, defender));

            int damage = ComputeDamage(attacker, defender);

            #region Damage Multipliers
            /*
			 * The following damage bonuses multiply damage by a factor.
			 * Capped at x3 (300%).
			 */
            int percentageBonus = 0;

            WeaponAbility a = WeaponAbility.GetCurrentAbility(attacker);
            SpecialMove move = SpecialMove.GetCurrentMove(attacker);

            if (a != null)
            {
                percentageBonus += (int)(a.DamageScalar * 100) - 100;
            }

            if (move != null)
            {
                percentageBonus += (int)(move.GetDamageScalar(attacker, defender) * 100) - 100;
            }

            percentageBonus += (int)(damageBonus * 100) - 100;

            CheckSlayerResult cs = CheckSlayers(attacker, defender);

            if (cs != CheckSlayerResult.None)
            {
                if (cs == CheckSlayerResult.Slayer)
                    defender.FixedEffect(0x37B9, 10, 5);

                percentageBonus += 100;
            }

            if (!attacker.Player)
            {
                if (defender is PlayerMobile)
                {
                    PlayerMobile pm = (PlayerMobile)defender;

                    if (pm.EnemyOfOneType != null && pm.EnemyOfOneType != attacker.GetType())
                    {
                        percentageBonus += 100;
                    }
                }
            }
            else if (!defender.Player)
            {
                if (attacker is PlayerMobile)
                {
                    PlayerMobile pm = (PlayerMobile)attacker;

                    if (pm.WaitingForEnemy)
                    {
                        pm.EnemyOfOneType = defender.GetType();
                        pm.WaitingForEnemy = false;
                    }

                    if (pm.EnemyOfOneType == defender.GetType())
                    {
                        defender.FixedEffect(0x37B9, 10, 5, 1160, 0);

                        percentageBonus += 50;
                    }
                }
            }

            int packInstinctBonus = GetPackInstinctBonus(attacker, defender);

            if (packInstinctBonus != 0)
            {
                percentageBonus += packInstinctBonus;
            }

            if (m_InDoubleStrike)
            {
                percentageBonus -= 10;
            }

            TransformContext context = TransformationSpellHelper.GetContext(defender);

            if ((m_Slayer == SlayerName.Silver || m_Slayer2 == SlayerName.Silver) && context != null && context.Spell is NecromancerSpell && context.Type != typeof(HorrificBeastSpell))
            {
                // Every necromancer transformation other than horrific beast takes an additional 25% damage
                percentageBonus += 25;
            }

            if (attacker is PlayerMobile && !(Core.ML && defender is PlayerMobile))
            {
                PlayerMobile pmAttacker = (PlayerMobile)attacker;

                if (pmAttacker.HonorActive && pmAttacker.InRange(defender, 1))
                {
                    percentageBonus += 25;
                }

                if (pmAttacker.SentHonorContext != null && pmAttacker.SentHonorContext.Target == defender)
                {
                    percentageBonus += pmAttacker.SentHonorContext.PerfectionDamageBonus;
                }
            }

            BaseTalisman talisman = attacker.Talisman as BaseTalisman;

            if (talisman != null && talisman.Killer != null)
                percentageBonus += talisman.Killer.DamageBonus(defender);

            percentageBonus = Math.Min(percentageBonus, 300);

            damage = AOS.Scale(damage, 100 + percentageBonus);
            #endregion

            if (attacker is BaseCreature)
                ((BaseCreature)attacker).AlterMeleeDamageTo(defender, ref damage);

            if (defender is BaseCreature)
                ((BaseCreature)defender).AlterMeleeDamageFrom(attacker, ref damage);

            damage = AbsorbDamage(attacker, defender, damage);

            if (!Core.AOS && damage < 1)
                damage = 1;
            else if (Core.AOS && damage == 0) // parried
            {
                if (a != null && a.Validate(attacker) /*&& a.CheckMana( attacker, true )*/ ) // Parried special moves have no mana cost 
                {
                    a = null;
                    WeaponAbility.ClearCurrentAbility(attacker);

                    attacker.SendLocalizedMessage(1061140); // Your attack was parried!
                }
            }

            AddBlood(attacker, defender, damage);

            int phys, fire, cold, pois, nrgy, chaos, direct;

            GetDamageTypes(attacker, out phys, out fire, out cold, out pois, out nrgy, out chaos, out direct);

            if (Core.ML && this is BaseRanged)
            {
                BaseQuiver quiver = attacker.FindItemOnLayer(Layer.Cloak) as BaseQuiver;

                if (quiver != null)
                    quiver.AlterBowDamage(ref phys, ref fire, ref cold, ref pois, ref nrgy, ref chaos, ref direct);
            }

            if (m_Consecrated)
            {
                phys = defender.PhysicalResistance;
                fire = defender.FireResistance;
                cold = defender.ColdResistance;
                pois = defender.PoisonResistance;
                nrgy = defender.EnergyResistance;

                int low = phys, type = 0;

                if (fire < low) { low = fire; type = 1; }
                if (cold < low) { low = cold; type = 2; }
                if (pois < low) { low = pois; type = 3; }
                if (nrgy < low) { low = nrgy; type = 4; }

                phys = fire = cold = pois = nrgy = chaos = direct = 0;

                if (type == 0) phys = 100;
                else if (type == 1) fire = 100;
                else if (type == 2) cold = 100;
                else if (type == 3) pois = 100;
                else if (type == 4) nrgy = 100;
            }

            // TODO: Scale damage, alongside the leech effects below, to weapon speed.
            if (ImmolatingWeaponSpell.IsImmolating(this) && damage > 0)
                ImmolatingWeaponSpell.DoEffect(this, defender);

            int damageGiven = damage;

            if (a != null && !a.OnBeforeDamage(attacker, defender))
            {
                WeaponAbility.ClearCurrentAbility(attacker);
                a = null;
            }

            if (move != null && !move.OnBeforeDamage(attacker, defender))
            {
                SpecialMove.ClearCurrentMove(attacker);
                move = null;
            }

            bool ignoreArmor = (a is ArmorIgnore || (move != null && move.IgnoreArmor(attacker)));

            damageGiven = AOS.Damage(defender, attacker, damage, ignoreArmor, phys, fire, cold, pois, nrgy, chaos, direct, false, this is BaseRanged, false);

            double propertyBonus = (move == null) ? 1.0 : move.GetPropertyBonus(attacker);

            if (Core.AOS)
            {
                int lifeLeech = 0;
                int stamLeech = 0;
                int manaLeech = 0;
                int wraithLeech = 0;

                if ((int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitLeechHits) * propertyBonus) > Utility.Random(100))
                    lifeLeech += 30; // HitLeechHits% chance to leech 30% of damage as hit points

                if ((int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitLeechStam) * propertyBonus) > Utility.Random(100))
                    stamLeech += 100; // HitLeechStam% chance to leech 100% of damage as stamina

                if ((int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitLeechMana) * propertyBonus) > Utility.Random(100))
                    manaLeech += 40; // HitLeechMana% chance to leech 40% of damage as mana

                if (m_Cursed)
                    lifeLeech += 50; // Additional 50% life leech for cursed weapons (necro spell)

                context = TransformationSpellHelper.GetContext(attacker);

                if (context != null && context.Type == typeof(VampiricEmbraceSpell))
                    lifeLeech += 20; // Vampiric embrace gives an additional 20% life leech

                if (context != null && context.Type == typeof(WraithFormSpell))
                {
                    wraithLeech = (5 + (int)((15 * attacker.Skills.SpiritSpeak.Value) / 100)); // Wraith form gives an additional 5-20% mana leech

                    // Mana leeched by the Wraith Form spell is actually stolen, not just leeched.
                    defender.Mana -= AOS.Scale(damageGiven, wraithLeech);

                    manaLeech += wraithLeech;
                }

                if (lifeLeech != 0)
                    attacker.Hits += AOS.Scale(damageGiven, lifeLeech);

                if (stamLeech != 0)
                    attacker.Stam += AOS.Scale(damageGiven, stamLeech);

                if (manaLeech != 0)
                    attacker.Mana += AOS.Scale(damageGiven, manaLeech);

                if (lifeLeech != 0 || stamLeech != 0 || manaLeech != 0)
                    attacker.PlaySound(0x44D);
            }

            if (m_MaxHits > 0 && ((MaxRange <= 1 && (defender is Slime || defender is AcidElemental)) || Utility.RandomDouble() < .04)) // Stratics says 50% chance, seems more like 4%..
            {
                if (MaxRange <= 1 && (defender is Slime || defender is AcidElemental))
                    attacker.LocalOverheadMessage(MessageType.Regular, 0x3B2, 500263); // *Acid blood scars your weapon!*

                if (Core.AOS && m_AosWeaponAttributes.SelfRepair > Utility.Random(10))
                {
                    HitPoints += 2;
                }
                else
                {
                    if (m_Hits > 0)
                    {
                        --HitPoints;
                    }
                    else if (m_MaxHits > 1)
                    {
                        --MaxHitPoints;

                        if (Parent is Mobile)
                            ((Mobile)Parent).LocalOverheadMessage(MessageType.Regular, 0x3B2, 1061121); // Your equipment is severely damaged.
                    }
                    else
                    {
                        Delete();
                    }
                }
            }

            if (attacker is VampireBatFamiliar)
            {
                BaseCreature bc = (BaseCreature)attacker;
                Mobile caster = bc.ControlMaster;

                if (caster == null)
                    caster = bc.SummonMaster;

                if (caster != null && caster.Map == bc.Map && caster.InRange(bc, 2))
                    caster.Hits += damage;
                else
                    bc.Hits += damage;
            }

            if (Core.AOS)
            {
                int physChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitPhysicalArea) * propertyBonus);
                int fireChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitFireArea) * propertyBonus);
                int coldChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitColdArea) * propertyBonus);
                int poisChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitPoisonArea) * propertyBonus);
                int nrgyChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitEnergyArea) * propertyBonus);

                if (physChance != 0 && physChance > Utility.Random(100))
                    DoAreaAttack(attacker, defender, 0x10E, 50, 100, 0, 0, 0, 0);

                if (fireChance != 0 && fireChance > Utility.Random(100))
                    DoAreaAttack(attacker, defender, 0x11D, 1160, 0, 100, 0, 0, 0);

                if (coldChance != 0 && coldChance > Utility.Random(100))
                    DoAreaAttack(attacker, defender, 0x0FC, 2100, 0, 0, 100, 0, 0);

                if (poisChance != 0 && poisChance > Utility.Random(100))
                    DoAreaAttack(attacker, defender, 0x205, 1166, 0, 0, 0, 100, 0);

                if (nrgyChance != 0 && nrgyChance > Utility.Random(100))
                    DoAreaAttack(attacker, defender, 0x1F1, 120, 0, 0, 0, 0, 100);

                int maChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitMagicArrow) * propertyBonus);
                int harmChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitHarm) * propertyBonus);
                int fireballChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitFireball) * propertyBonus);
                int lightningChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitLightning) * propertyBonus);
                int dispelChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitDispel) * propertyBonus);

                if (maChance != 0 && maChance > Utility.Random(100))
                    DoMagicArrow(attacker, defender);

                if (harmChance != 0 && harmChance > Utility.Random(100))
                    DoHarm(attacker, defender);

                if (fireballChance != 0 && fireballChance > Utility.Random(100))
                    DoFireball(attacker, defender);

                if (lightningChance != 0 && lightningChance > Utility.Random(100))
                    DoLightning(attacker, defender);

                if (dispelChance != 0 && dispelChance > Utility.Random(100))
                    DoDispel(attacker, defender);

                int laChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitLowerAttack) * propertyBonus);
                int ldChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitLowerDefend) * propertyBonus);

                if (laChance != 0 && laChance > Utility.Random(100))
                    DoLowerAttack(attacker, defender);

                if (ldChance != 0 && ldChance > Utility.Random(100))
                    DoLowerDefense(attacker, defender);
            }

            if (attacker is BaseCreature)
                ((BaseCreature)attacker).OnGaveMeleeAttack(defender);

            if (defender is BaseCreature)
                ((BaseCreature)defender).OnGotMeleeAttack(attacker);

            if (a != null)
                a.OnHit(attacker, defender, damage);

            if (move != null)
                move.OnHit(attacker, defender, damage);

            if (defender is IHonorTarget && ((IHonorTarget)defender).ReceivedHonorContext != null)
                ((IHonorTarget)defender).ReceivedHonorContext.OnTargetHit(attacker);

            if (!(this is BaseRanged))
            {
                if (AnimalForm.UnderTransformation(attacker, typeof(GiantSerpent)))
                    defender.ApplyPoison(attacker, Poison.Lesser);

                if (AnimalForm.UnderTransformation(defender, typeof(BullFrog)))
                    attacker.ApplyPoison(defender, Poison.Regular);
            }
        }

        public virtual double GetAosDamage(Mobile attacker, int bonus, int dice, int sides)
        {
            int damage = Utility.Dice(dice, sides, bonus) * 100;
            int damageBonus = 0;

            // Inscription bonus
            int inscribeSkill = attacker.Skills[SkillName.Inscribe].Fixed;

            damageBonus += inscribeSkill / 200;

            if (inscribeSkill >= 1000)
                damageBonus += 5;

            if (attacker.Player)
            {
                // Int bonus
                damageBonus += (attacker.Int / 10);

                // SDI bonus
                damageBonus += AosAttributes.GetValue(attacker, AosAttribute.SpellDamage);

                TransformContext context = TransformationSpellHelper.GetContext(attacker);

                if (context != null && context.Spell is ReaperFormSpell)
                    damageBonus += ((ReaperFormSpell)context.Spell).SpellDamageBonus;
            }

            damage = AOS.Scale(damage, 100 + damageBonus);

            return damage / 100;
        }

        #region Do<AoSEffect>
        public virtual void DoMagicArrow(Mobile attacker, Mobile defender)
        {
            if (!attacker.CanBeHarmful(defender, false))
                return;

            attacker.DoHarmful(defender);

            double damage = GetAosDamage(attacker, 10, 1, 4);

            attacker.MovingParticles(defender, 0x36E4, 5, 0, false, true, 3006, 4006, 0);
            attacker.PlaySound(0x1E5);

            SpellHelper.Damage(TimeSpan.FromSeconds(1.0), defender, attacker, damage, 0, 100, 0, 0, 0);
        }

        public virtual void DoHarm(Mobile attacker, Mobile defender)
        {
            if (!attacker.CanBeHarmful(defender, false))
                return;

            attacker.DoHarmful(defender);

            double damage = GetAosDamage(attacker, 17, 1, 5);

            if (!defender.InRange(attacker, 2))
                damage *= 0.25; // 1/4 damage at > 2 tile range
            else if (!defender.InRange(attacker, 1))
                damage *= 0.50; // 1/2 damage at 2 tile range

            defender.FixedParticles(0x374A, 10, 30, 5013, 1153, 2, EffectLayer.Waist);
            defender.PlaySound(0x0FC);

            SpellHelper.Damage(TimeSpan.Zero, defender, attacker, damage, 0, 0, 100, 0, 0);
        }

        public virtual void DoFireball(Mobile attacker, Mobile defender)
        {
            if (!attacker.CanBeHarmful(defender, false))
                return;

            attacker.DoHarmful(defender);

            double damage = GetAosDamage(attacker, 19, 1, 5);

            attacker.MovingParticles(defender, 0x36D4, 7, 0, false, true, 9502, 4019, 0x160);
            attacker.PlaySound(0x15E);

            SpellHelper.Damage(TimeSpan.FromSeconds(1.0), defender, attacker, damage, 0, 100, 0, 0, 0);
        }

        public virtual void DoLightning(Mobile attacker, Mobile defender)
        {
            if (!attacker.CanBeHarmful(defender, false))
                return;

            attacker.DoHarmful(defender);

            double damage = GetAosDamage(attacker, 23, 1, 4);

            defender.BoltEffect(0);

            SpellHelper.Damage(TimeSpan.Zero, defender, attacker, damage, 0, 0, 0, 0, 100);
        }

        public virtual void DoDispel(Mobile attacker, Mobile defender)
        {
            bool dispellable = false;

            if (defender is BaseCreature)
                dispellable = ((BaseCreature)defender).Summoned && !((BaseCreature)defender).IsAnimatedDead;

            if (!dispellable)
                return;

            if (!attacker.CanBeHarmful(defender, false))
                return;

            attacker.DoHarmful(defender);

            Spells.MagerySpell sp = new Spells.Sixth.DispelSpell(attacker, null);

            if (sp.CheckResisted(defender))
            {
                defender.FixedEffect(0x3779, 10, 20);
            }
            else
            {
                Effects.SendLocationParticles(EffectItem.Create(defender.Location, defender.Map, EffectItem.DefaultDuration), 0x3728, 8, 20, 5042);
                Effects.PlaySound(defender, defender.Map, 0x201);

                defender.Delete();
            }
        }

        public virtual void DoLowerAttack(Mobile from, Mobile defender)
        {
            if (HitLower.ApplyAttack(defender))
            {
                defender.PlaySound(0x28E);
                Effects.SendTargetEffect(defender, 0x37BE, 1, 4, 0xA, 3);
            }
        }

        public virtual void DoLowerDefense(Mobile from, Mobile defender)
        {
            if (HitLower.ApplyDefense(defender))
            {
                defender.PlaySound(0x28E);
                Effects.SendTargetEffect(defender, 0x37BE, 1, 4, 0x23, 3);
            }
        }

        public virtual void DoAreaAttack(Mobile from, Mobile defender, int sound, int hue, int phys, int fire, int cold, int pois, int nrgy)
        {
            Map map = from.Map;

            if (map == null)
                return;

            List<Mobile> list = new List<Mobile>();

            int range = Core.ML ? 5 : 10;

            IPooledEnumerable eable = from.GetMobilesInRange(range);
            foreach (Mobile m in eable)
            {
                if (from != m && defender != m && SpellHelper.ValidIndirectTarget(from, m) && from.CanBeHarmful(m, false) && (!Core.ML || from.InLOS(m)))
                    list.Add(m);
            }
            eable.Free();

            if (list.Count == 0)
                return;

            Effects.PlaySound(from.Location, map, sound);

            for (int i = 0; i < list.Count; ++i)
            {
                Mobile m = list[i];

                double scalar = Core.ML ? 1.0 : (11 - from.GetDistanceToSqrt(m)) / 10;
                double damage = GetBaseDamage(from);

                if (scalar <= 0)
                {
                    continue;
                }
                else if (scalar < 1.0)
                {
                    damage *= (11 - from.GetDistanceToSqrt(m)) / 10;
                }

                from.DoHarmful(m, true);
                m.FixedEffect(0x3779, 1, 15, hue, 0);
                AOS.Damage(m, from, (int)damage, phys, fire, cold, pois, nrgy);
            }
        }
        #endregion

        public virtual CheckSlayerResult CheckSlayers(Mobile attacker, Mobile defender)
        {
            BaseWeapon atkWeapon = attacker.Weapon as BaseWeapon;
            SlayerEntry atkSlayer = SlayerGroup.GetEntryByName(atkWeapon.Slayer);
            SlayerEntry atkSlayer2 = SlayerGroup.GetEntryByName(atkWeapon.Slayer2);

            if (atkWeapon is ButchersWarCleaver && TalismanSlayer.Slays(TalismanSlayerName.Bovine, defender))
                return CheckSlayerResult.Slayer;

            if (atkSlayer != null && atkSlayer.Slays(defender) || atkSlayer2 != null && atkSlayer2.Slays(defender))
                return CheckSlayerResult.Slayer;

            BaseTalisman talisman = attacker.Talisman as BaseTalisman;

            if (talisman != null && TalismanSlayer.Slays(talisman.Slayer, defender))
                return CheckSlayerResult.Slayer;

            if (!Core.SE)
            {
                ISlayer defISlayer = Spellbook.FindEquippedSpellbook(defender);

                if (defISlayer == null)
                    defISlayer = defender.Weapon as ISlayer;

                if (defISlayer != null)
                {
                    SlayerEntry defSlayer = SlayerGroup.GetEntryByName(defISlayer.Slayer);
                    SlayerEntry defSlayer2 = SlayerGroup.GetEntryByName(defISlayer.Slayer2);

                    if (defSlayer != null && defSlayer.Group.OppositionSuperSlays(attacker) || defSlayer2 != null && defSlayer2.Group.OppositionSuperSlays(attacker))
                        return CheckSlayerResult.Opposition;
                }
            }

            return CheckSlayerResult.None;
        }

        public virtual void AddBlood(Mobile attacker, Mobile defender, int damage)
        {
            if (damage > 0)
            {
                new Blood().MoveToWorld(defender.Location, defender.Map);

                int extraBlood = (Core.SE ? Utility.RandomMinMax(3, 4) : Utility.RandomMinMax(0, 1));

                for (int i = 0; i < extraBlood; i++)
                {
                    new Blood().MoveToWorld(new Point3D(
                        defender.X + Utility.RandomMinMax(-1, 1),
                        defender.Y + Utility.RandomMinMax(-1, 1),
                        defender.Z), defender.Map);
                }
            }
        }

        public virtual void GetDamageTypes(Mobile wielder, out int phys, out int fire, out int cold, out int pois, out int nrgy, out int chaos, out int direct)
        {
            if (wielder is BaseCreature)
            {
                BaseCreature bc = (BaseCreature)wielder;

                phys = bc.PhysicalDamage;
                fire = bc.FireDamage;
                cold = bc.ColdDamage;
                pois = bc.PoisonDamage;
                nrgy = bc.EnergyDamage;
                chaos = bc.ChaosDamage;
                direct = bc.DirectDamage;
            }
            else
            {
                fire = m_AosElementDamages.Fire;
                cold = m_AosElementDamages.Cold;
                pois = m_AosElementDamages.Poison;
                nrgy = m_AosElementDamages.Energy;
                chaos = m_AosElementDamages.Chaos;
                direct = m_AosElementDamages.Direct;

                phys = 100 - fire - cold - pois - nrgy - chaos - direct;

                CraftResourceInfo resInfo = CraftResources.GetInfo(m_Resource);

                if (resInfo != null)
                {
                    CraftAttributeInfo attrInfo = resInfo.AttributeInfo;

                    if (attrInfo != null)
                    {
                        int left = phys;

                        left = ApplyCraftAttributeElementDamage(attrInfo.WeaponColdDamage, ref cold, left);
                        left = ApplyCraftAttributeElementDamage(attrInfo.WeaponEnergyDamage, ref nrgy, left);
                        left = ApplyCraftAttributeElementDamage(attrInfo.WeaponFireDamage, ref fire, left);
                        left = ApplyCraftAttributeElementDamage(attrInfo.WeaponPoisonDamage, ref pois, left);
                        left = ApplyCraftAttributeElementDamage(attrInfo.WeaponChaosDamage, ref chaos, left);
                        left = ApplyCraftAttributeElementDamage(attrInfo.WeaponDirectDamage, ref direct, left);

                        phys = left;
                    }
                }
            }
        }

        private int ApplyCraftAttributeElementDamage(int attrDamage, ref int element, int totalRemaining)
        {
            if (totalRemaining <= 0)
                return 0;

            if (attrDamage <= 0)
                return totalRemaining;

            int appliedDamage = attrDamage;

            if ((appliedDamage + element) > 100)
                appliedDamage = 100 - element;

            if (appliedDamage > totalRemaining)
                appliedDamage = totalRemaining;

            element += appliedDamage;

            return totalRemaining - appliedDamage;
        }

        public virtual void OnMiss(Mobile attacker, Mobile defender)
        {
            PlaySwingAnimation(attacker);
            attacker.PlaySound(GetMissAttackSound(attacker, defender));
            defender.PlaySound(GetMissDefendSound(attacker, defender));

            WeaponAbility ability = WeaponAbility.GetCurrentAbility(attacker);

            if (ability != null)
                ability.OnMiss(attacker, defender);

            SpecialMove move = SpecialMove.GetCurrentMove(attacker);

            if (move != null)
                move.OnMiss(attacker, defender);

            if (defender is IHonorTarget && ((IHonorTarget)defender).ReceivedHonorContext != null)
                ((IHonorTarget)defender).ReceivedHonorContext.OnTargetMissed(attacker);
        }

        public virtual void GetBaseDamageRange(Mobile attacker, out int min, out int max)
        {
            if (attacker is BaseCreature)
            {
                BaseCreature c = (BaseCreature)attacker;

                if (c.DamageMin >= 0)
                {
                    min = c.DamageMin;
                    max = c.DamageMax;
                    return;
                }

                if (this is Fists && !attacker.Body.IsHuman)
                {
                    min = attacker.Str / 28;
                    max = attacker.Str / 28;
                    return;
                }
            }

            min = MinDamage;
            max = MaxDamage;
        }

        public virtual double GetBaseDamage(Mobile attacker)
        {
            int min, max;

            GetBaseDamageRange(attacker, out min, out max);

            int damage = Utility.RandomMinMax(min, max);

            if (Core.AOS) return damage;

            /* Apply damage level offset
             * : Regular : 0
             * : Ruin    : 1
             * : Might   : 3
             * : Force   : 5
             * : Power   : 7
             * : Vanq    : 9
             */
            if (m_DamageLevel != WeaponDamageLevel.Regular)
                damage += (2 * (int)m_DamageLevel) - 1;

            return damage;
        }

        public virtual double GetBonus(double value, double scalar, double threshold, double offset)
        {
            double bonus = value * scalar;

            if (value >= threshold)
                bonus += offset;

            return bonus / 100;
        }

        public virtual int GetHitChanceBonus()
        {
            if (!Core.AOS)
                return 0;

            int bonus = 0;

            switch (m_AccuracyLevel)
            {
                case WeaponAccuracyLevel.Accurate: bonus += 02; break;
                case WeaponAccuracyLevel.Surpassingly: bonus += 04; break;
                case WeaponAccuracyLevel.Eminently: bonus += 06; break;
                case WeaponAccuracyLevel.Exceedingly: bonus += 08; break;
                case WeaponAccuracyLevel.Supremely: bonus += 10; break;
            }

            return bonus;
        }

        public virtual int GetDamageBonus()
        {
            int bonus = VirtualDamageBonus;

            switch (m_Quality)
            {
                case WeaponQuality.Low: bonus -= 20; break;
                case WeaponQuality.Exceptional: bonus += 20; break;
            }

            switch (m_DamageLevel)
            {
                case WeaponDamageLevel.Ruin: bonus += 15; break;
                case WeaponDamageLevel.Might: bonus += 20; break;
                case WeaponDamageLevel.Force: bonus += 25; break;
                case WeaponDamageLevel.Power: bonus += 30; break;
                case WeaponDamageLevel.Vanq: bonus += 35; break;
            }

            return bonus;
        }

        public virtual void GetStatusDamage(Mobile from, out int min, out int max)
        {
            int baseMin, baseMax;

            GetBaseDamageRange(from, out baseMin, out baseMax);

            if (Core.AOS)
            {
                min = Math.Max((int)ScaleDamageAOS(from, baseMin, false), 1);
                max = Math.Max((int)ScaleDamageAOS(from, baseMax, false), 1);
            }
            else
            {
                min = Math.Max((int)ScaleDamageOld(from, baseMin, false), 1);
                max = Math.Max((int)ScaleDamageOld(from, baseMax, false), 1);
            }
        }

        public virtual double ScaleDamageAOS(Mobile attacker, double damage, bool checkSkills)
        {
            if (checkSkills)
            {
                attacker.CheckSkill(SkillName.Tactics, 0.0, attacker.Skills[SkillName.Tactics].Cap); // Passively check tactics for gain
                attacker.CheckSkill(SkillName.Anatomy, 0.0, attacker.Skills[SkillName.Anatomy].Cap); // Passively check Anatomy for gain

                if (Type == WeaponType.Axe)
                    attacker.CheckSkill(SkillName.Lumberjacking, 0.0, 100.0); // Passively check Lumberjacking for gain
            }

            #region Physical bonuses
            /*
			 * These are the bonuses given by the physical characteristics of the mobile.
			 * No caps apply.
			 */
            double strengthBonus = GetBonus(attacker.Str, 0.300, 100.0, 5.00);
            double anatomyBonus = GetBonus(attacker.Skills[SkillName.Anatomy].Value, 0.500, 100.0, 5.00);
            double tacticsBonus = GetBonus(attacker.Skills[SkillName.Tactics].Value, 0.625, 100.0, 6.25);
            double lumberBonus = GetBonus(attacker.Skills[SkillName.Lumberjacking].Value, 0.200, 100.0, 10.00);

            if (Type != WeaponType.Axe)
                lumberBonus = 0.0;
            #endregion

            #region Modifiers
            /*
			 * The following are damage modifiers whose effect shows on the status bar.
			 * Capped at 100% total.
			 */
            int damageBonus = AosAttributes.GetValue(attacker, AosAttribute.WeaponDamage);

            // Horrific Beast transformation gives a +25% bonus to damage.
            if (TransformationSpellHelper.UnderTransformation(attacker, typeof(HorrificBeastSpell)))
                damageBonus += 25;

            // Divine Fury gives a +10% bonus to damage.
            if (Spells.Chivalry.DivineFurySpell.UnderEffect(attacker))
                damageBonus += 10;

            int defenseMasteryMalus = 0;

            // Defense Mastery gives a -50%/-80% malus to damage.
            if (Server.Items.DefenseMastery.GetMalus(attacker, ref defenseMasteryMalus))
                damageBonus -= defenseMasteryMalus;

            int discordanceEffect = 0;

            // Discordance gives a -2%/-48% malus to damage.
            if (SkillHandlers.Discordance.GetEffect(attacker, ref discordanceEffect))
                damageBonus -= discordanceEffect * 2;

            if (damageBonus > 100)
                damageBonus = 100;
            #endregion

            double totalBonus = strengthBonus + anatomyBonus + tacticsBonus + lumberBonus + ((double)(GetDamageBonus() + damageBonus) / 100.0);

            return damage + (int)(damage * totalBonus);
        }

        public virtual int VirtualDamageBonus { get { return 0; } }

        public virtual int ComputeDamageAOS(Mobile attacker, Mobile defender)
        {
            return (int)ScaleDamageAOS(attacker, GetBaseDamage(attacker), true);
        }

        public virtual double ScaleDamageOld(Mobile attacker, double damage, bool checkSkills)
        {
            if (checkSkills)
            {
                attacker.CheckSkill(SkillName.Tactics, 0.0, attacker.Skills[SkillName.Tactics].Cap); // Passively check tactics for gain
                attacker.CheckSkill(SkillName.Anatomy, 0.0, attacker.Skills[SkillName.Anatomy].Cap); // Passively check Anatomy for gain

                if (Type == WeaponType.Axe)
                    attacker.CheckSkill(SkillName.Lumberjacking, 0.0, 100.0); // Passively check Lumberjacking for gain
            }

            /* Compute tactics modifier
             * :   0.0 = 50% loss
             * :  50.0 = unchanged
             * : 100.0 = 50% bonus
             */
            damage += (damage * ((attacker.Skills[SkillName.Tactics].Value - 50.0) / 100.0));


            /* Compute strength modifier
             * : 1% bonus for every 5 strength
             */
            double modifiers = (attacker.Str / 5.0) / 100.0;

            /* Compute anatomy modifier
             * : 1% bonus for every 5 points of anatomy
             * : +10% bonus at Grandmaster or higher
             */
            double anatomyValue = attacker.Skills[SkillName.Anatomy].Value;
            modifiers += ((anatomyValue / 5.0) / 100.0);

            if (anatomyValue >= 100.0)
                modifiers += 0.1;

            /* Compute lumberjacking bonus
             * : 1% bonus for every 5 points of lumberjacking
             * : +10% bonus at Grandmaster or higher
             */
            if (Type == WeaponType.Axe)
            {
                double lumberValue = attacker.Skills[SkillName.Lumberjacking].Value;

                modifiers += ((lumberValue / 5.0) / 100.0);

                if (lumberValue >= 100.0)
                    modifiers += 0.1;
            }

            // New quality bonus:
            if (m_Quality != WeaponQuality.Regular)
                modifiers += (((int)m_Quality - 1) * 0.2);

            // Virtual damage bonus:
            if (VirtualDamageBonus != 0)
                modifiers += (VirtualDamageBonus / 100.0);

            // Apply bonuses
            damage += (damage * modifiers);

            return ScaleDamageByDurability((int)damage);
        }

        public virtual int ScaleDamageByDurability(int damage)
        {
            int scale = 100;

            if (m_MaxHits > 0 && m_Hits < m_MaxHits)
                scale = 50 + ((50 * m_Hits) / m_MaxHits);

            return AOS.Scale(damage, scale);
        }

        public virtual int ComputeDamage(Mobile attacker, Mobile defender)
        {
            if (Core.AOS)
                return ComputeDamageAOS(attacker, defender);

            int damage = (int)ScaleDamageOld(attacker, GetBaseDamage(attacker), true);

            // pre-AOS, halve damage if the defender is a player or the attacker is not a player
            if (defender is PlayerMobile || !(attacker is PlayerMobile))
                damage = (int)(damage / 2.0);

            return damage;
        }

        public virtual void PlayHurtAnimation(Mobile from)
        {
            int action;
            int frames;

            switch (from.Body.Type)
            {
                case BodyType.Sea:
                case BodyType.Animal:
                    {
                        action = 7;
                        frames = 5;
                        break;
                    }
                case BodyType.Monster:
                    {
                        action = 10;
                        frames = 4;
                        break;
                    }
                case BodyType.Human:
                    {
                        action = 20;
                        frames = 5;
                        break;
                    }
                default: return;
            }

            if (from.Mounted)
                return;

            from.Animate(action, frames, 1, true, false, 0);
        }

        public virtual void PlaySwingAnimation(Mobile from)
        {
            int action;

            switch (from.Body.Type)
            {
                case BodyType.Sea:
                case BodyType.Animal:
                    {
                        action = Utility.Random(5, 2);
                        break;
                    }
                case BodyType.Monster:
                    {
                        switch (Animation)
                        {
                            default:
                            case WeaponAnimation.Wrestle:
                            case WeaponAnimation.Bash1H:
                            case WeaponAnimation.Pierce1H:
                            case WeaponAnimation.Slash1H:
                            case WeaponAnimation.Bash2H:
                            case WeaponAnimation.Pierce2H:
                            case WeaponAnimation.Slash2H: action = Utility.Random(4, 3); break;
                            case WeaponAnimation.ShootBow: return; // 7
                            case WeaponAnimation.ShootXBow: return; // 8
                        }

                        break;
                    }
                case BodyType.Human:
                    {
                        if (!from.Mounted)
                        {
                            action = (int)Animation;
                        }
                        else
                        {
                            switch (Animation)
                            {
                                default:
                                case WeaponAnimation.Wrestle:
                                case WeaponAnimation.Bash1H:
                                case WeaponAnimation.Pierce1H:
                                case WeaponAnimation.Slash1H: action = 26; break;
                                case WeaponAnimation.Bash2H:
                                case WeaponAnimation.Pierce2H:
                                case WeaponAnimation.Slash2H: action = 29; break;
                                case WeaponAnimation.ShootBow: action = 27; break;
                                case WeaponAnimation.ShootXBow: action = 28; break;
                            }
                        }

                        break;
                    }
                default: return;
            }

            from.Animate(action, 7, 1, true, false, 0);
        }

        #region Serialization/Deserialization
        private static void SetSaveFlag(ref SaveFlag flags, SaveFlag toSet, bool setIf)
        {
            if (setIf)
                flags |= toSet;
        }

        private static bool GetSaveFlag(SaveFlag flags, SaveFlag toGet)
        {
            return ((flags & toGet) != 0);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)9); // version

            SaveFlag flags = SaveFlag.None;

            SetSaveFlag(ref flags, SaveFlag.DamageLevel, m_DamageLevel != WeaponDamageLevel.Regular);
            SetSaveFlag(ref flags, SaveFlag.AccuracyLevel, m_AccuracyLevel != WeaponAccuracyLevel.Regular);
            SetSaveFlag(ref flags, SaveFlag.DurabilityLevel, m_DurabilityLevel != WeaponDurabilityLevel.Regular);
            SetSaveFlag(ref flags, SaveFlag.Quality, m_Quality != WeaponQuality.Regular);
            SetSaveFlag(ref flags, SaveFlag.Hits, m_Hits != 0);
            SetSaveFlag(ref flags, SaveFlag.MaxHits, m_MaxHits != 0);
            SetSaveFlag(ref flags, SaveFlag.Slayer, m_Slayer != SlayerName.None);
            SetSaveFlag(ref flags, SaveFlag.Poison, m_Poison != null);
            SetSaveFlag(ref flags, SaveFlag.PoisonCharges, m_PoisonCharges != 0);
            SetSaveFlag(ref flags, SaveFlag.Crafter, m_Crafter != null);
            SetSaveFlag(ref flags, SaveFlag.Identified, m_Identified != false);
            SetSaveFlag(ref flags, SaveFlag.StrReq, m_StrReq != -1);
            SetSaveFlag(ref flags, SaveFlag.DexReq, m_DexReq != -1);
            SetSaveFlag(ref flags, SaveFlag.IntReq, m_IntReq != -1);
            SetSaveFlag(ref flags, SaveFlag.MinDamage, m_MinDamage != -1);
            SetSaveFlag(ref flags, SaveFlag.MaxDamage, m_MaxDamage != -1);
            SetSaveFlag(ref flags, SaveFlag.HitSound, m_HitSound != -1);
            SetSaveFlag(ref flags, SaveFlag.MissSound, m_MissSound != -1);
            SetSaveFlag(ref flags, SaveFlag.Speed, m_Speed != -1);
            SetSaveFlag(ref flags, SaveFlag.MaxRange, m_MaxRange != -1);
            SetSaveFlag(ref flags, SaveFlag.Skill, m_Skill != (SkillName)(-1));
            SetSaveFlag(ref flags, SaveFlag.Type, m_Type != (WeaponType)(-1));
            SetSaveFlag(ref flags, SaveFlag.Animation, m_Animation != (WeaponAnimation)(-1));
            SetSaveFlag(ref flags, SaveFlag.Resource, m_Resource != CraftResource.Iron);
            SetSaveFlag(ref flags, SaveFlag.xAttributes, !m_AosAttributes.IsEmpty);
            SetSaveFlag(ref flags, SaveFlag.xWeaponAttributes, !m_AosWeaponAttributes.IsEmpty);
            SetSaveFlag(ref flags, SaveFlag.PlayerConstructed, m_PlayerConstructed);
            SetSaveFlag(ref flags, SaveFlag.SkillBonuses, !m_AosSkillBonuses.IsEmpty);
            SetSaveFlag(ref flags, SaveFlag.Slayer2, m_Slayer2 != SlayerName.None);
            SetSaveFlag(ref flags, SaveFlag.ElementalDamages, !m_AosElementDamages.IsEmpty);
            SetSaveFlag(ref flags, SaveFlag.EngravedText, !String.IsNullOrEmpty(m_EngravedText));

            writer.Write((int)flags);

            if (GetSaveFlag(flags, SaveFlag.DamageLevel))
                writer.Write((int)m_DamageLevel);

            if (GetSaveFlag(flags, SaveFlag.AccuracyLevel))
                writer.Write((int)m_AccuracyLevel);

            if (GetSaveFlag(flags, SaveFlag.DurabilityLevel))
                writer.Write((int)m_DurabilityLevel);

            if (GetSaveFlag(flags, SaveFlag.Quality))
                writer.Write((int)m_Quality);

            if (GetSaveFlag(flags, SaveFlag.Hits))
                writer.Write((int)m_Hits);

            if (GetSaveFlag(flags, SaveFlag.MaxHits))
                writer.Write((int)m_MaxHits);

            if (GetSaveFlag(flags, SaveFlag.Slayer))
                writer.Write((int)m_Slayer);

            if (GetSaveFlag(flags, SaveFlag.Poison))
                Poison.Serialize(m_Poison, writer);

            if (GetSaveFlag(flags, SaveFlag.PoisonCharges))
                writer.Write((int)m_PoisonCharges);

            if (GetSaveFlag(flags, SaveFlag.Crafter))
                writer.Write((Mobile)m_Crafter);

            if (GetSaveFlag(flags, SaveFlag.StrReq))
                writer.Write((int)m_StrReq);

            if (GetSaveFlag(flags, SaveFlag.DexReq))
                writer.Write((int)m_DexReq);

            if (GetSaveFlag(flags, SaveFlag.IntReq))
                writer.Write((int)m_IntReq);

            if (GetSaveFlag(flags, SaveFlag.MinDamage))
                writer.Write((int)m_MinDamage);

            if (GetSaveFlag(flags, SaveFlag.MaxDamage))
                writer.Write((int)m_MaxDamage);

            if (GetSaveFlag(flags, SaveFlag.HitSound))
                writer.Write((int)m_HitSound);

            if (GetSaveFlag(flags, SaveFlag.MissSound))
                writer.Write((int)m_MissSound);

            if (GetSaveFlag(flags, SaveFlag.Speed))
                writer.Write((float)m_Speed);

            if (GetSaveFlag(flags, SaveFlag.MaxRange))
                writer.Write((int)m_MaxRange);

            if (GetSaveFlag(flags, SaveFlag.Skill))
                writer.Write((int)m_Skill);

            if (GetSaveFlag(flags, SaveFlag.Type))
                writer.Write((int)m_Type);

            if (GetSaveFlag(flags, SaveFlag.Animation))
                writer.Write((int)m_Animation);

            if (GetSaveFlag(flags, SaveFlag.Resource))
                writer.Write((int)m_Resource);

            if (GetSaveFlag(flags, SaveFlag.xAttributes))
                m_AosAttributes.Serialize(writer);

            if (GetSaveFlag(flags, SaveFlag.xWeaponAttributes))
                m_AosWeaponAttributes.Serialize(writer);

            if (GetSaveFlag(flags, SaveFlag.SkillBonuses))
                m_AosSkillBonuses.Serialize(writer);

            if (GetSaveFlag(flags, SaveFlag.Slayer2))
                writer.Write((int)m_Slayer2);

            if (GetSaveFlag(flags, SaveFlag.ElementalDamages))
                m_AosElementDamages.Serialize(writer);

            if (GetSaveFlag(flags, SaveFlag.EngravedText))
                writer.Write((string)m_EngravedText);
        }

        [Flags]
        private enum SaveFlag
        {
            None = 0x00000000,
            DamageLevel = 0x00000001,
            AccuracyLevel = 0x00000002,
            DurabilityLevel = 0x00000004,
            Quality = 0x00000008,
            Hits = 0x00000010,
            MaxHits = 0x00000020,
            Slayer = 0x00000040,
            Poison = 0x00000080,
            PoisonCharges = 0x00000100,
            Crafter = 0x00000200,
            Identified = 0x00000400,
            StrReq = 0x00000800,
            DexReq = 0x00001000,
            IntReq = 0x00002000,
            MinDamage = 0x00004000,
            MaxDamage = 0x00008000,
            HitSound = 0x00010000,
            MissSound = 0x00020000,
            Speed = 0x00040000,
            MaxRange = 0x00080000,
            Skill = 0x00100000,
            Type = 0x00200000,
            Animation = 0x00400000,
            Resource = 0x00800000,
            xAttributes = 0x01000000,
            xWeaponAttributes = 0x02000000,
            PlayerConstructed = 0x04000000,
            SkillBonuses = 0x08000000,
            Slayer2 = 0x10000000,
            ElementalDamages = 0x20000000,
            EngravedText = 0x40000000
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 9:
                case 8:
                case 7:
                case 6:
                case 5:
                    {
                        SaveFlag flags = (SaveFlag)reader.ReadInt();

                        if (GetSaveFlag(flags, SaveFlag.DamageLevel))
                        {
                            m_DamageLevel = (WeaponDamageLevel)reader.ReadInt();

                            if (m_DamageLevel > WeaponDamageLevel.Vanq)
                                m_DamageLevel = WeaponDamageLevel.Ruin;
                        }

                        if (GetSaveFlag(flags, SaveFlag.AccuracyLevel))
                        {
                            m_AccuracyLevel = (WeaponAccuracyLevel)reader.ReadInt();

                            if (m_AccuracyLevel > WeaponAccuracyLevel.Supremely)
                                m_AccuracyLevel = WeaponAccuracyLevel.Accurate;
                        }

                        if (GetSaveFlag(flags, SaveFlag.DurabilityLevel))
                        {
                            m_DurabilityLevel = (WeaponDurabilityLevel)reader.ReadInt();

                            if (m_DurabilityLevel > WeaponDurabilityLevel.Indestructible)
                                m_DurabilityLevel = WeaponDurabilityLevel.Durable;
                        }

                        if (GetSaveFlag(flags, SaveFlag.Quality))
                            m_Quality = (WeaponQuality)reader.ReadInt();
                        else
                            m_Quality = WeaponQuality.Regular;

                        if (GetSaveFlag(flags, SaveFlag.Hits))
                            m_Hits = reader.ReadInt();

                        if (GetSaveFlag(flags, SaveFlag.MaxHits))
                            m_MaxHits = reader.ReadInt();

                        if (GetSaveFlag(flags, SaveFlag.Slayer))
                            m_Slayer = (SlayerName)reader.ReadInt();

                        if (GetSaveFlag(flags, SaveFlag.Poison))
                            m_Poison = Poison.Deserialize(reader);

                        if (GetSaveFlag(flags, SaveFlag.PoisonCharges))
                            m_PoisonCharges = reader.ReadInt();

                        if (GetSaveFlag(flags, SaveFlag.Crafter))
                            m_Crafter = reader.ReadMobile();

                        if (GetSaveFlag(flags, SaveFlag.Identified))
                            m_Identified = (version >= 6 || reader.ReadBool());

                        if (GetSaveFlag(flags, SaveFlag.StrReq))
                            m_StrReq = reader.ReadInt();
                        else
                            m_StrReq = -1;

                        if (GetSaveFlag(flags, SaveFlag.DexReq))
                            m_DexReq = reader.ReadInt();
                        else
                            m_DexReq = -1;

                        if (GetSaveFlag(flags, SaveFlag.IntReq))
                            m_IntReq = reader.ReadInt();
                        else
                            m_IntReq = -1;

                        if (GetSaveFlag(flags, SaveFlag.MinDamage))
                            m_MinDamage = reader.ReadInt();
                        else
                            m_MinDamage = -1;

                        if (GetSaveFlag(flags, SaveFlag.MaxDamage))
                            m_MaxDamage = reader.ReadInt();
                        else
                            m_MaxDamage = -1;

                        if (GetSaveFlag(flags, SaveFlag.HitSound))
                            m_HitSound = reader.ReadInt();
                        else
                            m_HitSound = -1;

                        if (GetSaveFlag(flags, SaveFlag.MissSound))
                            m_MissSound = reader.ReadInt();
                        else
                            m_MissSound = -1;

                        if (GetSaveFlag(flags, SaveFlag.Speed))
                        {
                            if (version < 9)
                                m_Speed = reader.ReadInt();
                            else
                                m_Speed = reader.ReadFloat();
                        }
                        else
                            m_Speed = -1;

                        if (GetSaveFlag(flags, SaveFlag.MaxRange))
                            m_MaxRange = reader.ReadInt();
                        else
                            m_MaxRange = -1;

                        if (GetSaveFlag(flags, SaveFlag.Skill))
                            m_Skill = (SkillName)reader.ReadInt();
                        else
                            m_Skill = (SkillName)(-1);

                        if (GetSaveFlag(flags, SaveFlag.Type))
                            m_Type = (WeaponType)reader.ReadInt();
                        else
                            m_Type = (WeaponType)(-1);

                        if (GetSaveFlag(flags, SaveFlag.Animation))
                            m_Animation = (WeaponAnimation)reader.ReadInt();
                        else
                            m_Animation = (WeaponAnimation)(-1);

                        if (GetSaveFlag(flags, SaveFlag.Resource))
                            m_Resource = (CraftResource)reader.ReadInt();
                        else
                            m_Resource = CraftResource.Iron;

                        if (GetSaveFlag(flags, SaveFlag.xAttributes))
                            m_AosAttributes = new AosAttributes(this, reader);
                        else
                            m_AosAttributes = new AosAttributes(this);

                        if (GetSaveFlag(flags, SaveFlag.xWeaponAttributes))
                            m_AosWeaponAttributes = new AosWeaponAttributes(this, reader);
                        else
                            m_AosWeaponAttributes = new AosWeaponAttributes(this);

                        if (UseSkillMod && m_AccuracyLevel != WeaponAccuracyLevel.Regular && Parent is Mobile)
                        {
                            m_SkillMod = new DefaultSkillMod(AccuracySkill, true, (int)m_AccuracyLevel * 5);
                            ((Mobile)Parent).AddSkillMod(m_SkillMod);
                        }

                        if (version < 7 && m_AosWeaponAttributes.MageWeapon != 0)
                            m_AosWeaponAttributes.MageWeapon = 30 - m_AosWeaponAttributes.MageWeapon;

                        if (Core.AOS && m_AosWeaponAttributes.MageWeapon != 0 && m_AosWeaponAttributes.MageWeapon != 30 && Parent is Mobile)
                        {
                            m_MageMod = new DefaultSkillMod(SkillName.Magery, true, -30 + m_AosWeaponAttributes.MageWeapon);
                            ((Mobile)Parent).AddSkillMod(m_MageMod);
                        }

                        if (GetSaveFlag(flags, SaveFlag.PlayerConstructed))
                            m_PlayerConstructed = true;

                        if (GetSaveFlag(flags, SaveFlag.SkillBonuses))
                            m_AosSkillBonuses = new AosSkillBonuses(this, reader);
                        else
                            m_AosSkillBonuses = new AosSkillBonuses(this);

                        if (GetSaveFlag(flags, SaveFlag.Slayer2))
                            m_Slayer2 = (SlayerName)reader.ReadInt();

                        if (GetSaveFlag(flags, SaveFlag.ElementalDamages))
                            m_AosElementDamages = new AosElementAttributes(this, reader);
                        else
                            m_AosElementDamages = new AosElementAttributes(this);

                        if (GetSaveFlag(flags, SaveFlag.EngravedText))
                            m_EngravedText = reader.ReadString();

                        break;
                    }
                case 4:
                    {
                        m_Slayer = (SlayerName)reader.ReadInt();

                        goto case 3;
                    }
                case 3:
                    {
                        m_StrReq = reader.ReadInt();
                        m_DexReq = reader.ReadInt();
                        m_IntReq = reader.ReadInt();

                        goto case 2;
                    }
                case 2:
                    {
                        m_Identified = reader.ReadBool();

                        goto case 1;
                    }
                case 1:
                    {
                        m_MaxRange = reader.ReadInt();

                        goto case 0;
                    }
                case 0:
                    {
                        if (version == 0)
                            m_MaxRange = 1; // default

                        if (version < 5)
                        {
                            m_Resource = CraftResource.Iron;
                            m_AosAttributes = new AosAttributes(this);
                            m_AosWeaponAttributes = new AosWeaponAttributes(this);
                            m_AosElementDamages = new AosElementAttributes(this);
                            m_AosSkillBonuses = new AosSkillBonuses(this);
                        }

                        m_MinDamage = reader.ReadInt();
                        m_MaxDamage = reader.ReadInt();

                        m_Speed = reader.ReadInt();

                        m_HitSound = reader.ReadInt();
                        m_MissSound = reader.ReadInt();

                        m_Skill = (SkillName)reader.ReadInt();
                        m_Type = (WeaponType)reader.ReadInt();
                        m_Animation = (WeaponAnimation)reader.ReadInt();
                        m_DamageLevel = (WeaponDamageLevel)reader.ReadInt();
                        m_AccuracyLevel = (WeaponAccuracyLevel)reader.ReadInt();
                        m_DurabilityLevel = (WeaponDurabilityLevel)reader.ReadInt();
                        m_Quality = (WeaponQuality)reader.ReadInt();

                        m_Crafter = reader.ReadMobile();

                        m_Poison = Poison.Deserialize(reader);
                        m_PoisonCharges = reader.ReadInt();

                        if (m_StrReq == OldStrengthReq)
                            m_StrReq = -1;

                        if (m_DexReq == OldDexterityReq)
                            m_DexReq = -1;

                        if (m_IntReq == OldIntelligenceReq)
                            m_IntReq = -1;

                        if (m_MinDamage == OldMinDamage)
                            m_MinDamage = -1;

                        if (m_MaxDamage == OldMaxDamage)
                            m_MaxDamage = -1;

                        if (m_HitSound == OldHitSound)
                            m_HitSound = -1;

                        if (m_MissSound == OldMissSound)
                            m_MissSound = -1;

                        if (m_Speed == OldSpeed)
                            m_Speed = -1;

                        if (m_MaxRange == OldMaxRange)
                            m_MaxRange = -1;

                        if (m_Skill == OldSkill)
                            m_Skill = (SkillName)(-1);

                        if (m_Type == OldType)
                            m_Type = (WeaponType)(-1);

                        if (m_Animation == OldAnimation)
                            m_Animation = (WeaponAnimation)(-1);

                        if (UseSkillMod && m_AccuracyLevel != WeaponAccuracyLevel.Regular && Parent is Mobile)
                        {
                            m_SkillMod = new DefaultSkillMod(AccuracySkill, true, (int)m_AccuracyLevel * 5);
                            ((Mobile)Parent).AddSkillMod(m_SkillMod);
                        }

                        break;
                    }
            }

            if (Core.AOS && Parent is Mobile)
                m_AosSkillBonuses.AddTo((Mobile)Parent);

            int strBonus = m_AosAttributes.BonusStr;
            int dexBonus = m_AosAttributes.BonusDex;
            int intBonus = m_AosAttributes.BonusInt;

            if (this.Parent is Mobile && (strBonus != 0 || dexBonus != 0 || intBonus != 0))
            {
                Mobile m = (Mobile)this.Parent;

                string modName = this.Serial.ToString();

                if (strBonus != 0)
                    m.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));

                if (dexBonus != 0)
                    m.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));

                if (intBonus != 0)
                    m.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
            }

            if (Parent is Mobile)
                ((Mobile)Parent).CheckStatTimers();

            if (m_Hits <= 0 && m_MaxHits <= 0)
            {
                m_Hits = m_MaxHits = Utility.RandomMinMax(InitMinHits, InitMaxHits);
            }

            if (version < 6)
                m_PlayerConstructed = true; // we don't know, so, assume it's crafted
        }
        #endregion

        public BaseWeapon(int itemID)
            : base(itemID)
        {
            Layer = (Layer)ItemData.Quality;

            m_Quality = WeaponQuality.Regular;
            m_StrReq = -1;
            m_DexReq = -1;
            m_IntReq = -1;
            m_MinDamage = -1;
            m_MaxDamage = -1;
            m_HitSound = -1;
            m_MissSound = -1;
            m_Speed = -1;
            m_MaxRange = -1;
            m_Skill = (SkillName)(-1);
            m_Type = (WeaponType)(-1);
            m_Animation = (WeaponAnimation)(-1);

            m_Hits = m_MaxHits = Utility.RandomMinMax(InitMinHits, InitMaxHits);

            m_Resource = CraftResource.Iron;

            m_AosAttributes = new AosAttributes(this);
            m_AosWeaponAttributes = new AosWeaponAttributes(this);
            m_AosSkillBonuses = new AosSkillBonuses(this);
            m_AosElementDamages = new AosElementAttributes(this);
        }

        public BaseWeapon(Serial serial)
            : base(serial)
        {
        }

        private string GetNameString()
        {
            string name = this.Name;

            if (name == null)
                name = String.Format("#{0}", LabelNumber);

            return name;
        }

        [Hue, CommandProperty(AccessLevel.GameMaster)]
        public override int Hue
        {
            get { return base.Hue; }
            set { base.Hue = value; InvalidateProperties(); }
        }

        public int GetElementalDamageHue()
        {
            int phys, fire, cold, pois, nrgy, chaos, direct;
            GetDamageTypes(null, out phys, out fire, out cold, out pois, out nrgy, out chaos, out direct);
            //Order is Cold, Energy, Fire, Poison, Physical left

            int currentMax = 50;
            int hue = 0;

            if (pois >= currentMax)
            {
                hue = 1267 + (pois - 50) / 10;
                currentMax = pois;
            }

            if (fire >= currentMax)
            {
                hue = 1255 + (fire - 50) / 10;
                currentMax = fire;
            }

            if (nrgy >= currentMax)
            {
                hue = 1273 + (nrgy - 50) / 10;
                currentMax = nrgy;
            }

            if (cold >= currentMax)
            {
                hue = 1261 + (cold - 50) / 10;
                currentMax = cold;
            }

            return hue;
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            int oreType;

            switch (m_Resource)
            {
                case CraftResource.DullCopper: oreType = 1053108; break; // dull copper
                case CraftResource.ShadowIron: oreType = 1053107; break; // shadow iron
                case CraftResource.Copper: oreType = 1053106; break; // copper
                case CraftResource.Bronze: oreType = 1053105; break; // bronze
                case CraftResource.Gold: oreType = 1053104; break; // golden
                case CraftResource.Agapite: oreType = 1053103; break; // agapite
                case CraftResource.Verite: oreType = 1053102; break; // verite
                case CraftResource.Valorite: oreType = 1053101; break; // valorite
                case CraftResource.SpinedLeather: oreType = 1061118; break; // spined
                case CraftResource.HornedLeather: oreType = 1061117; break; // horned
                case CraftResource.BarbedLeather: oreType = 1061116; break; // barbed
                case CraftResource.RedScales: oreType = 1060814; break; // red
                case CraftResource.YellowScales: oreType = 1060818; break; // yellow
                case CraftResource.BlackScales: oreType = 1060820; break; // black
                case CraftResource.GreenScales: oreType = 1060819; break; // green
                case CraftResource.WhiteScales: oreType = 1060821; break; // white
                case CraftResource.BlueScales: oreType = 1060815; break; // blue
                default: oreType = 0; break;
            }

            if (oreType != 0)
                list.Add(1053099, "#{0}\t{1}", oreType, GetNameString()); // ~1_oretype~ ~2_armortype~
            else if (Name == null)
                list.Add(LabelNumber);
            else
                list.Add(Name);

            /*
             * Want to move this to the engraving tool, let the non-harmful 
             * formatting show, and remove CLILOCs embedded: more like OSI
             * did with the books that had markup, etc.
             * 
             * This will have a negative effect on a few event things imgame 
             * as is.
             * 
             * If we cant find a more OSI-ish way to clean it up, we can 
             * easily put this back, and use it in the deserialize
             * method and engraving tool, to make it perm cleaned up.
             */

            if (!String.IsNullOrEmpty(m_EngravedText))
                list.Add(1062613, m_EngravedText);

            /* list.Add( 1062613, Utility.FixHtml( m_EngravedText ) ); */
        }

        public override bool AllowEquipedCast(Mobile from)
        {
            if (base.AllowEquipedCast(from))
                return true;

            return (m_AosAttributes.SpellChanneling != 0);
        }

        public virtual int ArtifactRarity
        {
            get { return 0; }
        }

        public virtual int GetLuckBonus()
        {
            CraftResourceInfo resInfo = CraftResources.GetInfo(m_Resource);

            if (resInfo == null)
                return 0;

            CraftAttributeInfo attrInfo = resInfo.AttributeInfo;

            if (attrInfo == null)
                return 0;

            return attrInfo.WeaponLuck;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (m_Crafter != null)
                list.Add(1050043, m_Crafter.Name); // crafted by ~1_NAME~

            #region Factions
            if (m_FactionState != null)
                list.Add(1041350); // faction item
            #endregion

            if (m_AosSkillBonuses != null)
                m_AosSkillBonuses.GetProperties(list);

            if (m_Quality == WeaponQuality.Exceptional)
                list.Add(1060636); // exceptional

            if (RequiredRace == Race.Elf)
                list.Add(1075086); // Elves Only

            if (ArtifactRarity > 0)
                list.Add(1061078, ArtifactRarity.ToString()); // artifact rarity ~1_val~

            if (this is IUsesRemaining && ((IUsesRemaining)this).ShowUsesRemaining)
                list.Add(1060584, ((IUsesRemaining)this).UsesRemaining.ToString()); // uses remaining: ~1_val~

            if (m_Poison != null && m_PoisonCharges > 0)
                list.Add(1062412 + m_Poison.Level, m_PoisonCharges.ToString());

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


            base.AddResistanceProperties(list);

            int prop;

            if (Core.ML && this is BaseRanged && ((BaseRanged)this).Balanced)
                list.Add(1072792); // Balanced

            if ((prop = m_AosWeaponAttributes.UseBestSkill) != 0)
                list.Add(1060400); // use best weapon skill

            if ((prop = (GetDamageBonus() + m_AosAttributes.WeaponDamage)) != 0)
                list.Add(1060401, prop.ToString()); // damage increase ~1_val~%

            if ((prop = m_AosAttributes.DefendChance) != 0)
                list.Add(1060408, prop.ToString()); // defense chance increase ~1_val~%

            if ((prop = m_AosAttributes.EnhancePotions) != 0)
                list.Add(1060411, prop.ToString()); // enhance potions ~1_val~%

            if ((prop = m_AosAttributes.CastRecovery) != 0)
                list.Add(1060412, prop.ToString()); // faster cast recovery ~1_val~

            if ((prop = m_AosAttributes.CastSpeed) != 0)
                list.Add(1060413, prop.ToString()); // faster casting ~1_val~

            if ((prop = (GetHitChanceBonus() + m_AosAttributes.AttackChance)) != 0)
                list.Add(1060415, prop.ToString()); // hit chance increase ~1_val~%

            if ((prop = m_AosWeaponAttributes.HitColdArea) != 0)
                list.Add(1060416, prop.ToString()); // hit cold area ~1_val~%

            if ((prop = m_AosWeaponAttributes.HitDispel) != 0)
                list.Add(1060417, prop.ToString()); // hit dispel ~1_val~%

            if ((prop = m_AosWeaponAttributes.HitEnergyArea) != 0)
                list.Add(1060418, prop.ToString()); // hit energy area ~1_val~%

            if ((prop = m_AosWeaponAttributes.HitFireArea) != 0)
                list.Add(1060419, prop.ToString()); // hit fire area ~1_val~%

            if ((prop = m_AosWeaponAttributes.HitFireball) != 0)
                list.Add(1060420, prop.ToString()); // hit fireball ~1_val~%

            if ((prop = m_AosWeaponAttributes.HitHarm) != 0)
                list.Add(1060421, prop.ToString()); // hit harm ~1_val~%

            if ((prop = m_AosWeaponAttributes.HitLeechHits) != 0)
                list.Add(1060422, prop.ToString()); // hit life leech ~1_val~%

            if ((prop = m_AosWeaponAttributes.HitLightning) != 0)
                list.Add(1060423, prop.ToString()); // hit lightning ~1_val~%

            if ((prop = m_AosWeaponAttributes.HitLowerAttack) != 0)
                list.Add(1060424, prop.ToString()); // hit lower attack ~1_val~%

            if ((prop = m_AosWeaponAttributes.HitLowerDefend) != 0)
                list.Add(1060425, prop.ToString()); // hit lower defense ~1_val~%

            if ((prop = m_AosWeaponAttributes.HitMagicArrow) != 0)
                list.Add(1060426, prop.ToString()); // hit magic arrow ~1_val~%

            if ((prop = m_AosWeaponAttributes.HitLeechMana) != 0)
                list.Add(1060427, prop.ToString()); // hit mana leech ~1_val~%

            if ((prop = m_AosWeaponAttributes.HitPhysicalArea) != 0)
                list.Add(1060428, prop.ToString()); // hit physical area ~1_val~%

            if ((prop = m_AosWeaponAttributes.HitPoisonArea) != 0)
                list.Add(1060429, prop.ToString()); // hit poison area ~1_val~%

            if ((prop = m_AosWeaponAttributes.HitLeechStam) != 0)
                list.Add(1060430, prop.ToString()); // hit stamina leech ~1_val~%

            if (ImmolatingWeaponSpell.IsImmolating(this))
                list.Add(1111917); // Immolated

            if (Core.ML && this is BaseRanged && (prop = ((BaseRanged)this).Velocity) != 0)
                list.Add(1072793, prop.ToString()); // Velocity ~1_val~%

            if ((prop = m_AosAttributes.BonusDex) != 0)
                list.Add(1060409, prop.ToString()); // dexterity bonus ~1_val~

            if ((prop = m_AosAttributes.BonusHits) != 0)
                list.Add(1060431, prop.ToString()); // hit point increase ~1_val~

            if ((prop = m_AosAttributes.BonusInt) != 0)
                list.Add(1060432, prop.ToString()); // intelligence bonus ~1_val~

            if ((prop = m_AosAttributes.LowerManaCost) != 0)
                list.Add(1060433, prop.ToString()); // lower mana cost ~1_val~%

            if ((prop = m_AosAttributes.LowerRegCost) != 0)
                list.Add(1060434, prop.ToString()); // lower reagent cost ~1_val~%

            if ((prop = GetLowerStatReq()) != 0)
                list.Add(1060435, prop.ToString()); // lower requirements ~1_val~%

            if ((prop = (GetLuckBonus() + m_AosAttributes.Luck)) != 0)
                list.Add(1060436, prop.ToString()); // luck ~1_val~

            if ((prop = m_AosWeaponAttributes.MageWeapon) != 0)
                list.Add(1060438, (30 - prop).ToString()); // mage weapon -~1_val~ skill

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

            if ((prop = m_AosWeaponAttributes.SelfRepair) != 0)
                list.Add(1060450, prop.ToString()); // self repair ~1_val~

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

            int phys, fire, cold, pois, nrgy, chaos, direct;

            GetDamageTypes(null, out phys, out fire, out cold, out pois, out nrgy, out chaos, out direct);

            if (phys != 0)
                list.Add(1060403, phys.ToString()); // physical damage ~1_val~%

            if (fire != 0)
                list.Add(1060405, fire.ToString()); // fire damage ~1_val~%

            if (cold != 0)
                list.Add(1060404, cold.ToString()); // cold damage ~1_val~%

            if (pois != 0)
                list.Add(1060406, pois.ToString()); // poison damage ~1_val~%

            if (nrgy != 0)
                list.Add(1060407, nrgy.ToString()); // energy damage ~1_val

            if (Core.ML && chaos != 0)
                list.Add(1072846, chaos.ToString()); // chaos damage ~1_val~%

            if (Core.ML && direct != 0)
                list.Add(1079978, direct.ToString()); // Direct Damage: ~1_PERCENT~%

            list.Add(1061168, "{0}\t{1}", MinDamage.ToString(), MaxDamage.ToString()); // weapon damage ~1_val~ - ~2_val~

            if (Core.ML)
                list.Add(1061167, String.Format("{0}s", Speed)); // weapon speed ~1_val~
            else
                list.Add(1061167, Speed.ToString());

            if (MaxRange > 1)
                list.Add(1061169, MaxRange.ToString()); // range ~1_val~

            int strReq = AOS.Scale(StrRequirement, 100 - GetLowerStatReq());

            if (strReq > 0)
                list.Add(1061170, strReq.ToString()); // strength requirement ~1_val~

            if (Layer == Layer.TwoHanded)
                list.Add(1061171); // two-handed weapon
            else
                list.Add(1061824); // one-handed weapon

            if (Core.SE || m_AosWeaponAttributes.UseBestSkill == 0)
            {
                switch (Skill)
                {
                    case SkillName.Swords: list.Add(1061172); break; // skill required: swordsmanship
                    case SkillName.Macing: list.Add(1061173); break; // skill required: mace fighting
                    case SkillName.Fencing: list.Add(1061174); break; // skill required: fencing
                    case SkillName.Archery: list.Add(1061175); break; // skill required: archery
                }
            }

            if (m_Hits >= 0 && m_MaxHits > 0)
                list.Add(1060639, "{0}\t{1}", m_Hits, m_MaxHits); // durability ~1_val~ / ~2_val~
        }

        public override void OnSingleClick(Mobile from)
        {
            List<EquipInfoAttribute> attrs = new List<EquipInfoAttribute>();

            if (DisplayLootType)
            {
                if (LootType == LootType.Blessed)
                    attrs.Add(new EquipInfoAttribute(1038021)); // blessed
                else if (LootType == LootType.Cursed)
                    attrs.Add(new EquipInfoAttribute(1049643)); // cursed
            }

            #region Factions
            if (m_FactionState != null)
                attrs.Add(new EquipInfoAttribute(1041350)); // faction item
            #endregion

            if (m_Quality == WeaponQuality.Exceptional)
                attrs.Add(new EquipInfoAttribute(1018305 - (int)m_Quality));

            if (m_Identified || from.AccessLevel >= AccessLevel.GameMaster)
            {
                if (m_Slayer != SlayerName.None)
                {
                    SlayerEntry entry = SlayerGroup.GetEntryByName(m_Slayer);
                    if (entry != null)
                        attrs.Add(new EquipInfoAttribute(entry.Title));
                }

                if (m_Slayer2 != SlayerName.None)
                {
                    SlayerEntry entry = SlayerGroup.GetEntryByName(m_Slayer2);
                    if (entry != null)
                        attrs.Add(new EquipInfoAttribute(entry.Title));
                }

                if (m_DurabilityLevel != WeaponDurabilityLevel.Regular)
                    attrs.Add(new EquipInfoAttribute(1038000 + (int)m_DurabilityLevel));

                if (m_DamageLevel != WeaponDamageLevel.Regular)
                    attrs.Add(new EquipInfoAttribute(1038015 + (int)m_DamageLevel));

                if (m_AccuracyLevel != WeaponAccuracyLevel.Regular)
                    attrs.Add(new EquipInfoAttribute(1038010 + (int)m_AccuracyLevel));
            }
            else if (m_Slayer != SlayerName.None || m_Slayer2 != SlayerName.None || m_DurabilityLevel != WeaponDurabilityLevel.Regular || m_DamageLevel != WeaponDamageLevel.Regular || m_AccuracyLevel != WeaponAccuracyLevel.Regular)
                attrs.Add(new EquipInfoAttribute(1038000)); // Unidentified

            if (m_Poison != null && m_PoisonCharges > 0)
                attrs.Add(new EquipInfoAttribute(1017383, m_PoisonCharges));

            int number;

            if (Name == null)
            {
                number = LabelNumber;
            }
            else
            {
                this.LabelTo(from, Name);
                number = 1041000;
            }

            if (attrs.Count == 0 && Crafter == null && Name != null)
                return;

            EquipmentInfo eqInfo = new EquipmentInfo(number, m_Crafter, false, attrs.ToArray());

            from.Send(new DisplayEquipmentInfo(this, eqInfo));
        }

        private static BaseWeapon m_Fists; // This value holds the default--fist--weapon

        public static BaseWeapon Fists
        {
            get { return m_Fists; }
            set { m_Fists = value; }
        }

        #region ICraftable Members

        public int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool, CraftItem craftItem, int resHue)
        {
            Quality = (WeaponQuality)quality;

            if (makersMark)
                Crafter = from;

            PlayerConstructed = true;

            Type resourceType = typeRes;

            if (resourceType == null)
                resourceType = craftItem.Resources.GetAt(0).ItemType;

            if (Core.AOS)
            {
                Resource = CraftResources.GetFromType(resourceType);

                CraftContext context = craftSystem.GetContext(from);

                if (context != null && context.DoNotColor)
                    Hue = 0;

                if (tool is BaseRunicTool)
                    ((BaseRunicTool)tool).ApplyAttributesTo(this);

                if (Quality == WeaponQuality.Exceptional)
                {
                    if (Attributes.WeaponDamage > 35)
                        Attributes.WeaponDamage -= 20;
                    else
                        Attributes.WeaponDamage = 15;

                    if (Core.ML)
                    {
                        Attributes.WeaponDamage += (int)(from.Skills.ArmsLore.Value / 20);

                        if (Attributes.WeaponDamage > 50)
                            Attributes.WeaponDamage = 50;

                        from.CheckSkill(SkillName.ArmsLore, 0, 100);
                    }
                }
            }
            else if (tool is BaseRunicTool)
            {
                CraftResource thisResource = CraftResources.GetFromType(resourceType);

                if (thisResource == ((BaseRunicTool)tool).Resource)
                {
                    Resource = thisResource;

                    CraftContext context = craftSystem.GetContext(from);

                    if (context != null && context.DoNotColor)
                        Hue = 0;

                    switch (thisResource)
                    {
                        case CraftResource.DullCopper:
                            {
                                Identified = true;
                                DurabilityLevel = WeaponDurabilityLevel.Durable;
                                AccuracyLevel = WeaponAccuracyLevel.Accurate;
                                break;
                            }
                        case CraftResource.ShadowIron:
                            {
                                Identified = true;
                                DurabilityLevel = WeaponDurabilityLevel.Durable;
                                DamageLevel = WeaponDamageLevel.Ruin;
                                break;
                            }
                        case CraftResource.Copper:
                            {
                                Identified = true;
                                DurabilityLevel = WeaponDurabilityLevel.Fortified;
                                DamageLevel = WeaponDamageLevel.Ruin;
                                AccuracyLevel = WeaponAccuracyLevel.Surpassingly;
                                break;
                            }
                        case CraftResource.Bronze:
                            {
                                Identified = true;
                                DurabilityLevel = WeaponDurabilityLevel.Fortified;
                                DamageLevel = WeaponDamageLevel.Might;
                                AccuracyLevel = WeaponAccuracyLevel.Surpassingly;
                                break;
                            }
                        case CraftResource.Gold:
                            {
                                Identified = true;
                                DurabilityLevel = WeaponDurabilityLevel.Indestructible;
                                DamageLevel = WeaponDamageLevel.Force;
                                AccuracyLevel = WeaponAccuracyLevel.Eminently;
                                break;
                            }
                        case CraftResource.Agapite:
                            {
                                Identified = true;
                                DurabilityLevel = WeaponDurabilityLevel.Indestructible;
                                DamageLevel = WeaponDamageLevel.Power;
                                AccuracyLevel = WeaponAccuracyLevel.Eminently;
                                break;
                            }
                        case CraftResource.Verite:
                            {
                                Identified = true;
                                DurabilityLevel = WeaponDurabilityLevel.Indestructible;
                                DamageLevel = WeaponDamageLevel.Power;
                                AccuracyLevel = WeaponAccuracyLevel.Exceedingly;
                                break;
                            }
                        case CraftResource.Valorite:
                            {
                                Identified = true;
                                DurabilityLevel = WeaponDurabilityLevel.Indestructible;
                                DamageLevel = WeaponDamageLevel.Vanq;
                                AccuracyLevel = WeaponAccuracyLevel.Supremely;
                                break;
                            }
                    }
                }
            }

            return quality;
        }

        #endregion
    }

    public abstract class BaseMeleeWeapon : BaseWeapon
    {
        public BaseMeleeWeapon(int itemID)
            : base(itemID)
        {
        }

        public BaseMeleeWeapon(Serial serial)
            : base(serial)
        {
        }

        public override int AbsorbDamage(Mobile attacker, Mobile defender, int damage)
        {
            damage = base.AbsorbDamage(attacker, defender, damage);

            AttuneWeaponSpell.TryAbsorb(defender, ref damage);

            if (Core.AOS)
                return damage;

            int absorb = defender.MeleeDamageAbsorb;

            if (absorb > 0)
            {
                if (absorb > damage)
                {
                    int react = damage / 5;

                    if (react <= 0)
                        react = 1;

                    defender.MeleeDamageAbsorb -= damage;
                    damage = 0;

                    attacker.Damage(react, defender);

                    attacker.PlaySound(0x1F1);
                    attacker.FixedEffect(0x374A, 10, 16);
                }
                else
                {
                    defender.MeleeDamageAbsorb = 0;
                    defender.SendLocalizedMessage(1005556); // Your reactive armor spell has been nullified.
                    DefensiveSpell.Nullify(defender);
                }
            }

            return damage;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
        }
    }

    #region Weapon Abilities

    public abstract class WeaponAbility
    {
        public virtual int BaseMana { get { return 0; } }

        public virtual int AccuracyBonus { get { return 0; } }
        public virtual double DamageScalar { get { return 1.0; } }

        public virtual bool RequiresSE { get { return false; } }

        public virtual void OnHit(Mobile attacker, Mobile defender, int damage)
        {
        }

        public virtual void OnMiss(Mobile attacker, Mobile defender)
        {
        }

        public virtual bool OnBeforeSwing(Mobile attacker, Mobile defender)
        {
            // Here because you must be sure you can use the skill before calling CheckHit if the ability has a HCI bonus for example
            return true;
        }

        public virtual bool OnBeforeDamage(Mobile attacker, Mobile defender)
        {
            return true;
        }

        public virtual bool RequiresTactics(Mobile from)
        {
            return true;
        }

        public virtual double GetRequiredSkill(Mobile from)
        {
            BaseWeapon weapon = from.Weapon as BaseWeapon;

            if (weapon != null && weapon.PrimaryAbility == this)
                return 70.0;
            else if (weapon != null && weapon.SecondaryAbility == this)
                return 90.0;

            return 200.0;
        }

        public virtual int CalculateMana(Mobile from)
        {
            int mana = BaseMana;

            double skillTotal = GetSkill(from, SkillName.Swords) + GetSkill(from, SkillName.Macing)
                + GetSkill(from, SkillName.Fencing) + GetSkill(from, SkillName.Archery) + GetSkill(from, SkillName.Parry)
                + GetSkill(from, SkillName.Lumberjacking) + GetSkill(from, SkillName.Stealth)
                + GetSkill(from, SkillName.Poisoning) + GetSkill(from, SkillName.Bushido) + GetSkill(from, SkillName.Ninjitsu);

            if (skillTotal >= 300.0)
                mana -= 10;
            else if (skillTotal >= 200.0)
                mana -= 5;

            double scalar = 1.0;
            if (!Server.Spells.Necromancy.MindRotSpell.GetMindRotScalar(from, ref scalar))
                scalar = 1.0;

            // Lower Mana Cost = 40%
            int lmc = Math.Min(AosAttributes.GetValue(from, AosAttribute.LowerManaCost), 40);

            scalar -= (double)lmc / 100;
            mana = (int)(mana * scalar);

            // Using a special move within 3 seconds of the previous special move costs double mana 
            if (GetContext(from) != null)
                mana *= 2;

            return mana;
        }

        public virtual bool CheckWeaponSkill(Mobile from)
        {
            BaseWeapon weapon = from.Weapon as BaseWeapon;

            if (weapon == null)
                return false;

            Skill skill = from.Skills[weapon.Skill];
            double reqSkill = GetRequiredSkill(from);
            bool reqTactics = Core.ML && RequiresTactics(from);

            if (Core.ML && reqTactics && from.Skills[SkillName.Tactics].Base < reqSkill)
            {
                from.SendLocalizedMessage(1079308, reqSkill.ToString()); // You need ~1_SKILL_REQUIREMENT~ weapon and tactics skill to perform that attack
                return false;
            }

            if (skill != null && skill.Base >= reqSkill)
                return true;

            /* <UBWS> */
            if (weapon.WeaponAttributes.UseBestSkill > 0 && (from.Skills[SkillName.Swords].Base >= reqSkill || from.Skills[SkillName.Macing].Base >= reqSkill || from.Skills[SkillName.Fencing].Base >= reqSkill))
                return true;
            /* </UBWS> */

            if (reqTactics)
            {
                from.SendLocalizedMessage(1079308, reqSkill.ToString()); // You need ~1_SKILL_REQUIREMENT~ weapon and tactics skill to perform that attack
            }
            else
            {
                from.SendLocalizedMessage(1060182, reqSkill.ToString()); // You need ~1_SKILL_REQUIREMENT~ weapon skill to perform that attack
            }

            return false;
        }

        public virtual bool CheckSkills(Mobile from)
        {
            return CheckWeaponSkill(from);
        }

        public virtual double GetSkill(Mobile from, SkillName skillName)
        {
            Skill skill = from.Skills[skillName];

            if (skill == null)
                return 0.0;

            return skill.Value;
        }

        public virtual bool CheckMana(Mobile from, bool consume)
        {
            int mana = CalculateMana(from);

            if (from.Mana < mana)
            {
                if ((from is BaseCreature) && (from as BaseCreature).HasManaOveride)
                {
                    return true;
                }

                from.SendLocalizedMessage(1060181, mana.ToString()); // You need ~1_MANA_REQUIREMENT~ mana to perform that attack
                return false;
            }

            if (consume)
            {
                if (GetContext(from) == null)
                {
                    Timer timer = new WeaponAbilityTimer(from);
                    timer.Start();

                    AddContext(from, new WeaponAbilityContext(timer));
                }

                from.Mana -= mana;
            }

            return true;
        }

        public virtual bool Validate(Mobile from)
        {
            if (!from.Player)
                return true;

            NetState state = from.NetState;

            if (state == null)
                return false;

            if (RequiresSE && !state.SupportsExpansion(Expansion.SE))
            {
                from.SendLocalizedMessage(1063456); // You must upgrade to Samurai Empire in order to use that ability.
                return false;
            }

            if (Spells.Bushido.HonorableExecution.IsUnderPenalty(from) || Spells.Ninjitsu.AnimalForm.UnderTransformation(from))
            {
                from.SendLocalizedMessage(1063024); // You cannot perform this special move right now.
                return false;
            }

            if (Core.ML && from.Spell != null)
            {
                from.SendLocalizedMessage(1063024); // You cannot perform this special move right now.
                return false;
            }

            #region Dueling
            string option = null;

            if (this is ArmorIgnore)
                option = "Armor Ignore";
            else if (this is BleedAttack)
                option = "Bleed Attack";
            else if (this is ConcussionBlow)
                option = "Concussion Blow";
            else if (this is CrushingBlow)
                option = "Crushing Blow";
            else if (this is Disarm)
                option = "Disarm";
            else if (this is Dismount)
                option = "Dismount";
            else if (this is DoubleStrike)
                option = "Double Strike";
            else if (this is InfectiousStrike)
                option = "Infectious Strike";
            else if (this is MortalStrike)
                option = "Mortal Strike";
            else if (this is MovingShot)
                option = "Moving Shot";
            else if (this is ParalyzingBlow)
                option = "Paralyzing Blow";
            else if (this is ShadowStrike)
                option = "Shadow Strike";
            else if (this is WhirlwindAttack)
                option = "Whirlwind Attack";
            else if (this is RidingSwipe)
                option = "Riding Swipe";
            else if (this is FrenziedWhirlwind)
                option = "Frenzied Whirlwind";
            else if (this is Block)
                option = "Block";
            else if (this is DefenseMastery)
                option = "Defense Mastery";
            else if (this is NerveStrike)
                option = "Nerve Strike";
            else if (this is TalonStrike)
                option = "Talon Strike";
            else if (this is Feint)
                option = "Feint";
            else if (this is DualWield)
                option = "Dual Wield";
            else if (this is DoubleShot)
                option = "Double Shot";
            else if (this is ArmorPierce)
                option = "Armor Pierce";


            if (option != null && !Engines.ConPVP.DuelContext.AllowSpecialAbility(from, option, true))
                return false;
            #endregion

            return CheckSkills(from) && CheckMana(from, false);
        }

        private static WeaponAbility[] m_Abilities = new WeaponAbility[31]
			{
				null,
				new ArmorIgnore(),
				new BleedAttack(),
				new ConcussionBlow(),
				new CrushingBlow(),
				new Disarm(),
				new Dismount(),
				new DoubleStrike(),
				new InfectiousStrike(),
				new MortalStrike(),
				new MovingShot(),
				new ParalyzingBlow(),
				new ShadowStrike(),
				new WhirlwindAttack(),

				new RidingSwipe(),
				new FrenziedWhirlwind(),
				new Block(),
				new DefenseMastery(),
				new NerveStrike(),
				new TalonStrike(),
				new Feint(),
				new DualWield(),
				new DoubleShot(),
				new ArmorPierce(),
				null,
				null,
				null,
				null,
				null,
				null,
				new Disrobe()
			};

        public static WeaponAbility[] Abilities { get { return m_Abilities; } }

        private static Hashtable m_Table = new Hashtable();

        public static Hashtable Table { get { return m_Table; } }

        public static readonly WeaponAbility ArmorIgnore = m_Abilities[1];
        public static readonly WeaponAbility BleedAttack = m_Abilities[2];
        public static readonly WeaponAbility ConcussionBlow = m_Abilities[3];
        public static readonly WeaponAbility CrushingBlow = m_Abilities[4];
        public static readonly WeaponAbility Disarm = m_Abilities[5];
        public static readonly WeaponAbility Dismount = m_Abilities[6];
        public static readonly WeaponAbility DoubleStrike = m_Abilities[7];
        public static readonly WeaponAbility InfectiousStrike = m_Abilities[8];
        public static readonly WeaponAbility MortalStrike = m_Abilities[9];
        public static readonly WeaponAbility MovingShot = m_Abilities[10];
        public static readonly WeaponAbility ParalyzingBlow = m_Abilities[11];
        public static readonly WeaponAbility ShadowStrike = m_Abilities[12];
        public static readonly WeaponAbility WhirlwindAttack = m_Abilities[13];

        public static readonly WeaponAbility RidingSwipe = m_Abilities[14];
        public static readonly WeaponAbility FrenziedWhirlwind = m_Abilities[15];
        public static readonly WeaponAbility Block = m_Abilities[16];
        public static readonly WeaponAbility DefenseMastery = m_Abilities[17];
        public static readonly WeaponAbility NerveStrike = m_Abilities[18];
        public static readonly WeaponAbility TalonStrike = m_Abilities[19];
        public static readonly WeaponAbility Feint = m_Abilities[20];
        public static readonly WeaponAbility DualWield = m_Abilities[21];
        public static readonly WeaponAbility DoubleShot = m_Abilities[22];
        public static readonly WeaponAbility ArmorPierce = m_Abilities[23];

        public static readonly WeaponAbility Bladeweave = m_Abilities[24];
        public static readonly WeaponAbility ForceArrow = m_Abilities[25];
        public static readonly WeaponAbility LightningArrow = m_Abilities[26];
        public static readonly WeaponAbility PsychicAttack = m_Abilities[27];
        public static readonly WeaponAbility SerpentArrow = m_Abilities[28];
        public static readonly WeaponAbility ForceOfNature = m_Abilities[29];

        public static readonly WeaponAbility Disrobe = m_Abilities[30];

        public static bool IsWeaponAbility(Mobile m, WeaponAbility a)
        {
            if (a == null)
                return true;

            if (!m.Player)
                return true;

            BaseWeapon weapon = m.Weapon as BaseWeapon;

            return (weapon != null && (weapon.PrimaryAbility == a || weapon.SecondaryAbility == a));
        }

        public virtual bool ValidatesDuringHit { get { return true; } }

        public static WeaponAbility GetCurrentAbility(Mobile m)
        {
            if (!Core.AOS)
            {
                ClearCurrentAbility(m);
                return null;
            }

            WeaponAbility a = (WeaponAbility)m_Table[m];

            if (!IsWeaponAbility(m, a))
            {
                ClearCurrentAbility(m);
                return null;
            }

            if (a != null && a.ValidatesDuringHit && !a.Validate(m))
            {
                ClearCurrentAbility(m);
                return null;
            }

            return a;
        }

        public static bool SetCurrentAbility(Mobile m, WeaponAbility a)
        {
            if (!Core.AOS)
            {
                ClearCurrentAbility(m);
                return false;
            }

            if (!IsWeaponAbility(m, a))
            {
                ClearCurrentAbility(m);
                return false;
            }

            if (a != null && !a.Validate(m))
            {
                ClearCurrentAbility(m);
                return false;
            }

            if (a == null)
            {
                m_Table.Remove(m);
            }
            else
            {
                SpecialMove.ClearCurrentMove(m);

                m_Table[m] = a;
            }

            return true;
        }

        public static void ClearCurrentAbility(Mobile m)
        {
            m_Table.Remove(m);

            if (Core.AOS && m.NetState != null)
                m.Send(ClearWeaponAbility.Instance);
        }

        public static void Initialize()
        {
            EventSink.SetAbility += new SetAbilityEventHandler(EventSink_SetAbility);
        }

        public WeaponAbility()
        {
        }

        private static void EventSink_SetAbility(SetAbilityEventArgs e)
        {
            int index = e.Index;

            if (index == 0)
                ClearCurrentAbility(e.Mobile);
            else if (index >= 1 && index < m_Abilities.Length)
                SetCurrentAbility(e.Mobile, m_Abilities[index]);
        }


        private static Hashtable m_PlayersTable = new Hashtable();

        private static void AddContext(Mobile m, WeaponAbilityContext context)
        {
            m_PlayersTable[m] = context;
        }

        private static void RemoveContext(Mobile m)
        {
            WeaponAbilityContext context = GetContext(m);

            if (context != null)
                RemoveContext(m, context);
        }

        private static void RemoveContext(Mobile m, WeaponAbilityContext context)
        {
            m_PlayersTable.Remove(m);

            context.Timer.Stop();
        }

        private static WeaponAbilityContext GetContext(Mobile m)
        {
            return (m_PlayersTable[m] as WeaponAbilityContext);
        }

        private class WeaponAbilityTimer : Timer
        {
            private Mobile m_Mobile;

            public WeaponAbilityTimer(Mobile from)
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

        private class WeaponAbilityContext
        {
            private Timer m_Timer;

            public Timer Timer { get { return m_Timer; } }

            public WeaponAbilityContext(Timer timer)
            {
                m_Timer = timer;
            }
        }
    }

    /// <summary>
    /// This special move allows the skilled warrior to bypass his target's physical resistance, for one shot only.
    /// The Armor Ignore shot does slightly less damage than normal.
    /// Against a heavily armored opponent, this ability is a big win, but when used against a very lightly armored foe, it might be better to use a standard strike!
    /// </summary>
    public class ArmorIgnore : WeaponAbility
    {
        public ArmorIgnore()
        {
        }

        public override int BaseMana { get { return 30; } }
        public override double DamageScalar { get { return 0.9; } }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!Validate(attacker) || !CheckMana(attacker, true))
                return;

            ClearCurrentAbility(attacker);

            attacker.SendLocalizedMessage(1060076); // Your attack penetrates their armor!
            defender.SendLocalizedMessage(1060077); // The blow penetrated your armor!

            defender.PlaySound(0x56);
            defender.FixedParticles(0x3728, 200, 25, 9942, EffectLayer.Waist);
        }
    }

    /// <summary>
    /// Strike your opponent with great force, partially bypassing their armor and inflicting greater damage. Requires either Bushido or Ninjitsu skill
    /// </summary>
    public class ArmorPierce : WeaponAbility
    {
        public ArmorPierce()
        {
        }

        public override bool CheckSkills(Mobile from)
        {
            if (GetSkill(from, SkillName.Ninjitsu) < 50.0 && GetSkill(from, SkillName.Bushido) < 50.0)
            {
                from.SendLocalizedMessage(1063347, "50"); // You need ~1_SKILL_REQUIREMENT~ Bushido or Ninjitsu skill to perform that attack!
                return false;
            }

            return base.CheckSkills(from);
        }

        public override int BaseMana { get { return 30; } }
        public override double DamageScalar { get { return 1.5; } }

        public override bool RequiresSE { get { return true; } }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!Validate(attacker) || !CheckMana(attacker, true))
                return;

            ClearCurrentAbility(attacker);

            attacker.SendLocalizedMessage(1063350); // You pierce your opponent's armor!
            defender.SendLocalizedMessage(1063351); // Your attacker pierced your armor!

            defender.FixedParticles(0x3728, 1, 26, 0x26D6, 0, 0, EffectLayer.Waist);
        }
    }

    /// <summary>
    /// Make your opponent bleed profusely with this wicked use of your weapon.
    /// When successful, the target will bleed for several seconds, taking damage as time passes for up to ten seconds.
    /// The rate of damage slows down as time passes, and the blood loss can be completely staunched with the use of bandages. 
    /// </summary>
    public class BleedAttack : WeaponAbility
    {
        public BleedAttack()
        {
        }

        public override int BaseMana { get { return 30; } }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!Validate(attacker) || !CheckMana(attacker, true))
                return;

            ClearCurrentAbility(attacker);

            // Necromancers under Lich or Wraith Form are immune to Bleed Attacks.
            TransformContext context = TransformationSpellHelper.GetContext(defender);

            if ((context != null && (context.Type == typeof(LichFormSpell) || context.Type == typeof(WraithFormSpell))) ||
                (defender is BaseCreature && ((BaseCreature)defender).BleedImmune))
            {
                attacker.SendLocalizedMessage(1062052); // Your target is not affected by the bleed attack!
                return;
            }

            attacker.SendLocalizedMessage(1060159); // Your target is bleeding!
            defender.SendLocalizedMessage(1060160); // You are bleeding!

            if (defender is PlayerMobile)
            {
                defender.LocalOverheadMessage(MessageType.Regular, 0x21, 1060757); // You are bleeding profusely
                defender.NonlocalOverheadMessage(MessageType.Regular, 0x21, 1060758, defender.Name); // ~1_NAME~ is bleeding profusely
            }

            defender.PlaySound(0x133);
            defender.FixedParticles(0x377A, 244, 25, 9950, 31, 0, EffectLayer.Waist);

            BeginBleed(defender, attacker);
        }

        private static Hashtable m_Table = new Hashtable();

        public static bool IsBleeding(Mobile m)
        {
            return m_Table.Contains(m);
        }

        public static void BeginBleed(Mobile m, Mobile from)
        {
            Timer t = (Timer)m_Table[m];

            if (t != null)
                t.Stop();

            t = new InternalTimer(from, m);
            m_Table[m] = t;

            t.Start();
        }

        public static void DoBleed(Mobile m, Mobile from, int level)
        {
            if (m.Alive)
            {
                int damage = Utility.RandomMinMax(level, level * 2);

                if (!m.Player)
                    damage *= 2;

                m.PlaySound(0x133);
                m.Damage(damage, from);

                Blood blood = new Blood();

                blood.ItemID = Utility.Random(0x122A, 5);

                blood.MoveToWorld(m.Location, m.Map);
            }
            else
            {
                EndBleed(m, false);
            }
        }

        public static void EndBleed(Mobile m, bool message)
        {
            Timer t = (Timer)m_Table[m];

            if (t == null)
                return;

            t.Stop();
            m_Table.Remove(m);

            if (message)
                m.SendLocalizedMessage(1060167); // The bleeding wounds have healed, you are no longer bleeding!
        }

        private class InternalTimer : Timer
        {
            private Mobile m_From;
            private Mobile m_Mobile;
            private int m_Count;

            public InternalTimer(Mobile from, Mobile m)
                : base(TimeSpan.FromSeconds(2.0), TimeSpan.FromSeconds(2.0))
            {
                m_From = from;
                m_Mobile = m;
                Priority = TimerPriority.TwoFiftyMS;
            }

            protected override void OnTick()
            {
                DoBleed(m_Mobile, m_From, 5 - m_Count);

                if (++m_Count == 5)
                    EndBleed(m_Mobile, true);
            }
        }
    }

    /// <summary>
    /// Raises your defenses for a short time. Requires Bushido or Ninjitsu skill.
    /// </summary>
    public class Block : WeaponAbility
    {
        public Block()
        {
        }

        public override int BaseMana { get { return 30; } }

        public override bool CheckSkills(Mobile from)
        {
            if (GetSkill(from, SkillName.Ninjitsu) < 50.0 && GetSkill(from, SkillName.Bushido) < 50.0)
            {
                from.SendLocalizedMessage(1063347, "50"); // You need ~1_SKILL_REQUIREMENT~ Bushido or Ninjitsu skill to perform that attack!
                return false;
            }

            return base.CheckSkills(from);
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!Validate(attacker) || !CheckMana(attacker, true))
                return;

            ClearCurrentAbility(attacker);

            attacker.SendLocalizedMessage(1063345); // You block an attack!
            defender.SendLocalizedMessage(1063346); // Your attack was blocked!

            attacker.FixedParticles(0x37C4, 1, 16, 0x251D, 0x39D, 0x3, EffectLayer.RightHand);

            int bonus = (int)(10.0 * ((Math.Max(attacker.Skills[SkillName.Bushido].Value, attacker.Skills[SkillName.Ninjitsu].Value) - 50.0) / 70.0 + 5));

            BeginBlock(attacker, bonus);
        }

        private class BlockInfo
        {
            public Mobile m_Target;
            public Timer m_Timer;
            public int m_Bonus;

            public BlockInfo(Mobile target, int bonus)
            {
                m_Target = target;
                m_Bonus = bonus;
            }
        }

        private static Hashtable m_Table = new Hashtable();

        public static bool GetBonus(Mobile targ, ref int bonus)
        {
            BlockInfo info = m_Table[targ] as BlockInfo;

            if (info == null)
                return false;

            bonus = info.m_Bonus;
            return true;
        }

        public static void BeginBlock(Mobile m, int bonus)
        {
            EndBlock(m);

            BlockInfo info = new BlockInfo(m, bonus);
            info.m_Timer = new InternalTimer(m);

            m_Table[m] = info;
        }

        public static void EndBlock(Mobile m)
        {
            BlockInfo info = m_Table[m] as BlockInfo;

            if (info != null)
            {
                if (info.m_Timer != null)
                    info.m_Timer.Stop();

                m_Table.Remove(m);
            }
        }

        private class InternalTimer : Timer
        {
            private Mobile m_Mobile;

            public InternalTimer(Mobile m)
                : base(TimeSpan.FromSeconds(6.0))
            {
                m_Mobile = m;
                Priority = TimerPriority.TwoFiftyMS;
            }

            protected override void OnTick()
            {
                EndBlock(m_Mobile);
            }
        }
    }

    /// <summary>
    /// This devastating strike is most effective against those who are in good health and whose reserves of mana are low, or vice versa.
    /// </summary>
    public class ConcussionBlow : WeaponAbility
    {
        public ConcussionBlow()
        {
        }

        public override int BaseMana { get { return 25; } }

        public override bool OnBeforeDamage(Mobile attacker, Mobile defender)
        {
            if (!Validate(attacker) || !CheckMana(attacker, true))
                return false;

            ClearCurrentAbility(attacker);

            attacker.SendLocalizedMessage(1060165); // You have delivered a concussion!
            defender.SendLocalizedMessage(1060166); // You feel disoriented!

            defender.PlaySound(0x213);
            defender.FixedParticles(0x377A, 1, 32, 9949, 1153, 0, EffectLayer.Head);

            Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(defender.X, defender.Y, defender.Z + 10), defender.Map), new Entity(Serial.Zero, new Point3D(defender.X, defender.Y, defender.Z + 20), defender.Map), 0x36FE, 1, 0, false, false, 1133, 3, 9501, 1, 0, EffectLayer.Waist, 0x100);

            int damage = 10; // Base damage is 10.

            if (defender.HitsMax > 0)
            {
                double hitsPercent = ((double)defender.Hits / (double)defender.HitsMax) * 100.0;

                double manaPercent = 0;

                if (defender.ManaMax > 0)
                    manaPercent = ((double)defender.Mana / (double)defender.ManaMax) * 100.0;

                damage += Math.Min((int)(Math.Abs(hitsPercent - manaPercent) / 4), 20);
            }

            // Total damage is 10 + (0~20) = 10~30, physical, non-resistable.

            defender.Damage(damage, attacker);

            return true;
        }
    }

    /// <summary>
    /// Also known as the Haymaker, this attack dramatically increases the damage done by a weapon reaching its mark.
    /// </summary>
    public class CrushingBlow : WeaponAbility
    {
        public CrushingBlow()
        {
        }

        public override int BaseMana { get { return 25; } }
        public override double DamageScalar { get { return 1.5; } }


        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!Validate(attacker) || !CheckMana(attacker, true))
                return;

            ClearCurrentAbility(attacker);

            attacker.SendLocalizedMessage(1060090); // You have delivered a crushing blow!
            defender.SendLocalizedMessage(1060091); // You take extra damage from the crushing attack!

            defender.PlaySound(0x1E1);
            defender.FixedParticles(0, 1, 0, 9946, EffectLayer.Head);

            Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(defender.X, defender.Y, defender.Z + 50), defender.Map), new Entity(Serial.Zero, new Point3D(defender.X, defender.Y, defender.Z + 20), defender.Map), 0xFB4, 1, 0, false, false, 0, 3, 9501, 1, 0, EffectLayer.Head, 0x100);
        }
    }

    /// <summary>
    /// Raises your physical resistance for a short time while lowering your ability to inflict damage. Requires Bushido or Ninjitsu skill.
    /// </summary>
    public class DefenseMastery : WeaponAbility
    {
        public DefenseMastery()
        {
        }

        public override bool CheckSkills(Mobile from)
        {
            if (GetSkill(from, SkillName.Ninjitsu) < 50.0 && GetSkill(from, SkillName.Bushido) < 50.0)
            {
                from.SendLocalizedMessage(1063347, "50"); // You need ~1_SKILL_REQUIREMENT~ Bushido or Ninjitsu skill to perform that attack!
                return false;
            }

            return base.CheckSkills(from);
        }

        public override int BaseMana { get { return 30; } }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!Validate(attacker) || !CheckMana(attacker, true))
                return;

            ClearCurrentAbility(attacker);

            attacker.SendLocalizedMessage(1063353); // You perform a masterful defense!

            attacker.FixedParticles(0x375A, 1, 17, 0x7F2, 0x3E8, 0x3, EffectLayer.Waist);

            int modifier = (int)(30.0 * ((Math.Max(attacker.Skills[SkillName.Bushido].Value, attacker.Skills[SkillName.Ninjitsu].Value) - 50.0) / 70.0));

            DefenseMasteryInfo info = m_Table[attacker] as DefenseMasteryInfo;

            if (info != null)
                EndDefense((object)info);

            ResistanceMod mod = new ResistanceMod(ResistanceType.Physical, 50 + modifier);
            attacker.AddResistanceMod(mod);

            info = new DefenseMasteryInfo(attacker, 80 - modifier, mod);
            info.m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(3.0), new TimerStateCallback(EndDefense), info);

            m_Table[attacker] = info;

            attacker.Delta(MobileDelta.WeaponDamage);
        }

        private class DefenseMasteryInfo
        {
            public Mobile m_From;
            public Timer m_Timer;
            public int m_DamageMalus;
            public ResistanceMod m_Mod;

            public DefenseMasteryInfo(Mobile from, int damageMalus, ResistanceMod mod)
            {
                m_From = from;
                m_DamageMalus = damageMalus;
                m_Mod = mod;
            }
        }

        private static Hashtable m_Table = new Hashtable();

        public static bool GetMalus(Mobile targ, ref int damageMalus)
        {
            DefenseMasteryInfo info = m_Table[targ] as DefenseMasteryInfo;

            if (info == null)
                return false;

            damageMalus = info.m_DamageMalus;
            return true;
        }

        private static void EndDefense(object state)
        {
            DefenseMasteryInfo info = (DefenseMasteryInfo)state;

            if (info.m_Mod != null)
                info.m_From.RemoveResistanceMod(info.m_Mod);

            if (info.m_Timer != null)
                info.m_Timer.Stop();

            // No message is sent to the player.

            m_Table.Remove(info.m_From);

            info.m_From.Delta(MobileDelta.WeaponDamage);
        }
    }

    /// <summary>
    /// This attack allows you to disarm your foe.
    /// Now in Age of Shadows, a successful Disarm leaves the victim unable to re-arm another weapon for several seconds.
    /// </summary>
    public class Disarm : WeaponAbility
    {
        public Disarm()
        {
        }

        public override int BaseMana { get { return 20; } }

        // No longer active in pub21:
        /*public override bool CheckSkills( Mobile from )
        {
            if ( !base.CheckSkills( from ) )
                return false;

            if ( !(from.Weapon is Fists) )
                return true;

            Skill skill = from.Skills[SkillName.ArmsLore];

            if ( skill != null && skill.Base >= 80.0 )
                return true;

            from.SendLocalizedMessage( 1061812 ); // You lack the required skill in armslore to perform that attack!

            return false;
        }*/

        public override bool RequiresTactics(Mobile from)
        {
            BaseWeapon weapon = from.Weapon as BaseWeapon;

            if (weapon == null)
                return false;

            return weapon.Skill != SkillName.Wrestling;
        }

        public static readonly TimeSpan BlockEquipDuration = TimeSpan.FromSeconds(5.0);

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!Validate(attacker))
                return;

            ClearCurrentAbility(attacker);

            Item toDisarm = defender.FindItemOnLayer(Layer.OneHanded);

            if (toDisarm == null || !toDisarm.Movable)
                toDisarm = defender.FindItemOnLayer(Layer.TwoHanded);

            Container pack = defender.Backpack;

            if (pack == null || (toDisarm != null && !toDisarm.Movable))
            {
                attacker.SendLocalizedMessage(1004001); // You cannot disarm your opponent.
            }
            else if (toDisarm == null || toDisarm is BaseShield || toDisarm is Spellbook && !Core.ML)
            {
                attacker.SendLocalizedMessage(1060849); // Your target is already unarmed!
            }
            else if (CheckMana(attacker, true))
            {
                attacker.SendLocalizedMessage(1060092); // You disarm their weapon!
                defender.SendLocalizedMessage(1060093); // Your weapon has been disarmed!

                defender.PlaySound(0x3B9);
                defender.FixedParticles(0x37BE, 232, 25, 9948, EffectLayer.LeftHand);

                pack.DropItem(toDisarm);

                BaseWeapon.BlockEquip(defender, BlockEquipDuration);
            }
        }
    }

    /// <summary>
    /// Perfect for the foot-soldier, the Dismount special attack can unseat a mounted opponent.
    /// The fighter using this ability must be on his own two feet and not in the saddle of a steed
    /// (with one exception: players may use a lance to dismount other players while mounted).
    /// If it works, the target will be knocked off his own mount and will take some extra damage from the fall!
    /// </summary>
    public class Dismount : WeaponAbility
    {
        public Dismount()
        {
        }

        public override int BaseMana { get { return 20; } }

        public override bool Validate(Mobile from)
        {
            if (!base.Validate(from))
                return false;

            if (from.Mounted && !(from.Weapon is Lance))
            {
                from.SendLocalizedMessage(1061283); // You cannot perform that attack while mounted!
                return false;
            }

            return true;
        }

        public static readonly TimeSpan RemountDelay = TimeSpan.FromSeconds(10.0);

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!Validate(attacker))
                return;

            if (defender is ChaosDragoon || defender is ChaosDragoonElite)
                return;

            if (attacker.Mounted && (!(attacker.Weapon is Lance) || !(defender.Weapon is Lance))) // TODO: Should there be a message here?
                return;

            ClearCurrentAbility(attacker);

            IMount mount = defender.Mount;

            if (mount == null && !Server.Spells.Ninjitsu.AnimalForm.UnderTransformation(defender))
            {
                attacker.SendLocalizedMessage(1060848); // This attack only works on mounted targets
                return;
            }

            if (!CheckMana(attacker, true))
                return;

            if (Core.ML && attacker is LesserHiryu && 0.8 >= Utility.RandomDouble())
            {
                return; //Lesser Hiryu have an 80% chance of missing this attack
            }

            attacker.SendLocalizedMessage(1060082); // The force of your attack has dislodged them from their mount!

            if (attacker.Mounted)
                defender.SendLocalizedMessage(1062315); // You fall off your mount!
            else
                defender.SendLocalizedMessage(1060083); // You fall off of your mount and take damage!

            defender.PlaySound(0x140);
            defender.FixedParticles(0x3728, 10, 15, 9955, EffectLayer.Waist);

            if (defender is PlayerMobile)
            {
                if (Server.Spells.Ninjitsu.AnimalForm.UnderTransformation(defender))
                {
                    defender.SendLocalizedMessage(1114066, attacker.Name); // ~1_NAME~ knocked you out of animal form!
                }
                else if (defender.Mounted)
                {
                    defender.SendLocalizedMessage(1040023); // You have been knocked off of your mount!
                }

                (defender as PlayerMobile).SetMountBlock(BlockMountType.Dazed, TimeSpan.FromSeconds(10), true);
            }
            else
            {
                defender.Mount.Rider = null;
            }

            if (attacker is PlayerMobile)
            {
                (attacker as PlayerMobile).SetMountBlock(BlockMountType.DismountRecovery, RemountDelay, true);
            }
            else if (Core.ML && attacker is BaseCreature)
            {
                BaseCreature bc = attacker as BaseCreature;

                if (bc.ControlMaster is PlayerMobile)
                {
                    PlayerMobile pm = bc.ControlMaster as PlayerMobile;

                    pm.SetMountBlock(BlockMountType.DismountRecovery, RemountDelay, false);
                }
            }

            if (!attacker.Mounted)
                AOS.Damage(defender, attacker, Utility.RandomMinMax(15, 25), 100, 0, 0, 0, 0);
        }
    }

    /// <summary>
    /// This attack allows you to disrobe your foe.
    /// </summary>
    public class Disrobe : WeaponAbility
    {
        public Disrobe()
        {
        }

        public override int BaseMana { get { return 20; } } // Not Sure what amount of mana a creature uses.

        public static readonly TimeSpan BlockEquipDuration = TimeSpan.FromSeconds(5.0);

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!Validate(attacker))
                return;

            ClearCurrentAbility(attacker);
            Item toDisrobe = defender.FindItemOnLayer(Layer.InnerTorso);

            if (toDisrobe == null || !toDisrobe.Movable)
                toDisrobe = defender.FindItemOnLayer(Layer.OuterTorso);

            Container pack = defender.Backpack;

            if (pack == null || toDisrobe == null || !toDisrobe.Movable)
            {
                attacker.SendLocalizedMessage(1004001); // You cannot disarm your opponent.
            }
            else if (CheckMana(attacker, true))
            {
                //attacker.SendLocalizedMessage( 1060092 ); // You disarm their weapon!
                defender.SendLocalizedMessage(1062002); // You can no longer wear your ~1_ARMOR~

                defender.PlaySound(0x3B9);
                //defender.FixedParticles( 0x37BE, 232, 25, 9948, EffectLayer.InnerTorso );

                pack.DropItem(toDisrobe);

                BaseWeapon.BlockEquip(defender, BlockEquipDuration);
            }
        }
    }

    /// <summary>
    /// Send two arrows flying at your opponent if you're mounted. Requires Bushido or Ninjitsu skill.
    /// </summary>
    public class DoubleShot : WeaponAbility
    {
        public DoubleShot()
        {
        }

        public override int BaseMana { get { return 30; } }

        public override bool CheckSkills(Mobile from)
        {
            if (GetSkill(from, SkillName.Ninjitsu) < 50.0 && GetSkill(from, SkillName.Bushido) < 50.0)
            {
                from.SendLocalizedMessage(1063347, "50"); // You need ~1_SKILL_REQUIREMENT~ Bushido or Ninjitsu skill to perform that attack!
                return false;
            }

            return base.CheckSkills(from);
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            Use(attacker, defender);
        }

        public override void OnMiss(Mobile attacker, Mobile defender)
        {
            Use(attacker, defender);
        }

        public override bool Validate(Mobile from)
        {
            if (base.Validate(from))
            {
                if (from.Mounted)
                    return true;
                else
                {
                    from.SendLocalizedMessage(1070770); // You can only execute this attack while mounted!
                    ClearCurrentAbility(from);
                }
            }

            return false;
        }

        public void Use(Mobile attacker, Mobile defender)
        {
            if (!Validate(attacker) || !CheckMana(attacker, true) || attacker.Weapon == null)	//sanity
                return;

            ClearCurrentAbility(attacker);

            attacker.SendLocalizedMessage(1063348); // You launch two shots at once!
            defender.SendLocalizedMessage(1063349); // You're attacked with a barrage of shots!

            defender.FixedParticles(0x37B9, 1, 19, 0x251D, EffectLayer.Waist);

            attacker.Weapon.OnSwing(attacker, defender);
        }
    }

    /// <summary>
    /// The highly skilled warrior can use this special attack to make two quick swings in succession.
    /// Landing both blows would be devastating! 
    /// </summary>
    public class DoubleStrike : WeaponAbility
    {
        public DoubleStrike()
        {
        }

        public override int BaseMana { get { return 30; } }
        public override double DamageScalar { get { return 0.9; } }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!Validate(attacker) || !CheckMana(attacker, true))
                return;

            ClearCurrentAbility(attacker);

            attacker.SendLocalizedMessage(1060084); // You attack with lightning speed!
            defender.SendLocalizedMessage(1060085); // Your attacker strikes with lightning speed!

            defender.PlaySound(0x3BB);
            defender.FixedEffect(0x37B9, 244, 25);

            // Swing again:

            // If no combatant, wrong map, one of us is a ghost, or cannot see, or deleted, then stop combat
            if (defender == null || defender.Deleted || attacker.Deleted || defender.Map != attacker.Map || !defender.Alive || !attacker.Alive || !attacker.CanSee(defender))
            {
                attacker.Combatant = null;
                return;
            }

            IWeapon weapon = attacker.Weapon;

            if (weapon == null)
                return;

            if (!attacker.InRange(defender, weapon.MaxRange))
                return;

            if (attacker.InLOS(defender))
            {
                BaseWeapon.InDoubleStrike = true;
                attacker.RevealingAction();
                attacker.NextCombatTime = Core.TickCount + (int)weapon.OnSwing(attacker, defender).TotalMilliseconds;
                BaseWeapon.InDoubleStrike = false;
            }
        }
    }

    /// <summary>
    /// Attack faster as you swing with both weapons.
    /// </summary>
    public class DualWield : WeaponAbility
    {
        private static Hashtable m_Registry = new Hashtable();
        public static Hashtable Registry { get { return m_Registry; } }

        public DualWield()
        {
        }

        public override int BaseMana { get { return 30; } }

        public override bool CheckSkills(Mobile from)
        {
            if (GetSkill(from, SkillName.Ninjitsu) < 50.0)
            {
                from.SendLocalizedMessage(1063352, "50"); // You need ~1_SKILL_REQUIREMENT~ Ninjitsu skill to perform that attack!
                return false;
            }

            return base.CheckSkills(from);
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!Validate(attacker) || !CheckMana(attacker, true))
                return;

            if (Registry.Contains(attacker))
            {
                DualWieldTimer existingtimer = (DualWieldTimer)Registry[attacker];
                existingtimer.Stop();
                Registry.Remove(attacker);
            }

            ClearCurrentAbility(attacker);

            attacker.SendLocalizedMessage(1063362); // You dually wield for increased speed!

            attacker.FixedParticles(0x3779, 1, 15, 0x7F6, 0x3E8, 3, EffectLayer.LeftHand);

            Timer t = new DualWieldTimer(attacker, (int)(20.0 + 3.0 * (attacker.Skills[SkillName.Ninjitsu].Value - 50.0) / 7.0));	//20-50 % increase

            t.Start();
            Registry.Add(attacker, t);
        }

        public class DualWieldTimer : Timer
        {
            private Mobile m_Owner;
            private int m_BonusSwingSpeed;

            public int BonusSwingSpeed { get { return m_BonusSwingSpeed; } }

            public DualWieldTimer(Mobile owner, int bonusSwingSpeed)
                : base(TimeSpan.FromSeconds(6.0))
            {
                m_Owner = owner;
                m_BonusSwingSpeed = bonusSwingSpeed;
                Priority = TimerPriority.FiftyMS;
            }

            protected override void OnTick()
            {
                Registry.Remove(m_Owner);
            }
        }
    }

    /// <summary>
    /// Gain a defensive advantage over your primary opponent for a short time.
    /// </summary>
    public class Feint : WeaponAbility
    {
        private static Hashtable m_Registry = new Hashtable();
        public static Hashtable Registry { get { return m_Registry; } }

        public Feint()
        {
        }

        public override int BaseMana { get { return 30; } }

        public override bool CheckSkills(Mobile from)
        {
            if (GetSkill(from, SkillName.Ninjitsu) < 50.0 && GetSkill(from, SkillName.Bushido) < 50.0)
            {
                from.SendLocalizedMessage(1063347, "50"); // You need ~1_SKILL_REQUIREMENT~ Bushido or Ninjitsu skill to perform that attack!
                return false;
            }

            return base.CheckSkills(from);
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!Validate(attacker) || !CheckMana(attacker, true))
                return;

            if (Registry.Contains(defender))
            {
                FeintTimer existingtimer = (FeintTimer)Registry[defender];
                existingtimer.Stop();
                Registry.Remove(defender);
            }

            ClearCurrentAbility(attacker);

            attacker.SendLocalizedMessage(1063360); // You baffle your target with a feint!
            defender.SendLocalizedMessage(1063361); // You were deceived by an attacker's feint!

            attacker.FixedParticles(0x3728, 1, 13, 0x7F3, 0x962, 0, EffectLayer.Waist);

            Timer t = new FeintTimer(defender, (int)(20.0 + 3.0 * (Math.Max(attacker.Skills[SkillName.Ninjitsu].Value, attacker.Skills[SkillName.Bushido].Value) - 50.0) / 7.0));	//20-50 % decrease

            t.Start();
            Registry.Add(defender, t);
        }

        public class FeintTimer : Timer
        {
            private Mobile m_Defender;
            private int m_SwingSpeedReduction;

            public int SwingSpeedReduction { get { return m_SwingSpeedReduction; } }

            public FeintTimer(Mobile defender, int swingSpeedReduction)
                : base(TimeSpan.FromSeconds(6.0))
            {
                m_Defender = defender;
                m_SwingSpeedReduction = swingSpeedReduction;
                Priority = TimerPriority.FiftyMS;
            }

            protected override void OnTick()
            {
                Registry.Remove(m_Defender);
            }
        }
    }

    /// <summary>
    /// A quick attack to all enemies in range of your weapon that causes damage over time. Requires Bushido or Ninjitsu skill.
    /// </summary>
    public class FrenziedWhirlwind : WeaponAbility
    {
        public FrenziedWhirlwind()
        {
        }

        public override bool CheckSkills(Mobile from)
        {
            if (GetSkill(from, SkillName.Ninjitsu) < 50.0 && GetSkill(from, SkillName.Bushido) < 50.0)
            {
                from.SendLocalizedMessage(1063347, "50"); // You need ~1_SKILL_REQUIREMENT~ Bushido or Ninjitsu skill to perform that attack!
                return false;
            }

            return base.CheckSkills(from);
        }

        public override int BaseMana { get { return 30; } }

        private static Hashtable m_Registry = new Hashtable();
        public static Hashtable Registry { get { return m_Registry; } }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!Validate(attacker))	//Mana check after check that there are targets
                return;

            ClearCurrentAbility(attacker);

            Map map = attacker.Map;

            if (map == null)
                return;

            BaseWeapon weapon = attacker.Weapon as BaseWeapon;

            if (weapon == null)
                return;

            ArrayList list = new ArrayList();

            foreach (Mobile m in attacker.GetMobilesInRange(1))
                list.Add(m);

            ArrayList targets = new ArrayList();

            for (int i = 0; i < list.Count; ++i)
            {
                Mobile m = (Mobile)list[i];

                if (m != defender && m != attacker && SpellHelper.ValidIndirectTarget(attacker, m))
                {
                    if (m == null || m.Deleted || m.Map != attacker.Map || !m.Alive || !attacker.CanSee(m) || !attacker.CanBeHarmful(m))
                        continue;

                    if (!attacker.InRange(m, weapon.MaxRange))
                        continue;

                    if (attacker.InLOS(m))
                        targets.Add(m);
                }
            }

            if (targets.Count > 0)
            {
                if (!CheckMana(attacker, true))
                    return;

                attacker.FixedEffect(0x3728, 10, 15);
                attacker.PlaySound(0x2A1);

                // 5-15 damage
                int amount = (int)(10.0 * ((Math.Max(attacker.Skills[SkillName.Bushido].Value, attacker.Skills[SkillName.Ninjitsu].Value) - 50.0) / 70.0 + 5));

                for (int i = 0; i < targets.Count; ++i)
                {
                    Mobile m = (Mobile)targets[i];
                    attacker.DoHarmful(m, true);

                    Timer t = Registry[m] as Timer;

                    if (t != null)
                    {
                        t.Stop();
                        Registry.Remove(m);
                    }

                    t = new InternalTimer(attacker, m, amount);
                    t.Start();
                    Registry.Add(m, t);
                }

                Timer.DelayCall(TimeSpan.FromSeconds(2.0), new TimerStateCallback(RepeatEffect), attacker);
            }
        }

        private void RepeatEffect(object state)
        {
            Mobile attacker = (Mobile)state;

            attacker.FixedEffect(0x3728, 10, 15);
            attacker.PlaySound(0x2A1);
        }

        private class InternalTimer : Timer
        {
            private Mobile m_Attacker;
            private Mobile m_Defender;
            private double m_DamageRemaining;
            private double m_DamageToDo;

            private readonly double DamagePerTick;

            public InternalTimer(Mobile attacker, Mobile defender, int totalDamage)
                : base(TimeSpan.Zero, TimeSpan.FromSeconds(0.25), 12)	// 3 seconds at .25 seconds apart = 12.  Confirm delay inbetween of .25 each.
            {
                m_Attacker = attacker;
                m_Defender = defender;

                m_DamageRemaining = (double)totalDamage;
                DamagePerTick = (double)totalDamage / 12 + 0.01;

                Priority = TimerPriority.TwentyFiveMS;
            }

            protected override void OnTick()
            {
                if (!m_Defender.Alive || m_DamageRemaining <= 0)
                {
                    Stop();
                    Server.Items.FrenziedWhirlwind.Registry.Remove(m_Defender);
                    return;
                }

                m_DamageRemaining -= DamagePerTick;
                m_DamageToDo += DamagePerTick;

                if (m_DamageRemaining <= 0 && m_DamageToDo < 1)
                    m_DamageToDo = 1.0; //Confirm this 'round up' at the end

                int damage = (int)m_DamageToDo;

                if (damage > 0)
                {
                    m_Defender.Damage(damage, m_Attacker);
                    m_DamageToDo -= damage;
                }

                if (!m_Defender.Alive || m_DamageRemaining <= 0)
                {
                    Stop();
                    Server.Items.FrenziedWhirlwind.Registry.Remove(m_Defender);
                }
            }
        }
    }

    /// <summary>
    /// This special move represents a significant change to the use of poisons in Age of Shadows.
    /// Now, only certain weapon types — those that have Infectious Strike as an available special move — will be able to be poisoned.
    /// Targets will no longer be poisoned at random when hit by poisoned weapons.
    /// Instead, the wielder must use this ability to deliver the venom.
    /// While no skill in Poisoning is directly required to use this ability, being knowledgeable in the application and use of toxins
    /// will allow a character to use Infectious Strike at reduced mana cost and with a chance to inflict more deadly poison on his victim.
    /// With this change, weapons will no longer be corroded by poison.
    /// Level 5 poison will be possible when using this special move.
    /// </summary>
    public class InfectiousStrike : WeaponAbility
    {
        public InfectiousStrike()
        {
        }

        public override int BaseMana { get { return 15; } }

        public override bool RequiresTactics(Mobile from)
        {
            return false;
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!Validate(attacker))
                return;

            ClearCurrentAbility(attacker);

            BaseWeapon weapon = attacker.Weapon as BaseWeapon;

            if (weapon == null)
                return;

            Poison p = weapon.Poison;

            if (p == null || weapon.PoisonCharges <= 0)
            {
                attacker.SendLocalizedMessage(1061141); // Your weapon must have a dose of poison to perform an infectious strike!
                return;
            }

            if (!CheckMana(attacker, true))
                return;

            --weapon.PoisonCharges;

            // Infectious strike special move now uses poisoning skill to help determine potency 
            int maxLevel = attacker.Skills[SkillName.Poisoning].Fixed / 200;
            if (maxLevel < 0) maxLevel = 0;
            if (p.Level > maxLevel) p = Poison.GetPoison(maxLevel);

            if ((attacker.Skills[SkillName.Poisoning].Value / 100.0) > Utility.RandomDouble())
            {
                int level = p.Level + 1;
                Poison newPoison = Poison.GetPoison(level);

                if (newPoison != null)
                {
                    p = newPoison;

                    attacker.SendLocalizedMessage(1060080); // Your precise strike has increased the level of the poison by 1
                    defender.SendLocalizedMessage(1060081); // The poison seems extra effective!
                }
            }

            defender.PlaySound(0xDD);
            defender.FixedParticles(0x3728, 244, 25, 9941, 1266, 0, EffectLayer.Waist);

            if (defender.ApplyPoison(attacker, p) != ApplyPoisonResult.Immune)
            {
                attacker.SendLocalizedMessage(1008096, true, defender.Name); // You have poisoned your target : 
                defender.SendLocalizedMessage(1008097, false, attacker.Name); //  : poisoned you!
            }
        }
    }

    /// <summary>
    /// The assassin's friend.
    /// A successful Mortal Strike will render its victim unable to heal any damage for several seconds. 
    /// Use a gruesome follow-up to finish off your foe.
    /// </summary>
    public class MortalStrike : WeaponAbility
    {
        public MortalStrike()
        {
        }

        public override int BaseMana { get { return 30; } }

        public static readonly TimeSpan PlayerDuration = TimeSpan.FromSeconds(6.0);
        public static readonly TimeSpan NPCDuration = TimeSpan.FromSeconds(12.0);

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!Validate(attacker) || !CheckMana(attacker, true))
            {
                return;
            }

            ClearCurrentAbility(attacker);

            attacker.SendLocalizedMessage(1060086); // You deliver a mortal wound!
            defender.SendLocalizedMessage(1060087); // You have been mortally wounded!

            defender.PlaySound(0x1E1);
            defender.FixedParticles(0x37B9, 244, 25, 9944, 31, 0, EffectLayer.Waist);

            // Do not reset timer if one is already in place.
            if (!IsWounded(defender))
                BeginWound(defender, defender.Player ? PlayerDuration : NPCDuration);
        }

        private static Hashtable m_Table = new Hashtable();

        public static bool IsWounded(Mobile m)
        {
            return m_Table.Contains(m);
        }

        public static void BeginWound(Mobile m, TimeSpan duration)
        {
            Timer t = (Timer)m_Table[m];

            if (t != null)
                t.Stop();

            t = new InternalTimer(m, duration);
            m_Table[m] = t;

            t.Start();

            m.YellowHealthbar = true;
        }

        public static void EndWound(Mobile m)
        {
            if (!IsWounded(m))
                return;

            Timer t = (Timer)m_Table[m];

            if (t != null)
                t.Stop();

            m_Table.Remove(m);

            m.YellowHealthbar = false;
            m.SendLocalizedMessage(1060208); // You are no longer mortally wounded.
        }

        private class InternalTimer : Timer
        {
            private Mobile m_Mobile;

            public InternalTimer(Mobile m, TimeSpan duration)
                : base(duration)
            {
                m_Mobile = m;
                Priority = TimerPriority.TwoFiftyMS;
            }

            protected override void OnTick()
            {
                EndWound(m_Mobile);
            }
        }
    }

    /// <summary>
    /// Available on some crossbows, this special move allows archers to fire while on the move.
    /// This shot is somewhat less accurate than normal, but the ability to fire while running is a clear advantage.
    /// </summary>
    public class MovingShot : WeaponAbility
    {
        public MovingShot()
        {
        }

        public override int BaseMana { get { return 15; } }
        public override int AccuracyBonus { get { return -25; } }

        public override bool OnBeforeSwing(Mobile attacker, Mobile defender)
        {
            return (Validate(attacker) && CheckMana(attacker, true));
        }

        public override void OnMiss(Mobile attacker, Mobile defender)
        {
            //Validates in OnSwing for accuracy scalar

            ClearCurrentAbility(attacker);

            attacker.SendLocalizedMessage(1060089); // You fail to execute your special move
        }

        public override bool ValidatesDuringHit { get { return false; } }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            //Validates in OnSwing for accuracy scalar

            ClearCurrentAbility(attacker);

            attacker.SendLocalizedMessage(1060216); // Your shot was successful
        }
    }

    /// <summary>
    /// Does damage and paralyses your opponent for a short time.
    /// </summary>
    public class NerveStrike : WeaponAbility
    {
        public NerveStrike()
        {
        }

        public override int BaseMana { get { return 30; } }

        public override bool CheckSkills(Mobile from)
        {
            if (GetSkill(from, SkillName.Bushido) < 50.0)
            {
                from.SendLocalizedMessage(1070768, "50"); // You need ~1_SKILL_REQUIREMENT~ Bushido skill to perform that attack!
                return false;
            }

            return base.CheckSkills(from);
        }

        public override bool OnBeforeSwing(Mobile attacker, Mobile defender)
        {
            if (defender.Paralyzed)
            {
                attacker.SendLocalizedMessage(1061923); // The target is already frozen.
                return false;
            }

            return true;
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!Validate(attacker) || !CheckMana(attacker, true))
                return;

            ClearCurrentAbility(attacker);

            bool cantpara = Server.Items.ParalyzingBlow.IsImmune(defender);

            if (cantpara)
            {
                attacker.SendLocalizedMessage(1070804); // Your target resists paralysis.
                defender.SendLocalizedMessage(1070813); // You resist paralysis.
            }
            else
            {
                attacker.SendLocalizedMessage(1063356); // You cripple your target with a nerve strike!
                defender.SendLocalizedMessage(1063357); // Your attacker dealt a crippling nerve strike!
            }

            attacker.PlaySound(0x204);
            defender.FixedEffect(0x376A, 9, 32);
            defender.FixedParticles(0x37C4, 1, 8, 0x13AF, 0, 0, EffectLayer.Waist);

            if (Core.ML)
            {
                AOS.Damage(defender, attacker, (int)(15.0 * (attacker.Skills[SkillName.Bushido].Value - 50.0) / 70.0 + Utility.Random(10)), true, 100, 0, 0, 0, 0);	//0-25

                if (!cantpara && ((150.0 / 7.0 + (4.0 * attacker.Skills[SkillName.Bushido].Value) / 7.0) / 100.0) > Utility.RandomDouble())
                {
                    defender.Paralyze(TimeSpan.FromSeconds(2.0));
                    Server.Items.ParalyzingBlow.BeginImmunity(defender, Server.Items.ParalyzingBlow.FreezeDelayDuration);
                }
            }
            else if (!cantpara)
            {
                AOS.Damage(defender, attacker, (int)(15.0 * (attacker.Skills[SkillName.Bushido].Value - 50.0) / 70.0 + 10), true, 100, 0, 0, 0, 0); //10-25
                defender.Freeze(TimeSpan.FromSeconds(2.0));
                Server.Items.ParalyzingBlow.BeginImmunity(defender, Server.Items.ParalyzingBlow.FreezeDelayDuration);
            }
        }
    }

    /// <summary>
    /// A successful Paralyzing Blow will leave the target stunned, unable to move, attack, or cast spells, for a few seconds.
    /// </summary>
    public class ParalyzingBlow : WeaponAbility
    {
        public ParalyzingBlow()
        {
        }

        public override int BaseMana { get { return 30; } }

        public static readonly TimeSpan PlayerFreezeDuration = TimeSpan.FromSeconds(3.0);
        public static readonly TimeSpan NPCFreezeDuration = TimeSpan.FromSeconds(6.0);

        public static readonly TimeSpan FreezeDelayDuration = TimeSpan.FromSeconds(8.0);

        // No longer active in pub21:
        /*public override bool CheckSkills( Mobile from )
        {
            if ( !base.CheckSkills( from ) )
                return false;

            if ( !(from.Weapon is Fists) )
                return true;

            Skill skill = from.Skills[SkillName.Anatomy];

            if ( skill != null && skill.Base >= 80.0 )
                return true;

            from.SendLocalizedMessage( 1061811 ); // You lack the required anatomy skill to perform that attack!

            return false;
        }*/

        public override bool RequiresTactics(Mobile from)
        {
            BaseWeapon weapon = from.Weapon as BaseWeapon;

            if (weapon == null)
                return true;

            return weapon.Skill != SkillName.Wrestling;
        }

        public override bool OnBeforeSwing(Mobile attacker, Mobile defender)
        {
            if (defender.Paralyzed)
            {
                attacker.SendLocalizedMessage(1061923); // The target is already frozen.
                return false;
            }

            return true;
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!Validate(attacker) || !CheckMana(attacker, true))
                return;

            ClearCurrentAbility(attacker);

            if (IsImmune(defender))	//Intentionally going after Mana consumption
            {
                attacker.SendLocalizedMessage(1070804); // Your target resists paralysis.
                defender.SendLocalizedMessage(1070813); // You resist paralysis.
                return;
            }

            defender.FixedEffect(0x376A, 9, 32);
            defender.PlaySound(0x204);

            attacker.SendLocalizedMessage(1060163); // You deliver a paralyzing blow!
            defender.SendLocalizedMessage(1060164); // The attack has temporarily paralyzed you!

            TimeSpan duration = defender.Player ? PlayerFreezeDuration : NPCFreezeDuration;

            // Treat it as paralyze not as freeze, effect must be removed when damaged.
            defender.Paralyze(duration);

            BeginImmunity(defender, duration + FreezeDelayDuration);
        }

        private static Hashtable m_Table = new Hashtable();

        public static bool IsImmune(Mobile m)
        {
            return m_Table.Contains(m);
        }

        public static void BeginImmunity(Mobile m, TimeSpan duration)
        {
            Timer t = (Timer)m_Table[m];

            if (t != null)
                t.Stop();

            t = new InternalTimer(m, duration);
            m_Table[m] = t;

            t.Start();
        }

        public static void EndImmunity(Mobile m)
        {
            Timer t = (Timer)m_Table[m];

            if (t != null)
                t.Stop();

            m_Table.Remove(m);
        }

        private class InternalTimer : Timer
        {
            private Mobile m_Mobile;

            public InternalTimer(Mobile m, TimeSpan duration)
                : base(duration)
            {
                m_Mobile = m;
                Priority = TimerPriority.TwoFiftyMS;
            }

            protected override void OnTick()
            {
                EndImmunity(m_Mobile);
            }
        }
    }

    /// <summary>
    /// If you are on foot, dismounts your opponent and damage the ethereal's rider or the
    /// living mount(which must be healed before ridden again). If you are mounted, damages
    /// and stuns the mounted opponent.
    /// </summary>
    public class RidingSwipe : WeaponAbility
    {
        public RidingSwipe()
        {
        }

        public override int BaseMana { get { return 30; } }

        public override bool RequiresSE { get { return true; } }

        public override bool CheckSkills(Mobile from)
        {
            if (GetSkill(from, SkillName.Bushido) < 50.0)
            {
                from.SendLocalizedMessage(1070768, "50"); // You need ~1_SKILL_REQUIREMENT~ Bushido skill to perform that attack!
                return false;
            }

            return base.CheckSkills(from);
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!defender.Mounted)
            {
                attacker.SendLocalizedMessage(1060848); // This attack only works on mounted targets
                ClearCurrentAbility(attacker);
                return;
            }

            if (!Validate(attacker) || !CheckMana(attacker, true))
                return;

            ClearCurrentAbility(attacker);

            if (!attacker.Mounted)
            {
                Mobile mount = defender.Mount as Mobile;
                BaseMount.Dismount(defender);

                if (mount != null)	//Ethy mounts don't take damage
                {
                    int amount = 10 + (int)(10.0 * (attacker.Skills[SkillName.Bushido].Value - 50.0) / 70.0 + 5);

                    AOS.Damage(mount, null, amount, 100, 0, 0, 0, 0);	//The mount just takes damage, there's no flagging as if it was attacking the mount directly

                    //TODO: Mount prevention until mount healed
                }
            }
            else
            {
                int amount = 10 + (int)(10.0 * (attacker.Skills[SkillName.Bushido].Value - 50.0) / 70.0 + 5);

                AOS.Damage(defender, attacker, amount, 100, 0, 0, 0, 0);

                if (Server.Items.ParalyzingBlow.IsImmune(defender))	//Does it still do damage?
                {
                    attacker.SendLocalizedMessage(1070804); // Your target resists paralysis.
                    defender.SendLocalizedMessage(1070813); // You resist paralysis.
                }
                else
                {
                    defender.Paralyze(TimeSpan.FromSeconds(3.0));
                    Server.Items.ParalyzingBlow.BeginImmunity(defender, Server.Items.ParalyzingBlow.FreezeDelayDuration);
                }
            }
        }
    }

    /// <summary>
    /// This powerful ability requires secondary skills to activate.
    /// Successful use of Shadowstrike deals extra damage to the target — and renders the attacker invisible!
    /// Only those who are adept at the art of stealth will be able to use this ability.
    /// </summary>
    public class ShadowStrike : WeaponAbility
    {
        public ShadowStrike()
        {
        }

        public override int BaseMana { get { return 20; } }
        public override double DamageScalar { get { return 1.25; } }

        public override bool RequiresTactics(Mobile from)
        {
            return false;
        }

        public override bool CheckSkills(Mobile from)
        {
            if (!base.CheckSkills(from))
                return false;

            Skill skill = from.Skills[SkillName.Stealth];

            if (skill != null && skill.Value >= 80.0)
                return true;

            from.SendLocalizedMessage(1060183); // You lack the required stealth to perform that attack

            return false;
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!Validate(attacker) || !CheckMana(attacker, true))
                return;

            ClearCurrentAbility(attacker);

            attacker.SendLocalizedMessage(1060078); // You strike and hide in the shadows!
            defender.SendLocalizedMessage(1060079); // You are dazed by the attack and your attacker vanishes!

            Effects.SendLocationParticles(EffectItem.Create(attacker.Location, attacker.Map, EffectItem.DefaultDuration), 0x376A, 8, 12, 9943);
            attacker.PlaySound(0x482);

            defender.FixedEffect(0x37BE, 20, 25);

            attacker.Combatant = null;
            attacker.Warmode = false;
            attacker.Hidden = true;
        }
    }

    /// <summary>
    /// Attack with increased damage with additional damage over time.
    /// </summary>
    public class TalonStrike : WeaponAbility
    {
        private static Hashtable m_Registry = new Hashtable();
        public static Hashtable Registry { get { return m_Registry; } }

        public TalonStrike()
        {
        }

        public override int BaseMana { get { return 30; } }
        public override double DamageScalar { get { return 1.2; } }

        public override bool CheckSkills(Mobile from)
        {
            if (GetSkill(from, SkillName.Ninjitsu) < 50.0)
            {
                from.SendLocalizedMessage(1063352, "50"); // You need ~1_SKILL_REQUIREMENT~ Ninjitsu skill to perform that attack!
                return false;
            }

            return base.CheckSkills(from);
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (Registry.Contains(defender) || !Validate(attacker) || !CheckMana(attacker, true))
                return;

            ClearCurrentAbility(attacker);

            attacker.SendLocalizedMessage(1063358); // You deliver a talon strike!
            defender.SendLocalizedMessage(1063359); // Your attacker delivers a talon strike!

            defender.FixedParticles(0x373A, 1, 17, 0x26BC, 0x662, 0, EffectLayer.Waist);

            Timer t = new InternalTimer(defender, (int)(10.0 * (attacker.Skills[SkillName.Ninjitsu].Value - 50.0) / 70.0 + 5), attacker);	//5 - 15 damage

            t.Start();

            Registry.Add(defender, t);
        }

        private class InternalTimer : Timer
        {
            private Mobile m_Defender;
            private double m_DamageRemaining;
            private double m_DamageToDo;
            private Mobile m_Attacker;

            private readonly double DamagePerTick;

            public InternalTimer(Mobile defender, int totalDamage, Mobile attacker)
                : base(TimeSpan.Zero, TimeSpan.FromSeconds(0.25), 12)	// 3 seconds at .25 seconds apart = 12.  Confirm delay inbetween of .25 each.
            {
                m_Defender = defender;
                m_DamageRemaining = (double)totalDamage;
                Priority = TimerPriority.TwentyFiveMS;

                m_Attacker = attacker;

                DamagePerTick = (double)totalDamage / 12 + .01;
            }

            protected override void OnTick()
            {
                if (!m_Defender.Alive || m_DamageRemaining <= 0)
                {
                    Stop();
                    Server.Items.TalonStrike.Registry.Remove(m_Defender);
                    return;
                }

                m_DamageRemaining -= DamagePerTick;
                m_DamageToDo += DamagePerTick;

                if (m_DamageRemaining <= 0 && m_DamageToDo < 1)
                    m_DamageToDo = 1.0; //Confirm this 'round up' at the end

                int damage = (int)m_DamageToDo;

                if (damage > 0)
                {
                    //m_Defender.Damage( damage, m_Attacker, false );
                    m_Defender.Hits -= damage;	//Don't show damage, don't disrupt
                    m_DamageToDo -= damage;
                }

                if (!m_Defender.Alive || m_DamageRemaining <= 0)
                {
                    Stop();
                    Server.Items.TalonStrike.Registry.Remove(m_Defender);
                }
            }
        }
    }

    /// <summary>
    /// A godsend to a warrior surrounded, the Whirlwind Attack allows the fighter to strike at all nearby targets in one mighty spinning swing.
    /// </summary>
    public class WhirlwindAttack : WeaponAbility
    {
        public WhirlwindAttack()
        {
        }

        public override int BaseMana { get { return 15; } }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!Validate(attacker))
                return;

            ClearCurrentAbility(attacker);

            Map map = attacker.Map;

            if (map == null)
                return;

            BaseWeapon weapon = attacker.Weapon as BaseWeapon;

            if (weapon == null)
                return;

            if (!CheckMana(attacker, true))
                return;

            attacker.FixedEffect(0x3728, 10, 15);
            attacker.PlaySound(0x2A1);

            ArrayList list = new ArrayList();

            foreach (Mobile m in attacker.GetMobilesInRange(1))
                list.Add(m);

            ArrayList targets = new ArrayList();

            for (int i = 0; i < list.Count; ++i)
            {
                Mobile m = (Mobile)list[i];

                if (m != defender && m != attacker && SpellHelper.ValidIndirectTarget(attacker, m))
                {
                    if (m == null || m.Deleted || m.Map != attacker.Map || !m.Alive || !attacker.CanSee(m) || !attacker.CanBeHarmful(m))
                        continue;

                    if (!attacker.InRange(m, weapon.MaxRange))
                        continue;

                    if (attacker.InLOS(m))
                        targets.Add(m);
                }
            }

            if (targets.Count > 0)
            {
                double bushido = attacker.Skills.Bushido.Value;
                double damageBonus = 1.0 + Math.Pow((targets.Count * bushido) / 60, 2) / 100;

                if (damageBonus > 2.0)
                    damageBonus = 2.0;

                attacker.RevealingAction();

                for (int i = 0; i < targets.Count; ++i)
                {
                    Mobile m = (Mobile)targets[i];

                    attacker.SendLocalizedMessage(1060161); // The whirling attack strikes a target!
                    m.SendLocalizedMessage(1060162); // You are struck by the whirling attack and take damage!

                    weapon.OnHit(attacker, m, damageBonus);
                }
            }
        }
    }

    #endregion

    #region Weapon Attribute

    public class HitLower
    {
        public static readonly TimeSpan AttackEffectDuration = TimeSpan.FromSeconds(10.0);
        public static readonly TimeSpan DefenseEffectDuration = TimeSpan.FromSeconds(8.0);

        private static Hashtable m_AttackTable = new Hashtable();

        public static bool IsUnderAttackEffect(Mobile m)
        {
            return m_AttackTable.Contains(m);
        }

        public static bool ApplyAttack(Mobile m)
        {
            if (IsUnderAttackEffect(m))
                return false;

            m_AttackTable[m] = new AttackTimer(m);
            m.SendLocalizedMessage(1062319); // Your attack chance has been reduced!
            return true;
        }

        private static void RemoveAttack(Mobile m)
        {
            m_AttackTable.Remove(m);
            m.SendLocalizedMessage(1062320); // Your attack chance has returned to normal.
        }

        private class AttackTimer : Timer
        {
            private Mobile m_Player;

            public AttackTimer(Mobile player)
                : base(AttackEffectDuration)
            {
                m_Player = player;

                Priority = TimerPriority.TwoFiftyMS;

                Start();
            }

            protected override void OnTick()
            {
                RemoveAttack(m_Player);
            }
        }

        private static Hashtable m_DefenseTable = new Hashtable();

        public static bool IsUnderDefenseEffect(Mobile m)
        {
            return m_DefenseTable.Contains(m);
        }

        public static bool ApplyDefense(Mobile m)
        {
            if (IsUnderDefenseEffect(m))
                return false;

            m_DefenseTable[m] = new DefenseTimer(m);
            m.SendLocalizedMessage(1062318); // Your defense chance has been reduced!
            return true;
        }

        private static void RemoveDefense(Mobile m)
        {
            m_DefenseTable.Remove(m);
            m.SendLocalizedMessage(1062321); // Your defense chance has returned to normal.
        }

        private class DefenseTimer : Timer
        {
            private Mobile m_Player;

            public DefenseTimer(Mobile player)
                : base(DefenseEffectDuration)
            {
                m_Player = player;

                Priority = TimerPriority.TwoFiftyMS;

                Start();
            }

            protected override void OnTick()
            {
                RemoveDefense(m_Player);
            }
        }
    }

    public class SlayerEntry
    {
        private SlayerGroup m_Group;
        private SlayerName m_Name;
        private Type[] m_Types;

        public SlayerGroup Group { get { return m_Group; } set { m_Group = value; } }
        public SlayerName Name { get { return m_Name; } }
        public Type[] Types { get { return m_Types; } }

        private static int[] m_AosTitles = new int[]
			{
				1060479, // undead slayer
				1060470, // orc slayer
				1060480, // troll slayer
				1060468, // ogre slayer
				1060472, // repond slayer
				1060462, // dragon slayer
				1060478, // terathan slayer
				1060475, // snake slayer
				1060467, // lizardman slayer
				1060473, // reptile slayer
				1060460, // demon slayer
				1060466, // gargoyle slayer
				1017396, // Balron Damnation
				1060461, // demon slayer
				1060469, // ophidian slayer
				1060477, // spider slayer
				1060474, // scorpion slayer
				1060458, // arachnid slayer
				1060465, // fire elemental slayer
				1060481, // water elemental slayer
				1060457, // air elemental slayer
				1060471, // poison elemental slayer
				1060463, // earth elemental slayer
				1060459, // blood elemental slayer
				1060476, // snow elemental slayer
				1060464, // elemental slayer
				1070855  // fey slayer
			};

        private static int[] m_OldTitles = new int[]
			{
				1017384, // Silver
				1017385, // Orc Slaying
				1017386, // Troll Slaughter
				1017387, // Ogre Thrashing
				1017388, // Repond
				1017389, // Dragon Slaying
				1017390, // Terathan
				1017391, // Snake's Bane
				1017392, // Lizardman Slaughter
				1017393, // Reptilian Death
				1017394, // Daemon Dismissal
				1017395, // Gargoyle's Foe
				1017396, // Balron Damnation
				1017397, // Exorcism
				1017398, // Ophidian
				1017399, // Spider's Death
				1017400, // Scorpion's Bane
				1017401, // Arachnid Doom
				1017402, // Flame Dousing
				1017403, // Water Dissipation
				1017404, // Vacuum
				1017405, // Elemental Health
				1017406, // Earth Shatter
				1017407, // Blood Drinking
				1017408, // Summer Wind
				1017409, // Elemental Ban
				1070855  // fey slayer
			};

        public int Title
        {
            get
            {
                int[] titles = (Core.AOS ? m_AosTitles : m_OldTitles);

                return titles[(int)m_Name - 1];
            }
        }

        public SlayerEntry(SlayerName name, params Type[] types)
        {
            m_Name = name;
            m_Types = types;
        }

        public bool Slays(Mobile m)
        {
            Type t = m.GetType();

            for (int i = 0; i < m_Types.Length; ++i)
            {
                if (m_Types[i].IsAssignableFrom(t))
                    return true;
            }

            return false;
        }
    }

    public class SlayerGroup
    {
        private static SlayerEntry[] m_TotalEntries;
        private static SlayerGroup[] m_Groups;

        public static SlayerEntry[] TotalEntries
        {
            get { return m_TotalEntries; }
        }

        public static SlayerGroup[] Groups
        {
            get { return m_Groups; }
        }

        public static SlayerEntry GetEntryByName(SlayerName name)
        {
            int v = (int)name;

            if (v >= 0 && v < m_TotalEntries.Length)
                return m_TotalEntries[v];

            return null;
        }

        public static SlayerName GetLootSlayerType(Type type)
        {
            for (int i = 0; i < m_Groups.Length; ++i)
            {
                SlayerGroup group = m_Groups[i];
                Type[] foundOn = group.FoundOn;

                bool inGroup = false;

                for (int j = 0; foundOn != null && !inGroup && j < foundOn.Length; ++j)
                    inGroup = (foundOn[j] == type);

                if (inGroup)
                {
                    int index = Utility.Random(1 + group.Entries.Length);

                    if (index == 0)
                        return group.m_Super.Name;

                    return group.Entries[index - 1].Name;
                }
            }

            return SlayerName.Silver;
        }

        static SlayerGroup()
        {
            SlayerGroup humanoid = new SlayerGroup();
            SlayerGroup undead = new SlayerGroup();
            SlayerGroup elemental = new SlayerGroup();
            SlayerGroup abyss = new SlayerGroup();
            SlayerGroup arachnid = new SlayerGroup();
            SlayerGroup reptilian = new SlayerGroup();
            SlayerGroup fey = new SlayerGroup();

            humanoid.Opposition = new SlayerGroup[] { undead };
            humanoid.FoundOn = new Type[] { typeof(BoneKnight), typeof(Lich), typeof(LichLord) };
            humanoid.Super = new SlayerEntry(SlayerName.Repond, typeof(ArcticOgreLord), typeof(Cyclops), typeof(Ettin), typeof(EvilMage), typeof(EvilMageLord), typeof(FrostTroll), typeof(MeerCaptain), typeof(MeerEternal), typeof(MeerMage), typeof(MeerWarrior), typeof(Ogre), typeof(OgreLord), typeof(Orc), typeof(OrcBomber), typeof(OrcBrute), typeof(OrcCaptain), /*typeof( OrcChopper ), typeof( OrcScout ),*/ typeof(OrcishLord), typeof(OrcishMage), typeof(Ratman), typeof(RatmanArcher), typeof(RatmanMage), typeof(SavageRider), typeof(SavageShaman), typeof(Savage), typeof(Titan), typeof(Troglodyte), typeof(Troll));
            humanoid.Entries = new SlayerEntry[]
				{
					new SlayerEntry( SlayerName.OgreTrashing, typeof( Ogre ), typeof( OgreLord ), typeof( ArcticOgreLord ) ),
					new SlayerEntry( SlayerName.OrcSlaying, typeof( Orc ), typeof( OrcBomber ), typeof( OrcBrute ), typeof( OrcCaptain ),/* typeof( OrcChopper ), typeof( OrcScout ),*/ typeof( OrcishLord ), typeof( OrcishMage ) ),
					new SlayerEntry( SlayerName.TrollSlaughter, typeof( Troll ), typeof( FrostTroll ) )
				};

            undead.Opposition = new SlayerGroup[] { humanoid };
            undead.Super = new SlayerEntry(SlayerName.Silver, typeof(AncientLich), typeof(Bogle), typeof(BoneKnight), typeof(BoneMagi),/* typeof( DarkGuardian ), */typeof(DarknightCreeper), typeof(FleshGolem), typeof(Ghoul), typeof(GoreFiend), typeof(HellSteed), typeof(LadyOfTheSnow), typeof(Lich), typeof(LichLord), typeof(Mummy), typeof(PestilentBandage), typeof(Revenant), typeof(RevenantLion), typeof(RottingCorpse), typeof(Shade), typeof(ShadowKnight), typeof(SkeletalKnight), typeof(SkeletalMage), typeof(SkeletalMount), typeof(Skeleton), typeof(Spectre), typeof(Wraith), typeof(Zombie));
            undead.Entries = new SlayerEntry[0];

            fey.Opposition = new SlayerGroup[] { abyss };
            fey.Super = new SlayerEntry(SlayerName.Fey, typeof(Centaur), typeof(CuSidhe), typeof(EtherealWarrior), typeof(Kirin), typeof(LordOaks), typeof(Pixie), typeof(Silvani), typeof(Treefellow), typeof(Unicorn), typeof(Wisp), typeof(MLDryad), typeof(Satyr));
            fey.Entries = new SlayerEntry[0];

            elemental.Opposition = new SlayerGroup[] { abyss };
            elemental.FoundOn = new Type[] { typeof(Balron), typeof(Daemon) };
            elemental.Super = new SlayerEntry(SlayerName.ElementalBan, typeof(AcidElemental), typeof(AgapiteElemental), typeof(AirElemental), typeof(SummonedAirElemental), typeof(BloodElemental), typeof(BronzeElemental), typeof(CopperElemental), typeof(CrystalElemental), typeof(DullCopperElemental), typeof(EarthElemental), typeof(SummonedEarthElemental), typeof(Efreet), typeof(FireElemental), typeof(SummonedFireElemental), typeof(GoldenElemental), typeof(IceElemental), typeof(KazeKemono), typeof(PoisonElemental), typeof(RaiJu), typeof(SandVortex), typeof(ShadowIronElemental), typeof(SnowElemental), typeof(ValoriteElemental), typeof(VeriteElemental), typeof(WaterElemental), typeof(SummonedWaterElemental));
            elemental.Entries = new SlayerEntry[]
				{
					new SlayerEntry( SlayerName.BloodDrinking, typeof( BloodElemental ) ),
					new SlayerEntry( SlayerName.EarthShatter, typeof( AgapiteElemental ), typeof( BronzeElemental ), typeof( CopperElemental ), typeof( DullCopperElemental ), typeof( EarthElemental ), typeof( SummonedEarthElemental ), typeof( GoldenElemental ), typeof( ShadowIronElemental ), typeof( ValoriteElemental ), typeof( VeriteElemental ) ),
					new SlayerEntry( SlayerName.ElementalHealth, typeof( PoisonElemental ) ),
					new SlayerEntry( SlayerName.FlameDousing, typeof( FireElemental ), typeof( SummonedFireElemental ) ),
					new SlayerEntry( SlayerName.SummerWind, typeof( SnowElemental ), typeof( IceElemental ) ),
					new SlayerEntry( SlayerName.Vacuum, typeof( AirElemental ), typeof( SummonedAirElemental ) ),
					new SlayerEntry( SlayerName.WaterDissipation, typeof( WaterElemental ), typeof( SummonedWaterElemental ) )
				};

            abyss.Opposition = new SlayerGroup[] { elemental, fey };
            abyss.FoundOn = new Type[] { typeof(BloodElemental) };

            if (Core.AOS)
            {
                abyss.Super = new SlayerEntry(SlayerName.Exorcism, typeof(AbysmalHorror), typeof(ArcaneDaemon), typeof(Balron), typeof(BoneDemon), typeof(ChaosDaemon), typeof(Daemon), typeof(SummonedDaemon), typeof(DemonKnight), typeof(Devourer), typeof(EnslavedGargoyle), typeof(FanDancer), typeof(FireGargoyle), typeof(Gargoyle), typeof(GargoyleDestroyer), typeof(GargoyleEnforcer), typeof(Gibberling), typeof(HordeMinion), typeof(IceFiend), typeof(Imp), typeof(Impaler), typeof(Moloch), typeof(Oni), typeof(Ravager), typeof(Semidar), typeof(StoneGargoyle), typeof(Succubus), typeof(TsukiWolf));

                abyss.Entries = new SlayerEntry[]
					{
						// Daemon Dismissal & Balron Damnation have been removed and moved up to super slayer on OSI.
						new SlayerEntry( SlayerName.GargoylesFoe, typeof( EnslavedGargoyle ), typeof( FireGargoyle ), typeof( Gargoyle ), typeof( GargoyleDestroyer ), typeof( GargoyleEnforcer ), typeof( StoneGargoyle ) ),
					};
            }
            else
            {
                abyss.Super = new SlayerEntry(SlayerName.Exorcism, typeof(AbysmalHorror), typeof(Balron), typeof(BoneDemon), typeof(ChaosDaemon), typeof(Daemon), typeof(SummonedDaemon), typeof(DemonKnight), typeof(Devourer), typeof(Gargoyle), typeof(FireGargoyle), typeof(Gibberling), typeof(HordeMinion), typeof(IceFiend), typeof(Imp), typeof(Impaler), typeof(Ravager), typeof(StoneGargoyle), typeof(ArcaneDaemon), typeof(EnslavedGargoyle), typeof(GargoyleDestroyer), typeof(GargoyleEnforcer), typeof(Moloch));

                abyss.Entries = new SlayerEntry[]
					{
						new SlayerEntry( SlayerName.DaemonDismissal, typeof( AbysmalHorror ), typeof( Balron ), typeof( BoneDemon ), typeof( ChaosDaemon ), typeof( Daemon ), typeof( SummonedDaemon ), typeof( DemonKnight ), typeof( Devourer ), typeof( Gibberling ), typeof( HordeMinion ), typeof( IceFiend ), typeof( Imp ), typeof( Impaler ), typeof( Ravager ), typeof( ArcaneDaemon ), typeof( Moloch ) ),
						new SlayerEntry( SlayerName.GargoylesFoe, typeof( FireGargoyle ), typeof( Gargoyle ), typeof( StoneGargoyle ), typeof( EnslavedGargoyle ), typeof( GargoyleDestroyer ), typeof( GargoyleEnforcer ) ),
						new SlayerEntry( SlayerName.BalronDamnation, typeof( Balron ) )
					};
            }

            arachnid.Opposition = new SlayerGroup[] { reptilian };
            arachnid.FoundOn = new Type[] { typeof(AncientWyrm), typeof(GreaterDragon), typeof(Dragon), typeof(OphidianMatriarch), typeof(ShadowWyrm) };
            arachnid.Super = new SlayerEntry(SlayerName.ArachnidDoom, typeof(DreadSpider), typeof(FrostSpider), typeof(GiantBlackWidow), typeof(GiantSpider), typeof(Mephitis), typeof(Scorpion), typeof(TerathanAvenger), typeof(TerathanDrone), typeof(TerathanMatriarch), typeof(TerathanWarrior));
            arachnid.Entries = new SlayerEntry[]
				{
					new SlayerEntry( SlayerName.ScorpionsBane, typeof( Scorpion ) ),
					new SlayerEntry( SlayerName.SpidersDeath, typeof( DreadSpider ), typeof( FrostSpider ), typeof( GiantBlackWidow ), typeof( GiantSpider ), typeof( Mephitis ) ),
					new SlayerEntry( SlayerName.Terathan, typeof( TerathanAvenger ), typeof( TerathanDrone ), typeof( TerathanMatriarch ), typeof( TerathanWarrior ) )
				};

            reptilian.Opposition = new SlayerGroup[] { arachnid };
            reptilian.FoundOn = new Type[] { typeof(TerathanAvenger), typeof(TerathanMatriarch) };
            reptilian.Super = new SlayerEntry(SlayerName.ReptilianDeath, typeof(AncientWyrm), typeof(DeepSeaSerpent), typeof(GreaterDragon), typeof(Dragon), typeof(Drake), typeof(GiantIceWorm), typeof(IceSerpent), typeof(GiantSerpent), typeof(Hiryu), typeof(IceSnake), typeof(JukaLord), typeof(JukaMage), typeof(JukaWarrior), typeof(LavaSerpent), typeof(LavaSnake), typeof(LesserHiryu), typeof(Lizardman), typeof(OphidianArchmage), typeof(OphidianKnight), typeof(OphidianMage), typeof(OphidianMatriarch), typeof(OphidianWarrior), typeof(Reptalon), typeof(SeaSerpent), typeof(Serado), typeof(SerpentineDragon), typeof(ShadowWyrm), typeof(SilverSerpent), typeof(SkeletalDragon), typeof(Snake), typeof(SwampDragon), typeof(WhiteWyrm), typeof(Wyvern), typeof(Yamandon));
            reptilian.Entries = new SlayerEntry[]
				{
					new SlayerEntry( SlayerName.DragonSlaying, typeof( AncientWyrm ), typeof( GreaterDragon ), typeof( Dragon ), typeof( Drake ), typeof( Hiryu ), typeof( LesserHiryu ), typeof( Reptalon ), typeof( SerpentineDragon ), typeof( ShadowWyrm ), typeof( SkeletalDragon ), typeof( SwampDragon ), typeof( WhiteWyrm ), typeof( Wyvern ) ),
					new SlayerEntry( SlayerName.LizardmanSlaughter, typeof( Lizardman ) ),
					new SlayerEntry( SlayerName.Ophidian, typeof( OphidianArchmage ), typeof( OphidianKnight ), typeof( OphidianMage ), typeof( OphidianMatriarch ), typeof( OphidianWarrior ) ),
					new SlayerEntry( SlayerName.SnakesBane, typeof( DeepSeaSerpent ), typeof( GiantIceWorm ), typeof( GiantSerpent ), typeof( IceSerpent ), typeof( IceSnake ), typeof( LavaSerpent ), typeof( LavaSnake ), typeof( SeaSerpent ), typeof( Serado ), typeof( SilverSerpent ), typeof( Snake ), typeof( Yamandon ) )
				};

            m_Groups = new SlayerGroup[]
				{
					humanoid,
					undead,
					elemental,
					abyss,
					arachnid,
					reptilian,
					fey
				};

            m_TotalEntries = CompileEntries(m_Groups);
        }

        private static SlayerEntry[] CompileEntries(SlayerGroup[] groups)
        {
            SlayerEntry[] entries = new SlayerEntry[28];

            for (int i = 0; i < groups.Length; ++i)
            {
                SlayerGroup g = groups[i];

                g.Super.Group = g;

                entries[(int)g.Super.Name] = g.Super;

                for (int j = 0; j < g.Entries.Length; ++j)
                {
                    g.Entries[j].Group = g;
                    entries[(int)g.Entries[j].Name] = g.Entries[j];
                }
            }

            return entries;
        }

        private SlayerGroup[] m_Opposition;
        private SlayerEntry m_Super;
        private SlayerEntry[] m_Entries;
        private Type[] m_FoundOn;

        public SlayerGroup[] Opposition { get { return m_Opposition; } set { m_Opposition = value; } }
        public SlayerEntry Super { get { return m_Super; } set { m_Super = value; } }
        public SlayerEntry[] Entries { get { return m_Entries; } set { m_Entries = value; } }
        public Type[] FoundOn { get { return m_FoundOn; } set { m_FoundOn = value; } }

        public bool OppositionSuperSlays(Mobile m)
        {
            for (int i = 0; i < Opposition.Length; i++)
            {
                if (Opposition[i].Super.Slays(m))
                    return true;
            }

            return false;
        }

        public SlayerGroup()
        {
        }
    }

    #endregion

    /// <summary>
    /// Archery Weapons
    /// </summary>
    public abstract class BaseRanged : BaseMeleeWeapon
    {
        public abstract int EffectID { get; }
        public abstract Type AmmoType { get; }
        public abstract Item Ammo { get; }

        public override int DefHitSound { get { return 0x234; } }
        public override int DefMissSound { get { return 0x238; } }

        public override SkillName DefSkill { get { return SkillName.Archery; } }
        public override WeaponType DefType { get { return WeaponType.Ranged; } }
        public override WeaponAnimation DefAnimation { get { return WeaponAnimation.ShootXBow; } }

        public override SkillName AccuracySkill { get { return SkillName.Archery; } }

        private Timer m_RecoveryTimer; // so we don't start too many timers
        private bool m_Balanced;
        private int m_Velocity;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Balanced
        {
            get { return m_Balanced; }
            set { m_Balanced = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Velocity
        {
            get { return m_Velocity; }
            set { m_Velocity = value; InvalidateProperties(); }
        }

        public BaseRanged(int itemID)
            : base(itemID)
        {
        }

        public BaseRanged(Serial serial)
            : base(serial)
        {
        }

        public override TimeSpan OnSwing(Mobile attacker, Mobile defender)
        {
            WeaponAbility a = WeaponAbility.GetCurrentAbility(attacker);

            // Make sure we've been standing still for .25/.5/1 second depending on Era
            if (Core.TickCount - attacker.LastMoveTime >= (Core.SE ? 250 : Core.AOS ? 500 : 1000) || (Core.AOS && WeaponAbility.GetCurrentAbility(attacker) is MovingShot))
            {
                bool canSwing = true;

                if (Core.AOS)
                {
                    canSwing = (!attacker.Paralyzed && !attacker.Frozen);

                    if (canSwing)
                    {
                        Spell sp = attacker.Spell as Spell;

                        canSwing = (sp == null || !sp.IsCasting || !sp.BlocksMovement);
                    }
                }

                #region Dueling
                if (attacker is PlayerMobile)
                {
                    PlayerMobile pm = (PlayerMobile)attacker;

                    if (pm.DuelContext != null && !pm.DuelContext.CheckItemEquip(attacker, this))
                        canSwing = false;
                }
                #endregion

                if (canSwing && attacker.HarmfulCheck(defender))
                {
                    attacker.DisruptiveAction();
                    attacker.Send(new Swing(0, attacker, defender));

                    if (OnFired(attacker, defender))
                    {
                        if (CheckHit(attacker, defender))
                            OnHit(attacker, defender);
                        else
                            OnMiss(attacker, defender);
                    }
                }

                attacker.RevealingAction();

                return GetDelay(attacker);
            }
            else
            {
                attacker.RevealingAction();

                return TimeSpan.FromSeconds(0.25);
            }
        }

        public override void OnHit(Mobile attacker, Mobile defender, double damageBonus)
        {
            if (attacker.Player && !defender.Player && (defender.Body.IsAnimal || defender.Body.IsMonster) && 0.4 >= Utility.RandomDouble())
                defender.AddToBackpack(Ammo);

            if (Core.ML && m_Velocity > 0)
            {
                int bonus = (int)attacker.GetDistanceToSqrt(defender);

                if (bonus > 0 && m_Velocity > Utility.Random(100))
                {
                    AOS.Damage(defender, attacker, bonus * 3, 100, 0, 0, 0, 0);

                    if (attacker.Player)
                        attacker.SendLocalizedMessage(1072794); // Your arrow hits its mark with velocity!

                    if (defender.Player)
                        defender.SendLocalizedMessage(1072795); // You have been hit by an arrow with velocity!
                }
            }

            base.OnHit(attacker, defender, damageBonus);
        }

        public override void OnMiss(Mobile attacker, Mobile defender)
        {
            if (attacker.Player && 0.4 >= Utility.RandomDouble())
            {
                if (Core.SE)
                {
                    PlayerMobile p = attacker as PlayerMobile;

                    if (p != null)
                    {
                        Type ammo = AmmoType;

                        if (p.RecoverableAmmo.ContainsKey(ammo))
                            p.RecoverableAmmo[ammo]++;
                        else
                            p.RecoverableAmmo.Add(ammo, 1);

                        if (!p.Warmode)
                        {
                            if (m_RecoveryTimer == null)
                                m_RecoveryTimer = Timer.DelayCall(TimeSpan.FromSeconds(10), new TimerCallback(p.RecoverAmmo));

                            if (!m_RecoveryTimer.Running)
                                m_RecoveryTimer.Start();
                        }
                    }
                }
                else
                {
                    Ammo.MoveToWorld(new Point3D(defender.X + Utility.RandomMinMax(-1, 1), defender.Y + Utility.RandomMinMax(-1, 1), defender.Z), defender.Map);
                }
            }

            base.OnMiss(attacker, defender);
        }

        public virtual bool OnFired(Mobile attacker, Mobile defender)
        {
            if (attacker.Player)
            {
                BaseQuiver quiver = attacker.FindItemOnLayer(Layer.Cloak) as BaseQuiver;
                Container pack = attacker.Backpack;

                if (quiver == null || Utility.Random(100) >= quiver.LowerAmmoCost)
                {
                    // consume ammo
                    if (quiver != null && quiver.ConsumeTotal(AmmoType, 1))
                        quiver.InvalidateWeight();
                    else if (pack == null || !pack.ConsumeTotal(AmmoType, 1))
                        return false;
                }
                else if (quiver.FindItemByType(AmmoType) == null && (pack == null || pack.FindItemByType(AmmoType) == null))
                {
                    // lower ammo cost should not work when we have no ammo at all
                    return false;
                }
            }

            attacker.MovingEffect(defender, EffectID, 18, 1, false, false);

            return true;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)3); // version

            writer.Write((bool)m_Balanced);
            writer.Write((int)m_Velocity);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 3:
                    {
                        m_Balanced = reader.ReadBool();
                        m_Velocity = reader.ReadInt();

                        goto case 2;
                    }
                case 2:
                case 1:
                    {
                        break;
                    }
                case 0:
                    {
                        /*m_EffectID =*/
                        reader.ReadInt();
                        break;
                    }
            }

            if (version < 2)
            {
                WeaponAttributes.MageWeapon = 0;
                WeaponAttributes.UseBestSkill = 0;
            }
        }
    }

    /// <summary>
    /// Defensive Traps
    /// </summary>
    public abstract class BaseTrap : Item
    {
        public virtual bool PassivelyTriggered { get { return false; } }
        public virtual TimeSpan PassiveTriggerDelay { get { return TimeSpan.Zero; } }
        public virtual int PassiveTriggerRange { get { return -1; } }
        public virtual TimeSpan ResetDelay { get { return TimeSpan.Zero; } }

        private DateTime m_NextPassiveTrigger, m_NextActiveTrigger;

        public virtual void OnTrigger(Mobile from)
        {
        }

        public override bool HandlesOnMovement { get { return true; } } // Tell the core that we implement OnMovement

        public virtual int GetEffectHue()
        {
            int hue = this.Hue & 0x3FFF;

            if (hue < 2)
                return 0;

            return hue - 1;
        }

        public bool CheckRange(Point3D loc, Point3D oldLoc, int range)
        {
            return CheckRange(loc, range) && !CheckRange(oldLoc, range);
        }

        public bool CheckRange(Point3D loc, int range)
        {
            return ((this.Z + 8) >= loc.Z && (loc.Z + 16) > this.Z)
                && Utility.InRange(GetWorldLocation(), loc, range);
        }

        public override void OnMovement(Mobile m, Point3D oldLocation)
        {
            base.OnMovement(m, oldLocation);

            if (m.Location == oldLocation)
                return;

            if (CheckRange(m.Location, oldLocation, 0) && DateTime.UtcNow >= m_NextActiveTrigger)
            {
                m_NextActiveTrigger = m_NextPassiveTrigger = DateTime.UtcNow + ResetDelay;

                OnTrigger(m);
            }
            else if (PassivelyTriggered && CheckRange(m.Location, oldLocation, PassiveTriggerRange) && DateTime.UtcNow >= m_NextPassiveTrigger)
            {
                m_NextPassiveTrigger = DateTime.UtcNow + PassiveTriggerDelay;

                OnTrigger(m);
            }
        }

        public BaseTrap(int itemID)
            : base(itemID)
        {
            Movable = false;
        }

        public BaseTrap(Serial serial)
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

    /// <summary>
    /// Fencing Weapons
    /// </summary>
    public abstract class BaseSpear : BaseMeleeWeapon
    {
        public override int DefHitSound { get { return 0x23C; } }
        public override int DefMissSound { get { return 0x238; } }

        public override SkillName DefSkill { get { return SkillName.Fencing; } }
        public override WeaponType DefType { get { return WeaponType.Piercing; } }
        public override WeaponAnimation DefAnimation { get { return WeaponAnimation.Pierce2H; } }

        public BaseSpear(int itemID)
            : base(itemID)
        {
        }

        public BaseSpear(Serial serial)
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

        public override void OnHit(Mobile attacker, Mobile defender, double damageBonus)
        {
            base.OnHit(attacker, defender, damageBonus);

            if (!Core.AOS && Layer == Layer.TwoHanded && (attacker.Skills[SkillName.Anatomy].Value / 400.0) >= Utility.RandomDouble() && Engines.ConPVP.DuelContext.AllowSpecialAbility(attacker, "Paralyzing Blow", false))
            {
                defender.SendMessage("You receive a paralyzing blow!"); // Is this not localized?
                defender.Freeze(TimeSpan.FromSeconds(2.0));

                attacker.SendMessage("You deliver a paralyzing blow!"); // Is this not localized?
                attacker.PlaySound(0x11C);
            }

            if (!Core.AOS && Poison != null && PoisonCharges > 0)
            {
                --PoisonCharges;

                if (Utility.RandomDouble() >= 0.5) // 50% chance to poison
                    defender.ApplyPoison(attacker, Poison);
            }
        }
    }

    /// <summary>
    /// MaceFighting Weapons
    /// </summary>
    public abstract class BaseBashing : BaseMeleeWeapon
    {
        public override int DefHitSound { get { return 0x233; } }
        public override int DefMissSound { get { return 0x239; } }

        public override SkillName DefSkill { get { return SkillName.Macing; } }
        public override WeaponType DefType { get { return WeaponType.Bashing; } }
        public override WeaponAnimation DefAnimation { get { return WeaponAnimation.Bash1H; } }

        public BaseBashing(int itemID)
            : base(itemID)
        {
        }

        public BaseBashing(Serial serial)
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

        public override void OnHit(Mobile attacker, Mobile defender, double damageBonus)
        {
            base.OnHit(attacker, defender, damageBonus);

            defender.Stam -= Utility.Random(3, 3); // 3-5 points of stamina loss
        }

        public override double GetBaseDamage(Mobile attacker)
        {
            double damage = base.GetBaseDamage(attacker);

            if (!Core.AOS && (attacker.Player || attacker.Body.IsHuman) && Layer == Layer.TwoHanded && attacker.Skills[SkillName.Anatomy].Value >= 80 && (attacker.Skills[SkillName.Anatomy].Value / 400.0) >= Utility.RandomDouble() && Engines.ConPVP.DuelContext.AllowSpecialAbility(attacker, "Crushing Blow", false))
            {
                damage *= 1.5;

                attacker.SendMessage("You deliver a crushing blow!"); // Is this not localized?
                attacker.PlaySound(0x11C);
            }

            return damage;
        }
    }

    public abstract class BaseStaff : BaseMeleeWeapon
    {
        public override int DefHitSound { get { return 0x233; } }
        public override int DefMissSound { get { return 0x239; } }

        public override SkillName DefSkill { get { return SkillName.Macing; } }
        public override WeaponType DefType { get { return WeaponType.Staff; } }
        public override WeaponAnimation DefAnimation { get { return WeaponAnimation.Bash2H; } }

        public BaseStaff(int itemID)
            : base(itemID)
        {
        }

        public BaseStaff(Serial serial)
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

        public override void OnHit(Mobile attacker, Mobile defender, double damageBonus)
        {
            base.OnHit(attacker, defender, damageBonus);

            defender.Stam -= Utility.Random(3, 3); // 3-5 points of stamina loss
        }
    }

    /// <summary>
    /// Magic Weapons
    /// </summary>
    public enum WandEffect
    {
        Clumsiness,
        Identification,
        Healing,
        Feeblemindedness,
        Weakness,
        MagicArrow,
        Harming,
        Fireball,
        GreaterHealing,
        Lightning,
        ManaDraining
    }

    public abstract class BaseWand : BaseBashing, ITokunoDyable
    {
        public override WeaponAbility PrimaryAbility { get { return WeaponAbility.Dismount; } }
        public override WeaponAbility SecondaryAbility { get { return WeaponAbility.Disarm; } }

        public override int AosStrengthReq { get { return 5; } }
        public override int AosMinDamage { get { return 9; } }
        public override int AosMaxDamage { get { return 11; } }
        public override int AosSpeed { get { return 40; } }
        public override float MlSpeed { get { return 2.75f; } }

        public override int OldStrengthReq { get { return 0; } }
        public override int OldMinDamage { get { return 2; } }
        public override int OldMaxDamage { get { return 6; } }
        public override int OldSpeed { get { return 35; } }

        public override int InitMinHits { get { return 31; } }
        public override int InitMaxHits { get { return 110; } }

        private WandEffect m_WandEffect;
        private int m_Charges;

        public virtual TimeSpan GetUseDelay { get { return TimeSpan.FromSeconds(4.0); } }

        [CommandProperty(AccessLevel.GameMaster)]
        public WandEffect Effect
        {
            get { return m_WandEffect; }
            set { m_WandEffect = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Charges
        {
            get { return m_Charges; }
            set { m_Charges = value; InvalidateProperties(); }
        }

        public BaseWand(WandEffect effect, int minCharges, int maxCharges)
            : base(Utility.RandomList(0xDF2, 0xDF3, 0xDF4, 0xDF5))
        {
            Weight = 1.0;
            Effect = effect;
            Charges = Utility.RandomMinMax(minCharges, maxCharges);
            Attributes.SpellChanneling = 1;
            Attributes.CastSpeed = -1;
            WeaponAttributes.MageWeapon = Utility.RandomMinMax(1, 10);
        }

        public void ConsumeCharge(Mobile from)
        {
            --Charges;

            if (Charges == 0)
                from.SendLocalizedMessage(1019073); // This item is out of charges.

            ApplyDelayTo(from);
        }

        public BaseWand(Serial serial)
            : base(serial)
        {
        }

        public virtual void ApplyDelayTo(Mobile from)
        {
            from.BeginAction(typeof(BaseWand));
            Timer.DelayCall<Mobile>(GetUseDelay, new TimerStateCallback<Mobile>(ReleaseWandLock_Callback), from);
        }

        public virtual void ReleaseWandLock_Callback(Mobile state)
        {
            state.EndAction(typeof(BaseWand));
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.CanBeginAction(typeof(BaseWand)))
            {
                from.SendLocalizedMessage(1070860); // You must wait a moment for the wand to recharge.
                return;
            }

            if (Parent == from)
            {
                if (Charges > 0)
                    OnWandUse(from);
                else
                    from.SendLocalizedMessage(1019073); // This item is out of charges.
            }
            else
            {
                from.SendLocalizedMessage(502641); // You must equip this item to use it.
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write((int)m_WandEffect);
            writer.Write((int)m_Charges);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_WandEffect = (WandEffect)reader.ReadInt();
                        m_Charges = (int)reader.ReadInt();

                        break;
                    }
            }
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            switch (m_WandEffect)
            {
                case WandEffect.Clumsiness: list.Add(1017326, m_Charges.ToString()); break; // clumsiness charges: ~1_val~
                case WandEffect.Identification: list.Add(1017350, m_Charges.ToString()); break; // identification charges: ~1_val~
                case WandEffect.Healing: list.Add(1017329, m_Charges.ToString()); break; // healing charges: ~1_val~
                case WandEffect.Feeblemindedness: list.Add(1017327, m_Charges.ToString()); break; // feeblemind charges: ~1_val~
                case WandEffect.Weakness: list.Add(1017328, m_Charges.ToString()); break; // weakness charges: ~1_val~
                case WandEffect.MagicArrow: list.Add(1060492, m_Charges.ToString()); break; // magic arrow charges: ~1_val~
                case WandEffect.Harming: list.Add(1017334, m_Charges.ToString()); break; // harm charges: ~1_val~
                case WandEffect.Fireball: list.Add(1060487, m_Charges.ToString()); break; // fireball charges: ~1_val~
                case WandEffect.GreaterHealing: list.Add(1017330, m_Charges.ToString()); break; // greater healing charges: ~1_val~
                case WandEffect.Lightning: list.Add(1060491, m_Charges.ToString()); break; // lightning charges: ~1_val~
                case WandEffect.ManaDraining: list.Add(1017339, m_Charges.ToString()); break; // mana drain charges: ~1_val~
            }
        }

        public override void OnSingleClick(Mobile from)
        {
            ArrayList attrs = new ArrayList();

            if (DisplayLootType)
            {
                if (LootType == LootType.Blessed)
                    attrs.Add(new EquipInfoAttribute(1038021)); // blessed
                else if (LootType == LootType.Cursed)
                    attrs.Add(new EquipInfoAttribute(1049643)); // cursed
            }

            if (!Identified)
            {
                attrs.Add(new EquipInfoAttribute(1038000)); // Unidentified
            }
            else
            {
                int num = 0;

                switch (m_WandEffect)
                {
                    case WandEffect.Clumsiness: num = 3002011; break;
                    case WandEffect.Identification: num = 1044063; break;
                    case WandEffect.Healing: num = 3002014; break;
                    case WandEffect.Feeblemindedness: num = 3002013; break;
                    case WandEffect.Weakness: num = 3002018; break;
                    case WandEffect.MagicArrow: num = 3002015; break;
                    case WandEffect.Harming: num = 3002022; break;
                    case WandEffect.Fireball: num = 3002028; break;
                    case WandEffect.GreaterHealing: num = 3002039; break;
                    case WandEffect.Lightning: num = 3002040; break;
                    case WandEffect.ManaDraining: num = 3002041; break;
                }

                if (num > 0)
                    attrs.Add(new EquipInfoAttribute(num, m_Charges));
            }

            int number;

            if (Name == null)
            {
                number = 1017085;
            }
            else
            {
                this.LabelTo(from, Name);
                number = 1041000;
            }

            if (attrs.Count == 0 && Crafter == null && Name != null)
                return;

            EquipmentInfo eqInfo = new EquipmentInfo(number, Crafter, false, (EquipInfoAttribute[])attrs.ToArray(typeof(EquipInfoAttribute)));

            from.Send(new DisplayEquipmentInfo(this, eqInfo));
        }

        public void Cast(Spell spell)
        {
            bool m = Movable;

            Movable = false;
            spell.Cast();
            Movable = m;
        }

        public virtual void OnWandUse(Mobile from)
        {
            from.Target = new WandTarget(this);
        }

        public virtual void DoWandTarget(Mobile from, object o)
        {
            if (Deleted || Charges <= 0 || Parent != from || o is StaticTarget || o is LandTarget)
                return;

            if (OnWandTarget(from, o))
                ConsumeCharge(from);
        }

        public virtual bool OnWandTarget(Mobile from, object o)
        {
            return true;
        }
    }

    /// <summary>
    /// Swordsmanship Weapons
    /// </summary>
    public interface IAxe
    {
        bool Axe(Mobile from, BaseAxe axe);
    }

    public abstract class BaseAxe : BaseMeleeWeapon
    {
        public override int DefHitSound { get { return 0x232; } }
        public override int DefMissSound { get { return 0x23A; } }

        public override SkillName DefSkill { get { return SkillName.Swords; } }
        public override WeaponType DefType { get { return WeaponType.Axe; } }
        public override WeaponAnimation DefAnimation { get { return WeaponAnimation.Slash2H; } }

        public virtual HarvestSystem HarvestSystem { get { return Lumberjacking.System; } }

        private int m_UsesRemaining;
        private bool m_ShowUsesRemaining;

        [CommandProperty(AccessLevel.GameMaster)]
        public int UsesRemaining
        {
            get { return m_UsesRemaining; }
            set { m_UsesRemaining = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ShowUsesRemaining
        {
            get { return m_ShowUsesRemaining; }
            set { m_ShowUsesRemaining = value; InvalidateProperties(); }
        }

        public virtual int GetUsesScalar()
        {
            if (Quality == WeaponQuality.Exceptional)
                return 200;

            return 100;
        }

        public override void UnscaleDurability()
        {
            base.UnscaleDurability();

            int scale = GetUsesScalar();

            m_UsesRemaining = ((m_UsesRemaining * 100) + (scale - 1)) / scale;
            InvalidateProperties();
        }

        public override void ScaleDurability()
        {
            base.ScaleDurability();

            int scale = GetUsesScalar();

            m_UsesRemaining = ((m_UsesRemaining * scale) + 99) / 100;
            InvalidateProperties();
        }

        public BaseAxe(int itemID)
            : base(itemID)
        {
            m_UsesRemaining = 150;
        }

        public BaseAxe(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (HarvestSystem == null || Deleted)
                return;

            Point3D loc = GetWorldLocation();

            if (!from.InLOS(loc) || !from.InRange(loc, 2))
            {
                from.LocalOverheadMessage(Server.Network.MessageType.Regular, 0x3E9, 1019045); // I can't reach that
                return;
            }
            else if (!this.IsAccessibleTo(from))
            {
                this.PublicOverheadMessage(MessageType.Regular, 0x3E9, 1061637); // You are not allowed to access this.
                return;
            }

            if (!(this.HarvestSystem is Mining))
                from.SendLocalizedMessage(1010018); // What do you want to use this item on?

            HarvestSystem.BeginHarvesting(from, this);
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);

            if (HarvestSystem != null)
                BaseHarvestTool.AddContextMenuEntries(from, this, list, HarvestSystem);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)2); // version

            writer.Write((bool)m_ShowUsesRemaining);

            writer.Write((int)m_UsesRemaining);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 2:
                    {
                        m_ShowUsesRemaining = reader.ReadBool();
                        goto case 1;
                    }
                case 1:
                    {
                        m_UsesRemaining = reader.ReadInt();
                        goto case 0;
                    }
                case 0:
                    {
                        if (m_UsesRemaining < 1)
                            m_UsesRemaining = 150;

                        break;
                    }
            }
        }

        public override void OnHit(Mobile attacker, Mobile defender, double damageBonus)
        {
            base.OnHit(attacker, defender, damageBonus);

            if (!Core.AOS && (attacker.Player || attacker.Body.IsHuman) && Layer == Layer.TwoHanded && attacker.Skills[SkillName.Anatomy].Value >= 80 && (attacker.Skills[SkillName.Anatomy].Value / 400.0) >= Utility.RandomDouble() && Engines.ConPVP.DuelContext.AllowSpecialAbility(attacker, "Concussion Blow", false))
            {
                StatMod mod = defender.GetStatMod("Concussion");

                if (mod == null)
                {
                    defender.SendMessage("You receive a concussion blow!");
                    defender.AddStatMod(new StatMod(StatType.Int, "Concussion", -(defender.RawInt / 2), TimeSpan.FromSeconds(30.0)));

                    attacker.SendMessage("You deliver a concussion blow!");
                    attacker.PlaySound(0x308);
                }
            }
        }
    }

    public abstract class BaseKnife : BaseMeleeWeapon
    {
        public override int DefHitSound { get { return 0x23B; } }
        public override int DefMissSound { get { return 0x238; } }

        public override SkillName DefSkill { get { return SkillName.Swords; } }
        public override WeaponType DefType { get { return WeaponType.Slashing; } }
        public override WeaponAnimation DefAnimation { get { return WeaponAnimation.Slash1H; } }

        public BaseKnife(int itemID)
            : base(itemID)
        {
        }

        public BaseKnife(Serial serial)
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

        public override void OnDoubleClick(Mobile from)
        {
            from.SendLocalizedMessage(1010018); // What do you want to use this item on?

            from.Target = new BladedItemTarget(this);
        }

        public override void OnHit(Mobile attacker, Mobile defender, double damageBonus)
        {
            base.OnHit(attacker, defender, damageBonus);

            if (!Core.AOS && Poison != null && PoisonCharges > 0)
            {
                --PoisonCharges;

                if (Utility.RandomDouble() >= 0.5) // 50% chance to poison
                    defender.ApplyPoison(attacker, Poison);
            }
        }
    }

    public abstract class BasePoleArm : BaseMeleeWeapon, IUsesRemaining
    {
        public override int DefHitSound { get { return 0x237; } }
        public override int DefMissSound { get { return 0x238; } }

        public override SkillName DefSkill { get { return SkillName.Swords; } }
        public override WeaponType DefType { get { return WeaponType.Polearm; } }
        public override WeaponAnimation DefAnimation { get { return WeaponAnimation.Slash2H; } }

        public virtual HarvestSystem HarvestSystem { get { return Lumberjacking.System; } }

        private int m_UsesRemaining;
        private bool m_ShowUsesRemaining;

        [CommandProperty(AccessLevel.GameMaster)]
        public int UsesRemaining
        {
            get { return m_UsesRemaining; }
            set { m_UsesRemaining = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ShowUsesRemaining
        {
            get { return m_ShowUsesRemaining; }
            set { m_ShowUsesRemaining = value; InvalidateProperties(); }
        }

        public BasePoleArm(int itemID)
            : base(itemID)
        {
            m_UsesRemaining = 150;
        }

        public BasePoleArm(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (HarvestSystem == null)
                return;

            if (IsChildOf(from.Backpack) || Parent == from)
                HarvestSystem.BeginHarvesting(from, this);
            else
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);

            if (HarvestSystem != null)
                BaseHarvestTool.AddContextMenuEntries(from, this, list, HarvestSystem);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)2); // version

            writer.Write((bool)m_ShowUsesRemaining);

            writer.Write((int)m_UsesRemaining);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 2:
                    {
                        m_ShowUsesRemaining = reader.ReadBool();
                        goto case 1;
                    }
                case 1:
                    {
                        m_UsesRemaining = reader.ReadInt();
                        goto case 0;
                    }
                case 0:
                    {
                        if (m_UsesRemaining < 1)
                            m_UsesRemaining = 150;

                        break;
                    }
            }
        }

        public override void OnHit(Mobile attacker, Mobile defender, double damageBonus)
        {
            base.OnHit(attacker, defender, damageBonus);

            if (!Core.AOS && (attacker.Player || attacker.Body.IsHuman) && Layer == Layer.TwoHanded && attacker.Skills[SkillName.Anatomy].Value >= 80 && (attacker.Skills[SkillName.Anatomy].Value / 400.0) >= Utility.RandomDouble() && Engines.ConPVP.DuelContext.AllowSpecialAbility(attacker, "Concussion Blow", false))
            {
                StatMod mod = defender.GetStatMod("Concussion");

                if (mod == null)
                {
                    defender.SendMessage("You receive a concussion blow!");
                    defender.AddStatMod(new StatMod(StatType.Int, "Concussion", -(defender.RawInt / 2), TimeSpan.FromSeconds(30.0)));

                    attacker.SendMessage("You deliver a concussion blow!");
                    attacker.PlaySound(0x11C);
                }
            }
        }
    }

    public abstract class BaseSword : BaseMeleeWeapon
    {
        public override SkillName DefSkill { get { return SkillName.Swords; } }
        public override WeaponType DefType { get { return WeaponType.Slashing; } }
        public override WeaponAnimation DefAnimation { get { return WeaponAnimation.Slash1H; } }

        public BaseSword(int itemID)
            : base(itemID)
        {
        }

        public BaseSword(Serial serial)
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

        public override void OnDoubleClick(Mobile from)
        {
            from.SendLocalizedMessage(1010018); // What do you want to use this item on?

            from.Target = new BladedItemTarget(this);
        }

        public override void OnHit(Mobile attacker, Mobile defender, double damageBonus)
        {
            base.OnHit(attacker, defender, damageBonus);

            if (!Core.AOS && Poison != null && PoisonCharges > 0)
            {
                --PoisonCharges;

                if (Utility.RandomDouble() >= 0.5) // 50% chance to poison
                    defender.ApplyPoison(attacker, Poison);
            }
        }
    }

    /// <summary>
    /// Ninja Weapons
    /// </summary>
    public interface INinjaAmmo : IUsesRemaining
    {
        int PoisonCharges { get; set; }
        Poison Poison { get; set; }
    }

    public interface INinjaWeapon : IUsesRemaining
    {
        int NoFreeHandMessage { get; }
        int EmptyWeaponMessage { get; }
        int RecentlyUsedMessage { get; }
        int FullWeaponMessage { get; }
        int WrongAmmoMessage { get; }
        Type AmmoType { get; }
        int PoisonCharges { get; set; }
        Poison Poison { get; set; }
        int WeaponDamage { get; }
        int WeaponMinRange { get; }
        int WeaponMaxRange { get; }

        void AttackAnimation(Mobile from, Mobile to);
    }

    public class NinjaWeapon
    {
        private const int MaxUses = 10;

        public static void AttemptShoot(PlayerMobile from, INinjaWeapon weapon)
        {
            if (CanUseWeapon(from, weapon))
            {
                from.BeginTarget(weapon.WeaponMaxRange, false, TargetFlags.Harmful, new TargetStateCallback<INinjaWeapon>(OnTarget), weapon);
            }
        }

        private static void Shoot(PlayerMobile from, Mobile target, INinjaWeapon weapon)
        {
            if (from != target && CanUseWeapon(from, weapon) && from.CanBeHarmful(target))
            {
                if (weapon.WeaponMinRange == 0 || !from.InRange(target, weapon.WeaponMinRange))
                {
                    from.NinjaWepCooldown = true;

                    from.Direction = from.GetDirectionTo(target);

                    from.RevealingAction();

                    weapon.AttackAnimation(from, target);

                    ConsumeUse(weapon);

                    if (CombatCheck(from, target))
                    {
                        Timer.DelayCall(TimeSpan.FromSeconds(1.0), new TimerStateCallback<object[]>(OnHit), new object[] { from, target, weapon });
                    }

                    Timer.DelayCall(TimeSpan.FromSeconds(2.5), new TimerStateCallback<PlayerMobile>(ResetUsing), from);
                }
                else
                {
                    from.SendLocalizedMessage(1063303); // Your target is too close!
                }
            }
        }

        private static void ResetUsing(PlayerMobile from)
        {
            from.NinjaWepCooldown = false;
        }

        private static void Unload(Mobile from, INinjaWeapon weapon)
        {
            if (weapon.UsesRemaining > 0)
            {
                INinjaAmmo ammo = Activator.CreateInstance(weapon.AmmoType, new object[] { weapon.UsesRemaining }) as INinjaAmmo;

                ammo.Poison = weapon.Poison;
                ammo.PoisonCharges = weapon.PoisonCharges;

                from.AddToBackpack((Item)ammo);

                weapon.UsesRemaining = 0;
                weapon.PoisonCharges = 0;
                weapon.Poison = null;
            }
        }

        private static void Reload(PlayerMobile from, INinjaWeapon weapon, INinjaAmmo ammo)
        {
            if (weapon.UsesRemaining < MaxUses)
            {
                int need = Math.Min((MaxUses - weapon.UsesRemaining), ammo.UsesRemaining);

                if (need > 0)
                {
                    if (weapon.Poison != null && (ammo.Poison == null || weapon.Poison.Level > ammo.Poison.Level))
                    {
                        from.SendLocalizedMessage(1070767); // Loaded projectile is stronger, unload it first
                    }
                    else
                    {
                        if (weapon.UsesRemaining > 0)
                        {
                            if ((weapon.Poison == null && ammo.Poison != null)
                                || ((weapon.Poison != null && ammo.Poison != null) && weapon.Poison.Level != ammo.Poison.Level))
                            {
                                Unload(from, weapon);
                                need = Math.Min(MaxUses, ammo.UsesRemaining);
                            }
                        }
                        int poisonneeded = Math.Min((MaxUses - weapon.PoisonCharges), ammo.PoisonCharges);

                        weapon.UsesRemaining += need;
                        weapon.PoisonCharges += poisonneeded;

                        if (weapon.PoisonCharges > 0)
                        {
                            weapon.Poison = ammo.Poison;
                        }

                        ammo.PoisonCharges -= poisonneeded;
                        ammo.UsesRemaining -= need;

                        if (ammo.UsesRemaining < 1)
                        {
                            ((Item)ammo).Delete();
                        }
                        else if (ammo.PoisonCharges < 1)
                        {
                            ammo.Poison = null;
                        }
                    }
                } // "else" here would mean they targeted "ammo" with 0 uses.  undefined behavior.
            }
            else
            {
                from.SendLocalizedMessage(weapon.FullWeaponMessage);
            }
        }

        private static void ConsumeUse(INinjaWeapon weapon)
        {
            if (weapon.UsesRemaining > 0)
            {
                weapon.UsesRemaining--;

                if (weapon.UsesRemaining < 1)
                {
                    weapon.PoisonCharges = 0;
                    weapon.Poison = null;
                }
            }
        }

        private static bool CanUseWeapon(PlayerMobile from, INinjaWeapon weapon)
        {
            if (WeaponIsValid(weapon, from))
            {
                if (weapon.UsesRemaining > 0)
                {
                    if (!from.NinjaWepCooldown)
                    {
                        if (BasePotion.HasFreeHand(from))
                        {
                            return true;
                        }
                        else
                        {
                            from.SendLocalizedMessage(weapon.NoFreeHandMessage);
                        }
                    }
                    else
                    {
                        from.SendLocalizedMessage(weapon.RecentlyUsedMessage);
                    }
                }
                else
                {
                    from.SendLocalizedMessage(weapon.EmptyWeaponMessage);
                }
            }
            return false;
        }

        private static bool CombatCheck(Mobile attacker, Mobile defender) /* mod'd from baseweapon */
        {
            BaseWeapon defWeapon = defender.Weapon as BaseWeapon;

            Skill atkSkill = defender.Skills.Ninjitsu;
            Skill defSkill = defender.Skills[defWeapon.Skill];

            double atSkillValue = attacker.Skills.Ninjitsu.Value;
            double defSkillValue = defWeapon.GetDefendSkillValue(attacker, defender);

            double attackValue = AosAttributes.GetValue(attacker, AosAttribute.AttackChance);

            if (defSkillValue <= -20.0)
            {
                defSkillValue = -19.9;
            }

            if (Spells.Chivalry.DivineFurySpell.UnderEffect(attacker))
            {
                attackValue += 10;
            }

            if (AnimalForm.UnderTransformation(attacker, typeof(GreyWolf)) || AnimalForm.UnderTransformation(attacker, typeof(BakeKitsune)))
            {
                attackValue += 20;
            }

            if (HitLower.IsUnderAttackEffect(attacker))
            {
                attackValue -= 25;
            }

            if (attackValue > 45)
            {
                attackValue = 45;
            }

            attackValue = (atSkillValue + 20.0) * (100 + attackValue);

            double defenseValue = AosAttributes.GetValue(defender, AosAttribute.DefendChance);

            if (Spells.Chivalry.DivineFurySpell.UnderEffect(defender))
            {
                defenseValue -= 20;
            }

            if (HitLower.IsUnderDefenseEffect(defender))
            {
                defenseValue -= 25;
            }

            int refBonus = 0;

            if (Block.GetBonus(defender, ref refBonus))
            {
                defenseValue += refBonus;
            }

            if (SkillHandlers.Discordance.GetEffect(attacker, ref refBonus))
            {
                defenseValue -= refBonus;
            }

            if (defenseValue > 45)
            {
                defenseValue = 45;
            }

            defenseValue = (defSkillValue + 20.0) * (100 + defenseValue);

            double chance = attackValue / (defenseValue * 2.0);

            if (chance < 0.02)
            {
                chance = 0.02;
            }

            return attacker.CheckSkill(atkSkill.SkillName, chance);
        }

        private static void OnHit(object[] states)
        {
            Mobile from = states[0] as Mobile;
            Mobile target = states[1] as Mobile;
            INinjaWeapon weapon = states[2] as INinjaWeapon;

            if (from.CanBeHarmful(target))
            {
                from.DoHarmful(target);

                AOS.Damage(target, from, weapon.WeaponDamage, 100, 0, 0, 0, 0);

                if (weapon.Poison != null && weapon.PoisonCharges > 0)
                {
                    if (EvilOmenSpell.TryEndEffect(target))
                    {
                        target.ApplyPoison(from, Poison.GetPoison(weapon.Poison.Level + 1));
                    }
                    else
                    {
                        target.ApplyPoison(from, weapon.Poison);
                    }

                    weapon.PoisonCharges--;

                    if (weapon.PoisonCharges < 1)
                    {
                        weapon.Poison = null;
                    }
                }
            }
        }

        private static void OnTarget(Mobile from, object targeted, INinjaWeapon weapon)
        {
            PlayerMobile player = from as PlayerMobile;

            if (WeaponIsValid(weapon, from))
            {
                if (targeted is Mobile)
                {
                    Shoot(player, (Mobile)targeted, weapon);
                }
                else if (targeted.GetType() == weapon.AmmoType)
                {
                    Reload(player, weapon, (INinjaAmmo)targeted);
                }
                else
                {
                    player.SendLocalizedMessage(weapon.WrongAmmoMessage);
                }
            }
        }

        private static bool WeaponIsValid(INinjaWeapon weapon, Mobile from)
        {
            Item item = weapon as Item;

            if (!item.Deleted && item.RootParent == from)
            {
                return true;
            }
            return false;
        }

        public class LoadEntry : ContextMenuEntry
        {
            private INinjaWeapon weapon;

            public LoadEntry(INinjaWeapon wep, int entry)
                : base(entry, 0)
            {
                weapon = wep;
            }

            public override void OnClick()
            {
                if (WeaponIsValid(weapon, Owner.From))
                {
                    Owner.From.BeginTarget(10, false, TargetFlags.Harmful, new TargetStateCallback<INinjaWeapon>(OnTarget), weapon);
                }
            }
        }

        public class UnloadEntry : ContextMenuEntry
        {
            private INinjaWeapon weapon;

            public UnloadEntry(INinjaWeapon wep, int entry)
                : base(entry, 0)
            {
                weapon = wep;

                Enabled = (weapon.UsesRemaining > 0);
            }

            public override void OnClick()
            {
                if (WeaponIsValid(weapon, Owner.From))
                {
                    Unload(Owner.From, weapon);
                }
            }
        }
    }
}

namespace Server.Targeting
{
    public class WandTarget : Target
    {
        private BaseWand m_Item;

        public WandTarget(BaseWand item)
            : base(6, false, TargetFlags.None)
        {
            m_Item = item;
        }

        private static int GetOffset(Mobile caster)
        {
            return 5 + (int)(caster.Skills[SkillName.Magery].Value * 0.02);
        }

        protected override void OnTarget(Mobile from, object targeted)
        {
            m_Item.DoWandTarget(from, targeted);
        }
    }
}

namespace Server.Targets
{
    public class BladedItemTarget : Target
    {
        private Item m_Item;

        public BladedItemTarget(Item item)
            : base(2, false, TargetFlags.None)
        {
            m_Item = item;
        }

        protected override void OnTargetOutOfRange(Mobile from, object targeted)
        {
            if (targeted is UnholyBone && from.InRange(((UnholyBone)targeted), 12))
                ((UnholyBone)targeted).Carve(from, m_Item);
            else
                base.OnTargetOutOfRange(from, targeted);
        }

        protected override void OnTarget(Mobile from, object targeted)
        {
            if (m_Item.Deleted)
                return;

            if (targeted is ICarvable)
            {
                ((ICarvable)targeted).Carve(from, m_Item);
            }
            else if (targeted is SwampDragon && ((SwampDragon)targeted).HasBarding)
            {
                SwampDragon pet = (SwampDragon)targeted;

                if (!pet.Controlled || pet.ControlMaster != from)
                    from.SendLocalizedMessage(1053022); // You cannot remove barding from a swamp dragon you do not own.
                else
                    pet.HasBarding = false;
            }
            else
            {
                if (targeted is StaticTarget)
                {
                    int itemID = ((StaticTarget)targeted).ItemID;

                    if (itemID == 0xD15 || itemID == 0xD16) // red mushroom
                    {
                        PlayerMobile player = from as PlayerMobile;

                        if (player != null)
                        {
                            QuestSystem qs = player.Quest;

                            if (qs is WitchApprenticeQuest)
                            {
                                FindIngredientObjective obj = qs.FindObjective(typeof(FindIngredientObjective)) as FindIngredientObjective;

                                if (obj != null && !obj.Completed && obj.Ingredient == Ingredient.RedMushrooms)
                                {
                                    player.SendLocalizedMessage(1055036); // You slice a red cap mushroom from its stem.
                                    obj.Complete();
                                    return;
                                }
                            }
                        }
                    }
                }

                HarvestSystem system = Lumberjacking.System;
                HarvestDefinition def = Lumberjacking.System.Definition;

                int tileID;
                Map map;
                Point3D loc;

                if (!system.GetHarvestDetails(from, m_Item, targeted, out tileID, out map, out loc))
                {
                    from.SendLocalizedMessage(500494); // You can't use a bladed item on that!
                }
                else if (!def.Validate(tileID))
                {
                    from.SendLocalizedMessage(500494); // You can't use a bladed item on that!
                }
                else
                {
                    HarvestBank bank = def.GetBank(map, loc.X, loc.Y);

                    if (bank == null)
                        return;

                    if (bank.Current < 5)
                    {
                        from.SendLocalizedMessage(500493); // There's not enough wood here to harvest.
                    }
                    else
                    {
                        bank.Consume(5, from);

                        Item item = new Kindling();

                        if (from.PlaceInBackpack(item))
                        {
                            from.SendLocalizedMessage(500491); // You put some kindling into your backpack.
                            from.SendLocalizedMessage(500492); // An axe would probably get you more wood.
                        }
                        else
                        {
                            from.SendLocalizedMessage(500490); // You can't place any kindling into your backpack!

                            item.Delete();
                        }
                    }
                }
            }
        }
    }
}