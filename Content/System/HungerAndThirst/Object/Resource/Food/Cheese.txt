using System;
using System.Collections;
using System.Collections.Generic;

using Server.ContextMenus;
using Server.Network;

namespace Server.Items
{
    public class CheeseWheel : Food
    {
        public override double DefaultWeight
        {
            get { return 0.1; }
        }

        [Constructable]
        public CheeseWheel()
            : this(1)
        {
        }

        [Constructable]
        public CheeseWheel(int amount)
            : base(amount, 0x97E)
        {
            this.FillFactor = 3;
        }

        public CheeseWheel(Serial serial)
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

    public class CheeseWedge : Food
    {
        public override double DefaultWeight
        {
            get { return 0.1; }
        }

        [Constructable]
        public CheeseWedge()
            : this(1)
        {
        }

        [Constructable]
        public CheeseWedge(int amount)
            : base(amount, 0x97D)
        {
            this.FillFactor = 3;
        }

        public CheeseWedge(Serial serial)
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

    public class CheeseSlice : Food
    {
        public override double DefaultWeight
        {
            get { return 0.1; }
        }

        [Constructable]
        public CheeseSlice()
            : this(1)
        {
        }

        [Constructable]
        public CheeseSlice(int amount)
            : base(amount, 0x97C)
        {
            this.FillFactor = 1;
        }

        public CheeseSlice(Serial serial)
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