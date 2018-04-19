using System;

namespace Server.Items
{
    public class SwitchItem : Item
    {
        [Constructable]
        public SwitchItem()
            : this(1)
        {
        }

        [Constructable]
        public SwitchItem(int amountFrom, int amountTo)
            : this(Utility.RandomMinMax(amountFrom, amountTo))
        {
        }

        [Constructable]
        public SwitchItem(int amount)
            : base(0x2F5F)
        {
            Name = "Switch";
            Stackable = true;
            Amount = amount;
        }

        public SwitchItem(Serial serial)
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