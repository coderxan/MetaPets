using System;

using Server;
using Server.Items;

namespace Server.Items
{
    /// <summary>
    /// Magery: 1st Circle Spells: Server.Spells.First
    /// </summary>
    public class ClumsyScroll : SpellScroll
    {
        [Constructable]
        public ClumsyScroll()
            : this(1)
        {
        }

        [Constructable]
        public ClumsyScroll(int amount)
            : base(0, 0x1F2E, amount)
        {
        }

        public ClumsyScroll(Serial serial)
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

    public class CreateFoodScroll : SpellScroll
    {
        [Constructable]
        public CreateFoodScroll()
            : this(1)
        {
        }

        [Constructable]
        public CreateFoodScroll(int amount)
            : base(1, 0x1F2F, amount)
        {
        }

        public CreateFoodScroll(Serial serial)
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

    public class FeeblemindScroll : SpellScroll
    {
        [Constructable]
        public FeeblemindScroll()
            : this(1)
        {
        }

        [Constructable]
        public FeeblemindScroll(int amount)
            : base(2, 0x1F30, amount)
        {
        }

        public FeeblemindScroll(Serial serial)
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

    public class HealScroll : SpellScroll
    {
        [Constructable]
        public HealScroll()
            : this(1)
        {
        }

        [Constructable]
        public HealScroll(int amount)
            : base(3, 0x1F31, amount)
        {
        }

        public HealScroll(Serial serial)
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

    public class MagicArrowScroll : SpellScroll
    {
        [Constructable]
        public MagicArrowScroll()
            : this(1)
        {
        }

        [Constructable]
        public MagicArrowScroll(int amount)
            : base(4, 0x1F32, amount)
        {
        }

        public MagicArrowScroll(Serial serial)
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

    public class NightSightScroll : SpellScroll
    {
        [Constructable]
        public NightSightScroll()
            : this(1)
        {
        }

        [Constructable]
        public NightSightScroll(int amount)
            : base(5, 0x1F33, amount)
        {
        }

        public NightSightScroll(Serial ser)
            : base(ser)
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

    public class ReactiveArmorScroll : SpellScroll
    {
        [Constructable]
        public ReactiveArmorScroll()
            : this(1)
        {
        }

        [Constructable]
        public ReactiveArmorScroll(int amount)
            : base(6, 0x1F2D, amount)
        {
        }

        public ReactiveArmorScroll(Serial ser)
            : base(ser)
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

    public class WeakenScroll : SpellScroll
    {
        [Constructable]
        public WeakenScroll()
            : this(1)
        {
        }

        [Constructable]
        public WeakenScroll(int amount)
            : base(7, 0x1F34, amount)
        {
        }

        public WeakenScroll(Serial serial)
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
    /// Magery: 2nd Circle Spells: Server.Spells.Second
    /// </summary>
    public class AgilityScroll : SpellScroll
    {
        [Constructable]
        public AgilityScroll()
            : this(1)
        {
        }

        [Constructable]
        public AgilityScroll(int amount)
            : base(8, 0x1F35, amount)
        {
        }

        public AgilityScroll(Serial serial)
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

    public class CunningScroll : SpellScroll
    {
        [Constructable]
        public CunningScroll()
            : this(1)
        {
        }

        [Constructable]
        public CunningScroll(int amount)
            : base(9, 0x1F36, amount)
        {
        }

        public CunningScroll(Serial serial)
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

    public class CureScroll : SpellScroll
    {
        [Constructable]
        public CureScroll()
            : this(1)
        {
        }

        [Constructable]
        public CureScroll(int amount)
            : base(10, 0x1F37, amount)
        {
        }

        public CureScroll(Serial serial)
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

    public class HarmScroll : SpellScroll
    {
        [Constructable]
        public HarmScroll()
            : this(1)
        {
        }

        [Constructable]
        public HarmScroll(int amount)
            : base(11, 0x1F38, amount)
        {
        }

        public HarmScroll(Serial serial)
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

    public class MagicTrapScroll : SpellScroll
    {
        [Constructable]
        public MagicTrapScroll()
            : this(1)
        {
        }

        [Constructable]
        public MagicTrapScroll(int amount)
            : base(12, 0x1F39, amount)
        {
        }

        public MagicTrapScroll(Serial serial)
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

    public class MagicUnTrapScroll : SpellScroll
    {
        [Constructable]
        public MagicUnTrapScroll()
            : this(1)
        {
        }

        [Constructable]
        public MagicUnTrapScroll(int amount)
            : base(13, 0x1F3A, amount)
        {
        }

        public MagicUnTrapScroll(Serial serial)
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

    public class ProtectionScroll : SpellScroll
    {
        [Constructable]
        public ProtectionScroll()
            : this(1)
        {
        }

