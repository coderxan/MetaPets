using System;

using Server;
using Server.Misc;
using Server.Mobiles;

namespace Server.Items
{
    public class DemonForks : Sai
    {
        public override int LabelNumber { get { return 1070917; } } // Demon Forks

        [Constructable]
        public DemonForks()
            : base()
        {
            WeaponAttributes.ResistFireBonus = 10;
            WeaponAttributes.ResistPoisonBonus = 10;

            Attributes.ReflectPhysical = 10;
            Attributes.WeaponDamage = 35;
            Attributes.DefendChance = 10;
        }

        public DemonForks(Serial serial)
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