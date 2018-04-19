using System;
using System.Collections;
using System.Collections.Generic;

using Server.ContextMenus;
using Server.Network;

namespace Server.Items
{
    public class RoastPig : Food
    {
        [Constructable]
        public RoastPig()
            : this(1)
        {
        }

        [Constructable]
        public RoastPig(int amount)
            : base(amount, 0x9BB)
        {
            this.Weight = 45.0;
            this.FillFactor = 20;
        }

        public RoastPig(Serial serial)
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

    public class Ham : Food
    {
        [Constructable]
        public Ham()
            : this(1)
        {
        }

        [Constructable]
        public Ham(int amount)
            : base(amount, 0x9C9)
        {
            this.Weight = 1.0;
            this.FillFactor = 5;
        }

        public Ham(Serial serial)
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

    public class Bacon : Food
    {
        [Constructable]
        public Bacon()
            : this(1)
        {
        }

        [Constructable]
        public Bacon(int amount)
            : base(amount, 0x979)
        {
            this.Weight = 1.0;
            this.FillFactor = 1;
        }

        public Bacon(Serial serial)
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

    public class SlabOfBacon : Food
    {
        [Constructable]
        public SlabOfBacon()
            : this(1)
        {
        }

        [Constructable]
        public SlabOfBacon(int amount)
            : base(amount, 0x976)
        {
            this.Weight = 1.0;
            this.FillFactor = 3;
        }

        public SlabOfBacon(Serial serial)
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

    public class FishSteak : Food
    {
        public override double DefaultWeight
        {
            get { return 0.1; }
        }

        [Constructable]
        public FishSteak()
            : this(1)
        {
        }

        [Constructable]
        public FishSteak(int amount)
            : base(amount, 0x97B)
        {
            this.FillFactor = 3;
        }

        public FishSteak(Serial serial)
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

    public class CookedBird : Food
    {
        [Constructable]
        public CookedBird()
            : this(1)
        {
        }

        [Constructable]
        public CookedBird(int amount)
            : base(amount, 0x9B7)
        {
            this.Weight = 1.0;
            this.FillFactor = 5;
        }

        public CookedBird(Serial serial)
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

    public class ChickenLeg : Food
    {
        [Constructable]
        public ChickenLeg()
            : this(1)
        {
        }

        [Constructable]
        public ChickenLeg(int amount)
            : base(amount, 0x1608)
        {
            this.Weight = 1.0;
            this.FillFactor = 4;
        }

        public ChickenLeg(Serial serial)
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

    public class Ribs : Food
    {
        [Constructable]
        public Ribs()
            : this(1)
        {
        }

        [Constructable]
        public Ribs(int amount)
            : base(amount, 0x9F2)
        {
            this.Weight = 1.0;
            this.FillFactor = 5;
        }

        public Ribs(Serial serial)
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

    public class Sausage : Food
    {
        [Constructable]
        public Sausage()
            : this(1)
        {
        }

        [Constructable]
        public Sausage(int amount)
            : base(amount, 0x9C0)
        {
            this.Weight = 1.0;
            this.FillFactor = 4;
        }

        public Sausage(Serial serial)
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

    public class LambLeg : Food
    {
        [Constructable]
        public LambLeg()
            : this(1)
        {
        }

        [Constructable]
        public LambLeg(int amount)
            : base(amount, 0x160a)
        {
            this.Weight = 2.0;
            this.FillFactor = 5;
        }

        public LambLeg(Serial serial)
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