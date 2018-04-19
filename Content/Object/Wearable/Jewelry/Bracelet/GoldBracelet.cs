using System;

namespace Server.Items
{
    public class GoldBracelet : BaseBracelet
    {
        [Constructable]
        public GoldBracelet()
            : base(0x1086)
        {
            Weight = 0.1;
        }

        public GoldBracelet(Serial serial)
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