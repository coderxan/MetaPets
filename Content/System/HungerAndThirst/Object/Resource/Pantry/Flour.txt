using System;

using Server.Items;
using Server.Network;
using Server.Targeting;

namespace Server.Items
{
    [TypeAlias("Server.Items.SackFlourOpen")]
    public class SackFlour : Item, IHasQuantity
    {
        private int m_Quantity;

        [CommandProperty(AccessLevel.GameMaster)]
        public int Quantity
        {
            get { return m_Quantity; }
            set
            {
                if (value < 0)
                    value = 0;
                else if (value > 20)
                    value = 20;

                m_Quantity = value;

                if (m_Quantity == 0)
                    Delete();
                else if (m_Quantity < 20 && (ItemID == 0x1039 || ItemID == 0x1045))
                    ++ItemID;
            }
        }

        [Constructable]
        public SackFlour()
            : base(0x1039)
        {
            Weight = 5.0;
            m_Quantity = 20;
        }

        public SackFlour(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)2); // version

            writer.Write((int)m_Quantity);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 2:
                case 1:
                    {
                        m_Quantity = reader.ReadInt();
                        break;
                    }
                case 0:
                    {
                        m_Quantity = 20;
                        break;
                    }
            }

            if (version < 2 && Weight == 1.0)
                Weight = 5.0;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!Movable)
                return;

            if ((ItemID == 0x1039 || ItemID == 0x1045))
                ++ItemID;

#if false
			this.Delete();

			from.AddToBackpack( new SackFlourOpen() );
#endif
        }

    }

#if false
	// ********** SackFlourOpen **********
	public class SackFlourOpen : Item
	{
		public override int LabelNumber{ get{ return 1024166; } } // open sack of flour

		[Constructable]
		public SackFlourOpen() : base(UtilityItem.RandomChoice( 0x1046, 0x103a ))
		{
			Weight = 1.0;
		}

		public SackFlourOpen( Serial serial ) : base( serial )
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

		public override void OnDoubleClick( Mobile from )
		{
			if ( !Movable )
				return;

			from.Target = new InternalTarget( this );
		}

		private class InternalTarget : Target
		{
			private SackFlourOpen m_Item;

			public InternalTarget( SackFlourOpen item ) : base( 1, false, TargetFlags.None )
			{
				m_Item = item;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( m_Item.Deleted ) return;

				if ( targeted is WoodenBowl )
				{
					m_Item.Delete();
					((WoodenBowl)targeted).Delete();

					from.AddToBackpack( new BowlFlour() );
				}
				else if ( targeted is TribalBerry )
				{
					if ( from.Skills[SkillName.Cooking].Base >= 80.0 )
					{
						m_Item.Delete();
						((TribalBerry)targeted).Delete();

						from.AddToBackpack( new TribalPaint() );

						from.SendLocalizedMessage( 1042002 ); // You combine the berry and the flour into the tribal paint worn by the savages.
					}
					else
					{
						from.SendLocalizedMessage( 1042003 ); // You don't have the cooking skill to create the body paint.
					}
				}
			}
		}
	}
#endif

    public class BowlFlour : Item
    {
        [Constructable]
        public BowlFlour()
            : base(0xa1e)
        {
            Weight = 1.0;
        }

        public BowlFlour(Serial serial)
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