using System;

using Server.Network;

namespace Server.Items
{
    public class TrulyRareFish : BaseMagicFish
    {
        public override int Bonus { get { return 5; } }
        public override StatType Type { get { return StatType.Str; } }

        public override int LabelNumber { get { return 1041075; } } // truly rare fish

        [Constructable]
        public TrulyRareFish()
            : base(76)
        {
        }

        public TrulyRareFish(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (Hue == 376)
                Hue = 76;
        }
    }
}