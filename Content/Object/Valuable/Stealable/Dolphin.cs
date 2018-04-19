using System;

using Server;

namespace Server.Items
{
    public class DolphinRightArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 8; } }

        [Constructable]
        public DolphinRightArtifact()
            : base(0x2847)
        {
        }

        public DolphinRightArtifact(Serial serial)
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

    public class DolphinLeftArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 8; } }

        [Constructable]
        public DolphinLeftArtifact()
            : base(0x2846)
        {
        }

        public DolphinLeftArtifact(Serial serial)
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