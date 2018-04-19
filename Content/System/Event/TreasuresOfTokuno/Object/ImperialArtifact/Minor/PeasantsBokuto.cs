using System;

using Server;
using Server.Misc;
using Server.Mobiles;

namespace Server.Items
{
    public class PeasantsBokuto : Bokuto
    {
        public override int LabelNumber { get { return 1070912; } } // Peasant's Bokuto

        [Constructable]
        public PeasantsBokuto()
            : base()
        {
            WeaponAttributes.SelfRepair = 3;
            WeaponAttributes.HitLowerDefend = 30;

            Attributes.WeaponDamage = 35;
            Attributes.WeaponSpeed = 10;
            Slayer = SlayerName.SnakesBane;
        }

        public PeasantsBokuto(Serial serial)
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