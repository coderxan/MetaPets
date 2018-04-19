using System;

namespace Server.Items
{
    public class LardOfParoxysmus : Item
    {
        [Constructable]
        public LardOfParoxysmus()
            : this(1)
        {
        }

        [Constructable]
        public LardOfParoxysmus(int amountFrom, int amountTo)
            : this(Utility.RandomMinMax(amountFrom, amountTo))
        {
        }

        [Constructable]
        public LardOfParoxysmus(int amount)
            : base(0x3189)
        {
            Stackable = true;
            Amount = amount;
        }

        public LardOfParoxysmus(Serial serial)
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