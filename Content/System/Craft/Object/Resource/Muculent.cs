using System;

namespace Server.Items
{
    public class Muculent : Item
    {
        [Constructable]
        public Muculent()
            : this(1)
        {
        }

        [Constructable]
        public Muculent(int amountFrom, int amountTo)
            : this(Utility.RandomMinMax(amountFrom, amountTo))
        {
        }

        [Constructable]
        public Muculent(int amount)
            : base(0x3188)
        {
            Stackable = true;
            Amount = amount;
        }

        public Muculent(Serial serial)
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