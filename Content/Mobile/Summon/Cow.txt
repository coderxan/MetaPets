using System;
using System.Collections.Generic;

using Server.ContextMenus;
using Server.Items;
using Server.Regions;

using BunnyHole = Server.Mobiles.VorpalBunny.BunnyHole;

namespace Server.Mobiles
{
    public class SummonedCow : BaseTalismanSummon
    {
        [Constructable]
        public SummonedCow()
            : base()
        {
            Name = "a cow";
            Body = Utility.RandomList(0xD8, 0xE7);
            BaseSoundID = 0x78;
        }

        public SummonedCow(Serial serial)
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