        [Constructable]
        public ProtectionScroll(int amount)
            : base(14, 0x1F3B, amount)
        {
        }

        public ProtectionScroll(Serial serial)
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

    public class StrengthScroll : SpellScroll
    {
        [Constructable]
        public StrengthScroll()
            : this(1)
        {
        }

        [Constructable]
        public StrengthScroll(int amount)
            : base(15, 0x1F3C, amount)
        {
        }

        public StrengthScroll(Serial serial)
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
    /// Magery: 3rd Circle Spells: Server.Spells.Third
    /// </summary>
    public class BlessScroll : SpellScroll
    {
        [Constructable]
        public BlessScroll()
            : this(1)
        {
        }

        [Constructable]
        public BlessScroll(int amount)
            : base(16, 0x1F3D, amount)
        {
        }

        public BlessScroll(Serial serial)
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

    public class FireballScroll : SpellScroll
    {
        [Constructable]
        public FireballScroll()
            : this(1)
        {
        }

        [Constructable]
        public FireballScroll(int amount)
            : base(17, 0x1F3E, amount)
        {
        }

        public FireballScroll(Serial serial)
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

    public class MagicLockScroll : SpellScroll
    {
        [Constructable]
        public MagicLockScroll()
            : this(1)
        {
        }

        [Constructable]
        public MagicLockScroll(int amount)
            : base(18, 0x1F3F, amount)
        {
        }

        public MagicLockScroll(Serial serial)
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

    public class PoisonScroll : SpellScroll
    {
        [Constructable]
        public PoisonScroll()
            : this(1)
        {
        }

        [Constructable]
        public PoisonScroll(int amount)
            : base(19, 0x1F40, amount)
        {
        }

        public PoisonScroll(Serial serial)
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

    public class TelekinisisScroll : SpellScroll
    {
        [Constructable]
        public TelekinisisScroll()
            : this(1)
        {
        }

        [Constructable]
        public TelekinisisScroll(int amount)
            : base(20, 0x1F41, amount)
        {
        }

        public TelekinisisScroll(Serial serial)
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

    public class TeleportScroll : SpellScroll
    {
        [Constructable]
        public TeleportScroll()
            : this(1)
        {
        }

        [Constructable]
        public TeleportScroll(int amount)
            : base(21, 0x1F42, amount)
        {
        }

        public TeleportScroll(Serial serial)
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

    public class UnlockScroll : SpellScroll
    {
        [Constructable]
        public UnlockScroll()
            : this(1)
        {
        }

        [Constructable]
        public UnlockScroll(int amount)
            : base(22, 0x1F43, amount)
        {
        }

        public UnlockScroll(Serial serial)
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

    public class WallOfStoneScroll : SpellScroll
    {
        [Constructable]
        public WallOfStoneScroll()
            : this(1)
        {
        }

        [Constructable]
        public WallOfStoneScroll(int amount)
            : base(23, 0x1F44, amount)
        {
        }

        public WallOfStoneScroll(Serial serial)
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
    /// Magery: 4th Circle Spells: Server.Spells.Fourth
    /// </summary>
    public class ArchCureScroll : SpellScroll
    {
        [Constructable]
        public ArchCureScroll()
            : this(1)
        {
        }

        [Constructable]
        public ArchCureScroll(int amount)
            : base(24, 0x1F45, amount)
        {
        }

        public ArchCureScroll(Serial serial)
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

    public class ArchProtectionScroll : SpellScroll
    {
        [Constructable]
        public ArchProtectionScroll()
            : this(1)
        {
        }

        [Constructable]
        public ArchProtectionScroll(int amount)
            : base(25, 0x1F46, amount)
        {
        }

        public ArchProtectionScroll(Serial serial)
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

    public class CurseScroll : SpellScroll
    {
        [Constructable]
        public CurseScroll()
            : this(1)
        {
        }

        [Constructable]
        public CurseScroll(int amount)
            : base(26, 0x1F47, amount)
        {
        }

        public CurseScroll(Serial serial)
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

    public class FireFieldScroll : SpellScroll
    {
        [Constructable]
        public FireFieldScroll()
            : this(1)
        {
        }

        [Constructable]
        public FireFieldScroll(int amount)
            : base(27, 0x1F48, amount)
        {
        }

        public FireFieldScroll(Serial serial)
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

    public class GreaterHealScroll : SpellScroll
    {
        [Constructable]
        public GreaterHealScroll()
            : this(1)
        {
        }

        [Constructable]
        public GreaterHealScroll(int amount)
            : base(28, 0x1F49, amount)
        {
        }

        public GreaterHealScroll(Serial serial)
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

    public class LightningScroll : SpellScroll
    {
        [Constructable]
        public LightningScroll()
            : this(1)
        {
        }

        [Constructable]
        public LightningScroll(int amount)
            : base(29, 0x1F4A, amount)
        {
        }

