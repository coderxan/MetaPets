using System;

using Server.Items;

namespace Server.Items
{
    public class HeartwoodLog : Log
    {
        [Constructable]
        public HeartwoodLog()
            : this(1)
        {
        }
        [Constructable]
        public HeartwoodLog(int amount)
            : base(CraftResource.Heartwood, amount)
        {
        }
        public HeartwoodLog(Serial serial)
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

        public override bool Axe(Mobile from, BaseAxe axe)
        {
            if (!TryCreateBoards(from, 100, new HeartwoodBoard()))
                return false;

            return true;
        }
    }
}