using System;

namespace Server.Items
{
    public class WhiteDriedFlowers : Item
    {
        [Constructable]
        public WhiteDriedFlowers()
            : this(1)
        {
        }

        [Constructable]
        public WhiteDriedFlowers(int amount)
            : base(0xC3C)
        {
            Stackable = true;
            Weight = 1.0;
            Amount = amount;
        }

        public WhiteDriedFlowers(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}