        public LightningScroll(Serial serial)
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

    public class ManaDrainScroll : SpellScroll
    {
        [Constructable]
        public ManaDrainScroll()
            : this(1)
        {
        }

        [Constructable]
        public ManaDrainScroll(int amount)
            : base(30, 0x1F4B, amount)
        {
        }

        public ManaDrainScroll(Serial serial)
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

    public class RecallScroll : SpellScroll
    {
        [Constructable]
        public RecallScroll()
            : this(1)
        {
        }

        [Constructable]
        public RecallScroll(int amount)
            : base(31, 0x1F4C, amount)
        {
        }

        public RecallScroll(Serial serial)
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
    /// Magery: 5th Circle Spells: Server.Spells.Fifth
    /// </summary>
    public class BladeSpiritsScroll : SpellScroll
    {
        [Constructable]
        public BladeSpiritsScroll()
            : this(1)
        {
        }

        [Constructable]
        public BladeSpiritsScroll(int amount)
            : base(32, 0x1F4D, amount)
        {
        }

        public BladeSpiritsScroll(Serial serial)
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

    public class DispelFieldScroll : SpellScroll
    {
        [Constructable]
        public DispelFieldScroll()
            : this(1)
        {
        }

        [Constructable]
        public DispelFieldScroll(int amount)
            : base(33, 0x1F4E, amount)
        {
        }

        public DispelFieldScroll(Serial serial)
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

    public class IncognitoScroll : SpellScroll
    {
        [Constructable]
        public IncognitoScroll()
            : this(1)
        {
        }

        [Constructable]
        public IncognitoScroll(int amount)
            : base(34, 0x1F4F, amount)
        {
        }

        public IncognitoScroll(Serial serial)
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

    public class MagicReflectScroll : SpellScroll
    {
        [Constructable]
        public MagicReflectScroll()
            : this(1)
        {
        }

        [Constructable]
        public MagicReflectScroll(int amount)
            : base(35, 0x1F50, amount)
        {
        }

        public MagicReflectScroll(Serial serial)
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

    public class MindBlastScroll : SpellScroll
    {
        [Constructable]
        public MindBlastScroll()
            : this(1)
        {
        }

        [Constructable]
        public MindBlastScroll(int amount)
            : base(36, 0x1F51, amount)
        {
        }

        public MindBlastScroll(Serial serial)
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

    public class ParalyzeScroll : SpellScroll
    {
        [Constructable]
        public ParalyzeScroll()
            : this(1)
        {
        }

        [Constructable]
        public ParalyzeScroll(int amount)
            : base(37, 0x1F52, amount)
        {
        }

        public ParalyzeScroll(Serial serial)
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

    public class PoisonFieldScroll : SpellScroll
    {
        [Constructable]
        public PoisonFieldScroll()
            : this(1)
        {
        }

        [Constructable]
        public PoisonFieldScroll(int amount)
            : base(38, 0x1F53, amount)
        {
        }

        public PoisonFieldScroll(Serial serial)
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

    public class SummonCreatureScroll : SpellScroll
    {
        [Constructable]
        public SummonCreatureScroll()
            : this(1)
        {
        }

        [Constructable]
        public SummonCreatureScroll(int amount)
            : base(39, 0x1F54, amount)
        {
        }

        public SummonCreatureScroll(Serial serial)
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
    /// Magery: 6th Circle Spells: Server.Spells.Sixth
    /// </summary>
    public class DispelScroll : SpellScroll
    {
        [Constructable]
        public DispelScroll()
            : this(1)
        {
        }

        [Constructable]
        public DispelScroll(int amount)
            : base(40, 0x1F55, amount)
        {
        }

        public DispelScroll(Serial serial)
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

    public class EnergyBoltScroll : SpellScroll
    {
        [Constructable]
        public EnergyBoltScroll()
            : this(1)
        {
        }

        [Constructable]
        public EnergyBoltScroll(int amount)
            : base(41, 0x1F56, amount)
        {
        }

        public EnergyBoltScroll(Serial serial)
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

    public class ExplosionScroll : SpellScroll
    {
        [Constructable]
        public ExplosionScroll()
            : this(1)
        {
        }

        [Constructable]
        public ExplosionScroll(int amount)
            : base(42, 0x1F57, amount)
        {
        }

        public ExplosionScroll(Serial serial)
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

    public class InvisibilityScroll : SpellScroll
    {
        [Constructable]
        public InvisibilityScroll()
            : this(1)
        {
        }

        [Constructable]
        public InvisibilityScroll(int amount)
            : base(43, 0x1F58, amount)
        {
        }

        public InvisibilityScroll(Serial serial)
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

    public class MarkScroll : SpellScroll
    {
        [Constructable]
        public MarkScroll()
            : this(1)
        {
        }

