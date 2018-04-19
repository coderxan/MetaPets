using System;
using System.Collections.Generic;

using Server.ContextMenus;
using Server.Items;
using Server.Regions;

using BunnyHole = Server.Mobiles.VorpalBunny.BunnyHole;

namespace Server.Mobiles
{
    public class SummonedSkeletalKnight : BaseTalismanSummon
    {
        [Constructable]
        public SummonedSkeletalKnight()
            : base()
        {
            Name = "a skeletal knight";
            Body = 147;
            BaseSoundID = 451;
        }

        public SummonedSkeletalKnight(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }
}