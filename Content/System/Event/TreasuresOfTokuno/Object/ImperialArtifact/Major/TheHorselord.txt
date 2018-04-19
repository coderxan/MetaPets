using System;

using Server;
using Server.Misc;
using Server.Mobiles;

namespace Server.Items
{
    public class TheHorselord : Yumi
    {
        public override int InitMinHits { get { return 255; } }
        public override int InitMaxHits { get { return 255; } }

        public override int LabelNumber { get { return 1070967; } } // The Horselord

        [Constructable]
        public TheHorselord()
            : base()
        {
            Attributes.BonusDex = 5;
            Attributes.RegenMana = 1;
            Attributes.Luck = 125;
            Attributes.WeaponDamage = 50;

            Slayer = SlayerName.ElementalBan;
            Slayer2 = SlayerName.ReptilianDeath;
        }

        public TheHorselord(Serial serial)
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