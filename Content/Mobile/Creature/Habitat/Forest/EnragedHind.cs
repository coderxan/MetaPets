using System;

using Server;
using Server.Mobiles;
using Server.Network;

namespace Server.Mobiles
{
    [CorpseName("a deer corpse")]
    public class EnragedHind : BaseEnraged
    {
        public EnragedHind(Mobile summoner)
            : base(summoner)
        {
            Name = "a hind";
            Body = 0xed;
        }
        public override int GetAttackSound()
        {
            return 0x82;
        }

        public override int GetHurtSound()
        {
            return 0x83;
        }

        public override int GetDeathSound()
        {
            return 0x84;
        }

        public EnragedHind(Serial serial)
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