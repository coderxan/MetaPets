using System;

using Server.Engines.VeteranRewards;

namespace Server.Items
{
    [Flipable(0x2FB9, 0x3173)]
    public class MaleElvenRobe : BaseOuterTorso
    {
        public override Race RequiredRace { get { return Race.Elf; } }

        [Constructable]
        public MaleElvenRobe()
            : this(0)
        {
        }

        [Constructable]
        public MaleElvenRobe(int hue)
            : base(0x2FB9, hue)
        {
            Weight = 2.0;
        }

        public MaleElvenRobe(Serial serial)
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