using System;
using System.Collections.Generic;

using Server.ContextMenus;
using Server.Items;
using Server.Regions;

using BunnyHole = Server.Mobiles.VorpalBunny.BunnyHole;

namespace Server.Mobiles
{
    public class SummonedOrcBrute : BaseTalismanSummon
    {
        [Constructable]
        public SummonedOrcBrute()
            : base()
        {
            Body = 189;
            Name = "an orc brute";
            BaseSoundID = 0x45A;
        }

        public SummonedOrcBrute(Serial serial)
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