using System;

namespace Server.Items
{
    public class DarkSapphire : Item
    {
        [Constructable]
        public DarkSapphire()
            : this(1)
        {
        }

        [Constructable]
        public DarkSapphire(int amountFrom, int amountTo)
            : this(Utility.RandomMinMax(amountFrom, amountTo))
        {
        }

        [Constructable]
        public DarkSapphire(int amount)
            : base(0x3192)
        {
            Stackable = true;
            Amount = amount;
        }

        public DarkSapphire(Serial serial)
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