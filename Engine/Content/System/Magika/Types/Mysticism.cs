using System;
using System.Collections;
using System.Collections.Generic;

using Server;
using Server.Mobiles;
using Server.Network;
using Server.Spells;
using Server.Spells.Fifth;
using Server.Spells.Seventh;
using Server.Targeting;

namespace Server.Spells.Mysticism
{
    public abstract class MysticSpell : Spell
    {
        public abstract double RequiredSkill { get; }
        public abstract int RequiredMana { get; }

        public override SkillName CastSkill { get { return SkillName.Mysticism; } }

        /* 
         * As per OSI Publish 64:
         * Imbuing is not the only skill associated with Mysticism now.
         * Players can use EITHER their Focus skill or Imbuing skill.
         * Evaluate Intelligence no longer has any effect on a Mystic’s spell power.
         */
        public override double GetDamageSkill(Mobile m)
        {
            return Math.Max(m.Skills[SkillName.Imbuing].Value, m.Skills[SkillName.Focus].Value);
        }

        public override int GetDamageFixed(Mobile m)
        {
            return Math.Max(m.Skills[SkillName.Imbuing].Fixed, m.Skills[SkillName.Focus].Fixed);
        }

        public MysticSpell(Mobile caster, Item scroll, SpellInfo info)
            : base(caster, scroll, info)
        {
        }

        public override void GetCastSkills(out double min, out double max)
        {
            // As per Mysticism page at the UO Herald Playguide
            // This means that we have 25% success chance at min Required Skill

            min = RequiredSkill - 12.5;
            max = RequiredSkill + 37.5;
        }

        public override int GetMana()
        {
            return RequiredMana;
        }

        public override bool CheckCast()
        {
            if (!base.CheckCast())
                return false;

            int mana = ScaleMana(RequiredMana);

            if (Caster.Mana < mana)
            {
                Caster.SendLocalizedMessage(1060174, mana.ToString()); // You must have at least ~1_MANA_REQUIREMENT~ Mana to use this ability.
                return false;
            }

            if (Caster.Skills[CastSkill].Value < RequiredSkill)
            {
                Caster.SendLocalizedMessage(1063013, String.Format("{0}\t{1}\t ", RequiredSkill.ToString("F1"), CastSkill.ToString())); // You need at least ~1_SKILL_REQUIREMENT~ ~2_SKILL_NAME~ skill to use that ability.
                return false;
            }

            return true;
        }

        public override void OnBeginCast()
        {
            base.OnBeginCast();

            SendCastEffect();
        }

        public virtual void SendCastEffect()
        {
            Caster.FixedEffect(0x37C4, 10, (int)(GetCastDelay().TotalSeconds * 28), 0x66C, 3);
        }

        public static double GetBaseSkill(Mobile m)
        {
            return m.Skills[SkillName.Mysticism].Value;
        }

        public static double GetBoostSkill(Mobile m)
        {
            return Math.Max(m.Skills[SkillName.Imbuing].Value, m.Skills[SkillName.Focus].Value);
        }
    }

