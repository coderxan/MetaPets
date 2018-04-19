using System;

namespace Server.Items
{
    public class GrizzledBones : Item
    {
        [Constructable]
        public GrizzledBones()
            : this(1)
        {
        }

        [Constructable]
        public GrizzledBones(int amountFrom, int amountTo)
            : this(Utility.RandomMinMax(amountFrom, amountTo))
        {
        }

        [Constructable]
        public GrizzledBones(int amount)
            : base(0x318C)
        {
            Stackable = true;
            Amount = amount;
        }

        public GrizzledBones(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version <= 0 && ItemID == 0x318F)
                ItemID = 0x318C;
        }
    }
}