using System;
using System.Collections;

using Server;

namespace Server.Items
{
    public class AgilityPotion : BaseAgilityPotion
    {
        public override int DexOffset { get { return 10; } }
        public override TimeSpan Duration { get { return TimeSpan.FromMinutes(2.0); } }

        [Constructable]
        public AgilityPotion()
            : base(PotionEffect.Agility)
        {
        }

        public AgilityPotion(Serial serial)
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

    public class GreaterAgilityPotion : BaseAgilityPotion
    {
        public override int DexOffset { get { return 20; } }
        public override TimeSpan Duration { get { return TimeSpan.FromMinutes(2.0); } }

        [Constructable]
        public GreaterAgilityPotion()
            : base(PotionEffect.AgilityGreater)
        {
        }

        public GreaterAgilityPotion(Serial serial)
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

    public class ConflagrationPotion : BaseConflagrationPotion
    {
        public override int MinDamage { get { return 2; } }
        public override int MaxDamage { get { return 4; } }

        public override int LabelNumber { get { return 1072095; } } // a Conflagration potion

        [Constructable]
        public ConflagrationPotion()
            : base(PotionEffect.Conflagration)
        {
        }

        public ConflagrationPotion(Serial serial)
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

    public class GreaterConflagrationPotion : BaseConflagrationPotion
    {
        public override int MinDamage { get { return 4; } }
        public override int MaxDamage { get { return 8; } }

        public override int LabelNumber { get { return 1072098; } } // a Greater Conflagration potion

        [Constructable]
        public GreaterConflagrationPotion()
            : base(PotionEffect.ConflagrationGreater)
        {
        }

        public GreaterConflagrationPotion(Serial serial)
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

    public class ConfusionBlastPotion : BaseConfusionBlastPotion
    {
        public override int Radius { get { return 5; } }

        public override int LabelNumber { get { return 1072105; } } // a Confusion Blast potion

        [Constructable]
        public ConfusionBlastPotion()
            : base(PotionEffect.ConfusionBlast)
        {
        }

        public ConfusionBlastPotion(Serial serial)
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

    public class GreaterConfusionBlastPotion : BaseConfusionBlastPotion
    {
        public override int Radius { get { return 7; } }

        public override int LabelNumber { get { return 1072108; } } // a Greater Confusion Blast potion

        [Constructable]
        public GreaterConfusionBlastPotion()
            : base(PotionEffect.ConfusionBlastGreater)
        {
        }

        public GreaterConfusionBlastPotion(Serial serial)
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

    public class CurePotion : BaseCurePotion
    {
        private static CureLevelInfo[] m_OldLevelInfo = new CureLevelInfo[]
			{
				new CureLevelInfo( Poison.Lesser,  1.00 ), // 100% chance to cure lesser poison
				new CureLevelInfo( Poison.Regular, 0.75 ), //  75% chance to cure regular poison
				new CureLevelInfo( Poison.Greater, 0.50 ), //  50% chance to cure greater poison
				new CureLevelInfo( Poison.Deadly,  0.15 )  //  15% chance to cure deadly poison
			};

        private static CureLevelInfo[] m_AosLevelInfo = new CureLevelInfo[]
			{
				new CureLevelInfo( Poison.Lesser,  1.00 ),
				new CureLevelInfo( Poison.Regular, 0.95 ),
				new CureLevelInfo( Poison.Greater, 0.75 ),
				new CureLevelInfo( Poison.Deadly,  0.50 ),
				new CureLevelInfo( Poison.Lethal,  0.25 )
			};

        public override CureLevelInfo[] LevelInfo { get { return Core.AOS ? m_AosLevelInfo : m_OldLevelInfo; } }

        [Constructable]
        public CurePotion()
            : base(PotionEffect.Cure)
        {
        }

        public CurePotion(Serial serial)
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

