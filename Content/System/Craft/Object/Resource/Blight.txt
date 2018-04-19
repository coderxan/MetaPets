using System;

namespace Server.Items
{
    public class Blight : Item
    {
        [Constructable]
        public Blight()
            : this(1)
        {
        }

        [Constructable]
        public Blight(int amount)
            : base(0x3183)
        {
            Stackable = true;
            Amount = amount;
        }

        public Blight(Serial serial)
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