using System;
using System.Collections.Generic;

using Server.Mobiles;
using Server.Network;

namespace Server.Items
{
    public class GingerBreadCookie : Food
    {
        private readonly int[] m_Messages =
		{
			0,
			1077396, // Noooo!
			1077397, // Please don't eat me... *whimper*
			1077405, // Not the face!
			1077406, // Ahhhhhh! My foot’s gone!
			1077407, // Please. No! I have gingerkids!
			1077408, // No, no! I’m really made of poison. Really.
			1077409 // Run, run as fast as you can! You can't catch me! I'm the gingerbread man!
		};

        [Constructable]
        public GingerBreadCookie()
            : base(Utility.RandomBool() ? 0x2be1 : 0x2be2)
        {
            Stackable = false;
            LootType = LootType.Blessed;
        }

        public GingerBreadCookie(Serial serial)
            : base(serial)
        {
        }

        public override bool Eat(Mobile from)
        {
            int message = m_Messages[Utility.Random(m_Messages.Length)];

            if (message != 0)
            {
                SendLocalizedMessageTo(from, message);
                return false;
            }

            return base.Eat(from);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}