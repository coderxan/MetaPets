using System;

namespace Server.Items
{
    public class Turquoise : Item
    {
        [Constructable]
        public Turquoise()
            : this(1)
        {
        }

        [Constructable]
        public Turquoise(int amountFrom, int amountTo)
            : this(Utility.RandomMinMax(amountFrom, amountTo))
        {
        }

        [Constructable]
        public Turquoise(int amount)
            : base(0x3193)
        {
            Stackable = true;
            Amount = amount;
        }

        public Turquoise(Serial serial)
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