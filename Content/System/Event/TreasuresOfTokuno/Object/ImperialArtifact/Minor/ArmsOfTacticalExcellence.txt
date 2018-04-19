using System;

using Server;
using Server.Misc;
using Server.Mobiles;

namespace Server.Items
{
    public class ArmsOfTacticalExcellence : LeatherHiroSode
    {
        public override int LabelNumber { get { return 1070921; } } // Arms of Tactical Excellence

        public override int BaseFireResistance { get { return 9; } }
        public override int BaseColdResistance { get { return 13; } }
        public override int BasePoisonResistance { get { return 8; } }

        [Constructable]
        public ArmsOfTacticalExcellence()
            : base()
        {
            Attributes.BonusDex = 5;
            SkillBonuses.SetValues(0, SkillName.Tactics, 12.0);
        }

        public ArmsOfTacticalExcellence(Serial serial)
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