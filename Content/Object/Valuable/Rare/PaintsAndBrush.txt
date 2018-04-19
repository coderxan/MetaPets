using System;

namespace Server.Items
{
    public class PaintsAndBrush : Item
    {
        [Constructable]
        public PaintsAndBrush()
            : base(0xFC1)
        {
            Weight = 1.0;
        }

        public PaintsAndBrush(Serial serial)
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