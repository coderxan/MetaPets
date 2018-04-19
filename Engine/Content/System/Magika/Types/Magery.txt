using System;
using System.Collections;
using System.Collections.Generic;

using Server;
using Server.Engines.PartySystem;
using Server.Gumps;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Regions;
using Server.Spells;
using Server.Spells.Fifth;
using Server.Spells.Seventh;
using Server.Spells.Chivalry;
using Server.Spells.Necromancy;
using Server.Targeting;
using Server.Targets;

namespace Server
{
    public interface ITelekinesisable : IPoint3D
    {
        void OnTelekinesis(Mobile from);
    }
}

namespace Server.Gumps
{
    public class PolymorphEntry
    {
        public static readonly PolymorphEntry Chicken = new PolymorphEntry(8401, 0xD0, 1015236, 15, 10);
        public static readonly PolymorphEntry Dog = new PolymorphEntry(8405, 0xD9, 1015237, 17, 10);
        public static readonly PolymorphEntry Wolf = new PolymorphEntry(8426, 0xE1, 1015238, 18, 10);
        public static readonly PolymorphEntry Panther = new PolymorphEntry(8473, 0xD6, 1015239, 20, 14);
        public static readonly PolymorphEntry Gorilla = new PolymorphEntry(8437, 0x1D, 1015240, 23, 10);
        public static readonly PolymorphEntry BlackBear = new PolymorphEntry(8399, 0xD3, 1015241, 22, 10);
        public static readonly PolymorphEntry GrizzlyBear = new PolymorphEntry(8411, 0xD4, 1015242, 22, 12);
        public static readonly PolymorphEntry PolarBear = new PolymorphEntry(8417, 0xD5, 1015243, 26, 10);
        public static readonly PolymorphEntry HumanMale = new PolymorphEntry(8397, 0x190, 1015244, 29, 8);
        public static readonly PolymorphEntry HumanFemale = new PolymorphEntry(8398, 0x191, 1015254, 29, 10);
        public static readonly PolymorphEntry Slime = new PolymorphEntry(8424, 0x33, 1015246, 5, 10);
        public static readonly PolymorphEntry Orc = new PolymorphEntry(8416, 0x11, 1015247, 29, 10);
        public static readonly PolymorphEntry LizardMan = new PolymorphEntry(8414, 0x21, 1015248, 26, 10);
        public static readonly PolymorphEntry Gargoyle = new PolymorphEntry(8409, 0x04, 1015249, 22, 10);
        public static readonly PolymorphEntry Ogre = new PolymorphEntry(8415, 0x01, 1015250, 24, 9);
        public static readonly PolymorphEntry Troll = new PolymorphEntry(8425, 0x36, 1015251, 25, 9);
        public static readonly PolymorphEntry Ettin = new PolymorphEntry(8408, 0x02, 1015252, 25, 8);
        public static readonly PolymorphEntry Daemon = new PolymorphEntry(8403, 0x09, 1015253, 25, 8);


        private int m_Art, m_Body, m_Num, m_X, m_Y;

        private PolymorphEntry(int Art, int Body, int LocNum, int X, int Y)
        {
            m_Art = Art;
            m_Body = Body;
            m_Num = LocNum;
            m_X = X;
            m_Y = Y;
        }

        public int ArtID { get { return m_Art; } }
        public int BodyID { get { return m_Body; } }
        public int LocNumber { get { return m_Num; } }
        public int X { get { return m_X; } }
        public int Y { get { return m_Y; } }
    }

    public class PolymorphGump : Gump
    {
        private class PolymorphCategory
        {
            private int m_Num;
            private PolymorphEntry[] m_Entries;

            public PolymorphCategory(int num, params PolymorphEntry[] entries)
            {
                m_Num = num;
                m_Entries = entries;
            }

            public PolymorphEntry[] Entries { get { return m_Entries; } }
            public int LocNumber { get { return m_Num; } }
        }

        private static PolymorphCategory[] Categories = new PolymorphCategory[]
			{
				new PolymorphCategory( 1015235, // Animals
					PolymorphEntry.Chicken,
					PolymorphEntry.Dog,
					PolymorphEntry.Wolf,
					PolymorphEntry.Panther,
					PolymorphEntry.Gorilla,
					PolymorphEntry.BlackBear,
					PolymorphEntry.GrizzlyBear,
					PolymorphEntry.PolarBear,
					PolymorphEntry.HumanMale ),

				new PolymorphCategory( 1015245, // Monsters
					PolymorphEntry.Slime,
					PolymorphEntry.Orc,
					PolymorphEntry.LizardMan,
					PolymorphEntry.Gargoyle,
					PolymorphEntry.Ogre,
					PolymorphEntry.Troll,
					PolymorphEntry.Ettin,
					PolymorphEntry.Daemon,
					PolymorphEntry.HumanFemale )
			};


        private Mobile m_Caster;
        private Item m_Scroll;

        public PolymorphGump(Mobile caster, Item scroll)
            : base(50, 50)
        {
            m_Caster = caster;
            m_Scroll = scroll;

            int x, y;
            AddPage(0);
            AddBackground(0, 0, 585, 393, 5054);
            AddBackground(195, 36, 387, 275, 3000);
            AddHtmlLocalized(0, 0, 510, 18, 1015234, false, false); // <center>Polymorph Selection Menu</center>
            AddHtmlLocalized(60, 355, 150, 18, 1011036, false, false); // OKAY
            AddButton(25, 355, 4005, 4007, 1, GumpButtonType.Reply, 1);
            AddHtmlLocalized(320, 355, 150, 18, 1011012, false, false); // CANCEL
            AddButton(285, 355, 4005, 4007, 0, GumpButtonType.Reply, 2);

            y = 35;
            for (int i = 0; i < Categories.Length; i++)
            {
                PolymorphCategory cat = (PolymorphCategory)Categories[i];
                AddHtmlLocalized(5, y, 150, 25, cat.LocNumber, true, false);
                AddButton(155, y, 4005, 4007, 0, GumpButtonType.Page, i + 1);
                y += 25;
            }

            for (int i = 0; i < Categories.Length; i++)
            {
                PolymorphCategory cat = (PolymorphCategory)Categories[i];
                AddPage(i + 1);

                for (int c = 0; c < cat.Entries.Length; c++)
                {
                    PolymorphEntry entry = (PolymorphEntry)cat.Entries[c];
                    x = 198 + (c % 3) * 129;
                    y = 38 + (c / 3) * 67;

                    AddHtmlLocalized(x, y, 100, 18, entry.LocNumber, false, false);
                    AddItem(x + 20, y + 25, entry.ArtID);
                    AddRadio(x, y + 20, 210, 211, false, (c << 8) + i);
                }
            }
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            if (info.ButtonID == 1 && info.Switches.Length > 0)
            {
                int cnum = info.Switches[0];
                int cat = cnum % 256;
                int ent = cnum >> 8;

                if (cat >= 0 && cat < Categories.Length)
                {
                    if (ent >= 0 && ent < Categories[cat].Entries.Length)
                    {
                        Spell spell = new PolymorphSpell(m_Caster, m_Scroll, Categories[cat].Entries[ent].BodyID);
                        spell.Cast();
                    }
                }
            }
        }
    }

    public class NewPolymorphGump : Gump
    {
        private static readonly PolymorphEntry[] m_Entries = new PolymorphEntry[]
			{
				PolymorphEntry.Chicken,
				PolymorphEntry.Dog,
				PolymorphEntry.Wolf,
				PolymorphEntry.Panther,
				PolymorphEntry.Gorilla,
				PolymorphEntry.BlackBear,
				PolymorphEntry.GrizzlyBear,
				PolymorphEntry.PolarBear,
				PolymorphEntry.HumanMale,
				PolymorphEntry.HumanFemale,
				PolymorphEntry.Slime,
				PolymorphEntry.Orc,
				PolymorphEntry.LizardMan,
				PolymorphEntry.Gargoyle,
				PolymorphEntry.Ogre,
				PolymorphEntry.Troll,
				PolymorphEntry.Ettin,
				PolymorphEntry.Daemon
			};

        private Mobile m_Caster;
        private Item m_Scroll;

        public NewPolymorphGump(Mobile caster, Item scroll)
            : base(0, 0)
        {
            m_Caster = caster;
            m_Scroll = scroll;

            AddPage(0);

            AddBackground(0, 0, 520, 404, 0x13BE);
            AddImageTiled(10, 10, 500, 20, 0xA40);
            AddImageTiled(10, 40, 500, 324, 0xA40);
            AddImageTiled(10, 374, 500, 20, 0xA40);
            AddAlphaRegion(10, 10, 500, 384);

            AddHtmlLocalized(14, 12, 500, 20, 1015234, 0x7FFF, false, false); // <center>Polymorph Selection Menu</center>

            AddButton(10, 374, 0xFB1, 0xFB2, 0, GumpButtonType.Reply, 0);
            AddHtmlLocalized(45, 376, 450, 20, 1060051, 0x7FFF, false, false); // CANCEL

            for (int i = 0; i < m_Entries.Length; i++)
            {
                PolymorphEntry entry = m_Entries[i];

                int page = i / 10 + 1;
                int pos = i % 10;

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

                int x = (pos % 2 == 0) ? 14 : 264;
                int y = (pos / 2) * 64 + 44;

                AddImageTiledButton(x, y, 0x918, 0x919, i + 1, GumpButtonType.Reply, 0, entry.ArtID, 0x0, entry.X, entry.Y);
                AddHtmlLocalized(x + 84, y, 250, 60, entry.LocNumber, 0x7FFF, false, false);
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            int idx = info.ButtonID - 1;

            if (idx < 0 || idx >= m_Entries.Length)
                return;

            Spell spell = new PolymorphSpell(m_Caster, m_Scroll, m_Entries[idx].BodyID);
            spell.Cast();
        }
    }
}

namespace Server.Spells
{
    public enum SpellCircle
    {
        First,
        Second,
        Third,
        Fourth,
        Fifth,
        Sixth,
        Seventh,
        Eighth
    }

    public abstract class MagerySpell : Spell
    {
        public MagerySpell(Mobile caster, Item scroll, SpellInfo info)
            : base(caster, scroll, info)
        {
        }

        public abstract SpellCircle Circle { get; }

        public override bool ConsumeReagents()
        {
            if (base.ConsumeReagents())
                return true;

            if (ArcaneGem.ConsumeCharges(Caster, (Core.SE ? 1 : 1 + (int)Circle)))
                return true;

            return false;
        }

        private const double ChanceOffset = 20.0, ChanceLength = 100.0 / 7.0;

        public override void GetCastSkills(out double min, out double max)
        {
            int circle = (int)Circle;

            if (Scroll != null)
                circle -= 2;

            double avg = ChanceLength * circle;

            min = avg - ChanceOffset;
            max = avg + ChanceOffset;
        }

        private static int[] m_ManaTable = new int[] { 4, 6, 9, 11, 14, 20, 40, 50 };

        public override int GetMana()
        {
            if (Scroll is BaseWand)
                return 0;

            return m_ManaTable[(int)Circle];
        }

        public override double GetResistSkill(Mobile m)
        {
            int maxSkill = (1 + (int)Circle) * 10;
            maxSkill += (1 + ((int)Circle / 6)) * 25;

            if (m.Skills[SkillName.MagicResist].Value < maxSkill)
                m.CheckSkill(SkillName.MagicResist, 0.0, m.Skills[SkillName.MagicResist].Cap);

            return m.Skills[SkillName.MagicResist].Value;
        }

        public virtual bool CheckResisted(Mobile target)
        {
            double n = GetResistPercent(target);

            n /= 100.0;

            if (n <= 0.0)
                return false;

            if (n >= 1.0)
                return true;

            int maxSkill = (1 + (int)Circle) * 10;
            maxSkill += (1 + ((int)Circle / 6)) * 25;

            if (target.Skills[SkillName.MagicResist].Value < maxSkill)
                target.CheckSkill(SkillName.MagicResist, 0.0, target.Skills[SkillName.MagicResist].Cap);

            return (n >= Utility.RandomDouble());
        }

        public virtual double GetResistPercentForCircle(Mobile target, SpellCircle circle)
        {
            double firstPercent = target.Skills[SkillName.MagicResist].Value / 5.0;
            double secondPercent = target.Skills[SkillName.MagicResist].Value - (((Caster.Skills[CastSkill].Value - 20.0) / 5.0) + (1 + (int)circle) * 5.0);

            return (firstPercent > secondPercent ? firstPercent : secondPercent) / 2.0; // Seems should be about half of what stratics says.
        }

        public virtual double GetResistPercent(Mobile target)
        {
            return GetResistPercentForCircle(target, Circle);
        }

        public override TimeSpan GetCastDelay()
        {
            if (!Core.ML && Scroll is BaseWand)
                return TimeSpan.Zero;

            if (!Core.AOS)
                return TimeSpan.FromSeconds(0.5 + (0.25 * (int)Circle));

            return base.GetCastDelay();
        }

        public override TimeSpan CastDelayBase
        {
            get
            {
                return TimeSpan.FromSeconds((3 + (int)Circle) * CastDelaySecondsPerTick);
            }
        }
    }
}

namespace Server.Spells.First
{
    public class ClumsySpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Clumsy", "Uus Jux",
                212,
                9031,
                Reagent.Bloodmoss,
                Reagent.Nightshade
            );

        public override SpellCircle Circle { get { return SpellCircle.First; } }

        public ClumsySpell(Mobile caster, Item scroll)
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
                SpellHelper.Turn(Caster, m);

                SpellHelper.CheckReflect((int)this.Circle, Caster, ref m);

                SpellHelper.AddStatCurse(Caster, m, StatType.Dex);

                if (m.Spell != null)
                    m.Spell.OnCasterHurt();

                m.Paralyzed = false;

                m.FixedParticles(0x3779, 10, 15, 5002, EffectLayer.Head);
                m.PlaySound(0x1DF);

                int percentage = (int)(SpellHelper.GetOffsetScalar(Caster, m, true) * 100);
                TimeSpan length = SpellHelper.GetDuration(Caster, m);

                BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.Clumsy, 1075831, length, m, percentage.ToString()));

                HarmfulSpell(m);
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private ClumsySpell m_Owner;

            public InternalTarget(ClumsySpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Harmful)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                {
                    m_Owner.Target((Mobile)o);
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class CreateFoodSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Create Food", "In Mani Ylem",
                224,
                9011,
                Reagent.Garlic,
                Reagent.Ginseng,
                Reagent.MandrakeRoot
            );

        public override SpellCircle Circle { get { return SpellCircle.First; } }

        public CreateFoodSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        private static FoodInfo[] m_Food = new FoodInfo[]
			{
				new FoodInfo( typeof( Grapes ), "a grape bunch" ),
				new FoodInfo( typeof( Ham ), "a ham" ),
				new FoodInfo( typeof( CheeseWedge ), "a wedge of cheese" ),
				new FoodInfo( typeof( Muffins ), "muffins" ),
				new FoodInfo( typeof( FishSteak ), "a fish steak" ),
				new FoodInfo( typeof( Ribs ), "cut of ribs" ),
				new FoodInfo( typeof( CookedBird ), "a cooked bird" ),
				new FoodInfo( typeof( Sausage ), "sausage" ),
				new FoodInfo( typeof( Apple ), "an apple" ),
				new FoodInfo( typeof( Peach ), "a peach" )
			};

        public override void OnCast()
        {
            if (CheckSequence())
            {
                FoodInfo foodInfo = m_Food[Utility.Random(m_Food.Length)];
                Item food = foodInfo.Create();

                if (food != null)
                {
                    Caster.AddToBackpack(food);

                    // You magically create food in your backpack:
                    Caster.SendLocalizedMessage(1042695, true, " " + foodInfo.Name);

                    Caster.FixedParticles(0, 10, 5, 2003, EffectLayer.RightHand);
                    Caster.PlaySound(0x1E2);
                }
            }

            FinishSequence();
        }
    }

    public class FoodInfo
    {
        private Type m_Type;
        private string m_Name;

        public Type Type { get { return m_Type; } set { m_Type = value; } }
        public string Name { get { return m_Name; } set { m_Name = value; } }

        public FoodInfo(Type type, string name)
        {
            m_Type = type;
            m_Name = name;
        }

        public Item Create()
        {
            Item item;

            try
            {
                item = (Item)Activator.CreateInstance(m_Type);
            }
            catch
            {
                item = null;
            }

            return item;
        }
    }

    public class FeeblemindSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Feeblemind", "Rel Wis",
                212,
                9031,
                Reagent.Ginseng,
                Reagent.Nightshade
            );

        public override SpellCircle Circle { get { return SpellCircle.First; } }

        public FeeblemindSpell(Mobile caster, Item scroll)
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
                SpellHelper.Turn(Caster, m);

                SpellHelper.CheckReflect((int)this.Circle, Caster, ref m);

                SpellHelper.AddStatCurse(Caster, m, StatType.Int);

                if (m.Spell != null)
                    m.Spell.OnCasterHurt();

                m.Paralyzed = false;

                m.FixedParticles(0x3779, 10, 15, 5004, EffectLayer.Head);
                m.PlaySound(0x1E4);

                int percentage = (int)(SpellHelper.GetOffsetScalar(Caster, m, true) * 100);
                TimeSpan length = SpellHelper.GetDuration(Caster, m);

                BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.FeebleMind, 1075833, length, m, percentage.ToString()));

                HarmfulSpell(m);
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private FeeblemindSpell m_Owner;

            public InternalTarget(FeeblemindSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Harmful)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                {
                    m_Owner.Target((Mobile)o);
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class HealSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Heal", "In Mani",
                224,
                9061,
                Reagent.Garlic,
                Reagent.Ginseng,
                Reagent.SpidersSilk
            );

        public override SpellCircle Circle { get { return SpellCircle.First; } }

        public HealSpell(Mobile caster, Item scroll)
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
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (m.IsDeadBondedPet)
            {
                Caster.SendLocalizedMessage(1060177); // You cannot heal a creature that is already dead!
            }
            else if (m is BaseCreature && ((BaseCreature)m).IsAnimatedDead)
            {
                Caster.SendLocalizedMessage(1061654); // You cannot heal that which is not alive.
            }
            else if (m is Golem)
            {
                Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, 500951); // You cannot heal that.
            }
            else if (m.Poisoned || Server.Items.MortalStrike.IsWounded(m))
            {
                Caster.LocalOverheadMessage(MessageType.Regular, 0x22, (Caster == m) ? 1005000 : 1010398);
            }
            else if (CheckBSequence(m))
            {
                SpellHelper.Turn(Caster, m);

                int toHeal;

                if (Core.AOS)
                {
                    toHeal = Caster.Skills.Magery.Fixed / 120;
                    toHeal += Utility.RandomMinMax(1, 4);

                    if (Core.SE && Caster != m)
                        toHeal = (int)(toHeal * 1.5);
                }
                else
                {
                    toHeal = (int)(Caster.Skills[SkillName.Magery].Value * 0.1);
                    toHeal += Utility.Random(1, 5);
                }

                //m.Heal( toHeal, Caster );
                SpellHelper.Heal(toHeal, m, Caster);

                m.FixedParticles(0x376A, 9, 32, 5005, EffectLayer.Waist);
                m.PlaySound(0x1F2);
            }

            FinishSequence();
        }

        public class InternalTarget : Target
        {
            private HealSpell m_Owner;

            public InternalTarget(HealSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Beneficial)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                {
                    m_Owner.Target((Mobile)o);
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class MagicArrowSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Magic Arrow", "In Por Ylem",
                212,
                9041,
                Reagent.SulfurousAsh
            );

        public override SpellCircle Circle { get { return SpellCircle.First; } }

        public MagicArrowSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override bool DelayedDamageStacking { get { return !Core.AOS; } }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public override bool DelayedDamage { get { return true; } }

        public void Target(Mobile m)
        {
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckHSequence(m))
            {
                Mobile source = Caster;

                SpellHelper.Turn(source, m);

                SpellHelper.CheckReflect((int)this.Circle, ref source, ref m);

                double damage;

                if (Core.AOS)
                {
                    damage = GetNewAosDamage(10, 1, 4, m);
                }
                else
                {
                    damage = Utility.Random(4, 4);

                    if (CheckResisted(m))
                    {
                        damage *= 0.75;

                        m.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
                    }

                    damage *= GetDamageScalar(m);
                }

                source.MovingParticles(m, 0x36E4, 5, 0, false, false, 3006, 0, 0);
                source.PlaySound(0x1E5);

                SpellHelper.Damage(this, m, damage, 0, 100, 0, 0, 0);
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private MagicArrowSpell m_Owner;

            public InternalTarget(MagicArrowSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Harmful)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                {
                    m_Owner.Target((Mobile)o);
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class NightSightSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Night Sight", "In Lor",
                236,
                9031,
                Reagent.SulfurousAsh,
                Reagent.SpidersSilk
            );

        public override SpellCircle Circle { get { return SpellCircle.First; } }

        public NightSightSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new NightSightTarget(this);
        }

        private class NightSightTarget : Target
        {
            private Spell m_Spell;

            public NightSightTarget(Spell spell)
                : base(12, false, TargetFlags.Beneficial)
            {
                m_Spell = spell;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is Mobile && m_Spell.CheckBSequence((Mobile)targeted))
                {
                    Mobile targ = (Mobile)targeted;

                    SpellHelper.Turn(m_Spell.Caster, targ);

                    if (targ.BeginAction(typeof(LightCycle)))
                    {
                        new LightCycle.NightSightTimer(targ).Start();
                        int level = (int)(LightCycle.DungeonLevel * ((Core.AOS ? targ.Skills[SkillName.Magery].Value : from.Skills[SkillName.Magery].Value) / 100));

                        if (level < 0)
                            level = 0;

                        targ.LightLevel = level;

                        targ.FixedParticles(0x376A, 9, 32, 5007, EffectLayer.Waist);
                        targ.PlaySound(0x1E3);

                        BuffInfo.AddBuff(targ, new BuffInfo(BuffIcon.NightSight, 1075643));	//Night Sight/You ignore lighting effects
                    }
                    else
                    {
                        from.SendMessage("{0} already have nightsight.", from == targ ? "You" : "They");
                    }
                }

                m_Spell.FinishSequence();
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Spell.FinishSequence();
            }
        }
    }

    public class ReactiveArmorSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Reactive Armor", "Flam Sanct",
                236,
                9011,
                Reagent.Garlic,
                Reagent.SpidersSilk,
                Reagent.SulfurousAsh
            );

        public override SpellCircle Circle { get { return SpellCircle.First; } }

        public ReactiveArmorSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override bool CheckCast()
        {
            if (Core.AOS)
                return true;

            if (Caster.MeleeDamageAbsorb > 0)
            {
                Caster.SendLocalizedMessage(1005559); // This spell is already in effect.
                return false;
            }
            else if (!Caster.CanBeginAction(typeof(DefensiveSpell)))
            {
                Caster.SendLocalizedMessage(1005385); // The spell will not adhere to you at this time.
                return false;
            }

            return true;
        }

        private static Hashtable m_Table = new Hashtable();

        public override void OnCast()
        {
            if (Core.AOS)
            {
                /* The reactive armor spell increases the caster's physical resistance, while lowering the caster's elemental resistances.
                 * 15 + (Inscription/20) Physcial bonus
                 * -5 Elemental
                 * The reactive armor spell has an indefinite duration, becoming active when cast, and deactivated when re-cast. 
                 * Reactive Armor, Protection, and Magic Reflection will stay on—even after logging out, even after dying—until you “turn them off” by casting them again. 
                 * (+20 physical -5 elemental at 100 Inscription)
                 */

                if (CheckSequence())
                {
                    Mobile targ = Caster;

                    ResistanceMod[] mods = (ResistanceMod[])m_Table[targ];

                    if (mods == null)
                    {
                        targ.PlaySound(0x1E9);
                        targ.FixedParticles(0x376A, 9, 32, 5008, EffectLayer.Waist);

                        mods = new ResistanceMod[5]
							{
								new ResistanceMod( ResistanceType.Physical, 15 + (int)(targ.Skills[SkillName.Inscribe].Value / 20) ),
								new ResistanceMod( ResistanceType.Fire, -5 ),
								new ResistanceMod( ResistanceType.Cold, -5 ),
								new ResistanceMod( ResistanceType.Poison, -5 ),
								new ResistanceMod( ResistanceType.Energy, -5 )
							};

                        m_Table[targ] = mods;

                        for (int i = 0; i < mods.Length; ++i)
                            targ.AddResistanceMod(mods[i]);

                        int physresist = 15 + (int)(targ.Skills[SkillName.Inscribe].Value / 20);
                        string args = String.Format("{0}\t{1}\t{2}\t{3}\t{4}", physresist, 5, 5, 5, 5);

                        BuffInfo.AddBuff(Caster, new BuffInfo(BuffIcon.ReactiveArmor, 1075812, 1075813, args.ToString()));
                    }
                    else
                    {
                        targ.PlaySound(0x1ED);
                        targ.FixedParticles(0x376A, 9, 32, 5008, EffectLayer.Waist);

                        m_Table.Remove(targ);

                        for (int i = 0; i < mods.Length; ++i)
                            targ.RemoveResistanceMod(mods[i]);

                        BuffInfo.RemoveBuff(Caster, BuffIcon.ReactiveArmor);
                    }
                }

                FinishSequence();
            }
            else
            {
                if (Caster.MeleeDamageAbsorb > 0)
                {
                    Caster.SendLocalizedMessage(1005559); // This spell is already in effect.
                }
                else if (!Caster.CanBeginAction(typeof(DefensiveSpell)))
                {
                    Caster.SendLocalizedMessage(1005385); // The spell will not adhere to you at this time.
                }
                else if (CheckSequence())
                {
                    if (Caster.BeginAction(typeof(DefensiveSpell)))
                    {
                        int value = (int)(Caster.Skills[SkillName.Magery].Value + Caster.Skills[SkillName.Meditation].Value + Caster.Skills[SkillName.Inscribe].Value);
                        value /= 3;

                        if (value < 0)
                            value = 1;
                        else if (value > 75)
                            value = 75;

                        Caster.MeleeDamageAbsorb = value;

                        Caster.FixedParticles(0x376A, 9, 32, 5008, EffectLayer.Waist);
                        Caster.PlaySound(0x1F2);
                    }
                    else
                    {
                        Caster.SendLocalizedMessage(1005385); // The spell will not adhere to you at this time.
                    }
                }

                FinishSequence();
            }
        }

        public static void EndArmor(Mobile m)
        {
            if (m_Table.Contains(m))
            {
                ResistanceMod[] mods = (ResistanceMod[])m_Table[m];

                if (mods != null)
                {
                    for (int i = 0; i < mods.Length; ++i)
                        m.RemoveResistanceMod(mods[i]);
                }

                m_Table.Remove(m);
                BuffInfo.RemoveBuff(m, BuffIcon.ReactiveArmor);
            }
        }
    }

    public class WeakenSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Weaken", "Des Mani",
                212,
                9031,
                Reagent.Garlic,
                Reagent.Nightshade
            );

        public override SpellCircle Circle { get { return SpellCircle.First; } }

        public WeakenSpell(Mobile caster, Item scroll)
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
                SpellHelper.Turn(Caster, m);

                SpellHelper.CheckReflect((int)this.Circle, Caster, ref m);

                SpellHelper.AddStatCurse(Caster, m, StatType.Str);

                if (m.Spell != null)
                    m.Spell.OnCasterHurt();

                m.Paralyzed = false;

                m.FixedParticles(0x3779, 10, 15, 5009, EffectLayer.Waist);
                m.PlaySound(0x1E6);

                int percentage = (int)(SpellHelper.GetOffsetScalar(Caster, m, true) * 100);
                TimeSpan length = SpellHelper.GetDuration(Caster, m);

                BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.Weaken, 1075837, length, m, percentage.ToString()));

                HarmfulSpell(m);
            }

            FinishSequence();
        }

        public class InternalTarget : Target
        {
            private WeakenSpell m_Owner;

            public InternalTarget(WeakenSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Harmful)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                {
                    m_Owner.Target((Mobile)o);
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }
}

namespace Server.Spells.Second
{
    public class AgilitySpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Agility", "Ex Uus",
                212,
                9061,
                Reagent.Bloodmoss,
                Reagent.MandrakeRoot
            );

        public override SpellCircle Circle { get { return SpellCircle.Second; } }

        public AgilitySpell(Mobile caster, Item scroll)
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
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckBSequence(m))
            {
                SpellHelper.Turn(Caster, m);

                SpellHelper.AddStatBonus(Caster, m, StatType.Dex);

                m.FixedParticles(0x375A, 10, 15, 5010, EffectLayer.Waist);
                m.PlaySound(0x1e7);

                int percentage = (int)(SpellHelper.GetOffsetScalar(Caster, m, false) * 100);
                TimeSpan length = SpellHelper.GetDuration(Caster, m);

                BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.Agility, 1075841, length, m, percentage.ToString()));
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private AgilitySpell m_Owner;

            public InternalTarget(AgilitySpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Beneficial)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                {
                    m_Owner.Target((Mobile)o);
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class CunningSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Cunning", "Uus Wis",
                212,
                9061,
                Reagent.MandrakeRoot,
                Reagent.Nightshade
            );

        public override SpellCircle Circle { get { return SpellCircle.Second; } }

        public CunningSpell(Mobile caster, Item scroll)
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
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckBSequence(m))
            {
                SpellHelper.Turn(Caster, m);

                SpellHelper.AddStatBonus(Caster, m, StatType.Int);

                m.FixedParticles(0x375A, 10, 15, 5011, EffectLayer.Head);
                m.PlaySound(0x1EB);

                int percentage = (int)(SpellHelper.GetOffsetScalar(Caster, m, false) * 100);
                TimeSpan length = SpellHelper.GetDuration(Caster, m);

                BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.Cunning, 1075843, length, m, percentage.ToString()));
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private CunningSpell m_Owner;

            public InternalTarget(CunningSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Beneficial)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                {
                    m_Owner.Target((Mobile)o);
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class CureSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Cure", "An Nox",
                212,
                9061,
                Reagent.Garlic,
                Reagent.Ginseng
            );

        public override SpellCircle Circle { get { return SpellCircle.Second; } }

        public CureSpell(Mobile caster, Item scroll)
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
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckBSequence(m))
            {
                SpellHelper.Turn(Caster, m);

                Poison p = m.Poison;

                if (p != null)
                {
                    int chanceToCure = 10000 + (int)(Caster.Skills[SkillName.Magery].Value * 75) - ((p.Level + 1) * (Core.AOS ? (p.Level < 4 ? 3300 : 3100) : 1750));
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

                m.FixedParticles(0x373A, 10, 15, 5012, EffectLayer.Waist);
                m.PlaySound(0x1E0);
            }

            FinishSequence();
        }

        public class InternalTarget : Target
        {
            private CureSpell m_Owner;

            public InternalTarget(CureSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Beneficial)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                {
                    m_Owner.Target((Mobile)o);
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class HarmSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Harm", "An Mani",
                212,
                Core.AOS ? 9001 : 9041,
                Reagent.Nightshade,
                Reagent.SpidersSilk
            );

        public override SpellCircle Circle { get { return SpellCircle.Second; } }

        public HarmSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public override bool DelayedDamage { get { return false; } }


        public override double GetSlayerDamageScalar(Mobile target)
        {
            return 1.0; //This spell isn't affected by slayer spellbooks
        }


        public void Target(Mobile m)
        {
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckHSequence(m))
            {
                SpellHelper.Turn(Caster, m);

                SpellHelper.CheckReflect((int)this.Circle, Caster, ref m);

                double damage;

                if (Core.AOS)
                {
                    damage = GetNewAosDamage(17, 1, 5, m);
                }
                else
                {
                    damage = Utility.Random(1, 15);

                    if (CheckResisted(m))
                    {
                        damage *= 0.75;

                        m.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
                    }

                    damage *= GetDamageScalar(m);
                }

                if (!m.InRange(Caster, 2))
                    damage *= 0.25; // 1/4 damage at > 2 tile range
                else if (!m.InRange(Caster, 1))
                    damage *= 0.50; // 1/2 damage at 2 tile range

                if (Core.AOS)
                {
                    m.FixedParticles(0x374A, 10, 30, 5013, 1153, 2, EffectLayer.Waist);
                    m.PlaySound(0x0FC);
                }
                else
                {
                    m.FixedParticles(0x374A, 10, 15, 5013, EffectLayer.Waist);
                    m.PlaySound(0x1F1);
                }

                SpellHelper.Damage(this, m, damage, 0, 0, 100, 0, 0);
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private HarmSpell m_Owner;

            public InternalTarget(HarmSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Harmful)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                {
                    m_Owner.Target((Mobile)o);
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class MagicTrapSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Magic Trap", "In Jux",
                212,
                9001,
                Reagent.Garlic,
                Reagent.SpidersSilk,
                Reagent.SulfurousAsh
            );

        public override SpellCircle Circle { get { return SpellCircle.Second; } }

        public MagicTrapSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(TrapableContainer item)
        {
            if (!Caster.CanSee(item))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (item.TrapType != TrapType.None && item.TrapType != TrapType.MagicTrap)
            {
                base.DoFizzle();
            }
            else if (CheckSequence())
            {
                SpellHelper.Turn(Caster, item);

                item.TrapType = TrapType.MagicTrap;
                item.TrapPower = Core.AOS ? Utility.RandomMinMax(10, 50) : 1;
                item.TrapLevel = 0;

                Point3D loc = item.GetWorldLocation();

                Effects.SendLocationParticles(EffectItem.Create(new Point3D(loc.X + 1, loc.Y, loc.Z), item.Map, EffectItem.DefaultDuration), 0x376A, 9, 10, 9502);
                Effects.SendLocationParticles(EffectItem.Create(new Point3D(loc.X, loc.Y - 1, loc.Z), item.Map, EffectItem.DefaultDuration), 0x376A, 9, 10, 9502);
                Effects.SendLocationParticles(EffectItem.Create(new Point3D(loc.X - 1, loc.Y, loc.Z), item.Map, EffectItem.DefaultDuration), 0x376A, 9, 10, 9502);
                Effects.SendLocationParticles(EffectItem.Create(new Point3D(loc.X, loc.Y + 1, loc.Z), item.Map, EffectItem.DefaultDuration), 0x376A, 9, 10, 9502);
                Effects.SendLocationParticles(EffectItem.Create(new Point3D(loc.X, loc.Y, loc.Z), item.Map, EffectItem.DefaultDuration), 0, 0, 0, 5014);

                Effects.PlaySound(loc, item.Map, 0x1EF);
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private MagicTrapSpell m_Owner;

            public InternalTarget(MagicTrapSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.None)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is TrapableContainer)
                {
                    m_Owner.Target((TrapableContainer)o);
                }
                else
                {
                    from.SendMessage("You can't trap that");
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class ProtectionSpell : MagerySpell
    {
        private static Hashtable m_Registry = new Hashtable();
        public static Hashtable Registry { get { return m_Registry; } }

        private static SpellInfo m_Info = new SpellInfo(
                "Protection", "Uus Sanct",
                236,
                9011,
                Reagent.Garlic,
                Reagent.Ginseng,
                Reagent.SulfurousAsh
            );

        public override SpellCircle Circle { get { return SpellCircle.Second; } }

        public ProtectionSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override bool CheckCast()
        {
            if (Core.AOS)
                return true;

            if (m_Registry.ContainsKey(Caster))
            {
                Caster.SendLocalizedMessage(1005559); // This spell is already in effect.
                return false;
            }
            else if (!Caster.CanBeginAction(typeof(DefensiveSpell)))
            {
                Caster.SendLocalizedMessage(1005385); // The spell will not adhere to you at this time.
                return false;
            }

            return true;
        }

        private static Hashtable m_Table = new Hashtable();

        public static void Toggle(Mobile caster, Mobile target)
        {
            /* Players under the protection spell effect can no longer have their spells "disrupted" when hit.
             * Players under the protection spell have decreased physical resistance stat value (-15 + (Inscription/20),
             * a decreased "resisting spells" skill value by -35 + (Inscription/20),
             * and a slower casting speed modifier (technically, a negative "faster cast speed") of 2 points.
             * The protection spell has an indefinite duration, becoming active when cast, and deactivated when re-cast.
             * Reactive Armor, Protection, and Magic Reflection will stay on—even after logging out,
             * even after dying—until you “turn them off” by casting them again.
             */

            object[] mods = (object[])m_Table[target];

            if (mods == null)
            {
                target.PlaySound(0x1E9);
                target.FixedParticles(0x375A, 9, 20, 5016, EffectLayer.Waist);

                mods = new object[2]
					{
						new ResistanceMod( ResistanceType.Physical, -15 + Math.Min( (int)(caster.Skills[SkillName.Inscribe].Value / 20), 15 ) ),
						new DefaultSkillMod( SkillName.MagicResist, true, -35 + Math.Min( (int)(caster.Skills[SkillName.Inscribe].Value / 20), 35 ) )
					};

                m_Table[target] = mods;
                Registry[target] = 100.0;

                target.AddResistanceMod((ResistanceMod)mods[0]);
                target.AddSkillMod((SkillMod)mods[1]);

                int physloss = -15 + (int)(caster.Skills[SkillName.Inscribe].Value / 20);
                int resistloss = -35 + (int)(caster.Skills[SkillName.Inscribe].Value / 20);
                string args = String.Format("{0}\t{1}", physloss, resistloss);
                BuffInfo.AddBuff(target, new BuffInfo(BuffIcon.Protection, 1075814, 1075815, args.ToString()));
            }
            else
            {
                target.PlaySound(0x1ED);
                target.FixedParticles(0x375A, 9, 20, 5016, EffectLayer.Waist);

                m_Table.Remove(target);
                Registry.Remove(target);

                target.RemoveResistanceMod((ResistanceMod)mods[0]);
                target.RemoveSkillMod((SkillMod)mods[1]);

                BuffInfo.RemoveBuff(target, BuffIcon.Protection);
            }
        }

        public static void EndProtection(Mobile m)
        {
            if (m_Table.Contains(m))
            {
                object[] mods = (object[])m_Table[m];

                m_Table.Remove(m);
                Registry.Remove(m);

                m.RemoveResistanceMod((ResistanceMod)mods[0]);
                m.RemoveSkillMod((SkillMod)mods[1]);

                BuffInfo.RemoveBuff(m, BuffIcon.Protection);
            }
        }

        public override void OnCast()
        {
            if (Core.AOS)
            {
                if (CheckSequence())
                    Toggle(Caster, Caster);

                FinishSequence();
            }
            else
            {
                if (m_Registry.ContainsKey(Caster))
                {
                    Caster.SendLocalizedMessage(1005559); // This spell is already in effect.
                }
                else if (!Caster.CanBeginAction(typeof(DefensiveSpell)))
                {
                    Caster.SendLocalizedMessage(1005385); // The spell will not adhere to you at this time.
                }
                else if (CheckSequence())
                {
                    if (Caster.BeginAction(typeof(DefensiveSpell)))
                    {
                        double value = (int)(Caster.Skills[SkillName.EvalInt].Value + Caster.Skills[SkillName.Meditation].Value + Caster.Skills[SkillName.Inscribe].Value);
                        value /= 4;

                        if (value < 0)
                            value = 0;
                        else if (value > 75)
                            value = 75.0;

                        Registry.Add(Caster, value);
                        new InternalTimer(Caster).Start();

                        Caster.FixedParticles(0x375A, 9, 20, 5016, EffectLayer.Waist);
                        Caster.PlaySound(0x1ED);
                    }
                    else
                    {
                        Caster.SendLocalizedMessage(1005385); // The spell will not adhere to you at this time.
                    }
                }

                FinishSequence();
            }
        }

        private class InternalTimer : Timer
        {
            private Mobile m_Caster;

            public InternalTimer(Mobile caster)
                : base(TimeSpan.FromSeconds(0))
            {
                double val = caster.Skills[SkillName.Magery].Value * 2.0;
                if (val < 15)
                    val = 15;
                else if (val > 240)
                    val = 240;

                m_Caster = caster;
                Delay = TimeSpan.FromSeconds(val);
                Priority = TimerPriority.OneSecond;
            }

            protected override void OnTick()
            {
                ProtectionSpell.Registry.Remove(m_Caster);
                DefensiveSpell.Nullify(m_Caster);
            }
        }
    }

    public class RemoveTrapSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Remove Trap", "An Jux",
                212,
                9001,
                Reagent.Bloodmoss,
                Reagent.SulfurousAsh
            );

        public override SpellCircle Circle { get { return SpellCircle.Second; } }

        public RemoveTrapSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
            Caster.SendMessage("What do you wish to untrap?");
        }

        public void Target(TrapableContainer item)
        {
            if (!Caster.CanSee(item))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (item.TrapType != TrapType.None && item.TrapType != TrapType.MagicTrap)
            {
                base.DoFizzle();
            }
            else if (CheckSequence())
            {
                SpellHelper.Turn(Caster, item);

                Point3D loc = item.GetWorldLocation();

                Effects.SendLocationParticles(EffectItem.Create(loc, item.Map, EffectItem.DefaultDuration), 0x376A, 9, 32, 5015);
                Effects.PlaySound(loc, item.Map, 0x1F0);

                item.TrapType = TrapType.None;
                item.TrapPower = 0;
                item.TrapLevel = 0;
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private RemoveTrapSpell m_Owner;

            public InternalTarget(RemoveTrapSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.None)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is TrapableContainer)
                {
                    m_Owner.Target((TrapableContainer)o);
                }
                else
                {
                    from.SendMessage("You can't disarm that");
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class StrengthSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Strength", "Uus Mani",
                212,
                9061,
                Reagent.MandrakeRoot,
                Reagent.Nightshade
            );

        public override SpellCircle Circle { get { return SpellCircle.Second; } }

        public StrengthSpell(Mobile caster, Item scroll)
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
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckBSequence(m))
            {
                SpellHelper.Turn(Caster, m);

                SpellHelper.AddStatBonus(Caster, m, StatType.Str);

                m.FixedParticles(0x375A, 10, 15, 5017, EffectLayer.Waist);
                m.PlaySound(0x1EE);

                int percentage = (int)(SpellHelper.GetOffsetScalar(Caster, m, false) * 100);
                TimeSpan length = SpellHelper.GetDuration(Caster, m);

                BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.Strength, 1075845, length, m, percentage.ToString()));
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private StrengthSpell m_Owner;

            public InternalTarget(StrengthSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Beneficial)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                {
                    m_Owner.Target((Mobile)o);
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }
}

