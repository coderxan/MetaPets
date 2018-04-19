using System;

using Server;

namespace Server.Items
{
    public class BottleArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 1; } }

        [Constructable]
        public BottleArtifact()
            : base(0xE28)
        {
        }

        public BottleArtifact(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }
}