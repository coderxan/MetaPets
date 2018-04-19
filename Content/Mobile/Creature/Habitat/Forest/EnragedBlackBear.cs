using System;

using Server;
using Server.Mobiles;
using Server.Network;

namespace Server.Mobiles
{
    [CorpseName("a bear corpse")]
    public class EnragedBlackBear : BaseEnraged
    {
        public EnragedBlackBear(Mobile summoner)
            : base(summoner)
        {
            Name = "a black bear";
            Body = 0xd3;
            BaseSoundID = 0xa3;
        }
        public EnragedBlackBear(Serial serial)
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