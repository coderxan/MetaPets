using System;

using Server;
using Server.Gumps;
using Server.Network;

namespace Server.Items
{
    [FlipableAttribute(0x48E2, 0x48E3)]
    public class StValentinesPolarBear : StValentinesBear
    {
        [Constructable]
        public StValentinesPolarBear()
            : this(null)
        {
        }

        [Constructable]
        public StValentinesPolarBear(string name)
            : base(0x48E2, name)
        {
        }

        public StValentinesPolarBear(Serial serial)
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