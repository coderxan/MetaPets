using System;

using Server;

namespace Server.Items
{
    public class RuinedPaintingArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 12; } }

        [Constructable]
        public RuinedPaintingArtifact()
            : base(0xC2C)
        {
        }

        public RuinedPaintingArtifact(Serial serial)
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