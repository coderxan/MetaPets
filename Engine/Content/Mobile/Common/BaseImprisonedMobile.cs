using System;

using Server;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;

namespace Server.Items
{
    public abstract class BaseImprisonedMobile : Item
    {
        public abstract BaseCreature Summon { get; }

        [Constructable]
        public BaseImprisonedMobile(int itemID)
            : base(itemID)
        {
        }

        public BaseImprisonedMobile(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsChildOf(from.Backpack))
                from.SendGump(new ConfirmBreakCrystalGump(this));
            else
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
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

        public virtual void Release(Mobile from, BaseCreature summon)
        {
        }
    }
}

namespace Server.Gumps
{
    public class ConfirmBreakCrystalGump : BaseConfirmGump
    {
        public override int LabelNumber { get { return 1075084; } } // This statuette will be destroyed when its trapped creature is summoned. The creature will be bonded to you but will disappear if released. <br><br>Do you wish to proceed?

        private BaseImprisonedMobile m_Item;

        public ConfirmBreakCrystalGump(BaseImprisonedMobile item)
            : base()
        {
            m_Item = item;
        }

        public override void Confirm(Mobile from)
        {
            if (m_Item == null || m_Item.Deleted)
                return;

            BaseCreature summon = m_Item.Summon;

            if (summon != null)
            {
                if (!summon.SetControlMaster(from))
                {
                    summon.Delete();
                }
                else
                {
                    from.SendLocalizedMessage(1049666); // Your pet has bonded with you!

                    summon.MoveToWorld(from.Location, from.Map);
                    summon.IsBonded = true;

                    summon.Skills.Wrestling.Base = 100;
                    summon.Skills.Tactics.Base = 100;
                    summon.Skills.MagicResist.Base = 100;
                    summon.Skills.Anatomy.Base = 100;

                    Effects.PlaySound(summon.Location, summon.Map, summon.BaseSoundID);
                    Effects.SendLocationParticles(EffectItem.Create(summon.Location, summon.Map, EffectItem.DefaultDuration), 0x3728, 1, 10, 0x26B6);

                    m_Item.Release(from, summon);
                    m_Item.Delete();
                }
            }
        }
    }
}