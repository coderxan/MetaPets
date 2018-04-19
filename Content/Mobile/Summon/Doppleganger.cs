using System;
using System.Collections.Generic;

using Server.ContextMenus;
using Server.Items;
using Server.Regions;

using BunnyHole = Server.Mobiles.VorpalBunny.BunnyHole;

namespace Server.Mobiles
{
    public class SummonedDoppleganger : BaseTalismanSummon
    {
        [Constructable]
        public SummonedDoppleganger()
            : base()
        {
            Name = "a doppleganger";
            Body = 0x309;
            BaseSoundID = 0x451;
        }

        public SummonedDoppleganger(Serial serial)
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