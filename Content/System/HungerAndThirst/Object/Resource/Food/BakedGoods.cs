using System;
using System.Collections;
using System.Collections.Generic;

using Server.ContextMenus;
using Server.Network;

namespace Server.Items
{
    public class BreadLoaf : Food
    {
        [Constructable]
        public BreadLoaf()
            : this(1)
        {
        }

        [Constructable]
        public BreadLoaf(int amount)
            : base(amount, 0x103B)
        {
            this.Weight = 1.0;
            this.FillFactor = 3;
        }

        public BreadLoaf(Serial serial)
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

    public class FrenchBread : Food
    {
        [Constructable]
        public FrenchBread()
            : this(1)
        {
        }

        [Constructable]
        public FrenchBread(int amount)
            : base(amount, 0x98C)
        {
            this.Weight = 2.0;
            this.FillFactor = 3;
        }

        public FrenchBread(Serial serial)
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

    public class Cake : Food
    {
        [Constructable]
        public Cake()
            : base(0x9E9)
        {
            Stackable = false;
            this.Weight = 1.0;
            this.FillFactor = 10;
        }

        public Cake(Serial serial)
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

    public class Cookies : Food
    {
        [Constructable]
        public Cookies()
            : base(0x160b)
        {
            Stackable = Core.ML;
            this.Weight = 1.0;
            this.FillFactor = 4;
        }

        public Cookies(Serial serial)
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

    public class Muffins : Food
    {
        [Constructable]
        public Muffins()
            : base(0x9eb)
        {
            Stackable = false;
            this.Weight = 1.0;
            this.FillFactor = 4;
        }

        public Muffins(Serial serial)
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

    public class FruitPie : Food
    {
        public override int LabelNumber { get { return 1041346; } } // baked fruit pie

        [Constructable]
        public FruitPie()
            : base(0x1041)
        {
            Stackable = false;
            this.Weight = 1.0;
            this.FillFactor = 5;
        }

        public FruitPie(Serial serial)
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

    public class MeatPie : Food
    {
        public override int LabelNumber { get { return 1041347; } } // baked meat pie

        [Constructable]
        public MeatPie()
            : base(0x1041)
        {
            Stackable = false;
            this.Weight = 1.0;
            this.FillFactor = 5;
        }

        public MeatPie(Serial serial)
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

    public class PumpkinPie : Food
    {
        public override int LabelNumber { get { return 1041348; } } // baked pumpkin pie

        [Constructable]
        public PumpkinPie()
            : base(0x1041)
        {
            Stackable = false;
            this.Weight = 1.0;
            this.FillFactor = 5;
        }

        public PumpkinPie(Serial serial)
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

    public class ApplePie : Food
    {
        public override int LabelNumber { get { return 1041343; } } // baked apple pie

        [Constructable]
        public ApplePie()
            : base(0x1041)
        {
            Stackable = false;
            this.Weight = 1.0;
            this.FillFactor = 5;
        }

        public ApplePie(Serial serial)
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

    public class PeachCobbler : Food
    {
        public override int LabelNumber { get { return 1041344; } } // baked peach cobbler

        [Constructable]
        public PeachCobbler()
            : base(0x1041)
        {
            Stackable = false;
            this.Weight = 1.0;
            this.FillFactor = 5;
        }

        public PeachCobbler(Serial serial)
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

    public class Quiche : Food
    {
        public override int LabelNumber { get { return 1041345; } } // baked quiche

        [Constructable]
        public Quiche()
            : base(0x1041)
        {
            Stackable = Core.ML;
            this.Weight = 1.0;
            this.FillFactor = 5;
        }

        public Quiche(Serial serial)
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

    [TypeAlias("Server.Items.Pizza")]
    public class CheesePizza : Food
    {
        public override int LabelNumber { get { return 1044516; } } // cheese pizza

        [Constructable]
        public CheesePizza()
            : base(0x1040)
        {
            Stackable = false;
            this.Weight = 1.0;
            this.FillFactor = 6;
        }

        public CheesePizza(Serial serial)
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

#if false
	public class Pizza : Food
	{
		[Constructable]
		public Pizza() : base( 0x1040 )
		{
			Stackable = false;
			this.Weight = 1.0;
			this.FillFactor = 6;
		}

		public Pizza( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}
#endif

    public class SausagePizza : Food
    {
        public override int LabelNumber { get { return 1044517; } } // sausage pizza

        [Constructable]
        public SausagePizza()
            : base(0x1040)
        {
            Stackable = false;
            this.Weight = 1.0;
            this.FillFactor = 6;
        }

        public SausagePizza(Serial serial)
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