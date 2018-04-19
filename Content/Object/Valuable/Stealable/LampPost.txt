using System;

using Server;

namespace Server.Items
{
    public class LampPostArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 3; } }

        [Constructable]
        public LampPostArtifact()
            : base(0xB24)
        {
            Light = LightType.Circle300;
        }

        public LampPostArtifact(Serial serial)
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