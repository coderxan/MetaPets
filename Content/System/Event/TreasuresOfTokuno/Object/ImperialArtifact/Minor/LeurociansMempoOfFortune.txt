using System;

using Server;
using Server.Misc;
using Server.Mobiles;

namespace Server.Items
{
    public class LeurociansMempoOfFortune : LeatherMempo
    {
        public override int LabelNumber { get { return 1071460; } } // Leurocian's mempo of fortune

        public override int BasePhysicalResistance { get { return 15; } }
        public override int BaseFireResistance { get { return 10; } }
        public override int BaseColdResistance { get { return 10; } }
        public override int BasePoisonResistance { get { return 10; } }
        public override int BaseEnergyResistance { get { return 15; } }

        [Constructable]
        public LeurociansMempoOfFortune()
            : base()
        {
            LootType = LootType.Regular;
            Hue = 0x501;

            Attributes.Luck = 300;
            Attributes.RegenMana = 1;
        }

        public LeurociansMempoOfFortune(Serial serial)
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
        public override int InitMinHits { get { return 255; } }
        public override int InitMaxHits { get { return 255; } }
    }
}