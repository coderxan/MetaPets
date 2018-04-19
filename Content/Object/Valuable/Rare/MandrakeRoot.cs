﻿using System;

namespace Server.Items
{
    /// <summary>
    /// Facing North-South
    /// </summary>
    public class DecoMandrakeRoot2 : Item
    {
        [Constructable]
        public DecoMandrakeRoot2()
            : base(0x18DD)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoMandrakeRoot2(Serial serial)
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

    /// <summary>
    /// Facing East-West
    /// </summary>
    public class DecoMandrakeRoot : Item
    {
        [Constructable]
        public DecoMandrakeRoot()
            : base(0x18DE)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoMandrakeRoot(Serial serial)
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