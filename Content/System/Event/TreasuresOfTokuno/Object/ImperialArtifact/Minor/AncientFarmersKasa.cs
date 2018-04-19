using System;

using Server;
using Server.Misc;
using Server.Mobiles;

namespace Server.Items
{
    public class AncientFarmersKasa : Kasa
    {
        public override int LabelNumber { get { return 1070922; } } // Ancient Farmer's Kasa
        public override int BaseColdResistance { get { return 19; } }

        public override int InitMinHits { get { return 255; } }
        public override int InitMaxHits { get { return 255; } }

        [Constructable]
        public AncientFarmersKasa()
            : base()
        {
            Attributes.BonusStr = 5;
            Attributes.BonusStam = 5;
            Attributes.RegenStam = 5;

            SkillBonuses.SetValues(0, SkillName.AnimalLore, 5.0);
        }

        public AncientFarmersKasa(Serial serial)
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
                SkillBonuses.SetValues(0, SkillName.AnimalLore, 5.0);
        }
    }
}