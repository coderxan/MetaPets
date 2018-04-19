using System;

using Server.Network;

namespace Server.Items
{
    public class RuinedBooks : Item
    {
        [Constructable]
        public RuinedBooks()
            : base(0xC16)
        {
            Movable = false;
        }

        public RuinedBooks(Serial serial)
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