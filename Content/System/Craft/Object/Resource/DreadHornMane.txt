using System;

namespace Server.Items
{
    public class DreadHornMane : Item
    {
        [Constructable]
        public DreadHornMane()
            : this(1)
        {
        }

        [Constructable]
        public DreadHornMane(int amountFrom, int amountTo)
            : this(Utility.RandomMinMax(amountFrom, amountTo))
        {
        }

        [Constructable]
        public DreadHornMane(int amount)
            : base(0x318A)
        {
            Stackable = true;
            Amount = amount;
        }

        public DreadHornMane(Serial serial)
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