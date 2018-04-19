using System;

using Server;

namespace Server.Items
{
    public class SwordDisplay1NorthArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 5; } }

        [Constructable]
        public SwordDisplay1NorthArtifact()
            : base(0x2843)
        {
        }

        public SwordDisplay1NorthArtifact(Serial serial)
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

    public class SwordDisplay1WestArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 5; } }

        [Constructable]
        public SwordDisplay1WestArtifact()
            : base(0x2842)
        {
        }

        public SwordDisplay1WestArtifact(Serial serial)
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

    public class SwordDisplay2NorthArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 6; } }

        [Constructable]
        public SwordDisplay2NorthArtifact()
            : base(0x2845)
        {
        }

        public SwordDisplay2NorthArtifact(Serial serial)
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

    public class SwordDisplay2WestArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 6; } }

        [Constructable]
        public SwordDisplay2WestArtifact()
            : base(0x2844)
        {
        }

        public SwordDisplay2WestArtifact(Serial serial)
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

    public class SwordDisplay3EastArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 8; } }

        [Constructable]
        public SwordDisplay3EastArtifact()
            : base(0x2856)
        {
        }

        public SwordDisplay3EastArtifact(Serial serial)
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

    public class SwordDisplay3SouthArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 8; } }

        [Constructable]
        public SwordDisplay3SouthArtifact()
            : base(0x2855)
        {
        }

        public SwordDisplay3SouthArtifact(Serial serial)
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

    public class SwordDisplay4NorthArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 9; } }

        [Constructable]
        public SwordDisplay4NorthArtifact()
            : base(0x2854)
        {
        }

        public SwordDisplay4NorthArtifact(Serial serial)
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

    public class SwordDisplay4WestArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 8; } }

        [Constructable]
        public SwordDisplay4WestArtifact()
            : base(0x2853)
        {
        }

        public SwordDisplay4WestArtifact(Serial serial)
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

    public class SwordDisplay5NorthArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 9; } }

        [Constructable]
        public SwordDisplay5NorthArtifact()
            : base(0x2852)
        {
        }

        public SwordDisplay5NorthArtifact(Serial serial)
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

    public class SwordDisplay5WestArtifact : BaseDecorationArtifact
    {
        public override int ArtifactRarity { get { return 9; } }

        [Constructable]
        public SwordDisplay5WestArtifact()
            : base(0x2851)
        {
        }

        public SwordDisplay5WestArtifact(Serial serial)
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