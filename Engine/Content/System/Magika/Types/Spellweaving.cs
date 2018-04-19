using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Server.Engines.MLQuests;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Regions;
using Server.Spells.Fifth;
using Server.Spells.Seventh;
using Server.Spells.Necromancy;
using Server.Spells.Ninjitsu;
using Server.Targeting;

namespace Server.Spells.Spellweaving
{
    public abstract class ArcanistSpell : Spell
    {
        public abstract double RequiredSkill { get; }
        public abstract int RequiredMana { get; }

        public override SkillName CastSkill { get { return SkillName.Spellweaving; } }
        public override SkillName DamageSkill { get { return SkillName.Spellweaving; } }

        public override bool ClearHandsOnCast { get { return false; } }

        private int m_CastTimeFocusLevel;

        public ArcanistSpell(Mobile caster, Item scroll, SpellInfo info)
            : base(caster, scroll, info)
        {
        }

        public virtual int FocusLevel
        {
            get { return m_CastTimeFocusLevel; }
        }

        public static int GetFocusLevel(Mobile from)
        {
            ArcaneFocus focus = FindArcaneFocus(from);

            if (focus == null || focus.Deleted)
                return 0;

            return focus.StrengthBonus;
        }

        public static ArcaneFocus FindArcaneFocus(Mobile from)
        {
            if (from == null || from.Backpack == null)
                return null;

            if (from.Holding is ArcaneFocus)
                return (ArcaneFocus)from.Holding;

            return from.Backpack.FindItemByType<ArcaneFocus>();
        }

        public static bool CheckExpansion(Mobile from)
        {
            if (!(from is PlayerMobile))
                return true;

            if (from.NetState == null)
                return false;

            return from.NetState.SupportsExpansion(Expansion.ML);
        }

        public override bool CheckCast()
        {
            if (!base.CheckCast())
                return false;

            Mobile caster = Caster;

            if (!CheckExpansion(caster))
            {
                caster.SendLocalizedMessage(1072176); // You must upgrade to the Mondain's Legacy Expansion Pack before using that ability
                return false;
            }

            if (caster is PlayerMobile)
            {
                MLQuestContext context = MLQuestSystem.GetContext((PlayerMobile)caster);

                if (context == null || !context.Spellweaving)
                {
                    caster.SendLocalizedMessage(1073220); // You must have completed the epic arcanist quest to use this ability.
                    return false;
                }
            }

            int mana = ScaleMana(RequiredMana);

            if (caster.Mana < mana)
            {
                caster.SendLocalizedMessage(1060174, mana.ToString()); // You must have at least ~1_MANA_REQUIREMENT~ Mana to use this ability.
                return false;
            }
            else if (caster.Skills[CastSkill].Value < RequiredSkill)
            {
                caster.SendLocalizedMessage(1063013, String.Format("{0}\t{1}", RequiredSkill.ToString("F1"), "#1044114")); // You need at least ~1_SKILL_REQUIREMENT~ ~2_SKILL_NAME~ skill to use that ability.
                return false;
            }

            return true;
        }

        public override void GetCastSkills(out double min, out double max)
        {
            min = RequiredSkill - 12.5;	//per 5 on friday, 2/16/07
            max = RequiredSkill + 37.5;
        }

        public override int GetMana()
        {
            return RequiredMana;
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
            m_CastTimeFocusLevel = GetFocusLevel(Caster);
        }

        public virtual void SendCastEffect()
        {
            Caster.FixedEffect(0x37C4, 10, (int)(GetCastDelay().TotalSeconds * 28), 4, 3);
        }

        public virtual bool CheckResisted(Mobile m)
        {
            double percent = (50 + 2 * (GetResistSkill(m) - GetDamageSkill(Caster))) / 100;	//TODO: According to the guide this is it.. but.. is it correct per OSI?

            if (percent <= 0)
                return false;

            if (percent >= 1.0)
                return true;

            return (percent >= Utility.RandomDouble());
        }
    }

    public abstract class ArcaneForm : ArcanistSpell, ITransformationSpell
    {
        public abstract int Body { get; }
        public virtual int Hue { get { return 0; } }

        public virtual int PhysResistOffset { get { return 0; } }
        public virtual int FireResistOffset { get { return 0; } }
        public virtual int ColdResistOffset { get { return 0; } }
        public virtual int PoisResistOffset { get { return 0; } }
        public virtual int NrgyResistOffset { get { return 0; } }

        public ArcaneForm(Mobile caster, Item scroll, SpellInfo info)
            : base(caster, scroll, info)
        {
        }

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

        public virtual double TickRate
        {
            get { return 1.0; }
        }

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

    public abstract class ArcaneSummon<T> : ArcanistSpell where T : BaseCreature
    {
        public abstract int Sound { get; }

        public ArcaneSummon(Mobile caster, Item scroll, SpellInfo info)
            : base(caster, scroll, info)
        {
        }

        public override bool CheckCast()
        {
            if (!base.CheckCast())
                return false;

            if ((Caster.Followers + 1) > Caster.FollowersMax)
            {
                Caster.SendLocalizedMessage(1074270); // You have too many followers to summon another one.
                return false;
            }

            return true;
        }

        public override void OnCast()
        {
            if (CheckSequence())
            {
                TimeSpan duration = TimeSpan.FromMinutes(Caster.Skills.Spellweaving.Value / 24 + FocusLevel * 2);
                int summons = Math.Min(1 + FocusLevel, Caster.FollowersMax - Caster.Followers);

                for (int i = 0; i < summons; i++)
                {
                    BaseCreature bc;

                    try { bc = Activator.CreateInstance<T>(); }
                    catch { break; }

                    SpellHelper.Summon(bc, Caster, Sound, duration, false, false);
                }

                FinishSequence();
            }
        }
    }