        [Constructable]
        public MarkScroll(int amount)
            : base(44, 0x1F59, amount)
        {
        }

        public MarkScroll(Serial serial)
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

    public class MassCurseScroll : SpellScroll
    {
        [Constructable]
        public MassCurseScroll()
            : this(1)
        {
        }

        [Constructable]
        public MassCurseScroll(int amount)
            : base(45, 0x1F5A, amount)
        {
        }

        public MassCurseScroll(Serial serial)
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

    public class ParalyzeFieldScroll : SpellScroll
    {
        [Constructable]
        public ParalyzeFieldScroll()
            : this(1)
        {
        }

        [Constructable]
        public ParalyzeFieldScroll(int amount)
            : base(46, 0x1F5B, amount)
        {
        }

        public ParalyzeFieldScroll(Serial serial)
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

    public class RevealScroll : SpellScroll
    {
        [Constructable]
        public RevealScroll()
            : this(1)
        {
        }

        [Constructable]
        public RevealScroll(int amount)
            : base(47, 0x1F5C, amount)
        {
        }

        public RevealScroll(Serial serial)
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
    /// Magery: 7th Circle Spells: Server.Spells.Seventh
    /// </summary>
    public class ChainLightningScroll : SpellScroll
    {
        [Constructable]
        public ChainLightningScroll()
            : this(1)
        {
        }

        [Constructable]
        public ChainLightningScroll(int amount)
            : base(48, 0x1F5D, amount)
        {
        }

        public ChainLightningScroll(Serial serial)
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

    public class EnergyFieldScroll : SpellScroll
    {
        [Constructable]
        public EnergyFieldScroll()
            : this(1)
        {
        }

        [Constructable]
        public EnergyFieldScroll(int amount)
            : base(49, 0x1F5E, amount)
        {
        }

        public EnergyFieldScroll(Serial serial)
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

    public class FlamestrikeScroll : SpellScroll
    {
        [Constructable]
        public FlamestrikeScroll()
            : this(1)
        {
        }

        [Constructable]
        public FlamestrikeScroll(int amount)
            : base(50, 0x1F5F, amount)
        {
        }

        public FlamestrikeScroll(Serial serial)
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

    public class GateTravelScroll : SpellScroll
    {
        [Constructable]
        public GateTravelScroll()
            : this(1)
        {
        }

        [Constructable]
        public GateTravelScroll(int amount)
            : base(51, 0x1F60, amount)
        {
        }

        public GateTravelScroll(Serial serial)
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

    public class ManaVampireScroll : SpellScroll
    {
        [Constructable]
        public ManaVampireScroll()
            : this(1)
        {
        }

        [Constructable]
        public ManaVampireScroll(int amount)
            : base(52, 0x1F61, amount)
        {
        }

        public ManaVampireScroll(Serial serial)
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

    public class MassDispelScroll : SpellScroll
    {
        [Constructable]
        public MassDispelScroll()
            : this(1)
        {
        }

        [Constructable]
        public MassDispelScroll(int amount)
            : base(53, 0x1F62, amount)
        {
        }

        public MassDispelScroll(Serial serial)
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

    public class MeteorSwarmScroll : SpellScroll
    {
        [Constructable]
        public MeteorSwarmScroll()
            : this(1)
        {
        }

        [Constructable]
        public MeteorSwarmScroll(int amount)
            : base(54, 0x1F63, amount)
        {
        }

        public MeteorSwarmScroll(Serial serial)
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

    public class PolymorphScroll : SpellScroll
    {
        [Constructable]
        public PolymorphScroll()
            : this(1)
        {
        }

        [Constructable]
        public PolymorphScroll(int amount)
            : base(55, 0x1F64, amount)
        {
        }

        public PolymorphScroll(Serial serial)
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
    /// Magery: 8th Circle Spells: Server.Spells.Eighth
    /// </summary>
    public class EarthquakeScroll : SpellScroll
    {
        [Constructable]
        public EarthquakeScroll()
            : this(1)
        {
        }

        [Constructable]
        public EarthquakeScroll(int amount)
            : base(56, 0x1F65, amount)
        {
        }

        public EarthquakeScroll(Serial serial)
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

    public class EnergyVortexScroll : SpellScroll
    {
        [Constructable]
        public EnergyVortexScroll()
            : this(1)
        {
        }

        [Constructable]
        public EnergyVortexScroll(int amount)
            : base(57, 0x1F66, amount)
        {
        }

        public EnergyVortexScroll(Serial serial)
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

    public class ResurrectionScroll : SpellScroll
    {
        [Constructable]
        public ResurrectionScroll()
            : this(1)
        {
        }

        [Constructable]
        public ResurrectionScroll(int amount)
            : base(58, 0x1F67, amount)
        {
        }

        public ResurrectionScroll(Serial serial)
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

