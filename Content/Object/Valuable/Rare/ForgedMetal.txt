using System;

namespace Server.Items
{
    public class ForgedMetal : Item
    {
        [Constructable]
        public ForgedMetal()
            : base(0xFB8)
        {
            Weight = 5.0;
        }

        public ForgedMetal(Serial serial)
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