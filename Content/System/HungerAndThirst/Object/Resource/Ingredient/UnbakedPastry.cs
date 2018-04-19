using System;

using Server.Items;
using Server.Network;
using Server.Targeting;

namespace Server.Items
{
    public class UnbakedPeachCobbler : CookableFood
    {
        public override int LabelNumber { get { return 1041335; } } // unbaked peach cobbler

        [Constructable]
        public UnbakedPeachCobbler()
            : base(0x1042, 25)
        {
            Weight = 1.0;
        }

        public UnbakedPeachCobbler(Serial serial)
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
            return new PeachCobbler();
        }
    }

    public class UnbakedFruitPie : CookableFood
    {
        public override int LabelNumber { get { return 1041334; } } // unbaked fruit pie

        [Constructable]
        public UnbakedFruitPie()
            : base(0x1042, 25)
        {
            Weight = 1.0;
        }

        public UnbakedFruitPie(Serial serial)
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
            return new FruitPie();
        }
    }

    public class UnbakedMeatPie : CookableFood
    {
        public override int LabelNumber { get { return 1041338; } } // unbaked meat pie

        [Constructable]
        public UnbakedMeatPie()
            : base(0x1042, 25)
        {
            Weight = 1.0;
        }

        public UnbakedMeatPie(Serial serial)
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
            return new MeatPie();
        }
    }

    public class UnbakedPumpkinPie : CookableFood
    {
        public override int LabelNumber { get { return 1041342; } } // unbaked pumpkin pie

        [Constructable]
        public UnbakedPumpkinPie()
            : base(0x1042, 25)
        {
            Weight = 1.0;
        }

        public UnbakedPumpkinPie(Serial serial)
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
            return new PumpkinPie();
        }
    }

    public class UnbakedApplePie : CookableFood
    {
        public override int LabelNumber { get { return 1041336; } } // unbaked apple pie

        [Constructable]
        public UnbakedApplePie()
            : base(0x1042, 25)
        {
            Weight = 1.0;
        }

        public UnbakedApplePie(Serial serial)
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
            return new ApplePie();
        }
    }

    public class UnbakedQuiche : CookableFood
    {
        public override int LabelNumber { get { return 1041339; } } // unbaked quiche

        [Constructable]
        public UnbakedQuiche()
            : base(0x1042, 25)
        {
            Weight = 1.0;
        }

        public UnbakedQuiche(Serial serial)
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
            return new Quiche();
        }
    }
}