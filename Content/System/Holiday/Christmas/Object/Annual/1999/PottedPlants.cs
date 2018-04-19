using System;

namespace Server.Items
{
    /// <summary>
    /// Empty Planters
    /// </summary>
    public class SmallEmptyPot : Item
    {
        [Constructable]
        public SmallEmptyPot()
            : base(0x11C6)
        {
            Weight = 100;
        }

        public SmallEmptyPot(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class LargeEmptyPot : Item
    {
        [Constructable]
        public LargeEmptyPot()
            : base(0x11C7)
        {
            Weight = 6;
        }

        public LargeEmptyPot(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    /// <summary>
    /// Potted Plants
    /// </summary>
    public class PottedPlant : Item
    {
        [Constructable]
        public PottedPlant()
            : base(0x11CA)
        {
            Weight = 100;
        }

        public PottedPlant(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class PottedPlant1 : Item
    {
        [Constructable]
        public PottedPlant1()
            : base(0x11CB)
        {
            Weight = 100;
        }

        public PottedPlant1(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class PottedPlant2 : Item
    {
        [Constructable]
        public PottedPlant2()
            : base(0x11CC)
        {
            Weight = 100;
        }

        public PottedPlant2(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    /// <summary>
    /// Potted Trees
    /// </summary>
    public class PottedTree : Item
    {
        [Constructable]
        public PottedTree()
            : base(0x11C8)
        {
            Weight = 100;
        }

        public PottedTree(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class PottedTree1 : Item
    {
        [Constructable]
        public PottedTree1()
            : base(0x11C9)
        {
            Weight = 100;
        }

        public PottedTree1(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    /// <summary>
    /// Potted Cacti
    /// </summary>
    public class PottedCactus : Item
    {
        [Constructable]
        public PottedCactus()
            : base(0x1E0F)
        {
            Weight = 100;
        }

        public PottedCactus(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class PottedCactus1 : Item
    {
        [Constructable]
        public PottedCactus1()
            : base(0x1E10)
        {
            Weight = 100;
        }

        public PottedCactus1(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class PottedCactus2 : Item
    {
        [Constructable]
        public PottedCactus2()
            : base(0x1E11)
        {
            Weight = 100;
        }

        public PottedCactus2(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class PottedCactus3 : Item
    {
        [Constructable]
        public PottedCactus3()
            : base(0x1E12)
        {
            Weight = 100;
        }

        public PottedCactus3(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class PottedCactus4 : Item
    {
        [Constructable]
        public PottedCactus4()
            : base(0x1E13)
        {
            Weight = 100;
        }

        public PottedCactus4(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class PottedCactus5 : Item
    {
        [Constructable]
        public PottedCactus5()
            : base(0x1E14)
        {
            Weight = 100;
        }

        public PottedCactus5(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}