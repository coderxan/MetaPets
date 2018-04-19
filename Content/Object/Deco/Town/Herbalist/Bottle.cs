using System;

using Server.Network;

namespace Server.Items
{
    public class GreenBottle : Item
    {
        [Constructable]
        public GreenBottle()
            : base(0x0EFB)
        {
            Weight = 1.0;
            Movable = true;
        }

        public GreenBottle(Serial serial)
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

    public class RedBottle : Item
    {
        [Constructable]
        public RedBottle()
            : base(0x0EFC)
        {
            Weight = 1.0;
            Movable = true;
        }

        public RedBottle(Serial serial)
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

    public class SmallBrownBottle : Item
    {
        [Constructable]
        public SmallBrownBottle()
            : base(0x0EFD)
        {
            Weight = 1.0;
            Movable = true;
        }

        public SmallBrownBottle(Serial serial)
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

    public class SmallGreenBottle : Item
    {
        [Constructable]
        public SmallGreenBottle()
            : base(0x0F01)
        {
            Weight = 1.0;
            Movable = true;
        }

        public SmallGreenBottle(Serial serial)
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

    public class SmallVioletBottle : Item
    {
        [Constructable]
        public SmallVioletBottle()
            : base(0x0F02)
        {
            Weight = 1.0;
            Movable = true;
        }

        public SmallVioletBottle(Serial serial)
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

    public class TinyYellowBottle : Item
    {
        [Constructable]
        public TinyYellowBottle()
            : base(0x0F03)
        {
            Weight = 1.0;
            Movable = true;
        }

        public TinyYellowBottle(Serial serial)
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

    public class SmallBlueBottle : Item
    {
        [Constructable]
        public SmallBlueBottle()
            : base(0x1847)
        {
            Weight = 1.0;
            Movable = true;
        }

        public SmallBlueBottle(Serial serial)
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

    public class SmallGreenBottle2 : Item
    {
        [Constructable]
        public SmallGreenBottle2()
            : base(0x1848)
        {
            Weight = 1.0;
            Movable = true;
        }

        public SmallGreenBottle2(Serial serial)
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

    public class TinyRedBottle : Item
    {
        [Constructable]
        public TinyRedBottle()
            : base(0x0F04)
        {
            Weight = 1.0;
            Movable = true;
        }

        public TinyRedBottle(Serial serial)
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