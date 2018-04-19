using System;

using Server.Network;

namespace Server.Items
{
    [FlipableAttribute(0xC13, 0xC12)]
    public class RuinedArmoire : Item
    {
        [Constructable]
        public RuinedArmoire()
            : base(0xC13)
        {
            Movable = false;
        }

        public RuinedArmoire(Serial serial)
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