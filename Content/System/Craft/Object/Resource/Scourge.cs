using System;

namespace Server.Items
{
    public class Scourge : Item
    {
        [Constructable]
        public Scourge()
            : this(1)
        {
        }

        [Constructable]
        public Scourge(int amountFrom, int amountTo)
            : this(Utility.RandomMinMax(amountFrom, amountTo))
        {
        }

        [Constructable]
        public Scourge(int amount)
            : base(0x3185)
        {
            Stackable = true;
            Amount = amount;
            Hue = 150;
        }

        public Scourge(Serial serial)
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