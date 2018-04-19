using System;

using Server;
using Server.Gumps;
using Server.Network;

namespace Server.Items
{
    [FlipableAttribute(0x48E0, 0x48E1)]
    public class StValentinesPanda : StValentinesBear
    {
        [Constructable]
        public StValentinesPanda()
            : this(null)
        {
        }

        [Constructable]
        public StValentinesPanda(string name)
            : base(0x48E0, name)
        {
        }

        public StValentinesPanda(Serial serial)
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