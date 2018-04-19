using System;

using Server;
using Server.Misc;
using Server.Mobiles;

namespace Server.Items
{
    public class BlackLotusHood : ClothNinjaHood
    {
        public override int LabelNumber { get { return 1070919; } } // Black Lotus Hood

        public override int BasePhysicalResistance { get { return 0; } }
        public override int BaseFireResistance { get { return 11; } }
        public override int BaseColdResistance { get { return 15; } }
        public override int BasePoisonResistance { get { return 11; } }
        public override int BaseEnergyResistance { get { return 11; } }

        public override int InitMinHits { get { return 255; } }
        public override int InitMaxHits { get { return 255; } }

        [Constructable]
        public BlackLotusHood()
            : base()
        {
            Attributes.LowerManaCost = 6;
            Attributes.AttackChance = 6;
            ClothingAttributes.SelfRepair = 5;
        }

        public BlackLotusHood(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version == 0)
            {
                MaxHitPoints = 255;
                HitPoints = 255;
            }
        }
    }
}