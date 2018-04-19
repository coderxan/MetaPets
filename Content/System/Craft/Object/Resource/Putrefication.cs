using System;

namespace Server.Items
{
    public class Putrefication : Item
    {
        [Constructable]
        public Putrefication()
            : this(1)
        {
        }

        [Constructable]
        public Putrefication(int amountFrom, int amountTo)
            : this(Utility.RandomMinMax(amountFrom, amountTo))
        {
        }

        [Constructable]
        public Putrefication(int amount)
            : base(0x3186)
        {
            Stackable = true;
            Amount = amount;
            Hue = 883;
        }

        public Putrefication(Serial serial)
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