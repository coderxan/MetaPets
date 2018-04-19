using System;

namespace Server.Items
{
    public class SilverEarrings : BaseEarrings
    {
        [Constructable]
        public SilverEarrings()
            : base(0x1F07)
        {
            Weight = 0.1;
        }

        public SilverEarrings(Serial serial)
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