namespace Server.Spells.Third
{
    public class BlessSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Bless", "Rel Sanct",
                203,
                9061,
                Reagent.Garlic,
                Reagent.MandrakeRoot
            );

        public override SpellCircle Circle { get { return SpellCircle.Third; } }

        public BlessSpell(Mobile caster, Item scroll)
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
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckBSequence(m))
            {
                SpellHelper.Turn(Caster, m);

                SpellHelper.AddStatBonus(Caster, m, StatType.Str); SpellHelper.DisableSkillCheck = true;
                SpellHelper.AddStatBonus(Caster, m, StatType.Dex);
                SpellHelper.AddStatBonus(Caster, m, StatType.Int); SpellHelper.DisableSkillCheck = false;

                m.FixedParticles(0x373A, 10, 15, 5018, EffectLayer.Waist);
                m.PlaySound(0x1EA);

                int percentage = (int)(SpellHelper.GetOffsetScalar(Caster, m, false) * 100);
                TimeSpan length = SpellHelper.GetDuration(Caster, m);

                string args = String.Format("{0}\t{1}\t{2}", percentage, percentage, percentage);

                BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.Bless, 1075847, 1075848, length, m, args.ToString()));
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private BlessSpell m_Owner;

            public InternalTarget(BlessSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Beneficial)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                {
                    m_Owner.Target((Mobile)o);
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class FireballSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Fireball", "Vas Flam",
                203,
                9041,
                Reagent.BlackPearl
            );

        public override SpellCircle Circle { get { return SpellCircle.Third; } }

        public FireballSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public override bool DelayedDamage { get { return true; } }

        public void Target(Mobile m)
        {
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckHSequence(m))
            {
                Mobile source = Caster;

                SpellHelper.Turn(source, m);

                SpellHelper.CheckReflect((int)this.Circle, ref source, ref m);

                double damage;

                if (Core.AOS)
                {
                    damage = GetNewAosDamage(19, 1, 5, m);
                }
                else
                {
                    damage = Utility.Random(10, 7);

                    if (CheckResisted(m))
                    {
                        damage *= 0.75;

                        m.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
                    }

                    damage *= GetDamageScalar(m);
                }

                source.MovingParticles(m, 0x36D4, 7, 0, false, true, 9502, 4019, 0x160);
                source.PlaySound(Core.AOS ? 0x15E : 0x44B);

                SpellHelper.Damage(this, m, damage, 0, 100, 0, 0, 0);
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private FireballSpell m_Owner;

            public InternalTarget(FireballSpell owner)
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

    public class MagicLockSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Magic Lock", "An Por",
                215,
                9001,
                Reagent.Garlic,
                Reagent.Bloodmoss,
                Reagent.SulfurousAsh
            );

        public override SpellCircle Circle { get { return SpellCircle.Third; } }

        public MagicLockSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(LockableContainer targ)
        {
            if (Multis.BaseHouse.CheckLockedDownOrSecured(targ))
            {
                // You cannot cast this on a locked down item.
                Caster.LocalOverheadMessage(MessageType.Regular, 0x22, 501761);
            }
            else if (targ.Locked || targ.LockLevel == 0 || targ is ParagonChest)
            {
                // Target must be an unlocked chest.
                Caster.SendLocalizedMessage(501762);
            }
            else if (CheckSequence())
            {
                SpellHelper.Turn(Caster, targ);

                Point3D loc = targ.GetWorldLocation();

                Effects.SendLocationParticles(
                    EffectItem.Create(loc, targ.Map, EffectItem.DefaultDuration),
                    0x376A, 9, 32, 5020);

                Effects.PlaySound(loc, targ.Map, 0x1FA);

                // The chest is now locked!
                Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, 501763);

                targ.LockLevel = -255; // signal magic lock
                targ.Locked = true;
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private MagicLockSpell m_Owner;

            public InternalTarget(MagicLockSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.None)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is LockableContainer)
                    m_Owner.Target((LockableContainer)o);
                else
                    from.SendLocalizedMessage(501762); // Target must be an unlocked chest.
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class PoisonSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Poison", "In Nox",
                203,
                9051,
                Reagent.Nightshade
            );

        public override SpellCircle Circle { get { return SpellCircle.Third; } }

        public PoisonSpell(Mobile caster, Item scroll)
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
                SpellHelper.Turn(Caster, m);

                SpellHelper.CheckReflect((int)this.Circle, Caster, ref m);

                if (m.Spell != null)
                    m.Spell.OnCasterHurt();

                m.Paralyzed = false;

                if (CheckResisted(m))
                {
                    m.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
                }
                else
                {
                    int level;

                    if (Core.AOS)
                    {
                        if (Caster.InRange(m, 2))
                        {
                            int total = (Caster.Skills.Magery.Fixed + Caster.Skills.Poisoning.Fixed) / 2;

                            if (total >= 1000)
                                level = 3;
                            else if (total > 850)
                                level = 2;
                            else if (total > 650)
                                level = 1;
                            else
                                level = 0;
                        }
                        else
                        {
                            level = 0;
                        }
                    }
                    else
                    {
                        //double total = Caster.Skills[SkillName.Magery].Value + Caster.Skills[SkillName.Poisoning].Value;

                        #region Dueling
                        double total = Caster.Skills[SkillName.Magery].Value;

                        if (Caster is Mobiles.PlayerMobile)
                        {
                            Mobiles.PlayerMobile pm = (Mobiles.PlayerMobile)Caster;

                            if (pm.DuelContext != null && pm.DuelContext.Started && !pm.DuelContext.Finished && !pm.DuelContext.Ruleset.GetOption("Skills", "Poisoning"))
                            {
                            }
                            else
                            {
                                total += Caster.Skills[SkillName.Poisoning].Value;
                            }
                        }
                        else
                        {
                            total += Caster.Skills[SkillName.Poisoning].Value;
                        }
                        #endregion

                        double dist = Caster.GetDistanceToSqrt(m);

                        if (dist >= 3.0)
                            total -= (dist - 3.0) * 10.0;

                        if (total >= 200.0 && 1 > Utility.Random(10))
                            level = 3;
                        else if (total > (Core.AOS ? 170.1 : 170.0))
                            level = 2;
                        else if (total > (Core.AOS ? 130.1 : 130.0))
                            level = 1;
                        else
                            level = 0;
                    }

                    m.ApplyPoison(Caster, Poison.GetPoison(level));
                }

                m.FixedParticles(0x374A, 10, 15, 5021, EffectLayer.Waist);
                m.PlaySound(0x205);

                HarmfulSpell(m);
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private PoisonSpell m_Owner;

            public InternalTarget(PoisonSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Harmful)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                {
                    m_Owner.Target((Mobile)o);
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class TelekinesisSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Telekinesis", "Ort Por Ylem",
                203,
                9031,
                Reagent.Bloodmoss,
                Reagent.MandrakeRoot
            );

        public override SpellCircle Circle { get { return SpellCircle.Third; } }

        public TelekinesisSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(ITelekinesisable obj)
        {
            if (CheckSequence())
            {
                SpellHelper.Turn(Caster, obj);

                obj.OnTelekinesis(Caster);
            }

            FinishSequence();
        }

        public void Target(Container item)
        {
            if (CheckSequence())
            {
                SpellHelper.Turn(Caster, item);

                object root = item.RootParent;

                if (!item.IsAccessibleTo(Caster))
                {
                    item.OnDoubleClickNotAccessible(Caster);
                }
                else if (!item.CheckItemUse(Caster, item))
                {
                }
                else if (root != null && root is Mobile && root != Caster)
                {
                    item.OnSnoop(Caster);
                }
                else if (item is Corpse && !((Corpse)item).CheckLoot(Caster, null))
                {
                }
                else if (Caster.Region.OnDoubleClick(Caster, item))
                {
                    Effects.SendLocationParticles(EffectItem.Create(item.Location, item.Map, EffectItem.DefaultDuration), 0x376A, 9, 32, 5022);
                    Effects.PlaySound(item.Location, item.Map, 0x1F5);

                    item.OnItemUsed(Caster, item);
                }
            }

            FinishSequence();
        }

        public class InternalTarget : Target
        {
            private TelekinesisSpell m_Owner;

            public InternalTarget(TelekinesisSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.None)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is ITelekinesisable)
                    m_Owner.Target((ITelekinesisable)o);
                else if (o is Container)
                    m_Owner.Target((Container)o);
                else
                    from.SendLocalizedMessage(501857); // This spell won't work on that!
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class TeleportSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Teleport", "Rel Por",
                215,
                9031,
                Reagent.Bloodmoss,
                Reagent.MandrakeRoot
            );

        public override SpellCircle Circle { get { return SpellCircle.Third; } }

        public TeleportSpell(Mobile caster, Item scroll)
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
            else if (Server.Misc.WeightOverloading.IsOverloaded(Caster))
            {
                Caster.SendLocalizedMessage(502359, "", 0x22); // Thou art too encumbered to move.
                return false;
            }

            return SpellHelper.CheckTravel(Caster, TravelCheckType.TeleportFrom);
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(IPoint3D p)
        {
            IPoint3D orig = p;
            Map map = Caster.Map;

            SpellHelper.GetSurfaceTop(ref p);

            Point3D from = Caster.Location;
            Point3D to = new Point3D(p);

            if (Factions.Sigil.ExistsOn(Caster))
            {
                Caster.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
            }
            else if (Server.Misc.WeightOverloading.IsOverloaded(Caster))
            {
                Caster.SendLocalizedMessage(502359, "", 0x22); // Thou art too encumbered to move.
            }
            else if (!SpellHelper.CheckTravel(Caster, TravelCheckType.TeleportFrom))
            {
            }
            else if (!SpellHelper.CheckTravel(Caster, map, to, TravelCheckType.TeleportTo))
            {
            }
            else if (map == null || !map.CanSpawnMobile(p.X, p.Y, p.Z))
            {
                Caster.SendLocalizedMessage(501942); // That location is blocked.
            }
            else if (SpellHelper.CheckMulti(to, map))
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

                if (m.Player)
                {
                    Effects.SendLocationParticles(EffectItem.Create(from, m.Map, EffectItem.DefaultDuration), 0x3728, 10, 10, 2023);
                    Effects.SendLocationParticles(EffectItem.Create(to, m.Map, EffectItem.DefaultDuration), 0x3728, 10, 10, 5023);
                }
                else
                {
                    m.FixedParticles(0x376A, 9, 32, 0x13AF, EffectLayer.Waist);
                }

                m.PlaySound(0x1FE);

                IPooledEnumerable eable = m.GetItemsInRange(0);

                foreach (Item item in eable)
                {
                    if (item is Server.Spells.Sixth.ParalyzeFieldSpell.InternalItem || item is Server.Spells.Fifth.PoisonFieldSpell.InternalItem || item is Server.Spells.Fourth.FireFieldSpell.FireFieldItem)
                        item.OnMoveOver(m);
                }

                eable.Free();
            }

            FinishSequence();
        }

        public class InternalTarget : Target
        {
            private TeleportSpell m_Owner;

            public InternalTarget(TeleportSpell owner)
                : base(Core.ML ? 11 : 12, true, TargetFlags.None)
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

    public class UnlockSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Unlock Spell", "Ex Por",
                215,
                9001,
                Reagent.Bloodmoss,
                Reagent.SulfurousAsh
            );

        public override SpellCircle Circle { get { return SpellCircle.Third; } }

        public UnlockSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        private class InternalTarget : Target
        {
            private UnlockSpell m_Owner;

            public InternalTarget(UnlockSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.None)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                IPoint3D loc = o as IPoint3D;

                if (loc == null)
                    return;

                if (m_Owner.CheckSequence())
                {
                    SpellHelper.Turn(from, o);

                    Effects.SendLocationParticles(EffectItem.Create(new Point3D(loc), from.Map, EffectItem.DefaultDuration), 0x376A, 9, 32, 5024);

                    Effects.PlaySound(loc, from.Map, 0x1FF);

                    if (o is Mobile)
                        from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 503101); // That did not need to be unlocked.
                    else if (!(o is LockableContainer))
                        from.SendLocalizedMessage(501666); // You can't unlock that!
                    else
                    {
                        LockableContainer cont = (LockableContainer)o;

                        if (Multis.BaseHouse.CheckSecured(cont))
                            from.SendLocalizedMessage(503098); // You cannot cast this on a secure item.
                        else if (!cont.Locked)
                            from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 503101); // That did not need to be unlocked.
                        else if (cont.LockLevel == 0)
                            from.SendLocalizedMessage(501666); // You can't unlock that!
                        else
                        {
                            int level = (int)(from.Skills[SkillName.Magery].Value * 0.8) - 4;

                            if (level >= cont.RequiredSkill && !(cont is TreasureMapChest && ((TreasureMapChest)cont).Level > 2))
                            {
                                cont.Locked = false;

                                if (cont.LockLevel == -255)
                                    cont.LockLevel = cont.RequiredSkill - 10;
                            }
                            else
                                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 503099); // My spell does not seem to have an effect on that lock.
                        }
                    }
                }

                m_Owner.FinishSequence();
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class WallOfStoneSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Wall of Stone", "In Sanct Ylem",
                227,
                9011,
                false,
                Reagent.Bloodmoss,
                Reagent.Garlic
            );

        public override SpellCircle Circle { get { return SpellCircle.Third; } }

        public WallOfStoneSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(IPoint3D p)
        {
            if (!Caster.CanSee(p))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (SpellHelper.CheckTown(p, Caster) && CheckSequence())
            {
                SpellHelper.Turn(Caster, p);

                SpellHelper.GetSurfaceTop(ref p);

                int dx = Caster.Location.X - p.X;
                int dy = Caster.Location.Y - p.Y;
                int rx = (dx - dy) * 44;
                int ry = (dx + dy) * 44;

                bool eastToWest;

                if (rx >= 0 && ry >= 0)
                {
                    eastToWest = false;
                }
                else if (rx >= 0)
                {
                    eastToWest = true;
                }
                else if (ry >= 0)
                {
                    eastToWest = true;
                }
                else
                {
                    eastToWest = false;
                }

                Effects.PlaySound(p, Caster.Map, 0x1F6);

                for (int i = -1; i <= 1; ++i)
                {
                    Point3D loc = new Point3D(eastToWest ? p.X + i : p.X, eastToWest ? p.Y : p.Y + i, p.Z);
                    bool canFit = SpellHelper.AdjustField(ref loc, Caster.Map, 22, true);

                    //Effects.SendLocationParticles( EffectItem.Create( loc, Caster.Map, EffectItem.DefaultDuration ), 0x376A, 9, 10, 5025 );

                    if (!canFit)
                        continue;

                    Item item = new InternalItem(loc, Caster.Map, Caster);

                    Effects.SendLocationParticles(item, 0x376A, 9, 10, 5025);

                    //new InternalItem( loc, Caster.Map, Caster );
                }
            }

            FinishSequence();
        }

        [DispellableField]
        private class InternalItem : Item
        {
            private Timer m_Timer;
            private DateTime m_End;
            private Mobile m_Caster;

            public override bool BlocksFit { get { return true; } }

            public InternalItem(Point3D loc, Map map, Mobile caster)
                : base(0x82)
            {
                Visible = false;
                Movable = false;

                MoveToWorld(loc, map);

                m_Caster = caster;

                if (caster.InLOS(this))
                    Visible = true;
                else
                    Delete();

                if (Deleted)
                    return;

                m_Timer = new InternalTimer(this, TimeSpan.FromSeconds(10.0));
                m_Timer.Start();

                m_End = DateTime.UtcNow + TimeSpan.FromSeconds(10.0);
            }

            public InternalItem(Serial serial)
                : base(serial)
            {
            }

            public override void Serialize(GenericWriter writer)
            {
                base.Serialize(writer);

                writer.Write((int)1); // version

                writer.WriteDeltaTime(m_End);
            }

            public override void Deserialize(GenericReader reader)
            {
                base.Deserialize(reader);

                int version = reader.ReadInt();

                switch (version)
                {
                    case 1:
                        {
                            m_End = reader.ReadDeltaTime();

                            m_Timer = new InternalTimer(this, m_End - DateTime.UtcNow);
                            m_Timer.Start();

                            break;
                        }
                    case 0:
                        {
                            TimeSpan duration = TimeSpan.FromSeconds(10.0);

                            m_Timer = new InternalTimer(this, duration);
                            m_Timer.Start();

                            m_End = DateTime.UtcNow + duration;

                            break;
                        }
                }
            }

            public override bool OnMoveOver(Mobile m)
            {
                int noto;

                if (m is PlayerMobile)
                {
                    noto = Notoriety.Compute(m_Caster, m);
                    if (noto == Notoriety.Enemy || noto == Notoriety.Ally)
                        return false;
                }
                return base.OnMoveOver(m);
            }

            public override void OnAfterDelete()
            {
                base.OnAfterDelete();

                if (m_Timer != null)
                    m_Timer.Stop();
            }

            private class InternalTimer : Timer
            {
                private InternalItem m_Item;

                public InternalTimer(InternalItem item, TimeSpan duration)
                    : base(duration)
                {
                    Priority = TimerPriority.OneSecond;
                    m_Item = item;
                }

                protected override void OnTick()
                {
                    m_Item.Delete();
                }
            }
        }

        private class InternalTarget : Target
        {
            private WallOfStoneSpell m_Owner;

            public InternalTarget(WallOfStoneSpell owner)
                : base(Core.ML ? 10 : 12, true, TargetFlags.None)
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
}

