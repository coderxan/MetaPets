using System;

using Server.Items;
using Server.Network;
using Server.Targeting;

namespace Server.Items
{
    public class RawRibs : CookableFood
    {
        [Constructable]
        public RawRibs()
            : this(1)
        {
        }

        [Constructable]
        public RawRibs(int amount)
            : base(0x9F1, 10)
        {
            Weight = 1.0;
            Stackable = true;
            Amount = amount;
        }

        public RawRibs(Serial serial)
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
            return new Ribs();
        }
    }

    public class RawLambLeg : CookableFood
    {
        [Constructable]
        public RawLambLeg()
            : this(1)
        {
        }

        [Constructable]
        public RawLambLeg(int amount)
            : base(0x1609, 10)
        {
            Stackable = true;
            Amount = amount;
        }

        public RawLambLeg(Serial serial)
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

            if (version == 0 && Weight == 1)
                Weight = -1;
        }

        public override Food Cook()
        {
            return new LambLeg();
        }
    }

    public class RawChickenLeg : CookableFood
    {
        [Constructable]
        public RawChickenLeg()
            : base(0x1607, 10)
        {
            Weight = 1.0;
            Stackable = true;
        }

        public RawChickenLeg(Serial serial)
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
            return new ChickenLeg();
        }
    }

    public class RawBird : CookableFood
    {
        [Constructable]
        public RawBird()
            : this(1)
        {
        }

        [Constructable]
        public RawBird(int amount)
            : base(0x9B9, 10)
        {
            Weight = 1.0;
            Stackable = true;
            Amount = amount;
        }

        public RawBird(Serial serial)
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
            return new CookedBird();
        }
    }

    public class RawFishSteak : CookableFood
    {
        public override double DefaultWeight
        {
            get { return 0.1; }
        }

        [Constructable]
        public RawFishSteak()
            : this(1)
        {
        }

        [Constructable]
        public RawFishSteak(int amount)
            : base(0x097A, 10)
        {
            Stackable = true;
            Amount = amount;
        }

        public RawFishSteak(Serial serial)
            : base(serial)
        {
        }

        public override Food Cook()
        {
            return new FishSteak();
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