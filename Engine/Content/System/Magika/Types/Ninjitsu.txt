using System;
using System.Collections;
using System.Collections.Generic;

using Server;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Regions;
using Server.SkillHandlers;
using Server.Spells;
using Server.Spells.Fifth;
using Server.Spells.Seventh;
using Server.Spells.Necromancy;
using Server.Spells.Ninjitsu;
using Server.Targeting;

namespace Server.Spells.Ninjitsu
{
    public abstract class NinjaSpell : Spell
    {
        public abstract double RequiredSkill { get; }
        public abstract int RequiredMana { get; }

        public override SkillName CastSkill { get { return SkillName.Ninjitsu; } }
        public override SkillName DamageSkill { get { return SkillName.Ninjitsu; } }

        public override bool RevealOnCast { get { return false; } }
        public override bool ClearHandsOnCast { get { return false; } }
        public override bool ShowHandMovement { get { return false; } }

        public override bool BlocksMovement { get { return false; } }

        //public override int CastDelayBase{ get{ return 1; } }

        public override int CastRecoveryBase { get { return 7; } }

        public NinjaSpell(Mobile caster, Item scroll, SpellInfo info)
            : base(caster, scroll, info)
        {
        }

        public static bool CheckExpansion(Mobile from)
        {
            if (!(from is PlayerMobile))
                return true;

            if (from.NetState == null)
                return false;

            return from.NetState.SupportsExpansion(Expansion.SE);
        }

        public override bool CheckCast()
        {
            int mana = ScaleMana(RequiredMana);

            if (!base.CheckCast())
                return false;

            if (!CheckExpansion(Caster))
            {
                Caster.SendLocalizedMessage(1063456); // You must upgrade to Samurai Empire in order to use that ability.
                return false;
            }

            if (Caster.Skills[CastSkill].Value < RequiredSkill)
            {
                string args = String.Format("{0}\t{1}\t ", RequiredSkill.ToString("F1"), CastSkill.ToString());
                Caster.SendLocalizedMessage(1063013, args); // You need at least ~1_SKILL_REQUIREMENT~ ~2_SKILL_NAME~ skill to use that ability.
                return false;
            }
            else if (Caster.Mana < mana)
            {
                Caster.SendLocalizedMessage(1060174, mana.ToString()); // You must have at least ~1_MANA_REQUIREMENT~ Mana to use this ability.
                return false;
            }

            return true;
        }

        public override bool CheckFizzle()
        {
            int mana = ScaleMana(RequiredMana);

            if (Caster.Skills[CastSkill].Value < RequiredSkill)
            {
                Caster.SendLocalizedMessage(1063352, RequiredSkill.ToString("F1")); // You need ~1_SKILL_REQUIREMENT~ Ninjitsu skill to perform that attack!
                return false;
            }
            else if (Caster.Mana < mana)
            {
                Caster.SendLocalizedMessage(1060174, mana.ToString()); // You must have at least ~1_MANA_REQUIREMENT~ Mana to use this ability.
                return false;
            }

            if (!base.CheckFizzle())
                return false;

            Caster.Mana -= mana;

            return true;
        }

        public override void GetCastSkills(out double min, out double max)
        {
            min = RequiredSkill - 12.5;	//Per 5 on friday 2/16/07
            max = RequiredSkill + 37.5;
        }

        public override int GetMana()
        {
            return 0;
        }
    }

    public class NinjaMove : SpecialMove
    {
        public override SkillName MoveSkill { get { return SkillName.Ninjitsu; } }

        public override void CheckGain(Mobile m)
        {
            m.CheckSkill(MoveSkill, RequiredSkill - 12.5, RequiredSkill + 37.5);	//Per five on friday 02/16/07
        }
    }

    public class AnimalForm : NinjaSpell
    {
        public static void Initialize()
        {
            EventSink.Login += new LoginEventHandler(OnLogin);
        }

        public static void OnLogin(LoginEventArgs e)
        {
            AnimalFormContext context = AnimalForm.GetContext(e.Mobile);

            if (context != null && context.SpeedBoost)
                e.Mobile.Send(SpeedControl.MountSpeed);
        }

        private static SpellInfo m_Info = new SpellInfo(
            "Animal Form", null,
            -1,
            9002
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(1.0); } }

        public override double RequiredSkill { get { return 0.0; } }
        public override int RequiredMana { get { return (Core.ML ? 10 : 0); } }
        public override int CastRecoveryBase { get { return (Core.ML ? 10 : base.CastRecoveryBase); } }

        public override bool BlockedByAnimalForm { get { return false; } }

