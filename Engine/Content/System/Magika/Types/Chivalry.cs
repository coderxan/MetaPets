using System;
using System.Collections;
using System.Collections.Generic;

using Server;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Regions;
using Server.Spells;
using Server.Spells.Fourth;
using Server.Spells.Necromancy;
using Server.Targeting;

namespace Server.Spells.Chivalry
{
    public abstract class PaladinSpell : Spell
    {
        public abstract double RequiredSkill { get; }
        public abstract int RequiredMana { get; }
        public abstract int RequiredTithing { get; }
        public abstract int MantraNumber { get; }

        public override SkillName CastSkill { get { return SkillName.Chivalry; } }
        public override SkillName DamageSkill { get { return SkillName.Chivalry; } }

        public override bool ClearHandsOnCast { get { return false; } }

        //public override int CastDelayBase{ get{ return 1; } }

        public override int CastRecoveryBase { get { return 7; } }

        public PaladinSpell(Mobile caster, Item scroll, SpellInfo info)
            : base(caster, scroll, info)
        {
        }

        public override bool CheckCast()
        {
            int mana = ScaleMana(RequiredMana);

            if (!base.CheckCast())
                return false;

            if (Caster.TithingPoints < RequiredTithing)
            {
                Caster.SendLocalizedMessage(1060173, RequiredTithing.ToString()); // You must have at least ~1_TITHE_REQUIREMENT~ Tithing Points to use this ability,
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
            int requiredTithing = this.RequiredTithing;

            if (AosAttributes.GetValue(Caster, AosAttribute.LowerRegCost) > Utility.Random(100))
                requiredTithing = 0;

            int mana = ScaleMana(RequiredMana);

            if (Caster.TithingPoints < requiredTithing)
            {
                Caster.SendLocalizedMessage(1060173, RequiredTithing.ToString()); // You must have at least ~1_TITHE_REQUIREMENT~ Tithing Points to use this ability,
                return false;
            }
            else if (Caster.Mana < mana)
            {
                Caster.SendLocalizedMessage(1060174, mana.ToString()); // You must have at least ~1_MANA_REQUIREMENT~ Mana to use this ability.
                return false;
            }

            Caster.TithingPoints -= requiredTithing;

            if (!base.CheckFizzle())
                return false;

            Caster.Mana -= mana;

            return true;
        }

        public override void SayMantra()
        {
            Caster.PublicOverheadMessage(MessageType.Regular, 0x3B2, MantraNumber, "", false);
        }

        public override void DoFizzle()
        {
            Caster.PlaySound(0x1D6);
            Caster.NextSpellTime = Core.TickCount;
        }

        public override void DoHurtFizzle()
        {
            Caster.PlaySound(0x1D6);
        }

        public override void OnDisturb(DisturbType type, bool message)
        {
            base.OnDisturb(type, message);

            if (message)
                Caster.PlaySound(0x1D6);
        }

        public override void OnBeginCast()
        {
            base.OnBeginCast();

            SendCastEffect();
        }

        public virtual void SendCastEffect()
        {
            Caster.FixedEffect(0x37C4, 10, (int)(GetCastDelay().TotalSeconds * 28), 4, 3);
        }

        public override void GetCastSkills(out double min, out double max)
        {
            min = RequiredSkill;
            max = RequiredSkill + 50.0;
        }

        public override int GetMana()
        {
            return 0;
        }

        public int ComputePowerValue(int div)
        {
            return ComputePowerValue(Caster, div);
        }

        public static int ComputePowerValue(Mobile from, int div)
        {
            if (from == null)
                return 0;

            int v = (int)Math.Sqrt(from.Karma + 20000 + (from.Skills.Chivalry.Fixed * 10));

            return v / div;
        }
    }

