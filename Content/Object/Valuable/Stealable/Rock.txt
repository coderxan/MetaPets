using System;

using Server;

namespace Server.Items
{
    public class RockArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 1; } }

        [Constructable]
        public RockArtifact()
            : base(0x1363)
        {
        }

        public RockArtifact(Serial serial)
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