using System;

using Server.Items;
using Server.Network;
using Server.Targeting;

namespace Server.Items
{
    public class Eggs : CookableFood
    {
        [Constructable]
        public Eggs()
            : this(1)
        {
        }

        [Constructable]
        public Eggs(int amount)
            : base(0x9B5, 15)
        {
            Weight = 1.0;
            Stackable = true;
            Amount = amount;
        }

        public Eggs(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version < 1)
            {
                Stackable = true;

                if (Weight == 0.5)
                    Weight = 1.0;
            }
        }

        public override Food Cook()
        {
            return new FriedEggs();
        }
    }

    public class BrightlyColoredEggs : CookableFood
    {
        public override string DefaultName
        {
            get { return "brightly colored eggs"; }
        }

        [Constructable]
        public BrightlyColoredEggs()
            : base(0x9B5, 15)
        {
            Weight = 0.5;
            Hue = 3 + (Utility.Random(20) * 5);
        }

        public BrightlyColoredEggs(Serial serial)
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

        public override Food Cook()
        {
            return new FriedEggs();
        }
    }

    public class EasterEggs : CookableFood
    {
        public override int LabelNumber { get { return 1016105; } } // Easter Eggs

        [Constructable]
        public EasterEggs()
            : base(0x9B5, 15)
        {
            Weight = 0.5;
            Hue = 3 + (Utility.Random(20) * 5);
        }

        public EasterEggs(Serial serial)
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

        public override Food Cook()
        {
            return new FriedEggs();
        }
    }

    public class Eggshells : Item
    {
        [Constructable]
        public Eggshells()
            : base(0x9b4)
        {
            Weight = 0.5;
        }

        public Eggshells(Serial serial)
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