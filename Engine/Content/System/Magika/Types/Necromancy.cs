using System;
using System.Collections;
using System.Collections.Generic;

using Server;
using Server.Engines.CannedEvil;
using Server.Engines.PartySystem;
using Server.Engines.Quests;
using Server.Engines.Quests.Necro;
using Server.Factions;
using Server.Guilds;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Regions;
using Server.Spells.Fifth;
using Server.Spells.Seventh;
using Server.Targeting;

namespace Server.Spells.Necromancy
{
    public abstract class NecromancerSpell : Spell
    {
        public abstract double RequiredSkill { get; }
        public abstract int RequiredMana { get; }

        public override SkillName CastSkill { get { return SkillName.Necromancy; } }
        public override SkillName DamageSkill { get { return SkillName.SpiritSpeak; } }

        //public override int CastDelayBase{ get{ return base.CastDelayBase; } } // Reference, 3

        public override bool ClearHandsOnCast { get { return false; } }

        public override double CastDelayFastScalar { get { return (Core.SE ? base.CastDelayFastScalar : 0); } } // Necromancer spells are not affected by fast cast items, though they are by fast cast recovery

        public NecromancerSpell(Mobile caster, Item scroll, SpellInfo info)
            : base(caster, scroll, info)
        {
        }

        public override int ComputeKarmaAward()
        {
            //TODO: Verify this formula being that Necro spells don't HAVE a circle.
            //int karma = -(70 + (10 * (int)Circle));
            int karma = -(40 + (int)(10 * (CastDelayBase.TotalSeconds / CastDelaySecondsPerTick)));

            if (Core.ML) // Pub 36: "Added a new property called Increased Karma Loss which grants higher karma loss for casting necromancy spells."
                karma += AOS.Scale(karma, AosAttributes.GetValue(Caster, AosAttribute.IncreasedKarmaLoss));

            return karma;
        }

        public override void GetCastSkills(out double min, out double max)
        {
            min = RequiredSkill;
            max = Scroll != null ? min : RequiredSkill + 40.0;
        }

        public override bool ConsumeReagents()
        {
            if (base.ConsumeReagents())
                return true;

            if (ArcaneGem.ConsumeCharges(Caster, 1))
                return true;

            return false;
        }

        public override int GetMana()
        {
            return RequiredMana;
        }
    }

    public abstract class TransformationSpell : NecromancerSpell, ITransformationSpell
    {
        public abstract int Body { get; }
        public virtual int Hue { get { return 0; } }

        public virtual int PhysResistOffset { get { return 0; } }
        public virtual int FireResistOffset { get { return 0; } }
        public virtual int ColdResistOffset { get { return 0; } }
        public virtual int PoisResistOffset { get { return 0; } }
        public virtual int NrgyResistOffset { get { return 0; } }

        public TransformationSpell(Mobile caster, Item scroll, SpellInfo info)
            : base(caster, scroll, info)
        {
        }

        public override bool BlockedByHorrificBeast { get { return false; } }

        public override bool CheckCast()
        {
            if (!TransformationSpellHelper.CheckCast(Caster, this))
                return false;

            return base.CheckCast();
        }

        public override void OnCast()
        {
            TransformationSpellHelper.OnCast(Caster, this);

            FinishSequence();
        }

        public virtual double TickRate { get { return 1.0; } }

        public virtual void OnTick(Mobile m)
        {
        }

        public virtual void DoEffect(Mobile m)
        {
        }

        public virtual void RemoveEffect(Mobile m)
        {
        }
    }

