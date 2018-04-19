using System;

using Server.Network;

namespace Server.Items
{
    [FlipableAttribute(0xC1B, 0xC1C, 0xC1E, 0xC1D)]
    public class RuinedChair : Item
    {
        [Constructable]
        public RuinedChair()
            : base(0xC1B)
        {
            Movable = false;
        }

        public RuinedChair(Serial serial)
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

    [FlipableAttribute(0xC17, 0xC18)]
    public class CoveredChair : Item
    {
        [Constructable]
        public CoveredChair()
            : base(0xC17)
        {
            Movable = false;
        }

        public CoveredChair(Serial serial)
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

    [FlipableAttribute(0xC10, 0xC11)]
    public class RuinedFallenChairA : Item
    {
        [Constructable]
        public RuinedFallenChairA()
            : base(0xC10)
        {
            Movable = false;
        }

        public RuinedFallenChairA(Serial serial)
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

    [FlipableAttribute(0xC19, 0xC1A)]
    public class RuinedFallenChairB : Item
    {
        [Constructable]
        public RuinedFallenChairB()
            : base(0xC19)
        {
            Movable = false;
        }

        public RuinedFallenChairB(Serial serial)
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