    public class GreaterCurePotion : BaseCurePotion
    {
        private static CureLevelInfo[] m_OldLevelInfo = new CureLevelInfo[]
			{
				new CureLevelInfo( Poison.Lesser,  1.00 ), // 100% chance to cure lesser poison
				new CureLevelInfo( Poison.Regular, 1.00 ), // 100% chance to cure regular poison
				new CureLevelInfo( Poison.Greater, 1.00 ), // 100% chance to cure greater poison
				new CureLevelInfo( Poison.Deadly,  0.75 ), //  75% chance to cure deadly poison
				new CureLevelInfo( Poison.Lethal,  0.25 )  //  25% chance to cure lethal poison
			};

        private static CureLevelInfo[] m_AosLevelInfo = new CureLevelInfo[]
			{
				new CureLevelInfo( Poison.Lesser,  1.00 ),
				new CureLevelInfo( Poison.Regular, 1.00 ),
				new CureLevelInfo( Poison.Greater, 1.00 ),
				new CureLevelInfo( Poison.Deadly,  0.95 ),
				new CureLevelInfo( Poison.Lethal,  0.75 )
			};

        public override CureLevelInfo[] LevelInfo { get { return Core.AOS ? m_AosLevelInfo : m_OldLevelInfo; } }

        [Constructable]
        public GreaterCurePotion()
            : base(PotionEffect.CureGreater)
        {
        }

        public GreaterCurePotion(Serial serial)
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

    public class LesserCurePotion : BaseCurePotion
    {
        private static CureLevelInfo[] m_OldLevelInfo = new CureLevelInfo[]
			{
				new CureLevelInfo( Poison.Lesser,  0.75 ), // 75% chance to cure lesser poison
				new CureLevelInfo( Poison.Regular, 0.50 ), // 50% chance to cure regular poison
				new CureLevelInfo( Poison.Greater, 0.15 )  // 15% chance to cure greater poison
			};

        private static CureLevelInfo[] m_AosLevelInfo = new CureLevelInfo[]
			{
				new CureLevelInfo( Poison.Lesser,  0.75 ),
				new CureLevelInfo( Poison.Regular, 0.50 ),
				new CureLevelInfo( Poison.Greater, 0.25 )
			};

        public override CureLevelInfo[] LevelInfo { get { return Core.AOS ? m_AosLevelInfo : m_OldLevelInfo; } }

        [Constructable]
        public LesserCurePotion()
            : base(PotionEffect.CureLesser)
        {
        }

        public LesserCurePotion(Serial serial)
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

    public class ExplosionPotion : BaseExplosionPotion
    {
        public override int MinDamage { get { return 10; } }
        public override int MaxDamage { get { return 20; } }

        [Constructable]
        public ExplosionPotion()
            : base(PotionEffect.Explosion)
        {
        }

        public ExplosionPotion(Serial serial)
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

    public class GreaterExplosionPotion : BaseExplosionPotion
    {
        public override int MinDamage { get { return Core.AOS ? 20 : 15; } }
        public override int MaxDamage { get { return Core.AOS ? 40 : 30; } }

        [Constructable]
        public GreaterExplosionPotion()
            : base(PotionEffect.ExplosionGreater)
        {
        }

        public GreaterExplosionPotion(Serial serial)
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

    public class LesserExplosionPotion : BaseExplosionPotion
    {
        public override int MinDamage { get { return 5; } }
        public override int MaxDamage { get { return 10; } }

        [Constructable]
        public LesserExplosionPotion()
            : base(PotionEffect.ExplosionLesser)
        {
        }

        public LesserExplosionPotion(Serial serial)
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

    public class HealPotion : BaseHealPotion
    {
        public override int MinHeal { get { return (Core.AOS ? 13 : 6); } }
        public override int MaxHeal { get { return (Core.AOS ? 16 : 20); } }
        public override double Delay { get { return (Core.AOS ? 8.0 : 10.0); } }

        [Constructable]
        public HealPotion()
            : base(PotionEffect.Heal)
        {
        }

        public HealPotion(Serial serial)
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

    public class GreaterHealPotion : BaseHealPotion
    {
        public override int MinHeal { get { return (Core.AOS ? 20 : 9); } }
        public override int MaxHeal { get { return (Core.AOS ? 25 : 30); } }
        public override double Delay { get { return 10.0; } }

