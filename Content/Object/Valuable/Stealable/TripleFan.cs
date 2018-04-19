using System;

using Server;

namespace Server.Items
{
    public class TripleFanNorthArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 4; } }

        [Constructable]
        public TripleFanNorthArtifact()
            : base(0x240B)
        {
        }

        public TripleFanNorthArtifact(Serial serial)
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

    public class TripleFanWestArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 4; } }

        [Constructable]
        public TripleFanWestArtifact()
            : base(0x240C)
        {
        }

        public TripleFanWestArtifact(Serial serial)
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