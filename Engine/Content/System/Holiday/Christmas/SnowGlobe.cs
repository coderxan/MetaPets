using System;

using Server;

namespace Server.Items
{
    public class SnowGlobe : Item
    {
        public override double DefaultWeight { get { return 1.0; } }

        public SnowGlobe()
            : base(0xE2F)
        {
            LootType = LootType.Blessed;
            Light = LightType.Circle150;
        }

        public SnowGlobe(Serial serial)
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