        [Constructable]
        public GreaterHealPotion()
            : base(PotionEffect.HealGreater)
        {
        }

        public GreaterHealPotion(Serial serial)
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

    public class LesserHealPotion : BaseHealPotion
    {
        public override int MinHeal { get { return (Core.AOS ? 6 : 3); } }
        public override int MaxHeal { get { return (Core.AOS ? 8 : 10); } }
        public override double Delay { get { return (Core.AOS ? 3.0 : 10.0); } }

        [Constructable]
        public LesserHealPotion()
            : base(PotionEffect.HealLesser)
        {
        }

        public LesserHealPotion(Serial serial)
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

    public class PoisonPotion : BasePoisonPotion
    {
        public override Poison Poison { get { return Poison.Regular; } }

        public override double MinPoisoningSkill { get { return 30.0; } }
        public override double MaxPoisoningSkill { get { return 70.0; } }

        [Constructable]
        public PoisonPotion()
            : base(PotionEffect.Poison)
        {
        }

        public PoisonPotion(Serial serial)
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

    public class DeadlyPoisonPotion : BasePoisonPotion
    {
        public override Poison Poison { get { return Poison.Deadly; } }

        public override double MinPoisoningSkill { get { return 95.0; } }
        public override double MaxPoisoningSkill { get { return 100.0; } }

        [Constructable]
        public DeadlyPoisonPotion()
            : base(PotionEffect.PoisonDeadly)
        {
        }

        public DeadlyPoisonPotion(Serial serial)
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

    public class GreaterPoisonPotion : BasePoisonPotion
    {
        public override Poison Poison { get { return Poison.Greater; } }

        public override double MinPoisoningSkill { get { return 60.0; } }
        public override double MaxPoisoningSkill { get { return 100.0; } }

        [Constructable]
        public GreaterPoisonPotion()
            : base(PotionEffect.PoisonGreater)
        {
        }

        public GreaterPoisonPotion(Serial serial)
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

    public class LesserPoisonPotion : BasePoisonPotion
    {
        public override Poison Poison { get { return Poison.Lesser; } }

        public override double MinPoisoningSkill { get { return 0.0; } }
        public override double MaxPoisoningSkill { get { return 60.0; } }

        [Constructable]
        public LesserPoisonPotion()
            : base(PotionEffect.PoisonLesser)
        {
        }

        public LesserPoisonPotion(Serial serial)
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

    public class RefreshPotion : BaseRefreshPotion
    {
        public override double Refresh { get { return 0.25; } }

        [Constructable]
        public RefreshPotion()
            : base(PotionEffect.Refresh)
        {
        }

        public RefreshPotion(Serial serial)
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

    public class TotalRefreshPotion : BaseRefreshPotion
    {
        public override double Refresh { get { return 1.0; } }

        [Constructable]
        public TotalRefreshPotion()
            : base(PotionEffect.RefreshTotal)
        {
        }

        public TotalRefreshPotion(Serial serial)
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

    public class StrengthPotion : BaseStrengthPotion
    {
        public override int StrOffset { get { return 10; } }
        public override TimeSpan Duration { get { return TimeSpan.FromMinutes(2.0); } }

        [Constructable]
        public StrengthPotion()
            : base(PotionEffect.Strength)
        {
        }

        public StrengthPotion(Serial serial)
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

    public class GreaterStrengthPotion : BaseStrengthPotion
    {
        public override int StrOffset { get { return 20; } }
        public override TimeSpan Duration { get { return TimeSpan.FromMinutes(2.0); } }

        [Constructable]
        public GreaterStrengthPotion()
            : base(PotionEffect.StrengthGreater)
        {
        }

        public GreaterStrengthPotion(Serial serial)
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

    public class NightSightPotion : BasePotion
    {
        [Constructable]
        public NightSightPotion()
            : base(0xF06, PotionEffect.Nightsight)
        {
        }

