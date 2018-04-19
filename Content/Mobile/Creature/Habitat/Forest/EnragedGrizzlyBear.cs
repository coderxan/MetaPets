using System;

using Server;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Mobiles
{
    [CorpseName("a grizzly bear corpse")]
    [TypeAlias("Server.Mobiles.Grizzlybear")]
    public class EnragedGrizzlyBear : BaseEnraged
    {
        [Constructable]
        public EnragedGrizzlyBear(Mobile summoner)
            : base(summoner)
        {
            Name = "a grizzly bear";
            Body = 212;
            BaseSoundID = 0xA3;
        }

        public EnragedGrizzlyBear(Serial serial)
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