using System;

namespace Server.Items
{
    /// <summary>
    /// Facing North-South
    /// </summary>
    public class DecoGarlic : Item
    {
        [Constructable]
        public DecoGarlic()
            : base(0x18E1)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoGarlic(Serial serial)
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
    public class DecoGarlic2 : Item
    {
        [Constructable]
        public DecoGarlic2()
            : base(0x18E2)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoGarlic2(Serial serial)
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