using System;

using Server;
using Server.Misc;
using Server.Mobiles;

namespace Server.Items
{
    public class MetalPigmentsOfTokuno : BasePigmentsOfTokuno
    {
        [Constructable]
        public MetalPigmentsOfTokuno()
            : base(1)
        {
            RandomHue();
            Label = -1;
        }

        public MetalPigmentsOfTokuno(Serial serial)
            : base(serial)
        {
        }

        public void RandomHue()
        {
            int a = Utility.Random(0, 30);
            if (a != 0)
                Hue = a + 0x960;
            else
                Hue = 0;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = (InheritsItem ? 0 : reader.ReadInt()); // Required for BasePigmentsOfTokuno insertion
        }
    }
}