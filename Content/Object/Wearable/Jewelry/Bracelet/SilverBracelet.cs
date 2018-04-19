using System;

namespace Server.Items
{
    public class SilverBracelet : BaseBracelet
    {
        [Constructable]
        public SilverBracelet()
            : base(0x1F06)
        {
            Weight = 0.1;
        }

        public SilverBracelet(Serial serial)
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