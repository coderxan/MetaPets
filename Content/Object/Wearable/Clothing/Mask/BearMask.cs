using System;
using System.Collections.Generic;

using Server.Engines.Craft;
using Server.Network;

namespace Server.Items
{
    public class BearMask : BaseHat
    {
        public override int BasePhysicalResistance { get { return 5; } }
        public override int BaseFireResistance { get { return 3; } }
        public override int BaseColdResistance { get { return 8; } }
        public override int BasePoisonResistance { get { return 4; } }
        public override int BaseEnergyResistance { get { return 4; } }

        public override int InitMinHits { get { return 20; } }
        public override int InitMaxHits { get { return 30; } }

        [Constructable]
        public BearMask()
            : this(0)
        {
        }

        [Constructable]
        public BearMask(int hue)
            : base(0x1545, hue)
        {
            Weight = 5.0;
        }

        public override bool Dye(Mobile from, DyeTub sender)
        {
            from.SendLocalizedMessage(sender.FailMessage);
            return false;
        }

        public BearMask(Serial serial)
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