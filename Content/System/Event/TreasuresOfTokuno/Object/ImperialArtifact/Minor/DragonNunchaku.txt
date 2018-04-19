using System;

using Server;
using Server.Misc;
using Server.Mobiles;

namespace Server.Items
{
    public class DragonNunchaku : Nunchaku
    {
        public override int LabelNumber { get { return 1070914; } } // Dragon Nunchaku

        [Constructable]
        public DragonNunchaku()
            : base()
        {
            WeaponAttributes.ResistFireBonus = 5;
            WeaponAttributes.SelfRepair = 3;
            WeaponAttributes.HitFireball = 50;

            Attributes.WeaponDamage = 40;
            Attributes.WeaponSpeed = 20;
        }

        public DragonNunchaku(Serial serial)
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