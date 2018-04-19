using System;

namespace Server.Items
{
    public class SushiRolls : Food
    {
        [Constructable]
        public SushiRolls()
            : base(0x283E)
        {
            Stackable = false;
            Weight = 3.0;
            FillFactor = 2;
        }

        public SushiRolls(Serial serial)
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

    public class SushiPlatter : Food
    {
        [Constructable]
        public SushiPlatter()
            : base(0x2840)
        {
            Stackable = Core.ML;
            Weight = 3.0;
            FillFactor = 2;
        }

        public SushiPlatter(Serial serial)
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