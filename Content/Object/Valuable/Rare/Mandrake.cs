using System;

namespace Server.Items
{
    /// <summary>
    /// Facing North-South
    /// </summary>
    public class DecoMandrake2 : Item
    {
        [Constructable]
        public DecoMandrake2()
            : base(0x18E0)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoMandrake2(Serial serial)
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

    /// <summary>
    /// Facing East-West
    /// </summary>
    public class DecoMandrake : Item
    {
        [Constructable]
        public DecoMandrake()
            : base(0x18DF)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoMandrake(Serial serial)
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

    public class DecoMandrake3 : Item
    {
        [Constructable]
        public DecoMandrake3()
            : base(0x18DF)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoMandrake3(Serial serial)
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