namespace Server.Spells.Fourth
{
    public class ArchCureSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Arch Cure", "Vas An Nox",
                215,
                9061,
                Reagent.Garlic,
                Reagent.Ginseng,
                Reagent.MandrakeRoot
            );

        public override SpellCircle Circle { get { return SpellCircle.Fourth; } }

        public ArchCureSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        // Arch cure is now 1/4th of a second faster
        public override TimeSpan CastDelayBase { get { return base.CastDelayBase - TimeSpan.FromSeconds(0.25); } }

        public void Target(IPoint3D p)
        {
            if (!Caster.CanSee(p))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckSequence())
            {
                SpellHelper.Turn(Caster, p);

                SpellHelper.GetSurfaceTop(ref p);

                List<Mobile> targets = new List<Mobile>();

                Map map = Caster.Map;
                Mobile directTarget = p as Mobile;

                if (map != null)
                {
                    bool feluccaRules = (map.Rules == MapRules.FeluccaRules);

                    // You can target any living mobile directly, beneficial checks apply
                    if (directTarget != null && Caster.CanBeBeneficial(directTarget, false))
                        targets.Add(directTarget);

                    IPooledEnumerable eable = map.GetMobilesInRange(new Point3D(p), 2);

                    foreach (Mobile m in eable)
                    {
                        if (m == directTarget)
                            continue;

                        if (AreaCanTarget(m, feluccaRules))
                            targets.Add(m);
                    }

                    eable.Free();
                }

                Effects.PlaySound(p, Caster.Map, 0x299);

                if (targets.Count > 0)
                {
                    int cured = 0;

                    for (int i = 0; i < targets.Count; ++i)
                    {
                        Mobile m = targets[i];

                        Caster.DoBeneficial(m);

                        Poison poison = m.Poison;

                        if (poison != null)
                        {
                            int chanceToCure = 10000 + (int)(Caster.Skills[SkillName.Magery].Value * 75) - ((poison.Level + 1) * 1750);
                            chanceToCure /= 100;
                            chanceToCure -= 1;

                            if (chanceToCure > Utility.Random(100) && m.CurePoison(Caster))
                                ++cured;
                        }

                        m.FixedParticles(0x373A, 10, 15, 5012, EffectLayer.Waist);
                        m.PlaySound(0x1E0);
                    }

                    if (cured > 0)
                        Caster.SendLocalizedMessage(1010058); // You have cured the target of all poisons!
                }
            }

            FinishSequence();
        }

        private bool AreaCanTarget(Mobile target, bool feluccaRules)
        {
            /* Arch cure area effect won't cure aggressors, victims, murderers, criminals or monsters.
             * In Felucca, it will also not cure summons and pets.
             * For red players it will only cure themselves and guild members.
             */

            if (!Caster.CanBeBeneficial(target, false))
                return false;

            if (Core.AOS && target != Caster)
            {
                if (IsAggressor(target) || IsAggressed(target))
                    return false;

                if ((!IsInnocentTo(Caster, target) || !IsInnocentTo(target, Caster)) && !IsAllyTo(Caster, target))
                    return false;

                if (feluccaRules && !(target is PlayerMobile))
                    return false;
            }

            return true;
        }

        private bool IsAggressor(Mobile m)
        {
            foreach (AggressorInfo info in Caster.Aggressors)
            {
                if (m == info.Attacker && !info.Expired)
                    return true;
            }

            return false;
        }

        private bool IsAggressed(Mobile m)
        {
            foreach (AggressorInfo info in Caster.Aggressed)
            {
                if (m == info.Defender && !info.Expired)
                    return true;
            }

            return false;
        }

        private static bool IsInnocentTo(Mobile from, Mobile to)
        {
            return (Notoriety.Compute(from, (Mobile)to) == Notoriety.Innocent);
        }

        private static bool IsAllyTo(Mobile from, Mobile to)
        {
            return (Notoriety.Compute(from, (Mobile)to) == Notoriety.Ally);
        }

        private class InternalTarget : Target
        {
            private ArchCureSpell m_Owner;

            public InternalTarget(ArchCureSpell owner)
                : base(Core.ML ? 10 : 12, true, TargetFlags.None)
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

    public class ArchProtectionSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Arch Protection", "Vas Uus Sanct",
                Core.AOS ? 239 : 215,
                9011,
                Reagent.Garlic,
                Reagent.Ginseng,
                Reagent.MandrakeRoot,
                Reagent.SulfurousAsh
            );

        public override SpellCircle Circle { get { return SpellCircle.Fourth; } }

        public ArchProtectionSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(IPoint3D p)
        {
            if (!Caster.CanSee(p))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckSequence())
            {
                SpellHelper.Turn(Caster, p);

                SpellHelper.GetSurfaceTop(ref p);

                List<Mobile> targets = new List<Mobile>();

                Map map = Caster.Map;

                if (map != null)
                {
                    IPooledEnumerable eable = map.GetMobilesInRange(new Point3D(p), Core.AOS ? 2 : 3);

                    foreach (Mobile m in eable)
                    {
                        if (Caster.CanBeBeneficial(m, false))
                            targets.Add(m);
                    }

                    eable.Free();
                }

                if (Core.AOS)
                {
                    Party party = Party.Get(Caster);

                    for (int i = 0; i < targets.Count; ++i)
                    {
                        Mobile m = targets[i];

                        if (m == Caster || (party != null && party.Contains(m)))
                        {
                            Caster.DoBeneficial(m);
                            Spells.Second.ProtectionSpell.Toggle(Caster, m);
                        }
                    }
                }
                else
                {
                    Effects.PlaySound(p, Caster.Map, 0x299);

                    int val = (int)(Caster.Skills[SkillName.Magery].Value / 10.0 + 1);

                    if (targets.Count > 0)
                    {
                        for (int i = 0; i < targets.Count; ++i)
                        {
                            Mobile m = targets[i];

                            if (m.BeginAction(typeof(ArchProtectionSpell)))
                            {
                                Caster.DoBeneficial(m);
                                m.VirtualArmorMod += val;

                                AddEntry(m, val);
                                new InternalTimer(m, Caster).Start();

                                m.FixedParticles(0x375A, 9, 20, 5027, EffectLayer.Waist);
                                m.PlaySound(0x1F7);
                            }
                        }
                    }
                }
            }

            FinishSequence();
        }

        private static Dictionary<Mobile, Int32> _Table = new Dictionary<Mobile, Int32>();

        private static void AddEntry(Mobile m, Int32 v)
        {
            _Table[m] = v;
        }

        public static void RemoveEntry(Mobile m)
        {
            if (_Table.ContainsKey(m))
            {
                int v = _Table[m];
                _Table.Remove(m);
                m.EndAction(typeof(ArchProtectionSpell));
                m.VirtualArmorMod -= v;
                if (m.VirtualArmorMod < 0)
                    m.VirtualArmorMod = 0;
            }
        }

        private class InternalTimer : Timer
        {
            private Mobile m_Owner;

            public InternalTimer(Mobile target, Mobile caster)
                : base(TimeSpan.FromSeconds(0))
            {
                double time = caster.Skills[SkillName.Magery].Value * 1.2;
                if (time > 144)
                    time = 144;
                Delay = TimeSpan.FromSeconds(time);
                Priority = TimerPriority.OneSecond;

                m_Owner = target;
            }

            protected override void OnTick()
            {
                ArchProtectionSpell.RemoveEntry(m_Owner);
            }
        }

        private class InternalTarget : Target
        {
            private ArchProtectionSpell m_Owner;

            public InternalTarget(ArchProtectionSpell owner)
                : base(Core.ML ? 10 : 12, true, TargetFlags.None)
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

    public class CurseSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Curse", "Des Sanct",
                227,
                9031,
                Reagent.Nightshade,
                Reagent.Garlic,
                Reagent.SulfurousAsh
            );

        public override SpellCircle Circle { get { return SpellCircle.Fourth; } }

        public CurseSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        private static Hashtable m_UnderEffect = new Hashtable();

        public static void RemoveEffect(object state)
        {
            Mobile m = (Mobile)state;

            m_UnderEffect.Remove(m);

            m.UpdateResistances();
        }

        public static bool UnderEffect(Mobile m)
        {
            return m_UnderEffect.Contains(m);
        }

        public void Target(Mobile m)
        {
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckHSequence(m))
            {
                SpellHelper.Turn(Caster, m);

                SpellHelper.CheckReflect((int)this.Circle, Caster, ref m);

                SpellHelper.AddStatCurse(Caster, m, StatType.Str); SpellHelper.DisableSkillCheck = true;
                SpellHelper.AddStatCurse(Caster, m, StatType.Dex);
                SpellHelper.AddStatCurse(Caster, m, StatType.Int); SpellHelper.DisableSkillCheck = false;

                Timer t = (Timer)m_UnderEffect[m];

                if (Caster.Player && m.Player /*&& Caster != m */ && t == null)	//On OSI you CAN curse yourself and get this effect.
                {
                    TimeSpan duration = SpellHelper.GetDuration(Caster, m);
                    m_UnderEffect[m] = t = Timer.DelayCall(duration, new TimerStateCallback(RemoveEffect), m);
                    m.UpdateResistances();
                }

                if (m.Spell != null)
                    m.Spell.OnCasterHurt();

                m.Paralyzed = false;

                m.FixedParticles(0x374A, 10, 15, 5028, EffectLayer.Waist);
                m.PlaySound(0x1E1);

                int percentage = (int)(SpellHelper.GetOffsetScalar(Caster, m, true) * 100);
                TimeSpan length = SpellHelper.GetDuration(Caster, m);

                string args = String.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", percentage, percentage, percentage, 10, 10, 10, 10);

                BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.Curse, 1075835, 1075836, length, m, args.ToString()));

                HarmfulSpell(m);
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private CurseSpell m_Owner;

            public InternalTarget(CurseSpell owner)
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

    public class FireFieldSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Fire Field", "In Flam Grav",
                215,
                9041,
                false,
                Reagent.BlackPearl,
                Reagent.SpidersSilk,
                Reagent.SulfurousAsh
            );

        public override SpellCircle Circle { get { return SpellCircle.Fourth; } }

        public FireFieldSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(IPoint3D p)
        {
            if (!Caster.CanSee(p))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (SpellHelper.CheckTown(p, Caster) && CheckSequence())
            {
                SpellHelper.Turn(Caster, p);

                SpellHelper.GetSurfaceTop(ref p);

                int dx = Caster.Location.X - p.X;
                int dy = Caster.Location.Y - p.Y;
                int rx = (dx - dy) * 44;
                int ry = (dx + dy) * 44;

                bool eastToWest;

                if (rx >= 0 && ry >= 0)
                {
                    eastToWest = false;
                }
                else if (rx >= 0)
                {
                    eastToWest = true;
                }
                else if (ry >= 0)
                {
                    eastToWest = true;
                }
                else
                {
                    eastToWest = false;
                }

                Effects.PlaySound(p, Caster.Map, 0x20C);

                int itemID = eastToWest ? 0x398C : 0x3996;

                TimeSpan duration;

                if (Core.AOS)
                    duration = TimeSpan.FromSeconds((15 + (Caster.Skills.Magery.Fixed / 5)) / 4);
                else
                    duration = TimeSpan.FromSeconds(4.0 + (Caster.Skills[SkillName.Magery].Value * 0.5));

                for (int i = -2; i <= 2; ++i)
                {
                    Point3D loc = new Point3D(eastToWest ? p.X + i : p.X, eastToWest ? p.Y : p.Y + i, p.Z);

                    new FireFieldItem(itemID, loc, Caster, Caster.Map, duration, i);
                }
            }

            FinishSequence();
        }

        [DispellableField]
        public class FireFieldItem : Item
        {
            private Timer m_Timer;
            private DateTime m_End;
            private Mobile m_Caster;
            private int m_Damage;

            public override bool BlocksFit { get { return true; } }

            public FireFieldItem(int itemID, Point3D loc, Mobile caster, Map map, TimeSpan duration, int val)
                : this(itemID, loc, caster, map, duration, val, 2)
            {
            }

            public FireFieldItem(int itemID, Point3D loc, Mobile caster, Map map, TimeSpan duration, int val, int damage)
                : base(itemID)
            {
                bool canFit = SpellHelper.AdjustField(ref loc, map, 12, false);

                Visible = false;
                Movable = false;
                Light = LightType.Circle300;

                MoveToWorld(loc, map);

                m_Caster = caster;

                m_Damage = damage;

                m_End = DateTime.UtcNow + duration;

                m_Timer = new InternalTimer(this, TimeSpan.FromSeconds(Math.Abs(val) * 0.2), caster.InLOS(this), canFit);
                m_Timer.Start();
            }

            public override void OnAfterDelete()
            {
                base.OnAfterDelete();

                if (m_Timer != null)
                    m_Timer.Stop();
            }

            public FireFieldItem(Serial serial)
                : base(serial)
            {
            }

            public override void Serialize(GenericWriter writer)
            {
                base.Serialize(writer);

                writer.Write((int)2); // version

                writer.Write(m_Damage);
                writer.Write(m_Caster);
                writer.WriteDeltaTime(m_End);
            }

            public override void Deserialize(GenericReader reader)
            {
                base.Deserialize(reader);

                int version = reader.ReadInt();

                switch (version)
                {
                    case 2:
                        {
                            m_Damage = reader.ReadInt();
                            goto case 1;
                        }
                    case 1:
                        {
                            m_Caster = reader.ReadMobile();

                            goto case 0;
                        }
                    case 0:
                        {
                            m_End = reader.ReadDeltaTime();

                            m_Timer = new InternalTimer(this, TimeSpan.Zero, true, true);
                            m_Timer.Start();

                            break;
                        }
                }

                if (version < 2)
                    m_Damage = 2;
            }

            public override bool OnMoveOver(Mobile m)
            {
                if (Visible && m_Caster != null && (!Core.AOS || m != m_Caster) && SpellHelper.ValidIndirectTarget(m_Caster, m) && m_Caster.CanBeHarmful(m, false))
                {
                    if (SpellHelper.CanRevealCaster(m))
                        m_Caster.RevealingAction();

                    m_Caster.DoHarmful(m);

                    int damage = m_Damage;

                    if (!Core.AOS && m.CheckSkill(SkillName.MagicResist, 0.0, 30.0))
                    {
                        damage = 1;

                        m.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
                    }

                    AOS.Damage(m, m_Caster, damage, 0, 100, 0, 0, 0);
                    m.PlaySound(0x208);

                    if (m is BaseCreature)
                        ((BaseCreature)m).OnHarmfulSpell(m_Caster);
                }

                return true;
            }

            private class InternalTimer : Timer
            {
                private FireFieldItem m_Item;
                private bool m_InLOS, m_CanFit;

                private static Queue m_Queue = new Queue();

                public InternalTimer(FireFieldItem item, TimeSpan delay, bool inLOS, bool canFit)
                    : base(delay, TimeSpan.FromSeconds(1.0))
                {
                    m_Item = item;
                    m_InLOS = inLOS;
                    m_CanFit = canFit;

                    Priority = TimerPriority.FiftyMS;
                }

                protected override void OnTick()
                {
                    if (m_Item.Deleted)
                        return;

                    if (!m_Item.Visible)
                    {
                        if (m_InLOS && m_CanFit)
                            m_Item.Visible = true;
                        else
                            m_Item.Delete();

                        if (!m_Item.Deleted)
                        {
                            m_Item.ProcessDelta();
                            Effects.SendLocationParticles(EffectItem.Create(m_Item.Location, m_Item.Map, EffectItem.DefaultDuration), 0x376A, 9, 10, 5029);
                        }
                    }
                    else if (DateTime.UtcNow > m_Item.m_End)
                    {
                        m_Item.Delete();
                        Stop();
                    }
                    else
                    {
                        Map map = m_Item.Map;
                        Mobile caster = m_Item.m_Caster;

                        if (map != null && caster != null)
                        {
                            foreach (Mobile m in m_Item.GetMobilesInRange(0))
                            {
                                if ((m.Z + 16) > m_Item.Z && (m_Item.Z + 12) > m.Z && (!Core.AOS || m != caster) && SpellHelper.ValidIndirectTarget(caster, m) && caster.CanBeHarmful(m, false))
                                    m_Queue.Enqueue(m);
                            }

                            while (m_Queue.Count > 0)
                            {
                                Mobile m = (Mobile)m_Queue.Dequeue();

                                if (SpellHelper.CanRevealCaster(m))
                                    caster.RevealingAction();

                                caster.DoHarmful(m);

                                int damage = m_Item.m_Damage;

                                if (!Core.AOS && m.CheckSkill(SkillName.MagicResist, 0.0, 30.0))
                                {
                                    damage = 1;

                                    m.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
                                }

                                AOS.Damage(m, caster, damage, 0, 100, 0, 0, 0);
                                m.PlaySound(0x208);

                                if (m is BaseCreature)
                                    ((BaseCreature)m).OnHarmfulSpell(caster);
                            }
                        }
                    }
                }
            }
        }

        private class InternalTarget : Target
        {
            private FireFieldSpell m_Owner;

            public InternalTarget(FireFieldSpell owner)
                : base(Core.ML ? 10 : 12, true, TargetFlags.None)
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

    public class GreaterHealSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Greater Heal", "In Vas Mani",
                204,
                9061,
                Reagent.Garlic,
                Reagent.Ginseng,
                Reagent.MandrakeRoot,
                Reagent.SpidersSilk
            );

        public override SpellCircle Circle { get { return SpellCircle.Fourth; } }

        public GreaterHealSpell(Mobile caster, Item scroll)
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
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (m is BaseCreature && ((BaseCreature)m).IsAnimatedDead)
            {
                Caster.SendLocalizedMessage(1061654); // You cannot heal that which is not alive.
            }
            else if (m.IsDeadBondedPet)
            {
                Caster.SendLocalizedMessage(1060177); // You cannot heal a creature that is already dead!
            }
            else if (m is Golem)
            {
                Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, 500951); // You cannot heal that.
            }
            else if (m.Poisoned || Server.Items.MortalStrike.IsWounded(m))
            {
                Caster.LocalOverheadMessage(MessageType.Regular, 0x22, (Caster == m) ? 1005000 : 1010398);
            }
            else if (CheckBSequence(m))
            {
                SpellHelper.Turn(Caster, m);

                // Algorithm: (40% of magery) + (1-10)

                int toHeal = (int)(Caster.Skills[SkillName.Magery].Value * 0.4);
                toHeal += Utility.Random(1, 10);

                //m.Heal( toHeal, Caster );
                SpellHelper.Heal(toHeal, m, Caster);

                m.FixedParticles(0x376A, 9, 32, 5030, EffectLayer.Waist);
                m.PlaySound(0x202);
            }

            FinishSequence();
        }

        public class InternalTarget : Target
        {
            private GreaterHealSpell m_Owner;

            public InternalTarget(GreaterHealSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Beneficial)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                {
                    m_Owner.Target((Mobile)o);
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class LightningSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Lightning", "Por Ort Grav",
                239,
                9021,
                Reagent.MandrakeRoot,
                Reagent.SulfurousAsh
            );

        public override SpellCircle Circle { get { return SpellCircle.Fourth; } }

        public LightningSpell(Mobile caster, Item scroll)
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
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckHSequence(m))
            {
                SpellHelper.Turn(Caster, m);

                SpellHelper.CheckReflect((int)this.Circle, Caster, ref m);

                double damage;

                if (Core.AOS)
                {
                    damage = GetNewAosDamage(23, 1, 4, m);
                }
                else
                {
                    damage = Utility.Random(12, 9);

                    if (CheckResisted(m))
                    {
                        damage *= 0.75;

                        m.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
                    }

                    damage *= GetDamageScalar(m);
                }

                m.BoltEffect(0);

                SpellHelper.Damage(this, m, damage, 0, 0, 0, 0, 100);
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private LightningSpell m_Owner;

            public InternalTarget(LightningSpell owner)
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

    public class ManaDrainSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Mana Drain", "Ort Rel",
                215,
                9031,
                Reagent.BlackPearl,
                Reagent.MandrakeRoot,
                Reagent.SpidersSilk
            );

        public override SpellCircle Circle { get { return SpellCircle.Fourth; } }

        public ManaDrainSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        private static Dictionary<Mobile, Timer> m_Table = new Dictionary<Mobile, Timer>();

        private void AosDelay_Callback(object state)
        {
            object[] states = (object[])state;

            Mobile m = (Mobile)states[0];
            int mana = (int)states[1];

            if (m.Alive && !m.IsDeadBondedPet)
            {
                m.Mana += mana;

                m.FixedEffect(0x3779, 10, 25);
                m.PlaySound(0x28E);
            }

            m_Table.Remove(m);
        }

        public void Target(Mobile m)
        {
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckHSequence(m))
            {
                SpellHelper.Turn(Caster, m);

                SpellHelper.CheckReflect((int)this.Circle, Caster, ref m);

                if (m.Spell != null)
                    m.Spell.OnCasterHurt();

                m.Paralyzed = false;

                if (Core.AOS)
                {
                    int toDrain = 40 + (int)(GetDamageSkill(Caster) - GetResistSkill(m));

                    if (toDrain < 0)
                        toDrain = 0;
                    else if (toDrain > m.Mana)
                        toDrain = m.Mana;

                    if (m_Table.ContainsKey(m))
                        toDrain = 0;

                    m.FixedParticles(0x3789, 10, 25, 5032, EffectLayer.Head);
                    m.PlaySound(0x1F8);

                    if (toDrain > 0)
                    {
                        m.Mana -= toDrain;

                        m_Table[m] = Timer.DelayCall(TimeSpan.FromSeconds(5.0), new TimerStateCallback(AosDelay_Callback), new object[] { m, toDrain });
                    }
                }
                else
                {
                    if (CheckResisted(m))
                        m.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
                    else if (m.Mana >= 100)
                        m.Mana -= Utility.Random(1, 100);
                    else
                        m.Mana -= Utility.Random(1, m.Mana);

                    m.FixedParticles(0x374A, 10, 15, 5032, EffectLayer.Head);
                    m.PlaySound(0x1F8);
                }

                HarmfulSpell(m);
            }

            FinishSequence();
        }

        public override double GetResistPercent(Mobile target)
        {
            return 99.0;
        }

        private class InternalTarget : Target
        {
            private ManaDrainSpell m_Owner;

            public InternalTarget(ManaDrainSpell owner)
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

    public class RecallSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Recall", "Kal Ort Por",
                239,
                9031,
                Reagent.BlackPearl,
                Reagent.Bloodmoss,
                Reagent.MandrakeRoot
            );

        public override SpellCircle Circle { get { return SpellCircle.Fourth; } }

        private RunebookEntry m_Entry;
        private Runebook m_Book;

        public RecallSpell(Mobile caster, Item scroll)
            : this(caster, scroll, null, null)
        {
        }

        public RecallSpell(Mobile caster, Item scroll, RunebookEntry entry, Runebook book)
            : base(caster, scroll, m_Info)
        {
            m_Entry = entry;
            m_Book = book;
        }

        public override void GetCastSkills(out double min, out double max)
        {
            if (TransformationSpellHelper.UnderTransformation(Caster, typeof(WraithFormSpell)))
                min = max = 0;
            else if (Core.SE && m_Book != null)	//recall using Runebook charge
                min = max = 0;
            else
                base.GetCastSkills(out min, out max);
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
                Caster.SendLocalizedMessage(1005564, "", 0x22); // Wouldst thou flee during the heat of battle??
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
                Caster.SendLocalizedMessage(1005564, "", 0x22); // Wouldst thou flee during the heat of battle??
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

                Caster.PlaySound(0x1FC);
                Caster.MoveToWorld(loc, map);
                Caster.PlaySound(0x1FC);
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private RecallSpell m_Owner;

            public InternalTarget(RecallSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.None)
            {
                m_Owner = owner;

                owner.Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, 501029); // Select Marked item.
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

namespace Server.Spells.Fifth
{
    public class BladeSpiritsSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Blade Spirits", "In Jux Hur Ylem",
                266,
                9040,
                false,
                Reagent.BlackPearl,
                Reagent.MandrakeRoot,
                Reagent.Nightshade
            );

        public override SpellCircle Circle { get { return SpellCircle.Fifth; } }

        public BladeSpiritsSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override TimeSpan GetCastDelay()
        {
            if (Core.AOS)
                return TimeSpan.FromTicks(base.GetCastDelay().Ticks * ((Core.SE) ? 3 : 5));

            return base.GetCastDelay() + TimeSpan.FromSeconds(6.0);
        }

        public override bool CheckCast()
        {
            if (!base.CheckCast())
                return false;

            if ((Caster.Followers + (Core.SE ? 2 : 1)) > Caster.FollowersMax)
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

        public void Target(IPoint3D p)
        {
            Map map = Caster.Map;

            SpellHelper.GetSurfaceTop(ref p);

            if (map == null || !map.CanSpawnMobile(p.X, p.Y, p.Z))
            {
                Caster.SendLocalizedMessage(501942); // That location is blocked.
            }
            else if (SpellHelper.CheckTown(p, Caster) && CheckSequence())
            {
                TimeSpan duration;

                if (Core.AOS)
                    duration = TimeSpan.FromSeconds(120);
                else
                    duration = TimeSpan.FromSeconds(Utility.Random(80, 40));

                BaseCreature.Summon(new BladeSpirits(), false, Caster, new Point3D(p), 0x212, duration);
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private BladeSpiritsSpell m_Owner;

            public InternalTarget(BladeSpiritsSpell owner)
                : base(Core.ML ? 10 : 12, true, TargetFlags.None)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is IPoint3D)
                    m_Owner.Target((IPoint3D)o);
            }

            protected override void OnTargetOutOfLOS(Mobile from, object o)
            {
                from.SendLocalizedMessage(501943); // Target cannot be seen. Try again.
                from.Target = new InternalTarget(m_Owner);
                from.Target.BeginTimeout(from, TimeoutTime - DateTime.UtcNow);
                m_Owner = null;
            }

            protected override void OnTargetFinish(Mobile from)
            {
                if (m_Owner != null)
                    m_Owner.FinishSequence();
            }
        }
    }

    public class DispelFieldSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Dispel Field", "An Grav",
                206,
                9002,
                Reagent.BlackPearl,
                Reagent.SpidersSilk,
                Reagent.SulfurousAsh,
                Reagent.Garlic
            );

        public override SpellCircle Circle { get { return SpellCircle.Fifth; } }

        public DispelFieldSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(Item item)
        {
            Type t = item.GetType();

            if (!Caster.CanSee(item))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (!t.IsDefined(typeof(DispellableFieldAttribute), false))
            {
                Caster.SendLocalizedMessage(1005049); // That cannot be dispelled.
            }
            else if (item is Moongate && !((Moongate)item).Dispellable)
            {
                Caster.SendLocalizedMessage(1005047); // That magic is too chaotic
            }
            else if (CheckSequence())
            {
                SpellHelper.Turn(Caster, item);

                Effects.SendLocationParticles(EffectItem.Create(item.Location, item.Map, EffectItem.DefaultDuration), 0x376A, 9, 20, 5042);
                Effects.PlaySound(item.GetWorldLocation(), item.Map, 0x201);

                item.Delete();
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private DispelFieldSpell m_Owner;

            public InternalTarget(DispelFieldSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.None)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Item)
                {
                    m_Owner.Target((Item)o);
                }
                else
                {
                    m_Owner.Caster.SendLocalizedMessage(1005049); // That cannot be dispelled.
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class IncognitoSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Incognito", "Kal In Ex",
                206,
                9002,
                Reagent.Bloodmoss,
                Reagent.Garlic,
                Reagent.Nightshade
            );

        public override SpellCircle Circle { get { return SpellCircle.Fifth; } }

        public IncognitoSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override bool CheckCast()
        {
            if (Factions.Sigil.ExistsOn(Caster))
            {
                Caster.SendLocalizedMessage(1010445); // You cannot incognito if you have a sigil
                return false;
            }
            else if (!Caster.CanBeginAction(typeof(IncognitoSpell)))
            {
                Caster.SendLocalizedMessage(1005559); // This spell is already in effect.
                return false;
            }
            else if (Caster.BodyMod == 183 || Caster.BodyMod == 184)
            {
                Caster.SendLocalizedMessage(1042402); // You cannot use incognito while wearing body paint
                return false;
            }

            return true;
        }

        public override void OnCast()
        {
            if (Factions.Sigil.ExistsOn(Caster))
            {
                Caster.SendLocalizedMessage(1010445); // You cannot incognito if you have a sigil
            }
            else if (!Caster.CanBeginAction(typeof(IncognitoSpell)))
            {
                Caster.SendLocalizedMessage(1005559); // This spell is already in effect.
            }
            else if (Caster.BodyMod == 183 || Caster.BodyMod == 184)
            {
                Caster.SendLocalizedMessage(1042402); // You cannot use incognito while wearing body paint
            }
            else if (DisguiseTimers.IsDisguised(Caster))
            {
                Caster.SendLocalizedMessage(1061631); // You can't do that while disguised.
            }
            else if (!Caster.CanBeginAction(typeof(PolymorphSpell)) || Caster.IsBodyMod)
            {
                DoFizzle();
            }
            else if (CheckSequence())
            {
                if (Caster.BeginAction(typeof(IncognitoSpell)))
                {
                    DisguiseTimers.StopTimer(Caster);

                    Caster.HueMod = Caster.Race.RandomSkinHue();
                    Caster.NameMod = Caster.Female ? NameList.RandomName("female") : NameList.RandomName("male");

                    PlayerMobile pm = Caster as PlayerMobile;

                    if (pm != null && pm.Race != null)
                    {
                        pm.SetHairMods(pm.Race.RandomHair(pm.Female), pm.Race.RandomFacialHair(pm.Female));
                        pm.HairHue = pm.Race.RandomHairHue();
                        pm.FacialHairHue = pm.Race.RandomHairHue();
                    }

                    Caster.FixedParticles(0x373A, 10, 15, 5036, EffectLayer.Head);
                    Caster.PlaySound(0x3BD);

                    BaseArmor.ValidateMobile(Caster);
                    BaseClothing.ValidateMobile(Caster);

                    StopTimer(Caster);


                    int timeVal = ((6 * Caster.Skills.Magery.Fixed) / 50) + 1;

                    if (timeVal > 144)
                        timeVal = 144;

                    TimeSpan length = TimeSpan.FromSeconds(timeVal);


                    Timer t = new InternalTimer(Caster, length);

                    m_Timers[Caster] = t;

                    t.Start();

                    BuffInfo.AddBuff(Caster, new BuffInfo(BuffIcon.Incognito, 1075819, length, Caster));

                }
                else
                {
                    Caster.SendLocalizedMessage(1079022); // You're already incognitoed!
                }
            }

            FinishSequence();
        }

        private static Hashtable m_Timers = new Hashtable();

        public static bool StopTimer(Mobile m)
        {
            Timer t = (Timer)m_Timers[m];

            if (t != null)
            {
                t.Stop();
                m_Timers.Remove(m);
                BuffInfo.RemoveBuff(m, BuffIcon.Incognito);
            }

            return (t != null);
        }

        private static int[] m_HairIDs = new int[]
			{
				0x2044, 0x2045, 0x2046,
				0x203C, 0x203B, 0x203D,
				0x2047, 0x2048, 0x2049,
				0x204A, 0x0000
			};

        private static int[] m_BeardIDs = new int[]
			{
				0x203E, 0x203F, 0x2040,
				0x2041, 0x204B, 0x204C,
				0x204D, 0x0000
			};

        private class InternalTimer : Timer
        {
            private Mobile m_Owner;

            public InternalTimer(Mobile owner, TimeSpan length)
                : base(length)
            {
                m_Owner = owner;

                /*
                int val = ((6 * owner.Skills.Magery.Fixed) / 50) + 1;

                if ( val > 144 )
                    val = 144;

                Delay = TimeSpan.FromSeconds( val );
                 * */
                Priority = TimerPriority.OneSecond;
            }

            protected override void OnTick()
            {
                if (!m_Owner.CanBeginAction(typeof(IncognitoSpell)))
                {
                    if (m_Owner is PlayerMobile)
                        ((PlayerMobile)m_Owner).SetHairMods(-1, -1);

                    m_Owner.BodyMod = 0;
                    m_Owner.HueMod = -1;
                    m_Owner.NameMod = null;
                    m_Owner.EndAction(typeof(IncognitoSpell));

                    BaseArmor.ValidateMobile(m_Owner);
                    BaseClothing.ValidateMobile(m_Owner);
                }
            }
        }
    }

    public class MagicReflectSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Magic Reflection", "In Jux Sanct",
                242,
                9012,
                Reagent.Garlic,
                Reagent.MandrakeRoot,
                Reagent.SpidersSilk
            );

        public override SpellCircle Circle { get { return SpellCircle.Fifth; } }

        public MagicReflectSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override bool CheckCast()
        {
            if (Core.AOS)
                return true;

            if (Caster.MagicDamageAbsorb > 0)
            {
                Caster.SendLocalizedMessage(1005559); // This spell is already in effect.
                return false;
            }
            else if (!Caster.CanBeginAction(typeof(DefensiveSpell)))
            {
                Caster.SendLocalizedMessage(1005385); // The spell will not adhere to you at this time.
                return false;
            }

            return true;
        }

        private static Hashtable m_Table = new Hashtable();

        public override void OnCast()
        {
            if (Core.AOS)
            {
                /* The magic reflection spell decreases the caster's physical resistance, while increasing the caster's elemental resistances.
                 * Physical decrease = 25 - (Inscription/20).
                 * Elemental resistance = +10 (-20 physical, +10 elemental at GM Inscription)
                 * The magic reflection spell has an indefinite duration, becoming active when cast, and deactivated when re-cast.
                 * Reactive Armor, Protection, and Magic Reflection will stay on—even after logging out, even after dying—until you “turn them off” by casting them again. 
                 */

                if (CheckSequence())
                {
                    Mobile targ = Caster;

                    ResistanceMod[] mods = (ResistanceMod[])m_Table[targ];

                    if (mods == null)
                    {
                        targ.PlaySound(0x1E9);
                        targ.FixedParticles(0x375A, 10, 15, 5037, EffectLayer.Waist);

                        int physiMod = -25 + (int)(targ.Skills[SkillName.Inscribe].Value / 20);
                        int otherMod = 10;

                        mods = new ResistanceMod[5]
							{
								new ResistanceMod( ResistanceType.Physical, physiMod ),
								new ResistanceMod( ResistanceType.Fire,		otherMod ),
								new ResistanceMod( ResistanceType.Cold,		otherMod ),
								new ResistanceMod( ResistanceType.Poison,	otherMod ),
								new ResistanceMod( ResistanceType.Energy,	otherMod )
							};

                        m_Table[targ] = mods;

                        for (int i = 0; i < mods.Length; ++i)
                            targ.AddResistanceMod(mods[i]);

                        string buffFormat = String.Format("{0}\t+{1}\t+{1}\t+{1}\t+{1}", physiMod, otherMod);

                        BuffInfo.AddBuff(targ, new BuffInfo(BuffIcon.MagicReflection, 1075817, buffFormat, true));
                    }
                    else
                    {
                        targ.PlaySound(0x1ED);
                        targ.FixedParticles(0x375A, 10, 15, 5037, EffectLayer.Waist);

                        m_Table.Remove(targ);

                        for (int i = 0; i < mods.Length; ++i)
                            targ.RemoveResistanceMod(mods[i]);

                        BuffInfo.RemoveBuff(targ, BuffIcon.MagicReflection);
                    }
                }

                FinishSequence();
            }
            else
            {
                if (Caster.MagicDamageAbsorb > 0)
                {
                    Caster.SendLocalizedMessage(1005559); // This spell is already in effect.
                }
                else if (!Caster.CanBeginAction(typeof(DefensiveSpell)))
                {
                    Caster.SendLocalizedMessage(1005385); // The spell will not adhere to you at this time.
                }
                else if (CheckSequence())
                {
                    if (Caster.BeginAction(typeof(DefensiveSpell)))
                    {
                        int value = (int)(Caster.Skills[SkillName.Magery].Value + Caster.Skills[SkillName.Inscribe].Value);
                        value = (int)(8 + (value / 200) * 7.0);//absorb from 8 to 15 "circles"

                        Caster.MagicDamageAbsorb = value;

                        Caster.FixedParticles(0x375A, 10, 15, 5037, EffectLayer.Waist);
                        Caster.PlaySound(0x1E9);
                    }
                    else
                    {
                        Caster.SendLocalizedMessage(1005385); // The spell will not adhere to you at this time.
                    }
                }

                FinishSequence();
            }
        }

        public static void EndReflect(Mobile m)
        {
            if (m_Table.Contains(m))
            {
                ResistanceMod[] mods = (ResistanceMod[])m_Table[m];

                if (mods != null)
                {
                    for (int i = 0; i < mods.Length; ++i)
                        m.RemoveResistanceMod(mods[i]);
                }

                m_Table.Remove(m);
                BuffInfo.RemoveBuff(m, BuffIcon.MagicReflection);
            }
        }
    }

    public class MindBlastSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Mind Blast", "Por Corp Wis",
                218,
                Core.AOS ? 9002 : 9032,
                Reagent.BlackPearl,
                Reagent.MandrakeRoot,
                Reagent.Nightshade,
                Reagent.SulfurousAsh
            );

        public override SpellCircle Circle { get { return SpellCircle.Fifth; } }

        public MindBlastSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
            if (Core.AOS)
                m_Info.LeftHandEffect = m_Info.RightHandEffect = 9002;
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        private void AosDelay_Callback(object state)
        {
            object[] states = (object[])state;
            Mobile caster = (Mobile)states[0];
            Mobile target = (Mobile)states[1];
            Mobile defender = (Mobile)states[2];
            int damage = (int)states[3];

            if (caster.HarmfulCheck(defender))
            {
                SpellHelper.Damage(this, target, Utility.RandomMinMax(damage, damage + 4), 0, 0, 100, 0, 0);

                target.FixedParticles(0x374A, 10, 15, 5038, 1181, 2, EffectLayer.Head);
                target.PlaySound(0x213);
            }
        }

        public override bool DelayedDamage { get { return !Core.AOS; } }

        public void Target(Mobile m)
        {
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (Core.AOS)
            {
                if (Caster.CanBeHarmful(m) && CheckSequence())
                {
                    Mobile from = Caster, target = m;

                    SpellHelper.Turn(from, target);

                    SpellHelper.CheckReflect((int)this.Circle, ref from, ref target);

                    int damage = (int)((Caster.Skills[SkillName.Magery].Value + Caster.Int) / 5);

                    if (damage > 60)
                        damage = 60;

                    Timer.DelayCall(TimeSpan.FromSeconds(1.0),
                        new TimerStateCallback(AosDelay_Callback),
                        new object[] { Caster, target, m, damage });
                }
            }
            else if (CheckHSequence(m))
            {
                Mobile from = Caster, target = m;

                SpellHelper.Turn(from, target);

                SpellHelper.CheckReflect((int)this.Circle, ref from, ref target);

                // Algorithm: (highestStat - lowestStat) / 2 [- 50% if resisted]

                int highestStat = target.Str, lowestStat = target.Str;

                if (target.Dex > highestStat)
                    highestStat = target.Dex;

                if (target.Dex < lowestStat)
                    lowestStat = target.Dex;

                if (target.Int > highestStat)
                    highestStat = target.Int;

                if (target.Int < lowestStat)
                    lowestStat = target.Int;

                if (highestStat > 150)
                    highestStat = 150;

                if (lowestStat > 150)
                    lowestStat = 150;

                double damage = GetDamageScalar(m) * (highestStat - lowestStat) / 2; // Many users prefer 3 or 4

                if (damage > 45)
                    damage = 45;

                if (CheckResisted(target))
                {
                    damage /= 2;
                    target.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
                }

                from.FixedParticles(0x374A, 10, 15, 2038, EffectLayer.Head);

                target.FixedParticles(0x374A, 10, 15, 5038, EffectLayer.Head);
                target.PlaySound(0x213);

                SpellHelper.Damage(this, target, damage, 0, 0, 100, 0, 0);
            }

            FinishSequence();
        }

        public override double GetSlayerDamageScalar(Mobile target)
        {
            return 1.0; //This spell isn't affected by slayer spellbooks
        }

        private class InternalTarget : Target
        {
            private MindBlastSpell m_Owner;

            public InternalTarget(MindBlastSpell owner)
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

    public class ParalyzeSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Paralyze", "An Ex Por",
                218,
                9012,
                Reagent.Garlic,
                Reagent.MandrakeRoot,
                Reagent.SpidersSilk
            );

        public override SpellCircle Circle { get { return SpellCircle.Fifth; } }

        public ParalyzeSpell(Mobile caster, Item scroll)
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
            else if (Core.AOS && (m.Frozen || m.Paralyzed || (m.Spell != null && m.Spell.IsCasting && !(m.Spell is PaladinSpell))))
            {
                Caster.SendLocalizedMessage(1061923); // The target is already frozen.
            }
            else if (CheckHSequence(m))
            {
                SpellHelper.Turn(Caster, m);

                SpellHelper.CheckReflect((int)this.Circle, Caster, ref m);

                double duration;

                if (Core.AOS)
                {
                    int secs = (int)((GetDamageSkill(Caster) / 10) - (GetResistSkill(m) / 10));

                    if (!Core.SE)
                        secs += 2;

                    if (!m.Player)
                        secs *= 3;

                    if (secs < 0)
                        secs = 0;

                    duration = secs;
                }
                else
                {
                    // Algorithm: ((20% of magery) + 7) seconds [- 50% if resisted]

                    duration = 7.0 + (Caster.Skills[SkillName.Magery].Value * 0.2);

                    if (CheckResisted(m))
                        duration *= 0.75;
                }

                if (m is PlagueBeastLord)
                {
                    ((PlagueBeastLord)m).OnParalyzed(Caster);
                    duration = 120;
                }

                m.Paralyze(TimeSpan.FromSeconds(duration));

                m.PlaySound(0x204);
                m.FixedEffect(0x376A, 6, 1);

                HarmfulSpell(m);
            }

            FinishSequence();
        }

        public class InternalTarget : Target
        {
            private ParalyzeSpell m_Owner;

            public InternalTarget(ParalyzeSpell owner)
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

    public class PoisonFieldSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Poison Field", "In Nox Grav",
                230,
                9052,
                false,
                Reagent.BlackPearl,
                Reagent.Nightshade,
                Reagent.SpidersSilk
            );

        public override SpellCircle Circle { get { return SpellCircle.Fifth; } }

        public PoisonFieldSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(IPoint3D p)
        {
            if (!Caster.CanSee(p))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (SpellHelper.CheckTown(p, Caster) && CheckSequence())
            {
                SpellHelper.Turn(Caster, p);

                SpellHelper.GetSurfaceTop(ref p);

                int dx = Caster.Location.X - p.X;
                int dy = Caster.Location.Y - p.Y;
                int rx = (dx - dy) * 44;
                int ry = (dx + dy) * 44;

                bool eastToWest;

                if (rx >= 0 && ry >= 0)
                {
                    eastToWest = false;
                }
                else if (rx >= 0)
                {
                    eastToWest = true;
                }
                else if (ry >= 0)
                {
                    eastToWest = true;
                }
                else
                {
                    eastToWest = false;
                }

                Effects.PlaySound(p, Caster.Map, 0x20B);

                int itemID = eastToWest ? 0x3915 : 0x3922;

                TimeSpan duration = TimeSpan.FromSeconds(3 + (Caster.Skills.Magery.Fixed / 25));

                for (int i = -2; i <= 2; ++i)
                {
                    Point3D loc = new Point3D(eastToWest ? p.X + i : p.X, eastToWest ? p.Y : p.Y + i, p.Z);

                    new InternalItem(itemID, loc, Caster, Caster.Map, duration, i);
                }
            }

            FinishSequence();
        }

        [DispellableField]
        public class InternalItem : Item
        {
            private Timer m_Timer;
            private DateTime m_End;
            private Mobile m_Caster;

            public override bool BlocksFit { get { return true; } }

            public InternalItem(int itemID, Point3D loc, Mobile caster, Map map, TimeSpan duration, int val)
                : base(itemID)
            {
                bool canFit = SpellHelper.AdjustField(ref loc, map, 12, false);

                Visible = false;
                Movable = false;
                Light = LightType.Circle300;

                MoveToWorld(loc, map);

                m_Caster = caster;

                m_End = DateTime.UtcNow + duration;

                m_Timer = new InternalTimer(this, TimeSpan.FromSeconds(Math.Abs(val) * 0.2), caster.InLOS(this), canFit);
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

            public override void Serialize(GenericWriter writer)
            {
                base.Serialize(writer);

                writer.Write((int)1); // version

                writer.Write(m_Caster);
                writer.WriteDeltaTime(m_End);
            }

            public override void Deserialize(GenericReader reader)
            {
                base.Deserialize(reader);

                int version = reader.ReadInt();

                switch (version)
                {
                    case 1:
                        {
                            m_Caster = reader.ReadMobile();

                            goto case 0;
                        }
                    case 0:
                        {
                            m_End = reader.ReadDeltaTime();

                            m_Timer = new InternalTimer(this, TimeSpan.Zero, true, true);
                            m_Timer.Start();

                            break;
                        }
                }
            }

            public void ApplyPoisonTo(Mobile m)
            {
                if (m_Caster == null)
                    return;

                Poison p;

                if (Core.AOS)
                {
                    int total = (m_Caster.Skills.Magery.Fixed + m_Caster.Skills.Poisoning.Fixed) / 2;

                    if (total >= 1000)
                        p = Poison.Deadly;
                    else if (total > 850)
                        p = Poison.Greater;
                    else if (total > 650)
                        p = Poison.Regular;
                    else
                        p = Poison.Lesser;
                }
                else
                {
                    p = Poison.Regular;
                }

                if (m.ApplyPoison(m_Caster, p) == ApplyPoisonResult.Poisoned)
                    if (SpellHelper.CanRevealCaster(m))
                        m_Caster.RevealingAction();

                if (m is BaseCreature)
                    ((BaseCreature)m).OnHarmfulSpell(m_Caster);
            }

            public override bool OnMoveOver(Mobile m)
            {
                if (Visible && m_Caster != null && (!Core.AOS || m != m_Caster) && SpellHelper.ValidIndirectTarget(m_Caster, m) && m_Caster.CanBeHarmful(m, false))
                {
                    m_Caster.DoHarmful(m);

                    ApplyPoisonTo(m);
                    m.PlaySound(0x474);
                }

                return true;
            }

            private class InternalTimer : Timer
            {
                private InternalItem m_Item;
                private bool m_InLOS, m_CanFit;

                private static Queue m_Queue = new Queue();

                public InternalTimer(InternalItem item, TimeSpan delay, bool inLOS, bool canFit)
                    : base(delay, TimeSpan.FromSeconds(1.5))
                {
                    m_Item = item;
                    m_InLOS = inLOS;
                    m_CanFit = canFit;

                    Priority = TimerPriority.FiftyMS;
                }

                protected override void OnTick()
                {
                    if (m_Item.Deleted)
                        return;

                    if (!m_Item.Visible)
                    {
                        if (m_InLOS && m_CanFit)
                            m_Item.Visible = true;
                        else
                            m_Item.Delete();

                        if (!m_Item.Deleted)
                        {
                            m_Item.ProcessDelta();
                            Effects.SendLocationParticles(EffectItem.Create(m_Item.Location, m_Item.Map, EffectItem.DefaultDuration), 0x376A, 9, 10, 5040);
                        }
                    }
                    else if (DateTime.UtcNow > m_Item.m_End)
                    {
                        m_Item.Delete();
                        Stop();
                    }
                    else
                    {
                        Map map = m_Item.Map;
                        Mobile caster = m_Item.m_Caster;

                        if (map != null && caster != null)
                        {
                            bool eastToWest = (m_Item.ItemID == 0x3915);
                            IPooledEnumerable eable = map.GetMobilesInBounds(new Rectangle2D(m_Item.X - (eastToWest ? 0 : 1), m_Item.Y - (eastToWest ? 1 : 0), (eastToWest ? 1 : 2), (eastToWest ? 2 : 1)));

                            foreach (Mobile m in eable)
                            {
                                if ((m.Z + 16) > m_Item.Z && (m_Item.Z + 12) > m.Z && (!Core.AOS || m != caster) && SpellHelper.ValidIndirectTarget(caster, m) && caster.CanBeHarmful(m, false))
                                    m_Queue.Enqueue(m);
                            }

                            eable.Free();

                            while (m_Queue.Count > 0)
                            {
                                Mobile m = (Mobile)m_Queue.Dequeue();

                                caster.DoHarmful(m);

                                m_Item.ApplyPoisonTo(m);
                                m.PlaySound(0x474);
                            }
                        }
                    }
                }
            }
        }

        private class InternalTarget : Target
        {
            private PoisonFieldSpell m_Owner;

            public InternalTarget(PoisonFieldSpell owner)
                : base(Core.ML ? 10 : 12, true, TargetFlags.None)
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

    public class SummonCreatureSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Summon Creature", "Kal Xen",
                16,
                false,
                Reagent.Bloodmoss,
                Reagent.MandrakeRoot,
                Reagent.SpidersSilk
            );

        public override SpellCircle Circle { get { return SpellCircle.Fifth; } }

        public SummonCreatureSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        // NOTE: Creature list based on 1hr of summon/release on OSI.

        private static Type[] m_Types = new Type[]
			{
				typeof( PolarBear ),
				typeof( GrizzlyBear ),
				typeof( BlackBear ),
				typeof( Horse ),
				typeof( Walrus ),
				typeof( Chicken ),
				typeof( Scorpion ),
				typeof( GiantSerpent ),
				typeof( Llama ),
				typeof( Alligator ),
				typeof( GreyWolf ),
				typeof( Slime ),
				typeof( Eagle ),
				typeof( Gorilla ),
				typeof( SnowLeopard ),
				typeof( Pig ),
				typeof( Hind ),
				typeof( Rabbit )
			};

        public override bool CheckCast()
        {
            if (!base.CheckCast())
                return false;

            if ((Caster.Followers + 2) > Caster.FollowersMax)
            {
                Caster.SendLocalizedMessage(1049645); // You have too many followers to summon that creature.
                return false;
            }

            return true;
        }

        public override void OnCast()
        {
            if (CheckSequence())
            {
                try
                {
                    BaseCreature creature = (BaseCreature)Activator.CreateInstance(m_Types[Utility.Random(m_Types.Length)]);

                    //creature.ControlSlots = 2;

                    TimeSpan duration;

                    if (Core.AOS)
                        duration = TimeSpan.FromSeconds((2 * Caster.Skills.Magery.Fixed) / 5);
                    else
                        duration = TimeSpan.FromSeconds(4.0 * Caster.Skills[SkillName.Magery].Value);

                    SpellHelper.Summon(creature, Caster, 0x215, duration, false, false);
                }
                catch
                {
                }
            }

            FinishSequence();
        }

        public override TimeSpan GetCastDelay()
        {
            if (Core.AOS)
                return TimeSpan.FromTicks(base.GetCastDelay().Ticks * 5);

            return base.GetCastDelay() + TimeSpan.FromSeconds(6.0);
        }
    }
}