    public class SummonAirElementalScroll : SpellScroll
    {
        [Constructable]
        public SummonAirElementalScroll()
            : this(1)
        {
        }

        [Constructable]
        public SummonAirElementalScroll(int amount)
            : base(59, 0x1F68, amount)
        {
        }

        public SummonAirElementalScroll(Serial serial)
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

    public class SummonDaemonScroll : SpellScroll
    {
        [Constructable]
        public SummonDaemonScroll()
            : this(1)
        {
        }

        [Constructable]
        public SummonDaemonScroll(int amount)
            : base(60, 0x1F69, amount)
        {
        }

        public SummonDaemonScroll(Serial serial)
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

    public class SummonEarthElementalScroll : SpellScroll
    {
        [Constructable]
        public SummonEarthElementalScroll()
            : this(1)
        {
        }

        [Constructable]
        public SummonEarthElementalScroll(int amount)
            : base(61, 0x1F6A, amount)
        {
        }

        public SummonEarthElementalScroll(Serial serial)
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

    public class SummonFireElementalScroll : SpellScroll
    {
        [Constructable]
        public SummonFireElementalScroll()
            : this(1)
        {
        }

        [Constructable]
        public SummonFireElementalScroll(int amount)
            : base(62, 0x1F6B, amount)
        {
        }

        public SummonFireElementalScroll(Serial serial)
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

    public class SummonWaterElementalScroll : SpellScroll
    {
        [Constructable]
        public SummonWaterElementalScroll()
            : this(1)
        {
        }

        [Constructable]
        public SummonWaterElementalScroll(int amount)
            : base(63, 0x1F6C, amount)
        {
        }

        public SummonWaterElementalScroll(Serial serial)
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
    /// Necromancy: Spellbook Spells: Server.Spells.Necromancy
    /// </summary>
    public class AnimateDeadScroll : SpellScroll
    {
        [Constructable]
        public AnimateDeadScroll()
            : this(1)
        {
        }

        [Constructable]
        public AnimateDeadScroll(int amount)
            : base(100, 0x2260, amount)
        {
        }

        public AnimateDeadScroll(Serial serial)
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

    public class BloodOathScroll : SpellScroll
    {
        [Constructable]
        public BloodOathScroll()
            : this(1)
        {
        }

        [Constructable]
        public BloodOathScroll(int amount)
            : base(101, 0x2261, amount)
        {
        }

        public BloodOathScroll(Serial serial)
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

    public class CorpseSkinScroll : SpellScroll
    {
        [Constructable]
        public CorpseSkinScroll()
            : this(1)
        {
        }

        [Constructable]
        public CorpseSkinScroll(int amount)
            : base(102, 0x2262, amount)
        {
        }

        public CorpseSkinScroll(Serial serial)
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

    public class CurseWeaponScroll : SpellScroll
    {
        [Constructable]
        public CurseWeaponScroll()
            : this(1)
        {
        }

        [Constructable]
        public CurseWeaponScroll(int amount)
            : base(103, 0x2263, amount)
        {
        }

        public CurseWeaponScroll(Serial serial)
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

    public class EvilOmenScroll : SpellScroll
    {
        [Constructable]
        public EvilOmenScroll()
            : this(1)
        {
        }

        [Constructable]
        public EvilOmenScroll(int amount)
            : base(104, 0x2264, amount)
        {
        }

        public EvilOmenScroll(Serial serial)
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

    public class ExorcismScroll : SpellScroll
    {
        [Constructable]
        public ExorcismScroll()
            : this(1)
        {
        }

        [Constructable]
        public ExorcismScroll(int amount)
            : base(116, 0x2270, amount)
        {
        }

        public ExorcismScroll(Serial serial)
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

    public class HorrificBeastScroll : SpellScroll
    {
        [Constructable]
        public HorrificBeastScroll()
            : this(1)
        {
        }

        [Constructable]
        public HorrificBeastScroll(int amount)
            : base(105, 0x2265, amount)
        {
        }

        public HorrificBeastScroll(Serial serial)
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

    public class LichFormScroll : SpellScroll
    {
        [Constructable]
        public LichFormScroll()
            : this(1)
        {
        }

        [Constructable]
        public LichFormScroll(int amount)
            : base(106, 0x2266, amount)
        {
        }

        public LichFormScroll(Serial serial)
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

    public class MindRotScroll : SpellScroll
    {
        [Constructable]
        public MindRotScroll()
            : this(1)
        {
        }

        [Constructable]
        public MindRotScroll(int amount)
            : base(107, 0x2267, amount)
        {
        }

        public MindRotScroll(Serial serial)
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

    public class PainSpikeScroll : SpellScroll
    {
        [Constructable]
        public PainSpikeScroll()
            : this(1)
        {
        }

        [Constructable]
        public PainSpikeScroll(int amount)
            : base(108, 0x2268, amount)
        {
        }