        public NightSightPotion(Serial serial)
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
            if (from.BeginAction(typeof(LightCycle)))
            {
                new LightCycle.NightSightTimer(from).Start();
                from.LightLevel = LightCycle.DungeonLevel / 2;

                from.FixedParticles(0x376A, 9, 32, 5007, EffectLayer.Waist);
                from.PlaySound(0x1E3);

                BasePotion.PlayDrinkEffect(from);

                if (!Engines.ConPVP.DuelContext.IsFreeConsume(from))
                    this.Consume();
            }
            else
            {
                from.SendMessage("You already have nightsight.");
            }
        }
    }

    public class DarkglowPotion : BasePoisonPotion
    {
        public override Poison Poison { get { return Poison.Greater; } } /*  MUST be restored when prerequisites are done */

        public override double MinPoisoningSkill { get { return 95.0; } }
        public override double MaxPoisoningSkill { get { return 100.0; } }

        public override int LabelNumber { get { return 1072849; } } // Darkglow Poison

        [Constructable]
        public DarkglowPotion()
            : base(PotionEffect.Darkglow)
        {
            Hue = 0x96;
        }

        public DarkglowPotion(Serial serial)
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

    public class InvisibilityPotion : BasePotion
    {
        public override int LabelNumber { get { return 1072941; } } // Potion of Invisibility

        [Constructable]
        public InvisibilityPotion()
            : base(0xF0A, PotionEffect.Invisibility)
        {
            Hue = 0x48D;
        }

        public InvisibilityPotion(Serial serial)
            : base(serial)
        {
        }

        public override void Drink(Mobile from)
        {
            if (from.Hidden)
            {
                from.SendLocalizedMessage(1073185); // You are already unseen.
                return;
            }

            if (HasTimer(from))
            {
                from.SendLocalizedMessage(1073186); // An invisibility potion is already taking effect on your person.
                return;
            }

            Consume();
            m_Table[from] = Timer.DelayCall(TimeSpan.FromSeconds(2), new TimerStateCallback(Hide_Callback), from);
            PlayDrinkEffect(from);
        }

        private static void Hide_Callback(object obj)
        {
            if (obj is Mobile)
                Hide((Mobile)obj);
        }

        public static void Hide(Mobile m)
        {
            Effects.SendLocationParticles(EffectItem.Create(new Point3D(m.X, m.Y, m.Z + 16), m.Map, EffectItem.DefaultDuration), 0x376A, 10, 15, 5045);
            m.PlaySound(0x3C4);

            m.Hidden = true;

            BuffInfo.RemoveBuff(m, BuffIcon.HidingAndOrStealth);
            BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.Invisibility, 1075825));	//Invisibility/Invisible

            RemoveTimer(m);

            Timer.DelayCall(TimeSpan.FromSeconds(30), new TimerStateCallback(EndHide_Callback), m);
        }

        private static void EndHide_Callback(object obj)
        {
            if (obj is Mobile)
                EndHide((Mobile)obj);
        }

        public static void EndHide(Mobile m)
        {
            m.RevealingAction();
            RemoveTimer(m);
        }

        private static Hashtable m_Table = new Hashtable();

        public static bool HasTimer(Mobile m)
        {
            return m_Table[m] != null;
        }

        public static void RemoveTimer(Mobile m)
        {
            Timer t = (Timer)m_Table[m];

            if (t != null)
            {
                t.Stop();
                m_Table.Remove(m);
            }
        }

        public static void Iterrupt(Mobile m)
        {
            m.SendLocalizedMessage(1073187); // The invisibility effect is interrupted.
            RemoveTimer(m);
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

    public class ParasiticPotion : BasePoisonPotion
    {
        public override Poison Poison { get { return Poison.Greater; } } 		/* public override Poison Poison{ get{ return Poison.Darkglow; } }  MUST be restored when prerequisites are done */

        public override double MinPoisoningSkill { get { return 95.0; } }
        public override double MaxPoisoningSkill { get { return 100.0; } }

        public override int LabelNumber { get { return 1072848; } } // Parasitic Poison

        [Constructable]
        public ParasiticPotion()
            : base(PotionEffect.Parasitic)
        {
            Hue = 0x17C;
        }

        public ParasiticPotion(Serial serial)
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
}