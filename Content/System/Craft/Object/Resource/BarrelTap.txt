using System;

namespace Server.Items
{
    public class BarrelTap : Item
    {
        [Constructable]
        public BarrelTap()
            : base(0x1004)
        {
            Weight = 1;
        }

        public BarrelTap(Serial serial)
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