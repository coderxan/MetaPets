using System;

using Server;

namespace Server.Items
{
    public class StuddedTunicArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 7; } }

        [Constructable]
        public StuddedTunicArtifact()
            : base(0x13D9)
        {
        }

        public StuddedTunicArtifact(Serial serial)
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