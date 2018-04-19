using System;

namespace Server.Items
{
    /// <summary>
    /// Facing North-South
    /// </summary>
    public class DecoGarlicBulb : Item
    {
        [Constructable]
        public DecoGarlicBulb()
            : base(0x18E3)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoGarlicBulb(Serial serial)
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
    public class DecoGarlicBulb2 : Item
    {
        [Constructable]
        public DecoGarlicBulb2()
            : base(0x18E4)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoGarlicBulb2(Serial serial)
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