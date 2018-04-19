using System;

using Server;

namespace Server.Items
{
    public class SkinnedDeerArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 8; } }

        [Constructable]
        public SkinnedDeerArtifact()
            : base(0x1E91)
        {
        }

        public SkinnedDeerArtifact(Serial serial)
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