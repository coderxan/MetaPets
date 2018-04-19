using System;

using Server;
using Server.Misc;
using Server.Mobiles;

namespace Server.Items
{
    public class DaimyosHelm : PlateBattleKabuto
    {
        public override int LabelNumber { get { return 1070920; } } // Daimyo's Helm

        public override int BaseColdResistance { get { return 10; } }

        [Constructable]
        public DaimyosHelm()
            : base()
        {
            ArmorAttributes.LowerStatReq = 100;
            ArmorAttributes.MageArmor = 1;
            ArmorAttributes.SelfRepair = 3;
            Attributes.WeaponSpeed = 10;
        }

        public DaimyosHelm(Serial serial)
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