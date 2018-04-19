using System;

using Server;

namespace Server.Items
{
    public class SaddleArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 9; } }

        [Constructable]
        public SaddleArtifact()
            : base(0xF38)
        {
        }

        public SaddleArtifact(Serial serial)
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