        public PainSpikeScroll(Serial serial)
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

    public class PoisonStrikeScroll : SpellScroll
    {
        [Constructable]
        public PoisonStrikeScroll()
            : this(1)
        {
        }

        [Constructable]
        public PoisonStrikeScroll(int amount)
            : base(109, 0x2269, amount)
        {
        }

        public PoisonStrikeScroll(Serial serial)
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

    public class StrangleScroll : SpellScroll
    {
        [Constructable]
        public StrangleScroll()
            : this(1)
        {
        }

        [Constructable]
        public StrangleScroll(int amount)
            : base(110, 0x226A, amount)
        {
        }

        public StrangleScroll(Serial serial)
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

    public class SummonFamiliarScroll : SpellScroll
    {
        [Constructable]
        public SummonFamiliarScroll()
            : this(1)
        {
        }

        [Constructable]
        public SummonFamiliarScroll(int amount)
            : base(111, 0x226B, amount)
        {
        }

        public SummonFamiliarScroll(Serial serial)
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

    public class VampiricEmbraceScroll : SpellScroll
    {
        [Constructable]
        public VampiricEmbraceScroll()
            : this(1)
        {
        }

        [Constructable]
        public VampiricEmbraceScroll(int amount)
            : base(112, 0x226C, amount)
        {
        }

        public VampiricEmbraceScroll(Serial serial)
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

    public class VengefulSpiritScroll : SpellScroll
    {
        [Constructable]
        public VengefulSpiritScroll()
            : this(1)
        {
        }

        [Constructable]
        public VengefulSpiritScroll(int amount)
            : base(113, 0x226D, amount)
        {
        }

        public VengefulSpiritScroll(Serial serial)
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

    public class WitherScroll : SpellScroll
    {
        [Constructable]
        public WitherScroll()
            : this(1)
        {
        }

        [Constructable]
        public WitherScroll(int amount)
            : base(114, 0x226E, amount)
        {
        }

        public WitherScroll(Serial serial)
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

    public class WraithFormScroll : SpellScroll
    {
        [Constructable]
        public WraithFormScroll()
            : this(1)
        {
        }

        [Constructable]
        public WraithFormScroll(int amount)
            : base(115, 0x226F, amount)
        {
        }

        public WraithFormScroll(Serial serial)
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
    /// Spellweaving: Spellbook Spells: Server.Spells.Spellweaving
    /// </summary>
    public class ArcaneCircleScroll : SpellScroll
    {
        [Constructable]
        public ArcaneCircleScroll()
            : this(1)
        {
        }

        [Constructable]
        public ArcaneCircleScroll(int amount)
            : base(600, 0x2D51, amount)
        {
            Hue = 0x8FD;
        }

        public ArcaneCircleScroll(Serial serial)
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

    public class ArcaneEmpowermentScroll : SpellScroll
    {
        [Constructable]
        public ArcaneEmpowermentScroll()
            : this(1)
        {
        }

        [Constructable]
        public ArcaneEmpowermentScroll(int amount)
            : base(615, 0x2D60, amount)
        {
            Hue = 0x8FD;
        }

        public ArcaneEmpowermentScroll(Serial serial)
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

    public class AttuneWeaponScroll : SpellScroll
    {
        [Constructable]
        public AttuneWeaponScroll()
            : this(1)
        {
        }

        [Constructable]
        public AttuneWeaponScroll(int amount)
            : base(603, 0x2D54, amount)
        {
            Hue = 0x8FD;
        }

        public AttuneWeaponScroll(Serial serial)
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

    public class DryadAllureScroll : SpellScroll
    {
        [Constructable]
        public DryadAllureScroll()
            : this(1)
        {
        }

        [Constructable]
        public DryadAllureScroll(int amount)
            : base(611, 0x2D5C, amount)
        {
            Hue = 0x8FD;
        }

        public DryadAllureScroll(Serial serial)
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

    public class EssenceOfWindScroll : SpellScroll
    {
        [Constructable]
        public EssenceOfWindScroll()
            : this(1)
        {
        }

        [Constructable]
        public EssenceOfWindScroll(int amount)
            : base(610, 0x2D5B, amount)
        {
            Hue = 0x8FD;
        }

        public EssenceOfWindScroll(Serial serial)
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

    public class EtherealVoyageScroll : SpellScroll
    {
        [Constructable]
        public EtherealVoyageScroll()
            : this(1)
        {
        }

        [Constructable]
        public EtherealVoyageScroll(int amount)
            : base(612, 0x2D5D, amount)
        {
            Hue = 0x8FD;
        }

        public EtherealVoyageScroll(Serial serial)
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

    public class GiftOfLifeScroll : SpellScroll
    {
        [Constructable]
        public GiftOfLifeScroll()
            : this(1)
        {
        }

