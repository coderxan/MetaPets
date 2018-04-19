using System;

namespace Server.Items
{
    public class EyeOfTheTravesty : Item
    {
        [Constructable]
        public EyeOfTheTravesty()
            : this(1)
        {
        }

        [Constructable]
        public EyeOfTheTravesty(int amountFrom, int amountTo)
            : this(Utility.RandomMinMax(amountFrom, amountTo))
        {
        }

        [Constructable]
        public EyeOfTheTravesty(int amount)
            : base(0x318D)
        {
            Stackable = true;
            Amount = amount;
        }

        public EyeOfTheTravesty(Serial serial)
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