using System;

using Server;
using Server.Misc;
using Server.Mobiles;

namespace Server.Items
{
    public class TomeOfLostKnowledge : Spellbook
    {
        public override int LabelNumber { get { return 1070971; } } // Tome of Lost Knowledge

        [Constructable]
        public TomeOfLostKnowledge()
            : base()
        {
            LootType = LootType.Regular;
            Hue = 0x530;

            SkillBonuses.SetValues(0, SkillName.Magery, 15.0);
            Attributes.BonusInt = 8;
            Attributes.LowerManaCost = 15;
            Attributes.SpellDamage = 15;
        }

        public TomeOfLostKnowledge(Serial serial)
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