using System;

using Server.Items;
using Server.Network;
using Server.Targeting;

namespace Server.Items
{
    /// <summary>
    /// This was commented out for some reason in the original RunUO distribution
    /// </summary>

    #region Old School Water Pitcher
    /*
    public class PitcherWater : Item
    {
        [Constructable]
        public PitcherWater()
            : base(Utility.Random(0x1f9d, 2))
        {
            Weight = 1.0;
        }

        public PitcherWater(Serial serial)
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

        public override void OnDoubleClick(Mobile from)
        {
            if (!Movable)
                return;

            from.Target = new InternalTarget(this);
        }

        private class InternalTarget : Target
        {
            private PitcherWater m_Item;

            public InternalTarget(PitcherWater item)
                : base(1, false, TargetFlags.None)
            {
                m_Item = item;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (m_Item.Deleted) return;

                if (targeted is BowlFlour)
                {
                    m_Item.Delete();
                    ((BowlFlour)targeted).Delete();

                    from.AddToBackpack(new Dough());
                    from.AddToBackpack(new WoodenBowl());
                }
            }
        }
    }
    */
    #endregion
}