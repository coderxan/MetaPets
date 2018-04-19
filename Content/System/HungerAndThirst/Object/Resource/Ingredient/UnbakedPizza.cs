using System;

using Server.Items;
using Server.Network;
using Server.Targeting;

namespace Server.Items
{
    [TypeAlias("Server.Items.UncookedPizza")]
    public class UncookedCheesePizza : CookableFood
    {
        public override int LabelNumber { get { return 1041341; } } // uncooked cheese pizza

        [Constructable]
        public UncookedCheesePizza()
            : base(0x1083, 20)
        {
            Weight = 1.0;
        }

        public UncookedCheesePizza(Serial serial)
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

            if (ItemID == 0x1040)
                ItemID = 0x1083;

            if (Hue == 51)
                Hue = 0;
        }

        public override Food Cook()
        {
            return new CheesePizza();
        }
    }

#if false
	// ********** UncookedPizza **********
	public class UncookedPizza : CookableFood
	{
		[Constructable]
		public UncookedPizza() : base( 0x1083, 20 )
		{
			Weight = 1.0;
		}

		public UncookedPizza( Serial serial ) : base( serial )
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

			if ( ItemID == 0x1040 )
				ItemID = 0x1083;

			if ( Hue == 51 )
				Hue = 0;
		}

		public override Food Cook()
		{
			return new Pizza();
		}
	}
#endif

    public class UncookedSausagePizza : CookableFood
    {
        public override int LabelNumber { get { return 1041337; } } // uncooked sausage pizza

        [Constructable]
        public UncookedSausagePizza()
            : base(0x1083, 20)
        {
            Weight = 1.0;
        }

        public UncookedSausagePizza(Serial serial)
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

        public override Food Cook()
        {
            return new SausagePizza();
        }
    }
}