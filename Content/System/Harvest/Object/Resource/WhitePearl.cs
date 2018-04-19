using System;

namespace Server.Items
{
    public class WhitePearl : Item
    {
        [Constructable]
        public WhitePearl()
            : this(1)
        {
        }

        [Constructable]
        public WhitePearl(int amountFrom, int amountTo)
            : this(Utility.RandomMinMax(amountFrom, amountTo))
        {
        }

        [Constructable]
        public WhitePearl(int amount)
            : base(0x3196)
        {
            Stackable = true;
            Amount = amount;
        }

        public WhitePearl(Serial serial)
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