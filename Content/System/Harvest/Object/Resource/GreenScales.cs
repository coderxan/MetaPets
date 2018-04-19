using System;

using Server.Items;
using Server.Network;

namespace Server.Items
{
    public class GreenScales : BaseScales
    {
        [Constructable]
        public GreenScales()
            : this(1)
        {
        }

        [Constructable]
        public GreenScales(int amount)
            : base(CraftResource.GreenScales, amount)
        {
        }

        public GreenScales(Serial serial)
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