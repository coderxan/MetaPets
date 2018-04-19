using System;
using System.Collections.Generic;

using Server.ContextMenus;
using Server.Mobiles;
using Server.Multis;
using Server.Network;

namespace Server.Items
{
    [Furniture]
    [Flipable(0x2811, 0x2812)]
    public class WoodenFootLocker : LockableContainer
    {
        [Constructable]
        public WoodenFootLocker()
            : base(0x2811)
        {
            GumpID = 0x10B;
        }

        public WoodenFootLocker(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)2); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version == 0 && Weight == 15)
                Weight = -1;

            if (version < 2)
                GumpID = 0x10B;
        }
    }
}