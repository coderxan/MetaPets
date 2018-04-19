using System;

namespace Server.Items
{
    [FlipableAttribute(0x1EB1, 0x1EB2, 0x1EB3, 0x1EB4)]
    public class BarrelStaves : Item
    {
        [Constructable]
        public BarrelStaves()
            : base(0x1EB1)
        {
            Weight = 1;
        }

        public BarrelStaves(Serial serial)
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