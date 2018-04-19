using System;

using Server.Items;
using Server.Network;
using Server.Targeting;

namespace Server.Items
{
    public class CookieMix : CookableFood
    {
        [Constructable]
        public CookieMix()
            : base(0x103F, 20)
        {
            Weight = 1.0;
        }

        public CookieMix(Serial serial)
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

        public override Food Cook()
        {
            return new Cookies();
        }
    }

    public class CakeMix : CookableFood
    {
        public override int LabelNumber { get { return 1041002; } } // cake mix

        [Constructable]
        public CakeMix()
            : base(0x103F, 40)
        {
            Weight = 1.0;
        }

        public CakeMix(Serial serial)
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

        public override Food Cook()
        {
            return new Cake();
        }
    }
}