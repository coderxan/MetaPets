using System;

using Server.Items;
using Server.Targeting;

namespace Server.Items
{
    public class DarkYarn : BaseClothMaterial
    {
        [Constructable]
        public DarkYarn()
            : this(1)
        {
        }

        [Constructable]
        public DarkYarn(int amount)
            : base(0xE1D, amount)
        {
        }

        public DarkYarn(Serial serial)
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
        }
    }
}