using System;

using Server;
using Server.Gumps;
using Server.Items;
using Server.Multis;
using Server.Network;

namespace Server.Items
{
    public class WindChimes : BaseWindChimes
    {
        public override int LabelNumber { get { return 1030290; } }

        [Constructable]
        public WindChimes()
            : base(0x2832)
        {
        }

        public WindChimes(Serial serial)
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

    public class FancyWindChimes : BaseWindChimes
    {
        public override int LabelNumber { get { return 1030291; } }

        [Constructable]
        public FancyWindChimes()
            : base(0x2833)
        {
        }

        public FancyWindChimes(Serial serial)
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