﻿using System;

using Server.Engines.VeteranRewards;

namespace Server.Items
{
    public class MonkRobe : BaseOuterTorso
    {
        [Constructable]
        public MonkRobe()
            : this(0x21E)
        {
        }

        [Constructable]
        public MonkRobe(int hue)
            : base(0x2687, hue)
        {
            Weight = 1.0;
            StrRequirement = 0;
        }
        public override int LabelNumber { get { return 1076584; } } // A monk's robe
        public override bool CanBeBlessed { get { return false; } }
        public override bool Dye(Mobile from, DyeTub sender)
        {
            from.SendLocalizedMessage(sender.FailMessage);
            return false;
        }
        public MonkRobe(Serial serial)
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