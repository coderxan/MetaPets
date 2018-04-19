using System;

using Server.Network;

namespace Server.Items
{
    public class PrizedFish : BaseMagicFish
    {
        public override int Bonus { get { return 5; } }
        public override StatType Type { get { return StatType.Int; } }

        public override int LabelNumber { get { return 1041073; } } // prized fish

        [Constructable]
        public PrizedFish()
            : base(51)
        {
        }

        public PrizedFish(Serial serial)
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

            if (Hue == 151)
                Hue = 51;
        }
    }
}