using System;
using System.Collections;
using System.Collections.Generic;

using Server;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Spells;
using Server.Targeting;

namespace Server.Spells.Bushido
{
    public abstract class SamuraiSpell : Spell
    {
        public abstract double RequiredSkill { get; }
        public abstract int RequiredMana { get; }

        public override SkillName CastSkill { get { return SkillName.Bushido; } }
        public override SkillName DamageSkill { get { return SkillName.Bushido; } }

        public override bool ClearHandsOnCast { get { return false; } }
        public override bool BlocksMovement { get { return false; } }
        public override bool ShowHandMovement { get { return false; } }

        //public override int CastDelayBase{ get{ return 1; } }
        public override double CastDelayFastScalar { get { return 0; } }

        public override int CastRecoveryBase { get { return 7; } }

        public SamuraiSpell(Mobile caster, Item scroll, SpellInfo info)
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
                Caster.SendLocalizedMessage(1070768, RequiredSkill.ToString("F1")); // You need ~1_SKILL_REQUIREMENT~ Bushido skill to perform that attack!
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
            min = RequiredSkill - 12.5;	//per 5 on friday, 2/16/07
            max = RequiredSkill + 37.5;
        }

        public override int GetMana()
        {
            return 0;
        }

        public virtual void OnCastSuccessful(Mobile caster)
        {
            if (Evasion.IsEvading(caster))
                Evasion.EndEvasion(caster);

            if (Confidence.IsConfident(caster))
                Confidence.EndConfidence(caster);

            if (CounterAttack.IsCountering(caster))
                CounterAttack.StopCountering(caster);

            int spellID = SpellRegistry.GetRegistryNumber(this);

            if (spellID > 0)
                caster.Send(new ToggleSpecialAbility(spellID + 1, true));
        }