    public class CleanseByFireSpell : PaladinSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Cleanse By Fire", "Expor Flamus",
                -1,
                9002
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(1.0); } }

        public override double RequiredSkill { get { return 5.0; } }
        public override int RequiredMana { get { return 10; } }
        public override int RequiredTithing { get { return 10; } }
        public override int MantraNumber { get { return 1060718; } } // Expor Flamus

        public CleanseByFireSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override bool CheckCast()
        {
            if (Engines.ConPVP.DuelContext.CheckSuddenDeath(Caster))
            {
                Caster.SendMessage(0x22, "You cannot cast this spell when in sudden death.");
                return false;
            }

            return base.CheckCast();
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(Mobile m)
        {
            if (!m.Poisoned)
            {
                Caster.SendLocalizedMessage(1060176); // That creature is not poisoned!
            }
            else if (CheckBSequence(m))
            {
                SpellHelper.Turn(Caster, m);

                /* Cures the target of poisons, but causes the caster to be burned by fire damage for 13-55 hit points.
                 * The amount of fire damage is lessened if the caster has high Karma.
                 */

                Poison p = m.Poison;

                if (p != null)
                {
                    // Cleanse by fire is now difficulty based 
                    int chanceToCure = 10000 + (int)(Caster.Skills[SkillName.Chivalry].Value * 75) - ((p.Level + 1) * 2000);
                    chanceToCure /= 100;

                    if (chanceToCure > Utility.Random(100))
                    {
                        if (m.CurePoison(Caster))
                        {
                            if (Caster != m)
                                Caster.SendLocalizedMessage(1010058); // You have cured the target of all poisons!

                            m.SendLocalizedMessage(1010059); // You have been cured of all poisons.
                        }
                    }
                    else
                    {
                        m.SendLocalizedMessage(1010060); // You have failed to cure your target!
                    }
                }

                m.PlaySound(0x1E0);
                m.FixedParticles(0x373A, 1, 15, 5012, 3, 2, EffectLayer.Waist);

                IEntity from = new Entity(Serial.Zero, new Point3D(m.X, m.Y, m.Z - 5), m.Map);
                IEntity to = new Entity(Serial.Zero, new Point3D(m.X, m.Y, m.Z + 45), m.Map);
                Effects.SendMovingParticles(from, to, 0x374B, 1, 0, false, false, 63, 2, 9501, 1, 0, EffectLayer.Head, 0x100);

                Caster.PlaySound(0x208);
                Caster.FixedParticles(0x3709, 1, 30, 9934, 0, 7, EffectLayer.Waist);

                int damage = 50 - ComputePowerValue(4);

                // TODO: Should caps be applied?
                if (damage < 13)
                    damage = 13;
                else if (damage > 55)
                    damage = 55;

                AOS.Damage(Caster, Caster, damage, 0, 100, 0, 0, 0, true);
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private CleanseByFireSpell m_Owner;

            public InternalTarget(CleanseByFireSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Beneficial)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                    m_Owner.Target((Mobile)o);
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class CloseWoundsSpell : PaladinSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Close Wounds", "Obsu Vulni",
                -1,
                9002
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(1.5); } }

        public override double RequiredSkill { get { return 0.0; } }
        public override int RequiredMana { get { return 10; } }
        public override int RequiredTithing { get { return 10; } }
        public override int MantraNumber { get { return 1060719; } } // Obsu Vulni

        public CloseWoundsSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override bool CheckCast()
        {
            if (Engines.ConPVP.DuelContext.CheckSuddenDeath(Caster))
            {
                Caster.SendMessage(0x22, "You cannot cast this spell when in sudden death.");
                return false;
            }

            return base.CheckCast();
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(Mobile m)
        {
            if (!Caster.InRange(m, 2))
            {
                Caster.SendLocalizedMessage(1060178); // You are too far away to perform that action!
            }
            else if (m is BaseCreature && ((BaseCreature)m).IsAnimatedDead)
            {
                Caster.SendLocalizedMessage(1061654); // You cannot heal that which is not alive.
            }
            else if (m.IsDeadBondedPet)
            {
                Caster.SendLocalizedMessage(1060177); // You cannot heal a creature that is already dead!
            }
            else if (m.Hits >= m.HitsMax)
            {
                Caster.SendLocalizedMessage(500955); // That being is not damaged!
            }
            else if (m.Poisoned || Server.Items.MortalStrike.IsWounded(m))
            {
                Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, (Caster == m) ? 1005000 : 1010398);
            }
            else if (CheckBSequence(m))
            {
                SpellHelper.Turn(Caster, m);

                /* Heals the target for 7 to 39 points of damage.
                 * The caster's Karma affects the amount of damage healed.
                 */

                int toHeal = ComputePowerValue(6) + Utility.RandomMinMax(0, 2);

                // TODO: Should caps be applied?
                if (toHeal < 7)
                    toHeal = 7;
                else if (toHeal > 39)
                    toHeal = 39;

                if ((m.Hits + toHeal) > m.HitsMax)
                    toHeal = m.HitsMax - m.Hits;

                //m.Hits += toHeal;	//Was previosuly due to the message
                //m.Heal( toHeal, Caster, false );
                SpellHelper.Heal(toHeal, m, Caster, false);

                m.SendLocalizedMessage(1060203, toHeal.ToString()); // You have had ~1_HEALED_AMOUNT~ hit points of damage healed.

                m.PlaySound(0x202);
                m.FixedParticles(0x376A, 1, 62, 9923, 3, 3, EffectLayer.Waist);
                m.FixedParticles(0x3779, 1, 46, 9502, 5, 3, EffectLayer.Waist);
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private CloseWoundsSpell m_Owner;

            public InternalTarget(CloseWoundsSpell owner)
                : base(12, false, TargetFlags.Beneficial)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                    m_Owner.Target((Mobile)o);
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class ConsecrateWeaponSpell : PaladinSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Consecrate Weapon", "Consecrus Arma",
                -1,
                9002
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(0.5); } }

        public override double RequiredSkill { get { return 15.0; } }
        public override int RequiredMana { get { return 10; } }
        public override int RequiredTithing { get { return 10; } }
        public override int MantraNumber { get { return 1060720; } } // Consecrus Arma
        public override bool BlocksMovement { get { return false; } }

        public ConsecrateWeaponSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            BaseWeapon weapon = Caster.Weapon as BaseWeapon;

            if (weapon == null || weapon is Fists)
            {
                Caster.SendLocalizedMessage(501078); // You must be holding a weapon.
            }
            else if (CheckSequence())
            {
                /* Temporarily enchants the weapon the caster is currently wielding.
                 * The type of damage the weapon inflicts when hitting a target will
                 * be converted to the target's worst Resistance type.
                 * Duration of the effect is affected by the caster's Karma and lasts for 3 to 11 seconds.
                 */

                int itemID, soundID;

                switch (weapon.Skill)
                {
                    case SkillName.Macing: itemID = 0xFB4; soundID = 0x232; break;
                    case SkillName.Archery: itemID = 0x13B1; soundID = 0x145; break;
                    default: itemID = 0xF5F; soundID = 0x56; break;
                }

                Caster.PlaySound(0x20C);
                Caster.PlaySound(soundID);
                Caster.FixedParticles(0x3779, 1, 30, 9964, 3, 3, EffectLayer.Waist);

                IEntity from = new Entity(Serial.Zero, new Point3D(Caster.X, Caster.Y, Caster.Z), Caster.Map);
                IEntity to = new Entity(Serial.Zero, new Point3D(Caster.X, Caster.Y, Caster.Z + 50), Caster.Map);
                Effects.SendMovingParticles(from, to, itemID, 1, 0, false, false, 33, 3, 9501, 1, 0, EffectLayer.Head, 0x100);

                double seconds = ComputePowerValue(20);

                // TODO: Should caps be applied?
                if (seconds < 3.0)
                    seconds = 3.0;
                else if (seconds > 11.0)
                    seconds = 11.0;

                TimeSpan duration = TimeSpan.FromSeconds(seconds);

                Timer t = (Timer)m_Table[weapon];

                if (t != null)
                    t.Stop();

                weapon.Consecrated = true;

                m_Table[weapon] = t = new ExpireTimer(weapon, duration);

                t.Start();
            }

            FinishSequence();
        }

        private static Hashtable m_Table = new Hashtable();

        private class ExpireTimer : Timer
        {
            private BaseWeapon m_Weapon;

            public ExpireTimer(BaseWeapon weapon, TimeSpan delay)
                : base(delay)
            {
                m_Weapon = weapon;
                Priority = TimerPriority.FiftyMS;
            }

            protected override void OnTick()
            {
                m_Weapon.Consecrated = false;
                Effects.PlaySound(m_Weapon.GetWorldLocation(), m_Weapon.Map, 0x1F8);
                m_Table.Remove(this);
            }
        }
    }

    public class DispelEvilSpell : PaladinSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Dispel Evil", "Dispiro Malas",
                -1,
                9002
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(0.25); } }

        public override double RequiredSkill { get { return 35.0; } }
        public override int RequiredMana { get { return 10; } }
        public override int RequiredTithing { get { return 10; } }
        public override int MantraNumber { get { return 1060721; } } // Dispiro Malas
        public override bool BlocksMovement { get { return false; } }

        public DispelEvilSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override bool DelayedDamage { get { return false; } }

        public override void SendCastEffect()
        {
            Caster.FixedEffect(0x37C4, 10, 7, 4, 3); // At player
        }

        public override void OnCast()
        {
            if (CheckSequence())
            {
                List<Mobile> targets = new List<Mobile>();

                foreach (Mobile m in Caster.GetMobilesInRange(8))
                {
                    if (Caster != m && SpellHelper.ValidIndirectTarget(Caster, m) && Caster.CanBeHarmful(m, false))
                        targets.Add(m);
                }

                Caster.PlaySound(0xF5);
                Caster.PlaySound(0x299);
                Caster.FixedParticles(0x37C4, 1, 25, 9922, 14, 3, EffectLayer.Head);

                int dispelSkill = ComputePowerValue(2);

                double chiv = Caster.Skills.Chivalry.Value;

                for (int i = 0; i < targets.Count; ++i)
                {
                    Mobile m = targets[i];
                    BaseCreature bc = m as BaseCreature;

                    if (bc != null)
                    {
                        bool dispellable = bc.Summoned && !bc.IsAnimatedDead;

                        if (dispellable)
                        {
                            double dispelChance = (50.0 + ((100 * (chiv - bc.DispelDifficulty)) / (bc.DispelFocus * 2))) / 100;
                            dispelChance *= dispelSkill / 100.0;

                            if (dispelChance > Utility.RandomDouble())
                            {
                                Effects.SendLocationParticles(EffectItem.Create(m.Location, m.Map, EffectItem.DefaultDuration), 0x3728, 8, 20, 5042);
                                Effects.PlaySound(m, m.Map, 0x201);

                                m.Delete();
                                continue;
                            }
                        }

                        bool evil = !bc.Controlled && bc.Karma < 0;

                        if (evil)
                        {
                            // TODO: Is this right?
                            double fleeChance = (100 - Math.Sqrt(m.Fame / 2)) * chiv * dispelSkill;
                            fleeChance /= 1000000;

                            if (fleeChance > Utility.RandomDouble())
                            {
                                // guide says 2 seconds, it's longer
                                bc.BeginFlee(TimeSpan.FromSeconds(30.0));
                            }
                        }
                    }

                    TransformContext context = TransformationSpellHelper.GetContext(m);
                    if (context != null && context.Spell is NecromancerSpell)	//Trees are not evil!	TODO: OSI confirm?
                    {
                        // transformed ..

                        double drainChance = 0.5 * (Caster.Skills.Chivalry.Value / Math.Max(m.Skills.Necromancy.Value, 1));

                        if (drainChance > Utility.RandomDouble())
                        {
                            int drain = (5 * dispelSkill) / 100;

                            m.Stam -= drain;
                            m.Mana -= drain;
                        }
                    }
                }
            }

            FinishSequence();
        }
    }

    public class DivineFurySpell : PaladinSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Divine Fury", "Divinum Furis",
                -1,
                9002
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(1.0); } }

        public override double RequiredSkill { get { return 25.0; } }
        public override int RequiredMana { get { return 15; } }
        public override int RequiredTithing { get { return 10; } }
        public override int MantraNumber { get { return 1060722; } } // Divinum Furis
        public override bool BlocksMovement { get { return false; } }

        public DivineFurySpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            if (CheckSequence())
            {
                Caster.PlaySound(0x20F);
                Caster.PlaySound(Caster.Female ? 0x338 : 0x44A);
                Caster.FixedParticles(0x376A, 1, 31, 9961, 1160, 0, EffectLayer.Waist);
                Caster.FixedParticles(0x37C4, 1, 31, 9502, 43, 2, EffectLayer.Waist);

                Caster.Stam = Caster.StamMax;

                Timer t = (Timer)m_Table[Caster];

                if (t != null)
                    t.Stop();

                int delay = ComputePowerValue(10);

                // TODO: Should caps be applied?
                if (delay < 7)
                    delay = 7;
                else if (delay > 24)
                    delay = 24;

                m_Table[Caster] = t = Timer.DelayCall(TimeSpan.FromSeconds(delay), new TimerStateCallback(Expire_Callback), Caster);
                Caster.Delta(MobileDelta.WeaponDamage);

                BuffInfo.AddBuff(Caster, new BuffInfo(BuffIcon.DivineFury, 1060589, 1075634, TimeSpan.FromSeconds(delay), Caster));
            }

            FinishSequence();
        }

        private static Hashtable m_Table = new Hashtable();

        public static bool UnderEffect(Mobile m)
        {
            return m_Table.Contains(m);
        }

        private static void Expire_Callback(object state)
        {
            Mobile m = (Mobile)state;

            m_Table.Remove(m);

            m.Delta(MobileDelta.WeaponDamage);
            m.PlaySound(0xF8);
        }
    }

    public class EnemyOfOneSpell : PaladinSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Enemy of One", "Forul Solum",
                -1,
                9002
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(0.5); } }

        public override double RequiredSkill { get { return 45.0; } }
        public override int RequiredMana { get { return 20; } }
        public override int RequiredTithing { get { return 10; } }
        public override int MantraNumber { get { return 1060723; } } // Forul Solum
        public override bool BlocksMovement { get { return false; } }

        public EnemyOfOneSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            if (CheckSequence())
            {
                Caster.PlaySound(0x0F5);
                Caster.PlaySound(0x1ED);
                Caster.FixedParticles(0x375A, 1, 30, 9966, 33, 2, EffectLayer.Head);
                Caster.FixedParticles(0x37B9, 1, 30, 9502, 43, 3, EffectLayer.Head);

                Timer t = (Timer)m_Table[Caster];

                if (t != null)
                    t.Stop();

                double delay = (double)ComputePowerValue(1) / 60;

                // TODO: Should caps be applied?
                if (delay < 1.5)
                    delay = 1.5;
                else if (delay > 3.5)
                    delay = 3.5;

                m_Table[Caster] = Timer.DelayCall(TimeSpan.FromMinutes(delay), new TimerStateCallback(Expire_Callback), Caster);

                if (Caster is PlayerMobile)
                {
                    ((PlayerMobile)Caster).EnemyOfOneType = null;
                    ((PlayerMobile)Caster).WaitingForEnemy = true;

                    BuffInfo.AddBuff(Caster, new BuffInfo(BuffIcon.EnemyOfOne, 1075653, 1044111, TimeSpan.FromMinutes(delay), Caster));
                }
            }

            FinishSequence();
        }

        private static Hashtable m_Table = new Hashtable();

        private static void Expire_Callback(object state)
        {
            Mobile m = (Mobile)state;

            m_Table.Remove(m);

            m.PlaySound(0x1F8);

            if (m is PlayerMobile)
            {
                ((PlayerMobile)m).EnemyOfOneType = null;
                ((PlayerMobile)m).WaitingForEnemy = false;
            }
        }
    }

    public class HolyLightSpell : PaladinSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Holy Light", "Augus Luminos",
                -1,
                9002
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(1.75); } }

        public override double RequiredSkill { get { return 55.0; } }
        public override int RequiredMana { get { return 10; } }
        public override int RequiredTithing { get { return 10; } }
        public override int MantraNumber { get { return 1060724; } } // Augus Luminos
        public override bool BlocksMovement { get { return false; } }

        public HolyLightSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override bool DelayedDamage { get { return false; } }

        public override void OnCast()
        {
            if (CheckSequence())
            {
                List<Mobile> targets = new List<Mobile>();

                foreach (Mobile m in Caster.GetMobilesInRange(3))
                    if (Caster != m && SpellHelper.ValidIndirectTarget(Caster, m) && Caster.CanBeHarmful(m, false) && (!Core.AOS || Caster.InLOS(m)))
                        targets.Add(m);

                Caster.PlaySound(0x212);
                Caster.PlaySound(0x206);

                Effects.SendLocationParticles(EffectItem.Create(Caster.Location, Caster.Map, EffectItem.DefaultDuration), 0x376A, 1, 29, 0x47D, 2, 9962, 0);
                Effects.SendLocationParticles(EffectItem.Create(new Point3D(Caster.X, Caster.Y, Caster.Z - 7), Caster.Map, EffectItem.DefaultDuration), 0x37C4, 1, 29, 0x47D, 2, 9502, 0);

                for (int i = 0; i < targets.Count; ++i)
                {
                    Mobile m = targets[i];

                    int damage = ComputePowerValue(10) + Utility.RandomMinMax(0, 2);

                    // TODO: Should caps be applied?
                    if (damage < 8)
                        damage = 8;
                    else if (damage > 24)
                        damage = 24;

                    Caster.DoHarmful(m);
                    SpellHelper.Damage(this, m, damage, 0, 0, 0, 0, 100);
                }
            }

            FinishSequence();
        }
    }

    public class NobleSacrificeSpell : PaladinSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Noble Sacrifice", "Dium Prostra",
                -1,
                9002
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(1.5); } }

        public override double RequiredSkill { get { return 65.0; } }
        public override int RequiredMana { get { return 20; } }
        public override int RequiredTithing { get { return 30; } }
        public override int MantraNumber { get { return 1060725; } } // Dium Prostra
        public override bool BlocksMovement { get { return false; } }

        public NobleSacrificeSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            if (CheckSequence())
            {
                List<Mobile> targets = new List<Mobile>();

                foreach (Mobile m in Caster.GetMobilesInRange(3)) // TODO: Validate range
                {
                    if (m is BaseCreature && ((BaseCreature)m).IsAnimatedDead)
                        continue;

                    if (Caster != m && m.InLOS(Caster) && Caster.CanBeBeneficial(m, false, true) && !(m is Golem))
                        targets.Add(m);
                }

                Caster.PlaySound(0x244);
                Caster.FixedParticles(0x3709, 1, 30, 9965, 5, 7, EffectLayer.Waist);
                Caster.FixedParticles(0x376A, 1, 30, 9502, 5, 3, EffectLayer.Waist);

                /* Attempts to Resurrect, Cure and Heal all targets in a radius around the caster.
                 * If any target is successfully assisted, the Paladin's current
                 * Hit Points, Mana and Stamina are set to 1.
                 * Amount of damage healed is affected by the Caster's Karma, from 8 to 24 hit points.
                 */

                bool sacrifice = false;

                // TODO: Is there really a resurrection chance?
                double resChance = 0.1 + (0.9 * ((double)Caster.Karma / 10000));

                for (int i = 0; i < targets.Count; ++i)
                {
                    Mobile m = targets[i];

                    if (!m.Alive)
                    {
                        if (m.Region != null && m.Region.IsPartOf("Khaldun"))
                        {
                            Caster.SendLocalizedMessage(1010395); // The veil of death in this area is too strong and resists thy efforts to restore life.
                        }
                        else if (resChance > Utility.RandomDouble())
                        {
                            m.FixedParticles(0x375A, 1, 15, 5005, 5, 3, EffectLayer.Head);
                            m.CloseGump(typeof(ResurrectGump));
                            m.SendGump(new ResurrectGump(m, Caster));
                            sacrifice = true;
                        }
                    }
                    else
                    {
                        bool sendEffect = false;

                        if (m.Poisoned && m.CurePoison(Caster))
                        {
                            Caster.DoBeneficial(m);

                            if (Caster != m)
                                Caster.SendLocalizedMessage(1010058); // You have cured the target of all poisons!

                            m.SendLocalizedMessage(1010059); // You have been cured of all poisons.
                            sendEffect = true;
                            sacrifice = true;
                        }

                        if (m.Hits < m.HitsMax)
                        {
                            int toHeal = ComputePowerValue(10) + Utility.RandomMinMax(0, 2);

                            // TODO: Should caps be applied?
                            if (toHeal < 8)
                                toHeal = 8;
                            else if (toHeal > 24)
                                toHeal = 24;

                            Caster.DoBeneficial(m);
                            m.Heal(toHeal, Caster);
                            sendEffect = true;
                        }

                        StatMod mod;

                        mod = m.GetStatMod("[Magic] Str Offset");
                        if (mod != null && mod.Offset < 0)
                        {
                            m.RemoveStatMod("[Magic] Str Offset");
                            sendEffect = true;
                        }

                        mod = m.GetStatMod("[Magic] Dex Offset");
                        if (mod != null && mod.Offset < 0)
                        {
                            m.RemoveStatMod("[Magic] Dex Offset");
                            sendEffect = true;
                        }

                        mod = m.GetStatMod("[Magic] Int Offset");
                        if (mod != null && mod.Offset < 0)
                        {
                            m.RemoveStatMod("[Magic] Int Offset");
                            sendEffect = true;
                        }

                        if (m.Paralyzed)
                        {
                            m.Paralyzed = false;
                            sendEffect = true;
                        }

                        if (EvilOmenSpell.TryEndEffect(m))
                            sendEffect = true;

                        if (StrangleSpell.RemoveCurse(m))
                            sendEffect = true;

                        if (CorpseSkinSpell.RemoveCurse(m))
                            sendEffect = true;

                        // TODO: Should this remove blood oath? Pain spike?

                        if (sendEffect)
                        {
                            m.FixedParticles(0x375A, 1, 15, 5005, 5, 3, EffectLayer.Head);
                            sacrifice = true;
                        }
                    }
                }

                if (sacrifice)
                {
                    Caster.PlaySound(Caster.Body.IsFemale ? 0x150 : 0x423);
                    Caster.Hits = 1;
                    Caster.Stam = 1;
                    Caster.Mana = 1;
                }
            }

            FinishSequence();
        }
    }

    public class RemoveCurseSpell : PaladinSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Remove Curse", "Extermo Vomica",
                -1,
                9002
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(1.5); } }

        public override double RequiredSkill { get { return 5.0; } }
        public override int RequiredMana { get { return 20; } }
        public override int RequiredTithing { get { return 10; } }
        public override int MantraNumber { get { return 1060726; } } // Extermo Vomica

        public RemoveCurseSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override bool CheckCast()
        {
            if (Engines.ConPVP.DuelContext.CheckSuddenDeath(Caster))
            {
                Caster.SendMessage(0x22, "You cannot cast this spell when in sudden death.");
                return false;
            }

            return base.CheckCast();
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(Mobile m)
        {
            if (CheckBSequence(m))
            {
                SpellHelper.Turn(Caster, m);

                /* Attempts to remove all Curse effects from Target.
                 * Curses include Mage spells such as Clumsy, Weaken, Feeblemind and Paralyze
                 * as well as all Necromancer curses.
                 * Chance of removing curse is affected by Caster's Karma.
                 */

                int chance = 0;

                if (Caster.Karma < -5000)
                    chance = 0;
                else if (Caster.Karma < 0)
                    chance = (int)Math.Sqrt(20000 + Caster.Karma) - 122;
                else if (Caster.Karma < 5625)
                    chance = (int)Math.Sqrt(Caster.Karma) + 25;
                else
                    chance = 100;

                if (chance > Utility.Random(100))
                {
                    m.PlaySound(0xF6);
                    m.PlaySound(0x1F7);
                    m.FixedParticles(0x3709, 1, 30, 9963, 13, 3, EffectLayer.Head);

                    IEntity from = new Entity(Serial.Zero, new Point3D(m.X, m.Y, m.Z - 10), Caster.Map);
                    IEntity to = new Entity(Serial.Zero, new Point3D(m.X, m.Y, m.Z + 50), Caster.Map);
                    Effects.SendMovingParticles(from, to, 0x2255, 1, 0, false, false, 13, 3, 9501, 1, 0, EffectLayer.Head, 0x100);

                    StatMod mod;

                    mod = m.GetStatMod("[Magic] Str Offset");
                    if (mod != null && mod.Offset < 0)
                        m.RemoveStatMod("[Magic] Str Offset");

                    mod = m.GetStatMod("[Magic] Dex Offset");
                    if (mod != null && mod.Offset < 0)
                        m.RemoveStatMod("[Magic] Dex Offset");

                    mod = m.GetStatMod("[Magic] Int Offset");
                    if (mod != null && mod.Offset < 0)
                        m.RemoveStatMod("[Magic] Int Offset");

                    m.Paralyzed = false;

                    EvilOmenSpell.TryEndEffect(m);
                    StrangleSpell.RemoveCurse(m);
                    CorpseSkinSpell.RemoveCurse(m);
                    CurseSpell.RemoveEffect(m);
                    MortalStrike.EndWound(m);
                    if (Core.ML) { BloodOathSpell.RemoveCurse(m); }
                    MindRotSpell.ClearMindRotScalar(m);

                    BuffInfo.RemoveBuff(m, BuffIcon.Clumsy);
                    BuffInfo.RemoveBuff(m, BuffIcon.FeebleMind);
                    BuffInfo.RemoveBuff(m, BuffIcon.Weaken);
                    BuffInfo.RemoveBuff(m, BuffIcon.Curse);
                    BuffInfo.RemoveBuff(m, BuffIcon.MassCurse);
                    BuffInfo.RemoveBuff(m, BuffIcon.MortalStrike);
                    BuffInfo.RemoveBuff(m, BuffIcon.Mindrot);

                    // TODO: Should this remove blood oath? Pain spike?
                }
                else
                {
                    m.PlaySound(0x1DF);
                }
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private RemoveCurseSpell m_Owner;

            public InternalTarget(RemoveCurseSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Beneficial)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                    m_Owner.Target((Mobile)o);
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class SacredJourneySpell : PaladinSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Sacred Journey", "Sanctum Viatas",
                -1,
                9002
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(1.5); } }

        public override double RequiredSkill { get { return 15.0; } }
        public override int RequiredMana { get { return 10; } }
        public override int RequiredTithing { get { return 15; } }
        public override int MantraNumber { get { return 1060727; } } // Sanctum Viatas
        public override bool BlocksMovement { get { return false; } }

        private RunebookEntry m_Entry;
        private Runebook m_Book;

        public SacredJourneySpell(Mobile caster, Item scroll)
            : this(caster, scroll, null, null)
        {
        }

        public SacredJourneySpell(Mobile caster, Item scroll, RunebookEntry entry, Runebook book)
            : base(caster, scroll, m_Info)
        {
            m_Entry = entry;
            m_Book = book;
        }

        public override void OnCast()
        {
            if (m_Entry == null)
                Caster.Target = new InternalTarget(this);
            else
                Effect(m_Entry.Location, m_Entry.Map, true);
        }

        public override bool CheckCast()
        {
            if (!base.CheckCast())
                return false;

            if (Factions.Sigil.ExistsOn(Caster))
            {
                Caster.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
                return false;
            }
            else if (Caster.Criminal)
            {
                Caster.SendLocalizedMessage(1005561, "", 0x22); // Thou'rt a criminal and cannot escape so easily.
                return false;
            }
            else if (SpellHelper.CheckCombat(Caster))
            {
                Caster.SendLocalizedMessage(1061282); // You cannot use the Sacred Journey ability to flee from combat.
                return false;
            }
            else if (Server.Misc.WeightOverloading.IsOverloaded(Caster))
            {
                Caster.SendLocalizedMessage(502359, "", 0x22); // Thou art too encumbered to move.
                return false;
            }

            return SpellHelper.CheckTravel(Caster, TravelCheckType.RecallFrom);
        }

        public void Effect(Point3D loc, Map map, bool checkMulti)
        {
            if (Factions.Sigil.ExistsOn(Caster))
            {
                Caster.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
            }
            else if (map == null || (!Core.AOS && Caster.Map != map))
            {
                Caster.SendLocalizedMessage(1005569); // You can not recall to another facet.
            }
            else if (!SpellHelper.CheckTravel(Caster, TravelCheckType.RecallFrom))
            {
            }
            else if (!SpellHelper.CheckTravel(Caster, map, loc, TravelCheckType.RecallTo))
            {
            }
            else if (map == Map.Felucca && Caster is PlayerMobile && ((PlayerMobile)Caster).Young)
            {
                Caster.SendLocalizedMessage(1049543); // You decide against traveling to Felucca while you are still young.
            }
            else if (Caster.Kills >= 5 && map != Map.Felucca)
            {
                Caster.SendLocalizedMessage(1019004); // You are not allowed to travel there.
            }
            else if (Caster.Criminal)
            {
                Caster.SendLocalizedMessage(1005561, "", 0x22); // Thou'rt a criminal and cannot escape so easily.
            }
            else if (SpellHelper.CheckCombat(Caster))
            {
                Caster.SendLocalizedMessage(1061282); // You cannot use the Sacred Journey ability to flee from combat.
            }
            else if (Server.Misc.WeightOverloading.IsOverloaded(Caster))
            {
                Caster.SendLocalizedMessage(502359, "", 0x22); // Thou art too encumbered to move.
            }
            else if (!map.CanSpawnMobile(loc.X, loc.Y, loc.Z))
            {
                Caster.SendLocalizedMessage(501942); // That location is blocked.
            }
            else if ((checkMulti && SpellHelper.CheckMulti(loc, map)))
            {
                Caster.SendLocalizedMessage(501942); // That location is blocked.
            }
            else if (m_Book != null && m_Book.CurCharges <= 0)
            {
                Caster.SendLocalizedMessage(502412); // There are no charges left on that item.
            }
            else if (CheckSequence())
            {
                BaseCreature.TeleportPets(Caster, loc, map, true);

                if (m_Book != null)
                    --m_Book.CurCharges;

                Effects.SendLocationParticles(EffectItem.Create(Caster.Location, Caster.Map, EffectItem.DefaultDuration), 0, 0, 0, 5033);

                Caster.PlaySound(0x1FC);
                Caster.MoveToWorld(loc, map);
                Caster.PlaySound(0x1FC);
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private SacredJourneySpell m_Owner;

            public InternalTarget(SacredJourneySpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.None)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is RecallRune)
                {
                    RecallRune rune = (RecallRune)o;

                    if (rune.Marked)
                        m_Owner.Effect(rune.Target, rune.TargetMap, true);
                    else
                        from.SendLocalizedMessage(501805); // That rune is not yet marked.
                }
                else if (o is Runebook)
                {
                    RunebookEntry e = ((Runebook)o).Default;

                    if (e != null)
                        m_Owner.Effect(e.Location, e.Map, true);
                    else
                        from.SendLocalizedMessage(502354); // Target is not marked.
                }
                else if (o is Key && ((Key)o).KeyValue != 0 && ((Key)o).Link is BaseBoat)
                {
                    BaseBoat boat = ((Key)o).Link as BaseBoat;

                    if (!boat.Deleted && boat.CheckKey(((Key)o).KeyValue))
                        m_Owner.Effect(boat.GetMarkedLocation(), boat.Map, false);
                    else
                        from.Send(new MessageLocalized(from.Serial, from.Body, MessageType.Regular, 0x3B2, 3, 502357, from.Name, "")); // I can not recall from that object.
                }
                else if (o is HouseRaffleDeed && ((HouseRaffleDeed)o).ValidLocation())
                {
                    HouseRaffleDeed deed = (HouseRaffleDeed)o;

                    m_Owner.Effect(deed.PlotLocation, deed.PlotFacet, true);
                }
                else
                {
                    from.Send(new MessageLocalized(from.Serial, from.Body, MessageType.Regular, 0x3B2, 3, 502357, from.Name, "")); // I can not recall from that object.
                }
            }

            protected override void OnNonlocalTarget(Mobile from, object o)
            {
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }
}