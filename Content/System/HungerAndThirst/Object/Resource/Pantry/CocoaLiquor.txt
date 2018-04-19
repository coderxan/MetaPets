using System;

using Server;

namespace Server.Items
{
    public class CocoaLiquor : Item
    {
        public override int LabelNumber { get { return 1080007; } } // Cocoa liquor
        public override double DefaultWeight { get { return 1.0; } }

        [Constructable]
        public CocoaLiquor()
            : base(0x103F)
        {
            Hue = 0x46A;
        }

        public CocoaLiquor(Serial serial)
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