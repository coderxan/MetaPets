using System;

using Server;
using Server.Misc;
using Server.Mobiles;

namespace Server.Items
{
    public class RuneBeetleCarapace : PlateDo
    {
        public override int InitMinHits { get { return 255; } }
        public override int InitMaxHits { get { return 255; } }

        public override int LabelNumber { get { return 1070968; } } // Rune Beetle Carapace

        public override int BaseColdResistance { get { return 14; } }
        public override int BaseEnergyResistance { get { return 14; } }

        [Constructable]
        public RuneBeetleCarapace()
            : base()
        {
            Attributes.BonusMana = 10;
            Attributes.RegenMana = 3;
            Attributes.LowerManaCost = 15;
            ArmorAttributes.LowerStatReq = 100;
            ArmorAttributes.MageArmor = 1;
        }

        public RuneBeetleCarapace(Serial serial)
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