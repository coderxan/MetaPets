using System;

using Server.Network;

namespace Server.Items
{
    public class RuinedPainting : Item
    {
        [Constructable]
        public RuinedPainting()
            : base(0xC2C)
        {
            Movable = false;
        }

        public RuinedPainting(Serial serial)
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