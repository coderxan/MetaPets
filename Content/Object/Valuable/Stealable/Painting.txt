using System;

using Server;

namespace Server.Items
{
    public class Painting1NorthArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 4; } }

        [Constructable]
        public Painting1NorthArtifact()
            : base(0x240D)
        {
        }

        public Painting1NorthArtifact(Serial serial)
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

    public class Painting1WestArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 4; } }

        [Constructable]
        public Painting1WestArtifact()
            : base(0x240E)
        {
        }

        public Painting1WestArtifact(Serial serial)
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

    public class Painting2NorthArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 4; } }

        [Constructable]
        public Painting2NorthArtifact()
            : base(0x240F)
        {
        }

        public Painting2NorthArtifact(Serial serial)
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

    public class Painting2WestArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 4; } }

        [Constructable]
        public Painting2WestArtifact()
            : base(0x2410)
        {
        }

        public Painting2WestArtifact(Serial serial)
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

    public class Painting3Artifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 5; } }

        [Constructable]
        public Painting3Artifact()
            : base(0x2411)
        {
        }

        public Painting3Artifact(Serial serial)
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

    public class Painting4NorthArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 6; } }

        [Constructable]
        public Painting4NorthArtifact()
            : base(0x2411)
        {
        }

        public Painting4NorthArtifact(Serial serial)
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

    public class Painting4WestArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 6; } }

        [Constructable]
        public Painting4WestArtifact()
            : base(0x2412)
        {
        }

        public Painting4WestArtifact(Serial serial)
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

    public class Painting5NorthArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 8; } }

        [Constructable]
        public Painting5NorthArtifact()
            : base(0x2415)
        {
        }

        public Painting5NorthArtifact(Serial serial)
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

    public class Painting5WestArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 8; } }

        [Constructable]
        public Painting5WestArtifact()
            : base(0x2416)
        {
        }

        public Painting5WestArtifact(Serial serial)
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

    public class Painting6NorthArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 9; } }

        [Constructable]
        public Painting6NorthArtifact()
            : base(0x2417)
        {
        }

        public Painting6NorthArtifact(Serial serial)
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

    public class Painting6WestArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 9; } }

        [Constructable]
        public Painting6WestArtifact()
            : base(0x2418)
        {
        }

        public Painting6WestArtifact(Serial serial)
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