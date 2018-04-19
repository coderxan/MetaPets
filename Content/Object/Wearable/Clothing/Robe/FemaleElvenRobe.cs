using System;

using Server.Engines.VeteranRewards;

namespace Server.Items
{
    [Flipable(0x2FBA, 0x3174)]
    public class FemaleElvenRobe : BaseOuterTorso
    {
        public override Race RequiredRace { get { return Race.Elf; } }
        [Constructable]
        public FemaleElvenRobe()
            : this(0)
        {
        }

        [Constructable]
        public FemaleElvenRobe(int hue)
            : base(0x2FBA, hue)
        {
            Weight = 2.0;
        }

        public override bool AllowMaleWearer { get { return false; } }

        public FemaleElvenRobe(Serial serial)
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