using System;
using System.Collections.Generic;

using Server;
using Server.Multis;
using Server.Network;

namespace Server.Items
{
    [Furniture]
    [Flipable(0xa4f, 0xa53)]
    public class Armoire : BaseContainer
    {
        [Constructable]
        public Armoire()
            : base(0xA4F)
        {
            Weight = 1.0;
        }

        public override void DisplayTo(Mobile m)
        {
            if (DynamicFurniture.Open(this, m))
                base.DisplayTo(m);
        }

        public Armoire(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            DynamicFurniture.Close(this);
        }
    }

    [Furniture]
    [Flipable(0x285D, 0x285E)]
    public class CherryArmoire : BaseContainer
    {
        [Constructable]
        public CherryArmoire()
            : base(0x285D)
        {
            Weight = 1.0;
        }

        public CherryArmoire(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }

    [Furniture]
    [Flipable(0x285B, 0x285C)]
    public class MapleArmoire : BaseContainer
    {
        [Constructable]
        public MapleArmoire()
            : base(0x285B)
        {
            Weight = 1.0;
        }

        public MapleArmoire(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }

    [Furniture]
    [Flipable(0x2857, 0x2858)]
    public class RedArmoire : BaseContainer
    {
        [Constructable]
        public RedArmoire()
            : base(0x2857)
        {
            Weight = 1.0;
        }

        public RedArmoire(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }

    [Furniture]
    [Flipable(0xa4d, 0xa51)]
    public class FancyArmoire : BaseContainer
    {
        [Constructable]
        public FancyArmoire()
            : base(0xA4D)
        {
            Weight = 1.0;
        }

        public override void DisplayTo(Mobile m)
        {
            if (DynamicFurniture.Open(this, m))
                base.DisplayTo(m);
        }

        public FancyArmoire(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            DynamicFurniture.Close(this);
        }
    }

    [Furniture]
    [Flipable(0x2859, 0x285A)]
    public class ElegantArmoire : BaseContainer
    {
        [Constructable]
        public ElegantArmoire()
            : base(0x2859)
        {
            Weight = 1.0;
        }

        public ElegantArmoire(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}