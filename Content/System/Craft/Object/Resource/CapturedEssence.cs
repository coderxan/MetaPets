using System;

namespace Server.Items
{
    public class CapturedEssence : Item
    {
        [Constructable]
        public CapturedEssence()
            : this(1)
        {
        }

        [Constructable]
        public CapturedEssence(int amount)
            : base(0x318E)
        {
            Stackable = true;
            Amount = amount;
        }

        public CapturedEssence(Serial serial)
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