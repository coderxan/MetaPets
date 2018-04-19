using System;

using Server;
using Server.Misc;
using Server.Mobiles;

namespace Server.Items
{
    public class TomeOfEnlightenment : Spellbook
    {
        public override int LabelNumber { get { return 1070934; } } // Tome of Enlightenment

        [Constructable]
        public TomeOfEnlightenment()
            : base()
        {
            LootType = LootType.Regular;
            Hue = 0x455;

            Attributes.BonusInt = 5;
            Attributes.SpellDamage = 10;
            Attributes.CastSpeed = 1;
        }

        public TomeOfEnlightenment(Serial serial)
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