using System;

using Server.Mobiles;
using Server.Targeting;

namespace Server.Items
{
    public class ValentinesCardEast : ValentinesCard
    {
        [Constructable]
        public ValentinesCardEast()
            : base(0x0EBE)
        {
        }

        public ValentinesCardEast(Serial serial)
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

    public class ValentinesCardSouth : ValentinesCard
    {
        [Constructable]
        public ValentinesCardSouth()
            : base(0x0EBD)
        {
        }

        public ValentinesCardSouth(Serial serial)
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