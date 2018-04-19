using System;

using Server;
using Server.Misc;
using Server.Mobiles;

namespace Server.Items
{
    public class FluteOfRenewal : BambooFlute
    {
        public override int LabelNumber { get { return 1070927; } } // Flute of Renewal

        [Constructable]
        public FluteOfRenewal()
            : base()
        {
            Slayer = SlayerGroup.Groups[Utility.Random(SlayerGroup.Groups.Length - 1)].Super.Name; //-1 to exclude Fey slayer.  Try to confrim no fey slayer on this on OSI

            ReplenishesCharges = true;
        }

        public override int InitMinUses { get { return 300; } }
        public override int InitMaxUses { get { return 300; } }

        public FluteOfRenewal(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version == 0 && Slayer == SlayerName.Fey)
                Slayer = SlayerGroup.Groups[Utility.Random(SlayerGroup.Groups.Length - 1)].Super.Name;
        }
    }
}