        public AnimalForm(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override bool CheckCast()
        {
            if (!Caster.CanBeginAction(typeof(PolymorphSpell)))
            {
                Caster.SendLocalizedMessage(1061628); // You can't do that while polymorphed.
                return false;
            }
            else if (TransformationSpellHelper.UnderTransformation(Caster))
            {
                Caster.SendLocalizedMessage(1063219); // You cannot mimic an animal while in that form.
                return false;
            }
            else if (DisguiseTimers.IsDisguised(Caster))
            {
                Caster.SendLocalizedMessage(1061631); // You can't do that while disguised.
                return false;
            }

            return base.CheckCast();
        }

        public override bool CheckDisturb(DisturbType type, bool firstCircle, bool resistable)
        {
            return false;
        }

        private bool CasterIsMoving()
        {
            return (Core.TickCount - Caster.LastMoveTime <= Caster.ComputeMovementSpeed(Caster.Direction));
        }

        private bool m_WasMoving;

        public override void OnBeginCast()
        {
            base.OnBeginCast();

            Caster.FixedEffect(0x37C4, 10, 14, 4, 3);
            m_WasMoving = CasterIsMoving();
        }

        public override bool CheckFizzle()
        {
            // Spell is initially always successful, and with no skill gain.
            return true;
        }

        public override void OnCast()
        {
            if (!Caster.CanBeginAction(typeof(PolymorphSpell)))
            {
                Caster.SendLocalizedMessage(1061628); // You can't do that while polymorphed.
            }
            else if (TransformationSpellHelper.UnderTransformation(Caster))
            {
                Caster.SendLocalizedMessage(1063219); // You cannot mimic an animal while in that form.
            }
            else if (!Caster.CanBeginAction(typeof(IncognitoSpell)) || (Caster.IsBodyMod && GetContext(Caster) == null))
            {
                DoFizzle();
            }
            else if (CheckSequence())
            {
                AnimalFormContext context = GetContext(Caster);

                int mana = ScaleMana(RequiredMana);
                if (mana > Caster.Mana)
                {
                    Caster.SendLocalizedMessage(1060174, mana.ToString()); // You must have at least ~1_MANA_REQUIREMENT~ Mana to use this ability.
                }
                else if (context != null)
                {
                    RemoveContext(Caster, context, true);
                    Caster.Mana -= mana;
                }
                else if (Caster is PlayerMobile)
                {
                    bool skipGump = (m_WasMoving || CasterIsMoving());

                    if (GetLastAnimalForm(Caster) == -1 || !skipGump)
                    {
                        Caster.CloseGump(typeof(AnimalFormGump));
                        Caster.SendGump(new AnimalFormGump(Caster, m_Entries, this));
                    }
                    else
                    {
                        if (Morph(Caster, GetLastAnimalForm(Caster)) == MorphResult.Fail)
                        {
                            DoFizzle();
                        }
                        else
                        {
                            Caster.FixedParticles(0x3728, 10, 13, 2023, EffectLayer.Waist);
                            Caster.Mana -= mana;
                        }
                    }
                }
                else
                {
                    if (Morph(Caster, GetLastAnimalForm(Caster)) == MorphResult.Fail)
                    {
                        DoFizzle();
                    }
                    else
                    {
                        Caster.FixedParticles(0x3728, 10, 13, 2023, EffectLayer.Waist);
                        Caster.Mana -= mana;
                    }
                }
            }

            FinishSequence();
        }

        private static Hashtable m_LastAnimalForms = new Hashtable();

        public int GetLastAnimalForm(Mobile m)
        {
            if (m_LastAnimalForms.Contains(m))
                return (int)m_LastAnimalForms[m];

            return -1;
        }

        public enum MorphResult
        {
            Success,
            Fail,
            NoSkill
        }

        public static MorphResult Morph(Mobile m, int entryID)
        {
            if (entryID < 0 || entryID >= m_Entries.Length)
                return MorphResult.Fail;

            AnimalFormEntry entry = m_Entries[entryID];

            m_LastAnimalForms[m] = entryID;	//On OSI, it's the last /attempted/ one not the last succeeded one

            if (m.Skills.Ninjitsu.Value < entry.ReqSkill)
            {
                string args = String.Format("{0}\t{1}\t ", entry.ReqSkill.ToString("F1"), SkillName.Ninjitsu);
                m.SendLocalizedMessage(1063013, args); // You need at least ~1_SKILL_REQUIREMENT~ ~2_SKILL_NAME~ skill to use that ability.
                return MorphResult.NoSkill;
            }

            /*
            if( !m.CheckSkill( SkillName.Ninjitsu, entry.ReqSkill, entry.ReqSkill + 37.5 ) )
                return MorphResult.Fail;
             *
             * On OSI,it seems you can only gain starting at '0' using Animal form.
            */

            double ninjitsu = m.Skills.Ninjitsu.Value;

            if (ninjitsu < entry.ReqSkill + 37.5)
            {
                double chance = (ninjitsu - entry.ReqSkill) / 37.5;

                if (chance < Utility.RandomDouble())
                    return MorphResult.Fail;
            }

            m.CheckSkill(SkillName.Ninjitsu, 0.0, 37.5);

            if (!BaseFormTalisman.EntryEnabled(m, entry.Type))
                return MorphResult.Success; // Still consumes mana, just no effect

            BaseMount.Dismount(m);

            int bodyMod = entry.BodyMod;
            int hueMod = entry.HueMod;

            m.BodyMod = bodyMod;
            m.HueMod = hueMod;

            if (entry.SpeedBoost)
                m.Send(SpeedControl.MountSpeed);

            SkillMod mod = null;

            if (entry.StealthBonus)
            {
                mod = new DefaultSkillMod(SkillName.Stealth, true, 20.0);
                mod.ObeyCap = true;
                m.AddSkillMod(mod);
            }

            SkillMod stealingMod = null;

            if (entry.StealingBonus)
            {
                stealingMod = new DefaultSkillMod(SkillName.Stealing, true, 10.0);
                stealingMod.ObeyCap = true;
                m.AddSkillMod(stealingMod);
            }

            Timer timer = new AnimalFormTimer(m, bodyMod, hueMod);
            timer.Start();

            AddContext(m, new AnimalFormContext(timer, mod, entry.SpeedBoost, entry.Type, stealingMod));
            m.CheckStatTimers();
            return MorphResult.Success;
        }

        private static Hashtable m_Table = new Hashtable();

        public static void AddContext(Mobile m, AnimalFormContext context)
        {
            m_Table[m] = context;

            if (context.Type == typeof(BakeKitsune) || context.Type == typeof(GreyWolf))
                m.CheckStatTimers();
        }

        public static void RemoveContext(Mobile m, bool resetGraphics)
        {
            AnimalFormContext context = GetContext(m);

            if (context != null)
                RemoveContext(m, context, resetGraphics);
        }

        public static void RemoveContext(Mobile m, AnimalFormContext context, bool resetGraphics)
        {
            m_Table.Remove(m);

            if (context.SpeedBoost)
                m.Send(SpeedControl.Disable);

            SkillMod mod = context.Mod;

            if (mod != null)
                m.RemoveSkillMod(mod);

            mod = context.StealingMod;

            if (mod != null)
                m.RemoveSkillMod(mod);

            if (resetGraphics)
            {
                m.HueMod = -1;
                m.BodyMod = 0;
            }

            m.FixedParticles(0x3728, 10, 13, 2023, EffectLayer.Waist);

            context.Timer.Stop();
        }

        public static AnimalFormContext GetContext(Mobile m)
        {
            return (m_Table[m] as AnimalFormContext);
        }

        public static bool UnderTransformation(Mobile m)
        {
            return (GetContext(m) != null);
        }

        public static bool UnderTransformation(Mobile m, Type type)
        {
            AnimalFormContext context = GetContext(m);

            return (context != null && context.Type == type);
        }

        /*
                private delegate void AnimalFormCallback( Mobile from );
                private delegate bool AnimalFormRequirementCallback( Mobile from );
        */

        public class AnimalFormEntry
        {
            private Type m_Type;
            private TextDefinition m_Name;
            private int m_ItemID;
            private int m_Hue;
            private int m_Tooltip;
            private double m_ReqSkill;
            private int m_BodyMod;
            private int m_HueModMin;
            private int m_HueModMax;
            private bool m_StealthBonus;
            private bool m_SpeedBoost;
            private bool m_StealingBonus;

            public Type Type { get { return m_Type; } }
            public TextDefinition Name { get { return m_Name; } }
            public int ItemID { get { return m_ItemID; } }
            public int Hue { get { return m_Hue; } }
            public int Tooltip { get { return m_Tooltip; } }
            public double ReqSkill { get { return m_ReqSkill; } }
            public int BodyMod { get { return m_BodyMod; } }
            public int HueMod { get { return Utility.RandomMinMax(m_HueModMin, m_HueModMax); } }
            public bool StealthBonus { get { return m_StealthBonus; } }
            public bool SpeedBoost { get { return m_SpeedBoost; } }
            public bool StealingBonus { get { return m_StealingBonus; } }
            /*
            private AnimalFormCallback m_TransformCallback;
            private AnimalFormCallback m_UntransformCallback;
            private AnimalFormRequirementCallback m_RequirementCallback;
            */

            public AnimalFormEntry(Type type, TextDefinition name, int itemID, int hue, int tooltip, double reqSkill, int bodyMod, int hueModMin, int hueModMax, bool stealthBonus, bool speedBoost, bool stealingBonus)
            {
                m_Type = type;
                m_Name = name;
                m_ItemID = itemID;
                m_Hue = hue;
                m_Tooltip = tooltip;
                m_ReqSkill = reqSkill;
                m_BodyMod = bodyMod;
                m_HueModMin = hueModMin;
                m_HueModMax = hueModMax;
                m_StealthBonus = stealthBonus;
                m_SpeedBoost = speedBoost;
                m_StealingBonus = stealingBonus;
            }
        }

        private static AnimalFormEntry[] m_Entries = new AnimalFormEntry[]
			{
				new AnimalFormEntry( typeof( Kirin ),        1029632,  9632,    0, 1070811, 100.0,  0x84,     0,     0, false,  true, false ),
				new AnimalFormEntry( typeof( Unicorn ),      1018214,  9678,    0, 1070812, 100.0,  0x7A,     0,     0, false,  true, false ),
				new AnimalFormEntry( typeof( BakeKitsune ),  1030083, 10083,    0, 1070810,  82.5,  0xF6,     0,     0, false,  true, false ),
				new AnimalFormEntry( typeof( GreyWolf ),     1028482,  9681, 2309, 1070810,  82.5,  0x19, 0x8FD, 0x90E, false,  true, false ),
				new AnimalFormEntry( typeof( Llama ),        1028438,  8438,    0, 1070809,  70.0,  0xDC,     0,     0, false,  true, false ),
				new AnimalFormEntry( typeof( ForestOstard ), 1018273,  8503, 2212, 1070809,  70.0,  0xDB, 0x899, 0x8B0, false,  true, false ),
				new AnimalFormEntry( typeof( BullFrog ),     1028496,  8496, 2003, 1070807,  50.0,  0x51, 0x7D1, 0x7D6, false, false, false ),
				new AnimalFormEntry( typeof( GiantSerpent ), 1018114,  9663, 2009, 1070808,  50.0,  0x15, 0x7D1, 0x7E2, false, false, false ),
				new AnimalFormEntry( typeof( Dog ),          1018280,  8476, 2309, 1070806,  40.0,  0xD9, 0x8FD, 0x90E, false, false, false ),
				new AnimalFormEntry( typeof( Cat ),          1018264,  8475, 2309, 1070806,  40.0,  0xC9, 0x8FD, 0x90E, false, false, false ),
				new AnimalFormEntry( typeof( Rat ),          1018294,  8483, 2309, 1070805,  20.0,  0xEE, 0x8FD, 0x90E,  true, false, false ),
				new AnimalFormEntry( typeof( Rabbit ),       1028485,  8485, 2309, 1070805,  20.0,  0xCD, 0x8FD, 0x90E,  true, false, false ),
				new AnimalFormEntry( typeof( Squirrel ),     1031671, 11671,    0,       0,  20.0, 0x116,     0,     0, false, false, false ),
				new AnimalFormEntry( typeof( Ferret ),       1031672, 11672,    0, 1075220,  40.0, 0x117,     0,     0, false, false,  true ),
				new AnimalFormEntry( typeof( CuSidhe ),      1031670, 11670,    0, 1075221,  60.0, 0x115,     0,     0, false, false, false ),
				new AnimalFormEntry( typeof( Reptalon ),     1075202, 11669,    0, 1075222,  90.0, 0x114,     0,     0, false, false, false ),
			};

        public static AnimalFormEntry[] Entries { get { return m_Entries; } }

        public class AnimalFormGump : Gump
        {
            //TODO: Convert this for ML to the BaseImageTileButtonsgump
            private Mobile m_Caster;
            private AnimalForm m_Spell;
            private Item m_Talisman;

            public AnimalFormGump(Mobile caster, AnimalFormEntry[] entries, AnimalForm spell)
                : base(50, 50)
            {
                m_Caster = caster;
                m_Spell = spell;
                m_Talisman = caster.Talisman;

                AddPage(0);

                AddBackground(0, 0, 520, 404, 0x13BE);
                AddImageTiled(10, 10, 500, 20, 0xA40);
                AddImageTiled(10, 40, 500, 324, 0xA40);
                AddImageTiled(10, 374, 500, 20, 0xA40);
                AddAlphaRegion(10, 10, 500, 384);

                AddHtmlLocalized(14, 12, 500, 20, 1063394, 0x7FFF, false, false); // <center>Polymorph Selection Menu</center>

                AddButton(10, 374, 0xFB1, 0xFB2, 0, GumpButtonType.Reply, 0);
                AddHtmlLocalized(45, 376, 450, 20, 1011012, 0x7FFF, false, false); // CANCEL

                double ninjitsu = caster.Skills.Ninjitsu.Value;

                int current = 0;

                for (int i = 0; i < entries.Length; ++i)
                {
                    bool enabled = (ninjitsu >= entries[i].ReqSkill && BaseFormTalisman.EntryEnabled(caster, entries[i].Type));

                    int page = current / 10 + 1;
                    int pos = current % 10;

                    if (pos == 0)
                    {
                        if (page > 1)
                        {
                            AddButton(400, 374, 0xFA5, 0xFA7, 0, GumpButtonType.Page, page);
                            AddHtmlLocalized(440, 376, 60, 20, 1043353, 0x7FFF, false, false); // Next
                        }

                        AddPage(page);

                        if (page > 1)
                        {
                            AddButton(300, 374, 0xFAE, 0xFB0, 0, GumpButtonType.Page, 1);
                            AddHtmlLocalized(340, 376, 60, 20, 1011393, 0x7FFF, false, false); // Back
                        }
                    }

                    if (enabled)
                    {
                        int x = (pos % 2 == 0) ? 14 : 264;
                        int y = (pos / 2) * 64 + 44;

                        Rectangle2D b = ItemBounds.Table[entries[i].ItemID];

                        AddImageTiledButton(x, y, 0x918, 0x919, i + 1, GumpButtonType.Reply, 0, entries[i].ItemID, entries[i].Hue, 40 - b.Width / 2 - b.X, 30 - b.Height / 2 - b.Y, entries[i].Tooltip);
                        AddHtmlLocalized(x + 84, y, 250, 60, entries[i].Name, 0x7FFF, false, false);

                        current++;
                    }
                }
            }

            public override void OnResponse(NetState sender, RelayInfo info)
            {
                int entryID = info.ButtonID - 1;

                if (entryID < 0 || entryID >= m_Entries.Length)
                    return;

                int mana = m_Spell.ScaleMana(m_Spell.RequiredMana);
                AnimalFormEntry entry = AnimalForm.Entries[entryID];

                if (mana > m_Caster.Mana)
                {
                    m_Caster.SendLocalizedMessage(1060174, mana.ToString()); // You must have at least ~1_MANA_REQUIREMENT~ Mana to use this ability.
                }
                else if ((m_Caster is PlayerMobile) && (m_Caster as PlayerMobile).MountBlockReason != BlockMountType.None)
                {
                    m_Caster.SendLocalizedMessage(1063108); // You cannot use this ability right now.
                }
                else if (BaseFormTalisman.EntryEnabled(sender.Mobile, entry.Type))
                {
                    #region Dueling
                    if (m_Caster is PlayerMobile && ((PlayerMobile)m_Caster).DuelContext != null && !((PlayerMobile)m_Caster).DuelContext.AllowSpellCast(m_Caster, m_Spell))
                    {
                    }
                    #endregion
                    else if (AnimalForm.Morph(m_Caster, entryID) == MorphResult.Fail)
                    {
                        m_Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, 502632); // The spell fizzles.
                        m_Caster.FixedParticles(0x3735, 1, 30, 9503, EffectLayer.Waist);
                        m_Caster.PlaySound(0x5C);
                    }
                    else
                    {
                        m_Caster.FixedParticles(0x3728, 10, 13, 2023, EffectLayer.Waist);
                        m_Caster.Mana -= mana;
                    }
                }
            }
        }
    }

    public class AnimalFormContext
    {
        private Timer m_Timer;
        private SkillMod m_Mod;
        private bool m_SpeedBoost;
        private Type m_Type;
        private SkillMod m_StealingMod;

        public Timer Timer { get { return m_Timer; } }
        public SkillMod Mod { get { return m_Mod; } }
        public bool SpeedBoost { get { return m_SpeedBoost; } }
        public Type Type { get { return m_Type; } }
        public SkillMod StealingMod { get { return m_StealingMod; } }

        public AnimalFormContext(Timer timer, SkillMod mod, bool speedBoost, Type type, SkillMod stealingMod)
        {
            m_Timer = timer;
            m_Mod = mod;
            m_SpeedBoost = speedBoost;
            m_Type = type;
            m_StealingMod = stealingMod;
        }
    }

    public class AnimalFormTimer : Timer
    {
        private Mobile m_Mobile;
        private int m_Body;
        private int m_Hue;
        private int m_Counter;
        private Mobile m_LastTarget;

        public AnimalFormTimer(Mobile from, int body, int hue)
            : base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
        {
            m_Mobile = from;
            m_Body = body;
            m_Hue = hue;
            m_Counter = 0;

            Priority = TimerPriority.FiftyMS;
        }

        protected override void OnTick()
        {
            if (m_Mobile.Deleted || !m_Mobile.Alive || m_Mobile.Body != m_Body || m_Mobile.Hue != m_Hue)
            {
                AnimalForm.RemoveContext(m_Mobile, true);
                Stop();
            }
            else
            {
                if (m_Body == 0x115) // Cu Sidhe
                {
                    if (m_Counter++ >= 8)
                    {
                        if (m_Mobile.Hits < m_Mobile.HitsMax && m_Mobile.Backpack != null)
                        {
                            Bandage b = m_Mobile.Backpack.FindItemByType(typeof(Bandage)) as Bandage;

                            if (b != null)
                            {
                                m_Mobile.Hits += Utility.RandomMinMax(20, 50);
                                b.Consume();
                            }
                        }

                        m_Counter = 0;
                    }
                }
                else if (m_Body == 0x114) // Reptalon
                {
                    if (m_Mobile.Combatant != null && m_Mobile.Combatant != m_LastTarget)
                    {
                        m_Counter = 1;
                        m_LastTarget = m_Mobile.Combatant;
                    }

                    if (m_Mobile.Warmode && m_LastTarget != null && m_LastTarget.Alive && !m_LastTarget.Deleted && m_Counter-- <= 0)
                    {
                        if (m_Mobile.CanBeHarmful(m_LastTarget) && m_LastTarget.Map == m_Mobile.Map && m_LastTarget.InRange(m_Mobile.Location, BaseCreature.DefaultRangePerception) && m_Mobile.InLOS(m_LastTarget))
                        {
                            m_Mobile.Direction = m_Mobile.GetDirectionTo(m_LastTarget);
                            m_Mobile.Freeze(TimeSpan.FromSeconds(1));
                            m_Mobile.PlaySound(0x16A);

                            Timer.DelayCall<Mobile>(TimeSpan.FromSeconds(1.3), new TimerStateCallback<Mobile>(BreathEffect_Callback), m_LastTarget);
                        }

                        m_Counter = Math.Min((int)m_Mobile.GetDistanceToSqrt(m_LastTarget), 10);
                    }
                }
            }
        }

        public void BreathEffect_Callback(Mobile target)
        {
            if (m_Mobile.CanBeHarmful(target))
            {
                m_Mobile.RevealingAction();
                m_Mobile.PlaySound(0x227);
                Effects.SendMovingEffect(m_Mobile, target, 0x36D4, 5, 0, false, false, 0, 0);

                Timer.DelayCall<Mobile>(TimeSpan.FromSeconds(1), new TimerStateCallback<Mobile>(BreathDamage_Callback), target);
            }
        }

        public void BreathDamage_Callback(Mobile target)
        {
            if (m_Mobile.CanBeHarmful(target))
            {
                m_Mobile.RevealingAction();
                m_Mobile.DoHarmful(target);
                AOS.Damage(target, m_Mobile, 20, !target.Player, 0, 100, 0, 0, 0);
            }
        }
    }

    public class Backstab : NinjaMove
    {
        public Backstab()
        {
        }

        public override int BaseMana { get { return 30; } }
        public override double RequiredSkill { get { return Core.ML ? 40.0 : 20.0; } }

        public override TextDefinition AbilityMessage { get { return new TextDefinition(1063089); } } // You prepare to Backstab your opponent.

        public override double GetDamageScalar(Mobile attacker, Mobile defender)
        {
            double ninjitsu = attacker.Skills[SkillName.Ninjitsu].Value;

            return 1.0 + (ninjitsu / 360) + Tracking.GetStalkingBonus(attacker, defender) / 100;
        }

        public override bool Validate(Mobile from)
        {
            if (!from.Hidden || from.AllowedStealthSteps <= 0)
            {
                from.SendLocalizedMessage(1063087); // You must be in stealth mode to use this ability.
                return false;
            }

            return base.Validate(from);
        }

        public override bool OnBeforeSwing(Mobile attacker, Mobile defender)
        {
            bool valid = Validate(attacker) && CheckMana(attacker, true);

            if (valid)
            {
                attacker.BeginAction(typeof(Stealth));
                Timer.DelayCall(TimeSpan.FromSeconds(5.0), delegate { attacker.EndAction(typeof(Stealth)); });
            }

            return valid;

        }

        public override bool ValidatesDuringHit { get { return false; } }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            //Validates before swing

            ClearCurrentMove(attacker);

            attacker.SendLocalizedMessage(1063090); // You quickly stab your opponent as you come out of hiding!

            defender.FixedParticles(0x37B9, 1, 5, 0x251D, 0x651, 0, EffectLayer.Waist);

            attacker.RevealingAction();

            CheckGain(attacker);
        }

        public override void OnMiss(Mobile attacker, Mobile defender)
        {
            ClearCurrentMove(attacker);

            attacker.SendLocalizedMessage(1063161); // You failed to properly use the element of surprise.

            attacker.RevealingAction();
        }
    }

    public class DeathStrike : NinjaMove
    {
        public DeathStrike()
        {
        }

        public override int BaseMana { get { return 30; } }
        public override double RequiredSkill { get { return 85.0; } }

        public override TextDefinition AbilityMessage { get { return new TextDefinition(1063091); } } // You prepare to hit your opponent with a Death Strike.

        public override double GetDamageScalar(Mobile attacker, Mobile defender)
        {
            return 0.5;
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!Validate(attacker) || !CheckMana(attacker, true))
                return;

            ClearCurrentMove(attacker);

            double ninjitsu = attacker.Skills[SkillName.Ninjitsu].Value;

            double chance;
            bool isRanged = false; // should be defined onHit method, what if the player hit and remove the weapon before process? ;)

            if (attacker.Weapon is BaseRanged)
                isRanged = true;

            if (ninjitsu < 100) //This formula is an approximation from OSI data.  TODO: find correct formula
                chance = 30 + (ninjitsu - 85) * 2.2;
            else
                chance = 63 + (ninjitsu - 100) * 1.1;

            if ((chance / 100) < Utility.RandomDouble())
            {
                attacker.SendLocalizedMessage(1070779); // You missed your opponent with a Death Strike.
                return;
            }


            DeathStrikeInfo info;

            int damageBonus = 0;

            if (m_Table.Contains(defender))
            {
                defender.SendLocalizedMessage(1063092); // Your opponent lands another Death Strike!

                info = (DeathStrikeInfo)m_Table[defender];

                if (info.m_Steps > 0)
                    damageBonus = attacker.Skills[SkillName.Ninjitsu].Fixed / 150;

                if (info.m_Timer != null)
                    info.m_Timer.Stop();

                m_Table.Remove(defender);
            }
            else
            {
                defender.SendLocalizedMessage(1063093); // You have been hit by a Death Strike!  Move with caution!
            }

            attacker.SendLocalizedMessage(1063094); // You inflict a Death Strike upon your opponent!

            defender.FixedParticles(0x374A, 1, 17, 0x26BC, EffectLayer.Waist);
            attacker.PlaySound(attacker.Female ? 0x50D : 0x50E);

            info = new DeathStrikeInfo(defender, attacker, damageBonus, isRanged);
            info.m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(5.0), new TimerStateCallback(ProcessDeathStrike), defender);

            m_Table[defender] = info;

            CheckGain(attacker);
        }


        private static Hashtable m_Table = new Hashtable();

        private class DeathStrikeInfo
        {
            public Mobile m_Target;
            public Mobile m_Attacker;
            public int m_Steps;
            public int m_DamageBonus;
            public Timer m_Timer;
            public bool m_isRanged;

            public DeathStrikeInfo(Mobile target, Mobile attacker, int damageBonus, bool isRanged)
            {
                m_Target = target;
                m_Attacker = attacker;
                m_DamageBonus = damageBonus;
                m_isRanged = isRanged;
            }
        }

        public static void AddStep(Mobile m)
        {
            DeathStrikeInfo info = m_Table[m] as DeathStrikeInfo;

            if (info == null)
                return;

            if (++info.m_Steps >= 5)
                ProcessDeathStrike(m);
        }

        private static void ProcessDeathStrike(object state)
        {
            Mobile defender = (Mobile)state;

            DeathStrikeInfo info = m_Table[defender] as DeathStrikeInfo;

            if (info == null)	//sanity
                return;

            int maxDamage, damage = 0;

            double ninjitsu = info.m_Attacker.Skills[SkillName.Ninjitsu].Value;
            double stalkingBonus = Tracking.GetStalkingBonus(info.m_Attacker, info.m_Target);

            if (Core.ML)
            {
                double scalar = (info.m_Attacker.Skills[SkillName.Hiding].Value + info.m_Attacker.Skills[SkillName.Stealth].Value) / 220;

                if (scalar > 1)
                    scalar = 1;

                // New formula doesn't apply DamageBonus anymore, caps must be, directly, 60/30.
                if (info.m_Steps >= 5)
                    damage = (int)Math.Floor(Math.Min(60, (ninjitsu / 3) * (0.3 + 0.7 * scalar) + stalkingBonus));
                else
                    damage = (int)Math.Floor(Math.Min(30, (ninjitsu / 9) * (0.3 + 0.7 * scalar) + stalkingBonus));

                if (info.m_isRanged)
                    damage /= 2;
            }
            else
            {
                int divisor = (info.m_Steps >= 5) ? 30 : 80;
                double baseDamage = ninjitsu / divisor * 10;

                maxDamage = (info.m_Steps >= 5) ? 62 : 22; // DamageBonus is 8 at most. That brings the cap up to 70/30.
                damage = Math.Max(0, Math.Min(maxDamage, (int)(baseDamage + stalkingBonus))) + info.m_DamageBonus;
            }

            if (Core.ML)
                info.m_Target.Damage(damage, info.m_Attacker); // Damage is direct.
            else
                AOS.Damage(info.m_Target, info.m_Attacker, damage, true, 100, 0, 0, 0, 0, 0, 0, false, false, true); // Damage is physical.

            if (info.m_Timer != null)
                info.m_Timer.Stop();

            m_Table.Remove(info.m_Target);
        }
    }

    public class FocusAttack : NinjaMove
    {
        public FocusAttack()
        {
        }

        public override int BaseMana { get { return Core.ML ? 10 : 20; } }
        public override double RequiredSkill { get { return Core.ML ? 30.0 : 60; } }

        public override TextDefinition AbilityMessage { get { return new TextDefinition(1063095); } } // You prepare to focus all of your abilities into your next strike.

        public override bool Validate(Mobile from)
        {
            if (from.FindItemOnLayer(Layer.TwoHanded) as BaseShield != null)
            {
                from.SendLocalizedMessage(1063096); // You cannot use this ability while holding a shield.
                return false;
            }

            Item handOne = from.FindItemOnLayer(Layer.OneHanded) as BaseWeapon;

            if (handOne != null && !(handOne is BaseRanged))
                return base.Validate(from);

            Item handTwo = from.FindItemOnLayer(Layer.TwoHanded) as BaseWeapon;

            if (handTwo != null && !(handTwo is BaseRanged))
                return base.Validate(from);

            from.SendLocalizedMessage(1063097); // You must be wielding a melee weapon without a shield to use this ability.
            return false;
        }

        public override double GetDamageScalar(Mobile attacker, Mobile defender)
        {
            double ninjitsu = attacker.Skills[SkillName.Ninjitsu].Value;

            return 1.0 + (ninjitsu * ninjitsu) / 43636;
        }

        public override double GetPropertyBonus(Mobile attacker)
        {
            double ninjitsu = attacker.Skills[SkillName.Ninjitsu].Value;

            double bonus = (ninjitsu * ninjitsu) / 43636;

            return 1.0 + (bonus * 3 + 0.01);
        }

        public override bool OnBeforeDamage(Mobile attacker, Mobile defender)
        {
            return Validate(attacker) && CheckMana(attacker, true);
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            ClearCurrentMove(attacker);

            attacker.SendLocalizedMessage(1063098); // You focus all of your abilities and strike with deadly force!
            attacker.PlaySound(0x510);

            CheckGain(attacker);
        }
    }

    public class KiAttack : NinjaMove
    {
        public KiAttack()
        {
        }

        public override int BaseMana { get { return 25; } }
        public override double RequiredSkill { get { return 80.0; } }

        public override TextDefinition AbilityMessage { get { return new TextDefinition(1063099); } } // Your Ki Attack must be complete within 2 seconds for the damage bonus!

        public override void OnUse(Mobile from)
        {
            if (!Validate(from))
                return;

            KiAttackInfo info = new KiAttackInfo(from);
            info.m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(2.0), new TimerStateCallback(EndKiAttack), info);

            m_Table[from] = info;
        }

        public override bool Validate(Mobile from)
        {
            if (from.Hidden && from.AllowedStealthSteps > 0)
            {
                from.SendLocalizedMessage(1063127); // You cannot use this ability while in stealth mode.
                return false;
            }

            if (Core.ML)
            {
                BaseRanged ranged = from.Weapon as BaseRanged;

                if (ranged != null)
                {
                    from.SendLocalizedMessage(1075858); // You can only use this with melee attacks.
                    return false;
                }
            }


            return base.Validate(from);
        }

        public override double GetDamageScalar(Mobile attacker, Mobile defender)
        {
            if (attacker.Hidden)
                return 1.0;

            /*
             * Pub40 changed pvp damage max to 55%
             */

            return 1.0 + GetBonus(attacker) / ((Core.ML && attacker.Player && defender.Player) ? 40 : 10);
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!Validate(attacker) || !CheckMana(attacker, true))
                return;

            if (GetBonus(attacker) == 0.0)
            {
                attacker.SendLocalizedMessage(1063101); // You were too close to your target to cause any additional damage.
            }
            else
            {
                attacker.FixedParticles(0x37BE, 1, 5, 0x26BD, 0x0, 0x1, EffectLayer.Waist);
                attacker.PlaySound(0x510);

                attacker.SendLocalizedMessage(1063100); // Your quick flight to your target causes extra damage as you strike!
                defender.FixedParticles(0x37BE, 1, 5, 0x26BD, 0, 0x1, EffectLayer.Waist);

                CheckGain(attacker);
            }

            ClearCurrentMove(attacker);
        }

        public override void OnClearMove(Mobile from)
        {
            KiAttackInfo info = m_Table[from] as KiAttackInfo;

            if (info != null)
            {
                if (info.m_Timer != null)
                    info.m_Timer.Stop();

                m_Table.Remove(info.m_Mobile);
            }
        }

        private static Hashtable m_Table = new Hashtable();

        public static double GetBonus(Mobile from)
        {
            KiAttackInfo info = m_Table[from] as KiAttackInfo;

            if (info == null)
                return 0.0;

            int xDelta = info.m_Location.X - from.X;
            int yDelta = info.m_Location.Y - from.Y;

            double bonus = Math.Sqrt((xDelta * xDelta) + (yDelta * yDelta));

            if (bonus > 20.0)
                bonus = 20.0;

            return bonus;
        }

        private class KiAttackInfo
        {
            public Mobile m_Mobile;
            public Point3D m_Location;
            public Timer m_Timer;

            public KiAttackInfo(Mobile m)
            {
                m_Mobile = m;
                m_Location = m.Location;
            }
        }

        private static void EndKiAttack(object state)
        {
            KiAttackInfo info = (KiAttackInfo)state;

            if (info.m_Timer != null)
                info.m_Timer.Stop();

            ClearCurrentMove(info.m_Mobile);
            info.m_Mobile.SendLocalizedMessage(1063102); // You failed to complete your Ki Attack in time.

            m_Table.Remove(info.m_Mobile);
        }
    }

    public class MirrorImage : NinjaSpell
    {
        private static Dictionary<Mobile, int> m_CloneCount = new Dictionary<Mobile, int>();

        public static bool HasClone(Mobile m)
        {
            return m_CloneCount.ContainsKey(m);
        }

        public static void AddClone(Mobile m)
        {
            if (m == null)
                return;

            if (m_CloneCount.ContainsKey(m))
                m_CloneCount[m]++;
            else
                m_CloneCount[m] = 1;
        }

        public static void RemoveClone(Mobile m)
        {
            if (m == null)
                return;

            if (m_CloneCount.ContainsKey(m))
            {
                m_CloneCount[m]--;

                if (m_CloneCount[m] == 0)
                    m_CloneCount.Remove(m);
            }
        }

        private static SpellInfo m_Info = new SpellInfo(
            "Mirror Image", null,
            -1,
            9002
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(1.5); } }

        public override double RequiredSkill { get { return Core.ML ? 20.0 : 40.0; } }
        public override int RequiredMana { get { return 10; } }

        public override bool BlockedByAnimalForm { get { return false; } }

        public MirrorImage(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override bool CheckCast()
        {
            if (Caster.Mounted)
            {
                Caster.SendLocalizedMessage(1063132); // You cannot use this ability while mounted.
                return false;
            }
            else if ((Caster.Followers + 1) > Caster.FollowersMax)
            {
                Caster.SendLocalizedMessage(1063133); // You cannot summon a mirror image because you have too many followers.
                return false;
            }
            else if (TransformationSpellHelper.UnderTransformation(Caster, typeof(HorrificBeastSpell)))
            {
                Caster.SendLocalizedMessage(1061091); // You cannot cast that spell in this form.
                return false;
            }

            return base.CheckCast();
        }

        public override bool CheckDisturb(DisturbType type, bool firstCircle, bool resistable)
        {
            return false;
        }

        public override void OnBeginCast()
        {
            base.OnBeginCast();

            Caster.SendLocalizedMessage(1063134); // You begin to summon a mirror image of yourself.
        }

        public override void OnCast()
        {
            if (Caster.Mounted)
            {
                Caster.SendLocalizedMessage(1063132); // You cannot use this ability while mounted.
            }
            else if ((Caster.Followers + 1) > Caster.FollowersMax)
            {
                Caster.SendLocalizedMessage(1063133); // You cannot summon a mirror image because you have too many followers.
            }
            else if (TransformationSpellHelper.UnderTransformation(Caster, typeof(HorrificBeastSpell)))
            {
                Caster.SendLocalizedMessage(1061091); // You cannot cast that spell in this form.
            }
            else if (CheckSequence())
            {
                Caster.FixedParticles(0x376A, 1, 14, 0x13B5, EffectLayer.Waist);
                Caster.PlaySound(0x511);

                new Clone(Caster).MoveToWorld(Caster.Location, Caster.Map);
            }

            FinishSequence();
        }
    }

    public class Shadowjump : NinjaSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
            "Shadowjump", null,
            -1,
            9002
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(1.0); } }

        public override double RequiredSkill { get { return 50.0; } }
        public override int RequiredMana { get { return 15; } }

        public override bool BlockedByAnimalForm { get { return false; } }

        public Shadowjump(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override bool CheckCast()
        {
            PlayerMobile pm = Caster as PlayerMobile; // IsStealthing should be moved to Server.Mobiles
            if (!pm.IsStealthing)
            {
                Caster.SendLocalizedMessage(1063087); // You must be in stealth mode to use this ability.
                return false;
            }

            return base.CheckCast();
        }

        public override bool CheckDisturb(DisturbType type, bool firstCircle, bool resistable)
        {
            return false;
        }

        public override void OnCast()
        {
            Caster.SendLocalizedMessage(1063088); // You prepare to perform a Shadowjump.
            Caster.Target = new InternalTarget(this);
        }

        public void Target(IPoint3D p)
        {
            IPoint3D orig = p;
            Map map = Caster.Map;

            SpellHelper.GetSurfaceTop(ref p);

            Point3D from = Caster.Location;
            Point3D to = new Point3D(p);

            PlayerMobile pm = Caster as PlayerMobile; // IsStealthing should be moved to Server.Mobiles

            if (!pm.IsStealthing)
            {
                Caster.SendLocalizedMessage(1063087); // You must be in stealth mode to use this ability.
            }
            else if (Factions.Sigil.ExistsOn(Caster))
            {
                Caster.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
            }
            else if (Server.Misc.WeightOverloading.IsOverloaded(Caster))
            {
                Caster.SendLocalizedMessage(502359, "", 0x22); // Thou art too encumbered to move.
            }
            else if (!SpellHelper.CheckTravel(Caster, TravelCheckType.TeleportFrom) || !SpellHelper.CheckTravel(Caster, map, to, TravelCheckType.TeleportTo))
            {
            }
            else if (map == null || !map.CanSpawnMobile(p.X, p.Y, p.Z))
            {
                Caster.SendLocalizedMessage(502831); // Cannot teleport to that spot.
            }
            else if (SpellHelper.CheckMulti(to, map, true, 5))
            {
                Caster.SendLocalizedMessage(502831); // Cannot teleport to that spot.
            }
            else if (Region.Find(to, map).GetRegion(typeof(HouseRegion)) != null)
            {
                Caster.SendLocalizedMessage(502829); // Cannot teleport to that spot.
            }
            else if (CheckSequence())
            {
                SpellHelper.Turn(Caster, orig);

                Mobile m = Caster;

                m.Location = to;
                m.ProcessDelta();

                Effects.SendLocationParticles(EffectItem.Create(from, m.Map, EffectItem.DefaultDuration), 0x3728, 10, 10, 2023);

                m.PlaySound(0x512);

                Server.SkillHandlers.Stealth.OnUse(m); // stealth check after the a jump
            }

            FinishSequence();
        }
        public class InternalTarget : Target
        {
            private Shadowjump m_Owner;

            public InternalTarget(Shadowjump owner)
                : base(11, true, TargetFlags.None)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                IPoint3D p = o as IPoint3D;

                if (p != null)
                    m_Owner.Target(p);
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class SurpriseAttack : NinjaMove
    {
        public SurpriseAttack()
        {
        }

        public override int BaseMana { get { return 20; } }
        public override double RequiredSkill { get { return Core.ML ? 60.0 : 30.0; } }

        public override TextDefinition AbilityMessage { get { return new TextDefinition(1063128); } } // You prepare to surprise your prey.

        public override bool Validate(Mobile from)
        {
            if (!from.Hidden || from.AllowedStealthSteps <= 0)
            {
                from.SendLocalizedMessage(1063087); // You must be in stealth mode to use this ability.
                return false;
            }

            return base.Validate(from);
        }

        public override bool OnBeforeSwing(Mobile attacker, Mobile defender)
        {
            bool valid = Validate(attacker) && CheckMana(attacker, true);

            if (valid)
            {
                attacker.BeginAction(typeof(Stealth));
                Timer.DelayCall(TimeSpan.FromSeconds(5.0), delegate { attacker.EndAction(typeof(Stealth)); });
            }

            return valid;

        }

        public override bool ValidatesDuringHit { get { return false; } }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            //Validates before swing

            ClearCurrentMove(attacker);

            attacker.SendLocalizedMessage(1063129); // You catch your opponent off guard with your Surprise Attack!
            defender.SendLocalizedMessage(1063130); // Your defenses are lowered as your opponent surprises you!

            defender.FixedParticles(0x37B9, 1, 5, 0x26DA, 0, 3, EffectLayer.Head);

            attacker.RevealingAction();

            SurpriseAttackInfo info;

            if (m_Table.Contains(defender))
            {
                info = (SurpriseAttackInfo)m_Table[defender];

                if (info.m_Timer != null)
                    info.m_Timer.Stop();

                m_Table.Remove(defender);
            }

            int ninjitsu = attacker.Skills[SkillName.Ninjitsu].Fixed;

            int malus = ninjitsu / 60 + (int)Tracking.GetStalkingBonus(attacker, defender);

            info = new SurpriseAttackInfo(defender, malus);
            info.m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(8.0), new TimerStateCallback(EndSurprise), info);

            m_Table[defender] = info;

            CheckGain(attacker);
        }

        public override void OnMiss(Mobile attacker, Mobile defender)
        {
            ClearCurrentMove(attacker);

            attacker.SendLocalizedMessage(1063161); // You failed to properly use the element of surprise.

            attacker.RevealingAction();
        }


        private static Hashtable m_Table = new Hashtable();

        public static bool GetMalus(Mobile target, ref int malus)
        {
            SurpriseAttackInfo info = m_Table[target] as SurpriseAttackInfo;

            if (info == null)
                return false;

            malus = info.m_Malus;
            return true;
        }

        private class SurpriseAttackInfo
        {
            public Mobile m_Target;
            public int m_Malus;
            public Timer m_Timer;

            public SurpriseAttackInfo(Mobile target, int effect)
            {
                m_Target = target;
                m_Malus = effect;
            }
        }

        private static void EndSurprise(object state)
        {
            SurpriseAttackInfo info = (SurpriseAttackInfo)state;

            if (info.m_Timer != null)
                info.m_Timer.Stop();

            info.m_Target.SendLocalizedMessage(1063131); // Your defenses have returned to normal.

            m_Table.Remove(info.m_Target);
        }
    }
}