        [Constructable]
        public GiftOfLifeScroll(int amount)
            : base(614, 0x2D5F, amount)
        {
            Hue = 0x8FD;
        }

        public GiftOfLifeScroll(Serial serial)
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

    public class GiftOfRenewalScroll : SpellScroll
    {
        [Constructable]
        public GiftOfRenewalScroll()
            : this(1)
        {
        }

        [Constructable]
        public GiftOfRenewalScroll(int amount)
            : base(601, 0x2D52, amount)
        {
            Hue = 0x8FD;
        }

        public GiftOfRenewalScroll(Serial serial)
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

    public class ImmolatingWeaponScroll : SpellScroll
    {
        [Constructable]
        public ImmolatingWeaponScroll()
            : this(1)
        {
        }

        [Constructable]
        public ImmolatingWeaponScroll(int amount)
            : base(602, 0x2D53, amount)
        {
            Hue = 0x8FD;
        }

        public ImmolatingWeaponScroll(Serial serial)
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

    public class NatureFuryScroll : SpellScroll
    {
        [Constructable]
        public NatureFuryScroll()
            : this(1)
        {
        }

        [Constructable]
        public NatureFuryScroll(int amount)
            : base(605, 0x2D56, amount)
        {
            Hue = 0x8FD;
        }

        public NatureFuryScroll(Serial serial)
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

    public class ReaperFormScroll : SpellScroll
    {
        [Constructable]
        public ReaperFormScroll()
            : this(1)
        {
        }

        [Constructable]
        public ReaperFormScroll(int amount)
            : base(608, 0x2D59, amount)
        {
            Hue = 0x8FD;
        }

        public ReaperFormScroll(Serial serial)
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

    public class SummonFeyScroll : SpellScroll
    {
        [Constructable]
        public SummonFeyScroll()
            : this(1)
        {
        }

        [Constructable]
        public SummonFeyScroll(int amount)
            : base(606, 0x2D57, amount)
        {
            Hue = 0x8FD;
        }

        public SummonFeyScroll(Serial serial)
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

    public class SummonFiendScroll : SpellScroll
    {
        [Constructable]
        public SummonFiendScroll()
            : this(1)
        {
        }

        [Constructable]
        public SummonFiendScroll(int amount)
            : base(607, 0x2D58, amount)
        {
            Hue = 0x8FD;
        }

        public SummonFiendScroll(Serial serial)
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

    public class ThunderstormScroll : SpellScroll
    {
        [Constructable]
        public ThunderstormScroll()
            : this(1)
        {
        }

        [Constructable]
        public ThunderstormScroll(int amount)
            : base(604, 0x2D55, amount)
        {
            Hue = 0x8FD;
        }

        public ThunderstormScroll(Serial serial)
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

    public class WildfireScroll : SpellScroll
    {
        [Constructable]
        public WildfireScroll()
            : this(1)
        {
        }

        [Constructable]
        public WildfireScroll(int amount)
            : base(609, 0x2D5A, amount)
        {
            Hue = 0x8FD;
        }

        public WildfireScroll(Serial serial)
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

    public class WordOfDeathScroll : SpellScroll
    {
        [Constructable]
        public WordOfDeathScroll()
            : this(1)
        {
        }

        [Constructable]
        public WordOfDeathScroll(int amount)
            : base(613, 0x2D5E, amount)
        {
            Hue = 0x8FD;
        }

        public WordOfDeathScroll(Serial serial)
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
    /// Mysticism: Spellbook Spells: Server.Spells.Mysticism
    /// </summary>
    public class AnimatedWeaponScroll : SpellScroll
    {
        [Constructable]
        public AnimatedWeaponScroll()
            : this(1)
        {
        }

        [Constructable]
        public AnimatedWeaponScroll(int amount)
            : base(683, 0x2DA4, amount)
        {
        }

        public AnimatedWeaponScroll(Serial serial)
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

            /*int version = */
            reader.ReadInt();
        }
    }

    public class BombardScroll : SpellScroll
    {
        [Constructable]
        public BombardScroll()
            : this(1)
        {
        }

        [Constructable]
        public BombardScroll(int amount)
            : base(688, 0x2DA9, amount)
        {
        }

        public BombardScroll(Serial serial)
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

            /*int version = */
            reader.ReadInt();
        }
    }

    public class CleansingWindsScroll : SpellScroll
    {
        [Constructable]
        public CleansingWindsScroll()
            : this(1)
        {
        }

        [Constructable]
        public CleansingWindsScroll(int amount)
            : base(687, 0x2DA8, amount)
        {
        }

        public CleansingWindsScroll(Serial serial)
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

            /*int version = */
            reader.ReadInt();
        }
    }

    public class EagleStrikeScroll : SpellScroll
    {
        [Constructable]
        public EagleStrikeScroll()
            : this(1)
        {
        }

