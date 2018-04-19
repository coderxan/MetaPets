using System;

using Server;
using Server.Misc;
using Server.Mobiles;

namespace Server.Items
{
    public class DarkenedSky : Kama
    {
        public override int InitMinHits { get { return 255; } }
        public override int InitMaxHits { get { return 255; } }

        public override int LabelNumber { get { return 1070966; } } // Darkened Sky

        [Constructable]
        public DarkenedSky()
            : base()
        {
            WeaponAttributes.HitLightning = 60;
            Attributes.WeaponSpeed = 25;
            Attributes.WeaponDamage = 50;
        }

        public override void GetDamageTypes(Mobile wielder, out int phys, out int fire, out int cold, out int pois, out int nrgy, out int chaos, out int direct)
        {
            phys = fire = pois = chaos = direct = 0;
            cold = nrgy = 50;
        }

        public DarkenedSky(Serial serial)
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