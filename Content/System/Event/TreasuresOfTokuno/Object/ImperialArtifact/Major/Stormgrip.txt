using System;

using Server;
using Server.Misc;
using Server.Mobiles;

namespace Server.Items
{
    public class Stormgrip : LeatherNinjaMitts
    {
        public override int InitMinHits { get { return 255; } }
        public override int InitMaxHits { get { return 255; } }

        public override int LabelNumber { get { return 1070970; } } // Stormgrip

        public override int BasePhysicalResistance { get { return 10; } }
        public override int BaseColdResistance { get { return 18; } }
        public override int BaseEnergyResistance { get { return 18; } }

        [Constructable]
        public Stormgrip()
            : base()
        {
            Attributes.BonusInt = 8;
            Attributes.Luck = 125;
            Attributes.WeaponDamage = 25;
        }

        public Stormgrip(Serial serial)
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