        [Constructable]
        public EagleStrikeScroll(int amount)
            : base(682, 0x2DA3, amount)
        {
        }

        public EagleStrikeScroll(Serial serial)
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

            /*int version = */
            reader.ReadInt();
        }
    }

    public class EnchantScroll : SpellScroll
    {
        [Constructable]
        public EnchantScroll()
            : this(1)
        {
        }

        [Constructable]
        public EnchantScroll(int amount)
            : base(680, 0x2DA1, amount)
        {
        }

        public EnchantScroll(Serial serial)
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

            /*int version = */
            reader.ReadInt();
        }
    }

    public class HailStormScroll : SpellScroll
    {
        [Constructable]
        public HailStormScroll()
            : this(1)
        {
        }

        [Constructable]
        public HailStormScroll(int amount)
            : base(690, 0x2DAB, amount)
        {
        }

        public HailStormScroll(Serial serial)
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

            /*int version = */
            reader.ReadInt();
        }
    }

    public class HealingStoneScroll : SpellScroll
    {
        [Constructable]
        public HealingStoneScroll()
            : this(1)
        {
        }

        [Constructable]
        public HealingStoneScroll(int amount)
            : base(678, 0x2D9F, amount)
        {
        }

        public HealingStoneScroll(Serial serial)
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

            /*int version = */
            reader.ReadInt();
        }
    }

    public class MassSleepScroll : SpellScroll
    {
        [Constructable]
        public MassSleepScroll()
            : this(1)
        {
        }

        [Constructable]
        public MassSleepScroll(int amount)
            : base(686, 0x2DA7, amount)
        {
        }

        public MassSleepScroll(Serial serial)
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

            /*int version = */
            reader.ReadInt();
        }
    }

    public class NetherBoltScroll : SpellScroll
    {
        [Constructable]
        public NetherBoltScroll()
            : this(1)
        {
        }

        [Constructable]
        public NetherBoltScroll(int amount)
            : base(677, 0x2D9E, amount)
        {
        }

        public NetherBoltScroll(Serial serial)
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

            /*int version = */
            reader.ReadInt();
        }
    }

    public class NetherCycloneScroll : SpellScroll
    {
        [Constructable]
        public NetherCycloneScroll()
            : this(1)
        {
        }

        [Constructable]
        public NetherCycloneScroll(int amount)
            : base(691, 0x2DAC, amount)
        {
        }

        public NetherCycloneScroll(Serial serial)
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

            /*int version = */
            reader.ReadInt();
        }
    }

    public class PurgeMagicScroll : SpellScroll
    {
        [Constructable]
        public PurgeMagicScroll()
            : this(1)
        {
        }

        [Constructable]
        public PurgeMagicScroll(int amount)
            : base(679, 0x2DA0, amount)
        {
        }

        public PurgeMagicScroll(Serial serial)
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

            /*int version = */
            reader.ReadInt();
        }
    }

    public class RisingColossusScroll : SpellScroll
    {
        [Constructable]
        public RisingColossusScroll()
            : this(1)
        {
        }

        [Constructable]
        public RisingColossusScroll(int amount)
            : base(692, 0x2DAD, amount)
        {
        }

        public RisingColossusScroll(Serial serial)
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

            /*int version = */
            reader.ReadInt();
        }
    }

    public class SleepScroll : SpellScroll
    {
        [Constructable]
        public SleepScroll()
            : this(1)
        {
        }

        [Constructable]
        public SleepScroll(int amount)
            : base(681, 0x2DA2, amount)
        {
        }

        public SleepScroll(Serial serial)
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

            /*int version = */
            reader.ReadInt();
        }
    }

    public class SpellPlagueScroll : SpellScroll
    {
        [Constructable]
        public SpellPlagueScroll()
            : this(1)
        {
        }

        [Constructable]
        public SpellPlagueScroll(int amount)
            : base(689, 0x2DAA, amount)
        {
        }

        public SpellPlagueScroll(Serial serial)
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

            /*int version = */
            reader.ReadInt();
        }
    }

    public class SpellTriggerScroll : SpellScroll
    {
        [Constructable]
        public SpellTriggerScroll()
            : this(1)
        {
        }

        [Constructable]
        public SpellTriggerScroll(int amount)
            : base(685, 0x2DA6, amount)
        {
        }

        public SpellTriggerScroll(Serial serial)
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

            /*int version = */
            reader.ReadInt();
        }
    }

    public class StoneFormScroll : SpellScroll
    {
        [Constructable]
        public StoneFormScroll()
            : this(1)
        {
        }

        [Constructable]
        public StoneFormScroll(int amount)
            : base(684, 0x2DA5, amount)
        {
        }

        public StoneFormScroll(Serial serial)
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

            /*int version = */
            reader.ReadInt();
        }
    }
}