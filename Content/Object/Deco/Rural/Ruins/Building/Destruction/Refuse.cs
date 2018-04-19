using System;

using Server.Network;

namespace Server.Items
{
    [FlipableAttribute(0xC2D, 0xC2F, 0xC2E, 0xC30)]
    public class RottedWoodDebris : Item
    {
        [Constructable]
        public RottedWoodDebris()
            : base(0xC2D)
        {
            Movable = false;
        }

        public RottedWoodDebris(Serial serial)
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