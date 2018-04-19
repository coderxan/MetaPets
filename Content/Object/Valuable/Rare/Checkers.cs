using System;

namespace Server.Items
{
    public class Checkers : Item
    {

        [Constructable]
        public Checkers()
            : base(0xE1A)
        {
            Movable = true;
            Stackable = false;
        }

        public Checkers(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class Checkers2 : Item
    {

        [Constructable]
        public Checkers2()
            : base(0xE1B)
        {
            Movable = true;
            Stackable = false;
        }

        public Checkers2(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}