using System;

using Server.Items;
using Server.Targeting;

namespace Server.Items
{
    public class LightYarn : BaseClothMaterial
    {
        [Constructable]
        public LightYarn()
            : this(1)
        {
        }

        [Constructable]
        public LightYarn(int amount)
            : base(0xE1E, amount)
        {
        }

        public LightYarn(Serial serial)
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