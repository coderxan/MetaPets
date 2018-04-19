using System;

using Server;
using Server.Misc;
using Server.Mobiles;

namespace Server.Items
{
    public class LegsOfStability : PlateSuneate
    {
        public override int LabelNumber { get { return 1070925; } } // Legs of Stability

        public override int BasePhysicalResistance { get { return 20; } }
        public override int BasePoisonResistance { get { return 18; } }

        [Constructable]
        public LegsOfStability()
            : base()
        {
            Attributes.BonusStam = 5;

            ArmorAttributes.SelfRepair = 3;
            ArmorAttributes.LowerStatReq = 100;
            ArmorAttributes.MageArmor = 1;
        }

        public LegsOfStability(Serial serial)
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