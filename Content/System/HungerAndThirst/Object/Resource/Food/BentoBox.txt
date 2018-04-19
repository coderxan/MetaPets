using System;

namespace Server.Items
{
    public class EmptyBentoBox : Item
    {
        [Constructable]
        public EmptyBentoBox()
            : base(0x2834)
        {
            Weight = 5.0;
        }

        public EmptyBentoBox(Serial serial)
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

    public class BentoBox : Food
    {
        [Constructable]
        public BentoBox()
            : base(0x2836)
        {
            Stackable = false;
            Weight = 5.0;
            FillFactor = 2;
        }

        public override bool Eat(Mobile from)
        {
            if (!base.Eat(from))
                return false;

            from.AddToBackpack(new EmptyBentoBox());
            return true;
        }

        public BentoBox(Serial serial)
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