    public class ArcaneCircleSpell : ArcanistSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Arcane Circle", "Myrshalee",
                -1
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(0.5); } }

        public override double RequiredSkill { get { return 0.0; } }
        public override int RequiredMana { get { return 24; } }

        public ArcaneCircleSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override bool CheckCast()
        {
            if (!IsValidLocation(Caster.Location, Caster.Map))
            {
                Caster.SendLocalizedMessage(1072705); // You must be standing on an arcane circle, pentagram or abbatoir to use this spell.
                return false;
            }

            if (GetArcanists().Count < 2)
            {
                Caster.SendLocalizedMessage(1080452); //There are not enough spellweavers present to create an Arcane Focus.
                return false;
            }

            return base.CheckCast();
        }

        public override void OnCast()
        {
            if (CheckSequence())
            {
                Caster.FixedParticles(0x3779, 10, 20, 0x0, EffectLayer.Waist);
                Caster.PlaySound(0x5C0);

                List<Mobile> Arcanists = GetArcanists();

                TimeSpan duration = TimeSpan.FromHours(Math.Max(1, (int)(Caster.Skills.Spellweaving.Value / 24)));

                int strengthBonus = Math.Min(Arcanists.Count, IsSanctuary(Caster.Location, Caster.Map) ? 6 : 5);	//The Sanctuary is a special, single location place

                for (int i = 0; i < Arcanists.Count; i++)
                    GiveArcaneFocus(Arcanists[i], duration, strengthBonus);
            }

            FinishSequence();
        }

        private static bool IsSanctuary(Point3D p, Map m)
        {
            return (m == Map.Trammel || m == Map.Felucca) && p.X == 6267 && p.Y == 131;
        }

        private static bool IsValidLocation(Point3D location, Map map)
        {
            LandTile lt = map.Tiles.GetLandTile(location.X, location.Y);         // Land   Tiles            

            if (IsValidTile(lt.ID) && lt.Z == location.Z)
                return true;

            StaticTile[] tiles = map.Tiles.GetStaticTiles(location.X, location.Y); // Static Tiles

            for (int i = 0; i < tiles.Length; ++i)
            {
                StaticTile t = tiles[i];
                ItemData id = TileData.ItemTable[t.ID & TileData.MaxItemValue];

                int tand = t.ID;

                if (t.Z + id.CalcHeight != location.Z)
                    continue;
                else if (IsValidTile(tand))
                    return true;
            }

            IPooledEnumerable eable = map.GetItemsInRange(location, 0);      // Added  Tiles

            foreach (Item item in eable)
            {
                ItemData id = item.ItemData;

                if (item == null || item.Z + id.CalcHeight != location.Z)
                    continue;
                else if (IsValidTile(item.ItemID))
                {
                    eable.Free();
                    return true;
                }
            }

            eable.Free();
            return false;
        }

        public static bool IsValidTile(int itemID)
        {
            //Per OSI, Center tile only
            return (itemID == 0xFEA || itemID == 0x1216 || itemID == 0x307F || itemID == 0x1D10 || itemID == 0x1D0F || itemID == 0x1D1F || itemID == 0x1D12);	// Pentagram center, Abbatoir center, Arcane Circle Center, Bloody Pentagram has 4 tiles at center
        }

        private List<Mobile> GetArcanists()
        {
            List<Mobile> weavers = new List<Mobile>();

            weavers.Add(Caster);

            //OSI Verified: Even enemies/combatants count
            foreach (Mobile m in Caster.GetMobilesInRange(1))	//Range verified as 1
            {
                if (m != Caster && m is PlayerMobile && Caster.CanBeBeneficial(m, false) && Math.Abs(Caster.Skills.Spellweaving.Value - m.Skills.Spellweaving.Value) <= 20 && !(m is Clone))
                {
                    weavers.Add(m);
                }
                // Everyone gets the Arcane Focus, power capped elsewhere
            }

            return weavers;
        }

        private void GiveArcaneFocus(Mobile to, TimeSpan duration, int strengthBonus)
        {
            if (to == null)	//Sanity
                return;

            ArcaneFocus focus = FindArcaneFocus(to);

            if (focus == null)
            {
                ArcaneFocus f = new ArcaneFocus(duration, strengthBonus);
                if (to.PlaceInBackpack(f))
                {
                    f.SendTimeRemainingMessage(to);
                    to.SendLocalizedMessage(1072740); // An arcane focus appears in your backpack.
                }
                else
                {
                    f.Delete();
                }

            }
            else		//OSI renewal rules: the new one will override the old one, always.
            {
                to.SendLocalizedMessage(1072828); // Your arcane focus is renewed.
                focus.LifeSpan = duration;
                focus.CreationTime = DateTime.UtcNow;
                focus.StrengthBonus = strengthBonus;
                focus.InvalidateProperties();
                focus.SendTimeRemainingMessage(to);
            }
        }
    }

    public class AttuneWeaponSpell : ArcanistSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Attune Weapon", "Haeldril",
                -1
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(1.0); } }

        public override double RequiredSkill { get { return 0.0; } }
        public override int RequiredMana { get { return 24; } }

        public AttuneWeaponSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override bool CheckCast()
        {
            if (m_Table.ContainsKey(Caster))
            {
                Caster.SendLocalizedMessage(501775); // This spell is already in effect.
                return false;
            }
            else if (!Caster.CanBeginAction(typeof(AttuneWeaponSpell)))
            {
                Caster.SendLocalizedMessage(1075124); // You must wait before casting that spell again.
                return false;
            }

            return base.CheckCast();
        }

        public override void OnCast()
        {
            if (CheckSequence())
            {
                Caster.PlaySound(0x5C3);
                Caster.FixedParticles(0x3728, 1, 13, 0x26B8, 0x455, 7, EffectLayer.Waist);
                Caster.FixedParticles(0x3779, 1, 15, 0x251E, 0x3F, 7, EffectLayer.Waist);

                double skill = Caster.Skills[SkillName.Spellweaving].Value;

                int damageAbsorb = (int)(18 + ((skill - 10) / 10) * 3 + (FocusLevel * 6));
                Caster.MeleeDamageAbsorb = damageAbsorb;

                TimeSpan duration = TimeSpan.FromSeconds(60 + (FocusLevel * 12));

                ExpireTimer t = new ExpireTimer(Caster, duration);
                t.Start();

                m_Table[Caster] = t;

                Caster.BeginAction(typeof(AttuneWeaponSpell));

                BuffInfo.AddBuff(Caster, new BuffInfo(BuffIcon.AttuneWeapon, 1075798, duration, Caster, damageAbsorb.ToString()));
            }

            FinishSequence();
        }

        private static Dictionary<Mobile, ExpireTimer> m_Table = new Dictionary<Mobile, ExpireTimer>();

        public static void TryAbsorb(Mobile defender, ref int damage)
        {
            if (damage == 0 || !IsAbsorbing(defender) || defender.MeleeDamageAbsorb <= 0)
                return;

            int absorbed = Math.Min(damage, defender.MeleeDamageAbsorb);

            damage -= absorbed;
            defender.MeleeDamageAbsorb -= absorbed;

            defender.SendLocalizedMessage(1075127, String.Format("{0}\t{1}", absorbed, defender.MeleeDamageAbsorb)); // ~1_damage~ point(s) of damage have been absorbed. A total of ~2_remaining~ point(s) of shielding remain.

            if (defender.MeleeDamageAbsorb <= 0)
                StopAbsorbing(defender, true);
        }

        public static bool IsAbsorbing(Mobile m)
        {
            return m_Table.ContainsKey(m);
        }

        public static void StopAbsorbing(Mobile m, bool message)
        {
            ExpireTimer t;
            if (m_Table.TryGetValue(m, out t))
            {
                t.DoExpire(message);
            }
        }

        private class ExpireTimer : Timer
        {
            private Mobile m_Mobile;

            public ExpireTimer(Mobile m, TimeSpan delay)
                : base(delay)
            {
                m_Mobile = m;
            }

            protected override void OnTick()
            {
                DoExpire(true);
            }

            public void DoExpire(bool message)
            {
                Stop();

                m_Mobile.MeleeDamageAbsorb = 0;

                if (message)
                {
                    m_Mobile.SendLocalizedMessage(1075126); // Your attunement fades.
                    m_Mobile.PlaySound(0x1F8);
                }

                m_Table.Remove(m_Mobile);

                Timer.DelayCall(TimeSpan.FromSeconds(120), delegate { m_Mobile.EndAction(typeof(AttuneWeaponSpell)); });
                BuffInfo.RemoveBuff(m_Mobile, BuffIcon.AttuneWeapon);
            }
        }
    }

    public class EssenceOfWindSpell : ArcanistSpell
    {
        private static SpellInfo m_Info = new SpellInfo("Essence of Wind", "Anathrae", -1);

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(3.0); } }

        public override double RequiredSkill { get { return 52.0; } }
        public override int RequiredMana { get { return 40; } }

        public EssenceOfWindSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            if (CheckSequence())
            {
                Caster.PlaySound(0x5C6);

                int range = 5 + FocusLevel;
                int damage = 25 + FocusLevel;

                double skill = Caster.Skills[SkillName.Spellweaving].Value;

                TimeSpan duration = TimeSpan.FromSeconds((int)(skill / 24) + FocusLevel);

                int fcMalus = FocusLevel + 1;
                int ssiMalus = 2 * (FocusLevel + 1);

                List<Mobile> targets = new List<Mobile>();

                foreach (Mobile m in Caster.GetMobilesInRange(range))
                {
                    if (Caster != m && Caster.InLOS(m) && SpellHelper.ValidIndirectTarget(Caster, m) && Caster.CanBeHarmful(m, false))
                        targets.Add(m);
                }

                for (int i = 0; i < targets.Count; i++)
                {
                    Mobile m = targets[i];

                    Caster.DoHarmful(m);

                    SpellHelper.Damage(this, m, damage, 0, 0, 100, 0, 0);

                    if (!CheckResisted(m))	//No message on resist
                    {
                        m_Table[m] = new EssenceOfWindInfo(m, fcMalus, ssiMalus, duration);

                        BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.EssenceOfWind, 1075802, duration, m, String.Format("{0}\t{1}", fcMalus.ToString(), ssiMalus.ToString())));
                    }
                }
            }

            FinishSequence();
        }

        private static Dictionary<Mobile, EssenceOfWindInfo> m_Table = new Dictionary<Mobile, EssenceOfWindInfo>();

        private class EssenceOfWindInfo
        {
            private Mobile m_Defender;
            private int m_FCMalus;
            private int m_SSIMalus;
            private ExpireTimer m_Timer;

            public Mobile Defender { get { return m_Defender; } }
            public int FCMalus { get { return m_FCMalus; } }
            public int SSIMalus { get { return m_SSIMalus; } }
            public ExpireTimer Timer { get { return m_Timer; } }

            public EssenceOfWindInfo(Mobile defender, int fcMalus, int ssiMalus, TimeSpan duration)
            {
                m_Defender = defender;
                m_FCMalus = fcMalus;
                m_SSIMalus = ssiMalus;

                m_Timer = new ExpireTimer(m_Defender, duration);
                m_Timer.Start();
            }
        }

        public static int GetFCMalus(Mobile m)
        {
            EssenceOfWindInfo info;

            if (m_Table.TryGetValue(m, out info))
                return info.FCMalus;

            return 0;
        }

        public static int GetSSIMalus(Mobile m)
        {
            EssenceOfWindInfo info;

            if (m_Table.TryGetValue(m, out info))
                return info.SSIMalus;

            return 0;
        }

        public static bool IsDebuffed(Mobile m)
        {
            return m_Table.ContainsKey(m);
        }

        public static void StopDebuffing(Mobile m, bool message)
        {
            EssenceOfWindInfo info;

            if (m_Table.TryGetValue(m, out info))
                info.Timer.DoExpire(message);
        }

        private class ExpireTimer : Timer
        {
            private Mobile m_Mobile;

            public ExpireTimer(Mobile m, TimeSpan delay)
                : base(delay)
            {
                m_Mobile = m;
            }

            protected override void OnTick()
            {
                DoExpire(true);
            }

            public void DoExpire(bool message)
            {
                Stop();
                /*
                if( message )
                {
                }
                */
                m_Table.Remove(m_Mobile);

                BuffInfo.RemoveBuff(m_Mobile, BuffIcon.EssenceOfWind);
            }
        }
    }

    public class EtherealVoyageSpell : ArcaneForm
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Ethereal Voyage", "Orlavdra",
                -1
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(3.5); } }

        public override double RequiredSkill { get { return 24.0; } }
        public override int RequiredMana { get { return 32; } }

        public override int Body { get { return 0x302; } }
        public override int Hue { get { return 0x48F; } }

        public EtherealVoyageSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public static void Initialize()
        {
            EventSink.AggressiveAction += new AggressiveActionEventHandler(delegate(AggressiveActionEventArgs e)
            {
                if (TransformationSpellHelper.UnderTransformation(e.Aggressor, typeof(EtherealVoyageSpell)))
                {
                    TransformationSpellHelper.RemoveContext(e.Aggressor, true);
                }
            });
        }

        public override bool CheckCast()
        {
            if (TransformationSpellHelper.UnderTransformation(Caster, typeof(EtherealVoyageSpell)))
            {
                Caster.SendLocalizedMessage(501775); // This spell is already in effect.
            }
            else if (!Caster.CanBeginAction(typeof(EtherealVoyageSpell)))
            {
                Caster.SendLocalizedMessage(1075124); // You must wait before casting that spell again.
            }
            else if (Caster.Combatant != null)
            {
                Caster.SendLocalizedMessage(1072586); // You cannot cast Ethereal Voyage while you are in combat.
            }
            else
            {
                return base.CheckCast();
            }

            return false;
        }

        public override void DoEffect(Mobile m)
        {
            m.PlaySound(0x5C8);
            m.SendLocalizedMessage(1074770); // You are now under the effects of Ethereal Voyage.

            double skill = Caster.Skills.Spellweaving.Value;

            TimeSpan duration = TimeSpan.FromSeconds(12 + (int)(skill / 24) + (FocusLevel * 2));

            Timer.DelayCall<Mobile>(duration, new TimerStateCallback<Mobile>(RemoveEffect), Caster);

            Caster.BeginAction(typeof(EtherealVoyageSpell));	//Cannot cast this spell for another 5 minutes(300sec) after effect removed.

            BuffInfo.AddBuff(Caster, new BuffInfo(BuffIcon.EtherealVoyage, 1031613, 1075805, duration, Caster));
        }

        public override void RemoveEffect(Mobile m)
        {
            m.SendLocalizedMessage(1074771); // You are no longer under the effects of Ethereal Voyage.

            TransformationSpellHelper.RemoveContext(m, true);

            Timer.DelayCall(TimeSpan.FromMinutes(5), delegate
            {
                m.EndAction(typeof(EtherealVoyageSpell));
            });

            BuffInfo.RemoveBuff(m, BuffIcon.EtherealVoyage);
        }
    }

    public class GiftOfLifeSpell : ArcanistSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Gift of Life", "Illorae",
                -1
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(4.0); } }

        public override double RequiredSkill { get { return 38.0; } }
        public override int RequiredMana { get { return 70; } }

        public GiftOfLifeSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public static void Initialize()
        {
            EventSink.PlayerDeath += new PlayerDeathEventHandler(delegate(PlayerDeathEventArgs e)
            {
                HandleDeath(e.Mobile);
            });
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(Mobile m)
        {
            BaseCreature bc = m as BaseCreature;

            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (m.IsDeadBondedPet || !m.Alive)
            {
                // As per Osi: Nothing happens.
            }
            else if (m != Caster && (bc == null || !bc.IsBonded || bc.ControlMaster != Caster))
            {
                Caster.SendLocalizedMessage(1072077); // You may only cast this spell on yourself or a bonded pet.
            }
            else if (m_Table.ContainsKey(m))
            {
                Caster.SendLocalizedMessage(501775); // This spell is already in effect.
            }
            else if (CheckBSequence(m))
            {
                if (Caster == m)
                {
                    Caster.SendLocalizedMessage(1074774); // You weave powerful magic, protecting yourself from death.
                }
                else
                {
                    Caster.SendLocalizedMessage(1074775); // You weave powerful magic, protecting your pet from death.
                    SpellHelper.Turn(Caster, m);
                }


                m.PlaySound(0x244);
                m.FixedParticles(0x3709, 1, 30, 0x26ED, 5, 2, EffectLayer.Waist);
                m.FixedParticles(0x376A, 1, 30, 0x251E, 5, 3, EffectLayer.Waist);

                double skill = Caster.Skills[SkillName.Spellweaving].Value;

                TimeSpan duration = TimeSpan.FromMinutes(((int)(skill / 24)) * 2 + FocusLevel);

                ExpireTimer t = new ExpireTimer(m, duration, this);
                t.Start();

                m_Table[m] = t;

                BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.GiftOfLife, 1031615, 1075807, duration, m, null, true));
            }

            FinishSequence();
        }

        private static Dictionary<Mobile, ExpireTimer> m_Table = new Dictionary<Mobile, ExpireTimer>();

        public static void HandleDeath(Mobile m)
        {
            if (m_Table.ContainsKey(m))
                Timer.DelayCall<Mobile>(TimeSpan.FromSeconds(Utility.RandomMinMax(2, 4)), new TimerStateCallback<Mobile>(HandleDeath_OnCallback), m);
        }

        private static void HandleDeath_OnCallback(Mobile m)
        {
            ExpireTimer timer;

            if (m_Table.TryGetValue(m, out timer))
            {
                double hitsScalar = timer.Spell.HitsScalar;

                if (m is BaseCreature && m.IsDeadBondedPet)
                {
                    BaseCreature pet = (BaseCreature)m;
                    Mobile master = pet.GetMaster();

                    if (master != null && master.NetState != null && Utility.InUpdateRange(pet, master))
                    {
                        master.CloseGump(typeof(PetResurrectGump));
                        master.SendGump(new PetResurrectGump(master, pet, hitsScalar));
                    }
                    else
                    {
                        List<Mobile> friends = pet.Friends;

                        for (int i = 0; friends != null && i < friends.Count; i++)
                        {
                            Mobile friend = friends[i];

                            if (friend.NetState != null && Utility.InUpdateRange(pet, friend))
                            {
                                friend.CloseGump(typeof(PetResurrectGump));
                                friend.SendGump(new PetResurrectGump(friend, pet));
                                break;
                            }
                        }
                    }
                }
                else
                {
                    m.CloseGump(typeof(ResurrectGump));
                    m.SendGump(new ResurrectGump(m, hitsScalar));
                }

                //Per OSI, buff is removed when gump sent, irregardless of online status or acceptence
                timer.DoExpire();
            }

        }

        public double HitsScalar { get { return ((Caster.Skills.Spellweaving.Value / 2.4) + FocusLevel) / 100; } }

        public static void OnLogin(LoginEventArgs e)
        {
            Mobile m = e.Mobile;

            if (m == null || m.Alive || m_Table[m] == null)
                return;

            HandleDeath_OnCallback(m);
        }

        private class ExpireTimer : Timer
        {
            private Mobile m_Mobile;

            private GiftOfLifeSpell m_Spell;

            public GiftOfLifeSpell Spell { get { return m_Spell; } }

            public ExpireTimer(Mobile m, TimeSpan delay, GiftOfLifeSpell spell)
                : base(delay)
            {
                m_Mobile = m;
                m_Spell = spell;
            }

            protected override void OnTick()
            {
                DoExpire();
            }

            public void DoExpire()
            {
                Stop();

                m_Mobile.SendLocalizedMessage(1074776); // You are no longer protected with Gift of Life.
                m_Table.Remove(m_Mobile);

                BuffInfo.RemoveBuff(m_Mobile, BuffIcon.GiftOfLife);
            }
        }

        public class InternalTarget : Target
        {
            private GiftOfLifeSpell m_Owner;

            public InternalTarget(GiftOfLifeSpell owner)
                : base(10, false, TargetFlags.Beneficial)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile m, object o)
            {
                if (o is Mobile)
                {
                    m_Owner.Target((Mobile)o);
                }
                else
                {
                    m.SendLocalizedMessage(1072077); // You may only cast this spell on yourself or a bonded pet.
                }
            }

            protected override void OnTargetFinish(Mobile m)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class GiftOfRenewalSpell : ArcanistSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Gift of Renewal", "Olorisstra",
                -1
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(3.0); } }

        public override double RequiredSkill { get { return 0.0; } }
        public override int RequiredMana { get { return 24; } }

        public GiftOfRenewalSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(Mobile m)
        {
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            if (m_Table.ContainsKey(m))
            {
                Caster.SendLocalizedMessage(501775); // This spell is already in effect.
            }
            else if (!Caster.CanBeginAction(typeof(GiftOfRenewalSpell)))
            {
                Caster.SendLocalizedMessage(501789); // You must wait before trying again.
            }
            else if (CheckBSequence(m))
            {
                SpellHelper.Turn(Caster, m);

                Caster.FixedEffect(0x374A, 10, 20);
                Caster.PlaySound(0x5C9);

                if (m.Poisoned)
                {
                    m.CurePoison(m);
                }
                else
                {
                    double skill = Caster.Skills[SkillName.Spellweaving].Value;

                    int hitsPerRound = 5 + (int)(skill / 24) + FocusLevel;
                    TimeSpan duration = TimeSpan.FromSeconds(30 + (FocusLevel * 10));

                    GiftOfRenewalInfo info = new GiftOfRenewalInfo(Caster, m, hitsPerRound);

                    Timer.DelayCall(duration,
                        delegate
                        {
                            if (StopEffect(m))
                            {
                                m.PlaySound(0x455);
                                m.SendLocalizedMessage(1075071); // The Gift of Renewal has faded.
                            }
                        });



                    m_Table[m] = info;

                    Caster.BeginAction(typeof(GiftOfRenewalSpell));

                    BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.GiftOfRenewal, 1031602, 1075797, duration, m, hitsPerRound.ToString()));
                }
            }

            FinishSequence();
        }

        private static Dictionary<Mobile, GiftOfRenewalInfo> m_Table = new Dictionary<Mobile, GiftOfRenewalInfo>();

        private class GiftOfRenewalInfo
        {
            public Mobile m_Caster;
            public Mobile m_Mobile;
            public int m_HitsPerRound;
            public InternalTimer m_Timer;

            public GiftOfRenewalInfo(Mobile caster, Mobile mobile, int hitsPerRound)
            {
                m_Caster = caster;
                m_Mobile = mobile;
                m_HitsPerRound = hitsPerRound;

                m_Timer = new InternalTimer(this);
                m_Timer.Start();
            }
        }

        private class InternalTimer : Timer
        {
            public GiftOfRenewalInfo m_Info;

            public InternalTimer(GiftOfRenewalInfo info)
                : base(TimeSpan.FromSeconds(2.0), TimeSpan.FromSeconds(2.0))
            {
                m_Info = info;
            }

            protected override void OnTick()
            {
                Mobile m = m_Info.m_Mobile;

                if (!m_Table.ContainsKey(m))
                {
                    Stop();
                    return;
                }

                if (!m.Alive)
                {
                    Stop();
                    StopEffect(m);
                    return;
                }

                if (m.Hits >= m.HitsMax)
                    return;

                int toHeal = m_Info.m_HitsPerRound;

                SpellHelper.Heal(toHeal, m, m_Info.m_Caster);
                m.FixedParticles(0x376A, 9, 32, 5005, EffectLayer.Waist);
            }
        }

        public static bool StopEffect(Mobile m)
        {
            GiftOfRenewalInfo info;

            if (m_Table.TryGetValue(m, out info))
            {
                m_Table.Remove(m);

                info.m_Timer.Stop();
                BuffInfo.RemoveBuff(m, BuffIcon.GiftOfRenewal);

                Timer.DelayCall(TimeSpan.FromSeconds(60), delegate { info.m_Caster.EndAction(typeof(GiftOfRenewalSpell)); });

                return true;
            }

            return false;
        }

        public class InternalTarget : Target
        {
            private GiftOfRenewalSpell m_Owner;

            public InternalTarget(GiftOfRenewalSpell owner)
                : base(10, false, TargetFlags.Beneficial)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile m, object o)
            {
                if (o is Mobile)
                {
                    m_Owner.Target((Mobile)o);
                }
            }

            protected override void OnTargetFinish(Mobile m)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class ImmolatingWeaponSpell : ArcanistSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Immolating Weapon", "Thalshara",
                -1
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(1.0); } }

        public override double RequiredSkill { get { return 10.0; } }
        public override int RequiredMana { get { return 32; } }

        public ImmolatingWeaponSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override bool CheckCast()
        {
            BaseWeapon weapon = Caster.Weapon as BaseWeapon;

            if (weapon == null || weapon is Fists || weapon is BaseRanged)
            {
                Caster.SendLocalizedMessage(1060179); // You must be wielding a weapon to use this ability!
                return false;
            }

            return base.CheckCast();
        }

        public override void OnCast()
        {
            BaseWeapon weapon = Caster.Weapon as BaseWeapon;

            if (weapon == null || weapon is Fists || weapon is BaseRanged)
            {
                Caster.SendLocalizedMessage(1060179); // You must be wielding a weapon to use this ability!
            }
            else if (CheckSequence())
            {
                Caster.PlaySound(0x5CA);
                Caster.FixedParticles(0x36BD, 20, 10, 5044, EffectLayer.Head);

                if (!IsImmolating(weapon)) // On OSI, the effect is not re-applied
                {
                    double skill = Caster.Skills.Spellweaving.Value;

                    int duration = 10 + (int)(skill / 24) + FocusLevel;
                    int damage = 5 + (int)(skill / 24) + FocusLevel;

                    Timer stopTimer = Timer.DelayCall<BaseWeapon>(TimeSpan.FromSeconds(duration), StopImmolating, weapon);

                    m_WeaponDamageTable[weapon] = new ImmolatingWeaponEntry(damage, stopTimer, Caster);
                    weapon.InvalidateProperties();
                }
            }

            FinishSequence();
        }

        private static Dictionary<BaseWeapon, ImmolatingWeaponEntry> m_WeaponDamageTable = new Dictionary<BaseWeapon, ImmolatingWeaponEntry>();

        public static bool IsImmolating(BaseWeapon weapon)
        {
            return m_WeaponDamageTable.ContainsKey(weapon);
        }

        public static int GetImmolatingDamage(BaseWeapon weapon)
        {
            ImmolatingWeaponEntry entry;

            if (m_WeaponDamageTable.TryGetValue(weapon, out entry))
                return entry.m_Damage;

            return 0;
        }

        public static void DoEffect(BaseWeapon weapon, Mobile target)
        {
            Timer.DelayCall<DelayedEffectEntry>(TimeSpan.FromSeconds(0.25), FinishEffect, new DelayedEffectEntry(weapon, target));
        }

        private static void FinishEffect(DelayedEffectEntry effect)
        {
            ImmolatingWeaponEntry entry;

            if (m_WeaponDamageTable.TryGetValue(effect.m_Weapon, out entry))
                AOS.Damage(effect.m_Target, entry.m_Caster, entry.m_Damage, 0, 100, 0, 0, 0);
        }

        public static void StopImmolating(BaseWeapon weapon)
        {
            ImmolatingWeaponEntry entry;

            if (m_WeaponDamageTable.TryGetValue(weapon, out entry))
            {
                if (entry.m_Caster != null)
                    entry.m_Caster.PlaySound(0x27);

                entry.m_Timer.Stop();

                m_WeaponDamageTable.Remove(weapon);

                weapon.InvalidateProperties();
            }
        }

        private class ImmolatingWeaponEntry
        {
            public int m_Damage;
            public Timer m_Timer;
            public Mobile m_Caster;

            public ImmolatingWeaponEntry(int damage, Timer stopTimer, Mobile caster)
            {
                m_Damage = damage;
                m_Timer = stopTimer;
                m_Caster = caster;
            }
        }

        private class DelayedEffectEntry
        {
            public BaseWeapon m_Weapon;
            public Mobile m_Target;

            public DelayedEffectEntry(BaseWeapon weapon, Mobile target)
            {
                m_Weapon = weapon;
                m_Target = target;
            }
        }
    }

    public class NatureFurySpell : ArcanistSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Nature's Fury", "Rauvvrae",
                -1,
                false
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(1.5); } }

        public override double RequiredSkill { get { return 0.0; } }
        public override int RequiredMana { get { return 24; } }

        public NatureFurySpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override bool CheckCast()
        {
            if (!base.CheckCast())
                return false;

            if ((Caster.Followers + 1) > Caster.FollowersMax)
            {
                Caster.SendLocalizedMessage(1049645); // You have too many followers to summon that creature.
                return false;
            }

            return true;
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(IPoint3D point)
        {
            Point3D p = new Point3D(point);
            Map map = Caster.Map;

            if (map == null)
                return;

            HouseRegion r = Region.Find(p, map).GetRegion(typeof(HouseRegion)) as HouseRegion;

            if (r != null && r.House != null && !r.House.IsFriend(Caster))
                return;

            if (!map.CanSpawnMobile(p.X, p.Y, p.Z))
            {
                Caster.SendLocalizedMessage(501942); // That location is blocked.
            }
            else if (SpellHelper.CheckTown(p, Caster) && CheckSequence())
            {
                TimeSpan duration = TimeSpan.FromSeconds(Caster.Skills.Spellweaving.Value / 24 + 25 + FocusLevel * 2);

                NatureFury nf = new NatureFury();
                BaseCreature.Summon(nf, false, Caster, p, 0x5CB, duration);

                new InternalTimer(nf).Start();
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private NatureFurySpell m_Owner;

            public InternalTarget(NatureFurySpell owner)
                : base(10, true, TargetFlags.None)
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
                if (m_Owner != null)
                    m_Owner.FinishSequence();
            }
        }

        private class InternalTimer : Timer
        {
            private NatureFury m_NatureFury;

            public InternalTimer(NatureFury nf)
                : base(TimeSpan.FromSeconds(5.0), TimeSpan.FromSeconds(5.0))
            {
                m_NatureFury = nf;
            }

            protected override void OnTick()
            {
                if (m_NatureFury.Deleted || !m_NatureFury.Alive || m_NatureFury.DamageMin > 20)
                {
                    Stop();
                }
                else
                {
                    ++m_NatureFury.DamageMin;
                    ++m_NatureFury.DamageMax;
                }
            }
        }
    }

    public class ReaperFormSpell : ArcaneForm
    {
        private static SpellInfo m_Info = new SpellInfo("Reaper Form", "Tarisstree", -1);

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(2.5); } }

        public static void Initialize()
        {
            EventSink.Login += new LoginEventHandler(OnLogin);
        }

        public static void OnLogin(LoginEventArgs e)
        {
            TransformContext context = TransformationSpellHelper.GetContext(e.Mobile);

            if (context != null && context.Type == typeof(ReaperFormSpell))
                e.Mobile.Send(SpeedControl.WalkSpeed);
        }

        public override double RequiredSkill { get { return 24.0; } }
        public override int RequiredMana { get { return 34; } }

        public override int Body { get { return 0x11D; } }

        public override int FireResistOffset { get { return -25; } }
        public override int PhysResistOffset { get { return 5 + FocusLevel; } }
        public override int ColdResistOffset { get { return 5 + FocusLevel; } }
        public override int PoisResistOffset { get { return 5 + FocusLevel; } }
        public override int NrgyResistOffset { get { return 5 + FocusLevel; } }

        public virtual int SwingSpeedBonus { get { return 10 + FocusLevel; } }
        public virtual int SpellDamageBonus { get { return 10 + FocusLevel; } }

        public ReaperFormSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void DoEffect(Mobile m)
        {
            m.PlaySound(0x1BA);

            m.Send(SpeedControl.WalkSpeed);
        }

        public override void RemoveEffect(Mobile m)
        {
            m.Send(SpeedControl.Disable);
        }
    }

    public class SummonFeySpell : ArcaneSummon<ArcaneFey>
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Summon Fey", "Alalithra",
                -1
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(1.5); } }

        public override double RequiredSkill { get { return 38.0; } }
        public override int RequiredMana { get { return 10; } }

        public SummonFeySpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override int Sound { get { return 0x217; } }

        public override bool CheckSequence()
        {
            Mobile caster = Caster;

            // This is done after casting completes
            if (caster is PlayerMobile)
            {
                MLQuestContext context = MLQuestSystem.GetContext((PlayerMobile)caster);

                if (context == null || !context.SummonFey)
                {
                    caster.SendLocalizedMessage(1074563); // You haven't forged a friendship with the fey and are unable to summon their aid.
                    return false;
                }
            }

            return base.CheckSequence();
        }
    }

    public class SummonFiendSpell : ArcaneSummon<ArcaneFiend>
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Summon Fiend", "Nylisstra",
                -1
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(2.0); } }

        public override double RequiredSkill { get { return 38.0; } }
        public override int RequiredMana { get { return 10; } }

        public SummonFiendSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override int Sound { get { return 0x216; } }

        public override bool CheckSequence()
        {
            Mobile caster = Caster;

            // This is done after casting completes
            if (caster is PlayerMobile)
            {
                MLQuestContext context = MLQuestSystem.GetContext((PlayerMobile)caster);

                if (context == null || !context.SummonFiend)
                {
                    caster.SendLocalizedMessage(1074564); // You haven't demonstrated mastery to summon a fiend.
                    return false;
                }
            }

            return base.CheckSequence();
        }
    }

    public class ThunderstormSpell : ArcanistSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Thunderstorm", "Erelonia",
                -1
            );

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(1.5); } }

        public override double RequiredSkill { get { return 10.0; } }
        public override int RequiredMana { get { return 32; } }

        public ThunderstormSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            if (CheckSequence())
            {
                Caster.PlaySound(0x5CE);

                double skill = Caster.Skills[SkillName.Spellweaving].Value;

                int damage = Math.Max(11, 10 + (int)(skill / 24)) + FocusLevel;

                int sdiBonus = AosAttributes.GetValue(Caster, AosAttribute.SpellDamage);

                int pvmDamage = damage * (100 + sdiBonus);
                pvmDamage /= 100;

                if (sdiBonus > 15)
                    sdiBonus = 15;

                int pvpDamage = damage * (100 + sdiBonus);
                pvpDamage /= 100;

                int range = 2 + FocusLevel;
                TimeSpan duration = TimeSpan.FromSeconds(5 + FocusLevel);

                List<Mobile> targets = new List<Mobile>();

                foreach (Mobile m in Caster.GetMobilesInRange(range))
                {
                    if (Caster != m && SpellHelper.ValidIndirectTarget(Caster, m) && Caster.CanBeHarmful(m, false) && Caster.InLOS(m))
                        targets.Add(m);
                }

                for (int i = 0; i < targets.Count; i++)
                {
                    Mobile m = targets[i];

                    Caster.DoHarmful(m);

                    Spell oldSpell = m.Spell as Spell;

                    SpellHelper.Damage(this, m, (m.Player && Caster.Player) ? pvpDamage : pvmDamage, 0, 0, 0, 0, 100);

                    if (oldSpell != null && oldSpell != m.Spell)
                    {
                        if (!CheckResisted(m))
                        {
                            m_Table[m] = Timer.DelayCall<Mobile>(duration, DoExpire, m);

                            BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.Thunderstorm, 1075800, duration, m, GetCastRecoveryMalus(m)));
                        }
                    }
                }
            }

            FinishSequence();
        }

        private static Dictionary<Mobile, Timer> m_Table = new Dictionary<Mobile, Timer>();

        public static int GetCastRecoveryMalus(Mobile m)
        {
            return m_Table.ContainsKey(m) ? 6 : 0;
        }

        public static void DoExpire(Mobile m)
        {
            Timer t;

            if (m_Table.TryGetValue(m, out t))
            {
                t.Stop();
                m_Table.Remove(m);

                BuffInfo.RemoveBuff(m, BuffIcon.Thunderstorm);
            }
        }
    }

    public class WordOfDeathSpell : ArcanistSpell
    {
        private static SpellInfo m_Info = new SpellInfo("Word of Death", "Nyraxle", -1);

        public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(3.5); } }

        public override double RequiredSkill { get { return 80.0; } }
        public override int RequiredMana { get { return 50; } }

        public WordOfDeathSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(Mobile m)
        {
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckHSequence(m))
            {
                Point3D loc = m.Location;
                loc.Z += 50;

                m.PlaySound(0x211);
                m.FixedParticles(0x3779, 1, 30, 0x26EC, 0x3, 0x3, EffectLayer.Waist);

                Effects.SendMovingParticles(new Entity(Serial.Zero, loc, m.Map), new Entity(Serial.Zero, m.Location, m.Map), 0xF5F, 1, 0, true, false, 0x21, 0x3F, 0x251D, 0, 0, EffectLayer.Head, 0);

                double percentage = 0.05 * FocusLevel;

                int damage;

                if (!m.Player && (((double)m.Hits / (double)m.HitsMax) < percentage))
                {
                    damage = 300;
                }
                else
                {
                    int minDamage = (int)Caster.Skills.Spellweaving.Value / 5;
                    int maxDamage = (int)Caster.Skills.Spellweaving.Value / 3;
                    damage = Utility.RandomMinMax(minDamage, maxDamage);
                    int damageBonus = AosAttributes.GetValue(Caster, AosAttribute.SpellDamage);
                    if (m.Player && damageBonus > 15)
                        damageBonus = 15;
                    damage *= damageBonus + 100;
                    damage /= 100;
                }

                int[] types = new int[4];
                types[Utility.Random(types.Length)] = 100;

                SpellHelper.Damage(this, m, damage, 0, types[0], types[1], types[2], types[3]);	//Chaos damage.  Random elemental damage
            }

            FinishSequence();
        }

        public class InternalTarget : Target
        {
            private WordOfDeathSpell m_Owner;

            public InternalTarget(WordOfDeathSpell owner)
                : base(10, false, TargetFlags.Harmful)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile m, object o)
            {
                if (o is Mobile)
                {
                    m_Owner.Target((Mobile)o);
                }
            }

            protected override void OnTargetFinish(Mobile m)
            {
                m_Owner.FinishSequence();
            }
        }
    }
}