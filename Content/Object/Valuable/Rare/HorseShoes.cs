using System;

namespace Server.Items
{
    public class HorseShoes : Item
    {
        [Constructable]
        public HorseShoes()
            : base(0xFB6)
        {
            Weight = 3.0;
        }

        public HorseShoes(Serial serial)
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