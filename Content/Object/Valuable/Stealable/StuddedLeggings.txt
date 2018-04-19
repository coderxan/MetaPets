using System;

using Server;

namespace Server.Items
{
    public class StuddedLeggingsArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 5; } }

        [Constructable]
        public StuddedLeggingsArtifact()
            : base(0x13D8)
        {
        }

        public StuddedLeggingsArtifact(Serial serial)
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