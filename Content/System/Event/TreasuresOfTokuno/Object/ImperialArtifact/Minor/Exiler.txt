using System;

using Server;
using Server.Misc;
using Server.Mobiles;

namespace Server.Items
{
    public class Exiler : Tetsubo
    {
        public override int LabelNumber { get { return 1070913; } } // Exiler

        [Constructable]
        public Exiler()
            : base()
        {
            WeaponAttributes.HitDispel = 33;
            Slayer = SlayerName.Exorcism;

            Attributes.WeaponDamage = 40;
            Attributes.WeaponSpeed = 20;
        }

        public override void GetDamageTypes(Mobile wielder, out int phys, out int fire, out int cold, out int pois, out int nrgy, out int chaos, out int direct)
        {
            phys = fire = cold = pois = chaos = direct = 0;

            nrgy = 100;
        }


        public Exiler(Serial serial)
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