using System;

using Server;

namespace Server.Items
{
    public class BowlArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 4; } }

        [Constructable]
        public BowlArtifact()
            : base(0x24DE)
        {
        }

        public BowlArtifact(Serial serial)
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