using System;

namespace Server.Items
{
    [FlipableAttribute(0x2B68, 0x315F)]
    public class WoodlandBelt : BaseWaist
    {
        public override Race RequiredRace { get { return Race.Elf; } }

        [Constructable]
        public WoodlandBelt()
            : this(0)
        {
        }

        [Constructable]
        public WoodlandBelt(int hue)
            : base(0x2B68, hue)
        {
            Weight = 4.0;
        }

        public WoodlandBelt(Serial serial)
            : base(serial)
        {
        }

        public override bool Dye(Mobile from, DyeTub sender)
        {
            from.SendLocalizedMessage(sender.FailMessage);
            return false;
        }

        public override bool Scissor(Mobile from, Scissors scissors)
        {
            from.SendLocalizedMessage(502440); // Scissors can not be used on that to produce anything.
            return false;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }
}