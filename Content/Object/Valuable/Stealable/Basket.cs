using System;

using Server;

namespace Server.Items
{
    public class Basket1Artifact : BaseDecorationContainerArtifact
    {
        public override int ArtifactRarity { get { return 1; } }

        [Constructable]
        public Basket1Artifact()
            : base(0x24DD)
        {
        }

        public Basket1Artifact(Serial serial)
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

    public class Basket2Artifact : BaseDecorationContainerArtifact
    {
        public override int ArtifactRarity { get { return 1; } }

        [Constructable]
        public Basket2Artifact()
            : base(0x24D7)
        {
        }

        public Basket2Artifact(Serial serial)
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

    public class Basket3NorthArtifact : BaseDecorationContainerArtifact
    {
        public override int ArtifactRarity { get { return 1; } }

        [Constructable]
        public Basket3NorthArtifact()
            : base(0x24DA)
        {
        }

        public Basket3NorthArtifact(Serial serial)
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

    public class Basket3WestArtifact : BaseDecorationContainerArtifact
    {
        public override int ArtifactRarity { get { return 1; } }

        [Constructable]
        public Basket3WestArtifact()
            : base(0x24D9)
        {
        }

        public Basket3WestArtifact(Serial serial)
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

    public class Basket4Artifact : BaseDecorationContainerArtifact
    {
        public override int ArtifactRarity { get { return 2; } }

        [Constructable]
        public Basket4Artifact()
            : base(0x24D8)
        {
        }

        public Basket4Artifact(Serial serial)
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

    public class Basket5NorthArtifact : BaseDecorationContainerArtifact
    {
        public override int ArtifactRarity { get { return 2; } }

        [Constructable]
        public Basket5NorthArtifact()
            : base(0x24DB)
        {
        }

        public Basket5NorthArtifact(Serial serial)
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

    public class Basket5WestArtifact : BaseDecorationContainerArtifact
    {
        public override int ArtifactRarity { get { return 2; } }

        [Constructable]
        public Basket5WestArtifact()
            : base(0x24DC)
        {
        }

        public Basket5WestArtifact(Serial serial)
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

    public class Basket6Artifact : BaseDecorationContainerArtifact
    {
        public override int ArtifactRarity { get { return 2; } }

        [Constructable]
        public Basket6Artifact()
            : base(0x24D5)
        {
        }

        public Basket6Artifact(Serial serial)
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