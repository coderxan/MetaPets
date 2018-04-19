﻿using System;

using Server;
using Server.Misc;
using Server.Mobiles;

namespace Server.Items
{
    public class TheDestroyer : NoDachi
    {
        public override int LabelNumber { get { return 1070915; } } // The Destroyer

        [Constructable]
        public TheDestroyer()
            : base()
        {
            WeaponAttributes.HitLeechStam = 40;

            Attributes.BonusStr = 6;
            Attributes.AttackChance = 10;
            Attributes.WeaponDamage = 50;
        }

        public TheDestroyer(Serial serial)
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