    public class AnimatedWeaponSpell : MysticSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Animated Weapon", "In Jux Por Ylem",
                -1,
                9002,
                Reagent.Bone,
                Reagent.BlackPearl,
                Reagent.MandrakeRoot,
                Reagent.Nightshade
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(1.5); } }

        public override double RequiredSkill { get { return 33.0; } }
        public override int RequiredMana { get { return 11; } }

        public AnimatedWeaponSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(IPoint3D p)
        {
            if ((Caster.Followers + 4) > Caster.FollowersMax)
            {
                Caster.SendLocalizedMessage(1049645); // You have too many followers to summon that creature.
                return;
            }

            var map = Caster.Map;

            SpellHelper.GetSurfaceTop(ref p);

            if (map == null || (Caster.Player && !map.CanSpawnMobile(p.X, p.Y, p.Z)))
            {
                Caster.SendLocalizedMessage(501942); // That location is blocked.
            }
            else if (SpellHelper.CheckTown(p, Caster) && CheckSequence())
            {
                var level = (int)((GetBaseSkill(Caster) + GetBoostSkill(Caster)) / 2.0);

                var duration = TimeSpan.FromSeconds(10 + level);

                var summon = new AnimatedWeapon(Caster, level);
                BaseCreature.Summon(summon, false, Caster, new Point3D(p), 0x212, duration);

                summon.PlaySound(0x64A);

                Effects.SendTargetParticles(summon, 0x3728, 10, 10, 0x13AA, (EffectLayer)255);
            }

            FinishSequence();
        }

        public class InternalTarget : Target
        {
            private AnimatedWeaponSpell m_Owner;

            public InternalTarget(AnimatedWeaponSpell owner)
                : base(12, true, TargetFlags.None)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is IPoint3D)
                    m_Owner.Target((IPoint3D)o);
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class EagleStrikeSpell : MysticSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Eagle Strike", "Kal Por Xen",
                -1,
                9002,
                Reagent.Bloodmoss,
                Reagent.Bone,
                Reagent.SpidersSilk,
                Reagent.MandrakeRoot
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(1.25); } }

        public override double RequiredSkill { get { return 20.0; } }
        public override int RequiredMana { get { return 9; } }

        public EagleStrikeSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(Mobile m)
        {
            if (CheckHSequence(m))
            {
                /* Conjures a magical eagle that assaults the Target with
                 * its talons, dealing energy damage.
                 */

                SpellHelper.Turn(Caster, m);

                SpellHelper.CheckReflect(2, Caster, ref m);

                Caster.MovingParticles(m, 0x407A, 7, 0, false, true, 0, 0, 0xBBE, 0xFA6, 0xFFFF, 0);
                Caster.PlaySound(0x2EE);

                Timer.DelayCall(TimeSpan.FromSeconds(1.0), Damage, m);
            }

            FinishSequence();
        }

        private void Damage(Mobile to)
        {
            if (to == null)
                return;

            double damage = GetNewAosDamage(19, 1, 5, to);

            SpellHelper.Damage(this, to, damage, 0, 0, 0, 0, 100);

            to.PlaySound(0x64D);
        }

        private class InternalTarget : Target
        {
            private EagleStrikeSpell m_Owner;

            public InternalTarget(EagleStrikeSpell owner)
                : base(12, false, TargetFlags.Harmful)
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

    public class HailStormSpell : MysticSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Hail Storm", "Kal Des Ylem",
                -1,
                9002,
                Reagent.DragonsBlood,
                Reagent.Bloodmoss,
                Reagent.BlackPearl,
                Reagent.MandrakeRoot
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(2.25); } }

        public override double RequiredSkill { get { return 70.0; } }
        public override int RequiredMana { get { return 40; } }

        public HailStormSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(IPoint3D p)
        {
            if (SpellHelper.CheckTown(p, Caster) && CheckSequence())
            {
                /* Summons a storm of hailstones that strikes all Targets
                 * within a radius around the Target's Location, dealing
                 * cold damage.
                 */

                SpellHelper.Turn(Caster, p);

                if (p is Item)
                    p = ((Item)p).GetWorldLocation();

                var targets = new List<Mobile>();

                var map = Caster.Map;

                var pvp = false;

                if (map != null)
                {
                    PlayEffect(p, Caster.Map);

                    foreach (var m in map.GetMobilesInRange(new Point3D(p), 2))
                    {
                        if (m == Caster)
                            continue;

                        if (SpellHelper.ValidIndirectTarget(Caster, m) && Caster.CanBeHarmful(m, false) && Caster.CanSee(m))
                        {
                            if (!Caster.InLOS(m))
                                continue;

                            targets.Add(m);

                            if (m.Player)
                                pvp = true;
                        }
                    }
                }

                double damage = GetNewAosDamage(51, 1, 5, pvp);

                foreach (var m in targets)
                {
                    Caster.DoHarmful(m);
                    SpellHelper.Damage(this, m, damage, 0, 0, 100, 0, 0);
                }
            }

            FinishSequence();
        }

        private static void PlayEffect(IPoint3D p, Map map)
        {
            Effects.PlaySound(p, map, 0x64F);

            PlaySingleEffect(p, map, -1, 1, -1, 1);
            PlaySingleEffect(p, map, -2, 0, -3, -1);
            PlaySingleEffect(p, map, -3, -1, -1, 1);
            PlaySingleEffect(p, map, 1, 3, -1, 1);
            PlaySingleEffect(p, map, -1, 1, 1, 3);
        }

        private static void PlaySingleEffect(IPoint3D p, Map map, int a, int b, int c, int d)
        {
            int x = p.X, y = p.Y, z = p.Z + 18;

            SendEffectPacket(p, map, new Point3D(x + a, y + c, z), new Point3D(x + a, y + c, z));
            SendEffectPacket(p, map, new Point3D(x + b, y + c, z), new Point3D(x + b, y + c, z));
            SendEffectPacket(p, map, new Point3D(x + b, y + d, z), new Point3D(x + b, y + d, z));
            SendEffectPacket(p, map, new Point3D(x + a, y + d, z), new Point3D(x + a, y + d, z));

            SendEffectPacket(p, map, new Point3D(x + b, y + c, z), new Point3D(x + a, y + c, z));
            SendEffectPacket(p, map, new Point3D(x + b, y + d, z), new Point3D(x + b, y + c, z));
            SendEffectPacket(p, map, new Point3D(x + a, y + d, z), new Point3D(x + b, y + d, z));
            SendEffectPacket(p, map, new Point3D(x + a, y + c, z), new Point3D(x + a, y + d, z));
        }

        private static void SendEffectPacket(IPoint3D p, Map map, Point3D orig, Point3D dest)
        {
            Effects.SendPacket(p, map, new HuedEffect(EffectType.Moving, Serial.Zero, Serial.Zero, 0x36D4, orig, dest, 0, 0, false, false, 0x63, 0x4));
        }

        private class InternalTarget : Target
        {
            private HailStormSpell m_Owner;

            public InternalTarget(HailStormSpell owner)
                : base(12, true, TargetFlags.None)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                var p = o as IPoint3D;

                if (p != null)
                    m_Owner.Target(p);
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class NetherCycloneSpell : MysticSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Nether Cyclone", "Grav Hur",
                -1,
                9002,
                Reagent.MandrakeRoot,
                Reagent.Nightshade,
                Reagent.SulfurousAsh,
                Reagent.Bloodmoss
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(2.5); } }

        public override double RequiredSkill { get { return 83.0; } }
        public override int RequiredMana { get { return 50; } }

        public NetherCycloneSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(IPoint3D p)
        {
            if (SpellHelper.CheckTown(p, Caster) && CheckSequence())
            {
                /* Summons a gale of lethal winds that strikes all Targets within a radius around
                 * the Target's Location, dealing chaos damage. In addition to inflicting damage,
                 * each Target of the Nether Cyclone temporarily loses a percentage of mana and
                 * stamina. The effectiveness of the Nether Cyclone is determined by a comparison
                 * between the Caster's Mysticism and either Focus or Imbuing (whichever is greater)
                 * skills and the Resisting Spells skill of the Target.
                 */

                SpellHelper.Turn(Caster, p);

                if (p is Item)
                    p = ((Item)p).GetWorldLocation();

                var targets = new List<Mobile>();

                var map = Caster.Map;

                var pvp = false;

                if (map != null)
                {
                    PlayEffect(p, Caster.Map);

                    foreach (var m in map.GetMobilesInRange(new Point3D(p), 2))
                    {
                        if (m == Caster)
                            continue;

                        if (SpellHelper.ValidIndirectTarget(Caster, m) && Caster.CanBeHarmful(m, false) && Caster.CanSee(m))
                        {
                            if (!Caster.InLOS(m))
                                continue;

                            targets.Add(m);

                            if (m.Player)
                                pvp = true;
                        }
                    }
                }

                var damage = GetNewAosDamage(51, 1, 5, pvp);
                var reduction = (GetBaseSkill(Caster) + GetBoostSkill(Caster)) / 1200.0;

                foreach (var m in targets)
                {
                    Caster.DoHarmful(m);

                    var types = new int[4];
                    types[Utility.Random(types.Length)] = 100;

                    SpellHelper.Damage(this, m, damage, 0, types[0], types[1], types[2], types[3]);

                    var resistedReduction = reduction - (m.Skills[SkillName.MagicResist].Value / 800.0);

                    m.Stam -= (int)(m.StamMax * resistedReduction);
                    m.Mana -= (int)(m.ManaMax * resistedReduction);
                }
            }

            FinishSequence();
        }

        private static void PlayEffect(IPoint3D p, Map map)
        {
            Effects.PlaySound(p, map, 0x64F);

            PlaySingleEffect(p, map, -1, 1, -1, 1);
            PlaySingleEffect(p, map, -2, 0, -3, -1);
            PlaySingleEffect(p, map, -3, -1, -1, 1);
            PlaySingleEffect(p, map, 1, 3, -1, 1);
            PlaySingleEffect(p, map, -1, 1, 1, 3);
        }

        private static void PlaySingleEffect(IPoint3D p, Map map, int a, int b, int c, int d)
        {
            int x = p.X, y = p.Y, z = p.Z + 18;

            SendEffectPacket(p, map, new Point3D(x + a, y + c, z), new Point3D(x + a, y + c, z));
            SendEffectPacket(p, map, new Point3D(x + b, y + c, z), new Point3D(x + b, y + c, z));
            SendEffectPacket(p, map, new Point3D(x + b, y + d, z), new Point3D(x + b, y + d, z));
            SendEffectPacket(p, map, new Point3D(x + a, y + d, z), new Point3D(x + a, y + d, z));

            SendEffectPacket(p, map, new Point3D(x + b, y + c, z), new Point3D(x + a, y + c, z));
            SendEffectPacket(p, map, new Point3D(x + b, y + d, z), new Point3D(x + b, y + c, z));
            SendEffectPacket(p, map, new Point3D(x + a, y + d, z), new Point3D(x + b, y + d, z));
            SendEffectPacket(p, map, new Point3D(x + a, y + c, z), new Point3D(x + a, y + d, z));
        }

        private static void SendEffectPacket(IPoint3D p, Map map, Point3D orig, Point3D dest)
        {
            Effects.SendPacket(p, map, new HuedEffect(EffectType.Moving, Serial.Zero, Serial.Zero, 0x375A, orig, dest, 0, 0, false, false, 0x49A, 0x4));
        }

        private class InternalTarget : Target
        {
            private NetherCycloneSpell m_Owner;

            public InternalTarget(NetherCycloneSpell owner)
                : base(12, true, TargetFlags.None)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                var p = o as IPoint3D;

                if (p != null)
                    m_Owner.Target(p);
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class SpellPlagueSpell : MysticSpell
    {
        public static void Initialize()
        {
            EventSink.PlayerDeath += new PlayerDeathEventHandler(OnPlayerDeath);
        }

        private static SpellInfo m_Info = new SpellInfo(
                "Spell Plague", "Vas Rel Jux Ort",
                -1,
                9002,
                Reagent.DaemonBone,
                Reagent.DragonsBlood,
                Reagent.Nightshade,
                Reagent.SulfurousAsh
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(2.25); } }

        public override double RequiredSkill { get { return 70.0; } }
        public override int RequiredMana { get { return 40; } }

        public SpellPlagueSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(Mobile targeted)
        {
            if (!Caster.CanSee(targeted))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckHSequence(targeted))
            {
                SpellHelper.Turn(Caster, targeted);

                SpellHelper.CheckReflect(6, Caster, ref targeted);

                /* The target is hit with an explosion of chaos damage and then inflicted
                 * with the spell plague curse. Each time the target is damaged while under
                 * the effect of the spell plague, they may suffer an explosion of chaos
                 * damage. The initial chance to trigger the explosion starts at 90% and
                 * reduces by 30% every time an explosion occurs. Once the target is
                 * afflicted by 3 explosions or 8 seconds have passed, that spell plague
                 * is removed from the target. Spell Plague will stack with other spell
                 * plagues so that they are applied one after the other.
                 */

                VisualEffect(targeted);

                var damage = GetNewAosDamage(33, 1, 5, targeted);

                var types = new int[4];
                types[Utility.Random(types.Length)] = 100;

                SpellHelper.Damage(this, targeted, damage, 0, types[0], types[1], types[2], types[3]);

                var context = new SpellPlagueContext(this, targeted);

                if (m_Table.ContainsKey(targeted))
                {
                    var oldContext = m_Table[targeted];
                    oldContext.SetNext(context);
                }
                else
                {
                    m_Table[targeted] = context;
                    context.Start();
                }
            }

            FinishSequence();
        }

        public static bool UnderEffect(Mobile m)
        {
            return m_Table.ContainsKey(m);
        }

        public static void RemoveEffect(Mobile m)
        {
            if (!m_Table.ContainsKey(m))
                return;

            var context = m_Table[m];

            context.EndPlague(false);
        }

        public static void CheckPlague(Mobile m)
        {
            if (!m_Table.ContainsKey(m))
                return;

            var context = m_Table[m];

            context.OnDamage();
        }

        private static void OnPlayerDeath(PlayerDeathEventArgs e)
        {
            RemoveEffect(e.Mobile);
        }

        private static Dictionary<Mobile, SpellPlagueContext> m_Table = new Dictionary<Mobile, SpellPlagueContext>();

        protected void VisualEffect(Mobile to)
        {
            to.PlaySound(0x658);

            to.FixedParticles(0x3728, 1, 13, 0x26B8, 0x47E, 7, EffectLayer.Head, 0);
            to.FixedParticles(0x3779, 1, 15, 0x251E, 0x43, 7, EffectLayer.Head, 0);
        }

        private class SpellPlagueContext
        {
            private SpellPlagueSpell m_Owner;
            private Mobile m_Target;
            private DateTime m_LastExploded;
            private int m_Explosions;
            private Timer m_Timer;
            private SpellPlagueContext m_Next;

            public SpellPlagueContext(SpellPlagueSpell owner, Mobile target)
            {
                m_Owner = owner;
                m_Target = target;
            }

            public void SetNext(SpellPlagueContext context)
            {
                if (m_Next == null)
                    m_Next = context;
                else
                    m_Next.SetNext(context);
            }

            public void Start()
            {
                m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(8.0), new TimerCallback(EndPlague));
                m_Timer.Start();

                BuffInfo.AddBuff(m_Target, new BuffInfo(BuffIcon.SpellPlague, 1031690, 1080167, TimeSpan.FromSeconds(8.5), m_Target));
            }

            public void OnDamage()
            {
                if (DateTime.Now > (m_LastExploded + TimeSpan.FromSeconds(2.0)))
                {
                    var exploChance = 90 - (m_Explosions * 30);

                    var resist = m_Target.Skills[SkillName.MagicResist].Value;

                    if (resist >= 70)
                        exploChance -= (int)((resist - 70.0) * 3.0 / 10.0);

                    if (exploChance > Utility.Random(100))
                    {
                        m_Owner.VisualEffect(m_Target);

                        var damage = m_Owner.GetNewAosDamage(15 + (m_Explosions * 3), 1, 5, m_Target);

                        m_Explosions++;
                        m_LastExploded = DateTime.Now;

                        var types = new int[4];
                        types[Utility.Random(types.Length)] = 100;

                        SpellHelper.Damage(m_Owner, m_Target, damage, 0, types[0], types[1], types[2], types[3]);

                        if (m_Explosions >= 3)
                            EndPlague();
                    }
                }
            }

            private void EndPlague()
            {
                EndPlague(true);
            }

            public void EndPlague(bool restart)
            {
                if (m_Timer != null)
                    m_Timer.Stop();

                if (restart && m_Next != null)
                {
                    m_Table[m_Target] = m_Next;
                    m_Next.Start();
                }
                else
                {
                    m_Table.Remove(m_Target);

                    BuffInfo.RemoveBuff(m_Target, BuffIcon.SpellPlague);
                }
            }
        }

        private class InternalTarget : Target
        {
            private SpellPlagueSpell m_Owner;

            public InternalTarget(SpellPlagueSpell owner)
                : base(12, false, TargetFlags.Harmful)
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

    public class StoneFormSpell : MysticSpell
    {
        public static void Initialize()
        {
            EventSink.PlayerDeath += OnPlayerDeath;
        }

        private static SpellInfo m_Info = new SpellInfo(
                "Stone Form", "In Rel Ylem",
                -1,
                9002,
                Reagent.Bloodmoss,
                Reagent.FertileDirt,
                Reagent.Garlic
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(1.5); } }

        public override double RequiredSkill { get { return 33.0; } }
        public override int RequiredMana { get { return 11; } }

        private static Hashtable m_Table = new Hashtable();

        public static bool UnderEffect(Mobile m)
        {
            return m_Table.Contains(m);
        }

        public StoneFormSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override bool CheckCast()
        {
            if (Factions.Sigil.ExistsOn(Caster))
            {
                Caster.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
                return false;
            }
            else if (!Caster.CanBeginAction(typeof(PolymorphSpell)))
            {
                Caster.SendLocalizedMessage(1061628); // You can't do that while polymorphed.
                return false;
            }
            else if (Ninjitsu.AnimalForm.UnderTransformation(Caster))
            {
                Caster.SendLocalizedMessage(1063218); // You cannot use that ability in this form.
                return false;
            }
            else if (Caster.Flying)
            {
                Caster.SendLocalizedMessage(1113415); // You cannot use this ability while flying.
                return false;
            }

            return base.CheckCast();
        }

        public override void OnCast()
        {
            if (Factions.Sigil.ExistsOn(Caster))
            {
                Caster.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
            }
            else if (!Caster.CanBeginAction(typeof(PolymorphSpell)))
            {
                Caster.SendLocalizedMessage(1061628); // You can't do that while polymorphed.
            }
            else if (!Caster.CanBeginAction(typeof(IncognitoSpell)) || (Caster.IsBodyMod && !UnderEffect(Caster)))
            {
                Caster.SendLocalizedMessage(1063218); // You cannot use that ability in this form.
            }
            else if (CheckSequence())
            {
                if (UnderEffect(Caster))
                {
                    RemoveEffects(Caster);

                    Caster.PlaySound(0xFA);
                    Caster.Delta(MobileDelta.Resistances);
                }
                else
                {
                    var mount = Caster.Mount;

                    if (mount != null)
                        mount.Rider = null;

                    Caster.BodyMod = 0x2C1;
                    Caster.HueMod = 0;

                    var offset = (int)((GetBaseSkill(Caster) + GetBoostSkill(Caster)) / 24.0);

                    var mods = new List<ResistanceMod>
					{
						new ResistanceMod( ResistanceType.Physical, offset ),
						new ResistanceMod( ResistanceType.Fire, offset ),
						new ResistanceMod( ResistanceType.Cold, offset ),
						new ResistanceMod( ResistanceType.Poison, offset ),
						new ResistanceMod( ResistanceType.Energy, offset )
					};

                    foreach (var mod in mods)
                        Caster.AddResistanceMod(mod);

                    m_Table[Caster] = mods;

                    Caster.PlaySound(0x65A);
                    Caster.Delta(MobileDelta.Resistances);

                    BuffInfo.AddBuff(Caster, new BuffInfo(BuffIcon.StoneForm, 1080145, 1080146,
                        string.Format("-10\t-2\t{0}\t{1}\t{2}", offset, GetResistCapBonus(Caster), GetDIBonus(Caster)), false));
                }
            }

            FinishSequence();
        }

        public static int GetDIBonus(Mobile m)
        {
            return (int)((GetBaseSkill(m) + GetBoostSkill(m)) / 12.0);
        }

        public static int GetResistCapBonus(Mobile m)
        {
            return (int)((GetBaseSkill(m) + GetBoostSkill(m)) / 48.0);
        }

        public static void RemoveEffects(Mobile m)
        {
            var mods = (List<ResistanceMod>)m_Table[m];

            foreach (var mod in mods)
                m.RemoveResistanceMod(mod);

            m.BodyMod = 0;
            m.HueMod = -1;

            m_Table.Remove(m);

            BuffInfo.RemoveBuff(m, BuffIcon.StoneForm);
        }

        private static void OnPlayerDeath(PlayerDeathEventArgs e)
        {
            var m = e.Mobile;

            if (UnderEffect(m))
                RemoveEffects(m);
        }
    }
}