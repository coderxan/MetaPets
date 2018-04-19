using System;

using Server;
using Server.Mobiles;
using Server.Network;

namespace Server.Mobiles
{
    [CorpseName("a hare corpse")]
    public class EnragedRabbit : BaseEnraged
    {
        public EnragedRabbit(Mobile summoner)
            : base(summoner)
        {
            Name = "a rabbit";
            Body = 0xcd;
        }

        public override int GetAttackSound()
        {
            return 0xC9;
        }

        public override int GetHurtSound()
        {
            return 0xCA;
        }

        public override int GetDeathSound()
        {
            return 0xCB;
        }

        public EnragedRabbit(Serial serial)
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