namespace Server.Spells.Sixth
{
    public class DispelSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Dispel", "An Ort",
                218,
                9002,
                Reagent.Garlic,
                Reagent.MandrakeRoot,
                Reagent.SulfurousAsh
            );

        public override SpellCircle Circle { get { return SpellCircle.Sixth; } }

        public DispelSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public class InternalTarget : Target
        {
            private DispelSpell m_Owner;

            public InternalTarget(DispelSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Harmful)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                {
                    Mobile m = (Mobile)o;
                    BaseCreature bc = m as BaseCreature;

                    if (!from.CanSee(m))
                    {
                        from.SendLocalizedMessage(500237); // Target can not be seen.
                    }
                    else if (bc == null || !bc.IsDispellable)
                    {
                        from.SendLocalizedMessage(1005049); // That cannot be dispelled.
                    }
                    else if (m_Owner.CheckHSequence(m))
                    {
                        SpellHelper.Turn(from, m);

                        double dispelChance = (50.0 + ((100 * (from.Skills.Magery.Value - bc.DispelDifficulty)) / (bc.DispelFocus * 2))) / 100;

                        if (dispelChance > Utility.RandomDouble())
                        {
                            Effects.SendLocationParticles(EffectItem.Create(m.Location, m.Map, EffectItem.DefaultDuration), 0x3728, 8, 20, 5042);
                            Effects.PlaySound(m, m.Map, 0x201);

                            m.Delete();
                        }
                        else
                        {
                            m.FixedEffect(0x3779, 10, 20);
                            from.SendLocalizedMessage(1010084); // The creature resisted the attempt to dispel it!
                        }
                    }
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class EnergyBoltSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Energy Bolt", "Corp Por",
                230,
                9022,
                Reagent.BlackPearl,
                Reagent.Nightshade
            );

        public override SpellCircle Circle { get { return SpellCircle.Sixth; } }

        public EnergyBoltSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public override bool DelayedDamage { get { return true; } }

        public void Target(Mobile m)
        {
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckHSequence(m))
            {
                Mobile source = Caster;

                SpellHelper.Turn(Caster, m);

                SpellHelper.CheckReflect((int)this.Circle, ref source, ref m);

                double damage;

                if (Core.AOS)
                {
                    damage = GetNewAosDamage(40, 1, 5, m);
                }
                else
                {
                    damage = Utility.Random(24, 18);

                    if (CheckResisted(m))
                    {
                        damage *= 0.75;

                        m.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
                    }

                    // Scale damage based on evalint and resist
                    damage *= GetDamageScalar(m);
                }

                // Do the effects
                source.MovingParticles(m, 0x379F, 7, 0, false, true, 3043, 4043, 0x211);
                source.PlaySound(0x20A);

                // Deal the damage
                SpellHelper.Damage(this, m, damage, 0, 0, 0, 0, 100);
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private EnergyBoltSpell m_Owner;

            public InternalTarget(EnergyBoltSpell owner)
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

    public class ExplosionSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Explosion", "Vas Ort Flam",
                230,
                9041,
                Reagent.Bloodmoss,
                Reagent.MandrakeRoot
            );

        public override SpellCircle Circle { get { return SpellCircle.Sixth; } }

        public ExplosionSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override bool DelayedDamageStacking { get { return !Core.AOS; } }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public override bool DelayedDamage { get { return false; } }

        public void Target(Mobile m)
        {
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (Caster.CanBeHarmful(m) && CheckSequence())
            {
                Mobile attacker = Caster, defender = m;

                SpellHelper.Turn(Caster, m);

                SpellHelper.CheckReflect((int)this.Circle, Caster, ref m);

                InternalTimer t = new InternalTimer(this, attacker, defender, m);
                t.Start();
            }

            FinishSequence();
        }

        private class InternalTimer : Timer
        {
            private MagerySpell m_Spell;
            private Mobile m_Target;
            private Mobile m_Attacker, m_Defender;

            public InternalTimer(MagerySpell spell, Mobile attacker, Mobile defender, Mobile target)
                : base(TimeSpan.FromSeconds(Core.AOS ? 3.0 : 2.5))
            {
                m_Spell = spell;
                m_Attacker = attacker;
                m_Defender = defender;
                m_Target = target;

                if (m_Spell != null)
                    m_Spell.StartDelayedDamageContext(attacker, this);

                Priority = TimerPriority.FiftyMS;
            }

            protected override void OnTick()
            {
                if (m_Attacker.HarmfulCheck(m_Defender))
                {
                    double damage;

                    if (Core.AOS)
                    {
                        damage = m_Spell.GetNewAosDamage(40, 1, 5, m_Defender);
                    }
                    else
                    {
                        damage = Utility.Random(23, 22);

                        if (m_Spell.CheckResisted(m_Target))
                        {
                            damage *= 0.75;

                            m_Target.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
                        }

                        damage *= m_Spell.GetDamageScalar(m_Target);
                    }

                    m_Target.FixedParticles(0x36BD, 20, 10, 5044, EffectLayer.Head);
                    m_Target.PlaySound(0x307);

                    SpellHelper.Damage(m_Spell, m_Target, damage, 0, 100, 0, 0, 0);

                    if (m_Spell != null)
                        m_Spell.RemoveDelayedDamageContext(m_Attacker);
                }
            }
        }

        private class InternalTarget : Target
        {
            private ExplosionSpell m_Owner;

            public InternalTarget(ExplosionSpell owner)
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

    public class InvisibilitySpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Invisibility", "An Lor Xen",
                206,
                9002,
                Reagent.Bloodmoss,
                Reagent.Nightshade
            );

        public override SpellCircle Circle { get { return SpellCircle.Sixth; } }

        public InvisibilitySpell(Mobile caster, Item scroll)
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
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (m is Mobiles.BaseVendor || m is Mobiles.PlayerVendor || m is Mobiles.PlayerBarkeeper || m.AccessLevel > Caster.AccessLevel)
            {
                Caster.SendLocalizedMessage(501857); // This spell won't work on that!
            }
            else if (CheckBSequence(m))
            {
                SpellHelper.Turn(Caster, m);

                Effects.SendLocationParticles(EffectItem.Create(new Point3D(m.X, m.Y, m.Z + 16), Caster.Map, EffectItem.DefaultDuration), 0x376A, 10, 15, 5045);
                m.PlaySound(0x3C4);

                m.Hidden = true;
                m.Combatant = null;
                m.Warmode = false;

                RemoveTimer(m);

                TimeSpan duration = TimeSpan.FromSeconds(((1.2 * Caster.Skills.Magery.Fixed) / 10));

                Timer t = new InternalTimer(m, duration);

                BuffInfo.RemoveBuff(m, BuffIcon.HidingAndOrStealth);
                BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.Invisibility, 1075825, duration, m));	//Invisibility/Invisible

                m_Table[m] = t;

                t.Start();
            }

            FinishSequence();
        }

        private static Dictionary<Mobile, Timer> m_Table = new Dictionary<Mobile, Timer>();

        public static bool HasTimer(Mobile m)
        {
            return m_Table.ContainsKey(m);
        }

        public static void RemoveTimer(Mobile m)
        {
            Timer t = null;
            m_Table.TryGetValue(m, out t);

            if (t != null)
            {
                t.Stop();
                m_Table.Remove(m);
            }
        }

        private class InternalTimer : Timer
        {
            private Mobile m_Mobile;

            public InternalTimer(Mobile m, TimeSpan duration)
                : base(duration)
            {
                Priority = TimerPriority.OneSecond;
                m_Mobile = m;
            }

            protected override void OnTick()
            {
                m_Mobile.RevealingAction();
                RemoveTimer(m_Mobile);
            }
        }

        public class InternalTarget : Target
        {
            private InvisibilitySpell m_Owner;

            public InternalTarget(InvisibilitySpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Beneficial)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                {
                    m_Owner.Target((Mobile)o);
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class MarkSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Mark", "Kal Por Ylem",
                218,
                9002,
                Reagent.BlackPearl,
                Reagent.Bloodmoss,
                Reagent.MandrakeRoot
            );

        public override SpellCircle Circle { get { return SpellCircle.Sixth; } }

        public MarkSpell(Mobile caster, Item scroll)
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

            return SpellHelper.CheckTravel(Caster, TravelCheckType.Mark);
        }

        public void Target(RecallRune rune)
        {
            if (!Caster.CanSee(rune))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (!SpellHelper.CheckTravel(Caster, TravelCheckType.Mark))
            {
            }
            else if (SpellHelper.CheckMulti(Caster.Location, Caster.Map, !Core.AOS))
            {
                Caster.SendLocalizedMessage(501942); // That location is blocked.
            }
            else if (!rune.IsChildOf(Caster.Backpack))
            {
                Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1062422); // You must have this rune in your backpack in order to mark it.
            }
            else if (CheckSequence())
            {
                rune.Mark(Caster);

                Caster.PlaySound(0x1FA);
                Effects.SendLocationEffect(Caster, Caster.Map, 14201, 16);
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private MarkSpell m_Owner;

            public InternalTarget(MarkSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.None)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is RecallRune)
                {
                    m_Owner.Target((RecallRune)o);
                }
                else
                {
                    from.Send(new MessageLocalized(from.Serial, from.Body, MessageType.Regular, 0x3B2, 3, 501797, from.Name, "")); // I cannot mark that object.
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class MassCurseSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Mass Curse", "Vas Des Sanct",
                218,
                9031,
                false,
                Reagent.Garlic,
                Reagent.Nightshade,
                Reagent.MandrakeRoot,
                Reagent.SulfurousAsh
            );

        public override SpellCircle Circle { get { return SpellCircle.Sixth; } }

        public MassCurseSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(IPoint3D p)
        {
            if (!Caster.CanSee(p))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (SpellHelper.CheckTown(p, Caster) && CheckSequence())
            {
                SpellHelper.Turn(Caster, p);

                SpellHelper.GetSurfaceTop(ref p);

                List<Mobile> targets = new List<Mobile>();

                Map map = Caster.Map;

                if (map != null)
                {
                    IPooledEnumerable eable = map.GetMobilesInRange(new Point3D(p), 2);

                    foreach (Mobile m in eable)
                    {
                        if (Core.AOS && m == Caster)
                            continue;

                        if (SpellHelper.ValidIndirectTarget(Caster, m) && Caster.CanSee(m) && Caster.CanBeHarmful(m, false))
                            targets.Add(m);
                    }

                    eable.Free();
                }

                for (int i = 0; i < targets.Count; ++i)
                {
                    Mobile m = targets[i];

                    Caster.DoHarmful(m);

                    SpellHelper.AddStatCurse(Caster, m, StatType.Str); SpellHelper.DisableSkillCheck = true;
                    SpellHelper.AddStatCurse(Caster, m, StatType.Dex);
                    SpellHelper.AddStatCurse(Caster, m, StatType.Int); SpellHelper.DisableSkillCheck = false;

                    m.FixedParticles(0x374A, 10, 15, 5028, EffectLayer.Waist);
                    m.PlaySound(0x1FB);

                    HarmfulSpell(m);
                }
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private MassCurseSpell m_Owner;

            public InternalTarget(MassCurseSpell owner)
                : base(Core.ML ? 10 : 12, true, TargetFlags.None)
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

    public class ParalyzeFieldSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Paralyze Field", "In Ex Grav",
                230,
                9012,
                false,
                Reagent.BlackPearl,
                Reagent.Ginseng,
                Reagent.SpidersSilk
            );

        public override SpellCircle Circle { get { return SpellCircle.Sixth; } }

        public ParalyzeFieldSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(IPoint3D p)
        {
            if (!Caster.CanSee(p))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (SpellHelper.CheckTown(p, Caster) && CheckSequence())
            {
                SpellHelper.Turn(Caster, p);

                SpellHelper.GetSurfaceTop(ref p);

                int dx = Caster.Location.X - p.X;
                int dy = Caster.Location.Y - p.Y;
                int rx = (dx - dy) * 44;
                int ry = (dx + dy) * 44;

                bool eastToWest;

                if (rx >= 0 && ry >= 0)
                    eastToWest = false;
                else if (rx >= 0)
                    eastToWest = true;
                else if (ry >= 0)
                    eastToWest = true;
                else
                    eastToWest = false;

                Effects.PlaySound(p, Caster.Map, 0x20B);

                int itemID = eastToWest ? 0x3967 : 0x3979;

                TimeSpan duration = TimeSpan.FromSeconds(3.0 + (Caster.Skills[SkillName.Magery].Value / 3.0));

                for (int i = -2; i <= 2; ++i)
                {
                    Point3D loc = new Point3D(eastToWest ? p.X + i : p.X, eastToWest ? p.Y : p.Y + i, p.Z);
                    bool canFit = SpellHelper.AdjustField(ref loc, Caster.Map, 12, false);

                    if (!canFit)
                        continue;

                    Item item = new InternalItem(Caster, itemID, loc, Caster.Map, duration);
                    item.ProcessDelta();

                    Effects.SendLocationParticles(EffectItem.Create(loc, Caster.Map, EffectItem.DefaultDuration), 0x376A, 9, 10, 5048);
                }
            }

            FinishSequence();
        }

        [DispellableField]
        public class InternalItem : Item
        {
            private Timer m_Timer;
            private Mobile m_Caster;
            private DateTime m_End;

            public override bool BlocksFit { get { return true; } }

            public InternalItem(Mobile caster, int itemID, Point3D loc, Map map, TimeSpan duration)
                : base(itemID)
            {
                Visible = false;
                Movable = false;
                Light = LightType.Circle300;

                MoveToWorld(loc, map);

                if (caster.InLOS(this))
                    Visible = true;
                else
                    Delete();

                if (Deleted)
                    return;

                m_Caster = caster;

                m_Timer = new InternalTimer(this, duration);
                m_Timer.Start();

                m_End = DateTime.UtcNow + duration;
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

            public override void Serialize(GenericWriter writer)
            {
                base.Serialize(writer);

                writer.Write((int)0); // version

                writer.Write(m_Caster);
                writer.WriteDeltaTime(m_End);
            }

            public override void Deserialize(GenericReader reader)
            {
                base.Deserialize(reader);

                int version = reader.ReadInt();

                switch (version)
                {
                    case 0:
                        {
                            m_Caster = reader.ReadMobile();
                            m_End = reader.ReadDeltaTime();

                            m_Timer = new InternalTimer(this, m_End - DateTime.UtcNow);
                            m_Timer.Start();

                            break;
                        }
                }
            }

            public override bool OnMoveOver(Mobile m)
            {
                if (Visible && m_Caster != null && (!Core.AOS || m != m_Caster) && SpellHelper.ValidIndirectTarget(m_Caster, m) && m_Caster.CanBeHarmful(m, false))
                {
                    if (SpellHelper.CanRevealCaster(m))
                        m_Caster.RevealingAction();

                    m_Caster.DoHarmful(m);

                    double duration;

                    if (Core.AOS)
                    {
                        duration = 2.0 + ((int)(m_Caster.Skills[SkillName.EvalInt].Value / 10) - (int)(m.Skills[SkillName.MagicResist].Value / 10));

                        if (!m.Player)
                            duration *= 3.0;

                        if (duration < 0.0)
                            duration = 0.0;
                    }
                    else
                    {
                        duration = 7.0 + (m_Caster.Skills[SkillName.Magery].Value * 0.2);
                    }

                    m.Paralyze(TimeSpan.FromSeconds(duration));

                    m.PlaySound(0x204);
                    m.FixedEffect(0x376A, 10, 16);

                    if (m is BaseCreature)
                        ((BaseCreature)m).OnHarmfulSpell(m_Caster);
                }

                return true;
            }

            private class InternalTimer : Timer
            {
                private Item m_Item;

                public InternalTimer(Item item, TimeSpan duration)
                    : base(duration)
                {
                    Priority = TimerPriority.OneSecond;
                    m_Item = item;
                }

                protected override void OnTick()
                {
                    m_Item.Delete();
                }
            }
        }

        private class InternalTarget : Target
        {
            private ParalyzeFieldSpell m_Owner;

            public InternalTarget(ParalyzeFieldSpell owner)
                : base(Core.ML ? 10 : 12, true, TargetFlags.None)
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

    public class RevealSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Reveal", "Wis Quas",
                206,
                9002,
                Reagent.Bloodmoss,
                Reagent.SulfurousAsh
            );

        public override SpellCircle Circle { get { return SpellCircle.Sixth; } }

        public RevealSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(IPoint3D p)
        {
            if (!Caster.CanSee(p))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckSequence())
            {
                SpellHelper.Turn(Caster, p);

                SpellHelper.GetSurfaceTop(ref p);

                List<Mobile> targets = new List<Mobile>();

                Map map = Caster.Map;

                if (map != null)
                {
                    IPooledEnumerable eable = map.GetMobilesInRange(new Point3D(p), 1 + (int)(Caster.Skills[SkillName.Magery].Value / 20.0));

                    foreach (Mobile m in eable)
                    {
                        if (m is Mobiles.ShadowKnight && (m.X != p.X || m.Y != p.Y))
                            continue;

                        if (m.Hidden && (m.AccessLevel == AccessLevel.Player || Caster.AccessLevel > m.AccessLevel) && CheckDifficulty(Caster, m))
                            targets.Add(m);
                    }

                    eable.Free();
                }

                for (int i = 0; i < targets.Count; ++i)
                {
                    Mobile m = targets[i];

                    m.RevealingAction();

                    m.FixedParticles(0x375A, 9, 20, 5049, EffectLayer.Head);
                    m.PlaySound(0x1FD);
                }
            }

            FinishSequence();
        }

        // Reveal uses magery and detect hidden vs. hide and stealth 
        private static bool CheckDifficulty(Mobile from, Mobile m)
        {
            // Reveal always reveals vs. invisibility spell 
            if (!Core.AOS || InvisibilitySpell.HasTimer(m))
                return true;

            int magery = from.Skills[SkillName.Magery].Fixed;
            int detectHidden = from.Skills[SkillName.DetectHidden].Fixed;

            int hiding = m.Skills[SkillName.Hiding].Fixed;
            int stealth = m.Skills[SkillName.Stealth].Fixed;
            int divisor = hiding + stealth;

            int chance;
            if (divisor > 0)
                chance = 50 * (magery + detectHidden) / divisor;
            else
                chance = 100;

            return chance > Utility.Random(100);
        }

        public class InternalTarget : Target
        {
            private RevealSpell m_Owner;

            public InternalTarget(RevealSpell owner)
                : base(Core.ML ? 10 : 12, true, TargetFlags.None)
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
}

namespace Server.Spells.Seventh
{
    public class ChainLightningSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Chain Lightning", "Vas Ort Grav",
                209,
                9022,
                false,
                Reagent.BlackPearl,
                Reagent.Bloodmoss,
                Reagent.MandrakeRoot,
                Reagent.SulfurousAsh
            );

        public override SpellCircle Circle { get { return SpellCircle.Seventh; } }

        public ChainLightningSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public override bool DelayedDamage { get { return true; } }

        public void Target(IPoint3D p)
        {
            if (!Caster.CanSee(p))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (SpellHelper.CheckTown(p, Caster) && CheckSequence())
            {
                SpellHelper.Turn(Caster, p);

                if (p is Item)
                    p = ((Item)p).GetWorldLocation();

                List<Mobile> targets = new List<Mobile>();

                Map map = Caster.Map;

                bool playerVsPlayer = false;

                if (map != null)
                {
                    IPooledEnumerable eable = map.GetMobilesInRange(new Point3D(p), 2);

                    foreach (Mobile m in eable)
                    {
                        if (Core.AOS && m == Caster)
                            continue;

                        if (SpellHelper.ValidIndirectTarget(Caster, m) && Caster.CanBeHarmful(m, false))
                        {
                            if (Core.AOS && !Caster.InLOS(m))
                                continue;

                            targets.Add(m);

                            if (m.Player)
                                playerVsPlayer = true;
                        }
                    }

                    eable.Free();
                }

                double damage;

                if (Core.AOS)
                    damage = GetNewAosDamage(51, 1, 5, playerVsPlayer);
                else
                    damage = Utility.Random(27, 22);

                if (targets.Count > 0)
                {
                    if (Core.AOS && targets.Count > 2)
                        damage = (damage * 2) / targets.Count;
                    else if (!Core.AOS)
                        damage /= targets.Count;

                    double toDeal;
                    for (int i = 0; i < targets.Count; ++i)
                    {
                        toDeal = damage;
                        Mobile m = targets[i];

                        if (!Core.AOS && CheckResisted(m))
                        {
                            toDeal *= 0.5;

                            m.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
                        }
                        toDeal *= GetDamageScalar(m);
                        Caster.DoHarmful(m);
                        SpellHelper.Damage(this, m, toDeal, 0, 0, 0, 0, 100);

                        m.BoltEffect(0);
                    }
                }
                else
                {
                    Caster.PlaySound(0x29);
                }
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private ChainLightningSpell m_Owner;

            public InternalTarget(ChainLightningSpell owner)
                : base(Core.ML ? 10 : 12, true, TargetFlags.None)
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

    public class EnergyFieldSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Energy Field", "In Sanct Grav",
                221,
                9022,
                false,
                Reagent.BlackPearl,
                Reagent.MandrakeRoot,
                Reagent.SpidersSilk,
                Reagent.SulfurousAsh
            );

        public override SpellCircle Circle { get { return SpellCircle.Seventh; } }

        public EnergyFieldSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(IPoint3D p)
        {
            if (!Caster.CanSee(p))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (SpellHelper.CheckTown(p, Caster) && CheckSequence())
            {
                SpellHelper.Turn(Caster, p);

                SpellHelper.GetSurfaceTop(ref p);

                int dx = Caster.Location.X - p.X;
                int dy = Caster.Location.Y - p.Y;
                int rx = (dx - dy) * 44;
                int ry = (dx + dy) * 44;

                bool eastToWest;

                if (rx >= 0 && ry >= 0)
                {
                    eastToWest = false;
                }
                else if (rx >= 0)
                {
                    eastToWest = true;
                }
                else if (ry >= 0)
                {
                    eastToWest = true;
                }
                else
                {
                    eastToWest = false;
                }

                Effects.PlaySound(p, Caster.Map, 0x20B);

                TimeSpan duration;

                if (Core.AOS)
                    duration = TimeSpan.FromSeconds((15 + (Caster.Skills.Magery.Fixed / 5)) / 7);
                else
                    duration = TimeSpan.FromSeconds(Caster.Skills[SkillName.Magery].Value * 0.28 + 2.0); // (28% of magery) + 2.0 seconds

                int itemID = eastToWest ? 0x3946 : 0x3956;

                for (int i = -2; i <= 2; ++i)
                {
                    Point3D loc = new Point3D(eastToWest ? p.X + i : p.X, eastToWest ? p.Y : p.Y + i, p.Z);
                    bool canFit = SpellHelper.AdjustField(ref loc, Caster.Map, 12, false);

                    if (!canFit)
                        continue;

                    Item item = new InternalItem(loc, Caster.Map, duration, itemID, Caster);
                    item.ProcessDelta();

                    Effects.SendLocationParticles(EffectItem.Create(loc, Caster.Map, EffectItem.DefaultDuration), 0x376A, 9, 10, 5051);
                }
            }

            FinishSequence();
        }

        [DispellableField]
        private class InternalItem : Item
        {
            private Timer m_Timer;
            private Mobile m_Caster;

            public override bool BlocksFit { get { return true; } }

            public InternalItem(Point3D loc, Map map, TimeSpan duration, int itemID, Mobile caster)
                : base(itemID)
            {
                Visible = false;
                Movable = false;
                Light = LightType.Circle300;

                MoveToWorld(loc, map);

                m_Caster = caster;

                if (caster.InLOS(this))
                    Visible = true;
                else
                    Delete();

                if (Deleted)
                    return;

                m_Timer = new InternalTimer(this, duration);
                m_Timer.Start();
            }

            public InternalItem(Serial serial)
                : base(serial)
            {
                m_Timer = new InternalTimer(this, TimeSpan.FromSeconds(5.0));
                m_Timer.Start();
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

            public override bool OnMoveOver(Mobile m)
            {
                int noto;

                if (m is PlayerMobile)
                {
                    noto = Notoriety.Compute(m_Caster, m);
                    if (noto == Notoriety.Enemy || noto == Notoriety.Ally)
                        return false;
                }
                return base.OnMoveOver(m);
            }

            public override void OnAfterDelete()
            {
                base.OnAfterDelete();

                if (m_Timer != null)
                    m_Timer.Stop();
            }

            private class InternalTimer : Timer
            {
                private InternalItem m_Item;

                public InternalTimer(InternalItem item, TimeSpan duration)
                    : base(duration)
                {
                    Priority = TimerPriority.OneSecond;
                    m_Item = item;
                }

                protected override void OnTick()
                {
                    m_Item.Delete();
                }
            }
        }

        private class InternalTarget : Target
        {
            private EnergyFieldSpell m_Owner;

            public InternalTarget(EnergyFieldSpell owner)
                : base(Core.ML ? 10 : 12, true, TargetFlags.None)
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

    public class FlameStrikeSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Flame Strike", "Kal Vas Flam",
                245,
                9042,
                Reagent.SpidersSilk,
                Reagent.SulfurousAsh
            );

        public override SpellCircle Circle { get { return SpellCircle.Seventh; } }

        public FlameStrikeSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public override bool DelayedDamage { get { return true; } }

        public void Target(Mobile m)
        {
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckHSequence(m))
            {
                SpellHelper.Turn(Caster, m);

                SpellHelper.CheckReflect((int)this.Circle, Caster, ref m);

                double damage;

                if (Core.AOS)
                {
                    damage = GetNewAosDamage(48, 1, 5, m);
                }
                else
                {
                    damage = Utility.Random(27, 22);

                    if (CheckResisted(m))
                    {
                        damage *= 0.6;

                        m.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
                    }

                    damage *= GetDamageScalar(m);
                }

                m.FixedParticles(0x3709, 10, 30, 5052, EffectLayer.LeftFoot);
                m.PlaySound(0x208);

                SpellHelper.Damage(this, m, damage, 0, 100, 0, 0, 0);
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private FlameStrikeSpell m_Owner;

            public InternalTarget(FlameStrikeSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Harmful)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                {
                    m_Owner.Target((Mobile)o);
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class GateTravelSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Gate Travel", "Vas Rel Por",
                263,
                9032,
                Reagent.BlackPearl,
                Reagent.MandrakeRoot,
                Reagent.SulfurousAsh
            );

        public override SpellCircle Circle { get { return SpellCircle.Seventh; } }

        private RunebookEntry m_Entry;

        public GateTravelSpell(Mobile caster, Item scroll)
            : this(caster, scroll, null)
        {
        }

        public GateTravelSpell(Mobile caster, Item scroll, RunebookEntry entry)
            : base(caster, scroll, m_Info)
        {
            m_Entry = entry;
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
                Caster.SendLocalizedMessage(1005564, "", 0x22); // Wouldst thou flee during the heat of battle??
                return false;
            }

            return SpellHelper.CheckTravel(Caster, TravelCheckType.GateFrom);
        }

        private bool GateExistsAt(Map map, Point3D loc)
        {
            bool _gateFound = false;

            IPooledEnumerable eable = map.GetItemsInRange(loc, 0);
            foreach (Item item in eable)
            {
                if (item is Moongate || item is PublicMoongate)
                {
                    _gateFound = true;
                    break;
                }
            }
            eable.Free();

            return _gateFound;
        }

        public void Effect(Point3D loc, Map map, bool checkMulti)
        {
            if (Factions.Sigil.ExistsOn(Caster))
            {
                Caster.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
            }
            else if (map == null || (!Core.AOS && Caster.Map != map))
            {
                Caster.SendLocalizedMessage(1005570); // You can not gate to another facet.
            }
            else if (!SpellHelper.CheckTravel(Caster, TravelCheckType.GateFrom))
            {
            }
            else if (!SpellHelper.CheckTravel(Caster, map, loc, TravelCheckType.GateTo))
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
                Caster.SendLocalizedMessage(1005564, "", 0x22); // Wouldst thou flee during the heat of battle??
            }
            else if (!map.CanSpawnMobile(loc.X, loc.Y, loc.Z))
            {
                Caster.SendLocalizedMessage(501942); // That location is blocked.
            }
            else if ((checkMulti && SpellHelper.CheckMulti(loc, map)))
            {
                Caster.SendLocalizedMessage(501942); // That location is blocked.
            }
            else if (Core.SE && (GateExistsAt(map, loc) || GateExistsAt(Caster.Map, Caster.Location))) // SE restricted stacking gates
            {
                Caster.SendLocalizedMessage(1071242); // There is already a gate there.
            }
            else if (CheckSequence())
            {
                Caster.SendLocalizedMessage(501024); // You open a magical gate to another location

                Effects.PlaySound(Caster.Location, Caster.Map, 0x20E);

                InternalItem firstGate = new InternalItem(loc, map);
                firstGate.MoveToWorld(Caster.Location, Caster.Map);

                Effects.PlaySound(loc, map, 0x20E);

                InternalItem secondGate = new InternalItem(Caster.Location, Caster.Map);
                secondGate.MoveToWorld(loc, map);
            }

            FinishSequence();
        }

        [DispellableField]
        private class InternalItem : Moongate
        {
            public override bool ShowFeluccaWarning { get { return Core.AOS; } }

            public InternalItem(Point3D target, Map map)
                : base(target, map)
            {
                Map = map;

                if (ShowFeluccaWarning && map == Map.Felucca)
                    ItemID = 0xDDA;

                Dispellable = true;

                InternalTimer t = new InternalTimer(this);
                t.Start();
            }

            public InternalItem(Serial serial)
                : base(serial)
            {
            }

            public override void Serialize(GenericWriter writer)
            {
                base.Serialize(writer);
            }

            public override void Deserialize(GenericReader reader)
            {
                base.Deserialize(reader);

                Delete();
            }

            private class InternalTimer : Timer
            {
                private Item m_Item;

                public InternalTimer(Item item)
                    : base(TimeSpan.FromSeconds(30.0))
                {
                    Priority = TimerPriority.OneSecond;
                    m_Item = item;
                }

                protected override void OnTick()
                {
                    m_Item.Delete();
                }
            }
        }

        private class InternalTarget : Target
        {
            private GateTravelSpell m_Owner;

            public InternalTarget(GateTravelSpell owner)
                : base(12, false, TargetFlags.None)
            {
                m_Owner = owner;

                owner.Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, 501029); // Select Marked item.
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is RecallRune)
                {
                    RecallRune rune = (RecallRune)o;

                    if (rune.Marked)
                        m_Owner.Effect(rune.Target, rune.TargetMap, true);
                    else
                        from.SendLocalizedMessage(501803); // That rune is not yet marked.
                }
                else if (o is Runebook)
                {
                    RunebookEntry e = ((Runebook)o).Default;

                    if (e != null)
                        m_Owner.Effect(e.Location, e.Map, true);
                    else
                        from.SendLocalizedMessage(502354); // Target is not marked.
                }
                /*else if ( o is Key && ((Key)o).KeyValue != 0 && ((Key)o).Link is BaseBoat )
                {
                    BaseBoat boat = ((Key)o).Link as BaseBoat;

                    if ( !boat.Deleted && boat.CheckKey( ((Key)o).KeyValue ) )
                        m_Owner.Effect( boat.GetMarkedLocation(), boat.Map, false );
                    else
                        from.Send( new MessageLocalized( from.Serial, from.Body, MessageType.Regular, 0x3B2, 3, 501030, from.Name, "" ) ); // I can not gate travel from that object.
                }*/
                else if (o is HouseRaffleDeed && ((HouseRaffleDeed)o).ValidLocation())
                {
                    HouseRaffleDeed deed = (HouseRaffleDeed)o;

                    m_Owner.Effect(deed.PlotLocation, deed.PlotFacet, true);
                }
                else
                {
                    from.Send(new MessageLocalized(from.Serial, from.Body, MessageType.Regular, 0x3B2, 3, 501030, from.Name, "")); // I can not gate travel from that object.
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

    public class ManaVampireSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Mana Vampire", "Ort Sanct",
                221,
                9032,
                Reagent.BlackPearl,
                Reagent.Bloodmoss,
                Reagent.MandrakeRoot,
                Reagent.SpidersSilk
            );

        public override SpellCircle Circle { get { return SpellCircle.Seventh; } }

        public ManaVampireSpell(Mobile caster, Item scroll)
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
                SpellHelper.Turn(Caster, m);

                SpellHelper.CheckReflect((int)this.Circle, Caster, ref m);

                if (m.Spell != null)
                    m.Spell.OnCasterHurt();

                m.Paralyzed = false;

                int toDrain = 0;

                if (Core.AOS)
                {
                    toDrain = (int)(GetDamageSkill(Caster) - GetResistSkill(m));

                    if (!m.Player)
                        toDrain /= 2;

                    if (toDrain < 0)
                        toDrain = 0;
                    else if (toDrain > m.Mana)
                        toDrain = m.Mana;
                }
                else
                {
                    if (CheckResisted(m))
                        m.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
                    else
                        toDrain = m.Mana;
                }

                if (toDrain > (Caster.ManaMax - Caster.Mana))
                    toDrain = Caster.ManaMax - Caster.Mana;

                m.Mana -= toDrain;
                Caster.Mana += toDrain;

                if (Core.AOS)
                {
                    m.FixedParticles(0x374A, 1, 15, 5054, 23, 7, EffectLayer.Head);
                    m.PlaySound(0x1F9);

                    Caster.FixedParticles(0x0000, 10, 5, 2054, EffectLayer.Head);
                }
                else
                {
                    m.FixedParticles(0x374A, 10, 15, 5054, EffectLayer.Head);
                    m.PlaySound(0x1F9);
                }

                HarmfulSpell(m);
            }

            FinishSequence();
        }

        public override double GetResistPercent(Mobile target)
        {
            return 98.0;
        }

        private class InternalTarget : Target
        {
            private ManaVampireSpell m_Owner;

            public InternalTarget(ManaVampireSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.Harmful)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                {
                    m_Owner.Target((Mobile)o);
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class MassDispelSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Mass Dispel", "Vas An Ort",
                263,
                9002,
                Reagent.Garlic,
                Reagent.MandrakeRoot,
                Reagent.BlackPearl,
                Reagent.SulfurousAsh
            );

        public override SpellCircle Circle { get { return SpellCircle.Seventh; } }

        public MassDispelSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public void Target(IPoint3D p)
        {
            if (!Caster.CanSee(p))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckSequence())
            {
                SpellHelper.Turn(Caster, p);

                SpellHelper.GetSurfaceTop(ref p);

                List<Mobile> targets = new List<Mobile>();

                Map map = Caster.Map;

                if (map != null)
                {
                    IPooledEnumerable eable = map.GetMobilesInRange(new Point3D(p), 8);

                    foreach (Mobile m in eable)
                        if (m is BaseCreature && (m as BaseCreature).IsDispellable && Caster.CanBeHarmful(m, false))
                            targets.Add(m);

                    eable.Free();
                }

                for (int i = 0; i < targets.Count; ++i)
                {
                    Mobile m = targets[i];

                    BaseCreature bc = m as BaseCreature;

                    if (bc == null)
                        continue;

                    double dispelChance = (50.0 + ((100 * (Caster.Skills.Magery.Value - bc.DispelDifficulty)) / (bc.DispelFocus * 2))) / 100;

                    if (dispelChance > Utility.RandomDouble())
                    {
                        Effects.SendLocationParticles(EffectItem.Create(m.Location, m.Map, EffectItem.DefaultDuration), 0x3728, 8, 20, 5042);
                        Effects.PlaySound(m, m.Map, 0x201);

                        m.Delete();
                    }
                    else
                    {
                        Caster.DoHarmful(m);

                        m.FixedEffect(0x3779, 10, 20);
                    }
                }
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private MassDispelSpell m_Owner;

            public InternalTarget(MassDispelSpell owner)
                : base(Core.ML ? 10 : 12, true, TargetFlags.None)
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

    public class MeteorSwarmSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Meteor Swarm", "Flam Kal Des Ylem",
                233,
                9042,
                false,
                Reagent.Bloodmoss,
                Reagent.MandrakeRoot,
                Reagent.SulfurousAsh,
                Reagent.SpidersSilk
            );

        public override SpellCircle Circle { get { return SpellCircle.Seventh; } }

        public MeteorSwarmSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override void OnCast()
        {
            Caster.Target = new InternalTarget(this);
        }

        public override bool DelayedDamage { get { return true; } }

        public void Target(IPoint3D p)
        {
            if (!Caster.CanSee(p))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (SpellHelper.CheckTown(p, Caster) && CheckSequence())
            {
                SpellHelper.Turn(Caster, p);

                if (p is Item)
                    p = ((Item)p).GetWorldLocation();

                List<Mobile> targets = new List<Mobile>();

                Map map = Caster.Map;

                bool playerVsPlayer = false;

                if (map != null)
                {
                    IPooledEnumerable eable = map.GetMobilesInRange(new Point3D(p), 2);

                    foreach (Mobile m in eable)
                    {
                        if (Caster != m && SpellHelper.ValidIndirectTarget(Caster, m) && Caster.CanBeHarmful(m, false))
                        {
                            if (Core.AOS && !Caster.InLOS(m))
                                continue;

                            targets.Add(m);

                            if (m.Player)
                                playerVsPlayer = true;
                        }
                    }

                    eable.Free();
                }

                double damage;

                if (Core.AOS)
                    damage = GetNewAosDamage(51, 1, 5, playerVsPlayer);
                else
                    damage = Utility.Random(27, 22);

                if (targets.Count > 0)
                {
                    Effects.PlaySound(p, Caster.Map, 0x160);

                    if (Core.AOS && targets.Count > 2)
                        damage = (damage * 2) / targets.Count;
                    else if (!Core.AOS)
                        damage /= targets.Count;

                    double toDeal;
                    for (int i = 0; i < targets.Count; ++i)
                    {
                        Mobile m = targets[i];

                        toDeal = damage;

                        if (!Core.AOS && CheckResisted(m))
                        {
                            damage *= 0.5;

                            m.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
                        }
                        toDeal *= GetDamageScalar(m);
                        Caster.DoHarmful(m);
                        SpellHelper.Damage(this, m, toDeal, 0, 100, 0, 0, 0);

                        Caster.MovingParticles(m, 0x36D4, 7, 0, false, true, 9501, 1, 0, 0x100);
                    }
                }
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private MeteorSwarmSpell m_Owner;

            public InternalTarget(MeteorSwarmSpell owner)
                : base(Core.ML ? 10 : 12, true, TargetFlags.None)
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

    public class PolymorphSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Polymorph", "Vas Ylem Rel",
                221,
                9002,
                Reagent.Bloodmoss,
                Reagent.SpidersSilk,
                Reagent.MandrakeRoot
            );

        public override SpellCircle Circle { get { return SpellCircle.Seventh; } }

        private int m_NewBody;

        public PolymorphSpell(Mobile caster, Item scroll, int body)
            : base(caster, scroll, m_Info)
        {
            m_NewBody = body;
        }

        public PolymorphSpell(Mobile caster, Item scroll)
            : this(caster, scroll, 0)
        {
        }

        public override bool CheckCast()
        {
            /*if ( Caster.Mounted )
            {
                Caster.SendLocalizedMessage( 1042561 ); //Please dismount first.
                return false;
            }
            else */
            if (Factions.Sigil.ExistsOn(Caster))
            {
                Caster.SendLocalizedMessage(1010521); // You cannot polymorph while you have a Town Sigil
                return false;
            }
            else if (TransformationSpellHelper.UnderTransformation(Caster))
            {
                Caster.SendLocalizedMessage(1061633); // You cannot polymorph while in that form.
                return false;
            }
            else if (DisguiseTimers.IsDisguised(Caster))
            {
                Caster.SendLocalizedMessage(502167); // You cannot polymorph while disguised.
                return false;
            }
            else if (Caster.BodyMod == 183 || Caster.BodyMod == 184)
            {
                Caster.SendLocalizedMessage(1042512); // You cannot polymorph while wearing body paint
                return false;
            }
            else if (!Caster.CanBeginAction(typeof(PolymorphSpell)))
            {
                if (Core.ML)
                    EndPolymorph(Caster);
                else
                    Caster.SendLocalizedMessage(1005559); // This spell is already in effect.
                return false;
            }
            else if (m_NewBody == 0)
            {
                Gump gump;
                if (Core.SE)
                    gump = new NewPolymorphGump(Caster, Scroll);
                else
                    gump = new PolymorphGump(Caster, Scroll);

                Caster.SendGump(gump);
                return false;
            }

            return true;
        }

        public override void OnCast()
        {
            /*if ( Caster.Mounted )
            {
                Caster.SendLocalizedMessage( 1042561 ); //Please dismount first.
            } 
            else */
            if (Factions.Sigil.ExistsOn(Caster))
            {
                Caster.SendLocalizedMessage(1010521); // You cannot polymorph while you have a Town Sigil
            }
            else if (!Caster.CanBeginAction(typeof(PolymorphSpell)))
            {
                if (Core.ML)
                    EndPolymorph(Caster);
                else
                    Caster.SendLocalizedMessage(1005559); // This spell is already in effect.
            }
            else if (TransformationSpellHelper.UnderTransformation(Caster))
            {
                Caster.SendLocalizedMessage(1061633); // You cannot polymorph while in that form.
            }
            else if (DisguiseTimers.IsDisguised(Caster))
            {
                Caster.SendLocalizedMessage(502167); // You cannot polymorph while disguised.
            }
            else if (Caster.BodyMod == 183 || Caster.BodyMod == 184)
            {
                Caster.SendLocalizedMessage(1042512); // You cannot polymorph while wearing body paint
            }
            else if (!Caster.CanBeginAction(typeof(IncognitoSpell)) || Caster.IsBodyMod)
            {
                DoFizzle();
            }
            else if (CheckSequence())
            {
                if (Caster.BeginAction(typeof(PolymorphSpell)))
                {
                    if (m_NewBody != 0)
                    {
                        if (!((Body)m_NewBody).IsHuman)
                        {
                            Mobiles.IMount mt = Caster.Mount;

                            if (mt != null)
                                mt.Rider = null;
                        }

                        Caster.BodyMod = m_NewBody;

                        if (m_NewBody == 400 || m_NewBody == 401)
                            Caster.HueMod = Utility.RandomSkinHue();
                        else
                            Caster.HueMod = 0;

                        BaseArmor.ValidateMobile(Caster);
                        BaseClothing.ValidateMobile(Caster);

                        if (!Core.ML)
                        {
                            StopTimer(Caster);

                            Timer t = new InternalTimer(Caster);

                            m_Timers[Caster] = t;

                            t.Start();
                        }
                    }
                }
                else
                {
                    Caster.SendLocalizedMessage(1005559); // This spell is already in effect.
                }
            }

            FinishSequence();
        }

        private static Hashtable m_Timers = new Hashtable();

        public static bool StopTimer(Mobile m)
        {
            Timer t = (Timer)m_Timers[m];

            if (t != null)
            {
                t.Stop();
                m_Timers.Remove(m);
            }

            return (t != null);
        }

        private static void EndPolymorph(Mobile m)
        {
            if (!m.CanBeginAction(typeof(PolymorphSpell)))
            {
                m.BodyMod = 0;
                m.HueMod = -1;
                m.EndAction(typeof(PolymorphSpell));

                BaseArmor.ValidateMobile(m);
                BaseClothing.ValidateMobile(m);
            }
        }

        private class InternalTimer : Timer
        {
            private Mobile m_Owner;

            public InternalTimer(Mobile owner)
                : base(TimeSpan.FromSeconds(0))
            {
                m_Owner = owner;

                int val = (int)owner.Skills[SkillName.Magery].Value;

                if (val > 120)
                    val = 120;

                Delay = TimeSpan.FromSeconds(val);
                Priority = TimerPriority.OneSecond;
            }

            protected override void OnTick()
            {
                EndPolymorph(m_Owner);
            }
        }
    }
}