        public static void OnEffectEnd(Mobile caster, Type type)
        {
            int spellID = SpellRegistry.GetRegistryNumber(type);

            if (spellID > 0)
                caster.Send(new ToggleSpecialAbility(spellID + 1, false));
        }
    }

    public class SamuraiMove : SpecialMove
    {
        public override SkillName MoveSkill { get { return SkillName.Bushido; } }

        public override void CheckGain(Mobile m)
        {
            m.CheckSkill(MoveSkill, RequiredSkill - 12.5, RequiredSkill + 37.5);	//Per five on friday 02/16/07
        }
    }

    public class Confidence : SamuraiSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Confidence", null,
                -1,
                9002
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(0.25); } }

        public override double RequiredSkill { get { return 25.0; } }
        public override int RequiredMana { get { return 10; } }

        public Confidence(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnBeginCast()
        {
            base.OnBeginCast();

            Caster.FixedEffect(0x37C4, 10, 7, 4, 3);
        }

        public override void OnCast()
        {
            if (CheckSequence())
            {
                Caster.SendLocalizedMessage(1063115); // You exude confidence.

                Caster.FixedParticles(0x375A, 1, 17, 0x7DA, 0x960, 0x3, EffectLayer.Waist);
                Caster.PlaySound(0x51A);

                OnCastSuccessful(Caster);

                BeginConfidence(Caster);
                BeginRegenerating(Caster);
            }

            FinishSequence();
        }

        private static Hashtable m_Table = new Hashtable();

        public static bool IsConfident(Mobile m)
        {
            return m_Table.Contains(m);
        }

        public static void BeginConfidence(Mobile m)
        {
            Timer t = (Timer)m_Table[m];

            if (t != null)
                t.Stop();

            t = new InternalTimer(m);

            m_Table[m] = t;

            t.Start();
        }

        public static void EndConfidence(Mobile m)
        {
            Timer t = (Timer)m_Table[m];

            if (t != null)
                t.Stop();

            m_Table.Remove(m);

            OnEffectEnd(m, typeof(Confidence));
        }

        private class InternalTimer : Timer
        {
            private Mobile m_Mobile;

            public InternalTimer(Mobile m)
                : base(TimeSpan.FromSeconds(15.0))
            {
                m_Mobile = m;
                Priority = TimerPriority.TwoFiftyMS;
            }

            protected override void OnTick()
            {
                EndConfidence(m_Mobile);
                m_Mobile.SendLocalizedMessage(1063116); // Your confidence wanes.
            }
        }

        private static Hashtable m_RegenTable = new Hashtable();

        public static bool IsRegenerating(Mobile m)
        {
            return m_RegenTable.Contains(m);
        }

        public static void BeginRegenerating(Mobile m)
        {
            Timer t = (Timer)m_RegenTable[m];

            if (t != null)
                t.Stop();

            t = new RegenTimer(m);

            m_RegenTable[m] = t;

            t.Start();
        }

        public static void StopRegenerating(Mobile m)
        {
            Timer t = (Timer)m_RegenTable[m];

            if (t != null)
                t.Stop();

            m_RegenTable.Remove(m);
        }

        private class RegenTimer : Timer
        {
            private Mobile m_Mobile;
            private int m_Ticks;
            private int m_Hits;

            public RegenTimer(Mobile m)
                : base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
            {
                m_Mobile = m;
                m_Hits = 15 + (m.Skills.Bushido.Fixed * m.Skills.Bushido.Fixed / 57600);
                Priority = TimerPriority.TwoFiftyMS;
            }

            protected override void OnTick()
            {
                ++m_Ticks;

                if (m_Ticks >= 5)
                {
                    m_Mobile.Hits += (m_Hits - (m_Hits * 4 / 5));
                    StopRegenerating(m_Mobile);
                }

                m_Mobile.Hits += (m_Hits / 5);
            }
        }
    }

    public class CounterAttack : SamuraiSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "CounterAttack", null,
                -1,
                9002
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(0.25); } }

        public override double RequiredSkill { get { return 40.0; } }
        public override int RequiredMana { get { return 5; } }

        public override bool CheckCast()
        {
            if (!base.CheckCast())
                return false;

            if (Caster.FindItemOnLayer(Layer.TwoHanded) as BaseShield != null)
                return true;

            if (Caster.FindItemOnLayer(Layer.OneHanded) as BaseWeapon != null)
                return true;

            if (Caster.FindItemOnLayer(Layer.TwoHanded) as BaseWeapon != null)
                return true;

            Caster.SendLocalizedMessage(1062944); // You must have a weapon or a shield equipped to use this ability!
            return false;
        }

        public CounterAttack(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnBeginCast()
        {
            base.OnBeginCast();

            Caster.FixedEffect(0x37C4, 10, 7, 4, 3);
        }

        public override void OnCast()
        {
            if (CheckSequence())
            {
                Caster.SendLocalizedMessage(1063118); // You prepare to respond immediately to the next blocked blow.

                OnCastSuccessful(Caster);

                StartCountering(Caster);
            }

            FinishSequence();
        }

        private static Hashtable m_Table = new Hashtable();

        public static bool IsCountering(Mobile m)
        {
            return m_Table.Contains(m);
        }

        public static void StartCountering(Mobile m)
        {
            Timer t = (Timer)m_Table[m];

            if (t != null)
                t.Stop();

            t = new InternalTimer(m);

            m_Table[m] = t;

            t.Start();
        }

        public static void StopCountering(Mobile m)
        {
            Timer t = (Timer)m_Table[m];

            if (t != null)
                t.Stop();

            m_Table.Remove(m);

            OnEffectEnd(m, typeof(CounterAttack));
        }

        private class InternalTimer : Timer
        {
            private Mobile m_Mobile;

            public InternalTimer(Mobile m)
                : base(TimeSpan.FromSeconds(30.0))
            {
                m_Mobile = m;
                Priority = TimerPriority.TwoFiftyMS;
            }

            protected override void OnTick()
            {
                StopCountering(m_Mobile);
                m_Mobile.SendLocalizedMessage(1063119); // You return to your normal stance.
            }
        }
    }

    public class Evasion : SamuraiSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Evasion", null,
                -1,
                9002
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(0.25); } }

        public override double RequiredSkill { get { return 60.0; } }
        public override int RequiredMana { get { return 10; } }

        public override bool CheckCast()
        {
            if (VerifyCast(Caster, true))
                return base.CheckCast();

            return false;
        }

        public static bool VerifyCast(Mobile Caster, bool messages)
        {
            if (Caster == null) // Sanity
                return false;

            BaseWeapon weap = Caster.FindItemOnLayer(Layer.OneHanded) as BaseWeapon;

            if (weap == null)
                weap = Caster.FindItemOnLayer(Layer.TwoHanded) as BaseWeapon;

            if (weap != null)
            {
                if (Core.ML && Caster.Skills[weap.Skill].Base < 50)
                {
                    if (messages)
                    {
                        Caster.SendLocalizedMessage(1076206); // Your skill with your equipped weapon must be 50 or higher to use Evasion.
                    }
                    return false;
                }
            }
            else if (!(Caster.FindItemOnLayer(Layer.TwoHanded) is BaseShield))
            {
                if (messages)
                {
                    Caster.SendLocalizedMessage(1062944); // You must have a weapon or a shield equipped to use this ability!
                }
                return false;
            }

            if (!Caster.CanBeginAction(typeof(Evasion)))
            {
                if (messages)
                {
                    Caster.SendLocalizedMessage(501789); // You must wait before trying again.
                }
                return false;
            }

            return true;
        }

        public static bool CheckSpellEvasion(Mobile defender)
        {
            BaseWeapon weap = defender.FindItemOnLayer(Layer.OneHanded) as BaseWeapon;

            if (weap == null)
                weap = defender.FindItemOnLayer(Layer.TwoHanded) as BaseWeapon;

            if (Core.ML)
            {
                if (defender.Spell != null && defender.Spell.IsCasting)
                {
                    return false;
                }

                if (weap != null)
                {
                    if (defender.Skills[weap.Skill].Base < 50)
                    {
                        return false;
                    }
                }
                else if (!(defender.FindItemOnLayer(Layer.TwoHanded) is BaseShield))
                {
                    return false;
                }
            }

            if (IsEvading(defender) && BaseWeapon.CheckParry(defender))
            {
                defender.Emote("*evades*"); // Yes.  Eew.  Blame OSI.
                defender.FixedEffect(0x37B9, 10, 16);
                return true;
            }

            return false;
        }

        public Evasion(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnBeginCast()
        {
            base.OnBeginCast();

            Caster.FixedEffect(0x37C4, 10, 7, 4, 3);
        }

        public override void OnCast()
        {
            if (CheckSequence())
            {
                Caster.SendLocalizedMessage(1063120); // You feel that you might be able to deflect any attack!
                Caster.FixedParticles(0x376A, 1, 20, 0x7F5, 0x960, 3, EffectLayer.Waist);
                Caster.PlaySound(0x51B);

                OnCastSuccessful(Caster);

                BeginEvasion(Caster);

                Caster.BeginAction(typeof(Evasion));
                Timer.DelayCall(TimeSpan.FromSeconds(20.0), delegate { Caster.EndAction(typeof(Evasion)); });
            }

            FinishSequence();
        }

        private static Hashtable m_Table = new Hashtable();

        public static bool IsEvading(Mobile m)
        {
            return m_Table.Contains(m);
        }

        public static TimeSpan GetEvadeDuration(Mobile m)
        {

            /* Evasion duration now scales with Bushido skill
             * 
             * If the player has higher than GM Bushido, and GM Tactics and Anatomy, they get a 1 second bonus
             * Evasion duration range:
             * o 3-6 seconds w/o tactics/anatomy
             * o 6-7 seconds w/ GM+ Bushido and GM tactics/anatomy 
             */

            if (!Core.ML)
                return TimeSpan.FromSeconds(8.0);

            double seconds = 3;

            if (m.Skills.Bushido.Value > 60)
                seconds += (m.Skills.Bushido.Value - 60) / 20;

            if (m.Skills.Anatomy.Value >= 100.0 && m.Skills.Tactics.Value >= 100.0 && m.Skills.Bushido.Value > 100.0)	//Bushido being HIGHER than 100 for bonus is intended
                seconds++;

            return TimeSpan.FromSeconds((int)seconds);
        }

        public static double GetParryScalar(Mobile m)
        {
            /* Evasion modifier to parry now scales with Bushido skill
             * 
             * If the player has higher than GM Bushido, and at least GM Tactics and Anatomy, they get a bonus to their evasion modifier (10% bonus to the evasion modifier to parry NOT 10% to the final parry chance)
             * 
             * Bonus modifier to parry range: (these are the ranges for the evasion modifier)
             * o 16-40% bonus w/o tactics/anatomy
             * o 42-50% bonus w/ GM+ bushido and GM tactics/anatomy
             */

            if (!Core.ML)
                return 1.5;

            double bonus = 0;

            if (m.Skills.Bushido.Value >= 60)
                bonus += (((m.Skills.Bushido.Value - 60) * .004) + 0.16);

            if (m.Skills.Anatomy.Value >= 100 && m.Skills.Tactics.Value >= 100 && m.Skills.Bushido.Value > 100) //Bushido being HIGHER than 100 for bonus is intended
                bonus += 0.10;

            return 1.0 + bonus;
        }

        public static void BeginEvasion(Mobile m)
        {
            Timer t = (Timer)m_Table[m];

            if (t != null)
                t.Stop();

            t = new InternalTimer(m, GetEvadeDuration(m));

            m_Table[m] = t;

            t.Start();
        }

        public static void EndEvasion(Mobile m)
        {
            Timer t = (Timer)m_Table[m];

            if (t != null)
                t.Stop();

            m_Table.Remove(m);

            OnEffectEnd(m, typeof(Evasion));
        }

        private class InternalTimer : Timer
        {
            private Mobile m_Mobile;

            public InternalTimer(Mobile m, TimeSpan delay)
                : base(delay)
            {
                m_Mobile = m;
                Priority = TimerPriority.TwoFiftyMS;
            }

            protected override void OnTick()
            {
                EndEvasion(m_Mobile);
                m_Mobile.SendLocalizedMessage(1063121); // You no longer feel that you could deflect any attack.
            }
        }
    }

    public class HonorableExecution : SamuraiMove
    {
        public HonorableExecution()
        {
        }

        public override int BaseMana { get { return 0; } }
        public override double RequiredSkill { get { return 25.0; } }

        public override TextDefinition AbilityMessage { get { return new TextDefinition(1063122); } } // You better kill your enemy with your next hit or you'll be rather sorry...

        public override double GetDamageScalar(Mobile attacker, Mobile defender)
        {
            double bushido = attacker.Skills[SkillName.Bushido].Value;

            // TODO: 20 -> Perfection
            return 1.0 + (bushido * 20) / 10000;
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!Validate(attacker) || !CheckMana(attacker, true))
                return;

            ClearCurrentMove(attacker);

            HonorableExecutionInfo info = m_Table[attacker] as HonorableExecutionInfo;

            if (info != null)
            {
                info.Clear();

                if (info.m_Timer != null)
                    info.m_Timer.Stop();
            }

            if (!defender.Alive)
            {
                attacker.FixedParticles(0x373A, 1, 17, 0x7E2, EffectLayer.Waist);

                double bushido = attacker.Skills[SkillName.Bushido].Value;

                attacker.Hits += 20 + (int)((bushido * bushido) / 480.0);

                int swingBonus = Math.Max(1, (int)((bushido * bushido) / 720.0));

                info = new HonorableExecutionInfo(attacker, swingBonus);
                info.m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(20.0), new TimerStateCallback(EndEffect), info);

                m_Table[attacker] = info;
            }
            else
            {
                ArrayList mods = new ArrayList();

                mods.Add(new ResistanceMod(ResistanceType.Physical, -40));
                mods.Add(new ResistanceMod(ResistanceType.Fire, -40));
                mods.Add(new ResistanceMod(ResistanceType.Cold, -40));
                mods.Add(new ResistanceMod(ResistanceType.Poison, -40));
                mods.Add(new ResistanceMod(ResistanceType.Energy, -40));

                double resSpells = attacker.Skills[SkillName.MagicResist].Value;

                if (resSpells > 0.0)
                    mods.Add(new DefaultSkillMod(SkillName.MagicResist, true, -resSpells));

                info = new HonorableExecutionInfo(attacker, mods);
                info.m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(7.0), new TimerStateCallback(EndEffect), info);

                m_Table[attacker] = info;
            }

            CheckGain(attacker);
        }


        private static Hashtable m_Table = new Hashtable();

        public static int GetSwingBonus(Mobile target)
        {
            HonorableExecutionInfo info = m_Table[target] as HonorableExecutionInfo;

            if (info == null)
                return 0;

            return info.m_SwingBonus;
        }

        public static bool IsUnderPenalty(Mobile target)
        {
            HonorableExecutionInfo info = m_Table[target] as HonorableExecutionInfo;

            if (info == null)
                return false;

            return info.m_Penalty;
        }

        public static void RemovePenalty(Mobile target)
        {
            HonorableExecutionInfo info = m_Table[target] as HonorableExecutionInfo;

            if (info == null || !info.m_Penalty)
                return;

            info.Clear();

            if (info.m_Timer != null)
                info.m_Timer.Stop();

            m_Table.Remove(target);
        }

        private class HonorableExecutionInfo
        {
            public Mobile m_Mobile;
            public int m_SwingBonus;
            public ArrayList m_Mods;
            public bool m_Penalty;
            public Timer m_Timer;

            public HonorableExecutionInfo(Mobile from, int swingBonus)
                : this(from, swingBonus, null, false)
            {
            }

            public HonorableExecutionInfo(Mobile from, ArrayList mods)
                : this(from, 0, mods, true)
            {
            }

            public HonorableExecutionInfo(Mobile from, int swingBonus, ArrayList mods, bool penalty)
            {
                m_Mobile = from;
                m_SwingBonus = swingBonus;
                m_Mods = mods;
                m_Penalty = penalty;

                Apply();
            }

            public void Apply()
            {
                if (m_Mods == null)
                    return;

                for (int i = 0; i < m_Mods.Count; ++i)
                {
                    object mod = m_Mods[i];

                    if (mod is ResistanceMod)
                        m_Mobile.AddResistanceMod((ResistanceMod)mod);
                    else if (mod is SkillMod)
                        m_Mobile.AddSkillMod((SkillMod)mod);
                }
            }

            public void Clear()
            {
                if (m_Mods == null)
                    return;

                for (int i = 0; i < m_Mods.Count; ++i)
                {
                    object mod = m_Mods[i];

                    if (mod is ResistanceMod)
                        m_Mobile.RemoveResistanceMod((ResistanceMod)mod);
                    else if (mod is SkillMod)
                        m_Mobile.RemoveSkillMod((SkillMod)mod);
                }
            }
        }

        public void EndEffect(object state)
        {
            HonorableExecutionInfo info = (HonorableExecutionInfo)state;

            RemovePenalty(info.m_Mobile);
        }
    }

    public class LightningStrike : SamuraiMove
    {
        public LightningStrike()
        {
        }

        public override int BaseMana { get { return 5; } }
        public override double RequiredSkill { get { return 50.0; } }

        public override TextDefinition AbilityMessage { get { return new TextDefinition(1063167); } } // You prepare to strike quickly.

        public override bool DelayedContext { get { return true; } }

        public override int GetAccuracyBonus(Mobile attacker)
        {
            return 50;
        }

        public override bool Validate(Mobile from)
        {
            bool isValid = base.Validate(from);
            if (isValid)
            {
                PlayerMobile ThePlayer = from as PlayerMobile;
                ThePlayer.ExecutesLightningStrike = BaseMana;
            }
            return isValid;
        }

        public override bool IgnoreArmor(Mobile attacker)
        {
            double bushido = attacker.Skills[SkillName.Bushido].Value;
            double criticalChance = (bushido * bushido) / 72000.0;
            return (criticalChance >= Utility.RandomDouble());
        }

        public override bool OnBeforeSwing(Mobile attacker, Mobile defender)
        {
            /* no mana drain before actual hit */
            bool enoughMana = CheckMana(attacker, false);
            return Validate(attacker);
        }

        public override bool ValidatesDuringHit { get { return false; } }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            ClearCurrentMove(attacker);
            if (CheckMana(attacker, true))
            {
                attacker.SendLocalizedMessage(1063168); // You attack with lightning precision!
                defender.SendLocalizedMessage(1063169); // Your opponent's quick strike causes extra damage!
                defender.FixedParticles(0x3818, 1, 11, 0x13A8, 0, 0, EffectLayer.Waist);
                defender.PlaySound(0x51D);
                CheckGain(attacker);
                SetContext(attacker);
            }
        }

        public override void OnClearMove(Mobile attacker)
        {
            PlayerMobile ThePlayer = attacker as PlayerMobile; // this can be deletet if the PlayerMobile parts are moved to Server.Mobile 
            ThePlayer.ExecutesLightningStrike = 0;
        }
    }

    public class MomentumStrike : SamuraiMove
    {
        public MomentumStrike()
        {
        }

        public override int BaseMana { get { return 10; } }
        public override double RequiredSkill { get { return 70.0; } }

        public override TextDefinition AbilityMessage { get { return new TextDefinition(1070757); } } // You prepare to strike two enemies with one blow.

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!Validate(attacker) || !CheckMana(attacker, false))
                return;

            ClearCurrentMove(attacker);

            BaseWeapon weapon = attacker.Weapon as BaseWeapon;

            List<Mobile> targets = new List<Mobile>();

            foreach (Mobile m in attacker.GetMobilesInRange(weapon.MaxRange))
            {
                if (m == defender)
                    continue;

                if (m.Combatant != attacker)
                    continue;

                targets.Add(m);
            }

            if (targets.Count > 0)
            {
                if (!CheckMana(attacker, true))
                    return;

                Mobile target = targets[Utility.Random(targets.Count)];

                double damageBonus = attacker.Skills[SkillName.Bushido].Value / 100.0;

                if (!defender.Alive)
                    damageBonus *= 1.5;

                attacker.SendLocalizedMessage(1063171); // You transfer the momentum of your weapon into another enemy!
                target.SendLocalizedMessage(1063172); // You were hit by the momentum of a Samurai's weapon!

                target.FixedParticles(0x37B9, 1, 4, 0x251D, 0, 0, EffectLayer.Waist);

                attacker.PlaySound(0x510);

                weapon.OnSwing(attacker, target, damageBonus);

                CheckGain(attacker);
            }
            else
            {
                attacker.SendLocalizedMessage(1063123); // There are no valid targets to attack!
            }
        }

        public override void CheckGain(Mobile m)
        {
            m.CheckSkill(MoveSkill, RequiredSkill, 120.0);
        }
    }
}