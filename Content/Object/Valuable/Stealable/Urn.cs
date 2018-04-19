using System;

using Server;

namespace Server.Items
{
    public class Urn1Artifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 3; } }

        [Constructable]
        public Urn1Artifact()
            : base(0x241D)
        {
        }

        public Urn1Artifact(Serial serial)
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

    public class Urn2Artifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 3; } }

        [Constructable]
        public Urn2Artifact()
            : base(0x241E)
        {
        }

        public Urn2Artifact(Serial serial)
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