    public class AnimateDeadSpell : NecromancerSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Animate Dead", "Uus Corp",
                203,
                9031,
                Reagent.GraveDust,
                Reagent.DaemonBlood
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(1.5); } }

        public override double RequiredSkill { get { return 40.0; } }
        public override int RequiredMana { get { return 23; } }

        public AnimateDeadSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
            Caster.SendLocalizedMessage(1061083); // Animate what corpse?
        }

        private class CreatureGroup
        {
            public Type[] m_Types;
            public SummonEntry[] m_Entries;

            public CreatureGroup(Type[] types, SummonEntry[] entries)
            {
                m_Types = types;
                m_Entries = entries;
            }
        }

        private class SummonEntry
        {
            public Type[] m_ToSummon;
            public int m_Requirement;

            public SummonEntry(int requirement, params Type[] toSummon)
            {
                m_ToSummon = toSummon;
                m_Requirement = requirement;
            }
        }

        private static CreatureGroup FindGroup(Type type)
        {
            for (int i = 0; i < m_Groups.Length; ++i)
            {
                CreatureGroup group = m_Groups[i];
                Type[] types = group.m_Types;

                bool contains = (types.Length == 0);

                for (int j = 0; !contains && j < types.Length; ++j)
                    contains = types[j].IsAssignableFrom(type);

                if (contains)
                    return group;
            }

            return null;
        }

        private static CreatureGroup[] m_Groups = new CreatureGroup[]
			{
				// Undead group--empty
				new CreatureGroup( SlayerGroup.GetEntryByName( SlayerName.Silver ).Types, new SummonEntry[0] ),
				// Insects
				new CreatureGroup( new Type[]
				{
					typeof( DreadSpider ), typeof( FrostSpider ), typeof( GiantSpider ), typeof( GiantBlackWidow ),
					typeof( BlackSolenInfiltratorQueen ), typeof( BlackSolenInfiltratorWarrior ),
					typeof( BlackSolenQueen ), typeof( BlackSolenWarrior ), typeof( BlackSolenWorker ),
					typeof( RedSolenInfiltratorQueen ), typeof( RedSolenInfiltratorWarrior ),
					typeof( RedSolenQueen ), typeof( RedSolenWarrior ), typeof( RedSolenWorker ),
					typeof( TerathanAvenger ), typeof( TerathanDrone ), typeof( TerathanMatriarch ),
					typeof( TerathanWarrior )
					// TODO: Giant beetle? Ant lion? Ophidians?
				},
				new SummonEntry[]
				{
					new SummonEntry( 0, typeof( MoundOfMaggots ) )
				} ),
				// Mounts
				new CreatureGroup( new Type[]
				{
					typeof( Horse ), typeof( Nightmare ), typeof( FireSteed ),
					typeof( Kirin ), typeof( Unicorn )
				}, new SummonEntry[]
				{
					new SummonEntry( 10000, typeof( HellSteed ) ),
					new SummonEntry(     0, typeof( SkeletalMount ) )
				} ),
				// Elementals
				new CreatureGroup( new Type[]
				{
					typeof( BloodElemental ), typeof( EarthElemental ), typeof( SummonedEarthElemental ),
					typeof( AgapiteElemental ), typeof( BronzeElemental ), typeof( CopperElemental ),
					typeof( DullCopperElemental ), typeof( GoldenElemental ), typeof( ShadowIronElemental ),
					typeof( ValoriteElemental ), typeof( VeriteElemental ), typeof( PoisonElemental ),
					typeof( FireElemental ), typeof( SummonedFireElemental ), typeof( SnowElemental ),
					typeof( AirElemental ), typeof( SummonedAirElemental ), typeof( WaterElemental ),
					typeof( SummonedAirElemental ), typeof ( AcidElemental )
				}, new SummonEntry[]
				{
					new SummonEntry( 5000, typeof( WailingBanshee ) ),
					new SummonEntry(    0, typeof( Wraith ) )
				} ),
				// Dragons
				new CreatureGroup( new Type[]
				{
					typeof( AncientWyrm ), typeof( Dragon ), typeof( GreaterDragon ), typeof( SerpentineDragon ),
					typeof( ShadowWyrm ), typeof( SkeletalDragon ), typeof( WhiteWyrm ),
					typeof( Drake ), typeof( Wyvern ), typeof( LesserHiryu ), typeof( Hiryu )
				}, new SummonEntry[]
				{
					new SummonEntry( 18000, typeof( SkeletalDragon ) ),
					new SummonEntry( 10000, typeof( FleshGolem ) ),
					new SummonEntry(  5000, typeof( Lich ) ),
					new SummonEntry(  3000, typeof( SkeletalKnight ), typeof( BoneKnight ) ),
					new SummonEntry(  2000, typeof( Mummy ) ),
					new SummonEntry(  1000, typeof( SkeletalMage ), typeof( BoneMagi ) ),
					new SummonEntry(     0, typeof( PatchworkSkeleton ) )
				} ),
				// Default group
				new CreatureGroup( new Type[0], new SummonEntry[]
				{
					new SummonEntry( 18000, typeof( LichLord ) ),
					new SummonEntry( 10000, typeof( FleshGolem ) ),
					new SummonEntry(  5000, typeof( Lich ) ),
					new SummonEntry(  3000, typeof( SkeletalKnight ), typeof( BoneKnight ) ),
					new SummonEntry(  2000, typeof( Mummy ) ),
					new SummonEntry(  1000, typeof( SkeletalMage ), typeof( BoneMagi ) ),
					new SummonEntry(     0, typeof( PatchworkSkeleton ) )
				} ),
			};

        public void Target(object obj)
        {
            MaabusCoffinComponent comp = obj as MaabusCoffinComponent;

            if (comp != null)
            {
                MaabusCoffin addon = comp.Addon as MaabusCoffin;

                if (addon != null)
                {
                    PlayerMobile pm = Caster as PlayerMobile;

                    if (pm != null)
                    {
                        QuestSystem qs = pm.Quest;

                        if (qs is DarkTidesQuest)
                        {
                            QuestObjective objective = qs.FindObjective(typeof(AnimateMaabusCorpseObjective));

                            if (objective != null && !objective.Completed)
                            {
                                addon.Awake(Caster);
                                objective.Complete();
                            }
                        }
                    }

                    return;
                }
            }

            Corpse c = obj as Corpse;

            if (c == null)
            {
                Caster.SendLocalizedMessage(1061084); // You cannot animate that.
            }
            else
            {
                Type type = null;

                if (c.Owner != null)
                {
                    type = c.Owner.GetType();
                }

                if (c.ItemID != 0x2006 || c.Animated || type == typeof(PlayerMobile) || type == null || (c.Owner != null && c.Owner.Fame < 100) || ((c.Owner != null) && (c.Owner is BaseCreature) && (((BaseCreature)c.Owner).Summoned || ((BaseCreature)c.Owner).IsBonded)))
                {
                    Caster.SendLocalizedMessage(1061085); // There's not enough life force there to animate.
                }
                else
                {
                    CreatureGroup group = FindGroup(type);

                    if (group != null)
                    {
                        if (group.m_Entries.Length == 0 || type == typeof(DemonKnight))
                        {
                            Caster.SendLocalizedMessage(1061086); // You cannot animate undead remains.
                        }
                        else if (CheckSequence())
                        {
                            Point3D p = c.GetWorldLocation();
                            Map map = c.Map;

                            if (map != null)
                            {
                                Effects.PlaySound(p, map, 0x1FB);
                                Effects.SendLocationParticles(EffectItem.Create(p, map, EffectItem.DefaultDuration), 0x3789, 1, 40, 0x3F, 3, 9907, 0);

                                Timer.DelayCall(TimeSpan.FromSeconds(2.0), new TimerStateCallback(SummonDelay_Callback), new object[] { Caster, c, p, map, group });
                            }
                        }
                    }
                }
            }

            FinishSequence();
        }

        private static Dictionary<Mobile, List<Mobile>> m_Table = new Dictionary<Mobile, List<Mobile>>();

        public static void Unregister(Mobile master, Mobile summoned)
        {
            if (master == null)
                return;

            List<Mobile> list = null;
            m_Table.TryGetValue(master, out list);

            if (list == null)
                return;

            list.Remove(summoned);

            if (list.Count == 0)
                m_Table.Remove(master);
        }

        public static void Register(Mobile master, Mobile summoned)
        {
            if (master == null)
                return;

            List<Mobile> list = null;
            m_Table.TryGetValue(master, out list);

            if (list == null)
                m_Table[master] = list = new List<Mobile>();

            for (int i = list.Count - 1; i >= 0; --i)
            {
                if (i >= list.Count)
                    continue;

                Mobile mob = list[i];

                if (mob.Deleted)
                    list.RemoveAt(i--);
            }

            list.Add(summoned);

            if (list.Count > 3)
                Timer.DelayCall(TimeSpan.Zero, new TimerCallback(list[0].Kill));

            Timer.DelayCall(TimeSpan.FromSeconds(2.0), TimeSpan.FromSeconds(2.0), new TimerStateCallback(Summoned_Damage), summoned);
        }

        private static void Summoned_Damage(object state)
        {
            Mobile mob = (Mobile)state;

            if (mob.Hits > 0)
                --mob.Hits;
            else
                mob.Kill();
        }

        private static void SummonDelay_Callback(object state)
        {
            object[] states = (object[])state;

            Mobile caster = (Mobile)states[0];
            Corpse corpse = (Corpse)states[1];
            Point3D loc = (Point3D)states[2];
            Map map = (Map)states[3];
            CreatureGroup group = (CreatureGroup)states[4];

            if (corpse.Animated)
                return;

            Mobile owner = corpse.Owner;

            if (owner == null)
                return;

            double necromancy = caster.Skills[SkillName.Necromancy].Value;
            double spiritSpeak = caster.Skills[SkillName.SpiritSpeak].Value;

            int casterAbility = 0;

            casterAbility += (int)(necromancy * 30);
            casterAbility += (int)(spiritSpeak * 70);
            casterAbility /= 10;
            casterAbility *= 18;

            if (casterAbility > owner.Fame)
                casterAbility = owner.Fame;

            if (casterAbility < 0)
                casterAbility = 0;

            Type toSummon = null;
            SummonEntry[] entries = group.m_Entries;

            for (int i = 0; toSummon == null && i < entries.Length; ++i)
            {
                SummonEntry entry = entries[i];

                if (casterAbility < entry.m_Requirement)
                    continue;

                Type[] animates = entry.m_ToSummon;

                if (animates.Length >= 0)
                    toSummon = animates[Utility.Random(animates.Length)];
            }

            if (toSummon == null)
                return;

            Mobile summoned = null;

            try { summoned = Activator.CreateInstance(toSummon) as Mobile; }
            catch { }

            if (summoned == null)
                return;

            if (summoned is BaseCreature)
            {
                BaseCreature bc = (BaseCreature)summoned;

                // to be sure
                bc.Tamable = false;

                if (bc is BaseMount)
                    bc.ControlSlots = 1;
                else
                    bc.ControlSlots = 0;

                Effects.PlaySound(loc, map, bc.GetAngerSound());

                BaseCreature.Summon((BaseCreature)summoned, false, caster, loc, 0x28, TimeSpan.FromDays(1.0));
            }

            if (summoned is SkeletalDragon)
                Scale((SkeletalDragon)summoned, 50); // lose 50% hp and strength

            summoned.Fame = 0;
            summoned.Karma = -1500;

            summoned.MoveToWorld(loc, map);

            corpse.Hue = 1109;
            corpse.Animated = true;

            Register(caster, summoned);
        }

        public static void Scale(BaseCreature bc, int scalar)
        {
            int toScale;

            toScale = bc.RawStr;
            bc.RawStr = AOS.Scale(toScale, scalar);

            toScale = bc.HitsMaxSeed;

            if (toScale > 0)
                bc.HitsMaxSeed = AOS.Scale(toScale, scalar);

            bc.Hits = bc.Hits; // refresh hits
        }

        private class InternalTarget : Target
        {
            private AnimateDeadSpell m_Owner;

            public InternalTarget(AnimateDeadSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.None)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                m_Owner.Target(o);
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class BloodOathSpell : NecromancerSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Blood Oath", "In Jux Mani Xen",
                203,
                9031,
                Reagent.DaemonBlood
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(1.5); } }

        public override double RequiredSkill { get { return 20.0; } }
        public override int RequiredMana { get { return 13; } }

        public BloodOathSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(Mobile m)
        {
            if (Caster == m || !(m is PlayerMobile || m is BaseCreature)) // only PlayerMobile and BaseCreature implement blood oath checking
            {
                Caster.SendLocalizedMessage(1060508); // You can't curse that.
            }
            else if (m_OathTable.Contains(Caster))
            {
                Caster.SendLocalizedMessage(1061607); // You are already bonded in a Blood Oath.
            }
            else if (m_OathTable.Contains(m))
            {
                if (m.Player)
                    Caster.SendLocalizedMessage(1061608); // That player is already bonded in a Blood Oath.
                else
                    Caster.SendLocalizedMessage(1061609); // That creature is already bonded in a Blood Oath.
            }
            else if (CheckHSequence(m))
            {
                SpellHelper.Turn(Caster, m);

                /* Temporarily creates a dark pact between the caster and the target.
                 * Any damage dealt by the target to the caster is increased, but the target receives the same amount of damage.
                 * The effect lasts for ((Spirit Speak skill level - target's Resist Magic skill level) / 80 ) + 8 seconds.
                 * 
                 * NOTE: The above algorithm must be fixed point, it should be:
                 * ((ss-rm)/8)+8
                 */

                ExpireTimer timer = (ExpireTimer)m_Table[m];
                if (timer != null)
                    timer.DoExpire();

                m_OathTable[Caster] = Caster;
                m_OathTable[m] = Caster;

                if (m.Spell != null)
                    m.Spell.OnCasterHurt();

                Caster.PlaySound(0x175);

                Caster.FixedParticles(0x375A, 1, 17, 9919, 33, 7, EffectLayer.Waist);
                Caster.FixedParticles(0x3728, 1, 13, 9502, 33, 7, (EffectLayer)255);

                m.FixedParticles(0x375A, 1, 17, 9919, 33, 7, EffectLayer.Waist);
                m.FixedParticles(0x3728, 1, 13, 9502, 33, 7, (EffectLayer)255);

                TimeSpan duration = TimeSpan.FromSeconds(((GetDamageSkill(Caster) - GetResistSkill(m)) / 8) + 8);
                m.CheckSkill(SkillName.MagicResist, 0.0, 120.0);	//Skill check for gain

                timer = new ExpireTimer(Caster, m, duration);
                timer.Start();

                BuffInfo.AddBuff(Caster, new BuffInfo(BuffIcon.BloodOathCaster, 1075659, duration, Caster, m.Name.ToString()));
                BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.BloodOathCurse, 1075661, duration, m, Caster.Name.ToString()));

                m_Table[m] = timer;
                HarmfulSpell(m);
            }

            FinishSequence();
        }

        public static bool RemoveCurse(Mobile m)
        {
            ExpireTimer t = (ExpireTimer)m_Table[m];

            if (t == null)
                return false;

            t.DoExpire();
            return true;
        }

        private static Hashtable m_OathTable = new Hashtable();
        private static Hashtable m_Table = new Hashtable();

        public static Mobile GetBloodOath(Mobile m)
        {
            if (m == null)
                return null;

            Mobile oath = (Mobile)m_OathTable[m];

            if (oath == m)
                oath = null;

            return oath;
        }

        private class ExpireTimer : Timer
        {
            private Mobile m_Caster;
            private Mobile m_Target;
            private DateTime m_End;

            public ExpireTimer(Mobile caster, Mobile target, TimeSpan delay)
                : base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
            {
                m_Caster = caster;
                m_Target = target;
                m_End = DateTime.UtcNow + delay;

                Priority = TimerPriority.TwoFiftyMS;
            }

            protected override void OnTick()
            {
                if (m_Caster.Deleted || m_Target.Deleted || !m_Caster.Alive || !m_Target.Alive || DateTime.UtcNow >= m_End)
                {
                    DoExpire();
                }
            }
            public void DoExpire()
            {
                if (m_OathTable.Contains(m_Caster))
                {
                    m_Caster.SendLocalizedMessage(1061620); // Your Blood Oath has been broken.
                    m_OathTable.Remove(m_Caster);
                }

                if (m_OathTable.Contains(m_Target))
                {
                    m_Target.SendLocalizedMessage(1061620); // Your Blood Oath has been broken.
                    m_OathTable.Remove(m_Target);
                }

                Stop();

                BuffInfo.RemoveBuff(m_Caster, BuffIcon.BloodOathCaster);
                BuffInfo.RemoveBuff(m_Target, BuffIcon.BloodOathCurse);

                m_Table.Remove(m_Caster);
            }
        }

        private class InternalTarget : Target
        {
            private BloodOathSpell m_Owner;

            public InternalTarget(BloodOathSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Harmful)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                    m_Owner.Target((Mobile)o);
                else
                    from.SendLocalizedMessage(1060508); // You can't curse that.
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class CorpseSkinSpell : NecromancerSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Corpse Skin", "In Agle Corp Ylem",
                203,
                9051,
                Reagent.BatWing,
                Reagent.GraveDust
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(1.5); } }

        public override double RequiredSkill { get { return 20.0; } }
        public override int RequiredMana { get { return 11; } }

        public CorpseSkinSpell(Mobile caster, Item scroll)
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
                SpellHelper.Turn(Caster, m);

                /* Transmogrifies the flesh of the target creature or player to resemble rotted corpse flesh,
                 * making them more vulnerable to Fire and Poison damage,
                 * but increasing their resistance to Physical and Cold damage.
                 * 
                 * The effect lasts for ((Spirit Speak skill level - target's Resist Magic skill level) / 25 ) + 40 seconds.
                 * 
                 * NOTE: Algorithm above is fixed point, should be:
                 * ((ss-mr)/2.5) + 40
                 * 
                 * NOTE: Resistance is not checked if targeting yourself
                 */

                ExpireTimer timer = (ExpireTimer)m_Table[m];

                if (timer != null)
                    timer.DoExpire();
                else
                    m.SendLocalizedMessage(1061689); // Your skin turns dry and corpselike.

                if (m.Spell != null)
                    m.Spell.OnCasterHurt();

                m.FixedParticles(0x373A, 1, 15, 9913, 67, 7, EffectLayer.Head);
                m.PlaySound(0x1BB);

                double ss = GetDamageSkill(Caster);
                double mr = (Caster == m ? 0.0 : GetResistSkill(m));
                m.CheckSkill(SkillName.MagicResist, 0.0, 120.0);	//Skill check for gain

                TimeSpan duration = TimeSpan.FromSeconds(((ss - mr) / 2.5) + 40.0);

                ResistanceMod[] mods = new ResistanceMod[4]
					{
						new ResistanceMod( ResistanceType.Fire, -15 ),
						new ResistanceMod( ResistanceType.Poison, -15 ),
						new ResistanceMod( ResistanceType.Cold, +10 ),
						new ResistanceMod( ResistanceType.Physical, +10 )
					};

                timer = new ExpireTimer(m, mods, duration);
                timer.Start();

                BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.CorpseSkin, 1075663, duration, m));

                m_Table[m] = timer;

                for (int i = 0; i < mods.Length; ++i)
                    m.AddResistanceMod(mods[i]);

                HarmfulSpell(m);
            }

            FinishSequence();
        }

        private static Hashtable m_Table = new Hashtable();

        public static bool RemoveCurse(Mobile m)
        {
            ExpireTimer t = (ExpireTimer)m_Table[m];

            if (t == null)
                return false;

            m.SendLocalizedMessage(1061688); // Your skin returns to normal.
            t.DoExpire();
            return true;
        }

        private class ExpireTimer : Timer
        {
            private Mobile m_Mobile;
            private ResistanceMod[] m_Mods;

            public ExpireTimer(Mobile m, ResistanceMod[] mods, TimeSpan delay)
                : base(delay)
            {
                m_Mobile = m;
                m_Mods = mods;
            }

            public void DoExpire()
            {
                for (int i = 0; i < m_Mods.Length; ++i)
                    m_Mobile.RemoveResistanceMod(m_Mods[i]);

                Stop();
                BuffInfo.RemoveBuff(m_Mobile, BuffIcon.CorpseSkin);
                m_Table.Remove(m_Mobile);
            }

            protected override void OnTick()
            {
                m_Mobile.SendLocalizedMessage(1061688); // Your skin returns to normal.
                DoExpire();
            }
        }

        private class InternalTarget : Target
        {
            private CorpseSkinSpell m_Owner;

            public InternalTarget(CorpseSkinSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Harmful)
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

    public class CurseWeaponSpell : NecromancerSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Curse Weapon", "An Sanct Gra Char",
                203,
                9031,
                Reagent.PigIron
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(0.75); } }

        public override double RequiredSkill { get { return 0.0; } }
        public override int RequiredMana { get { return 7; } }

        public CurseWeaponSpell(Mobile caster, Item scroll)
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
                /* Temporarily imbues a weapon with a life draining effect.
                 * Half the damage that the weapon inflicts is added to the necromancer's health.
                 * The effects lasts for (Spirit Speak skill level / 34) + 1 seconds.
                 * 
                 * NOTE: Above algorithm is fixed point, should be :
                 * (Spirit Speak skill level / 3.4) + 1
                 * 
                 * TODO: What happens if you curse a weapon then give it to someone else? Should they get the drain effect?
                 */

                Caster.PlaySound(0x387);
                Caster.FixedParticles(0x3779, 1, 15, 9905, 32, 2, EffectLayer.Head);
                Caster.FixedParticles(0x37B9, 1, 14, 9502, 32, 5, (EffectLayer)255);
                new SoundEffectTimer(Caster).Start();

                TimeSpan duration = TimeSpan.FromSeconds((Caster.Skills[SkillName.SpiritSpeak].Value / 3.4) + 1.0);


                Timer t = (Timer)m_Table[weapon];

                if (t != null)
                    t.Stop();

                weapon.Cursed = true;

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
                Priority = TimerPriority.OneSecond;
            }

            protected override void OnTick()
            {
                m_Weapon.Cursed = false;
                Effects.PlaySound(m_Weapon.GetWorldLocation(), m_Weapon.Map, 0xFA);
                m_Table.Remove(this);
            }
        }

        private class SoundEffectTimer : Timer
        {
            private Mobile m_Mobile;

            public SoundEffectTimer(Mobile m)
                : base(TimeSpan.FromSeconds(0.75))
            {
                m_Mobile = m;
                Priority = TimerPriority.FiftyMS;
            }

            protected override void OnTick()
            {
                m_Mobile.PlaySound(0xFA);
            }
        }
    }

    public class EvilOmenSpell : NecromancerSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Evil Omen", "Pas Tym An Sanct",
                203,
                9031,
                Reagent.BatWing,
                Reagent.NoxCrystal
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(0.75); } }

        public override double RequiredSkill { get { return 20.0; } }
        public override int RequiredMana { get { return 11; } }

        public EvilOmenSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(Mobile m)
        {
            if (!(m is BaseCreature || m is PlayerMobile))
            {
                Caster.SendLocalizedMessage(1060508); // You can't curse that.
            }
            else if (CheckHSequence(m))
            {
                SpellHelper.Turn(Caster, m);

                /* Curses the target so that the next harmful event that affects them is magnified.
                 * Damage to the target's hit points is increased 25%,
                 * the poison level of the attack will be 1 higher
                 * and the Resist Magic skill of the target will be fixed on 50.
                 *
                 * The effect lasts for one harmful event only.
                 */

                if (m.Spell != null)
                    m.Spell.OnCasterHurt();

                m.PlaySound(0xFC);
                m.FixedParticles(0x3728, 1, 13, 9912, 1150, 7, EffectLayer.Head);
                m.FixedParticles(0x3779, 1, 15, 9502, 67, 7, EffectLayer.Head);

                if (!m_Table.Contains(m))
                {
                    SkillMod mod = new DefaultSkillMod(SkillName.MagicResist, false, 50.0);

                    if (m.Skills[SkillName.MagicResist].Base > 50.0)
                        m.AddSkillMod(mod);

                    m_Table[m] = mod;
                }

                TimeSpan duration = TimeSpan.FromSeconds((Caster.Skills[SkillName.SpiritSpeak].Value / 12) + 1.0);

                Timer.DelayCall(duration, new TimerStateCallback(EffectExpire_Callback), m);

                HarmfulSpell(m);

                BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.EvilOmen, 1075647, 1075648, duration, m));

            }

            FinishSequence();
        }

        private static Hashtable m_Table = new Hashtable();

        private static void EffectExpire_Callback(object state)
        {
            TryEndEffect((Mobile)state);
        }

        /*
         * The naming here was confusing. Its a 1-off effect spell.
         * So, we dont actually "checkeffect"; we endeffect with bool
         * return to determine external behaviors.
         *
         * -refactored.
         */

        public static bool TryEndEffect(Mobile m)
        {
            SkillMod mod = (SkillMod)m_Table[m];

            if (mod == null)
                return false;

            m_Table.Remove(m);
            mod.Remove();

            return true;
        }

        private class InternalTarget : Target
        {
            private EvilOmenSpell m_Owner;

            public InternalTarget(EvilOmenSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Harmful)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                    m_Owner.Target((Mobile)o);
                else
                    from.SendLocalizedMessage(1060508); // You can't curse that.
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class ExorcismSpell : NecromancerSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Exorcism", "Ort Corp Grav",
                203,
                9031,
                Reagent.NoxCrystal,
                Reagent.GraveDust
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(2.0); } }

        public override double RequiredSkill { get { return 80.0; } }
        public override int RequiredMana { get { return 40; } }

        public ExorcismSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override bool CheckCast()
        {
            if (Caster.Skills.SpiritSpeak.Value < 100.0)
            {
                Caster.SendLocalizedMessage(1072112); // You must have GM Spirit Speak to use this spell
                return false;
            }

            return base.CheckCast();
        }


        public override bool DelayedDamage { get { return false; } }

        private static readonly int Range = (Core.ML ? 48 : 18);

        public override int ComputeKarmaAward()
        {
            return 0;	//no karma lost from this spell!
        }

        public override void OnCast()
        {
            ChampionSpawnRegion r = Caster.Region.GetRegion(typeof(ChampionSpawnRegion)) as ChampionSpawnRegion;

            if (r == null || !Caster.InRange(r.ChampionSpawn, Range))
            {
                Caster.SendLocalizedMessage(1072111); // You are not in a valid exorcism region.
            }
            else if (CheckSequence())
            {
                Map map = Caster.Map;

                if (map != null)
                {
                    List<Mobile> targets = new List<Mobile>();

                    foreach (Mobile m in r.ChampionSpawn.GetMobilesInRange(Range))
                        if (IsValidTarget(m))
                            targets.Add(m);

                    for (int i = 0; i < targets.Count; ++i)
                    {
                        Mobile m = targets[i];

                        //Suprisingly, no sparkle type effects

                        m.Location = GetNearestShrine(m);
                    }
                }
            }

            FinishSequence();
        }

        private bool IsValidTarget(Mobile m)
        {
            if (!m.Player || m.Alive)
                return false;

            Corpse c = m.Corpse as Corpse;
            Map map = m.Map;

            if (c != null && !c.Deleted && map != null && c.Map == map)
            {
                if (SpellHelper.IsAnyT2A(map, c.Location) && SpellHelper.IsAnyT2A(map, m.Location))
                    return false;	//Same Map, both in T2A, ie, same 'sub server'.

                if (m.Region.IsPartOf(typeof(DungeonRegion)) == Region.Find(c.Location, map).IsPartOf(typeof(DungeonRegion)))
                    return false; //Same Map, both in Dungeon region OR They're both NOT in a dungeon region.

                //Just an approximation cause RunUO doens't divide up the world the same way OSI does ;p

            }

            Party p = Party.Get(m);

            if (p != null && p.Contains(Caster))
                return false;

            if (m.Guild != null && Caster.Guild != null)
            {
                Guild mGuild = m.Guild as Guild;
                Guild cGuild = Caster.Guild as Guild;

                if (mGuild.IsAlly(cGuild))
                    return false;

                if (mGuild == cGuild)
                    return false;
            }

            Faction f = Faction.Find(m);

            if (Faction.Facet == m.Map && f != null && f == Faction.Find(Caster))
                return false;

            return true;
        }

        private static Point3D GetNearestShrine(Mobile m)
        {
            Map map = m.Map;

            Point3D[] locList;


            if (map == Map.Felucca || map == Map.Trammel)
                locList = m_BritanniaLocs;
            else if (map == Map.Ilshenar)
                locList = m_IllshLocs;
            else if (map == Map.Tokuno)
                locList = m_TokunoLocs;
            else if (map == Map.Malas)
                locList = m_MalasLocs;
            else
                locList = new Point3D[0];

            Point3D closest = Point3D.Zero;
            double minDist = double.MaxValue;

            for (int i = 0; i < locList.Length; i++)
            {
                Point3D p = locList[i];

                double dist = m.GetDistanceToSqrt(p);
                if (minDist > dist)
                {
                    closest = p;
                    minDist = dist;
                }
            }

            return closest;
        }

        private static readonly Point3D[] m_BritanniaLocs = new Point3D[]
			{
				new Point3D( 1470, 843, 0 ),
				new Point3D( 1857, 865, -1 ),
				new Point3D( 4220, 563, 36 ),
				new Point3D( 1732, 3528, 0 ),
				new Point3D( 1300, 644, 8 ),
				new Point3D( 3355, 302, 9 ),
				new Point3D( 1606, 2490, 5 ),
				new Point3D( 2500, 3931, 3 ),
				new Point3D( 4264, 3707, 0 )
			};
        private static readonly Point3D[] m_IllshLocs = new Point3D[]
			{
				new Point3D( 1222, 474, -17 ),
				new Point3D( 718, 1360, -60),
				new Point3D( 297, 1014, -19 ),
				new Point3D( 986, 1006, -36 ),
				new Point3D( 1180, 1288, -30 ),
				new Point3D( 1538, 1341, -3 ),
				new Point3D( 528, 223, -38 )
			};
        private static readonly Point3D[] m_MalasLocs = new Point3D[]
			{
				new Point3D ( 976, 517, -30 )
			};
        private static readonly Point3D[] m_TokunoLocs = new Point3D[]
			{
				new Point3D( 710, 1162, 25 ),
				new Point3D( 1034, 515, 18 ),
				new Point3D( 295, 712, 55 )
			};
    }

    public class HorrificBeastSpell : TransformationSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Horrific Beast", "Rel Xen Vas Bal",
                203,
                9031,
                Reagent.BatWing,
                Reagent.DaemonBlood
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(2.0); } }

        public override double RequiredSkill { get { return 40.0; } }
        public override int RequiredMana { get { return 11; } }

        public override int Body { get { return 746; } }

        public HorrificBeastSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void DoEffect(Mobile m)
        {
            m.PlaySound(0x165);
            m.FixedParticles(0x3728, 1, 13, 9918, 92, 3, EffectLayer.Head);

            m.Delta(MobileDelta.WeaponDamage);
            m.CheckStatTimers();
        }

        public override void RemoveEffect(Mobile m)
        {
            m.Delta(MobileDelta.WeaponDamage);
        }
    }

    public class LichFormSpell : TransformationSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Lich Form", "Rel Xen Corp Ort",
                203,
                9031,
                Reagent.GraveDust,
                Reagent.DaemonBlood,
                Reagent.NoxCrystal
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(2.0); } }

        public override double RequiredSkill { get { return 70.0; } }
        public override int RequiredMana { get { return 23; } }

        public override int Body { get { return 749; } }

        public override int FireResistOffset { get { return -10; } }
        public override int ColdResistOffset { get { return +10; } }
        public override int PoisResistOffset { get { return +10; } }

        public override double TickRate { get { return 2.5; } }

        public LichFormSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void DoEffect(Mobile m)
        {
            m.PlaySound(0x19C);
            m.FixedParticles(0x3709, 1, 30, 9904, 1108, 6, EffectLayer.RightFoot);
        }

        public override void OnTick(Mobile m)
        {
            --m.Hits;
        }
    }

    public class MindRotSpell : NecromancerSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Mind Rot", "Wis An Ben",
                203,
                9031,
                Reagent.BatWing,
                Reagent.PigIron,
                Reagent.DaemonBlood
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(1.5); } }

        public override double RequiredSkill { get { return 30.0; } }
        public override int RequiredMana { get { return 17; } }

        public MindRotSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(Mobile m)
        {
            if (HasMindRotScalar(m))
            {
                Caster.SendLocalizedMessage(1005559); // This spell is already in effect.
            }
            else if (CheckHSequence(m))
            {
                SpellHelper.Turn(Caster, m);

                /* Attempts to place a curse on the Target that increases the mana cost of any spells they cast,
                 * for a duration based off a comparison between the Caster's Spirit Speak skill and the Target's Resisting Spells skill.
                 * The effect lasts for ((Spirit Speak skill level - target's Resist Magic skill level) / 50 ) + 20 seconds.
                 */

                if (m.Spell != null)
                    m.Spell.OnCasterHurt();

                m.PlaySound(0x1FB);
                m.PlaySound(0x258);
                m.FixedParticles(0x373A, 1, 17, 9903, 15, 4, EffectLayer.Head);

                TimeSpan duration = TimeSpan.FromSeconds((((GetDamageSkill(Caster) - GetResistSkill(m)) / 5.0) + 20.0) * (m.Player ? 1.0 : 2.0));
                m.CheckSkill(SkillName.MagicResist, 0.0, 120.0);	//Skill check for gain

                if (m.Player)
                    SetMindRotScalar(Caster, m, 1.25, duration);
                else
                    SetMindRotScalar(Caster, m, 2.00, duration);

                HarmfulSpell(m);
            }

            FinishSequence();
        }

        private static Hashtable m_Table = new Hashtable();

        public static void ClearMindRotScalar(Mobile m)
        {
            if (!m_Table.ContainsKey(m))
                return;

            BuffInfo.RemoveBuff(m, BuffIcon.Mindrot);
            MRBucket tmpB = (MRBucket)m_Table[m];
            MRExpireTimer tmpT = (MRExpireTimer)tmpB.m_MRExpireTimer;
            tmpT.Stop();
            m_Table.Remove(m);
            m.SendLocalizedMessage(1060872); // Your mind feels normal again.
        }

        public static bool HasMindRotScalar(Mobile m)
        {
            return m_Table.ContainsKey(m);
        }

        public static bool GetMindRotScalar(Mobile m, ref double scalar)
        {
            if (!m_Table.ContainsKey(m))
                return false;

            MRBucket tmpB = (MRBucket)m_Table[m];
            scalar = tmpB.m_Scalar;
            return true;
        }

        public static void SetMindRotScalar(Mobile caster, Mobile target, double scalar, TimeSpan duration)
        {
            if (!m_Table.ContainsKey(target))
            {
                m_Table.Add(target, new MRBucket(scalar, new MRExpireTimer(caster, target, duration)));
                BuffInfo.AddBuff(target, new BuffInfo(BuffIcon.Mindrot, 1075665, duration, target));
                MRBucket tmpB = (MRBucket)m_Table[target];
                MRExpireTimer tmpT = (MRExpireTimer)tmpB.m_MRExpireTimer;
                tmpT.Start();
                target.SendLocalizedMessage(1074384);
            }
        }

        private class InternalTarget : Target
        {
            private MindRotSpell m_Owner;

            public InternalTarget(MindRotSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Harmful)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                    m_Owner.Target((Mobile)o);
                else
                    from.SendLocalizedMessage(1060508); // You can't curse that.
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class MRExpireTimer : Timer
    {
        private Mobile m_Caster;
        private Mobile m_Target;
        private DateTime m_End;

        public MRExpireTimer(Mobile caster, Mobile target, TimeSpan delay)
            : base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
        {
            m_Caster = caster;
            m_Target = target;
            m_End = DateTime.UtcNow + delay;
            Priority = TimerPriority.TwoFiftyMS;
        }

        public void RenewDelay(TimeSpan delay)
        {
            m_End = DateTime.UtcNow + delay;
        }

        public void Halt()
        {
            Stop();
        }

        protected override void OnTick()
        {
            if (m_Target.Deleted || !m_Target.Alive || DateTime.UtcNow >= m_End)
            {
                MindRotSpell.ClearMindRotScalar(m_Target);
                Stop();
            }
        }
    }

    public class MRBucket
    {
        public MRBucket(double theScalar, MRExpireTimer theTimer)
        {
            m_Scalar = theScalar;
            m_MRExpireTimer = theTimer;
        }

        public double m_Scalar;
        public MRExpireTimer m_MRExpireTimer;
    }

    public class PainSpikeSpell : NecromancerSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Pain Spike", "In Sar",
                203,
                9031,
                Reagent.GraveDust,
                Reagent.PigIron
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(1.0); } }

        public override double RequiredSkill { get { return 20.0; } }
        public override int RequiredMana { get { return 5; } }

        public PainSpikeSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public override bool DelayedDamage { get { return false; } }

        public void Target(Mobile m)
        {
            if (CheckHSequence(m))
            {
                SpellHelper.Turn(Caster, m);

                //SpellHelper.CheckReflect( (int)this.Circle, Caster, ref m ); //Irrelevent asfter AoS

                /* Temporarily causes intense physical pain to the target, dealing direct damage.
                 * After 10 seconds the spell wears off, and if the target is still alive, 
                 * some of the Hit Points lost through Pain Spike are restored.
                 */

                m.FixedParticles(0x37C4, 1, 8, 9916, 39, 3, EffectLayer.Head);
                m.FixedParticles(0x37C4, 1, 8, 9502, 39, 4, EffectLayer.Head);
                m.PlaySound(0x210);

                double damage = ((GetDamageSkill(Caster) - GetResistSkill(m)) / 10) + (m.Player ? 18 : 30);
                m.CheckSkill(SkillName.MagicResist, 0.0, 120.0);	//Skill check for gain

                if (damage < 1)
                    damage = 1;

                TimeSpan buffTime = TimeSpan.FromSeconds(10.0);

                if (m_Table.Contains(m))
                {
                    damage = Utility.RandomMinMax(3, 7);
                    Timer t = m_Table[m] as Timer;

                    if (t != null)
                    {
                        t.Delay += TimeSpan.FromSeconds(2.0);

                        buffTime = t.Next - DateTime.UtcNow;
                    }
                }
                else
                {
                    new InternalTimer(m, damage).Start();
                }

                BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.PainSpike, 1075667, buffTime, m, Convert.ToString((int)damage)));

                Misc.WeightOverloading.DFA = Misc.DFAlgorithm.PainSpike;
                m.Damage((int)damage, Caster);
                SpellHelper.DoLeech((int)damage, Caster, m);
                Misc.WeightOverloading.DFA = Misc.DFAlgorithm.Standard;

                //SpellHelper.Damage( this, m, damage, 100, 0, 0, 0, 0, Misc.DFAlgorithm.PainSpike );
                HarmfulSpell(m);
            }

            FinishSequence();
        }

        private static Hashtable m_Table = new Hashtable();

        private class InternalTimer : Timer
        {
            private Mobile m_Mobile;
            private int m_ToRestore;

            public InternalTimer(Mobile m, double toRestore)
                : base(TimeSpan.FromSeconds(10.0))
            {
                Priority = TimerPriority.OneSecond;

                m_Mobile = m;
                m_ToRestore = (int)toRestore;

                m_Table[m] = this;
            }

            protected override void OnTick()
            {
                m_Table.Remove(m_Mobile);

                if (m_Mobile.Alive && !m_Mobile.IsDeadBondedPet)
                    m_Mobile.Hits += m_ToRestore;

                BuffInfo.RemoveBuff(m_Mobile, BuffIcon.PainSpike);
            }
        }

        private class InternalTarget : Target
        {
            private PainSpikeSpell m_Owner;

            public InternalTarget(PainSpikeSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Harmful)
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

    public class PoisonStrikeSpell : NecromancerSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Poison Strike", "In Vas Nox",
                203,
                9031,
                Reagent.NoxCrystal
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds((Core.ML ? 1.75 : 1.5)); } }

        public override double RequiredSkill { get { return 50.0; } }
        public override int RequiredMana { get { return 17; } }

        public PoisonStrikeSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public override bool DelayedDamage { get { return false; } }

        public void Target(Mobile m)
        {
            if (CheckHSequence(m))
            {
                SpellHelper.Turn(Caster, m);

                /* Creates a blast of poisonous energy centered on the target.
                 * The main target is inflicted with a large amount of Poison damage, and all valid targets in a radius of 2 tiles around the main target are inflicted with a lesser effect.
                 * One tile from main target receives 50% damage, two tiles from target receives 33% damage.
                 */

                //CheckResisted( m ); // Check magic resist for skill, but do not use return value	//reports from OSI:  Necro spells don't give Resist gain

                Effects.SendLocationParticles(EffectItem.Create(m.Location, m.Map, EffectItem.DefaultDuration), 0x36B0, 1, 14, 63, 7, 9915, 0);
                Effects.PlaySound(m.Location, m.Map, 0x229);

                double damage = Utility.RandomMinMax((Core.ML ? 32 : 36), 40) * ((300 + (GetDamageSkill(Caster) * 9)) / 1000);

                double sdiBonus = (double)AosAttributes.GetValue(Caster, AosAttribute.SpellDamage) / 100;
                double pvmDamage = damage * (1 + sdiBonus);

                if (Core.ML && sdiBonus > 0.15)
                    sdiBonus = 0.15;
                double pvpDamage = damage * (1 + sdiBonus);

                Map map = m.Map;

                if (map != null)
                {
                    List<Mobile> targets = new List<Mobile>();

                    if (Caster.CanBeHarmful(m, false))
                        targets.Add(m);

                    foreach (Mobile targ in m.GetMobilesInRange(2))
                        if (!(Caster is BaseCreature && targ is BaseCreature))
                            if ((targ != Caster && m != targ) && (SpellHelper.ValidIndirectTarget(Caster, targ) && Caster.CanBeHarmful(targ, false)))
                                targets.Add(targ);

                    for (int i = 0; i < targets.Count; ++i)
                    {
                        Mobile targ = targets[i];
                        int num;

                        if (targ.InRange(m.Location, 0))
                            num = 1;
                        else if (targ.InRange(m.Location, 1))
                            num = 2;
                        else
                            num = 3;

                        Caster.DoHarmful(targ);
                        SpellHelper.Damage(this, targ, ((m.Player && Caster.Player) ? pvpDamage : pvmDamage) / num, 0, 0, 0, 100, 0);
                    }
                }
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private PoisonStrikeSpell m_Owner;

            public InternalTarget(PoisonStrikeSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Harmful)
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

    public class StrangleSpell : NecromancerSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Strangle", "In Bal Nox",
                209,
                9031,
                Reagent.DaemonBlood,
                Reagent.NoxCrystal
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(2.0); } }

        public override double RequiredSkill { get { return 65.0; } }
        public override int RequiredMana { get { return 29; } }

        public StrangleSpell(Mobile caster, Item scroll)
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
                SpellHelper.Turn(Caster, m);

                //SpellHelper.CheckReflect( (int)this.Circle, Caster, ref m );	//Irrelevent after AoS

                /* Temporarily chokes off the air suply of the target with poisonous fumes.
                 * The target is inflicted with poison damage over time.
                 * The amount of damage dealt each "hit" is based off of the caster's Spirit Speak skill and the Target's current Stamina.
                 * The less Stamina the target has, the more damage is done by Strangle.
                 * Duration of the effect is Spirit Speak skill level / 10 rounds, with a minimum number of 4 rounds.
                 * The first round of damage is dealt after 5 seconds, and every next round after that comes 1 second sooner than the one before, until there is only 1 second between rounds.
                 * The base damage of the effect lies between (Spirit Speak skill level / 10) - 2 and (Spirit Speak skill level / 10) + 1.
                 * Base damage is multiplied by the following formula: (3 - (target's current Stamina / target's maximum Stamina) * 2).
                 * Example:
                 * For a target at full Stamina the damage multiplier is 1,
                 * for a target at 50% Stamina the damage multiplier is 2 and
                 * for a target at 20% Stamina the damage multiplier is 2.6
                 */

                if (m.Spell != null)
                    m.Spell.OnCasterHurt();

                m.PlaySound(0x22F);
                m.FixedParticles(0x36CB, 1, 9, 9911, 67, 5, EffectLayer.Head);
                m.FixedParticles(0x374A, 1, 17, 9502, 1108, 4, (EffectLayer)255);

                if (!m_Table.Contains(m))
                {
                    Timer t = new InternalTimer(m, Caster);
                    t.Start();

                    m_Table[m] = t;
                }

                HarmfulSpell(m);
            }

            //Calculations for the buff bar
            double spiritlevel = Caster.Skills[SkillName.SpiritSpeak].Value / 10;
            if (spiritlevel < 4)
                spiritlevel = 4;
            int d_MinDamage = 4;
            int d_MaxDamage = ((int)spiritlevel + 1) * 3;
            string args = String.Format("{0}\t{1}", d_MinDamage, d_MaxDamage);

            int i_Count = (int)spiritlevel;
            int i_MaxCount = i_Count;
            int i_HitDelay = 5;
            int i_Length = i_HitDelay;

            while (i_Count > 1)
            {
                --i_Count;
                if (i_HitDelay > 1)
                {
                    if (i_MaxCount < 5)
                    {
                        --i_HitDelay;
                    }
                    else
                    {
                        int delay = (int)(Math.Ceiling((1.0 + (5 * i_Count)) / i_MaxCount));

                        if (delay <= 5)
                            i_HitDelay = delay;
                        else
                            i_HitDelay = 5;
                    }
                }
                i_Length += i_HitDelay;
            }
            TimeSpan t_Duration = TimeSpan.FromSeconds(i_Length);
            BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.Strangle, 1075794, 1075795, t_Duration, m, args.ToString()));

            FinishSequence();
        }

        private static Hashtable m_Table = new Hashtable();

        public static bool RemoveCurse(Mobile m)
        {
            Timer t = (Timer)m_Table[m];

            if (t == null)
                return false;

            t.Stop();
            m.SendLocalizedMessage(1061687); // You can breath normally again.

            m_Table.Remove(m);
            return true;
        }

        private class InternalTimer : Timer
        {
            private Mobile m_Target, m_From;
            private double m_MinBaseDamage, m_MaxBaseDamage;

            private DateTime m_NextHit;
            private int m_HitDelay;

            private int m_Count, m_MaxCount;

            public InternalTimer(Mobile target, Mobile from)
                : base(TimeSpan.FromSeconds(0.1), TimeSpan.FromSeconds(0.1))
            {
                Priority = TimerPriority.FiftyMS;

                m_Target = target;
                m_From = from;

                double spiritLevel = from.Skills[SkillName.SpiritSpeak].Value / 10;

                m_MinBaseDamage = spiritLevel - 2;
                m_MaxBaseDamage = spiritLevel + 1;

                m_HitDelay = 5;
                m_NextHit = DateTime.UtcNow + TimeSpan.FromSeconds(m_HitDelay);

                m_Count = (int)spiritLevel;

                if (m_Count < 4)
                    m_Count = 4;

                m_MaxCount = m_Count;
            }

            protected override void OnTick()
            {
                if (!m_Target.Alive)
                {
                    m_Table.Remove(m_Target);
                    Stop();
                }

                if (!m_Target.Alive || DateTime.UtcNow < m_NextHit)
                    return;

                --m_Count;

                if (m_HitDelay > 1)
                {
                    if (m_MaxCount < 5)
                    {
                        --m_HitDelay;
                    }
                    else
                    {
                        int delay = (int)(Math.Ceiling((1.0 + (5 * m_Count)) / m_MaxCount));

                        if (delay <= 5)
                            m_HitDelay = delay;
                        else
                            m_HitDelay = 5;
                    }
                }

                if (m_Count == 0)
                {
                    m_Target.SendLocalizedMessage(1061687); // You can breath normally again.
                    m_Table.Remove(m_Target);
                    Stop();
                }
                else
                {
                    m_NextHit = DateTime.UtcNow + TimeSpan.FromSeconds(m_HitDelay);

                    double damage = m_MinBaseDamage + (Utility.RandomDouble() * (m_MaxBaseDamage - m_MinBaseDamage));

                    damage *= (3 - (((double)m_Target.Stam / m_Target.StamMax) * 2));

                    if (damage < 1)
                        damage = 1;

                    if (!m_Target.Player)
                        damage *= 1.75;

                    AOS.Damage(m_Target, m_From, (int)damage, 0, 0, 0, 100, 0);

                    if (0.60 <= Utility.RandomDouble()) // OSI: randomly revealed between first and third damage tick, guessing 60% chance
                        m_Target.RevealingAction();
                }
            }
        }

        private class InternalTarget : Target
        {
            private StrangleSpell m_Owner;

            public InternalTarget(StrangleSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Harmful)
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

    public class SummonFamiliarSpell : NecromancerSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Summon Familiar", "Kal Xen Bal",
                203,
                9031,
                Reagent.BatWing,
                Reagent.GraveDust,
                Reagent.DaemonBlood
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(2.0); } }

        public override double RequiredSkill { get { return 30.0; } }
        public override int RequiredMana { get { return 17; } }

        public SummonFamiliarSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        private static Hashtable m_Table = new Hashtable();

        public static Hashtable Table { get { return m_Table; } }

        public override bool CheckCast()
        {
            BaseCreature check = (BaseCreature)m_Table[Caster];

            if (check != null && !check.Deleted)
            {
                Caster.SendLocalizedMessage(1061605); // You already have a familiar.
                return false;
            }

            return base.CheckCast();
        }

        public override void OnCast()
        {
            if (CheckSequence())
            {
                Caster.CloseGump(typeof(SummonFamiliarGump));
                Caster.SendGump(new SummonFamiliarGump(Caster, m_Entries, this));
            }

            FinishSequence();
        }

        private static SummonFamiliarEntry[] m_Entries = new SummonFamiliarEntry[]
			{
				new SummonFamiliarEntry( typeof( HordeMinionFamiliar ), 1060146,  30.0,  30.0 ), // Horde Minion
				new SummonFamiliarEntry( typeof( ShadowWispFamiliar ), 1060142,  50.0,  50.0 ), // Shadow Wisp
				new SummonFamiliarEntry( typeof( DarkWolfFamiliar ), 1060143,  60.0,  60.0 ), // Dark Wolf
				new SummonFamiliarEntry( typeof( DeathAdder ), 1060145,  80.0,  80.0 ), // Death Adder
				new SummonFamiliarEntry( typeof( VampireBatFamiliar ), 1060144, 100.0, 100.0 )  // Vampire Bat
			};

        public static SummonFamiliarEntry[] Entries { get { return m_Entries; } }
    }

    public class SummonFamiliarEntry
    {
        private Type m_Type;
        private object m_Name;
        private double m_ReqNecromancy;
        private double m_ReqSpiritSpeak;

        public Type Type { get { return m_Type; } }
        public object Name { get { return m_Name; } }
        public double ReqNecromancy { get { return m_ReqNecromancy; } }
        public double ReqSpiritSpeak { get { return m_ReqSpiritSpeak; } }

        public SummonFamiliarEntry(Type type, object name, double reqNecromancy, double reqSpiritSpeak)
        {
            m_Type = type;
            m_Name = name;
            m_ReqNecromancy = reqNecromancy;
            m_ReqSpiritSpeak = reqSpiritSpeak;
        }
    }

    public class SummonFamiliarGump : Gump
    {
        private Mobile m_From;
        private SummonFamiliarEntry[] m_Entries;

        private SummonFamiliarSpell m_Spell;

        private const int EnabledColor16 = 0x0F20;
        private const int DisabledColor16 = 0x262A;

        private const int EnabledColor32 = 0x18CD00;
        private const int DisabledColor32 = 0x4A8B52;

        public SummonFamiliarGump(Mobile from, SummonFamiliarEntry[] entries, SummonFamiliarSpell spell)
            : base(200, 100)
        {
            m_From = from;
            m_Entries = entries;
            m_Spell = spell;

            AddPage(0);

            AddBackground(10, 10, 250, 178, 9270);
            AddAlphaRegion(20, 20, 230, 158);

            AddImage(220, 20, 10464);
            AddImage(220, 72, 10464);
            AddImage(220, 124, 10464);

            AddItem(188, 16, 6883);
            AddItem(198, 168, 6881);
            AddItem(8, 15, 6882);
            AddItem(2, 168, 6880);

            AddHtmlLocalized(30, 26, 200, 20, 1060147, EnabledColor16, false, false); // Chose thy familiar...

            double necro = from.Skills[SkillName.Necromancy].Value;
            double spirit = from.Skills[SkillName.SpiritSpeak].Value;

            for (int i = 0; i < entries.Length; ++i)
            {
                object name = entries[i].Name;

                bool enabled = (necro >= entries[i].ReqNecromancy && spirit >= entries[i].ReqSpiritSpeak);

                AddButton(27, 53 + (i * 21), 9702, 9703, i + 1, GumpButtonType.Reply, 0);

                if (name is int)
                    AddHtmlLocalized(50, 51 + (i * 21), 150, 20, (int)name, enabled ? EnabledColor16 : DisabledColor16, false, false);
                else if (name is string)
                    AddHtml(50, 51 + (i * 21), 150, 20, String.Format("<BASEFONT COLOR=#{0:X6}>{1}</BASEFONT>", enabled ? EnabledColor32 : DisabledColor32, name), false, false);
            }
        }

        private static Hashtable m_Table = new Hashtable();

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            int index = info.ButtonID - 1;

            if (index >= 0 && index < m_Entries.Length)
            {
                SummonFamiliarEntry entry = m_Entries[index];

                double necro = m_From.Skills[SkillName.Necromancy].Value;
                double spirit = m_From.Skills[SkillName.SpiritSpeak].Value;

                BaseCreature check = (BaseCreature)SummonFamiliarSpell.Table[m_From];

                #region Dueling
                if (m_From is PlayerMobile && ((PlayerMobile)m_From).DuelContext != null && !((PlayerMobile)m_From).DuelContext.AllowSpellCast(m_From, m_Spell))
                {
                }
                #endregion
                else if (check != null && !check.Deleted)
                {
                    m_From.SendLocalizedMessage(1061605); // You already have a familiar.
                }
                else if (necro < entry.ReqNecromancy || spirit < entry.ReqSpiritSpeak)
                {
                    // That familiar requires ~1_NECROMANCY~ Necromancy and ~2_SPIRIT~ Spirit Speak.
                    m_From.SendLocalizedMessage(1061606, String.Format("{0:F1}\t{1:F1}", entry.ReqNecromancy, entry.ReqSpiritSpeak));

                    m_From.CloseGump(typeof(SummonFamiliarGump));
                    m_From.SendGump(new SummonFamiliarGump(m_From, SummonFamiliarSpell.Entries, m_Spell));
                }
                else if (entry.Type == null)
                {
                    m_From.SendMessage("That familiar has not yet been defined.");

                    m_From.CloseGump(typeof(SummonFamiliarGump));
                    m_From.SendGump(new SummonFamiliarGump(m_From, SummonFamiliarSpell.Entries, m_Spell));
                }
                else
                {
                    try
                    {
                        BaseCreature bc = (BaseCreature)Activator.CreateInstance(entry.Type);

                        bc.Skills.MagicResist = m_From.Skills.MagicResist;

                        if (BaseCreature.Summon(bc, m_From, m_From.Location, -1, TimeSpan.FromDays(1.0)))
                        {
                            m_From.FixedParticles(0x3728, 1, 10, 9910, EffectLayer.Head);
                            bc.PlaySound(bc.GetIdleSound());
                            SummonFamiliarSpell.Table[m_From] = bc;
                        }
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                m_From.SendLocalizedMessage(1061825); // You decide not to summon a familiar.
            }
        }
    }

    public class VampiricEmbraceSpell : TransformationSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Vampiric Embrace", "Rel Xen An Sanct",
                203,
                9031,
                Reagent.BatWing,
                Reagent.NoxCrystal,
                Reagent.PigIron
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(2.0); } }

        public override double RequiredSkill { get { return 99.0; } }
        public override int RequiredMana { get { return 23; } }

        public override int Body { get { return Caster.Female ? 745 : 744; } }
        public override int Hue { get { return 0x847E; } }

        public override int FireResistOffset { get { return -25; } }

        public VampiricEmbraceSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void GetCastSkills(out double min, out double max)
        {
            if (Caster.Skills[CastSkill].Value >= RequiredSkill)
            {
                min = 80.0;
                max = 120.0;
            }
            else
            {
                base.GetCastSkills(out min, out max);
            }
        }

        public override void DoEffect(Mobile m)
        {
            Effects.SendLocationParticles(EffectItem.Create(m.Location, m.Map, EffectItem.DefaultDuration), 0x373A, 1, 17, 1108, 7, 9914, 0);
            Effects.SendLocationParticles(EffectItem.Create(m.Location, m.Map, EffectItem.DefaultDuration), 0x376A, 1, 22, 67, 7, 9502, 0);
            Effects.PlaySound(m.Location, m.Map, 0x4B1);
        }
    }

    public class VengefulSpiritSpell : NecromancerSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Vengeful Spirit", "Kal Xen Bal Beh",
                203,
                9031,
                Reagent.BatWing,
                Reagent.GraveDust,
                Reagent.PigIron
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(2.0); } }

        public override double RequiredSkill { get { return 80.0; } }
        public override int RequiredMana { get { return 41; } }

        public VengefulSpiritSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public override bool CheckCast()
        {
            if (!base.CheckCast())
                return false;

            if ((Caster.Followers + 3) > Caster.FollowersMax)
            {
                Caster.SendLocalizedMessage(1049645); // You have too many followers to summon that creature.
                return false;
            }

            return true;
        }

        public void Target(Mobile m)
        {
            if (Caster == m)
            {
                Caster.SendLocalizedMessage(1061832); // You cannot exact vengeance on yourself.
            }
            else if (CheckHSequence(m))
            {
                SpellHelper.Turn(Caster, m);

                /* Summons a Revenant which haunts the target until either the target or the Revenant is dead.
                 * Revenants have the ability to track down their targets wherever they may travel.
                 * A Revenant's strength is determined by the Necromancy and Spirit Speak skills of the Caster.
                 * The effect lasts for ((Spirit Speak skill level * 80) / 120) + 10 seconds.
                 */

                TimeSpan duration = TimeSpan.FromSeconds(((GetDamageSkill(Caster) * 80) / 120) + 10);

                Revenant rev = new Revenant(Caster, m, duration);

                if (BaseCreature.Summon(rev, false, Caster, m.Location, 0x81, TimeSpan.FromSeconds(duration.TotalSeconds + 2.0)))
                    rev.FixedParticles(0x373A, 1, 15, 9909, EffectLayer.Waist);
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private VengefulSpiritSpell m_Owner;

            public InternalTarget(VengefulSpiritSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Harmful)
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

    public class WitherSpell : NecromancerSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Wither", "Kal Vas An Flam",
                203,
                9031,
                Reagent.NoxCrystal,
                Reagent.GraveDust,
                Reagent.PigIron
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(1.5); } }

        public override double RequiredSkill { get { return 60.0; } }

        public override int RequiredMana { get { return 23; } }

        public WitherSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override bool DelayedDamage { get { return false; } }

        public override void OnCast()
        {
            if (CheckSequence())
            {
                /* Creates a withering frost around the Caster,
                 * which deals Cold Damage to all valid targets in a radius of 5 tiles.
                 */

                Map map = Caster.Map;

                if (map != null)
                {
                    List<Mobile> targets = new List<Mobile>();

                    BaseCreature cbc = Caster as BaseCreature;
                    bool isMonster = (cbc != null && !cbc.Controlled && !cbc.Summoned);

                    foreach (Mobile m in Caster.GetMobilesInRange(Core.ML ? 4 : 5))
                    {
                        if (Caster != m && Caster.InLOS(m) && (isMonster || SpellHelper.ValidIndirectTarget(Caster, m)) && Caster.CanBeHarmful(m, false))
                        {
                            if (isMonster)
                            {
                                if (m is BaseCreature)
                                {
                                    BaseCreature bc = (BaseCreature)m;

                                    if (!bc.Controlled && !bc.Summoned && bc.Team == cbc.Team)
                                        continue;
                                }
                                else if (!m.Player)
                                {
                                    continue;
                                }
                            }

                            targets.Add(m);
                        }
                    }

                    Effects.PlaySound(Caster.Location, map, 0x1FB);
                    Effects.PlaySound(Caster.Location, map, 0x10B);
                    Effects.SendLocationParticles(EffectItem.Create(Caster.Location, map, EffectItem.DefaultDuration), 0x37CC, 1, 40, 97, 3, 9917, 0);

                    for (int i = 0; i < targets.Count; ++i)
                    {
                        Mobile m = targets[i];

                        Caster.DoHarmful(m);
                        m.FixedParticles(0x374A, 1, 15, 9502, 97, 3, (EffectLayer)255);

                        double damage = Utility.RandomMinMax(30, 35);

                        damage *= (300 + (m.Karma / 100) + (GetDamageSkill(Caster) * 10));
                        damage /= 1000;

                        int sdiBonus = AosAttributes.GetValue(Caster, AosAttribute.SpellDamage);

                        // PvP spell damage increase cap of 15% from an item’s magic property in Publish 33(SE)
                        if (Core.SE && m.Player && Caster.Player && sdiBonus > 15)
                            sdiBonus = 15;

                        damage *= (100 + sdiBonus);
                        damage /= 100;

                        // TODO: cap?
                        //if ( damage > 40 )
                        //	damage = 40;

                        SpellHelper.Damage(this, m, damage, 0, 0, 100, 0, 0);
                    }
                }
            }

            FinishSequence();
        }
    }

    public class WraithFormSpell : TransformationSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Wraith Form", "Rel Xen Um",
                203,
                9031,
                Reagent.NoxCrystal,
                Reagent.PigIron
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(2.0); } }

        public override double RequiredSkill { get { return 20.0; } }
        public override int RequiredMana { get { return 17; } }

        public override int Body { get { return Caster.Female ? 747 : 748; } }
        public override int Hue { get { return Caster.Female ? 0 : 0x4001; } }

        public override int PhysResistOffset { get { return +15; } }
        public override int FireResistOffset { get { return -5; } }
        public override int ColdResistOffset { get { return 0; } }
        public override int PoisResistOffset { get { return 0; } }
        public override int NrgyResistOffset { get { return -5; } }

        public WraithFormSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void DoEffect(Mobile m)
        {
            if (m is PlayerMobile)
                ((PlayerMobile)m).IgnoreMobiles = true;

            m.PlaySound(0x17F);
            m.FixedParticles(0x374A, 1, 15, 9902, 1108, 4, EffectLayer.Waist);
        }

        public override void RemoveEffect(Mobile m)
        {
            if (m is PlayerMobile && m.AccessLevel == AccessLevel.Player)
                ((PlayerMobile)m).IgnoreMobiles = false;
        }
    }
}