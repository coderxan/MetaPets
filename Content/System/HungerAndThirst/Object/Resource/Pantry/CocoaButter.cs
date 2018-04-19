using System;

using Server;

namespace Server.Items
{
    public class CocoaButter : Item
    {
        public override int LabelNumber { get { return 1080005; } } // Cocoa butter
        public override double DefaultWeight { get { return 1.0; } }

        [Constructable]
        public CocoaButter()
            : base(0x1044)
        {
            Hue = 0x457;
        }

        public CocoaButter(Serial serial)
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