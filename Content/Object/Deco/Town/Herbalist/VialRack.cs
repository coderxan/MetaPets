using System;

using Server.Network;

namespace Server.Items
{
    [FlipableAttribute(0x185B, 0x185C)]
    public class EmptyVialsWRack : Item
    {
        [Constructable]
        public EmptyVialsWRack()
            : base(0x185B)
        {
            Weight = 1.0;
            Movable = true;
        }

        public EmptyVialsWRack(Serial serial)
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

    [FlipableAttribute(0x185D, 0x185E)]
    public class FullVialsWRack : Item
    {
        [Constructable]
        public FullVialsWRack()
            : base(0x185D)
        {
            Weight = 1.0;
            Movable = true;
        }

        public FullVialsWRack(Serial serial)
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

    public class EmptyVial : Item
    {
        [Constructable]
        public EmptyVial()
            : base(0x0E24)
        {
            Weight = 1.0;
            Movable = true;
        }

        public EmptyVial(Serial serial)
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