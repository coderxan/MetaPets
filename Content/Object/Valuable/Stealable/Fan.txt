using System;

using Server;

namespace Server.Items
{
    public class FanNorthArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 3; } }

        [Constructable]
        public FanNorthArtifact()
            : base(0x2409)
        {
        }

        public FanNorthArtifact(Serial serial)
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

    public class FanWestArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 3; } }

        [Constructable]
        public FanWestArtifact()
            : base(0x240A)
        {
        }

        public FanWestArtifact(Serial serial)
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