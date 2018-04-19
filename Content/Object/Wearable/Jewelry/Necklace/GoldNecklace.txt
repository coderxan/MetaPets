using System;

namespace Server.Items
{
    public class GoldNecklace : BaseNecklace
    {
        [Constructable]
        public GoldNecklace()
            : base(0x1088)
        {
            Weight = 0.1;
        }

        public GoldNecklace(Serial serial)
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