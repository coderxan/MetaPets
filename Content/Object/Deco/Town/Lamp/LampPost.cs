using System;

using Server;

namespace Server.Items
{
    public class LampPost1 : BaseLight
    {
        public override int LitItemID { get { return 0xB20; } }
        public override int UnlitItemID { get { return 0xB21; } }

        [Constructable]
        public LampPost1()
            : base(0xB21)
        {
            Movable = false;
            Duration = TimeSpan.Zero; // Never burnt out
            Burning = false;
            Light = LightType.Circle300;
            Weight = 40.0;
        }

        public LampPost1(Serial serial)
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

    public class LampPost2 : BaseLight
    {
        public override int LitItemID { get { return 0xB22; } }
        public override int UnlitItemID { get { return 0xB23; } }

        [Constructable]
        public LampPost2()
            : base(0xB23)
        {
            Movable = false;
            Duration = TimeSpan.Zero; // Never burnt out
            Burning = false;
            Light = LightType.Circle300;
            Weight = 40.0;
        }

        public LampPost2(Serial serial)
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

    public class LampPost3 : BaseLight
    {
        public override int LitItemID { get { return 0xB24; } }
        public override int UnlitItemID { get { return 0xB25; } }

        [Constructable]
        public LampPost3()
            : base(0xb25)
        {
            Movable = false;
            Duration = TimeSpan.Zero; // Never burnt out
            Burning = false;
            Light = LightType.Circle300;
            Weight = 40.0;
        }

        public LampPost3(Serial serial)
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