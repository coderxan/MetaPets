using System;

using Server.Network;

namespace Server.Items
{
    [FlipableAttribute(0xC14, 0xC15)]
    public class RuinedBookcase : Item
    {
        [Constructable]
        public RuinedBookcase()
            : base(0xC14)
        {
            Movable = false;
        }

        public RuinedBookcase(Serial serial)
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