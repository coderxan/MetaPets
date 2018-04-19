using System;

using Server;
using Server.Accounting;
using Server.Engines.VeteranRewards;
using Server.Gumps;
using Server.Mobiles;
using Server.Multis;
using Server.Network;

namespace Server.Items
{
    public class SoulstoneFragment : SoulStone, IUsesRemaining
    {
        private int m_UsesRemaining;

        public override int LabelNumber { get { return 1071000; } } // soulstone fragment

        [Constructable]
        public SoulstoneFragment()
            : this(5, null)
        {
        }

        [Constructable]
        public SoulstoneFragment(int usesRemaining)
            : this(usesRemaining, null)
        {
        }

        [Constructable]
        public SoulstoneFragment(string account)
            : this(5, account)
        {
        }

        [Constructable]
        public SoulstoneFragment(int usesRemaining, string account)
            : base(account, Utility.Random(0x2AA1, 9))
        {
            m_UsesRemaining = usesRemaining;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1060584, m_UsesRemaining.ToString()); // uses remaining: ~1_val~
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int UsesRemaining
        {
            get
            {
                return m_UsesRemaining;
            }
            set
            {
                m_UsesRemaining = value; InvalidateProperties();
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(2); // version

            writer.WriteEncodedInt(m_UsesRemaining);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();

            m_UsesRemaining = reader.ReadEncodedInt();

            if (version <= 1)
            {
                if (ItemID == 0x2A93 || ItemID == 0x2A94)
                {
                    ActiveItemID = Utility.Random(0x2AA1, 9);
                }
                else
                {
                    ActiveItemID = ItemID;
                }

                InactiveItemID = ActiveItemID;
            }

            if (version == 0 && Weight == 1)
                Weight = -1;
        }

        public SoulstoneFragment(Serial serial)
            : base(serial)
        {
        }

        protected override bool CheckUse(Mobile from)
        {
            bool canUse = base.CheckUse(from);

            if (canUse)
            {
                if (m_UsesRemaining <= 0)
                {
                    from.SendLocalizedMessage(1070975); // That soulstone fragment has no more uses.
                    return false;
                }
            }

            return canUse;
        }

        public bool ShowUsesRemaining { get { return true; } set { } }
    }
}