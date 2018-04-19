using System;

namespace Server.Items
{
    public class SilverNecklace : BaseNecklace
    {
        [Constructable]
        public SilverNecklace()
            : base(0x1F08)
        {
            Weight = 0.1;
        }

        public SilverNecklace(Serial serial)
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