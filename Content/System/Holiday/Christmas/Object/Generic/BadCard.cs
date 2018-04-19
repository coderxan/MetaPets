using System;
using System.Collections.Generic;

using Server.Mobiles;
using Server.Network;

namespace Server.Items
{
    public class BadCard : Item
    {
        public override int LabelNumber { get { return 1041428; } } // Maybe next year youll get a better...

        [Constructable]
        public BadCard()
            : base(0x14ef)
        {
            int[] m_CardHues = new int[] { 0x45, 0x27, 0x3d0 };
            Hue = m_CardHues[Utility.Random(m_CardHues.Length)];
            Stackable = false;
            LootType = LootType.Blessed;
            Movable = true;
        }

        public BadCard(Serial serial)
            : base(serial)
        {
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