namespace Server.Spells.Eighth
{
    public class AirElementalSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Air Elemental", "Kal Vas Xen Hur",
                269,
                9010,
                false,
                Reagent.Bloodmoss,
                Reagent.MandrakeRoot,
                Reagent.SpidersSilk
            );

        public override SpellCircle Circle { get { return SpellCircle.Eighth; } }

        public AirElementalSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override bool CheckCast()
        {
            if (!base.CheckCast())
                return false;

            if ((Caster.Followers + 2) > Caster.FollowersMax)
            {
                Caster.SendLocalizedMessage(1049645); // You have too many followers to summon that creature.
                return false;
            }

            return true;
        }

        public override void OnCast()
        {
            if (CheckSequence())
            {
                TimeSpan duration = TimeSpan.FromSeconds((2 * Caster.Skills.Magery.Fixed) / 5);

                if (Core.AOS)
                    SpellHelper.Summon(new SummonedAirElemental(), Caster, 0x217, duration, false, false);
                else
                    SpellHelper.Summon(new AirElemental(), Caster, 0x217, duration, false, false);
            }

            FinishSequence();
        }
    }

    public class EarthElementalSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Earth Elemental", "Kal Vas Xen Ylem",
                269,
                9020,
                false,
                Reagent.Bloodmoss,
                Reagent.MandrakeRoot,
                Reagent.SpidersSilk
            );

        public override SpellCircle Circle { get { return SpellCircle.Eighth; } }

        public EarthElementalSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override bool CheckCast()
        {
            if (!base.CheckCast())
                return false;

            if ((Caster.Followers + 2) > Caster.FollowersMax)
            {
                Caster.SendLocalizedMessage(1049645); // You have too many followers to summon that creature.
                return false;
            }

            return true;
        }

        public override void OnCast()
        {
            if (CheckSequence())
            {
                TimeSpan duration = TimeSpan.FromSeconds((2 * Caster.Skills.Magery.Fixed) / 5);

                if (Core.AOS)
                    SpellHelper.Summon(new SummonedEarthElemental(), Caster, 0x217, duration, false, false);
                else
                    SpellHelper.Summon(new EarthElemental(), Caster, 0x217, duration, false, false);
            }

            FinishSequence();
        }
    }

    public class EarthquakeSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Earthquake", "In Vas Por",
                233,
                9012,
                false,
                Reagent.Bloodmoss,
                Reagent.Ginseng,
                Reagent.MandrakeRoot,
                Reagent.SulfurousAsh
            );

        public override SpellCircle Circle { get { return SpellCircle.Eighth; } }

        public EarthquakeSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override bool DelayedDamage { get { return !Core.AOS; } }

        public override void OnCast()
        {
            if (SpellHelper.CheckTown(Caster, Caster) && CheckSequence())
            {
                List<Mobile> targets = new List<Mobile>();

                Map map = Caster.Map;

                if (map != null)
                    foreach (Mobile m in Caster.GetMobilesInRange(1 + (int)(Caster.Skills[SkillName.Magery].Value / 15.0)))
                        if (Caster != m && SpellHelper.ValidIndirectTarget(Caster, m) && Caster.CanBeHarmful(m, false) && (!Core.AOS || Caster.InLOS(m)))
                            targets.Add(m);

                Caster.PlaySound(0x220);

                for (int i = 0; i < targets.Count; ++i)
                {
                    Mobile m = targets[i];

                    int damage;

                    if (Core.AOS)
                    {
                        damage = m.Hits / 2;

                        if (!m.Player)
                            damage = Math.Max(Math.Min(damage, 100), 15);
                        damage += Utility.RandomMinMax(0, 15);

                    }
                    else
                    {
                        damage = (m.Hits * 6) / 10;

                        if (!m.Player && damage < 10)
                            damage = 10;
                        else if (damage > 75)
                            damage = 75;
                    }

                    Caster.DoHarmful(m);
                    SpellHelper.Damage(TimeSpan.Zero, m, Caster, damage, 100, 0, 0, 0, 0);
                }
            }

            FinishSequence();
        }
    }

    public class EnergyVortexSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Energy Vortex", "Vas Corp Por",
                260,
                9032,
                false,
                Reagent.Bloodmoss,
                Reagent.BlackPearl,
                Reagent.MandrakeRoot,
                Reagent.Nightshade
            );

        public override SpellCircle Circle { get { return SpellCircle.Eighth; } }

        public EnergyVortexSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override bool CheckCast()
        {
            if (!base.CheckCast())
                return false;

            if ((Caster.Followers + (Core.SE ? 2 : 1)) > Caster.FollowersMax)
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

        public void Target(IPoint3D p)
        {
            Map map = Caster.Map;

            SpellHelper.GetSurfaceTop(ref p);

            if (map == null || !map.CanSpawnMobile(p.X, p.Y, p.Z))
            {
                Caster.SendLocalizedMessage(501942); // That location is blocked.
            }
            else if (SpellHelper.CheckTown(p, Caster) && CheckSequence())
            {
                TimeSpan duration;

                if (Core.AOS)
                    duration = TimeSpan.FromSeconds(90.0);
                else
                    duration = TimeSpan.FromSeconds(Utility.Random(80, 40));

                BaseCreature.Summon(new EnergyVortex(), false, Caster, new Point3D(p), 0x212, duration);
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private EnergyVortexSpell m_Owner;

            public InternalTarget(EnergyVortexSpell owner)
                : base(Core.ML ? 10 : 12, true, TargetFlags.None)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is IPoint3D)
                    m_Owner.Target((IPoint3D)o);
            }

            protected override void OnTargetOutOfLOS(Mobile from, object o)
            {
                from.SendLocalizedMessage(501943); // Target cannot be seen. Try again.
                from.Target = new InternalTarget(m_Owner);
                from.Target.BeginTimeout(from, TimeoutTime - DateTime.UtcNow);
                m_Owner = null;
            }

            protected override void OnTargetFinish(Mobile from)
            {
                if (m_Owner != null)
                    m_Owner.FinishSequence();
            }
        }
    }

    public class FireElementalSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Fire Elemental", "Kal Vas Xen Flam",
                269,
                9050,
                false,
                Reagent.Bloodmoss,
                Reagent.MandrakeRoot,
                Reagent.SpidersSilk,
                Reagent.SulfurousAsh
            );

        public override SpellCircle Circle { get { return SpellCircle.Eighth; } }

        public FireElementalSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override bool CheckCast()
        {
            if (!base.CheckCast())
                return false;

            if ((Caster.Followers + 4) > Caster.FollowersMax)
            {
                Caster.SendLocalizedMessage(1049645); // You have too many followers to summon that creature.
                return false;
            }

            return true;
        }

        public override void OnCast()
        {
            if (CheckSequence())
            {
                TimeSpan duration = TimeSpan.FromSeconds((2 * Caster.Skills.Magery.Fixed) / 5);

                if (Core.AOS)
                    SpellHelper.Summon(new SummonedFireElemental(), Caster, 0x217, duration, false, false);
                else
                    SpellHelper.Summon(new FireElemental(), Caster, 0x217, duration, false, false);
            }

            FinishSequence();
        }
    }

    public class ResurrectionSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Resurrection", "An Corp",
                245,
                9062,
                Reagent.Bloodmoss,
                Reagent.Garlic,
                Reagent.Ginseng
            );

        public override SpellCircle Circle { get { return SpellCircle.Eighth; } }

        public ResurrectionSpell(Mobile caster, Item scroll)
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
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (m == Caster)
            {
                Caster.SendLocalizedMessage(501039); // Thou can not resurrect thyself.
            }
            else if (!Caster.Alive)
            {
                Caster.SendLocalizedMessage(501040); // The resurrecter must be alive.
            }
            else if (m.Alive)
            {
                Caster.SendLocalizedMessage(501041); // Target is not dead.
            }
            else if (!Caster.InRange(m, 1))
            {
                Caster.SendLocalizedMessage(501042); // Target is not close enough.
            }
            else if (!m.Player)
            {
                Caster.SendLocalizedMessage(501043); // Target is not a being.
            }
            else if (m.Map == null || !m.Map.CanFit(m.Location, 16, false, false))
            {
                Caster.SendLocalizedMessage(501042); // Target can not be resurrected at that location.
                m.SendLocalizedMessage(502391); // Thou can not be resurrected there!
            }
            else if (m.Region != null && m.Region.IsPartOf("Khaldun"))
            {
                Caster.SendLocalizedMessage(1010395); // The veil of death in this area is too strong and resists thy efforts to restore life.
            }
            else if (CheckBSequence(m, true))
            {
                SpellHelper.Turn(Caster, m);

                m.PlaySound(0x214);
                m.FixedEffect(0x376A, 10, 16);

                m.CloseGump(typeof(ResurrectGump));
                m.SendGump(new ResurrectGump(m, Caster));
            }

            FinishSequence();
        }

        private class InternalTarget : Target
        {
            private ResurrectionSpell m_Owner;

            public InternalTarget(ResurrectionSpell owner)
                : base(1, false, TargetFlags.Beneficial)
            {
                m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (o is Mobile)
                {
                    m_Owner.Target((Mobile)o);
                }
            }

            protected override void OnTargetFinish(Mobile from)
            {
                m_Owner.FinishSequence();
            }
        }
    }

    public class SummonDaemonSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Summon Daemon", "Kal Vas Xen Corp",
                269,
                9050,
                false,
                Reagent.Bloodmoss,
                Reagent.MandrakeRoot,
                Reagent.SpidersSilk,
                Reagent.SulfurousAsh
            );

        public override SpellCircle Circle { get { return SpellCircle.Eighth; } }

        public SummonDaemonSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override bool CheckCast()
        {
            if (!base.CheckCast())
                return false;

            if ((Caster.Followers + (Core.SE ? 4 : 5)) > Caster.FollowersMax)
            {
                Caster.SendLocalizedMessage(1049645); // You have too many followers to summon that creature.
                return false;
            }

            return true;
        }

        public override void OnCast()
        {
            if (CheckSequence())
            {
                TimeSpan duration = TimeSpan.FromSeconds((2 * Caster.Skills.Magery.Fixed) / 5);

                if (Core.AOS)  /* Why two diff daemons? TODO: solve this */
                {
                    BaseCreature m_Daemon = new SummonedDaemon();
                    SpellHelper.Summon(m_Daemon, Caster, 0x216, duration, false, false);
                    m_Daemon.FixedParticles(0x3728, 8, 20, 5042, EffectLayer.Head);
                }
                else
                    SpellHelper.Summon(new Daemon(), Caster, 0x216, duration, false, false);
            }

            FinishSequence();
        }
    }

    public class WaterElementalSpell : MagerySpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                "Water Elemental", "Kal Vas Xen An Flam",
                269,
                9070,
                false,
                Reagent.Bloodmoss,
                Reagent.MandrakeRoot,
                Reagent.SpidersSilk
            );

        public override SpellCircle Circle { get { return SpellCircle.Eighth; } }

        public WaterElementalSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
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

        public override void OnCast()
        {
            if (CheckSequence())
            {
                TimeSpan duration = TimeSpan.FromSeconds((2 * Caster.Skills.Magery.Fixed) / 5);

                if (Core.AOS)
                    SpellHelper.Summon(new SummonedWaterElemental(), Caster, 0x217, duration, false, false);
                else
                    SpellHelper.Summon(new WaterElemental(), Caster, 0x217, duration, false, false);
            }

            FinishSequence();
        }
    }
}