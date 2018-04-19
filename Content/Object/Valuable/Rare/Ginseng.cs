using System;

namespace Server.Items
{
    /// <summary>
    /// Facing North-South
    /// </summary>
    public class DecoGinseng2 : Item
    {
        [Constructable]
        public DecoGinseng2()
            : base(0x18EA)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoGinseng2(Serial serial)
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
    public class DecoGinseng : Item
    {
        [Constructable]
        public DecoGinseng()
            : base(0x18E9)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoGinseng(Serial serial)
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