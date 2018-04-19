using System;

using Server;

namespace Server.Items
{
    public class ZenRock1Artifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 2; } }

        [Constructable]
        public ZenRock1Artifact()
            : base(0x24E4)
        {
        }

        public ZenRock1Artifact(Serial serial)
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

    public class ZenRock2Artifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 3; } }

        [Constructable]
        public ZenRock2Artifact()
            : base(0x24E3)
        {
        }

        public ZenRock2Artifact(Serial serial)
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

    public class ZenRock3Artifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 3; } }

        [Constructable]
        public ZenRock3Artifact()
            : base(0x24E5)
        {
        }

        public ZenRock3Artifact(Serial serial)
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