using System;

namespace Server.Items
{
    public class Taint : Item
    {
        [Constructable]
        public Taint()
            : this(1)
        {
        }

        [Constructable]
        public Taint(int amountFrom, int amountTo)
            : this(Utility.RandomMinMax(amountFrom, amountTo))
        {
        }

        [Constructable]
        public Taint(int amount)
            : base(0x3187)
        {
            Stackable = true;
            Amount = amount;
            Hue = 731;
        }

        public Taint(Serial serial)
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