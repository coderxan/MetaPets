using System;

using Server;
using Server.Misc;
using Server.Mobiles;

namespace Server.Items
{
    public class KasaOfTheRajin : Kasa
    {
        public override int LabelNumber { get { return 1070969; } } // Kasa of the Raj-in

        public override int BasePhysicalResistance { get { return 12; } }
        public override int BaseFireResistance { get { return 17; } }
        public override int BaseColdResistance { get { return 21; } }
        public override int BasePoisonResistance { get { return 17; } }
        public override int BaseEnergyResistance { get { return 17; } }

        public override int InitMinHits { get { return 255; } }
        public override int InitMaxHits { get { return 255; } }

        [Constructable]
        public KasaOfTheRajin()
            : base()
        {
            Attributes.SpellDamage = 12;
        }

        public KasaOfTheRajin(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)2);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version <= 1)
            {
                MaxHitPoints = 255;
                HitPoints = 255;
            }

            if (version == 0)
                LootType = LootType.Regular;
        }
    }
}