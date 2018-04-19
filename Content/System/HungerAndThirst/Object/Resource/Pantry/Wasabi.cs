using System;

namespace Server.Items
{
    public class Wasabi : Item
    {
        [Constructable]
        public Wasabi()
            : base(0x24E8)
        {
            Weight = 1.0;
        }

        public Wasabi(Serial serial)
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

    public class WasabiClumps : Food
    {
        [Constructable]
        public WasabiClumps()
            : base(0x24EB)
        {
            Stackable = false;
            Weight = 1.0;
            FillFactor = 2;
        }

        public WasabiClumps(Serial serial)
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