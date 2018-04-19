using System;

using Server.Engines.VeteranRewards;
using Server.Items;
using Server.Mobiles;
using Server.Spells;

namespace Server.Mobiles
{
    public class ChargerOfTheFallen : EtherealMount
    {
        public override int LabelNumber { get { return 1074816; } } // Charger of the Fallen Statuette

        [Constructable]
        public ChargerOfTheFallen()
            : base(0x2D9C, 0x3E92)
        {
        }

        public override int EtherealHue { get { return 0; } }

        public ChargerOfTheFallen(Serial serial)
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

            if (version <= 1 && Hue != 0)
            {
                Hue = 0;
            }
        }
    }
}