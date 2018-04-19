using System;

using Server;

namespace Server.Items
{
    public class BowlsHorizontalArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 4; } }

        [Constructable]
        public BowlsHorizontalArtifact()
            : base(0x24E0)
        {
        }

        public BowlsHorizontalArtifact(Serial serial)
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