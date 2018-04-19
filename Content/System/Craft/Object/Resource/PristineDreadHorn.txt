using System;

namespace Server.Items
{
    [Flipable(0x315A, 0x315B)]
    public class PristineDreadHorn : Item
    {
        [Constructable]
        public PristineDreadHorn()
            : base(0x315A)
        {

        }

        public PristineDreadHorn(Serial serial)
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